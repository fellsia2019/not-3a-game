using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;

namespace Not3A.Stage2.Integration
{
    public enum Stage2BlueprintResult { Ready, Busy, InvalidItem, AlreadyStudied, PreviousStudyRequired, Unavailable }
    public sealed class Stage2BlueprintItem
    {
        public int Id { get; }
        public Stage2BlueprintKind Kind { get; }
        internal Stage2BlueprintItem(int id,Stage2BlueprintKind kind){Id=id;Kind=kind;}
    }

    /// <summary>Owned document instances and permanent current-world knowledge; no item levels/quality or scene checks.</summary>
    public sealed class Stage2Blueprints
    {
        private readonly Stage2Equipment owner;
        private readonly Stage2Inventory wallet;
        private readonly List<Stage2BlueprintItem> papers=new List<Stage2BlueprintItem>();
        private readonly HashSet<Stage2BlueprintKind> studied=new HashSet<Stage2BlueprintKind>();
        public Stage2BlueprintCatalog Catalog {get;}
        public IReadOnlyList<Stage2BlueprintItem> Items {get;}
        public int StudiedCount=>studied.Count;
        public event Action Changed;
        internal Stage2Blueprints(Stage2Equipment owner,Stage2Inventory wallet,Stage2BlueprintCatalog catalog)
        {this.owner=owner;this.wallet=wallet;Catalog=catalog;Items=papers.AsReadOnly();}
        public bool IsStudied(Stage2BlueprintKind kind)=>studied.Contains(kind);
        public Stage2BlueprintItem Find(int id)=>papers.Find(item=>item.Id==id);
        public Stage2BlueprintItem Find(Stage2BlueprintKind kind)=>papers.Find(item=>item.Kind==kind);
        internal Stage2BlueprintItem CreateMerchantItem(Stage2BlueprintKind kind)
        {Catalog.Get(kind);return new Stage2BlueprintItem(owner.ReserveInventoryId(),kind);}
        public Stage2BlueprintResult EvaluateStudy(int id)
        {
            if(owner.ItemMutationActive)return Stage2BlueprintResult.Busy;
            var item=Find(id);if(item==null)return Stage2BlueprintResult.InvalidItem;
            if(studied.Contains(item.Kind))return Stage2BlueprintResult.AlreadyStudied;
            if(item.Kind==Stage2BlueprintKind.ForgeIII&&!studied.Contains(Stage2BlueprintKind.ForgeII))return Stage2BlueprintResult.PreviousStudyRequired;
            return Stage2BlueprintResult.Ready;
        }
        public Stage2BlueprintResult TryStudy(int id)
        {
            var result=EvaluateStudy(id);if(result!=Stage2BlueprintResult.Ready)return result;
            if(!owner.BeginItemMutation())return Stage2BlueprintResult.Busy;
            try {var item=Find(id);papers.Remove(item);studied.Add(item.Kind);}
            finally {owner.EndItemMutation();}
            Changed?.Invoke();return Stage2BlueprintResult.Ready;
        }
        internal Stage2TradeResult EvaluateTrade(List<Stage2BlueprintItem> stock,bool buy,int id,int gold)
        {
            if(owner.ItemMutationActive)return Stage2TradeResult.Busy;
            if(gold<=0||(buy?stock:papers).Find(item=>item.Id==id)==null)return Stage2TradeResult.InvalidItem;
            return buy?(wallet.Gold>=gold?Stage2TradeResult.Ready:Stage2TradeResult.InsufficientGold):
                wallet.Gold<=int.MaxValue-gold?Stage2TradeResult.Ready:Stage2TradeResult.GoldLimit;
        }
        internal Stage2TradeResult TransferTrade(List<Stage2BlueprintItem> stock,bool buy,int id,int gold)
        {
            var result=EvaluateTrade(stock,buy,id,gold);if(result!=Stage2TradeResult.Ready)return result;
            if(!owner.BeginItemMutation())return Stage2TradeResult.Busy;
            var source=buy?stock:papers;var target=buy?papers:stock;var index=source.FindIndex(item=>item.Id==id);var item=source[index];
            try
            {
                // Shared equipment transaction guard prevents cross-family re-entry; wallet sees completed ownership.
                source.RemoveAt(index);target.Add(item);
                if(!(buy?wallet.TrySpend(0,0,0,gold):wallet.TryAddGold(gold)))
                {target.Remove(item);source.Insert(index,item);return buy?Stage2TradeResult.InsufficientGold:Stage2TradeResult.GoldLimit;}
            }
            finally {owner.EndItemMutation();}
            Changed?.Invoke();return Stage2TradeResult.Ready;
        }
        internal void ResetFromOwner(){papers.Clear();studied.Clear();Changed?.Invoke();}
        internal void ExtractRecoveryPapers(List<Stage2BlueprintItem> target)
        { target.AddRange(papers);papers.Clear(); }
        internal void RestoreRecoveryPapers(IReadOnlyList<Stage2BlueprintItem> source)
        { foreach(var paper in source)papers.Add(paper); }
        internal void NotifyRecoveryChanged()=>Changed?.Invoke();
    }
}
