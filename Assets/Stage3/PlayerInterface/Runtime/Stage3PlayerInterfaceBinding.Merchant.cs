using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage2Merchant merchant;
        private Stage3WorldTravel merchantTravel;
        private bool merchantDirty=true,merchantPointerNeedsRelease=true,merchantBuyMode=true;
        private int selectedTradeItem=-1;
        public Stage2Merchant Merchant=>merchant;
        public Stage2TradeResult LastTradeResult {get;private set;}
        public bool NearMerchant=>merchant!=null&&merchantTravel!=null&&merchantTravel.CityReady&&
            Vector2.Distance(scene.PlayerMover.transform.position,merchantTravel.CityPresentation.MerchantPosition)<=merchant.Catalog.InteractionRadius;
        private bool MerchantModeAllowed(bool ownWindow)=>CanManageEquipment&&isActiveAndEnabled&&Time.timeScale>0f&&
            merchantTravel!=null&&merchantTravel.CityReady&&!IsDraggingBuilding&&!game.Building.Defense.IsPlacing&&
            !(game.Production?.IsPlacing??false)&&!(GetComponent<Stage3Dwelling>()?.IsPlacing??false)&&
            !(forge?.IsPlacing??false)&&!merchantTravel.IsPlacingWorkshop&&!merchantTravel.IsPlacingBaseCircle&&
            (!game.PlayerInputBlocked||(ownWindow&&presentation.MerchantOpen));
        private void BindMerchant()
        {
            if(equipmentDriver==null)return;
            var catalog=Resources.Load<Stage2MerchantCatalog>("Stage3Merchant");
            if(catalog==null||!catalog.TryValidate(out _))
            {Debug.LogError("S3_MERCHANT_BINDING_ERROR: valid Stage3Merchant data required.");return;}
            merchantTravel=GetComponent<Stage3WorldTravel>();
            merchant=new Stage2Merchant(equipmentDriver.Rules,catalog);
            merchant.Changed+=MarkMerchantDirty;
            equipmentDriver.Rules.Changed+=MarkMerchantDirty;
            game.Gathering.Economy.Inventory.Changed+=MarkMerchantDirty;
            game.RunDriver.Run.Reset+=ResetMerchant;
            presentation.MerchantItemSelected+=SelectTradeItem;
            presentation.MerchantTradeRequested+=ConfirmMerchantTrade;
            presentation.MerchantClosed+=OnMerchantClosed;
            Debug.Log("S3_MERCHANT_READY: finite gold-only buy/sell, same item instances, current stock survives retry.");
        }
        private void MarkMerchantDirty()=>merchantDirty=true;
        private void ResetMerchant()
        {presentation.ShowMerchant(false);merchant.Reset();selectedTradeItem=-1;merchantPointerNeedsRelease=true;}
        private void OnMerchantClosed(){merchantPointerNeedsRelease=true;selectedTradeItem=-1;}
        public bool TryOpenMerchant()
        {
            if(merchant==null||!MerchantModeAllowed(false))return false;
            ClearBuildingSelection();CancelBlink();CancelPlacement();
            merchantBuyMode=true;selectedTradeItem=merchant.Items.Count>0?merchant.Items[0].Id:-1;
            merchantPointerNeedsRelease=true;presentation.ShowMerchant(true);RefreshMerchant();return true;
        }
        public bool TryOpenMerchantAtScreen(Vector2 pointer)
        {
            if(merchantTravel==null||!merchantTravel.CityReady||game.ContainsScreenPoint(pointer)||
                !TryHitBuildingVisual(scene.CameraFollow.BoundCamera,merchantTravel.CityPresentation.ShopVisual,pointer,out _))return false;
            return TryOpenMerchant();
        }
        private void SelectTradeItem(bool buy,int id)
        {
            if(!presentation.MerchantOpen||merchant==null)return;
            var item=buy?merchant.Find(id):equipmentDriver.Rules.Find(id);
            var paper=buy?merchant.FindBlueprint(id):Blueprints?.Find(id);
            if(item==null&&paper==null||!buy&&item!=null&&equipmentDriver.Rules.IsEquipped(item))return;
            merchantBuyMode=buy;selectedTradeItem=id;RefreshMerchant();
        }
        private void ConfirmMerchantTrade(bool buy,int id)=>TryMerchantTrade(buy,id);
        public bool TryMerchantTrade(bool buy,int id)
        {
            if(merchant==null||!presentation.MerchantOpen||buy!=merchantBuyMode||id!=selectedTradeItem||
                !MerchantModeAllowed(true)||!NearMerchant)
            {LastTradeResult=Stage2TradeResult.Unavailable;MarkMerchantDirty();return false;}
            var paper=buy?merchant.FindBlueprint(id):Blueprints?.Find(id);
            LastTradeResult=paper!=null?merchant.TryTradeBlueprint(buy,id):merchant.TryTrade(buy,id);
            if(LastTradeResult==Stage2TradeResult.Ready)selectedTradeItem=-1;
            MarkEquipmentDirty();RefreshMerchant();return LastTradeResult==Stage2TradeResult.Ready;
        }
        private void UpdateMerchant()
        {
            if(merchant==null)return;
            var mouse=Mouse.current;var held=mouse!=null&&mouse.leftButton.isPressed;
            if(!held)merchantPointerNeedsRelease=false;
            if(presentation.MerchantOpen)
            {
                if(!CanManageEquipment||merchantTravel==null||!merchantTravel.CityReady)
                {presentation.ShowMerchant(false);return;}
                if(merchantDirty)RefreshMerchant();
                UpdateMerchantAvailability();return;
            }
            if(!merchantPointerNeedsRelease&&mouse!=null&&mouse.leftButton.wasPressedThisFrame)
                TryOpenMerchantAtScreen(mouse.position.ReadValue());
        }
        private void RefreshMerchant()
        {
            if(merchant==null)return;merchantDirty=false;
            var bag=new List<Stage3TradeItemView>();var stock=new List<Stage3TradeItemView>();
            foreach(var item in equipmentDriver.Rules.Items)
                if(!equipmentDriver.Rules.IsEquipped(item))bag.Add(new Stage3TradeItemView{Item=ItemView(item),Price=merchant.Catalog.SellPrice(item)});
            foreach(var item in merchant.Items)stock.Add(new Stage3TradeItemView{Item=ItemView(item),Price=merchant.Catalog.BuyPrice(item)});
            if(Blueprints!=null)
            {
                foreach(var paper in Blueprints.Items)bag.Add(new Stage3TradeItemView{Item=BlueprintItemView(paper),Price=merchant.BlueprintPrice(paper,false)});
                foreach(var paper in merchant.BlueprintItems)stock.Add(new Stage3TradeItemView{Item=BlueprintItemView(paper),Price=merchant.BlueprintPrice(paper,true)});
            }
            presentation.SetMerchant(new Stage3MerchantView{PlayerItems=bag.ToArray(),MerchantItems=stock.ToArray(),
                Gold=game.Gathering.Economy.Inventory.Gold,SelectedItemId=selectedTradeItem,BuyMode=merchantBuyMode});
            UpdateMerchantAvailability();
        }
        private void UpdateMerchantAvailability()
        {
            var view=presentation.MerchantView;if(view==null)return;
            var paper=merchantBuyMode?merchant.FindBlueprint(selectedTradeItem):Blueprints?.Find(selectedTradeItem);
            var result=paper!=null?merchant.EvaluateBlueprint(merchantBuyMode,selectedTradeItem):merchant.Evaluate(merchantBuyMode,selectedTradeItem);
            view.CanExecute=MerchantModeAllowed(true)&&NearMerchant&&result==Stage2TradeResult.Ready;
            view.UnavailableReason=!MerchantModeAllowed(true)?"Торговля сейчас недоступна":!NearMerchant?"Подойдите к торговцу у входа в лавку":TradeReason(result);
        }
        public static string TradeReason(Stage2TradeResult result)=>result switch
        {
            Stage2TradeResult.Ready=>null,
            Stage2TradeResult.InsufficientGold=>"Недостаточно золота",
            Stage2TradeResult.EquippedItem=>"Сначала снимите предмет",
            Stage2TradeResult.GoldLimit=>"Достигнут предел золота",
            Stage2TradeResult.InvalidItem=>"Выберите предмет",
            _=>"Торговля сейчас недоступна"
        };
        private void UnbindMerchant()
        {
            if(merchant==null)return;
            merchant.Changed-=MarkMerchantDirty;equipmentDriver.Rules.Changed-=MarkMerchantDirty;
            game.Gathering.Economy.Inventory.Changed-=MarkMerchantDirty;game.RunDriver.Run.Reset-=ResetMerchant;
            presentation.MerchantItemSelected-=SelectTradeItem;presentation.MerchantTradeRequested-=ConfirmMerchantTrade;
            presentation.MerchantClosed-=OnMerchantClosed;presentation.SetMerchant(null);
        }
    }
}
