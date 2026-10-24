using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private Texture2D weaponControlSkins;
        private bool weaponControlSkinsLoaded;
        // Original 1254px ImageGen atlas, alpha > 12 bounds in top-left pixels.
        // Source: art/source/hud_weapon_control_skins_2026-10-23.png; PNG bytes are unmodified.
        private static readonly Rect EmptyWeaponSkinPixels=new Rect(99,53,714,698);
        private static readonly Rect WeaponKeySkinPixels=new Rect(921,241,215,365);
        private static readonly Rect WeaponSwitchSkinPixels=new Rect(44,812,1164,165);
        private static readonly Rect WeaponSwitchDisabledSkinPixels=new Rect(44,1029,1165,162);

        private Rect WeaponSkinUv(Rect pixels)
        {
            if(!weaponControlSkinsLoaded)
            {
                weaponControlSkinsLoaded=true;
                weaponControlSkins=Resources.Load<Texture2D>("Stage3WeaponControlSkins");
            }
            if(weaponControlSkins==null)return Rect.zero;
            return new Rect(pixels.x/weaponControlSkins.width,1f-pixels.yMax/weaponControlSkins.height,
                pixels.width/weaponControlSkins.width,pixels.height/weaponControlSkins.height);
        }

        private void DrawWeaponSkin(Rect target,Rect pixels,bool circular=false)
        {
            var uv=WeaponSkinUv(pixels);
            if(weaponControlSkins==null)return;
            if(circular)DrawCircularTexture(target,weaponControlSkins,uv);
            else DrawUiTexture(target,weaponControlSkins,uv);
        }

        public void RequestWeaponSwitch() => WeaponSwitchRequested?.Invoke();

        private void DrawHeroLoadoutDock()
        {
            var r = LoadoutWeaponArea; var s = UiScale;
            DrawLoadoutVitals();
            var front=WeaponFrontIndex;var back=1-front;
            DrawWeaponDisc(back);DrawWeaponDisc(front);
            DrawActiveWeaponRimJewels();
            var switchRequested=false;
            if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
            // Resolve the overlap to the visibly front disc; clicks never hit a hidden weapon.
            var pointer=CurrentGuiPointer;
            var hit=WeaponDiscContains(front,pointer)?front:WeaponDiscContains(back,pointer)?back:-1;
            if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&hit>=0)
            {
                if(CanSwitchDockWeapon&&hit!=heroLoadout.ActiveWeapon)switchRequested=true;
                Event.current.Use();
            }
            var swap=WeaponSwitchRect;
            if(DrawWeaponSwitchFooter(swap))switchRequested=true;
            Label(new Rect(r.x,r.center.y+29f*s,r.width,20f*s),heroLoadout.ActiveWeapon==0?"Лук":"Посох",13,TextAnchor.MiddleCenter);
            if(hit>=0)DrawLoadoutTooltip(WeaponDiscRect(hit),(hit==0?"Лук":"Посох")+
                (EquippedItem(hit==0?4:7)==null?" · Не экипировано":hit==heroLoadout.ActiveWeapon?" · Активно":" · Нажмите для смены"));
            DrawRule(new Rect(r.xMax, r.y + 8f * s, s, r.height - 16f * s));

            var arm = -1;
            for (var id = 0; id <= 10; id++)
            {
                var node = FindCombatSkill(id);
                if (DrawLoadoutSkillSlot(SkillSlotRect(id), node, id)) arm = id;
            }
            // Dispatch after drawing one stable snapshot; gameplay owns every validity check.
            if (switchRequested) RequestWeaponSwitch();
            if (arm >= 0) SkillArmRequested?.Invoke(arm);
        }

        private bool DrawWeaponSwitchFooter(Rect r)
        {
            var enabled=GUI.enabled;var color=GUI.color;
            var usable=enabled&&CanSwitchDockWeapon;
            // Keep the painted disabled skin intact; GUI's automatic grey tint is only for hit handling.
            GUI.enabled=true;
            GUI.color=color*(usable&&r.Contains(CurrentGuiPointer)?new Color(.88f,1f,1f):Color.white);
            DrawWeaponSkin(r,usable?WeaponSwitchSkinPixels:WeaponSwitchDisabledSkinPixels);
            GUI.color=color;
            var s=UiScale;
            if(usable)
            {
                DrawLunarJewel(new Vector2(r.x+6f*s,r.center.y),8f*s);
                DrawLunarJewel(new Vector2(r.xMax-6f*s,r.center.y),8f*s);
            }
            var key=new Rect(r.center.x-46f*s,r.center.y-10.5f*s,21f*s,21f*s);
            DrawWeaponSkin(key,WeaponKeySkinPixels);
            Label(key,"Z",17,TextAnchor.MiddleCenter);
            FitPaleLabel(new Rect(key.xMax+5f*s,r.y+3f*s,72f*s,r.height-6f*s),"· Сменить",15);
            GUI.enabled=usable;
            var clicked=GUI.Button(r,GUIContent.none,blankButton);
            GUI.enabled=enabled;
            return clicked;
        }

        private bool WeaponDiscContains(int index,Vector2 point)
        {
            var r=WeaponDiscRect(index);return (point-r.center).sqrMagnitude<=r.width*r.width*.25f;
        }

        private void DrawWeaponDisc(int index)
        {
            var r=WeaponDiscRect(index);var s=UiScale;var item=EquippedItem(index==0?4:7);
            DrawDisc(r,new Color(.20f,.16f,.20f));
            DrawDisc(Inset(r,1f*s),Silver);
            DrawDisc(Inset(r,2.5f*s),PlumInk);
            var inner=Inset(r,4f*s);
            if(item!=null)
            {
                if(!equipmentQualityAtlasLoaded)
                {equipmentQualityAtlasLoaded=true;equipmentQualityAtlas=Resources.Load<Texture2D>("Stage3EquipmentQualitySkins");}
                if(equipmentQualityAtlas!=null)
                {
                    var q=Mathf.Clamp(item.QualityIndex,0,7);var source=EquipmentQualitySkinPixels[q];var edge=q==7?76f:60f;
                    source=Inset(source,edge);
                    var uv=new Rect(source.x/equipmentQualityAtlas.width,1f-source.yMax/equipmentQualityAtlas.height,
                        source.width/equipmentQualityAtlas.width,source.height/equipmentQualityAtlas.height);
                    DrawCircularTexture(inner,equipmentQualityAtlas,uv);
                }
            }
            else DrawWeaponSkin(inner,EmptyWeaponSkinPixels,true);
            var matrix=GUI.matrix;var arc=Mathf.Sin(WeaponSwitchAnimationProgress*Mathf.PI);
            GUIUtility.RotateAroundPivot((index==weaponVisualTarget?1f:-1f)*arc*12f,r.center);
            if(item!=null)DrawEquipmentIcon(Inset(r,14f*s),item);
            else
            {
                var tint=GUI.color;GUI.color=tint*new Color(.68f,.65f,.61f,.45f);
                DrawEquipmentIcon(Inset(r,18f*s),index,0);GUI.color=tint;
            }
            GUI.matrix=matrix;
            if(item==null)PaleLabel(new Rect(r.x,r.yMax-23f*s,r.width,18f*s),"Пусто",11,TextAnchor.MiddleCenter);
        }

        private void DrawActiveWeaponRimJewels()
        {
            var index=heroLoadout.ActiveWeapon;
            if(EquippedItem(index==0?4:7)==null)return;
            var r=WeaponDiscRect(index);var inset=2f*UiScale;var side=11f*UiScale;
            // One active rim owns all four stones, including during the brief depth exchange.
            DrawLunarJewel(new Vector2(r.center.x,r.y+inset),side);
            DrawLunarJewel(new Vector2(r.xMax-inset,r.center.y),side);
            DrawLunarJewel(new Vector2(r.center.x,r.yMax-inset),side);
            DrawLunarJewel(new Vector2(r.x+inset,r.center.y),side);
        }

        private void DrawLoadoutVitals()
        {
            var r=LoadoutPortraitArea; var s=UiScale;
            RefreshHeroProgressionLabels();
            var portraitSide = 110f*s;
            var portrait = new Rect(r.x, r.center.y - portraitSide * .5f, portraitSide, portraitSide);
            DrawRoundSprite(Inset(portrait, 8f * s), art.heroPortrait); DrawMedallion(portrait);
            if (heroProgression != null)
            {
                var level = new Rect(portrait.center.x - 43f * s, portrait.yMax - 6f * s, 86f * s, 27f * s);
                DrawPanel(level); Label(level, heroLevelLabel, 14, TextAnchor.MiddleCenter);
            }
            if (heroProgression != null)
            {
                DrawLiveHeroStatus(0, "HP", heroProgression.Health, heroProgression.MaximumHealth, heroHealthLabel, new Color(.82f, .08f, .20f));
                DrawLiveHeroStatus(1, "MP", heroProgression.Mana, heroProgression.MaximumMana, heroManaLabel, new Color(.02f, .65f, .94f));
                var xp = HeroStatusBarRect(2);
                Label(new Rect(HeroStatusRect.x, xp.y, 42f * s, xp.height), "XP", 18, TextAnchor.MiddleLeft);
                DrawExperienceBar(xp); DrawExperienceFeedback();
            }
            else
            {
                DrawUnavailableHeroStatus(0, "HP", new Color(.82f, .08f, .20f));
                DrawUnavailableHeroStatus(1, "MP", new Color(.02f, .65f, .94f));
                DrawUnavailableHeroStatus(2, "XP", ExperienceOrange);
            }
        }

        private bool DrawLoadoutSkillSlot(Rect r, Stage3SkillNodeView node, int id)
        {
            var s = UiScale;
            if(node?.KnowledgeLocked==true){DrawKnowledgeLockedSkill(r,id);return false;}
            var current = id == 0 || (id <= 5 ? 0 : 1) == heroLoadout.ActiveWeapon;
            var usable = node != null && current && (node.CanCast || node.Armed);
            var old = GUI.color;
            GUI.color = old * (!current ? new Color(.45f, .44f, .52f, 1f) : node == null || node.Rank <= 0 ? new Color(.60f, .57f, .63f, 1f) : Color.white);
            DrawDisc(Inset(r, r.width * .13f), PlumInk);
            if (node != null) DrawSkillArtwork(Inset(r, r.width * .12f), id);
            DrawMedallion(r, node?.Armed == true);
            GUI.color = old;
            if (node != null)
            {
                DrawSkillCooldown(Inset(r, r.width * .16f), node.CooldownRemaining, node.CooldownTotal);
                if (node.Rank <= 0) DrawGuildLock(new Rect(r.center.x - 8f * s, r.center.y - 10f * s, 16f * s, 20f * s));
            }
            if (current)
            {
                var key = new Rect(r.xMax - 23f * s, r.yMax - 23f * s, 22f * s, 24f * s);
                DrawDisc(key, Silver); DrawDisc(Inset(key, 1.5f * s), PlumInk);
                PaleLabel(key, id == 0 ? "Q" : ((id - 1) % 5 + 1).ToString(), 16, TextAnchor.MiddleCenter);
            }
            if (node?.CooldownRemaining > 0)
                PaleLabel(new Rect(r.x, r.center.y - 10f * s, r.width, 22f * s), Mathf.CeilToInt(node.CooldownRemaining) + "с", 18, TextAnchor.MiddleCenter);
            DrawAutocastMark(r,node);
            HandleSkillAutocast(r,node);
            if (r.Contains(CurrentGuiPointer) && node != null)
            {
                var reason = !current ? "Z · Переключить оружие" : node.UnavailableReason;
                DrawLoadoutTooltip(r, node.Title + (node.Rank <= 0 ? " · Не изучено" : "") + "\n" +
                    (string.IsNullOrEmpty(reason) ? node.Description : reason)+AutocastHint(node));
            }
            var enabled = GUI.enabled; GUI.enabled = enabled && usable;
            if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            var clicked = GUI.Button(r, GUIContent.none, blankButton);
            GUI.enabled = enabled;
            return clicked;
        }

        private void DrawKnowledgeLockedSkill(Rect r,int id)
        {
            DrawDisc(Inset(r,r.width*.10f),new Color(.32f,.31f,.34f));
            DrawSkillArtwork(Inset(r,r.width*.105f),id,true);
            DrawDisc(Inset(r,r.width*.12f),new Color(.58f,.57f,.60f,.30f));
            var old=GUI.color;GUI.color*=new Color(.65f,.65f,.67f,.72f);DrawMedallion(r,false);GUI.color=old;
            var side=Mathf.Min(16f*UiScale,r.width*.28f);
            DrawGuildLock(new Rect(r.center.x-side*.5f,r.center.y-side*.65f,side,side*1.3f));
        }

        private string AutocastHint(Stage3SkillNodeView node) => node!=null&&!node.KnowledgeLocked&&node.Rank>0&&node.SupportsAutocast
            ?"\nПКМ · Автоприменение: "+(node.AutocastEnabled?"вкл.":"выкл."):"";

        private void HandleSkillAutocast(Rect r,Stage3SkillNodeView node)
        {
            if(node==null||node.KnowledgeLocked||node.Rank<=0||!node.SupportsAutocast||!GUI.enabled)return;
            if(Event.current.type==EventType.MouseDown&&Event.current.button==1&&r.Contains(CurrentGuiPointer))
            {SkillAutocastToggleRequested?.Invoke(node.Id);Event.current.Use();}
        }

        private void DrawAutocastMark(Rect r,Stage3SkillNodeView node)
        {
            if(node==null||node.KnowledgeLocked||node.Rank<=0||!node.SupportsAutocast||!node.AutocastEnabled)return;
            var side=Mathf.Min(18f*UiScale,r.width*.28f);
            var badge=new Rect(r.xMax-side-3f*UiScale,r.y+3f*UiScale,side,side);
            DrawDisc(badge,Accent);DrawDisc(Inset(badge,1.4f*UiScale),Pearl);
            HudReferenceLabel(badge,"А",11,TextAnchor.MiddleCenter,true);
        }

        private Texture2D hudPolishSkins;
        private bool hudPolishSkinsLoaded;
        private Texture2D hudReferenceSkins,hudReferencePotions,hudReferenceRibbon,hudReferenceStates;
        private bool hudReferenceArtLoaded;
        private static readonly Color HudReferenceInk=new Color(.19f,.055f,.30f);
        // UI-only source windows: the first atlas's rejected ribbon/state rows are never sampled.
        private static readonly Rect[] ReferenceSkinSources={
            Rect.zero,Rect.zero,Rect.zero,Rect.zero,Rect.zero,Rect.zero,
            new Rect(52,896,258,245),new Rect(408,915,208,201),new Rect(706,915,207,200),
            new Rect(1018,920,167,186)};
        private static readonly Rect[] ReferencePotionSources={new Rect(82,45,528,632),new Rect(807,45,527,632),new Rect(1532,45,528,630)};
        private static readonly Rect ReferenceRibbonSource=new Rect(31,237,2017,181);
        private static readonly Rect[] ReferenceStateSources={new Rect(43,28,617,631),new Rect(777,28,615,627),new Rect(1511,25,612,627)};

        private void EnsureHudReferenceArt()
        {
            if(hudReferenceArtLoaded)return;
            hudReferenceSkins=Resources.Load<Texture2D>("Stage3HudReferenceSkins");
            hudReferencePotions=Resources.Load<Texture2D>("Stage3HudReferencePotions");
            hudReferenceRibbon=Resources.Load<Texture2D>("Stage3HudReferenceRibbon");
            hudReferenceStates=Resources.Load<Texture2D>("Stage3HudReferenceStates");
            hudReferenceArtLoaded=true;
        }

        private void HudReferenceLabel(Rect r,string text,int size,TextAnchor anchor,bool bold,Color? ink=null)
        {
            var style=GetLabelStyle(size,false);var oldFont=style.fontStyle;var oldInk=style.normal.textColor;
            var oldAlignment=style.alignment;var oldWrap=style.wordWrap;
            style.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;style.normal.textColor=ink??HudReferenceInk;
            style.alignment=anchor;style.wordWrap=true;
            GUI.Label(r,LabelContent(text??""),style);
            style.fontStyle=oldFont;style.normal.textColor=oldInk;style.alignment=oldAlignment;style.wordWrap=oldWrap;
        }

        private void DrawReferenceSource(Rect target,Texture2D texture,Rect source)
        {
            DrawUiTexture(target,texture,new Rect(source.x/texture.width,1f-source.yMax/texture.height,
                source.width/texture.width,source.height/texture.height));
        }

        private bool DrawReferenceSkin(Rect target,int part,float sourceBorder=0,float targetBorder=0)
        {
            EnsureHudReferenceArt();if(hudReferenceSkins==null||part<0||part>=ReferenceSkinSources.Length)return false;
            var source=ReferenceSkinSources[part];
            if(source.width<=0||source.height<=0)return false;
            if(sourceBorder<=0){DrawReferenceSource(target,hudReferenceSkins,source);return true;}
            var corner=Mathf.Min(targetBorder,Mathf.Min(target.width,target.height)*.3f);
            hudPatchX[0]=target.x;hudPatchX[1]=target.x+corner;hudPatchX[2]=target.xMax-corner;hudPatchX[3]=target.xMax;
            hudPatchY[0]=target.y;hudPatchY[1]=target.y+corner;hudPatchY[2]=target.yMax-corner;hudPatchY[3]=target.yMax;
            hudPatchU[0]=source.x;hudPatchU[1]=source.x+sourceBorder;hudPatchU[2]=source.xMax-sourceBorder;hudPatchU[3]=source.xMax;
            hudPatchV[0]=source.y;hudPatchV[1]=source.y+sourceBorder;hudPatchV[2]=source.yMax-sourceBorder;hudPatchV[3]=source.yMax;
            for(var i=0;i<4;i++){hudPatchX[i]=Mathf.Round(hudPatchX[i]);hudPatchY[i]=Mathf.Round(hudPatchY[i]);}
            for(var y=0;y<3;y++)for(var x=0;x<3;x++)
                DrawReferenceSource(Rect.MinMaxRect(hudPatchX[x],hudPatchY[y],hudPatchX[x+1],hudPatchY[y+1]),hudReferenceSkins,
                    Rect.MinMaxRect(hudPatchU[x],hudPatchV[y],hudPatchU[x+1],hudPatchV[y+1]));
            return true;
        }

        private void DrawReferenceServiceRibbon()
        {
            EnsureHudReferenceArt();if(hudReferenceRibbon==null)return;
            DrawReferenceSource(NavigationRibbonRect,hudReferenceRibbon,ReferenceRibbonSource);
        }

        private readonly float[] referenceStateX=new float[6],referenceStateU=new float[6];
        private void DrawReferenceState(Rect r,int state,float opacity)
        {
            EnsureHudReferenceArt();if(hudReferenceStates==null||opacity<=0)return;
            if(state==1||state==2)
            {
                // A coloured outer echo makes the short result pulse readable over ivory.
                // It samples only the painted edge and omits the crown to avoid a double jewel.
                var halo=state==1?new Color(.15f,1f,1f):new Color(1f,.18f,.28f);
                DrawReferenceStateBorder(Inset(r,-1.5f*UiScale),state,opacity*.85f,halo,true);
            }
            var tint=state==1?new Color(.65f,1f,1f):state==2?new Color(1f,.64f,.68f):Color.white;
            DrawReferenceStateBorder(r,state,opacity,tint,false);
        }

        private void DrawReferenceStateBorder(Rect r,int state,float opacity,Color tint,bool omitCrown)
        {
            var src=ReferenceStateSources[Mathf.Clamp(state,0,2)];var s=UiScale;
            // Border-only source windows. The generated interior is deliberately NEVER drawn.
            // Separate crown band keeps the jewel isotropic on navigation and wide build cards.
            var corner=6f*s;var gem=11f*s;
            referenceStateX[0]=r.x-6f*s;referenceStateX[1]=r.x+corner;referenceStateX[2]=r.center.x-gem;
            referenceStateX[3]=r.center.x+gem;referenceStateX[4]=r.xMax-corner;referenceStateX[5]=r.xMax+6f*s;
            referenceStateU[0]=src.x;referenceStateU[1]=src.x+src.width*.18f;referenceStateU[2]=src.center.x-src.width*.105f;
            referenceStateU[3]=src.center.x+src.width*.105f;referenceStateU[4]=src.xMax-src.width*.18f;referenceStateU[5]=src.xMax;
            hudPatchY[0]=r.y-12f*s;hudPatchY[1]=r.y+7f*s;hudPatchY[2]=r.yMax-6f*s;hudPatchY[3]=r.yMax+5f*s;
            hudPatchV[0]=src.y;hudPatchV[1]=src.y+src.height*.205f;hudPatchV[2]=src.yMax-src.height*.14f;hudPatchV[3]=src.yMax;
            var previous=GUI.color;GUI.color=new Color(previous.r*tint.r,previous.g*tint.g,previous.b*tint.b,previous.a*opacity);
            for(var y=0;y<3;y++)for(var x=0;x<5;x++)
            {
                if(y==1&&x>0&&x<4)continue;
                if(omitCrown&&y==0&&x==2)continue;
                DrawReferenceSource(Rect.MinMaxRect(Mathf.Round(referenceStateX[x]),Mathf.Round(hudPatchY[y]),
                    Mathf.Round(referenceStateX[x+1]),Mathf.Round(hudPatchY[y+1])),hudReferenceStates,
                    Rect.MinMaxRect(referenceStateU[x],hudPatchV[y],referenceStateU[x+1],hudPatchV[y+1]));
            }
            GUI.color=previous;
        }

        private void DrawReferencePotion(Rect target,int index)
        {
            EnsureHudReferenceArt();if(hudReferencePotions==null)return;
            var src=ReferencePotionSources[index];var scale=Mathf.Min(target.width/src.width,target.height/src.height);
            DrawReferenceSource(new Rect(target.center-src.size*scale*.5f,src.size*scale),hudReferencePotions,src);
        }
        private readonly float[] hudPatchX=new float[4],hudPatchY=new float[4],hudPatchU=new float[4],hudPatchV=new float[4];
        // Bounds are top-left source pixels measured at alpha>8 on the original1254px atlas.
        // Sampling excludes near-transparent coloured fringe; original PNG bytes stay intact.
        private static readonly Rect[] HudSkinSources={new Rect(146,78,474,452),new Rect(718,205,468,159),
            new Rect(102,610,556,193),new Rect(815,523,290,285),new Rect(55,879,749,309)};
        private bool DrawHudSkinPart(Rect r,int part,bool sliced=true)
        {
            if(!hudPolishSkinsLoaded){hudPolishSkins=Resources.Load<Texture2D>("Stage3HudPolishSkins");hudPolishSkinsLoaded=true;}
            if(hudPolishSkins==null)return false;
            var src=HudSkinSources[part];var texture=hudPolishSkins;
            if(!sliced)
            {
                DrawUiTexture(r,texture,new Rect(src.x/texture.width,1f-src.yMax/texture.height,src.width/texture.width,src.height/texture.height));
                return true;
            }
            var border=part==0?52f:48f;var corner=(part==0?6f:part==4?5f:4f)*UiScale;
            corner=Mathf.Min(corner,Mathf.Min(r.width,r.height)*.3f);
            hudPatchX[0]=r.x;hudPatchX[1]=r.x+corner;hudPatchX[2]=r.xMax-corner;hudPatchX[3]=r.xMax;
            hudPatchY[0]=r.y;hudPatchY[1]=r.y+corner;hudPatchY[2]=r.yMax-corner;hudPatchY[3]=r.yMax;
            hudPatchU[0]=src.x;hudPatchU[1]=src.x+border;hudPatchU[2]=src.xMax-border;hudPatchU[3]=src.xMax;
            hudPatchV[0]=src.y;hudPatchV[1]=src.y+border;hudPatchV[2]=src.yMax-border;hudPatchV[3]=src.yMax;
            for(var i=0;i<4;i++){hudPatchX[i]=Mathf.Round(hudPatchX[i]);hudPatchY[i]=Mathf.Round(hudPatchY[i]);}
            for(var y=0;y<3;y++)for(var x=0;x<3;x++)
            {
                var target=Rect.MinMaxRect(hudPatchX[x],hudPatchY[y],hudPatchX[x+1],hudPatchY[y+1]);
                var uv=new Rect(hudPatchU[x]/texture.width,1f-hudPatchV[y+1]/texture.height,
                    (hudPatchU[x+1]-hudPatchU[x])/texture.width,(hudPatchV[y+1]-hudPatchV[y])/texture.height);
                DrawUiTexture(target,texture,uv);
            }
            return true;
        }
        private void DrawHudCoralWell(Rect r)
        {
            if(!DrawHudSkinPart(r,4))Fill(r,new Color(1f,.81f,.75f));
        }
        private void DrawHudRubyAlert(Rect r)
        {
            if(!DrawHudSkinPart(r,3,false))DrawDisc(r,new Color(.91f,.02f,.10f));
            PaleLabel(new Rect(r.x,r.y-1f*UiScale,r.width,r.height),"!",24,TextAnchor.MiddleCenter);
        }
        private void DrawHudPlainTile(Rect r, Color tint)
        {
            if(!DrawReferenceSkin(r,6,48f,8f*UiScale)&&!DrawHudSkinPart(r,0))Fill(r,Pearl);
            if(tint.a>0)Fill(Inset(r,3f*UiScale),tint);
        }
        private void DrawHudKeycap(Rect r,string text,bool plum,int size)
        {
            if(!DrawReferenceSkin(r,plum?7:8,40f,5f*UiScale)&&!DrawHudSkinPart(r,plum?1:2))Fill(r,plum?PlumInk:Pearl);
            var box=Inset(r,UiScale);var style=GetLabelStyle(size,false);
            var oldSize=style.fontSize;var oldWrap=style.wordWrap;var oldFont=style.fontStyle;
            var oldInk=style.normal.textColor;var oldAlignment=style.alignment;
            style.fontStyle=FontStyle.Bold;style.wordWrap=false;style.alignment=TextAnchor.MiddleCenter;
            style.normal.textColor=plum?new Color(1f,.98f,.94f):HudReferenceInk;
            var content=LabelContent(text);var measured=style.CalcSize(content);
            while((measured.x>box.width||measured.y>box.height)&&style.fontSize>8)
            {style.fontSize--;measured=style.CalcSize(content);}
            GUI.Label(box,content,style);
            style.fontSize=oldSize;style.wordWrap=oldWrap;style.fontStyle=oldFont;
            style.normal.textColor=oldInk;style.alignment=oldAlignment;
        }
        private void DrawHudChamferRim(Rect r,Color color,float thickness)
        {
            var cut=4f*UiScale;
            var a=new Vector2(r.x+cut,r.y);var b=new Vector2(r.xMax-cut,r.y);
            var c=new Vector2(r.xMax,r.y+cut);var d=new Vector2(r.xMax,r.yMax-cut);
            var e=new Vector2(r.xMax-cut,r.yMax);var f=new Vector2(r.x+cut,r.yMax);
            var g=new Vector2(r.x,r.yMax-cut);var h=new Vector2(r.x,r.y+cut);
            DrawGlyphLine(a,b,thickness,color);DrawGlyphLine(b,c,thickness,color);
            DrawGlyphLine(c,d,thickness,color);DrawGlyphLine(d,e,thickness,color);
            DrawGlyphLine(e,f,thickness,color);DrawGlyphLine(f,g,thickness,color);
            DrawGlyphLine(g,h,thickness,color);DrawGlyphLine(h,a,thickness,color);
        }
        private void DrawHudSoftRim(Rect r,Color color,float strength)
        {
            var s=UiScale;
            for(var i=3;i>=1;i--)DrawHudChamferRim(Inset(r,-i*s),new Color(color.r,color.g,color.b,strength*.075f),s);
            DrawHudChamferRim(r,new Color(color.r,color.g,color.b,strength),1.5f*s);
        }
        private void DrawQuickAccessArtwork(Rect r,Stage3QuickSlotView slot)
        {
            if(slot==null||slot.ItemTypeIndex<0)return;
            var potion=slot.ItemTypeIndex-11;
            if(potion>=0&&potion<3)DrawReferencePotion(r,potion);
            else DrawEquipmentIcon(r,slot.ItemTypeIndex,0);
        }

        private bool DrawQuickSlot(Rect r, Stage3QuickSlotView slot, int index)
        {
            var s=UiScale;var assigned=slot!=null&&slot.ItemTypeIndex>=0;
            DrawHudPlainTile(r,Color.clear);
            if(assigned)
            {
                DrawQuickAccessArtwork(QuickSlotIconRect(index),slot);
            }
            DrawHudKeycap(QuickSlotKeyRect(index),"F"+(index+1),true,15);
            if(assigned)DrawHudKeycap(QuickSlotCountRect(index),slot.Count.ToString(),true,16);
            if(!assigned)return false;
            var hover=GUI.enabled&&r.Contains(CurrentGuiPointer);
            if(hover)DrawReferenceState(Inset(r,s),0,1f);
            var age=Time.unscaledTime-slot.FeedbackStartedAt;
            if(slot.FeedbackSerial>0&&age>=0f&&age<.48f)
            {
                var strength=Mathf.Sqrt(Mathf.Sin(Mathf.Clamp01(age/.48f)*Mathf.PI));
                DrawReferenceState(Inset(r,s),slot.FeedbackSucceeded?1:2,strength);
            }
            if(slot.CooldownRemaining>0)
            {
                var cooldown=new Rect(r.xMax-29f*s,r.y+4f*s,27f*s,17f*s);
                HudReferenceLabel(cooldown,Mathf.CeilToInt(slot.CooldownRemaining)+"с",12,TextAnchor.MiddleCenter,true);
            }
            if(hover)DrawLoadoutTooltip(r,slot.Title+(string.IsNullOrEmpty(slot.UnavailableReason)?"":"\n"+slot.UnavailableReason));
            var enabled=GUI.enabled;
            GUI.enabled=enabled&&game?.RunDriver?.Run!=null&&!game.RunDriver.Run.IsPaused&&!game.RunDriver.Run.IsTerminal&&!IsModalOpen&&legacyHud?.DebugOpen!=true;
            if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
            var clicked=GUI.Button(r,GUIContent.none,blankButton);GUI.enabled=enabled;
            return clicked;
        }

        private void DrawSkillCooldown(Rect r, float remaining, float total)
        {
            if (remaining <= 0 || total <= 0) return;
            EnsureCooldownSectors();
            var step = Mathf.Clamp(Mathf.CeilToInt(remaining / total * 63f), 1, 63);
            DrawUiTexture(r, cooldownSectors, new Rect(step % 8 / 8f, (7 - step / 8) / 8f, 1f / 8, 1f / 8));
        }

        private Rect pendingLoadoutTooltipAnchor;
        private string pendingLoadoutTooltipText;
        public Rect LastLoadoutTooltipRect { get; private set; }
        public Rect LastLoadoutTooltipTextRect { get; private set; }
        public string LastLoadoutTooltipText { get; private set; }

        private void ResetLoadoutTooltip()
        {
            pendingLoadoutTooltipText=null;
            LastLoadoutTooltipRect=Rect.zero;
            LastLoadoutTooltipTextRect=Rect.zero;
            LastLoadoutTooltipText=null;
        }

        private void DrawLoadoutTooltip(Rect anchor, string text)
        {
            // Callers submit content; later panels must never paint over the tooltip.
            pendingLoadoutTooltipAnchor=anchor;
            pendingLoadoutTooltipText=text;
        }

        public static Rect CalculateLoadoutTooltipRect(Rect anchor, Vector2 viewport, Vector2 measuredContent, float scale)
        {
            var margin=12f*scale;var padding=24f*scale;var gap=8f*scale;
            var width=Mathf.Min(measuredContent.x+padding,Mathf.Max(1f,viewport.x-2f*margin));
            var height=Mathf.Min(measuredContent.y+padding,Mathf.Max(1f,viewport.y-2f*margin));
            var x=Mathf.Clamp(anchor.center.x-width*.5f,margin,Mathf.Max(margin,viewport.x-margin-width));
            var y=anchor.yMin-gap-height;
            if(y<margin)y=anchor.yMax+gap;
            y=Mathf.Clamp(y,margin,Mathf.Max(margin,viewport.y-margin-height));
            return new Rect(x,y,width,height);
        }

        private void DrawDeferredLoadoutTooltip()
        {
            if(string.IsNullOrEmpty(pendingLoadoutTooltipText)||Event.current.type!=EventType.Repaint)return;
            var s=UiScale;var padding=12f*s;
            var style=GetLabelStyle(17,false);
            var oldSize=style.fontSize;var oldWrap=style.wordWrap;var oldAlignment=style.alignment;var oldColor=style.normal.textColor;var oldFont=style.fontStyle;
            style.fontStyle=FontStyle.Bold;
            var widest=0f;
            foreach(var line in pendingLoadoutTooltipText.Split('\n'))widest=Mathf.Max(widest,style.CalcSize(LabelContent(line)).x);
            var textWidth=Mathf.Clamp(Mathf.Ceil(widest),64f*s,Mathf.Min(350f*s,Screen.width-48f*s));
            style.wordWrap=true;style.alignment=TextAnchor.MiddleCenter;style.normal.textColor=new Color(1f,.97f,.92f);
            var content=LabelContent(pendingLoadoutTooltipText);
            var height=style.CalcHeight(content,textWidth);
            while(height>Screen.height-48f*s&&style.fontSize>Mathf.RoundToInt(11f*s))
            {style.fontSize--;height=style.CalcHeight(content,textWidth);}
            var tooltip=CalculateLoadoutTooltipRect(pendingLoadoutTooltipAnchor,new Vector2(Screen.width,Screen.height),new Vector2(textWidth,Mathf.Ceil(height)),s);
            var savedDepth=GUI.depth;GUI.depth=-1100;
            if(!DrawReferenceSkin(tooltip,10,40f,8f*s))
            {
                DrawBevelGradient(tooltip,new Color(.72f,.68f,.73f),new Color(.29f,.23f,.33f),5f*s);
                DrawBevelGradient(Inset(tooltip,1.2f*s),new Color(.27f,.21f,.30f),new Color(.12f,.08f,.16f),4f*s);
            }
            GUI.Label(Inset(tooltip,padding),content,style);
            GUI.depth=savedDepth;
            LastLoadoutTooltipRect=tooltip;
            LastLoadoutTooltipTextRect=Inset(tooltip,padding);
            LastLoadoutTooltipText=pendingLoadoutTooltipText;
            style.fontSize=oldSize;style.wordWrap=oldWrap;style.alignment=oldAlignment;style.normal.textColor=oldColor;style.fontStyle=oldFont;
        }
    }
}
