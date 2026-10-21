using System;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public enum Stage3ForgeMode { Create, Level, Reforge }
    public sealed class Stage3ForgePropertyView { public string Label,Before,After; }
    public sealed class Stage3ForgeCostView { public int Wood,Stone,Metal,Gold,Scrolls; }
    public sealed class Stage3ForgeView
    {
        public Stage3ForgeMode Mode;
        public int SelectedTypeIndex,SelectedItemId=-1,BuildingLevel,MaximumQualityIndex;
        public Stage3EquipmentItemView[] Items;
        public Stage3EquipmentItemView CurrentItem,Outcome;
        public Stage3ForgePropertyView[] Properties;
        public Stage3ForgeCostView Cost,UpgradeCost;
        public bool CanExecute,CanUpgradeBuilding;
        public string UnavailableReason,UpgradeUnavailableReason;
        public int NextMaximumQualityIndex;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3ForgeView forgeView;
        private Vector2 forgeItemScroll;
        public Stage3ForgeView ForgeView=>forgeView;
        public string ForgeInteractionHint { get; set; }
        public event Action<Stage3ForgeMode> ForgeModeSelected;
        public event Action<int> ForgeTypeSelected,ForgeItemSelected;
        public event Action ForgeExecuteRequested,ForgeUpgradeRequested;
        public void SetForge(Stage3ForgeView value)=>forgeView=value;
        private Rect ForgeTypesRect=>new Rect(WindowContentRect.x,WindowContentRect.y+126f*UiScale,WindowContentRect.width*.27f,WindowContentRect.height-126f*UiScale);
        public Rect ForgeModeRect(int index)
        {
            var r=WindowContentRect;var width=r.width*.70f/3f;
            return new Rect(r.x+r.width*.30f+index*width,r.y,width-4f*UiScale,58f*UiScale);
        }
        public Rect ForgeTypeRect(int index)
        {
            var r=ForgeTypesRect;var height=Mathf.Min(54f*UiScale,r.height/8f);
            return new Rect(r.x,r.y+index*height,r.width,height-3f*UiScale);
        }
        public Rect ForgeExecuteRect=>new Rect(WindowContentRect.x+WindowContentRect.width*.77f,WindowContentRect.yMax-82f*UiScale,WindowContentRect.width*.23f,54f*UiScale);
        private void DrawForge(Rect content)
        {
            var s=UiScale;
            Label(new Rect(content.x,content.y,content.width*.28f,62f*s),"Кузница",42);
            Label(new Rect(content.x,content.y+64f*s,content.width*.28f,50f*s),"Оружие, броня и инструменты",18,TextAnchor.UpperLeft,true);
            if(forgeView==null){Label(content,"Кузница недоступна",24,TextAnchor.MiddleCenter);return;}
            var mode=forgeView.Mode;var modeRequest=-1;var typeRequest=-1;var itemRequest=-1;
            var modes=new[]{"Создать","Повысить уровень","Перековать качество"};
            for(var i=0;i<3;i++)if(Button(ForgeModeRect(i),modes[i],(int)mode==i))modeRequest=i;
            for(var i=0;i<8;i++)
            {
                var row=ForgeTypeRect(i);
                if(Button(row,"",forgeView.SelectedTypeIndex==i))typeRequest=i;
                DrawEquipmentIcon(new Rect(row.x+15f*s,row.y+3f*s,42f*s,row.height-6f*s),i,0);
                FitPaleLabel(new Rect(row.x+65f*s,row.y,row.width-82f*s,row.height),EquipmentTypeNames[i],18);
            }
            var main=new Rect(content.x+content.width*.30f,content.y+82f*s,content.width*.44f,content.height-82f*s);
            var before=forgeView.CurrentItem;var after=forgeView.Outcome;
            var title=mode==Stage3ForgeMode.Create?"Создание предмета":mode==Stage3ForgeMode.Level?"Повышение уровня":$"Перековка: {QualityName(before?.QualityIndex??-1)} → {QualityName(after?.QualityIndex??-1)}";
            Label(new Rect(main.x,main.y,main.width,42f*s),title,26,TextAnchor.MiddleCenter);
            var imageY=main.y+45f*s;var imageHeight=main.height*.48f;
            var first=new Rect(main.x,imageY,main.width*.43f,imageHeight);
            var second=new Rect(main.x+main.width*.57f,imageY,main.width*.43f,imageHeight);
            if(before!=null)DrawEquipmentCell(first,before);
            else Label(first,mode==Stage3ForgeMode.Create?"Материалы":"Выберите предмет",18,TextAnchor.MiddleCenter,true);
            Label(new Rect(main.x+main.width*.43f,imageY,main.width*.14f,imageHeight),"→",36,TextAnchor.MiddleCenter,true);
            if(after!=null)DrawEquipmentCell(second,after);else Label(second,"—",28,TextAnchor.MiddleCenter,true);
            DrawForgeItemCaption(first,before);DrawForgeItemCaption(second,after);
            var chainY=main.yMax-75f*s;
            if(mode!=Stage3ForgeMode.Create)
            {
                var choices=forgeView.Items??Array.Empty<Stage3EquipmentItemView>();
                var area=new Rect(main.x,chainY-76f*s,main.width,66f*s);var stride=92f*s;
                forgeItemScroll=GUI.BeginScrollView(area,forgeItemScroll,new Rect(0,0,Mathf.Max(area.width-16f*s,choices.Length*stride),48f*s));
                equipmentTextureClip=new Rect(forgeItemScroll.x,forgeItemScroll.y,area.width,48f*s);
                for(var i=0;i<choices.Length;i++)
                {
                    var item=choices[i];if(item==null)continue;
                    var box=new Rect(i*stride,0,stride-5f*s,46f*s);
                    if(SurfaceButton(box,false,false))itemRequest=item.Id;
                    DrawEquipmentQualityFrame(box,item.QualityIndex);
                    var iconSafe=EquipmentIconSafeRect(box);
                    DrawEquipmentIcon(new Rect(iconSafe.x,iconSafe.y,
                        Mathf.Max(1f,box.x+35f*s-iconSafe.x),iconSafe.height),item);
                    PaleLabel(new Rect(box.x+38f*s,box.y,box.width-44f*s,box.height),$"{QualityName(item.QualityIndex)}\n{item.Level}",12,TextAnchor.MiddleCenter);
                    if(item.Id==forgeView.SelectedItemId)DrawEquipmentSelection(box);
                }
                equipmentTextureClip=null;
                GUI.EndScrollView();
            }
            for(var i=0;i<8;i++)
            {
                var r=new Rect(main.x+i*main.width/8f,chainY,main.width/8f-4f*s,42f*s);
                var old=GUI.color;if(i>forgeView.MaximumQualityIndex)GUI.color=new Color(.65f,.65f,.65f,.65f);
                DrawQualityBadge(r,i);GUI.color=old;
                if(after!=null&&after.QualityIndex==i)DrawEquipmentSelection(r);
            }
            Label(new Rect(main.x,chainY+45f*s,main.width,25f*s),"SSS — только ультраредкая добыча",13,TextAnchor.MiddleCenter,true);
            DrawForgeOutcome(new Rect(content.x+content.width*.77f,content.y+90f*s,content.width*.23f,content.height-90f*s));
            if(modeRequest>=0)ForgeModeSelected?.Invoke((Stage3ForgeMode)modeRequest);
            else if(typeRequest>=0)ForgeTypeSelected?.Invoke(typeRequest);
            else if(itemRequest>=0)ForgeItemSelected?.Invoke(itemRequest);
        }
        private void DrawForgeItemCaption(Rect image,Stage3EquipmentItemView item)
        {
            if(item==null)return;var s=UiScale;
            DrawQualityBadge(new Rect(image.center.x-24f*s,image.yMax+3f*s,48f*s,38f*s),item.QualityIndex);
            Label(new Rect(image.x,image.yMax+43f*s,image.width,28f*s),$"LVL {item.Level} / 100",20,TextAnchor.MiddleCenter);
        }
        private void DrawForgeOutcome(Rect r)
        {
            var s=UiScale;
            Label(new Rect(r.x,r.y,r.width,42f*s),"Результат",28);
            DrawRule(new Rect(r.x,r.y+47f*s,r.width,s));
            var props=forgeView.Properties??Array.Empty<Stage3ForgePropertyView>();
            for(var i=0;i<props.Length;i++)
            {
                var row=new Rect(r.x,r.y+60f*s+i*34f*s,r.width,32f*s);
                Label(new Rect(row.x,row.y,row.width*.47f,row.height),props[i].Label,16,TextAnchor.MiddleLeft,true);
                Label(new Rect(row.x+row.width*.47f,row.y,row.width*.53f,row.height),$"{props[i].Before} → {props[i].After}",17,TextAnchor.MiddleRight);
            }
            var capY=r.y+Mathf.Max(174f,60f+props.Length*34f)*s;
            Label(new Rect(r.x,capY,r.width,44f*s),$"Кузница {forgeView.BuildingLevel} · предел {QualityName(forgeView.MaximumQualityIndex)}",17,TextAnchor.MiddleCenter);
            DrawRule(new Rect(r.x,capY+49f*s,r.width,s));
            Label(new Rect(r.x,capY+57f*s,r.width,34f*s),"Стоимость",24);
            var cost=forgeView.Cost;var line=0;
            void Cost(int value,string name,int icon)
            {
                if(value<=0)return;
                var row=new Rect(r.x,capY+(97f+line++*32f)*s,r.width,30f*s);
                if(icon>=0&&art.resourceIcons!=null&&icon<art.resourceIcons.Length)DrawIcon(new Rect(row.x,row.y,28f*s,28f*s),art.resourceIcons[icon]);
                Label(new Rect(row.x+35f*s,row.y,row.width-75f*s,row.height),name,16,TextAnchor.MiddleLeft,true);
                Label(new Rect(row.xMax-50f*s,row.y,50f*s,row.height),value.ToString(),18,TextAnchor.MiddleRight);
            }
            if(cost!=null){Cost(cost.Wood,"Дерево",1);Cost(cost.Stone,"Камень",2);Cost(cost.Metal,"Металл",3);Cost(cost.Gold,"Золото",0);Cost(cost.Scrolls,"Свиток",-1);}
            if(line==0)Label(new Rect(r.x,capY+97f*s,r.width,30f*s),forgeView.Outcome!=null?"Без затрат":"—",16,TextAnchor.MiddleLeft,true);
            var action=forgeView.Mode==Stage3ForgeMode.Create?"Создать":forgeView.Mode==Stage3ForgeMode.Level?"Повысить":"Перековать";
            if(Button(ForgeExecuteRect,action,false,forgeView.CanExecute))ForgeExecuteRequested?.Invoke();
            Label(new Rect(r.x,ForgeExecuteRect.yMax+3f*s,r.width,27f*s),forgeView.CanExecute?"":forgeView.UnavailableReason??"",12,TextAnchor.MiddleCenter,true);
        }
    }
}
