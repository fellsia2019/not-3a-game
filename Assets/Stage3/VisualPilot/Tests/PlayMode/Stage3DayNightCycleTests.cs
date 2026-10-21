#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Economy;
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
    public sealed class Stage3DayNightCycleTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component home;
        private T Get<T>(string name) => (T)home.GetType().GetProperty(name).GetValue(home);
        private object Call(string name, params object[] args) => home.GetType().GetMethod(name).Invoke(home, args);
        private string Evaluate(Vector2 point) => Call("EvaluateAt", point).ToString();

        [UnitySetUp] public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            home = game.GetComponent("Stage3Dwelling"); Assert.That(home, Is.Not.Null);
            for (var i = 0; i < 8 && !Get<bool>("IsReady"); i++) yield return null;
            Assert.That(Get<bool>("IsReady"), Is.True);
            Assert.That(game.RunDriver.Run.ContinuousDays, Is.True);
            Assert.That(game.Building.Defense.TowerCount, Is.Zero, "Fresh survival start must not grant showcase towers.");
            Assert.That(Get<bool>("IsBuilt"), Is.False);
            Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            game.HeroCombat.AutomaticTick = false;
            game.RunDriver.enabled = false; game.RunDriver.ResetRun(); game.SetPaused(false);
            yield return null;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale = 1f;
        }

        private Vector2 FindLegalSite()
        {
            var origin = (Vector2)scene.PlayerMover.transform.position;
            for (var ring = 5; ring < 35; ring += 2)
                for (var y = -ring; y <= ring; y += 2) for (var x = -ring; x <= ring; x += 2)
                {
                    var point = origin + new Vector2(x, y);
                    if (Evaluate(point) == "Ready") return point;
                }
            Assert.Fail("Actual world must contain a legal nearby dwelling footprint and entrance."); return default;
        }

        private Vector2 BuildHome()
        {
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood, 10);
            Call("BeginPlacement"); var point = FindLegalSite();
            Assert.That(Call("TryBuildAt", point).ToString(), Is.EqualTo("Built"));
            return Get<Vector2>("Position");
        }

        private void ClearNight()
        {
            var run = game.RunDriver.Run; Assert.That(run.IsNight, Is.True);
            run.Tick(run.NightDuration); Assert.That(run.SpawningFinished, Is.True);
            for (var i = game.Enemies.ActiveCount - 1; i >= 0; i--)
            {
                var enemy = game.Enemies.GetEnemy(i); enemy.SetAutomaticTick(false); enemy.TakeDamage(enemy.Health);
            }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.IsNightTime, Is.False, "A streaming night can only finish at/after dawn.");
        }

        [UnityTest] public IEnumerator DayIsTenActiveMinutesAndSpaceCannotStartNightRemotely()
        {
            var run = game.RunDriver.Run;
            Assert.That(run.PreparationRemaining, Is.EqualTo(600f));
            run.SetPaused(true); run.Tick(600f); Assert.That(run.IsDay, Is.True);
            Assert.That(run.PreparationRemaining, Is.EqualTo(600f));
            game.RunDriver.enabled = true; run.SetPaused(false);
            input.Press(Keyboard.current.spaceKey); yield return null; yield return null;
            Assert.That(run.IsDay, Is.True);
            input.Release(Keyboard.current.spaceKey); game.RunDriver.enabled = false; game.SetPaused(false);
            run.Tick(run.PreparationRemaining - 1f); Assert.That(run.IsDay, Is.True);
            run.Tick(1.001f); Assert.That(run.IsNight, Is.True);
            Assert.That(game.Enemies.ActiveCount, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest] public IEnumerator HomePurchaseIsAtomicRejectsInvalidGroundAndProtectsOneFootprint()
        {
            var inventory = game.Gathering.Economy.Inventory;
            Call("BeginPlacement"); Assert.That(Get<bool>("IsPlacing"), Is.False);
            inventory.TryAdd(MapResourceKind.Wood, 10); Call("BeginPlacement");
            Assert.That(Evaluate(scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)), Is.EqualTo("InvalidGround"));
            Assert.That(Evaluate(scene.PlayerMover.transform.position), Is.EqualTo("HeroOverlap"));
            Assert.That(Call("TryBuildAt", new Vector2(float.NaN, 0)).ToString(), Is.EqualTo("InvalidGround"));
            var position = FindLegalSite(); var before = scene.PlayerMover.Obstacles.Rectangles.Count;
            Assert.That(Call("TryBuildAt", position).ToString(), Is.EqualTo("Built"));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(scene.PlayerMover.Obstacles.Rectangles.Count, Is.EqualTo(before + 1));
            Assert.That(scene.PlayerMover.Obstacles.IsBlocked(Get<Vector2>("Position"), scene.PlayerMover.CollisionRadius), Is.True);
            Assert.That(scene.PlayerMover.Obstacles.IsBlocked(Get<Vector2>("InteractionPosition"), scene.PlayerMover.CollisionRadius), Is.False);
            Assert.That(Call("TryBuildAt", position).ToString(), Is.EqualTo("NotPlacing"));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(scene.PlayerMover.Obstacles.Rectangles.Count, Is.EqualTo(before + 1));
            yield return null;
        }

        [UnityTest] public IEnumerator ActualGatheringPaysForHomeWithoutMaterialOrGoldGrants()
        {
            var inventory = game.Gathering.Economy.Inventory;
            scene.PlayerMover.transform.position = game.Gathering.Economy.WorldPosition(MapResourceKind.Wood);
            var hud = game.GetComponent<Stage2GameHud>(); hud.SetGameSpeed(10);
            input.Release(Keyboard.current.eKey); yield return null;
            input.Press(Keyboard.current.eKey);
            var deadline = Time.realtimeSinceStartup + 22f;
            while (inventory.Get(MapResourceKind.Wood) < 10 && Time.realtimeSinceStartup < deadline) yield return null;
            input.Release(Keyboard.current.eKey); hud.SetGameSpeed(1); yield return null;
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.GreaterThanOrEqualTo(10));
            Assert.That(inventory.Gold, Is.EqualTo(5));
            var before = inventory.Get(MapResourceKind.Wood);
            Call("BeginPlacement"); var point = FindLegalSite();
            Assert.That(Call("TryBuildAt", point).ToString(), Is.EqualTo("Built"));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.EqualTo(before - 10));
        }

        [UnityTest] public IEnumerator MouseUiHeldClicksAndSameFrameCancelDoNotSpendOrBuildTwice()
        {
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood, 10);
            Call("BeginPlacement"); var point = FindLegalSite();
            var camera = scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled = false;
            camera.transform.position = (Vector3)point - camera.transform.forward * 30f;
            var hud = game.GetComponent<Stage2GameHud>(); var panel = hud.ResourcesRect.center * hud.Scale;
            input.Set(Mouse.current.position, new Vector2(panel.x, Screen.height - panel.y));
            input.Release(Mouse.current.leftButton); yield return null;
            input.Press(Mouse.current.leftButton); yield return null;
            Assert.That(Get<bool>("IsBuilt"), Is.False);
            input.Set(Mouse.current.position, (Vector2)camera.WorldToScreenPoint(point)); yield return null;
            Assert.That(Get<bool>("IsBuilt"), Is.False, "Dragging a held UI click onto the world cannot build.");
            input.Release(Mouse.current.leftButton); yield return null;
            input.Press(Mouse.current.leftButton); input.Press(Mouse.current.rightButton); yield return null;
            Assert.That(Get<bool>("IsBuilt"), Is.False); Assert.That(Get<bool>("IsPlacing"), Is.False);
            Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(10));
            input.Release(Mouse.current.leftButton); input.Release(Mouse.current.rightButton); yield return null;
            Call("BeginPlacement"); yield return null;
            input.Press(Mouse.current.leftButton); yield return null; yield return null;
            Assert.That(Get<bool>("IsBuilt"), Is.True);
            Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            yield return null; yield return null;
            Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            input.Release(Mouse.current.leftButton);
        }

        [UnityTest] public IEnumerator HomeSleepStopsBeforeNightWithoutSkippedProductionOrRecovery()
        {
            BuildHome();
            var production = game.Production;
            foreach (MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind))) game.Gathering.Economy.Inventory.TryAdd(kind, 100);
            var point = production.Rules.Points[0];
            scene.PlayerMover.transform.position = Get<Vector2>("InteractionPosition");
            production.SelectAndBeginPlacement(point.Building);
            Assert.That(production.TryBuildAt(0), Is.EqualTo(ProductionPlacementResult.Built));
            production.enabled = false; production.Rules.Tick(1f);
            var building = production.Rules.Buildings[0];
            var progress = building.ProgressSeconds;
            var inventory = game.Gathering.Economy.Inventory;
            var node = game.Gathering.Economy.GetNode(MapResourceKind.Wood);
            for (var i = node.Remaining; i > 0; i--)
                game.Gathering.Economy.AdvanceGather(MapResourceKind.Wood,
                    game.Gathering.Economy.WorldPosition(MapResourceKind.Wood), true, node.GatherSeconds, true);
            var recovery = node.RecoveryRemaining;
            Assert.That(recovery, Is.GreaterThan(0f));
            var resources = new[] { inventory.Get(MapResourceKind.Wood), inventory.Get(MapResourceKind.Stone), inventory.Get(MapResourceKind.Metal) };
            scene.PlayerMover.transform.position += Vector3.right * 10f;
            Assert.That((bool)Call("TrySkipDay"), Is.False, "Rest requires the real doorway.");
            scene.PlayerMover.transform.position = Get<Vector2>("InteractionPosition");
            var time = game.RunDriver.Run.WorldMinutes;
            Assert.That((bool)Call("TrySkipDay"), Is.True);
            Assert.That(game.RunDriver.Run.WorldMinutes, Is.GreaterThan(time));
            Assert.That(game.RunDriver.Run.ClockHour, Is.EqualTo(19));
            Assert.That(game.RunDriver.Run.ClockMinute, Is.EqualTo(50));
            Assert.That(game.RunDriver.Run.IsDay, Is.True);
            Assert.That(building.ProgressSeconds, Is.EqualTo(progress));
            Assert.That(node.RecoveryRemaining, Is.EqualTo(recovery));
            Assert.That(new[] { inventory.Get(MapResourceKind.Wood), inventory.Get(MapResourceKind.Stone), inventory.Get(MapResourceKind.Metal) }, Is.EqualTo(resources));
            Assert.That((bool)Call("TrySkipDay"), Is.False);
            Assert.That(game.Enemies.ActiveCount, Is.Zero);
            yield return null;
        }

        [UnityTest] public IEnumerator HeldHomeSleepCannotStartNightAndLaterNightsContinueGrowing()
        {
            BuildHome(); scene.PlayerMover.transform.position = Get<Vector2>("InteractionPosition");
            input.Release(Keyboard.current.eKey); yield return null;
            input.Press(Keyboard.current.eKey); yield return null; yield return null;
            Assert.That(game.RunDriver.Run.IsDay, Is.True);
            Assert.That(game.RunDriver.Run.ClockHour, Is.EqualTo(19));
            Assert.That(game.RunDriver.Run.ClockMinute, Is.EqualTo(50));
            Assert.That((bool)Call("TrySkipDay"), Is.False);
            Stage3TestCycle.StartNight(game.RunDriver.Run); ClearNight();
            input.Release(Keyboard.current.eKey); yield return null;
            var tiers = new int[4];
            for (var night = 0; night < tiers.Length; night++)
            {
                Assert.That(Stage3TestCycle.StartNight(game.RunDriver.Run), Is.True);
                tiers[night] = game.Enemies.GetEnemy(0).ThreatLevel; ClearNight();
                Assert.That(Get<bool>("IsBuilt"), Is.True);
            }
            Assert.That(tiers, Is.EqualTo(new[] { 2, 3, 4, 5 }), "Timed nights increase difficulty, not a finite spawn quota.");
            Assert.That(game.RunDriver.Run.WaveNumber, Is.EqualTo(6));
            Assert.That(game.RunDriver.Run.IsTerminal, Is.False);
        }

        [UnityTest] public IEnumerator DawnDuringUnfinishedHordeChangesLightingButDoesNotAllowRestOrOverlap()
        {
            BuildHome();
            scene.PlayerMover.transform.position = Get<Vector2>("InteractionPosition");
            yield return null;
            Assert.That(Get<bool>("NearEntrance"), Is.True);
            var run = game.RunDriver.Run;
            var lighting = game.GetComponent("Stage3DayNightPresentation");
            Assert.That(lighting, Is.Not.Null);
            Assert.That(Stage3TestCycle.StartNight(run), Is.True);
            for (var i = 0; i < game.Enemies.ActiveCount; i++) game.Enemies.GetEnemy(i).SetAutomaticTick(false);
            yield return new WaitForSeconds(.65f);
            Assert.That((float)lighting.GetType().GetProperty("NightAmount").GetValue(lighting), Is.GreaterThan(.95f));
            run.Tick(run.NightDuration);
            for (var i = 0; i < game.Enemies.ActiveCount; i++) game.Enemies.GetEnemy(i).SetAutomaticTick(false);
            Assert.That(run.ClockHour, Is.EqualTo(8));
            Assert.That(run.IsNightTime, Is.False);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(Get<bool>("CanSkipDay"), Is.False, "Daylight must not permit rest during a still-active horde.");
            input.Press(Keyboard.current.eKey);
            yield return new WaitForSeconds(.65f);
            Assert.That((float)lighting.GetType().GetProperty("NightAmount").GetValue(lighting), Is.LessThan(.05f));
            Assert.That(Stage3TestCycle.StartNight(run), Is.False);
            Assert.That(run.WaveNumber, Is.EqualTo(1));
            Assert.That(game.Enemies.ActiveCount, Is.EqualTo(run.WaveEnemyCount));
            for (var i = game.Enemies.ActiveCount - 1; i >= 0; i--)
            { var enemy = game.Enemies.GetEnemy(i); enemy.TakeDamage(enemy.Health); }
            yield return null; yield return null;
            Assert.That(run.IsDay, Is.True, "Held E from combat must not skip the newly earned preparation.");
            // StartNight crosses the clock boundary by1ms; continuous time keeps
            // that remainder rather than rounding the world's clock backwards.
            Assert.That(run.PreparationRemaining, Is.EqualTo(600f).Within(.01f));
            input.Release(Keyboard.current.eKey);
        }

        [UnityTest] public IEnumerator ConstructionModesPauseDebugResetAndRegenerationDoNotLeakHomeState()
        {
            var inventory = game.Gathering.Economy.Inventory; inventory.TryAdd(MapResourceKind.Wood, 20);
            Call("BeginPlacement"); Assert.That(Get<bool>("IsPlacing"), Is.True);
            var travel = game.GetComponent("Stage3WorldTravel");
            travel.GetType().GetMethod("BeginWorkshopPlacement").Invoke(travel, null); yield return null;
            Assert.That(Get<bool>("IsPlacing"), Is.False);
            Call("BeginPlacement"); Assert.That((bool)travel.GetType().GetProperty("IsPlacingWorkshop").GetValue(travel), Is.False);
            game.SetPaused(true); yield return null; Assert.That(Get<bool>("IsPlacing"), Is.False);
            game.SetPaused(false); Call("BeginPlacement");
            game.GetComponent<Stage2GameHud>().SetDebugOpen(true); yield return null;
            Assert.That(Get<bool>("IsPlacing"), Is.False);
            game.GetComponent<Stage2GameHud>().SetDebugOpen(false);
            Call("BeginPlacement"); var site = FindLegalSite(); Assert.That(Call("TryBuildAt", site).ToString(), Is.EqualTo("Built"));
            game.RunDriver.ResetRun(); game.SetPaused(false);
            Assert.That(Get<bool>("IsBuilt"), Is.False);
            Assert.That(scene.PlayerMover.Obstacles.IsBlocked(site, scene.PlayerMover.CollisionRadius), Is.False);
            BuildHome(); var old = scene.PlayerMover.Obstacles; var oldPosition = Get<Vector2>("Position");
            var world = game.GetComponent("Stage3ResourceDistrictWorld");
            world.GetType().GetMethod("Regenerate").Invoke(world, new object[] { 164033 }); yield return null; yield return null;
            Assert.That(Get<bool>("IsBuilt"), Is.False);
            Assert.That(scene.PlayerMover.Obstacles, Is.Not.SameAs(old));
            Assert.That(old.IsBlocked(oldPosition, scene.PlayerMover.CollisionRadius), Is.False);
            Assert.That((bool)Call("TrySkipDay"), Is.False);
        }
    }
}
#endif
