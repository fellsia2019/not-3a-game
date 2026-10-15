#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
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
    public sealed class Stage3MinimapTests
    {
        private InputTestFixture input;
        private Mouse mouse;
        private Stage2SceneBootstrap bootstrap;
        private Stage2GameComposition game;
        private Stage2Minimap minimap;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            input = new InputTestFixture();
            input.Setup();
            InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            bootstrap = game.GetComponent<Stage2SceneBootstrap>();
            minimap = game.GetComponent<Stage2Minimap>();
            Assert.That(minimap, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown();
        }

        [UnityTest]
        public IEnumerator TerrainMatchesAuthoredRoadAndMarkersFollowLiveRun()
        {
            var map = bootstrap.MapDefinition;
            for (var y = 0; y < map.Size.y; y++)
            for (var x = 0; x < map.Size.x; x++)
            {
                var road = map.GetSurface(map.MinimumCell + new Vector2Int(x, y)) == MapSurfaceKind.InvasionCorridor;
                var sample = minimap.Terrain.GetPixel((x * 2 + 1) * minimap.Terrain.width / (map.Size.x * 2),
                    (y * 2 + 1) * minimap.Terrain.height / (map.Size.y * 2));
                Assert.That(sample.r > .45f, Is.EqualTo(road), $"Road mismatch at {x},{y}");
            }
            var entrance = minimap.WorldToMapPoint(bootstrap.EntranceMarker.position);
            var throne = minimap.WorldToMapPoint(bootstrap.ThroneMarker.position);
            Assert.That(entrance.x, Is.LessThan(throne.x));
            Assert.That(entrance.y, Is.GreaterThan(throne.y));
            Assert.That(Count(Stage2MinimapMarkerKind.Tower), Is.EqualTo(game.Building.Defense.TowerCount));
            bootstrap.PlayerMover.transform.position = new Vector3(4f, 18f, 0f);
            var enemy = game.Enemies.Spawn();
            enemy.SetAutomaticTick(false);
            enemy.Tick(1f);
            yield return null;
            Assert.That(minimap.Markers.Single(m => m.Kind == Stage2MinimapMarkerKind.Hero).World,
                Is.EqualTo((Vector2)bootstrap.PlayerMover.transform.position));
            Assert.That(minimap.Markers.Single(m => m.Kind == Stage2MinimapMarkerKind.Enemy).World,
                Is.EqualTo((Vector2)enemy.transform.position));
            enemy.TakeDamage(int.MaxValue);
            var tower = game.Building.Defense.GetTower(0);
            tower.TakeDamage(int.MaxValue);
            var economy = game.Gathering.Economy;
            var wood = economy.GetNode(MapResourceKind.Wood);
            for (var i = 0; i < wood.Capacity; i++)
                Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                    economy.WorldPosition(MapResourceKind.Wood), true, wood.GatherSeconds, true), Is.True);
            yield return null;
            Assert.That(Count(Stage2MinimapMarkerKind.Enemy), Is.Zero);
            Assert.That(Count(Stage2MinimapMarkerKind.Tower), Is.EqualTo(game.Building.Defense.TowerCount));
            Assert.That(Count(Stage2MinimapMarkerKind.Wood), Is.Zero);
            economy.Tick(wood.RecoverySeconds, true);
            yield return null;
            Assert.That(Count(Stage2MinimapMarkerKind.Wood), Is.EqualTo(1));
            game.RunDriver.ResetRun();
            yield return null;
            Assert.That(Count(Stage2MinimapMarkerKind.Tower), Is.Zero);
            Assert.That(Count(Stage2MinimapMarkerKind.Enemy), Is.Zero);
            Assert.That(minimap.Markers.Count, Is.EqualTo(6));
            Assert.That(Count(Stage2MinimapMarkerKind.Portal), Is.EqualTo(1));
            Assert.That(Count(Stage2MinimapMarkerKind.Throne), Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Stage2Minimap>().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PanelAnchorsAndCameraFootprintTracksZoomAtMapEdge()
        {
            foreach (var size in new[] { new Vector2(640, 360), new Vector2(1280, 720), new Vector2(1920, 1080), new Vector2(800, 1000) })
            {
                var panel = Stage2Minimap.Layout(size);
                Assert.That(panel.xMin, Is.EqualTo(16f).Within(.1f));
                Assert.That(panel.yMax, Is.EqualTo(size.y - 16f).Within(.1f));
                Assert.That(panel.xMax, Is.LessThan(size.x));
                Assert.That(panel.yMin, Is.GreaterThanOrEqualTo(0));
            }
            bootstrap.PlayerMover.transform.position = new Vector3(-48f, -48f, 0f);
            bootstrap.CameraFollow.SnapToTarget();
            var corners = new Vector2[4];
            Assert.That(minimap.GetCameraFootprint(corners), Is.True);
            var wide = Vector2.Distance(corners[0], corners[1]);
            bootstrap.CameraFollow.ZoomByPinchScale(2f);
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(minimap.GetCameraFootprint(corners), Is.True);
            Assert.That(Vector2.Distance(corners[0], corners[1]), Is.LessThan(wide * .52f));
            foreach (var corner in corners)
            {
                Assert.That(corner.x, Is.InRange(minimap.MapRect.xMin - .1f, minimap.MapRect.xMax + .1f));
                Assert.That(corner.y, Is.InRange(minimap.MapRect.yMin - .1f, minimap.MapRect.yMax + .1f));
            }
            game.SetPaused(true);
            yield return null;
            Assert.That(minimap.isActiveAndEnabled, Is.True);
            Assert.That(minimap.Markers.Any(m => m.Kind == Stage2MinimapMarkerKind.Hero), Is.True);
            game.SetPaused(false);
        }

        [UnityTest]
        public IEnumerator PanelBlocksWorldPurchaseAndZoomButOutsideClickStillBuilds()
        {
            game.RunDriver.ResetRun();
            yield return null;
            var camera = bootstrap.CameraFollow.BoundCamera;
            var gui = minimap.PanelRect.center;
            var screen = new Vector2(gui.x, Screen.height - gui.y);
            input.Set(mouse.position, screen);
            var startingSize = camera.orthographicSize;
            input.Set(mouse.scroll, new Vector2(0f, 120f));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(camera.orthographicSize, Is.EqualTo(startingSize).Within(.01f));
            bootstrap.CameraFollow.enabled = false;
            var cell = new Vector2Int(39, -7);
            var ray = camera.ScreenPointToRay(screen);
            Assert.That(new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance), Is.True);
            camera.transform.position += (Vector3)(bootstrap.MapDefinition.CellToWorld(cell) - (Vector2)ray.GetPoint(distance));
            Assert.That(game.Building.TryScreenToCell(screen, out var behind), Is.True);
            Assert.That(behind, Is.EqualTo(cell), "The world cell behind the panel must actually be buildable.");
            var defense = game.Building.Defense;
            var inventory = game.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood, 2);
            inventory.TryAdd(MapResourceKind.Stone, 1);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(cell);
            Assert.That(defense.EvaluatePreview(), Is.EqualTo(PlacementResult.Ready));
            input.Press(mouse.leftButton);
            yield return null;
            input.Release(mouse.leftButton);
            Assert.That(game.Building.PointerOverUi, Is.True);
            Assert.That(game.Building.PreviewRenderer.gameObject.activeSelf, Is.False);
            Assert.That(GameObject.Find("Tactical mesh placement preview"), Is.Null);
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(inventory.Gold, Is.EqualTo(5));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(2));
            var nextCell = cell + Vector2Int.up * 10;
            var nextScreen = (Vector2)camera.WorldToScreenPoint(bootstrap.MapDefinition.CellToWorld(nextCell));
            Assert.That(minimap.ContainsScreenPoint(nextScreen), Is.False);
            input.Set(mouse.position, nextScreen);
            yield return null;
            Assert.That(game.Building.PreviewResult, Is.EqualTo(PlacementResult.Ready));
            input.Press(mouse.leftButton);
            yield return null;
            input.Release(mouse.leftButton);
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(defense.GetTower(0).Cell, Is.EqualTo(nextCell));
            Assert.That(inventory.Gold, Is.Zero);
        }

        private int Count(Stage2MinimapMarkerKind kind) => minimap.Markers.Count(m => m.Kind == kind);
    }
}
#endif
