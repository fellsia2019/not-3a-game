using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private void DrawInventory(Rect content)
        {
            var s=UiScale;
            var left=new Rect(content.x,content.y,content.width*.31f,content.height);
            var hero=new Rect(content.x+content.width*.33f,content.y,content.width*.36f,content.height);
            var right=new Rect(content.x+content.width*.72f,content.y,content.width*.28f,content.height);
            Label(new Rect(left.x,left.y,left.width,58f*s),"Инвентарь",42);
            Label(new Rect(left.x,left.y+59f*s,left.width,32f*s),"Запасы и снаряжение",22,TextAnchor.UpperLeft,true);
            DrawRule(new Rect(left.x,left.y+103f*s,left.width,1f*s),false);
            var wallet=game.Gathering.Economy.Inventory;
            var names=new[]{"Золото","Дерево","Камень","Металл"};
            var values=new[]{wallet.Gold,wallet.Get(MapResourceKind.Wood),wallet.Get(MapResourceKind.Stone),wallet.Get(MapResourceKind.Metal)};
            var cardHeight=Mathf.Min(100f*s,(left.height-202f*s)/4f);
            for(var i=0;i<4;i++)
            {
                var card=new Rect(left.x,left.y+122f*s+i*(cardHeight+10f*s),left.width,cardHeight);
                DrawPanel(card);
                var safe=PanelContentRect(card);var iconSide=Mathf.Min(61f*s,safe.height);
                if(art.resourceIcons!=null&&i<art.resourceIcons.Length)
                    DrawIcon(new Rect(safe.x,safe.center.y-iconSide*.5f,iconSide,iconSide),art.resourceIcons[i]);
                Label(new Rect(safe.x+iconSide+16f*s,safe.y,safe.width-iconSide-86f*s,safe.height),names[i],21,TextAnchor.MiddleLeft);
                Label(new Rect(safe.xMax-60f*s,safe.y,60f*s,safe.height),values[i].ToString(),30,TextAnchor.MiddleRight);
            }
            var note=new Rect(left.x,left.yMax-48f*s,left.width,48f*s);
            Label(note,"Хранилище предметов\nпока недоступно",17,TextAnchor.MiddleCenter,true);
            if(art.inventoryHero!=null) DrawIcon(Inset(hero,12f*s),art.inventoryHero);
            else if(art.heroPortrait!=null) DrawRoundSprite(hero,art.heroPortrait);
            Label(new Rect(right.x,right.y,right.width,58f*s),"Снаряжение",35);
            DrawRule(new Rect(right.x,right.y+83f*s,right.width,1f*s),false);
            var bowRect=new Rect(right.x+right.width*.10f,right.y+110f*s,right.width*.80f,right.height*.43f);
            DrawIcon(bowRect,art.bowIcon);
            var nameY=bowRect.yMax+24f*s;
            Label(new Rect(right.x,nameY,right.width,42f*s),"Лук · Надет",26,TextAnchor.MiddleCenter);
            DrawRule(new Rect(right.x+18f*s,nameY+58f*s,right.width-36f*s,1f*s));
            var bow=game.HeroCombat?.Catalog;
            var statNames=new[]{"Урон","Радиус","Интервал"};
            var statValues=bow==null?new[]{"—","—","—"}:new[]{bow.AttackDamage.ToString(),bow.AttackRange.ToString("0.#"),bow.FireInterval.ToString("0.##")+" с"};
            for(var i=0;i<3;i++)
            {
                var row=new Rect(right.x+20f*s,nameY+78f*s+i*39f*s,right.width-40f*s,35f*s);
                Label(new Rect(row.x,row.y,row.width*.6f,row.height),statNames[i],21,TextAnchor.MiddleLeft,true);
                Label(new Rect(row.x+row.width*.6f,row.y,row.width*.4f,row.height),statValues[i],24,TextAnchor.MiddleRight);
            }
        }

        private void DrawCurrentBowStats(Rect rect)
        {
            var bow = game.HeroCombat?.Catalog;
            var value = bow == null ? "—" : $"Урон  {bow.AttackDamage}\nРадиус  {bow.AttackRange:0.#}\nИнтервал  {bow.FireInterval:0.##} с";
            Label(rect,value,16,TextAnchor.UpperLeft);
        }
    }
}
