using System;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Builds visual prefabs for the existing three economy nodes.</summary>
    public static class Stage3ResourceSourceBuilder
    {
        private const string Folder = "Assets/Stage3/VisualPilot/";
        public const string StonePrefabPath = Folder + "StoneResourceSource.prefab";
        public const string WoodPrefabPath = Folder + "WoodResourceSource.prefab";
        public const string IronOrePrefabPath = Folder + "IronOreResourceSource.prefab";

        public static void BuildPrefabs()
        {
            Build("StoneResourceSource", StonePrefabPath, new[] { AtlasMaterial("StoneResourceSource") });
            Build("WoodResourceSource", WoodPrefabPath, new[] { AtlasMaterial("WoodResourceSource") });
            Build("IronOreResourceSource", IronOrePrefabPath, new[] { AtlasMaterial("IronOreResourceSource") });
            AssetDatabase.SaveAssets();
        }

        private static Material AtlasMaterial(string name)
        {
            var texturePath = Folder + name + "Albedo.png";
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Resource albedo missing: " + name);
            if (!importer.sRGBTexture || !importer.mipmapEnabled || importer.maxTextureSize != 2048 ||
                importer.textureCompression != TextureImporterCompression.CompressedHQ)
            {
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
            var path = Folder + name + "Surface.mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetFloat("_Smoothness", .08f);
            if (name != "WoodResourceSource")
            {
                var normalPath = Folder + name + "Normal.png";
                var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if (normalImporter == null) throw new InvalidOperationException("Resource relief missing: " + name);
                if (normalImporter.textureType != TextureImporterType.NormalMap ||
                    normalImporter.maxTextureSize != 2048 ||
                    normalImporter.textureCompression != TextureImporterCompression.CompressedHQ)
                {
                    normalImporter.textureType = TextureImporterType.NormalMap;
                    normalImporter.maxTextureSize = 2048;
                    normalImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                    normalImporter.SaveAndReimport();
                }
                material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
                material.EnableKeyword("_NORMALMAP");
            }
            material.SetFloat("_ReceiveShadows", 1f);
            material.DisableKeyword("_RECEIVE_SHADOWS_OFF");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Build(string name, string path, Material[] materials)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".fbx");
            if (model == null) throw new InvalidOperationException("Resource FBX missing: " + name);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(path)
                : new GameObject(name + " Visual");
            try
            {
                for (var i = root.transform.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                var pivot = new GameObject("Ground aligned source").transform;
                pivot.SetParent(root.transform, false);
                pivot.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                var imported = (GameObject)PrefabUtility.InstantiatePrefab(model);
                imported.transform.SetParent(pivot, false);
                var renderers = imported.GetComponentsInChildren<MeshRenderer>();
                if (renderers.Length != 1 || renderers[0].sharedMaterials.Length != materials.Length)
                    throw new InvalidOperationException($"Unexpected {name} FBX mesh/material count.");
                renderers[0].sharedMaterials = materials;
                if (root.GetComponentsInChildren<Collider>().Length != 0)
                    throw new InvalidOperationException(name + " has a gameplay collider.");
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                if (existing != null) PrefabUtility.UnloadPrefabContents(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
