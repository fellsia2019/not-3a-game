#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3GuildTests
    {
        private InputTestFixture input;private Stage2GameComposition game;private Stage2SceneBootstrap scene;
        private Stage2Guild guild;private Component binding,hud,world,travel;private Stage2HeroProgressionDriver hero;
        private static T Read<T>(object o,string p)=>(T)o.GetType().GetProperty(p).GetValue(o);
        private static object Call(object o,string m,params object[] args)=>o.GetType().GetMethod(m).Invoke(o,args);
        private static void Set(object o,string p,object value)=>o.GetType().GetProperty(p).SetValue(o,value);
        [UnitySetUp]public IEnumerator Setup()
        {
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();Time.timeScale=1;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<18;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");
            guild=Read<Stage2Guild>(binding,"Guild");world=game.GetComponent("Stage3GuildWorldDriver");travel=game.GetComponent("Stage3WorldTravel");
            Assert.That(guild,Is.Not.Null);Assert.That(Read<bool>(world,"Ready"),Is.True);
            Set(world,"AutomaticTick",false);game.HeroCombat.AutomaticTick=false;hero=game.GetComponent<Stage2HeroProgressionDriver>();hero.AutomaticTick=false;
            game.RunDriver.enabled=false;game.RunDriver.ResetRun();game.SetPaused(false);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {foreach(var r in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(r);yield return null;yield return null;input.TearDown();Time.timeScale=1;}
        private Vector2 Board=>Read<Vector2>(Read<object>(travel,"CityPresentation"),"GuildInteractionPosition");
        private Rect GuildFootprint=>Read<Rect>(Read<object>(travel,"CityPresentation"),"GuildFootprint");
        private T QuestField<T>(string name)
        {
            var view=Read<object>(hud,"GuildView");var list=(Array)view.GetType().GetField("Quests").GetValue(view);
            var entry=list.Cast<object>().First(q=>(int)q.GetType().GetField("Id").GetValue(q)==1);
            return (T)entry.GetType().GetField(name).GetValue(entry);
        }
        private IEnumerable<Stage2GuildEnemy> Units=>Read<IReadOnlyList<Stage2GuildEnemy>>(world,"Enemies");
        private void OpenNearby()
        {scene.PlayerMover.transform.position=Board+Vector2.down;Assert.That(Call(binding,"TryOpenGuild"),Is.True);}
        private void KillContract(int id)
        {
            var c=guild.FindActive(id);
            foreach(var unit in Units.Where(u=>u.Generation==c.Generation&&u.Profile.enemyKind==c.Quest.enemyKind).ToArray())
                Assert.That(Call(world,"Damage",unit,unit.Health),Is.True);
        }
        [UnityTest]public IEnumerator BoardClickPreviewRangeAcceptClearTurnInAndRepeatReset()
        {
            var body=Read<SpriteRenderer>(Read<object>(travel,"CityPresentation"),"GuildVisual");
            var decoration=Read<SpriteRenderer>(Read<object>(travel,"CityPresentation"),"GuildBoardVisual");
            scene.PlayerMover.transform.position=Board+Vector2.down;scene.CameraFollow.ConfigureTacticalView(10);scene.CameraFollow.SnapToTarget();yield return null;
            var decorativeScreen=(Vector2)scene.CameraFollow.BoundCamera.WorldToScreenPoint(decoration.bounds.center);
            Assert.That(Call(binding,"TryOpenGuildAtScreen",decorativeScreen),Is.False,"World notice board is decoration only.");
            var screen=(Vector2)scene.CameraFollow.BoundCamera.WorldToScreenPoint(body.bounds.center);
            Assert.That(Call(binding,"TryOpenGuildAtScreen",screen),Is.True);Assert.That(guild.Active,Is.Empty,"Preview cannot accept/reset.");
            scene.PlayerMover.transform.position=Board+Vector2.down*10;Assert.That(Call(binding,"TryGuildAction",1),Is.False);
            scene.PlayerMover.transform.position=Board+Vector2.down;
            Assert.That(Call(binding,"TryGuildAction",1),Is.True);var first=guild.FindActive(1);var gold=game.Gathering.Economy.Inventory.Gold;
            Assert.That(first,Is.Not.Null);KillContract(1);yield return null;
            Assert.That(first.Cleared,Is.True);Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold));
            Assert.That(guild.CompletedForRank,Is.Zero);Assert.That(Units.Count(u=>u.Alive&&u.Profile.enemyKind==Stage2GuildEnemyKind.Orc),Is.Zero);
            Assert.That(Call(binding,"TryGuildAction",1),Is.True);Assert.That(guild.CompletedForRank,Is.EqualTo(1));
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold+first.Quest.gold));
            Assert.That(Units.Count(u=>u.Alive&&u.Profile.enemyKind==Stage2GuildEnemyKind.Orc),Is.Zero,"Turn-in must leave spot cleared.");
            Assert.That(Call(binding,"TryGuildAction",1),Is.True);Assert.That(guild.FindActive(1).Generation,Is.Not.EqualTo(first.Generation));
            Assert.That(Units.Count(u=>u.Alive&&u.Profile.enemyKind==Stage2GuildEnemyKind.Orc),Is.EqualTo(first.Quest.population));
        }
        [UnityTest]public IEnumerator AutomaticBowKillsWorldTargetWithoutTouchingHordeCountersAndWorldMeleeUsesRecovery()
        {
            OpenNearby();Call(binding,"TryGuildAction",1);Call(hud,"CloseWindow");
            var unit=Units.First(u=>u.Profile.enemyKind==Stage2GuildEnemyKind.Orc&&u.Alive);
            scene.PlayerMover.transform.position=unit.Position+Vector2.down*2;
            var portals=game.Enemies.ActiveCount;var before=unit.Health;
            for(var i=0;i<8&&unit.Alive;i++){game.HeroCombat.Tick(2);Call(world,"Tick",.5f);}
            Assert.That(unit.Health,Is.LessThan(before));Assert.That(game.HeroCombat.Rules.ShotsFired,Is.GreaterThan(0));
            Assert.That(game.Enemies.ActiveCount,Is.EqualTo(portals));
            var alive=Units.First(u=>u.Alive&&u.Profile.enemyKind==Stage2GuildEnemyKind.Orc);
            scene.PlayerMover.transform.position=alive.Position+Vector2.down*.5f;
            Call(world,"Tick",.01f);Call(world,"Tick",alive.Profile.attackWindup+.01f);
            Assert.That(hero.Rules.Health,Is.LessThan(100));
            var recovery=game.GetComponent<Stage2HeroRecoveryDriver>();recovery.TryApplyExplorationDamage(hero.Rules.Health-1);
            Call(world,"Tick",3f);Call(world,"Tick",1f);Assert.That(recovery.Rules.DeathCount,Is.EqualTo(1));
            Assert.That(hero.Rules.Health,Is.EqualTo(100));Assert.That(Vector2.Distance(scene.PlayerMover.transform.position,alive.Position),Is.GreaterThan(10));yield return null;
        }
        [UnityTest]public IEnumerator GuildActionsUseBuildingApproachAndExplainRemotePreviewWithoutRemoteRewards()
        {
            var footprint=GuildFootprint;
            scene.PlayerMover.transform.position=footprint.center+Vector2.down*(footprint.height*.5f+12);
            Assert.That(Call(binding,"TryOpenGuild"),Is.True);
            Assert.That(Read<bool>(binding,"NearGuild"),Is.False);
            Assert.That(QuestField<bool>("CanExecute"),Is.False);
            Assert.That(QuestField<string>("ActionLabel"),Is.EqualTo("Подойдите к гильдии"));
            Assert.That(Call(binding,"TryGuildAction",1),Is.False);Assert.That(guild.Active,Is.Empty);

            var front=new Vector2(footprint.center.x,footprint.yMin-2f);
            Assert.That(scene.PlayerMover.CanTeleportTo(front),Is.True,"The actual front approach must remain walkable.");
            scene.PlayerMover.transform.position=front;yield return null;
            Assert.That(Read<bool>(binding,"NearGuild"),Is.True);
            Assert.That(QuestField<bool>("CanExecute"),Is.True);
            Assert.That(Call(binding,"TryGuildAction",1),Is.True);
            Assert.That(QuestField<string>("ActionLabel"),Is.EqualTo("Выполняется"));
            var contract=guild.FindActive(1);KillContract(1);
            var gold=game.Gathering.Economy.Inventory.Gold;
            scene.PlayerMover.transform.position=footprint.center+Vector2.down*(footprint.height*.5f+12);yield return null;
            Assert.That(Call(binding,"TryGuildAction",1),Is.False);
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold));
            Assert.That(guild.FindActive(1),Is.SameAs(contract));

            var side=new Vector2(footprint.xMax+2.4f,footprint.yMin-.5f);
            Assert.That(scene.PlayerMover.CanTeleportTo(side),Is.True,"The side of the service frontage must remain walkable.");
            scene.PlayerMover.transform.position=side;yield return null;
            Assert.That(Read<bool>(binding,"NearGuild"),Is.True);
            Assert.That(Call(binding,"TryGuildAction",1),Is.True);
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold+contract.Quest.gold));
        }
        [UnityTest]public IEnumerator SharedWorldBowDoesNotLaunchDuringGroundMovementButAlreadyReleasedArrowLands()
        {
            OpenNearby();Assert.That(Call(binding,"TryGuildAction",1),Is.True);Call(hud,"CloseWindow");
            var unit=Units.First(u=>u.Profile.enemyKind==Stage2GuildEnemyKind.Orc&&u.Alive);
            var position=unit.Position+Vector2.down*2;
            Assert.That(scene.PlayerMover.TryTeleportTo(position),Is.True);
            unit=Units.Where(u=>u.Profile.enemyKind==Stage2GuildEnemyKind.Orc&&u.Alive)
                .OrderBy(u=>Vector2.Distance(u.Position,position)).First();
            var before=unit.Health;game.HeroCombat.Tick(.01f);
            Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(1));
            Assert.That(unit.Health,Is.EqualTo(before),"The first arrow is in flight.");
            scene.PlayerMover.MoveBy(Vector2.down*.05f);
            Assert.That(scene.PlayerMover.IsMoving,Is.True);
            game.HeroCombat.Tick(game.HeroCombat.Rules.EffectiveFireInterval+1f);
            Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(1),"Walking must not invoke the shared portal/world targeting bridge.");
            Assert.That(game.HeroCombat.Rules.CooldownRemaining,Is.Zero,"Walking does not freeze the previous shot's cooldown.");
            Call(world,"Tick",.5f);Assert.That(unit.Health,Is.LessThan(before),"Released arrows keep their normal flight/damage.");
            yield return null;Assert.That(scene.PlayerMover.IsMoving,Is.False);
            game.HeroCombat.Tick(.01f);Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(2));
        }
        [UnityTest]public IEnumerator FailedNightRetryKeepsCurrentQuestAndCampFullResetClearsBoth()
        {
            OpenNearby();Call(binding,"TryGuildAction",1);var c=guild.FindActive(1);
            var unit=Units.First(u=>u.Generation==c.Generation);Call(world,"Damage",unit,unit.Health);var remaining=c.Remaining;
            Call(hud,"CloseWindow");game.RunDriver.Run.StartNextWaveForTesting();
            for(var n=0;n<game.RunDriver.Run.ThroneMaximum;n++)
            {var e=game.Enemies.SpawnAt(scene.MapDefinition.Throne);e.SetAutomaticTick(false);e.Tick(.01f);}
            Assert.That(game.RunDriver.Run.Phase,Is.EqualTo(Stage2RunPhase.Lost));Assert.That(game.RunDriver.Run.RetryFailedWave(),Is.True);
            Assert.That(guild.FindActive(1),Is.SameAs(c));Assert.That(c.Remaining,Is.EqualTo(remaining));
            Assert.That(Units.Count(u=>u.Alive&&u.Generation==c.Generation),Is.EqualTo(remaining));
            game.RunDriver.ResetRun();yield return null;Assert.That(guild.Active,Is.Empty);Assert.That(guild.Rank,Is.EqualTo(Stage2GuildRank.F));
            Assert.That(Units.Any(u=>u.Generation==c.Generation),Is.False);
        }
        [UnityTest]public IEnumerator BoardActionsRevalidatePauseDebugAndOtherModalWithoutRefilling()
        {
            OpenNearby();var initial=Units.Select(u=>u.Id).ToArray();
            game.SetPaused(true);Assert.That(Call(binding,"TryGuildAction",1),Is.False);game.SetPaused(false);
            var legacy=game.GetComponent<Stage2GameHud>();legacy.SetDebugOpen(true);Assert.That(Call(binding,"TryGuildAction",1),Is.False);legacy.SetDebugOpen(false);
            var type=hud.GetType().GetProperty("ActiveWindow").PropertyType;Call(hud,"SetWindow",Enum.Parse(type,"Inventory"));
            Assert.That(Call(binding,"TryGuildAction",1),Is.False);Assert.That(guild.Active,Is.Empty);
            Assert.That(Units.Select(u=>u.Id),Is.EqualTo(initial));Call(hud,"CloseWindow");yield return null;
        }
    }
}
#endif
