#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroCombatSliceTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private int peakPopulation;
        private float simulatedSeconds;
        private static string Evidence => "Artifacts/S3-HERO-COMBAT-20261018";

        [UnitySetUp] public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            scene = game.GetComponent<Stage2SceneBootstrap>();
            Assert.That(game.IsInitialized, Is.True);
            Assert.That(game.HeroCombat, Is.Not.Null);
            Assert.That(game.Building.Defense.RoleCount, Is.EqualTo(3));
            game.HeroCombat.AutomaticTick = false;
            game.RunDriver.enabled = false;
            game.RunDriver.ResetRun(); game.SetPaused(false);
            Directory.CreateDirectory(Evidence);
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown(); Time.timeScale = 1f;
        }

        [UnityTest] public IEnumerator BowSelectsNearestAndRespectsRangeFlightPauseBuildGatherDebugTerminalAndReset()
        {
            var origin = scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance);
            scene.PlayerMover.transform.position = origin;
            var near = game.Enemies.Spawn(); near.SetAutomaticTick(false); near.transform.position = origin + Vector2.right;
            var far = game.Enemies.Spawn(); far.SetAutomaticTick(false); far.transform.position = origin + Vector2.right * 3;
            var nearInitialHealth = near.Health;
            var outside = game.Enemies.Spawn(); outside.SetAutomaticTick(false);
            outside.transform.position = origin + Vector2.right * (game.HeroCombat.Catalog.AttackRange + 1);
            game.HeroCombat.Tick(.01f);
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.EqualTo(1));
            Assert.That(near.IsAlive, Is.True, "A launched arrow has not yet landed.");
            game.Enemies.TickShots(game.HeroCombat.Catalog.ProjectileFlightSeconds);
            Assert.That(near.Health, Is.EqualTo(nearInitialHealth - game.HeroCombat.Catalog.AttackDamage));
            Assert.That(far.Health, Is.EqualTo(nearInitialHealth));
            Assert.That(outside.IsAlive, Is.True);

            var remaining = game.HeroCombat.Rules.CooldownRemaining;
            game.SetPaused(true); game.HeroCombat.Tick(100f);
            Assert.That(game.HeroCombat.Rules.CooldownRemaining, Is.EqualTo(remaining));
            game.SetPaused(false);
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood, 1);
            game.Building.SelectAndBeginPlacement(2); game.HeroCombat.Tick(100f);
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.EqualTo(1));
            game.Building.CancelPlacement();
            input.Press(Keyboard.current.eKey); yield return null;
            Assert.That(game.Gathering.IsInteracting, Is.True);
            game.HeroCombat.Tick(100f);
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.EqualTo(1));
            input.Release(Keyboard.current.eKey); yield return null;
            var hud = game.GetComponent<Stage2GameHud>(); hud.SetDebugOpen(true);
            game.HeroCombat.Tick(100f); Assert.That(game.HeroCombat.Rules.ShotsFired, Is.EqualTo(1));
            hud.SetDebugOpen(false);
            game.HeroCombat.Tick(game.HeroCombat.Catalog.FireInterval);
            game.Enemies.TickShots(game.HeroCombat.Catalog.ProjectileFlightSeconds);
            Assert.That(near.IsAlive, Is.False);
            for (var arrow = 0; arrow < 2; arrow++)
            {
                game.HeroCombat.Tick(game.HeroCombat.Catalog.FireInterval);
                game.Enemies.TickShots(game.HeroCombat.Catalog.ProjectileFlightSeconds);
            }
            Assert.That(far.IsAlive, Is.False);
            game.HeroCombat.Tick(game.HeroCombat.Catalog.FireInterval);
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.EqualTo(4), "Out-of-range enemies cannot be shot.");

            Stage3TestCycle.StartNight(game.RunDriver.Run);
            typeof(Stage2Run).GetMethod("OnReachedThrone", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(game.RunDriver.Run, new object[] { game.RunDriver.Run.ThroneMaximum });
            game.HeroCombat.Tick(100f);
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.EqualTo(4));
            Assert.That(game.Enemies.PendingShotCount, Is.Zero);
            game.RunDriver.ResetRun();
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.Zero);
            Assert.That(game.HeroCombat.Rules.CooldownRemaining, Is.Zero);
            yield return null;
        }

        [UnityTest] public IEnumerator FirstWaveCanBeClearedByBowAloneWithoutStartingTowersOrMaterialGrants()
        {
            var maximumGold=game.Gathering.Economy.Inventory.Gold+
                game.RunDriver.Run.WaveEnemyCount*game.Gathering.Economy.GoldPerKill;
            // Throughput fixture follows targets with no walking delay. The old
            // stationary-at-portal policy can leak5HP enemies; it is not a player requirement.
            SimulateWave(true, false);
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(game.RunDriver.Run.WaveIndex, Is.EqualTo(1));
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Assert.That(game.Gathering.Economy.Inventory.Gold, Is.InRange(0, maximumGold), "Only successful per-kill gold rolls pay a reward.");
            foreach (MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind)))
                Assert.That(game.Gathering.Economy.Inventory.Get(kind), Is.Zero);
            WriteEvidence("first-wave-bow-only");
            yield return null;
        }

        [UnityTest] public IEnumerator ActualWasdMovementPreventsPortalBowLaunchInEveryDirectionUntilStopped()
        {
            var preferred=scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)+Vector2.down*3;
            var origin=preferred;var found=false;
            // A legal standing point can still touch a randomly generated trunk/rock.
            // This fixture needs a genuinely open walking area in all four directions.
            for(var radius=0;radius<=6&&!found;radius++)
            for(var y=-radius;y<=radius&&!found;y++)for(var x=-radius;x<=radius;x++)
            {
                if(Mathf.Max(Mathf.Abs(x),Mathf.Abs(y))!=radius)continue;
                var candidate=preferred+new Vector2(x,y);
                if(!scene.PlayerMover.CanTeleportTo(candidate)||scene.PlayerMover.Obstacles.IsBlocked(candidate,1.5f))continue;
                origin=candidate;found=true;break;
            }
            Assert.That(found,Is.True,"A clear area for four actual WASD walking directions is required.");
            Assert.That(scene.PlayerMover.TryTeleportTo(origin),Is.True);
            Assert.That(scene.PlayerMover.isActiveAndEnabled,Is.True);Assert.That(game.PlayerInputBlocked,Is.False);
            var enemy=game.Enemies.Spawn();enemy.SetAutomaticTick(false);enemy.transform.position=origin+Vector2.right*2;
            game.HeroCombat.Tick(.01f);Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(1));
            var before=enemy.Health;
            var keyboard=Keyboard.current;var keys=new[]{Key.S,Key.W,Key.A,Key.D};
            foreach(var key in keys)
            {
                var start=(Vector2)scene.PlayerMover.transform.position;
                // UnityTest fixture Press/Release queues a full state snapshot. Calling
                // Release(old) then Press(new) before an input update can reintroduce the
                // old key from the still-current snapshot, cancelling opposite directions.
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));
                // Allow the ordinary Input System/player Update cycle to consume the new key state.
                for(var frame=0;frame<3;frame++)yield return null;
                Assert.That(keyboard[key].isPressed,Is.True,"The queued "+key+" state must have been consumed by the normal input loop.");
                Assert.That(Vector2.Distance(start,scene.PlayerMover.transform.position),Is.GreaterThan(.0001f),
                    "Held "+key+" must move through the clear test area; inputBlocked="+game.PlayerInputBlocked);
                Assert.That(scene.PlayerMover.IsMoving,Is.True);
                game.HeroCombat.Tick(game.HeroCombat.Rules.EffectiveFireInterval+1);
                Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(1),"Ground movement must suppress new arrows in every direction.");
                Assert.That(game.HeroCombat.Rules.CooldownRemaining,Is.Zero);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return null;yield return null;
            }
            game.Enemies.TickShots(game.HeroCombat.Catalog.ProjectileFlightSeconds);
            Assert.That(enemy.Health,Is.LessThan(before),"Movement cannot cancel an arrow already released.");
            yield return null;Assert.That(scene.PlayerMover.IsMoving,Is.False);
            game.HeroCombat.Tick(.01f);Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(2));
        }

        [UnityTest] public IEnumerator DenseSecondHordeOverwhelmsEvenBowWithUnrestrictedAccessToEveryTarget()
        {
            SimulateWave(false, false);
            Assert.That(game.RunDriver.Run.WaveIndex, Is.EqualTo(1));
            // A deliberately generous upper-bound policy: no walking time or aiming mistakes.
            // Failure here is stronger evidence than one stationary hero losing at the portal.
            SimulateWave(true, false);
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(peakPopulation, Is.GreaterThanOrEqualTo(80));
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            WriteEvidence("second-horde-unrestricted-bow");
            yield return null;
        }

        [UnityTest] public IEnumerator EnemyBreachesCompletePassiveWallAndResumesTowardThrone()
        {
            var map = scene.MapDefinition;
            var cut = map.EnemyEntrance.x + 5;
            var walls = new List<Stage2Tower>();
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood, 100);
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
            {
                game.Building.SelectAndBeginPlacement(2);
                game.Building.Defense.SetPreviewCell(new Vector2Int(cut, y));
                if (game.Building.Defense.EvaluatePreview() != PlacementResult.Ready) continue;
                Assert.That(game.Building.Defense.TryConfirm(out var wall), Is.EqualTo(PlacementResult.Ready));
                walls.Add(wall);
            }
            game.Building.CancelPlacement();
            Assert.That(walls.Count, Is.GreaterThan(0));
            Assert.That(game.Navigation.PlanFrom(map.EnemyEntrance).Status.ToString(), Is.EqualTo("ReachableBlocker"));
            var enemy = game.Enemies.Spawn(); enemy.SetAutomaticTick(false);
            for (var i = 0; i < 1000 && enemy.IsAlive && enemy.CurrentCell.x <= cut; i++) enemy.Tick(.05f);
            Assert.That(walls.Count(w => w == null || !w.IsAlive), Is.EqualTo(1));
            Assert.That(enemy.CurrentCell.x, Is.GreaterThan(cut));
            Assert.That(game.Navigation.PlanFrom(enemy.CurrentCell).Status.ToString(), Is.EqualTo("ReachesThrone"));
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.Zero);
            yield return null;
        }

        [UnityTest] public IEnumerator SixCheapBarricadesAndUnrestrictedBowStillNeedAttackingTowers()
        {
            SimulateWave(false, false);
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood, 6);
            BuildNearEntrance(2, 6);
            Assert.That(game.Building.Defense.TowerCount, Is.EqualTo(6));
            SimulateWave(true, true);
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            WriteEvidence("second-horde-six-passive-walls-and-bow");
            yield return null;
        }

        [UnityTest] public IEnumerator BowWithBoundedPaidDefensesClearsBothHordesAndRunStillRestartsCleanly()
        {
            SimulateWave(false, false);
            var inventory = game.Gathering.Economy.Inventory;
            // Declared combat fixture: actually buy/study D102's Longshot paper,
            // funding only its exact price so the bounded defense budget stays unchanged.
            var binding=game.GetComponent("Stage3PlayerInterfaceBinding");
            var merchant=(Stage2Merchant)binding.GetType().GetProperty("Merchant").GetValue(binding);
            var paper=merchant.BlueprintItems.Single(item=>item.Kind==Stage2BlueprintKind.Longshot);
            var gold=inventory.Gold;inventory.TryAddGold(merchant.BlueprintPrice(paper,true));
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(game.GetComponent<Stage2EquipmentDriver>().Rules.Blueprints.TryStudy(paper.Id),Is.EqualTo(Stage2BlueprintResult.Ready));
            Assert.That(inventory.Gold,Is.EqualTo(gold));
            inventory.TryAdd(MapResourceKind.Wood, 20); inventory.TryAdd(MapResourceKind.Stone, 14);
            BuildNearEntrance(0, 2);
            BuildNearEntrance(1, 2);
            BuildNearEntrance(2, 6);
            var paidWood = 20 - inventory.Get(MapResourceKind.Wood);
            var paidStone = 14 - inventory.Get(MapResourceKind.Stone);
            Assert.That(paidWood, Is.LessThanOrEqualTo(20));
            Assert.That(paidStone, Is.LessThanOrEqualTo(14));
            SimulateWave(true, true);
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(game.RunDriver.Run.WaveIndex, Is.EqualTo(2));
            WriteEvidence("second-horde-paid-defenses");
            SimulateWave(true, true);
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(game.RunDriver.Run.ContinuousDays ? Stage2RunPhase.Preparation : Stage2RunPhase.Won));
            if (game.RunDriver.Run.ContinuousDays) Assert.That(game.RunDriver.Run.WaveNumber, Is.EqualTo(4));
            WriteEvidence("final-horde-paid-defenses");
            game.RunDriver.ResetRun();
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            Assert.That(game.Navigation.OccupiedCount, Is.Zero);
            Assert.That(game.Enemies.ActiveCount, Is.Zero);
            Assert.That(game.Enemies.PendingShotCount, Is.Zero);
            Assert.That(game.HeroCombat.Rules.ShotsFired, Is.Zero);
            Assert.That(game.RunDriver.Run.WaveIndex, Is.Zero);
            yield return null;
        }

        private void SimulateWave(bool unrestrictedHero, bool towers)
        {
            var run = game.RunDriver.Run;
            scene.PlayerMover.transform.position = scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance) + Vector2.up * 2f;
            Assert.That(Stage3TestCycle.StartNight(run), Is.True);
            peakPopulation = game.Enemies.ActiveCount;
            simulatedSeconds = 0f;
            const float dt = .05f;
            while (run.Phase == Stage2RunPhase.Wave && simulatedSeconds < 360f)
            {
                run.Tick(dt);
                for (var i = game.Enemies.ActiveCount - 1; i >= 0 && !run.IsTerminal; i--)
                {
                    if (i >= game.Enemies.ActiveCount) continue;
                    var enemy = game.Enemies.GetEnemy(i);
                    enemy.SetAutomaticTick(false); enemy.Tick(dt);
                }
                if (unrestrictedHero && game.Enemies.ActiveCount > 0)
                    scene.PlayerMover.transform.position = game.Enemies.GetEnemy(0).transform.position;
                game.HeroCombat.Tick(dt);
                if (towers)
                    for (var i = game.Building.Defense.TowerCount - 1; i >= 0; i--)
                        game.Building.Defense.GetTower(i).Tick(dt);
                peakPopulation = Mathf.Max(peakPopulation, game.Enemies.ActiveCount);
                simulatedSeconds += dt;
            }
            Assert.That(simulatedSeconds, Is.LessThan(360f), "The actual authored road/wave must reach an outcome.");
        }

        private void BuildNearEntrance(int role, int count)
        {
            var map = scene.MapDefinition;
            var path = game.Navigation.PlanFrom(map.EnemyEntrance).Path;
            var candidates = new List<Vector2Int>();
            for (var i = 6; i < Math.Min(path.Count - 1, 28); i++)
                for (var y = -3; y <= 3; y++) for (var x = -3; x <= 3; x++)
                {
                    var cell = path[i] + new Vector2Int(x, y);
                    if (!candidates.Contains(cell)) candidates.Add(cell);
                }
            foreach (var cell in candidates.OrderBy(c => Vector2.Distance(map.CellToWorld(c), map.CellToWorld(path[12]))))
            {
                if (count == 0) break;
                game.Building.SelectAndBeginPlacement(role);
                game.Building.Defense.SetPreviewCell(cell);
                if (game.Building.Defense.EvaluatePreview() != PlacementResult.Ready) continue;
                Assert.That(game.Building.Defense.TryConfirm(out _), Is.EqualTo(PlacementResult.Ready));
                count--;
            }
            game.Building.CancelPlacement();
            Assert.That(count, Is.Zero, "The bounded defense policy needs enough legal cells.");
        }

        private void WriteEvidence(string policy)
        {
            File.AppendAllText(Path.Combine(Evidence, "combat-policies.txt"),
                $"{policy}: phase={game.RunDriver.Run.Phase}, seconds={simulatedSeconds:F2}, peak={peakPopulation}, " +
                $"bowShots={game.HeroCombat.Rules.ShotsFired}, defenses={game.Building.Defense.TowerCount}, throne={game.RunDriver.Run.ThroneHealth}\n");
        }
    }
}
#endif
