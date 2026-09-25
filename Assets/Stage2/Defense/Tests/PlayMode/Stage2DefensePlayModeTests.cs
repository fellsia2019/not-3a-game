#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Defense.Tests
{
    public sealed class Stage2DefensePlayModeTests
    {
        private InputTestFixture input;
        private Keyboard keyboard;
        private Mouse mouse;
        private Stage2MapDefinition map;
        private Stage2DefenseCatalog catalog;
        private Stage2Navigation navigation;
        private GameObject hero;
        private GameObject owner;
        private GameObject cameraObject;
        private Stage2GatheringDriver gathering;
        private Stage2BuildDriver building;
        private Vector2Int cell;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            input = new InputTestFixture();
            input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            var economyCatalog = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset");
            Assert.That(catalog.TryValidate(out var reason), Is.True, reason);
            navigation = new Stage2Navigation(map);
            cell = FindBuildableCell();
            hero = new GameObject("Hero");
            hero.transform.position = map.CellToWorld(map.Throne);
            cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            var center = map.CellToWorld(cell);
            cameraObject.transform.position = new Vector3(center.x, center.y, -10f);
            owner = new GameObject("Defense Owner");
            gathering = owner.AddComponent<Stage2GatheringDriver>();
            gathering.Configure(map, economyCatalog, hero.transform);
            building = owner.AddComponent<Stage2BuildDriver>();
            Assert.That(building.Configure(map, catalog, gathering, hero.transform, camera, navigation), Is.True);
            building.SetRunState(Stage2BuildPhase.Preparation, false);
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                gathering.Economy.Inventory.TryAdd(kind, 1);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(owner);
            Object.Destroy(hero);
            Object.Destroy(cameraObject);
            yield return null;
            input?.TearDown();
        }

        [UnityTest]
        public IEnumerator BuildInput_ClickCommitsOnceThenDeathReleasesForReplacement()
        {
            var camera = cameraObject.GetComponent<Camera>();
            var screen = camera.WorldToScreenPoint(map.CellToWorld(cell));
            input.Set(mouse.position, new Vector2(screen.x, screen.y));
            input.Press(keyboard.bKey);
            yield return null;
            input.Release(keyboard.bKey);
            yield return null;
            Assert.That(building.Defense.IsPlacing, Is.True);
            Assert.That(building.PreviewResult, Is.EqualTo(PlacementResult.Ready));

            input.Press(mouse.leftButton);
            yield return null;
            Assert.That(building.Defense.TowerCount, Is.EqualTo(1));
            Assert.That(navigation.TryGetOccupant(cell, out var ownerToken), Is.True);
            var tower = ownerToken as Stage2Tower;
            Assert.That(tower, Is.Not.Null);
            Assert.That(tower.gameObject.activeInHierarchy, Is.True);
            Assert.That(navigation.Revision, Is.EqualTo(1));
            Assert.That(gathering.Economy.Inventory.Get(MapResourceKind.Metal), Is.Zero);
            yield return null;
            Assert.That(building.Defense.TowerCount, Is.EqualTo(1), "Held click must not repeat purchase.");
            input.Release(mouse.leftButton);
            tower.TakeDamage(catalog.TowerHealth);
            yield return null;
            Assert.That(navigation.OccupiedCount, Is.Zero);
            Assert.That(building.Defense.TowerCount, Is.Zero);
            Assert.That(navigation.Revision, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CancelWinsOverClick_AndWaveRequiresLiveEnemyProtection()
        {
            var camera = cameraObject.GetComponent<Camera>();
            var screen = camera.WorldToScreenPoint(map.CellToWorld(cell));
            input.Set(mouse.position, new Vector2(screen.x, screen.y));
            input.Press(keyboard.bKey);
            yield return null;
            input.Release(keyboard.bKey);
            yield return null;
            input.Press(keyboard.escapeKey);
            input.Press(mouse.leftButton);
            yield return null;
            Assert.That(building.Defense.TowerCount, Is.Zero);
            Assert.That(navigation.Revision, Is.Zero);
            Assert.That(gathering.Economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(1));
            input.Release(keyboard.escapeKey);
            input.Release(mouse.leftButton);
            building.SetRunState(Stage2BuildPhase.Wave, false);
            input.Press(keyboard.bKey);
            yield return null;
            input.Release(keyboard.bKey);
            yield return null;
            Assert.That(building.PreviewResult, Is.EqualTo(PlacementResult.EnemyStateUnavailable));
            building.SetEnemyProtection(_ => false);
            yield return null;
            Assert.That(building.PreviewResult, Is.EqualTo(PlacementResult.Ready));
            building.SetRunState(Stage2BuildPhase.Wave, true);
            Assert.That(building.Defense.IsPlacing, Is.False);
            Assert.That(navigation.Revision, Is.Zero);
        }

        private Vector2Int FindBuildableCell()
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
    }
}
#endif
