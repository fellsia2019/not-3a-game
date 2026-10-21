using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Imports the owned profession medallions; all page layout and text remain native.</summary>
    public static class Stage3ProfessionArtImporter
    {
        public const string IconsPath = "Assets/Stage3/PlayerInterface/Resources/Stage3ProfessionIcons.png";

        [MenuItem("Tools/Stage 3/Professions/Import Approved Medallions")]
        public static void BuildAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing profession artwork.");
            if (!File.Exists(IconsPath))
                throw new FileNotFoundException("Approved profession icon atlas is missing.", IconsPath);
            AssetDatabase.ImportAsset(IconsPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconsPath);
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.isReadable = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.userData = "Owned ImageGen profession medallions; 4 columns, 2 rows. Native UI uses cell UVs.";
            importer.SaveAndReimport();
            Debug.Log("S3_PROFESSION_ART_READY: eight approved medallions, native page layout.");
        }
    }
}
