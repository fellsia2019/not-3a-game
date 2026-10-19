using System.IO;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3ProductionBuilder
    {
        public const string Folder = "Assets/Stage3/ResourceDistricts/Production";
        [MenuItem("Tools/Stage 3/Resource Districts/Import Production Buildings")]
        public static void Import()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new System.InvalidOperationException("Stop Play before importing production art.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Stage3/ResourceDistricts", "Production");
            var names = new[] { "sawmill", "quarry", "iron_mine" };
            var titles = new[] { "Лесопилка", "Каменоломня", "Железная шахта" };
            var widths = new[] { 3f, 2.8f, 2.8f };
            var intervals = new[] { 5f, 7f, 10f };
            var wood = new[] { 6, 5, 6 }; var stone = new[] { 3, 6, 8 }; var metal = new[] { 0, 0, 3 };
            var pivots = new[] { .23f, .24f, .20f };
            var entries = new Stage2ProductionEntry[3];
            for (var i = 0; i < names.Length; i++)
            {
                var path = Folder + "/" + names[i] + ".png";
                File.Copy("art/concepts/resource_production_buildings_2026-10-16/" + names[i] + "_v1.png", path, true);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.isReadable = true; importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Trilinear; importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.spritePixelsPerUnit = 100;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = new Vector2(.5f, pivots[i]);
                settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
                entries[i] = new Stage2ProductionEntry { building = (ResourceProductionBuilding)i, displayName = titles[i],
                    woodCost = wood[i], stoneCost = stone[i], metalCost = metal[i], outputAmount = 1, intervalSeconds = intervals[i],
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path), visualWidth = widths[i], footprintSize = new Vector2(1.4f, .8f) };
            }
            var catalogPath = Folder + "/ProductionCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2ProductionCatalog>(catalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<Stage2ProductionCatalog>(); AssetDatabase.CreateAsset(catalog, catalogPath); }
            var art = AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>("Assets/Stage3/ResourceDistricts/ResourceDistrictArt.asset");
            catalog.Configure(entries, art.material); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var serialized = new SerializedObject(composition);
            serialized.FindProperty("productionCatalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Stage2ProductionCatalog>(catalogPath); serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            Debug.Log("S3_PRODUCTION_ART_BOUND entries=3 sourceRGBA=true lockedPoints=true");
        }
    }
}
