using System;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3EntrancePortalBuilder
    {
        private const string Folder = "Assets/Stage3/VisualPilot/";
        public const string PrefabPath = Folder + "EntrancePortal.prefab";

        public static GameObject BuildPrefab()
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"EntrancePortal.fbx");
            if (model == null) throw new InvalidOperationException("Import the approved EntrancePortal FBX first.");
            ConfigureTexture("EntrancePortalStone.png",1024);
            ConfigureTexture("EntrancePortalVortex.png",512);
            var stone=Material("EntrancePortalStone.mat","Universal Render Pipeline/Lit");
            stone.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"EntrancePortalStone.png"));
            stone.SetColor("_BaseColor",new Color(.70f,.72f,.76f));
            stone.SetFloat("_Smoothness",.16f);
            var gems=Material("EntrancePortalInlays.mat","Universal Render Pipeline/Lit");
            gems.SetColor("_BaseColor",new Color(.45f,.008f,.003f));
            gems.SetFloat("_Smoothness",.35f);
            gems.EnableKeyword("_EMISSION");
            gems.SetColor("_EmissionColor",new Color(.85f,.012f,.004f));
            var vortex=Material("EntrancePortalVortex.mat","Not3A/Red Entrance Vortex");
            vortex.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"EntrancePortalVortex.png"));
            vortex.SetFloat("_Intensity",1.8f);
            vortex.SetFloat("_CirculationSpeed",.9f);
            var existing=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root=existing != null ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("Intact Red Entrance Portal");
            try
            {
                for(var i=root.transform.childCount-1;i>=0;i--)
                    UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                var pivot=new GameObject("Ground aligned portal").transform;
                pivot.SetParent(root.transform,false);
                // Ground the FBX and establish the portal's unrotated +X bearing.
                // The authored aperture is 45 degrees within its square base;
                // the scene selects the grid-aligned quarter turn for the route.
                pivot.localRotation=Quaternion.AngleAxis(270f,Vector3.forward)*Quaternion.Euler(-90f,0,0);
                var imported=(GameObject)PrefabUtility.InstantiatePrefab(model);
                imported.transform.SetParent(pivot,false);
                Renderer surface=null;
                foreach(var renderer in imported.GetComponentsInChildren<MeshRenderer>())
                {
                    if(renderer.name == "PortalStone") renderer.sharedMaterials=new[]{stone};
                    else if(renderer.name == "PortalInlays") renderer.sharedMaterials=new[]{gems};
                    else if(renderer.name == "PortalVortex")
                    {
                        renderer.sharedMaterials=new[]{vortex};
                        renderer.shadowCastingMode=ShadowCastingMode.Off;
                        surface=renderer;
                    }
                    else throw new InvalidOperationException("Unexpected portal mesh: "+renderer.name);
                }
                if(surface == null) throw new InvalidOperationException("Portal vortex surface missing.");
                var presentation=root.GetComponent<Stage3EntrancePortal>();
                if(presentation == null) presentation=root.AddComponent<Stage3EntrancePortal>();
                presentation.Configure(surface);
                PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
            }
            finally
            {
                if(existing != null) PrefabUtility.UnloadPrefabContents(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
            EditorUtility.SetDirty(stone); EditorUtility.SetDirty(gems); EditorUtility.SetDirty(vortex);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        public static void ReserveFootprint(Stage2MapDefinition map)
        {
            var data=new SerializedObject(map);
            var areas=data.FindProperty("buildableMask").FindPropertyRelative("disabledAreas");
            var minimum=map.EnemyEntrance-Vector2Int.one;
            SerializedProperty target=null;
            for(var i=0;i<areas.arraySize;i++)
            {
                var area=areas.GetArrayElementAtIndex(i);
                var start=area.FindPropertyRelative("minimum").vector2IntValue;
                if(start==map.EnemyEntrance || start==minimum) { target=area; break; }
            }
            if(target==null)
            {
                areas.InsertArrayElementAtIndex(areas.arraySize);
                target=areas.GetArrayElementAtIndex(areas.arraySize-1);
            }
            target.FindPropertyRelative("minimum").vector2IntValue=minimum;
            target.FindPropertyRelative("size").vector2IntValue=new Vector2Int(3,3);
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(map);
        }

        private static void ConfigureTexture(string filename,int size)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+filename);
            importer.textureType=TextureImporterType.Default;
            importer.sRGBTexture=true; importer.mipmapEnabled=true;
            importer.wrapMode=TextureWrapMode.Clamp;
            importer.maxTextureSize=size;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static Material Material(string filename,string shaderName)
        {
            var shader=Shader.Find(shaderName);
            if(shader == null) throw new InvalidOperationException("Missing shader: "+shaderName);
            var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+filename);
            if(material == null)
            {
                material=new Material(shader);
                AssetDatabase.CreateAsset(material,Folder+filename);
            }
            material.shader=shader;
            return material;
        }
    }
}
