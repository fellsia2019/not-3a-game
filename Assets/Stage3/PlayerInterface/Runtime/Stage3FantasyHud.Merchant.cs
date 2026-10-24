using System;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed class Stage3TradeItemView
    {
        public Stage3EquipmentItemView Item;
        public int Price;
    }

    public sealed class Stage3MerchantView
    {
        public Stage3TradeItemView[] PlayerItems, MerchantItems;
        public int SelectedItemId = -1, Gold;
        public bool BuyMode = true, CanExecute;
        public string UnavailableReason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3MerchantView merchant;
        private Stage3CityArt merchantCityArt;
        private bool merchantArtLoaded;
        private Vector2 merchantPlayerScroll, merchantStockScroll;
        private string[] merchantPlayerLabels = Array.Empty<string>();
        private string[] merchantStockLabels = Array.Empty<string>();
        private string merchantPriceLabel = "", merchantLevelLabel = "";
        private const int MerchantColumns = 6, MerchantVisibleRows = 5;

        public bool MerchantOpen => ActiveWindow == Stage3PlayerWindow.Merchant;
        public Stage3MerchantView MerchantView => merchant;
        public event Action<bool, int> MerchantItemSelected, MerchantTradeRequested;
        public event Action MerchantClosed;

        public void SetMerchant(Stage3MerchantView value)
        {
            merchant = value;
            merchantPlayerLabels = MerchantLabels(value?.PlayerItems);
            merchantStockLabels = MerchantLabels(value?.MerchantItems);
            CacheMerchantSelection();
        }

        public void ShowMerchant(bool show)
        {
            if (show)
            {
                if (!MerchantOpen) { merchantPlayerScroll = Vector2.zero; merchantStockScroll = Vector2.zero; }
                SetWindow(Stage3PlayerWindow.Merchant);
            }
            else if (MerchantOpen) CloseWindow();
        }

        public void SelectMerchantItem(bool buy, int id)
        {
            if (merchant == null || FindMerchantItem(buy, id) == null) return;
            MerchantItemSelected?.Invoke(buy, id);
        }

        private static string[] MerchantLabels(Stage3TradeItemView[] items)
        {
            if (items == null || items.Length == 0) return Array.Empty<string>();
            var labels = new string[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i]?.Item;
                labels[i] = item == null ? "" : ItemCellCaption(item);
            }
            return labels;
        }

        private Stage3TradeItemView FindMerchantItem(bool buy, int id)
        {
            if (id < 0) return null;
            var items = buy ? merchant?.MerchantItems : merchant?.PlayerItems;
            if (items != null)
                foreach (var entry in items)
                    if (entry?.Item != null && entry.Item.Id == id) return entry;
            return null;
        }

        private void CacheMerchantSelection()
        {
            var selected = merchant == null ? null : FindMerchantItem(merchant.BuyMode, merchant.SelectedItemId);
            merchantPriceLabel = selected == null ? "" : selected.Price + (selected.Item.Consumable ? " золота за 1 шт." : " золота");
            merchantLevelLabel = selected == null ? "" : selected.Item.Blueprint != null
                ? BlueprintCategoryCaption(selected.Item.Blueprint) : selected.Item.Consumable ? "Количество: "+selected.Item.Quantity : ItemCellCaption(selected.Item);
        }

        private Rect MerchantBodyRect => new Rect(32f * UiScale, 148f * UiScale,
            Screen.width - 64f * UiScale, Screen.height - 230f * UiScale);

        private Rect MerchantInventoryPanel(bool buy)
        {
            var body = MerchantBodyRect;
            var width = (body.width - 24f * UiScale) * .39f;
            return new Rect(buy ? body.xMax - width : body.x, body.y, width, body.height);
        }

        private Rect MerchantDetailRect
        {
            get
            {
                var left = MerchantInventoryPanel(false); var right = MerchantInventoryPanel(true);
                return Rect.MinMaxRect(left.xMax + 16f * UiScale, left.y + 8f * UiScale,
                    right.x - 16f * UiScale, left.yMax - 6f * UiScale);
            }
        }

        private Rect MerchantGridRect(bool buy)
        {
            var panel = MerchantInventoryPanel(buy); var s = UiScale;
            var grid = new Rect(panel.x + 14f * s, panel.y + 98f * s, panel.width - 28f * s, panel.height - 114f * s);
            // Five complete rows at the top/bottom stops, including their captions.
            // A spare sliver of a sixth row makes a finished inventory look clipped.
            grid.height = MerchantVisibleRows * (MerchantCellSide(grid) + 22f * s);
            return grid;
        }

        // IMGUI's built-in scrollbar remains physical pixels at 720p; it does
        // not shrink with UiScale. Reserve it before computing any cell widths.
        private float MerchantScrollbarSpace => Mathf.Max(20f, 20f * UiScale);

        private float MerchantCellSide(Rect grid)
        {
            var s = UiScale;
            return Mathf.Min((grid.width - MerchantScrollbarSpace - (MerchantColumns - 1) * 9f * s - 8f * s) / MerchantColumns,
                grid.height / MerchantVisibleRows - 22f * s);
        }

        public Rect MerchantItemRect(bool buy, int index)
        {
            if (!MerchantOpen || index < 0) return Rect.zero;
            var items = buy ? merchant?.MerchantItems : merchant?.PlayerItems;
            if (items == null || index >= items.Length) return Rect.zero;
            var grid = MerchantGridRect(buy); var side = MerchantCellSide(grid); var s = UiScale;
            var scroll = buy ? merchantStockScroll : merchantPlayerScroll;
            var rect = new Rect(grid.x + 4f*s + (index % MerchantColumns) * (side + 9f * s),
                grid.y + (index / MerchantColumns) * (side + 22f * s) - scroll.y, side, side);
            return rect.yMin >= grid.yMin && rect.yMax <= grid.yMax ? rect : Rect.zero;
        }

        public Rect MerchantActionRect
        {
            get
            {
                if (!MerchantOpen) return Rect.zero;
                var r = MerchantDetailRect;
                return new Rect(r.x + 5f * UiScale, r.yMax - 86f * UiScale, r.width - 10f * UiScale, 56f * UiScale);
            }
        }

        private void DrawMerchant()
        {
            if (!merchantArtLoaded)
            {
                merchantArtLoaded = true;
                merchantCityArt = Resources.Load<Stage3CityArt>("Stage3CityArt");
            }
            var selectedPlayer = DrawMerchantInventory(false);
            var selectedStock = DrawMerchantInventory(true);
            var execute = DrawMerchantDetails();
            // Dispatch after rendering both sides so binding refresh never mixes snapshots.
            if (execute && merchant != null) MerchantTradeRequested?.Invoke(merchant.BuyMode, merchant.SelectedItemId);
            else if (selectedPlayer >= 0) SelectMerchantItem(false, selectedPlayer);
            else if (selectedStock >= 0) SelectMerchantItem(true, selectedStock);
        }

        private int DrawMerchantInventory(bool buy)
        {
            var s = UiScale; var panel = MerchantInventoryPanel(buy);
            DrawSliced(panel, art.windowFrame, false, .13f * s);
            var portrait = new Rect(panel.x + 19f * s, panel.y + 12f * s, 72f * s, 72f * s);
            DrawDisc(Inset(portrait, 5f * s), PlumInk);
            if (!buy) DrawRoundSprite(Inset(portrait, 6f * s), art.heroPortrait);
            else if (merchantCityArt?.kit != null && merchantCityArt.kit.Length > 8)
            {
                var person = merchantCityArt.kit[8];
                if (person != null)
                {
                    // Portrait from the existing town merchant; no new identity/texture.
                    var source = person.rect;
                    var headSide = source.width * .55f;
                    var uv = new Rect((source.center.x - headSide * .5f) / person.texture.width,
                        (source.yMax - headSide) / person.texture.height,
                        headSide / person.texture.width, headSide / person.texture.height);
                    DrawCircularTexture(Inset(portrait, 9f * s), person.texture, uv);
                }
            }
            DrawMedallion(portrait);
            var titleLeft = portrait.xMax + 12f * s;
            var titleWidth = panel.xMax - titleLeft - (buy ? 73f : 18f) * s;
            Label(new Rect(titleLeft, panel.y + 17f * s, titleWidth, 35f * s),
                buy ? "Товары торговца" : "Ваш инвентарь", 27);
            Label(new Rect(titleLeft, panel.y + 53f * s, titleWidth, 30f * s),
                buy ? "Выберите предмет для покупки" : "Выберите предмет для продажи", 15, TextAnchor.MiddleLeft, true);
            if (buy && merchantCityArt?.kit != null && merchantCityArt.kit.Length > 11)
                DrawIcon(new Rect(panel.xMax - 69f * s, panel.y + 17f * s, 52f * s, 61f * s), merchantCityArt.kit[11]);

            var grid = MerchantGridRect(buy); var side = MerchantCellSide(grid);
            var items = (buy ? merchant?.MerchantItems : merchant?.PlayerItems) ?? Array.Empty<Stage3TradeItemView>();
            var labels = buy ? merchantStockLabels : merchantPlayerLabels;
            var count = Mathf.Max(MerchantColumns * MerchantVisibleRows, items.Length);
            var rows = (count + MerchantColumns - 1) / MerchantColumns;
            var scroll = buy ? merchantStockScroll : merchantPlayerScroll;
            var height = Mathf.Max(grid.height, rows * (side + 22f * s));
            scroll.x = 0f;
            scroll.y = Mathf.Clamp(scroll.y, 0f, Mathf.Max(0f, height - grid.height));
            scroll = GUI.BeginScrollView(grid, scroll, new Rect(0, 0, grid.width - MerchantScrollbarSpace, height));
            equipmentGuiOrigin=grid.position-scroll;
            var selected = -1;
            equipmentTextureClip = new Rect(scroll.x, scroll.y, grid.width - MerchantScrollbarSpace, grid.height);
            for (var i = 0; i < count; i++)
            {
                var box = new Rect(4f*s + (i % MerchantColumns) * (side + 9f * s),
                    (i / MerchantColumns) * (side + 22f * s), side, side);
                if (box.yMax + 20f * s < scroll.y || box.y > scroll.y + grid.height) continue;
                var item = i < items.Length ? items[i]?.Item : null;
                if (item == null)
                {
                    // Generic sliced art has no explicit Metal clip. Empty cells
                    // have no interaction and draw only inside the viewport.
                    if (box.yMin >= scroll.y && box.yMax <= scroll.y + grid.height)
                        DrawSliced(box, (art.controlPanel??art.panelFrame), true, .09f * s);
                }
                else
                {
                    if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
                    if (GUI.Button(box, GUIContent.none, blankButton)) selected = item.Id;
                    DrawEquipmentCell(box, item, merchant != null && merchant.BuyMode == buy && merchant.SelectedItemId == item.Id);
                    Label(new Rect(box.x, box.yMax, box.width, 20f * s),
                        i < labels.Length ? labels[i] : "", 12, TextAnchor.MiddleCenter);
                }
            }
            equipmentTextureClip = null;
            GUI.EndScrollView();
            equipmentGuiOrigin=Vector2.zero;
            if (buy) merchantStockScroll = scroll; else merchantPlayerScroll = scroll;
            return selected;
        }

        private bool DrawMerchantDetails()
        {
            var r = MerchantDetailRect; var s = UiScale;
            Label(new Rect(r.x, r.y, r.width, 34f * s), "Выбранный предмет", 24, TextAnchor.MiddleCenter);
            var selected = merchant == null ? null : FindMerchantItem(merchant.BuyMode, merchant.SelectedItemId);
            if (selected == null)
            {
                Label(new Rect(r.x, r.y + 70f * s, r.width, r.height - 170f * s),
                    "Выберите предмет\nв одном из инвентарей", 19, TextAnchor.MiddleCenter, true);
                return false;
            }
            Label(new Rect(r.x, r.y + 37f * s, r.width, 28f * s),
                merchant.BuyMode ? "У торговца" : "У вас", 17, TextAnchor.MiddleCenter, true);
            DrawRule(new Rect(r.x + 16f * s, r.y + 73f * s, r.width - 32f * s, s));
            var iconSide = Mathf.Min(168f * s, r.width - 38f * s);
            var preview = new Rect(r.center.x - iconSide * .5f, r.y + 88f * s, iconSide, iconSide);
            if(selected.Item.Blueprint != null) DrawBlueprintIcon(Inset(preview, 8f*s), selected.Item.Blueprint);
            else DrawEquipmentCell(preview, selected.Item);
            FitText(new Rect(r.x, r.y + 267f * s, r.width, 35f * s), selected.Item.Name ?? "", 26, false);
            Label(new Rect(r.x, r.y + 303f * s, r.width, 27f * s), merchantLevelLabel, 17, TextAnchor.MiddleCenter);
            var properties = selected.Item.Properties ?? Array.Empty<Stage3SelectedBuildingStat>();
            var statsTop = r.y + 342f * s;
            var statsBottom = MerchantActionRect.y - 92f * s;
            var rowHeight = Mathf.Min(28f * s, (statsBottom - statsTop) / Mathf.Max(1, properties.Length));
            if(selected.Item.Blueprint != null)
            {
                if(selected.Item.Blueprint.Studied)
                {
                    DrawBlueprintKnownPill(new Rect(r.x,statsTop,r.width,28f*s));
                    statsTop+=36f*s;
                }
                Label(new Rect(r.x+7f*s,statsTop,r.width-14f*s,Mathf.Max(0,statsBottom-statsTop)),
                    selected.Item.Blueprint.Description??"",17,TextAnchor.UpperLeft,true);
            }
            else if(selected.Item.Consumable)
            {
                var consumableRowHeight=Mathf.Min(54f*s,(statsBottom-statsTop)/Mathf.Max(1,properties.Length));
                for(var i=0;i<properties.Length;i++)
                    DrawConsumableProperty(new Rect(r.x+7f*s,statsTop+consumableRowHeight*i,
                        r.width-14f*s,consumableRowHeight),properties[i]);
            }
            else for (var i = 0; i < properties.Length; i++)
            {
                var row = new Rect(r.x + 7f * s, statsTop + rowHeight * i, r.width - 14f * s, rowHeight);
                Label(new Rect(row.x, row.y, row.width * .64f, row.height), properties[i].Label, 16, TextAnchor.MiddleLeft, true);
                Label(new Rect(row.x + row.width * .64f, row.y, row.width * .36f, row.height), properties[i].Value, 17, TextAnchor.MiddleRight);
            }
            var action = MerchantActionRect;
            DrawRule(new Rect(r.x + 9f * s, action.y - 83f * s, r.width - 18f * s, s));
            Label(new Rect(r.x, action.y - 76f * s, r.width, 28f * s),
                merchant.BuyMode ? "Цена покупки" : "Цена продажи", 17, TextAnchor.MiddleCenter);
            if (art.resourceIcons != null && art.resourceIcons.Length > 0)
                DrawIcon(new Rect(r.x + 16f * s, action.y - 44f * s, 32f * s, 32f * s), art.resourceIcons[0]);
            FitText(new Rect(r.x + 53f * s, action.y - 46f * s, r.width - 64f * s, 35f * s), merchantPriceLabel, 23, false);
            var execute = Button(action, merchant.BuyMode ? "Купить" : "Продать", false, merchant.CanExecute);
            Label(new Rect(r.x, action.yMax + 2f * s, r.width, 27f * s),
                merchant.CanExecute ? "" : merchant.UnavailableReason ?? "", 12, TextAnchor.MiddleCenter, true);
            return execute;
        }
    }
}
