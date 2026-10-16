using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage2.Map
{
    [Serializable]
    public sealed class ResourceDistrictShapeSettings
    {
        [Min(.1f)] public float treeCrownRadius = .85f;
        [Min(.1f)] public float minimumTreeGap = .65f;
        [Min(.1f)] public float maximumTreeGap = 1.35f;
        [Min(.1f)] public float minimumTreeScale = .55f;
        [Min(.1f)] public float maximumTreeScale = 1.4f;
        [Min(.1f)] public float stoneTerraceHeight = 1.1f;
        [Min(.1f)] public float mineTerraceDepth = 1.4f;
        [Min(1)] public int terraceWidthCells = 2;
        [Range(2,5)] public int maximumTerraces = 4;
        public void Validate()
        {
            if(treeCrownRadius<=0 || minimumTreeGap<=0 || maximumTreeGap<minimumTreeGap ||
                minimumTreeScale<=0 || maximumTreeScale<minimumTreeScale || stoneTerraceHeight<=0 ||
                mineTerraceDepth<=0 || terraceWidthCells<1 || maximumTerraces<2 || maximumTerraces>5)
                throw new ArgumentException("Invalid resource district shape settings.");
        }
    }

    public readonly struct DistrictTree
    {
        public DistrictTree(Vector2 point, float scale, int variant) { Point=point; Scale=scale; Variant=variant; }
        public Vector2 Point { get; }
        public float Scale { get; }
        public int Variant { get; }
    }

    public readonly struct DistrictStair
    {
        public DistrictStair(Vector2Int cell, Vector2Int direction, float top, float bottom)
        { Cell=cell; Direction=direction; Top=top; Bottom=bottom; }
        public Vector2Int Cell { get; }
        public Vector2Int Direction { get; }
        public float Top { get; }
        public float Bottom { get; }
        public Vector2 Start => (Vector2)Cell-(Vector2)Direction*.5f;
        public Vector2 End => (Vector2)Cell+(Vector2)Direction*.5f;
    }

    /// <summary>One reproducible structural plan. Meshes, stairs, decoration and collision
    /// consume this data instead of independently scattering mine pieces.</summary>
    public sealed class ResourceDistrictLayout
    {
        private static readonly Vector2Int[] Neighbors={Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
        private readonly Dictionary<Vector2Int,float> heights=new Dictionary<Vector2Int,float>();
        private readonly Dictionary<Vector2Int,int> depths=new Dictionary<Vector2Int,int>();
        private readonly List<DistrictTree> trees=new List<DistrictTree>();
        private readonly List<DistrictStair> stairs=new List<DistrictStair>();
        public ResourceDistrict District { get; }
        public IReadOnlyDictionary<Vector2Int,float> Heights => heights;
        public IReadOnlyDictionary<Vector2Int,int> Depths => depths;
        public IReadOnlyList<DistrictTree> Trees => trees;
        public IReadOnlyList<DistrictStair> Stairs => stairs;
        public Vector2Int MineEntrance { get; private set; }

        public ResourceDistrictLayout(ResourceDistrict district, ResourceDistrictShapeSettings settings, Vector2 viewUp)
        {
            District=district;
            settings.Validate();
            var cells=new HashSet<Vector2Int>(district.Cells);var queue=new Queue<Vector2Int>();
            foreach(var cell in district.Cells)
                foreach(var direction in Neighbors)
                    if(!cells.Contains(cell+direction)) { depths[cell]=0;queue.Enqueue(cell);break; }
            while(queue.Count>0)
            {
                var cell=queue.Dequeue();
                foreach(var direction in Neighbors)
                    if(cells.Contains(cell+direction) && !depths.ContainsKey(cell+direction))
                    { depths[cell+direction]=depths[cell]+1;queue.Enqueue(cell+direction); }
            }
            if(district.Kind==MapResourceKind.Wood) { PlaceTrees(settings);return; }
            foreach(var cell in district.Cells)
            {
                var depth=depths[cell];
                if(district.Kind==MapResourceKind.Metal)
                    heights[cell]=-settings.mineTerraceDepth*(1+Mathf.Min(settings.maximumTerraces-1,depth/settings.terraceWidthCells));
                else
                {
                    var back=Vector2.Dot((Vector2)(cell-district.Center),viewUp.normalized);
                    var ridge=depth*.68f+back*.10f+
                        Mathf.PerlinNoise(cell.x*.16f+(district.Seed&255),cell.y*.16f)*.30f;
                    var tier=Mathf.Clamp(Mathf.FloorToInt(ridge),0,settings.maximumTerraces-1);
                    heights[cell]=.45f+tier*settings.stoneTerraceHeight;
                }
            }
            if(district.Kind==MapResourceKind.Metal) ConnectMine(viewUp.normalized);
        }

        private void PlaceTrees(ResourceDistrictShapeSettings settings)
        {
            var rng=new System.Random(District.Seed);
            var candidates=new List<Vector2Int>(District.Cells);
            for(var i=candidates.Count-1;i>0;i--) { var j=rng.Next(i+1);var value=candidates[j];candidates[j]=candidates[i];candidates[i]=value; }
            var angle=(float)rng.NextDouble()*Mathf.PI*2;
            var trail=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            var cross=new Vector2(-trail.y,trail.x);
            var phase=(District.Seed&255)*.071f;
            foreach(var cell in candidates)
            {
                var point=(Vector2)cell+new Vector2((float)rng.NextDouble()-.5f,(float)rng.NextDouble()-.5f)*.6f;
                var relative=point-District.Center;
                var along=Vector2.Dot(relative,trail);
                // A loose winding opening through each forest, with younger trees around its edge.
                var trailDistance=Mathf.Abs(Vector2.Dot(relative,cross)-Mathf.Sin(along*.38f+phase)*1.2f);
                if(trailDistance<.85f)continue;
                var scale=Mathf.Lerp(settings.minimumTreeScale,settings.maximumTreeScale,Mathf.Pow((float)rng.NextDouble(),1.35f));
                if(trailDistance<1.8f)scale=Mathf.Min(scale,.8f);
                var gap=Mathf.Lerp(settings.minimumTreeGap,settings.maximumTreeGap,(float)rng.NextDouble());
                var clear=true;
                foreach(var other in trees)
                    if(Vector2.Distance(point,other.Point)<settings.treeCrownRadius*(scale+other.Scale)+gap) { clear=false;break; }
                if(clear)trees.Add(new DistrictTree(point,scale,rng.Next(6)));
            }
        }

        private void ConnectMine(Vector2 up)
        {
            // A complete straight approach is selected before any flights are built. This
            // guarantees every stair has a matching landing and cannot point off a ledge.
            var outward=Vector2Int.down;var length=0;var best=float.NegativeInfinity;
            foreach(var direction in Neighbors)
            {
                var count=0;while(heights.ContainsKey(District.Center+direction*(count+1)))count++;
                var score=Vector2.Dot(direction,-up)*4+count*.1f;
                if(count>=3 && score>best){best=score;outward=direction;length=count;}
            }
            MineEntrance=District.Center+outward*length;
            var path=new List<Vector2Int>();
            for(var i=length;i>=0;i--)path.Add(District.Center+outward*i);
            var previous=.15f;
            for(var i=0;i<path.Count;i++)
            {
                var cell=path[i];heights[cell]=i==0?previous:Mathf.Min(previous,heights[cell]);
                previous=heights[cell];
            }
            for(var i=0;i+1<path.Count;i++)
            {
                var cell=path[i];var next=path[i+1];
                if(heights[cell]>heights[next]+.01f)
                    stairs.Add(new DistrictStair(cell,next-cell,heights[cell],heights[next]));
            }
        }

        private static float Noise(Vector2Int p,int seed)
        {
            unchecked { var h=(uint)(p.x*73856093 ^ p.y*19349663 ^ seed);h^=h>>13;h*=1274126177;return (h&65535)/65535f; }
        }
    }
}
