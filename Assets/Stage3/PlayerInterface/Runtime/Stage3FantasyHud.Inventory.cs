using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private void DrawInventory(Rect content)
        {
            var s = UiScale; var gap = 22f * s;
            var left = new Rect(content.x, content.y, content.width * .49f - gap * .5f, content.height);
            var right = new Rect(left.xMax + gap, content.y, content.xMax - left.xMax - gap, content.height);
            Label(new Rect(left.x, left.y, left.width, 32f*s), "Предметы", 23, TextAnchor.MiddleCenter);
            Label(new Rect(right.x, right.y, right.width, 32f*s), "Снаряжение", 23, TextAnchor.MiddleCenter);
            var wallet = game.Gathering.Economy.Inventory;
            var names = new[] { "Золото", "Дерево", "Камень", "Металл" };
            var values = new[] { wallet.Gold, wallet.Get(MapResourceKind.Wood), wallet.Get(MapResourceKind.Stone), wallet.Get(MapResourceKind.Metal) };
            var cardWidth = (left.width - 12f * s) * .5f;
            var cardHeight = (content.height * .48f - 12f*s)*.5f;
            for (var i = 0; i < 4; i++)
            {
                var card = new Rect(left.x + i%2 * (cardWidth+12f*s), left.y+44f*s+i/2*(cardHeight+12f*s), cardWidth, cardHeight);
                DrawSlot(card);
                var safe = PanelContentRect(card);
                var iconSide = Mathf.Min(44f*s,safe.height);
                var iconRect = new Rect(safe.x,safe.center.y-iconSide*.5f,iconSide,iconSide);
                var textRect = new Rect(iconRect.xMax+12f*s,safe.y,Mathf.Max(1f,safe.xMax-iconRect.xMax-12f*s),safe.height);
                if (art != null && art.resourceIcons != null && i < art.resourceIcons.Length)
                    DrawIcon(iconRect,art.resourceIcons[i]);
                Label(new Rect(textRect.x,textRect.y,textRect.width,textRect.height*.46f), names[i], 14,TextAnchor.MiddleLeft);
                Label(new Rect(textRect.x,textRect.y+textRect.height*.46f,textRect.width,textRect.height*.54f), values[i].ToString(), 21,TextAnchor.MiddleLeft);
            }
            var reserveY = left.y + 44f*s + 2f*cardHeight+22f*s;
            Label(new Rect(left.x,reserveY,left.width,26f*s), "Для будущих находок", 14, TextAnchor.MiddleCenter,true);
            var side = Mathf.Min((left.width-24f*s)/3f, content.height*.14f,64f*s);
            for (var i=0;i<3;i++)
            {
                var slot=new Rect(left.center.x-(side*3f+24f*s)*.5f+i*(side+12f*s),reserveY+30f*s,side,side);
                DrawSlot(slot,true);
            }
            Label(new Rect(left.x,left.yMax-42f*s,left.width,42f*s), "Хранилище предметов ещё недоступно.\nРесурсы отображаются из общего запаса.",13,TextAnchor.MiddleCenter,true);

            var weaponHeight=Mathf.Min(145f*s,content.height*.35f);
            var portrait = new Rect(right.x+right.width*.16f,right.y+40f*s,right.width*.68f,right.height-weaponHeight-54f*s);
            if (art != null) DrawIcon(portrait,art.inventoryHero != null ? art.inventoryHero : art.heroPortrait);
            var weapon = new Rect(right.x,right.yMax-weaponHeight,right.width,weaponHeight);
            DrawPanel(weapon);
            var weaponSafe = PanelContentRect(weapon);
            var badgeSide = Mathf.Min(94f*s,weaponSafe.height);
            var badge = new Rect(weaponSafe.x,weaponSafe.center.y-badgeSide*.5f,badgeSide,badgeSide);
            DrawMedallion(badge);
            if (art != null) DrawIcon(Inset(badge,18f*s),art.bowIcon);
            var statsX=badge.xMax+20f*s;
            Label(new Rect(statsX,weaponSafe.y,weaponSafe.xMax-statsX,weaponSafe.height*.28f),"Лук · надет",18);
            DrawCurrentBowStats(new Rect(statsX,weaponSafe.y+weaponSafe.height*.33f,weaponSafe.xMax-statsX,weaponSafe.height*.67f));
        }

        private void DrawCurrentBowStats(Rect rect)
        {
            var bow = game.HeroCombat?.Catalog;
            var value = bow == null ? "—" : $"Урон  {bow.AttackDamage}\nРадиус  {bow.AttackRange:0.#}\nИнтервал  {bow.FireInterval:0.##} с";
            Label(rect,value,16,TextAnchor.UpperLeft);
        }
    }
}
