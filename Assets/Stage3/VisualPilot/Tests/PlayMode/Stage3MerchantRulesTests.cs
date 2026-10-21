#if UNITY_EDITOR
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3MerchantRulesTests
    {
        private Stage2EquipmentCatalog equipmentCatalog;
        private Stage2MerchantCatalog catalog;
        private Stage2Inventory wallet;
        private Stage2Equipment equipment;
        private Stage2Merchant merchant;
        [SetUp] public void Setup()
        {
            equipmentCatalog=ScriptableObject.CreateInstance<Stage2EquipmentCatalog>();
            catalog=ScriptableObject.CreateInstance<Stage2MerchantCatalog>();wallet=new Stage2Inventory();
            wallet.TryAddGold(100000);wallet.TryAdd(MapResourceKind.Wood,37);wallet.TryAdd(MapResourceKind.Stone,29);wallet.TryAdd(MapResourceKind.Metal,19);
            equipment=new Stage2Equipment(equipmentCatalog,wallet);merchant=new Stage2Merchant(equipment,catalog);
        }
        [TearDown] public void Cleanup(){Object.DestroyImmediate(equipmentCatalog);Object.DestroyImmediate(catalog);}
        private int[] Materials()=>new[]{wallet.Get(MapResourceKind.Wood),wallet.Get(MapResourceKind.Stone),wallet.Get(MapResourceKind.Metal)};
        [Test] public void InitialStockIsFiniteEightF1TypesAndNeverOverlapsPlayerOwnership()
        {
            Assert.That(catalog.TryValidate(out var reason),Is.True,reason);
            Assert.That(merchant.Items.Select(i=>i.Type),Is.EquivalentTo(System.Enum.GetValues(typeof(Stage2ItemType))));
            Assert.That(merchant.Items.All(i=>i.Level==1&&i.Quality==Stage2ItemQuality.F),Is.True);
            Assert.That(merchant.Items.Select(i=>i.Id).Intersect(equipment.Items.Select(i=>i.Id)),Is.Empty);
            Assert.That(merchant.Items.Select(i=>i.Id).Distinct().Count(),Is.EqualTo(8));
        }
        [Test] public void PurchaseMovesExactlyTheSameInstanceOnceAndSpendsOnlyGold()
        {
            var item=merchant.Items[0];var price=catalog.BuyPrice(item);var gold=wallet.Gold;var materials=Materials();
            Assert.That(merchant.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(equipment.Find(item.Id),Is.SameAs(item));Assert.That(merchant.Find(item.Id),Is.Null);
            Assert.That(equipment.IsEquipped(item),Is.False);Assert.That(wallet.Gold,Is.EqualTo(gold-price));Assert.That(Materials(),Is.EqualTo(materials));
            Assert.That(merchant.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.InvalidItem));
            Assert.That(wallet.Gold,Is.EqualTo(gold-price));Assert.That(equipment.Items.Count,Is.EqualTo(4));
        }
        [Test] public void EquippedItemCannotBeSoldAndSaleBuybackKeepQualityLevelAndIdentity()
        {
            var equipped=equipment.Items[0];var gold=wallet.Gold;
            Assert.That(merchant.TryTrade(false,equipped.Id),Is.EqualTo(Stage2TradeResult.EquippedItem));Assert.That(wallet.Gold,Is.EqualTo(gold));
            equipment.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.SSS,100,out var item);
            var sale=catalog.SellPrice(item);var buy=catalog.BuyPrice(item);
            Assert.That(sale,Is.EqualTo(buy/2));
            Assert.That(merchant.TryTrade(false,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(equipment.Find(item.Id),Is.Null);Assert.That(merchant.Find(item.Id),Is.SameAs(item));
            Assert.That(merchant.TryTrade(false,item.Id),Is.EqualTo(Stage2TradeResult.InvalidItem));
            Assert.That(wallet.Gold,Is.EqualTo(gold+sale));
            Assert.That(merchant.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(equipment.Find(item.Id),Is.SameAs(item));Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.SSS));Assert.That(item.Level,Is.EqualTo(100));
            Assert.That(wallet.Gold,Is.EqualTo(gold+sale-buy));
        }
        [Test] public void MissingGoldAndCreditOverflowCannotLoseItemsOrAlterOtherResources()
        {
            wallet.Reset(0);var item=merchant.Items[0];var count=merchant.Items.Count;
            Assert.That(merchant.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.InsufficientGold));
            Assert.That(merchant.Find(item.Id),Is.SameAs(item));Assert.That(merchant.Items.Count,Is.EqualTo(count));
            equipment.TryReceiveLoot(Stage2ItemType.Helmet,Stage2ItemQuality.F,1,out var owned);wallet.TryAddGold(int.MaxValue);
            Assert.That(merchant.TryTrade(false,owned.Id),Is.EqualTo(Stage2TradeResult.GoldLimit));
            Assert.That(equipment.Find(owned.Id),Is.SameAs(owned));Assert.That(merchant.Find(owned.Id),Is.Null);Assert.That(wallet.Gold,Is.EqualTo(int.MaxValue));
        }
        [Test] public void WalletObserversSeeCommittedOwnershipAndCannotReenterItemTransactions()
        {
            var item=merchant.Items[0];var other=merchant.Items[1];var before=wallet.Gold;
            var nested=Stage2TradeResult.Ready;var equipNested=Stage2EquipmentResult.Ready;var observed=false;
            wallet.Changed+=()=>
            {
                observed=ReferenceEquals(equipment.Find(item.Id),item)&&merchant.Find(item.Id)==null&&wallet.Gold==before-catalog.BuyPrice(item);
                nested=merchant.TryTrade(true,other.Id);equipNested=equipment.TryEquip(item.Id);
            };
            Assert.That(merchant.TryTrade(true,item.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Assert.That(observed,Is.True);Assert.That(nested,Is.EqualTo(Stage2TradeResult.Busy));Assert.That(equipNested,Is.EqualTo(Stage2EquipmentResult.Busy));
            Assert.That(merchant.Find(other.Id),Is.SameAs(other));Assert.That(equipment.Items.Count,Is.EqualTo(4));
        }
        [Test] public void FullResetCreatesFreshFiniteStockWithoutRecyclingSoldOrPurchasedIds()
        {
            var old=merchant.Items.Select(i=>i.Id).ToArray();var purchased=merchant.Items[0];merchant.TryTrade(true,purchased.Id);
            equipment.Reset();merchant.Reset();
            Assert.That(merchant.Items.Count,Is.EqualTo(8));Assert.That(equipment.Items.Count,Is.EqualTo(3));
            Assert.That(merchant.Items.Select(i=>i.Id).Intersect(old),Is.Empty);Assert.That(equipment.Find(purchased.Id),Is.Null);
            Assert.That(merchant.TryTrade(true,purchased.Id),Is.EqualTo(Stage2TradeResult.InvalidItem));
        }
    }
}
#endif
