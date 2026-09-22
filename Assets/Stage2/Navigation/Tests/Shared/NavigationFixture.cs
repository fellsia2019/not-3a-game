#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Navigation.Tests
{
    public static class NavigationFixture
    {
        public static Stage2MapDefinition LoadCandidate()
        {
            var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(
                "Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            Assert.That(map, Is.Not.Null);
            Assert.That(map.ValidateDefinition().IsValid, Is.True);
            return map;
        }

        public static void Wall(Stage2Navigation navigation, Stage2MapDefinition map, int x,
            int? gap = null, bool reverse = false)
        {
            for (var offset = 0; offset < map.Size.y; offset++)
            {
                var y = reverse ? map.MaximumCell.y - offset : map.MinimumCell.y + offset;
                if (y == gap) continue;
                var cell = new Vector2Int(x, y);
                if (!map.IsWalkable(cell)) continue;
                Assert.That(navigation.TryOccupy(cell, new object()), Is.EqualTo(OccupancyResult.Changed));
            }
        }

        public static void Zigzag(Stage2Navigation navigation, Stage2MapDefinition map)
        {
            Wall(navigation, map, 25, map.MaximumCell.y);
            Wall(navigation, map, 0, map.MinimumCell.y);
            Wall(navigation, map, -25, map.MaximumCell.y);
        }

        // Authored-mask variants are transient clones, never edited/saved source assets.
        public static Stage2MapDefinition MaskVariant(Stage2MapDefinition source, MapCellArea obstacle)
        {
            var copy = UnityEngine.Object.Instantiate(source);
            var serialized = new SerializedObject(copy);
            foreach (var name in new[] { "walkableMask", "buildableMask" })
            {
                var disabled = serialized.FindProperty(name).FindPropertyRelative("disabledAreas");
                var slot = disabled.arraySize;
                disabled.InsertArrayElementAtIndex(slot);
                var area = disabled.GetArrayElementAtIndex(slot);
                area.FindPropertyRelative("minimum").vector2IntValue = obstacle.Minimum;
                area.FindPropertyRelative("size").vector2IntValue = obstacle.Size;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(copy.ValidateDefinition().IsValid, Is.True);
            return copy;
        }

        public static void AssertPath(Stage2Navigation navigation, Stage2MapDefinition map,
            NavigationPlan plan, Vector2Int start)
        {
            Assert.That(navigation.IsCurrent(plan), Is.True);
            Assert.That(plan.Path.Count, Is.GreaterThan(0));
            Assert.That(plan.Path[0], Is.EqualTo(start));
            var unique = new HashSet<Vector2Int>();
            for (var i = 0; i < plan.Path.Count; i++)
            {
                var cell = plan.Path[i];
                Assert.That(map.Contains(cell) && map.IsWalkable(cell), Is.True, cell.ToString());
                Assert.That(navigation.TryGetOccupant(cell, out _), Is.False, cell.ToString());
                Assert.That(unique.Add(cell), Is.True, "Route must not cycle.");
                if (i > 0) Assert.That(Distance(plan.Path[i - 1], cell), Is.EqualTo(1));
            }
            var last = plan.Path[plan.Path.Count - 1];
            if (plan.Status == NavigationStatus.ReachesThrone)
            {
                Assert.That(last, Is.EqualTo(map.Throne));
                Assert.That(plan.BlockerCell, Is.Null);
            }
            else
            {
                Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachableBlocker));
                Assert.That(plan.BlockerCell.HasValue, Is.True);
                Assert.That(Distance(last, plan.BlockerCell.Value), Is.EqualTo(1));
                Assert.That(navigation.TryGetOccupant(plan.BlockerCell.Value, out var owner), Is.True);
                Assert.That(plan.BlockerOwner, Is.SameAs(owner));
            }
        }

        public static int Distance(Vector2Int a, Vector2Int b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        public static void RecordMeasurements(string mode)
        {
            const int warmups = 3;
            const int samples = 30;
            var map = LoadCandidate();
            var evidence = new BenchmarkEvidence
            {
                mode = mode, unity = Application.unityVersion, utc = DateTime.UtcNow.ToString("O"),
                cpu = SystemInfo.processorType, logicalProcessors = SystemInfo.processorCount,
                memoryMB = SystemInfo.systemMemorySize, gpu = SystemInfo.graphicsDeviceName,
                os = SystemInfo.operatingSystem, mapHash = map.ComputeDeterministicContentHash(),
                warmups = warmups, samples = samples, scenarios = new ScenarioEvidence[3]
            };
            for (var scenario = 0; scenario < 3; scenario++)
            {
                var row = new ScenarioEvidence
                {
                    name = new[] { "open", "zigzag", "full-wall" }[scenario],
                    searchMs = new double[samples], requestMs = new double[samples],
                    replanMs = new double[samples], cacheMs = new double[samples]
                };
                for (var sample = -warmups; sample < samples; sample++)
                {
                    var nav = new Stage2Navigation(map);
                    if (scenario == 1) Zigzag(nav, map);
                    if (scenario == 2) Wall(nav, map, 0);
                    var first = nav.ReplanIfStale(map.EnemyEntrance, null);
                    AssertPath(nav, map, first, map.EnemyEntrance);
                    Assert.That(nav.Diagnostics.FullSearchCount, Is.EqualTo(1));
                    // Real occupancy edit invalidates the initial field. This remote cell
                    // is not on any measured route; it changes revision, not scenario shape.
                    Assert.That(nav.TryOccupy(new Vector2Int(-49, 49), new object()), Is.EqualTo(OccupancyResult.Changed));
                    var replanned = nav.ReplanIfStale(map.EnemyEntrance, first);
                    var reused = nav.ReplanIfStale(map.EnemyEntrance, replanned);
                    Assert.That(reused, Is.SameAs(replanned));
                    Assert.That(nav.Diagnostics.FullSearchCount, Is.EqualTo(2));
                    Assert.That(nav.Diagnostics.ReplanCount, Is.EqualTo(2));
                    Assert.That(nav.Diagnostics.PlanCacheHitCount, Is.EqualTo(1));
                    if (sample < 0) continue;
                    row.searchMs[sample] = nav.Diagnostics.LastSearchMilliseconds;
                    row.requestMs[sample] = nav.Diagnostics.LastRequestMilliseconds;
                    row.replanMs[sample] = nav.Diagnostics.LastReplanMilliseconds;
                    row.cacheMs[sample] = nav.Diagnostics.LastCacheMilliseconds;
                    row.pathCells = replanned.Path.Count;
                    row.occupiedCells = nav.OccupiedCount;
                    row.expandedCells = nav.Diagnostics.LastExpandedCells;
                    row.fullSearchesPerSample = (int)nav.Diagnostics.FullSearchCount;
                    Assert.That(row.searchMs[sample], Is.GreaterThanOrEqualTo(0));
                }
                evidence.scenarios[scenario] = row;
            }
            var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Artifacts/S2-MAP-02"));
            Directory.CreateDirectory(directory);
            var output = Path.Combine(directory, mode + "-timings.json");
            File.WriteAllText(output, JsonUtility.ToJson(evidence, true));
            TestContext.WriteLine($"S2_NAV_MEASUREMENTS {output} map={evidence.mapHash} samples={samples}");
        }

        [Serializable]
        public sealed class BenchmarkEvidence
        {
            public string mode, unity, utc, cpu, gpu, os, mapHash;
            public int logicalProcessors, memoryMB, warmups, samples;
            public ScenarioEvidence[] scenarios;
        }

        [Serializable]
        public sealed class ScenarioEvidence
        {
            public string name;
            public int pathCells, occupiedCells, expandedCells, fullSearchesPerSample;
            public double[] searchMs, requestMs, replanMs, cacheMs;
        }
    }

    // Discrete movement harness only: no GameObjects, enemy stats, attack or spawning.
    public sealed class NavigationTestConsumer
    {
        private readonly Stage2Navigation navigation;
        private int pathIndex;
        public NavigationTestConsumer(Stage2Navigation navigation, Vector2Int start)
        {
            this.navigation = navigation;
            Cell = start;
        }
        public Vector2Int Cell { get; private set; }
        public NavigationPlan Plan { get; private set; }
        public bool AtThrone => Plan != null && Plan.Status == NavigationStatus.ReachesThrone && pathIndex >= Plan.Path.Count;
        public bool AtBlocker => Plan != null && Plan.Status == NavigationStatus.ReachableBlocker && pathIndex >= Plan.Path.Count;

        public void Tick(bool move = true)
        {
            var updated = navigation.ReplanIfStale(Cell, Plan);
            if (!ReferenceEquals(updated, Plan))
            {
                Plan = updated;
                pathIndex = 1;
            }
            Assert.That(Plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone).Or.EqualTo(NavigationStatus.ReachableBlocker));
            if (!move || pathIndex >= Plan.Path.Count) return;
            var nextCell = Plan.Path[pathIndex++];
            Assert.That(navigation.TryGetOccupant(nextCell, out _), Is.False);
            Assert.That(NavigationFixture.Distance(Cell, nextCell), Is.EqualTo(1));
            Cell = nextCell;
        }
    }
}
#endif
