using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private void DrawHero(Rect content)
        {
            DrawHeroDevelopmentTabs();
            if(showProfessions) DrawProfessions();
            else if(heroLoadout!=null) DrawHeroSkillTree();
            else DrawHeroCombatDevelopment(HeroDevelopmentContentRect);
        }

        private void DrawHeroCombatDevelopment(Rect content)
        {
            if(heroProgression==null){DrawUnavailableHero(content);return;}
            RefreshHeroProgressionLabels();
            var v=heroProgression;var s=UiScale;var left=HeroOverviewRect;
            var figure=new Rect(left.x,left.y-5f*s,left.width,left.height*.69f);
            DrawIcon(figure,art.inventoryHero!=null?art.inventoryHero:art.heroPortrait);
            var level=new Rect(left.x+left.width*.12f,figure.yMax+4f*s,left.width*.76f,46f*s);
            DrawPanel(level);Label(level,heroLevelCapLabel,24,TextAnchor.MiddleCenter);
            var xp=new Rect(left.x,level.yMax+18f*s,left.width,34f*s);
            DrawExperienceBar(xp);
            Label(new Rect(left.x,xp.yMax+4f*s,left.width,22f*s),"XP",14,TextAnchor.MiddleCenter,true);
            var points=new Rect(left.center.x-80f*s,xp.yMax+33f*s,160f*s,42f*s);
            DrawPanel(points);Label(points,heroPointsLabel,19,TextAnchor.MiddleCenter);
            Label(new Rect(left.x,points.yMax+6f*s,left.width,24f*s),"XP от героя и башен",16,TextAnchor.MiddleCenter,true);

            var blink=HeroBlinkRect;
            Label(new Rect(blink.x,blink.y,blink.width,40f*s),"Блинк",29,TextAnchor.MiddleCenter);
            DrawRule(new Rect(blink.x,blink.y+48f*s,blink.width,1f*s));
            var iconSize=Mathf.Min(205f*s,blink.height*.31f);
            var icon=new Rect(blink.center.x-iconSize*.5f,blink.y+59f*s,iconSize,iconSize);
            DrawDisc(Inset(icon,iconSize*.16f),PlumInk);DrawProgressionIcon(Inset(icon,iconSize*.12f),0);DrawMedallion(icon);
            var upgrade=false;
            for(var i=0;i<4;i++)
            {
                var row=BlinkRankRow(i);var learned=i<v.BlinkRank;var next=i==v.BlinkRank;
                DrawSliced(row,art.panelFrame,true,.12f*s);
                if(next)Stroke(Inset(row,3f*s),Accent,1.5f*s);
                Label(new Rect(row.x+22f*s,row.y+5f*s,row.width*.46f,24f*s),blinkRangeLabels[i],20,TextAnchor.MiddleLeft);
                Label(new Rect(row.center.x,row.y+5f*s,row.width*.5f-22f*s,24f*s),learned?"Изучено":next?"Следующий ранг":"Недоступно",15,TextAnchor.MiddleRight,true);
                Label(new Rect(row.x+22f*s,row.y+28f*s,row.width-44f*s,18f*s),blinkRankResourceLabels[i],13,TextAnchor.MiddleLeft,true);
                if(next)
                {
                    Label(new Rect(row.x,row.yMax+1f*s,row.width,25f*s),"Скачок к курсору",16,TextAnchor.MiddleCenter);
                    upgrade=Button(BlinkUpgradeRect,blinkUpgradeLabel,false,v.CanUpgradeBlink);
                }
            }
            if(v.BlinkRank>=4)
                Label(new Rect(blink.x,blink.yMax-48f*s,blink.width,35f*s),"Максимальный ранг",17,TextAnchor.MiddleCenter,true);
            else if(!v.CanUpgradeBlink&&!string.IsNullOrEmpty(v.BlinkUpgradeReason))
                Label(new Rect(blink.x,blink.yMax-28f*s,blink.width,28f*s),v.BlinkUpgradeReason,14,TextAnchor.MiddleCenter,true);

            var passives=HeroPassivesRect;
            Label(new Rect(passives.x,passives.y,passives.width,40f*s),"Пассивные навыки",27,TextAnchor.MiddleCenter);
            DrawRule(new Rect(passives.x,passives.y+48f*s,passives.width,1f*s));
            var purchased=-1;
            for(var i=0;i<3;i++)
            {
                if(v.Passives==null||i>=v.Passives.Length||v.Passives[i]==null)continue;
                var p=v.Passives[i];var card=HeroPassiveCard(i);DrawPanel(card);
                var diameter=Mathf.Min(110f*s,card.height-35f*s);
                var medal=new Rect(card.x+12f*s,card.center.y-diameter*.5f,diameter,diameter);
                DrawDisc(Inset(medal,diameter*.15f),PlumInk);DrawProgressionIcon(Inset(medal,diameter*.12f),i+1);DrawMedallion(medal);
                var textX=medal.xMax+6f*s;var textWidth=card.xMax-textX-17f*s;
                Label(new Rect(textX,card.y+18f*s,textWidth,28f*s),p.Title,21,TextAnchor.MiddleLeft);
                Label(new Rect(textX,card.y+47f*s,textWidth,24f*s),p.Description,16,TextAnchor.MiddleLeft,true);
                Label(new Rect(textX,card.y+71f*s,textWidth,23f*s),p.EffectText,14,TextAnchor.MiddleLeft,true);
                var button=HeroPassiveUpgradeRect(i);
                var dotSide=17f*s;var dotGap=7f*s;var dotsY=button.y+3f*s;
                for(var j=0;j<Mathf.Min(p.MaxRank,3);j++)
                {
                    var dot=new Rect(textX+j*(dotSide+dotGap),dotsY,dotSide,dotSide);
                    DrawDisc(dot,Silver);DrawDisc(Inset(dot,2f*s),j<p.Rank?Accent:Pearl);
                }
                Label(new Rect(textX,dotsY+21f*s,76f*s,20f*s),passiveRankLabels[i],15,TextAnchor.MiddleLeft);
                if(Button(button,p.Rank>=p.MaxRank?"Изучено":p.Rank>0?"Улучшить":"Изучить",false,p.CanUpgrade))purchased=i;
                if(!p.CanUpgrade&&p.Rank<p.MaxRank&&!string.IsNullOrEmpty(p.UnavailableReason))
                    Label(new Rect(button.x+8f*s,button.y-23f*s,button.width-24f*s,20f*s),p.UnavailableReason,12,TextAnchor.MiddleRight,true);
            }
            // Defer callbacks until the complete snapshot has been drawn.
            if(upgrade)BlinkUpgradeRequested?.Invoke();
            if(purchased>=0)HeroPassiveUpgradeRequested?.Invoke(purchased);
        }

        private void DrawUnavailableHero(Rect content)
        {
            var s=UiScale;
            var figure=new Rect(content.x,content.y,content.width*.45f,content.height);
            if(art.inventoryHero!=null) DrawIcon(figure,art.inventoryHero);
            else if(art.heroPortrait!=null) DrawRoundSprite(figure,art.heroPortrait);
            var message=new Rect(content.x+content.width*.49f,content.y,content.width*.51f,content.height);
            var diameter=Mathf.Min(200f*s,message.height*.34f);
            var moon=new Rect(message.center.x-diameter*.5f,message.y+message.height*.10f,diameter,diameter);
            // A decorative crescent, deliberately not a progression node or interactive control.
            DrawDisc(moon,new Color(.67f,.60f,.63f,.22f));
            DrawDisc(new Rect(moon.x+diameter*.25f,moon.y-diameter*.08f,diameter*.90f,diameter*.90f),Pearl);
            var titleY=message.y+message.height*.49f;
            Label(new Rect(message.x,titleY,message.width,65f*s),"Развитие героя",38,TextAnchor.MiddleCenter);
            DrawRule(new Rect(message.x+32f*s,titleY+87f*s,message.width-64f*s,1f*s));
            Label(new Rect(message.x,titleY+121f*s,message.width,56f*s),"Пока недоступно",32,TextAnchor.MiddleCenter,true);
            Label(new Rect(message.x,titleY+188f*s,message.width,36f*s),"Этот раздел появится позже.",21,TextAnchor.MiddleCenter,true);
        }
    }
}
