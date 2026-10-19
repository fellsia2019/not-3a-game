using System;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3ManualResourceBuilder
    {
        public const string EconomyPath = "Assets/Stage3/ResourceSpritePilot/ManualEconomy.asset";

        public static void BuildAllApproved()
        {
            Stage3ManualResourceArtBuilder.BuildApprovedArt();
            BuildAndWire();
        }

        [MenuItem("Tools/Stage 3/Manual Resources/Wire Distribution")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                    .Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Stop Play and save open scene edits before manual resource wiring.");
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>(EconomyPath);
            if (catalog == null)
            {
                var prior = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset");
                if (prior == null) throw new InvalidOperationException("Original economy catalog missing.");
                catalog = ScriptableObject.CreateInstance<Stage2EconomyCatalog>();
                catalog.Configure(prior.InteractionRange, Enumerable.Range(0, 3).Select(i =>
                {
                    var source = prior.Get((MapResourceKind)i);
                    return new Stage2ResourceSettings(source.Resource, 5, source.GatherSeconds, source.RecoverySeconds);
                }).ToArray(), prior.StartingGold, prior.GoldPerKill);
                catalog.ConfigureManualResources(new Stage2ManualResourceSettings());
                AssetDatabase.CreateAsset(catalog, EconomyPath);
            }
            // Migrate only this slice's initial mineral footprint, preserving all other tuning.
            var manual = catalog.ManualResources;
            if (manual != null && Mathf.Approximately(manual.smallHarvestRadius, .35f) &&
                Mathf.Approximately(manual.mediumHarvestRadius, .65f) && Mathf.Approximately(manual.largeHarvestRadius, 1.05f))
            {
                manual.smallHarvestRadius = .65f;
                manual.mediumHarvestRadius = 1.1f;
                manual.largeHarvestRadius = 1.8f;
                EditorUtility.SetDirty(catalog);
            }
            var art = AssetDatabase.LoadAssetAtPath<Stage3ManualResourceArt>(Stage3ManualResourceArtBuilder.ArtPath);
            if (manual != null && art != null)
            {
                manual.stoneHarvestRadii = (float[])art.stoneHarvestRadii.Clone();
                manual.oreHarvestRadii = (float[])art.oreHarvestRadii.Clone();
            }
            if (!catalog.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            // OpenScene can unload a newly created asset held only by this local variable.
            catalog = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>(EconomyPath);
            if (catalog == null || !EditorUtility.IsPersistent(catalog))
                throw new InvalidOperationException("Persistent Stage3 economy catalog failed to reload.");
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            if (composition == null) throw new InvalidOperationException("Stage3 composition missing.");
            var serialized = new SerializedObject(composition);
            serialized.FindProperty("economyCatalog").objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(composition);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_MANUAL_RESOURCE_DISTRIBUTION_WIRED: Stage3-only catalog; legacy Stage2 data preserved.");
        }
    }
}
