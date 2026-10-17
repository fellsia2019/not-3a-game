#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
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
    public sealed class Stage3WorldTravelTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component travel, world;
        private Type type;
        private Camera camera;
        private static string Evidence => Environment.GetEnvironmentVariable("NOT3A_TRAVEL_EVIDENCE") ?? "Artifacts/S3-WORKSHOP-BUILD-20261017/game";
        private bool Flag(string name) => (bool)type.GetProperty(name).GetValue(travel);
        private Vector2 Point(string name) => (Vector2)type.GetProperty(name).GetValue(travel);
        private HeroObstacleMap Obstacles => scene.PlayerMover.Obstacles;
        private Stage2Minimap Minimap => game.GetComponent<Stage2Minimap>();
        private bool TryInteract() => (bool)type.GetMethod("TryInteract").Invoke(travel, null);
        private string EvaluateWorkshop(Vector2 point) => type.GetMethod("EvaluateWorkshopAt").Invoke(travel, new object[] { point }).ToString();

        [UnitySetUp] public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            travel = game.GetComponent("Stage3WorldTravel"); world = game.GetComponent("Stage3ResourceDistrictWorld");
            Assert.That(travel, Is.Not.Null); type = travel.GetType();
            Assert.That(Flag("IsReady"), Is.True);
            camera = scene.CameraFollow.BoundCamera; game.RunDriver.ResetRun();
            Directory.CreateDirectory(Evidence); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale = 1f;
        }
        private IEnumerator Press()
        {
            input.Release(Keyboard.current.eKey); yield return null;
            input.Press(Keyboard.current.eKey); yield return null; yield return null;
        }
        private IEnumerator Study()
        {
            if (!Flag("WorkshopBuilt")) BuildWorkshop();
            scene.PlayerMover.transform.position = Point("WorkshopInteractionPosition");
            yield return Press(); Assert.That(Flag("IsResearched"), Is.True);
            input.Release(Keyboard.current.eKey); yield return null;
        }
        private void BuildWorkshop()
        {
            scene.PlayerMover.transform.position = Point("BasePosition");
            var hud = game.GetComponent<Stage2GameHud>(); hud.SelectWorkshop();
            Assert.That(hud.BuildCategory, Is.EqualTo(Stage2BuildCategory.Research));
            Assert.That(type.GetMethod("TryBuildWorkshopAt").Invoke(travel, new object[] { Point("WorkshopPosition") }).ToString(), Is.EqualTo("Built"));
            Assert.That(Flag("WorkshopBuilt"), Is.True);
        }

        [UnityTest] public IEnumerator BuildMenuMousePlacementIsFreeSingleAndMatchesPreviewBeforeResearch()
        {
            var hud = game.GetComponent<Stage2GameHud>(); var stock = Stock(); var site = Point("WorkshopPosition");
            Assert.That(hud.CanSelectWorkshop, Is.True, "The approved prototype costs no resources.");
            Assert.That(hud.ResearchTabRect.xMax, Is.LessThanOrEqualTo(hud.BuildRect.xMax));
            Assert.That(hud.GeneratorsTabRect.xMax, Is.LessThan(hud.ResearchTabRect.xMin));
            scene.CameraFollow.enabled = false; camera.orthographicSize = 5f;
            camera.transform.position = (Vector3)site - camera.transform.forward * 30f;
            hud.SelectWorkshop(); input.Set(Mouse.current.position, (Vector2)camera.WorldToScreenPoint(site));
            input.Release(Mouse.current.leftButton); yield return null; yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.True); Assert.That(EvaluateWorkshop(site), Is.EqualTo("Ready"));
            var preview = GameObject.Find("Research workshop preview"); Assert.That(preview, Is.Not.Null);
            var registration = preview.transform.position;
            Assert.That(preview.GetComponent<SpriteRenderer>().color.g, Is.GreaterThan(preview.GetComponent<SpriteRenderer>().color.r));
            input.Press(Mouse.current.leftButton); yield return null; yield return null;
            Assert.That(Flag("WorkshopBuilt"), Is.True); Assert.That(Flag("IsPlacingWorkshop"), Is.False);
            Assert.That(Flag("IsResearched"), Is.False); Assert.That(Flag("BaseVisible"), Is.False);
            Assert.That(Vector3.Distance(GameObject.Find("Research workshop").transform.position, registration), Is.LessThan(.001f));
            Assert.That(Stock(), Is.EqualTo(stock)); Assert.That(hud.CanSelectWorkshop, Is.False);
            yield return null; hud.SelectWorkshop(); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False);
            Assert.That(Obstacles.Rectangles.Count(r => r.Contains(site)), Is.EqualTo(1));
            Assert.That(Minimap.Markers.Count(m => m.Kind == Stage2MinimapMarkerKind.Research), Is.EqualTo(1));
            input.Release(Mouse.current.leftButton); yield return Study();
            Assert.That(Flag("BaseVisible"), Is.True);
        }

        [UnityTest] public IEnumerator PlacementRejectsRoadHeroResourcesCirclesObstaclesAndMenuClicksAndCancellationWins()
        {
            var hud = game.GetComponent<Stage2GameHud>(); var site = Point("WorkshopPosition"); var stock = Stock();
            hud.SelectWorkshop(); Assert.That(EvaluateWorkshop(site), Is.EqualTo("Ready"));
            scene.PlayerMover.transform.position = site;
            Assert.That(EvaluateWorkshop(site), Is.EqualTo("HeroOverlap"));
            scene.PlayerMover.transform.position = Point("BasePosition");
            foreach (var invalid in new[] { (Vector2)scene.ThroneMarker.position, (Vector2)scene.EntranceMarker.position,
                Point("BasePosition"), Point("RemotePosition"), game.Production.Rules.Points[0].Position, new Vector2(-9999, -9999) })
                Assert.That(EvaluateWorkshop(invalid), Is.Not.EqualTo("Ready"));
            var obstruction = new Rect(site - Vector2.one * .1f, Vector2.one * .2f);
            Obstacles.AddRectangle(obstruction); Assert.That(EvaluateWorkshop(site), Is.EqualTo("Occupied"));
            Assert.That(Obstacles.RemoveRectangle(obstruction), Is.True);
            scene.CameraFollow.enabled = false;
            var gui = hud.BuildChoiceRect(0).center * hud.Scale;
            var screen = new Vector2(gui.x, Screen.height - gui.y);
            var ray = camera.ScreenPointToRay(screen);
            Assert.That(new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance), Is.True);
            camera.transform.position += (Vector3)site - ray.GetPoint(distance);
            input.Set(Mouse.current.position, screen); yield return null;
            input.Press(Mouse.current.leftButton); yield return null; input.Release(Mouse.current.leftButton); yield return null;
            Assert.That(Flag("WorkshopBuilt"), Is.False, "A HUD click cannot construct on the ground beneath it.");
            camera.transform.position = (Vector3)site - camera.transform.forward * 30f;
            input.Set(Mouse.current.position, (Vector2)camera.WorldToScreenPoint(site)); yield return null;
            Assert.That(EvaluateWorkshop(site), Is.EqualTo("Ready"));
            input.Press(Keyboard.current.escapeKey); input.Press(Mouse.current.leftButton); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(Flag("WorkshopBuilt"), Is.False);
            Assert.That(GameObject.Find("Research workshop preview"), Is.Null); Assert.That(Stock(), Is.EqualTo(stock));
            input.Release(Keyboard.current.escapeKey); input.Release(Mouse.current.leftButton);
            hud.SelectWorkshop(); yield return null; input.Press(Mouse.current.rightButton); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); input.Release(Mouse.current.rightButton);
        }

        [UnityTest] public IEnumerator WorkshopPlacementYieldsToOtherModesAndCannotSurvivePauseDebugTerminalOrReset()
        {
            var hud = game.GetComponent<Stage2GameHud>(); hud.AddAllResources(100); hud.AddGold(100);
            hud.SelectWorkshop(); hud.SelectGenerator(ResourceProductionBuilding.Sawmill);
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(game.Production.IsPlacing, Is.True);
            hud.SelectWorkshop(); input.Press(Keyboard.current.digit1Key); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(game.Building.Defense.SelectedRoleIndex, Is.Zero);
            input.Release(Keyboard.current.digit1Key); hud.SelectWorkshop(); hud.SelectTower(0);
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(game.Building.Defense.IsPlacing, Is.True);
            hud.SelectWorkshop(); hud.SetDebugOpen(true); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); hud.SelectWorkshop(); Assert.That(Flag("IsPlacingWorkshop"), Is.False);
            hud.SetDebugOpen(false); hud.SelectWorkshop(); game.SetPaused(true); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(hud.CanSelectWorkshop, Is.False);
            game.SetPaused(false); hud.SelectWorkshop(); input.Press(Mouse.current.leftButton); hud.RestartRun(); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(Flag("WorkshopBuilt"), Is.False);
            input.Release(Mouse.current.leftButton); hud.SelectWorkshop();
            var run = game.RunDriver.Run; run.StartWave();
            run.GetType().GetMethod("OnReachedThrone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(run, new object[] { run.ThroneMaximum }); yield return null;
            Assert.That(Flag("IsPlacingWorkshop"), Is.False); Assert.That(hud.CanSelectWorkshop, Is.False);
            Assert.That(type.GetMethod("TryBuildWorkshopAt").Invoke(travel, new object[] { Point("WorkshopPosition") }).ToString(), Is.EqualTo("NotPlacing"));
        }

        [UnityTest] public IEnumerator ResearchCreatesBaseCircleAndResetRemovesItWithoutSpendingOrGranting()
        {
            Assert.That(Flag("IsResearched"), Is.False); Assert.That(Flag("BaseVisible"), Is.False);
            Assert.That(Flag("WorkshopBuilt"), Is.False); Assert.That(GameObject.Find("Research workshop"), Is.Null);
            scene.PlayerMover.transform.position = Point("WorkshopInteractionPosition");
            Assert.That(TryInteract(), Is.False, "Research requires an actually constructed workshop.");
            Assert.That(GameObject.Find("Base teleport circle"), Is.Null);
            Assert.That(GameObject.Find("World teleport circle"), Is.Not.Null);
            var stock = Stock(); yield return Study();
            Assert.That(Flag("BaseVisible"), Is.True); Assert.That(Flag("RemoteDiscovered"), Is.False);
            Assert.That(Stock(), Is.EqualTo(stock)); Assert.That(TryInteract(), Is.False, "Study is an idempotent one-time unlock.");
            var baseCircle = GameObject.Find("Base teleport circle"); var remote = GameObject.Find("World teleport circle");
            var a = baseCircle.GetComponent<MeshRenderer>(); var b = remote.GetComponent<MeshRenderer>();
            Assert.That(a.sharedMaterial, Is.SameAs(b.sharedMaterial));
            Assert.That(a.sharedMaterial.mainTexture.name, Is.EqualTo("TeleportSeal"));
            Assert.That(a.sharedMaterial.shader.name, Is.EqualTo("Not3A/Ground Teleport Seal"));
            Assert.That(baseCircle.GetComponentsInChildren<Collider>().Length, Is.Zero);
            var bounds = baseCircle.GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.That(bounds.size.x, Is.EqualTo(bounds.size.y).Within(.001f));
            Assert.That(bounds.size.z, Is.LessThan(.10f), "The seal is a ground marking, not an upright billboard.");
            Assert.That(baseCircle.transform.rotation, Is.EqualTo(Quaternion.identity));
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(Flag("BaseVisible"), Is.False); Assert.That(Flag("IsResearched"), Is.False);
            Assert.That(Flag("RemoteDiscovered"), Is.False);
            Assert.That(Flag("WorkshopBuilt"), Is.False); Assert.That(GameObject.Find("Research workshop"), Is.Null);
            Assert.That(Minimap.Markers.Count(m => m.Kind == Stage2MinimapMarkerKind.Research), Is.Zero);
            Assert.That(Minimap.Markers.Count(m => m.Kind == Stage2MinimapMarkerKind.Teleport), Is.Zero);
        }

        [UnityTest] public IEnumerator DiscoveryAndKeyboardTravelWorkBothWaysWithoutBounceOrBlockedArrival()
        {
            var stock = Stock(); yield return Study();
            scene.PlayerMover.transform.position = Point("BasePosition"); yield return Press();
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("BasePosition")), "Unknown destinations are not accessible.");
            scene.PlayerMover.transform.position = Point("RemotePosition");
            input.Release(Keyboard.current.eKey); yield return null; yield return null;
            Assert.That(Flag("RemoteDiscovered"), Is.True);
            var obstruction = new Rect(Point("BasePosition") - Vector2.one * .5f, Vector2.one);
            Obstacles.AddRectangle(obstruction); yield return Press();
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("RemotePosition")), "Blocked destinations must reject travel.");
            Assert.That(Obstacles.RemoveRectangle(obstruction), Is.True);
            yield return Press();
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("BasePosition")));
            yield return null; yield return null; yield return null;
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("BasePosition")), "Held E cannot immediately return.");
            yield return Press();
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("RemotePosition")));
            Assert.That(Obstacles.IsBlocked(scene.PlayerMover.transform.position, scene.PlayerMover.CollisionRadius), Is.False);
            Assert.That(Stock(), Is.EqualTo(stock));
            Assert.That(Minimap.Markers.Count(m => m.Kind == Stage2MinimapMarkerKind.Teleport), Is.EqualTo(2));
        }

        [UnityTest] public IEnumerator PauseBuildAndDebugBlockResearchAndHeldInputDoesNotReplay()
        {
            BuildWorkshop();
            scene.PlayerMover.transform.position = Point("WorkshopInteractionPosition");
            game.RunDriver.SetPaused(true); yield return Press(); Assert.That(Flag("IsResearched"), Is.False);
            game.RunDriver.SetPaused(false); yield return null; yield return null;
            Assert.That(Flag("IsResearched"), Is.False);
            var inventory = game.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood, 10); inventory.TryAdd(MapResourceKind.Stone, 10); inventory.TryAddGold(10);
            game.Building.SelectAndBeginPlacement(0); yield return Press(); Assert.That(TryInteract(), Is.False);
            game.Building.CancelPlacement(); yield return null;
            Assert.That(Flag("IsResearched"), Is.False);
            game.Production.SelectAndBeginPlacement(ResourceProductionBuilding.Sawmill);
            Assert.That(game.Production.IsPlacing, Is.True); yield return Press();
            Assert.That(Flag("IsResearched"), Is.False); Assert.That(TryInteract(), Is.False);
            game.Production.CancelPlacement(); yield return null;
            Assert.That(Flag("IsResearched"), Is.False);
            var hud = game.GetComponent<Stage2GameHud>(); hud.SetDebugOpen(true);
            yield return Press(); Assert.That(Flag("IsResearched"), Is.False); Assert.That(TryInteract(), Is.False);
            hud.SetDebugOpen(false); yield return null; Assert.That(Flag("IsResearched"), Is.False);
            yield return Press(); Assert.That(Flag("IsResearched"), Is.True);
            game.RunDriver.ResetRun(); yield return null; yield return null;
            Assert.That(Flag("IsResearched"), Is.False, "Held research input must not replay across reset.");
        }

        [UnityTest] public IEnumerator DiscoveryBeforeResearchPersistsThroughStudyAndWaveButTerminalBlocksTravel()
        {
            scene.PlayerMover.transform.position = Point("RemotePosition"); yield return null; yield return null;
            Assert.That(Flag("RemoteDiscovered"), Is.True); Assert.That(Flag("BaseVisible"), Is.False);
            Assert.That(TryInteract(), Is.False);
            Assert.That(game.RunDriver.Run.StartWave(), Is.True); yield return Study();
            Assert.That(Flag("RemoteDiscovered"), Is.True);
            scene.PlayerMover.transform.position = Point("BasePosition"); yield return Press();
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("RemotePosition")));
            var run = game.RunDriver.Run;
            run.GetType().GetMethod("OnReachedThrone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(run, new object[] { run.ThroneMaximum });
            Assert.That(run.IsTerminal, Is.True); yield return Press();
            Assert.That(TryInteract(), Is.False);
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(Point("RemotePosition")));
            Assert.That(string.IsNullOrEmpty(game.GetComponent<Stage2GameHud>().WorldInteractionHint), Is.True);
        }

        [UnityTest] public IEnumerator WorldRegenerationRebindsOneWorkshopObstacleAndKeepsArrivalSitesClear()
        {
            foreach (var seed in new[] { 164032, 164033, 164034 })
            {
                world.GetType().GetMethod("Regenerate").Invoke(world, new object[] { seed }); yield return null;
                Assert.That(Flag("IsResearched"), Is.False); Assert.That(Flag("BaseVisible"), Is.False);
                Assert.That(Flag("WorkshopBuilt"), Is.False);
                Assert.That(Obstacles.Rectangles.Count(r => r.Contains(Point("WorkshopPosition"))), Is.Zero);
                BuildWorkshop();
                Assert.That(Obstacles.Rectangles.Count(r => r.Contains(Point("WorkshopPosition"))), Is.EqualTo(1));
                foreach (var site in new[] { Point("BasePosition"), Point("RemotePosition"), Point("WorkshopInteractionPosition") })
                {
                    Assert.That(Obstacles.IsBlocked(site, scene.PlayerMover.CollisionRadius), Is.False);
                    Assert.That(scene.MapDefinition.GetSurface(scene.MapDefinition.WorldToCell(site)), Is.EqualTo(MapSurfaceKind.Forest));
                }
                scene.PlayerMover.transform.position = Point("WorkshopInteractionPosition");
                scene.PlayerMover.MoveBy(Vector2.up * 3f);
                Assert.That(scene.PlayerMover.transform.position.y, Is.LessThan(Point("WorkshopPosition").y));
                Assert.That(Obstacles.IsBlocked(scene.PlayerMover.transform.position, scene.PlayerMover.CollisionRadius), Is.False);
                Assert.That(Object.FindObjectsByType<MonoBehaviour>().Count(m => m.GetType() == type), Is.EqualTo(1));
            }
        }

        [UnityTest] public IEnumerator ApprovedSealRendersAtThreeZoomsWithTransparentSourceBackground()
        {
            BuildWorkshop();
            scene.CameraFollow.enabled = false; camera.orthographicSize = 5f; camera.aspect = 1280f / 900f;
            camera.transform.position = (Vector3)((Point("WorkshopPosition") + Point("BasePosition")) * .5f + Vector2.up) - camera.transform.forward * 30f;
            scene.PlayerMover.transform.position = Point("WorkshopInteractionPosition"); yield return null;
            Render("Before-research-normal.png"); yield return Study();
            Render("After-research-normal.png");
            var circle = GameObject.Find("Base teleport circle").GetComponent<MeshRenderer>();
            scene.PlayerMover.transform.position = Point("WorkshopInteractionPosition");
            foreach (var zoom in new[] { 5f, 2.5f, 10f })
            {
                camera.orthographicSize = zoom;
                camera.transform.position = (Vector3)Point("BasePosition") - camera.transform.forward * 30f;
                circle.enabled = true; var on = Render("Teleport-seal-zoom-" + zoom + ".png");
                circle.enabled = false; var off = Render(null); circle.enabled = true;
                var changed = on.Zip(off, (a, b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b)).Count(d => d > .08f);
                Assert.That(changed, Is.GreaterThan(zoom == 10f ? 40 : 180), "Approved linework must actually render at gameplay zoom.");
                foreach (var direction in new[] { new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(1, 1) })
                {
                    var p = Point("BasePosition") + direction * 1.55f;
                    var pixel = camera.WorldToViewportPoint(new Vector3(p.x, p.y, -.2f));
                    var x = Mathf.Clamp(Mathf.RoundToInt(pixel.x * 1280), 0, 1279); var y = Mathf.Clamp(Mathf.RoundToInt(pixel.y * 900), 0, 899);
                    var a = on[y * 1280 + x]; var b = off[y * 1280 + x];
                    Assert.That(Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b), Is.LessThan(.03f),
                        "The concept's opaque square background must not appear on the ground.");
                }
                File.AppendAllText(Path.Combine(Evidence, "render-contrast.txt"), "zoom=" + zoom + " changedPixels=" + changed + "\n");
            }
        }

        private int[] Stock()
        {
            var inventory = game.Gathering.Economy.Inventory;
            return new[] { inventory.Gold, inventory.Get(MapResourceKind.Wood), inventory.Get(MapResourceKind.Stone), inventory.Get(MapResourceKind.Metal) };
        }
        private Color[] Render(string name)
        {
            var old = camera.targetTexture; var active = RenderTexture.active; var aspect = camera.aspect;
            var target = new RenderTexture(1280, 900, 24); var image = new Texture2D(1280, 900, TextureFormat.RGB24, false);
            try
            {
                camera.aspect = 1280f / 900f; camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 900), 0, 0); image.Apply();
                if (name != null) File.WriteAllBytes(Path.Combine(Evidence, name), image.EncodeToPNG());
                return image.GetPixels();
            }
            finally { camera.targetTexture = old; camera.aspect = aspect; RenderTexture.active = active; Object.Destroy(target); Object.Destroy(image); }
        }
    }
}
#endif
