using System;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3ResourceDistrictBuilder
    {
        private const string Folder = "Assets/Stage3/ResourceDistricts/";
        public const string DataPath = Folder + "ResourceDistrictArt.asset";
        private const string ScenePath = "Assets/Scenes/Stage3Rapid3DGamePilot.unity";

        [MenuItem("Tools/Stage 3/Resource Districts/Build Sprite Kit And Wire")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty))
                throw new InvalidOperationException("Stop Play and save open scene edits first.");
            var art = AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>(DataPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<Stage3ResourceDistrictArt>();
                AssetDatabase.CreateAsset(art, DataPath);
            }
            art.trees = Slice("Trees", art, true);
            art.rocks = Slice("RockModules", art, false);
            art.mineParts = Slice("MineModules", art, false);
            var shader = Shader.Find("Not3A/Resource District Sprite");
            if (shader == null) throw new InvalidOperationException("Resource district sprite shader is missing.");
            var materialPath = Folder + "DistrictInk.mat";
            art.material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (art.material == null)
            {
                art.material = new Material(shader);
                AssetDatabase.CreateAsset(art.material, materialPath);
            }
            else art.material.shader = shader;
            EditorUtility.SetDirty(art.material);
            UpdateHybridData();
            art.settings.Validate();
            EditorUtility.SetDirty(art);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var presentation = Object.FindAnyObjectByType<Stage3TacticalPresentation>();
            Wire(presentation);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_RESOURCE_DISTRICT_KIT_READY sprites=18 area=200..300 scene=" + ScenePath);
        }

        public static void Wire(Stage3TacticalPresentation presentation)
        {
            var art = AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>(DataPath);
            if (art == null) throw new InvalidOperationException("Resource district data is missing; build the kit first.");
            var serialized = new SerializedObject(presentation);
            serialized.FindProperty("resourceDistrictArt").objectReferenceValue = art;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(presentation);
        }

        [MenuItem("Tools/Stage 3/Resource Districts/Update Hybrid Data")]
        public static void UpdateHybridData()
        {
            var art=AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>(DataPath);
            if(art==null)throw new InvalidOperationException("Build the resource sprite kit first.");
            Material Ensure(string name,string shaderName)
            {
                var path=Folder+name+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                var shader=Shader.Find(shaderName);
                if(shader==null)throw new InvalidOperationException("Missing hybrid shader "+shaderName);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
                else material.shader=shader;
                EditorUtility.SetDirty(material);return material;
            }
            art.geologyMaterial=Ensure("DistrictGeology","Not3A/District Geology");
            art.geologyMaterial.SetTexture("_StoneTex",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Stage3/VisualPilot/StoneResourceSourceAlbedo.png"));
            art.geologyMaterial.SetFloat("_ZWrite",1);art.geologyMaterial.SetFloat("_SrcBlend",1);
            art.geologyMaterial.SetFloat("_DstBlend",0);art.geologyMaterial.SetFloat("_RevealPass",0);
            art.geologyMaterial.renderQueue=2000;
            art.geologyMaterial.SetShaderPassEnabled("ShadowCaster",true);
            art.geologyRevealMaterial=Ensure("DistrictGeologyReveal","Not3A/District Geology");
            art.geologyRevealMaterial.SetTexture("_StoneTex",art.geologyMaterial.GetTexture("_StoneTex"));
            art.geologyRevealMaterial.SetFloat("_ZWrite",0);art.geologyRevealMaterial.SetFloat("_SrcBlend",5);
            art.geologyRevealMaterial.SetFloat("_DstBlend",10);art.geologyRevealMaterial.SetFloat("_RevealPass",1);
            art.geologyRevealMaterial.renderQueue=2990;
            art.geologyRevealMaterial.SetShaderPassEnabled("ShadowCaster",false);
            art.groundBlendMaterial=Ensure("DistrictGroundBlend","Not3A/District Ground Blend");
            var grass=AssetDatabase.LoadAssetAtPath<Material>("Assets/Stage3/VisualPilot/TacticalGrass.mat");
            art.groundBlendMaterial.SetTexture("_GrassTex",grass.GetTexture("_BaseMap"));
            art.groundBlendMaterial.SetTextureScale("_GrassTex",grass.GetTextureScale("_BaseMap"));
            art.groundBlendMaterial.SetTextureOffset("_GrassTex",grass.GetTextureOffset("_BaseMap"));
            foreach(var material in new[]{art.geologyMaterial,art.geologyRevealMaterial})
            {
                material.SetTexture("_GrassTex",grass.GetTexture("_BaseMap"));
                material.SetTextureScale("_GrassTex",grass.GetTextureScale("_BaseMap"));
                material.SetTextureOffset("_GrassTex",grass.GetTextureOffset("_BaseMap"));
            }
            if(art.shape==null)art.shape=new Not3A.Stage2.Map.ResourceDistrictShapeSettings();
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_RESOURCE_HYBRID_DATA_READY sourceAtlasPreserved=true");
        }

        private static Sprite[] Slice(string name, Stage3ResourceDistrictArt art, bool tree)
        {
            var path = Folder + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture.width != 1536 || texture.height != 1024)
                throw new InvalidOperationException("Unexpected sprite atlas size: " + path);
            var output = new Sprite[6];
            var old = AssetDatabase.LoadAllAssetsAtPath(DataPath);
            for (var i = 0; i < 6; i++)
            {
                var x = i % 3 * 512; var y = (1 - i / 3) * 512;
                var minX = 512; var minY = 512; var maxX = -1; var maxY = -1;
                for (var py = 0; py < 512; py++)
                for (var px = 0; px < 512; px++)
                {
                    if (texture.GetPixel(x + px, y + py).a < .5f) continue;
                    minX = Mathf.Min(minX,px); maxX = Mathf.Max(maxX,px);
                    minY = Mathf.Min(minY,py); maxY = Mathf.Max(maxY,py);
                }
                if (maxX < minX) throw new InvalidOperationException("Empty atlas cell: " + name + i);
                var pixelsPerUnit = tree ? (maxY - minY + 1f) / 3.6f : (maxX - minX + 1f) / 3.2f;
                var pivot = new Vector2((minX + maxX + 1f) * .5f / 512f, (minY + 8f) / 512f);
                var replacement = Sprite.Create(texture, new Rect(x,y,512,512), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                replacement.name = name + "_" + i;
                // Preserve subasset identity when rebuilding the sprite kit.
                Sprite existing = null;
                foreach (var candidate in old)
                    if (candidate is Sprite sprite && sprite.name == replacement.name) existing = sprite;
                if (existing != null)
                {
                    EditorUtility.CopySerialized(replacement, existing); Object.DestroyImmediate(replacement);
                    output[i] = existing; EditorUtility.SetDirty(existing);
                }
                else { output[i] = replacement; AssetDatabase.AddObjectToAsset(replacement, art); }
            }
            return output;
        }
    }
}
