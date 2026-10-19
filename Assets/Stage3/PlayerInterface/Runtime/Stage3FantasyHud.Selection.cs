using System;
using System.Collections.Generic;
using Not3A.Stage2.Defense;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public struct Stage3SelectedBuildingStat
    {
        public string Label, Value;
        public Stage3SelectedBuildingStat(string label,string value) { Label=label;Value=value; }
    }

    /// <summary>Read-only presentation snapshot prepared by the scene binding from a live owned building.</summary>
    public sealed class Stage3SelectedBuildingView
    {
        public string Title, Subtitle;
        public Sprite Icon;
        public Stage3SelectedBuildingStat[] Stats;
        public bool ShowUpgrade, ShowSell, ShowResearch;
        public bool UpgradeAvailable, SellAvailable;
        public string UpgradePrice, SellRefund, UpgradePreview;
        public string UpgradeUnavailableReason, SellUnavailableReason, Hint;
        public Bounds WorldBounds;
        public Vector2 WorldPosition;
    }

    public sealed partial class Stage3FantasyHud
    {
        private readonly Dictionary<Stage2Tower,SpriteRenderer[]> towerBadgeRenderers=new Dictionary<Stage2Tower,SpriteRenderer[]>();
        private readonly Dictionary<int,string> towerLevelBadgeLabels=new Dictionary<int,string>();
        private readonly List<Stage2Tower> expiredTowerBadges=new List<Stage2Tower>();

        private void DrawTowerLevelBadges()
        {
            if(Event.current.type!=EventType.Repaint || IsModalOpen || scene?.CameraFollow?.BoundCamera==null) return;
            var defense=game?.Building?.Defense;if(defense==null) return;
            expiredTowerBadges.Clear();
            foreach(var pair in towerBadgeRenderers)
                if(pair.Key==null || !pair.Key.IsAlive) expiredTowerBadges.Add(pair.Key);
            foreach(var tower in expiredTowerBadges) towerBadgeRenderers.Remove(tower);
            var camera=scene.CameraFollow.BoundCamera;var s=UiScale;
            for(var i=0;i<defense.TowerCount;i++)
            {
                var tower=defense.GetTower(i);
                if(tower==null || !tower.IsAlive || !tower.gameObject.activeInHierarchy) continue;
                if(!towerBadgeRenderers.TryGetValue(tower,out var renderers))
                {
                    var native=tower.GetComponentInChildren<Stage3TowerSprite>(true);
                    renderers=native!=null?native.GetComponentsInChildren<SpriteRenderer>(true):tower.GetComponentsInChildren<SpriteRenderer>(true);
                    towerBadgeRenderers.Add(tower,renderers);
                }
                var left=float.PositiveInfinity;var right=float.NegativeInfinity;var top=float.PositiveInfinity;
                foreach(var renderer in renderers)
                {
                    if(renderer==null || !renderer.enabled || !renderer.gameObject.activeInHierarchy || renderer.sprite==null || renderer.color.a<=.01f) continue;
                    var bounds=renderer.sprite.bounds;
                    for(var corner=0;corner<4;corner++)
                    {
                        var local=new Vector3((corner&1)==0?bounds.min.x:bounds.max.x,(corner&2)==0?bounds.min.y:bounds.max.y,0);
                        if(renderer.flipX) local.x=-local.x;if(renderer.flipY) local.y=-local.y;
                        var point=camera.WorldToScreenPoint(renderer.transform.TransformPoint(local));
                        if(point.z<=0) continue;
                        left=Mathf.Min(left,point.x);right=Mathf.Max(right,point.x);top=Mathf.Min(top,Screen.height-point.y);
                    }
                }
                if(!float.IsFinite(top)) continue;
                var badge=new Rect((left+right)*.5f-24f*s,top-21f*s,48f*s,17f*s);
                if(badge.xMin<0 || badge.xMax>Screen.width || badge.yMin<0 || badge.yMax>WorldHudRect.y ||
                    badge.Overlaps(ResourcesRect) || badge.Overlaps(StatusRect)) continue;
                // Constant-cost metal bands replace the former per-pixel gradient scanlines.
                var edge=new Color(.18f,.22f,.27f);
                Fill(new Rect(badge.x+2f*s,badge.y,badge.width-4f*s,badge.height),edge);
                Fill(new Rect(badge.x,badge.y+2f*s,badge.width,badge.height-4f*s),edge);
                Fill(Inset(badge,s),new Color(.57f,.64f,.71f));
                Fill(new Rect(badge.x+2f*s,badge.y+2f*s,badge.width-4f*s,6f*s),new Color(.88f,.92f,.96f));
                Fill(new Rect(badge.x+2f*s,badge.y+8f*s,badge.width-4f*s,6f*s),new Color(.73f,.80f,.86f));
                Fill(new Rect(badge.x+3f*s,badge.y+s,badge.width-6f*s,s),Color.white);
                Fill(new Rect(badge.x+3f*s,badge.yMax-2f*s,badge.width-6f*s,s),new Color(.34f,.42f,.50f));
                Fill(new Rect(badge.x+s,badge.y+3f*s,s,badge.height-6f*s),new Color(.95f,.98f,1f));
                if(!towerLevelBadgeLabels.TryGetValue(tower.Level,out var levelLabel))
                { levelLabel="LVL "+tower.Level;towerLevelBadgeLabels.Add(tower.Level,levelLabel); }
                Label(badge,levelLabel,11,TextAnchor.MiddleCenter);
            }
        }

        private Stage3SelectedBuildingView selectedBuilding;
        public Stage3SelectedBuildingView SelectedBuildingView => selectedBuilding;
        public event Action SelectedBuildingCloseRequested;
        public event Action SelectedBuildingUpgradeRequested;
        public event Action SelectedBuildingSellRequested;
        public bool HasSelectedBuilding => selectedBuilding!=null &&
            (ActiveWindow==Stage3PlayerWindow.None || ActiveWindow==Stage3PlayerWindow.Construction);
        public Rect SelectedBuildingRect => HasSelectedBuilding
            ? new Rect(BottomRect.x,NavigationRibbonRect.y-164f*UiScale,BottomRect.width,164f*UiScale) : Rect.zero;
        public Rect SelectedBuildingCloseRect => !HasSelectedBuilding?Rect.zero:
            new Rect(SelectedBuildingRect.xMax-66f*UiScale,SelectedBuildingRect.y+10f*UiScale,30f*UiScale,30f*UiScale);
        public void SetSelectedBuilding(Stage3SelectedBuildingView value) => selectedBuilding=value;
        public void ClearSelectedBuilding() => selectedBuilding=null;

        public Rect SelectedBuildingResearchRect
        {
            get
            {
                if(!HasSelectedBuilding||!selectedBuilding.ShowResearch) return Rect.zero;
                return SelectedBuildingActionRect(0);
            }
        }
        public Rect SelectedBuildingUpgradeRect => !HasSelectedBuilding||!selectedBuilding.ShowUpgrade?Rect.zero:
            SelectedBuildingActionRect(selectedBuilding.ShowResearch?1:0);
        public Rect SelectedBuildingSellRect => !HasSelectedBuilding||!selectedBuilding.ShowSell?Rect.zero:
            SelectedBuildingActionRect((selectedBuilding.ShowResearch?1:0)+(selectedBuilding.ShowUpgrade?1:0));
        private Rect SelectedBuildingActionRect(int index)
        {
            var r=SelectedBuildingActionsRect;var width=r.width/SelectedBuildingActionCount;
            return new Rect(r.x+index*width,r.y,width-8f*UiScale,64f*UiScale);
        }
        private int SelectedBuildingActionCount => (selectedBuilding.ShowResearch?1:0)+(selectedBuilding.ShowUpgrade?1:0)+(selectedBuilding.ShowSell?1:0);
        private Rect SelectedBuildingActionsRect
        {
            get {var r=SelectedBuildingRect;return new Rect(r.x+r.width*.72f,r.y+42f*UiScale,r.width*.26f-10f*UiScale,102f*UiScale);}
        }

        private void DrawSelectedBuilding()
        {
            if(!HasSelectedBuilding) return;
            var view=selectedBuilding;var outer=SelectedBuildingRect;var s=UiScale;
            var r=Rect.MinMaxRect(outer.x+24f*s,outer.y+18f*s,outer.xMax-24f*s,outer.yMax-16f*s);
            var imageSide=Mathf.Min(130f*s,r.height);
            var icon=new Rect(r.x,r.center.y-imageSide*.5f,imageSide,imageSide);
            DrawIcon(Inset(icon,8f*s),view.Icon);DrawMedallion(icon);
            var title=new Rect(icon.xMax+16f*s,r.y+14f*s,outer.width*.27f-imageSide-46f*s,70f*s);
            Label(title,view.Title,21,TextAnchor.MiddleLeft);
            Label(new Rect(title.x,title.yMax,title.width,25f*s),view.Subtitle,16,TextAnchor.MiddleLeft,true);
            var stats=new Rect(outer.x+outer.width*.29f,r.y,outer.width*.40f,r.height-22f*s);
            DrawRule(new Rect(stats.x-12f*s,r.y+8f*s,1f*s,r.height-16f*s));
            var count=view.Stats?.Length??0;
            var rowHasGlyph=false;
            for(var i=0;i<count;i++)
            {
                var label=view.Stats[i].Label;
                rowHasGlyph|=label=="Урон"||label=="Дальность"||label=="Интервал"||label=="Здоровье";
            }
            for(var i=0;i<count;i++)
            {
                var cell=new Rect(stats.x+i*stats.width/count,stats.y,stats.width/count,stats.height);
                var glyphRect=new Rect(cell.center.x-23f*s,cell.y,46f*s,46f*s);
                TryDrawStatGlyph(glyphRect,view.Stats[i].Label);
                var textY=cell.y+(rowHasGlyph?48f:28f)*s;
                FitText(new Rect(cell.x+4f*s,textY,cell.width-8f*s,25f*s),view.Stats[i].Label,16,false);
                FitText(new Rect(cell.x+4f*s,textY+27f*s,cell.width-8f*s,30f*s),view.Stats[i].Value,23,false);
                if(i>0) DrawRule(new Rect(cell.x,cell.y+14f*s,1f*s,cell.height-20f*s),false);
            }
            var preview=string.IsNullOrEmpty(view.UpgradePreview)?view.Hint:view.UpgradePreview;
            if(!string.IsNullOrEmpty(preview))
                FitText(new Rect(stats.x,outer.yMax-33f*s,stats.width,20f*s),preview,12,false);
            var actions=SelectedBuildingActionsRect;var actionCount=SelectedBuildingActionCount;
            var researchRequested=false;var upgradeRequested=false;var sellRequested=false;
            if(actionCount>0)
            {
                DrawRule(new Rect(actions.x-12f*s,r.y+8f*s,1f*s,r.height-16f*s));
                if(view.ShowResearch)
                {
                    researchRequested=DrawBuildingAction(SelectedBuildingResearchRect,"Исследования",true,BuildingAction.Research);
                }
                if(view.ShowUpgrade) upgradeRequested=DrawPricedBuildingAction(SelectedBuildingUpgradeRect,"Улучшить",BuildingAction.Upgrade,
                    view.UpgradeAvailable,view.UpgradePrice,"Цена",view.UpgradeUnavailableReason);
                if(view.ShowSell) sellRequested=DrawPricedBuildingAction(SelectedBuildingSellRect,"Продать",BuildingAction.Sell,
                    view.SellAvailable,view.SellRefund,"Возврат",view.SellUnavailableReason);
            }
            if(CloseButton(SelectedBuildingCloseRect)) { ClearSelectedBuilding();SelectedBuildingCloseRequested?.Invoke(); }
            else if(researchRequested) SetWindow(Stage3PlayerWindow.Research);
            else if(upgradeRequested) SelectedBuildingUpgradeRequested?.Invoke();
            else if(sellRequested) SelectedBuildingSellRequested?.Invoke();
        }

        private enum BuildingAction { Research, Upgrade, Sell }
        private Sprite buildingUpgradeIcon;
        private bool buildingUpgradeIconLoaded;

        private bool DrawPricedBuildingAction(Rect button,string title,BuildingAction action,bool available,
            string amount,string amountLabel,string reason)
        {
            var s=UiScale;var clicked=DrawBuildingAction(button,title,available,action);
            var hasAmount=!string.IsNullOrEmpty(amount);
            if(hasAmount)
                Label(new Rect(button.x,button.yMax+2f*s,button.width,28f*s),amountLabel+": "+amount,11,TextAnchor.UpperCenter);
            if(!available)
                Label(new Rect(button.x,button.yMax+(hasAmount?28f:4f)*s,button.width,(hasAmount?18f:40f)*s),
                    string.IsNullOrEmpty(reason)?"Пока недоступно":reason,11,TextAnchor.UpperCenter,true);
            return clicked;
        }

        private bool DrawBuildingAction(Rect r,string title,bool enabled,BuildingAction action)
        {
            var s=UiScale;var savedEnabled=GUI.enabled;GUI.enabled=savedEnabled&&enabled;
            var clicked=SurfaceButton(r,false,false);GUI.enabled=savedEnabled;
            var circle=new Rect(r.x,r.center.y-30f*s,60f*s,60f*s);
            DrawDisc(Inset(circle,9f*s),enabled?PlumInk:new Color(.35f,.29f,.37f));
            var saved=GUI.color;GUI.color=enabled?Color.white:new Color(.85f,.83f,.85f,1);
            DrawMedallion(circle);
            if(action==BuildingAction.Sell) DrawIcon(Inset(circle,17f*s),art.resourceIcons[0]);
            else if(action==BuildingAction.Research) DrawIcon(Inset(circle,15f*s),art.menuIcons[2]);
            else
            {
                if(!buildingUpgradeIconLoaded)
                {
                    buildingUpgradeIcon=Resources.Load<Sprite>("Stage3UpgradeSilverCyan");
                    buildingUpgradeIconLoaded=true;
                }
                DrawIcon(Inset(circle,14f*s),buildingUpgradeIcon);
            }
            GUI.color=saved;
            var text=new Rect(circle.xMax+5f*s,r.y,r.width-circle.width-5f*s,r.height);
            if(enabled&&r.Contains(Event.current.mousePosition)) PaleLabel(text,title,17,TextAnchor.MiddleLeft);
            else Label(text,title,17,TextAnchor.MiddleLeft,!enabled);
            return clicked;
        }

        private void DrawSelectedBuildingWorldOutline()
        {
            if(!HasSelectedBuilding||scene?.CameraFollow?.BoundCamera==null) return;
            var camera=scene.CameraFollow.BoundCamera;var bounds=selectedBuilding.WorldBounds;
            if(bounds.size.sqrMagnitude<.0001f) return;
            var min=new Vector2(float.PositiveInfinity,float.PositiveInfinity);
            var max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            for(var i=0;i<8;i++)
            {
                var world=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                var point=camera.WorldToScreenPoint(world);if(point.z<=0) return;
                var gui=new Vector2(point.x,Screen.height-point.y);min=Vector2.Min(min,gui);max=Vector2.Max(max,gui);
            }
            var r=Rect.MinMaxRect(min.x-4f*UiScale,min.y-4f*UiScale,max.x+4f*UiScale,max.y+4f*UiScale);
            var length=Mathf.Min(16f*UiScale,Mathf.Min(r.width,r.height)*.25f);var line=2f*UiScale;
            GUI.BeginGroup(new Rect(0,0,Screen.width,WorldHudRect.y));
            for(var i=0;i<4;i++)
            {
                var right=(i&1)!=0;var bottom=(i&2)!=0;var x=right?r.xMax:r.x;var y=bottom?r.yMax:r.y;
                Fill(new Rect(right?x-length:x,bottom?y-line:y,length,line),Accent);
                Fill(new Rect(right?x-line:x,bottom?y-length:y,line,length),Accent);
            }
            GUI.EndGroup();
        }
    }
}
