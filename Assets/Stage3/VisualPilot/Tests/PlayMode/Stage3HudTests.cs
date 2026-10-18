#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HudTests
    {
        private InputTestFixture input;
        private Keyboard keyboard;
        private Mouse mouse;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud hud;
        private float originalTimeScale;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            originalTimeScale = Time.timeScale;
            input = new InputTestFixture(); input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            scene = game.GetComponent<Stage2SceneBootstrap>();
            hud = game.GetComponent<Stage2GameHud>();
            Assert.That(hud, Is.Not.Null);
            game.RunDriver.ResetRun();
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            Time.timeScale = originalTimeScale;
            input.TearDown();
        }

        [UnityTest]
        public IEnumerator DebugGrantsFundRealPurchaseAndWaveStartObeysRunState()
        {
            var inventory = game.Gathering.Economy.Inventory;
            Assert.That(hud.DebugOpen, Is.False);
            Assert.That(hud.AddGold(100), Is.True);
            Assert.That(inventory.Gold, Is.EqualTo(105));
            Assert.That(hud.AddAllResources(1000), Is.True);
            for (var i = 0; i < 3; i++) Assert.That(inventory.Get((MapResourceKind)i), Is.EqualTo(1000));
            game.Building.SelectAndBeginPlacement(0);
            game.Building.Defense.SetPreviewCell(new Vector2Int(37, -7));
            Assert.That(game.Building.Defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(tower, Is.Not.Null);
            Assert.That(inventory.Gold, Is.EqualTo(100));
            Assert.That(hud.StartNextWave(), Is.True);
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            var count = game.Enemies.ActiveCount;
            Assert.That(hud.StartNextWave(), Is.False);
            Assert.That(game.Enemies.ActiveCount, Is.EqualTo(count));
            hud.SetDebugPaused(true);
            hud.RestartRun();
            Assert.That(hud.DebugPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(inventory.Gold, Is.EqualTo(5));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(game.Enemies.ActiveCount, Is.Zero);
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Assert.That(game.RunDriver.Run.WaveNumber, Is.EqualTo(1));
            Assert.That(hud.AddGold(int.MaxValue), Is.True);
            Assert.That(inventory.Gold, Is.EqualTo(int.MaxValue));
            Assert.That(hud.AddGold(1), Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PauseFreezesWorldAndMenuStillAcceptsKeyboardAndResume()
        {
            Assert.That(hud.StartNextWave(), Is.True);
            yield return null;
            var enemy = game.Enemies.GetEnemy(0);
            var position = enemy.transform.position;
            var hero = scene.PlayerMover.transform.position;
            var health = game.RunDriver.Run.ThroneHealth;
            hud.SetGameSpeed(5);
            hud.SetDebugPaused(true);
            input.Press(keyboard.dKey);
            input.Press(keyboard.f3Key);
            yield return new WaitForSecondsRealtime(.2f);
            input.Release(keyboard.dKey); input.Release(keyboard.f3Key);
            Assert.That(hud.DebugOpen, Is.True, "F3 must work even with zero timeScale.");
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(enemy.transform.position, Is.EqualTo(position));
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(hero));
            Assert.That(game.RunDriver.Run.ThroneHealth, Is.EqualTo(health));
            Assert.That(hud.StartNextWave(), Is.False);
            Assert.That(hud.AddAllResources(100), Is.True, "Menu commands remain usable while paused.");
            hud.SetDebugPaused(false);
            Assert.That(Time.timeScale, Is.EqualTo(5));
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(enemy.transform.position, Is.Not.EqualTo(position));
            hud.enabled = false;
            Assert.That(Time.timeScale, Is.EqualTo(originalTimeScale));
            Assert.That(game.RunDriver.Run.IsPaused, Is.False);
        }

        [UnityTest]
        public IEnumerator IndependentSpeedMultipliersChangeMovementAndSimulationAndRestoreOnDisable()
        {
            scene.PlayerMover.transform.position = Vector3.zero;
            var baseSpeed = scene.PlayerMover.Speed;
            hud.SetHeroSpeed(10); hud.SetGameSpeed(3);
            Assert.That(scene.PlayerMover.MovementMultiplier, Is.EqualTo(10));
            Assert.That(scene.PlayerMover.Speed, Is.EqualTo(baseSpeed), "Never rewrite serialized movement tuning.");
            Assert.That(Time.timeScale, Is.EqualTo(3));
            input.Press(keyboard.dKey);
            var before = scene.PlayerMover.transform.position;
            var time = Time.time;
            yield return new WaitForSecondsRealtime(.15f);
            input.Release(keyboard.dKey);
            var elapsed = Time.time - time;
            Assert.That(scene.PlayerMover.transform.position.x - before.x,
                Is.EqualTo(baseSpeed * 10f * elapsed).Within(.15f));
            Assert.That(elapsed, Is.GreaterThan(.25f), "Simulation uses the separate time multiplier.");
            hud.SetGameSpeed(99); hud.SetHeroSpeed(-1);
            Assert.That(hud.GameSpeed, Is.EqualTo(10)); Assert.That(hud.HeroSpeed, Is.EqualTo(1));
            hud.enabled = false;
            Assert.That(Time.timeScale, Is.EqualTo(originalTimeScale));
            Assert.That(scene.PlayerMover.MovementMultiplier, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DebugPanelBlocksRealWorldClickAndWheelWithoutReplacingMinimapBlocker()
        {
            hud.SetDebugOpen(true);
            var area = hud.DebugRect;
            var point = area.center * hud.Scale;
            var screenPoint = new Vector2(point.x, Screen.height - point.y);
            Assert.That(game.ContainsScreenPoint(screenPoint), Is.True);
            var minimap = game.GetComponent<Stage2Minimap>();
            var mapPoint = minimap.PanelRect.center;
            Assert.That(game.ContainsScreenPoint(new Vector2(mapPoint.x, Screen.height - mapPoint.y)), Is.True);
            input.Set(mouse.position, screenPoint);
            var camera = scene.CameraFollow.BoundCamera;
            var zoom = camera.orthographicSize;
            input.Set(mouse.scroll, new Vector2(0, 120));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(camera.orthographicSize, Is.EqualTo(zoom).Within(.01f));
            scene.CameraFollow.enabled = false;
            var cell = new Vector2Int(37, -7);
            var ray = camera.ScreenPointToRay(screenPoint);
            Assert.That(new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance), Is.True);
            camera.transform.position += (Vector3)(scene.MapDefinition.CellToWorld(cell) - (Vector2)ray.GetPoint(distance));
            hud.AddAllResources(100);
            game.Building.SelectAndBeginPlacement(0);
            game.Building.Defense.SetPreviewCell(cell);
            Assert.That(game.Building.Defense.EvaluatePreview(), Is.EqualTo(PlacementResult.Ready));
            input.Press(mouse.leftButton); yield return null; input.Release(mouse.leftButton);
            Assert.That(game.Building.PointerOverUi, Is.True);
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Assert.That(game.Gathering.Economy.Inventory.Gold, Is.EqualTo(5));
            Assert.That(game.Building.PreviewRenderer.gameObject.activeSelf, Is.False);
            hud.enabled = false;
            Assert.That(game.ContainsScreenPoint(new Vector2(mapPoint.x, Screen.height - mapPoint.y)), Is.True);
        }

        [UnityTest]
        public IEnumerator PlayerHudRemainsWhenDevelopmentToolsAreDisabled()
        {
            hud.Initialize(scene, game, false); hud.SetDebugOpen(false);
            Assert.That(hud.DebugAvailable, Is.False);
            hud.SetGameSpeed(10); hud.SetHeroSpeed(10); hud.SetDebugPaused(true);
            Assert.That(Time.timeScale, Is.EqualTo(originalTimeScale));
            Assert.That(scene.PlayerMover.MovementMultiplier, Is.EqualTo(1));
            Assert.That(hud.AddGold(100), Is.False);
            Assert.That(hud.AddAllResources(100), Is.False);
            Assert.That(hud.ResourcesRect.width, Is.GreaterThan(200));
            var playerUi = game.GetComponent("Stage3FantasyHud"); Assert.That(playerUi, Is.Not.Null);
            var dock = (Rect)playerUi.GetType().GetProperty("BottomRect").GetValue(playerUi);
            var map = (Rect)playerUi.GetType().GetProperty("MinimapRect").GetValue(playerUi);
            Assert.That(dock.xMin, Is.GreaterThanOrEqualTo(map.xMax));
            var safeScreen = (Rect)playerUi.GetType().GetProperty("SafeScreenRect").GetValue(playerUi);
            Assert.That(dock.xMin, Is.GreaterThanOrEqualTo(safeScreen.xMin));
            Assert.That(dock.yMin, Is.GreaterThanOrEqualTo(safeScreen.yMin));
            Assert.That(dock.xMax, Is.EqualTo(safeScreen.xMax).Within(.01f));
            Assert.That(dock.yMax, Is.LessThanOrEqualTo(safeScreen.yMax + .01f));
            Assert.That(hud.BuildRect.xMax * hud.Scale, Is.EqualTo(dock.xMax).Within(.01f));
            yield return null;
        }
    }
}
#endif
