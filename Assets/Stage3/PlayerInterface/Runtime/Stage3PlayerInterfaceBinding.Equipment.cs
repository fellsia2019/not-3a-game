using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage2EquipmentDriver equipmentDriver;
        private Stage3Forge forge;
        private Stage3ForgeArt equipmentArt;
        private int selectedEquipmentId = -1, selectedForgeItemId = -1, equipmentStamp;
        private Stage2ItemType selectedForgeType;
        private Stage3ForgeMode forgeMode;
        private bool equipmentDirty = true;
        public Stage2EquipmentDriver Equipment => equipmentDriver;
        public Stage3Forge Forge => forge;
        public Stage2EquipmentResult LastEquipmentResult { get; private set; }
        private bool CanManageEquipment => bound && equipmentDriver != null && !game.RunDriver.Run.IsPaused &&
            !game.RunDriver.Run.IsTerminal && !(legacy?.DebugOpen ?? false) && presentation.isActiveAndEnabled;
        private void BindEquipment()
        {
            var catalog = Resources.Load<Stage2EquipmentCatalog>("Stage3Equipment");
            var building = Resources.Load<Stage3ForgeCatalog>("Stage3ForgeBuilding");
            equipmentArt = Resources.Load<Stage3ForgeArt>("Stage3ForgeArt");
            if (catalog == null || !catalog.TryValidate(out _) || building == null || !building.TryValidate(out _) || equipmentArt == null)
            { Debug.LogError("S3_EQUIPMENT_BINDING_ERROR: equipment/forge catalogs and approved art are required."); return; }
            equipmentDriver = GetComponent<Stage2EquipmentDriver>() ?? gameObject.AddComponent<Stage2EquipmentDriver>();
            equipmentDriver.Configure(game, catalog);
            forge = GetComponent<Stage3Forge>() ?? gameObject.AddComponent<Stage3Forge>();
            forge.Initialize(building, equipmentArt, equipmentDriver);
            equipmentDriver.Rules.Changed += MarkEquipmentDirty;
            game.Gathering.Economy.Inventory.Changed += MarkEquipmentDirty;
            presentation.WindowChanged += MarkEquipmentDirty;
            presentation.EquipmentItemSelected += SelectEquipmentItem;
            presentation.EquipItemRequested += EquipItem;
            presentation.UnequipSlotRequested += UnequipSlot;
            presentation.ForgeModeSelected += SelectForgeMode;
            presentation.ForgeTypeSelected += SelectForgeType;
            presentation.ForgeItemSelected += SelectForgeItem;
            presentation.ForgeExecuteRequested += ExecuteForge;
            presentation.ForgeUpgradeRequested += UpgradeForge;
            forge.OpenRequested += OpenForge;
            game.RunDriver.Run.Reset += ClearEquipmentSelection;
            presentation.SetEquipmentArt(equipmentArt);
            RefreshEquipment();
            Debug.Log("S3_EQUIPMENT_READY:5 combat slots plus tools,8 qualities,3 scroll-only forge stages; current items survive retry.");
        }
        private void MarkEquipmentDirty() => equipmentDirty = true;
        private void ClearEquipmentSelection() { selectedEquipmentId = selectedForgeItemId = -1; MarkEquipmentDirty(); }
        private void SelectEquipmentItem(int id)
        { if (equipmentDriver.Rules.Find(id) == null&&Blueprints?.Find(id)==null) return; selectedEquipmentId = id; MarkEquipmentDirty(); RefreshEquipment(); }
        private void EquipItem(int id) => TryEquipItem(id);
        public bool TryEquipItem(int id)
        {
            if (!CanManageEquipment || presentation.ActiveWindow != Stage3PlayerWindow.Inventory) return false;
            LastEquipmentResult = equipmentDriver.Rules.TryEquip(id); MarkEquipmentDirty(); RefreshEquipment();
            return LastEquipmentResult == Stage2EquipmentResult.Ready;
        }
        private void UnequipSlot(int slot) => TryUnequipSlot(slot);
        public bool TryUnequipSlot(int slot)
        {
            if (!CanManageEquipment || presentation.ActiveWindow != Stage3PlayerWindow.Inventory || slot < 0 || slot > 6) return false;
            LastEquipmentResult = equipmentDriver.Rules.TryUnequip((Stage2ItemSlot)slot); MarkEquipmentDirty(); RefreshEquipment();
            return LastEquipmentResult == Stage2EquipmentResult.Ready;
        }
        private void SelectForgeMode(Stage3ForgeMode mode) { forgeMode = mode; MarkEquipmentDirty(); RefreshEquipment(); }
        private void SelectForgeType(int index)
        { if (index < 0 || index > 7) return; selectedForgeType = (Stage2ItemType)index; selectedForgeItemId = -1; MarkEquipmentDirty(); RefreshEquipment(); }
        private void SelectForgeItem(int id)
        {
            var item = equipmentDriver.Rules.Find(id); if (item == null || item.Type != selectedForgeType) return;
            selectedForgeItemId = id; MarkEquipmentDirty(); RefreshEquipment();
        }
        private void OpenForge()
        {
            ClearBuildingSelection(); CancelBlink();
            presentation.SetWindow(Stage3PlayerWindow.Forge); MarkEquipmentDirty(); RefreshEquipment();
        }
        private void ExecuteForge() => TryForgeOperation();
        public bool TryForgeOperation()
        {
            if (!CanManageEquipment || presentation.ActiveWindow != Stage3PlayerWindow.Forge || !forge.CanUse) return false;
            var rules = equipmentDriver.Rules;
            if (forgeMode == Stage3ForgeMode.Create)
            {
                LastEquipmentResult = rules.TryCraft(selectedForgeType, out var created);
                if (created != null) selectedEquipmentId = selectedForgeItemId = created.Id;
            }
            else LastEquipmentResult = forgeMode == Stage3ForgeMode.Level ? rules.TryLevel(selectedForgeItemId) : rules.TryReforge(selectedForgeItemId);
            MarkEquipmentDirty(); RefreshEquipment(); return LastEquipmentResult == Stage2EquipmentResult.Ready;
        }
        private void UpgradeForge() { forge.TryUpgradeBuilding(); MarkEquipmentDirty(); RefreshEquipment(); }
        private void UpdateEquipment()
        {
            if (equipmentDriver == null) return;
            var stamp = HashCode.Combine(CanManageEquipment, presentation.ActiveWindow, forge.CanUse, forge.CanUpgradeBuilding);
            if (stamp != equipmentStamp) { equipmentStamp = stamp; equipmentDirty = true; }
            if (equipmentDirty) RefreshEquipment();
            presentation.EquipmentInteractionHint = null;
            var target = game.Gathering.InteractionTarget;
            if (target != null && game.Gathering.Economy.HarvestPower(target.Resource) == 0 && !presentation.IsModalOpen)
                presentation.EquipmentInteractionHint = target.Resource == Stage2.Map.MapResourceKind.Wood ? "Наденьте топор в инвентаре" : "Наденьте кирку в инвентаре";
        }
        private void RefreshEquipment()
        {
            if (equipmentDriver == null) return;
            equipmentDirty = false;
            var rules = equipmentDriver.Rules;var allRows=new List<Stage3EquipmentItemView>();
            foreach(var item in rules.Items)allRows.Add(ItemView(item));
            if(Blueprints!=null)foreach(var paper in Blueprints.Items)allRows.Add(BlueprintItemView(paper));
            var rows=allRows.ToArray();
            var bag = new List<Stage3EquipmentItemView>();
            foreach (var row in rows) if (!row.IsEquipped) bag.Add(row);
            if (Array.Find(rows,row=>row.Id==selectedEquipmentId)==null) selectedEquipmentId = rows.Length > 0 ? rows[0].Id : -1;
            var slots = new int[7];
            for (var i = 0; i < slots.Length; i++) slots[i] = rules.Equipped((Stage2ItemSlot)i)?.Id ?? -1;
            presentation.SetEquipment(new Stage3EquipmentView { Items = rows, BagItems = bag.ToArray(), EquippedItemIds = slots, SelectedItemId = selectedEquipmentId,
                CanUnequip = CanManageEquipment && presentation.ActiveWindow == Stage3PlayerWindow.Inventory, UnequipUnavailableReason = "Сейчас недоступно" });
            RefreshForgeView();
            var weapon = rules.Equipped(Stage2ItemSlot.Weapon);
            var avatar = scene.PlayerMover.GetComponent<Stage3HeroAvatar>();
            avatar?.SetEquipmentVisual(equipmentArt, weapon == null ? -1 : (int)weapon.Type, weapon == null ? 0 : (int)weapon.Quality);
            var pickaxe = rules.Equipped(Stage2ItemSlot.Pickaxe);
            var axe = rules.Equipped(Stage2ItemSlot.Axe);
            avatar?.SetHarvestEquipmentVisual(pickaxe == null ? -1 : (int)pickaxe.Quality, axe == null ? -1 : (int)axe.Quality);
            RefreshHeroProgression();
        }
        private Stage3EquipmentItemView ItemView(Stage2EquipmentItem item) => ItemView(item.Type, item.Quality, item.Level, item.Id, equipmentDriver.Rules.IsEquipped(item));
        private Stage3EquipmentItemView ItemView(Stage2ItemType type, Stage2ItemQuality quality, int level, int id = -1, bool equipped = false)
        {
            return new Stage3EquipmentItemView { Id = id, TypeIndex = (int)type, QualityIndex = (int)quality, Level = level,
                Name = equipmentDriver.Catalog.Get(type).displayName, Properties = ItemProperties(type, quality, level), IsEquipped = equipped,
                EquippedLabel = "Надето", CanEquip = id >= 0 && CanManageEquipment && presentation.ActiveWindow == Stage3PlayerWindow.Inventory,
                EquipUnavailableReason = "Сейчас недоступно" };
        }
        private Stage3SelectedBuildingStat[] ItemProperties(Stage2ItemType type, Stage2ItemQuality quality, int level)
        {
            var stats = equipmentDriver.Catalog.Stats(type, quality, level);
            if (stats.Weapon) return new[] { new Stage3SelectedBuildingStat(stats.Magic ? "Магический урон" : "Урон", stats.Damage.ToString()),
                new Stage3SelectedBuildingStat("Радиус", stats.Range.ToString("0.##",DisplayCulture)),
                new Stage3SelectedBuildingStat("Интервал", stats.Interval.ToString("0.##",DisplayCulture)+" с") };
            if (stats.HarvestPower > 0) return new[] { new Stage3SelectedBuildingStat("За удар", stats.HarvestPower.ToString()),
                new Stage3SelectedBuildingStat("Темп добычи", "×"+stats.HarvestSpeed.ToString("0.##",DisplayCulture)) };
            return new[] { new Stage3SelectedBuildingStat("Броня", stats.Armor.ToString()),
                new Stage3SelectedBuildingStat("Защита героя", "Не открыта") };
        }
        private void RefreshForgeView()
        {
            var rules = equipmentDriver.Rules; var catalog = equipmentDriver.Catalog;
            var choices = new List<Stage3EquipmentItemView>();
            foreach (var item in rules.Items) if (item.Type == selectedForgeType) choices.Add(ItemView(item));
            var current = rules.Find(selectedForgeItemId);
            if (current == null || current.Type != selectedForgeType) current = choices.Count > 0 ? rules.Find(choices[0].Id) : null;
            selectedForgeItemId = current?.Id ?? -1;
            Stage3EquipmentItemView before = null, outcome = null; Stage2EquipmentResult result; Stage2ResourceCost cost = default;
            if (forgeMode == Stage3ForgeMode.Create)
            { outcome = ItemView(selectedForgeType,Stage2ItemQuality.F,1); cost = catalog.CraftCost(selectedForgeType); result = rules.EvaluateCraft(selectedForgeType); }
            else
            {
                result = forgeMode == Stage3ForgeMode.Level ? rules.EvaluateLevel(selectedForgeItemId) : rules.EvaluateReforge(selectedForgeItemId);
                if (current != null)
                {
                    before = ItemView(current);
                    if (forgeMode == Stage3ForgeMode.Level && current.Level < catalog.MaximumItemLevel)
                    { outcome = ItemView(current.Type,current.Quality,current.Level+1); cost = catalog.LevelCost(current.Type,current.Level); }
                    if (forgeMode == Stage3ForgeMode.Reforge && current.Quality < Stage2ItemQuality.S)
                    { outcome = ItemView(current.Type,current.Quality+1,current.Level); cost = catalog.ReforgeCost(current.Type,current.Quality+1); }
                }
            }
            var properties = new List<Stage3ForgePropertyView>();
            if (outcome?.Properties != null)
                for (var i = 0; i < outcome.Properties.Length; i++) properties.Add(new Stage3ForgePropertyView { Label = outcome.Properties[i].Label,
                    Before = before?.Properties != null && i < before.Properties.Length ? before.Properties[i].Value : "—", After = outcome.Properties[i].Value });
            presentation.SetForge(new Stage3ForgeView { Mode = forgeMode, SelectedTypeIndex = (int)selectedForgeType, SelectedItemId = selectedForgeItemId,
                BuildingLevel = rules.ForgeLevel, MaximumQualityIndex = (int)rules.MaximumQuality, Items = choices.ToArray(), CurrentItem = before, Outcome = outcome,
                Cost = ForgeCost(cost), Properties = properties.ToArray(), CanExecute = CanManageEquipment && forge.CanUse && result == Stage2EquipmentResult.Ready,
                UnavailableReason = !forge.IsBuilt ? "Сначала постройте кузницу" : !forge.NearEntrance ? "Подойдите ко входу в кузницу" :
                    !CanManageEquipment || !forge.CanUse ? "Кузница сейчас недоступна" : EquipmentReason(result),
                CanUpgradeBuilding = forge.CanUpgradeBuilding, UpgradeCost = new Stage3ForgeCostView { Scrolls = Blueprints==null?1:0 },
                NextMaximumQualityIndex = (int)catalog.ForgeCap(Math.Min(catalog.ForgeStageCount,rules.ForgeLevel+1)), UpgradeUnavailableReason = forge.UpgradeUnavailableReason });
        }
        private static Stage3ForgeCostView ForgeCost(Stage2ResourceCost cost) => new Stage3ForgeCostView { Wood = cost.Wood, Stone = cost.Stone, Metal = cost.Metal, Gold = cost.Gold };
        public static string EquipmentReason(Stage2EquipmentResult result) => result switch {
            Stage2EquipmentResult.Ready => null,
            Stage2EquipmentResult.ForgeRequired => "Сначала постройте кузницу",
            Stage2EquipmentResult.InsufficientResources => "Недостаточно ресурсов",
            Stage2EquipmentResult.MaximumLevel => "Максимальный уровень",
            Stage2EquipmentResult.ForgeQualityLimit => "Нужна следующая ступень кузницы",
            Stage2EquipmentResult.DropOnlyQuality => "SSS — только ультраредкая добыча",
            Stage2EquipmentResult.UpgradeScrollRequired => "Нужен свиток улучшения кузницы",
            Stage2EquipmentResult.ResearchRequired => "Изучите соответствующий чертёж в мастерской",
            Stage2EquipmentResult.MaximumForgeStage => "Максимальная ступень",
            Stage2EquipmentResult.InvalidItem => "Выберите предмет",
            _ => "Сейчас недоступно" };
        private void UnbindEquipment()
        {
            if (equipmentDriver == null) return;
            equipmentDriver.Rules.Changed -= MarkEquipmentDirty;
            game.Gathering.Economy.Inventory.Changed -= MarkEquipmentDirty;
            game.RunDriver.Run.Reset -= ClearEquipmentSelection;
            forge.OpenRequested -= OpenForge;
            presentation.WindowChanged -= MarkEquipmentDirty;
            presentation.EquipmentItemSelected -= SelectEquipmentItem; presentation.EquipItemRequested -= EquipItem;
            presentation.UnequipSlotRequested -= UnequipSlot; presentation.ForgeModeSelected -= SelectForgeMode;
            presentation.ForgeTypeSelected -= SelectForgeType; presentation.ForgeItemSelected -= SelectForgeItem;
            presentation.ForgeExecuteRequested -= ExecuteForge; presentation.ForgeUpgradeRequested -= UpgradeForge;
            presentation.SetEquipment(null); presentation.SetForge(null); presentation.SetEquipmentArt(null);
        }
    }
}
