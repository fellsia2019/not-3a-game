using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Defense.Tests
{
    public sealed class Stage2TowerUpgradeTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2Inventory stock;
        private Stage2DefenseCatalog catalog, other;
        private Stage2Defense defense;
        private Vector2 hero;
        private readonly List<Vector2Int> cells = new List<Vector2Int>();
        [SetUp] public void Setup()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            var authored = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            catalog = ScriptableObject.CreateInstance<Stage2DefenseCatalog>();
            catalog.Configure(authored.TowerPrefab, 3, 5, 1, 10, 4.25f, .5f, 1, 3);
            catalog.ConfigureUpgrades(
                new Stage2TowerUpgrade(new Stage2ResourceCost(3, 5, 1, 3), 20, 5, .4f, 2),
                new Stage2TowerUpgrade(new Stage2ResourceCost(6, 10, 2, 6), 40, 6, .3f, 4));
            other = Object.Instantiate(catalog);
            navigation = new Stage2Navigation(map); stock = new Stage2Inventory();
            stock.TryCredit(new Stage2ResourceCost(500, 500, 500, 500));
            hero = map.CellToWorld(map.Throne);
            defense = new Stage2Defense(map, navigation, stock, new[] { catalog, other }, () => hero,
                data => Object.Instantiate(data.TowerPrefab));
            defense.SetRunState(Stage2BuildPhase.Preparation, false);
            cells.Clear();
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
            for (var x = map.MinimumCell.x; x <= map.MaximumCell.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (map.GetSurface(cell) == MapSurfaceKind.InvasionCorridor && map.IsWalkable(cell) &&
                    map.IsBuildable(cell) && cell != map.Throne && cell != map.EnemyEntrance) cells.Add(cell);
            }
        }
        [TearDown] public void Teardown()
        { defense.ResetRun(); Object.DestroyImmediate(catalog); Object.DestroyImmediate(other); }
        private Stage2Tower Build(int index, int role = 0)
        {
            defense.SelectRole(role); defense.BeginPlacement(); defense.SetPreviewCell(cells[index]);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready)); return tower;
        }
        [Test] public void PaidUpgradeSpendsExactPricePreservesDamageAndAttackReadiness()
        {
            var tower = Build(0); tower.TakeDamage(3);
            defense.SetAttackTarget((_, __, ___, ____, _____) => true); tower.Tick(.01f);
            var before = stock.Get(MapResourceKind.Wood);
            Assert.That(defense.TryUpgrade(tower), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(tower.Level, Is.EqualTo(2)); Assert.That(tower.Health, Is.EqualTo(14));
            Assert.That(tower.MaximumHealth, Is.EqualTo(20)); Assert.That(tower.AttackDamage, Is.EqualTo(2));
            Assert.That(tower.AttackRange, Is.EqualTo(5)); Assert.That(tower.CooldownRemaining, Is.EqualTo(.4f).Within(.0001f));
            Assert.That(stock.Get(MapResourceKind.Wood), Is.EqualTo(before - 3));
            Assert.That(tower.Invested.Gold, Is.EqualTo(6)); Assert.That(navigation.OccupiedCount, Is.EqualTo(1));
        }
        [Test] public void MergeNeedsNoStockUsesRecipientCellCombinesInvestmentAndHealth()
        {
            var donor = Build(0); var target = Build(1); var donorCell = donor.Cell; var targetCell = target.Cell;
            donor.TakeDamage(8); target.TakeDamage(2); stock.Reset();
            defense.SetAttackTarget((_, __, ___, ____, _____) => true); donor.Tick(.01f);
            var revision = navigation.Revision;
            Assert.That(defense.TryMerge(donor, target), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(target.Level, Is.EqualTo(2)); Assert.That(target.Health, Is.EqualTo(10));
            Assert.That(target.CooldownRemaining, Is.EqualTo(.4f).Within(.0001f));
            Assert.That(target.Cell, Is.EqualTo(targetCell)); Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(navigation.TryGetOccupant(donorCell, out _), Is.False);
            Assert.That(navigation.TryGetOccupant(targetCell, out var owner), Is.True); Assert.That(owner, Is.SameAs(target));
            Assert.That(navigation.Revision, Is.EqualTo(revision + 1));
            Assert.That(target.Invested.Wood, Is.EqualTo(6)); Assert.That(stock.Gold, Is.Zero);
            Assert.That(defense.TryMerge(donor, target), Is.EqualTo(Stage2TowerActionResult.InvalidTarget));
        }
        [Test] public void MismatchSelfAndMaximumLevelNeverConsumeDonorOrMoney()
        {
            var a = Build(0); var b = Build(1); var wrong = Build(2, 1);
            Assert.That(defense.TryMerge(a, a), Is.EqualTo(Stage2TowerActionResult.Incompatible));
            Assert.That(defense.TryMerge(a, wrong), Is.EqualTo(Stage2TowerActionResult.Incompatible));
            defense.TryUpgrade(a);
            Assert.That(defense.TryMerge(a, b), Is.EqualTo(Stage2TowerActionResult.Incompatible));
            defense.TryUpgrade(a); defense.TryUpgrade(b); defense.TryUpgrade(b);
            var before = stock.Gold;
            Assert.That(defense.TryUpgrade(a), Is.EqualTo(Stage2TowerActionResult.MaximumLevel));
            Assert.That(defense.TryMerge(a, b), Is.EqualTo(Stage2TowerActionResult.MaximumLevel));
            Assert.That(defense.TowerCount, Is.EqualTo(3)); Assert.That(stock.Gold, Is.EqualTo(before));
        }
        [Test] public void CannotAffordDoesNotChangeStatsOrAnyCurrency()
        {
            var tower = Build(0); stock.Reset(); stock.TryCredit(new Stage2ResourceCost(3, 4, 1, 3));
            Assert.That(defense.TryUpgrade(tower), Is.EqualTo(Stage2TowerActionResult.CannotAfford));
            Assert.That(tower.Level, Is.EqualTo(1)); Assert.That(stock.Get(MapResourceKind.Stone), Is.EqualTo(4));
            Assert.That(stock.Get(MapResourceKind.Wood), Is.EqualTo(3)); Assert.That(stock.Gold, Is.EqualTo(3));
        }
        [Test] public void SalePaysFloorHalfOfActualFourCurrencyInvestmentOnce()
        {
            var tower = Build(0); var cell = tower.Cell; stock.Reset();
            Assert.That(defense.TrySell(tower), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(stock.Get(MapResourceKind.Wood), Is.EqualTo(1)); Assert.That(stock.Get(MapResourceKind.Stone), Is.EqualTo(2));
            Assert.That(stock.Get(MapResourceKind.Metal), Is.Zero); Assert.That(stock.Gold, Is.EqualTo(1));
            Assert.That(defense.TowerCount, Is.Zero); Assert.That(navigation.TryGetOccupant(cell, out _), Is.False);
            Assert.That(defense.TrySell(tower), Is.EqualTo(Stage2TowerActionResult.InvalidTarget)); Assert.That(stock.Gold, Is.EqualTo(1));
        }
        [Test] public void SaleIncludesBothMergedTowersAndPaidUpgrades()
        {
            var a = Build(0); var b = Build(1); defense.TryMerge(a, b); defense.TryUpgrade(b); stock.Reset();
            Assert.That(defense.TrySell(b), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(stock.Get(MapResourceKind.Wood), Is.EqualTo(6)); Assert.That(stock.Get(MapResourceKind.Stone), Is.EqualTo(10));
            Assert.That(stock.Get(MapResourceKind.Metal), Is.EqualTo(2)); Assert.That(stock.Gold, Is.EqualTo(6));
        }
        [Test] public void RefundOverflowRefusesWholeSaleAndCreditWithoutPartialMutation()
        {
            var tower = Build(0); stock.Reset(); stock.TryAdd(MapResourceKind.Wood, int.MaxValue);
            Assert.That(defense.TrySell(tower), Is.EqualTo(Stage2TowerActionResult.CapacityExceeded));
            Assert.That(tower.IsAlive, Is.True); Assert.That(navigation.OccupiedCount, Is.EqualTo(1));
            Assert.That(stock.TryCredit(new Stage2ResourceCost(1, 2, 3, 4)), Is.False);
            Assert.That(stock.Get(MapResourceKind.Stone), Is.Zero); Assert.That(stock.Gold, Is.Zero);
        }
        [Test] public void ReentrantInventoryNotificationsCannotBuyOrSellAgain()
        {
            var tower = Build(0); var calls = 0;
            stock.Changed += () => { calls++; Assert.That(defense.TryUpgrade(tower), Is.EqualTo(Stage2TowerActionResult.Busy));
                Assert.That(defense.TrySell(tower), Is.EqualTo(Stage2TowerActionResult.Busy)); };
            defense.TryUpgrade(tower); Assert.That(tower.Level, Is.EqualTo(2)); Assert.That(calls, Is.EqualTo(1));
            defense.TrySell(tower); Assert.That(calls, Is.EqualTo(2)); Assert.That(defense.TowerCount, Is.Zero);
        }
        [Test] public void MoveAndInvalidDropsPreserveCombatStateAndOccupancy()
        {
            var tower = Build(0); var blocker = Build(1); tower.TakeDamage(4); defense.TryUpgrade(tower);
            var from = tower.Cell; var gold = stock.Gold;
            Assert.That(defense.TryMove(tower, blocker.Cell), Is.EqualTo(Stage2TowerActionResult.Occupied));
            hero = map.CellToWorld(cells[2]); Assert.That(defense.TryMove(tower, cells[2]), Is.EqualTo(Stage2TowerActionResult.HeroProtected));
            hero = map.CellToWorld(map.Throne);
            Assert.That(defense.TryMove(tower, cells[2]), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(tower.Health, Is.EqualTo(12)); Assert.That(tower.Level, Is.EqualTo(2)); Assert.That(stock.Gold, Is.EqualTo(gold));
            Assert.That(navigation.TryGetOccupant(from, out _), Is.False); Assert.That(navigation.TryGetOccupant(cells[2], out _), Is.True);
            defense.SetRunState(Stage2BuildPhase.Wave, false);
            Assert.That(defense.TryMove(tower, cells[3]), Is.EqualTo(Stage2TowerActionResult.PreparationOnly));
            Assert.That(defense.TryUpgrade(tower), Is.EqualTo(Stage2TowerActionResult.Ready));
        }
        [Test] public void PausedInactiveAndStaleTargetsCannotMutate()
        {
            var tower = Build(0); defense.SetRunState(Stage2BuildPhase.Preparation, true);
            Assert.That(defense.TrySell(tower), Is.EqualTo(Stage2TowerActionResult.Paused));
            Assert.That(defense.TryUpgrade(tower), Is.EqualTo(Stage2TowerActionResult.Paused));
            defense.SetRunState(Stage2BuildPhase.Inactive, false);
            Assert.That(defense.TrySell(tower), Is.EqualTo(Stage2TowerActionResult.Inactive));
            Assert.That(tower.Level, Is.EqualTo(1)); Assert.That(defense.TowerCount, Is.EqualTo(1));
        }
        [Test] public void SnapshotRestoresLevelsInvestmentsCellsHealthCooldownAndOriginalEconomy()
        {
            var a = Build(0); var b = Build(1); defense.TryUpgrade(a); a.TakeDamage(6);
            defense.SetAttackTarget((_, __, ___, ____, _____) => true); a.Tick(.01f);
            var snapshot = defense.CaptureSnapshot(); var money = stock.CaptureState(); var initialGold = stock.Gold;
            defense.TryUpgrade(b); defense.TryMerge(b, a); defense.TryMove(a, cells[3]); defense.TrySell(a);
            stock.RestoreState(money); Assert.That(defense.RestoreSnapshot(snapshot), Is.True);
            var restored = defense.GetTower(0);
            Assert.That(restored.Level, Is.EqualTo(2)); Assert.That(restored.Health, Is.EqualTo(14));
            Assert.That(restored.Cell, Is.EqualTo(cells[0])); Assert.That(restored.Invested.Gold, Is.EqualTo(6));
            Assert.That(restored.CooldownRemaining, Is.EqualTo(.4f).Within(.0001f)); Assert.That(stock.Gold, Is.EqualTo(initialGold));
            Assert.That(defense.GetTower(1).Level, Is.EqualTo(1)); Assert.That(navigation.OccupiedCount, Is.EqualTo(2));
        }
        [Test] public void ActiveStage3CatalogsHaveResourceOnlyPricesAndExpandableValidTiers()
        {
            foreach (var path in new[] {"Assets/Stage3/CartoonWorldPilot/RapidCartoonCatalog.asset", "Assets/Stage3/CartoonWorldPilot/LongshotCartoonCatalog.asset", "Assets/Stage3/Barricades/WoodenBarricadeCatalog.asset"})
            {
                var data = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(path);
                Assert.That(data.TryValidate(out var reason), Is.True, reason); Assert.That(data.GoldCost, Is.Zero);
                Assert.That(data.MaximumLevel, Is.GreaterThan(1));
                for (var level = 1; level < data.MaximumLevel; level++)
                { Assert.That(data.TryGetUpgrade(level, out var tier), Is.True); Assert.That(tier.Cost.Gold, Is.Zero); }
            }
        }
    }
}
