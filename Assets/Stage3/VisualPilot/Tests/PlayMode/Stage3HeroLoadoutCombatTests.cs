#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroLoadoutCombatTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component binding,driver,hud,world;
        private Stage2EquipmentDriver equipment;
        private Stage2HeroProgressionDriver hero;
        private Stage2HeroSkills rules;
        private static T Read<T>(object o,string name)=>(T)o.GetType().GetProperty(name).GetValue(o);
        private static object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(o,args);
        private static void Set(object o,string name,object value)=>o.GetType().GetProperty(name).SetValue(o,value);
        [UnitySetUp]public IEnumerator Setup()
        {
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            Time.timeScale=1;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<18;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");driver=game.GetComponent("Stage3HeroSkillsDriver");
            hud=game.GetComponent("Stage3FantasyHud");world=game.GetComponent("Stage3GuildWorldDriver");
            hero=game.GetComponent<Stage2HeroProgressionDriver>();equipment=game.GetComponent<Stage2EquipmentDriver>();
            Assert.That(driver,Is.Not.Null);rules=Read<Stage2HeroSkills>(driver,"Rules");
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;hero.AutomaticTick=false;Set(driver,"AutomaticTick",false);Set(world,"AutomaticTick",false);
            game.RunDriver.ResetRun();game.SetPaused(false);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);yield return null;yield return null;input.TearDown();Time.timeScale=1;}
        private void Learn(int id)
        {var parent=rules.Catalog.Get(id).prerequisite;if(parent>=0&&rules.Rank(parent)==0)Learn(parent);Assert.That(rules.TryUpgrade(id),Is.True,"learn"+id);}
        private void EquipStaff()
        {equipment.Rules.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.F,1,out var staff);equipment.Rules.TryEquip(staff.Id);}
        private Stage2Enemy Portal(Vector2 point)
        {var enemy=game.Enemies.Spawn();enemy.SetAutomaticTick(false);enemy.transform.position=point;return enemy;}
        [UnityTest]public IEnumerator FireAndLightningHitAtMostFourRealPortalActorsAndNeverClearDenseHorde()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(9);Learn(10);EquipStaff();
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;
            for(var i=0;i<12;i++)Portal(point+new Vector2(i%3*.1f,i/3*.1f));
            var before=game.Enemies.ActiveCount;var mana=hero.Rules.Mana;
            Assert.That(Call(driver,"TryCast",9,point),Is.True);
            Assert.That(Read<int>(driver,"LastTargetCount"),Is.EqualTo(4));Assert.That(game.Enemies.ActiveCount,Is.GreaterThanOrEqualTo(before-4));
            Assert.That(hero.Rules.Mana,Is.LessThan(mana));Assert.That(rules.Cooldown(9),Is.EqualTo(25));
            Assert.That(Call(driver,"TryCast",10,point),Is.True);Assert.That(Read<int>(driver,"LastTargetCount"),Is.EqualTo(4));
            Assert.That(game.Enemies.ActiveCount,Is.GreaterThanOrEqualTo(before-8));yield return null;
        }
        [UnityTest]public IEnumerator AimedShotCancelsOnMovementWithoutManaOrCooldownAndMovingFireRequiresTalent()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(1);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;Portal(point);
            var mana=hero.Rules.Mana;Assert.That(Call(driver,"TryCast",1,point),Is.True);
            scene.PlayerMover.MoveBy(Vector2.right*.1f);Assert.That(scene.PlayerMover.IsMoving,Is.True);
            Call(driver,"Tick",.6f);
            Assert.That(Read<bool>(driver,"IsAiming"),Is.False);Assert.That(hero.Rules.Mana,Is.EqualTo(mana));Assert.That(rules.Cooldown(1),Is.Zero);
            var shots=game.HeroCombat.Rules.ShotsFired;game.HeroCombat.Tick(1);Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(shots));
            Learn(13);Learn(14);Call(driver,"Tick",0f);game.HeroCombat.Tick(1);
            Assert.That(game.HeroCombat.Rules.ShotsFired,Is.EqualTo(shots+1));yield return null;
        }
        [UnityTest]public IEnumerator StaffBoltAndColdArrowDamageAndSlowActualNeutralActors()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(6);Learn(3);
            var units=Read<System.Collections.Generic.IReadOnlyList<Stage2GuildEnemy>>(world,"Enemies");
            var unit=units.First(u=>u.Alive);scene.PlayerMover.transform.position=unit.Position+Vector2.down*3;
            var hp=unit.Health;EquipStaff();Assert.That(Call(driver,"TryCast",6,unit.Position),Is.True);Assert.That(unit.Health,Is.LessThan(hp));
            equipment.Rules.TrySwitchWeapon();Assert.That(Call(driver,"TryCast",3,unit.Position),Is.True);
            var slow=(float)typeof(Stage2GuildEnemy).GetField("slowRemaining",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(unit);
            Assert.That(slow,Is.GreaterThan(0));yield return null;
        }
        [UnityTest]public IEnumerator F3ConsumesMixedPotionAndF10AloneOpensDebug()
        {
            var shop=Read<Stage2Merchant>(binding,"Merchant");game.Gathering.Economy.Inventory.TryAddGold(250);
            var item=shop.Items.First(i=>i.Type==Stage2ItemType.MixedPotion);shop.TryTrade(true,item.Id);
            var recovery=game.GetComponent<Stage2HeroRecoveryDriver>();Assert.That(recovery.TryApplyExplorationDamage(40),Is.True);
            hero.Rules.TryBlink(Vector2.zero,Vector2.one,_=>true,out _);Assert.That(hero.Rules.Mana,Is.Zero);
            input.Press(Keyboard.current.f3Key);yield return null;yield return null;input.Release(Keyboard.current.f3Key);
            Assert.That(hero.Rules.Health,Is.EqualTo(100));Assert.That(hero.Rules.Mana,Is.EqualTo(100));
            Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.MixedPotion),Is.Zero);
            Assert.That(game.GetComponent<Stage2GameHud>().DebugOpen,Is.False);
            input.Press(Keyboard.current.f10Key);yield return null;yield return null;
            Assert.That(game.GetComponent<Stage2GameHud>().DebugOpen,Is.True);input.Release(Keyboard.current.f10Key);
        }
        [UnityTest]public IEnumerator QuickslotReflectsFullHealthWithoutConsumingStock()
        {
            var shop=Read<Stage2Merchant>(binding,"Merchant");game.Gathering.Economy.Inventory.TryAddGold(100);
            var item=shop.Items.First(i=>i.Type==Stage2ItemType.HealthPotion);shop.TryTrade(true,item.Id);
            yield return null;yield return null;
            object Slot(){var view=Read<object>(hud,"HeroLoadoutView");return ((Array)view.GetType().GetField("QuickSlots").GetValue(view)).GetValue(0);}
            var slot=Slot();Assert.That((bool)slot.GetType().GetField("CanUse").GetValue(slot),Is.False);
            Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.HealthPotion),Is.EqualTo(1));
            game.GetComponent<Stage2HeroRecoveryDriver>().TryApplyExplorationDamage(10);yield return null;yield return null;
            slot=Slot();Assert.That((bool)slot.GetType().GetField("CanUse").GetValue(slot),Is.True);
        }
        [UnityTest]public IEnumerator SmallWorldXpBonusAccumulatesAndDoesNotIncreasePortalReward()
        {
            hero.Rules.AwardExperience(15000);Learn(33);Call(driver,"Tick",0f);
            var total=0;hero.Rules.ExperienceGained+=amount=>total+=amount;
            var guild=Read<Stage2Guild>(binding,"Guild");var quest=guild.Catalog.GetAt(0);
            for(var i=0;i<10;i++)
            {
                Call(world,"ResetSpot",quest);
                var units=Read<System.Collections.Generic.IReadOnlyList<Stage2GuildEnemy>>(world,"Enemies");
                var unit=units.First(u=>u.Alive&&u.Profile.enemyKind==quest.enemyKind);
                Assert.That(Call(world,"Damage",unit,unit.Health),Is.True);
            }
            Assert.That(total,Is.EqualTo(Mathf.RoundToInt(quest.killExperience*10*1.05f)),"even2XPactors retain the5%bonus");
            var before=total;var enemy=Portal((Vector2)scene.PlayerMover.transform.position+Vector2.up*2);enemy.TakeDamage(enemy.Health);
            Assert.That(total-before,Is.EqualTo(hero.Catalog.XpPerKill),"portalXP remains unchanged");yield return null;
        }
    }
}
#endif
