using System;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Scene binding supplies actual progression values; the HUD never buys or casts.</summary>
    public sealed class Stage3HeroProgressionView
    {
        public int Level, MaxLevel, XpWithin, RequiredXp, AvailablePoints, BlinkRank;
        public int RankPointCost;
        public int WeaponDamage;
        public int MaximumHealth, MaximumMana, BlinkManaCost;
        public float Health, Mana;
        public int[] BlinkManaCosts;
        public float[] BlinkCooldowns;
        public float WeaponRange, WeaponInterval;
        public float[] BlinkRanges;
        public float BlinkCooldownRemaining, BlinkCooldownDuration, BlinkCooldownTotal;
        public bool BlinkArmed, CanArmBlink, CanUpgradeBlink;
        public string BlinkUnavailableReason, BlinkUpgradeReason;
        public Stage3HeroPassiveView[] Passives;
    }

    public sealed class Stage3HeroPassiveView
    {
        public string Title, Description, EffectText;
        public int Rank, MaxRank;
        public bool CanUpgrade;
        public string UnavailableReason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3HeroProgressionView heroProgression;
        public Stage3HeroProgressionView HeroProgressionView => heroProgression;
        public void SetHeroProgression(Stage3HeroProgressionView value) => heroProgression=value;
        public event Action BlinkArmRequested;
        public event Action BlinkCancelRequested;
        public event Action BlinkUpgradeRequested;
        public event Action<int> HeroPassiveUpgradeRequested;
        public Rect BlinkSlotRect => heroLoadout!=null?SkillSlotRect(0):AbilityReserveRect(0);
        private Texture2D progressionIcons, cooldownSectors;
        private bool progressionIconsLoaded;
        private readonly int[] progressionNumbers={-1,-1,-1,-1,-1,-1};
        private string heroLevelLabel, heroLevelCapLabel, heroXpLabel, heroPointsLabel, heroPointBadge, blinkCooldownLabel;
        private int displayedCooldown=-1, displayedRankPointCost=-1;
        private string blinkUpgradeLabel;
        private int displayedWeaponDamage=-1;
        private float displayedWeaponRange=-1,displayedWeaponInterval=-1;
        private string progressionBowStats;
        private readonly string[] blinkRangeLabels=new string[4];
        private readonly float[] displayedBlinkRanges={-1,-1,-1,-1};
        private readonly string[] blinkRankResourceLabels=new string[4];
        private readonly int[] displayedBlinkManaCosts={-1,-1,-1,-1};
        private readonly float[] displayedBlinkCooldowns={-1,-1,-1,-1};
        private int displayedHealth=-1,displayedMaximumHealth=-1,displayedMana=-1,displayedMaximumMana=-1;
        private string heroHealthLabel,heroManaLabel,blinkTooltipText,displayedBlinkReason;
        private int displayedBlinkManaCost=-1;
        private float displayedBlinkCooldownDuration=-1;
        private bool displayedBlinkArmed,displayedBlinkCanArm;
        private readonly int[] displayedPassiveRanks={-1,-1,-1}, displayedPassiveCaps={-1,-1,-1};
        private readonly string[] passiveRankLabels=new string[3];
        private static readonly string[] BlinkRankNames={"I","II","III","IV"};
        private static readonly Color ExperienceOrange=new Color(1f,.48f,.06f);

        private Rect HeroOverviewRect => new Rect(HeroDevelopmentContentRect.x,HeroDevelopmentContentRect.y,HeroDevelopmentContentRect.width*.29f,HeroDevelopmentContentRect.height);
        private Rect HeroBlinkRect => new Rect(HeroDevelopmentContentRect.x+HeroDevelopmentContentRect.width*.33f,HeroDevelopmentContentRect.y,HeroDevelopmentContentRect.width*.30f,HeroDevelopmentContentRect.height);
        private Rect HeroPassivesRect => new Rect(HeroDevelopmentContentRect.x+HeroDevelopmentContentRect.width*.68f,HeroDevelopmentContentRect.y,HeroDevelopmentContentRect.width*.32f,HeroDevelopmentContentRect.height);
        private Rect BlinkRankRow(int rank)
        {
            var r=HeroBlinkRect;var s=UiScale;var top=r.y+r.height*.43f;var height=52f*s;
            var next=Mathf.Clamp(heroProgression?.BlinkRank??0,0,3);
            return new Rect(r.x,top+rank*height+(rank>next?76f*s:0),r.width,height);
        }
        public Rect BlinkUpgradeRect
        {
            get { if(heroLoadout!=null)return selectedSkillId==0?SkillUpgradeRect:Rect.zero; if(heroProgression==null||heroProgression.BlinkRank>=4||ActiveWindow!=Stage3PlayerWindow.Hero||showProfessions)return Rect.zero;
                var r=BlinkRankRow(Mathf.Clamp(heroProgression.BlinkRank,0,3));
                return new Rect(r.x+r.width*.18f,r.yMax+27f*UiScale,r.width*.64f,43f*UiScale); }
        }
        private Rect HeroPassiveCard(int index)
        {
            var r=HeroPassivesRect;var gap=18f*UiScale;var top=r.y+66f*UiScale;var height=(r.yMax-top-gap*2)/3f;
            return new Rect(r.x,top+index*(height+gap),r.width,height);
        }
        public Rect HeroPassiveUpgradeRect(int index)
        {
            if(heroLoadout!=null)return selectedSkillId==index+11?SkillUpgradeRect:Rect.zero;
            if(index<0||index>=3||ActiveWindow!=Stage3PlayerWindow.Hero||heroProgression==null||showProfessions)return Rect.zero;
            var r=HeroPassiveCard(index);return new Rect(r.xMax-r.width*.35f-12f*UiScale,r.yMax-67f*UiScale,r.width*.35f,44f*UiScale);
        }

        private void RefreshHeroProgressionLabels()
        {
            var v=heroProgression;if(v==null)return;
            var health=Mathf.FloorToInt(Mathf.Clamp(v.Health,0,Mathf.Max(0,v.MaximumHealth)));
            var mana=Mathf.FloorToInt(Mathf.Clamp(v.Mana,0,Mathf.Max(0,v.MaximumMana)));
            if(displayedHealth!=health||displayedMaximumHealth!=v.MaximumHealth)
            {displayedHealth=health;displayedMaximumHealth=v.MaximumHealth;heroHealthLabel=health+" / "+v.MaximumHealth;}
            if(displayedMana!=mana||displayedMaximumMana!=v.MaximumMana)
            {displayedMana=mana;displayedMaximumMana=v.MaximumMana;heroManaLabel=mana+" / "+v.MaximumMana;}
            if(blinkTooltipText==null||displayedBlinkManaCost!=v.BlinkManaCost||displayedBlinkCooldownDuration!=v.BlinkCooldownDuration||
                displayedBlinkArmed!=v.BlinkArmed||displayedBlinkCanArm!=v.CanArmBlink||displayedBlinkReason!=v.BlinkUnavailableReason)
            {
                displayedBlinkManaCost=v.BlinkManaCost;displayedBlinkCooldownDuration=v.BlinkCooldownDuration;
                displayedBlinkArmed=v.BlinkArmed;displayedBlinkCanArm=v.CanArmBlink;displayedBlinkReason=v.BlinkUnavailableReason;
                var controls=v.BlinkArmed?"Блинк · ЛКМ — точка\nПКМ / Esc — отмена":"Блинк · Q — скачок к курсору\nКлик по ячейке — выбрать точку";
                if(!v.CanArmBlink&&!v.BlinkArmed&&!string.IsNullOrEmpty(v.BlinkUnavailableReason))
                    controls=v.BlinkUnavailableReason+"\nБлинк · Q — скачок к курсору";
                blinkTooltipText=controls+"\nМана "+v.BlinkManaCost+" · Перезарядка "+v.BlinkCooldownDuration.ToString("0.#")+" с";
            }
            if(displayedRankPointCost!=v.RankPointCost)
            {
                displayedRankPointCost=v.RankPointCost;
                var cost=v.RankPointCost;var last=cost%100;
                var noun=last>=11&&last<=14?"очков":cost%10==1?"очко":cost%10>=2&&cost%10<=4?"очка":"очков";
                blinkUpgradeLabel=cost>0?"Улучшить · "+cost+" "+noun:"Улучшить · бесплатно";
            }
            if(progressionNumbers[0]!=v.Level||progressionNumbers[1]!=v.MaxLevel)
            {progressionNumbers[0]=v.Level;progressionNumbers[1]=v.MaxLevel;heroLevelLabel="LVL "+v.Level;heroLevelCapLabel=heroLevelLabel+" / "+v.MaxLevel;}
            if(progressionNumbers[2]!=v.XpWithin||progressionNumbers[3]!=v.RequiredXp)
            {progressionNumbers[2]=v.XpWithin;progressionNumbers[3]=v.RequiredXp;heroXpLabel=v.RequiredXp>0?v.XpWithin+" / "+v.RequiredXp:"МАКС.";}
            if(progressionNumbers[4]!=v.AvailablePoints)
            {progressionNumbers[4]=v.AvailablePoints;heroPointsLabel="Очки: "+v.AvailablePoints;heroPointBadge=v.AvailablePoints.ToString();}
            var tenth=Mathf.CeilToInt(v.BlinkCooldownRemaining*10f);
            if(tenth!=displayedCooldown){displayedCooldown=tenth;blinkCooldownLabel=(tenth*.1f).ToString("0.0",System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));}
            for(var i=0;i<4;i++)
            {var range=v.BlinkRanges!=null&&i<v.BlinkRanges.Length?v.BlinkRanges[i]:0;
                if(displayedBlinkRanges[i]!=range){displayedBlinkRanges[i]=range;blinkRangeLabels[i]=BlinkRankNames[i]+"  ·  "+range.ToString("0.#");}
                var manaCost=v.BlinkManaCosts!=null&&i<v.BlinkManaCosts.Length?v.BlinkManaCosts[i]:0;
                var cooldown=v.BlinkCooldowns!=null&&i<v.BlinkCooldowns.Length?v.BlinkCooldowns[i]:0;
                if(displayedBlinkManaCosts[i]!=manaCost||displayedBlinkCooldowns[i]!=cooldown)
                {displayedBlinkManaCosts[i]=manaCost;displayedBlinkCooldowns[i]=cooldown;
                    blinkRankResourceLabels[i]="Мана "+manaCost+" · Перезарядка "+cooldown.ToString("0.#")+" с";}}
            for(var i=0;i<3;i++)
            {var p=v.Passives!=null&&i<v.Passives.Length?v.Passives[i]:null;if(p==null)continue;
                if(displayedPassiveRanks[i]!=p.Rank||displayedPassiveCaps[i]!=p.MaxRank)
                {displayedPassiveRanks[i]=p.Rank;displayedPassiveCaps[i]=p.MaxRank;passiveRankLabels[i]=p.Rank+" / "+p.MaxRank;}}
        }

        private void DrawProgressionBowStats(Rect r)
        {
            var v=heroProgression;
            if(displayedWeaponDamage!=v.WeaponDamage||displayedWeaponRange!=v.WeaponRange||displayedWeaponInterval!=v.WeaponInterval)
            {
                displayedWeaponDamage=v.WeaponDamage;displayedWeaponRange=v.WeaponRange;displayedWeaponInterval=v.WeaponInterval;
                progressionBowStats=$"Урон  {v.WeaponDamage}\nРадиус  {v.WeaponRange:0.#}\nИнтервал  {v.WeaponInterval:0.##} с";
            }
            Label(r,progressionBowStats,16,TextAnchor.UpperLeft);
        }

        private void DrawProgressionIcon(Rect r,int index)
        {
            if(!progressionIconsLoaded){progressionIconsLoaded=true;progressionIcons=Resources.Load<Texture2D>("Stage3HeroProgressionIcons");}
            if(progressionIcons!=null)
            {var uv=new Rect((index%2)*.5f,index<2?.5f:0f,.5f,.5f);DrawUiTexture(r,progressionIcons,uv);}
            else DrawIcon(r,index==0?art.teleportIcon:index==1?art.bowIcon:art.heroPortrait);
        }

        private void DrawExperienceBar(Rect r)
        {
            DrawStatusBevel(r,PlumInk);
            var v=heroProgression;var fraction=v.RequiredXp>0?Mathf.Clamp01((float)v.XpWithin/v.RequiredXp):1f;
            var track=Inset(r,5f*UiScale);
            // Clip a fixed-size gradient: XP changes never create a new cached texture size.
            if(fraction>0)
            {
                GUI.BeginGroup(new Rect(track.x,track.y,track.width*fraction,track.height));
                DrawBevelGradient(new Rect(0,0,track.width,track.height),new Color(1f,.72f,.16f),new Color(.79f,.27f,.02f),7f*UiScale);
                GUI.EndGroup();
            }
            PaleLabel(r,heroXpLabel,19,TextAnchor.MiddleCenter);
        }

        private void DrawBlinkSlot()
        {
            var v=heroProgression;var r=BlinkSlotRect;var s=UiScale;
            var clicked=Button(r,"",v.BlinkArmed,v.CanArmBlink||v.BlinkArmed);
            DrawProgressionIcon(Inset(r,r.width*.12f),0);
            var cooldownTotal=v.BlinkCooldownTotal>0?v.BlinkCooldownTotal:v.BlinkCooldownDuration;
            if(v.BlinkCooldownRemaining>0&&cooldownTotal>0)
            {
                EnsureCooldownSectors();
                var step=Mathf.Clamp(Mathf.CeilToInt(v.BlinkCooldownRemaining/cooldownTotal*63f),1,63);
                DrawUiTexture(Inset(r,r.width*.18f),cooldownSectors,new Rect((step%8)/8f,(7-step/8)/8f,1f/8,1f/8));
                PaleLabel(new Rect(r.x,r.yMax-25f*s,r.width,20f*s),blinkCooldownLabel,17,TextAnchor.MiddleCenter);
            }
            else PaleLabel(new Rect(r.x,r.yMax-24f*s,r.width,19f*s),"Q",13,TextAnchor.MiddleCenter);
            if(r.Contains(Event.current.mousePosition)||v.BlinkArmed)
            {
                var tooltip=new Rect(r.x-70f*s,WorldHudRect.y-102f*s,360f*s,94f*s);
                tooltip.x=Mathf.Clamp(tooltip.x,12f*s,Screen.width-tooltip.width-12f*s);
                DrawPanel(tooltip);
                Label(Inset(tooltip,12f*s),blinkTooltipText,15,TextAnchor.MiddleCenter);
            }
            if(clicked){if(v.BlinkArmed)BlinkCancelRequested?.Invoke();else BlinkArmRequested?.Invoke();}
        }

        private void DrawHeroPointBadge()
        {
            if(heroProgression==null||heroProgression.AvailablePoints<=0)return;
            RefreshHeroProgressionLabels();
            var r=HudMenuButtonRect(Stage3PlayerWindow.Hero);var s=UiScale;
            var badge=new Rect(r.xMax-9f*s,r.y-3f*s,22f*s,22f*s);
            DrawDisc(badge,Pearl);DrawDisc(Inset(badge,1.3f*s),new Color(.05f,.47f,.55f));
            PaleLabel(badge,heroPointBadge,13,TextAnchor.MiddleCenter);
        }

        // Approved O symbol: one rising stem and three luminous endpoints, readable at dock scale.
        // Existing I/B/P/M artwork and all navigation rectangles remain untouched.
        private void DrawDevelopmentTreeIcon(Rect r)
        {
            DrawDisc(r,Silver);
            DrawDisc(Inset(r,r.width*.055f),PlumInk);
            var stem=new Color(.96f,.85f,.62f);
            Vector2 P(float x,float y)=>new Vector2(r.x+r.width*x,r.y+r.height*y);
            void Segment(Vector2 a,Vector2 b)
            {
                var matrix=GUI.matrix;
                var delta=b-a;
                GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,a);
                Fill(new Rect(a.x,a.y-r.width*.027f,delta.magnitude,r.width*.054f),stem);
                GUI.matrix=matrix;
            }
            Segment(P(.5f,.85f),P(.5f,.24f));
            Segment(P(.5f,.78f),P(.27f,.59f));Segment(P(.27f,.59f),P(.24f,.40f));
            Segment(P(.5f,.78f),P(.73f,.59f));Segment(P(.73f,.59f),P(.76f,.40f));
            void Node(Vector2 center,Color color)
            {
                var diameter=r.width*.21f;
                var node=new Rect(center.x-diameter*.5f,center.y-diameter*.5f,diameter,diameter);
                DrawDisc(node,stem);DrawDisc(Inset(node,r.width*.027f),color);
                DrawDisc(new Rect(center.x-r.width*.025f,center.y-r.width*.055f,r.width*.055f,r.width*.055f),Pearl);
            }
            Node(P(.24f,.39f),new Color(1f,.60f,.14f));
            Node(P(.5f,.22f),Accent);
            Node(P(.76f,.39f),new Color(.78f,.40f,.92f));
        }

        private string experienceToast;
        private float experienceToastUntil;
        public void ShowExperienceFeedback(string text,float duration)
        {experienceToast=text;experienceToastUntil=Time.unscaledTime+duration;}
        public void ClearExperienceFeedback(){experienceToast=null;experienceToastUntil=0;}
        private void DrawExperienceFeedback()
        {
            var remaining=experienceToastUntil-Time.unscaledTime;
            if(remaining>0&&!(heroLevelUpPresentation?.IsPlaying??false)&&!string.IsNullOrEmpty(experienceToast))DrawExperienceToast(experienceToast,remaining);
        }

        public void DrawExperienceToast(string text,float remaining)
        {
            if(!IsReady||ActiveWindow!=Stage3PlayerWindow.None||Event.current.type!=EventType.Repaint)return;
            var oldDepth=GUI.depth;GUI.depth=-21;
            var oldColor=GUI.color;GUI.color=new Color(1,1,1,Mathf.Min(1,remaining/.35f));
            var s=UiScale;var bar=HeroStatusBarRect(2);
            var r=new Rect(bar.xMax-91f*s,bar.y-15f*s-(1.25f-remaining)*7f*s,88f*s,23f*s);
            Fill(r,PlumInk);Stroke(r,ExperienceOrange,1f*s);PaleLabel(r,text,14,TextAnchor.MiddleCenter);
            GUI.color=oldColor;GUI.depth=oldDepth;
        }

        private Stage3HeroLevelUpPresentation heroLevelUpPresentation;
        public bool HeroPresentationPaused => game?.RunDriver?.Run?.IsPaused??false;
        public void BindHeroLevelUpPresentation(Stage3HeroLevelUpPresentation presentation)=>heroLevelUpPresentation=presentation;
        private void DrawHeroLevelUpOverlay()=>heroLevelUpPresentation?.DrawOverlay();

        public void DrawHeroLevelUpArtwork(Texture2D crest,Texture2D waves,string level,string points,float elapsed,float duration)
        {
            if(Event.current.type!=EventType.Repaint)return;
            var s=UiScale;
            var fadeIn=Mathf.Clamp01(elapsed/.13f);
            var fadeOut=1f-Mathf.SmoothStep(0,1,Mathf.InverseLerp(duration-.65f,duration,elapsed));
            var opacity=fadeIn*fadeOut;
            var stampScale=elapsed<.18f?Mathf.Lerp(1.12f,1f,Mathf.SmoothStep(0,1,elapsed/.18f)):1f;
            var height=Mathf.Min(320f*s,WorldHudRect.y-100f*s);
            var center=new Vector2(Screen.width*.5f,20f*s+height*.5f);
            var crestWidth=height*1.2f;
            var crestRect=new Rect(center.x-crestWidth*.5f,center.y-height*.5f,crestWidth,height);
            var waveWidth=Mathf.Min(Screen.width*.92f,1480f*s);
            var waveHeight=Mathf.Min(height*1.65f,WorldHudRect.y-28f*s);
            var waveRect=new Rect(center.x-waveWidth*.5f,center.y-waveHeight*.5f,waveWidth,waveHeight);
            var oldColor=GUI.color;
            GUI.BeginGroup(new Rect(0,0,Screen.width,WorldHudRect.y));
            void Layer(Texture2D texture,Rect rectangle,float scale,float alpha)
            {
                GUI.color=new Color(1,1,1,Mathf.Clamp01(alpha));
                var scaled=new Rect(rectangle.center-rectangle.size*(scale*.5f),rectangle.size*scale);
                DrawUiTexture(scaled,texture,new Rect(0,0,1,1));
            }
            // Independent broad energy echoes leave the central angular silhouette sharp.
            var waveProgress=Mathf.Clamp01(elapsed/1.1f);
            Layer(waves,waveRect,Mathf.Lerp(.75f,1.08f,waveProgress),opacity*.43f);
            if(elapsed>.15f)
            {
                var echo=Mathf.Clamp01((elapsed-.15f)/1.1f);
                Layer(waves,waveRect,Mathf.Lerp(.86f,1.19f,echo),opacity*(1f-echo)*.20f);
            }
            Layer(crest,crestRect,stampScale,opacity);
            GUI.color=Color.white;
            var textColor=new Color(1f,.94f,.72f,opacity);
            void GoldText(Rect r,string text,int fontSize)
            {
                var content=LabelContent(text);
                var style=GetLabelStyle(fontSize,true);
                while(fontSize>10&&style.CalcSize(content).x>r.width)
                    style=GetLabelStyle(--fontSize,true);
                var original=style.normal.textColor;
                style.alignment=TextAnchor.MiddleCenter;style.normal.textColor=textColor;
                GUI.Label(r,content,style);style.normal.textColor=original;
            }
            // Keep all three lines inside the faceted inner field, including LVL 100 / +96 points.
            GoldText(new Rect(center.x-crestWidth*.21f,center.y-height*.15f-13f*s,crestWidth*.42f,26f*s),"НОВЫЙ УРОВЕНЬ",16);
            GoldText(new Rect(center.x-crestWidth*.20f,center.y-50f*s,crestWidth*.40f,100f*s),level,level.Length>=3?70:84);
            GoldText(new Rect(center.x-crestWidth*.20f,center.y+height*.15625f-13f*s,crestWidth*.40f,26f*s),points,18);
            GUI.EndGroup();
            GUI.color=oldColor;
        }

        private void EnsureCooldownSectors()
        {
            if(cooldownSectors!=null)return;
            const int cell=64;const int side=cell*8;
            cooldownSectors=new Texture2D(side,side,TextureFormat.RGBA32,false,false)
            {name="Blink cooldown radial sectors",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};
            var pixels=new Color32[side*side];
            for(var step=1;step<64;step++)for(var y=0;y<cell;y++)for(var x=0;x<cell;x++)
            {
                var dx=(x+.5f)/cell-.5f;var dy=(y+.5f)/cell-.5f;
                var angle=Mathf.Repeat(Mathf.Atan2(dx,dy),Mathf.PI*2f)/(Mathf.PI*2f);
                if(dx*dx+dy*dy<=.25f&&angle<=step/63f)
                    pixels[((7-step/8)*cell+y)*side+(step%8)*cell+x]=new Color32(15,8,25,175);
            }
            cooldownSectors.SetPixels32(pixels);cooldownSectors.Apply(false,true);
        }

        private void ReleaseProgressionVisuals()
        {if(cooldownSectors!=null)Destroy(cooldownSectors);cooldownSectors=null;}
    }
}
