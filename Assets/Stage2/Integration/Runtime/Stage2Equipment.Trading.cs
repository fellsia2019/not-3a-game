using System;
using System.Collections.Generic;

namespace Not3A.Stage2.Integration
{
    public sealed partial class Stage2Equipment
    {
        internal Stage2EquipmentItem CreateMerchantItem(Stage2ItemType type)
        {
            if(mutating||!Stage2EquipmentCatalog.ValidType(type))throw new InvalidOperationException("Cannot seed merchant during an item transaction.");
            if(nextId==int.MaxValue)throw new InvalidOperationException("Item identity space exhausted.");
            return new Stage2EquipmentItem(nextId++,type,Stage2ItemQuality.F,1);
        }
        internal Stage2TradeResult EvaluateTrade(List<Stage2EquipmentItem> stock,bool buy,int id,int gold)
        {
            if(mutating)return Stage2TradeResult.Busy;
            var item=buy?stock.Find(i=>i.Id==id):Find(id);
            if(item==null||gold<=0)return Stage2TradeResult.InvalidItem;
            if(!buy&&IsEquipped(item))return Stage2TradeResult.EquippedItem;
            if(buy)return wallet.Gold>=gold?Stage2TradeResult.Ready:Stage2TradeResult.InsufficientGold;
            return wallet.Gold<=int.MaxValue-gold?Stage2TradeResult.Ready:Stage2TradeResult.GoldLimit;
        }
        internal Stage2TradeResult TransferTrade(List<Stage2EquipmentItem> stock,bool buy,int id,int gold)
        {
            var result=EvaluateTrade(stock,buy,id,gold);if(result!=Stage2TradeResult.Ready)return result;
            var source=buy?stock:items;var target=buy?items:stock;var index=source.FindIndex(i=>i.Id==id);var item=source[index];
            mutating=true;
            try
            {
                // Wallet observers see completed ownership and balance together; nested equipment operations are blocked.
                source.RemoveAt(index);target.Add(item);
                var paid=buy?wallet.TrySpend(0,0,0,gold):wallet.TryAddGold(gold);
                if(!paid)
                {
                    target.Remove(item);source.Insert(index,item);
                    return buy?Stage2TradeResult.InsufficientGold:Stage2TradeResult.GoldLimit;
                }
            }
            finally{mutating=false;}
            Changed?.Invoke();return Stage2TradeResult.Ready;
        }
    }
}
