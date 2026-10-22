using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private static readonly Color QuestIvory = new Color(1f, .94f, .73f);
        private static readonly Color QuestPaper = new Color(.91f, .79f, .53f);
        private Stage3GuildWorldDriver guildMapSource;
        private Stage3PlayerInterfaceBinding guildMapBinding;
        private readonly Dictionary<int, Rect> guildQuestMapRects = new Dictionary<int, Rect>();
        private int pressedMapQuest = -1;
        private Vector2 pressedMapQuestPosition;
        private bool draggedMapQuest;
        public int GuildQuestMapPinCount => guildQuestMapRects.Count;
        public Rect GuildQuestMapArrowRect { get; private set; }
        public Vector2 GuildQuestMapDirection { get; private set; }
        public Rect GuildQuestMinimapMarkerRect { get; private set; }
        public Rect GuildQuestMinimapArrowRect { get; private set; }
        public Vector2 GuildQuestMinimapDirection { get; private set; }
        public bool IsGuildQuestMinimapOffscreen { get; private set; }
        public bool IsGuildQuestTurnIn { get; private set; }
        public Rect GuildQuestMapMarkerRect(int id) => guildQuestMapRects.TryGetValue(id, out var rect) ? rect : Rect.zero;
        private Stage3GuildWorldDriver GuildMapSource
        {
            get
            {
                if (guildMapSource == null && game != null) guildMapSource = game.GetComponent<Stage3GuildWorldDriver>();
                return guildMapSource;
            }
        }
        private Stage3PlayerInterfaceBinding GuildMapBinding
        {
            get
            {
                if (guildMapBinding == null && game != null) guildMapBinding = game.GetComponent<Stage3PlayerInterfaceBinding>();
                return guildMapBinding;
            }
        }

        private void ResetGuildMapMetadata()
        {
            guildQuestMapRects.Clear();
            GuildQuestMapArrowRect = GuildQuestMinimapMarkerRect = GuildQuestMinimapArrowRect = Rect.zero;
            GuildQuestMapDirection = GuildQuestMinimapDirection = Vector2.zero;
            IsGuildQuestMinimapOffscreen = IsGuildQuestTurnIn = false;
        }
        private void ReleaseGuildMapView()
        {
            guildMapBinding = null;
            pressedMapQuest = -1;
            draggedMapQuest = false;
            ResetGuildMapMetadata();
        }

        private void DrawGuildMapMarkers()
        {
            var source = GuildMapSource;
            if (source == null) return;
            var viewport = MapViewportRect;
            var enemies = source.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.Alive) continue;
                var p = WorldToLargeMapPoint(enemy.Position) - viewport.position;
                if (p.x < 0 || p.y < 0 || p.x > viewport.width || p.y > viewport.height) continue;
                DrawWorldMapMarker(p, Stage2MinimapMarkerKind.Enemy, UiScale);
            }
            var binding = GuildMapBinding;
            if (binding?.Guild == null || scene?.PlayerMover == null) return;
            var s = UiScale;
            var hero = WorldToLargeMapPoint(scene.PlayerMover.transform.position);
            if (binding.TryGetTrackedGuildTarget(out var target, out var turnIn))
            {
                IsGuildQuestTurnIn = turnIn;
                var delta = WorldToLargeMapPoint(target) - hero;
                GuildQuestMapDirection = delta.normalized;
                // A short bearing from the hero, never a route or a map-wide line.
                if (viewport.Contains(hero) && delta.magnitude > 62f * s)
                {
                    var start = hero + delta.normalized * 22f * s;
                    var end = hero + delta.normalized * Mathf.Min(104f * s, delta.magnitude - 29f * s);
                    if (Inset(viewport, 9f * s).Contains(end))
                    {
                        DrawQuestDirection(start - viewport.position, end - viewport.position, s);
                        GuildQuestMapArrowRect = QuestArrowBounds(start, end, 8f * s);
                    }
                }
            }
            foreach (var contract in binding.Guild.Active)
            {
                if (!binding.TryGetGuildQuestTarget(contract.Quest.id, out var world, out var returnToGuild)) continue;
                var anchor = WorldToLargeMapPoint(world);
                if (!viewport.Contains(anchor)) continue;
                var size = new Vector2(27f, 33f) * s;
                var p = anchor;
                var rect = new Rect(p - size * .5f, size);
                // Separate guild turn-ins, the city house and the cyan hero without moving the true anchor.
                if ((p - hero).sqrMagnitude < Mathf.Pow(34f * s, 2f))
                    p += new Vector2(39f, -23f) * s;
                if (new Rect(p - size * .5f, size).Overlaps(CityLargeMapMarkerRect))
                    p = new Vector2(CityLargeMapMarkerRect.xMax + size.x * .5f + 8f * s, CityLargeMapMarkerRect.center.y);
                foreach (var previous in guildQuestMapRects.Values)
                    if (new Rect(p - size * .5f, size).Overlaps(previous)) p += Vector2.right * 33f * s;
                p.x = Mathf.Clamp(p.x, viewport.xMin + 19f * s, viewport.xMax - 19f * s);
                p.y = Mathf.Clamp(p.y, viewport.yMin + 22f * s, viewport.yMax - 22f * s);
                rect = new Rect(p - size * .5f, size);
                if ((p - anchor).sqrMagnitude > 4f)
                    DrawGlyphLine(anchor - viewport.position, p - viewport.position, s, QuestPaper);
                DrawQuestPaper(new Rect(rect.position - viewport.position, rect.size), returnToGuild,
                    contract.Quest.id == binding.TrackedGuildQuestId);
                guildQuestMapRects[contract.Quest.id] = rect;
            }
        }

        // Runs outside the viewport group, before ordinary pan handling. A drag keeps its normal pan ownership.
        private void HandleGuildMapPointer()
        {
            var e = Event.current;
            if (MapTouchOwnsPointer) { pressedMapQuest = -1; return; }
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                pressedMapQuest = -1;
                draggedMapQuest = false;
                pressedMapQuestPosition = e.mousePosition;
                foreach (var entry in guildQuestMapRects)
                    if (Inset(entry.Value, -4f * UiScale).Contains(e.mousePosition))
                    { pressedMapQuest = entry.Key; break; }
            }
            if (pressedMapQuest < 0) return;
            if (e.type == EventType.MouseDrag && (e.mousePosition - pressedMapQuestPosition).sqrMagnitude > Mathf.Pow(6f * UiScale, 2f))
                draggedMapQuest = true;
            if (e.type != EventType.MouseUp || e.button != 0) return;
            var id = pressedMapQuest;
            pressedMapQuest = -1;
            if (!draggedMapQuest && (e.mousePosition - pressedMapQuestPosition).sqrMagnitude <= Mathf.Pow(6f * UiScale, 2f) &&
                Inset(GuildQuestMapMarkerRect(id), -4f * UiScale).Contains(e.mousePosition))
                GuildMapBinding?.TrackGuildQuest(id);
        }

        private void DrawGuildMinimapMarkers(Rect inner, Rect uv)
        {
            var source = GuildMapSource;
            if (source == null || scene?.MapDefinition == null) return;
            var enemies = source.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.Alive) continue;
                var p = GuildMinimapPoint(enemy.Position, inner, uv);
                if ((p - inner.center).sqrMagnitude > inner.width * inner.width * .235f) continue;
                DrawWorldMapMarker(p, Stage2MinimapMarkerKind.Enemy, UiScale * .65f);
            }
        }

        private Vector2 GuildMinimapPoint(Vector2 world, Rect inner, Rect uv)
        {
            var map = scene.MapDefinition;
            var cell = map.WorldToCellCoordinates(world) - (Vector2)map.MinimumCell + Vector2.one * .5f;
            var unit = new Vector2((cell.x / map.Size.x - uv.x) / uv.width, (cell.y / map.Size.y - uv.y) / uv.height);
            return new Vector2(inner.x + unit.x * inner.width, inner.yMax - unit.y * inner.height);
        }

        private void DrawGuildQuestMinimap(Rect inner, Rect uv)
        {
            var binding = GuildMapBinding;
            if (scene?.MapDefinition == null || binding == null || !binding.TryGetTrackedGuildTarget(out var target, out var turnIn)) return;
            var s = UiScale;
            IsGuildQuestTurnIn = turnIn;
            var p = GuildMinimapPoint(target, inner, uv);
            var delta = p - inner.center;
            var direction = delta.sqrMagnitude > .001f ? delta.normalized : Vector2.up;
            GuildQuestMinimapDirection = delta.sqrMagnitude > .001f ? direction : Vector2.zero;
            var radius = inner.width * .5f - 19f * s;
            IsGuildQuestMinimapOffscreen = delta.magnitude > radius;
            if (IsGuildQuestMinimapOffscreen)
            {
                // Keep the bearing inside the compass frame, including its north caption.
                var tip = inner.center + direction * (radius - 1f * s);
                var tail = tip - direction * 18f * s;
                DrawQuestDirection(tail, tip, s * .85f);
                GuildQuestMinimapArrowRect = QuestArrowBounds(tail, tip, 6f * s);
                return;
            }
            if (delta.magnitude < 22f * s)
            {
                var anchor = p;
                p = inner.center + direction * 23f * s;
                DrawGlyphLine(anchor, p, s, QuestPaper);
            }
            var size = new Vector2(19f, 24f) * s;
            var rect = new Rect(p - size * .5f, size);
            if (rect.Overlaps(CityMinimapMarkerRect))
            {
                var anchor = p;
                var tangent = new Vector2(-direction.y, direction.x);
                p += tangent * 24f * s;
                var offset = p - inner.center;
                if (offset.magnitude > radius) p = inner.center + offset.normalized * radius;
                DrawGlyphLine(anchor, p, s, QuestPaper);
                rect = new Rect(p - size * .5f, size);
            }
            DrawQuestPaper(rect, turnIn, true);
            GuildQuestMinimapMarkerRect = rect;
        }

        private void DrawQuestPaper(Rect rect, bool turnIn, bool selected)
        {
            var u = rect.width / 27f;
            var c = rect.center;
            // Native parchment folds echo the approved scroll pin without adding a new item asset.
            Fill(new Rect(rect.x + 1.5f * u, rect.y + 2f * u, rect.width, rect.height), new Color(.15f, .10f, .12f, .75f));
            Fill(rect, selected ? QuestIvory : GuildInk);
            Fill(Inset(rect, 1.3f * u), QuestPaper);
            Fill(new Rect(rect.x + 3f * u, rect.y + 3f * u, rect.width - 6f * u, rect.height - 6f * u), new Color(.98f, .89f, .68f));
            DrawGlyphLine(new Vector2(rect.x - 2f * u, rect.y + 2f * u), new Vector2(rect.xMax + 2f * u, rect.y + 2f * u), 3f * u, GuildInk);
            DrawGlyphLine(new Vector2(rect.x - 1f * u, rect.y + 1f * u), new Vector2(rect.xMax + 1f * u, rect.y + 1f * u), 1.6f * u, QuestIvory);
            DrawGlyphLine(new Vector2(rect.x + 1f * u, rect.yMax - 2f * u), new Vector2(rect.xMax + 2f * u, rect.yMax - 2f * u), 2.4f * u, GuildInk);
            if (turnIn)
            {
                DrawGlyphLine(c + new Vector2(-7f, 0f) * u, c + new Vector2(-2f, 5f) * u, 2.5f * u, GuildInk);
                DrawGlyphLine(c + new Vector2(-2f, 5f) * u, c + new Vector2(7f, -6f) * u, 2.5f * u, GuildInk);
            }
            else
            {
                // Tiny camp silhouette, with a single open tent entrance.
                DrawGlyphLine(c + new Vector2(-8f, 6f) * u, c + new Vector2(0f, -7f) * u, 1.7f * u, GuildInk);
                DrawGlyphLine(c + new Vector2(0f, -7f) * u, c + new Vector2(8f, 6f) * u, 1.7f * u, GuildInk);
                DrawGlyphLine(c + new Vector2(-8f, 6f) * u, c + new Vector2(8f, 6f) * u, 1.7f * u, GuildInk);
                DrawGlyphLine(c + new Vector2(0f, -3f) * u, c + new Vector2(0f, 6f) * u, 2.2f * u, GuildInk);
            }
        }

        private void DrawQuestDirection(Vector2 start, Vector2 tip, float scale)
        {
            var direction = (tip - start).normalized;
            var side = new Vector2(-direction.y, direction.x);
            var left = tip - direction * 8f * scale + side * 4f * scale;
            var right = tip - direction * 8f * scale - side * 4f * scale;
            DrawGlyphLine(start, tip, 3.4f * scale, PlumInk);
            DrawGlyphLine(left, tip, 3.4f * scale, PlumInk);
            DrawGlyphLine(right, tip, 3.4f * scale, PlumInk);
            DrawGlyphLine(start, tip, 1.8f * scale, QuestIvory);
            DrawGlyphLine(left, tip, 1.8f * scale, QuestIvory);
            DrawGlyphLine(right, tip, 1.8f * scale, QuestIvory);
        }
        private static Rect QuestArrowBounds(Vector2 a, Vector2 b, float pad) => Rect.MinMaxRect(
            Mathf.Min(a.x, b.x) - pad, Mathf.Min(a.y, b.y) - pad, Mathf.Max(a.x, b.x) + pad, Mathf.Max(a.y, b.y) + pad);
    }
}
