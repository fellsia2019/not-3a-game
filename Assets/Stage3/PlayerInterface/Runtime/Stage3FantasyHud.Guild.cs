using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed class Stage3GuildQuestView
    {
        public int Id,Rank,EnemyKind,RewardGold,RewardExperience;
        public string Title,Location,Objective,Status,ActionLabel,UnavailableReason;
        public bool IsTrial,CanExecute;
    }
    public sealed class Stage3GuildView
    {
        public int PersonalRank,SelectedRank,CompletedForRank,RequiredForTrial=3,SelectedQuestId=-1;
        public Stage3GuildQuestView[] Quests;
        public string BoardMessage;
    }
    public sealed partial class Stage3FantasyHud
    {
        private static readonly string[] GuildRanks={"F","E","D","C","B","A","S"};
        private static readonly Color GuildInk=new Color(.21f,.115f,.065f),GuildGold=new Color(.94f,.75f,.39f);
        private Stage3GuildView guild;
        private Stage3GuildArt guildArt;
        private bool guildArtLoaded;
        private Vector2 guildScroll;
        private Font guildFont;
        private readonly Dictionary<int,GUIStyle> guildStyles=new Dictionary<int,GUIStyle>();
        private string guildRankCaption="",guildProgressCaption="";
        private string[] guildRewards=Array.Empty<string>(),guildPlaces=Array.Empty<string>(),guildTrialCaptions=Array.Empty<string>();
        public bool GuildOpen=>ActiveWindow==Stage3PlayerWindow.Guild;
        public Stage3GuildView GuildView=>guild;
        public event Action<int> GuildRankSelected,GuildQuestSelected,GuildActionRequested;
        public event Action GuildClosed;
        public void SetGuild(Stage3GuildView value)
        {
            var reset=guild==null||value==null||guild.SelectedRank!=value.SelectedRank;
            guild=value;if(reset)guildScroll=Vector2.zero;
            guildRankCaption=value==null?"":"Ваш ранг "+GuildRank(value.PersonalRank);
            guildProgressCaption=value==null?"":"Заказы ранга "+GuildRank(value.SelectedRank)+": "+value.CompletedForRank+" / "+value.RequiredForTrial;
            var entries=value?.Quests;guildRewards=new string[entries?.Length??0];
            guildPlaces=new string[guildRewards.Length];guildTrialCaptions=new string[guildRewards.Length];
            for(var i=0;i<guildRewards.Length;i++)
            {
                var entry=entries[i];if(entry==null)continue;
                guildRewards[i]=entry.RewardGold+" золота  ·  "+entry.RewardExperience+" XP";
                guildPlaces[i]="Место: "+entry.Location;
                guildTrialCaptions[i]="Пройдите испытание, чтобы получить ранг "+GuildRank(entry.Rank);
            }
        }
        public void ShowGuild(bool show){if(show)SetWindow(Stage3PlayerWindow.Guild);else if(GuildOpen)CloseWindow();}
        public void SelectGuildRank(int rank)
        {if(guild!=null&&rank>=0&&rank<7&&rank<=guild.PersonalRank)GuildRankSelected?.Invoke(rank);}
        public void SelectGuildQuest(int id)
        {if(FindGuildQuest(id)>=0)GuildQuestSelected?.Invoke(id);}
        private int FindGuildQuest(int id)
        {var list=guild?.Quests;if(list!=null)for(var i=0;i<list.Length;i++)if(list[i]!=null&&list[i].Id==id)return i;return -1;}
        private static string GuildRank(int rank)=>GuildRanks[Mathf.Clamp(rank,0,6)];
        private float GuildX=>(Screen.width-1600f*UiScale)*.5f;
        public Rect GuildCloseRect=>new Rect(Screen.width-302f*UiScale,42f*UiScale,230f*UiScale,46f*UiScale);
        public Rect GuildRankRect(int rank)=>rank<0||rank>6?Rect.zero:new Rect(GuildX+(640f+rank*111f)*UiScale,108f*UiScale,90f*UiScale,126f*UiScale);
        private Rect GuildListRect=>new Rect(GuildX+140f*UiScale,260f*UiScale,535f*UiScale,Screen.height-330f*UiScale);
        private Rect GuildDetailRect=>new Rect(GuildX+698f*UiScale,250f*UiScale,766f*UiScale,Screen.height-320f*UiScale);
        private float GuildCardHeight=>(GuildListRect.height-12f*UiScale)*.5f;
        public Rect GuildQuestRect(int visibleIndex)
        {
            if(!GuildOpen||guild?.Quests==null||visibleIndex<0||visibleIndex>=guild.Quests.Length)return Rect.zero;
            var list=GuildListRect;var r=new Rect(list.x,list.y+visibleIndex*(GuildCardHeight+12f*UiScale)-guildScroll.y,list.width-(guild.Quests.Length>2?20f*UiScale:0),GuildCardHeight);
            return r.yMin>=list.yMin-.1f&&r.yMax<=list.yMax+.1f?r:Rect.zero;
        }
        public Rect GuildActionRect=>!GuildOpen?Rect.zero:new Rect(GuildDetailRect.x+90f*UiScale,GuildDetailRect.yMax-103f*UiScale,GuildDetailRect.width-180f*UiScale,68f*UiScale);
        private void DrawGuild()
        {
            if(!guildArtLoaded){guildArt=Resources.Load<Stage3GuildArt>("Stage3GuildArt");guildArtLoaded=true;}
            Fill(CurrentWindowRect,new Color(.18f,.09f,.035f));
            if(guildArt?.boardBackground!=null)DrawUiTexture(CurrentWindowRect,guildArt.boardBackground,new Rect(0,0,1,1));
            var s=UiScale;
            var title=new Rect(GuildX+430f*s,8f*s,740f*s,81f*s);
            GuildPanel(title,guildArt?.headerPlaque);GuildText(Inset(title,16f*s),"Гильдия авантюристов",34,true,TextAnchor.MiddleCenter,GuildGold);
            var close=GuildButton(GuildCloseRect,"Esc · Закрыть",true,false);
            DrawGuildProgress();
            var requestedRank=-1;
            for(var i=0;i<7;i++)if(DrawGuildRank(i))requestedRank=i;
            var requestedQuest=DrawGuildOrders();
            var execute=DrawGuildDetails();
            var footer=new Rect(GuildX+620f*s,Screen.height-53f*s,360f*s,35f*s);
            GuildPanel(footer,guildArt?.headerPlaque);GuildText(Inset(footer,4f*s),"Esc · Вернуться в город",16,false,TextAnchor.MiddleCenter,GuildGold);
            // Selection and execution are deliberately separate, and dispatched after the complete snapshot is drawn.
            if(close)CloseWindow();
            else if(execute&&guild!=null)GuildActionRequested?.Invoke(guild.SelectedQuestId);
            else if(requestedRank>=0)SelectGuildRank(requestedRank);
            else if(requestedQuest>=0)SelectGuildQuest(requestedQuest);
        }
        private void DrawGuildProgress()
        {
            var s=UiScale;var badge=new Rect(GuildX+138f*s,95f*s,130f*s,140f*s);
            DrawIcon(badge,guildArt?.rankShield);GuildText(new Rect(badge.x,badge.y+20f*s,badge.width,79f*s),GuildRank(guild?.PersonalRank??0),61,true,TextAnchor.MiddleCenter,GuildGold);
            var caption=new Rect(GuildX+125f*s,217f*s,165f*s,30f*s);GuildPanel(caption,guildArt?.orderPaper);GuildText(caption,guildRankCaption,16,true,TextAnchor.MiddleCenter);
            var progress=new Rect(GuildX+287f*s,122f*s,308f*s,112f*s);GuildPanel(progress,guildArt?.orderPaper);
            GuildText(new Rect(progress.x+20f*s,progress.y+12f*s,progress.width-40f*s,29f*s),guildProgressCaption,20,true,TextAnchor.MiddleCenter);
            var required=Mathf.Clamp(guild?.RequiredForTrial??3,1,3);var done=guild?.CompletedForRank??0;
            for(var i=0;i<required;i++)
            {
                var r=new Rect(progress.x+(42f+i*78f)*s,progress.y+49f*s,46f*s,46f*s);
                if(i<done){DrawIcon(r,guildArt?.waxSeal);GuildText(r,"✓",29,true,TextAnchor.MiddleCenter,new Color(.98f,.89f,.67f));}
                else {Stroke(Inset(r,5f*s),new Color(.54f,.36f,.20f,.6f),1.3f*s);}
            }
        }
        private bool DrawGuildRank(int index)
        {
            var r=GuildRankRect(index);var s=UiScale;var unlocked=guild!=null&&index<=guild.PersonalRank;var selected=guild!=null&&index==guild.SelectedRank;
            var color=GUI.color;GUI.color=selected?Color.white:new Color(.39f,.31f,.25f,1f);DrawIcon(r,guildArt?.rankShield);GUI.color=color;
            GuildText(new Rect(r.x,r.y+18f*s,r.width,65f*s),GuildRank(index),43,true,TextAnchor.MiddleCenter,unlocked?GuildGold:new Color(.71f,.61f,.43f));
            if(!unlocked)DrawGuildLock(new Rect(r.center.x-10f*s,r.yMax-38f*s,20f*s,23f*s));
            return unlocked&&GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        private void DrawGuildLock(Rect r)
        {
            var c=new Color(.78f,.66f,.45f);var s=UiScale;
            Stroke(new Rect(r.x+4f*s,r.y,r.width-8f*s,r.height*.63f),c,2f*s);
            Fill(new Rect(r.x,r.y+r.height*.4f,r.width,r.height*.6f),c);
            Fill(new Rect(r.center.x-s,r.y+r.height*.58f,2f*s,5f*s),GuildInk);
        }
        private int DrawGuildOrders()
        {
            var list=GuildListRect;var entries=guild?.Quests;var result=-1;
            if(entries==null||entries.Length==0){GuildPanel(list,guildArt?.orderPaper);GuildText(Inset(list,30f*UiScale),guild?.BoardMessage??"На этой доске пока нет заказов",23,false,TextAnchor.MiddleCenter);return result;}
            var scrolling=entries.Length>2;var width=list.width-(scrolling?20f*UiScale:0f);
            var content=new Rect(0,0,width,entries.Length*(GuildCardHeight+12f*UiScale)-12f*UiScale);
            guildScroll=GUI.BeginScrollView(list,guildScroll,content,false,scrolling);
            var previousClip=equipmentTextureClip;equipmentTextureClip=new Rect(guildScroll.x,guildScroll.y,list.width,list.height);
            try
            {
                for(var i=0;i<entries.Length;i++)
                {
                    var r=new Rect(0,i*(GuildCardHeight+12f*UiScale),width,GuildCardHeight);
                    if(r.yMax<guildScroll.y||r.yMin>guildScroll.y+list.height)continue;
                    if(DrawGuildOrder(r,entries[i],i))result=entries[i].Id;
                }
            }
            finally{equipmentTextureClip=previousClip;GUI.EndScrollView();}
            return result;
        }
        private bool DrawGuildOrder(Rect r,Stage3GuildQuestView entry,int index)
        {
            if(entry==null)return false;var s=UiScale;GuildPanel(r,guildArt?.orderPaper,true);
            if(entry.Id==guild.SelectedQuestId)DrawEquipmentSelection(Inset(r,5f*s));
            var seal=new Rect(r.x+25f*s,r.y+17f*s,62f*s,62f*s);GuildSprite(seal,guildArt?.waxSeal,true);
            GuildText(seal,GuildRank(entry.Rank),34,true,TextAnchor.MiddleCenter,new Color(.96f,.79f,.49f));
            GuildText(new Rect(seal.xMax+13f*s,r.y+18f*s,r.width-122f*s,59f*s),entry.Title,25,true,TextAnchor.UpperLeft);
            var artRect=new Rect(r.x+27f*s,r.y+80f*s,r.width-54f*s,Mathf.Max(40f*s,r.height-159f*s));
            GuildSprite(artRect,entry.EnemyKind==1?guildArt?.ogreInk:guildArt?.orcInk,true);
            GuildText(new Rect(r.x+66f*s,r.yMax-70f*s,r.width-132f*s,27f*s),guildPlaces[index],18,true);
            GuildText(new Rect(r.x+66f*s,r.yMax-39f*s,r.width-132f*s,27f*s),"Награда: "+guildRewards[index],18,true);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        private bool DrawGuildDetails()
        {
            var r=GuildDetailRect;var s=UiScale;GuildPanel(r,guildArt?.detailPaper);
            var index=FindGuildQuest(guild?.SelectedQuestId??-1);
            if(index<0){GuildText(Inset(r,36f*s),guild?.BoardMessage??"Выберите заказ на доске",25,false,TextAnchor.MiddleCenter);return false;}
            var entry=guild.Quests[index];var seal=new Rect(r.x+27f*s,r.y+27f*s,95f*s,95f*s);DrawIcon(seal,guildArt?.waxSeal);
            GuildText(seal,GuildRank(entry.Rank),49,true,TextAnchor.MiddleCenter,new Color(.96f,.79f,.49f));
            GuildText(new Rect(seal.xMax+16f*s,r.y+28f*s,r.width-168f*s,92f*s),entry.Title,37,true,TextAnchor.MiddleLeft);
            var bodyTop=r.y+145f*s;
            if(entry.IsTrial){GuildText(new Rect(r.x+36f*s,r.y+118f*s,r.width-72f*s,31f*s),guildTrialCaptions[index],20,true);bodyTop+=26f*s;}
            DrawIcon(new Rect(r.x+r.width*.55f,bodyTop+8f*s,r.width*.39f,240f*s),entry.EnemyKind==1?guildArt?.ogreInk:guildArt?.orcInk);
            var textX=r.x+45f*s;var textW=r.width*.49f;
            GuildText(new Rect(textX,bodyTop,textW,29f*s),"Место",24,true);
            GuildText(new Rect(textX,bodyTop+34f*s,textW,49f*s),entry.Location,21);
            GuildText(new Rect(textX,bodyTop+99f*s,textW,30f*s),"Цель",24,true);
            GuildText(new Rect(textX,bodyTop+135f*s,textW,62f*s),entry.Objective,21);
            var rewardY=r.yMax-210f*s;
            GuildText(new Rect(textX,rewardY,r.width-90f*s,29f*s),"Награда",24,true);
            var coin=new Rect(textX,rewardY+37f*s,35f*s,35f*s);if(art.resourceIcons?.Length>0)DrawIcon(coin,art.resourceIcons[0]);
            GuildText(new Rect(coin.xMax+10f*s,rewardY+34f*s,r.width-150f*s,39f*s),guildRewards[index],27,true);
            var action=entry.ActionLabel??(entry.IsTrial?"Принять испытание":"Принять задание");
            var clicked=GuildButton(GuildActionRect,action,entry.CanExecute,true);
            var reason=entry.CanExecute?entry.Status:entry.UnavailableReason??entry.Status;
            if(!string.IsNullOrEmpty(reason))GuildText(new Rect(r.x+60f*s,r.yMax-133f*s,r.width-120f*s,24f*s),reason,16,false,TextAnchor.MiddleCenter);
            return clicked;
        }
        private bool GuildButton(Rect r,string label,bool enabled,bool action)
        {
            var old=GUI.color;if(!enabled)GUI.color=new Color(.65f,.61f,.55f,1);
            GuildPanel(r,action?guildArt?.actionButton:guildArt?.headerPlaque);GUI.color=old;
            GuildText(Inset(r,8f*UiScale),label,action?27:20,true,TextAnchor.MiddleCenter,enabled?new Color(.99f,.89f,.68f):new Color(.68f,.62f,.51f));
            return enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        private void GuildPanel(Rect r,Sprite sprite,bool clipped=false)
        {
            if(sprite==null){Fill(r,new Color(.82f,.69f,.46f));return;}
            var source=sprite.rect;
            var framed=sprite==guildArt?.orderPaper||sprite==guildArt?.detailPaper||sprite==guildArt?.headerPlaque||sprite==guildArt?.actionButton;
            if(!framed){GuildTexture(r,sprite.texture,source,clipped);return;}
            // Shared rounded destination edges prevent seams; paper pins and carved ends retain their shape.
            var bar=sprite==guildArt?.headerPlaque||sprite==guildArt?.actionButton;
            var bx=source.width*(bar ? .18f : .09f);var by=source.height*(bar ? .29f : .10f);
            var scale=bar?r.height/source.height:Mathf.Min(r.width/source.width,r.height/source.height);
            var dx=Mathf.Min(r.width*.3f,bx*scale);var dy=Mathf.Min(r.height*.3f,by*scale);
            for(var y=0;y<3;y++)for(var x=0;x<3;x++)
            {
                var left=Mathf.Round(x==0?r.x:x==1?r.x+dx:r.xMax-dx);
                var right=Mathf.Round(x==0?r.x+dx:x==1?r.xMax-dx:r.xMax);
                var top=Mathf.Round(y==0?r.y:y==1?r.y+dy:r.yMax-dy);
                var bottom=Mathf.Round(y==0?r.y+dy:y==1?r.yMax-dy:r.yMax);
                var sx=x==0?source.x:x==1?source.x+bx:source.xMax-bx;
                var sw=x==1?source.width-2*bx:bx;
                var sy=y==0?source.yMax-by:y==1?source.y+by:source.y;
                var sh=y==1?source.height-2*by:by;
                GuildTexture(new Rect(left,top,right-left,bottom-top),sprite.texture,new Rect(sx,sy,sw,sh),clipped);
            }
        }
        private void GuildTexture(Rect r,Texture2D texture,Rect source,bool clipped)
        {
            var uv=new Rect(source.x/texture.width,source.y/texture.height,source.width/texture.width,source.height/texture.height);
            if(clipped)DrawEquipmentTexture(r,texture,uv);else DrawUiTexture(r,texture,uv);
        }
        private void GuildSprite(Rect r,Sprite sprite,bool clipped)
        {
            if(sprite==null)return;var source=sprite.rect;var ratio=Mathf.Min(r.width/source.width,r.height/source.height);
            var target=new Rect(r.center-source.size*ratio*.5f,source.size*ratio);GuildPanel(target,sprite,clipped);
        }
        private void GuildText(Rect r,string text,int size,bool bold=false,TextAnchor align=TextAnchor.UpperLeft,Color? color=null)
        {
            if(string.IsNullOrEmpty(text))return;
            var actual=Mathf.Max(10,Mathf.RoundToInt(size*UiScale));var key=actual*2+(bold?1:0);
            if(!guildStyles.TryGetValue(key,out var style))
            {
                if(guildFont==null)guildFont=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman","Liberation Serif"},20);
                style=new GUIStyle(GUI.skin.label){font=guildFont,fontSize=actual,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,wordWrap=true,padding=new RectOffset(0,0,0,0)};
                guildStyles[key]=style;
            }
            style.alignment=align;style.normal.textColor=color??GuildInk;GUI.Label(r,LabelContent(text),style);
        }
        private void ReleaseGuildVisuals(){if(guildFont!=null)Destroy(guildFont);guildFont=null;guildStyles.Clear();}
    }
}
