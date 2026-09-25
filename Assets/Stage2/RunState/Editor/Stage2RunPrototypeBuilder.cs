using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.RunState.Editor
{
    public static class Stage2RunPrototypeBuilder
    {
        private const string CatalogPath = "Assets/Stage2/RunState/Data/Stage2RunPrototype.asset";

        [MenuItem("Not 3A/Stage 2/Create Run Prototype")]
        public static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(CatalogPath) != null)
            {
                Debug.Log("S2_RUN_ASSET_EXISTS: prototype already authored; no asset overwritten.");
                return;
            }
            var catalog = ScriptableObject.CreateInstance<Stage2RunCatalog>();
            catalog.ConfigureWaves(new[]
            {
                new Stage2WaveDefinition(45f, 6, 0.8f),
                new Stage2WaveDefinition(30f, 8, 0.8f),
                new Stage2WaveDefinition(30f, 10, 0.8f)
            }, 5);
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();
            Debug.Log("S2_RUN_ASSET_READY: diagnostic three-wave catalog created.");
        }
    }
}
