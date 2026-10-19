using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Explicit local measurement of the saved road and actual spawn path; no scene or data writes.</summary>
    public static class Stage3MovementReachReview
    {
        public static void MeasureBefore() => Measure("before");
        public static void MeasureAfter() => Measure("after");
        private static void Measure(string name)
        {
            var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
            var navigation = new Stage2Navigation(map);
            var group = new Stage2EnemyGroup(map, navigation, catalog);
            double Time(Action action)
            { var clock = Stopwatch.StartNew(); action(); return clock.Elapsed.TotalMilliseconds; }
            var validation = new double[8]; var hashes = new double[8]; var spawns = new double[8];
            try
            {
                for (var i = 0; i < spawns.Length; i++)
                {
                    validation[i] = Time(() => { if (!map.ValidateDefinition().IsValid) throw new InvalidOperationException("Invalid saved road."); });
                    hashes[i] = Time(() => map.ComputeDeterministicContentHash());
                    spawns[i] = Time(() => group.Spawn().SetAutomaticTick(false));
                }
                var report = $"{{\"mode\":\"Editor Mono, actual saved 250x250 road, eight fresh enemies, no pooling\",\"validation_mean_ms\":{validation.Average().ToString(System.Globalization.CultureInfo.InvariantCulture)},\"hash_mean_ms\":{hashes.Average().ToString(System.Globalization.CultureInfo.InvariantCulture)},\"spawn_mean_ms\":{spawns.Average().ToString(System.Globalization.CultureInfo.InvariantCulture)},\"spawn_max_ms\":{spawns.Max().ToString(System.Globalization.CultureInfo.InvariantCulture)}}}";
                Directory.CreateDirectory("Artifacts/S3-MOVEMENT-REACH-20261019");
                File.WriteAllText($"Artifacts/S3-MOVEMENT-REACH-20261019/spawn-{name}.json", report + "\n");
                UnityEngine.Debug.Log("S3_CREEP_SPAWN_MEASURE " + name + " " + report);
            }
            finally { group.ResetRun(); }
        }
    }
}
