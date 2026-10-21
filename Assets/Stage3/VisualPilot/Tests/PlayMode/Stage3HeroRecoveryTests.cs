#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
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
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroRecoveryRulesTests
    {
        private Stage2HeroProgression hero;
        private Stage2Equipment equipment;
        private Stage2HeroRecovery recovery;
        private Stage2Inventory wallet;
        [SetUp]public void Setup()
        {
            hero=new Stage2HeroProgression(Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression"));
            wallet=new Stage2Inventory();equipment=new Stage2Equipment(Resources.Load<Stage2EquipmentCatalog>("Stage3Equipment"),wallet);
            equipment.EnableBlueprintResearch(Resources.Load<Stage2BlueprintCatalog>("Stage3Blueprints"));
            recovery=new Stage2HeroRecovery(hero,equipment);
        }
        private Stage2EquipmentItem BagItem(Stage2ItemType type=Stage2ItemType.Helmet)
        {Assert.That(equipment.TryReceiveLoot(type,Stage2ItemQuality.S,37,out var item),Is.True);return item;}
        [Test]public void FatalDamageLosesOnlyHalfCurrentLevelXpAndOnlyBagInstances()
        {
            var data=Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression");hero.AwardExperience(data.RequiredXp(1)+35);
            var level=hero.Level;var points=hero.AvailablePoints;var bow=equipment.Equipped(Stage2ItemSlot.Weapon);
            var axe=equipment.Equipped(Stage2ItemSlot.Axe);var pick=equipment.Equipped(Stage2ItemSlot.Pickaxe);var bag=BagItem();
            wallet.TryAddGold(10000);wallet.TryAdd(MapResourceKind.Wood,73);
            var merchant=new Stage2Merchant(equipment,Resources.Load<Stage2MerchantCatalog>("Stage3Merchant"));
            var known=merchant.BlueprintItems.First();merchant.TryTradeBlueprint(true,known.Id);equipment.Blueprints.TryStudy(known.Id);
            var paper=merchant.BlueprintItems.First();merchant.TryTradeBlueprint(true,paper.Id);
            var gold=wallet.Gold;var raises=0;hero.LevelRaised+=_=>raises++;
            Assert.That(recovery.TryApplyExplorationDamage(100,new Vector2(50,70)),Is.True);
            Assert.That(hero.Level,Is.EqualTo(level));Assert.That(hero.AvailablePoints,Is.EqualTo(points));Assert.That(hero.XpWithin,Is.EqualTo(18));
            Assert.That(recovery.LastXpLost,Is.EqualTo(17));Assert.That(raises,Is.Zero);
            Assert.That(hero.Health,Is.EqualTo(100));Assert.That(hero.Mana,Is.EqualTo(100));
            Assert.That(equipment.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(bow));Assert.That(equipment.Equipped(Stage2ItemSlot.Axe),Is.SameAs(axe));
            Assert.That(equipment.Equipped(Stage2ItemSlot.Pickaxe),Is.SameAs(pick));Assert.That(equipment.Find(bag.Id),Is.Null);
            var drop=recovery.Drops.Single();Assert.That(drop.Position,Is.EqualTo(new Vector2(50,70)));Assert.That(drop.ItemCount,Is.EqualTo(2));
            Assert.That(drop.Equipment.Single(),Is.SameAs(bag));Assert.That(drop.Blueprints.Single(),Is.SameAs(paper));
            Assert.That(equipment.Blueprints.Find(paper.Id),Is.Null);Assert.That(equipment.Blueprints.IsStudied(known.Kind),Is.True);
            Assert.That(wallet.Gold,Is.EqualTo(gold));Assert.That(wallet.Get(MapResourceKind.Wood),Is.EqualTo(73));
            Assert.That(recovery.TryRecover(drop.Id),Is.True);Assert.That(recovery.TryRecover(drop.Id),Is.False);
            Assert.That(equipment.Find(bag.Id),Is.SameAs(bag));Assert.That(bag.Level,Is.EqualTo(37));Assert.That(bag.Quality,Is.EqualTo(Stage2ItemQuality.S));
            Assert.That(equipment.Blueprints.Find(paper.Id),Is.SameAs(paper));Assert.That(recovery.Drops,Is.Empty);
            Assert.That(wallet.Gold,Is.EqualTo(gold));Assert.That(hero.XpWithin,Is.EqualTo(18));
        }
        [Test]public void NonfatalDamageKeepsBagAndInvalidDamageCannotMutateAnything()
        {
            var item=BagItem();Assert.That(recovery.TryApplyExplorationDamage(25,Vector2.one),Is.True);
            Assert.That(hero.Health,Is.EqualTo(75));hero.Tick(1,true);Assert.That(hero.Health,Is.EqualTo(76));
            foreach(var bad in new[]{0f,-1f,float.NaN,float.PositiveInfinity})Assert.That(recovery.TryApplyExplorationDamage(bad,Vector2.zero),Is.False);
            Assert.That(recovery.TryApplyExplorationDamage(100,new Vector2(float.NaN,1)),Is.False);
            Assert.That(recovery.DeathCount,Is.Zero);Assert.That(recovery.Drops,Is.Empty);Assert.That(equipment.Find(item.Id),Is.SameAs(item));
        }
        [Test]public void MultipleDeathsRetainEarlierDropsAndEmptyBagCreatesNoFalseMarker()
        {
            var one=BagItem();recovery.TryApplyExplorationDamage(100,Vector2.one);var first=recovery.Drops.Single();
            var two=BagItem(Stage2ItemType.Legs);recovery.TryApplyExplorationDamage(100,Vector2.one*10);var second=recovery.Drops.Last();
            recovery.TryApplyExplorationDamage(100,Vector2.one*20);Assert.That(recovery.Drops.Count,Is.EqualTo(2));
            Assert.That(first.Id,Is.Not.EqualTo(second.Id));Assert.That(recovery.TryRecover(second.Id),Is.True);
            Assert.That(equipment.Find(two.Id),Is.SameAs(two));Assert.That(equipment.Find(one.Id),Is.Null);
            Assert.That(recovery.TryRecover(first.Id),Is.True);Assert.That(equipment.Find(one.Id),Is.SameAs(one));
        }
        [Test]public void MutationObserversCannotClaimOrKillTwiceAndResetDoesNotReturnOldDrops()
        {
            BagItem();var nested=0;
            equipment.Changed+=()=>{if(recovery.TryApplyExplorationDamage(100,Vector2.one))nested++;foreach(var d in recovery.Drops)if(recovery.TryRecover(d.Id))nested++;};
            recovery.TryApplyExplorationDamage(100,Vector2.one);Assert.That(nested,Is.Zero);Assert.That(recovery.DeathCount,Is.EqualTo(1));
            var id=recovery.Drops.Single().Id;recovery.Reset();equipment.Reset();Assert.That(recovery.TryRecover(id),Is.False);
            Assert.That(recovery.Drops,Is.Empty);Assert.That(equipment.Items.Count,Is.EqualTo(3));
        }
        [Test]public void MaxLevelAndOneXpNeverLoseALevelOrAwardPoints()
        {
            hero.AwardExperience(1);recovery.TryApplyExplorationDamage(100,Vector2.zero);Assert.That(hero.XpWithin,Is.EqualTo(1));
            hero.AwardExperience(int.MaxValue);var points=hero.AvailablePoints;recovery.TryApplyExplorationDamage(100,Vector2.zero);
            Assert.That(hero.Level,Is.EqualTo(100));Assert.That(hero.XpWithin,Is.Zero);Assert.That(hero.AvailablePoints,Is.EqualTo(points));
        }
    }

    public sealed class Stage3HeroRecoveryTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2HeroRecoveryDriver recovery;
        private Stage2Equipment gear;
        private Stage2HeroProgressionDriver hero;
        private Component binding,hud;
        private static T Read<T>(object owner,string name)=>(T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name).Invoke(owner,args);
        [UnitySetUp]public IEnumerator Load()
        {
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();Time.timeScale=1;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<14;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            recovery=game.GetComponent<Stage2HeroRecoveryDriver>();Assert.That(recovery,Is.Not.Null);
            gear=game.GetComponent<Stage2EquipmentDriver>().Rules;hero=game.GetComponent<Stage2HeroProgressionDriver>();
            hero.AutomaticTick=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.enabled=false;game.RunDriver.ResetRun();game.SetPaused(false);
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");
            yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;input.TearDown();Time.timeScale=1;
        }
        private Stage2HeroDrop DieAt(Vector2 position)
        {
            scene.PlayerMover.transform.position=position;
            Assert.That(gear.TryReceiveLoot(Stage2ItemType.Helmet,Stage2ItemQuality.A,19,out _),Is.True);
            Assert.That(recovery.TryApplyExplorationDamage(100),Is.True);return recovery.Rules.Drops.Last();
        }
        [UnityTest]public IEnumerator DeathRespawnsAtBaseClearsTransientUiAndLeavesCurrentWorldAndEquippedStats()
        {
            var basePoint=Read<Vector2>(game.GetComponent("Stage3WorldTravel"),"BasePosition");
            hero.Rules.AwardExperience(hero.Catalog.RequiredXp(1)+35);var level=hero.Rules.Level;
            var wood=game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood);var weapon=gear.Equipped(Stage2ItemSlot.Weapon);
            var window=hud.GetType().GetProperty("ActiveWindow").PropertyType;Call(hud,"SetWindow",Enum.Parse(window,"Inventory"));
            var death=basePoint+Vector2.right*20;var drop=DieAt(death);yield return null;
            Assert.That(drop.Position,Is.EqualTo(death));Assert.That(Vector2.Distance(scene.PlayerMover.transform.position,basePoint),Is.LessThanOrEqualTo(12));
            Assert.That(hero.CanLand(scene.PlayerMover.transform.position),Is.True);Assert.That(hero.Rules.Level,Is.EqualTo(level));
            Assert.That(hero.Rules.XpWithin,Is.EqualTo(18));Assert.That(hero.Rules.Health,Is.EqualTo(100));Assert.That(hero.Rules.Mana,Is.EqualTo(100));
            Assert.That(Read<bool>(hud,"IsModalOpen"),Is.False);Assert.That(Read<bool>(binding,"IsBlinkArmed"),Is.False);
            Assert.That(gear.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(weapon));Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood),Is.EqualTo(wood));
        }
        [UnityTest]public IEnumerator ActualERecoversOnceAndHeldKeyCannotClaimAnotherDropOrTriggerPortal()
        {
            var gate=(Vector2)scene.EntranceMarker.position;var first=DieAt(gate+Vector2.down);
            var second=DieAt(gate+Vector2.down*1.5f);scene.PlayerMover.transform.position=first.Position;
            Assert.That(recovery.TryRecover(first.Id),Is.True); // direct command also claims this frame
            Assert.That(game.PlayerInputBlocked,Is.True);yield return null;
            input.Release(Keyboard.current.eKey);yield return null;input.Press(Keyboard.current.eKey);yield return null;
            Assert.That(recovery.Rules.Drops.Count,Is.Zero);Assert.That(gear.Items.Count,Is.EqualTo(5));
            Assert.That(game.RunDriver.Run.DaytimeGateOpen,Is.False);
            Assert.That(gear.TryReceiveLoot(Stage2ItemType.Legs,Stage2ItemQuality.F,1,out _),Is.True);
            Assert.That(recovery.TryApplyExplorationDamage(100),Is.True);scene.PlayerMover.transform.position=second.Position;
            yield return null;yield return null;Assert.That(recovery.Rules.Drops.Count,Is.EqualTo(1),"Held E must be released after death.");
            input.Release(Keyboard.current.eKey);yield return null;input.Press(Keyboard.current.eKey);yield return null;
            Assert.That(recovery.Rules.Drops,Is.Empty);input.Release(Keyboard.current.eKey);
        }
        [UnityTest]public IEnumerator RecoveryRefusesRemotePausedModalDebugAndConstructionAndReleasesAfterRetry()
        {
            var drop=DieAt((Vector2)scene.PlayerMover.transform.position+Vector2.right*20);
            Assert.That(recovery.TryRecover(drop.Id),Is.False);scene.PlayerMover.transform.position=drop.Position;
            game.SetPaused(true);Assert.That(recovery.TryRecover(drop.Id),Is.False);Assert.That(recovery.TryApplyExplorationDamage(100),Is.False);game.SetPaused(false);
            var type=hud.GetType().GetProperty("ActiveWindow").PropertyType;Call(hud,"SetWindow",Enum.Parse(type,"Inventory"));Assert.That(recovery.TryRecover(drop.Id),Is.False);Call(hud,"CloseWindow");
            var legacy=game.GetComponent<Stage2GameHud>();legacy.SetDebugOpen(true);Assert.That(recovery.TryRecover(drop.Id),Is.False);legacy.SetDebugOpen(false);
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood,1);game.Building.SelectAndBeginPlacement(2);
            Assert.That(recovery.TryRecover(drop.Id),Is.False);game.Building.CancelPlacement();Assert.That(recovery.TryRecover(drop.Id),Is.True);yield return null;
        }
        [UnityTest]public IEnumerator ThroneRetryPreservesDropsAndPortalCreepsNeverDamageHeroButResetClearsDrops()
        {
            var drop=DieAt((Vector2)scene.PlayerMover.transform.position+Vector2.right*20);var xp=hero.Rules.XpWithin;
            game.RunDriver.Run.StartNextWaveForTesting();
            for(var i=0;i<game.RunDriver.Run.ThroneMaximum;i++)
            {var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(game.RunDriver.Run.Phase,Is.EqualTo(Stage2RunPhase.Lost));Assert.That(hero.Rules.Health,Is.EqualTo(100));
            Assert.That(recovery.TryApplyExplorationDamage(100),Is.False);Assert.That(game.RunDriver.Run.RetryFailedWave(),Is.True);
            Assert.That(recovery.Rules.Find(drop.Id),Is.SameAs(drop));Assert.That(hero.Rules.XpWithin,Is.EqualTo(xp));
            game.RunDriver.ResetRun();Assert.That(recovery.Rules.Drops,Is.Empty);Assert.That(gear.Items.Count,Is.EqualTo(3));yield return null;
        }
        [UnityTest]public IEnumerator ExistingDevelopmentCommandTestsDeathWithoutGivingBagOrXp()
        {
            var legacy=game.GetComponent<Stage2GameHud>();var itemCount=gear.Items.Count;var xp=hero.Rules.XpWithin;
            legacy.SetDebugOpen(true);Assert.That(legacy.TestHeroDeath(),Is.True);
            Assert.That(recovery.Rules.DeathCount,Is.EqualTo(1));Assert.That(recovery.Rules.Drops,Is.Empty);
            Assert.That(gear.Items.Count,Is.EqualTo(itemCount));Assert.That(hero.Rules.XpWithin,Is.EqualTo(xp));
            game.SetPaused(true);Assert.That(legacy.TestHeroDeath(),Is.False);game.SetPaused(false);legacy.SetDebugOpen(false);yield return null;
        }
        [UnityTest]public IEnumerator AnalogHudObservesClockSleepDebugNightPauseAndResetWithoutItsOwnTimer()
        {
            var type=hud.GetType();
            var cycle=(Func<float>)type.GetProperty("CycleProgress").GetValue(hud);
            var night=(Func<bool>)type.GetProperty("IsNightPhase").GetValue(hud);
            var run=game.RunDriver.Run;Assert.That(cycle,Is.Not.Null);Assert.That(night,Is.Not.Null);
            Assert.That(cycle(),Is.Zero);Assert.That(night(),Is.False);
            run.Tick(300);Assert.That(cycle(),Is.EqualTo(300f/780f).Within(.00001));
            Assert.That(run.SleepUntilBeforeNight(),Is.True);Assert.That(cycle(),Is.GreaterThan(300f/780f));
            var tools=game.GetComponent<Stage2GameHud>();Assert.That(tools.StartNextWave(),Is.True);
            Assert.That(Vector2.Distance(tools.DebugButtonRect.position*tools.Scale,Read<Rect>(hud,"DevelopmentAccessRect").position),Is.LessThan(.01f));
            Assert.That(Vector2.Distance(tools.DebugButtonRect.size*tools.Scale,Read<Rect>(hud,"DevelopmentAccessRect").size),Is.LessThan(.01f));
            Assert.That(night(),Is.True);Assert.That(cycle(),Is.EqualTo(600f/780f).Within(.00001));
            game.SetPaused(true);var before=cycle();run.Tick(180);Assert.That(cycle(),Is.EqualTo(before));game.SetPaused(false);
            game.RunDriver.ResetRun();Assert.That(cycle(),Is.Zero);Assert.That(night(),Is.False);yield return null;
        }
    }
}
#endif
