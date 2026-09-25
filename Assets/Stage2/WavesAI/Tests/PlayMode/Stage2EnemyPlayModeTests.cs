#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.WavesAI.Tests
{
    public sealed class Stage2EnemyPlayModeTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2EnemyCatalog enemyCatalog;
        private Stage2DefenseCatalog defenseCatalog;
        private Stage2EnemyGroup group;
        private Stage2Defense defense;
        private Stage2Inventory inventory;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            enemyCatalog = AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
            defenseCatalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            navigation = new Stage2Navigation(map);
            inventory = new Stage2Inventory();
            defense = new Stage2Defense(map, navigation, inventory, defenseCatalog,
                () => map.CellToWorld(map.Throne), () => Object.Instantiate(defenseCatalog.TowerPrefab));
            defense.SetRunState(Stage2BuildPhase.Wave, false);
            group = new Stage2EnemyGroup(map, navigation, enemyCatalog);
            defense.SetEnemyProtection(group.IsCellProtected);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            group.ResetRun();
            defense.ResetRun();
            yield return null;
        }

        [UnityTest]
        public IEnumerator LiveUpdate_ProtectsStepDuringMovementAndClearsOnDeath()
        {
            var enemy = group.Spawn();
            var source = enemy.CurrentCell;
            yield return null;
            Assert.That(enemy.StepDestination.HasValue, Is.True);
            var destination = enemy.StepDestination.Value;
            Assert.That(group.IsCellProtected(source), Is.True);
            Assert.That(group.IsCellProtected(destination), Is.True);
            Assert.That(navigation.Revision, Is.Zero);
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                inventory.TryAdd(kind, 1);
            defense.BeginPlacement();
            defense.SetPreviewCell(destination);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.EnemyProtected));
            enemy.TakeDamage(enemyCatalog.Health);
            Assert.That(group.IsCellProtected(source), Is.False);
            Assert.That(group.IsCellProtected(destination), Is.False);
            yield return null;
            Assert.That(group.ActiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator SiegeDamage_ReleasesBlockerAndAllowsFreshRouteInPlayMode()
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                inventory.TryAdd(kind, 1);
            var cell = new Vector2Int(0, 0);
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
            {
                if (y == cell.y) continue;
                Assert.That(navigation.TryOccupy(new Vector2Int(0, y), new object()), Is.EqualTo(OccupancyResult.Changed));
            }
            var enemy = group.SpawnAt(new Vector2Int(1, 0));
            enemy.SetAutomaticTick(false);
            for (var index = 0; index < defenseCatalog.TowerHealth; index++)
                enemy.Tick(enemyCatalog.SiegeInterval);
            Assert.That(navigation.TryGetOccupant(cell, out _), Is.False);
            Assert.That(navigation.PlanFrom(enemy.CurrentCell).Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            yield return null;
            enemy.Tick(0.01f);
            Assert.That(enemy.StepDestination, Is.EqualTo(cell));
            Assert.That(group.IsCellProtected(cell), Is.True);
        }
    }
}
#endif
