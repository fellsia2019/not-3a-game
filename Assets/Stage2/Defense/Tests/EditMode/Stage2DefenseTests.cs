using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Defense.Tests
{
    public sealed class Stage2DefenseTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2Inventory inventory;
        private Stage2DefenseCatalog catalog;
        private Stage2Defense defense;
        private Vector2 heroWorld;
        private Vector2Int cell;
        private int created;
        private int facts;

        [SetUp]
        public void SetUp()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            Assert.That(map, Is.Not.Null);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.TryValidate(out var reason), Is.True, reason);
            navigation = new Stage2Navigation(map);
            inventory = new Stage2Inventory();
            heroWorld = map.CellToWorld(map.Throne);
            cell = FirstBuildable();
            created = facts = 0;
            defense = new Stage2Defense(map, navigation, inventory, catalog,
                () => heroWorld, () => { created++; return Object.Instantiate(catalog.TowerPrefab); });
            defense.Built += _ => facts++;
            defense.SetRunState(Stage2BuildPhase.Preparation, false);
        }

        [TearDown]
        public void TearDown() => defense?.ResetRun();

        [Test]
        public void Confirm_CommitsOneTowerFullPriceAndOneOccupancyRevision()
        {
            Fund(2);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(cell);
            Assert.That(defense.EvaluatePreview(), Is.EqualTo(PlacementResult.Ready));
            var revision = navigation.Revision;
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(tower.IsAlive && tower.gameObject.activeInHierarchy, Is.True);
            Assert.That(tower.Health, Is.EqualTo(catalog.TowerHealth));
            Assert.That(navigation.TryGetOccupant(cell, out var owner), Is.True);
            Assert.That(owner, Is.SameAs(tower));
            Assert.That(navigation.Revision, Is.EqualTo(revision + 1));
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(facts, Is.EqualTo(1));
            AssertBalances(1);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            Assert.That(created, Is.EqualTo(1));
            Assert.That(facts, Is.EqualTo(1));
            AssertBalances(1);
        }

        [Test]
        public void CancelPauseTerminalAndRestart_DiscardPreviewWithoutPayment()
        {
            Fund(1);
            var initial = navigation.Revision;
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            defense.CancelPlacement();
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            defense.SetRunState(Stage2BuildPhase.Wave, false);
            Assert.That(defense.IsPlacing, Is.True, "Wave start must preserve preview.");
            defense.SetRunState(Stage2BuildPhase.Wave, true);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            defense.SetRunState(Stage2BuildPhase.Wave, false);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            defense.SetRunState(Stage2BuildPhase.Inactive, false);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            defense.ResetRun();
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            Assert.That(navigation.Revision, Is.EqualTo(initial));
            Assert.That(facts, Is.Zero);
            AssertBalances(1);
        }

        [Test]
        public void FullCostAndStalePreview_RejectWithoutSpendOrRegistration()
        {
            inventory.TryAdd(MapResourceKind.Wood, 1);
            inventory.TryAdd(MapResourceKind.Stone, 1);
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.CannotAfford));
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(navigation.Revision, Is.Zero);
            inventory.TryAdd(MapResourceKind.Metal, 1);
            Assert.That(defense.EvaluatePreview(), Is.EqualTo(PlacementResult.Ready));
            inventory.TrySpend(0, 0, 1);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.CannotAfford));
            Assert.That(created, Is.Zero);
            Assert.That(facts, Is.Zero);
            Assert.That(navigation.Revision, Is.Zero);
        }

        [Test]
        public void MapReservationsExistingTowerAndHero_RejectWithNoSideEffects()
        {
            Fund(2);
            defense.BeginPlacement();
            foreach (var forbidden in new[] { map.Throne, map.EnemyEntrance,
                map.GetResourceRegion(0).Anchor, new Vector2Int(map.MaximumCell.x + 1, map.MaximumCell.y) })
            {
                defense.SetPreviewCell(forbidden);
                Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.InvalidCell));
            }
            heroWorld = map.CellToWorld(cell);
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.HeroProtected));
            heroWorld = map.CellToWorld(map.Throne);
            var other = new object();
            Assert.That(navigation.TryOccupy(cell, other), Is.EqualTo(OccupancyResult.Changed));
            var revision = navigation.Revision;
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.Occupied));
            Assert.That(navigation.Revision, Is.EqualTo(revision));
            Assert.That(navigation.TryRelease(cell, other), Is.EqualTo(OccupancyResult.Changed));
            Assert.That(created, Is.Zero);
            Assert.That(facts, Is.Zero);
            AssertBalances(2);
        }

        [Test]
        public void WaveProtection_FailsClosedAndRechecksCurrentStepAtConfirm()
        {
            Fund(1);
            defense.SetRunState(Stage2BuildPhase.Wave, false);
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.EnemyStateUnavailable));
            var protectedCell = false;
            defense.SetEnemyProtection(candidate => protectedCell && candidate == cell);
            Assert.That(defense.EvaluatePreview(), Is.EqualTo(PlacementResult.Ready));
            protectedCell = true;
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.EnemyProtected));
            Assert.That(created, Is.Zero);
            Assert.That(navigation.Revision, Is.Zero);
            AssertBalances(1);
            protectedCell = false;
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(tower, Is.Not.Null);
        }

        [Test]
        public void CreationFailureAndLateOccupancyContention_KeepFullPrice()
        {
            Fund(1);
            var failure = new Stage2Defense(map, navigation, inventory, catalog,
                () => heroWorld, () => null);
            failure.SetRunState(Stage2BuildPhase.Preparation, false);
            failure.BeginPlacement();
            failure.SetPreviewCell(cell);
            Assert.That(failure.TryConfirm(out _), Is.EqualTo(PlacementResult.CreationFailed));
            Assert.That(navigation.Revision, Is.Zero);
            AssertBalances(1);

            var competitor = new object();
            var contended = new Stage2Defense(map, navigation, inventory, catalog,
                () => heroWorld, () =>
                {
                    navigation.TryOccupy(cell, competitor);
                    return Object.Instantiate(catalog.TowerPrefab);
                });
            contended.SetRunState(Stage2BuildPhase.Preparation, false);
            contended.BeginPlacement();
            contended.SetPreviewCell(cell);
            Assert.That(contended.TryConfirm(out _), Is.EqualTo(PlacementResult.Occupied));
            Assert.That(navigation.TryGetOccupant(cell, out var owner), Is.True);
            Assert.That(owner, Is.SameAs(competitor));
            Assert.That(navigation.Revision, Is.EqualTo(1), "Only the competing registration changed occupancy.");
            AssertBalances(1);
            navigation.TryRelease(cell, competitor);
        }

        [Test]
        public void DeathReleasesIdentity_ReplacementSurvivesStaleRelease_AndResetClearsRun()
        {
            Fund(2);
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out var first), Is.EqualTo(PlacementResult.Ready));
            Assert.That(first.TakeDamage(catalog.TowerHealth), Is.True);
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(navigation.TryGetOccupant(cell, out _), Is.False);
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out var second), Is.EqualTo(PlacementResult.Ready));
            var revision = navigation.Revision;
            Assert.That(navigation.TryRelease(cell, first), Is.EqualTo(OccupancyResult.OwnerMismatch));
            Assert.That(navigation.Revision, Is.EqualTo(revision));
            Assert.That(navigation.TryGetOccupant(cell, out var owner), Is.True);
            Assert.That(owner, Is.SameAs(second));
            defense.ResetRun();
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(navigation.OccupiedCount, Is.Zero);
            Assert.That(defense.IsPlacing, Is.False);
            Assert.That(facts, Is.EqualTo(2));
            AssertBalances(0);
        }

        private Vector2Int FirstBuildable()
        {
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
                for (var x = map.MinimumCell.x; x <= map.MaximumCell.x; x++)
                {
                    var candidate = new Vector2Int(x, y);
                    if (map.IsBuildable(candidate) && map.IsWalkable(candidate)) return candidate;
                }
            Assert.Fail("Map has no buildable cell.");
            return default;
        }

        private void Fund(int amount)
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                inventory.TryAdd(kind, amount);
        }

        private void AssertBalances(int amount)
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                Assert.That(inventory.Get(kind), Is.EqualTo(amount));
        }
    }
}
