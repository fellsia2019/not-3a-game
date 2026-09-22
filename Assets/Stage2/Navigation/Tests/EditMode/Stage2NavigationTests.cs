using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Navigation.Tests
{
    public sealed class Stage2NavigationTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;

        [SetUp]
        public void SetUp()
        {
            map = NavigationFixture.LoadCandidate();
            navigation = new Stage2Navigation(map);
        }

        [Test]
        public void Candidate_OpenRouteReachesAuthoredThroneWithoutMutatingAsset()
        {
            var hash = map.ComputeDeterministicContentHash();
            var plan = navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            NavigationFixture.AssertPath(navigation, map, plan, map.EnemyEntrance);
            Assert.That(plan.Path.Count - 1, Is.EqualTo(NavigationFixture.Distance(map.EnemyEntrance, map.Throne)));
            Assert.That(navigation.MapHash, Is.EqualTo(hash));
            Assert.That(map.ComputeDeterministicContentHash(), Is.EqualTo(hash));
        }

        [Test]
        public void EqualRoutes_ChooseLowerNextYThenXAcrossFreshInstances()
        {
            var start = map.EnemyEntrance + Vector2Int.up;
            var first = navigation.PlanFrom(start);
            var second = new Stage2Navigation(map).PlanFrom(start);
            Assert.That(first.Path[1], Is.EqualTo(map.EnemyEntrance));
            CollectionAssert.AreEqual(first.Path, second.Path);
            CollectionAssert.AreEqual(first.Path, navigation.PlanFrom(start).Path);
        }

        [Test]
        public void AuthoredMasks_RejectOccupancyAndExcludeNonWalkableCellsFromRoutes()
        {
            var obstacle = new MapCellArea(new Vector2Int(20, -2), new Vector2Int(2, 5));
            var changed = NavigationFixture.MaskVariant(map, obstacle);
            try
            {
                var nav = new Stage2Navigation(changed);
                var plan = nav.PlanFrom(changed.EnemyEntrance);
                NavigationFixture.AssertPath(nav, changed, plan, changed.EnemyEntrance);
                foreach (var cell in plan.Path) Assert.That(obstacle.Contains(cell), Is.False);
                Assert.That(nav.TryOccupy(obstacle.Minimum, new object()), Is.EqualTo(OccupancyResult.NotWalkable));
                Assert.That(nav.PlanFrom(obstacle.Minimum).Status, Is.EqualTo(NavigationStatus.StartNotWalkable));
                Assert.That(nav.Revision, Is.Zero);
            }
            finally { Object.DestroyImmediate(changed); }
        }

        [TestCase(int.MinValue, int.MinValue)]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(50, 0)]
        public void OutOfBounds_IsDiagnosticWithoutSearchOrException(int x, int y)
        {
            var cell = new Vector2Int(x, y);
            var plan = navigation.PlanFrom(cell);
            Assert.That(plan.Status, Is.EqualTo(NavigationStatus.StartOutsideBounds));
            Assert.That(plan.Diagnostic, Is.Not.Empty);
            Assert.That(plan.Path, Is.Empty);
            Assert.That(navigation.TryOccupy(cell, new object()), Is.EqualTo(OccupancyResult.OutsideBounds));
            Assert.That(navigation.TryRelease(cell, new object()), Is.EqualTo(OccupancyResult.OutsideBounds));
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.Zero);
        }

        [Test]
        public void MissingOrInvalidDefinition_IsDiagnosticAndCannotBeOccupied()
        {
            var invalid = ScriptableObject.CreateInstance<Stage2MapDefinition>();
            try
            {
                foreach (var definition in new[] { null, invalid })
                {
                    var nav = new Stage2Navigation(definition);
                    Assert.That(nav.IsValid, Is.False);
                    Assert.That(nav.PlanFrom(Vector2Int.zero).Status, Is.EqualTo(NavigationStatus.InvalidMap));
                    Assert.That(nav.ValidationDiagnostic, Is.Not.Empty);
                    Assert.That(nav.TryOccupy(Vector2Int.zero, new object()), Is.EqualTo(OccupancyResult.InvalidMap));
                    Assert.That(nav.TryRelease(Vector2Int.zero, new object()), Is.EqualTo(OccupancyResult.InvalidMap));
                    nav.Reset();
                    Assert.That(nav.Revision, Is.Zero);
                }
            }
            finally { Object.DestroyImmediate(invalid); }
        }

        [Test]
        public void Occupancy_AtomicIdentityAndExactRevisionIncludingStaleRelease()
        {
            var cell = Vector2Int.zero;
            var owner = new object();
            var replacement = new object();
            var hash = map.ComputeDeterministicContentHash();
            Assert.That(navigation.TryOccupy(cell, null), Is.EqualTo(OccupancyResult.InvalidOwner));
            Assert.That(navigation.Revision, Is.Zero);
            Assert.That(navigation.TryOccupy(cell, owner), Is.EqualTo(OccupancyResult.Changed));
            Assert.That(navigation.Revision, Is.EqualTo(1));
            Assert.That(navigation.TryOccupy(cell, owner), Is.EqualTo(OccupancyResult.AlreadyOccupied));
            Assert.That(navigation.TryOccupy(cell, replacement), Is.EqualTo(OccupancyResult.AlreadyOccupied));
            Assert.That(navigation.TryRelease(cell, replacement), Is.EqualTo(OccupancyResult.OwnerMismatch));
            Assert.That(navigation.TryRelease(cell, null), Is.EqualTo(OccupancyResult.InvalidOwner));
            Assert.That(navigation.Revision, Is.EqualTo(1));
            Assert.That(navigation.TryRelease(cell, owner), Is.EqualTo(OccupancyResult.Changed));
            Assert.That(navigation.Revision, Is.EqualTo(2));
            Assert.That(navigation.TryRelease(cell, owner), Is.EqualTo(OccupancyResult.Empty));
            Assert.That(navigation.Revision, Is.EqualTo(2));
            Assert.That(navigation.TryOccupy(cell, replacement), Is.EqualTo(OccupancyResult.Changed));
            Assert.That(navigation.TryRelease(cell, owner), Is.EqualTo(OccupancyResult.OwnerMismatch));
            Assert.That(navigation.Revision, Is.EqualTo(3));
            Assert.That(navigation.OccupiedCount, Is.EqualTo(1));
            Assert.That(navigation.TryGetOccupant(cell, out var actual), Is.True);
            Assert.That(actual, Is.SameAs(replacement));
            Assert.That(map.ComputeDeterministicContentHash(), Is.EqualTo(hash));
        }

        [Test]
        public void ReservedLandmarksAndAuthoredNonBuildableCells_AreRejected()
        {
            Assert.That(navigation.TryOccupy(map.EnemyEntrance, new object()), Is.EqualTo(OccupancyResult.ReservedLandmark));
            Assert.That(navigation.TryOccupy(map.Throne, new object()), Is.EqualTo(OccupancyResult.ReservedLandmark));
            for (var i = 0; i < map.ResourceRegionCount; i++)
                Assert.That(navigation.TryOccupy(map.GetResourceRegion(i).Anchor, new object()), Is.EqualTo(OccupancyResult.NotBuildable));
            Assert.That(navigation.Revision, Is.Zero);
        }

        [Test]
        public void OccupiedStart_IsExplicitInsteadOfWalkingThroughTower()
        {
            navigation.TryOccupy(Vector2Int.zero, new object());
            var result = navigation.PlanFrom(Vector2Int.zero);
            Assert.That(result.Status, Is.EqualTo(NavigationStatus.StartOccupied));
            Assert.That(result.Path, Is.Empty);
            Assert.That(result.Diagnostic, Is.Not.Empty);
        }

        [Test]
        public void ZigzagOnEntireCandidate_IsLongerAndDeterministic()
        {
            var open = navigation.PlanFrom(map.EnemyEntrance);
            NavigationFixture.Zigzag(navigation, map);
            var maze = navigation.PlanFrom(map.EnemyEntrance);
            NavigationFixture.AssertPath(navigation, map, maze, map.EnemyEntrance);
            Assert.That(maze.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(maze.Path.Count, Is.GreaterThan(open.Path.Count));
            CollectionAssert.AreEqual(maze.Path, navigation.PlanFrom(map.EnemyEntrance).Path);
        }

        [Test]
        public void FullWall_ReachableBlockerRemovalRestoresRoute()
        {
            NavigationFixture.Wall(navigation, map, 0);
            var plan = navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachableBlocker));
            Assert.That(plan.BlockerCell, Is.EqualTo(Vector2Int.zero));
            NavigationFixture.AssertPath(navigation, map, plan, map.EnemyEntrance);
            Assert.That(navigation.TryRelease(plan.BlockerCell.Value, plan.BlockerOwner), Is.EqualTo(OccupancyResult.Changed));
            Assert.That(navigation.IsCurrent(plan), Is.False);
            var restored = navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(restored.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            NavigationFixture.AssertPath(navigation, map, restored, map.EnemyEntrance);
        }

        [Test]
        public void EquivalentBlockers_TieBreakIgnoresRegistrationOrder()
        {
            var variant = NavigationFixture.MaskVariant(map, new MapCellArea(Vector2Int.zero, Vector2Int.one));
            try
            {
                var a = new Stage2Navigation(variant);
                var b = new Stage2Navigation(variant);
                NavigationFixture.Wall(a, variant, 0);
                NavigationFixture.Wall(b, variant, 0, reverse: true);
                var first = a.PlanFrom(variant.EnemyEntrance);
                var second = b.PlanFrom(variant.EnemyEntrance);
                Assert.That(first.BlockerCell, Is.EqualTo(new Vector2Int(0, -1)));
                Assert.That(second.BlockerCell, Is.EqualTo(first.BlockerCell));
                CollectionAssert.AreEqual(first.Path, second.Path);
                NavigationFixture.AssertPath(a, variant, first, variant.EnemyEntrance);
            }
            finally { Object.DestroyImmediate(variant); }
        }

        [Test]
        public void MultipleWalls_RemovalMakesProgressUntilThroneIsReachable()
        {
            NavigationFixture.Wall(navigation, map, 20);
            NavigationFixture.Wall(navigation, map, 0);
            NavigationFixture.Wall(navigation, map, -20);
            for (var i = 0; i < 3; i++)
            {
                var result = navigation.PlanFrom(map.EnemyEntrance);
                Assert.That(result.Status, Is.EqualTo(NavigationStatus.ReachableBlocker));
                NavigationFixture.AssertPath(navigation, map, result, map.EnemyEntrance);
                Assert.That(result.BlockerCell.Value.x, Is.EqualTo(20 - i * 20));
                Assert.That(navigation.TryRelease(result.BlockerCell.Value, result.BlockerOwner), Is.EqualTo(OccupancyResult.Changed));
            }
            Assert.That(navigation.PlanFrom(map.EnemyEntrance).Status, Is.EqualTo(NavigationStatus.ReachesThrone));
        }

        [Test]
        public void AuthoredDisconnection_DoesNotChooseAnIrrelevantOccupant()
        {
            var variant = NavigationFixture.MaskVariant(map,
                new MapCellArea(new Vector2Int(0, map.MinimumCell.y), new Vector2Int(1, map.Size.y)));
            try
            {
                var nav = new Stage2Navigation(variant);
                nav.TryOccupy(new Vector2Int(40, 0), new object());
                var plan = nav.PlanFrom(variant.EnemyEntrance);
                Assert.That(plan.Status, Is.EqualTo(NavigationStatus.Unreachable));
                Assert.That(plan.BlockerCell, Is.Null);
                Assert.That(plan.Diagnostic, Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(variant); }
        }

        [Test]
        public void Reset_ClearsOccupancyAndCacheWithoutReusingOldRevisionIdentity()
        {
            navigation.TryOccupy(Vector2Int.zero, new object());
            navigation.TryOccupy(Vector2Int.one, new object());
            var old = navigation.PlanFrom(map.EnemyEntrance);
            navigation.Reset();
            Assert.That(navigation.OccupiedCount, Is.Zero);
            Assert.That(navigation.Revision, Is.EqualTo(3));
            Assert.That(navigation.IsCurrent(old), Is.False);
            var clean = navigation.PlanFrom(map.EnemyEntrance);
            navigation.Reset();
            Assert.That(navigation.Revision, Is.EqualTo(3), "Empty reset is no occupancy change.");
            Assert.That(navigation.IsCurrent(clean), Is.False, "Run generation still invalidates empty-run plans.");
            navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(3));
        }

        [Test]
        public void RequestsAndConsumerCache_ShareOneFieldAcrossDifferentStarts()
        {
            var first = navigation.ReplanIfStale(map.EnemyEntrance, null);
            for (var i = 0; i < 100; i++)
            {
                Assert.That(navigation.ReplanIfStale(map.EnemyEntrance, first), Is.SameAs(first));
                var other = navigation.PlanFrom(new Vector2Int(map.MaximumCell.x, map.MinimumCell.y + i));
                Assert.That(other.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            }
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(1));
            Assert.That(navigation.Diagnostics.PlanCacheHitCount, Is.EqualTo(100));
            Assert.That(navigation.Diagnostics.FieldReuseCount, Is.EqualTo(100));
            Assert.That(navigation.Diagnostics.RequestCount, Is.EqualTo(101));
            Assert.That(new Stage2Navigation(map).IsCurrent(first), Is.False);
        }

        [Test]
        public void RevisionChange_InvalidatesOldImmutablePlanAndReplansOnce()
        {
            var first = navigation.PlanFrom(map.EnemyEntrance);
            var blockedCell = first.Path[1];
            var original = new List<Vector2Int>(first.Path);
            navigation.TryOccupy(blockedCell, new object());
            Assert.That(navigation.IsCurrent(first), Is.False);
            var next = navigation.ReplanIfStale(map.EnemyEntrance, first);
            CollectionAssert.DoesNotContain(next.Path, blockedCell);
            CollectionAssert.AreEqual(original, first.Path);
            Assert.Throws<NotSupportedException>(() => ((IList<Vector2Int>)first.Path)[0] = Vector2Int.zero);
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(2));
        }

        [Test]
        public void RejectedChanges_PreserveCurrentPlanAndSearchCount()
        {
            var plan = navigation.PlanFrom(map.EnemyEntrance);
            navigation.TryOccupy(map.Throne, new object());
            navigation.TryRelease(Vector2Int.zero, new object());
            Assert.That(navigation.IsCurrent(plan), Is.True);
            Assert.That(navigation.ReplanIfStale(map.EnemyEntrance, plan), Is.SameAs(plan));
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(1));
        }

        [Test]
        public void AlreadyAtThrone_ReturnsSingleCellRoute()
        {
            var plan = navigation.PlanFrom(map.Throne);
            Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(plan.Path.Count, Is.EqualTo(1));
            Assert.That(plan.Path[0], Is.EqualTo(map.Throne));
        }

        [Test]
        public void RandomOccupancy_OpenPathLengthMatchesIndependentBreadthFirstOracle()
        {
            var random = new System.Random(90222);
            for (var sample = 0; sample < 6; sample++)
            {
                navigation.Reset();
                var occupied = new HashSet<Vector2Int>();
                for (var i = 0; i < 2000; i++)
                {
                    var cell = new Vector2Int(random.Next(-50, 50), random.Next(-50, 50));
                    if (navigation.TryOccupy(cell, new object()) == OccupancyResult.Changed) occupied.Add(cell);
                }
                var expected = OpenDistance(map.EnemyEntrance, occupied);
                var plan = navigation.PlanFrom(map.EnemyEntrance);
                NavigationFixture.AssertPath(navigation, map, plan, map.EnemyEntrance);
                if (expected >= 0)
                {
                    Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
                    Assert.That(plan.Path.Count - 1, Is.EqualTo(expected));
                }
                else Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachableBlocker));
            }
        }

        [Test]
        public void Diagnostics_Record100x100SamplesWithoutPerformanceThreshold() =>
            NavigationFixture.RecordMeasurements("EditMode");

        private int OpenDistance(Vector2Int start, HashSet<Vector2Int> occupied)
        {
            var queue = new Queue<Vector2Int>();
            var distances = new Dictionary<Vector2Int, int> { [start] = 0 };
            queue.Enqueue(start);
            var directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (cell == map.Throne) return distances[cell];
                foreach (var direction in directions)
                {
                    var adjacent = cell + direction;
                    if (!map.Contains(adjacent) || !map.IsWalkable(adjacent) ||
                        occupied.Contains(adjacent) || distances.ContainsKey(adjacent)) continue;
                    distances.Add(adjacent, distances[cell] + 1);
                    queue.Enqueue(adjacent);
                }
            }
            return -1;
        }
    }
}
