#if UNITY_EDITOR
using System;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3BlueprintRulesTests
    {
        private Stage2EquipmentCatalog equipmentCatalog;
        private Stage2MerchantCatalog merchantCatalog;
        private Stage2BlueprintCatalog catalog;
        private Stage2Inventory wallet;
        private Stage2Equipment equipment;
        private Stage2Merchant merchant;
        private Stage2Blueprints Rules=>equipment.Blueprints;
        [SetUp] public void Setup()
        {
            equipmentCatalog=ScriptableObject.CreateInstance<Stage2EquipmentCatalog>();
            merchantCatalog=ScriptableObject.CreateInstance<Stage2MerchantCatalog>();
            catalog=ScriptableObject.CreateInstance<Stage2BlueprintCatalog>();wallet=new Stage2Inventory();
            wallet.TryAddGold(10000);
            foreach(MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind)))wallet.TryAdd(kind,10000);
            equipment=new Stage2Equipment(equipmentCatalog,wallet);equipment.EnableBlueprintResearch(catalog);
            merchant=new Stage2Merchant(equipment,merchantCatalog);
        }
        [TearDown] public void Cleanup()
        {Object.DestroyImmediate(equipmentCatalog);Object.DestroyImmediate(merchantCatalog);Object.DestroyImmediate(catalog);}
        private int[] Stock()=>new[]{wallet.Get(MapResourceKind.Wood),wallet.Get(MapResourceKind.Stone),wallet.Get(MapResourceKind.Metal),wallet.Gold};
        private Stage2BlueprintItem Buy(Stage2BlueprintKind kind,Stage2Merchant shop=null)
        {
            shop=shop??merchant;var item=shop.BlueprintItems.Single(p=>p.Kind==kind);
            Assert.That(shop.TryTradeBlueprint(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));return item;
        }
        [Test] public void DocumentsAreSeparateFromEquipmentAndUseOneUniqueIdentitySpace()
        {
            Assert.That(catalog.TryValidate(out var reason),Is.True,reason);
            Assert.That(merchant.BlueprintItems.Select(i=>i.Kind),Is.EquivalentTo(Enum.GetValues(typeof(Stage2BlueprintKind))));
            var ids=merchant.Items.Select(i=>i.Id).Concat(equipment.Items.Select(i=>i.Id)).Concat(merchant.BlueprintItems.Select(i=>i.Id)).ToArray();
            Assert.That(ids.Distinct().Count(),Is.EqualTo(ids.Length));
            Assert.That(typeof(Stage2BlueprintItem).GetProperty("Quality"),Is.Null);
            Assert.That(typeof(Stage2BlueprintItem).GetProperty("Level"),Is.Null);
            Assert.That(catalog.Get(Stage2BlueprintKind.ForgeII).TargetStage,Is.EqualTo(2));
            Assert.That(catalog.Get(Stage2BlueprintKind.WandRecipe).TargetStage,Is.Zero);
        }
        [Test] public void BuySellAndBuybackMoveSamePaperAndSpendOnlyGoldAtHalfSaleValue()
        {
            var before=Stock();var paper=Buy(Stage2BlueprintKind.ForgeII);var price=catalog.Get(paper.Kind).goldPrice;
            Assert.That(Rules.Find(paper.Id),Is.SameAs(paper));Assert.That(merchant.FindBlueprint(paper.Id),Is.Null);
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.InvalidItem));
            Assert.That(equipment.TryEquip(paper.Id),Is.EqualTo(Stage2EquipmentResult.InvalidItem));
            Assert.That(merchant.TryTradeBlueprint(false,paper.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(merchant.FindBlueprint(paper.Id),Is.SameAs(paper));Assert.That(Rules.Find(paper.Id),Is.Null);
            Assert.That(Stock(),Is.EqualTo(new[]{before[0],before[1],before[2],before[3]-price+price/2}));
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(Rules.Find(paper.Id),Is.SameAs(paper));
        }
        [Test] public void FailedPaymentAndGoldOverflowPreservePaperOwnership()
        {
            wallet.Reset(0);var paper=merchant.BlueprintItems[0];
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.InsufficientGold));
            Assert.That(merchant.FindBlueprint(paper.Id),Is.SameAs(paper));Assert.That(Rules.Items,Is.Empty);
            wallet.TryAddGold(1000);Buy(paper.Kind);wallet.Reset(int.MaxValue);
            Assert.That(merchant.TryTradeBlueprint(false,paper.Id),Is.EqualTo(Stage2TradeResult.GoldLimit));
            Assert.That(Rules.Find(paper.Id),Is.SameAs(paper));Assert.That(wallet.Gold,Is.EqualTo(int.MaxValue));
        }
        [Test] public void WalletObserversSeeCommittedPaperAndCannotReenterAnyItemFamilyOrReset()
        {
            var paper=merchant.BlueprintItems[0];var other=merchant.Items[0];var observed=false;
            var nested=Stage2TradeResult.Ready;var study=Stage2BlueprintResult.Ready;var equip=Stage2EquipmentResult.Ready;
            wallet.Changed+=()=>
            {
                observed=ReferenceEquals(Rules.Find(paper.Id),paper)&&merchant.FindBlueprint(paper.Id)==null;
                nested=merchant.TryTrade(true,other.Id);study=Rules.TryStudy(paper.Id);equip=equipment.TryEquip(equipment.Items[0].Id);
                equipment.Reset();merchant.Reset();
            };
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(observed,Is.True);Assert.That(nested,Is.EqualTo(Stage2TradeResult.Busy));
            Assert.That(study,Is.EqualTo(Stage2BlueprintResult.Busy));Assert.That(equip,Is.EqualTo(Stage2EquipmentResult.Busy));
            Assert.That(Rules.Find(paper.Id),Is.SameAs(paper));Assert.That(merchant.Find(other.Id),Is.SameAs(other));
        }
        [Test] public void StudyConsumesOnePaperAndDuplicateKnownDocumentCannotBeConsumedAgain()
        {
            var paper=Buy(Stage2BlueprintKind.Longshot);var duplicateShop=new Stage2Merchant(equipment,merchantCatalog);
            var duplicate=Buy(paper.Kind,duplicateShop);var stock=Stock();
            Assert.That(Rules.TryStudy(paper.Id),Is.EqualTo(Stage2BlueprintResult.Ready));
            Assert.That(Rules.Find(paper.Id),Is.Null);Assert.That(Rules.IsStudied(paper.Kind),Is.True);Assert.That(Rules.StudiedCount,Is.EqualTo(1));
            Assert.That(Rules.TryStudy(paper.Id),Is.EqualTo(Stage2BlueprintResult.InvalidItem));
            Assert.That(Rules.TryStudy(duplicate.Id),Is.EqualTo(Stage2BlueprintResult.AlreadyStudied));
            Assert.That(Rules.Find(duplicate.Id),Is.SameAs(duplicate));Assert.That(Stock(),Is.EqualTo(stock));
            Assert.That(merchant.TryTradeBlueprint(false,duplicate.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(Rules.IsStudied(paper.Kind),Is.True,"Selling a duplicate cannot erase knowledge.");
        }
        [Test] public void ForgeThirdKnowledgeRequiresSecondKnowledgeButDoesNotUpgradeTheBuilding()
        {
            var third=Buy(Stage2BlueprintKind.ForgeIII);var second=Buy(Stage2BlueprintKind.ForgeII);
            Assert.That(Rules.TryStudy(third.Id),Is.EqualTo(Stage2BlueprintResult.PreviousStudyRequired));
            Assert.That(Rules.Find(third.Id),Is.SameAs(third));Assert.That(Rules.StudiedCount,Is.Zero);
            Assert.That(Rules.TryStudy(second.Id),Is.EqualTo(Stage2BlueprintResult.Ready));
            Assert.That(Rules.TryStudy(third.Id),Is.EqualTo(Stage2BlueprintResult.Ready));
            Assert.That(equipment.ForgeLevel,Is.Zero);Assert.That(Rules.Items,Is.Empty);Assert.That(Rules.StudiedCount,Is.EqualTo(2));
            equipment.ActivateForge();var stock=Stock();
            Assert.That(equipment.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.Ready));Assert.That(equipment.ForgeLevel,Is.EqualTo(2));
            Assert.That(equipment.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.Ready));Assert.That(equipment.ForgeLevel,Is.EqualTo(3));
            Assert.That(equipment.MaximumQuality,Is.EqualTo(Stage2ItemQuality.S));Assert.That(Stock(),Is.EqualTo(stock));
            Assert.That(equipment.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.MaximumForgeStage));
        }
        [Test] public void GenericLegacyScrollsCannotBypassStudiedForgeBlueprint()
        {
            equipment.ActivateForge();equipment.TryReceiveForgeScrolls(20);
            Assert.That(equipment.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.ResearchRequired));
            var second=Buy(Stage2BlueprintKind.ForgeII);Rules.TryStudy(second.Id);
            Assert.That(equipment.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.Ready));
            Assert.That(equipment.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.ResearchRequired));
            Assert.That(equipment.ForgeUpgradeScrolls,Is.EqualTo(20));
        }
        [Test] public void MasteryGatesEquipmentAndForgeTwoGatesStaffCrafting()
        {
            equipment.ActivateForge();var before=Stock();
            Assert.That(equipment.TryCraft(Stage2ItemType.Wand,out var created),Is.EqualTo(Stage2EquipmentResult.ResearchRequired));
            Assert.That(created,Is.Null);Assert.That(Stock(),Is.EqualTo(before));
            var bought=merchant.Items.Single(i=>i.Type==Stage2ItemType.Wand);
            Assert.That(merchant.TryTrade(true,bought.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(equipment.TryEquip(bought.Id),Is.EqualTo(Stage2EquipmentResult.ResearchRequired));
            Rules.TryStudy(Buy(Stage2BlueprintKind.WandRecipe).Id);
            Assert.That(equipment.TryEquip(bought.Id),Is.EqualTo(Stage2EquipmentResult.Ready));
            Assert.That(equipment.EvaluateCraft(Stage2ItemType.Wand),Is.EqualTo(Stage2EquipmentResult.ForgeStageRequired));
            Rules.TryStudy(Buy(Stage2BlueprintKind.ForgeII).Id);equipment.TryUpgradeForge();before=Stock();
            Assert.That(equipment.TryCraft(Stage2ItemType.Wand,out created),Is.EqualTo(Stage2EquipmentResult.Ready));
            var cost=equipmentCatalog.CraftCost(Stage2ItemType.Wand);
            Assert.That(Stock(),Is.EqualTo(new[]{before[0]-cost.Wood,before[1]-cost.Stone,before[2]-cost.Metal,before[3]-cost.Gold}));
            Assert.That(equipment.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(bought));
        }
        [Test] public void FullResetClearsKnowledgeAndPaperAndRestocksFreshNonRecycledIdentities()
        {
            var old=merchant.BlueprintItems.Select(i=>i.Id).ToArray();var paper=Buy(Stage2BlueprintKind.Longshot);Rules.TryStudy(paper.Id);
            Buy(Stage2BlueprintKind.ForgeII);equipment.Reset();merchant.Reset();
            Assert.That(Rules.StudiedCount,Is.Zero);Assert.That(Rules.Items,Is.Empty);Assert.That(merchant.BlueprintItems.Count,Is.EqualTo(catalog.Count));
            Assert.That(merchant.BlueprintItems.Select(i=>i.Id).Intersect(old),Is.Empty);
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.InvalidItem));
        }
    }
}
#endif
