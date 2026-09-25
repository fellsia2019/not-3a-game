#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.RunState.Tests
{
    public sealed class Stage2RunPlayModeTests
    {
        private InputTestFixture input;
        private Keyboard keyboard;
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2EnemyGroup enemies;
        private Stage2RunCatalog data;
        private GameObject hero;
        private GameObject cameraObject;
        private GameObject owner;
        private Stage2GatheringDriver gathering;
        private Stage2BuildDriver building;
        private Stage2RunDriver driver;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            input = new InputTestFixture();
            input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            var economyData = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset");
            var defenseData = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            var enemyData = AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
            data = ScriptableObject.CreateInstance<Stage2RunCatalog>();
            data.Configure(10f, 1, 0f, 1);
            navigation = new Stage2Navigation(map);
            enemies = new Stage2EnemyGroup(map, navigation, enemyData);
            hero = new GameObject("Hero");
            hero.transform.position = map.CellToWorld(map.Throne);
            cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            owner = new GameObject("Run Owner");
            gathering = owner.AddComponent<Stage2GatheringDriver>();
            gathering.Configure(map, economyData, hero.transform);
            building = owner.AddComponent<Stage2BuildDriver>();
            Assert.That(building.Configure(map, defenseData, gathering, hero.transform, camera, navigation), Is.True);
            building.SetEnemyProtection(enemies.IsCellProtected);
            driver = owner.AddComponent<Stage2RunDriver>();
            Assert.That(driver.Configure(data, gathering, building, hero.transform, navigation, enemies), Is.True);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(owner);
            enemies.ResetRun();
            Object.Destroy(hero);
            Object.Destroy(cameraObject);
            Object.Destroy(data);
            yield return null;
            input?.TearDown();
        }

        [UnityTest]
        public IEnumerator SpaceStartsWave_LastEnemyWins_RRestartsWithoutOldOccupancy()
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                gathering.Economy.Inventory.TryAdd(kind, 1);
            building.Defense.BeginPlacement();
            building.Defense.SetPreviewCell(new Vector2Int(0, 0));
            Assert.That(building.Defense.TryConfirm(out _), Is.EqualTo(PlacementResult.Ready));
            var oldPlan = navigation.PlanFrom(map.EnemyEntrance);
            input.Press(keyboard.spaceKey);
            yield return null;
            input.Release(keyboard.spaceKey);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(enemies.ActiveCount, Is.EqualTo(1));
            var enemy = enemies.GetEnemy(0);
            enemy.TakeDamage(enemy.Health);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Won));
            Assert.That(building.Defense.BeginPlacement(), Is.False);
            input.Press(keyboard.rKey);
            yield return null;
            input.Release(keyboard.rKey);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(driver.Run.ThroneHealth, Is.EqualTo(1));
            Assert.That(navigation.OccupiedCount, Is.Zero);
            Assert.That(navigation.IsCurrent(oldPlan), Is.False);
            Assert.That(gathering.Economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(enemies.ActiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator PauseFreezesPreparationAndContactLossClearsWave()
        {
            building.Defense.BeginPlacement();
            building.Defense.SetPreviewCell(new Vector2Int(0, 0));
            var remaining = driver.Run.PreparationRemaining;
            driver.SetPaused(true);
            Assert.That(building.Defense.IsPlacing, Is.False);
            yield return new WaitForSeconds(0.1f);
            Assert.That(driver.Run.PreparationRemaining, Is.EqualTo(remaining));
            driver.SetPaused(false);
            driver.Run.StartWave();
            var contact = enemies.SpawnAt(new Vector2Int(map.Throne.x + 1, map.Throne.y));
            contact.SetAutomaticTick(false);
            contact.Tick(1f);
            contact.Tick(0.01f);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(driver.Run.ThroneHealth, Is.Zero);
            Assert.That(enemies.ActiveCount, Is.Zero);
            Assert.That(building.Defense.BeginPlacement(), Is.False);
            yield return null;
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
        }

        [UnityTest]
        public IEnumerator BaselineTowerKillsEnemyAndNaturallyWinsOneWave()
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                gathering.Economy.Inventory.TryAdd(kind, 1);
            building.Defense.BeginPlacement();
            building.Defense.SetPreviewCell(new Vector2Int(map.EnemyEntrance.x - 1, map.EnemyEntrance.y));
            Assert.That(building.Defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(driver.Run.StartWave(), Is.True);
            for (var sample = 0; sample < 20 && !driver.Run.IsTerminal; sample++)
                yield return new WaitForSeconds(0.25f);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Won),
                "The authored prototype tower must make the real moving wave winnable.");
            Assert.That(driver.Run.ThroneHealth, Is.EqualTo(driver.Run.ThroneMaximum));
            Assert.That(enemies.ActiveCount, Is.Zero);
            Assert.That(tower.Health, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator RecoveryKeepsNodeTimer_PauseFreezesIt_SpaceAndRWorkAcrossWaves()
        {
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(10f, 1, 0f),
                new Stage2WaveDefinition(10f, 1, 0f) }, 2);
            Assert.That(driver.Initialize(), Is.True);
            var economy = gathering.Economy;
            var node = economy.GetNode(MapResourceKind.Wood);
            for (var i = 0; i < node.Capacity; i++)
                economy.AdvanceGather(MapResourceKind.Wood, economy.WorldPosition(MapResourceKind.Wood),
                    true, node.GatherSeconds, true);
            var stock = economy.Inventory.Get(MapResourceKind.Wood);
            driver.Run.StartWave();
            enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            Assert.That(driver.Run.WaveNumber, Is.EqualTo(2));
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(node.IsDepleted, Is.True);
            var timer = node.RecoveryRemaining;
            yield return new WaitForSeconds(0.1f);
            Assert.That(node.RecoveryRemaining, Is.LessThan(timer));
            driver.SetPaused(true);
            timer = node.RecoveryRemaining;
            input.Press(keyboard.spaceKey);
            yield return null;
            input.Release(keyboard.spaceKey);
            yield return new WaitForSeconds(0.1f);
            Assert.That(node.RecoveryRemaining, Is.EqualTo(timer));
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            driver.SetPaused(false);
            input.Press(keyboard.spaceKey);
            yield return null;
            input.Release(keyboard.spaceKey);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(stock));
            enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            Assert.That(driver.Run.Phase, Is.EqualTo(Stage2RunPhase.Won));
            timer = node.RecoveryRemaining;
            yield return new WaitForSeconds(0.1f);
            Assert.That(node.RecoveryRemaining, Is.EqualTo(timer));
            input.Press(keyboard.rKey);
            yield return null;
            input.Release(keyboard.rKey);
            Assert.That(driver.Run.WaveNumber, Is.EqualTo(1));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(node.Remaining, Is.EqualTo(node.Capacity));
            Assert.That(driver.Run.ThroneHealth, Is.EqualTo(2));
        }
    }
}
#endif
