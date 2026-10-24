using System;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        public Func<float> CycleProgress {get;set;}
        public Func<bool> IsNightPhase {get;set;}
        public Rect CycleDialRect=>new Rect(Screen.width*.5f-60f*UiScale,1f*UiScale,120f*UiScale,120f*UiScale);
        public Rect CycleCorniceRect=>new Rect(0,0,Screen.width,5.5f*UiScale);
        public Rect CyclePhaseRect=>new Rect(Screen.width*.5f-29f*UiScale,121f*UiScale,58f*UiScale,18f*UiScale);
        public Rect DevelopmentAccessRect=>new Rect(Screen.width-172f*UiScale,4f*UiScale,160f*UiScale,37f*UiScale);
        public float CycleHandDegrees=>Mathf.Repeat(CycleProgress?.Invoke()??0f,1f)*360f;
        public string CyclePhaseLabel=>(IsNightPhase?.Invoke()??false)?"Ночь":"День";
        private Stage3CycleArt cycleArt;
        private Material cycleHandMaterial;
        private Texture2D topTaper;
        private bool cycleArtLoaded;
        private string fullscreenPhaseLabel;
        private int fullscreenPhaseDay=-1;
        private bool fullscreenWasNight;

        private void DrawCycleTopHud()
        {
            var s=UiScale;
            if(!cycleArtLoaded){cycleArt=Resources.Load<Stage3CycleArt>("Stage3CycleArt");cycleArtLoaded=true;}
            // Only the thin cornice crosses the screen; the world remains exposed below the open spans.
            Fill(new Rect(0,0,Screen.width,3f*s),Pearl);
            Fill(new Rect(0,3f*s,Screen.width,1.5f*s),Silver);
            Fill(new Rect(0,4.5f*s,Screen.width,s),PlumInk);
            // End shoulders continue underneath the painted panels, removing exposed vertical seams.
            var resourcesRight=(8f+4f*114f)*s;
            DrawTopTaper(new Rect(resourcesRight-18f*s,0,70f*s,46f*s),false);
            DrawTopTaper(new Rect(StatusRect.x-52f*s,0,70f*s,46f*s),true);
            var inventory=game.Gathering.Economy.Inventory;
            for(var i=0;i<4;i++)
            {
                var r=new Rect((8f+i*114f)*s,0,114f*s,46f*s);
                DrawSliced(r,art.panelFrame,true);
                var icon=new Rect(r.x+14f*s,10f*s,24f*s,24f*s);
                if(art.resourceIcons!=null&&i<art.resourceIcons.Length)DrawIcon(icon,art.resourceIcons[i]);
                FitText(new Rect(icon.xMax+6f*s,7f*s,r.xMax-icon.xMax-28f*s,30f*s),
                    ResourceNumber(i,i==0?inventory.Gold:inventory.Get((MapResourceKind)(i-1))),18,false);
            }
            var day=new Rect(StatusRect.x,0,132f*s,46f*s);DrawSliced(day,art.panelFrame,true);
            var run=game.RunDriver.Run;
            if(displayedDay!=run.CalendarDay){displayedDay=run.CalendarDay;dayLabel="День "+displayedDay;}
            Label(new Rect(day.x+12f*s,5f*s,day.width-24f*s,34f*s),dayLabel,17,TextAnchor.MiddleCenter);
            if(cycleArt!=null&&cycleArt.IsReady)
            {
                DrawIcon(CycleDialRect,cycleArt.Face);DrawCycleHand();
            }
            DrawSliced(CyclePhaseRect,art.panelFrame,true);
            Label(Inset(CyclePhaseRect,2f*s),CyclePhaseLabel,12,TextAnchor.MiddleCenter);
        }

        private void DrawCycleHand()
        {
            if(Event.current.type!=EventType.Repaint)return;
            if(cycleHandMaterial==null)cycleHandMaterial=new Material(cycleArt.HandShader){hideFlags=HideFlags.HideAndDontSave};
            var sprite=cycleArt.Hand;var rect=sprite.rect;var texture=sprite.texture;
            cycleHandMaterial.SetVector("_SourceRect",new Vector4(rect.x/texture.width,rect.y/texture.height,rect.width/texture.width,rect.height/texture.height));
            cycleHandMaterial.SetVector("_Pivot",new Vector4(cycleArt.HandPivot.x,cycleArt.HandPivot.y,0,0));
            cycleHandMaterial.SetVector("_HandSize",new Vector4(.46f*rect.width/rect.height,.46f,0,0));
            cycleHandMaterial.SetFloat("_Angle",(CycleHandDegrees+90f)*Mathf.Deg2Rad);
            cycleHandMaterial.SetFloat("_EncodeToDisplay",UsesMetalLinearTarget?0f:1f);
            cycleHandMaterial.SetFloat("_Brightness",1.65f);
            Graphics.DrawTexture(CycleDialRect,texture,new Rect(0,0,1,1),0,0,0,0,Color.white,cycleHandMaterial);
        }

        private void DrawTopTaper(Rect rect,bool mirrored)
        {
            if(topTaper==null)
            {
                const int width=96,height=64;var pixels=new Color32[width*height];
                for(var x=0;x<width;x++)
                {
                    var t=x/(float)(width-1);var edge=5.5f/46f*height+(height-5.5f/46f*height)*Mathf.Pow(1f-t,3f);
                    for(var y=0;y<height;y++)
                    {
                        var inside=edge-y;Color color;
                        if(inside<0)color=Color.clear;
                        else if(inside<1.7f)color=PlumInk;
                        else if(inside<3.5f)color=Silver;
                        else if(inside<5.5f)color=new Color(.99f,.96f,.87f);
                        else if(inside<7f)color=new Color(.64f,.48f,.48f);
                        else color=Color.Lerp(Pearl,new Color(.99f,.97f,.91f),y/(float)height);
                        color.a*=Mathf.Clamp01(inside+.5f);
                        pixels[(height-1-y)*width+x]=color;
                    }
                }
                topTaper=new Texture2D(width,height,TextureFormat.RGBA32,false){name="Lunar top cornice taper",wrapMode=TextureWrapMode.Clamp};
                topTaper.SetPixels32(pixels);topTaper.Apply(false,true);
            }
            DrawUiTexture(rect,topTaper,mirrored?new Rect(1,0,-1,1):new Rect(0,0,1,1));
        }

        private string FullscreenCycleCaption()
        {
            var day=game.RunDriver.Run.CalendarDay;var night=IsNightPhase?.Invoke()??false;
            if(fullscreenPhaseLabel==null||day!=fullscreenPhaseDay||night!=fullscreenWasNight)
            {fullscreenPhaseDay=day;fullscreenWasNight=night;fullscreenPhaseLabel="День "+day+" · "+(night?"Ночь":"День");}
            return fullscreenPhaseLabel;
        }

        private void ReleaseCycleVisuals()
        {
            if(cycleHandMaterial!=null)Destroy(cycleHandMaterial);
            if(topTaper!=null)Destroy(topTaper);
        }
    }
}
