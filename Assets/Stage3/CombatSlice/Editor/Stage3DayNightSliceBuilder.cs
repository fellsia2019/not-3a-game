using System;
using System.IO;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3DayNightSliceBuilder
    {
        [MenuItem("Tools/Stage 3/Day Night/Build And Wire Cycle")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before wiring day/night.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Stage3DayNightArtBuilder.BuildAssets();
            var run = AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(Stage3HeroCombatSliceBuilder.RunPath);
            if (run == null) throw new InvalidOperationException("Build the approved hero/horde slice before adding day/night.");
            if (!run.ContinuousDays)
            {
                var waves = new Stage2WaveDefinition[run.WaveCount];
                for (var i = 0; i < waves.Length; i++)
                {
                    var wave = run.GetWave(i);
                    waves[i] = new Stage2WaveDefinition(1200f, wave.EnemyCount, wave.SpawnInterval);
                }
                run.ConfigureWaves(waves, run.ThroneHealth);
                run.ConfigureContinuousDays(true, 60);
                EditorUtility.SetDirty(run);
            }
            if (!run.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(Stage3HeroCombatSliceBuilder.ScenePath, OpenSceneMode.Single);
            var game = UnityEngine.Object.FindAnyObjectByType<Stage2GameComposition>();
            var tactical = UnityEngine.Object.FindAnyObjectByType<Stage3TacticalPresentation>();
            if (game == null || tactical == null) throw new InvalidOperationException("Missing playable composition/presentation.");
            var gameData = new SerializedObject(game);
            gameData.FindProperty("runCatalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(Stage3HeroCombatSliceBuilder.RunPath);
            gameData.ApplyModifiedPropertiesWithoutUndo();
            var art = new SerializedObject(tactical);
            art.FindProperty("heroArt").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Stage3HeroArt>(Stage3DayNightArtBuilder.HeroArtPath);
            art.FindProperty("enableDayNightPresentation").boolValue = true;
            art.FindProperty("dayNightGround").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Stage3/DayNight/DayNightGround.mat");
            art.ApplyModifiedPropertiesWithoutUndo();
            var home = game.GetComponent<Stage3Dwelling>();
            if (home == null) home = game.gameObject.AddComponent<Stage3Dwelling>();
            var homeData = new SerializedObject(home);
            var catalog = AssetDatabase.LoadAssetAtPath<Stage3DwellingCatalog>(Stage3DayNightArtBuilder.CatalogPath);
            if (catalog == null || !catalog.TryValidate(out reason)) throw new InvalidOperationException("Incomplete dwelling art/catalog.");
            homeData.FindProperty("catalog").objectReferenceValue = catalog;
            homeData.ApplyModifiedPropertiesWithoutUndo();
            if (art.FindProperty("heroArt").objectReferenceValue == null || art.FindProperty("dayNightGround").objectReferenceValue == null)
                throw new InvalidOperationException("Day/night material and hero art must remain bound after scene opening.");
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("S3_DAY_NIGHT_SLICE_READY: 20-minute days, continuing growing night hordes, paid dwelling and approved C/B presentation.");
        }

        [MenuItem("Tools/Stage 3/Day Night/Build Saved Windows Player")]
        public static void BuildWindowsPlayer()
            => BuildWindowsPlayerAt("Builds/WindowsDayNight/Not3ADayNight.exe");

        [MenuItem("Tools/Stage 3/Hero/Build Saved Motion Review Player")]
        public static void BuildHeroMotionPlayer()
            => BuildWindowsPlayerAt("Builds/WindowsHeroMotion/Not3AHeroMotion.exe");

        private static void BuildWindowsPlayerAt(string output)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { Stage3HeroCombatSliceBuilder.ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Day/night Windows build failed: " + report.summary.result);
            Debug.Log($"S3_DAY_NIGHT_BUILD_OK path={output} bytes={report.summary.totalSize}");
        }
    }
}
