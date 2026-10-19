using System.Collections.Generic;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>One visual hint for the gathering driver's exact E target.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class Stage3ResourceInteractionMarker : MonoBehaviour
    {
        private readonly Dictionary<Stage2ResourceNode, Stage3ResourceSprite> bindings = new();
        private Camera viewCamera;
        private Stage2GatheringDriver gathering;
        private MeshRenderer marker;
        private MeshFilter filter;
        private Mesh axe, pick;
        private Material material;

        public Stage2ResourceNode Target { get; private set; }
        public bool IsVisible => marker != null && marker.enabled;
        public bool ToolIsAxe => Target != null && Target.Resource == MapResourceKind.Wood;
        public Rect ScreenRect { get; private set; }

        public void Bind(Camera camera, Stage2GatheringDriver driver)
        {
            viewCamera = camera;
            gathering = driver;
            EnsureVisual();
            Hide();
        }

        public void Register(Stage3ResourceSprite sprite)
        {
            if (sprite != null && sprite.Node != null) bindings[sprite.Node] = sprite;
        }

        public void ClearBindings()
        {
            bindings.Clear();
            Hide();
        }

        private void LateUpdate()
        {
            var target = gathering != null ? gathering.InteractionTarget : null;
            if (viewCamera == null || target == null || target.IsDepleted ||
                !bindings.TryGetValue(target, out var sprite) || sprite == null ||
                sprite.Body == null || !sprite.Body.enabled || sprite.Body.sprite == null)
            { Hide(); return; }

            EnsureVisual();
            var screen = viewCamera.WorldToScreenPoint(sprite.VisibleTopWorld);
            var viewport = viewCamera.pixelRect;
            if (screen.z <= 0 || screen.x < viewport.xMin || screen.x > viewport.xMax ||
                screen.y < viewport.yMin)
            { Hide(); return; }

            // Fixed screen footprint makes the same target legible at every authored zoom.
            var diameter = Mathf.Clamp(viewport.height * .04f, 32f, 44f);
            screen.y = Mathf.Min(screen.y + diameter * .5f + 8f, viewport.yMax - diameter * .5f - 8f);
            screen.x = Mathf.Clamp(screen.x, viewport.xMin + diameter * .5f + 8f,
                viewport.xMax - diameter * .5f - 8f);
            var position = viewCamera.ScreenToWorldPoint(screen);
            var right = viewCamera.ScreenToWorldPoint(screen + new Vector3(diameter, 0, 0));
            marker.transform.SetPositionAndRotation(position - viewCamera.transform.forward * .08f,
                viewCamera.transform.rotation);
            marker.transform.localScale = Vector3.one * Vector3.Distance(position, right);
            Target = target;
            filter.sharedMesh = ToolIsAxe ? axe : pick;
            marker.enabled = true;
            ScreenRect = new Rect(screen.x - diameter * .5f, screen.y - diameter * .5f, diameter, diameter);
        }

        private void EnsureVisual()
        {
            if (marker != null) return;
            var visual = new GameObject("Current harvest tool");
            visual.transform.SetParent(transform, false);
            filter = visual.AddComponent<MeshFilter>();
            marker = visual.AddComponent<MeshRenderer>();
            marker.shadowCastingMode = ShadowCastingMode.Off;
            marker.receiveShadows = false;
            marker.sortingOrder = 32760;
            material = Stage2PlacementVisuals.Material(false);
            material.name = "Harvest tool pearl and plum";
            marker.sharedMaterial = material;
            axe = MakeMesh(Stage3ResourceInteractionGlyphs.AxeVertices, Stage3ResourceInteractionGlyphs.AxeTriangles, "Approved axe hint");
            pick = MakeMesh(Stage3ResourceInteractionGlyphs.PickVertices, Stage3ResourceInteractionGlyphs.PickTriangles, "Approved pick hint");
            marker.enabled = false;
        }

        private static Mesh MakeMesh(float[] glyph, int[] triangles, string name)
        {
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var indices = new List<int>();
            Color Pigment(float r, float g, float b)
            {
                var color = new Color(r, g, b, 1f);
                return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
            }
            void Disc(float radius, Color color)
            {
                const int segments = 64;
                var start = vertices.Count;
                vertices.Add(Vector3.zero); colors.Add(color);
                for (var i = 0; i <= segments; i++)
                {
                    var angle = i * Mathf.PI * 2f / segments;
                    vertices.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
                    colors.Add(color);
                    if (i > 0) { indices.Add(start); indices.Add(start + i); indices.Add(start + i + 1); }
                }
            }
            Disc(.5f, Pigment(.45f, .73f, .70f));
            Disc(.46f, Pigment(.30f, .247f, .357f));
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (var i = 0; i < glyph.Length; i += 2)
            {
                var point = new Vector2(glyph[i], glyph[i + 1]);
                min = Vector2.Min(min, point); max = Vector2.Max(max, point);
            }
            var center = (min + max) * .5f;
            var scale = .69f / Mathf.Max(max.x - min.x, max.y - min.y);
            var offset = vertices.Count;
            for (var i = 0; i < glyph.Length; i += 2)
            {
                vertices.Add(new Vector3((glyph[i] - center.x) * scale, -(glyph[i + 1] - center.y) * scale, 0));
                colors.Add(Pigment(.933f, .89f, .773f));
            }
            foreach (var index in triangles) indices.Add(offset + index);
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private void Hide()
        {
            Target = null;
            ScreenRect = default;
            if (marker != null) marker.enabled = false;
        }
        private void OnDisable() => Hide();
        private void OnDestroy()
        {
            if (axe != null) Destroy(axe);
            if (pick != null) Destroy(pick);
            if (material != null) Destroy(material);
            if (marker != null) Destroy(marker.gameObject);
        }
    }
}
