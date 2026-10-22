using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Read-only local spell feedback. Gameplay calls this only after a confirmed effect.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(255)]
    public sealed class Stage3HeroSkillPresentation : MonoBehaviour
    {
        public const int MaximumImpactCount = 12;
        public const float ImpactLifetime = .38f;
        private struct Impact { public int Skill; public Vector3 Position; public float Radius, Age; }
        private readonly List<Impact> impacts = new List<Impact>();
        private Stage2GameComposition game;
        private Stage3FantasyHud hud;
        private Stage2GameHud legacy;
        private Camera view;
        private bool bound, preview, previewValid;
        private int previewSkill;
        private Vector3 previewPosition;
        private float previewRadius;
        public int ActiveImpactCount => impacts.Count;
        public bool HasTargetPreview => preview;
        public int PreviewSkillId => previewSkill;
        public float PreviewRadius => previewRadius;

        public void Initialize(Stage2GameComposition owner)
        {
            Release(); game = owner;
            if (game?.RunDriver?.Run == null) return;
            hud = game.GetComponent<Stage3FantasyHud>();
            legacy = game.GetComponent<Stage2GameHud>();
            view = game.GetComponent<Stage2SceneBootstrap>()?.CameraFollow?.BoundCamera;
            game.RunDriver.Run.Reset += ClearPresentation;
            game.RunDriver.Run.Rewound += ClearPresentation;
            game.Enemies.ShotsCleared += ClearPresentation;
            bound = true;
        }

        private bool Hidden => hud != null && hud.IsModalOpen || legacy != null && legacy.DebugOpen;
        private bool Frozen => game?.RunDriver?.Run == null || game.RunDriver.Run.IsPaused || Hidden;
        private bool CanPresent => bound && isActiveAndEnabled && !Hidden && !game.RunDriver.Run.IsTerminal;

        public void PresentSkillImpact(int skillId, Vector3 position, float radius)
        {
            if (!CanPresent || skillId < 0 || skillId > 10 || !Finite(position) || !float.IsFinite(radius)) return;
            if (impacts.Count >= MaximumImpactCount) impacts.RemoveAt(0);
            impacts.Add(new Impact { Skill = skillId, Position = position, Radius = Mathf.Max(0, radius) });
        }

        /// <summary>Radius is the actual affected world radius; an invalid cast previews a muted red boundary.</summary>
        public void SetTargetPreview(int skillId, Vector3 position, float radius, bool valid)
        {
            if (!CanPresent || skillId < 0 || skillId > 10 || !Finite(position) || !float.IsFinite(radius) || radius <= 0)
            { ClearTargetPreview(); return; }
            preview = true; previewSkill = skillId; previewPosition = position; previewRadius = radius; previewValid = valid;
        }
        public void ClearTargetPreview() { preview = false; previewRadius = 0; }
        public void ClearPresentation() { impacts.Clear(); ClearTargetPreview(); }
        private static bool Finite(Vector3 p) => float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
        private void LateUpdate()
        {
            if (!bound) return;
            if (game.RunDriver.Run.IsTerminal) { ClearPresentation(); return; }
            if (Frozen) return;
            for (var i = impacts.Count - 1; i >= 0; i--)
            {
                var effect = impacts[i]; effect.Age += Time.deltaTime;
                if (effect.Age >= ImpactLifetime) impacts.RemoveAt(i); else impacts[i] = effect;
            }
        }
        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || !CanPresent || view == null) return;
            var depth = GUI.depth; var matrix = GUI.matrix; var color = GUI.color;
            GUI.depth = 1; GUI.matrix = Matrix4x4.identity;
            try
            {
                var scale = hud != null ? hud.UiScale : Mathf.Clamp(Screen.height / 900f, .55f, 2f);
                if (preview && !Frozen)
                    Ring(previewPosition, previewRadius, previewValid ? new Color(.23f, .86f, .91f, .85f) : new Color(.84f, .25f, .28f, .75f), 1.6f * scale);
                foreach (var effect in impacts)
                {
                    if (!Project(effect.Position, out var center)) continue;
                    var fraction = Mathf.Clamp01(effect.Age / ImpactLifetime);
                    var tint = effect.Skill == 2 || effect.Skill == 9 ? new Color(1f, .65f, .25f, 1f - fraction) :
                        effect.Skill == 3 || effect.Skill == 10 ? new Color(.51f, .83f, 1f, 1f - fraction) :
                        effect.Skill == 8 ? new Color(.48f, .94f, .65f, 1f - fraction) : new Color(.86f, .77f, 1f, 1f - fraction);
                    // The same restrained local star language as confirmed field damage; no new numbers or camera motion.
                    var extent = (5f + 11f * fraction) * scale;
                    for (var ray = 0; ray < 6; ray++)
                    {
                        var angle = ray * Mathf.PI / 3f + effect.Skill * .13f;
                        var axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Line(center + axis * extent * .33f, center + axis * extent, 1.7f * scale, tint);
                    }
                    if (effect.Radius > 0 && (effect.Skill == 9 || effect.Skill == 10))
                        Ring(effect.Position, effect.Radius * (.72f + .28f * fraction), new Color(tint.r, tint.g, tint.b, tint.a * .45f), scale);
                }
            }
            finally { GUI.depth = depth; GUI.matrix = matrix; GUI.color = color; }
        }
        private void Ring(Vector3 position, float radius, Color tint, float width)
        {
            Vector2 previous = default; var havePrevious = false;
            for (var i = 0; i <= 48; i++)
            {
                var angle = i * Mathf.PI * 2f / 48f;
                var point = position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                point.z = Stage3TacticalPresentation.GrassSurfaceZ(point);
                var projected = Project(point, out var next);
                if (projected && havePrevious) Line(previous, next, width, tint);
                previous = next; havePrevious = projected;
            }
        }
        private bool Project(Vector3 world, out Vector2 point)
        {
            var screen = view.WorldToScreenPoint(world); point = new Vector2(screen.x, Screen.height - screen.y);
            if (screen.z <= 0 || point.x < 0 || point.x > Screen.width || point.y < 0 || point.y > Screen.height) return false;
            return hud == null || !hud.WorldHudRect.Contains(point) && !hud.ResourcesRect.Contains(point) &&
                !hud.StatusRect.Contains(point) && !hud.CycleDialRect.Contains(point) && !hud.CyclePhaseRect.Contains(point);
        }
        private static void Line(Vector2 a, Vector2 b, float width, Color color)
        {
            var matrix = GUI.matrix; var delta = b - a;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
            GUI.color = color; GUI.DrawTexture(new Rect(a.x, a.y - width * .5f, delta.magnitude, width), Texture2D.whiteTexture);
            GUI.matrix = matrix;
        }
        private void Release()
        {
            if (bound && game != null)
            {
                game.RunDriver.Run.Reset -= ClearPresentation;
                game.RunDriver.Run.Rewound -= ClearPresentation;
                game.Enemies.ShotsCleared -= ClearPresentation;
            }
            bound = false; ClearPresentation();
        }
        private void OnDisable() => Release();
        private void OnDestroy() => Release();
    }
}
