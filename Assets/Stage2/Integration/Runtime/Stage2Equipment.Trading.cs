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
            if(Stage2EquipmentCatalog.IsConsumable(item.Type))return TransferPotion(source,target,item,index,buy,gold);
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
        private Stage2TradeResult TransferPotion(List<Stage2EquipmentItem> source,List<Stage2EquipmentItem> target,
            Stage2EquipmentItem item,int index,bool buy,int gold)
        {
            if(item.Quantity<=0||nextId==int.MaxValue)return Stage2TradeResult.InvalidItem;
            var stack=target.Find(i=>i.Type==item.Type);var created=stack==null;
            if(stack!=null&&stack.Quantity==int.MaxValue)return Stage2TradeResult.InvalidItem;
            mutating=true;
            try
            {
                item.Quantity--;if(item.Quantity==0)source.RemoveAt(index);
                if(created){stack=new Stage2EquipmentItem(nextId++,item.Type,Stage2ItemQuality.F,1);target.Add(stack);}else stack.Quantity++;
                if(!(buy?wallet.TrySpend(0,0,0,gold):wallet.TryAddGold(gold)))
                {
                    if(created)target.Remove(stack);else stack.Quantity--;
                    if(item.Quantity==0)source.Insert(index,item);item.Quantity++;
                    return buy?Stage2TradeResult.InsufficientGold:Stage2TradeResult.GoldLimit;
                }
            }
            finally{mutating=false;}
            Changed?.Invoke();return Stage2TradeResult.Ready;
        }
        public int ConsumableCount(Stage2ItemType type)
        {var total=0;foreach(var item in items)if(item.Type==type)total+=item.Quantity;return total;}
        public bool TryConsume(Stage2ItemType type,Func<bool> effect)
        {
            if(mutating||!Stage2EquipmentCatalog.IsConsumable(type)||effect==null)return false;
            var item=items.Find(i=>i.Type==type&&i.Quantity>0);if(item==null)return false;
            mutating=true;
            try {if(!effect())return false;item.Quantity--;if(item.Quantity==0)items.Remove(item);}
            finally{mutating=false;}
            Changed?.Invoke();return true;
        }
    }
}
