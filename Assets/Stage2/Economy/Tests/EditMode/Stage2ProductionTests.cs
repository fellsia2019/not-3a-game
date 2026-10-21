using NUnit.Framework;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy.Tests
{
    public sealed class Stage2ProductionTests
    {
        private Stage2ProductionCatalog catalog;
        private Texture2D texture;
        private Sprite sprite;
        private Material material;
        private Stage2Inventory inventory;
        private Stage2Production rules;
        [SetUp] public void SetUp()
        {
            texture = new Texture2D(4, 4); sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
            material = new Material(Shader.Find("Sprites/Default")); catalog = ScriptableObject.CreateInstance<Stage2ProductionCatalog>();
            var entries = new Stage2ProductionEntry[3];
            for (var i = 0; i < 3; i++) entries[i] = new Stage2ProductionEntry { building = (ResourceProductionBuilding)i,
                displayName = "Test", woodCost = 2, stoneCost = 1, intervalSeconds = 5, outputAmount = 2, sprite = sprite,
                footprintSize = Vector2.one };
            catalog.Configure(entries, material); inventory = new Stage2Inventory();
            inventory.TryAdd(MapResourceKind.Wood, 20); inventory.TryAdd(MapResourceKind.Stone, 20);
            rules = new Stage2Production(catalog, inventory);
            rules.BindPoints(new[] { new ResourceProductionPoint(0, MapResourceKind.Wood, Vector2.zero, Vector2.one * 2, 0),
                new ResourceProductionPoint(1, MapResourceKind.Stone, Vector2.right * 8, Vector2.one * 2, 0) });
            rules.SetActive(true);
        }
        [TearDown] public void TearDown()
        { Object.DestroyImmediate(catalog); Object.DestroyImmediate(material); Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
        private ProductionPlacementResult Build(int index = 0, ResourceProductionBuilding kind = ResourceProductionBuilding.Sawmill, bool visual = true)
            => rules.TryBuild(index, kind, Vector2.one * 20, .2f, () => visual, out _);

        [Test] public void PurchaseSpendsOnceAndOccupiedOrMissingVisualNeverSpend()
        {
            Assert.That(Build(visual: false), Is.EqualTo(ProductionPlacementResult.MissingVisual));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(20));
            Assert.That(Build(), Is.EqualTo(ProductionPlacementResult.Built));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(18));
            Assert.That(inventory.Get(MapResourceKind.Stone), Is.EqualTo(19));
            Assert.That(Build(), Is.EqualTo(ProductionPlacementResult.Occupied));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(18));
            Assert.That(rules.Buildings.Count, Is.EqualTo(1));
        }
        [Test] public void InvalidTypeHeroAndInactiveRejectBeforeFactoryOrPayment()
        {
            var factoryCalls = 0;
            ProductionPlacementResult Try(int index, ResourceProductionBuilding kind, Vector2 hero) =>
                rules.TryBuild(index, kind, hero, .2f, () => { factoryCalls++; return true; }, out _);
            Assert.That(Try(-1, ResourceProductionBuilding.Sawmill, Vector2.one * 5), Is.EqualTo(ProductionPlacementResult.InvalidPoint));
            Assert.That(Try(1, ResourceProductionBuilding.Sawmill, Vector2.one * 5), Is.EqualTo(ProductionPlacementResult.WrongType));
            Assert.That(Try(0, ResourceProductionBuilding.Sawmill, new Vector2(.65f, 0)), Is.EqualTo(ProductionPlacementResult.HeroBlocked));
            rules.SetActive(false);
            Assert.That(Try(0, ResourceProductionBuilding.Sawmill, Vector2.one * 5), Is.EqualTo(ProductionPlacementResult.Inactive));
            Assert.That(factoryCalls, Is.Zero); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(20));
        }
        [Test] public void InsufficientFundsAndOversizedFootprintCannotPurchase()
        {
            inventory.Reset();
            Assert.That(Build(), Is.EqualTo(ProductionPlacementResult.InsufficientResources));
            catalog.Get(ResourceProductionBuilding.Sawmill).footprintSize = Vector2.one * 3;
            Assert.That(Build(), Is.EqualTo(ProductionPlacementResult.InvalidFootprint));
            Assert.That(rules.Buildings.Count, Is.Zero);
        }
        [Test] public void MissingSpriteCannotEnableInvisibleProduction()
        {
            catalog.Get(ResourceProductionBuilding.Sawmill).sprite = null;
            Assert.That(Build(), Is.EqualTo(ProductionPlacementResult.MissingVisual));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(20));
        }
        [Test] public void ActiveTimeCatchupRetainsFractionButPauseAndTerminalTimeDoNotAccumulate()
        {
            Build(); rules.Tick(12); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(22));
            Assert.That(rules.Buildings[0].ProgressSeconds, Is.EqualTo(2));
            rules.SetActive(false); rules.Tick(100); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(22));
            rules.SetActive(true); rules.Tick(3); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(24));
        }
        [Test] public void OverflowSaturatesWithoutWrappingOrBankingAnUnboundedBacklog()
        {
            Build(); inventory.TryAdd(MapResourceKind.Wood, int.MaxValue - inventory.Get(MapResourceKind.Wood) - 1);
            rules.Tick(float.MaxValue); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(int.MaxValue));
            inventory.TrySpend(10, 0, 0); rules.Tick(.01f);
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(int.MaxValue - 10));
        }
        [Test] public void SnapshotRestoresBuildingsAndFractionalProductionWithoutRepurchasingOrPayingIncome()
        {
            Build(); rules.Tick(4f);
            var saved = rules.CaptureSnapshot(); var inventorySaved = inventory.CaptureState();
            rules.Tick(11f); Build(1, ResourceProductionBuilding.Quarry);
            inventory.RestoreState(inventorySaved); rules.RestoreSnapshot(saved);
            Assert.That(rules.Buildings.Count, Is.EqualTo(1));
            Assert.That(rules.Buildings[0].ProgressSeconds, Is.EqualTo(4d));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(18));
            rules.Tick(1f); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(20));
        }
        [Test] public void ResetAndPointReplacementDiscardBuildingsAndProgressWithoutRefund()
        {
            Build(); rules.Tick(4); rules.Reset(); rules.Tick(20);
            Assert.That(rules.Buildings.Count, Is.Zero); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(18));
            Build(); rules.BindPoints(new[] { new ResourceProductionPoint(4, MapResourceKind.Stone, Vector2.one * 30, Vector2.one * 2, 0) });
            Assert.That(rules.Buildings.Count, Is.Zero); Assert.That(rules.Points[0].DistrictIndex, Is.EqualTo(4));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(16));
        }
        [Test] public void ThrowingVisualFactoryLeavesFundsAndPointUntouched()
        {
            var result = rules.TryBuild(0, ResourceProductionBuilding.Sawmill, Vector2.one * 20, .2f,
                () => throw new System.InvalidOperationException("Unavailable visual"), out _);
            Assert.That(result, Is.EqualTo(ProductionPlacementResult.MissingVisual));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(20));
            Assert.That(rules.Buildings.Count, Is.Zero);
            Assert.That(Build(), Is.EqualTo(ProductionPlacementResult.Built));
        }
        [Test] public void ReentrantPurchaseCannotSpendTwice()
        {
            var nested = ProductionPlacementResult.Ready;
            var result = rules.TryBuild(0, ResourceProductionBuilding.Sawmill, Vector2.one * 20, .2f,
                () => { nested = Build(); return true; }, out _);
            Assert.That(result, Is.EqualTo(ProductionPlacementResult.Built));
            Assert.That(nested, Is.EqualTo(ProductionPlacementResult.Inactive));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(18));
        }
        private void EnableUpgrades()
        {
            foreach (var entry in catalog.Entries)
            {
                entry.outputAmount = 1;
                entry.upgrades = new Stage2ProductionUpgrade[4];
                for (var i = 0; i < 4; i++) entry.upgrades[i] = new Stage2ProductionUpgrade {
                    cost = new Stage2ResourceCost(1, 1, 1, 2), outputAmount = i + 2 };
            }
            inventory.TryAdd(MapResourceKind.Metal, 50); inventory.TryAddGold(50);
        }
        [Test] public void UpgradePaysOncePreservesPartialCycleAndProducesOnlyAtItsNextBoundary()
        {
            EnableUpgrades(); Build(); var building = rules.Buildings[0]; var footprint = building.Footprint;
            rules.Tick(2.5f); var wood = inventory.Get(MapResourceKind.Wood); var gold = inventory.Gold;
            Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.Upgraded));
            Assert.That(building.Level, Is.EqualTo(2)); Assert.That(building.OutputAmount, Is.EqualTo(2));
            Assert.That(building.ProgressSeconds, Is.EqualTo(2.5d)); Assert.That(building.IntervalSeconds, Is.EqualTo(5));
            Assert.That(building.Footprint, Is.EqualTo(footprint));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood - 1)); Assert.That(inventory.Gold, Is.EqualTo(gold - 2));
            rules.Tick(2.49f); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood - 1));
            rules.Tick(.02f); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood + 1));
        }
        [Test] public void UpgradeRejectsUnfundedInactiveStaleAndMaximumWithoutSpending()
        {
            EnableUpgrades(); Build(); var building = rules.Buildings[0];
            inventory.Reset(); Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.InsufficientResources));
            inventory.TryCredit(new Stage2ResourceCost(50, 50, 50, 50));
            rules.SetActive(false); Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.Inactive));
            Assert.That(inventory.Gold, Is.EqualTo(50)); rules.SetActive(true);
            for (var i = 0; i < 4; i++) Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.Upgraded));
            Assert.That(building.Level, Is.EqualTo(5)); Assert.That(building.OutputAmount, Is.EqualTo(5));
            var gold = inventory.Gold; Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.MaximumLevel));
            Assert.That(inventory.Gold, Is.EqualTo(gold)); rules.Reset();
            Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.InvalidBuilding));
            Assert.That(inventory.Gold, Is.EqualTo(gold));
        }
        [Test] public void SameTypeBuildingsKeepIndependentLevelsAndSnapshotRestoresThemWithoutIncome()
        {
            EnableUpgrades(); rules.BindPoints(new[] {
                new ResourceProductionPoint(0, MapResourceKind.Wood, Vector2.zero, Vector2.one * 2, 0),
                new ResourceProductionPoint(1, MapResourceKind.Wood, Vector2.right * 8, Vector2.one * 2, 0) });
            Build(); Build(1); var first = rules.Buildings[0]; var second = rules.Buildings[1];
            rules.TryUpgrade(first); rules.Tick(2); var saved = rules.CaptureSnapshot();
            Assert.That(second.Level, Is.EqualTo(1)); Assert.That(second.OutputAmount, Is.EqualTo(1));
            rules.TryUpgrade(first); var gold = inventory.Gold; var wood = inventory.Get(MapResourceKind.Wood);
            rules.RestoreSnapshot(saved);
            Assert.That(rules.Buildings[0].Level, Is.EqualTo(2)); Assert.That(rules.Buildings[1].Level, Is.EqualTo(1));
            Assert.That(rules.Buildings[0].ProgressSeconds, Is.EqualTo(2));
            Assert.That(inventory.Gold, Is.EqualTo(gold)); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood));
            rules.Tick(3); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood + 3));
        }
        [Test] public void ReentrantUpgradeCannotSpendOrAdvanceTwiceAndCatalogRejectsBrokenSteps()
        {
            EnableUpgrades(); Build(); var building = rules.Buildings[0];
            var nested = ProductionUpgradeResult.Ready; var gold = inventory.Gold;
            inventory.Changed += () => nested = rules.TryUpgrade(building);
            Assert.That(rules.TryUpgrade(building), Is.EqualTo(ProductionUpgradeResult.Upgraded));
            Assert.That(nested, Is.EqualTo(ProductionUpgradeResult.Inactive));
            Assert.That(building.Level, Is.EqualTo(2)); Assert.That(inventory.Gold, Is.EqualTo(gold - 2));
            building.Entry.upgrades[1].outputAmount = 1;
            Assert.That(catalog.TryValidate(out _), Is.False);
        }
    }
}
