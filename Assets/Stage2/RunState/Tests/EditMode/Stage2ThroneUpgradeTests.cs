using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.RunState.Tests
{
    public sealed class Stage2ThroneUpgradeTests
    {
        private Stage2MapDefinition map;
        private Stage2RunCatalog data;
        private Stage2Economy economy;
        private Stage2Defense defense;
        private Stage2EnemyGroup enemies;
        private Stage2Run run;
        [SetUp] public void Setup()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            var navigation = new Stage2Navigation(map);
            economy = new Stage2Economy(map, AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset"));
            var towers = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            defense = new Stage2Defense(map, navigation, economy.Inventory, towers, () => map.CellToWorld(map.Throne), () => Object.Instantiate(towers.TowerPrefab));
            enemies = new Stage2EnemyGroup(map, navigation, AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset"));
            defense.SetEnemyProtection(enemies.IsCellProtected);
            data = ScriptableObject.CreateInstance<Stage2RunCatalog>();
            data.Configure(12, 2, .25f, 5);
            data.ConfigureThroneUpgrades(new[] { new Stage2ResourceCost(2, 3, 4, 5), new Stage2ResourceCost(4, 6, 8, 10), new Stage2ResourceCost(8, 12, 16, 20) });
            run = new Stage2Run(data, navigation, economy, defense, enemies);
        }
        [TearDown] public void Cleanup()
        { run.Dispose(); enemies.ResetRun(); defense.ResetRun(); Object.DestroyImmediate(data); }
        private void Fund() => economy.Inventory.TryCredit(new Stage2ResourceCost(100, 100, 100, 100));
        private void Hit()
        { var enemy = enemies.SpawnAt(map.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
        private void Lose()
        { while (!run.IsTerminal) Hit(); Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost)); }
        [Test] public void ThreePaidStepsPreserveDamageAndCannotBuyAFourthOrEmitDamageOnUpgrade()
        {
            Fund(); run.StartWave(); Hit(); Hit(); Assert.That(run.ThroneHealth, Is.EqualTo(3));
            var damageEvents = 0; run.ThroneDamaged += _ => damageEvents++;
            for (var step = 0; step < 3; step++)
            {
                Assert.That(run.TryGetThroneUpgradeCost(out var cost), Is.True);
                var wood = economy.Inventory.Get(MapResourceKind.Wood); var stone = economy.Inventory.Get(MapResourceKind.Stone);
                var metal = economy.Inventory.Get(MapResourceKind.Metal); var gold = economy.Inventory.Gold;
                Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.Upgraded));
                Assert.That(run.ThroneUpgradeCount, Is.EqualTo(step + 1));
                Assert.That(run.ThroneMaximum, Is.EqualTo(5 + (step + 1) * 5));
                Assert.That(run.ThroneMaximum - run.ThroneHealth, Is.EqualTo(2));
                Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood - cost.Wood));
                Assert.That(economy.Inventory.Get(MapResourceKind.Stone), Is.EqualTo(stone - cost.Stone));
                Assert.That(economy.Inventory.Get(MapResourceKind.Metal), Is.EqualTo(metal - cost.Metal));
                Assert.That(economy.Inventory.Gold, Is.EqualTo(gold - cost.Gold));
            }
            var cappedGold = economy.Inventory.Gold;
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.MaximumUpgrades));
            Assert.That(run.TryGetThroneUpgradeCost(out _), Is.False);
            Assert.That(economy.Inventory.Gold, Is.EqualTo(cappedGold)); Assert.That(damageEvents, Is.Zero);
        }
        [Test] public void UnfundedPauseLossAndNestedRequestsNeverSpendOrResurrect()
        {
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.InsufficientResources));
            Fund(); var initialGold = economy.Inventory.Gold;
            run.SetPaused(true); Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.Inactive)); run.SetPaused(false);
            var nested = Stage2ThroneUpgradeResult.Ready;
            void TryNested() => nested = run.TryUpgradeThrone();
            economy.Inventory.Changed += TryNested;
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.Upgraded));
            economy.Inventory.Changed -= TryNested;
            Assert.That(nested, Is.EqualTo(Stage2ThroneUpgradeResult.Inactive));
            Assert.That(run.ThroneUpgradeCount, Is.EqualTo(1)); Assert.That(economy.Inventory.Gold, Is.EqualTo(initialGold - 5));
            run.StartWave(); Lose(); var gold = economy.Inventory.Gold;
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.Inactive));
            Assert.That(run.ThroneHealth, Is.Zero); Assert.That(economy.Inventory.Gold, Is.EqualTo(gold));
        }
        [Test] public void CurrentStage3RetryKeepsPostAnchorInvestmentRestoresUpgradedMaxAndFullResetClearsIt()
        {
            data.ConfigureContinuousDays(true); data.ConfigureDaytimeGates(true); Fund();
            run.Tick(12); Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.Upgraded));
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.Upgraded));
            var gold = economy.Inventory.Gold; var metal = economy.Inventory.Get(MapResourceKind.Metal);
            Lose(); Assert.That(run.RetryFailedWave(), Is.True);
            Assert.That(run.ClockHour, Is.EqualTo(8)); Assert.That(run.ThroneUpgradeCount, Is.EqualTo(2));
            Assert.That(run.ThroneHealth, Is.EqualTo(15)); Assert.That(run.ThroneMaximum, Is.EqualTo(15));
            Assert.That(economy.Inventory.Gold, Is.EqualTo(gold)); Assert.That(economy.Inventory.Get(MapResourceKind.Metal), Is.EqualTo(metal));
            run.ResetRun(); Assert.That(run.ThroneUpgradeCount, Is.Zero);
            Assert.That(run.ThroneHealth, Is.EqualTo(5)); Assert.That(run.ThroneMaximum, Is.EqualTo(5));
        }
        [Test] public void LegacySnapshotRestoresPreWaveUpgradeCountHealthAndWalletTogether()
        {
            Fund(); run.TryUpgradeThrone(); var gold = economy.Inventory.Gold;
            run.StartWave(); Hit(); run.TryUpgradeThrone(); Lose();
            Assert.That(run.RetryFailedWave(), Is.True);
            Assert.That(run.ThroneUpgradeCount, Is.EqualTo(1)); Assert.That(run.ThroneHealth, Is.EqualTo(10));
            Assert.That(run.ThroneMaximum, Is.EqualTo(10)); Assert.That(economy.Inventory.Gold, Is.EqualTo(gold));
        }
        [Test] public void CatalogRejectsWrongStepCountFreePriceAndOverflowWhileLegacyRemainsOptOut()
        {
            Assert.Throws<ArgumentException>(() => data.ConfigureThroneUpgrades(new Stage2ResourceCost[4]));
            data.ConfigureThroneUpgrades(new Stage2ResourceCost[3]); Assert.That(data.TryValidate(out _), Is.False);
            data.ConfigureThroneUpgrades(new[] { new Stage2ResourceCost(1, 0, 0), new Stage2ResourceCost(2, 0, 0), new Stage2ResourceCost(3, 0, 0) });
            data.Configure(12, 2, .25f, int.MaxValue); Assert.That(data.TryValidate(out _), Is.False);
            data.ConfigureThroneUpgrades(Array.Empty<Stage2ResourceCost>()); Assert.That(data.TryValidate(out _), Is.True);
            Assert.That(run.TryUpgradeThrone(), Is.EqualTo(Stage2ThroneUpgradeResult.MaximumUpgrades));
        }
    }
}
