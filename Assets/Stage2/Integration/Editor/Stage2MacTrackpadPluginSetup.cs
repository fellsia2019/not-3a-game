using System;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Integration.Editor
{
    public static class Stage2MacTrackpadPluginSetup
    {
        public const string PluginPath = "Assets/Plugins/macOS/libNot3ATrackpad.dylib";

        // Run after rebuilding the native library; the committed importer metadata
        // also makes a fresh Windows checkout exclude this macOS-only binary.
        public static void Configure()
        {
            AssetDatabase.ImportAsset(PluginPath,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(PluginPath) as PluginImporter;
            if (importer == null) throw new InvalidOperationException("Mac trackpad native library is missing.");
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(true);
            importer.SetEditorData("OS","OSX");
            importer.SetEditorData("CPU","AnyCPU");
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX,true);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64,false);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64,false);
            importer.SetPlatformData(BuildTarget.StandaloneOSX,"CPU","AnyCPU");
            importer.SaveAndReimport();
            Debug.Log("S3_MAC_TRACKPAD_CONFIGURED: macOS Editor/player only, Windows/Linux excluded.");
        }
    }
}
