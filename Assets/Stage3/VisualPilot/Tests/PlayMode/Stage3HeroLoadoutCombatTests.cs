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
            game.RunDriver.ResetRun();game.SetPaused(false);
            Stage3TestCycle.StudyBlueprint(game,Stage2BlueprintKind.WandRecipe);yield return null;
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
            Assert.That(unit.Statuses.SlowMultiplier,Is.LessThan(1));Assert.That(unit.Statuses.FrozenRemaining,Is.GreaterThan(0));yield return null;
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
        [UnityTest]public IEnumerator DigitKeysBelongToHeroSkillsAndNeverOpenConstruction()
        {
            // Fund real building options so affordability cannot hide a stale shortcut.
            game.GetComponent<Stage2GameHud>().AddAllResources(100);
            Assert.That(game.Building.CanBeginPlacement(0),Is.True);
            Assert.That(game.Building.UseRoleShortcuts,Is.False);
            var starts=0;game.Building.PlacementStarting+=()=>starts++;
            var role=game.Building.Defense.SelectedRoleIndex;
            foreach(var key in new[]{Keyboard.current.digit1Key,Keyboard.current.digit2Key,Keyboard.current.digit3Key,Keyboard.current.digit4Key,Keyboard.current.digit5Key})
            {
                input.Press(key);yield return null;input.Release(key);yield return null;
                Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("None"));
                Assert.That(game.Building.Defense.IsPlacing,Is.False);
                Assert.That(game.Building.Defense.SelectedRoleIndex,Is.EqualTo(role));
            }
            Assert.That(starts,Is.Zero);

            hero.Rules.AwardExperience(int.MaxValue);Learn(1);Learn(2);Learn(6);EquipStaff();
            equipment.Rules.TrySwitchWeapon();Assert.That(Read<int>(driver,"ActiveWeapon"),Is.Zero);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;
            Portal(point);
            input.Set(Mouse.current.position,(Vector2)scene.CameraFollow.BoundCamera.WorldToScreenPoint(point));yield return null;
            input.Press(Keyboard.current.digit1Key);yield return null;input.Release(Keyboard.current.digit1Key);
            Assert.That(Read<bool>(driver,"IsAiming"),Is.True,"1 starts the learned bow skill.");
            Call(binding,"CancelHeroSkill");yield return null;
            input.Press(Keyboard.current.digit2Key);yield return null;input.Release(Keyboard.current.digit2Key);yield return null;
            Assert.That(rules.Cooldown(2),Is.GreaterThan(0),"2 casts the learned fire arrow.");
            equipment.Rules.TrySwitchWeapon();Portal(point);
            input.Press(Keyboard.current.digit1Key);yield return null;input.Release(Keyboard.current.digit1Key);yield return null;
            Assert.That(rules.Cooldown(6),Is.GreaterThan(0),"1 casts the active staff skill after switching weapons.");
            Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("None"));
            Assert.That(starts,Is.Zero);Assert.That(game.Building.Defense.TowerCount,Is.Zero);
        }
        [UnityTest]public IEnumerator ConstructionDigitsSelectVisibleBuildingsAndNeverCastWeaponSkills()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(1);Learn(2);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;Portal(point);
            input.Set(Mouse.current.position,(Vector2)scene.CameraFollow.BoundCamera.WorldToScreenPoint(point));yield return null;
            Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,"Construction"));
            Call(hud,"SetBuildGroup",0);game.Gathering.Economy.Inventory.Reset();yield return null;
            input.Press(Keyboard.current.digit2Key);yield return null;input.Release(Keyboard.current.digit2Key);yield return null;
            Assert.That(game.Building.Defense.IsPlacing,Is.False,"An unfunded visible building cannot be armed.");
            Assert.That(rules.Cooldown(2),Is.Zero,"Unavailable construction must not fall through to a learned skill.");
            game.Gathering.Economy.Inventory.TryAdd(Not3A.Stage2.Map.MapResourceKind.Wood,10);
            input.Press(Keyboard.current.digit2Key);yield return null;input.Release(Keyboard.current.digit2Key);yield return null;
            Assert.That(game.Building.Defense.SelectedRoleIndex,Is.EqualTo(2),"Visible card2 is the barricade while Longshot is unknown.");
            Assert.That(game.Building.Defense.IsPlacing,Is.True);Assert.That(rules.Cooldown(2),Is.Zero);
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
        [UnityTest]public IEnumerator QuickItemMouseRequestsAndKeysReportActualSuccessRefusalAndEmptyReserve()
        {
            object Slot(int index){var view=Read<object>(hud,"HeroLoadoutView");return ((Array)view.GetType().GetField("QuickSlots").GetValue(view)).GetValue(index);}
            T Field<T>(int index,string field)=>(T)Slot(index).GetType().GetField(field).GetValue(Slot(index));
            Call(hud,"RequestQuickSlotUse",0);
            Assert.That(Field<int>(0,"FeedbackSerial"),Is.EqualTo(1));Assert.That(Field<bool>(0,"FeedbackSucceeded"),Is.False);
            Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.HealthPotion),Is.Zero);
            var shop=Read<Stage2Merchant>(binding,"Merchant");game.Gathering.Economy.Inventory.TryAddGold(200);
            var item=shop.Items.First(i=>i.Type==Stage2ItemType.HealthPotion);shop.TryTrade(true,item.Id);shop.TryTrade(true,item.Id);
            Call(hud,"RequestQuickSlotUse",0); // Full HP refuses without consuming.
            Assert.That(Field<int>(0,"FeedbackSerial"),Is.EqualTo(2));Assert.That(Field<bool>(0,"FeedbackSucceeded"),Is.False);
            Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.HealthPotion),Is.EqualTo(2));
            game.GetComponent<Stage2HeroRecoveryDriver>().TryApplyExplorationDamage(40);
            input.Press(Keyboard.current.f1Key);yield return null;yield return null;input.Release(Keyboard.current.f1Key);yield return null;
            Assert.That(Field<int>(0,"FeedbackSerial"),Is.EqualTo(3));Assert.That(Field<bool>(0,"FeedbackSucceeded"),Is.True);
            Assert.That(hero.Rules.Health,Is.EqualTo(100));Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.HealthPotion),Is.EqualTo(1));
            Call(hud,"RequestQuickSlotUse",0); // Shared cooldown refuses, leaves last bottle.
            Assert.That(Field<int>(0,"FeedbackSerial"),Is.EqualTo(4));Assert.That(Field<bool>(0,"FeedbackSucceeded"),Is.False);
            Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.HealthPotion),Is.EqualTo(1));
            Assert.That(Field<float>(3,"CooldownRemaining"),Is.Zero);
            Call(hud,"RequestQuickSlotUse",3);input.Press(Keyboard.current.f4Key);yield return null;yield return null;input.Release(Keyboard.current.f4Key);
            Assert.That(Field<int>(3,"FeedbackSerial"),Is.Zero);Assert.That(Field<int>(3,"Count"),Is.Zero);
            Assert.That(game.GetComponent<Stage2GameHud>().DebugOpen,Is.False);
        }

        private Stage2Enemy DurablePortal(Vector2 point)
        {var enemy=Portal(point);Call(enemy,"SetThreat",Stage2EnemySource.Night,1,40f,1f);return enemy;}
        [UnityTest]public IEnumerator ElementalAreasApplyActualFreezePoisonAndSchoolSpecificWeakening()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(3);Learn(5);Learn(10);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;
            var first=DurablePortal(point);var second=DurablePortal(point+Vector2.right*.2f);
            Assert.That(Call(driver,"TryCast",3,point),Is.True);
            Assert.That(first.Statuses.FrozenRemaining,Is.EqualTo(1));Assert.That(second.Statuses.Immobilized,Is.True);
            var position=first.transform.position;first.Tick(.5f);Assert.That(first.transform.position,Is.EqualTo(position));
            first.Tick(.5f);Assert.That(first.Statuses.Immobilized,Is.False);Assert.That(first.Statuses.SlowMultiplier,Is.LessThan(1));
            Assert.That(Call(driver,"TryCast",5,point),Is.True);
            Assert.That(first.Statuses.PoisonRemaining,Is.EqualTo(3));Assert.That(first.Statuses.PhysicalVulnerability,Is.EqualTo(.2f));
            var hp=first.Health;Call(driver,"Tick",1f);Assert.That(first.Health,Is.LessThan(hp),"Poison must tick actual damage.");
            EquipStaff();Assert.That(Call(driver,"TryCast",10,point),Is.True);
            Assert.That(first.Statuses.StunnedRemaining,Is.EqualTo(1));Assert.That(first.Statuses.MagicVulnerability,Is.EqualTo(.2f));
            var hits=first.Health;for(var i=0;i<5;i++)first.TakeDamage(1,Stage2DamageSchool.Magic);
            Assert.That(hits-first.Health,Is.EqualTo(6),"Magic weakening applies to real incoming damage.");yield return null;
        }
        [UnityTest]public IEnumerator BurnStaysOnTargetsWhileFireGroundAndRoamingArrowHitNewEntrantsForThreeSeconds()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(2);Learn(4);Learn(9);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;var initial=DurablePortal(point);
            Assert.That(Call(driver,"TryCast",2,point),Is.True);Assert.That(initial.Statuses.BurningRemaining,Is.EqualTo(3));
            initial.transform.position=point+Vector2.up*5;var hp=initial.Health;Call(driver,"Tick",1f);
            Assert.That(initial.Health,Is.LessThan(hp),"Target burn follows an enemy out of the original area.");
            initial.transform.position=point;Assert.That(Call(driver,"TryCast",4,point),Is.True);
            var entrant=DurablePortal(point+Vector2.right*3);hp=entrant.Health;entrant.transform.position=point;
            game.SetPaused(true);Call(driver,"Tick",2f);Assert.That(entrant.Health,Is.EqualTo(hp));game.SetPaused(false);
            Call(driver,"Tick",1f);Assert.That(entrant.Health,Is.LessThan(hp));Call(driver,"Tick",2f);
            hp=entrant.Health;Call(driver,"Tick",1f);Assert.That(entrant.Health,Is.EqualTo(hp),"Roaming arrow expires after three active seconds.");
            EquipStaff();Assert.That(Call(driver,"TryCast",9,point),Is.True);
            var late=DurablePortal(point+Vector2.right*3);hp=late.Health;late.transform.position=point;
            Call(driver,"Tick",1f);Assert.That(late.Health,Is.LessThan(hp),"Ground fire damages new entrants, not just initial burst targets.");
            Call(driver,"Tick",2f);hp=late.Health;Call(driver,"Tick",1f);Assert.That(late.Health,Is.EqualTo(hp));yield return null;
        }
        [UnityTest]public IEnumerator EveryThreeSecondPeriodicEffectPaysAllThreeTicksAtHighFrameRate()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(4);Learn(9);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;var target=DurablePortal(point);
            foreach(var id in new[]{2,4,9})
            {
                if(id==9)EquipStaff();
                Assert.That(Call(driver,"TryCast",id,point),Is.True);var health=target.Health;
                for(var frame=0;frame<360;frame++)Call(driver,"Tick",1f/120);
                Assert.That(health-target.Health,Is.EqualTo(3),"Exactly three starter periodic hits at120Hz for skill"+id);
                health=target.Health;Call(driver,"Tick",1f);Assert.That(target.Health,Is.EqualTo(health),"No extra tick after expiry");
            }
            yield return null;
        }
        [UnityTest]public IEnumerator AutocastUsesNearestNormalTargetAndManualStaffCastStillWorks()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(1);Learn(6);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;
            var near=DurablePortal(point);var far=DurablePortal(point+Vector2.up*2);var hp=near.Health;var farHp=far.Health;
            Assert.That(rules.ToggleAutocast(1),Is.True);Call(driver,"Tick",0f);Assert.That(Read<bool>(driver,"IsAiming"),Is.True);
            Call(driver,"Tick",.6f);Assert.That(near.Health,Is.LessThan(hp));Assert.That(far.Health,Is.EqualTo(farHp));
            Assert.That(rules.Cooldown(1),Is.GreaterThan(0));
            EquipStaff();Assert.That(rules.ToggleAutocast(6),Is.True);hp=near.Health;Call(driver,"Tick",.2f);
            Assert.That(near.Health,Is.LessThan(hp));Assert.That(rules.Cooldown(6),Is.GreaterThan(0));
            Set(driver,"AutoCastBlocked",new Func<bool>(()=>true));rules.Tick(5,true);hp=far.Health;
            Assert.That(Call(driver,"TryCast",6,(Vector2)far.transform.position),Is.True);Assert.That(far.Health,Is.LessThan(hp),"Manual target remains available with autocast enabled.");
            Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,"Hero"));yield return null;
            var mana=hero.Rules.Mana;Call(binding,"ToggleSkillAutocast",6);Assert.That(rules.AutocastEnabled(6),Is.False,"Development-window RMB must change preference without casting.");
            Call(binding,"ToggleSkillAutocast",6);Assert.That(rules.AutocastEnabled(6),Is.True);Assert.That(hero.Rules.Mana,Is.EqualTo(mana));
            game.SetPaused(true);Call(binding,"ToggleSkillAutocast",6);Assert.That(rules.AutocastEnabled(6),Is.True);game.SetPaused(false);
            Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,"Inventory"));
            Call(binding,"ToggleSkillAutocast",6);Assert.That(rules.AutocastEnabled(6),Is.True,"Other modals must reject stale skill events.");yield return null;
        }
        [UnityTest]public IEnumerator InventoryAssignmentMakesF4UseTheChosenPotionAndClearMakesItSilent()
        {
            var shop=Read<Stage2Merchant>(binding,"Merchant");game.Gathering.Economy.Inventory.TryAddGold(100);
            var potion=shop.Items.First(i=>i.Type==Stage2ItemType.ManaPotion);Assert.That(shop.TryTrade(true,potion.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,"Inventory"));yield return null;
            Assert.That(Call(binding,"TryAssignQuickAccess",3,equipment.Rules.Items.Single(i=>i.Type==Stage2ItemType.ManaPotion).Id),Is.True);Assert.That(equipment.Rules.QuickAccessType(3),Is.EqualTo(Stage2ItemType.ManaPotion));
            Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,"None"));
            hero.Rules.TryUseMana(50,()=>true);input.Press(Keyboard.current.f4Key);yield return null;yield return null;input.Release(Keyboard.current.f4Key);yield return null;
            Assert.That(hero.Rules.Mana,Is.EqualTo(100));Assert.That(equipment.Rules.ConsumableCount(Stage2ItemType.ManaPotion),Is.Zero);
            Assert.That(rules.PotionCooldownRemaining,Is.GreaterThan(0));
            Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,"Inventory"));yield return null;
            Assert.That(Call(binding,"TryClearQuickAccess",3),Is.True);Assert.That(equipment.Rules.QuickAccessType(3),Is.Null);
        }

        [UnityTest]public IEnumerator RetryPreservesBindingsAutocastAndPaidCooldownsButClearsAreasAndResetClearsAll()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(4);rules.ToggleAutocast(1);
            var shop=Read<Stage2Merchant>(binding,"Merchant");game.Gathering.Economy.Inventory.TryAddGold(100);
            var potion=shop.Items.First(i=>i.Type==Stage2ItemType.ManaPotion);shop.TryTrade(true,potion.Id);
            var owned=equipment.Rules.Items.Single(i=>i.Type==Stage2ItemType.ManaPotion);equipment.Rules.TryAssignQuickAccess(3,owned.Id);
            hero.Rules.TryUseMana(50,()=>true);Assert.That(rules.TryUseActiveItem(equipment.Rules,Stage2ItemType.ManaPotion),Is.True);
            Assert.That(Stage3TestCycle.StartNight(game.RunDriver.Run),Is.True);
            var point=(Vector2)scene.PlayerMover.transform.position+Vector2.up*2;DurablePortal(point);
            Assert.That(Call(driver,"TryCast",4,point),Is.True);
            var presentation=game.GetComponent("Stage3HeroSkillPresentation");Assert.That(Read<int>(presentation,"ActivePersistentAreaCount"),Is.EqualTo(1));
            var cooldown=rules.Cooldown(4);var potionCooldown=rules.PotionCooldownRemaining;var rank=rules.Rank(4);
            for(var i=0;i<20&&!game.RunDriver.Run.IsTerminal;i++)
            {var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(game.RunDriver.Run.RetryFailedWave(),Is.True);
            Assert.That(equipment.Rules.QuickAccessType(3),Is.EqualTo(Stage2ItemType.ManaPotion));Assert.That(rules.AutocastEnabled(1),Is.True);
            Assert.That(rules.Cooldown(4),Is.EqualTo(cooldown));Assert.That(rules.PotionCooldownRemaining,Is.EqualTo(potionCooldown));Assert.That(rules.Rank(4),Is.EqualTo(rank));
            Assert.That(Read<int>(presentation,"ActivePersistentAreaCount"),Is.Zero);
            game.RunDriver.ResetRun();Assert.That(equipment.Rules.QuickAccessType(3),Is.Null);Assert.That(rules.AutocastEnabled(1),Is.False);
            Assert.That(rules.Rank(4),Is.Zero);Assert.That(rules.PotionCooldownRemaining,Is.Zero);yield return null;
        }

        [UnityTest]public IEnumerator SmallWorldXpBonusAccumulatesAndDoesNotIncreasePortalReward()
        {
            var requiredXp=0;
            for(var level=1;level<rules.RequiredLevel(33);level++)requiredXp+=hero.Catalog.RequiredXp(level);
            hero.Rules.AwardExperience(requiredXp);Learn(33);Call(driver,"Tick",0f);
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
        [UnityTest]public IEnumerator NewAdventurerPassivesAffectRealWorldHitsAndRecoveryWithoutPortalBonus()
        {
            hero.Rules.AwardExperience(int.MaxValue);Learn(35);Call(driver,"Tick",0f);
            var units=Read<System.Collections.Generic.IReadOnlyList<Stage2GuildEnemy>>(world,"Enemies");
            var total=0;
            for(var i=0;i<20;i++)
            {
                var unit=units.First(u=>u.Alive&&u.Health>=2);var before=unit.Health;
                Assert.That(Call(world,"Damage",unit,1),Is.True);total+=before-unit.Health;
            }
            Assert.That(total,Is.EqualTo(21),"The5%world bonus carries fractions even at starter damage1.");
            var portal=Portal((Vector2)scene.PlayerMover.transform.position+Vector2.up*2);
            var hp=portal.Health;
            Assert.That(game.Enemies.TryHitNearest(scene.PlayerMover.transform.position,5,1,0,0),Is.True);
            Assert.That(hp-portal.Health,Is.EqualTo(1),"World bonus does not multiply portal hits.");
            var recovery=game.GetComponent<Stage2HeroRecoveryDriver>();var health=hero.Rules.Health;var mana=hero.Rules.Mana;
            Assert.That(recovery.TryApplyExplorationDamage(20),Is.True);
            Assert.That(health-hero.Rules.Health,Is.EqualTo(19).Within(.001f));Assert.That(hero.Rules.Mana,Is.EqualTo(mana));
            game.RunDriver.ResetRun();yield return null;
            Assert.That(rules.Rank(34),Is.Zero);Assert.That(rules.Rank(35),Is.Zero);
            Assert.That(Read<float>(world,"WorldDamageBonus"),Is.Zero);
        }
    }
}
#endif
