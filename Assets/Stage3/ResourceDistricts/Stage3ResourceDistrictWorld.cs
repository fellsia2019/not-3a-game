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
        private float visibilityClock;
        public int Seed { get; private set; }
        public IReadOnlyList<ResourceDistrict> Districts { get; private set; }
        public IReadOnlyList<ResourceDistrictLayout> Layouts => layouts;
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
            var generated=ResourceDistrictGenerator.Generate(map,art.settings,seed,heroStart);
            ClearScenery();Seed=seed;Districts=generated;Obstacles=new HeroObstacleMap();visibilityClock=0;
            scenery=new GameObject("Procedural resource districts seed "+seed);scenery.transform.SetParent(transform,false);
            var up=new Vector2(view.transform.up.x,view.transform.up.y).normalized;
            foreach(var district in Districts)
            {
                var layout=new ResourceDistrictLayout(district,art.shape,up);layouts.Add(layout);Build(layout);
            }
            mover?.BindObstacles(Obstacles,art.heroCollisionRadius);
            minimap?.SetResourceDistricts(Districts);rebuildLandscape?.Invoke(Districts);
            Debug.Log($"S3_RESOURCE_DISTRICTS_READY seed={Seed} districts={Districts.Count} sprites={SpriteCount} "+
                $"cells={art.settings.minimumCells}..{art.settings.maximumCells} economyChanges=0 "+
                $"solidStrips={Obstacles.Rectangles.Count} groundCircles={Obstacles.Circles.Count} hybrid=true masks={masks.Count}");
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
                var clear=true;foreach(var other in placed)if(Vector2.Distance(point,other)<2.25f){clear=false;break;}
                if(!clear)continue;placed.Add(point);
                var scale=(edge?.60f:.40f)+(float)rng.NextDouble()*.28f;
                var index=rng.Next(3)+(district.Kind==MapResourceKind.Metal?3:0);
                Add(art.rocks[index],point,scale,rng,root,true,plan.Heights[cell]);
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
            Add(art.mineParts[4],(Vector2)landing+up*.15f,.55f,rng,root,false,height);
            Add(art.mineParts[2],(Vector2)landing+right*.45f,.32f,rng,root,true,height);
        }

        private void Add(Sprite sprite,Vector2 point,float scale,System.Random rng,Transform root,bool flip=true,float height=0)
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
            renderer.transform.SetPositionAndRotation(new Vector3(point.x,point.y,z)-view.transform.forward*clearance,view.transform.rotation);
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
            foreach(var set in new[]{art.trees,art.rocks,art.mineParts})foreach(var sprite in set)
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
            details.Clear();layouts.Clear();
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
        }
    }
}
