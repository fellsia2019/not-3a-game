using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Economy.Tests
{
    public sealed class Stage2EconomyTests
    {
        private const string MapPath = "Assets/Stage2/Map/Data/Stage2Map100x100.asset";
        private const string CatalogPath = "Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset";
        private Stage2Economy economy;
        private Stage2EconomyCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(MapPath);
            catalog = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>(CatalogPath);
            Assert.That(map, Is.Not.Null);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.TryValidate(out var reason), Is.True, reason);
            economy = new Stage2Economy(map, catalog);
        }

        [Test]
        public void ThreeAuthoredSources_GatherExactlyOneFactAndRecoverIndependently()
        {
            var facts = 0;
            var expectedFacts = 0;
            economy.Gathered += (_, amount) => facts += amount;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var node = economy.GetNode(kind);
                expectedFacts += node.Capacity;
                Assert.That(node.Cell, Is.Not.EqualTo(Vector2Int.zero));
                var position = economy.WorldPosition(kind);
                for (var unit = 0; unit < node.Capacity; unit++)
                    Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds, true), Is.True);
                Assert.That(node.IsDepleted, Is.True);
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(node.Capacity));
                Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds, true), Is.False);
                Assert.That(node.RecoveryRemaining, Is.EqualTo(node.RecoverySeconds));
            }
            Assert.That(facts, Is.EqualTo(expectedFacts));
            economy.Tick(catalog.Get(MapResourceKind.Wood).RecoverySeconds / 2f, false);
            Assert.That(economy.GetNode(MapResourceKind.Wood).IsDepleted, Is.True);
            economy.Tick(catalog.Get(MapResourceKind.Wood).RecoverySeconds / 2f, true);
            Assert.That(economy.GetNode(MapResourceKind.Wood).IsDepleted, Is.True);
            economy.Tick(catalog.Get(MapResourceKind.Wood).RecoverySeconds / 2f, true);
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                Assert.That(economy.GetNode(kind).Remaining, Is.EqualTo(economy.GetNode(kind).Capacity));
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(economy.GetNode(kind).Capacity), "Recovery must not grant inventory.");
            }
            Assert.That(facts, Is.EqualTo(expectedFacts), "Recovery must not emit gather facts.");
            Assert.That(economy.AdvanceGather(MapResourceKind.Metal, economy.WorldPosition(MapResourceKind.Metal), true,
                economy.GetNode(MapResourceKind.Metal).GatherSeconds, true), Is.True);
            Assert.That(facts, Is.EqualTo(expectedFacts + 1), "Recovered nodes must be gatherable again.");
        }

        [Test]
        public void IndependentRecoveryTimers_DoNotRefillPartialStockOrAdvanceWhilePaused()
        {
            var transient = ScriptableObject.CreateInstance<Stage2EconomyCatalog>();
            try
            {
                transient.Configure(1.5f, new[]
                {
                    new Stage2ResourceSettings(MapResourceKind.Wood, 2, 0.5f, 2f),
                    new Stage2ResourceSettings(MapResourceKind.Stone, 1, 0.5f, 3f),
                    new Stage2ResourceSettings(MapResourceKind.Metal, 1, 0.5f, 4f)
                });
                var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(MapPath);
                var model = new Stage2Economy(map, transient);
                foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                    Assert.That(model.AdvanceGather(kind, model.WorldPosition(kind), true, 0.5f, true), Is.True);
                Assert.That(model.GetNode(MapResourceKind.Wood).Remaining, Is.EqualTo(1));
                Assert.That(model.GetNode(MapResourceKind.Wood).RecoveryRemaining, Is.Zero);
                Assert.That(model.AdvanceGather(MapResourceKind.Wood, model.WorldPosition(MapResourceKind.Wood), true, 0.5f, true), Is.True);
                model.Tick(10f, false);
                Assert.That(model.GetNode(MapResourceKind.Wood).IsDepleted, Is.True);
                model.Tick(2f, true);
                Assert.That(model.GetNode(MapResourceKind.Wood).Remaining, Is.EqualTo(2));
                Assert.That(model.GetNode(MapResourceKind.Stone).IsDepleted, Is.True);
                Assert.That(model.GetNode(MapResourceKind.Metal).IsDepleted, Is.True);
                model.Tick(1f, true);
                Assert.That(model.GetNode(MapResourceKind.Stone).Remaining, Is.EqualTo(1));
                Assert.That(model.GetNode(MapResourceKind.Metal).IsDepleted, Is.True);
                model.Tick(1f, true);
                Assert.That(model.GetNode(MapResourceKind.Metal).Remaining, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(transient); }
        }

        [Test]
        public void InterruptedOutOfRangeAndInactiveGather_NeverCreditsOrCarriesProgressAcrossRestart()
        {
            var kind = MapResourceKind.Stone;
            var node = economy.GetNode(kind);
            var position = economy.WorldPosition(kind);
            Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds / 2f, true), Is.False);
            Assert.That(economy.AdvanceGather(kind, position, false, node.GatherSeconds, true), Is.False);
            Assert.That(economy.GatherProgress, Is.Zero);
            Assert.That(economy.AdvanceGather(kind, position + Vector2.right * 10f, true, node.GatherSeconds, true), Is.False);
            Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds, false), Is.False);
            Assert.That(economy.Inventory.Get(kind), Is.Zero);
            Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds / 2f, true), Is.False);
            economy.Reset();
            Assert.That(economy.GatherProgress, Is.Zero);
            Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds / 2f, true), Is.False);
            Assert.That(economy.Inventory.Get(kind), Is.Zero);
            Assert.That(economy.AdvanceGather(kind, position, true, node.GatherSeconds / 2f, true), Is.True);
            Assert.That(economy.Inventory.Get(kind), Is.EqualTo(1));
        }

        [Test]
        public void Inventory_FullCostIsAtomicAndResetClearsAllBalancesAndTimers()
        {
            var inventory = economy.Inventory;
            Assert.That(inventory.Gold, Is.EqualTo(5));
            Assert.That(inventory.TryAdd(MapResourceKind.Wood, 2), Is.True);
            Assert.That(inventory.TryAdd(MapResourceKind.Stone, 1), Is.True);
            Assert.That(inventory.TryAdd(MapResourceKind.Metal, 1), Is.True);
            Assert.That(inventory.TrySpend(2, 1, 2), Is.False);
            Assert.That(inventory.TrySpend(2, 1, 1, 6), Is.False);
            Assert.That(inventory.Gold, Is.EqualTo(5));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(2));
            Assert.That(inventory.Get(MapResourceKind.Stone), Is.EqualTo(1));
            Assert.That(inventory.Get(MapResourceKind.Metal), Is.EqualTo(1));
            Assert.That(inventory.TrySpend(-1, 0, 0), Is.False);
            Assert.That(inventory.TrySpend(2, 1, 1, 5), Is.True);
            Assert.That(inventory.Gold, Is.Zero);
            Assert.That(inventory.TrySpend(2, 1, 1, 5), Is.False);
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var node = economy.GetNode(kind);
                for (var unit = 0; unit < node.Capacity; unit++)
                    economy.AdvanceGather(kind, economy.WorldPosition(kind), true, node.GatherSeconds, true);
            }
            economy.Tick(5f, true);
            economy.Reset();
            Assert.That(inventory.Gold, Is.EqualTo(5));
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                Assert.That(inventory.Get(kind), Is.Zero);
                Assert.That(economy.GetNode(kind).Remaining, Is.EqualTo(economy.GetNode(kind).Capacity));
                Assert.That(economy.GetNode(kind).RecoveryRemaining, Is.Zero);
            }
        }

        [Test]
        public void InvalidCatalog_ReportsMissingOrBrokenSourceWithoutInventingReward()
        {
            var transient = ScriptableObject.CreateInstance<Stage2EconomyCatalog>();
            try
            {
                transient.Configure(1.5f, new[] { new Stage2ResourceSettings(MapResourceKind.Wood, 1, 1f, 1f) });
                Assert.That(transient.TryValidate(out var missing), Is.False);
                Assert.That(missing, Does.Contain("Exactly one"));
                transient.Configure(1.5f, new[]
                {
                    new Stage2ResourceSettings(MapResourceKind.Wood, 1, 1f, 1f),
                    new Stage2ResourceSettings(MapResourceKind.Stone, 1, 1f, 1f),
                    new Stage2ResourceSettings(MapResourceKind.Metal, 1, float.NaN, 1f)
                });
                Assert.That(transient.TryValidate(out var invalid), Is.False);
                Assert.That(invalid, Does.Contain("finite"));
            }
            finally { Object.DestroyImmediate(transient); }
        }
    }
}
