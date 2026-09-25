using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.WavesAI.Editor
{
    public static class Stage2EnemyPrototypeBuilder
    {
        private const string PrefabPath = "Assets/Stage2/WavesAI/Prefabs/Stage2GroundEnemy.prefab";
        private const string CatalogPath = "Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset";

        [MenuItem("Not 3A/Stage 2/Create Enemy Prototype")]
        public static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null ||
                AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>(CatalogPath) != null)
            {
                Debug.Log("S2_AI_ASSETS_EXIST: prototype already authored; no asset overwritten.");
                return;
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Stage1/Generated/GrayboxSolid.asset");
            if (sprite == null)
            {
                Debug.LogError("S2_AI_ASSET_ERROR: Stage 1 graybox sprite is unavailable.");
                return;
            }
            var root = new GameObject("Stage 2 Ground Enemy");
            try
            {
                root.AddComponent<Stage2Enemy>();
                var renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = new Color(0.92f, 0.31f, 0.19f, 1f);
                renderer.sortingOrder = 300;
                root.transform.localScale = new Vector3(0.62f, 0.85f, 1f);
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<Stage2Enemy>();
            var catalog = ScriptableObject.CreateInstance<Stage2EnemyCatalog>();
            catalog.Configure(prefab, 2.2f, 3, 0.5f, 1, 1);
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();
            Debug.Log("S2_AI_ASSETS_READY: ground enemy prefab and prototype catalog created.");
        }
    }
}
