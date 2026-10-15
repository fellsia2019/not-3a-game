using System;
using Not3A.Stage2.Defense;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Visual binding for the approved catapult. Gameplay values come from the existing catalog.</summary>
    public static class Stage3LongshotBuilder
    {
        private const string Folder = "Assets/Stage3/VisualPilot/";
        public const string CatalogPath = Folder + "LongshotCatapultGameCatalog.asset";
        public const string PrefabPath = Folder + "LongshotCatapultGame.prefab";

        public static void BuildCombatReview()
        {
            BuildCatalog();
            Stage3RapidPilotBuilder.BuildCurrentGamePlayer();
        }

        public static Stage2DefenseCatalog BuildCatalog()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "LongshotCatapult.fbx");
            var baseline = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(
                "Assets/Stage2/Defense/Data/Stage2DefenseLongshot.asset");
            if (model == null || baseline == null)
                throw new InvalidOperationException("Longshot model and baseline catalog must be imported first.");
            var texturePath = Folder + "LongshotCatapultAlbedo.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            var materialPath = Folder + "LongshotCatapultAtlas.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", .21f);
            EditorUtility.SetDirty(material);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(PrefabPath)
                : new GameObject("Longshot Stone Catapult");
            try
            {
                root.transform.localScale = Vector3.one;
                if (root.GetComponent<Stage2Tower>() == null) root.AddComponent<Stage2Tower>();
                var source = root.GetComponent<SpriteRenderer>();
                if (source == null) source = root.AddComponent<SpriteRenderer>();
                source.sprite = baseline.TowerPrefab.GetComponent<SpriteRenderer>().sprite;
                source.color = Color.white;
                source.enabled = false;
                for (var i = root.transform.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                var pivot = new GameObject("Pilot ground-aligned mesh").transform;
                pivot.SetParent(root.transform, false);
                pivot.localPosition = new Vector3(0f, 0f, -.03f);
                pivot.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                var child = (GameObject)PrefabUtility.InstantiatePrefab(model);
                child.name = "Longshot Catapult Mesh";
                child.transform.SetParent(pivot, false);
                foreach (var renderer in child.GetComponentsInChildren<MeshRenderer>())
                    renderer.sharedMaterials = new[] { material };
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                if (existing != null) PrefabUtility.UnloadPrefabContents(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = UnityEngine.Object.Instantiate(baseline);
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            else EditorUtility.CopySerialized(baseline, catalog);
            catalog.name = "LongshotCatapultGameCatalog";
            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("towerPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<Stage2Tower>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }
    }
}
