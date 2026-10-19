#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3DefeatRewindTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component home, travel, effect;
        private int captureRate;
        private static T Read<T>(object owner, string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner, string name, params object[] values) => owner.GetType().GetMethod(name).Invoke(owner, values);
        private Stage2Run Run => game.RunDriver.Run;
        private Stage2Economy Economy => game.Gathering.Economy;
        private Stage2Inventory Inventory => Economy.Inventory;
        private int[] Stock() => new[] { Inventory.Get(MapResourceKind.Wood), Inventory.Get(MapResourceKind.Stone), Inventory.Get(MapResourceKind.Metal), Inventory.Gold };

        [UnitySetUp] public IEnumerator Load()
        {
            captureRate = Time.captureFramerate; Time.captureFramerate = 60; Time.timeScale = 1f;
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 12; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            home = game.GetComponent("Stage3Dwelling"); travel = game.GetComponent("Stage3WorldTravel");
            effect = game.GetComponent("Stage3DefeatPresentation"); Assert.That(effect, Is.Not.Null);
            Assert.That(Read<bool>(home, "IsReady"), Is.True); Assert.That(Read<bool>(travel, "IsReady"), Is.True);
            game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal }) Inventory.TryAdd(kind, 300);
            Inventory.TryAddGold(300); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.captureFramerate = captureRate; Time.timeScale = 1f;
        }

        private Stage2Tower BuildTower(int role)
        {
            var defense = game.Building.Defense;
            defense.SelectRole(role); defense.BeginPlacement();
            for (var y = scene.MapDefinition.MinimumCell.y; y <= scene.MapDefinition.MaximumCell.y; y++)
            for (var x = scene.MapDefinition.MinimumCell.x; x <= scene.MapDefinition.MaximumCell.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (defense.Evaluate(cell) != PlacementResult.Ready) continue;
                defense.SetPreviewCell(cell);
                Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready)); return tower;
            }
            Assert.Fail("No real legal defense cell."); return null;
        }
        private void BuildProduction()
        {
            var production = game.Production;
            for (var i = 0; i < production.Rules.Points.Count; i++)
            {
                if (production.Rules.IsOccupied(i)) continue;
                var kind = (ResourceProductionBuilding)(int)production.Rules.Points[i].Resource;
                production.SelectAndBeginPlacement(kind);
                if (production.TryBuildAt(i) == ProductionPlacementResult.Built) return;
            }
            Assert.Fail("No real legal production pad.");
        }
        private void BuildHome()
        {
            Call(home, "BeginPlacement"); var origin = (Vector2)scene.PlayerMover.transform.position;
            for (var radius = 5; radius < 45; radius += 2)
            for (var y = -radius; y <= radius; y += 2) for (var x = -radius; x <= radius; x += 2)
            {
                var point = origin + new Vector2(x, y);
                if (Call(home, "EvaluateAt", point).ToString() != "Ready") continue;
                Assert.That(Call(home, "TryBuildAt", point).ToString(), Is.EqualTo("Built")); return;
            }
            Assert.Fail("No real legal dwelling site.");
        }
        private IEnumerator BuildAndStudyWorkshop()
        {
            game.GetComponent<Stage2GameHud>().SelectWorkshop();
            Assert.That(Call(travel, "TryBuildWorkshopAt", Read<Vector2>(travel, "WorkshopPosition")).ToString(), Is.EqualTo("Built"));
            scene.PlayerMover.transform.position = Read<Vector2>(travel, "WorkshopInteractionPosition");
            yield return null; Assert.That(Call(travel, "TryInteract"), Is.True);
            scene.PlayerMover.transform.position = Read<Vector2>(travel, "RemotePosition");
            yield return null; yield return null;
            Assert.That(Read<bool>(travel, "RemoteDiscovered"), Is.True);
        }
        private void ArriveOne()
        {
            var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne);
            enemy.SetAutomaticTick(false); enemy.Tick(.01f);
        }
        private void Lose()
        {
            for (var i = 0; i < 20 && !Run.IsTerminal; i++) ArriveOne();
            Assert.That(Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
        }
        private IEnumerator WaitForRetry()
        {
            var limit = Time.realtimeSinceStartup + 8f;
            while (!game.RunDriver.CanRetry && Time.realtimeSinceStartup < limit) yield return null;
            Assert.That(game.RunDriver.CanRetry, Is.True, "Unscaled defeat transition must finish while the game is stopped.");
        }

        [UnityTest] public IEnumerator RetryUndoesPaidUpgradesMergingAndSaleIncludingInvestmentLedger()
        {
            var defense = game.Building.Defense; var a = BuildTower(0); var b = BuildTower(0);
            Assert.That(defense.TryUpgrade(a), Is.EqualTo(Stage2TowerActionResult.Ready)); a.TakeDamage(3);
            var health = a.Health; var cells = new[] {a.Cell,b.Cell}; var invested = a.Invested;
            var inventory = Stock(); Run.StartWave();
            Assert.That(defense.TryUpgrade(b), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(defense.TryMerge(a,b), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(defense.TrySell(b), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(defense.TowerCount, Is.Zero);
            Lose(); yield return WaitForRetry(); Assert.That(game.RunDriver.RetryFailedWave(), Is.True);
            Assert.That(Stock(), Is.EqualTo(inventory)); Assert.That(defense.TowerCount, Is.EqualTo(2));
            var restored = defense.GetTower(0);
            Assert.That(restored.Level, Is.EqualTo(2)); Assert.That(restored.Health, Is.EqualTo(health));
            Assert.That(restored.Cell, Is.EqualTo(cells[0])); Assert.That(restored.Invested.Wood, Is.EqualTo(invested.Wood));
            Assert.That(restored.Invested.Stone, Is.EqualTo(invested.Stone)); Assert.That(defense.GetTower(1).Level, Is.EqualTo(1));
            var refund = defense.GetSaleRefund(restored); var before = Inventory.Get(MapResourceKind.Wood);
            Assert.That(defense.TrySell(restored), Is.EqualTo(Stage2TowerActionResult.Ready));
            Assert.That(Inventory.Get(MapResourceKind.Wood), Is.EqualTo(before + refund.Wood));
            Assert.That(game.Navigation.TryGetOccupant(cells[0],out _), Is.False);
        }

        [UnityTest] public IEnumerator RetryRestoresAllWorldOwnersAndTheirBindingsWithoutKeepingTheFailedFuture()
        {
            var towers = Enumerable.Range(0, game.Building.Defense.RoleCount).Select(BuildTower).ToArray();
            foreach (var tower in towers) tower.TakeDamage(1);
            var wood = Economy.Nodes.First(n => n.Resource == MapResourceKind.Wood && n.GroupId >= 0);
            Economy.AdvanceGather(wood, wood.WorldPosition, true, wood.GatherSeconds, true);
            var depleted = Economy.Nodes.First(n => n.Resource == MapResourceKind.Metal && n.Size == Stage2ResourceSize.Medium);
            while (!depleted.IsDepleted) Economy.AdvanceGather(depleted, depleted.WorldPosition, true, depleted.GatherSeconds, true);
            Economy.CancelGather(); Economy.Tick(3f, true);
            BuildProduction(); game.Production.Rules.Tick(.4f);
            yield return BuildAndStudyWorkshop(); BuildHome();
            scene.PlayerMover.transform.position = Read<Vector2>(home, "InteractionPosition");
            yield return null; yield return null;
            var hero = scene.PlayerMover.transform.position; var inventory = Stock();
            var remains = Economy.Nodes.Select(n => n.Remaining).ToArray();
            var recovery = Economy.Nodes.Select(n => n.RecoveryRemaining).ToArray();
            var health = towers.Select(t => t.Health).ToArray(); var cells = towers.Select(t => t.Cell).ToArray();
            var generation = game.Navigation.PlanFrom(scene.MapDefinition.EnemyEntrance);
            var produced = game.Production.Rules.Buildings.Select(b => b.ProgressSeconds).ToArray();
            var homePosition = Read<Vector2>(home, "Position"); var workshopPosition = Read<Vector2>(travel, "WorkshopPosition");
            var rectangles = scene.PlayerMover.Obstacles.Rectangles.Count;
            Assert.That(Call(home, "TrySkipDay"), Is.True);
            foreach (var tower in towers) tower.TakeDamage(tower.Health);
            BuildTower(0); BuildProduction();
            Economy.Tick(100f, true); game.Production.Rules.Tick(100f); Inventory.TryAddGold(71);
            game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            scene.PlayerMover.transform.position = scene.ThroneMarker.position + Vector3.right * 8f;
            Lose(); Assert.That(game.RunDriver.CanRetry, Is.False);
            var frozenHero = scene.PlayerMover.transform.position; var frozenStock = Stock(); var frozenRecovery = depleted.RecoveryRemaining;
            input.Press(Keyboard.current.wKey); input.Press(Keyboard.current.eKey); input.Press(Keyboard.current.rKey);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(Run.IsTerminal, Is.True); Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(frozenHero));
            Assert.That(Stock(), Is.EqualTo(frozenStock)); Assert.That(depleted.RecoveryRemaining, Is.EqualTo(frozenRecovery));
            input.Release(Keyboard.current.wKey); input.Release(Keyboard.current.rKey);
            yield return WaitForRetry(); Assert.That(game.RunDriver.RetryFailedWave(), Is.True);
            Assert.That(Run.IsDay, Is.True); Assert.That(Run.WaveNumber, Is.EqualTo(1));
            Assert.That(Run.PreparationRemaining, Is.EqualTo(Run.PreparationDuration)); Assert.That(Run.ClockHour, Is.EqualTo(8));
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(hero)); Assert.That(Stock(), Is.EqualTo(inventory));
            Assert.That(Economy.Nodes.Select(n => n.Remaining), Is.EqualTo(remains));
            Assert.That(Economy.Nodes.Select(n => n.RecoveryRemaining), Is.EqualTo(recovery));
            Assert.That(game.Building.Defense.TowerCount, Is.EqualTo(towers.Length));
            for (var i = 0; i < towers.Length; i++)
            {
                var tower = game.Building.Defense.GetTower(i);
                Assert.That(tower.Health, Is.EqualTo(health[i])); Assert.That(tower.Cell, Is.EqualTo(cells[i]));
                Assert.That(game.Navigation.TryGetOccupant(cells[i], out var occupant), Is.True); Assert.That(occupant, Is.SameAs(tower));
            }
            Assert.That(game.Navigation.IsCurrent(generation), Is.False);
            Assert.That(game.Production.Rules.Buildings.Select(b => b.ProgressSeconds), Is.EqualTo(produced));
            Assert.That(Read<bool>(home, "IsBuilt"), Is.True); Assert.That(Read<Vector2>(home, "Position"), Is.EqualTo(homePosition));
            Assert.That(Read<bool>(travel, "WorkshopBuilt"), Is.True); Assert.That(Read<bool>(travel, "IsResearched"), Is.True);
            Assert.That(Read<bool>(travel, "RemoteDiscovered"), Is.True); Assert.That(Read<Vector2>(travel, "WorkshopPosition"), Is.EqualTo(workshopPosition));
            Assert.That(scene.PlayerMover.Obstacles.Rectangles.Count, Is.EqualTo(rectangles));
            Assert.That(game.Enemies.ActiveCount, Is.Zero); Assert.That(game.Enemies.PendingShotCount, Is.Zero);
            var combat = game.GetComponent("Stage3CombatPresentation");
            Assert.That(Read<int>(combat, "ActiveFlights"), Is.Zero); Assert.That(Read<int>(combat, "ActiveImpacts"), Is.Zero);
            scene.PlayerMover.transform.position = wood.WorldPosition + Vector2.down * .45f;
            var before = Inventory.Get(MapResourceKind.Wood);
            for (var i = 0; i < 60; i++) yield return null;
            Assert.That(Inventory.Get(MapResourceKind.Wood), Is.EqualTo(before), "Held E cannot resume a failed-future interaction.");
            input.Release(Keyboard.current.eKey); yield return null;
            input.Press(Keyboard.current.eKey);
            for (var i = 0; i < 100 && Inventory.Get(MapResourceKind.Wood) == before; i++) yield return null;
            input.Release(Keyboard.current.eKey); Assert.That(Inventory.Get(MapResourceKind.Wood), Is.EqualTo(before + 1));
        }

        [UnityTest] public IEnumerator FutureBuildingsResearchDiscoveryAndProductionDisappearAndRepeatedLaterNightRetryWorks()
        {
            var stock = Stock(); var rectangles = scene.PlayerMover.Obstacles.Rectangles.Count;
            Run.Tick(Run.PreparationRemaining); Assert.That(Run.IsNight, Is.True);
            BuildProduction(); yield return BuildAndStudyWorkshop(); BuildHome();
            Lose(); yield return WaitForRetry(); Assert.That(game.RunDriver.RetryFailedWave(), Is.True);
            Assert.That(Read<bool>(home, "IsBuilt"), Is.False); Assert.That(Read<bool>(travel, "WorkshopBuilt"), Is.False);
            Assert.That(Read<bool>(travel, "IsResearched"), Is.False); Assert.That(Read<bool>(travel, "RemoteDiscovered"), Is.False);
            Assert.That(game.Production.Rules.Buildings.Count, Is.Zero); Assert.That(Stock(), Is.EqualTo(stock));
            Assert.That(scene.PlayerMover.Obstacles.Rectangles.Count, Is.EqualTo(rectangles));
            Run.StartWave(); Run.Tick(60f);
            while (game.Enemies.ActiveCount > 0) game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            Assert.That(Run.WaveNumber, Is.EqualTo(2)); Assert.That(Run.IsDay, Is.True);
            for (var attempt = 0; attempt < 2; attempt++)
            {
                Inventory.TrySpend(1, 0, 0); var saved = Stock();
                Run.Tick(100f); Run.StartWave(); Inventory.TryAddGold(100); Lose();
                yield return WaitForRetry();
                if (attempt == 0) Assert.That(game.RunDriver.RetryFailedWave(), Is.True);
                else
                {
                    input.Press(Keyboard.current.rKey);
                    for (var i = 0; i < 5 && Run.IsTerminal; i++) yield return null;
                    input.Release(Keyboard.current.rKey);
                    Assert.That(Run.IsDay, Is.True, "The real R binding must invoke retry after the white screen.");
                }
                Assert.That(Run.WaveNumber, Is.EqualTo(2)); Assert.That(Run.PreparationRemaining,
                    Is.EqualTo(Run.PreparationDuration).Within(.2f));
                Assert.That(Stock(), Is.EqualTo(saved)); Assert.That(Read<bool>(home, "IsBuilt"), Is.False);
            }
        }

        [UnityTest] public IEnumerator DamageAfterAStalledFrameStillPresentsAndLossDoesNotInheritTheStall()
        {
            // Exercise real wall-clock timing rather than the fixture's fixed capture timestep.
            Time.captureFramerate = 0;
            var camera = scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled = false;
            camera.orthographicSize = 5f;
            camera.transform.position = scene.ThroneMarker.position + Vector3.back * .55f - camera.transform.forward * 30f;
            yield return null; yield return null;
            Run.StartWave();
            var first = game.Enemies.SpawnAt(scene.MapDefinition.Throne); first.SetAutomaticTick(false);
            for (var i = 0; i < 8; i++) yield return null;
            // A slow frame BEFORE the actual arrival must not consume the new .65-second pulse.
            System.Threading.Thread.Sleep(800);
            first.Tick(.01f); yield return null;
            Assert.That(Read<int>(effect, "HitSequence"), Is.EqualTo(1));
            Assert.That(Read<int>(effect, "ActiveHitCount"), Is.EqualTo(1));
            Assert.That(Read<bool>(effect, "HasVisibleFractures"), Is.True);
            Assert.That(Read<Vector2>(effect, "ShakePixels").sqrMagnitude, Is.GreaterThan(0f));
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(Read<int>(effect, "ActiveHitCount"), Is.Zero, "The first sample must not leave persistent fractures.");

            while (Run.ThroneHealth > 1) ArriveOne();
            yield return new WaitForSecondsRealtime(.8f);
            var fatal = game.Enemies.SpawnAt(scene.MapDefinition.Throne); fatal.SetAutomaticTick(false);
            for (var i = 0; i < 8; i++) yield return null;
            System.Threading.Thread.Sleep(800);
            fatal.Tick(.01f); yield return null;
            Assert.That(Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(Read<float>(effect, "LossElapsed"), Is.LessThan(.5f),
                "The white transition starts at the fatal arrival, not at the beginning of its stalled frame.");
            Assert.That(game.RunDriver.CanRetry, Is.False);
        }

        [UnityTest] public IEnumerator HitFracturesExpireOffscreenHitsOnlyShakeAndTheRenderViewIsRestored()
        {
            var camera = scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled = false;
            camera.orthographicSize = 5f;
            camera.transform.position = scene.ThroneMarker.position + Vector3.back * .55f - camera.transform.forward * 30f;
            yield return null; yield return null;
            Run.StartWave(); ArriveOne(); ArriveOne(); yield return null;
            Assert.That(Read<int>(effect, "HitSequence"), Is.EqualTo(2), "Two arrivals in one frame remain two damage facts.");
            Assert.That(Read<bool>(effect, "HasVisibleFractures"), Is.True);
            Assert.That(Read<float>(effect, "LastHitDamageFraction"), Is.EqualTo(.4f).Within(.001f));
            Assert.That(Read<Vector2>(effect, "ShakePixels").sqrMagnitude, Is.GreaterThan(0f));
            var position = camera.transform.position; var matrix = camera.worldToCameraMatrix;
            effect.GetType().GetMethod("BeginCamera", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(effect, new object[] { default(ScriptableRenderContext), camera });
            Assert.That(Read<bool>(effect, "RenderViewOverridden"), Is.True);
            Assert.That(camera.transform.position, Is.EqualTo(position));
            effect.GetType().GetMethod("EndCamera", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(effect, new object[] { default(ScriptableRenderContext), camera });
            Assert.That(Read<bool>(effect, "RenderViewOverridden"), Is.False);
            for (var i = 0; i < 16; i++) Assert.That(camera.worldToCameraMatrix[i], Is.EqualTo(matrix[i]).Within(.0001f));
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(Read<int>(effect, "ActiveHitCount"), Is.Zero); Assert.That(Read<bool>(effect, "HasVisibleFractures"), Is.False);
            input.Press(Keyboard.current.iKey); yield return null; input.Release(Keyboard.current.iKey); yield return null;
            ArriveOne(); yield return null;
            Assert.That(Read<bool>(effect, "HasVisibleFractures"), Is.False, "Opaque inventory covers the dome even if its world projection is onscreen.");
            input.Press(Keyboard.current.escapeKey); yield return null; input.Release(Keyboard.current.escapeKey);
            yield return new WaitForSecondsRealtime(.8f);
            camera.transform.position += new Vector3(100f, 100f, 0); yield return null;
            ArriveOne(); yield return null;
            Assert.That(Read<bool>(effect, "HasVisibleFractures"), Is.False);
            Assert.That(Read<Vector2>(effect, "ShakePixels").sqrMagnitude, Is.GreaterThan(0f));
            Lose(); Assert.That(game.RunDriver.CanRetry, Is.False);
            var hero = scene.PlayerMover.transform.position;
            yield return new WaitForSecondsRealtime(1.65f);
            Assert.That(Read<float>(effect, "Whiteout"), Is.EqualTo(1f).Within(.001f));
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(hero));
            yield return WaitForRetry(); Assert.That(game.RunDriver.RetryFailedWave(), Is.True);
            Assert.That(Read<float>(effect, "Whiteout"), Is.Zero); Assert.That(Read<int>(effect, "ActiveHitCount"), Is.Zero);
            Assert.That(Read<bool>(effect, "RenderViewOverridden"), Is.False);
        }
    }
}
#endif
