using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Preserves the approved raster skins and their measured pixel coordinates.</summary>
    public sealed class Stage3EquipmentQualityArtImporter : AssetPostprocessor
    {
        public const string SkinsPath =
            "Assets/Stage3/PlayerInterface/Resources/Stage3EquipmentQualitySkins.png";

        private void OnPreprocessTexture()
        {
            if (assetPath == SkinsPath) Configure((TextureImporter)assetImporter);
        }

        private static void Configure(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.isReadable = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.userData =
                "Owned ImageGen quality skins derived from owner-approved 64-cell v2. " +
                "Original PNG unchanged; 1774x887; measured pixel rects in Stage3FantasyHud.Quality.";
        }

        [MenuItem("Tools/Stage 3/Equipment/Import Approved Quality Skins")]
        public static void BuildAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing quality artwork.");
            if (!File.Exists(SkinsPath))
                throw new FileNotFoundException("Approved quality skins are missing.", SkinsPath);
            AssetDatabase.ImportAsset(SkinsPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SkinsPath);
            Configure(importer);
            importer.SaveAndReimport();
            Debug.Log("S3_QUALITY_ART_READY: original textured skins, measured UVs, no procedural plates.");
        }
    }
}
