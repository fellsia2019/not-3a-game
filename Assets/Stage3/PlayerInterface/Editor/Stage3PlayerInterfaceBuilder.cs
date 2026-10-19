using System;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3PlayerInterfaceBuilder
    {
        private const string ScenePath = "Assets/Scenes/Stage3Rapid3DGamePilot.unity";

        [MenuItem("Tools/Stage 3/Player Interface/Import And Wire Approved UI")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before binding the player interface.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Stage3FantasyHudArtImporter.Import();
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var game = UnityEngine.Object.FindAnyObjectByType<Stage2GameComposition>();
            var art = AssetDatabase.LoadAssetAtPath<Stage3FantasyHudArt>(Stage3FantasyHudArtImporter.ArtPath);
            if (game == null || art == null || !art.IsReady)
                throw new InvalidOperationException("Playable composition and complete lunar interface art are required.");
            var hud = game.GetComponent<Stage3FantasyHud>();
            if (hud == null) hud = game.gameObject.AddComponent<Stage3FantasyHud>();
            hud.ConfigureArt(art);
            if (game.GetComponent<Stage3PlayerInterfaceBinding>() == null)
                game.gameObject.AddComponent<Stage3PlayerInterfaceBinding>();
            if (game.GetComponent<Stage3InterfaceCapture>() == null)
                game.gameObject.AddComponent<Stage3InterfaceCapture>();
            EditorUtility.SetDirty(hud);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_PLAYER_INTERFACE_WIRED: approved C artwork, saved scene and opt-in input binding.");
        }

        [MenuItem("Tools/Stage 3/Player Interface/Build Saved Windows Player")]
        public static void BuildWindowsPlayer()
        {
            const string output = "Builds/WindowsInterface/Not3AInterface.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Player interface Windows build failed: " + report.summary.result);
            Debug.Log($"S3_PLAYER_INTERFACE_BUILD_OK path={output} bytes={report.summary.totalSize}");
        }

        // Local visual verification on the editor host; Windows remains the product target.
        [MenuItem("Tools/Stage 3/Player Interface/Build Local Mac Review Player")]
        public static void BuildMacReviewPlayer()
        {
            const string output = "Builds/MacFullscreenInterface/Not3AFullscreenInterface.app";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Player interface local Mac review build failed: " + report.summary.result);
            Debug.Log($"S3_PLAYER_INTERFACE_MAC_REVIEW_BUILD_OK path={output} bytes={report.summary.totalSize}");
        }
    }
}
