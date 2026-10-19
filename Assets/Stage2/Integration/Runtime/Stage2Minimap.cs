using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public enum Stage2MinimapMarkerKind { Wood, Stone, Metal, Tower, Enemy, Portal, Throne, Hero, Research, Teleport }

    public readonly struct Stage2MinimapMarker
    {
        public Stage2MinimapMarker(Stage2MinimapMarkerKind kind, Vector2 world)
        { Kind = kind; World = world; }
        public Stage2MinimapMarkerKind Kind { get; }
        public Vector2 World { get; }
    }

    /// <summary>Passive, scene-owned map. Terrain is cached; markers read the live rules.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage2Minimap : MonoBehaviour
    {
        private Stage2SceneBootstrap bootstrap;
        private Stage2GameComposition composition;
        private readonly List<Stage2MinimapMarker> markers = new List<Stage2MinimapMarker>();
        private readonly Vector2[] cameraCorners = new Vector2[4];
        private Texture2D terrain;
        private Texture2D districtTerrain;
        private Texture2D circle;
        private Texture2D diamond;
        private Texture2D arrow;
        private GUIStyle textStyle;
        private GUIStyle titleStyle;
        private Vector2? researchPosition, baseTeleportPosition, worldTeleportPosition;
        private static readonly Color Ink = new Color(.07f, .10f, .09f, 1f);
        private static readonly Color Gold = new Color(.72f, .65f, .42f, 1f);

        public Texture2D Terrain => terrain;
        public bool SuppressPresentation { get; set; }
        public Texture2D DistrictTerrain => districtTerrain;
        public IReadOnlyList<Stage2MinimapMarker> Markers => markers;
        public void SetWorldTravelMarkers(Vector2? research, Vector2? baseCircle, Vector2? worldCircle)
        {
            researchPosition=research; baseTeleportPosition=baseCircle; worldTeleportPosition=worldCircle;
        }
        public Rect PanelRect => Layout(new Vector2(Screen.width, Screen.height));
        public Rect MapRect => MapArea(PanelRect, bootstrap.MapDefinition.Size);

        public void Initialize(Stage2SceneBootstrap scene, Stage2GameComposition game)
        {
            bootstrap = scene;
            composition = game;
            BuildTerrain();
            if (circle == null) circle = CreateSymbol(0);
            if (diamond == null) diamond = CreateSymbol(1);
            if (arrow == null) arrow = CreateSymbol(2);
            RefreshMarkers();
        }

        public void SetResourceDistricts(IReadOnlyList<ResourceDistrict> districts)
        {
            if (districtTerrain != null) Destroy(districtTerrain);
            districtTerrain = null;
            if (districts == null || terrain == null) return;
            var lookup = new Dictionary<Vector2Int, MapResourceKind>();
            foreach (var district in districts)
                foreach (var cell in district.Cells) lookup[cell] = district.Kind;
            var pixels = new Color32[terrain.width * terrain.height];
            var map = bootstrap.MapDefinition;
            for (var y = 0; y < terrain.height; y++)
            for (var x = 0; x < terrain.width; x++)
            {
                var cell = map.MinimumCell + new Vector2Int(x * map.Size.x / terrain.width, y * map.Size.y / terrain.height);
                if (!lookup.TryGetValue(cell, out var kind)) continue;
                pixels[y * terrain.width + x] = kind == MapResourceKind.Wood ? new Color(.08f,.20f,.06f,.9f) :
                    kind == MapResourceKind.Stone ? new Color(.70f,.68f,.58f,.95f) : new Color(.42f,.29f,.20f,.95f);
            }
            districtTerrain = new Texture2D(terrain.width, terrain.height, TextureFormat.RGBA32, false)
            { name = "Minimap generated resource districts", filterMode = FilterMode.Bilinear };
            districtTerrain.SetPixels32(pixels); districtTerrain.Apply(false, true);
        }

        // GUI uses top-left pixels; Input System uses bottom-left pixels.
        public bool ContainsScreenPoint(Vector2 screen) => !SuppressPresentation && isActiveAndEnabled && terrain != null &&
            PanelRect.Contains(new Vector2(screen.x, Screen.height - screen.y));

        public static Rect Layout(Vector2 screen)
        {
            var side = Mathf.Min(Mathf.Clamp(screen.y * .27f, 150f, 256f),
                Mathf.Max(24f, screen.x - 44f), Mathf.Max(24f, screen.y - 68f));
            var margin = Mathf.Min(16f, Mathf.Max(0f, (screen.x - side - 12f) * .5f));
            return new Rect(margin, Mathf.Max(0f, screen.y - side - 48f - margin), side + 12f, side + 48f);
        }

        private static Rect MapArea(Rect panel, Vector2Int size)
        {
            var side = panel.width - 12f;
            var scale = side / Mathf.Max(size.x, size.y);
            return new Rect(panel.x + 6f + (side - size.x * scale) * .5f,
                panel.y + 25f + (side - size.y * scale) * .5f, size.x * scale, size.y * scale);
        }

        public Vector2 WorldToMapPoint(Vector2 world)
        {
            var map = bootstrap.MapDefinition;
            var cell = map.WorldToCellCoordinates(world) - (Vector2)map.MinimumCell + Vector2.one * .5f;
            var area = MapRect;
            return new Vector2(area.x + cell.x / map.Size.x * area.width,
                area.yMax - cell.y / map.Size.y * area.height);
        }

        public bool GetCameraFootprint(Vector2[] corners)
        {
            if (bootstrap == null || corners == null || corners.Length < 4) return false;
            var camera = bootstrap.CameraFollow.BoundCamera;
            var ground = new Plane(Vector3.forward, Vector3.zero);
            for (var i = 0; i < 4; i++)
            {
                // Perimeter order, rather than the crossing order of the viewport bits.
                var ray = camera.ViewportPointToRay(new Vector3(i == 1 || i == 2 ? 1f : 0f, i >= 2 ? 1f : 0f));
                if (!ground.Raycast(ray, out var distance)) return false;
                corners[i] = WorldToMapPoint(ray.GetPoint(distance));
            }
            return true;
        }

        private void LateUpdate() => RefreshMarkers();

        private void RefreshMarkers()
        {
            markers.Clear();
            if (composition == null || !composition.IsInitialized) return;
            foreach (var node in composition.Gathering.Economy.Nodes)
                if (!node.IsDepleted)
                    markers.Add(new Stage2MinimapMarker((Stage2MinimapMarkerKind)node.Resource, node.WorldPosition));
            var defense = composition.Building.Defense;
            for (var i = 0; i < defense.TowerCount; i++)
            {
                var tower = defense.GetTower(i);
                if (tower != null && tower.IsAlive)
                    markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Tower, tower.transform.position));
            }
            for (var i = 0; i < composition.Enemies.ActiveCount; i++)
            {
                var enemy = composition.Enemies.GetEnemy(i);
                if (enemy != null && enemy.IsAlive)
                    markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Enemy, enemy.transform.position));
            }
            markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Portal, bootstrap.EntranceMarker.position));
            markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Throne, bootstrap.ThroneMarker.position));
            markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Hero, bootstrap.PlayerMover.transform.position));
            if (researchPosition.HasValue) markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Research, researchPosition.Value));
            if (baseTeleportPosition.HasValue) markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Teleport, baseTeleportPosition.Value));
            if (worldTeleportPosition.HasValue) markers.Add(new Stage2MinimapMarker(Stage2MinimapMarkerKind.Teleport, worldTeleportPosition.Value));
        }

        private void BuildTerrain()
        {
            if (terrain != null) Destroy(terrain);
            if (districtTerrain != null) Destroy(districtTerrain);
            districtTerrain = null;
            var map = bootstrap.MapDefinition;
            var scale = Mathf.Min(3f, 512f / Mathf.Max(map.Size.x, map.Size.y));
            var width = Mathf.Max(1, Mathf.RoundToInt(map.Size.x * scale));
            var height = Mathf.Max(1, Mathf.RoundToInt(map.Size.y * scale));
            terrain = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = "Minimap authored terrain", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var cell = map.MinimumCell + new Vector2Int(x * map.Size.x / width, y * map.Size.y / height);
                var road = map.GetSurface(cell) == MapSurfaceKind.InvasionCorridor;
                var shade = Mathf.PerlinNoise(cell.x * .16f + 100f, cell.y * .16f + 100f);
                var color = road ? Color.Lerp(new Color(.53f, .53f, .45f), new Color(.64f, .63f, .53f), shade) :
                    Color.Lerp(new Color(.19f, .29f, .17f), new Color(.30f, .40f, .22f), shade);
                pixels[y * width + x] = color;
            }
            terrain.SetPixels32(pixels);
            terrain.Apply(false, false);
        }

        private static Texture2D CreateSymbol(int shape)
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = "Minimap symbol " + shape, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var p = new Vector2((x + .5f) / size * 2f - 1f, (y + .5f) / size * 2f - 1f);
                var distance = shape == 0 ? p.magnitude : shape == 1 ? Mathf.Abs(p.x) + Mathf.Abs(p.y) :
                    Mathf.Max(Mathf.Abs(p.x) * 1.8f + p.y * .7f, -p.y);
                pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01((1f - distance) * size * .5f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void OnGUI()
        {
            if (SuppressPresentation || terrain == null || composition == null || !composition.IsInitialized || Event.current.type != EventType.Repaint) return;
            var oldColor = GUI.color;
            var oldDepth = GUI.depth;
            GUI.color = Color.white;
            GUI.depth = -20;
            try
            {
                var panel = PanelRect;
                var area = MapRect;
                var scale = (panel.width - 12f) / 216f;
                if (textStyle == null)
                {
                    textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
                    titleStyle = new GUIStyle(textStyle) { fontStyle = FontStyle.Bold };
                }
                textStyle.fontSize = Mathf.RoundToInt(11f * Mathf.Clamp(scale, .85f, 1.2f));
                titleStyle.fontSize = textStyle.fontSize;
                textStyle.normal.textColor = new Color(.82f, .83f, .72f);
                titleStyle.normal.textColor = new Color(.89f, .85f, .65f);
                Fill(new Rect(panel.x + 3f, panel.y + 4f, panel.width, panel.height), new Color(0f, 0f, 0f, .45f));
                Fill(panel, Ink);
                Outline(new Rect(panel.x + 1f, panel.y + 1f, panel.width - 2f, panel.height - 2f), Gold, 1f);
                GUI.Label(new Rect(panel.x + 9f, panel.y + 2f, panel.width - 34f, 22f), "КАРТА", titleStyle);
                GUI.Label(new Rect(panel.xMax - 20f, panel.y + 2f, 16f, 22f), "N", textStyle);
                Fill(new Rect(panel.x + 5f, panel.y + 24f, panel.width - 10f, panel.width - 10f), new Color(.12f, .18f, .13f));
                GUI.DrawTexture(area, terrain);
                if (districtTerrain != null) GUI.DrawTexture(area, districtTerrain);
                Outline(area, new Color(.43f, .48f, .32f), 1f);
                if (GetCameraFootprint(cameraCorners))
                    for (var i = 0; i < 4; i++)
                    {
                        Line(cameraCorners[i], cameraCorners[(i + 1) % 4], Ink, 3f);
                        Line(cameraCorners[i], cameraCorners[(i + 1) % 4], new Color(.97f, .92f, .66f, .9f), 1f);
                    }
                foreach (var marker in markers) DrawMarker(WorldToMapPoint(marker.World), marker.Kind, Mathf.Clamp(scale, .85f, 1.2f));
                var legendY = panel.yMax - 21f;
                var step = (panel.width - 12f) / 3f;
                Legend(new Vector2(panel.x + 14f, legendY + 10f), Stage2MinimapMarkerKind.Hero, "Герой", step);
                Legend(new Vector2(panel.x + 14f + step, legendY + 10f), Stage2MinimapMarkerKind.Portal, "Врата", step);
                Legend(new Vector2(panel.x + 14f + step * 2f, legendY + 10f), Stage2MinimapMarkerKind.Throne, "Трон", step);
            }
            finally { GUI.color = oldColor; GUI.depth = oldDepth; }
        }

        private void Legend(Vector2 point, Stage2MinimapMarkerKind kind, string label, float width)
        {
            DrawMarker(point, kind, .8f);
            GUI.Label(new Rect(point.x + 9f, point.y - 10f, width - 10f, 20f), label, textStyle);
        }

        private void DrawMarker(Vector2 point, Stage2MinimapMarkerKind kind, float scale)
        {
            var symbol = circle;
            var color = new Color(.96f, .37f, .25f);
            var size = 5f;
            switch (kind)
            {
                case Stage2MinimapMarkerKind.Wood: symbol = diamond; color = new Color(.63f, .87f, .30f); size = 8f; break;
                case Stage2MinimapMarkerKind.Stone: symbol = diamond; color = new Color(.83f, .85f, .76f); size = 8f; break;
                case Stage2MinimapMarkerKind.Metal: symbol = diamond; color = new Color(.59f, .65f, .94f); size = 8f; break;
                case Stage2MinimapMarkerKind.Tower: symbol = Texture2D.whiteTexture; color = new Color(.94f, .73f, .33f); size = 5f; break;
                case Stage2MinimapMarkerKind.Portal: color = new Color(.95f, .27f, .25f); size = 12f; break;
                case Stage2MinimapMarkerKind.Throne: symbol = diamond; color = new Color(.35f, .85f, 1f); size = 14f; break;
                case Stage2MinimapMarkerKind.Hero: symbol = arrow; color = Color.white; size = 12f; break;
                case Stage2MinimapMarkerKind.Research: symbol = Texture2D.whiteTexture; color = new Color(.96f,.83f,.46f); size = 8f; break;
                case Stage2MinimapMarkerKind.Teleport: color = new Color(.83f,.72f,1f); size = 10f; break;
            }
            size *= scale;
            Symbol(point, symbol, size + 3f, Ink);
            Symbol(point, symbol, size, color);
            if (kind == Stage2MinimapMarkerKind.Portal) Symbol(point, circle, size * .45f, Ink);
            if (kind == Stage2MinimapMarkerKind.Teleport) Symbol(point, circle, size * .6f, Ink);
            if (kind == Stage2MinimapMarkerKind.Throne) Symbol(point, diamond, size * .4f, Color.white);
        }

        private static void Symbol(Vector2 point, Texture texture, float size, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(point.x - size * .5f, point.y - size * .5f, size, size), texture);
            GUI.color = Color.white;
        }

        private static void Fill(Rect area, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private static void Outline(Rect area, Color color, float width)
        {
            Line(new Vector2(area.x, area.y), new Vector2(area.xMax, area.y), color, width);
            Line(new Vector2(area.xMax, area.y), new Vector2(area.xMax, area.yMax), color, width);
            Line(new Vector2(area.xMax, area.yMax), new Vector2(area.x, area.yMax), color, width);
            Line(new Vector2(area.x, area.yMax), new Vector2(area.x, area.y), color, width);
        }

        private static void Line(Vector2 from, Vector2 to, Color color, float width)
        {
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg, from);
            GUI.color = color;
            GUI.DrawTexture(new Rect(from.x, from.y - width * .5f, Vector2.Distance(from, to), width), Texture2D.whiteTexture);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }

        private void OnDestroy()
        {
            if (terrain != null) Destroy(terrain);
            if (districtTerrain != null) Destroy(districtTerrain);
            if (circle != null) Destroy(circle);
            if (diamond != null) Destroy(diamond);
            if (arrow != null) Destroy(arrow);
        }
    }
}
