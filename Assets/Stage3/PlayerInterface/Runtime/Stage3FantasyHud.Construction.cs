using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private struct BuildCard
        {
            public string Name, Price, Detail;
            public Sprite Icon;
            public bool Enabled, Selected;
            public Action Select;
        }

        public int BuildGroup => buildGroup;
        public void SetBuildGroup(int index)
        {
            index=Mathf.Clamp(index,0,2); if(index==buildGroup) return;
            game?.Building?.CancelPlacement(); game?.Production?.CancelPlacement(); CancelPlacementRequested?.Invoke();
            buildGroup=index; inspectedBuildIndex=0;
            legacyHud?.SelectBuildCategory(index==0?Stage2BuildCategory.Towers:index==1?Stage2BuildCategory.Generators:Stage2BuildCategory.Research);
        }
        public Rect BuildTabRect(int index)
        {
            var r=ConstructionContentRect; var width=Mathf.Min(180f*UiScale,(r.width*.68f-16f*UiScale)/3f);
            return new Rect(r.x+index*(width+5f*UiScale),r.y,width,38f*UiScale);
        }
        public Rect BuildChoiceRect(int index)
        {
            var count=buildGroup==0?Mathf.Max(1,game?.Building?.Defense?.RoleCount??3):buildGroup==1?Mathf.Max(1,game?.Production?.Catalog?.Entries.Count??3):2;
            var r=ConstructionContentRect;var total=r.width*.68f-16f*UiScale;var width=(total-(count-1)*8f*UiScale)/count;
            return new Rect(r.x+index*(width+8f*UiScale),r.y+44f*UiScale,width,r.height-44f*UiScale);
        }
        public bool ActivateBuildChoice(int index)
        {
            var cards=GetBuildCards(); if(index<0||index>=cards.Count||!cards[index].Enabled) return false;
            inspectedBuildIndex=index;cards[index].Select?.Invoke();return true;
        }
        private void DrawConstructionRibbon()
        {
            var s=UiScale;var r=ConstructionContentRect;
            var names=new[]{"Оборона","Производство","База"};
            for(var i=0;i<3;i++) if(Button(BuildTabRect(i),names[i],buildGroup==i)) SetBuildGroup(i);
            var cards=GetBuildCards();
            for(var i=0;i<cards.Count;i++)
            {
                var card=cards[i];var box=BuildChoiceRect(i);
                if(Button(box,"",card.Selected,card.Enabled)) {inspectedBuildIndex=i;card.Select?.Invoke();}
                var safe=ButtonContentRect(box);var iconSide=Mathf.Min(38f*s,safe.width*.30f);
                var color=GUI.color; if(!card.Enabled) GUI.color=new Color(.65f,.65f,.65f,.8f);
                DrawIcon(new Rect(safe.x,safe.y,iconSide,iconSide),card.Icon);GUI.color=color;
                FitPaleLabel(new Rect(safe.x+iconSide+5f*s,safe.y,safe.width-iconSide-5f*s,iconSide),card.Name,14);
                FitPaleLabel(new Rect(safe.x,safe.yMax-20f*s,safe.width,20f*s),card.Price,12);
                if(box.Contains(Event.current.mousePosition)) inspectedBuildIndex=i;
            }
            if(cards.Count==0) return;
            inspectedBuildIndex=Mathf.Clamp(inspectedBuildIndex,0,cards.Count-1);
            var selected=cards[inspectedBuildIndex];
            var details=new Rect(r.x+r.width*.69f,r.y,r.width*.31f,r.height);
            Label(new Rect(details.x,details.y,details.width,25f*s),selected.Name,21,TextAnchor.MiddleLeft);
            Label(new Rect(details.x,details.y+29f*s,details.width,24f*s),selected.Price,15);
            Label(new Rect(details.x,details.y+55f*s,details.width,35f*s),selected.Detail,13,TextAnchor.UpperLeft,true);
            Label(new Rect(details.x,details.yMax-48f*s,details.width,46f*s),
                "Клик по карточке · выбрать\nЛКМ · поставить · Esc · отменить",13,TextAnchor.MiddleLeft,true);
        }

        private List<BuildCard> GetBuildCards()
        {
            var result=new List<BuildCard>(3);
            if(game==null||legacyHud==null) return result;
            var canAct=!game.RunDriver.Run.IsPaused&&!game.RunDriver.Run.IsTerminal&&!legacyHud.DebugOpen;
            if(buildGroup==0)
            {
                var defense=game.Building.Defense;
                for(var i=0;i<defense.RoleCount;i++)
                {
                    var index=i;var entry=defense.GetCatalog(i);
                    result.Add(new BuildCard {Name=entry.RoleName, Price=BuildPrice(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),
                        Detail=entry.CanAttack?"На серой дороге":"Преграда · без атаки",Icon=art!=null&&art.defenseIcons!=null&&i<art.defenseIcons.Length?art.defenseIcons[i]:null,
                        Enabled=canAct&&legacyHud.CanSelectTower(i),Selected=defense.IsPlacing&&defense.SelectedRoleIndex==i,Select=()=>legacyHud.SelectTower(index)});
                }
            }
            else if(buildGroup==1 && game.Production!=null)
            {
                var production=game.Production;
                foreach(var entry in production.Catalog.Entries)
                {
                    var building=entry.Building;
                    result.Add(new BuildCard {Name=entry.DisplayName,Price=BuildPrice(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),Icon=entry.Sprite,
                        Detail=$"На отмеченной площадке · +{entry.OutputAmount} / {entry.IntervalSeconds:0.#} с",
                        Enabled=canAct&&legacyHud.CanSelectGenerator(building),Selected=production.IsPlacing&&production.SelectedBuilding==building,Select=()=>legacyHud.SelectGenerator(building)});
                }
            }
            else if(buildGroup==2)
            {
                var dwelling=game.GetComponent<Stage3Dwelling>();
                result.Add(new BuildCard {Name="Мастерская",Price=legacyHud.WorkshopStatus==Stage2WorkshopBuildStatus.Built?"Построена":"Бесплатно",Icon=art?.workshopIcon,
                    Detail="На зелёной земле · исследование у мастерской",Enabled=canAct&&legacyHud.CanSelectWorkshop,Selected=legacyHud.WorkshopStatus==Stage2WorkshopBuildStatus.Placing,Select=legacyHud.SelectWorkshop});
                result.Add(new BuildCard {Name="Жилище",Price=legacyHud.DwellingStatus==Stage2WorkshopBuildStatus.Built?"Построено":dwelling?.Catalog!=null?BuildPrice(dwelling.Catalog.woodCost,0,0,0):"—",Icon=art?.dwellingIcon,
                    Detail="На зелёной земле · E у входа днём",Enabled=canAct&&legacyHud.CanSelectDwelling,Selected=legacyHud.DwellingStatus==Stage2WorkshopBuildStatus.Placing,Select=legacyHud.SelectDwelling});
            }
            return result;
        }

        private static string BuildPrice(int wood,int stone,int metal,int gold)
        {
            var price="";
            void Add(int value,string label) {if(value<=0)return;if(price.Length>0)price+=" · ";price+=value+" "+label;}
            Add(gold,"зол.");Add(wood,"дер.");Add(stone,"кам.");Add(metal,"мет.");
            return price.Length==0?"Бесплатно":price;
        }
    }
}
