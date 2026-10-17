using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Unity.Profiling;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Scene-owned structural scenery, ground collision and local, eased obstruction reveal.</summary>
    public sealed class Stage3ResourceDistrictWorld : MonoBehaviour
    {
        private static readonly ProfilerMarker VisibilityMarker=new ProfilerMarker("ResourceDistrict.Visibility");
        private static readonly int HeroProperty=Shader.PropertyToID("_DistrictHeroWorld");
        private static readonly int RevealProperty=Shader.PropertyToID("_DistrictRevealParameters");
        private Stage2MapDefinition map;
        private Stage3ResourceDistrictArt art;
        private Camera view;
        private Transform hero;
        private Stage2PlayerMover mover;
        private Vector2 heroStart;
        private Stage2Minimap minimap;
        private Action<IReadOnlyList<ResourceDistrict>> rebuildLandscape;
        private GameObject scenery;
        private readonly List<Detail> details=new List<Detail>();
        private readonly Dictionary<Sprite,Mask> masks=new Dictionary<Sprite,Mask>();
        private readonly List<ResourceDistrictLayout> layouts=new List<ResourceDistrictLayout>();
        private readonly List<ResourceProductionPoint> productionPoints=new List<ResourceProductionPoint>();
        private float visibilityClock;
        public int Seed { get; private set; }
        public IReadOnlyList<ResourceDistrict> Districts { get; private set; }
        public IReadOnlyList<ResourceDistrictLayout> Layouts => layouts;
        public IReadOnlyList<ResourceProductionPoint> ProductionPoints => productionPoints.AsReadOnly();
        public int SpriteCount => details.Count;
        public int CachedMaskCount => masks.Count;
        public HeroObstacleMap Obstacles { get; private set; }

        public void Initialize(Stage2MapDefinition definition,Stage3ResourceDistrictArt artwork,
            Camera camera,Transform player,Stage2Minimap mapView,Action<IReadOnlyList<ResourceDistrict>> landscapeChanged)
        {
            if(artwork==null || artwork.trees==null || artwork.trees.Length!=6 || artwork.rocks==null ||
                artwork.rocks.Length!=6 || artwork.mineParts==null || artwork.mineParts.Length!=6 ||
                artwork.material==null || artwork.geologyMaterial==null || artwork.geologyRevealMaterial==null || artwork.groundBlendMaterial==null)
                throw new ArgumentException("Resource district hybrid art kit is incomplete. Update hybrid data in the Editor.");
            map=definition;art=artwork;view=camera;hero=player;mover=player.GetComponent<Stage2PlayerMover>();
            heroStart=player.position;minimap=mapView;rebuildLandscape=landscapeChanged;
            PrepareMasks();
            var seed=art.randomizeWorldSeed?Guid.NewGuid().GetHashCode():art.fixedWorldSeed;
            var arguments=Environment.GetCommandLineArgs();
            for(var i=0;i+1<arguments.Length;i++)
                if(arguments[i]=="-worldSeed" && int.TryParse(arguments[i+1],out var parsed))seed=parsed;
            Regenerate(seed);
        }

        public void Regenerate(int seed)
        {
            var footprints = new Dictionary<MapResourceKind, IReadOnlyList<Vector2Int>>();
            foreach (var landmark in art.landmarks)
                if (landmark.layers.Length > 0) footprints.Add(landmark.kind, LandmarkFootprint(landmark));
            var generated=ResourceDistrictGenerator.Generate(map,art.settings,seed,heroStart,footprints);
            ClearScenery();Seed=seed;Districts=generated;Obstacles=new HeroObstacleMap();visibilityClock=0;
            scenery=new GameObject("Procedural resource districts seed "+seed);scenery.transform.SetParent(transform,false);
            var up=new Vector2(view.transform.up.x,view.transform.up.y).normalized;
            for (var districtIndex = 0; districtIndex < Districts.Count; districtIndex++)
            {
                var district = Districts[districtIndex];
                var landmark = Array.Find(art.landmarks, entry => entry.kind == district.Kind && entry.layers.Length > 0);
                if (landmark != null) { BuildLandmark(district, landmark, districtIndex); continue; }
                var layout=new ResourceDistrictLayout(district,art.shape,up);layouts.Add(layout);Build(layout);
            }
            GetComponent<Stage2GameComposition>()?.Production?.BindWorld(productionPoints, Obstacles);
            mover?.BindObstacles(Obstacles,art.heroCollisionRadius);
            minimap?.SetResourceDistricts(Districts);rebuildLandscape?.Invoke(Districts);
            var reservedCells = 0; foreach (var district in Districts) reservedCells += district.Cells.Count;
            var representation = layouts.Count == 0 ? "authored" : productionPoints.Count == 0 ? "hybrid" : "mixed-pilot";
            Debug.Log($"S3_RESOURCE_DISTRICTS_READY seed={Seed} districts={Districts.Count} sprites={SpriteCount} "+
                $"reservedCells={reservedCells} economyChanges=0 representation={representation} "+
                $"solidStrips={Obstacles.Rectangles.Count} groundCircles={Obstacles.Circles.Count} "+
                $"productionPoints={productionPoints.Count} masks={masks.Count}");
        }

        private IReadOnlyList<Vector2Int> LandmarkFootprint(Stage3ResourceLandmark definition)
        {
            if (definition.groundContour.Length < 3)
                throw new ArgumentException("Authored landmark needs a ground contour.");
            var scale = art.landmarkScale;
            var min = definition.groundContour[0] * scale;
            var max = min;
            foreach (var point in definition.groundContour)
            { min = Vector2.Min(min, point * scale); max = Vector2.Max(max, point * scale); }
            var half = definition.productionClearance * scale * .5f;
            var pointCenter = definition.productionPoint * scale;
            min = Vector2.Min(min, pointCenter - half); max = Vector2.Max(max, pointCenter + half);
            var cells = new List<Vector2Int>();
            var clearance = new Rect(pointCenter - half, half * 2);
            bool InContour(Vector2 point)
            {
                var inside = false;
                var polygon = definition.groundContour;
                for (var i = 0; i < polygon.Length; i++)
                {
                    var a = polygon[i] * scale;
                    var b = polygon[(i + 1) % polygon.Length] * scale;
                    if ((a.y > point.y) != (b.y > point.y) &&
                        point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
                }
                return inside;
            }
            // Reserve the authored contour and its future building clearance.
            // Collision remains independently authored ground areas/trunks, never image alpha.
            for (var y = Mathf.FloorToInt(min.y); y <= Mathf.CeilToInt(max.y); y++)
            for (var x = Mathf.FloorToInt(min.x); x <= Mathf.CeilToInt(max.x); x++)
            {
                var cell = new Rect(x-.5f,y-.5f,1,1);
                if (clearance.Overlaps(cell) || InContour(cell.center) ||
                    InContour(cell.min) || InContour(cell.max) ||
                    InContour(new Vector2(cell.xMin,cell.yMax)) || InContour(new Vector2(cell.xMax,cell.yMin)))
                    cells.Add(new Vector2Int(x, y));
            }
            return cells;
        }

        private void BuildLandmark(ResourceDistrict district, Stage3ResourceLandmark definition, int districtIndex)
        {
            var root = new GameObject(district.Kind + " authored landmark " + district.Seed).transform;
            root.SetParent(scenery.transform, false);
            var scale = art.landmarkScale;
            var origin = (Vector2)district.Center;
            foreach (var rect in definition.solidGround)
                Obstacles.AddRectangle(new Rect(origin + rect.position * scale, rect.size * scale));
            foreach (var trunk in definition.groundCircles)
                Obstacles.AddCircle(origin + trunk.center * scale, trunk.radius * scale);
            productionPoints.Add(new ResourceProductionPoint(districtIndex, district.Kind,
                origin + definition.productionPoint * scale, definition.productionClearance * scale,
                definition.productionFacingDegrees));
            foreach (var layer in definition.layers)
            {
                var point = origin + layer.groundAnchor * scale;
                var sprite = layer.sprite;
                var factor = layer.width * scale / sprite.bounds.size.x;
                var renderer = new GameObject(sprite.name).AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(root, false);
                renderer.sprite = sprite; renderer.sharedMaterial = art.material;
                renderer.color = Color.white;
                renderer.transform.localScale = Vector3.one * factor;
                var belowPivot = sprite.pivot.y / sprite.pixelsPerUnit * factor;
                var clearance = belowPivot * Mathf.Abs(view.transform.up.z) /
                    Mathf.Max(.1f, Mathf.Abs(view.transform.forward.z)) + .08f;
                var anchor = new Vector3(point.x, point.y, Stage3TacticalPresentation.GrassSurfaceZ(point));
                renderer.transform.SetPositionAndRotation(anchor - view.transform.forward * clearance, view.transform.rotation);
                renderer.sortingOrder = layer.groundLayer ? short.MinValue : -Mathf.RoundToInt(point.y * 100);
                if (layer.groundImageAreas.Length > 0)
                {
                    var block = new MaterialPropertyBlock();
                    var regions = new Vector4[8];
                    for (var i = 0; i < Mathf.Min(8, layer.groundImageAreas.Length); i++)
                    {
                        var region = layer.groundImageAreas[i];
                        regions[i] = new Vector4(region.xMin, region.yMin, region.xMax, region.yMax);
                    }
                    block.SetVectorArray("_LandmarkGroundAreas", regions);
                    block.SetFloat("_LandmarkGroundAreaCount", Mathf.Min(8, layer.groundImageAreas.Length));
                    var forward = view.transform.forward;
                    block.SetVector("_LandmarkGroundPlane", new Vector4(forward.x,forward.y,forward.z,-Stage3TacticalPresentation.GrassVisualLift-.125f));
                    renderer.SetPropertyBlock(block);
                }
                details.Add(new Detail { Renderer = renderer, Color = Color.white,
                    Center = renderer.transform.TransformPoint(sprite.bounds.center),
                    Width = sprite.bounds.size.x * factor, Height = sprite.bounds.size.y * factor, Mask = masks[sprite],
                    GroundImageAreas = layer.groundImageAreas });
            }
        }

        private void Build(ResourceDistrictLayout plan)
        {
            var district=plan.District;
            var root=new GameObject(district.Kind+" district "+district.Seed).transform;root.SetParent(scenery.transform,false);
            Stage3DistrictGeometry.Build(plan,art,root,view.transform.forward);
            var rng=new System.Random(district.Seed);
            if(district.Kind==MapResourceKind.Wood)
            {
                foreach(var tree in plan.Trees)
                {
                    Add(art.trees[tree.Variant],tree.Point,tree.Scale,rng,root);
                    Obstacles.AddCircle(tree.Point,tree.Scale*art.treeTrunkRadius);
                }
                return;
            }
            Obstacles.AddSolidCells(district.Cells);
            Vector3[] relief=null;
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
                if(filter.name=="Coherent terraced geology")relief=filter.sharedMesh.vertices;
            // A few edge accents sit on the structural surface. Interior cliffs belong to the mesh.
            var placed=new List<Vector2>();
            foreach(var cell in district.Cells)
            {
                var stairTile=false;foreach(var flight in plan.Stairs)if(flight.Cell==cell){stairTile=true;break;}
                if(stairTile)continue;
                var edge=plan.Depths[cell]==0;
                var lip=false;
                if(district.Kind==MapResourceKind.Stone && !edge)
                    foreach(var direction in new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left})
                        if(plan.Heights.TryGetValue(cell+direction,out var neighbor) && plan.Heights[cell]>neighbor+.5f)lip=true;
                if((!edge && !lip) || rng.NextDouble()>(edge?.78:.22))continue;
                var point=(Vector2)cell;
                if(edge && district.Kind==MapResourceKind.Metal)
                {
                    var rimOutward=Vector2.zero;
                    foreach(var direction in new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left})
                        if(!plan.Heights.ContainsKey(cell+direction))rimOutward+=(Vector2)direction;
                    if(rimOutward.sqrMagnitude>.01f)point+=rimOutward.normalized*.65f;
                }
                var clear=true;foreach(var other in placed)if(Vector2.Distance(point,other)<2.25f){clear=false;break;}
                if(!clear)continue;placed.Add(point);
                var scale=(edge?.60f:.40f)+(float)rng.NextDouble()*.28f;
                var index=rng.Next(3)+(district.Kind==MapResourceKind.Metal?3:0);
                Add(art.rocks[index],point,scale,rng,root,true,Stage3DistrictGeometry.SurfaceHeight(plan,art,point),relief);
                Obstacles.AddCircle(point,scale*art.rockFootprintRadius);
            }
            if(district.Kind!=MapResourceKind.Metal)return;
            var up=new Vector2(view.transform.up.x,view.transform.up.y).normalized;
            var right=new Vector2(view.transform.right.x,view.transform.right.y).normalized;
            var entrance=(Vector2)plan.MineEntrance;
            var outward=(entrance-district.Center).normalized;
            var hoist=entrance+outward*1.8f+right*1.3f;
            Add(art.mineParts[5],hoist,.8f,rng,root,false);
            Obstacles.AddCircle(hoist,art.hoistFootprintRadius*.8f);
            // The lower shoring/ore are grounded on the deepest connected landing, never scattered midair.
            var landing=district.Center;var height=plan.Heights[landing];
            Add(art.mineParts[4],(Vector2)landing+up*.15f,.55f,rng,root,false,height,relief);
            Add(art.mineParts[2],(Vector2)landing+right*.45f,.32f,rng,root,true,height,relief);
        }

        private void Add(Sprite sprite,Vector2 point,float scale,System.Random rng,Transform root,bool flip=true,float height=0,Vector3[] relief=null)
        {
            var renderer=new GameObject(sprite.name).AddComponent<SpriteRenderer>();renderer.transform.SetParent(root,false);
            renderer.sprite=sprite;renderer.sharedMaterial=art.material;
            renderer.transform.localScale=new Vector3(scale,scale*(.96f+(float)rng.NextDouble()*.08f),1);
            renderer.flipX=flip && rng.Next(2)==0;
            var brightness=.94f+(float)rng.NextDouble()*.09f;
            var color=new Color(brightness,brightness,brightness,1);renderer.color=color;
            var belowPivot=sprite.pivot.y/sprite.pixelsPerUnit*renderer.transform.localScale.y;
            var clearance=belowPivot*Mathf.Abs(view.transform.up.z)/Mathf.Max(.1f,Mathf.Abs(view.transform.forward.z))+.04f;
            var z=Mathf.Abs(height)>.0001f?-height:Stage3TacticalPresentation.GrassSurfaceZ(point);
            var anchor=new Vector3(point.x,point.y,z);
            if(relief!=null)
            {
                // A camera-facing painted rock has no physical thickness. Push its plane
                // only in view depth past its own relief envelope, preserving screen position
                // and the ground/collision anchor. Transparent margins do not participate.
                var bounds=sprite.bounds;var mask=masks[sprite];
                foreach(var vertex in relief)
                {
                    var relative=vertex-anchor;
                    var x=Vector3.Dot(relative,view.transform.right)/renderer.transform.localScale.x;
                    var y=Vector3.Dot(relative,view.transform.up)/renderer.transform.localScale.y;
                    if(renderer.flipX)x=-x;
                    var u=(x-bounds.min.x)/bounds.size.x;var v=(y-bounds.min.y)/bounds.size.y;
                    if(u<0 || u>=1 || v<0 || v>=1 || !mask.Opaque[(int)(v*Mask.Size)*Mask.Size+(int)(u*Mask.Size)])continue;
                    clearance=Mathf.Max(clearance,-Vector3.Dot(relative,view.transform.forward)+.08f);
                }
            }
            renderer.transform.SetPositionAndRotation(anchor-view.transform.forward*clearance,view.transform.rotation);
            renderer.sortingOrder=-Mathf.RoundToInt(point.y*100);
            var localCenter=sprite.bounds.center;
            if(renderer.flipX)localCenter.x=-localCenter.x;
            details.Add(new Detail{Renderer=renderer,Color=color,Center=renderer.transform.TransformPoint(localCenter),
                Width=sprite.bounds.size.x*scale,Height=sprite.bounds.size.y*renderer.transform.localScale.y,Mask=masks[sprite]});
        }

        private void PrepareMasks()
        {
            masks.Clear();
            var textures=new Dictionary<Texture2D,Color32[]>();
            var sprites = new List<Sprite>();
            foreach (var set in new[] { art.trees, art.rocks, art.mineParts }) sprites.AddRange(set);
            foreach (var landmark in art.landmarks)
                foreach (var layer in landmark.layers) sprites.Add(layer.sprite);
            foreach(var sprite in sprites)
            {
                var texture=sprite.texture;
                if(!textures.TryGetValue(texture,out var pixels)){pixels=texture.GetPixels32();textures[texture]=pixels;}
                var mask=new Mask();var rect=sprite.rect;
                for(var y=0;y<Mask.Size;y++)for(var x=0;x<Mask.Size;x++)
                {
                    var px=Mathf.Min(texture.width-1,(int)(rect.x+(x+.5f)/Mask.Size*rect.width));
                    var py=Mathf.Min(texture.height-1,(int)(rect.y+(y+.5f)/Mask.Size*rect.height));
                    mask.Opaque[y*Mask.Size+x]=pixels[py*texture.width+px].a>100;
                }
                masks[sprite]=mask;
            }
        }

        private void LateUpdate()=>RefreshVisibility(Time.unscaledDeltaTime);

        public void RefreshVisibility(float deltaTime)
        {
            if(hero==null || view==null || art==null)return;
            using(VisibilityMarker.Auto())
            {
                deltaTime=Mathf.Max(0,deltaTime);visibilityClock+=deltaTime;
                var focus=hero.position+Vector3.back*.5f;
                Shader.SetGlobalVector(HeroProperty,new Vector4(focus.x,focus.y,focus.z,1));
                Shader.SetGlobalVector(RevealProperty,new Vector4(art.revealRadius,art.revealSoftness,0,art.occludedOpacity));
                var heroPoint=view.WorldToScreenPoint(focus);var pixelsPerUnit=view.pixelHeight/(2*view.orthographicSize);
                var heroArea=new Rect(heroPoint.x-.35f*pixelsPerUnit,heroPoint.y-.45f*pixelsPerUnit,.7f*pixelsPerUnit,.9f*pixelsPerUnit);
                foreach(var detail in details)
                {
                    var center=view.WorldToScreenPoint(detail.Center);
                    var width=detail.Width*pixelsPerUnit;var height=detail.Height*pixelsPerUnit;
                    var area=new Rect(center.x-width*.5f,center.y-height*.5f,width,height);
                    var overlaps=center.z<heroPoint.z+.3f && area.Overlaps(heroArea) && OpaqueOverlap(detail,area,heroArea);
                    if(overlaps)detail.HoldUntil=visibilityClock+art.occlusionHoldSeconds;
                    var target=overlaps || visibilityClock<detail.HoldUntil?art.occludedOpacity:1;
                    var color=detail.Color;
                    color.a=Mathf.SmoothDamp(detail.Renderer.color.a,target,ref detail.Velocity,
                        target<detail.Renderer.color.a?art.fadeOutSeconds:art.fadeInSeconds,float.PositiveInfinity,deltaTime);
                    detail.Renderer.color=color;
                }
            }
        }

        private static bool OpaqueOverlap(Detail detail,Rect screen,Rect heroArea)
        {
            for(var y=0;y<5;y++)for(var x=0;x<5;x++)
            {
                var point=new Vector2(Mathf.Lerp(heroArea.xMin,heroArea.xMax,(x+.5f)/5),Mathf.Lerp(heroArea.yMin,heroArea.yMax,(y+.5f)/5));
                if(!screen.Contains(point))continue;
                var u=(point.x-screen.xMin)/screen.width;var v=(point.y-screen.yMin)/screen.height;
                if(detail.Renderer.flipX)u=1-u;
                var ground = false;
                foreach (var region in detail.GroundImageAreas)
                    if (region.Contains(new Vector2(u,v))) { ground = true; break; }
                if (ground) continue;
                if(detail.Mask.Opaque[Mathf.Clamp((int)(v*Mask.Size),0,Mask.Size-1)*Mask.Size+Mathf.Clamp((int)(u*Mask.Size),0,Mask.Size-1)])return true;
            }
            return false;
        }

        private void ClearScenery()
        {
            if(scenery!=null)
            {
                scenery.SetActive(false);
                foreach(var filter in scenery.GetComponentsInChildren<MeshFilter>(true))if(filter.sharedMesh!=null)Destroy(filter.sharedMesh);
                Destroy(scenery);
            }
            details.Clear();layouts.Clear();productionPoints.Clear();
        }
        private void OnDestroy()
        {
            if(mover!=null && ReferenceEquals(mover.Obstacles,Obstacles))mover.BindObstacles(null,art.heroCollisionRadius);
            if(minimap!=null)minimap.SetResourceDistricts(null);
            Shader.SetGlobalVector(HeroProperty,Vector4.zero);ClearScenery();masks.Clear();
        }
        private sealed class Mask { internal const int Size=32;internal readonly bool[] Opaque=new bool[Size*Size]; }
        private sealed class Detail
        {
            internal SpriteRenderer Renderer;internal Color Color;internal Vector3 Center;
            internal float Width,Height,Velocity,HoldUntil;internal Mask Mask;
            internal Rect[] GroundImageAreas = Array.Empty<Rect>();
        }
    }
}
