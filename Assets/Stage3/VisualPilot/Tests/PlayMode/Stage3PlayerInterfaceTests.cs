#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
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
        private bool previousPointerMerging;

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
            previousPointerMerging=InputSystem.settings.disableRedundantEventsMerging;
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
            Assert.That(InputSystem.settings.disableRedundantEventsMerging,Is.EqualTo(previousPointerMerging),
                "The scene input owner must restore the original mouse merging setting.");
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
                    Assert.That(window, Is.EqualTo(new Rect(0f, 0f, Screen.width, Screen.height)),
                        "Every modal page must cover the complete backbuffer, including former world-HUD margins.");
                    var header = Read<Rect>(playerUi, "FullscreenHeaderRect");
                    var wallet = Read<Rect>(playerUi, "FullscreenWalletRect");
                    var footer = Read<Rect>(playerUi, "FullscreenFooterRect");
                    var content = Read<Rect>(playerUi, "WindowContentRect");
                    AssertContained(window, header, "Shared navigation must remain inside the full-screen page.");
                    AssertContained(window, wallet, "The shared wallet must remain visible inside the page.");
                    AssertContained(window, footer, "Page controls must remain inside the backbuffer.");
                    AssertContained(window, content, "Page content must remain inside the backbuffer.");
                    AssertContained(header, accessButton, "Mouse navigation belongs in the shared full-screen header.");
                    Assert.That(accessButton, Is.EqualTo((Rect)Call(playerUi, "FullscreenTabRect", inventoryWindow)));
                    Assert.That(content.yMin, Is.GreaterThanOrEqualTo(wallet.yMax), "Wallet and content cannot overlap.");
                    Assert.That(content.yMax, Is.LessThanOrEqualTo(footer.yMin), "Content cannot cover footer controls.");
                    var constructionWindow = Enum.Parse(menuRectMethod.GetParameters()[0].ParameterType, "Construction");
                    Assert.That((Rect)Call(playerUi, "FullscreenTabRect", constructionWindow), Is.EqualTo(Rect.zero),
                        "Construction must never gain a full-screen catalogue or navigation tab.");
                    foreach (var corner in new[] { Vector2.one, new Vector2(Screen.width - 1f, 1f),
                        new Vector2(1f, Screen.height - 1f), new Vector2(Screen.width - 1f, Screen.height - 1f) })
                        Assert.That(game.ContainsScreenPoint(corner), Is.True,
                            "The entire full-screen page must protect the world behind it, including its corners.");
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
            Stage3TestCycle.FundBaseConstruction(game);hud.SelectWorkshop();
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
            Assert.That(Window,Is.EqualTo("Research"),"A fresh local E opens the research page.");
            Assert.That(Read<bool>(travel,"IsResearched"),Is.False,"Opening a page does not study.");
            Call(playerUi,"SelectResearchEntry",-1);
            Assert.That(Call(game.GetComponent("Stage3PlayerInterfaceBinding"),"TryStudyResearch",-1),Is.True);
            Assert.That(Read<bool>(travel,"IsResearched"),Is.True);
            Record("modal-input", "Research window blocks held E, movement, gathering and purchase; day keeps advancing; fresh local E opens the page and explicit Study grants research.");
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
        public IEnumerator TrackpadRoutesPinchAndPanToOnlyTheVisibleSurface()
        {
            var map = scene.MapDefinition;
            scene.PlayerMover.transform.position = (map.CellToWorld(map.MinimumCell) + map.CellToWorld(map.MaximumCell)) * .5f;
            yield return null;
            yield return Tap(keyboard.mKey);
            Call(playerUi, "ZoomMap", 6f / Read<float>(playerUi, "MapZoom"));
            Call(playerUi, "RecenterMap");
            var follow = scene.CameraFollow;
            var camera = follow.BoundCamera;
            var size = camera.orthographicSize;
            var viewport = Read<Rect>(playerUi, "MapViewportRect");
            var pointer = viewport.center + new Vector2(25f, -15f);
            var world = (Vector2)Call(playerUi, "MapPointToWorld", pointer);
            follow.ApplyTrackpadGesture(1.5f, Vector2.zero, GuiToScreen(pointer));
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(9f).Within(.001f));
            Assert.That(Vector2.Distance((Vector2)Call(playerUi, "MapPointToWorld", pointer), world), Is.LessThan(.01f));
            var pan = new Vector2(36f, -28f);
            follow.ApplyTrackpadGesture(1f, pan, GuiToScreen(pointer));
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(9f), "Two-finger translation pans without magnifying.");
            Assert.That(Vector2.Distance((Vector2)Call(playerUi, "MapPointToWorld", pointer + pan), world), Is.LessThan(.01f));
            var center = Read<Vector2>(playerUi, "MapCenter");
            follow.ApplyTrackpadGesture(2f, pan, GuiToScreen(new Vector2(20f, 20f)));
            Assert.That(Read<Vector2>(playerUi, "MapCenter"), Is.EqualTo(center), "The header does not accept map gestures.");
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(9f));
            follow.ApplyTrackpadGesture(float.NaN, new Vector2(float.NaN, 0), GuiToScreen(pointer));
            Assert.That(Read<Vector2>(playerUi, "MapCenter"), Is.EqualTo(center));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(camera.orthographicSize, Is.EqualTo(size).Within(.001f), "Map gestures never zoom the world camera.");
            yield return Tap(keyboard.escapeKey);
            follow.ApplyTrackpadGesture(1.25f, Vector2.zero, GuiToScreen(pointer));
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize, Is.EqualTo(size / 1.25f).Within(.03f), "Closing M returns pinch to the world.");
            Assert.That(Read<float>(playerUi, "MapZoom"), Is.EqualTo(9f), "World pinch cannot mutate the closed map.");
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

        [UnityTest]
        public IEnumerator UnifiedHudContainsMinimapTenReservesAndSeparateProtectedRibbon()
        {
            var dock = Read<Rect>(playerUi, "BottomRect");
            var safe = Read<Rect>(playerUi, "SafeScreenRect");
            var ribbon = Read<Rect>(playerUi, "NavigationRibbonRect");
            Assert.That(dock.xMin, Is.EqualTo(safe.xMin).Within(.01f));
            Assert.That(dock.xMax, Is.EqualTo(safe.xMax).Within(.01f));
            AssertContained(dock, Read<Rect>(playerUi, "MinimapRect"), "Minimap must belong to the continuous main dock.");
            Assert.That(ribbon.xMin, Is.EqualTo(dock.xMin).Within(.01f));
            Assert.That(ribbon.xMax, Is.EqualTo(dock.xMax).Within(.01f));
            Assert.That(ribbon.yMax, Is.EqualTo(dock.yMin).Within(.01f));
            Assert.That(ribbon.height, Is.LessThan(dock.height * .4f));
            Assert.That((int)playerUi.GetType().GetField("AbilityReserveCount").GetValue(null), Is.EqualTo(10));
            for (var i = 0; i < 10; i++)
            {
                var slot = (Rect)Call(playerUi, "AbilityReserveRect", i);
                AssertContained(dock, slot, "Every reserve must fit the main HUD.");
                for (var j = 0; j < i; j++)
                    Assert.That(slot.Overlaps((Rect)Call(playerUi, "AbilityReserveRect", j)), Is.False);
            }
            Assert.That(((Rect)Call(playerUi, "AbilityReserveRect", 5)).y,
                Is.GreaterThan(((Rect)Call(playerUi, "AbilityReserveRect", 0)).y));
            foreach (var x in new[] { ribbon.xMin + 3f, ribbon.center.x, ribbon.xMax - 3f })
                Assert.That(game.ContainsScreenPoint(GuiToScreen(new Vector2(x, ribbon.center.y))), Is.True,
                    "Empty ribbon backing also blocks world clicks and zoom.");
            for (var i = 0; i < 3; i++)
                AssertContained(dock, (Rect)Call(playerUi, "HeroStatusBarRect", i), "Numeric status bars must fit the dock.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator WorldSelectionShowsLiveTowerHealthClearsOnRemovalAndRejectsHeldUiDrag()
        {
            var binding = game.GetComponent("Stage3PlayerInterfaceBinding");
            var tower = BuildSelectionTower();
            yield return null;
            var point = FrameBuilding(tower.GetComponentsInChildren<Renderer>().First(r => r.enabled));
            yield return Click(point);
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.True);
            Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("Tower"));
            var selectedView = Read<object>(playerUi, "SelectedBuildingView");
            Assert.That(selectedView.GetType().GetField("Icon").GetValue(selectedView), Is.Not.Null,
                "The selected tower must use a real native thumbnail rather than an empty medallion.");
            var selection = Read<Rect>(playerUi, "SelectedBuildingRect");
            var ribbon = Read<Rect>(playerUi, "NavigationRibbonRect");
            Assert.That(selection.yMax, Is.EqualTo(ribbon.yMin).Within(.01f));
            Assert.That(selection.width, Is.EqualTo(ribbon.width).Within(.01f));
            var stock = Stock(); var hero = scene.PlayerMover.transform.position;
            tower.TakeDamage(2); yield return null;
            Assert.That(SelectedStat("Здоровье"), Is.EqualTo(tower.Health + "/" + tower.MaximumHealth));
            yield return Click(GuiToScreen(selection.center));
            Assert.That(Stock(), Is.EqualTo(stock), "Clicking a card or unavailable action cannot pay for anything.");
            Assert.That(game.Building.Defense.TowerCount, Is.EqualTo(1));
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(hero));
            yield return Tap(keyboard.escapeKey);
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False);
            var dockPoint = GuiToScreen(Read<Rect>(playerUi, "NavigationRibbonRect").center);
            input.Set(mouse.position, dockPoint); input.Press(mouse.leftButton); yield return null;
            input.Set(mouse.position, point); yield return null;
            input.Release(mouse.leftButton); yield return null;
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False, "A held UI press dragged into the world is not selection.");
            yield return Click(point);
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.True);
            tower.TakeDamage(tower.Health); yield return null; yield return null;
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False, "A destroyed target must not leave a stale card.");
            Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("None"));
        }

        [UnityTest]
        public IEnumerator WorkshopSelectionClearsForResearchWithoutBuyingAndLifecycleRequiresFreshClicks()
        {
            var travel = game.GetComponent("Stage3WorldTravel");
            Stage3TestCycle.FundBaseConstruction(game);Call(travel, "BeginWorkshopPlacement");
            var site = Read<Vector2>(travel, "WorkshopPosition");
            Assert.That(Call(travel, "TryBuildWorkshopAt", site).ToString(), Is.EqualTo("Built"));
            yield return null;
            var visual = Read<SpriteRenderer>(travel, "WorkshopVisual");
            var point = FrameBuilding(visual);
            yield return Click(point);
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.True);
            var stock = Stock();
            var view = Read<object>(playerUi, "SelectedBuildingView");
            Assert.That((string)view.GetType().GetField("Subtitle").GetValue(view), Is.EqualTo("Изучение чертежей"));
            Assert.That((bool)view.GetType().GetField("ShowUpgrade").GetValue(view), Is.False);
            // Native IMGUI buttons are verified in the real player; synthetic Input System
            // mouse events do not create Editor GUI.Button events in batchmode.
            yield return Tap(keyboard.pKey);
            Assert.That(Window, Is.EqualTo("Research"));
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False);
            Assert.That(Read<bool>(travel, "IsResearched"), Is.False, "The card opens an overview, not remote research.");
            Assert.That(Stock(), Is.EqualTo(stock));
            yield return Tap(keyboard.escapeKey);
            yield return Click(point);
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.True);
            game.SetPaused(true); yield return null;
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False);
            game.SetPaused(false); yield return null;
            yield return Click(point);
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.True);
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False);
            Assert.That(Read<bool>(travel, "WorkshopBuilt"), Is.False);
        }

        [UnityTest]
        public IEnumerator AllProductionKindsDwellingAndThroneExposeActualOwnedBuildingData()
        {
            var inventory = game.Gathering.Economy.Inventory;
            foreach (MapResourceKind resource in Enum.GetValues(typeof(MapResourceKind))) inventory.TryAdd(resource, 200);
            scene.PlayerMover.transform.position = scene.ThroneMarker.position + Vector3.right * 5f;
            var production = game.Production;
            foreach (ResourceProductionBuilding kind in Enum.GetValues(typeof(ResourceProductionBuilding)))
            {
                var pointIndex = Enumerable.Range(0, production.Rules.Points.Count)
                    .First(i => production.Rules.Points[i].Building == kind);
                Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(kind));
                production.SelectAndBeginPlacement(kind);
                Assert.That(production.TryBuildAt(pointIndex), Is.EqualTo(ProductionPlacementResult.Built));
                Call(playerUi, "CloseWindow"); yield return null;
                var building = production.Rules.Buildings.Last();
                var visual = production.GetBuiltVisual(building).GetComponent<SpriteRenderer>();
                FrameBuilding(visual);
                yield return Click(OpaquePoint(visual));
                Assert.That(Read<object>(game.GetComponent("Stage3PlayerInterfaceBinding"), "SelectedBuildingKind").ToString(), Is.EqualTo("Production"));
                Assert.That(SelectedStat("За цикл"), Is.EqualTo("+" + building.Entry.OutputAmount));
                Assert.That(SelectedStat("Ресурс"), Is.EqualTo(kind == ResourceProductionBuilding.Sawmill ? "Дерево" :
                    kind == ResourceProductionBuilding.Quarry ? "Камень" : "Металл"));
                yield return Tap(keyboard.escapeKey);
            }
            var dwelling = game.GetComponent("Stage3Dwelling");
            Call(dwelling, "BeginPlacement");
            var built = false;
            for (var x = -14; x <= 14 && !built; x += 2)
            for (var y = -14; y <= 14 && !built; y += 2)
            {
                var site = (Vector2)scene.ThroneMarker.position + new Vector2(x, y);
                if (Call(dwelling, "EvaluateAt", site).ToString() != "Ready") continue;
                built = Call(dwelling, "TryBuildAt", site).ToString() == "Built";
            }
            Assert.That(built, Is.True); yield return null;
            var home = Read<SpriteRenderer>(game.GetComponent("Stage3DwellingPresentation"), "BuiltVisual");
            FrameBuilding(home); yield return Click(OpaquePoint(home));
            var binding = game.GetComponent("Stage3PlayerInterfaceBinding");
            Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("Dwelling"));
            yield return Tap(keyboard.escapeKey);
            var dome = scene.ThroneMarker.GetComponentsInChildren<Component>().First(c => c.GetType().Name == "Stage3LandmarkSprite");
            var domeBody = Read<SpriteRenderer>(dome, "Body");
            FrameBuilding(domeBody); yield return Click(OpaquePoint(domeBody));
            Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("Throne"));
            Assert.That(SelectedStat("Здоровье"), Is.EqualTo(game.RunDriver.Run.ThroneHealth + "/" + game.RunDriver.Run.ThroneMaximum));
        }

        private Vector2 OpaquePoint(SpriteRenderer visual)
        {
            var sprite = visual.sprite;
            var camera = scene.CameraFollow.BoundCamera;
            if (sprite.texture.isReadable)
            {
                var rect = sprite.textureRect;
                for (var y = 5; y <= 15; y++)
                for (var x = 5; x <= 15; x++)
                {
                    var u = x / 20f; var v = y / 20f;
                    var px = Mathf.Clamp((int)(rect.x + u * rect.width), 0, sprite.texture.width - 1);
                    var py = Mathf.Clamp((int)(rect.y + v * rect.height), 0, sprite.texture.height - 1);
                    if (sprite.texture.GetPixel(px, py).a < .8f) continue;
                    var point = visual.transform.TransformPoint(new Vector3(Mathf.Lerp(sprite.bounds.min.x, sprite.bounds.max.x, u),
                        Mathf.Lerp(sprite.bounds.min.y, sprite.bounds.max.y, v), 0));
                    return camera.WorldToScreenPoint(point);
                }
            }
            return camera.WorldToScreenPoint(visual.bounds.center);
        }

        [UnityTest]
        public IEnumerator SelectedCardUsesLivePricesUpgradesBeyondFiveAndPaysSaleOnce()
        {
            var binding = game.GetComponent("Stage3PlayerInterfaceBinding");
            var tower = BuildSelectionTower(); yield return null;
            var point = FrameBuilding(tower.GetComponentsInChildren<Renderer>().First(r => r.enabled));
            yield return Click(point);
            yield return Tap(keyboard.bKey);
            Assert.That(Window, Is.EqualTo("Construction"));
            Assert.That(Call(binding, "TrySelectBuildingAtScreen", point), Is.True);
            var inventory = game.Gathering.Economy.Inventory;
            inventory.Reset(); yield return null;
            object View() => Read<object>(playerUi, "SelectedBuildingView");
            bool UpgradeAvailable() => (bool)View().GetType().GetField("UpgradeAvailable").GetValue(View());
            Assert.That(UpgradeAvailable(), Is.False);
            var defense = game.Building.Defense;
            var data = defense.GetTowerCatalog(tower); data.TryGetUpgrade(1, out var next);
            inventory.TryCredit(next.Cost); yield return null;
            Assert.That(UpgradeAvailable(), Is.True, "Gathering enough resources must refresh the selected card.");
            Assert.That(Call(binding, "TryUpgradeSelectedBuilding").ToString(), Is.EqualTo("Ready"));
            Assert.That(tower.Level, Is.EqualTo(2)); Assert.That(inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(SelectedStat("Урон"), Is.EqualTo(tower.AttackDamage.ToString()));
            Assert.That(data.UnlimitedUpgrades, Is.True);
            while (tower.Level < 8)
            {
                Assert.That(data.TryGetUpgrade(tower.Level, out var paidNext), Is.True);
                Assert.That(inventory.TryCredit(paidNext.Cost), Is.True);
                Assert.That(Call(binding, "TryUpgradeSelectedBuilding").ToString(), Is.EqualTo("Ready"));
            }
            Assert.That(tower.Level, Is.EqualTo(8));
            Assert.That(data.TryGetUpgrade(tower.Level, out var ninth), Is.True);
            Assert.That(inventory.TryCredit(ninth.Cost), Is.True);
            yield return null;
            Assert.That(UpgradeAvailable(), Is.True);
            Assert.That((string)View().GetType().GetField("Subtitle").GetValue(View()), Is.EqualTo("Уровень 8"));
            var refund = defense.GetSaleRefund(tower); inventory.Reset();
            Assert.That(Call(binding, "TrySellSelectedBuilding").ToString(), Is.EqualTo("Ready"));
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False);
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(refund.Wood));
            Assert.That(inventory.Get(MapResourceKind.Stone), Is.EqualTo(refund.Stone));
            Assert.That(Call(binding, "TrySellSelectedBuilding").ToString(), Is.EqualTo("InvalidTarget"));
            Assert.That(defense.TowerCount, Is.Zero); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(refund.Wood));
        }

        [UnityTest]
        public IEnumerator ExistingTowerSelectionPreemptsAllBuildingDraftsWithoutClosingConstruction()
        {
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintKind.Sawmill);
            var binding = game.GetComponent("Stage3PlayerInterfaceBinding");
            var tower = BuildSelectionTower(); yield return null;
            var point = FrameBuilding(tower.GetComponentsInChildren<Renderer>().First(r => r.enabled));
            var defense = game.Building.Defense;
            var production = game.Production;
            // Exercise the player's normal path: one catalogue card arms a draft, then
            // a real world click chooses an existing building instead of buying beneath it.
            foreach (var choice in new[] { (group:0,index:0), (group:1,index:0), (group:2,index:0), (group:2,index:1) })
            {
                if (Window != "Construction") yield return Tap(keyboard.bKey);
                Call(playerUi, "SetBuildGroup", choice.group);
                Assert.That(Call(playerUi, "ActivateBuildChoice", choice.index), Is.True);
                yield return null;
                Assert.That(Window, Is.EqualTo("Construction"));
                var stock = Stock(); var count = defense.TowerCount;
                yield return Click(point);
                Assert.That(Window, Is.EqualTo("Construction"));
                Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.True);
                Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("Tower"));
                Assert.That(defense.IsPlacing || production.IsPlacing ||
                    Read<bool>(game.GetComponent("Stage3WorldTravel"), "IsPlacingWorkshop") ||
                    Read<bool>(game.GetComponent("Stage3Dwelling"), "IsPlacing"), Is.False);
                Assert.That(Stock(), Is.EqualTo(stock), "Selecting a built object must never pay for a draft.");
                Assert.That(defense.TowerCount, Is.EqualTo(count));
                var strip = Read<Rect>(playerUi, "SelectedBuildingRect");
                Assert.That(strip.height, Is.GreaterThan(0));
                Assert.That(strip.yMax, Is.EqualTo(Read<Rect>(playerUi,"NavigationRibbonRect").yMin).Within(.01f));
            }
        }

        [UnityTest]
        public IEnumerator WorldDragRejectsOccupiedTowerAndEscapeOrModalCancelsWithoutPayment()
        {
            var binding = game.GetComponent("Stage3PlayerInterfaceBinding");
            var source = BuildSelectionTower(); var defense = game.Building.Defense;
            defense.BeginPlacement(); Stage2Tower target = null;
            for (var radius = 1; radius <= 3 && target == null; radius++)
            for (var y = -radius; y <= radius && target == null; y++)
            for (var x = -radius; x <= radius && target == null; x++)
            {
                var cell = source.Cell + new Vector2Int(x,y);
                if (defense.Evaluate(cell) != PlacementResult.Ready) continue;
                defense.SetPreviewCell(cell); defense.TryConfirm(out target);
            }
            Assert.That(target, Is.Not.Null); yield return null;
            yield return Tap(keyboard.bKey);
            Assert.That(Call(playerUi, "ActivateBuildChoice", 0), Is.True);
            Assert.That(defense.IsPlacing, Is.True);
            var sourcePoint = FrameBuilding(source.GetComponentsInChildren<Renderer>().First(r => r.enabled));
            var hero = scene.PlayerMover.transform.position; var stock = Stock();
            input.Set(mouse.position, sourcePoint); input.Press(mouse.leftButton); yield return null;
            Assert.That(defense.IsPlacing, Is.False, "Owned selection must win before the placement driver.");
            Assert.That(Window, Is.EqualTo("Construction"));
            Assert.That(Read<bool>(playerUi, "HasSelectedBuilding"), Is.False,
                "New card actions must not appear beneath a mouse-down that started in the world.");
            input.Set(mouse.position, sourcePoint + Vector2.right * 20); yield return null;
            Assert.That(Read<bool>(binding, "IsDraggingBuilding"), Is.True);
            Assert.That(Read<bool>(playerUi,"HasSelectedBuilding"), Is.True);
            input.Press(keyboard.wKey); yield return null; yield return null; input.Release(keyboard.wKey);
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(hero));
            var camera = scene.CameraFollow.BoundCamera;
            var destination = source.Cell; var movePoint = Vector2.zero; var foundMove = false;
            foreach (var offset in new[] {Vector2Int.right*2,Vector2Int.left*2,Vector2Int.up*2,Vector2Int.up})
            {
                var candidate = source.Cell + offset;
                var screen = (Vector2)camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(candidate));
                if (defense.EvaluateMove(source,candidate) != Stage2TowerActionResult.Ready || game.ContainsScreenPoint(screen)) continue;
                destination = candidate; movePoint = screen; foundMove = true; break;
            }
            Assert.That(foundMove, Is.True);
            input.Set(mouse.position, movePoint); yield return null;
            Assert.That(Read<bool>(binding,"DragDropValid"), Is.True);
            Assert.That(Read<bool>(binding,"DragWillMerge"), Is.False, "An empty-cell move is not a merge.");
            input.Release(mouse.leftButton); yield return null; yield return null;
            Assert.That(source.Cell, Is.EqualTo(destination)); Assert.That(Stock(), Is.EqualTo(stock));
            Assert.That(Window, Is.EqualTo("Construction"));
            sourcePoint = camera.WorldToScreenPoint(source.GetComponentsInChildren<Renderer>().First(r=>r.enabled).bounds.center);
            input.Set(mouse.position, sourcePoint); input.Press(mouse.leftButton); yield return null;
            input.Set(mouse.position, sourcePoint + Vector2.right*20); yield return null;
            Assert.That(Read<bool>(binding,"IsDraggingBuilding"), Is.True);
            var targetRenderer = target.GetComponentsInChildren<Renderer>().First(r => r.enabled);
            var targetPoint = (Vector2)scene.CameraFollow.BoundCamera.WorldToScreenPoint(targetRenderer.bounds.center);
            var sourceCell = source.Cell; var targetCell = target.Cell;
            var sourceHealth = source.Health; var targetHealth = target.Health;
            input.Set(mouse.position, targetPoint); yield return null;
            Assert.That(Read<bool>(binding, "DragDropValid"), Is.False, "Occupied tower silhouettes must reject drops: " + targetPoint);
            Assert.That(Read<bool>(binding, "DragWillMerge"), Is.False);
            input.Release(mouse.leftButton); yield return null; yield return null;
            Assert.That(source.Level, Is.EqualTo(1)); Assert.That(target.Level, Is.EqualTo(1));
            Assert.That(defense.TowerCount, Is.EqualTo(2));
            Assert.That(source.Cell, Is.EqualTo(sourceCell)); Assert.That(source.Health, Is.EqualTo(sourceHealth));
            Assert.That(target.Health, Is.EqualTo(targetHealth));
            Assert.That(target.Cell, Is.EqualTo(targetCell));
            Assert.That(Window, Is.EqualTo("Construction"));
            Assert.That(Read<bool>(playerUi,"HasSelectedBuilding"), Is.True);
            Assert.That(Stock(), Is.EqualTo(stock)); Assert.That(Read<bool>(binding, "IsDraggingBuilding"), Is.False);
            Assert.That(Read<bool>(binding, "DragWillMerge"), Is.False, "Drop cannot leave a stale merge highlight.");
            // A short real drag can arrive entirely within one Input System update.
            // Its press position must survive the later motion/release events.
            var fastOrigin=FrameBuilding(targetRenderer);yield return null;
            Assert.That(game.ContainsScreenPoint(fastOrigin),Is.False,"Batched drag must start in exposed world above the taller construction dock.");
            var fastCell = target.Cell; var fastPoint = Vector2.zero; var foundFast = false;
            foreach (var offset in new[] {Vector2Int.up*2,Vector2Int.right*2,Vector2Int.left*2})
            {
                var candidate = target.Cell+offset;
                var screen = (Vector2)camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(candidate));
                if (defense.EvaluateMove(target,candidate)!=Stage2TowerActionResult.Ready||game.ContainsScreenPoint(screen))continue;
                fastCell=candidate;fastPoint=screen;foundFast=true;break;
            }
            Assert.That(foundFast,Is.True);
            InputSystem.QueueStateEvent(mouse,new MouseState {position=fastOrigin}.WithButton(MouseButton.Left));
            InputSystem.QueueStateEvent(mouse,new MouseState {position=fastPoint}.WithButton(MouseButton.Left));
            InputSystem.QueueStateEvent(mouse,new MouseState {position=fastPoint});
            yield return null;yield return null;
            Assert.That(target.Cell,Is.EqualTo(fastCell),"Batched down/move/up must keep the actual world press origin.");
            Assert.That(Read<bool>(playerUi,"HasSelectedBuilding"),Is.True);
            Assert.That(Window,Is.EqualTo("Construction"));Assert.That(Stock(),Is.EqualTo(stock));
            Assert.That(Call(binding, "TryBeginBuildingDrag", target), Is.True);
            yield return Tap(keyboard.escapeKey); input.Release(mouse.leftButton); yield return null;
            Assert.That(Read<bool>(binding, "IsDraggingBuilding"), Is.False);
            Assert.That(source.Level, Is.EqualTo(1)); Assert.That(target.Level, Is.EqualTo(1));
            Assert.That(defense.TowerCount, Is.EqualTo(2));
            Assert.That(source.Cell, Is.EqualTo(sourceCell)); Assert.That(source.Health, Is.EqualTo(sourceHealth));
            Assert.That(target.Health, Is.EqualTo(targetHealth));
            Assert.That(Call(binding, "TryBeginBuildingDrag", target), Is.True);
            Call(playerUi, "SetWindow", Enum.Parse(playerUi.GetType().Assembly.GetType("Not3A.Stage3.PlayerInterface.Stage3PlayerWindow"), "Inventory"));
            yield return null;
            Assert.That(Read<bool>(binding, "IsDraggingBuilding"), Is.False); Assert.That(Stock(), Is.EqualTo(stock));
        }

        [UnityTest]
        public IEnumerator ShiftKeepsPaidTowerDraftForRepeatedClicksAndOrdinaryClickEndsIt()
        {
            game.RunDriver.enabled=false;game.RunDriver.ResetRun();yield return null;
            var inventory=game.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood,100);inventory.TryAdd(MapResourceKind.Stone,100);
            yield return Tap(keyboard.bKey);
            Assert.That(Call(playerUi,"ActivateBuildChoice",0),Is.True);
            var defense=game.Building.Defense;
            var cost=defense.SelectedCatalog.BuildCost;
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;
            var safe=new Vector2(Screen.width*.55f,Screen.height*.65f);
            var previousWood=inventory.Get(MapResourceKind.Wood);
            foreach(var shift in new[]{keyboard.leftShiftKey,keyboard.rightShiftKey})
            {
                var cell=FindLegalCell();AlignWorldPoint(camera,scene.MapDefinition.CellToWorld(cell),safe);
                input.Press(shift);yield return Click(safe);input.Release(shift);yield return null;
                Assert.That(defense.IsPlacing,Is.True,"Shift preserves the same paid blueprint.");
                Assert.That(inventory.Get(MapResourceKind.Wood),Is.EqualTo(previousWood-cost.Wood));
                previousWood=inventory.Get(MapResourceKind.Wood);
                var count=defense.TowerCount;var stock=Stock();
                // Existing-building selection wins over placement, even while Shift is held.
                // Test the occupied placement rule directly without initiating selection.
                defense.SetPreviewCell(cell);
                Assert.That(defense.TryConfirm(out _),Is.EqualTo(PlacementResult.Occupied));
                Assert.That(defense.TowerCount,Is.EqualTo(count));Assert.That(Stock(),Is.EqualTo(stock));
            }
            var last=FindLegalCell();AlignWorldPoint(camera,scene.MapDefinition.CellToWorld(last),safe);
            yield return Click(safe);
            Assert.That(defense.TowerCount,Is.EqualTo(3));Assert.That(defense.IsPlacing,Is.False);
            Assert.That(inventory.Get(MapResourceKind.Wood),Is.EqualTo(previousWood-cost.Wood));
            Assert.That(Call(playerUi,"ActivateBuildChoice",0),Is.True);
            input.Press(keyboard.leftShiftKey);yield return Tap(keyboard.escapeKey);input.Release(keyboard.leftShiftKey);
            Assert.That(defense.IsPlacing,Is.False);
        }

        [UnityTest]
        public IEnumerator TabReturnsToPlayerFromConstructionAndPagesWithoutSpending()
        {
            game.RunDriver.enabled=false;var stock=Stock();
            foreach(var key in new[]{keyboard.bKey,keyboard.iKey,keyboard.oKey,keyboard.pKey,keyboard.mKey})
            {
                yield return Tap(key);Assert.That(Window,Is.Not.EqualTo("None"));
                yield return Tap(keyboard.tabKey);Assert.That(Window,Is.EqualTo("None"));
                Assert.That(game.Building.Defense.IsPlacing,Is.False);Assert.That(Stock(),Is.EqualTo(stock));
            }
        }

        [UnityTest]
        public IEnumerator OnlyPaidUpgradesCreateFeedbackAndPauseOrModalFreezeUntilReset()
        {
            game.RunDriver.enabled=false;game.SetPaused(false);var tower=BuildSelectionTower();yield return null;
            var binding=game.GetComponent("Stage3PlayerInterfaceBinding");
            var point=FrameBuilding(tower.GetComponentsInChildren<Renderer>().First(r=>r.enabled));
            Assert.That(Call(binding,"TrySelectBuildingAtScreen",point),Is.True);
            game.Gathering.Economy.Inventory.Reset();
            Assert.That(Call(binding,"TryUpgradeSelectedBuilding").ToString(),Is.EqualTo("CannotAfford"));
            Assert.That(Read<int>(playerUi,"BuildingUpgradeEffectCount"),Is.Zero);
            game.Building.Defense.GetTowerCatalog(tower).TryGetUpgrade(tower.Level,out var next);
            game.Gathering.Economy.Inventory.TryCredit(next.Cost);
            Assert.That(Call(binding,"TryUpgradeSelectedBuilding").ToString(),Is.EqualTo("Ready"));
            Assert.That(Read<int>(playerUi,"BuildingUpgradeEffectCount"),Is.EqualTo(1));
            game.SetPaused(true);var elapsed=Read<float>(playerUi,"BuildingUpgradeEffectElapsed");
            yield return null;yield return null;
            Assert.That(Read<float>(playerUi,"BuildingUpgradeEffectElapsed"),Is.EqualTo(elapsed));
            game.SetPaused(false);yield return Tap(keyboard.iKey);
            elapsed=Read<float>(playerUi,"BuildingUpgradeEffectElapsed");yield return null;yield return null;
            Assert.That(Read<float>(playerUi,"BuildingUpgradeEffectElapsed"),Is.EqualTo(elapsed));
            yield return Tap(keyboard.tabKey);
            Assert.That(Read<float>(playerUi,"BuildingUpgradeEffectElapsed"),Is.GreaterThan(elapsed));
            game.RunDriver.ResetRun();yield return null;
            Assert.That(Read<int>(playerUi,"BuildingUpgradeEffectCount"),Is.Zero);
        }

        [UnityTest]
        public IEnumerator AltClickPaysOneUpgradeWithoutSelectingAndFailuresSpendNothing()
        {
            game.RunDriver.enabled=false;game.RunDriver.ResetRun();yield return null;
            var tower=BuildSelectionTower();yield return null;
            var binding=game.GetComponent("Stage3PlayerInterfaceBinding");
            Call(binding,"ClearBuildingSelection");
            var point=FrameBuilding(tower.GetComponentsInChildren<Renderer>().First(r=>r.enabled));
            var defense=game.Building.Defense;var inventory=game.Gathering.Economy.Inventory;
            foreach(var alt in new[]{keyboard.leftAltKey,keyboard.rightAltKey})
            {
                var level=tower.Level;var cost=defense.GetTowerCatalog(tower).TryGetUpgrade(level,out var next)?next.Cost:default;
                input.Press(alt);yield return null;
                Assert.That(Call(binding,"EvaluateQuickUpgrade",tower).ToString(),Is.EqualTo("Ready"));
                var wood=inventory.Get(MapResourceKind.Wood);yield return Click(point);
                Assert.That(tower.Level,Is.EqualTo(level+1));
                Assert.That(inventory.Get(MapResourceKind.Wood),Is.EqualTo(wood-cost.Wood));
                Assert.That(Read<object>(binding,"SelectedBuildingKind").ToString(),Is.EqualTo("None"));
                Assert.That(Read<bool>(playerUi,"HasSelectedBuilding"),Is.False);
                input.Release(alt);yield return null;
            }
            inventory.Reset();var stock=Stock();var previousLevel=tower.Level;
            input.Press(keyboard.leftAltKey);yield return null;
            Assert.That(Call(binding,"EvaluateQuickUpgrade",tower).ToString(),Is.EqualTo("CannotAfford"));
            yield return Click(point);
            Assert.That(tower.Level,Is.EqualTo(previousLevel));Assert.That(Stock(),Is.EqualTo(stock));
            Assert.That(Read<bool>(playerUi,"HasSelectedBuilding"),Is.False);
            game.SetPaused(true);yield return null;
            Assert.That(Call(binding,"TryQuickUpgradeAtScreen",point).ToString(),Is.EqualTo("Inactive"));
            Assert.That(Stock(),Is.EqualTo(stock));game.SetPaused(false);input.Release(keyboard.leftAltKey);yield return null;
            yield return Click(point);Assert.That(Read<bool>(playerUi,"HasSelectedBuilding"),Is.True,
                "A normal click still opens the existing card.");
        }

        private Stage2Tower BuildSelectionTower()
        {
            var inventory = game.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood, 100); inventory.TryAdd(MapResourceKind.Stone, 100);
            inventory.TryAdd(MapResourceKind.Metal, 100); inventory.TryAddGold(100);
            var defense = game.Building.Defense;
            defense.SelectRole(0); Assert.That(defense.BeginPlacement(), Is.True);
            var cell = FindLegalCell();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            return tower;
        }

        private Vector2 FrameBuilding(Renderer renderer)
        {
            var camera = scene.CameraFollow.BoundCamera;
            scene.CameraFollow.enabled = false;
            camera.orthographicSize = 5f;
            var point = new Vector2(Screen.width * .55f, Screen.height * .64f);
            var center = renderer.bounds.center;
            camera.transform.position = center - camera.transform.forward * 30f;
            var projected = camera.WorldToScreenPoint(center);
            camera.transform.position += center - camera.ScreenToWorldPoint(new Vector3(point.x, point.y, projected.z));
            return point;
        }

        private IEnumerator Click(Vector2 point)
        {
            input.Release(mouse.leftButton); input.Set(mouse.position, point); yield return null;
            input.Press(mouse.leftButton); yield return null; yield return null;
            input.Release(mouse.leftButton); yield return null; yield return null;
        }

        private string SelectedStat(string label)
        {
            var view = Read<object>(playerUi, "SelectedBuildingView");
            var stats = (Array)view.GetType().GetField("Stats").GetValue(view);
            foreach (var stat in stats)
                if ((string)stat.GetType().GetField("Label").GetValue(stat) == label)
                    return (string)stat.GetType().GetField("Value").GetValue(stat);
            Assert.Fail("Missing selected stat " + label); return null;
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
