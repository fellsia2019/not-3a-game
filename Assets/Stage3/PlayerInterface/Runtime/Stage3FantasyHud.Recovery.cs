using System;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    [Serializable]
    public sealed class Stage3RecoveryDropView
    {
        public int Id,ItemCount;
        public Vector2 Position;
    }
    [Serializable]
    public sealed class Stage3RecoveryView
    {
        public Stage3RecoveryDropView[] Drops=Array.Empty<Stage3RecoveryDropView>();
        public int NearbyDropId=-1;
        public bool CanRecover;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3RecoveryView recoveryView;
        private Stage3RecoveryArt recoveryArt;
        private Stage3HeroRecoveryPresentation recoveryPresentation;
        private Camera recoveryCamera;
        private bool recoveryArtLoaded;
        private Stage3RecoveryDropView[] drawnRecoveryDrops;
        private Rect recoveryPointerRect;
        private const string RecoveryCaption="Выпавшие вещи";
        private static readonly string[] RecoveryArrows={"→","↘","↓","↙","←","↖","↑","↗"};
        public Stage3RecoveryView RecoveryView => recoveryView;
        public Rect RecoveryPointerRect => IsModalOpen||!isActiveAndEnabled||
            (legacyHud!=null&&legacyHud.DebugOpen)||recoveryView?.Drops==null||recoveryView.Drops.Length==0 ? Rect.zero : recoveryPointerRect;
        public int RecoveryPointerBearing { get; private set; }=-1;
        public int RecoveryTargetId { get; private set; }=-1;

        public void SetRecovery(Stage3RecoveryView value)
        {
            recoveryView=value;
            if(!recoveryArtLoaded){recoveryArt=Resources.Load<Stage3RecoveryArt>("Stage3RecoveryArt");recoveryArtLoaded=true;}
            recoveryCamera=scene?.CameraFollow?.BoundCamera;
            if(recoveryArt==null||!recoveryArt.IsReady||recoveryCamera==null)return;
            if(recoveryPresentation==null)
            {
                var root=new GameObject("Hero recovery artwork");root.transform.SetParent(transform,false);
                recoveryPresentation=root.AddComponent<Stage3HeroRecoveryPresentation>();
                recoveryPresentation.Configure(recoveryArt,recoveryCamera);
            }
            recoveryPresentation.gameObject.SetActive(isActiveAndEnabled);
            if(!ReferenceEquals(drawnRecoveryDrops,value?.Drops))
            { recoveryPresentation.SetDrops(value?.Drops);drawnRecoveryDrops=value?.Drops; }
        }

        private Stage3RecoveryDropView RecoveryTarget()
        {
            if(recoveryView?.Drops==null||scene?.PlayerMover==null)return null;
            Stage3RecoveryDropView nearest=null;var distance=float.PositiveInfinity;
            var hero=(Vector2)scene.PlayerMover.transform.position;
            foreach(var drop in recoveryView.Drops)
            {
                if(drop.ItemCount<=0)continue;
                if(drop.Id==recoveryView.NearbyDropId)return drop;
                var d=(drop.Position-hero).sqrMagnitude;
                if(d<distance){nearest=drop;distance=d;}
            }
            return nearest;
        }

        private Vector3 RecoveryScreenPoint(Stage3RecoveryDropView drop)
        {
            var p=recoveryCamera.WorldToScreenPoint(Stage3HeroRecoveryPresentation.GroundPosition(drop.Position));
            return new Vector3(p.x,Screen.height-p.y,p.z);
        }

        private Rect RecoveryMarkerRect(Stage3RecoveryDropView drop)
        {
            var p=RecoveryScreenPoint(drop);var s=UiScale;
            var bagHeight=recoveryArt!=null&&recoveryArt.WorldBag!=null
                ? .64f*recoveryArt.WorldBag.bounds.size.y/recoveryArt.WorldBag.bounds.size.x: .55f;
            var pixels=bagHeight*Screen.height/(2f*Mathf.Max(.01f,recoveryCamera.orthographicSize));
            return new Rect(p.x-19f*s,p.y-pixels-46f*s,38f*s,38f*s);
        }

        public Rect RecoveryWorldMarkerRect(int id)
        {
            if(recoveryCamera==null||recoveryView?.Drops==null)return Rect.zero;
            foreach(var drop in recoveryView.Drops)if(drop.Id==id)return RecoveryMarkerRect(drop);
            return Rect.zero;
        }

        private bool RecoveryMarkerVisible(Stage3RecoveryDropView drop)
        {
            var p=RecoveryScreenPoint(drop);var r=RecoveryMarkerRect(drop);var s=UiScale;
            if(p.z<=0||r.x<12f*s||r.xMax>Screen.width-12f*s||r.y<12f*s||p.y>WorldHudRect.y-12f*s)return false;
            // Preserve the existing cards, development access and selected-building strip.
            var topRight=new Rect(StatusRect.x-8f*s,0,StatusRect.width+16f*s,StatusRect.yMax+8f*s);
            return !r.Overlaps(ResourcesRect)&&!r.Overlaps(topRight)&&!r.Overlaps(CycleDialRect)&&!r.Overlaps(CyclePhaseRect);
        }

        private void ResetRecoveryPointerMetadata()
        { recoveryPointerRect=Rect.zero;RecoveryPointerBearing=-1;RecoveryTargetId=-1; }

        private void DrawRecoveryWorld()
        {
            recoveryPointerRect=Rect.zero;RecoveryPointerBearing=-1;RecoveryTargetId=-1;
            if(recoveryArt==null||recoveryCamera==null||recoveryView?.Drops==null)return;
            var target=RecoveryTarget();if(target==null)return;
            RecoveryTargetId=target.Id;var s=UiScale;
            foreach(var drop in recoveryView.Drops)
            {
                if(drop.ItemCount<=0||!RecoveryMarkerVisible(drop))continue;
                var r=RecoveryMarkerRect(drop);DrawIcon(r,recoveryArt.WorldMedallion);
                if(drop.Id!=target.Id)continue;
                var p=RecoveryScreenPoint(drop);
                var height=drop.Id==recoveryView.NearbyDropId&&recoveryView.CanRecover?58f*s:28f*s;
                var x=p.x+30f*s;
                if(x+166f*s>Screen.width-16f*s)x=p.x-196f*s;
                var safeTop=Mathf.Max(90f*s,CyclePhaseRect.yMax+10f*s);
                var y=Mathf.Clamp(p.y-48f*s,safeTop,Mathf.Max(safeTop,WorldHudRect.y-height-12f*s));
                DrawRecoveryPill(new Rect(x,y,166f*s,28f*s),RecoveryCaption);
                if(height>28f*s)DrawRecoveryPill(new Rect(x,y+30f*s,166f*s,28f*s),"E · Забрать вещи");
            }
            if(!RecoveryMarkerVisible(target))DrawRecoveryPointer(target);
        }

        private void DrawRecoveryPill(Rect r,string text)
        {
            GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0f,
                NativeGuiColor(new Color(.82f,.74f,.84f)),0f,r.height*.48f);
            GUI.DrawTexture(Inset(r,1.3f*UiScale),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0f,
                NativeGuiColor(PlumInk),0f,r.height*.48f);
            if(text!=null)PaleLabel(Inset(r,5f*UiScale),text,14,TextAnchor.MiddleCenter);
        }

        private void DrawRecoveryPointer(Stage3RecoveryDropView target)
        {
            var s=UiScale;var p=RecoveryScreenPoint(target);
            var hero=recoveryCamera.WorldToScreenPoint(scene.PlayerMover.transform.position);
            var direction=new Vector2(p.x-hero.x,p.y-(Screen.height-hero.y));
            if(p.z<0&&!recoveryCamera.orthographic)direction=-direction;
            if(direction.sqrMagnitude<.01f)direction=Vector2.up;
            var bearing=(Mathf.RoundToInt(Mathf.Atan2(direction.y,direction.x)/(Mathf.PI*.25f))+8)%8;
            const float width=190f,height=30f;
            // The whole tooltip fits this rect; the upper exclusion includes the F3 button.
            var safe=Rect.MinMaxRect(24f*s,Mathf.Max(90f*s,CyclePhaseRect.yMax+10f*s),Screen.width-24f*s,WorldHudRect.y-18f*s);
            var center=safe.center;
            var half=new Vector2(Mathf.Max(1f,(safe.width-width*s)*.5f),Mathf.Max(1f,(safe.height-height*s)*.5f));
            var t=Mathf.Min(half.x/Mathf.Max(.001f,Mathf.Abs(direction.x)),half.y/Mathf.Max(.001f,Mathf.Abs(direction.y)));
            var position=center+direction*t;
            var r=new Rect(position.x-width*s*.5f,position.y-height*s*.5f,width*s,height*s);
            recoveryPointerRect=r;RecoveryPointerBearing=bearing;
            DrawRecoveryPill(r,null);
            var left=bearing>=3&&bearing<=5;
            var arrow=new Rect(left?r.x+8f*s:r.xMax-28f*s,r.y+4f*s,20f*s,r.height-8f*s);
            PaleLabel(arrow,RecoveryArrows[bearing],18,TextAnchor.MiddleCenter);
            var label=new Rect(left?r.x+32f*s:r.x+10f*s,r.y+4f*s,r.width-42f*s,r.height-8f*s);
            PaleLabel(label,RecoveryCaption,14,TextAnchor.MiddleCenter);
        }

        private void DrawRecoveryMapMarkers()
        {
            if(recoveryArt==null||recoveryView?.Drops==null)return;
            var s=UiScale;
            var hero=WorldToLargeMapPoint(scene.PlayerMover.transform.position)-MapViewportRect.position;
            var heroCaption=new Rect(Mathf.Clamp(hero.x+22f*s,8f*s,MapViewportRect.width-104f*s),
                Mathf.Clamp(hero.y+18f*s,8f*s,MapViewportRect.height-34f*s),96f*s,26f*s);
            foreach(var drop in recoveryView.Drops)
            {
                if(drop.ItemCount<=0)continue;
                var p=WorldToLargeMapPoint(drop.Position)-MapViewportRect.position;
                if(p.x<20f*s||p.y<20f*s||p.x>MapViewportRect.width-20f*s||p.y>MapViewportRect.height-20f*s)continue;
                var anchor=p;
                var separation=p-hero;
                if(separation.magnitude<38f*s)
                    p=hero+(separation.sqrMagnitude>.01f?separation.normalized:Vector2.right)*38f*s;
                if(new Rect(p.x-20f*s,p.y-20f*s,40f*s,40f*s).Overlaps(heroCaption))
                    p=new Vector2(p.x,hero.y-38f*s);
                p=new Vector2(Mathf.Clamp(p.x,20f*s,MapViewportRect.width-20f*s),
                    Mathf.Clamp(p.y,20f*s,MapViewportRect.height-20f*s));
                if((p-anchor).sqrMagnitude>1f)
                    DrawGlyphLine(anchor,p,1.2f*s,new Color(.82f,.74f,.84f,.8f));
                DrawIcon(new Rect(p.x-19f*s,p.y-19f*s,38f*s,38f*s),recoveryArt.MapIcon);
                var label=new Rect(Mathf.Clamp(p.x+24f*s,8f*s,MapViewportRect.width-174f*s),
                    Mathf.Clamp(p.y+8f*s,8f*s,MapViewportRect.height-36f*s),166f*s,28f*s);
                if(label.Overlaps(heroCaption))label.y=Mathf.Max(8f*s,heroCaption.y-34f*s);
                DrawRecoveryPill(label,RecoveryCaption);
            }
        }

        private void DrawRecoveryMinimap(Rect inner,Rect uv)
        {
            if(recoveryArt==null||recoveryView?.Drops==null)return;
            var map=scene.MapDefinition;var size=20f*UiScale;
            foreach(var drop in recoveryView.Drops)
            {
                if(drop.ItemCount<=0)continue;
                var cell=map.WorldToCellCoordinates(drop.Position)-(Vector2)map.MinimumCell+Vector2.one*.5f;
                var unit=new Vector2((cell.x/map.Size.x-uv.x)/uv.width,(cell.y/map.Size.y-uv.y)/uv.height);
                var offset=unit-Vector2.one*.5f;
                if(offset.magnitude>.48f-size*.5f/inner.width)continue;
                var p=new Vector2(inner.x+unit.x*inner.width,inner.yMax-unit.y*inner.height);
                var anchor=p;var separation=p-inner.center;
                if(separation.magnitude<20f*UiScale)
                {
                    p=inner.center+(separation.sqrMagnitude>.01f?separation.normalized:Vector2.right)*20f*UiScale;
                    DrawGlyphLine(anchor,p,UiScale,new Color(.82f,.74f,.84f,.8f));
                }
                DrawIcon(new Rect(p.x-size*.5f,p.y-size*.5f,size,size),recoveryArt.MapIcon);
            }
        }
    }
}
