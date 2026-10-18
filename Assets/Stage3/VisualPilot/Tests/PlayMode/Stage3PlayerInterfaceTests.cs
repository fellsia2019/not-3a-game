#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3PlayerInterfaceTests
    {
        private const string Evidence = "Artifacts/S3-PLAYER-INTERFACE-20261018";
        private const string CorrectionEvidence = "Artifacts/S3-INTERFACE-CORRECTION-20261018";
        private InputTestFixture input;
        private Keyboard keyboard;
        private Mouse mouse;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud hud;
        private Component playerUi;
        private float previousTimeScale;
        private int previousCaptureRate;

        private string Window => Read<object>(playerUi, "ActiveWindow").ToString();
        private bool Modal => Read<bool>(playerUi, "IsModalOpen");

        [UnitySetUp]
        public IEnumerator Load()
        {
            previousTimeScale = Time.timeScale;
            previousCaptureRate = Time.captureFramerate;
            Time.timeScale = 1f;
            Time.captureFramerate = 60;
            input = new InputTestFixture();
            input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            for (var frame = 0; frame < 12; frame++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.IsInitialized, Is.True);
            scene = game.GetComponent<Stage2SceneBootstrap>();
            hud = game.GetComponent<Stage2GameHud>();
            playerUi = game.GetComponent("Stage3FantasyHud");
            Assert.That(playerUi, Is.Not.Null, "The playable scene must bind the approved player interface.");
            var minimap = game.GetComponent<Stage2Minimap>();
            Assert.That(minimap != null && minimap.SuppressPresentation, Is.True,
                "The late-bound legacy minimap must remain a data source without drawing a duplicate panel.");
            game.RunDriver.ResetRun();
            game.SetPaused(false);
            game.HeroCombat.AutomaticTick = false;
            yield return null;
            Assert.That(Window, Is.EqualTo("None"));
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Directory.CreateDirectory(Evidence);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null;
            yield return null;
            input.TearDown();
            Time.captureFramerate = previousCaptureRate;
            Time.timeScale = previousTimeScale;
        }

        [UnityTest]
        public IEnumerator KeyboardWindowsAreExclusiveAndEscapeDoesNotPauseTheDay()
        {
            var keys = new[] { keyboard.iKey, keyboard.bKey, keyboard.pKey, keyboard.oKey, keyboard.mKey };
            var windows = new[] { "Inventory", "Construction", "Research", "Hero", "Map" };
            var inventory = Stock();
            var run = game.RunDriver.Run;
            var preparation = run.PreparationRemaining;
            for (var index = 0; index < keys.Length; index++)
            {
                yield return Tap(keys[index]);
                Assert.That(Window, Is.EqualTo(windows[index]));
                Assert.That(Modal, Is.EqualTo(index != 1), "Construction must leave the world available for placement.");
                var safeScreen = Read<Rect>(playerUi, "SafeScreenRect");
                AssertContained(new Rect(0f, 0f, Screen.width, Screen.height), safeScreen,
                    "The presentation safe area must fit inside the actual screen.");
                AssertContained(safeScreen, Read<Rect>(playerUi, "BottomRect"),
                    "Hero and construction contents must stay inside the screen safe area.");
                if (Modal)
                {
                    var window = Read<Rect>(playerUi, "CurrentWindowRect");
                    var menuRectMethod = playerUi.GetType().GetMethod("MenuButtonRect", BindingFlags.Public | BindingFlags.Instance);
                    Assert.That(menuRectMethod, Is.Not.Null);
                    var inventoryWindow = Enum.Parse(menuRectMethod.GetParameters()[0].ParameterType, "Inventory");
                    var accessButton = (Rect)menuRectMethod.Invoke(playerUi, new[] { inventoryWindow });
                    Assert.That(window.yMax, Is.LessThanOrEqualTo(accessButton.yMin),
                        "An open window must leave the persistent mouse access buttons visible.");
                    AssertContained(safeScreen, window, "Modal decoration must fit inside the screen safe area.");
                    AssertContained(safeScreen, accessButton, "Persistent mouse access must fit inside the screen safe area.");
                    AssertContained(window, (Rect)Call(playerUi, "PanelContentRect", window),
                        "A window's decoration-safe interior must remain inside its frame.");
                    AssertContained((Rect)Call(playerUi, "PanelContentRect", window), Read<Rect>(playerUi, "WindowContentRect"),
                        "Window controls must use the decoration-safe interior.");
                    AssertContained(accessButton, (Rect)Call(playerUi, "ButtonContentRect", accessButton),
                        "Button labels must have a nonempty interior inside their frame.");
                }
                Assert.That(run.IsPaused, Is.False, "Opening an interface window does not invent a game pause.");
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                Assert.That(game.Building.Defense.IsPlacing, Is.False, "Opening construction is not a purchase or tower selection.");
                yield return Tap(keys[index]);
                Assert.That(Window, Is.EqualTo("None"), "The same shortcut closes its window.");
                Assert.That(Modal, Is.False);
            }
            yield return Tap(keyboard.iKey);
            yield return Tap(keyboard.mKey);
            Assert.That(Window, Is.EqualTo("Map"), "A new window replaces the previous one.");
            yield return Tap(keyboard.escapeKey);
            Assert.That(Window, Is.EqualTo("None"));
            Assert.That(Modal, Is.False);
            Assert.That(run.PreparationRemaining, Is.LessThan(preparation), "The preparation clock keeps advancing through navigation.");
            Assert.That(Stock(), Is.EqualTo(inventory));
            Record("navigation", "I/B/P/O/M toggle one active window; Escape closes; day simulation continues; no resource changes.");
        }

        [UnityTest]
        public IEnumerator ConstructionPanelProtectsClicksAndAllowsFreshWorldPurchaseWithoutClosing()
        {
            var inventory = game.Gathering.Economy.Inventory;
            var defense = game.Building.Defense;
            var catalog = defense.GetCatalog(0);
            inventory.TryAdd(MapResourceKind.Wood, catalog.WoodCost);
            inventory.TryAdd(MapResourceKind.Stone, catalog.StoneCost);
            inventory.TryAdd(MapResourceKind.Metal, catalog.MetalCost);
            if (inventory.Gold < catalog.GoldCost) inventory.TryAddGold(catalog.GoldCost - inventory.Gold);
            yield return Tap(keyboard.bKey);
            Assert.That(Window, Is.EqualTo("Construction"));
            Assert.That(Modal, Is.False);
            hud.SelectTower(0);
            Assert.That(defense.IsPlacing, Is.True);
            var cell = FindLegalCell();
            var camera = scene.CameraFollow.BoundCamera;
            scene.CameraFollow.enabled = false;
            var panelPoint = new Vector2(hud.BuildRect.xMax - 8f, hud.BuildRect.yMax - 8f) * hud.Scale;
            panelPoint.y = Screen.height - panelPoint.y;
            AlignWorldPoint(camera, scene.MapDefinition.CellToWorld(cell), panelPoint);
            Assert.That(game.Building.TryScreenToCell(panelPoint, out var behind), Is.True);
            Assert.That(behind, Is.EqualTo(cell), "The cell behind the panel must really be legal, so protection is exercised.");
            Assert.That(game.ContainsScreenPoint(panelPoint), Is.True);
            var stock = Stock();
            var zoom = camera.orthographicSize;
            input.Set(mouse.position, panelPoint);
            input.Set(mouse.scroll, new Vector2(0f, 120f));
            input.Press(mouse.leftButton);
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return null;
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(Stock(), Is.EqualTo(stock));
            Assert.That(camera.orthographicSize, Is.EqualTo(zoom).Within(.001f));

            var worldPoint = new Vector2(Screen.width * .62f, Screen.height * .62f);
            AlignWorldPoint(camera, scene.MapDefinition.CellToWorld(cell), worldPoint);
            Assert.That(game.ContainsScreenPoint(worldPoint), Is.False);
            input.Set(mouse.position, worldPoint);
            yield return null;
            yield return null;
            Assert.That(defense.TowerCount, Is.Zero, "Dragging a held menu click into the world cannot place a tower.");
            input.Release(mouse.leftButton);
            yield return null;
            Assert.That(game.Building.PreviewResult, Is.EqualTo(PlacementResult.Ready));
            input.Press(mouse.leftButton);
            yield return null;
            yield return null;
            input.Release(mouse.leftButton);
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(defense.GetTower(0).Cell, Is.EqualTo(cell));
            Assert.That(Stock(), Is.EqualTo(new[]
            {
                stock[0] - catalog.GoldCost, stock[1] - catalog.WoodCost,
                stock[2] - catalog.StoneCost, stock[3] - catalog.MetalCost
            }));
            Assert.That(Window, Is.EqualTo("Construction"), "The construction dock remains available after placement.");
            Assert.That(Modal, Is.False);
            Record("construction", "Protected panel click and held drag never buy; fresh world click purchases exactly once while the construction dock stays open.");
        }

        [UnityTest]
        public IEnumerator ModalWindowBlocksHeroAndLocalResearchUntilFreshInputAfterClosing()
        {
            var travel = game.GetComponent("Stage3WorldTravel");
            Assert.That(travel, Is.Not.Null);
            hud.SelectWorkshop();
            var site = Read<Vector2>(travel, "WorkshopPosition");
            Assert.That(Call(travel, "TryBuildWorkshopAt", site).ToString(), Is.EqualTo("Built"));
            scene.PlayerMover.transform.position = Read<Vector2>(travel, "WorkshopInteractionPosition");
            yield return null;
            yield return Tap(keyboard.pKey);
            Assert.That(Window, Is.EqualTo("Research"));
            Assert.That(Modal, Is.True);
            var position = scene.PlayerMover.transform.position;
            var stock = Stock();
            var preparation = game.RunDriver.Run.PreparationRemaining;
            input.Press(keyboard.eKey);
            input.Press(keyboard.dKey);
            input.Press(keyboard.digit2Key);
            input.Press(mouse.leftButton);
            for (var frame = 0; frame < 8; frame++) yield return null;
            Assert.That(Read<bool>(travel, "IsResearched"), Is.False, "A modal research overview must not trigger local E behind it.");
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(position));
            Assert.That(game.Gathering.Economy.GatherProgress, Is.Zero);
            Assert.That(game.Building.Defense.IsPlacing, Is.False);
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Assert.That(Stock(), Is.EqualTo(stock));
            Assert.That(game.RunDriver.Run.PreparationRemaining, Is.LessThan(preparation));
            input.Release(keyboard.dKey);
            input.Release(keyboard.digit2Key);
            input.Release(mouse.leftButton);
            yield return Tap(keyboard.escapeKey);
            Assert.That(Window, Is.EqualTo("None"));
            yield return null;
            Assert.That(Read<bool>(travel, "IsResearched"), Is.False, "Held E from the closed modal must not replay as a local action.");
            input.Release(keyboard.eKey);
            yield return null;
            yield return Tap(keyboard.eKey);
            Assert.That(Read<bool>(travel, "IsResearched"), Is.True, "A fresh local E still performs the existing workshop research.");
            Record("modal-input", "Research window blocks held E, movement, gathering and purchase; day keeps advancing; closing requires fresh E before local research.");
        }

        [UnityTest]
        public IEnumerator RegionalMapNavigationDoesNotMoveWorldCameraAndLifecycleClearsWindows()
        {
            var map = scene.MapDefinition;
            scene.PlayerMover.transform.position = (map.CellToWorld(map.MinimumCell) + map.CellToWorld(map.MaximumCell)) * .5f;
            scene.CameraFollow.enabled = false;
            yield return null;
            var hero = (Vector2)scene.PlayerMover.transform.position;
            var camera = scene.CameraFollow.BoundCamera;
            var position = camera.transform.position;
            var size = camera.orthographicSize;
            yield return Tap(keyboard.mKey);
            Assert.That(Window, Is.EqualTo("Map"));
            var viewport = Read<Rect>(playerUi, "MapViewportRect");
            Assert.That(viewport.width, Is.GreaterThan(100f));
            Assert.That(viewport.height, Is.GreaterThan(100f));
            Call(playerUi, "ZoomMap", 100f);
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(12f));
            Call(playerUi, "RecenterMap");
            Assert.That(Vector2.Distance(Read<Vector2>(playerUi, "MapCenter"), hero), Is.LessThan(.1f));
            var center = Read<Vector2>(playerUi, "MapCenter");
            Call(playerUi, "PanMap", new Vector2(90f, -55f));
            Assert.That(Vector2.Distance(Read<Vector2>(playerUi, "MapCenter"), center), Is.GreaterThan(.01f));
            Call(playerUi, "PanMap", new Vector2(1000000f, -1000000f));
            var bounded = Read<Vector2>(playerUi, "MapCenter");
            var min = map.CellToWorld(map.MinimumCell);
            var max = map.CellToWorld(map.MaximumCell);
            Assert.That(bounded.x, Is.InRange(min.x, max.x));
            Assert.That(bounded.y, Is.InRange(min.y, max.y));
            Call(playerUi, "ZoomMap", .00001f);
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(1f));
            Assert.That(camera.transform.position, Is.EqualTo(position));
            Assert.That(camera.orthographicSize, Is.EqualTo(size));
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(hero));
            game.RunDriver.ResetRun();
            yield return null;
            Assert.That(Window, Is.EqualTo("None"));
            Assert.That(Modal, Is.False);
            yield return Tap(keyboard.iKey);
            var world = game.GetComponent("Stage3ResourceDistrictWorld");
            Assert.That(world, Is.Not.Null);
            Call(world, "Regenerate", 164033);
            for (var frame = 0; frame < 4; frame++) yield return null;
            Assert.That(Window, Is.EqualTo("None"), "Regeneration must not preserve a stale modal or map drag.");
            Assert.That(Modal, Is.False);
            Assert.That(game.Building.Defense.IsPlacing, Is.False);
            Assert.That(Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Count(component => component != null && component.GetType().Name == "Stage3FantasyHud"), Is.EqualTo(1));
            yield return Tap(keyboard.bKey);
            Assert.That(Window, Is.EqualTo("Construction"), "Navigation is still usable after the world rebinds.");
            yield return Tap(keyboard.escapeKey);
            Assert.That(Window, Is.EqualTo("None"));
            Record("map-lifecycle", "Regional map clamps pan/zoom and recenters on hero without moving world camera; reset/regeneration clear windows and retain one usable HUD.");
        }

        [UnityTest]
        public IEnumerator CursorZoomKeepsTwoAsymmetricWorldAnchorsWithoutMovingTheCamera()
        {
            var map = scene.MapDefinition;
            scene.PlayerMover.transform.position = (map.CellToWorld(map.MinimumCell) + map.CellToWorld(map.MaximumCell)) * .5f;
            scene.CameraFollow.enabled = false;
            yield return null;
            yield return Tap(keyboard.mKey);
            var camera = scene.CameraFollow.BoundCamera;
            var cameraPosition = camera.transform.position;
            var cameraRotation = camera.transform.rotation;
            var cameraSize = camera.orthographicSize;
            var hero = scene.PlayerMover.transform.position;
            var viewport = Read<Rect>(playerUi, "MapViewportRect");
            var maxAnchorError = 0f;
            foreach (var normalized in new[] { new Vector2(.23f, .37f), new Vector2(.76f, .68f) })
            {
                Call(playerUi, "ZoomMap", 6f / Read<float>(playerUi, "MapZoom"));
                Call(playerUi, "RecenterMap");
                var cursor = viewport.min + Vector2.Scale(viewport.size, normalized);
                var originalWorld = (Vector2)Call(playerUi, "MapPointToWorld", cursor);
                var originalCenter = Read<Vector2>(playerUi, "MapCenter");
                Assert.That(Vector2.Distance((Vector2)Call(playerUi, "WorldToLargeMapPoint", originalWorld), cursor),
                    Is.LessThan(.05f), "The cursor query and visible map projection must refer to the same world point.");
                Call(playerUi, "ZoomMapAt", 1.5f, cursor);
                Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(9f).Within(.0001f));
                var zoomInError = Vector2.Distance((Vector2)Call(playerUi, "MapPointToWorld", cursor), originalWorld);
                Assert.That(zoomInError, Is.LessThan(.005f), "Zooming in must retain the world point beneath an off-center cursor.");
                Assert.That(Vector2.Distance(Read<Vector2>(playerUi, "MapCenter"), originalCenter), Is.GreaterThan(.01f),
                    "An off-center anchor must change the map center instead of behaving like center zoom.");
                Call(playerUi, "ZoomMapAt", 1f / 1.5f, cursor);
                Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(6f).Within(.0001f));
                var zoomOutError = Vector2.Distance((Vector2)Call(playerUi, "MapPointToWorld", cursor), originalWorld);
                Assert.That(zoomOutError, Is.LessThan(.005f), "Zooming back out must retain the same cursor anchor.");
                Assert.That(Vector2.Distance(Read<Vector2>(playerUi, "MapCenter"), originalCenter), Is.LessThan(.005f));
                maxAnchorError = Mathf.Max(maxAnchorError, Mathf.Max(zoomInError, zoomOutError));
            }
            Assert.That(camera.transform.position, Is.EqualTo(cameraPosition));
            Assert.That(camera.transform.rotation, Is.EqualTo(cameraRotation));
            Assert.That(camera.orthographicSize, Is.EqualTo(cameraSize));
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(hero));
            RecordCorrection("map-cursor-anchor", "Two asymmetric cursor anchors retained across zoom 6 -> 9 -> 6; maximum world error=" +
                maxAnchorError.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "; world camera and hero unchanged.");
        }

        [UnityTest]
        public IEnumerator CursorZoomLimitsAndInvalidInputsDoNotDriftAndMapEdgesStayBounded()
        {
            scene.CameraFollow.enabled = false;
            yield return Tap(keyboard.mKey);
            var viewport = Read<Rect>(playerUi, "MapViewportRect");
            var cursor = viewport.min + Vector2.Scale(viewport.size, new Vector2(.21f, .72f));
            foreach (var limit in new[] { 12f, 1f })
            {
                Call(playerUi, "ZoomMap", limit / Read<float>(playerUi, "MapZoom"));
                var center = Read<Vector2>(playerUi, "MapCenter");
                var world = (Vector2)Call(playerUi, "MapPointToWorld", cursor);
                for (var attempt = 0; attempt < 4; attempt++)
                    Call(playerUi, "ZoomMapAt", limit == 12f ? 2f : .5f, cursor);
                Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(limit));
                Assert.That(Read<Vector2>(playerUi, "MapCenter"), Is.EqualTo(center), "Repeated zoom at a limit cannot drift the map.");
                Assert.That((Vector2)Call(playerUi, "MapPointToWorld", cursor), Is.EqualTo(world));
            }

            Call(playerUi, "ZoomMap", 6f);
            var validCenter = Read<Vector2>(playerUi, "MapCenter");
            var validZoom = Read<float>(playerUi, "MapZoom");
            foreach (var invalidFactor in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f, -1f })
            {
                Call(playerUi, "ZoomMapAt", invalidFactor, cursor);
                Call(playerUi, "ZoomMap", invalidFactor);
                Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(validZoom));
                Assert.That(Read<Vector2>(playerUi, "MapCenter"), Is.EqualTo(validCenter), "Invalid zoom factors are ignored atomically.");
            }
            foreach (var invalidCursor in new[]
            {
                new Vector2(float.NaN, cursor.y), new Vector2(cursor.x, float.PositiveInfinity),
                new Vector2(float.NegativeInfinity, cursor.y)
            })
            {
                Call(playerUi, "ZoomMapAt", 1.2f, invalidCursor);
                Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(validZoom));
                Assert.That(Read<Vector2>(playerUi, "MapCenter"), Is.EqualTo(validCenter), "Invalid cursor coordinates cannot corrupt map navigation.");
            }

            Call(playerUi, "ZoomMap", 12f / Read<float>(playerUi, "MapZoom"));
            var map = scene.MapDefinition;
            var minimum = map.CellCoordinatesToWorld((Vector2)map.MinimumCell - Vector2.one * .5f);
            var maximum = map.CellCoordinatesToWorld((Vector2)map.MaximumCell + Vector2.one * .5f);
            foreach (var edge in new[]
            {
                new Vector2(-1000000f, -1000000f), new Vector2(-1000000f, 1000000f),
                new Vector2(1000000f, -1000000f), new Vector2(1000000f, 1000000f)
            })
            {
                Call(playerUi, "PanMap", edge);
                var anchorBefore = (Vector2)Call(playerUi, "MapPointToWorld", cursor);
                Call(playerUi, "ZoomMapAt", .9f, cursor);
                foreach (var corner in new[]
                {
                    viewport.min, new Vector2(viewport.xMax, viewport.yMin), viewport.max,
                    new Vector2(viewport.xMin, viewport.yMax)
                })
                {
                    var world = (Vector2)Call(playerUi, "MapPointToWorld", corner);
                    Assert.That(world.x, Is.InRange(minimum.x - .005f, maximum.x + .005f));
                    Assert.That(world.y, Is.InRange(minimum.y - .005f, maximum.y + .005f));
                }
                Assert.That(float.IsNaN(anchorBefore.x) || float.IsInfinity(anchorBefore.x), Is.False);
                // At a world edge the clamped center may move the cursor anchor; containment wins there.
                Call(playerUi, "ZoomMap", 12f / Read<float>(playerUi, "MapZoom"));
            }
            RecordCorrection("map-zoom-safety", "Min/max zoom ignores repeated attempts without drift; NaN/infinite/nonpositive inputs are ignored; corner panning and anchored zoom remain inside authored map bounds.");
        }

        [UnityTest]
        public IEnumerator SyntheticTouchGesturePansAndPinchesAroundItsMovingMidpoint()
        {
            var map = scene.MapDefinition;
            scene.PlayerMover.transform.position = (map.CellToWorld(map.MinimumCell) + map.CellToWorld(map.MaximumCell)) * .5f;
            scene.CameraFollow.enabled = false;
            yield return null;
            yield return Tap(keyboard.mKey);
            Call(playerUi, "ZoomMap", 6f / Read<float>(playerUi, "MapZoom"));
            Call(playerUi, "RecenterMap");
            var touchscreen = InputSystem.AddDevice<Touchscreen>();
            var viewport = Read<Rect>(playerUi, "MapViewportRect");
            var camera = scene.CameraFollow.BoundCamera;
            var cameraPosition = camera.transform.position;
            var cameraSize = camera.orthographicSize;
            var midpoint = viewport.center + Vector2.Scale(viewport.size, new Vector2(.11f, -.08f));
            var halfSpan = Mathf.Min(viewport.width * .11f, viewport.height * .2f);
            var first = midpoint - Vector2.right * halfSpan;
            var second = midpoint + Vector2.right * halfSpan;
            input.BeginTouch(1, GuiToScreen(first), queueEventOnly: true, screen: touchscreen);
            input.BeginTouch(2, GuiToScreen(second), queueEventOnly: true, screen: touchscreen);
            InputSystem.Update();
            yield return null;
            yield return null;
            var worldAtMidpoint = (Vector2)Call(playerUi, "MapPointToWorld", midpoint);
            var originalCenter = Read<Vector2>(playerUi, "MapCenter");
            var originalZoom = Read<float>(playerUi, "MapZoom");
            var movedMidpoint = midpoint + Vector2.Scale(viewport.size, new Vector2(.035f, .06f));
            first = movedMidpoint - Vector2.right * (halfSpan * 1.4f);
            second = movedMidpoint + Vector2.right * (halfSpan * 1.4f);
            input.MoveTouch(1, GuiToScreen(first), queueEventOnly: true, screen: touchscreen);
            input.MoveTouch(2, GuiToScreen(second), queueEventOnly: true, screen: touchscreen);
            InputSystem.Update();
            yield return null;
            yield return null;
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(originalZoom * 1.4f).Within(.002f));
            Assert.That(Vector2.Distance((Vector2)Call(playerUi, "MapPointToWorld", movedMidpoint), worldAtMidpoint),
                Is.LessThan(.01f), "The world under the moving two-finger midpoint must follow the gesture.");
            Assert.That(Vector2.Distance(Read<Vector2>(playerUi, "MapCenter"), originalCenter), Is.GreaterThan(.01f));
            input.EndTouch(1, GuiToScreen(first), queueEventOnly: true, screen: touchscreen);
            input.EndTouch(2, GuiToScreen(second), queueEventOnly: true, screen: touchscreen);
            InputSystem.Update();
            yield return null;
            yield return null;

            var dragStart = viewport.center;
            var dragEnd = dragStart + Vector2.Scale(viewport.size, new Vector2(.04f, .06f));
            var worldAtFinger = (Vector2)Call(playerUi, "MapPointToWorld", dragStart);
            var pinchZoom = Read<float>(playerUi, "MapZoom");
            input.BeginTouch(3, GuiToScreen(dragStart), screen: touchscreen);
            yield return null;
            input.MoveTouch(3, GuiToScreen(dragEnd), screen: touchscreen);
            yield return null;
            yield return null;
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(pinchZoom), "One-finger drag pans without changing zoom.");
            Assert.That(Vector2.Distance((Vector2)Call(playerUi, "MapPointToWorld", dragEnd), worldAtFinger),
                Is.LessThan(.01f), "The grabbed map point follows a single finger.");
            input.EndTouch(3, GuiToScreen(dragEnd), screen: touchscreen);
            yield return null;
            Assert.That(camera.transform.position, Is.EqualTo(cameraPosition));
            Assert.That(camera.orthographicSize, Is.EqualTo(cameraSize));
            RecordCorrection("map-synthetic-touch", "Automated synthetic Input System Touchscreen events: a moving two-finger pinch preserves its world anchor and zooms; a fresh one-finger drag pans without zoom; the world camera is unchanged. No physical touch hardware was tested.");
        }

        private IEnumerator Tap(ButtonControl button)
        {
            input.Release(button);
            yield return null;
            input.Press(button);
            yield return null;
            yield return null;
            input.Release(button);
            yield return null;
        }

        private Vector2Int FindLegalCell()
        {
            var map = scene.MapDefinition;
            var preferred = new Vector2Int(37, -7);
            if (game.Building.Defense.Evaluate(preferred) == PlacementResult.Ready) return preferred;
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
            for (var x = map.MinimumCell.x; x <= map.MaximumCell.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (game.Building.Defense.Evaluate(cell) == PlacementResult.Ready) return cell;
            }
            Assert.Fail("The authored road must contain a legal construction cell.");
            return default;
        }

        private static void AlignWorldPoint(Camera camera, Vector2 world, Vector2 screen)
        {
            var ray = camera.ScreenPointToRay(screen);
            Assert.That(new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance), Is.True);
            camera.transform.position += (Vector3)world - ray.GetPoint(distance);
        }

        private static Vector2 GuiToScreen(Vector2 guiPoint) => new Vector2(guiPoint.x, Screen.height - guiPoint.y);

        private int[] Stock()
        {
            var inventory = game.Gathering.Economy.Inventory;
            return new[] { inventory.Gold, inventory.Get(MapResourceKind.Wood), inventory.Get(MapResourceKind.Stone), inventory.Get(MapResourceKind.Metal) };
        }

        private static T Read<T>(Component component, string name)
        {
            var property = component.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null, component.GetType().Name + "." + name + " must expose the agreed integration contract.");
            return (T)property.GetValue(component);
        }

        private static object Call(Component component, string name, params object[] arguments)
        {
            var method = component.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, component.GetType().Name + "." + name + " must expose the agreed integration contract.");
            return method.Invoke(component, arguments);
        }

        private static void AssertContained(Rect outer, Rect inner, string reason)
        {
            Assert.That(inner.width, Is.GreaterThan(0f), reason);
            Assert.That(inner.height, Is.GreaterThan(0f), reason);
            Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin - .01f), reason);
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin - .01f), reason);
            Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax + .01f), reason);
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax + .01f), reason);
        }

        private static void RecordCorrection(string name, string detail)
        {
            Directory.CreateDirectory(CorrectionEvidence);
            File.WriteAllText(Path.Combine(CorrectionEvidence, name + ".txt"), detail + "\n");
        }

        private static void Record(string name, string detail) => File.WriteAllText(Path.Combine(Evidence, name + ".txt"), detail + "\n");
    }
}
#endif
