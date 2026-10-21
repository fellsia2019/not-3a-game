#if UNITY_EDITOR
using System;
using System.Collections;
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
    public sealed class Stage3DaytimeGateTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component gate;
        private Stage2Run Run => game.RunDriver.Run;
        private static T Read<T>(object owner,string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner,string name,params object[] args) => owner.GetType().GetMethod(name).Invoke(owner,args);
        private bool Interact() => (bool)gate.GetType().GetMethod("TryInteract").Invoke(gate, null);
        [UnitySetUp] public IEnumerator Load()
        {
            Time.timeScale=1; input=new InputTestFixture(); input.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<12;i++) yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>(); scene=game.GetComponent<Stage2SceneBootstrap>();
            gate=game.GetComponent("Stage3DaytimeGate"); Assert.That(gate,Is.Not.Null);
            game.HeroCombat.AutomaticTick=false; game.RunDriver.enabled=false; game.RunDriver.ResetRun(); game.SetPaused(false);
            scene.PlayerMover.transform.position=scene.EntranceMarker.position+Vector3.down*2;
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale=1;
        }
        private void FirstVictoryAndDawn()
        {
            Assert.That(Stage3TestCycle.StartNight(Run),Is.True);
            while(!Run.SpawningFinished)Run.Tick(.25f);
            while(game.Enemies.ActiveCount>0) game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            Assert.That(Run.IsDay,Is.True); Assert.That(Run.LastClearedTier,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ActualLocalETogglesOncePerPressAndClosedLiveCreepsStillBlockSleep()
        {
            Assert.That(Interact(),Is.False); FirstVictoryAndDawn();
            input.Release(Keyboard.current.eKey); yield return null;
            input.Press(Keyboard.current.eKey); yield return null;
            Assert.That(Run.DaytimeGateOpen,Is.True); Run.Tick(.01f);
            var first=game.Enemies.GetEnemy(0); first.SetAutomaticTick(false);
            yield return null; yield return null; Assert.That(Run.DaytimeGateOpen,Is.True);
            Assert.That(Run.SleepUntilBeforeNight(),Is.False);
            input.Release(Keyboard.current.eKey); yield return null;
            input.Press(Keyboard.current.eKey); yield return null;
            Assert.That(Run.DaytimeGateOpen,Is.False); Assert.That(first.IsAlive,Is.True);
            Assert.That(Run.SleepUntilBeforeNight(),Is.False);
            first.TakeDamage(int.MaxValue); Assert.That(Run.SleepUntilBeforeNight(),Is.True);
            Assert.That(Run.ClockHour,Is.EqualTo(19)); Assert.That(Run.ClockMinute,Is.EqualTo(50));
            input.Release(Keyboard.current.eKey);
        }
        [UnityTest] public IEnumerator GateRefusesRemotePauseModalDebugAndBuildingInput()
        {
            FirstVictoryAndDawn(); var origin=scene.PlayerMover.transform.position;
            scene.PlayerMover.transform.position+=Vector3.right*15; Assert.That(Interact(),Is.False);
            scene.PlayerMover.transform.position=origin;
            game.SetPaused(true); Assert.That(Interact(),Is.False); game.SetPaused(false);
            var hud=game.GetComponent("Stage3FantasyHud");
            var windowType=hud.GetType().GetProperty("ActiveWindow").PropertyType;
            hud.GetType().GetMethod("SetWindow").Invoke(hud,new[]{Enum.Parse(windowType,"Inventory")});
            Assert.That(Interact(),Is.False); hud.GetType().GetMethod("CloseWindow").Invoke(hud,null);
            var legacy=game.GetComponent<Stage2GameHud>(); legacy.SetDebugOpen(true); Assert.That(Interact(),Is.False);
            legacy.SetDebugOpen(false); Assert.That(Interact(),Is.True); Assert.That(Run.DaytimeGateOpen,Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator LocalEAfterDebugSecondNightDefeatRepeatsOnlyClearedDifficultyIndefinitely()
        {
            FirstVictoryAndDawn();Run.OpenDaytimeGate();Run.Tick(.001f);
            var health=game.Enemies.GetEnemy(0).MaximumHealth;Run.CloseDaytimeGate();game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            var tools=game.GetComponent<Stage2GameHud>();Assert.That(tools.StartNextWave(),Is.True);
            Assert.That(game.Enemies.GetEnemy(0).ThreatLevel,Is.EqualTo(2));
            var data=UnityEditor.AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>("Assets/Stage3/CombatSlice/HordeRun.asset");
            for(var i=0;i<Run.ThroneMaximum&&!Run.IsTerminal;i++)
            {var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(Run.Phase,Is.EqualTo(Stage2RunPhase.Lost));Assert.That(Run.RetryFailedWave(),Is.True);
            Assert.That(Run.LastClearedTier,Is.EqualTo(1));Assert.That(Run.WaveNumber,Is.EqualTo(2));
            input.Release(Keyboard.current.eKey);yield return null;
            var legacy=game.GetComponent<Stage2GameHud>();Assert.That(legacy.PortalInteractionHint,Does.Contain("Угроза 1"));
            input.Press(Keyboard.current.eKey);yield return null;
            Assert.That(Run.DaytimeGateOpen,Is.True);Run.Tick(data.GetWave(0).SpawnInterval*3+.001f);
            Assert.That(game.Enemies.ActiveCount,Is.EqualTo(4));
            for(var i=0;i<game.Enemies.ActiveCount;i++)
            {
                var enemy=game.Enemies.GetEnemy(i);enemy.SetAutomaticTick(false);
                Assert.That(enemy.ThreatLevel,Is.EqualTo(1));Assert.That(enemy.MaximumHealth,Is.EqualTo(health));
            }
            Run.Tick(data.GetWave(0).EnemyCount*data.GetWave(0).SpawnInterval*2);
            Assert.That(game.Enemies.ActiveCount,Is.GreaterThan(data.GetWave(0).EnemyCount*2));
            for(var i=0;i<game.Enemies.ActiveCount;i++)game.Enemies.GetEnemy(i).SetAutomaticTick(false);
            var alive=game.Enemies.ActiveCount;
            input.Release(Keyboard.current.eKey);yield return null;input.Press(Keyboard.current.eKey);yield return null;
            Assert.That(Run.DaytimeGateOpen,Is.False);Assert.That(game.Enemies.ActiveCount,Is.EqualTo(alive));
            Assert.That(Run.LastClearedTier,Is.EqualTo(1));input.Release(Keyboard.current.eKey);
        }
        [UnityTest] public IEnumerator DaytimeKillsKeepXpGoldAfterDefeatAndNewAttemptAwardsAgain()
        {
            FirstVictoryAndDawn(); var progression=game.GetComponent<Stage2HeroProgressionDriver>();
            progression.AutomaticTick=false;
            var xp=0; progression.Rules.ExperienceGained+=amount=>xp+=amount;
            Run.OpenDaytimeGate(); Run.Tick(.01f); game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            var gold=game.Gathering.Economy.Inventory.Gold; var level=progression.Rules.Level;
            var within=progression.Rules.XpWithin; Assert.That(xp,Is.EqualTo(progression.Catalog.XpPerKill));
            for(var i=0;i<10&&!Run.IsTerminal;i++)
            {var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f);}
            Assert.That(Run.Phase,Is.EqualTo(Stage2RunPhase.Lost)); Assert.That(Run.RetryFailedWave(),Is.True);
            Assert.That(progression.Rules.Level,Is.EqualTo(level)); Assert.That(progression.Rules.XpWithin,Is.EqualTo(within));
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold)); Assert.That(xp,Is.EqualTo(progression.Catalog.XpPerKill));
            Run.OpenDaytimeGate(); Run.Tick(.01f); game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            Assert.That(xp,Is.EqualTo(2*progression.Catalog.XpPerKill));
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold+game.Gathering.Economy.GoldPerKill));
            yield return null;
        }
        [UnityTest] public IEnumerator SelectedDwellingSleepReflectsLiveFightAndUsesTheSameLocalRules()
        {
            FirstVictoryAndDawn();
            var home=game.GetComponent("Stage3Dwelling");
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood,10); Call(home,"BeginPlacement");
            var origin=(Vector2)scene.PlayerMover.transform.position; var built=false;
            for(var ring=5;ring<45&&!built;ring+=2)
            for(var x=-ring;x<=ring&&!built;x+=2) for(var y=-ring;y<=ring&&!built;y+=2)
            {
                var point=origin+new Vector2(x,y);
                if(Call(home,"EvaluateAt",point).ToString()!="Ready") continue;
                built=Call(home,"TryBuildAt",point).ToString()=="Built";
            }
            Assert.That(built,Is.True);
            scene.PlayerMover.transform.position=Read<Vector2>(home,"InteractionPosition");
            scene.CameraFollow.SnapToTarget(); yield return null; yield return null;
            var visual=Read<SpriteRenderer>(game.GetComponent("Stage3DwellingPresentation"),"BuiltVisual");
            var binding=game.GetComponent("Stage3PlayerInterfaceBinding"); var hud=game.GetComponent("Stage3FantasyHud");
            Assert.That(Call(binding,"TrySelectBuildingAtScreen",(Vector2)scene.CameraFollow.BoundCamera.WorldToScreenPoint(visual.bounds.center)),Is.True);
            Assert.That(Read<object>(binding,"SelectedBuildingKind").ToString(),Is.EqualTo("Dwelling"));
            object View() => Read<object>(hud,"SelectedBuildingView");
            bool Available() => (bool)View().GetType().GetField("SleepAvailable").GetValue(View());
            Assert.That(Available(),Is.True); Assert.That(Read<Rect>(hud,"SelectedBuildingSleepRect").width,Is.GreaterThan(0));
            Run.OpenDaytimeGate(); Run.Tick(.01f); var creep=game.Enemies.GetEnemy(0); creep.SetAutomaticTick(false);
            Run.CloseDaytimeGate(); yield return null;
            Assert.That(Available(),Is.False); Assert.That(Call(binding,"TrySleepInSelectedDwelling"),Is.False);
            Assert.That(View().GetType().GetField("SleepUnavailableReason").GetValue(View()).ToString(),Does.Contain("боя"));
            creep.TakeDamage(int.MaxValue); yield return null;
            Assert.That(Available(),Is.True); Assert.That(Call(binding,"TrySleepInSelectedDwelling"),Is.True);
            Assert.That(Run.ClockHour,Is.EqualTo(19)); Assert.That(Run.ClockMinute,Is.EqualTo(50));
            Assert.That(Available(),Is.False);
        }
    }
}
#endif
