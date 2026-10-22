using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private Stage2Minimap mapSource;
        private Vector2 mapCenterCells;
        private bool mapInitialized, mapDragging;
        private GUIStyle mapMarkerStyle;
        public Rect MapViewportRect => ActiveWindow!=Stage3PlayerWindow.Map ? Rect.zero :
            new Rect(0,184f*UiScale,Screen.width,Mathf.Max(40f,FullscreenFooterRect.y-184f*UiScale));
        public Rect MapRecenterRect => ActiveWindow!=Stage3PlayerWindow.Map ? Rect.zero :
            new Rect(Screen.width-216f*UiScale,FullscreenFooterRect.y+12f*UiScale,184f*UiScale,44f*UiScale);
        public Rect MapZoomInRect => ActiveWindow!=Stage3PlayerWindow.Map ? Rect.zero :
            new Rect(MapRecenterRect.x-55f*UiScale,MapRecenterRect.y,44f*UiScale,44f*UiScale);
        public Rect MapZoomOutRect => ActiveWindow!=Stage3PlayerWindow.Map ? Rect.zero :
            new Rect(MapZoomInRect.x-55f*UiScale,MapZoomInRect.y,44f*UiScale,44f*UiScale);
        public float MapZoom { get; private set; } = 1f;
        public Vector2 MapCenter => scene == null ? Vector2.zero : scene.MapDefinition.CellCoordinatesToWorld(mapCenterCells);

        private void InitializeMapView()
        {
            mapSource = game == null ? null : game.GetComponent<Stage2Minimap>();
            if (scene == null || scene.MapDefinition == null) return;
            mapInitialized = true;
            RecenterMap();
        }

        private void ReleaseMapDrag()
        {
            if (mapDragging) GUIUtility.hotControl = 0;
            mapDragging = false;
            ResetMapTouchInput();
        }
        private void ReleaseMapView() { ReleaseMapDrag(); ReleaseGuildMapView(); mapSource = null; guildMapSource=null; mapInitialized = false; mapMarkerStyle = null; }

        public void RecenterMap()
        {
            if (scene == null || scene.MapDefinition == null || scene.PlayerMover == null) return;
            mapCenterCells = scene.MapDefinition.WorldToCellCoordinates(scene.PlayerMover.transform.position);
            ClampMapCenter();
        }

        public void ZoomMap(float factor)
        {
            if (float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0f) return;
            MapZoom = Mathf.Clamp(MapZoom * factor, 1f, 12f);
            ClampMapCenter();
        }

        public void PanMap(Vector2 deltaPixels)
        {
            if (MapViewportRect.width <= 0f || MapViewportRect.height <= 0f) return;
            var extent = MapViewExtent();
            mapCenterCells += new Vector2(-deltaPixels.x / MapViewportRect.width * extent.x,
                deltaPixels.y / MapViewportRect.height * extent.y);
            ClampMapCenter();
        }

        private Vector2 MapViewExtent()
        {
            if (scene == null || scene.MapDefinition == null) return Vector2.one;
            var size = scene.MapDefinition.Size;
            var aspect = MapViewportRect.width / Mathf.Max(1f, MapViewportRect.height);
            var height = Mathf.Max(size.y, size.x / Mathf.Max(.1f, aspect)) / MapZoom;
            return new Vector2(height * aspect, height);
        }

        private void ClampMapCenter()
        {
            if (scene == null || scene.MapDefinition == null || MapViewportRect.width <= 0f || MapViewportRect.height <= 0f) return;
            var map = scene.MapDefinition;
            var minimum = (Vector2)map.MinimumCell - Vector2.one * .5f;
            var maximum = minimum + (Vector2)map.Size;
            var half = MapViewExtent() * .5f;
            mapCenterCells.x = half.x * 2f >= map.Size.x ? (minimum.x + maximum.x) * .5f :
                Mathf.Clamp(mapCenterCells.x, minimum.x + half.x, maximum.x - half.x);
            mapCenterCells.y = half.y * 2f >= map.Size.y ? (minimum.y + maximum.y) * .5f :
                Mathf.Clamp(mapCenterCells.y, minimum.y + half.y, maximum.y - half.y);
        }

        public Vector2 WorldToLargeMapPoint(Vector2 world)
        {
            if (scene == null) return MapViewportRect.center;
            var offset = scene.MapDefinition.WorldToCellCoordinates(world) - mapCenterCells;
            var extent = MapViewExtent();
            return MapViewportRect.center + new Vector2(offset.x / extent.x * MapViewportRect.width,
                -offset.y / extent.y * MapViewportRect.height);
        }

        private void DrawMap(Rect content)
        {
            if (!mapInitialized) InitializeMapView();
            if (mapSource == null && game != null) mapSource = game.GetComponent<Stage2Minimap>();
            var s = UiScale;
            ClampMapCenter();
            Label(new Rect(48f*s,141f*s,Screen.width-96f*s,36f*s),"Карта",27,TextAnchor.MiddleLeft);
            Fill(MapViewportRect,new Color(.17f,.25f,.17f));
            if (mapSource != null && mapSource.Terrain != null)
            {
                GUI.BeginGroup(MapViewportRect);
                try
                {
                    var map = scene.MapDefinition;
                    var min = (Vector2)map.MinimumCell - Vector2.one * .5f;
                    var extent = MapViewExtent();
                    var left = (min.x - mapCenterCells.x) / extent.x * MapViewportRect.width + MapViewportRect.width * .5f;
                    var top = MapViewportRect.height * .5f - (min.y + map.Size.y - mapCenterCells.y) / extent.y * MapViewportRect.height;
                    var terrainRect = new Rect(left, top, map.Size.x / extent.x * MapViewportRect.width,
                        map.Size.y / extent.y * MapViewportRect.height);
                    DrawMapTerrain(terrainRect,mapSource.Terrain);
                    if (mapSource.DistrictTerrain != null) DrawMapTerrain(terrainRect,mapSource.DistrictTerrain);
                    foreach (var marker in mapSource.Markers)
                    {
                        if (marker.Kind == Stage2MinimapMarkerKind.City) continue;
                        var p = WorldToLargeMapPoint(marker.World) - MapViewportRect.position;
                        if (p.x < 0 || p.y < 0 || p.x > MapViewportRect.width || p.y > MapViewportRect.height) continue;
                        DrawWorldMapMarker(p, marker.Kind, s);
                    }
                    DrawRecoveryMapMarkers();
                    DrawCityLargeMapMarker();
                    DrawGuildMapMarkers();
                }
                finally { GUI.EndGroup(); }
            }
            var e = Event.current;
            HandleGuildMapPointer();
            var dragControl = GUIUtility.GetControlID(FocusType.Passive);
            if (e.type == EventType.ScrollWheel && MapViewportRect.Contains(e.mousePosition))
            {
                if (!Stage2MacTrackpad.LastScrollWasPrecise)
                    ZoomMapAt(Mathf.Pow(1.15f, -e.delta.y), e.mousePosition);
                e.Use();
            }
            if (!MapTouchOwnsPointer && e.type == EventType.MouseDown && e.button == 0 && MapViewportRect.Contains(e.mousePosition))
            { mapDragging = true; GUIUtility.hotControl = dragControl; e.Use(); }
            if (!MapTouchOwnsPointer && e.type == EventType.MouseDrag && mapDragging && e.button == 0)
            { PanMap(e.delta); e.Use(); }
            if (e.type == EventType.MouseUp && mapDragging)
            { mapDragging = false; GUIUtility.hotControl = 0; e.Use(); }
            var y = FullscreenFooterRect.y + 12f * s;
            if (Button(MapRecenterRect, "К герою")) RecenterMap();
            if (Button(MapZoomInRect, "+")) ZoomMap(1.4f);
            if (Button(MapZoomOutRect, "−")) ZoomMap(1f / 1.4f);
            var legendKinds=new[]{Stage2MinimapMarkerKind.Hero,Stage2MinimapMarkerKind.Portal,Stage2MinimapMarkerKind.Throne};
            var legendNames=new[]{"Герой","Врата","Трон"};
            for(var i=0;i<3;i++)
            {
                var x=(44f+i*112f)*s;
                DrawWorldMapMarker(new Vector2(x,y+22f*s),legendKinds[i],s*.8f);
                Label(new Rect(x+20f*s,y,84f*s,44f*s),legendNames[i],14,TextAnchor.MiddleLeft);
            }
            var recoveryLegend=recoveryArt!=null&&recoveryView?.Drops!=null&&recoveryView.Drops.Length>0;
            if(recoveryLegend)
            {
                DrawIcon(new Rect(370f*s,y+8f*s,28f*s,28f*s),recoveryArt.MapIcon);
                Label(new Rect(406f*s,y,148f*s,44f*s),RecoveryCaption,14,TextAnchor.MiddleLeft);
            }
            var hintX=(recoveryLegend?568f:408f)*s;
            Label(new Rect(hintX,y,Mathf.Max(10f,MapZoomOutRect.x-hintX-16f*s),44f*s),
                recoveryLegend?"Перетаскивание · Обзор     Колесо · Масштаб":
                "Перетаскивание / 2 пальца · Обзор     Колесо / Pinch · Масштаб",14,TextAnchor.MiddleCenter,true);
        }

        private void DrawMapTerrain(Rect terrainRect,Texture texture)
        {
            // The custom Metal shader must never paint beyond the regional viewport.
            var clipped=Rect.MinMaxRect(Mathf.Max(0f,terrainRect.x),Mathf.Max(0f,terrainRect.y),
                Mathf.Min(MapViewportRect.width,terrainRect.xMax),Mathf.Min(MapViewportRect.height,terrainRect.yMax));
            if(clipped.width<=0f||clipped.height<=0f) return;
            var uv=new Rect((clipped.x-terrainRect.x)/terrainRect.width,
                1f-(clipped.yMax-terrainRect.y)/terrainRect.height,
                clipped.width/terrainRect.width,clipped.height/terrainRect.height);
            DrawUiTexture(clipped,texture,uv);
        }

        private void DrawWorldMapMarker(Vector2 p, Stage2MinimapMarkerKind kind, float s)
        {
            if (kind == Stage2MinimapMarkerKind.Hero)
            {
                var ring=new Rect(p.x-12f*s,p.y-12f*s,24f*s,24f*s);
                DrawDisc(ring,new Color(.97f,.95f,.85f));
                DrawDisc(Inset(ring,2f*s),PlumInk);
                DrawDisc(Inset(ring,4f*s),Accent);
                var heroStyle=GetLabelStyle(Mathf.RoundToInt(17f*s/UiScale),false);
                heroStyle.alignment=TextAnchor.MiddleCenter;heroStyle.normal.textColor=Color.white;
                GUI.Label(new Rect(p.x-10f*s,p.y-11f*s,20f*s,20f*s),LabelContent("▲"),heroStyle);
                return;
            }
            var color = Accent; var glyph = "◆"; var size = 15;
            switch (kind)
            {
                case Stage2MinimapMarkerKind.Hero: glyph = "●"; color = new Color(.96f,.91f,.68f); size = 24; break;
                case Stage2MinimapMarkerKind.Portal: glyph = "◉"; color = new Color(.95f,.22f,.31f); size = 23; break;
                case Stage2MinimapMarkerKind.Throne: color = new Color(.25f,.83f,1f); size = 23; break;
                case Stage2MinimapMarkerKind.Enemy: glyph = "•"; color = new Color(.95f,.25f,.26f); size = 15; break;
                case Stage2MinimapMarkerKind.Wood: glyph = "●"; color = new Color(.40f,.70f,.30f); break;
                case Stage2MinimapMarkerKind.Stone: color = new Color(.88f,.85f,.76f); break;
                case Stage2MinimapMarkerKind.Metal: color = new Color(.72f,.61f,.47f); break;
                case Stage2MinimapMarkerKind.Teleport: glyph = "◎"; color = new Color(.35f,.85f,1f); size = 21; break;
                case Stage2MinimapMarkerKind.Research: glyph = "◇"; color = new Color(.86f,.67f,1f); size = 20; break;
            }
            if (mapMarkerStyle == null) mapMarkerStyle = new GUIStyle(GetLabelStyle(15,false)) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var style = mapMarkerStyle; style.fontSize = Mathf.RoundToInt(size * s);
            var rect = new Rect(p.x - 18f * s, p.y - 18f * s, 36f * s, 36f * s);
            var content=LabelContent(glyph);
            style.normal.textColor = PlumInk; GUI.Label(new Rect(rect.x + s, rect.y + s, rect.width, rect.height), content, style);
            style.normal.textColor = color; GUI.Label(rect, content, style);
        }
    }
}
