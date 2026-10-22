using System;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Preserves original ImageGen bytes; only fixes import settings for the measured UI atlas.</summary>
    public sealed class Stage3HeroSkillArtImporter : AssetPostprocessor
    {
        public const string AtlasPath = "Assets/Stage3/PlayerInterface/Resources/Stage3HeroSkillIcons.png";
        private void OnPreprocessTexture()
        {
            if (assetPath != AtlasPath) return;
            Configure((TextureImporter)assetImporter);
        }
        private static void Configure(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.sRGBTexture = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Trilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;
        }
        [MenuItem("Tools/Stage 3/Player Interface/Import Hero Skill Icons")]
        public static void Import()
        {
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(AtlasPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Hero skill atlas is missing.");
            Configure(importer); importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
            if (texture == null || texture.width != 1774 || texture.height != 887)
                throw new InvalidOperationException("Hero skill source changed: inspect the32icon bounds before importing.");
            Debug.Log("S3_HERO_SKILL_ART_READY:32measured icons; original pixels preserved.");
        }
    }
}
