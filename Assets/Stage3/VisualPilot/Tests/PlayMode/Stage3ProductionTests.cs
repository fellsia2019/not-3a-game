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
    public sealed class Stage3ProductionTests
    {
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2ProductionDriver production;
        private InputTestFixture input;
        private Component world;
        private static string Evidence => "Artifacts/S3-RESOURCE-PRODUCTION-20261016/game";
        [UnitySetUp] public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            world = game.GetComponent("Stage3ResourceDistrictWorld");
            world.GetType().GetMethod("Regenerate").Invoke(world, new object[] { 164032 });
            production = game.Production; Assert.That(production, Is.Not.Null);
            var inventory = game.Gathering.Economy.Inventory;
            foreach (MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind))) inventory.TryAdd(kind, 100);
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale = 1;
        }
        [UnityTest] public IEnumerator BuildAllThreeAtLockedPointsWithActualArtAndCapture()
        {
            Directory.CreateDirectory(Evidence);
            var camera = scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled = false;
            camera.orthographicSize = 5;
            foreach (var kind in new[] { ResourceProductionBuilding.Sawmill, ResourceProductionBuilding.Quarry, ResourceProductionBuilding.IronMine })
            {
                var index = Enumerable.Range(0, production.Rules.Points.Count).First(i => production.Rules.Points[i].Building == kind);
                var point = production.Rules.Points[index];
                var heroPoint = point.Position + Vector2.down * 1.2f;
                if (scene.PlayerMover.Obstacles.IsBlocked(heroPoint, scene.PlayerMover.CollisionRadius))
                    scene.PlayerMover.Obstacles.TryFindFreePosition(heroPoint, scene.PlayerMover.CollisionRadius,
                        new Rect(point.Position - Vector2.one * 4, Vector2.one * 8), out heroPoint);
                scene.PlayerMover.transform.position = heroPoint;
                Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(kind)); production.SelectAndBeginPlacement(kind);
                Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.Built));
                Assert.That(scene.PlayerMover.Obstacles.IsBlocked(point.Position, scene.PlayerMover.CollisionRadius), Is.True);
                Assert.That(scene.PlayerMover.Obstacles.IsBlocked(heroPoint, scene.PlayerMover.CollisionRadius), Is.False);
                camera.transform.position = new Vector3(point.Position.x, point.Position.y + .5f, -.3f) - camera.transform.forward * 30;
                yield return null; yield return null;
                Save(camera, Path.Combine(Evidence, kind + "-built.png"));
                if (!Application.isBatchMode) ScreenCapture.CaptureScreenshot(Path.Combine(Evidence, kind + "-hud.png"));
                yield return null;
            }
            Assert.That(production.Rules.Buildings.Count, Is.EqualTo(3));
            var renderers = production.GetComponentsInChildren<SpriteRenderer>();
            Assert.That(renderers.Count(r => r.name.StartsWith("Production ") && r.gameObject.activeSelf), Is.EqualTo(3));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ActualPlacementProtectsHeroTypeFundsAndOccupiedPoint()
        {
            var index = Enumerable.Range(0, production.Rules.Points.Count).First(i => production.Rules.Points[i].Building == ResourceProductionBuilding.Sawmill);
            var point = production.Rules.Points[index]; var inventory = game.Gathering.Economy.Inventory;
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(ResourceProductionBuilding.Quarry)); production.SelectAndBeginPlacement(ResourceProductionBuilding.Quarry);
            Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.WrongType));
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(ResourceProductionBuilding.Sawmill)); production.SelectAndBeginPlacement(ResourceProductionBuilding.Sawmill); scene.PlayerMover.transform.position = point.Position;
            Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.HeroBlocked));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(100));
            scene.PlayerMover.transform.position = new Vector3(10, 10, 0);
            Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.Built));
            var spent = inventory.Get(MapResourceKind.Wood);
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(ResourceProductionBuilding.Sawmill)); production.SelectAndBeginPlacement(ResourceProductionBuilding.Sawmill);
            Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.Occupied));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(spent));
            yield return null;
        }
        [UnityTest] public IEnumerator RunPauseTerminalResetAndRegenerationOwnProductionLifecycle()
        {
            var index = 0; var point = production.Rules.Points[index];
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(point.Building)); production.SelectAndBeginPlacement(point.Building);
            Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.Built));
            var inventory = game.Gathering.Economy.Inventory; var before = inventory.Get(point.Resource);
            production.Rules.Tick(25); Assert.That(inventory.Get(point.Resource), Is.GreaterThan(before));
            game.SetPaused(true); before = inventory.Get(point.Resource); production.Rules.Tick(100);
            Assert.That(inventory.Get(point.Resource), Is.EqualTo(before));
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(point.Building)); production.SelectAndBeginPlacement(point.Building); Assert.That(production.IsPlacing, Is.False);
            game.SetPaused(false);
            game.RunDriver.Run.ResetRun();
            Assert.That(production.Rules.Buildings.Count, Is.Zero);
            Assert.That(scene.PlayerMover.Obstacles.IsBlocked(point.Position, scene.PlayerMover.CollisionRadius), Is.False);
            foreach (MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind))) inventory.TryAdd(kind, 100);
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(point.Building)); production.SelectAndBeginPlacement(point.Building); Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.Built));
            var old = scene.PlayerMover.Obstacles;
            world.GetType().GetMethod("Regenerate").Invoke(world, new object[] { 164033 });
            Assert.That(production.Rules.Buildings.Count, Is.Zero); Assert.That(production.IsPlacing, Is.False);
            Assert.That(scene.PlayerMover.Obstacles, Is.Not.SameAs(old));
            Assert.That(old.IsBlocked(point.Position, scene.PlayerMover.CollisionRadius), Is.False);
            var replacement = production.Rules.Points[0];
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(replacement.Building)); production.SelectAndBeginPlacement(replacement.Building);
            Assert.That(production.TryBuildAt(0), Is.EqualTo(ProductionPlacementResult.Built));
            var run = game.RunDriver.Run; Stage3TestCycle.StartNight(run);
            var method = run.GetType().GetMethod("OnReachedThrone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(run, new object[] { run.ThroneMaximum });
            Assert.That(run.IsTerminal, Is.True); before = inventory.Get(point.Resource); production.Rules.Tick(100);
            Assert.That(inventory.Get(point.Resource), Is.EqualTo(before));
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(point.Building)); production.SelectAndBeginPlacement(point.Building); Assert.That(production.IsPlacing, Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator KeyboardTowerSelectionCancelsGeneratorAndNeitherModeLeaksGathering()
        {
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(ResourceProductionBuilding.Sawmill)); production.SelectAndBeginPlacement(ResourceProductionBuilding.Sawmill); Assert.That(production.IsPlacing, Is.True);
            var playerUi = game.GetComponent("Stage3FantasyHud"); Assert.That(playerUi, Is.Not.Null);
            Assert.That(playerUi.GetType().GetProperty("ActiveWindow").GetValue(playerUi).ToString(), Is.EqualTo("Construction"));
            input.Press(Keyboard.current.bKey); yield return null;
            Assert.That(playerUi.GetType().GetProperty("ActiveWindow").GetValue(playerUi).ToString(), Is.EqualTo("None"));
            Assert.That(production.IsPlacing, Is.False);
            Assert.That(game.Building.Defense.IsPlacing, Is.False, "Closing the construction ribbon cancels placement.");
            input.Release(Keyboard.current.bKey); yield return null;
            input.Press(Keyboard.current.bKey); yield return null;
            Assert.That(playerUi.GetType().GetProperty("ActiveWindow").GetValue(playerUi).ToString(), Is.EqualTo("Construction"));
            Assert.That(game.Building.Defense.IsPlacing, Is.False, "B opens the construction ribbon without selecting a tower.");
            playerUi.GetType().GetMethod("SetBuildGroup").Invoke(playerUi, new object[] { 0 });
            Assert.That((bool)playerUi.GetType().GetMethod("ActivateBuildChoice").Invoke(playerUi, new object[] { 0 }), Is.True);
            Assert.That(game.Building.Defense.IsPlacing, Is.True, "Selecting the actual tower card begins placement.");
            input.Release(Keyboard.current.bKey); Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(ResourceProductionBuilding.Quarry)); production.SelectAndBeginPlacement(ResourceProductionBuilding.Quarry);
            Assert.That(game.Building.Defense.IsPlacing, Is.False);
            input.Press(Keyboard.current.digit1Key); yield return null; Assert.That(production.IsPlacing, Is.False);
            input.Release(Keyboard.current.digit1Key); Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(ResourceProductionBuilding.Sawmill)); production.SelectAndBeginPlacement(ResourceProductionBuilding.Sawmill);
            var kind = MapResourceKind.Wood; scene.PlayerMover.transform.position = game.Gathering.Economy.WorldPosition(kind);
            var before = game.Gathering.Economy.Inventory.Get(kind); input.Press(Keyboard.current.eKey);
            yield return new WaitForSeconds(game.Gathering.Catalog.Get(kind).GatherSeconds + .1f);
            Assert.That(game.Gathering.Economy.Inventory.Get(kind), Is.EqualTo(before));
            input.Release(Keyboard.current.eKey); production.CancelPlacement(); yield return null;
            input.Press(Keyboard.current.eKey); yield return new WaitForSeconds(game.Gathering.Catalog.Get(kind).GatherSeconds + .1f);
            Assert.That(game.Gathering.Economy.Inventory.Get(kind), Is.GreaterThan(before));
            input.Release(Keyboard.current.eKey);
        }
        [UnityTest] public IEnumerator RealWorldMouseClickBuildsOnceAndCancelWinsWithoutSpending()
        {
            var point = production.Rules.Points[0]; var camera = scene.CameraFollow.BoundCamera;
            scene.CameraFollow.enabled = false; camera.orthographicSize = 5;
            camera.transform.position = (Vector3)point.Position - camera.transform.forward * 30;
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(point.Building)); production.SelectAndBeginPlacement(point.Building);
            input.Set(Mouse.current.position, (Vector2)camera.WorldToScreenPoint(point.Position + Vector2.right * 4));
            input.Press(Mouse.current.leftButton); yield return null; input.Release(Mouse.current.leftButton);
            Assert.That(production.Rules.Buildings.Count, Is.Zero);
            var inventory = game.Gathering.Economy.Inventory; var before = inventory.Get(MapResourceKind.Wood);
            yield return null;
            input.Set(Mouse.current.position, (Vector2)camera.WorldToScreenPoint(point.Position));
            input.Press(Mouse.current.leftButton); input.Press(Mouse.current.rightButton); yield return null;
            input.Release(Mouse.current.leftButton); input.Release(Mouse.current.rightButton);
            Assert.That(production.Rules.Buildings.Count, Is.Zero); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(before));
            Assert.That(production.IsPlacing, Is.False, "Right-click cancellation must win over a simultaneous purchase click.");
            yield return null;
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(point.Building)); production.SelectAndBeginPlacement(point.Building); yield return null;
            input.Press(Mouse.current.leftButton); yield return null;
            input.Release(Mouse.current.leftButton); Assert.That(production.Rules.Buildings.Count, Is.EqualTo(1),
                $"result={production.PreviewResult} placing={production.IsPlacing} index={production.PreviewPointIndex} ui={production.PointerOverUi} mouse={Mouse.current.position.ReadValue()} point={camera.WorldToScreenPoint(point.Position)} screen={Screen.width}x{Screen.height}");
            var spent = inventory.Get(MapResourceKind.Wood); yield return null;
            input.Press(Mouse.current.leftButton); yield return null; input.Release(Mouse.current.leftButton);
            Assert.That(production.Rules.Buildings.Count, Is.EqualTo(1)); Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(spent));
        }

        private static void Save(Camera camera, string path)
        {
            var target = new RenderTexture(1280, 900, 24); var old = camera.targetTexture; var active = RenderTexture.active;
            var texture = new Texture2D(1280, 900, TextureFormat.RGB24, false);
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, 1280, 900), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
            camera.targetTexture = old; RenderTexture.active = active; Object.Destroy(target); Object.Destroy(texture);
        }
    }
}
#endif
