using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.WavesAI;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Read-only, bounded feedback for confirmed damage and manual material grants.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(250)]
    public sealed class Stage3FieldFeedbackPresentation : MonoBehaviour
    {
        public const int MaximumFloatingCount = 24;
        public const float FloatingLifetime = .85f;
        private sealed class Feedback
        {
            public object Source;
            public Vector3 Position;
            public long Amount;
            public float Age;
            public int Material = -1;
            public string Label;
            public SpriteRenderer Body;
        }
        private readonly List<Feedback> feedback = new List<Feedback>();
        private readonly List<Vector2> healthCenters = new List<Vector2>();
        private readonly List<float> healthFractions = new List<float>();
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage3FantasyHud hud;
        private Stage2GameHud legacy;
        private Stage3GuildWorldDriver guild;
        private Camera view;
        private Texture2D capsule, glow;
        private GUIStyle numberStyle;
        private Font fallbackFont;
        private bool bound;
        public int ActiveFloatingCount => feedback.Count;
        public int VisibleHealthBars { get; private set; }
        public int VisibleManaBars => 0; // Neither current enemy family has a mana source.
        public long ConfirmedDamageTotal { get; private set; }
        public long ConfirmedGatherTotal { get; private set; }
        public int LastActualAmount { get; private set; }
        public bool LastWasGather { get; private set; }
        public float OldestFeedbackAge => feedback.Count == 0 ? 0 : feedback[0].Age;
        public Rect LastHealthBarRect { get; private set; }
        public Rect LastFloatingRect { get; private set; }

        public void Initialize(Stage2GameComposition owner)
        {
            Release();
            game = owner;
            if (game == null || game.Enemies == null || game.Gathering?.Economy == null) return;
            scene = game.GetComponent<Stage2SceneBootstrap>();
            view = scene != null ? scene.CameraFollow.BoundCamera : null;
            hud = game.GetComponent<Stage3FantasyHud>();
            legacy = game.GetComponent<Stage2GameHud>();
            game.Enemies.EnemyDamaged += PortalDamaged;
            game.Gathering.Economy.ResourceGranted += ResourceGranted;
            game.RunDriver.Run.Reset += ClearFeedback;
            game.RunDriver.Run.Rewound += ClearFeedback;
            game.Enemies.ShotsCleared += ClearFeedback;
            bound = true;
            ObserveGuild();
            var review = Environment.GetEnvironmentVariable("NOT3A_FIELD_FEEDBACK_REVIEW");
            if ((Debug.isDebugBuild || Application.isEditor) && !string.IsNullOrWhiteSpace(review))
                StartCoroutine(Stage3FieldFeedbackReview.Capture(gameObject, review));
        }

        private void ObserveGuild()
        {
            if (guild != null || game == null) return;
            guild = game.GetComponent<Stage3GuildWorldDriver>();
            if (guild != null) guild.Damaged += GuildDamaged;
        }
        private bool Hidden => hud != null && hud.IsModalOpen || legacy != null && legacy.DebugOpen;
        private bool Frozen => game?.RunDriver?.Run == null || game.RunDriver.Run.IsPaused || Hidden;
        private bool CanReceive => isActiveAndEnabled && bound && !Hidden && !game.RunDriver.Run.IsTerminal;
        private float Scale => hud != null ? hud.UiScale : Mathf.Clamp(Screen.height / 900f, .55f, 2f);

        private void PortalDamaged(Stage2Enemy enemy, int amount)
        {
            if (enemy == null || amount <= 0) return;
            ConfirmedDamageTotal += amount;
            var sprite = enemy.GetComponentInChildren<Stage3SkeletonSprite>()?.Body;
            Add(enemy, sprite != null ? sprite.bounds.center : Ground(enemy.transform.position) + Vector3.back * .65f, amount, -1, sprite);
        }
        private void GuildDamaged(Stage2GuildEnemy enemy, int amount)
        {
            if (enemy == null || amount <= 0) return;
            ConfirmedDamageTotal += amount;
            var position = Ground(enemy.Position) + Vector3.back * .65f;
            SpriteRenderer body = null;
            if (guild != null && guild.TryGetVisual(enemy, out var visual) && visual != null && visual.Body != null)
            { body = visual.Body; position = body.bounds.center; }
            Add(enemy, position, amount, -1, body);
        }
        private void ResourceGranted(Stage2ResourceNode node, int amount)
        {
            if (node == null || amount <= 0) return;
            ConfirmedGatherTotal += amount;
            // Work takes place at the near edge, so a large crown never hides the reward.
            var toward = scene != null ? (Vector2)scene.PlayerMover.transform.position - node.WorldPosition : Vector2.down;
            var contact = node.WorldPosition + toward.normalized * Mathf.Min(node.InteractionRadius, toward.magnitude);
            Add(node, Ground(contact) + Vector3.back * .45f, amount, (int)node.Resource);
        }
        private static Vector3 Ground(Vector2 p) => new Vector3(p.x, p.y, Stage3TacticalPresentation.GrassSurfaceZ(p));
        private void Add(object source, Vector3 position, int amount, int material, SpriteRenderer body = null)
        {
            if (!CanReceive) return;
            LastActualAmount = amount; LastWasGather = material >= 0;
            // Combine only rapid hits on the same confirmed target; never combine different enemies.
            for (var i = feedback.Count - 1; i >= 0; i--)
            {
                var existing = feedback[i];
                if (!ReferenceEquals(existing.Source, source) || existing.Material != material || existing.Age > .12f) continue;
                existing.Amount += amount; existing.Position = position;
                existing.Label = (material >= 0 ? "+" : "") + existing.Amount;
                return;
            }
            if (feedback.Count == MaximumFloatingCount) feedback.RemoveAt(0);
            feedback.Add(new Feedback { Source = source, Position = position, Amount = amount, Material = material, Body = body,
                Label = (material >= 0 ? "+" : "") + amount });
        }
        private void LateUpdate()
        {
            if (!bound) return;
            ObserveGuild();
            if (game.RunDriver.Run.IsTerminal) { ClearFeedback(); return; }
            if (Frozen) return;
            for (var i = feedback.Count - 1; i >= 0; i--)
            {
                feedback[i].Age += Time.deltaTime;
                if (feedback[i].Age >= FloatingLifetime) feedback.RemoveAt(i);
            }
        }
        public void ClearFeedback()
        {
            feedback.Clear(); VisibleHealthBars = 0; LastHealthBarRect = LastFloatingRect = default;
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || !bound || view == null || Hidden || game.RunDriver.Run.IsTerminal) return;
            EnsureArt();
            var oldDepth = GUI.depth; var oldColor = GUI.color; var oldMatrix = GUI.matrix;
            GUI.depth = 0; GUI.matrix = Matrix4x4.identity; GUI.color = Color.white;
            try
            {
                VisibleHealthBars = 0; LastHealthBarRect = LastFloatingRect = default;
                healthCenters.Clear(); healthFractions.Clear();
                var s = Scale;
                numberStyle.fontSize = Mathf.Max(16, Mathf.RoundToInt(22 * s));
                if (hud?.Art?.font != null) numberStyle.font = hud.Art.font;
                for (var i = 0; i < game.Enemies.ActiveCount; i++)
                {
                    var enemy = game.Enemies.GetEnemy(i);
                    if (enemy == null || !enemy.IsAlive) continue;
                    var body = enemy.GetComponentInChildren<Stage3SkeletonSprite>()?.Body;
                    CollectHealth(body, Ground(enemy.transform.position) + Vector3.back * 1.3f, enemy.Health, enemy.MaximumHealth, s);
                }
                if (guild != null) foreach (var enemy in guild.Enemies)
                {
                    if (!enemy.Alive) continue;
                    SpriteRenderer body = null;
                    if (guild.TryGetVisual(enemy, out var visual) && visual != null) body = visual.Body;
                    CollectHealth(body, Ground(enemy.Position) + Vector3.back * 1.5f, enemy.Health, enemy.Profile.health, s);
                }
                var bars = LayoutHealthBars(healthCenters, healthFractions, s);
                for (var i = 0; i < bars.Length; i++)
                    if (bars[i].width > 0 && IsClearOfHud(bars[i])) HealthBar(bars[i], healthFractions[i], s);
                foreach (var item in feedback) DrawFeedback(item, s);
            }
            finally { GUI.depth = oldDepth; GUI.color = oldColor; GUI.matrix = oldMatrix; }
        }
        private bool Project(Vector3 world, out Vector2 pixel)
        {
            var p = view.WorldToScreenPoint(world);
            pixel = new Vector2(p.x, Screen.height - p.y);
            return p.z > 0 && p.x >= 0 && p.x <= Screen.width && p.y >= 0 && p.y <= Screen.height;
        }
        public bool IsClearOfHud(Rect rect)
        {
            if (rect.xMin < 5 || rect.yMin < 5 || rect.xMax > Screen.width - 5 || rect.yMax > Screen.height - 5) return false;
            return hud == null || !rect.Overlaps(hud.WorldHudRect) && !rect.Overlaps(hud.ResourcesRect) &&
                !rect.Overlaps(hud.StatusRect) && !rect.Overlaps(hud.CycleDialRect) && !rect.Overlaps(hud.CyclePhaseRect);
        }
        private void CollectHealth(SpriteRenderer body, Vector3 fallback, int health, int maximum, float s)
        {
            if (maximum <= 0 || health <= 0 || body != null && !body.enabled) return;
            var top = body != null && body.sprite != null
                ? body.transform.TransformPoint(new Vector3(body.sprite.bounds.center.x, body.sprite.bounds.max.y, 0)) : fallback;
            if (!Project(top, out var p)) return;
            var center = new Vector2(p.x, p.y - 7.5f * s);
            if (!IsClearOfHud(new Rect(center.x - 12 * s, center.y - 3.5f * s, 24 * s, 7 * s))) return;
            healthCenters.Add(center); healthFractions.Add(Mathf.Clamp01((float)health / maximum));
        }
        private void HealthBar(Rect r, float fraction, float s)
        {
            VisibleHealthBars++; LastHealthBarRect = r;
            TintTexture(r, capsule, new Color(.16f, .09f, .15f, .96f));
            var inner = new Rect(r.x + 1.4f * s, r.y + 1.4f * s, (r.width - 2.8f * s) * fraction, r.height - 2.8f * s);
            if (inner.width > 0)
            {
                TintTexture(inner, capsule, new Color(.82f, .18f, .25f));
                TintTexture(new Rect(inner.x + s, inner.y, Mathf.Max(0, inner.width - 2 * s), 1.2f * s), capsule,
                    new Color(1f, .52f, .49f, .75f));
            }
        }

        /// <summary>Readable screen-space health: bounded neighbour scan and non-overlapping priority placement.</summary>
        public static Rect[] LayoutHealthBars(IReadOnlyList<Vector2> centers, IReadOnlyList<float> fractions, float scale)
        {
            if (centers == null || fractions == null || centers.Count != fractions.Count || !float.IsFinite(scale) || scale <= 0)
                throw new ArgumentException("Valid matching health points and positive scale are required.");
            var count = centers.Count;
            var result = new Rect[count]; var widths = new float[count];
            var order = new int[count];
            for (var i = 0; i < count; i++) { order[i] = i; widths[i] = 50 * scale; }
            Array.Sort(order, (a, b) => { var x = centers[a].x.CompareTo(centers[b].x); return x != 0 ? x : a.CompareTo(b); });
            // At most 24 nearby candidates per point, independent of the complete horde population.
            for (var slot = 0; slot < count; slot++)
            {
                var index = order[slot];
                for (var neighbour = Mathf.Max(0, slot - 12); neighbour <= Mathf.Min(count - 1, slot + 12); neighbour++)
                {
                    if (neighbour == slot) continue;
                    var other = centers[order[neighbour]];
                    if (Mathf.Abs(other.y - centers[index].y) >= 9 * scale) continue;
                    widths[index] = Mathf.Min(widths[index], Mathf.Max(24 * scale, Mathf.Abs(other.x - centers[index].x) - 4 * scale));
                }
            }
            // Most injured first; stable source index resolves ties. The small screen grid stores
            // accepted non-overlapping bars only, so each cell's candidate count is geometrically bounded.
            Array.Sort(order, (a, b) => { var hp = fractions[a].CompareTo(fractions[b]); return hp != 0 ? hp : a.CompareTo(b); });
            var cells = new Dictionary<Vector2Int, List<Rect>>();
            foreach (var index in order)
            {
                var point = centers[index];
                var bar = new Rect(point.x - widths[index] * .5f, point.y - 3.5f * scale, widths[index], 7 * scale);
                // Tiny tolerance keeps exact four-pixel neighbours stable under UI-scale rounding.
                var padded = new Rect(bar.x - 1.9995f * scale, bar.y - .9995f * scale,
                    bar.width + 3.999f * scale, bar.height + 1.999f * scale);
                var key = new Vector2Int(Mathf.FloorToInt(point.x / (54 * scale)), Mathf.FloorToInt(point.y / (11 * scale)));
                var occupied = false;
                for (var y = -1; y <= 1 && !occupied; y++) for (var x = -1; x <= 1 && !occupied; x++)
                    if (cells.TryGetValue(key + new Vector2Int(x, y), out var nearby))
                        foreach (var previous in nearby) if (previous.Overlaps(padded)) { occupied = true; break; }
                if (occupied) continue;
                result[index] = bar;
                if (!cells.TryGetValue(key, out var accepted)) cells.Add(key, accepted = new List<Rect>());
                accepted.Add(padded);
            }
            return result;
        }
        private void DrawFeedback(Feedback item, float s)
        {
            if (!Project(item.Position, out var anchor)) return;
            var t = item.Age / FloatingLifetime;
            var alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1, t));
            var offset = new Vector2(18 * s, -(24 + t * 32) * s);
            var content = new GUIContent(item.Label);
            var width = numberStyle.CalcSize(content).x;
            var r = new Rect(anchor.x + offset.x, anchor.y + offset.y, width + (item.Material >= 0 ? 23 * s : 0), 30 * s);
            if (IsClearOfHud(r))
            {
                LastFloatingRect = r;
                var textRect = new Rect(r.x, r.y, width + s, r.height);
                var shadow = textRect; shadow.position += new Vector2(1.3f * s, 1.3f * s);
                numberStyle.normal.textColor = new Color(.16f, .08f, .17f, alpha);
                GUI.Label(shadow, content, numberStyle);
                numberStyle.normal.textColor = new Color(1f, .96f, .82f, alpha);
                GUI.Label(textRect, content, numberStyle);
                if (item.Material >= 0) DrawResourceIcon(new Rect(r.x + width + 3 * s, r.y + 4 * s, 21 * s, 21 * s), item.Material, alpha);
            }
            if (item.Age < .28f) DrawContact(anchor, item, s);
        }
        private void DrawContact(Vector2 p, Feedback item, float s)
        {
            if (item.Material < 0 && item.Body != null && item.Body.sprite != null)
            {
                var bounds = item.Body.sprite.bounds;
                var top = view.WorldToScreenPoint(item.Body.transform.TransformPoint(new Vector3(bounds.center.x, bounds.max.y, 0)));
                var bottom = view.WorldToScreenPoint(item.Body.transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0)));
                s *= Mathf.Clamp(Mathf.Abs(top.y - bottom.y) / (70 * s), .35f, 1f);
            }
            var t = item.Age / .28f;
            var extent = (10 + 7 * t) * s;
            if (!IsClearOfHud(new Rect(p.x - extent, p.y - extent, extent * 2, extent * 2))) return;
            var alpha = (1 - t) * .9f;
            var metal = item.Material == (int)MapResourceKind.Metal;
            if (item.Material < 0 || metal)
            {
                TintTexture(new Rect(p.x - 9 * s, p.y - 9 * s, 18 * s, 18 * s), glow, new Color(1f, .76f, .39f, alpha * .65f));
                DrawRay(p, (9 - 3 * t) * s, 0, new Color(1f, .95f, .73f, alpha));
                DrawRay(p, (9 - 3 * t) * s, 90, new Color(1f, .95f, .73f, alpha));
                DrawRay(p, (6 - 2 * t) * s, 45, new Color(1f, .86f, .58f, alpha));
                DrawRay(p, (6 - 2 * t) * s, -45, new Color(1f, .86f, .58f, alpha));
            }
            if (item.Material >= 0)
            {
                var wood = item.Material == (int)MapResourceKind.Wood;
                for (var i = 0; i < 4; i++)
                {
                    var angle = (i * 1.7f + .4f);
                    var q = p + new Vector2(Mathf.Cos(angle) * extent * t, -Mathf.Abs(Mathf.Sin(angle)) * extent * t + t * t * 8 * s);
                    TintTexture(new Rect(q.x, q.y, (wood ? 4 : 3) * s, 2 * s), capsule,
                        wood ? new Color(.83f, .61f, .35f, alpha) : new Color(.80f, .78f, .71f, alpha));
                }
                if (!wood && !metal) TintTexture(new Rect(p.x - 11 * s, p.y - 5 * s, 22 * s, 12 * s), glow, new Color(.78f, .75f, .65f, alpha * .45f));
            }
        }
        private static void DrawRay(Vector2 center, float radius, float angle, Color color)
        {
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, center);
            TintTexture(new Rect(center.x - radius, center.y - .65f, radius * 2, 1.3f), Texture2D.whiteTexture, color);
            GUI.matrix = matrix;
        }
        private void DrawResourceIcon(Rect target, int material, float alpha)
        {
            // HUD resource order is gold, wood, stone, metal; reuse these approved painted assets.
            var icons = hud?.Art?.resourceIcons;
            var index = material + 1;
            if (icons == null || index < 0 || index >= icons.Length || icons[index] == null) return;
            var sprite = icons[index]; var r = sprite.textureRect;
            var ratio = r.width / r.height;
            if (ratio > 1) { var h = target.width / ratio; target.y += (target.height - h) * .5f; target.height = h; }
            else { var w = target.height * ratio; target.x += (target.width - w) * .5f; target.width = w; }
            var color = GUI.color; GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTextureWithTexCoords(target, sprite.texture, new Rect(r.x / sprite.texture.width, r.y / sprite.texture.height,
                r.width / sprite.texture.width, r.height / sprite.texture.height));
            GUI.color = color;
        }
        private static void TintTexture(Rect r, Texture texture, Color color)
        {
            var previous = GUI.color; GUI.color = color; GUI.DrawTexture(r, texture); GUI.color = previous;
        }
        private void EnsureArt()
        {
            if (numberStyle == null)
            {
                fallbackFont = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Liberation Sans" }, 22);
                numberStyle = new GUIStyle(GUI.skin.label) { font = hud?.Art?.font != null ? hud.Art.font : fallbackFont,
                    fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(), clipping = TextClipping.Overflow };
            }
            if (capsule == null) capsule = MakeMask(false);
            if (glow == null) glow = MakeMask(true);
        }
        private static Texture2D MakeMask(bool soft)
        {
            const int size = 32;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
            {
                var p = new Vector2((x + .5f) / size * 2 - 1, (y + .5f) / size * 2 - 1);
                var d = soft ? p.magnitude : new Vector2(Mathf.Max(0, Mathf.Abs(p.x) - .65f), p.y * .35f).magnitude / .35f;
                var a = soft ? Mathf.Pow(Mathf.Clamp01(1 - d), 2) : Mathf.Clamp01((1 - d) * 20);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255));
            }
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = soft ? "Local hit glow" : "Compact health silhouette", wrapMode = TextureWrapMode.Clamp };
            result.SetPixels32(pixels); result.Apply(false, true); return result;
        }
        private void Release()
        {
            if (guild != null) guild.Damaged -= GuildDamaged;
            guild = null;
            if (bound && game != null)
            {
                game.Enemies.EnemyDamaged -= PortalDamaged;
                game.Enemies.ShotsCleared -= ClearFeedback;
                game.Gathering.Economy.ResourceGranted -= ResourceGranted;
                game.RunDriver.Run.Reset -= ClearFeedback;
                game.RunDriver.Run.Rewound -= ClearFeedback;
            }
            bound = false; ClearFeedback();
        }
        private void OnDisable() => ClearFeedback();
        private void OnDestroy()
        {
            Release();
            if (capsule != null) Destroy(capsule);
            if (glow != null) Destroy(glow);
            if (fallbackFont != null) Destroy(fallbackFont);
        }
    }
}
