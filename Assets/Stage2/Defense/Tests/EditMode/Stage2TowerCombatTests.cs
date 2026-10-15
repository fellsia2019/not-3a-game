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
            inventory.TryAddGold(5);
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
                inventory.TryAdd(kind, kind == MapResourceKind.Wood ? 2 : 1);
            defense.BeginPlacement();
            defense.SetPreviewCell(Vector2Int.zero);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            var near = enemies.SpawnAt(new Vector2Int(1, 0));
            var far = enemies.SpawnAt(new Vector2Int(2, 0));
            var nearHealth = near.Health;
            var farHealth = far.Health;
            tower.Tick(0.1f);
            Assert.That(near.Health, Is.EqualTo(nearHealth), "Flight starts before damage.");
            enemies.TickShots(catalog.ProjectileFlightSeconds);
            Assert.That(near.Health, Is.EqualTo(nearHealth - catalog.AttackDamage));
            Assert.That(tower.GetComponent<SpriteRenderer>().color, Is.EqualTo(catalog.TowerPrefab.GetComponent<SpriteRenderer>().color));
            Assert.That(near.GetComponent<SpriteRenderer>().color, Is.EqualTo(Color.white));
            Assert.That(far.Health, Is.EqualTo(farHealth));
            defense.SetAttackActive(false);
            enemies.SetPaused(true);
            tower.Tick(10f);
            Assert.That(near.Health, Is.EqualTo(nearHealth - catalog.AttackDamage));
            defense.SetAttackActive(true);
            enemies.SetPaused(false);
            tower.Tick(catalog.FireInterval / 2f);
            Assert.That(near.Health, Is.EqualTo(nearHealth - catalog.AttackDamage));
            Assert.That(tower.GetComponent<SpriteRenderer>().color, Is.EqualTo(catalog.TowerPrefab.GetComponent<SpriteRenderer>().color));
            tower.Tick(catalog.FireInterval / 2f);
            enemies.TickShots(catalog.ProjectileFlightSeconds);
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
            inventory.TryAdd(MapResourceKind.Wood, 4);
            inventory.TryAdd(MapResourceKind.Stone, 6);
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
            enemies.TickShots(longshot.ProjectileFlightSeconds);
            Assert.That(enemy.Health, Is.EqualTo(initialHealth - longshot.AttackDamage));
            distant.Tick(longshot.FireInterval / 2f);
            Assert.That(enemy.IsAlive, Is.True, "The slower attack must keep its cooldown.");
            distant.Tick(longshot.FireInterval / 2f);
            enemies.TickShots(longshot.ProjectileFlightSeconds);
            Assert.That(enemy.IsAlive, Is.False);
            Assert.That(enemies.ActiveCount, Is.Zero);
        }

        [Test]
        public void StoneBlast_HitsEnemiesWithinTwoCellsButNotBeyond()
        {
            var center = enemies.SpawnAt(new Vector2Int(1, 0));
            var neighbor = enemies.SpawnAt(new Vector2Int(2, 0));
            var boundary = enemies.SpawnAt(new Vector2Int(3, 0));
            var outside = enemies.SpawnAt(new Vector2Int(4, 0));
            var health = center.Health;
            Assert.That(enemies.TryHitNearest(map.CellToWorld(Vector2Int.zero), 10f, 2, 2f), Is.True);
            Assert.That(center.Health, Is.EqualTo(health - 2));
            Assert.That(neighbor.Health, Is.EqualTo(health - 2));
            Assert.That(boundary.Health, Is.EqualTo(health - 2));
            Assert.That(outside.Health, Is.EqualTo(health));
        }

        [Test]
        public void InFlightShots_PauseFollowTargetAndApplyAreaDamageOnlyOnArrival()
        {
            var target = enemies.SpawnAt(new Vector2Int(1,0));
            var neighbor = enemies.SpawnAt(new Vector2Int(2,0));
            var outside = enemies.SpawnAt(new Vector2Int(5,0));
            int hp = target.Health, impacts = 0;
            Stage2Shot shot = null;
            enemies.ShotLaunched += value => shot = value;
            enemies.ShotImpacted += _ => impacts++;
            Assert.That(enemies.TryHitNearest(map.CellToWorld(Vector2Int.zero),10f,2,2f,.5f), Is.True);
            Assert.That(target.Health, Is.EqualTo(hp));
            enemies.SetPaused(true); enemies.TickShots(10f);
            Assert.That(shot.Progress, Is.Zero);
            Assert.That(impacts, Is.Zero);
            enemies.SetPaused(false);
            target.transform.position = map.CellToWorld(new Vector2Int(2,0));
            enemies.TickShots(.5f);
            Assert.That(shot.Position, Is.EqualTo((Vector2)target.transform.position));
            Assert.That(target.Health, Is.EqualTo(hp-2));
            Assert.That(neighbor.Health, Is.EqualTo(hp-2));
            Assert.That(outside.Health, Is.EqualTo(hp));
            Assert.That(impacts, Is.EqualTo(1));
            Assert.That(enemies.PendingShotCount, Is.Zero);
        }

        [Test]
        public void InFlightShots_DeadTargetDoesNotRetargetOrDoubleReward_AndResetCancels()
        {
            var target = enemies.SpawnAt(new Vector2Int(1,0));
            var other = enemies.SpawnAt(new Vector2Int(2,0));
            int hp=other.Health, killed=0, impacts=0;
            enemies.EnemyRemoved += enemy => { if (enemy.WasKilled) killed++; };
            enemies.ShotImpacted += _ => impacts++;
            enemies.TryHitNearest(map.CellToWorld(Vector2Int.zero),10f,99,0f,.2f);
            enemies.TryHitNearest(map.CellToWorld(Vector2Int.zero),10f,99,0f,.3f);
            enemies.TickShots(.3f);
            Assert.That(killed, Is.EqualTo(1));
            Assert.That(other.Health, Is.EqualTo(hp));
            Assert.That(impacts, Is.EqualTo(2));
            enemies.TryHitNearest(map.CellToWorld(Vector2Int.zero),10f,99,2f,.5f);
            enemies.ResetRun();
            var fresh = enemies.SpawnAt(new Vector2Int(2,0));
            enemies.TickShots(10f);
            Assert.That(fresh.Health, Is.EqualTo(hp));
            Assert.That(enemies.PendingShotCount, Is.Zero);
            Assert.That(impacts, Is.EqualTo(2));
        }
    }
}
