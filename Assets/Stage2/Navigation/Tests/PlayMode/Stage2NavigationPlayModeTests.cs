#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Not3A.Stage2.Navigation.Tests
{
    public sealed class Stage2NavigationPlayModeTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;

        [SetUp]
        public void SetUp()
        {
            map = NavigationFixture.LoadCandidate();
            navigation = new Stage2Navigation(map);
        }

        [UnityTest]
        public IEnumerator MidWavePlacement_ChangesNextRouteAndReachesThrone()
        {
            var consumer = new NavigationTestConsumer(navigation, map.EnemyEntrance);
            consumer.Tick();
            yield return null;
            var previous = consumer.Plan;
            var next = previous.Path[2];
            Assert.That(navigation.TryOccupy(next, new object()), Is.EqualTo(OccupancyResult.Changed));
            consumer.Tick();
            Assert.That(consumer.Plan, Is.Not.SameAs(previous));
            CollectionAssert.DoesNotContain(consumer.Plan.Path, next);
            yield return Finish(consumer);
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator PlacementOnSharedNextCell_ReplansAllConsumersWithoutStuckState()
        {
            var consumers = new NavigationTestConsumer[24];
            for (var i = 0; i < consumers.Length; i++)
            {
                consumers[i] = new NavigationTestConsumer(navigation, map.EnemyEntrance);
                consumers[i].Tick(false);
            }
            var next = consumers[0].Plan.Path[1];
            Assert.That(navigation.TryOccupy(next, new object()), Is.EqualTo(OccupancyResult.Changed));
            for (var frame = 0; frame < 110; frame++)
            {
                foreach (var consumer in consumers)
                {
                    consumer.Tick();
                    Assert.That(consumer.Cell, Is.Not.EqualTo(next));
                }
                yield return null;
            }
            foreach (var consumer in consumers) Assert.That(consumer.AtThrone, Is.True);
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(2));
            Assert.That(navigation.Diagnostics.ReplanCount, Is.EqualTo(consumers.Length * 2));
        }

        [UnityTest]
        public IEnumerator FullBlock_ReturnsReachableBlockerAndWaitsWithoutNewSearch()
        {
            NavigationFixture.Wall(navigation, map, 0);
            var consumer = new NavigationTestConsumer(navigation, map.EnemyEntrance);
            for (var i = 0; i < 70; i++)
            {
                consumer.Tick();
                yield return null;
            }
            Assert.That(consumer.AtBlocker, Is.True);
            Assert.That(consumer.Plan.BlockerCell, Is.EqualTo(Vector2Int.zero));
            Assert.That(NavigationFixture.Distance(consumer.Cell, consumer.Plan.BlockerCell.Value), Is.EqualTo(1));
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SimulatedDestruction_ReleasesOwnedCellAndConsumerResumes()
        {
            NavigationFixture.Wall(navigation, map, 0);
            var consumer = new NavigationTestConsumer(navigation, map.EnemyEntrance);
            for (var i = 0; i < 55; i++)
            {
                consumer.Tick();
                yield return null;
            }
            Assert.That(consumer.AtBlocker, Is.True);
            var blocked = consumer.Plan;
            var revision = navigation.Revision;
            Assert.That(navigation.TryRelease(blocked.BlockerCell.Value, new object()), Is.EqualTo(OccupancyResult.OwnerMismatch));
            Assert.That(navigation.TryRelease(blocked.BlockerCell.Value, blocked.BlockerOwner), Is.EqualTo(OccupancyResult.Changed));
            Assert.That(navigation.TryRelease(blocked.BlockerCell.Value, blocked.BlockerOwner), Is.EqualTo(OccupancyResult.Empty));
            Assert.That(navigation.Revision, Is.EqualTo(revision + 1));
            Assert.That(navigation.TryGetOccupant(blocked.BlockerCell.Value, out _), Is.False);
            yield return Finish(consumer);
            Assert.That(consumer.Plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator RestartFromSiege_ClearsOccupancyAndPriorRunCache()
        {
            NavigationFixture.Wall(navigation, map, 0);
            var old = navigation.PlanFrom(map.EnemyEntrance);
            var revision = navigation.Revision;
            navigation.Reset();
            Assert.That(navigation.OccupiedCount, Is.Zero);
            Assert.That(navigation.Revision, Is.EqualTo(revision + 1));
            Assert.That(navigation.IsCurrent(old), Is.False);
            var consumer = new NavigationTestConsumer(navigation, map.EnemyEntrance);
            yield return Finish(consumer);
            var clean = consumer.Plan;
            navigation.Reset();
            Assert.That(navigation.IsCurrent(clean), Is.False);
            Assert.That(navigation.Revision, Is.EqualTo(revision + 1));
            consumer.Tick(false);
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ManyMovingConsumers_WithDifferentStartsShareOneSearchFor128Frames()
        {
            var consumers = new NavigationTestConsumer[64];
            for (var i = 0; i < consumers.Length; i++)
                consumers[i] = new NavigationTestConsumer(navigation, new Vector2Int(49, -32 + i));
            for (var frame = 0; frame < 128; frame++)
            {
                foreach (var consumer in consumers) consumer.Tick();
                yield return null;
            }
            foreach (var consumer in consumers) Assert.That(consumer.AtThrone, Is.True);
            Assert.That(navigation.Diagnostics.FullSearchCount, Is.EqualTo(1));
            Assert.That(navigation.Diagnostics.RequestCount, Is.EqualTo(64));
            Assert.That(navigation.Diagnostics.FieldReuseCount, Is.EqualTo(63));
            Assert.That(navigation.Diagnostics.PlanCacheHitCount, Is.EqualTo(64 * 127));
        }

        [UnityTest]
        public IEnumerator Diagnostics_RecordEditorPlayModeMeasurements()
        {
            yield return null;
            NavigationFixture.RecordMeasurements("PlayMode");
            yield return null;
        }

        private static IEnumerator Finish(NavigationTestConsumer consumer)
        {
            for (var i = 0; i < 200 && !consumer.AtThrone; i++)
            {
                consumer.Tick();
                yield return null;
            }
            Assert.That(consumer.AtThrone, Is.True, "Consumer must make finite progress to throne.");
        }
    }
}
#endif
