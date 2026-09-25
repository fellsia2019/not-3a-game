using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Defense.Tests
{
    public sealed class Stage2TowerCombatTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2DefenseCatalog catalog;
        private Stage2Inventory inventory;
        private Stage2Defense defense;
        private Stage2EnemyGroup enemies;

        [SetUp]
        public void SetUp()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            var enemyData = AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
            Assert.That(catalog.TryValidate(out var reason), Is.True, reason);
            navigation = new Stage2Navigation(map);
            inventory = new Stage2Inventory();
            defense = new Stage2Defense(map, navigation, inventory, catalog,
                () => map.CellToWorld(map.Throne), () => Object.Instantiate(catalog.TowerPrefab));
            enemies = new Stage2EnemyGroup(map, navigation, enemyData);
            defense.SetRunState(Stage2BuildPhase.Preparation, false);
            defense.SetEnemyProtection(enemies.IsCellProtected);
            defense.SetAttackTarget(enemies.TryHitNearest);
        }

        [TearDown]
        public void TearDown()
        {
            enemies.ResetRun();
            defense.ResetRun();
        }

        [Test]
        public void BaselineTower_HitsNearestLivingEnemyAtInterval_AndPauseFreezesCooldown()
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                inventory.TryAdd(kind, 1);
            defense.BeginPlacement();
            defense.SetPreviewCell(Vector2Int.zero);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            var near = enemies.SpawnAt(new Vector2Int(1, 0));
            var far = enemies.SpawnAt(new Vector2Int(2, 0));
            var nearHealth = near.Health;
            var farHealth = far.Health;
            tower.Tick(0.1f);
            Assert.That(near.Health, Is.EqualTo(nearHealth - catalog.AttackDamage));
            Assert.That(far.Health, Is.EqualTo(farHealth));
            defense.SetAttackActive(false);
            enemies.SetPaused(true);
            tower.Tick(10f);
            Assert.That(near.Health, Is.EqualTo(nearHealth - catalog.AttackDamage));
            defense.SetAttackActive(true);
            enemies.SetPaused(false);
            tower.Tick(catalog.FireInterval / 2f);
            Assert.That(near.Health, Is.EqualTo(nearHealth - catalog.AttackDamage));
            tower.Tick(catalog.FireInterval / 2f);
            Assert.That(near.Health, Is.EqualTo(nearHealth - 2 * catalog.AttackDamage));
            Assert.That(navigation.OccupiedCount, Is.EqualTo(1), "Attacking cannot change tower occupancy.");
        }

        [Test]
        public void Longshot_ReachesBeyondRapidAndDealsAHeavierHit()
        {
            var longshot = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(
                "Assets/Stage2/Defense/Data/Stage2DefenseLongshot.asset");
            Assert.That(longshot.TryValidate(out var reason), Is.True, reason);
            var towerCell = Vector2Int.zero;
            var towerWorld = map.CellToWorld(towerCell);
            Vector2Int? targetCell = null;
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y && !targetCell.HasValue; y++)
                for (var x = map.MinimumCell.x; x <= map.MaximumCell.x; x++)
                {
                    var candidate = new Vector2Int(x, y);
                    var distance = Vector2.Distance(towerWorld, map.CellToWorld(candidate));
                    if (map.IsWalkable(candidate) && distance > catalog.AttackRange + 0.2f &&
                        distance < longshot.AttackRange - 0.2f)
                    {
                        targetCell = candidate;
                        break;
                    }
                }
            Assert.That(targetCell.HasValue, Is.True, "The authored map must have a comparison cell.");
            var enemy = enemies.SpawnAt(targetCell.Value);
            var initialHealth = enemy.Health;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                inventory.TryAdd(kind, 3);
            defense.BeginPlacement();
            defense.SetPreviewCell(towerCell);
            Assert.That(defense.TryConfirm(out var rapid), Is.EqualTo(PlacementResult.Ready));
            rapid.Tick(0.1f);
            Assert.That(enemy.Health, Is.EqualTo(initialHealth));
            defense.ResetRun();
            defense = new Stage2Defense(map, navigation, inventory, longshot,
                () => map.CellToWorld(map.Throne), () => Object.Instantiate(longshot.TowerPrefab));
            defense.SetAttackTarget(enemies.TryHitNearest);
            defense.SetRunState(Stage2BuildPhase.Preparation, false);
            defense.BeginPlacement();
            defense.SetPreviewCell(towerCell);
            Assert.That(defense.TryConfirm(out var distant), Is.EqualTo(PlacementResult.Ready));
            distant.Tick(0.1f);
            Assert.That(enemy.Health, Is.EqualTo(initialHealth - longshot.AttackDamage));
            distant.Tick(longshot.FireInterval / 2f);
            Assert.That(enemy.IsAlive, Is.True, "The slower attack must keep its cooldown.");
            distant.Tick(longshot.FireInterval / 2f);
            Assert.That(enemy.IsAlive, Is.False);
            Assert.That(enemies.ActiveCount, Is.Zero);
        }
    }
}
