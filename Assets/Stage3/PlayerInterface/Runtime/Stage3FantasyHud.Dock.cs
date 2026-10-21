using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        public const int AbilityReserveCount = 10;
        private Rect HeroDockContentRect => Rect.MinMaxRect(MinimapRect.xMax+18f*UiScale,BottomRect.y+16f*UiScale,
            BottomRect.xMax-40f*UiScale,BottomRect.yMax-16f*UiScale);
        private Rect HeroWeaponRect
        {
            get { var r=HeroDockContentRect; return new Rect(r.x,r.y,r.width*.30f,r.height); }
        }
        private Rect AbilityReservesRect
        {
            get { var r=HeroDockContentRect; return new Rect(r.x+r.width*.30f,r.y,r.width*.36f,r.height); }
        }
        private Rect HeroStatusRect
        {
            get { var r=HeroDockContentRect; return new Rect(r.x+r.width*.66f+16f*UiScale,r.y,r.width*.34f-16f*UiScale,r.height); }
        }

        public Rect AbilityReserveRect(int index)
        {
            if(index<0||index>=AbilityReserveCount) return Rect.zero;
            var r=AbilityReservesRect;var s=UiScale;var gap=6f*s;
            var side=Mathf.Min(76f*s,Mathf.Min((r.width-20f*s-gap*4)/5f,(r.height-gap)/2f));
            var start=r.center.x-(side*5+gap*4)*.5f;
            return new Rect(start+(index%5)*(side+gap),r.y+(index/5)*(side+gap),side,side);
        }

        public Rect HeroStatusBarRect(int index)
        {
            if(index<0||index>2) return Rect.zero;
            var r=HeroStatusRect;var row=(r.height-20f*UiScale)/3f;
            return new Rect(r.x+48f*UiScale,r.y+index*row+5f*UiScale,r.width-48f*UiScale,34f*UiScale);
        }

        private void DrawUnifiedDockBackground()
        {
            var s=UiScale;
            // Common backing bridges the transparent tips of three adjoining original frames.
            Fill(Rect.MinMaxRect(WorldHudRect.x+22f*s,WorldHudRect.y+16f*s,
                WorldHudRect.xMax-22f*s,WorldHudRect.yMax-16f*s),Pearl);
            DrawPanel(BottomRect);
            DrawPanel(NavigationRibbonRect);
            if(HasSelectedBuilding) DrawPanel(SelectedBuildingRect);
            DrawRule(new Rect(MinimapRect.xMax+8f*s,BottomRect.y+24f*s,1f*s,BottomRect.height-48f*s));
        }

        private void DrawHeroDock()
        {
            var s=UiScale;var r=HeroWeaponRect;
            RefreshHeroProgressionLabels();
            var portraitSide=Mathf.Min(124f*s,r.width*.34f);
            var portrait=new Rect(r.x,r.center.y-portraitSide*.5f,portraitSide,portraitSide);
            DrawRoundSprite(Inset(portrait,8f*s),art.heroPortrait);DrawMedallion(portrait);
            if(heroProgression!=null)
            {
                var level=new Rect(portrait.center.x-42f*s,portrait.yMax-9f*s,84f*s,27f*s);
                DrawPanel(level);Label(level,heroLevelLabel,14,TextAnchor.MiddleCenter);
            }
            var weapon=new Rect(portrait.xMax+4f*s,r.center.y-38f*s,44f*s,76f*s);
            var equippedWeapon=EquippedItem(4);
            if(equipment!=null)DrawEquipmentCell(weapon,equippedWeapon);else DrawIcon(weapon,art.bowIcon);
            var stats=new Rect(weapon.xMax+8f*s,r.center.y-58f*s,r.xMax-weapon.xMax-16f*s,116f*s);
            Label(new Rect(stats.x,stats.y,stats.width,28f*s),equipment!=null?equippedWeapon?.Name??"Без оружия":"Лук",22,TextAnchor.MiddleLeft);
            var weaponStats=new Rect(stats.x,stats.y+32f*s,stats.width,stats.height-32f*s);
            if(heroProgression!=null)DrawProgressionBowStats(weaponStats);else DrawCurrentBowStats(weaponStats);
            DrawRule(new Rect(r.xMax,r.y+8f*s,1f*s,r.height-16f*s));
            for(var i=0;i<AbilityReserveCount;i++)
            {
                if(i==0&&heroProgression!=null){DrawBlinkSlot();continue;}
                var slot=AbilityReserveRect(i);
                DrawDisc(Inset(slot,slot.width*.17f),PlumInk);DrawMedallion(slot);
                PaleLabel(slot,"—",18,TextAnchor.MiddleCenter);
            }
            var reserves=AbilityReservesRect;
            DrawRule(new Rect(reserves.xMax+8f*s,r.y+8f*s,1f*s,r.height-16f*s));
            if(heroProgression!=null)
            {
                DrawLiveHeroStatus(0,"HP",heroProgression.Health,heroProgression.MaximumHealth,heroHealthLabel,new Color(.82f,.08f,.20f));
                DrawLiveHeroStatus(1,"MP",heroProgression.Mana,heroProgression.MaximumMana,heroManaLabel,new Color(.02f,.65f,.94f));
            }
            else
            {
                DrawUnavailableHeroStatus(0,"HP",new Color(.82f,.08f,.20f));
                DrawUnavailableHeroStatus(1,"MP",new Color(.02f,.65f,.94f));
            }
            if(heroProgression!=null)
            {
                var xp=HeroStatusBarRect(2);
                Label(new Rect(HeroStatusRect.x,xp.y,42f*s,xp.height),"XP",18,TextAnchor.MiddleLeft);
                DrawExperienceBar(xp);
                DrawExperienceFeedback();
            }
            else DrawUnavailableHeroStatus(2,"XP",new Color(1f,.48f,.06f));
            var status=HeroStatusRect;
            if(heroProgression==null)
                Label(new Rect(status.x+48f*s,status.yMax-17f*s,status.width-48f*s,16f*s),"Не открыто",11,TextAnchor.MiddleCenter,true);
        }

        private void DrawLiveHeroStatus(int index,string title,float current,int maximum,string caption,Color color)
        {
            var s=UiScale;var track=HeroStatusBarRect(index);
            Label(new Rect(HeroStatusRect.x,track.y,42f*s,track.height),title,18,TextAnchor.MiddleLeft);
            DrawStatusBevel(track,PlumInk);
            var fill=Inset(track,5f*s);
            var fraction=maximum>0?Mathf.Clamp01(current/maximum):0;
            if(fraction>0)
            {
                // The full-size cached gradient is clipped, so mana regeneration
                // does not allocate textures for each changing fill width.
                GUI.BeginGroup(new Rect(fill.x,fill.y,fill.width*fraction,fill.height));
                DrawBevelGradient(new Rect(0,0,fill.width,fill.height),Color.Lerp(color,Pearl,.25f),
                    Color.Lerp(PlumInk,color,.70f),7f*s);
                GUI.EndGroup();
            }
            PaleLabel(track,caption??"0 / 0",19,TextAnchor.MiddleCenter);
        }

        private void DrawUnavailableHeroStatus(int index,string title,Color color)
        {
            var s=UiScale;var area=HeroStatusRect;var track=HeroStatusBarRect(index);
            Label(new Rect(area.x,track.y,42f*s,track.height),title,18,TextAnchor.MiddleLeft);
            // Zero unavailable values are explicit; the muted colored surface is decorative,
            // not a filled fraction. Real progression sources will supply a fraction later.
            DrawStatusBevel(track,color);
            PaleLabel(track,"0 / 0",19,TextAnchor.MiddleCenter);
        }
    }
}
