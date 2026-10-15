using System;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Builds the approved three-cell-wide objective with rules authored in map data.</summary>
    public static class Stage3SoulSphereBuilder
    {
        private const string Folder = "Assets/Stage3/VisualPilot/";
        public const string PrefabPath = Folder + "SoulSphereIceDome.prefab";

        public static GameObject BuildPrefab()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "SoulSphereIceDome.fbx");
            if (model == null) throw new InvalidOperationException("Import SoulSphereIceDome.fbx before building the scene.");
            var dark = GetMaterial("SoulStoneDark.mat", new Color(.68f,.72f,.81f), .16f);
            dark.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"SoulSanctuaryStone.png"));
            var dome = Ice("SoulIceDome.mat", new Color(.035f,.30f,.73f), .024f, .85f, 3020,2.8f,.40f);
            var sphere = Ice("SoulGlassSphere.mat", new Color(.015f,.28f,.88f), .20f, .93f, 3000,3.3f,.35f);
            var energy = GetMaterial("SoulBlueEnergy.mat", new Color(.07f,.72f,1f), .35f, false, 1.8f);
            var fire=CustomMaterial("SoulFire.mat","Not3A/Soul Flame");
            fire.SetColor("_BaseColor",new Color(.025f,.65f,1.9f,.9f));
            var flame=CustomMaterial("SoulFlame.mat","Not3A/Soul Flame");
            flame.SetColor("_BaseColor",new Color(1.1f,1.7f,2.2f,.95f));
            flame.renderQueue=2980;
            var waveMaterial = CustomMaterial("SoulFlowingWave.mat", "Not3A/Flowing Soul Wave");
            waveMaterial.SetColor("_BaseColor",new Color(.025f,.70f,1.3f,.95f));
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("Soul Sphere Ice Dome");
            try
            {
                for (var i = root.transform.childCount-1; i>=0; i--)
                    UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                var pivot = new GameObject("Ground aligned ice objective").transform;
                pivot.SetParent(root.transform, false);
                pivot.localRotation = Quaternion.Euler(-90f,0,0);
                var imported = (GameObject)PrefabUtility.InstantiatePrefab(model);
                imported.name = "Original Blender soul sphere model";
                imported.transform.SetParent(pivot, false);
                Transform wave = null;
                foreach (var renderer in imported.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var name = renderer.gameObject.name;
                    Material[] assigned;
                    if (name.StartsWith("StoneBase", StringComparison.Ordinal)) assigned = new[] { dark };
                    else if (name.StartsWith("Dome", StringComparison.Ordinal)) assigned = new[] { dome };
                    else if (name.StartsWith("Sphere", StringComparison.Ordinal)) assigned = new[] { sphere };
                    else if (name.StartsWith("Soul", StringComparison.Ordinal)) assigned = new[] { fire, flame };
                    else if (name.StartsWith("Energy", StringComparison.Ordinal)) assigned = new[] { energy };
                    else if (name.StartsWith("OrbitingWave", StringComparison.Ordinal))
                    {
                        assigned = new[] { waveMaterial };
                        wave = renderer.transform;
                    }
                    else throw new InvalidOperationException("Unexpected soul objective mesh: " + name);
                    renderer.sharedMaterials = assigned;
                    if (name.StartsWith("Dome", StringComparison.Ordinal) ||
                        name.StartsWith("Sphere", StringComparison.Ordinal) ||
                        name.StartsWith("OrbitingWave", StringComparison.Ordinal) ||
                        name.StartsWith("Soul", StringComparison.Ordinal))
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                if (wave == null) throw new InvalidOperationException("Animated orbit mesh is missing.");
                var glowObject=new GameObject("Soul light on inner bowl");
                glowObject.transform.SetParent(root.transform,false);
                glowObject.transform.localPosition=new Vector3(0,0,-.72f);
                var glow=glowObject.AddComponent<Light>();
                glow.type=LightType.Point;
                glow.color=new Color(.06f,.46f,1f);
                glow.range=1.5f;
                glow.intensity=1.2f;
                glow.shadows=LightShadows.None;
                var motion = root.GetComponent<Stage3SoulSphereMotion>();
                if (motion == null) motion = root.AddComponent<Stage3SoulSphereMotion>();
                motion.Configure(wave);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                if (existing != null) PrefabUtility.UnloadPrefabContents(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        public static void ReserveFootprint(Stage2MapDefinition map)
        {
            var data = new SerializedObject(map);
            var areas = data.FindProperty("buildableMask").FindPropertyRelative("disabledAreas");
            var minimum = map.Throne-Vector2Int.one;
            SerializedProperty target = null;
            for (var i=0; i<areas.arraySize; i++)
            {
                var area=areas.GetArrayElementAtIndex(i);
                var start=area.FindPropertyRelative("minimum").vector2IntValue;
                if (start == map.Throne || start == minimum) { target=area; break; }
            }
            if (target == null)
            {
                areas.InsertArrayElementAtIndex(areas.arraySize);
                target=areas.GetArrayElementAtIndex(areas.arraySize-1);
            }
            target.FindPropertyRelative("minimum").vector2IntValue=minimum;
            target.FindPropertyRelative("size").vector2IntValue=new Vector2Int(3,3);
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(map);
        }

        public static void ConfigureGlow(Camera camera)
        {
            const string path=Folder+"SoulGlowProfile.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null)
            {
                profile=ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile,path);
            }
            if(!profile.TryGet<Bloom>(out var bloom))
            {
                bloom=profile.Add<Bloom>();
                AssetDatabase.AddObjectToAsset(bloom,profile);
            }
            bloom.threshold.Override(1.15f);
            bloom.intensity.Override(.35f);
            bloom.scatter.Override(.55f);
            bloom.clamp.Override(4f);
            EditorUtility.SetDirty(profile); EditorUtility.SetDirty(bloom);
            var effects=GameObject.Find("Soul energy glow");
            if(effects==null) effects=new GameObject("Soul energy glow");
            var volume=effects.GetComponent<Volume>();
            if(volume==null) volume=effects.AddComponent<Volume>();
            volume.isGlobal=true; volume.sharedProfile=profile;
            camera.allowHDR=true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        }

        private static Material CustomMaterial(string filename, string shaderName)
        {
            var path=Folder+filename;
            var shader=Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException("Missing shader: "+shaderName);
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material=new Material(shader);
                AssetDatabase.CreateAsset(material,path);
            }
            material.shader=shader;
            material.shaderKeywords=Array.Empty<string>();
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Ice(string filename,Color tint,float centre,float edge,int queue,float rimPower,float sheen)
        {
            var material=CustomMaterial(filename,"Not3A/Smooth Soul Ice");
            material.SetColor("_BaseColor",tint);
            material.SetColor("_RimColor",new Color(.20f,1.15f,2.0f));
            material.SetFloat("_CenterOpacity",centre);
            material.SetFloat("_EdgeOpacity",edge);
            material.SetFloat("_RimPower",rimPower);
            material.SetFloat("_Sheen",sheen);
            material.renderQueue=queue;
            return material;
        }

        private static Material GetMaterial(string filename, Color color, float smoothness,
            bool transparent = false, float emission = 0)
        {
            var path = Folder + filename;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat,path);
            }
            mat.shader=Shader.Find("Universal Render Pipeline/Lit");
            mat.shaderKeywords=Array.Empty<string>();
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Surface",0);
            mat.SetFloat("_SrcBlend",(float)BlendMode.One);
            mat.SetFloat("_DstBlend",(float)BlendMode.Zero);
            mat.SetFloat("_ZWrite",1);
            mat.SetOverrideTag("RenderType","Opaque");
            mat.renderQueue=(int)RenderQueue.Geometry;
            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
            if (emission > 0)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emission);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
