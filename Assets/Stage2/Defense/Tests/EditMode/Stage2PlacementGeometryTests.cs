using NUnit.Framework;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Defense.Tests
{
    public sealed class Stage2PlacementGeometryTests
    {
        [TestCase(MapResourceKind.Wood, 2.25f, 1.5f)]
        [TestCase(MapResourceKind.Stone, 2.325f, 1.575f)]
        [TestCase(MapResourceKind.Metal, 1.5f, 1.125f)]
        public void ApprovedVectorHasActualTransparentCutsAndStaysInsideReservedPad(MapResourceKind kind, float w, float h)
        {
            var parent = new GameObject("Pad geometry fixture");
            var material = Stage2PlacementVisuals.Material(false);
            var point = new ResourceProductionPoint(0,kind,new Vector2(23,41),new Vector2(w,h),0);
            var mark = Stage2PlacementVisuals.Pad(parent.transform,point,material);
            var mesh = mark.GetComponent<MeshFilter>().sharedMesh;
            try
            {
                Assert.That((Vector2)mark.transform.position, Is.EqualTo(point.Position));
                Assert.That(mark.transform.rotation, Is.EqualTo(Quaternion.identity));
                Assert.That(mesh.bounds.min.x, Is.GreaterThanOrEqualTo(-w*.5f-.0001f));
                Assert.That(mesh.bounds.max.x, Is.LessThanOrEqualTo(w*.5f+.0001f));
                Assert.That(mesh.bounds.min.y, Is.GreaterThanOrEqualTo(-h*.5f-.0001f));
                Assert.That(mesh.bounds.max.y, Is.LessThanOrEqualTo(h*.5f+.0001f));
                Assert.That(mark.GetComponents<Collider>(), Is.Empty);
                // These positions come from the approved SVG, independently of generated triangles.
                // Actual absent triangles are required: painting an earth-colored cut is insufficient.
                var eye = Rotate(new Vector2(130,kind==MapResourceKind.Wood ? 45 : 41));
                Assert.That(Covers(mesh,ToWorld(eye,w,h)), Is.False,"Tool eye must be a hole.");
                Assert.That(Covers(mesh,ToWorld(Rotate(new Vector2(130,100)),w,h)), Is.True,"Straight handle must remain solid.");
                Assert.That(Covers(mesh,ToWorld(new Vector2(120,157),w,h)), Is.False,"Tool/resource gap must stay open.");
                if(kind==MapResourceKind.Wood)
                {
                    Assert.That(Covers(mesh,ToWorld(new Vector2(78,208),w,h)), Is.False,"Log end-grain ring is transparent.");
                    Assert.That(Covers(mesh,ToWorld(new Vector2(64,208),w,h)), Is.True,"Ring center retains ink.");
                }
                else if(kind==MapResourceKind.Stone)
                {
                    Assert.That(Covers(mesh,ToWorld(new Vector2(108,202),w,h)), Is.False,"Stone facets are transparent cuts.");
                    Assert.That(Covers(mesh,ToWorld(new Vector2(194,225),w,h)), Is.True,"Separate stone chip remains present.");
                }
                else
                {
                    Assert.That(Covers(mesh,ToWorld(new Vector2(94,209),w,h)), Is.False,"Bar face divisions are transparent.");
                    Assert.That(Covers(mesh,ToWorld(new Vector2(130,190),w,h)), Is.True,"Bar top face remains solid.");
                }
                // The complete glyph artboard, with uniform XY scale, has visible safe margins.
                var halfSide=Mathf.Min(w,h)*.76f*.9f*.5f;
                Assert.That(halfSide,Is.LessThan(w*.38f)); Assert.That(halfSide,Is.LessThan(h*.38f));
            }
            finally
            {
                Object.DestroyImmediate(mesh); Object.DestroyImmediate(parent); Object.DestroyImmediate(material);
            }
        }
        private static Vector2 Rotate(Vector2 p)
        {
            var a=28*Mathf.Deg2Rad; var d=p-new Vector2(130,50);
            return new Vector2(130,50)+new Vector2(d.x*Mathf.Cos(a)-d.y*Mathf.Sin(a),d.x*Mathf.Sin(a)+d.y*Mathf.Cos(a));
        }
        private static Vector2 ToWorld(Vector2 svg,float w,float h)
            => new Vector2(svg.x/256f-.5f,.5f-svg.y/256f)*(Mathf.Min(w,h)*.76f*.9f);
        private static bool Covers(Mesh mesh, Vector2 p)
        {
            var vertices=mesh.vertices; var triangles=mesh.triangles;
            float Side(Vector2 a,Vector2 b) => (b.x-a.x)*(p.y-a.y)-(b.y-a.y)*(p.x-a.x);
            for(var i=0;i<triangles.Length;i+=3)
            {
                var a=Side(vertices[triangles[i]],vertices[triangles[i+1]]);
                var b=Side(vertices[triangles[i+1]],vertices[triangles[i+2]]);
                var c=Side(vertices[triangles[i+2]],vertices[triangles[i]]);
                if((a>=-1e-9f&&b>=-1e-9f&&c>=-1e-9f)||(a<=1e-9f&&b<=1e-9f&&c<=1e-9f))return true;
            }
            return false;
        }
    }
}
