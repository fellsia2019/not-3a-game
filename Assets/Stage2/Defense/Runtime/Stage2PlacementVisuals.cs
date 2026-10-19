using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage2.Defense
{
    /// <summary>Original vector ground marks and isolated transparent construction materials.</summary>
    public static class Stage2PlacementVisuals
    {
        public static readonly Color Valid = new Color(.12f, 1f, .23f, .52f);
        public static readonly Color Invalid = new Color(1f, .10f, .10f, .52f);
        private static readonly Color Ink = new Color(.96f, .88f, .65f, .92f);
        private static readonly Color Shadow = new Color(.12f, .16f, .12f, .85f);
        public static Material Material(bool sprite)
        {
            var material = new Material(Shader.Find("Not3A/Placement Preview"));
            material.SetFloat("_Sprite", sprite ? 1 : 0);
            material.SetFloat("_PreserveRGB", sprite ? 0 : 1);
            return material;
        }

        public static Stage2PlacementMark Outline(Transform parent, Vector2 size, Material material, string name)
            => Mark(parent, size, material, name);
        public static Stage2PlacementMark Pad(Transform parent, ResourceProductionPoint point, Material material)
        {
            var result = PadMesh(parent, point.Clearance.size, material, "Free production pad " + point.Building, point.Building);
            result.transform.position = new Vector3(point.Position.x, point.Position.y, -.25f);
            return result;
        }
        // Approved sheet v2: full reserved pad, 12% icon-layout inset on each edge,
        // then uniform world-space glyph fit. The camera alone projects all three.
        private static Stage2PlacementMark PadMesh(Transform parent, Vector2 size, Material material,
            string name, ResourceProductionBuilding kind)
        {
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var indices = new List<int>();
            var ink = new Color(.933f, .89f, .773f, .98f);
            void Quad(Vector2 a, Vector2 b, float width, Color color)
            {
                var n = new Vector2(-(b-a).y, (b-a).x).normalized * width * .5f;
                var start = vertices.Count;
                foreach(var p in new[]{a-n,b-n,b+n,a+n}) { vertices.Add(p); colors.Add(color); }
                indices.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
            }
            void Edge(Vector2 a, Vector2 b, float width, Color color)
            {
                // A restrained earth keyline separates pale ink from painted clearings.
                Quad(a,b,width+.018f,Shadow); Quad(a,b,width,color);
            }
            void Rectangle(Vector2 half)
            {
                var corners = new[]{new Vector2(-half.x,-half.y),new Vector2(half.x,-half.y),
                    new Vector2(half.x,half.y),new Vector2(-half.x,half.y)};
                for(var i=0;i<4;i++)
                {
                    var a=corners[i]; var b=corners[(i+1)%4];
                    Edge(a,b,.018f,ink);
                }
            }
            // Keep the complete outer stroke inside the immutable gameplay reservation.
            Rectangle(size*.5f-Vector2.one*.018f);
            // The 12% icon inset remains layout-only; no inner border is drawn.
            float[] source; int[] triangles;
            switch(kind)
            {
                case ResourceProductionBuilding.Sawmill:
                    source=Stage2ApprovedPadGeometry.SawmillVertices; triangles=Stage2ApprovedPadGeometry.SawmillTriangles; break;
                case ResourceProductionBuilding.Quarry:
                    source=Stage2ApprovedPadGeometry.QuarryVertices; triangles=Stage2ApprovedPadGeometry.QuarryTriangles; break;
                default:
                    source=Stage2ApprovedPadGeometry.IronMineVertices; triangles=Stage2ApprovedPadGeometry.IronMineTriangles; break;
            }
            var side=Mathf.Min(size.x,size.y)*.76f*.9f;
            var offset=vertices.Count;
            for(var i=0;i<source.Length;i+=2)
            {
                // SVG Y points down; gameplay +Y points up. No pad-aspect stretching.
                vertices.Add(new Vector3((source[i]/256f-.5f)*side,(.5f-source[i+1]/256f)*side,0));
                colors.Add(ink);
            }
            foreach(var index in triangles) indices.Add(offset+index);
            return Create(parent,material,name,vertices,colors,indices);
        }
        private static Stage2PlacementMark Mark(Transform parent, Vector2 size, Material material, string name)
        {
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var indices = new List<int>();
            void Polygon(Color color, params Vector2[] points)
            {
                var start = vertices.Count;
                foreach (var p in points) { vertices.Add(new Vector3(p.x * size.x, p.y * size.y, 0)); colors.Add(color); }
                for (var i = 1; i < points.Length - 1; i++) { indices.Add(start); indices.Add(start + i); indices.Add(start + i + 1); }
            }
            void Stroke(Vector2 a, Vector2 b, float width, Color color)
            {
                var n = new Vector2(-(b-a).y, (b-a).x).normalized * width * .5f;
                Polygon(color, a-n, b-n, b+n, a+n);
            }
            void Line(Vector2 a, Vector2 b, float width = .055f)
            { Stroke(a,b,width+.035f,Shadow); Stroke(a,b,width,Ink); }
            // Four separate corner brackets preserve the painted clearing beneath the glyph.
            foreach(var x in new[]{-1f,1f}) foreach(var y in new[]{-1f,1f})
            {
                Line(new Vector2(x*.48f,y*.27f),new Vector2(x*.48f,y*.48f),.035f);
                Line(new Vector2(x*.48f,y*.48f),new Vector2(x*.27f,y*.48f),.035f);
            }
            return Create(parent,material,name,vertices,colors,indices);
        }
        private static Stage2PlacementMark Create(Transform parent, Material material, string name,
            List<Vector3> vertices, List<Color> colors, List<int> indices)
        {
            var instance = new GameObject(name); instance.transform.SetParent(parent,false);
            var mesh = new Mesh { name = name + " vector mesh" };
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(indices,0); mesh.RecalculateBounds();
            var filter = instance.AddComponent<MeshFilter>(); filter.sharedMesh = mesh;
            var renderer = instance.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false; renderer.sortingOrder=29999;
            var mark = instance.AddComponent<Stage2PlacementMark>(); mark.Bind(mesh,renderer);
            return mark;
        }
    }
    public sealed class Stage2PlacementMark : MonoBehaviour
    {
        private Mesh mesh;
        private MeshRenderer body;
        private MaterialPropertyBlock tint;
        public void Bind(Mesh ownedMesh, MeshRenderer renderer) { mesh=ownedMesh; body=renderer; tint=new MaterialPropertyBlock(); }
        public void SetTint(Color color) { tint.SetColor("_Color",color); body.SetPropertyBlock(tint); }
        private void OnDestroy() { if(mesh!=null) Destroy(mesh); }
    }
}
