using System;
using System.Collections.Generic;

namespace Not3A.Stage2.Integration
{
    public enum Stage2TradeResult { Ready, Busy, InvalidItem, EquippedItem, InsufficientGold, GoldLimit, Unavailable }

    /// <summary>One finite merchant inventory. Items move as the same instance, never as duplicated rewards.</summary>
    public sealed class Stage2Merchant
    {
        private readonly Stage2Equipment equipment;
        private readonly List<Stage2EquipmentItem> stock=new List<Stage2EquipmentItem>();
        private readonly List<Stage2BlueprintItem> blueprintStock=new List<Stage2BlueprintItem>();
        public IReadOnlyList<Stage2EquipmentItem> Items {get;}
        public IReadOnlyList<Stage2BlueprintItem> BlueprintItems {get;}
        public Stage2MerchantCatalog Catalog {get;}
        public event Action Changed;
        public Stage2Merchant(Stage2Equipment owner,Stage2MerchantCatalog catalog)
        {
            equipment=owner??throw new ArgumentNullException(nameof(owner));
            if(catalog==null||!catalog.TryValidate(out var reason))throw new ArgumentException("Invalid merchant catalog.");
            Catalog=catalog;Items=stock.AsReadOnly();BlueprintItems=blueprintStock.AsReadOnly();Reset();
        }
        public Stage2EquipmentItem Find(int id)
        {foreach(var item in stock)if(item.Id==id)return item;return null;}
        public Stage2TradeResult Evaluate(bool buy,int id)
        {
            var item=buy?Find(id):equipment.Find(id);
            return item==null?Stage2TradeResult.InvalidItem:equipment.EvaluateTrade(stock,buy,id,buy?Catalog.BuyPrice(item):Catalog.SellPrice(item));
        }
        public Stage2TradeResult TryTrade(bool buy,int id)
        {
            var item=buy?Find(id):equipment.Find(id);
            if(item==null)return Stage2TradeResult.InvalidItem;
            var result=equipment.TransferTrade(stock,buy,id,buy?Catalog.BuyPrice(item):Catalog.SellPrice(item));
            if(result==Stage2TradeResult.Ready)Changed?.Invoke();
            return result;
        }
        public void Reset()
        {
            if(equipment.ItemMutationActive)return;
            stock.Clear();
            for(var i=0;i<8;i++)stock.Add(equipment.CreateMerchantItem((Stage2ItemType)i));
            if(equipment.Catalog.ExpandedLoadout)for(var i=11;i<14;i++)
            {var potion=equipment.CreateMerchantItem((Stage2ItemType)i);potion.Quantity=Catalog.PotionStock;stock.Add(potion);}
            blueprintStock.Clear();
            if(equipment.Blueprints!=null)for(var i=0;i<equipment.Blueprints.Catalog.Count;i++)
                blueprintStock.Add(equipment.Blueprints.CreateMerchantItem((Stage2BlueprintKind)i));
            Changed?.Invoke();
        }
        public Stage2BlueprintItem FindBlueprint(int id)=>blueprintStock.Find(item=>item.Id==id);
        public int BlueprintPrice(Stage2BlueprintItem item,bool buy)
        {var gold=equipment.Blueprints.Catalog.Get(item.Kind).goldPrice;return buy?gold:gold/2;}
        public Stage2TradeResult EvaluateBlueprint(bool buy,int id)
        {
            var rules=equipment.Blueprints;var item=buy?FindBlueprint(id):rules?.Find(id);
            return item==null?Stage2TradeResult.InvalidItem:rules.EvaluateTrade(blueprintStock,buy,id,BlueprintPrice(item,buy));
        }
        public Stage2TradeResult TryTradeBlueprint(bool buy,int id)
        {
            var rules=equipment.Blueprints;var item=buy?FindBlueprint(id):rules?.Find(id);
            if(item==null)return Stage2TradeResult.InvalidItem;
            var result=rules.TransferTrade(blueprintStock,buy,id,BlueprintPrice(item,buy));
            if(result==Stage2TradeResult.Ready)Changed?.Invoke();return result;
        }
    }
}
