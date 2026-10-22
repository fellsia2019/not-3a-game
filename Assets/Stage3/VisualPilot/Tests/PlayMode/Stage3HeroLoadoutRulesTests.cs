#if UNITY_EDITOR
using System;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroLoadoutRulesTests
    {
        private Stage2HeroProgression hero;
        private Stage2HeroSkills skills;
        private Stage2Equipment gear;
        private Stage2Inventory wallet;
        private Stage2Merchant shop;
        [SetUp] public void Setup()
        {
            hero=new Stage2HeroProgression(Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression"));
            skills=new Stage2HeroSkills(hero,Resources.Load<Stage2HeroSkillCatalog>("Stage3HeroSkills"));
            wallet=new Stage2Inventory();wallet.TryAddGold(100000);
            gear=new Stage2Equipment(Resources.Load<Stage2EquipmentCatalog>("Stage3Equipment"),wallet);
            shop=new Stage2Merchant(gear,Resources.Load<Stage2MerchantCatalog>("Stage3Merchant"));
        }
        private void MaxLevel()=>hero.AwardExperience(int.MaxValue);
        private void Learn(int id){var parent=skills.Catalog.Get(id).prerequisite;if(parent>=0&&skills.Rank(parent)==0)Learn(parent);Assert.That(skills.TryUpgrade(id),Is.True,"learn"+id);}
        [Test] public void CatalogHasExactElevenActiveTwentyPassiveAndBoundedSplash()
        {
            Assert.That(skills.Catalog.TryValidate(out var reason),Is.True,reason);
            Assert.That(Enumerable.Range(0,31).Count(i=>skills.Catalog.Get(i).active),Is.EqualTo(11));
            foreach(var id in new[]{9,10})
            {var n=skills.Catalog.Get(id);Assert.That(n.maxTargets,Is.LessThanOrEqualTo(4));Assert.That(n.radius,Is.LessThanOrEqualTo(2));Assert.That(n.cooldown.Min(),Is.GreaterThanOrEqualTo(15));}
        }
        [Test] public void ProgressionGatesSpendActualHeroPointsAndBranchesDoNotRequireEveryActive()
        {
            Assert.That(skills.TryUpgrade(1),Is.False);MaxLevel();var points=hero.AvailablePoints;
            Learn(1);Learn(3);Learn(5);
            Assert.That(skills.Rank(2),Is.Zero);Assert.That(hero.AvailablePoints,Is.EqualTo(points-3));
            Assert.That(skills.TryUpgrade(14),Is.False);Learn(13);Learn(14);
        }
        [Test] public void ExistingThreeRanksRemainSharedAndUseOriginalPointPool()
        {
            MaxLevel();
            for(var i=0;i<3;i++){hero.TryUpgradePassive((Stage2HeroPassive)i);Assert.That(skills.Rank(11+i),Is.EqualTo(1));}
            var points=hero.AvailablePoints;Assert.That(skills.TryUpgrade(11),Is.True);
            Assert.That(hero.PassiveRank(Stage2HeroPassive.PreciseStrike),Is.EqualTo(2));Assert.That(hero.AvailablePoints,Is.EqualTo(points-1));
        }
        [Test] public void WrongWeaponAbsentWeaponAndInvalidEffectSpendNothing()
        {
            MaxLevel();Learn(6);var mana=hero.Mana;
            Assert.That(skills.TryExecute(6,0,true,()=>true),Is.False);
            Assert.That(skills.TryExecute(6,1,false,()=>true),Is.False);
            Assert.That(skills.TryExecute(6,1,true,()=>false),Is.False);
            Assert.That(hero.Mana,Is.EqualTo(mana));Assert.That(skills.Cooldown(6),Is.Zero);
        }
        [Test] public void SkillPurchasePublishesCommittedRankAndRejectsReentrantPurchase()
        {
            MaxLevel();var points=hero.AvailablePoints;var nested=true;var observed=0;
            hero.Changed+=()=>{observed=skills.Rank(1);nested=skills.TryUpgrade(1);};
            Assert.That(skills.TryUpgrade(1),Is.True);Assert.That(observed,Is.EqualTo(1));
            Assert.That(nested,Is.False);Assert.That(hero.AvailablePoints,Is.EqualTo(points-1));
        }
        [Test] public void PotionObserverCannotConsumeAnotherBottleBeforeCooldownCommit()
        {
            var item=shop.Items.First(i=>i.Type==Stage2ItemType.MixedPotion);shop.TryTrade(true,item.Id);shop.TryTrade(true,item.Id);
            new Stage2HeroRecovery(hero,gear).TryApplyExplorationDamage(50,Vector2.zero);
            hero.SetAdditionalMana(300,1);hero.TryBlink(Vector2.zero,Vector2.one,_=>true,out _);
            var nested=true;gear.Changed+=()=>nested=skills.TryUsePotion(gear,Stage2ItemType.MixedPotion);
            Assert.That(skills.TryUsePotion(gear,Stage2ItemType.MixedPotion),Is.True);
            Assert.That(nested,Is.False);Assert.That(gear.ConsumableCount(Stage2ItemType.MixedPotion),Is.EqualTo(1));
        }
        [Test] public void DamageAmuletRetainsFractionalBonusAcrossShotsAndSnapshot()
        {
            var total=0;var data=ScriptableObject.CreateInstance<Stage2HeroCombatCatalog>();var combat=new Stage2HeroCombat(data,(o,r,d,s,f)=>{total+=d;return true;});
            combat.SetEquippedWeapon(true,2,5,1,0);combat.SetEquipmentDamageMultiplier(1.05f);
            for(var i=0;i<10;i++)combat.Tick(1,Vector2.zero,true);
            Assert.That(total,Is.EqualTo(21));
            var checkpoint=combat.CaptureSnapshot();combat.Tick(1,Vector2.zero,true);combat.RestoreSnapshot(checkpoint);
            Assert.That(combat.ShotsFired,Is.EqualTo(10));
            UnityEngine.Object.DestroyImmediate(data);
        }
        [Test] public void ActualCastSpendsOnceCooldownPausesAndWtfHasZeroCostAndCooldown()
        {
            MaxLevel();Learn(6);var mana=hero.Mana;
            Assert.That(skills.TryExecute(6,1,true,()=>true),Is.True);Assert.That(hero.Mana,Is.EqualTo(mana-skills.ManaCost(6)));
            var cd=skills.Cooldown(6);skills.Tick(2,false);Assert.That(skills.Cooldown(6),Is.EqualTo(cd));
            Assert.That(skills.TryExecute(6,1,true,()=>true),Is.False);
            hero.SetWtfForTesting(true);skills.Tick(0,true);var after=hero.Mana;
            Assert.That(skills.TryExecute(6,1,true,()=>true),Is.True);Assert.That(skills.TryExecute(6,1,true,()=>true),Is.True);
            Assert.That(hero.Mana,Is.EqualTo(after));Assert.That(skills.Cooldown(6),Is.Zero);
        }
        [Test] public void SwitchingKeepsBothInstancesEquippedAndCannotSellEither()
        {
            var bow=gear.ActiveWeapon;gear.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.D,7,out var staff);
            gear.TryEquip(staff.Id);Assert.That(gear.ActiveWeapon,Is.SameAs(staff));
            Assert.That(gear.IsEquipped(bow)&&gear.IsEquipped(staff),Is.True);
            Assert.That(gear.TrySwitchWeapon(),Is.True);Assert.That(gear.ActiveWeapon,Is.SameAs(bow));
            Assert.That(shop.TryTrade(false,staff.Id),Is.EqualTo(Stage2TradeResult.EquippedItem));
            Assert.That(shop.TryTrade(false,bow.Id),Is.EqualTo(Stage2TradeResult.EquippedItem));
        }
        [Test] public void WeaponSwitchPreservesSpellCooldownAndMana()
        {
            MaxLevel();Learn(6);skills.TryExecute(6,1,true,()=>true);
            gear.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.F,1,out var staff);gear.TryEquip(staff.Id);
            var cd=skills.Cooldown(6);var mana=hero.Mana;gear.TrySwitchWeapon();gear.TrySwitchWeapon();
            Assert.That(skills.Cooldown(6),Is.EqualTo(cd));Assert.That(hero.Mana,Is.EqualTo(mana));
        }
        [Test] public void PotionTradeMovesOneOfFiveAndUsesOwnerPrices()
        {
            foreach(var pair in new[]{(Stage2ItemType.HealthPotion,100),(Stage2ItemType.ManaPotion,100),(Stage2ItemType.MixedPotion,250)})
            {
                var item=shop.Items.First(i=>i.Type==pair.Item1);Assert.That(item.Quantity,Is.EqualTo(5));var gold=wallet.Gold;
                Assert.That(shop.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
                Assert.That(item.Quantity,Is.EqualTo(4));Assert.That(gear.ConsumableCount(pair.Item1),Is.EqualTo(1));
                Assert.That(wallet.Gold,Is.EqualTo(gold-pair.Item2));
            }
        }
        [Test] public void NoNeedPotionDoesNotConsumeAndAllPotionsShareCooldown()
        {
            var hp=shop.Items.First(i=>i.Type==Stage2ItemType.HealthPotion);shop.TryTrade(true,hp.Id);
            Assert.That(skills.TryUsePotion(gear,Stage2ItemType.HealthPotion),Is.False);Assert.That(gear.ConsumableCount(Stage2ItemType.HealthPotion),Is.EqualTo(1));
            var recovery=new Stage2HeroRecovery(hero,gear);recovery.TryApplyExplorationDamage(50,Vector2.zero);
            Assert.That(skills.TryUsePotion(gear,Stage2ItemType.HealthPotion),Is.True);
            Assert.That(hero.Health,Is.EqualTo(100));Assert.That(gear.ConsumableCount(Stage2ItemType.HealthPotion),Is.Zero);
            var mp=shop.Items.First(i=>i.Type==Stage2ItemType.ManaPotion);shop.TryTrade(true,mp.Id);hero.TryBlink(Vector2.zero,Vector2.one,_=>true,out _);
            Assert.That(skills.TryUsePotion(gear,Stage2ItemType.ManaPotion),Is.False);
            skills.Tick(20,true);Assert.That(skills.TryUsePotion(gear,Stage2ItemType.ManaPotion),Is.True);Assert.That(hero.Mana,Is.EqualTo(100));
        }
        [Test] public void MixedPotionRestoresBothOneHundredAndDeathDropsOnlyBag()
        {
            var item=shop.Items.First(i=>i.Type==Stage2ItemType.MixedPotion);shop.TryTrade(true,item.Id);shop.TryTrade(true,item.Id);
            var recovery=new Stage2HeroRecovery(hero,gear);recovery.TryApplyExplorationDamage(90,Vector2.one);
            hero.TryBlink(Vector2.zero,Vector2.one,_=>true,out _);Assert.That(skills.TryUsePotion(gear,Stage2ItemType.MixedPotion),Is.True);
            Assert.That(hero.Health,Is.EqualTo(100));Assert.That(hero.Mana,Is.EqualTo(100));
            gear.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.F,1,out var staff);gear.TryEquip(staff.Id);
            recovery.TryApplyExplorationDamage(100,Vector2.one);
            Assert.That(gear.ConsumableCount(Stage2ItemType.MixedPotion),Is.Zero);Assert.That(gear.IsEquipped(staff),Is.True);
            Assert.That(recovery.TryRecover(recovery.Drops[0].Id),Is.True);Assert.That(gear.ConsumableCount(Stage2ItemType.MixedPotion),Is.EqualTo(1));
        }
        [Test] public void ProtectionIsBoundedAndCostsNoManaBarrierAbsorbsOnlyReducedDamage()
        {
            MaxLevel();Learn(7);skills.ApplyBarrier();var mana=hero.Mana;var before=skills.Barrier;
            var expected=Mathf.Max(0,20*.92f-before);
            Assert.That(skills.PreviewDamage(20,.08f),Is.EqualTo(expected).Within(.001f));
            Assert.That(skills.AbsorbDamage(20,.08f),Is.EqualTo(expected).Within(.001f));Assert.That(hero.Mana,Is.EqualTo(mana));
            Assert.That(skills.PreviewDamage(100,1),Is.GreaterThanOrEqualTo(35));
        }
        [Test] public void TwoRingsOccupyDifferentSlotsAndRemovingOneKeepsOther()
        {
            gear.TryReceiveLoot(Stage2ItemType.Ring,Stage2ItemQuality.F,1,out var a);gear.TryReceiveLoot(Stage2ItemType.Ring,Stage2ItemQuality.D,20,out var b);
            gear.TryEquip(a.Id);gear.TryEquip(b.Id);
            Assert.That(gear.EquippedAt(Stage2ItemSlot.RingLeft),Is.SameAs(a));Assert.That(gear.EquippedAt(Stage2ItemSlot.RingRight),Is.SameAs(b));
            gear.TryUnequipAt(Stage2ItemSlot.RingLeft);Assert.That(gear.IsEquipped(b),Is.True);Assert.That(gear.IsEquipped(a),Is.False);
        }
        [Test] public void QuestBonusesCreditOnlyOnceAtTurnInAndOverflowPreservesContract()
        {
            var data=ScriptableObject.CreateInstance<Stage2GuildCatalog>();var guild=new Stage2Guild(data,wallet,hero);
            var quest=data.GetAt(0);guild.SetRewardBonuses(.15f,.15f);
            Assert.That(guild.TryAccept(quest.id,_=>new Stage2GuildSpawn{Generation=1,EnemyIds=Enumerable.Range(1,quest.population).ToArray()}),Is.True);
            for(var i=1;i<=quest.population;i++)guild.RecordKill(quest.enemyKind,1,i);
            var gold=wallet.Gold;var xp=0;hero.ExperienceGained+=amount=>xp=amount;
            Assert.That(guild.TryTurnIn(quest.id),Is.True);Assert.That(wallet.Gold-gold,Is.EqualTo(guild.RewardGold(quest)));
            Assert.That(xp,Is.EqualTo(guild.RewardExperience(quest)));Assert.That(guild.TryTurnIn(quest.id),Is.False);
            Assert.That(guild.TryAccept(quest.id,_=>new Stage2GuildSpawn{Generation=2,EnemyIds=Enumerable.Range(100,quest.population).ToArray()}),Is.True);
            for(var i=100;i<100+quest.population;i++)guild.RecordKill(quest.enemyKind,2,i);
            wallet.Reset(0);wallet.TryAddGold(int.MaxValue-quest.gold);
            Assert.That(guild.Evaluate(quest.id),Is.EqualTo(Stage2GuildAction.GoldLimit));Assert.That(guild.TryTurnIn(quest.id),Is.False);
            Assert.That(guild.FindActive(quest.id),Is.Not.Null);UnityEngine.Object.DestroyImmediate(data);
        }
        [Test] public void PotionExhaustionAndSaleBuybackNeverDuplicateStock()
        {
            var item=shop.Items.First(i=>i.Type==Stage2ItemType.HealthPotion);
            for(var i=0;i<5;i++)Assert.That(shop.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(shop.Find(item.Id),Is.Null);Assert.That(gear.ConsumableCount(item.Type),Is.EqualTo(5));
            Assert.That(shop.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.InvalidItem));
            var bag=gear.Items.First(i=>i.Type==item.Type);shop.TryTrade(false,bag.Id);
            Assert.That(gear.ConsumableCount(item.Type),Is.EqualTo(4));Assert.That(shop.Items.First(i=>i.Type==item.Type).Quantity,Is.EqualTo(1));
        }
    }
}
#endif
