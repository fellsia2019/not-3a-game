using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Defense.Editor
{
    public static class Stage2DefensePrototypeBuilder
    {
        private const string PrefabPath = "Assets/Stage2/Defense/Prefabs/Stage2BlockerTower.prefab";
        private const string CatalogPath = "Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset";
        private const string SpritePath = "Assets/Stage1/Generated/GrayboxSolid.asset";

        [MenuItem("Not 3A/Stage 2/Create Defense Prototype")]
        public static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null ||
                AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(CatalogPath) != null)
            {
                Debug.Log("S2_DEF_ASSETS_EXIST: prototype already authored; no asset overwritten.");
                return;
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite == null)
            {
                Debug.LogError("S2_DEF_ASSET_ERROR: Stage 1 graybox sprite is unavailable.");
                return;
            }

            var root = new GameObject("Stage 2 Blocker Tower");
            try
            {
                root.AddComponent<Stage2Tower>();
                var renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = new Color(0.2f, 0.55f, 0.86f, 1f);
                renderer.sortingOrder = 250;
                root.transform.localScale = new Vector3(0.86f, 1.4f, 1f);
                root.SetActive(false); // Prepared instances stay out of play until payment commits.
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<Stage2Tower>();
            var catalog = ScriptableObject.CreateInstance<Stage2DefenseCatalog>();
            catalog.Configure(prefab, 1, 1, 1, 10);
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();
            Debug.Log("S2_DEF_ASSETS_READY: blocker tower prefab and prototype catalog created.");
        }

        [MenuItem("Not 3A/Stage 2/Add Baseline Tower Attack")]
        public static void AddBaselineAttack()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("S2_DEF_ASSET_ERROR: create the defense prototype first.");
                return;
            }
            // Preserve the already-authored cost/health. Stage 1 attack values
            // are provisional; P-004 and S2-DEF-02 remain separate decisions.
            catalog.Configure(catalog.TowerPrefab, catalog.WoodCost, catalog.StoneCost,
                catalog.MetalCost, catalog.TowerHealth, 4.25f, 0.45f, 1);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("S2_DEF_ATTACK_READY: baseline tower attack added to prototype data.");
        }
    }
}
