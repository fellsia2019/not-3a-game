using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;

namespace Not3A.Stage2.Integration
{
    public enum Stage2EquipmentResult { Ready, Busy, InvalidItem, InvalidSlot, ForgeRequired, InsufficientResources,
        MaximumLevel, ForgeQualityLimit, DropOnlyQuality, MaximumForgeStage, UpgradeScrollRequired, ResearchRequired }
    public sealed class Stage2EquipmentItem
    {
        public int Id { get; }
        public Stage2ItemType Type { get; }
        public Stage2ItemQuality Quality { get; internal set; }
        public int Level { get; internal set; }
        internal Stage2EquipmentItem(int id, Stage2ItemType type, Stage2ItemQuality quality, int level)
        { Id = id; Type = type; Quality = quality; Level = level; }
    }

    /// <summary>World-owned item instances and atomic forge rules. No UI, random drops or scene references.</summary>
    public sealed partial class Stage2Equipment
    {
        private readonly Stage2EquipmentCatalog catalog;
        private readonly Stage2Inventory wallet;
        private readonly List<Stage2EquipmentItem> items = new List<Stage2EquipmentItem>();
        private readonly IReadOnlyList<Stage2EquipmentItem> itemView;
        private readonly Stage2EquipmentItem[] equipped = new Stage2EquipmentItem[7];
        private int nextId = 1;
        private bool mutating;
        public Stage2EquipmentCatalog Catalog => catalog;
        public IReadOnlyList<Stage2EquipmentItem> Items => itemView;
        public int ForgeLevel { get; private set; }
        public int ForgeUpgradeScrolls { get; private set; }
        public Stage2ItemQuality MaximumQuality => catalog.ForgeCap(ForgeLevel);
        public event Action Changed;
        public Stage2Equipment(Stage2EquipmentCatalog data, Stage2Inventory resources)
        {
            if (data == null || !data.TryValidate(out _)) throw new ArgumentException("Invalid equipment data.");
            catalog = data; wallet = resources ?? throw new ArgumentNullException(nameof(resources));
            itemView = items.AsReadOnly(); Reset();
        }
        public Stage2EquipmentItem Find(int id) { foreach (var item in items) if (item.Id == id) return item; return null; }
        public Stage2EquipmentItem Equipped(Stage2ItemSlot slot) => (int)slot >= 0 && (int)slot < equipped.Length ? equipped[(int)slot] : null;
        public bool IsEquipped(Stage2EquipmentItem item) => item != null && ReferenceEquals(Equipped(Stage2EquipmentCatalog.Slot(item.Type)), item);
        public Stage2EquipmentResult EvaluateEquip(int id) => mutating ? Stage2EquipmentResult.Busy : Find(id) == null ? Stage2EquipmentResult.InvalidItem : Stage2EquipmentResult.Ready;
        public Stage2EquipmentResult TryEquip(int id)
        {
            var result = EvaluateEquip(id); if (result != Stage2EquipmentResult.Ready) return result;
            var item = Find(id); if (IsEquipped(item)) return result;
            equipped[(int)Stage2EquipmentCatalog.Slot(item.Type)] = item; Changed?.Invoke(); return result;
        }
        public Stage2EquipmentResult TryUnequip(Stage2ItemSlot slot)
        {
            if (mutating) return Stage2EquipmentResult.Busy;
            if (Equipped(slot) == null) return Stage2EquipmentResult.InvalidSlot;
            equipped[(int)slot] = null; Changed?.Invoke(); return Stage2EquipmentResult.Ready;
        }
        public Stage2EquipmentResult EvaluateCraft(Stage2ItemType type) => mutating ? Stage2EquipmentResult.Busy :
            !Stage2EquipmentCatalog.ValidType(type) ? Stage2EquipmentResult.InvalidItem : ForgeLevel < 1 ? Stage2EquipmentResult.ForgeRequired :
            type==Stage2ItemType.Wand&&Blueprints!=null&&!Blueprints.IsStudied(Stage2BlueprintKind.WandRecipe)?Stage2EquipmentResult.ResearchRequired:
            !wallet.CanAfford(catalog.CraftCost(type)) ? Stage2EquipmentResult.InsufficientResources : Stage2EquipmentResult.Ready;
        public Stage2EquipmentResult TryCraft(Stage2ItemType type, out Stage2EquipmentItem created)
        {
            created = null; var result = EvaluateCraft(type); if (result != Stage2EquipmentResult.Ready) return result;
            mutating = true;
            try
            {
                if (!wallet.TrySpend(catalog.CraftCost(type))) return Stage2EquipmentResult.InsufficientResources;
                created = Add(type, Stage2ItemQuality.F, 1);
            }
            finally { mutating = false; }
            Changed?.Invoke(); return Stage2EquipmentResult.Ready;
        }
        public Stage2EquipmentResult EvaluateLevel(int id)
        {
            var item = Find(id);
            if (mutating) return Stage2EquipmentResult.Busy;
            if (item == null) return Stage2EquipmentResult.InvalidItem;
            if (ForgeLevel == 0) return Stage2EquipmentResult.ForgeRequired;
            if (item.Level >= catalog.MaximumItemLevel) return Stage2EquipmentResult.MaximumLevel;
            if (item.Quality > MaximumQuality) return Stage2EquipmentResult.ForgeQualityLimit;
            return wallet.CanAfford(catalog.LevelCost(item.Type, item.Level)) ? Stage2EquipmentResult.Ready : Stage2EquipmentResult.InsufficientResources;
        }
        public Stage2EquipmentResult EvaluateReforge(int id)
        {
            var item = Find(id);
            if (mutating) return Stage2EquipmentResult.Busy;
            if (item == null) return Stage2EquipmentResult.InvalidItem;
            if (ForgeLevel == 0) return Stage2EquipmentResult.ForgeRequired;
            if (item.Quality >= Stage2ItemQuality.S) return Stage2EquipmentResult.DropOnlyQuality;
            if (item.Quality + 1 > MaximumQuality) return Stage2EquipmentResult.ForgeQualityLimit;
            return wallet.CanAfford(catalog.ReforgeCost(item.Type, item.Quality + 1)) ? Stage2EquipmentResult.Ready : Stage2EquipmentResult.InsufficientResources;
        }
        public Stage2EquipmentResult TryLevel(int id) => Improve(id, false);
        public Stage2EquipmentResult TryReforge(int id) => Improve(id, true);
        private Stage2EquipmentResult Improve(int id, bool quality)
        {
            var result = quality ? EvaluateReforge(id) : EvaluateLevel(id); if (result != Stage2EquipmentResult.Ready) return result;
            var item = Find(id); mutating = true;
            try
            {
                var cost = quality ? catalog.ReforgeCost(item.Type, item.Quality + 1) : catalog.LevelCost(item.Type, item.Level);
                if (!wallet.TrySpend(cost)) return Stage2EquipmentResult.InsufficientResources;
                if (quality) item.Quality++; else item.Level++;
            }
            finally { mutating = false; }
            Changed?.Invoke(); return Stage2EquipmentResult.Ready;
        }
        // Future loot owner calls once per real reward. No automatic source/rate is invented here.
        public bool TryReceiveLoot(Stage2ItemType type, Stage2ItemQuality quality, int level, out Stage2EquipmentItem item)
        {
            item = null;
            if (mutating || !Stage2EquipmentCatalog.ValidType(type) || !Stage2EquipmentCatalog.ValidQuality(quality) ||
                level < 1 || level > catalog.MaximumItemLevel) return false;
            item = Add(type, quality, level); Changed?.Invoke(); return true;
        }
        public bool TryReceiveForgeScrolls(int amount)
        {
            if (mutating || amount <= 0 || amount > int.MaxValue - ForgeUpgradeScrolls) return false;
            ForgeUpgradeScrolls += amount; Changed?.Invoke(); return true;
        }
        public bool ActivateForge()
        { if (ForgeLevel != 0 || mutating) return false; ForgeLevel = 1; Changed?.Invoke(); return true; }
        public void DeactivateForge()
        { if (ForgeLevel == 0) return; ForgeLevel = 0; Changed?.Invoke(); }
        public Stage2EquipmentResult EvaluateForgeUpgrade() => mutating ? Stage2EquipmentResult.Busy :
            ForgeLevel == 0 ? Stage2EquipmentResult.ForgeRequired : ForgeLevel >= catalog.ForgeStageCount ? Stage2EquipmentResult.MaximumForgeStage :
            Blueprints!=null ? (KnowsForgeStage(ForgeLevel+1)?Stage2EquipmentResult.Ready:Stage2EquipmentResult.ResearchRequired) :
            ForgeUpgradeScrolls < 1 ? Stage2EquipmentResult.UpgradeScrollRequired : Stage2EquipmentResult.Ready;
        public Stage2EquipmentResult TryUpgradeForge()
        {
            var result = EvaluateForgeUpgrade(); if (result != Stage2EquipmentResult.Ready) return result;
            if(Blueprints==null)ForgeUpgradeScrolls--;
            ForgeLevel++; Changed?.Invoke(); return result;
        }
        private Stage2EquipmentItem Add(Stage2ItemType type, Stage2ItemQuality quality, int level)
        {
            if (nextId == int.MaxValue) throw new InvalidOperationException("Item identity space exhausted.");
            var item = new Stage2EquipmentItem(nextId++, type, quality, level); items.Add(item); return item;
        }
        public void Reset()
        {
            if(mutating)return;
            items.Clear(); Array.Clear(equipped, 0, equipped.Length); ForgeLevel = 0; ForgeUpgradeScrolls = 0;
            foreach (var type in new[] { Stage2ItemType.Bow, Stage2ItemType.Pickaxe, Stage2ItemType.Axe })
            { var item = Add(type, Stage2ItemQuality.F, 1); equipped[(int)Stage2EquipmentCatalog.Slot(type)] = item; }
            Blueprints?.ResetFromOwner();Changed?.Invoke();
        }
    }
}
