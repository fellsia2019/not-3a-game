#if UNITY_EDITOR
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3ActiveActionsRulesTests
    {
        private Stage2Inventory wallet;
        private Stage2Equipment gear;
        private Stage2Merchant shop;
        private Stage2HeroProgression hero;
        private Stage2HeroSkills skills;
        [SetUp] public void Setup()
        {
            wallet=new Stage2Inventory();wallet.TryAddGold(100000);
            foreach(var kind in new[]{MapResourceKind.Wood,MapResourceKind.Stone,MapResourceKind.Metal})wallet.TryAdd(kind,100000);
            gear=new Stage2Equipment(Resources.Load<Stage2EquipmentCatalog>("Stage3Equipment"),wallet);
            gear.EnableBlueprintResearch(Resources.Load<Stage2BlueprintCatalog>("Stage3Blueprints"));
            shop=new Stage2Merchant(gear,Resources.Load<Stage2MerchantCatalog>("Stage3Merchant"));
            hero=new Stage2HeroProgression(Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression"));
            skills=new Stage2HeroSkills(hero,Resources.Load<Stage2HeroSkillCatalog>("Stage3HeroSkills"),gear);
        }
        private Stage2EquipmentItem Buy(Stage2ItemType type)
        {
            var item=shop.Items.First(i=>i.Type==type);Assert.That(shop.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
            return gear.Items.First(i=>i.Type==type);
        }
        private int BuyPaper(Stage2BlueprintKind kind)
        {var paper=shop.BlueprintItems.Single(p=>p.Kind==kind);Assert.That(shop.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.Ready));return paper.Id;}
        [Test] public void OnlyOwnedItemsWithActiveActionsCanBeAssignedAndClearingDoesNotDiscardThem()
        {
            var bow=gear.Items.Single(i=>i.Type==Stage2ItemType.Bow);
            Assert.That(gear.TryAssignQuickAccess(3,bow.Id),Is.False);
            Assert.That(gear.TryAssignQuickAccess(3,int.MaxValue),Is.False);
            var potion=Buy(Stage2ItemType.ManaPotion);var count=gear.Items.Count;
            Assert.That(gear.TryAssignQuickAccess(3,potion.Id),Is.True);
            Assert.That(gear.QuickAccessType(3),Is.EqualTo(Stage2ItemType.ManaPotion));
            Assert.That(gear.Items.Count,Is.EqualTo(count));
            Assert.That(gear.TryClearQuickAccess(3),Is.True);Assert.That(gear.QuickAccessType(3),Is.Null);
            Assert.That(gear.Find(potion.Id),Is.SameAs(potion));Assert.That(potion.Quantity,Is.EqualTo(1));
        }
        [Test] public void ReboundFourthSlotUsesItsActualTypeAndConsumptionSharesThePotionCooldown()
        {
            var mana=Buy(Stage2ItemType.ManaPotion);var mixed=Buy(Stage2ItemType.MixedPotion);
            Assert.That(gear.TryAssignQuickAccess(3,mana.Id),Is.True);Assert.That(hero.TryUseMana(50,()=>true),Is.True);
            Assert.That(skills.TryUseActiveItem(gear,gear.QuickAccessType(3).Value),Is.True);
            Assert.That(hero.Mana,Is.EqualTo(hero.MaximumMana));Assert.That(gear.ConsumableCount(Stage2ItemType.ManaPotion),Is.Zero);
            Assert.That(skills.TryUseActiveItem(gear,Stage2ItemType.MixedPotion),Is.False);Assert.That(mixed.Quantity,Is.EqualTo(1));
            skills.Tick(20,false);Assert.That(skills.PotionCooldownRemaining,Is.EqualTo(20));
            skills.Tick(20,true);Assert.That(skills.PotionCooldownRemaining,Is.Zero);
            Assert.That(skills.TryUseActiveItem(gear,Stage2ItemType.MixedPotion),Is.False,"Full vitals must not consume the item or start cooldown.");
            Assert.That(mixed.Quantity,Is.EqualTo(1));Assert.That(skills.PotionCooldownRemaining,Is.Zero);
        }
        [Test] public void NonconsumableItemWithActualCapabilityCanBeBoundAndActivatedWithoutConsumingIt()
        {
            // Explicit future-item capability fixture; no ordinary item gains this action.
            var catalog=Object.Instantiate(gear.Catalog);
            try
            {
                catalog.Get(Stage2ItemType.Pickaxe).activeAction=Stage2ItemActiveAction.RestoreMana;
                var equipped=new Stage2Equipment(catalog,wallet);var tool=equipped.Items.Single(i=>i.Type==Stage2ItemType.Pickaxe);
                Assert.That(equipped.TryAssignQuickAccess(3,tool.Id),Is.True);hero.TryUseMana(50,()=>true);
                Assert.That(skills.TryUseActiveItem(equipped,Stage2ItemType.Pickaxe),Is.True);
                Assert.That(hero.Mana,Is.EqualTo(hero.MaximumMana));Assert.That(equipped.Find(tool.Id),Is.SameAs(tool));
                Assert.That(tool.Quantity,Is.EqualTo(1));Assert.That(equipped.IsEquipped(tool),Is.True);
            }
            finally {Object.DestroyImmediate(catalog);}
        }
        [Test] public void FullResetRestoresDefaultBindingsWithoutCreatingConsumables()
        {
            var potion=Buy(Stage2ItemType.HealthPotion);gear.TryAssignQuickAccess(3,potion.Id);gear.TryClearQuickAccess(0);
            gear.Reset();Assert.That(gear.QuickAccessType(0),Is.EqualTo(Stage2ItemType.HealthPotion));
            Assert.That(gear.QuickAccessType(3),Is.Null);Assert.That(gear.Items.Count,Is.EqualTo(3));
            Assert.That(gear.Items.All(i=>!Stage2EquipmentCatalog.IsConsumable(i.Type)),Is.True);
        }
        [Test] public void OneStudiedMasteryPaperUnlocksEquipmentAndSkillsButCraftStillRequiresForgeTwo()
        {
            hero.AwardExperience(int.MaxValue);var staff=Buy(Stage2ItemType.Wand);var points=hero.AvailablePoints;
            Assert.That(gear.EvaluateEquip(staff.Id),Is.EqualTo(Stage2EquipmentResult.ResearchRequired));
            Assert.That(skills.TryUpgrade(6),Is.False);Assert.That(skills.TryUpgrade(23),Is.False);Assert.That(hero.AvailablePoints,Is.EqualTo(points));
            var paper=BuyPaper(Stage2BlueprintKind.WandRecipe);Assert.That(gear.StaffMasteryKnown,Is.False,"Buying is separate from studying.");
            Assert.That(gear.Blueprints.TryStudy(paper),Is.EqualTo(Stage2BlueprintResult.Ready));
            Assert.That(gear.StaffMasteryKnown,Is.True);Assert.That(gear.Blueprints.Find(paper),Is.Null);
            Assert.That(gear.TryEquip(staff.Id),Is.EqualTo(Stage2EquipmentResult.Ready));Assert.That(skills.TryUpgrade(6),Is.True);
            Assert.That(gear.ActivateForge(),Is.True);Assert.That(gear.EvaluateCraft(Stage2ItemType.Wand),Is.EqualTo(Stage2EquipmentResult.ForgeStageRequired));
            var upgrade=BuyPaper(Stage2BlueprintKind.ForgeII);Assert.That(gear.Blueprints.TryStudy(upgrade),Is.EqualTo(Stage2BlueprintResult.Ready));
            Assert.That(gear.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.Ready));Assert.That(gear.ForgeLevel,Is.EqualTo(2));
            Assert.That(gear.EvaluateCraft(Stage2ItemType.Wand),Is.EqualTo(Stage2EquipmentResult.Ready));
        }
        [Test] public void AutocastToggleRequiresASupportedLearnedSpellAndKeepsManaAndCooldownUntouched()
        {
            hero.AwardExperience(int.MaxValue);Assert.That(skills.ToggleAutocast(1),Is.False);
            Assert.That(skills.TryUpgrade(1),Is.True);var mana=hero.Mana;
            Assert.That(skills.ToggleAutocast(1),Is.True);Assert.That(skills.AutocastEnabled(1),Is.True);
            Assert.That(skills.ToggleAutocast(2),Is.False);Assert.That(hero.Mana,Is.EqualTo(mana));Assert.That(skills.Cooldown(1),Is.Zero);
            Assert.That(skills.ToggleAutocast(1),Is.True);Assert.That(skills.AutocastEnabled(1),Is.False);
            skills.ToggleAutocast(1);skills.Reset();Assert.That(skills.AutocastEnabled(1),Is.False);
        }
        [Test] public void VulnerabilitiesAreSchoolSpecificRefreshWithoutStackingAndExpire()
        {
            var status=new Stage2CombatStatuses();status.Weaken(Stage2DamageSchool.Physical,.2f,3);
            Assert.That(status.AdjustDamage(10,Stage2DamageSchool.Physical),Is.EqualTo(12));
            Assert.That(status.AdjustDamage(10,Stage2DamageSchool.Magic),Is.EqualTo(10));
            status.Weaken(Stage2DamageSchool.Physical,.2f,3);Assert.That(status.AdjustDamage(10,Stage2DamageSchool.Physical),Is.EqualTo(12));
            status.Weaken(Stage2DamageSchool.Magic,.3f,1);Assert.That(status.AdjustDamage(10,Stage2DamageSchool.Magic),Is.EqualTo(13));
            status.Tick(1);Assert.That(status.MagicVulnerability,Is.Zero);Assert.That(status.PhysicalVulnerability,Is.EqualTo(.2f));
            status.Tick(2);Assert.That(status.AdjustDamage(10,Stage2DamageSchool.Physical),Is.EqualTo(10));
        }
        [Test] public void StarterDamageRetainsFractionalSchoolVulnerabilityWithoutIncreasingEveryHit()
        {
            var status=new Stage2CombatStatuses();status.Weaken(Stage2DamageSchool.Physical,.2f,3);
            var sum=0;for(var i=0;i<5;i++)sum+=status.AdjustDamage(1,Stage2DamageSchool.Physical);
            Assert.That(sum,Is.EqualTo(6));Assert.That(status.AdjustDamage(1,Stage2DamageSchool.Magic),Is.EqualTo(1));
        }
        [Test] public void FreezeEndsBeforeTheLongerSlowAndStatusResetClearsEveryEffect()
        {
            var status=new Stage2CombatStatuses();status.Freeze(1);status.ApplySlow(.65f,3);status.MarkPoisoned(3);
            status.Tick(.5f);Assert.That(status.Immobilized,Is.True);status.Tick(.5f);Assert.That(status.Immobilized,Is.False);
            Assert.That(status.SlowMultiplier,Is.EqualTo(.65f));status.Stun(1);Assert.That(status.Immobilized,Is.True);
            status.Reset();Assert.That(status.Immobilized,Is.False);Assert.That(status.SlowMultiplier,Is.EqualTo(1));Assert.That(status.PoisonRemaining,Is.Zero);
        }
    }
}
#endif
