using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private Stage3CityArt cityMapArt;
        private bool cityMapArtLoaded;
        public Rect CityLargeMapMarkerRect { get; private set; }
        public Rect CityLargeMapLabelRect { get; private set; }
        public Rect CityMinimapMarkerRect { get; private set; }
        public bool IsCityMinimapOffscreen { get; private set; }

        private void ResetCityMapMetadata()
        {
            CityLargeMapMarkerRect=CityLargeMapLabelRect=CityMinimapMarkerRect=Rect.zero;
            IsCityMinimapOffscreen=false;
        }

        private static bool TryCityMapPosition(Stage2Minimap source,out Vector2 position)
        {
            if(source!=null)
                foreach(var marker in source.Markers)
                    if(marker.Kind==Stage2MinimapMarkerKind.City)
                    { position=marker.World;return true; }
            position=Vector2.zero;return false;
        }

        private void DrawCityMapBadge(Rect rect)
        {
            if(!cityMapArtLoaded)
            { cityMapArt=Resources.Load<Stage3CityArt>("Stage3CityArt");cityMapArtLoaded=true; }
            DrawDisc(rect,new Color(.94f,.88f,.73f));
            DrawDisc(Inset(rect,1.5f*UiScale),PlumInk);
            var house=cityMapArt?.kit!=null&&cityMapArt.kit.Length>0?cityMapArt.kit[0]:null;
            if(house!=null)DrawIcon(Inset(rect,3f*UiScale),house);
            else PaleLabel(rect,"Г",14,TextAnchor.MiddleCenter);
        }

        // Called last inside the large-map viewport group, above resource/enemy dots.
        private void DrawCityLargeMapMarker()
        {
            if(!TryCityMapPosition(mapSource,out var world))return;
            var s=UiScale;var viewport=MapViewportRect;
            var p=WorldToLargeMapPoint(world)-viewport.position;
            if(p.x<0||p.y<0||p.x>viewport.width||p.y>viewport.height)return;
            var anchor=p;
            var hero=WorldToLargeMapPoint(scene.PlayerMover.transform.position)-viewport.position;
            // Keep the actual hero visible when it is standing in the city centre.
            if((p-hero).sqrMagnitude<Mathf.Pow(38f*s,2f))p=hero+Vector2.down*44f*s;
            p=new Vector2(Mathf.Clamp(p.x,24f*s,viewport.width-24f*s),
                Mathf.Clamp(p.y,24f*s,viewport.height-24f*s));
            if((p-anchor).sqrMagnitude>1f)DrawGlyphLine(anchor,p,1.2f*s,new Color(.94f,.88f,.73f));
            var badge=new Rect(p.x-20f*s,p.y-20f*s,40f*s,40f*s);
            DrawCityMapBadge(badge);
            CityLargeMapMarkerRect=new Rect(badge.position+viewport.position,badge.size);
        }

        private void DrawCityMinimapMarker(Stage2Minimap source,Rect inner,Rect uv)
        {
            if(scene?.MapDefinition==null||!TryCityMapPosition(source,out var world))return;
            var map=scene.MapDefinition;var s=UiScale;var size=27f*s;
            var cell=map.WorldToCellCoordinates(world)-(Vector2)map.MinimumCell+Vector2.one*.5f;
            var unit=new Vector2((cell.x/map.Size.x-uv.x)/uv.width,(cell.y/map.Size.y-uv.y)/uv.height);
            var delta=new Vector2((unit.x-.5f)*inner.width,-(unit.y-.5f)*inner.height);
            // A local marker only: distant cities have no rim indicator or screen rect.
            var radius=inner.width*.5f-size*.5f-4f*s;
            if(delta.magnitude>radius){IsCityMinimapOffscreen=true;return;}
            var direction=delta.sqrMagnitude>.001f?delta.normalized:Vector2.right;
            var p=inner.center+delta;
            if(delta.magnitude<27f*s)
            {
                var anchor=p;p=inner.center+direction*27f*s;
                DrawGlyphLine(anchor,p,s,new Color(.94f,.88f,.73f));
            }
            var rect=new Rect(p.x-size*.5f,p.y-size*.5f,size,size);
            var northCaption=new Rect(inner.center.x-16f*s,inner.y-22f*s,32f*s,18f*s);
            if(rect.Overlaps(northCaption))return;
            DrawCityMapBadge(rect);
            CityMinimapMarkerRect=rect;
        }
    }
}
