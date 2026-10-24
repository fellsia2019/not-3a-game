using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private struct BuildCard
        {
            public string Name, Price, Detail, Deficit, Trait;
            public Stage3SelectedBuildingStat[] Stats;
            public Sprite Icon;
            public bool Enabled, Selected, InsufficientResources;
            public Action Select;
        }

        public const int BuildPageSize = 3;
        private int buildPage;
        public int BuildGroup => buildGroup;
        public int BuildCardCount => GetAllBuildCards().Count;
        public int BuildPageCount => Mathf.Max(1, (BuildCardCount + BuildPageSize - 1) / BuildPageSize);
        public int BuildPage => Mathf.Clamp(buildPage, 0, BuildPageCount - 1);
        public int VisibleBuildCardCount => GetBuildCards().Count;
        public void SetBuildPage(int page)
        {
            page=Mathf.Clamp(page,0,BuildPageCount-1); if(page==BuildPage) return;
            game?.Building?.CancelPlacement(); game?.Production?.CancelPlacement(); CancelPlacementRequested?.Invoke();
            buildPage=page; inspectedBuildIndex=0;
        }
        public void SetBuildGroup(int index)
        {
            index=Mathf.Clamp(index,0,2); if(index==buildGroup) return;
            game?.Building?.CancelPlacement(); game?.Production?.CancelPlacement(); CancelPlacementRequested?.Invoke();
            buildGroup=index; buildPage=0; inspectedBuildIndex=0;
            legacyHud?.SelectBuildCategory(index==0?Stage2BuildCategory.Towers:index==1?Stage2BuildCategory.Generators:Stage2BuildCategory.Research);
        }
        public Rect BuildTabRect(int index)
        {
            var r=ConstructionContentRect;var reserve=BuildPageCount>1?128f*UiScale:0f;
            var width=(r.width-reserve-12f*UiScale)/3f;
            return new Rect(r.x+index*(width+6f*UiScale),r.y,width,39f*UiScale);
        }
        public Rect BuildChoiceRect(int index)
        {
            var count=VisibleBuildCardCount;
            if(index<0||index>=count)return Rect.zero;
            var r=ConstructionContentRect;var total=r.width;var width=(total-2*12f*UiScale)/3f;
            return new Rect(r.x+index*(width+12f*UiScale),r.y+55f*UiScale,width,r.height-55f*UiScale);
        }
        public bool ActivateBuildChoice(int index)
        {
            var cards=GetBuildCards(); if(index<0||index>=cards.Count) return false;
            inspectedBuildIndex=index;
            if(!cards[index].Enabled)return false;
            if(!cards[index].Selected)cards[index].Select?.Invoke();
            return true;
        }
        public Rect BuildPageArrowRect(bool next)
        {
            var r=ConstructionContentRect;var s=UiScale;
            return new Rect(r.xMax-(next?34f:118f)*s,r.y+3f*s,32f*s,30f*s);
        }
        private Vector2 buildTouchStart;
        private bool trackingBuildTouch;
        private int suppressBuildClickThroughFrame=-1;
        private void UpdateConstructionPagingInput()
        {
            if(ActiveWindow!=Stage3PlayerWindow.Construction){trackingBuildTouch=false;return;}
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                var p=mouse.position.ReadValue();p.y=Screen.height-p.y;
                var wheel=mouse.scroll.ReadValue().y;
                if(ConstructionContentRect.Contains(p)&&Mathf.Abs(wheel)>.1f)SetBuildPage(BuildPage+(wheel<0?1:-1));
            }
            var touch=Touchscreen.current?.primaryTouch;
            if(touch==null)return;
            var point=touch.position.ReadValue();point.y=Screen.height-point.y;
            if(touch.press.wasPressedThisFrame){buildTouchStart=point;trackingBuildTouch=ConstructionContentRect.Contains(point);}
            if(touch.press.wasReleasedThisFrame&&trackingBuildTouch)
            {
                var delta=point-buildTouchStart;
                if(Mathf.Abs(delta.x)>48f*UiScale&&Mathf.Abs(delta.x)>Mathf.Abs(delta.y))
                { SetBuildPage(BuildPage+(delta.x<0?1:-1));suppressBuildClickThroughFrame=Time.frameCount+1; }
                trackingBuildTouch=false;
            }
        }
        private void DrawConstructionRibbon()
        {
            var s=UiScale;var r=ConstructionContentRect;
            var names=new[]{"Оборона","Производство","База"};
            for(var i=0;i<3;i++) if(DrawConstructionTab(BuildTabRect(i),names[i],buildGroup==i))SetBuildGroup(i);
            if(BuildPageCount>1)
            {
                if(SurfaceButton(BuildPageArrowRect(false),false,true))SetBuildPage(BuildPage-1);
                if(SurfaceButton(BuildPageArrowRect(true),false,true))SetBuildPage(BuildPage+1);
                Label(BuildPageArrowRect(false),"‹",23,TextAnchor.MiddleCenter);
                Label(BuildPageArrowRect(true),"›",23,TextAnchor.MiddleCenter);
                Label(new Rect(r.xMax-85f*s,r.y,50f*s,36f*s),(BuildPage+1)+" / "+BuildPageCount,13,TextAnchor.MiddleCenter);
            }
            var cards=GetBuildCards();
            // A single footer allocation keeps all three cells fixed when a deficit appears.
            var costRows=1;
            for(var i=0;i<cards.Count;i++)costRows=Mathf.Max(costRows,BuildCostRowCount(cards[i].Price,BuildChoiceRect(i).width-20f*s));
            for(var i=0;i<cards.Count;i++)
            {
                var card=cards[i];var box=BuildChoiceRect(i);var safe=Inset(box,8f*s);
                var hover=box.Contains(CurrentGuiPointer);
                if(hover)inspectedBuildIndex=i;
                DrawHudPlainTile(box,card.InsufficientResources?new Color(1f,.50f,.43f,.10f):Color.clear);
                if(card.Selected)DrawReferenceState(Inset(box,2f*s),1,1f);
                else if(hover)DrawReferenceState(Inset(box,2f*s),0,1f);
                var costHeight=costRows*32f*s;var titleHeight=48f*s;
                var artHeight=safe.height-titleHeight-costHeight;
                var well=new Rect(safe.x,safe.y,safe.width,Mathf.Max(36f*s,artHeight));
                if(card.InsufficientResources)
                {
                    DrawHudCoralWell(well);
                }
                DrawBuildArtwork(Inset(well,3f*s),card.Icon);
                var title=new Rect(safe.x,well.yMax+1f*s,safe.width,titleHeight-2f*s);
                HudReferenceLabel(title,card.Name,20,TextAnchor.MiddleCenter,true);
                DrawHudKeycap(new Rect(box.x+8f*s,box.y+8f*s,30f*s,31f*s),(i+1).ToString(),true,21);
                DrawBuildCostFlow(new Rect(safe.x,safe.yMax-costHeight,safe.width,costHeight),card.Price,card.InsufficientResources);
                if(card.InsufficientResources)
                {
                    var alert=new Rect(box.xMax-43f*s,box.y+8f*s,34f*s,34f*s);
                    DrawHudRubyAlert(alert);
                }
                var enabled=GUI.enabled;GUI.enabled=enabled&&card.Enabled&&Time.frameCount>suppressBuildClickThroughFrame;
                if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
                if(GUI.Button(box,GUIContent.none,blankButton)){inspectedBuildIndex=i;card.Select?.Invoke();}
                GUI.enabled=enabled;
            }
            DrawConstructionDetails(cards);
            if(cards.Count==0)Label(new Rect(r.x,r.y+50f*s,r.width,r.height-50f*s),"Нет изученных построек в этой категории",16,TextAnchor.MiddleCenter,true);
        }

        private bool DrawConstructionTab(Rect r,string text,bool selected)
        {
            var s=UiScale;
            if(selected)DrawHudSkinPart(r,1);
            else
            {
                DrawHudPlainTile(r,Color.clear);
                Fill(new Rect(r.x+11f*s,r.yMax-5f*s,r.width-22f*s,2f*s),new Color(.62f,.52f,.47f,.55f));
            }
            HudReferenceLabel(Inset(r,5f*s),text,20,TextAnchor.MiddleCenter,true,selected?new Color(1f,.97f,.92f):HudReferenceInk);
            if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
            return GUI.Button(r,GUIContent.none,blankButton);
        }

        public Rect ConstructionDetailsRect => Rect.MinMaxRect(MinimapRect.xMax+24f*UiScale,BottomRect.y+18f*UiScale,
            ConstructionContentRect.x-24f*UiScale,BottomRect.yMax-18f*UiScale);
        public string InspectedBuildName
        {
            get {var cards=GetBuildCards();return cards.Count==0?null:cards[Mathf.Clamp(inspectedBuildIndex,0,cards.Count-1)].Name;}
        }
        public string InspectedBuildDetail { get; private set; }
        public string InspectedBuildDeficit { get; private set; }
        public string InspectedBuildDetailText { get; private set; }
        public Rect ConstructionDetailsTextRect { get; private set; }
        private void DrawConstructionDetails(List<BuildCard> cards)
        {
            var r=ConstructionDetailsRect;var s=UiScale;
            DrawRule(new Rect(r.xMax+12f*s,r.y+5f*s,s,r.height-10f*s),false);
            InspectedBuildDetail=null;InspectedBuildDeficit=null;InspectedBuildDetailText=null;ConstructionDetailsTextRect=Rect.zero;
            if(cards.Count==0)return;
            inspectedBuildIndex=Mathf.Clamp(inspectedBuildIndex,0,cards.Count-1);
            var card=cards[inspectedBuildIndex];
            var preview=new Rect(r.x,r.y+8f*s,164f*s,r.height-39f*s);
            DrawHudPlainTile(preview,Color.clear);
            DrawBuildArtwork(Inset(preview,8f*s),card.Icon);
            var text=Rect.MinMaxRect(preview.xMax+18f*s,r.y,r.xMax,r.yMax-31f*s);
            ConstructionDetailsTextRect=text;
            var titleStyle=GetLabelStyle(27,false);var oldFont=titleStyle.fontStyle;titleStyle.fontStyle=FontStyle.Bold;
            var titleHeight=Mathf.Max(38f*s,titleStyle.CalcHeight(LabelContent(card.Name),text.width));titleStyle.fontStyle=oldFont;
            HudReferenceLabel(new Rect(text.x,text.y,text.width,titleHeight),card.Name,27,TextAnchor.UpperLeft,true);
            var count=card.Stats?.Length??0;var rowHeight=28f*s;var y=text.y+titleHeight+7f*s;
            for(var i=0;i<count;i++)
            {
                var row=new Rect(text.x,y+i*rowHeight,text.width,rowHeight);
                var label=card.Stats[i].Label=="Темп"?"Скорострельность":card.Stats[i].Label;
                HudReferenceLabel(new Rect(row.x,row.y,row.width*.67f,row.height),label,18,TextAnchor.MiddleLeft,false);
                HudReferenceLabel(new Rect(row.x+row.width*.68f,row.y,row.width*.32f,row.height),card.Stats[i].Value,19,TextAnchor.MiddleRight,true);
                Fill(new Rect(row.x,row.yMax-s,row.width,s),new Color(.66f,.57f,.51f,.22f));
            }
            var traitY=y+count*rowHeight+7f*s;
            if(!string.IsNullOrEmpty(card.Trait))
            {HudReferenceLabel(new Rect(text.x,traitY,text.width,24f*s),card.Trait,17,TextAnchor.UpperLeft,true);traitY+=25f*s;}
            InspectedBuildDetail=card.Detail;
            InspectedBuildDeficit=card.InsufficientResources?card.Deficit:null;
            var detail=card.Detail;
            var detailStyle=GetLabelStyle(16,false);
            var detailHeight=Mathf.Max(20f*s,detailStyle.CalcHeight(LabelContent(detail??""),text.width));
            HudReferenceLabel(new Rect(text.x,traitY,text.width,detailHeight),detail,16,TextAnchor.UpperLeft,false);
            if(card.InsufficientResources&&!string.IsNullOrEmpty(card.Deficit))
            {
                HudReferenceLabel(new Rect(text.x,traitY+detailHeight,text.width,Mathf.Max(23f*s,text.yMax-traitY-detailHeight)),
                    card.Deficit,16,TextAnchor.UpperLeft,true,new Color(.72f,.04f,.08f));
                detail=(detail=="Недостаточно ресурсов"?"":detail+"\n")+card.Deficit;
            }
            var hint=new Rect(r.x,r.yMax-23f*s,r.width,23f*s);
            Fill(new Rect(hint.x,hint.y-5f*s,hint.width,s),new Color(.66f,.57f,.51f,.25f));
            var hintText=buildGroup==0?"Shift + клик · повторить постройку":"1–3 · выбрать постройку";
            HudReferenceLabel(hint,hintText,15,TextAnchor.MiddleCenter,false);
            InspectedBuildDetailText=card.Trait+"\n"+detail+"\n"+hintText;
        }

        private void DrawBuildArtwork(Rect area,Sprite sprite)
        {
            if(sprite==null)return;
            // UI-only alpha>8 bounds measured on the original production PNGs,
            // with4px safety padding. Source assets, pivots and world sprites are unchanged.
            var crop=sprite.texture.name switch
            {
                "sawmill"=>new Rect(24f,160f,1223f,974f),
                "quarry"=>new Rect(57f,208f,1149f,872f),
                "iron_mine"=>new Rect(95f,58f,1145f,1133f),
                "RapidGridFacing"=>new Rect(33f,14f,957f,1431f),
                "LongshotGridFacing"=>new Rect(78f,180f,1098f,1023f),
                "WoodenBarricadeA"=>new Rect(196f,167f,1145f,685f),
                "ResearchWorkshop"=>new Rect(167f,7f,1075f,1168f),
                "DwellingB"=>new Rect(175f,50f,974f,1099f),
                _=>Rect.zero
            };
            if(crop.width<=0){DrawIcon(area,sprite);return;}
            var source=sprite.rect;
            var uv=new Rect((source.x+crop.x)/sprite.texture.width,
                (source.y+source.height-crop.yMax)/sprite.texture.height,
                crop.width/sprite.texture.width,crop.height/sprite.texture.height);
            var scale=Mathf.Min(area.width/crop.width,area.height/crop.height);
            var size=crop.size*scale;
            DrawUiTexture(new Rect(area.center-size*.5f,size),sprite.texture,uv);
        }

        private int BuildCostRowCount(string price,float availableWidth)
        {
            var style=GetLabelStyle(22,false);var oldFont=style.fontStyle;style.fontStyle=FontStyle.Bold;var width=0f;var rows=1;
            foreach(var token in price.Split(new[]{" · "},StringSplitOptions.None))
            {
                var split=token.IndexOf(' ');var suffix=split<0?"":token.Substring(split+1);
                var resource=suffix=="зол."||suffix=="дер."||suffix=="кам."||suffix=="мет.";
                var number=resource?token.Substring(0,split):token;
                var item=Mathf.Ceil(style.CalcSize(new GUIContent(number)).x)+4f*UiScale+(resource?36f*UiScale:0f);
                if(width>0f&&width+14f*UiScale+item>availableWidth){rows++;width=0f;}
                width+=(width>0f?14f*UiScale:0f)+item;
            }
            style.fontStyle=oldFont;return rows;
        }

        private void DrawBuildCostFlow(Rect r,string price,bool insufficientResources)
        {
            var parts=price.Split(new[]{" · "},StringSplitOptions.None);
            var style=GetLabelStyle(22,false);var oldFont=style.fontStyle;style.fontStyle=FontStyle.Bold;var gap=14f*UiScale;var iconSide=32f*UiScale;
            var numbers=new string[parts.Length];var icons=new int[parts.Length];var widths=new float[parts.Length];
            for(var i=0;i<parts.Length;i++)
            {
                var token=parts[i];var split=token.IndexOf(' ');
                var suffix=split<0?"":token.Substring(split+1);
                icons[i]=suffix=="зол."?0:suffix=="дер."?1:suffix=="кам."?2:suffix=="мет."?3:-1;
                numbers[i]=icons[i]<0?token:token.Substring(0,split);
                widths[i]=Mathf.Ceil(style.CalcSize(new GUIContent(numbers[i])).x)+4f*UiScale+(icons[i]<0?0:iconSide+4f*UiScale);
            }
            style.fontStyle=oldFont;
            var first=0;var y=r.y;
            while(first<parts.Length)
            {
                var end=first;var width=0f;
                while(end<parts.Length)
                {
                    if(end>first&&width+gap+widths[end]>r.width)break;
                    width+=(end>first?gap:0)+widths[end];end++;
                }
                var x=r.center.x-width*.5f;
                for(var i=first;i<end;i++)
                {
                    var textX=x;
                    if(icons[i]>=0&&art.resourceIcons!=null&&icons[i]<art.resourceIcons.Length)
                    {DrawIcon(new Rect(x,y,iconSide,iconSide),art.resourceIcons[icons[i]]);textX+=iconSide+4f*UiScale;}
                    var textRect=new Rect(textX,y,x+widths[i]-textX,32f*UiScale);
                    HudReferenceLabel(textRect,numbers[i],22,TextAnchor.MiddleCenter,true,
                        insufficientResources&&icons[i]>=0?new Color(.72f,.04f,.08f):HudReferenceInk);
                    x+=widths[i]+gap;
                }
                first=end;y+=32f*UiScale;
            }
        }

        private List<BuildCard> GetBuildCards()
        {
            var cards=GetAllBuildCards();
            buildPage=Mathf.Clamp(buildPage,0,Mathf.Max(0,(cards.Count-1)/BuildPageSize));
            var start=buildPage*BuildPageSize;
            return cards.GetRange(start,Mathf.Min(BuildPageSize,cards.Count-start));
        }

        private List<BuildCard> GetAllBuildCards()
        {
            var result=new List<BuildCard>(4);
            if(game==null||legacyHud==null) return result;
            Stage3SelectedBuildingStat Stat(string label,string value)=>new Stage3SelectedBuildingStat(label,value);
            string Number(float value)=>value.ToString("0.##",System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
            var canAct=!game.RunDriver.Run.IsPaused&&!game.RunDriver.Run.IsTerminal&&!legacyHud.DebugOpen;
            if(buildGroup==0)
            {
                var defense=game.Building.Defense;
                for(var i=0;i<defense.RoleCount;i++)
                {
                    if(!defense.IsRoleAvailable(i))continue;
                    var index=i;var entry=defense.GetCatalog(i);
                    result.Add(new BuildCard {Name=entry.RoleName,
                        Stats=entry.CanAttack?new[]{Stat("HP",entry.TowerHealth.ToString()),Stat("Урон",entry.AttackDamage.ToString()),Stat("Радиус",Number(entry.AttackRange)),Stat("Темп",Number(1f/entry.FireInterval)+"/с")}:
                            new[]{Stat("HP",entry.TowerHealth.ToString())},
                        Trait=entry.CanAttack?(entry.SplashRadiusCells>0?"Сплеш · R "+Number(entry.SplashRadiusCells):"Одиночная цель"):"Преграда · без атаки",
                        Deficit=BuildDeficit(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost), Price=BuildPrice(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),
                        InsufficientResources=!game.Gathering.Economy.Inventory.CanAfford(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),
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
                    if(!production.Rules.IsBuildingAvailable(building))continue;
                    result.Add(new BuildCard {Name=entry.DisplayName,
                        Stats=new[]{Stat("За цикл","+"+entry.OutputAmount),Stat("Интервал",Number(entry.IntervalSeconds)+" с")},
                        Trait="Автодобыча",Deficit=BuildDeficit(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),Price=BuildPrice(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),Icon=entry.Sprite,
                        InsufficientResources=!game.Gathering.Economy.Inventory.CanAfford(entry.WoodCost,entry.StoneCost,entry.MetalCost,entry.GoldCost),
                        Detail=$"На отмеченной площадке · +{entry.OutputAmount} / {entry.IntervalSeconds:0.#} с",
                        Enabled=canAct&&legacyHud.CanSelectGenerator(building),Selected=production.IsPlacing&&production.SelectedBuilding==building,Select=()=>legacyHud.SelectGenerator(building)});
                }
            }
            else if(buildGroup==2)
            {
                var dwelling=game.GetComponent<Stage3Dwelling>();
                var travel=game.GetComponent<Stage3WorldTravel>();
                var workshopBuilt=legacyHud.WorkshopStatus==Stage2WorkshopBuildStatus.Built;
                result.Add(new BuildCard {Name="Исследовательский центр",Price=workshopBuilt?"Построен":travel!=null?BuildPrice(travel.WorkshopWoodCost,travel.WorkshopStoneCost,0,0):"—",Icon=art?.workshopIcon,
                    Stats=new[]{Stat("Назначение","Чертежи")},Trait="Изучение чертежей",
                    InsufficientResources=!workshopBuilt&&travel!=null&&!travel.CanAffordWorkshop,
                    Deficit=!workshopBuilt&&travel!=null?BuildDeficit(travel.WorkshopWoodCost,travel.WorkshopStoneCost,0,0):null,
                    Detail="На зелёной земле · изучение чертежей у центра",Enabled=canAct&&legacyHud.CanSelectWorkshop,Selected=legacyHud.WorkshopStatus==Stage2WorkshopBuildStatus.Placing,Select=legacyHud.SelectWorkshop});
                var dwellingBuilt=legacyHud.DwellingStatus==Stage2WorkshopBuildStatus.Built;
                result.Add(new BuildCard {Name="Жилище",Price=dwellingBuilt?"Построено":dwelling?.Catalog!=null?BuildPrice(dwelling.Catalog.woodCost,dwelling.Catalog.stoneCost,0,0):"—",Icon=art?.dwellingIcon,
                    Stats=new[]{Stat("Отдых","До ночи")},Trait="Сон до 19:50",
                    InsufficientResources=!dwellingBuilt&&dwelling?.Catalog!=null&&!game.Gathering.Economy.Inventory.CanAfford(dwelling.Catalog.woodCost,dwelling.Catalog.stoneCost,0,0),
                    Deficit=!dwellingBuilt&&dwelling?.Catalog!=null?BuildDeficit(dwelling.Catalog.woodCost,dwelling.Catalog.stoneCost,0,0):null,
                    Detail=game.RunDriver.Run.UsesDaytimeGates?"На зелёной земле · сон до начала ночи":"На зелёной земле · E у входа днём",Enabled=canAct&&legacyHud.CanSelectDwelling,Selected=legacyHud.DwellingStatus==Stage2WorkshopBuildStatus.Placing,Select=legacyHud.SelectDwelling});
                var forge=game.GetComponent<Stage3Forge>();
                if(forge!=null&&forge.Catalog!=null&&forge.ConstructionUnlocked)
                {
                    var cost=forge.Catalog.BuildCost;
                    result.Add(new BuildCard {Name="Кузница",Stats=new[]{Stat("Назначение","Ковка")},Trait="Создание снаряжения",Deficit=!forge.IsBuilt?BuildDeficit(cost.Wood,cost.Stone,cost.Metal,0):null,Price=forge.IsBuilt?"Построена":BuildPrice(cost.Wood,cost.Stone,cost.Metal,0),Icon=forge.Art?.worldSprite,
                        InsufficientResources=!forge.IsBuilt&&!game.Gathering.Economy.Inventory.CanAfford(cost.Wood,cost.Stone,cost.Metal,0),
                        Detail=!forge.ConstructionUnlocked?"Изучите чертёж кузницы в исследовательском центре":"На зелёной земле · E у входа · ковка предметов",Enabled=canAct&&forge.CanBeginPlacement,Selected=forge.IsPlacing,Select=forge.BeginPlacement});
                }
                if(travel!=null&&travel.IsResearched)
                    result.Add(new BuildCard {Name="Круг телепортации",Stats=new[]{Stat("Переход","К открытым точкам")},Trait="Быстрое перемещение",Price=travel.BaseBuilt?"Построен":"Бесплатно",Icon=art?.teleportIcon,
                        Detail="На зелёной земле · переходы к открытым точкам",Enabled=canAct&&travel.CanBeginBaseCirclePlacement,
                        Selected=travel.IsPlacingBaseCircle,Select=travel.BeginBaseCirclePlacement});
            }
            return result;
        }

        private string BuildDeficit(int wood,int stone,int metal,int gold)
        {
            var stock=game.Gathering.Economy.Inventory;var parts=new List<string>(4);
            string Plural(int amount,string one,string few,string many)
            {
                var lastHundred=amount%100;var last=amount%10;
                return lastHundred>=11&&lastHundred<=14?many:last==1?one:last>=2&&last<=4?few:many;
            }
            var missingWood=Mathf.Max(0,wood-stock.Get(MapResourceKind.Wood));
            var missingStone=Mathf.Max(0,stone-stock.Get(MapResourceKind.Stone));
            var missingMetal=Mathf.Max(0,metal-stock.Get(MapResourceKind.Metal));
            var missingGold=Mathf.Max(0,gold-stock.Gold);
            if(missingWood>0)parts.Add(missingWood+" "+Plural(missingWood,"бревно","бревна","брёвен"));
            if(missingStone>0)parts.Add(missingStone+" "+Plural(missingStone,"камень","камня","камней"));
            if(missingMetal>0)parts.Add(missingMetal+" ед. металла");
            if(missingGold>0)parts.Add(missingGold+" зол.");
            return parts.Count==0?null:"Нужно ещё "+string.Join(", ",parts);
        }

        private static string BuildPrice(int wood,int stone,int metal,int gold)
        {
            var price="";
            void Add(int value,string label) {if(value<=0)return;if(price.Length>0)price+=" · ";price+=value+" "+label;}
            Add(gold,"зол.");Add(wood,"дер.");Add(stone,"кам.");Add(metal,"мет.");
            return price.Length==0?"Бесплатно":price;
        }

        private static string CompactBuildPrice(string price)
        {
            if(string.IsNullOrEmpty(price))return "";
            price=price.Replace("дер.","дер").Replace("кам.","кам").Replace("мет.","мет").Replace("зол.","зол");
            var first=price.IndexOf(" · ",StringComparison.Ordinal);
            if(first<0)return price;
            var second=price.IndexOf(" · ",first+3,StringComparison.Ordinal);
            return second<0?price:price.Substring(0,second)+"\n"+price.Substring(second+3);
        }
    }
}
