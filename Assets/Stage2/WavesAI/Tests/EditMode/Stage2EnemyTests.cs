using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.WavesAI.Tests
{
    public sealed class Stage2EnemyTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2EnemyCatalog enemyCatalog;
        private Stage2DefenseCatalog defenseCatalog;
        private Stage2EnemyGroup group;
        private Stage2Defense defense;
        private Stage2Inventory inventory;

        [SetUp]
        public void SetUp()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            enemyCatalog = AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
            defenseCatalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            Assert.That(map, Is.Not.Null);
            Assert.That(enemyCatalog.TryValidate(out var reason), Is.True, reason);
            navigation = new Stage2Navigation(map);
            inventory = new Stage2Inventory();
            defense = new Stage2Defense(map, navigation, inventory, defenseCatalog,
                () => map.CellToWorld(map.Throne), () => Object.Instantiate(defenseCatalog.TowerPrefab));
            defense.SetRunState(Stage2BuildPhase.Preparation, false);
            group = new Stage2EnemyGroup(map, navigation, enemyCatalog);
            defense.SetEnemyProtection(group.IsCellProtected);
        }

        [TearDown]
        public void TearDown()
        {
            group?.ResetRun();
            defense?.ResetRun();
        }

        [Test]
        public void CommittedStep_ProtectsBothEndpoints_ThenReplansFromArrival()
        {
            var enemy = group.SpawnAt(new Vector2Int(map.EnemyEntrance.x - 1, map.EnemyEntrance.y));
            enemy.SetAutomaticTick(false);
            var source = enemy.CurrentCell;
            var revision = navigation.Revision;
            enemy.Tick(0.01f);
            var destination = enemy.StepDestination;
            Assert.That(destination.HasValue, Is.True);
            Assert.That(enemy.transform.position, Is.Not.EqualTo((Vector3)map.CellToWorld(source)));
            Assert.That(group.IsCellProtected(source), Is.True);
            Assert.That(group.IsCellProtected(destination.Value), Is.True);
            Assert.That(navigation.Revision, Is.EqualTo(revision));

            Fund(1);
            defense.SetRunState(Stage2BuildPhase.Wave, false);
            defense.BeginPlacement();
            defense.SetPreviewCell(destination.Value);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.EnemyProtected));
            defense.SetPreviewCell(source);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.EnemyProtected));
            Assert.That(navigation.Revision, Is.EqualTo(revision));
            var future = enemy.Plan.Path[3];
            defense.SetPreviewCell(future);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.Ready),
                "An uncommitted future route remains buildable.");
            var oldPlan = enemy.Plan;
            Assert.That(navigation.IsCurrent(oldPlan), Is.False);
            enemy.Tick(1f);
            Assert.That(enemy.CurrentCell, Is.EqualTo(destination.Value));
            Assert.That(enemy.IsAtCellCenter, Is.True);
            Assert.That(group.IsCellProtected(source), Is.False);
            Assert.That(group.IsCellProtected(destination.Value), Is.True);
            enemy.Tick(0.01f);
            Assert.That(enemy.Plan, Is.Not.SameAs(oldPlan));
            Assert.That(enemy.CurrentCell, Is.EqualTo(destination.Value));
            Assert.That(enemy.StepDestination, Is.Not.EqualTo(future));
        }

        [Test]
        public void SharedProtection_DeathAndResetReleaseOnlyTheirOwnCells()
        {
            var first = group.Spawn();
            var second = group.Spawn();
            first.SetAutomaticTick(false);
            second.SetAutomaticTick(false);
            first.Tick(0.01f);
            second.Tick(0.01f);
            var destination = first.StepDestination.Value;
            var revision = navigation.Revision;
            Assert.That(group.IsCellProtected(map.EnemyEntrance), Is.True);
            Assert.That(group.IsCellProtected(destination), Is.True);
            first.TakeDamage(enemyCatalog.Health);
            Assert.That(group.ActiveCount, Is.EqualTo(1));
            Assert.That(group.IsCellProtected(map.EnemyEntrance), Is.True);
            Assert.That(group.IsCellProtected(destination), Is.True);
            group.ResetRun();
            Assert.That(group.ActiveCount, Is.Zero);
            Assert.That(group.IsCellProtected(map.EnemyEntrance), Is.False);
            Assert.That(group.IsCellProtected(destination), Is.False);
            Assert.That(navigation.Revision, Is.EqualTo(revision));
        }

        [Test]
        public void PauseDuringStep_FreezesMotionAndRetainsProtectionUntilReset()
        {
            var enemy = group.Spawn();
            enemy.SetAutomaticTick(false);
            enemy.Tick(0.01f);
            var position = enemy.transform.position;
            var destination = enemy.StepDestination.Value;
            group.SetPaused(true);
            enemy.Tick(5f);
            Assert.That(enemy.transform.position, Is.EqualTo(position));
            Assert.That(group.IsCellProtected(destination), Is.True);
            group.ResetRun();
            Assert.That(group.IsCellProtected(destination), Is.False);
        }

        [Test]
        public void FullWall_SiegesOwnedTowerAtStableInterval_ThenResumesThroughGap()
        {
            var towerCell = new Vector2Int(0, 0);
            var tower = BuildTower(towerCell);
            WallExcept(towerCell);
            var enemy = group.SpawnAt(new Vector2Int(1, 0));
            enemy.SetAutomaticTick(false);
            enemy.Tick(0.2f);
            Assert.That(enemy.Plan.Status, Is.EqualTo(NavigationStatus.ReachableBlocker));
            Assert.That(enemy.Plan.BlockerOwner, Is.SameAs(tower));
            Assert.That(tower.Health, Is.EqualTo(defenseCatalog.TowerHealth));
            for (var index = 0; index < defenseCatalog.TowerHealth; index++)
            {
                // Revisions from remote edits must neither grant an early hit nor
                // restart the elapsed attack interval indefinitely.
                var remote = new Vector2Int(-49, 49);
                if (navigation.TryGetOccupant(remote, out var token)) navigation.TryRelease(remote, token);
                else navigation.TryOccupy(remote, new object());
                enemy.Tick(enemyCatalog.SiegeInterval);
            }
            Assert.That(tower.IsAlive, Is.False);
            Assert.That(navigation.TryGetOccupant(towerCell, out _), Is.False);
            enemy.Tick(0.01f);
            Assert.That(enemy.Plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(enemy.StepDestination, Is.EqualTo(towerCell));
        }

        [Test]
        public void RevisionChurn_DoesNotGrantEarlyHitsOrStarveSiegeCooldown()
        {
            var towerCell = new Vector2Int(0, 0);
            var tower = BuildTower(towerCell);
            WallExcept(towerCell);
            var enemy = group.SpawnAt(new Vector2Int(1, 0));
            enemy.SetAutomaticTick(false);
            var remote = new Vector2Int(-49, 49);
            for (var tick = 1; tick <= 8; tick++)
            {
                if (navigation.TryGetOccupant(remote, out var token)) navigation.TryRelease(remote, token);
                else navigation.TryOccupy(remote, new object());
                enemy.Tick(0.125f);
                var expectedHits = tick / 4;
                Assert.That(tower.Health, Is.EqualTo(defenseCatalog.TowerHealth - expectedHits),
                    $"Revision change on tick {tick} must not change the attack clock.");
            }
        }

        [Test]
        public void ReplacedBlocker_RequiresFreshPlanAndDoesNotGetFreeHit()
        {
            var towerCell = new Vector2Int(0, 0);
            var first = BuildTower(towerCell);
            WallExcept(towerCell);
            var enemy = group.SpawnAt(new Vector2Int(1, 0));
            enemy.SetAutomaticTick(false);
            enemy.Tick(0.125f);
            var stale = enemy.Plan;
            Assert.That(stale.BlockerOwner, Is.SameAs(first));
            first.TakeDamage(defenseCatalog.TowerHealth);
            var replacement = BuildTower(towerCell);
            Assert.That(navigation.IsCurrent(stale), Is.False);
            enemy.Tick(0.125f);
            Assert.That(enemy.Plan.BlockerOwner, Is.SameAs(replacement));
            Assert.That(replacement.Health, Is.EqualTo(defenseCatalog.TowerHealth));
            enemy.Tick(0.25f);
            Assert.That(replacement.Health, Is.EqualTo(defenseCatalog.TowerHealth - 1));
        }

        [Test]
        public void ThroneContactEmitsOneDamageFactAndRemovesProtection()
        {
            var enemy = group.SpawnAt(new Vector2Int(map.Throne.x + 1, map.Throne.y));
            enemy.SetAutomaticTick(false);
            var facts = 0;
            group.ReachedThrone += damage => facts += damage;
            enemy.Tick(1f);
            Assert.That(enemy.CurrentCell, Is.EqualTo(map.Throne));
            enemy.Tick(0.01f);
            enemy.Tick(0.01f);
            Assert.That(facts, Is.EqualTo(enemyCatalog.ThroneDamage));
            Assert.That(group.ActiveCount, Is.Zero);
            Assert.That(group.IsCellProtected(map.Throne), Is.False);
        }

        [TestCase(.004166667f)]
        [TestCase(.016666667f)]
        [TestCase(.033333333f)]
        public void ActualWindingRoadNeverStopsBeforeTheThrone(float delta)
        {
            var road = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset");
            var routes = new Stage2Navigation(road);
            var actors = new Stage2EnemyGroup(road, routes, enemyCatalog);
            try
            {
                var enemy = actors.Spawn(); enemy.SetAutomaticTick(false);
                var plan = routes.PlanFrom(road.EnemyEntrance);
                Assert.That(plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
                var contact = 0; actors.ReachedThrone += damage => contact += damage;
                var stillSeconds = 0f;
                var cellLength = Vector2.Distance(road.CellToWorld(plan.Path[0]), road.CellToWorld(plan.Path[1]));
                var limit = Mathf.CeilToInt(plan.Path.Count * cellLength / enemyCatalog.MoveSpeed / delta) * 2;
                for (var tick = 0; tick < limit && actors.ActiveCount > 0; tick++)
                {
                    var before = enemy.transform.position;
                    enemy.Tick(delta);
                    if (actors.ActiveCount == 0) break;
                    stillSeconds = (enemy.transform.position - before).sqrMagnitude < 1e-12f ? stillSeconds + delta : 0f;
                    Assert.That(stillSeconds, Is.LessThan(.2f),
                        $"Stalled at {enemy.CurrentCell}, next={enemy.StepDestination}, path={enemy.Plan?.Status}/{enemy.Plan?.Path.Count}, delta={delta}, tick={tick}.");
                }
                Assert.That(contact, Is.EqualTo(enemyCatalog.ThroneDamage), "The whole actual road must finish without forced kills/teleports.");
            }
            finally { actors.ResetRun(); }
        }

        [Test]
        public void ValidatedGroupStillRejectsInvalidAndOccupiedSpawnPoints()
        {
            var revision = navigation.Revision;
            Assert.Throws<System.ArgumentException>(() => group.SpawnAt(map.MinimumCell + Vector2Int.left));
            Assert.That(group.ActiveCount, Is.Zero);
            Assert.That(navigation.Revision, Is.EqualTo(revision));
            var tower = BuildTower(Vector2Int.zero);
            Assert.Throws<System.ArgumentException>(() => group.SpawnAt(tower.Cell));
            Assert.That(group.ActiveCount, Is.Zero);
            Assert.That(navigation.TryGetOccupant(tower.Cell, out var owner), Is.True);
            Assert.That(owner, Is.SameAs(tower));
        }

        [Test]
        public void DenseGroupSharesTheRoadSearchAndEveryActorAdvances()
        {
            var road = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset");
            var routes = new Stage2Navigation(road);
            var actors = new Stage2EnemyGroup(road, routes, enemyCatalog);
            try
            {
                for (var i = 0; i < 120; i++) actors.Spawn().SetAutomaticTick(false);
                for (var i = 0; i < actors.ActiveCount; i++) actors.GetEnemy(i).Tick(.1f);
                Assert.That(routes.Diagnostics.FullSearchCount, Is.EqualTo(1));
                for (var i = 0; i < actors.ActiveCount; i++)
                {
                    var enemy = actors.GetEnemy(i);
                    Assert.That(Vector2.Distance(enemy.transform.position, road.CellToWorld(road.EnemyEntrance)), Is.GreaterThan(.1f));
                    Assert.That(enemy.Plan.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
                }
            }
            finally { actors.ResetRun(); }
        }

        private Stage2Tower BuildTower(Vector2Int target)
        {
            Fund(1);
            defense.BeginPlacement();
            defense.SetPreviewCell(target);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            return tower;
        }

        private void WallExcept(Vector2Int gapTower)
        {
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
            {
                var target = new Vector2Int(gapTower.x, y);
                if (target == gapTower) continue;
                Assert.That(navigation.TryOccupy(target, new object()), Is.EqualTo(OccupancyResult.Changed));
            }
        }

        private void Fund(int amount)
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                inventory.TryAdd(kind, kind == MapResourceKind.Wood ? amount * 2 : amount);
            inventory.TryAddGold(amount * 5);
        }
    }
}
