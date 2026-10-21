using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3HeroCombatSliceBuilder
    {
        public const string ScenePath = "Assets/Scenes/Stage3Rapid3DGamePilot.unity";
        public const string HeroPath = "Assets/Stage3/CombatSlice/HeroBow.asset";
        public const string RunPath = "Assets/Stage3/CombatSlice/HordeRun.asset";

        [MenuItem("Tools/Stage 3/Hero Combat/Build And Wire Slice")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before wiring the combat slice.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Stage3BarricadeBuilder.BuildAssets();
            var hero = AssetDatabase.LoadAssetAtPath<Stage2HeroCombatCatalog>(HeroPath);
            if (hero == null)
            {
                hero = ScriptableObject.CreateInstance<Stage2HeroCombatCatalog>();
                AssetDatabase.CreateAsset(hero, HeroPath);
            }
            var run = AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(RunPath);
            if (run == null)
            {
                run = ScriptableObject.CreateInstance<Stage2RunCatalog>();
                run.ConfigureWaves(new[] {
                    new Stage2WaveDefinition(180f, 16, 2.2f),
                    new Stage2WaveDefinition(180f, 120, .12f),
                    new Stage2WaveDefinition(180f, 240, .08f)
                }, 5);
                AssetDatabase.CreateAsset(run, RunPath);
            }
            if (!hero.TryValidate(out var reason) || !run.TryValidate(out reason))
                throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var game = UnityEngine.Object.FindAnyObjectByType<Stage2GameComposition>();
            if (game == null) throw new InvalidOperationException("Playable scene is missing its composition.");
            var serialized = new SerializedObject(game);
            serialized.FindProperty("barricadeCatalog").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(Stage3BarricadeBuilder.CatalogPath);
            serialized.FindProperty("heroCombatCatalog").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Stage2HeroCombatCatalog>(HeroPath);
            serialized.FindProperty("runCatalog").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(RunPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (serialized.FindProperty("heroCombatCatalog").objectReferenceValue == null ||
                serialized.FindProperty("runCatalog").objectReferenceValue == null ||
                serialized.FindProperty("barricadeCatalog").objectReferenceValue == null)
                throw new InvalidOperationException("Combat slice assets must remain bound after opening the scene.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_HERO_COMBAT_SLICE_READY: nearest-target bow, passive barricade and separate horde run data; legacy Stage 2 unchanged.");
        }

        [MenuItem("Tools/Stage 3/Hero Combat/Apply Initial Acceptance Tuning")]
        public static void ApplyInitialAcceptanceTuning()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before changing the authored combat tuning.");
            var hero = AssetDatabase.LoadAssetAtPath<Stage2HeroCombatCatalog>(HeroPath);
            var run = AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(RunPath);
            if (hero == null || run == null) throw new InvalidOperationException("Build the combat slice first.");
            var serialized = new SerializedObject(hero);
            serialized.FindProperty("attackDamage").intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var preparation = run.ContinuousDays ? 600f : 180f;
            run.ConfigureWaves(new[] {
                new Stage2WaveDefinition(preparation, 16, 2.2f),
                new Stage2WaveDefinition(preparation, 120, .12f),
                new Stage2WaveDefinition(preparation, 240, .08f)
            }, 5);
            if(run.ContinuousDays)run.ConfigureClockDurations(600f,180f);
            EditorUtility.SetDirty(hero); EditorUtility.SetDirty(run); AssetDatabase.SaveAssets();
            Debug.Log("S3_HERO_COMBAT_TUNING: bow needs two hits on baseline skeleton; relaxed first cadence, dense later hordes.");
        }
    }
}
