using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Visible relief from one structural plan, not stacks of unrelated billboard ledges.</summary>
    internal static class Stage3DistrictGeometry
    {
        internal static void Build(ResourceDistrictLayout plan, Stage3ResourceDistrictArt art, Transform root,Vector3 cameraForward)
        {
            BuildGroundBlend(plan,art,root);
            if(plan.District.Kind==MapResourceKind.Wood)return;
            var mesh=new Buffer();var metal=plan.District.Kind==MapResourceKind.Metal;
            var stairs=new Dictionary<Vector2Int,DistrictStair>();
            var maximumDepth=.1f;foreach(var value in plan.Heights.Values)maximumDepth=Mathf.Max(maximumDepth,-value);
            foreach(var stair in plan.Stairs)stairs[stair.Cell]=stair;
            var boundary=new List<(Vector2 a,Vector2 b)>();
            foreach(var cell in plan.District.Cells)
            {
                var a=(Vector2)cell-new Vector2(.5f,.5f);var b=a+Vector2.up;var c=a+Vector2.one;var d=a+Vector2.right;
                if(!plan.Heights.ContainsKey(cell+Vector2Int.left))boundary.Add((a,b));
                if(!plan.Heights.ContainsKey(cell+Vector2Int.up))boundary.Add((b,c));
                if(!plan.Heights.ContainsKey(cell+Vector2Int.right))boundary.Add((c,d));
                if(!plan.Heights.ContainsKey(cell+Vector2Int.down))boundary.Add((d,a));
            }
            const int divisions=8;
            var bounds=plan.District.Bounds;var width=bounds.width*divisions+1;var height=bounds.height*divisions+1;
            var points=new Vector3[width*height];var coverage=new float[points.Length];
            for(var y=0;y<height;y++)for(var x=0;x<width;x++)
            {
                var p=new Vector2(bounds.xMin-.5f+x/(float)divisions,bounds.yMin-.5f+y/(float)divisions);
                var distance=float.PositiveInfinity;
                foreach(var segment in boundary)
                {
                    var line=segment.b-segment.a;
                    var closest=segment.a+line*Mathf.Clamp01(Vector2.Dot(p-segment.a,line)/line.sqrMagnitude);
                    distance=Mathf.Min(distance,(p-closest).sqrMagnitude);
                }
                distance=Mathf.Sqrt(distance);
                var edge=Mathf.SmoothStep(0,1,distance/.65f);
                if(!plan.Heights.ContainsKey(Vector2Int.RoundToInt(p)))edge=0;
                var ground=-(Grass(p).z+.008f);
                var h=HeightAt(plan,art,p);
                points[y*width+x]=Point(p,Mathf.Lerp(ground,h,edge));coverage[y*width+x]=edge;
            }
            var vertexNormals=new Vector3[points.Length];var vertexColors=new Color[points.Length];
            for(var y=0;y<height;y++)for(var x=0;x<width;x++)
            {
                var i=y*width+x;
                var left=points[y*width+Mathf.Max(0,x-1)];var right=points[y*width+Mathf.Min(width-1,x+1)];
                var down=points[Mathf.Max(0,y-1)*width+x];var up=points[Mathf.Min(height-1,y+1)*width+x];
                vertexNormals[i]=new Vector3((right.z-left.z)/Mathf.Max(.001f,right.x-left.x),
                    (up.z-down.z)/Mathf.Max(.001f,up.y-down.y),-1).normalized;
                var color=Tint(metal,-points[i].z,false,Vector2Int.RoundToInt(points[i]),plan.District.Seed,maximumDepth);
                var groundZ=Stage3TacticalPresentation.GrassSurfaceZ(points[i]);
                color.a=Mathf.Clamp01(Mathf.Abs(points[i].z-groundZ)/.18f);vertexColors[i]=color;
            }
            foreach(var cell in plan.District.Cells)
            {
                if(stairs.TryGetValue(cell,out var flight)){AddStair(mesh,flight,metal,plan.District.Seed,maximumDepth);continue;}
                var startX=(cell.x-bounds.xMin)*divisions;var startY=(cell.y-bounds.yMin)*divisions;
                for(var y=0;y<divisions;y++)for(var x=0;x<divisions;x++)
                {
                    var i=(startY+y)*width+startX+x;
                    var a=points[i];var b=points[i+width];var c=points[i+width+1];var d=points[i+1];
                    var normal=Vector3.Cross(b-a,c-a).normalized;
                    mesh.SurfaceQuad(a,b,c,d,Mathf.Abs(normal.z)>.6f,
                        vertexNormals[i],vertexNormals[i+width],vertexNormals[i+width+1],vertexNormals[i+1],
                        vertexColors[i],vertexColors[i+width],vertexColors[i+width+1],vertexColors[i+1]);
                }
            }
            Create("Coherent terraced geology",mesh,art.geologyMaterial,root,cameraForward,art.geologyRevealMaterial);
        }

        private static float HeightAt(ResourceDistrictLayout plan,Stage3ResourceDistrictArt art,Vector2 point)
            {
            var metal=plan.District.Kind==MapResourceKind.Metal;
                foreach(var flight in plan.Stairs)
                    if(Mathf.Abs(point.x-flight.Cell.x)<=.50001f && Mathf.Abs(point.y-flight.Cell.y)<=.50001f)
                        return Mathf.Lerp(flight.Top,flight.Bottom,Mathf.Clamp01(Vector2.Dot(point-(Vector2)flight.Cell,flight.Direction)+.5f));
                var baseCell=Vector2Int.FloorToInt(point);var f=point-(Vector2)baseCell;
                float Sample(Vector2Int cell)=>plan.Heights.TryGetValue(cell,out var value)?value:-Stage3TacticalPresentation.GrassSurfaceZ(cell);
                var raw=Mathf.Lerp(Mathf.Lerp(Sample(baseCell),Sample(baseCell+Vector2Int.right),f.x),
                    Mathf.Lerp(Sample(baseCell+Vector2Int.up),Sample(baseCell+Vector2Int.one),f.x),f.y);
                var step=metal?art.shape.mineTerraceDepth:art.shape.stoneTerraceHeight;
                if(!metal && raw<=.45f)return raw;
                var level=metal?-raw/step:(raw-.45f)/step;
                level=Mathf.Max(0,level);var whole=Mathf.Floor(level);var fraction=level-whole;
                var eased=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.25f,.75f,fraction));
                return metal?-(whole+eased)*step:.45f+(whole+eased)*step;
            }

        internal static float SurfaceHeight(ResourceDistrictLayout plan,Stage3ResourceDistrictArt art,Vector2 point)
        {
            var distance=float.PositiveInfinity;
            foreach(var cell in plan.District.Cells)
            foreach(var direction in new[]{Vector2Int.left,Vector2Int.up,Vector2Int.right,Vector2Int.down})
            {
                if(plan.Heights.ContainsKey(cell+direction))continue;
                var middle=(Vector2)cell+(Vector2)direction*.5f;
                var tangent=new Vector2(-direction.y,direction.x)*.5f;
                var a=middle-tangent;var line=tangent*2;
                var closest=a+line*Mathf.Clamp01(Vector2.Dot(point-a,line));
                distance=Mathf.Min(distance,Vector2.Distance(point,closest));
            }
            var edge=plan.Heights.ContainsKey(Vector2Int.RoundToInt(point))?Mathf.SmoothStep(0,1,distance/.65f):0;
            return Mathf.Lerp(-(Grass(point).z+.008f),HeightAt(plan,art,point),edge);
        }

        private static void AddStair(Buffer mesh,DistrictStair stair,bool metal,int seed,float maximumDepth)
        {
            var side=new Vector2(-stair.Direction.y,stair.Direction.x)*.5f;
            var count=Mathf.Max(6,Mathf.CeilToInt((stair.Top-stair.Bottom)/.19f));
            for(var i=0;i<count;i++)
            {
                var p=Vector2.Lerp(stair.Start,stair.End,i/(float)count);
                var q=Vector2.Lerp(stair.Start,stair.End,(i+1f)/count);
                var high=Mathf.Lerp(stair.Top,stair.Bottom,i/(float)count);
                var low=Mathf.Lerp(stair.Top,stair.Bottom,(i+1f)/count);
                var tint=Tint(metal,high,false,stair.Cell,seed,maximumDepth)*1.30f;
                mesh.Quad(Point(p-side,high),Point(p+side,high),Point(q+side,high),Point(q-side,high),tint,true);
                mesh.Quad(Point(q-side,high),Point(q+side,high),Point(q+side,low),Point(q-side,low),tint*.82f,false);
            }
        }

        private static Color Tint(bool metal,float height,bool wall,Vector2Int cell,int seed,float maximumDepth)
        {
            const float variation=1f;
            var tone=metal?new Color(.53f,.57f,.61f):new Color(1.1f,1.08f,1.02f);
            if(metal)tone*=Mathf.Lerp(1.05f,.42f,Mathf.Pow(Mathf.Clamp01(-height/maximumDepth),1.2f));
            if(wall)tone*=.82f;
            tone*=variation;tone.a=1;return tone;
        }

        private static void BuildGroundBlend(ResourceDistrictLayout plan,Stage3ResourceDistrictArt art,Transform root)
        {
            var buffer=new Buffer();var bounds=plan.District.Bounds;var margin=Mathf.CeilToInt(art.groundBlendWidth+1);
            var cells=new HashSet<Vector2Int>(plan.District.Cells);var wood=plan.District.Kind==MapResourceKind.Wood;
            for(var y=bounds.yMin-margin;y<bounds.yMax+margin;y++)
            for(var x=bounds.xMin-margin;x<bounds.xMax+margin;x++)
            {
                if(!wood && cells.Contains(new Vector2Int(x,y)))continue;
                for(var dx=0;dx<2;dx++)for(var dy=0;dy<2;dy++)
                {
                    var p=new Vector2(x-.5f+dx*.5f,y-.5f+dy*.5f);
                    var a=p;var b=p+Vector2.up*.5f;var c=p+Vector2.one*.5f;var d=p+Vector2.right*.5f;
                    var ca=ColorAt(a);var cb=ColorAt(b);var cc=ColorAt(c);var cd=ColorAt(d);
                    if(ca.a+cb.a+cc.a+cd.a<.001f)continue;
                    buffer.GroundQuad(Grass(a),Grass(b),Grass(c),Grass(d),ca,cb,cc,cd);
                }
            }
            Create("Soft conforming district ground",buffer,art.groundBlendMaterial,root);
            Color ColorAt(Vector2 point)
            {
                var distance=float.PositiveInfinity;
                foreach(var cell in plan.District.Cells)
                {
                    var delta=new Vector2(Mathf.Max(0,Mathf.Abs(point.x-cell.x)-.5f),Mathf.Max(0,Mathf.Abs(point.y-cell.y)-.5f));
                    distance=Mathf.Min(distance,delta.sqrMagnitude);if(distance==0)break;
                }
                distance=Mathf.Sqrt(distance);
                var noise=Mathf.PerlinNoise(point.x*.9f+(plan.District.Seed&255),point.y*.9f);
                var alpha=(1-Mathf.SmoothStep(0,1,(distance+noise*.65f)/art.groundBlendWidth))*(wood?.72f:.65f);
                var color=wood?new Color(.24f,.27f,.13f):new Color(.47f,.44f,.33f);
                color.a=alpha;return color;
            }
        }

        private static Vector3 Grass(Vector2 point)
        {
            // Match the actual two triangles of each rendered terrain cell, not a floating flat decal.
            var cell=Vector2Int.FloorToInt(point+Vector2.one*.5f);
            var a=(Vector2)cell-Vector2.one*.5f;var u=point.x-a.x;var v=point.y-a.y;
            var za=Stage3TacticalPresentation.GrassSurfaceZ(a);
            var zb=Stage3TacticalPresentation.GrassSurfaceZ(a+Vector2.up);
            var zc=Stage3TacticalPresentation.GrassSurfaceZ(a+Vector2.one);
            var zd=Stage3TacticalPresentation.GrassSurfaceZ(a+Vector2.right);
            var z=v>=u?za+(zb-za)*v+(zc-zb)*u:za+(zd-za)*u+(zc-zd)*v;
            return new Vector3(point.x,point.y,z-.008f);
        }
        private static Vector3 Point(Vector2 point,float height)=>new Vector3(point.x,point.y,-height);
        private static uint Hash(int x,int y,int seed){unchecked{var h=(uint)(x*73856093^y*19349663^seed);h^=h>>13;h*=1274126177;return h;}}
        private static void Create(string name,Buffer buffer,Material material,Transform root,Vector3 cameraForward=default,Material reveal=null)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);
            var mesh=buffer.Mesh(name,cameraForward);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            // Both transparent apron and local geology reveal precede painted accents.
            // Opaque geology still uses depth; sort order only resolves transparent layers.
            renderer.sortingOrder=short.MinValue+(reveal!=null?1:0);
            if(reveal!=null)
            {
                var indices=mesh.GetTriangles(0);mesh.subMeshCount=2;mesh.SetTriangles(indices,1);
                renderer.sharedMaterials=new[]{material,reveal};
            }
            renderer.shadowCastingMode=reveal!=null?ShadowCastingMode.On:ShadowCastingMode.Off;
        }

        private sealed class Buffer
        {
            private readonly List<Vector3> vertices=new List<Vector3>();
            private readonly List<Vector3> normals=new List<Vector3>();
            private readonly List<Vector2> uv=new List<Vector2>();
            private readonly List<Color> colors=new List<Color>();
            private readonly List<int> triangles=new List<int>();
            internal void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color,bool top)
            {
                color.a=Mathf.Clamp01(color.a);
                var n=Vector3.Cross(b-a,c-a).normalized;
                var first=vertices.Count;
                foreach(var p in new[]{a,b,c,d})
                {
                    vertices.Add(p);normals.Add(n);colors.Add(color);
                    uv.Add(top?new Vector2(p.x,p.y):new Vector2(Mathf.Abs(n.x)>.5f?p.y:p.x,-p.z));
                }
                triangles.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
            }
            internal void GroundQuad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color ca,Color cb,Color cc,Color cd)
            {
                var first=vertices.Count;Quad(a,b,c,d,ca,true);
                colors[first]=ca;colors[first+1]=cb;colors[first+2]=cc;colors[first+3]=cd;
                for(var i=first;i<first+4;i++)uv[i]=(new Vector2(vertices[i].x,vertices[i].y)+Vector2.one*.5f)/4;
            }
            internal void SurfaceQuad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,bool top,
                Vector3 na,Vector3 nb,Vector3 nc,Vector3 nd,Color ca,Color cb,Color cc,Color cd)
            {
                var first=vertices.Count;Quad(a,b,c,d,ca,top);
                normals[first]=na;normals[first+1]=nb;normals[first+2]=nc;normals[first+3]=nd;
                colors[first]=ca;colors[first+1]=cb;colors[first+2]=cc;colors[first+3]=cd;
            }
            internal Mesh Mesh(string name,Vector3 cameraForward)
            {
                if(cameraForward.sqrMagnitude>.1f)
                {
                    // Fixed camera: sort complete painted faces once so local alpha cutaways
                    // retain the correct back-to-front order without per-frame mesh work.
                    var order=new List<int>();for(var i=0;i<vertices.Count;i+=4)order.Add(i);
                    order.Sort((a,b)=>Vector3.Dot((vertices[b]+vertices[b+2])*.5f,cameraForward)
                        .CompareTo(Vector3.Dot((vertices[a]+vertices[a+2])*.5f,cameraForward)));
                    triangles.Clear();foreach(var first in order)
                        triangles.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
                }
                var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};
                mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
            }
        }
    }
}
