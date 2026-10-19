#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3BuildMenuTests
    {
        private InputTestFixture input;
        private Keyboard keyboard;
        private Mouse mouse;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud hud;

        [UnitySetUp]
        public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            scene = game.GetComponent<Stage2SceneBootstrap>(); hud = game.GetComponent<Stage2GameHud>();
            Assert.That(game.Production, Is.Not.Null);
            Assert.That(game.Production.Rules.Points.Count, Is.EqualTo(6));
            game.RunDriver.ResetRun();
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown();
        }

        [UnityTest]
        public IEnumerator CategoriesSelectThreeGeneratorsAndExistingTowersWithExclusiveInput()
        {
            hud.AddAllResources(100); hud.AddGold(100);
            Assert.That(hud.BuildCategory, Is.EqualTo(Stage2BuildCategory.Towers));
            Assert.That(game.Building.Defense.RoleCount, Is.EqualTo(3));
            hud.SelectBuildCategory(Stage2BuildCategory.Generators);
            Assert.That(game.Production.Catalog.Entries.Count, Is.EqualTo(3));
            foreach (var entry in game.Production.Catalog.Entries)
            {
                Assert.That(entry.Sprite, Is.Not.Null);
                hud.SelectGenerator(entry.Building);
                Assert.That(hud.BuildCategory, Is.EqualTo(Stage2BuildCategory.Generators));
                Assert.That(game.Production.IsPlacing, Is.True);
                Assert.That(game.Production.SelectedBuilding, Is.EqualTo(entry.Building));
                Assert.That(game.Building.Defense.IsPlacing, Is.False);
            }
            hud.SelectTower(1);
            Assert.That(game.Building.Defense.SelectedRoleIndex, Is.EqualTo(1));
            Assert.That(game.Building.Defense.IsPlacing, Is.True);
            Assert.That(game.Production.IsPlacing, Is.False);
            hud.SelectGenerator(ResourceProductionBuilding.Sawmill);
            input.Press(keyboard.digit2Key); yield return null; input.Release(keyboard.digit2Key);
            Assert.That(game.Production.IsPlacing, Is.False, "Existing tower hotkeys also exit generator placement.");
            Assert.That(hud.BuildCategory, Is.EqualTo(Stage2BuildCategory.Towers));
            hud.SelectGenerator(ResourceProductionBuilding.Quarry);
            input.Press(keyboard.escapeKey); yield return null; input.Release(keyboard.escapeKey);
            Assert.That(game.Production.IsPlacing, Is.False);
            Assert.That(game.Production.Rules.Buildings.Count, Is.Zero);
            Assert.That(game.Gathering.Economy.Inventory.Gold, Is.EqualTo(game.Gathering.Catalog.StartingGold + 100));
            for (var i = 0; i < 3; i++) Assert.That(game.Gathering.Economy.Inventory.Get((MapResourceKind)i), Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator UnaffordableCardsAndPublicSelectionStayDisabledUntilInventoryCanPay()
        {
            var production = game.Production;
            for (var role = 0; role < game.Building.Defense.RoleCount; role++)
            {
                Assert.That(hud.CanSelectTower(role), Is.False);
                hud.SelectTower(role);
                Assert.That(game.Building.Defense.IsPlacing, Is.False);
            }
            foreach (var entry in production.Catalog.Entries)
            {
                Assert.That(hud.CanSelectGenerator(entry.Building), Is.False);
                hud.SelectGenerator(entry.Building);
                Assert.That(production.IsPlacing, Is.False);
            }
            input.Press(keyboard.bKey); yield return null; input.Release(keyboard.bKey); yield return null;
            Assert.That(game.Building.Defense.IsPlacing, Is.False, "B cannot bypass a disabled purchase.");
            Assert.That(production.Rules.Buildings.Count, Is.Zero);
            hud.AddAllResources(100); hud.AddGold(100);
            for (var role = 0; role < game.Building.Defense.RoleCount; role++) Assert.That(hud.CanSelectTower(role), Is.True);
            foreach (var entry in production.Catalog.Entries) Assert.That(hud.CanSelectGenerator(entry.Building), Is.True);
            hud.SelectGenerator(ResourceProductionBuilding.Sawmill);
            Assert.That(production.IsPlacing, Is.True);
            production.CancelPlacement();
            hud.SelectTower(0);
            Assert.That(game.Building.Defense.IsPlacing, Is.True);
            game.Building.CancelPlacement();
            var inventory = game.Gathering.Economy.Inventory;
            Assert.That(inventory.TrySpend(inventory.Get(MapResourceKind.Wood), inventory.Get(MapResourceKind.Stone),
                inventory.Get(MapResourceKind.Metal), inventory.Gold), Is.True);
            Assert.That(hud.CanSelectTower(0), Is.False);
            Assert.That(hud.CanSelectGenerator(ResourceProductionBuilding.Sawmill), Is.False);
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Assert.That(production.Rules.Buildings.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator BottomPanelBlocksWorldBuildAndZoomAndKeepsMinimapAccessible()
        {
            hud.AddAllResources(100);
            hud.SelectGenerator(ResourceProductionBuilding.Sawmill);
            var bar = hud.BuildRect;
            var playerUi = game.GetComponent("Stage3FantasyHud"); Assert.That(playerUi, Is.Not.Null);
            var dock = (Rect)playerUi.GetType().GetProperty("BottomRect").GetValue(playerUi);
            var map = (Rect)playerUi.GetType().GetProperty("MinimapRect").GetValue(playerUi);
            Assert.That(map.xMin, Is.GreaterThanOrEqualTo(dock.xMin));
            Assert.That(map.xMax, Is.LessThanOrEqualTo(dock.xMax));
            Assert.That(map.yMin, Is.GreaterThanOrEqualTo(dock.yMin));
            Assert.That(map.yMax, Is.LessThanOrEqualTo(dock.yMax));
            var safeScreen = (Rect)playerUi.GetType().GetProperty("SafeScreenRect").GetValue(playerUi);
            Assert.That(dock.xMin, Is.GreaterThanOrEqualTo(safeScreen.xMin));
            Assert.That(dock.yMin, Is.GreaterThanOrEqualTo(safeScreen.yMin));
            Assert.That(dock.xMax, Is.EqualTo(safeScreen.xMax).Within(.01f));
            Assert.That(dock.yMax, Is.LessThanOrEqualTo(safeScreen.yMax + .01f));
            Assert.That(bar.xMin * hud.Scale, Is.EqualTo(dock.xMin).Within(.01f));
            Assert.That(bar.xMax * hud.Scale, Is.EqualTo(dock.xMax).Within(.01f));
            var point = hud.BuildChoiceRect(0).center * hud.Scale;
            var screen = new Vector2(point.x, Screen.height - point.y);
            Assert.That(game.ContainsScreenPoint(screen), Is.True);
            var camera = scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled = false;
            var ray = camera.ScreenPointToRay(screen);
            Assert.That(new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance), Is.True);
            camera.transform.position += (Vector3)game.Production.Rules.Points[0].Position - ray.GetPoint(distance);
            input.Set(mouse.position, screen);
            var zoom = camera.orthographicSize;
            input.Set(mouse.scroll, new Vector2(0, 120));
            input.Press(mouse.leftButton); yield return null; input.Release(mouse.leftButton);
            input.Set(mouse.scroll, Vector2.zero); yield return null;
            Assert.That(game.Production.PointerOverUi, Is.True);
            Assert.That(game.Production.Rules.Buildings.Count, Is.Zero, "A menu click must never build behind the panel.");
            Assert.That(camera.orthographicSize, Is.EqualTo(zoom).Within(.01f));
            var mapPoint = map.center;
            Assert.That(game.ContainsScreenPoint(new Vector2(mapPoint.x, Screen.height - mapPoint.y)), Is.True);
            for (var i = 0; i < 3; i++) Assert.That(game.Gathering.Economy.Inventory.Get((MapResourceKind)i), Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator PauseAndRestartDiscardGeneratorPreviewWithoutReplayingPurchase()
        {
            hud.AddAllResources(100);
            hud.SelectGenerator(ResourceProductionBuilding.IronMine);
            hud.SetDebugPaused(true); yield return null;
            Assert.That(game.Production.IsPlacing, Is.False);
            hud.SelectGenerator(ResourceProductionBuilding.Sawmill);
            Assert.That(game.Production.IsPlacing, Is.False);
            hud.SetDebugPaused(false);
            hud.SelectGenerator(ResourceProductionBuilding.Sawmill);
            hud.RestartRun(); yield return null;
            Assert.That(game.Production.IsPlacing, Is.False);
            Assert.That(game.Production.Rules.Buildings.Count, Is.Zero);
            for (var i = 0; i < 3; i++) Assert.That(game.Gathering.Economy.Inventory.Get((MapResourceKind)i), Is.Zero);
        }
    }
}
#endif
