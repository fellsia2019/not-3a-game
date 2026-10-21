using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Approved open clearings. Actors, quests, site reservations and collision integration are external.</summary>
    public sealed class Stage3CampPresentation : MonoBehaviour
    {
        public readonly struct PropPlacement
        {
            public readonly int SpriteIndex;
            public readonly Vector2 Offset,Footprint;
            public readonly float Width;
            public Rect GroundRect=>new Rect(Offset-Footprint*.5f,Footprint);
            public PropPlacement(int index,float x,float y,float width,float footprintX,float footprintY)
            {SpriteIndex=index;Offset=new Vector2(x,y);Width=width;Footprint=new Vector2(footprintX,footprintY);}
        }
        public const float CombatClearRadius=10f;
        public const float EscapeCorridorWidth=6f;
        public static Rect LocalBounds=>new Rect(-22,-20,44,40);
        // Camp silhouettes sit behind and to one side of the combat field. These same
        // rectangles are the sole solid-ground contract, never a ring around the actors.
        private static readonly PropPlacement[] OrcCore=
        {
            new PropPlacement(0,13,14,7.2f,5.6f,4.3f),
            new PropPlacement(1,19,10,3.8f,3f,2.2f),
            new PropPlacement(2,13,9.8f,2.3f,1.7f,1.7f),
            new PropPlacement(3,8.7f,14,1.8f,1.4f,1.1f),
            new PropPlacement(4,18.6f,17,1.8f,1.5f,.6f),
            new PropPlacement(5,10,10.8f,1.9f,1.5f,.8f),
            new PropPlacement(3,18,7.7f,1.8f,1.4f,1.1f)
        };
        private static readonly PropPlacement[] OgreCore=
        {
            new PropPlacement(0,-14,14,7.6f,6.2f,4.5f),
            new PropPlacement(1,-20,10,3.6f,2.8f,.9f),
            new PropPlacement(2,-8,16,2.1f,1.3f,1.3f),
            new PropPlacement(3,-18,7.6f,2.3f,1.9f,1.2f),
            new PropPlacement(4,-10,11.2f,2.6f,2f,1.2f),
            new PropPlacement(5,-14,9,2.7f,1.8f,1.8f),
            new PropPlacement(1,-19,16.8f,3.5f,2.9f,1f),
            new PropPlacement(2,-7.2f,12.6f,1.7f,1.1f,1.1f)
        };
        private static readonly PropPlacement[] OrcLayout=BuildLayout(0),OgreLayout=BuildLayout(1);
        private static readonly IReadOnlyList<PropPlacement>[] Layouts=
            {Array.AsReadOnly(OrcLayout),Array.AsReadOnly(OgreLayout)};
        private static readonly IReadOnlyList<Rect>[] Footprints=
            {MakeFootprints(OrcLayout),MakeFootprints(OgreLayout)};
        public static IReadOnlyList<PropPlacement> GetLayout(int kind)=>Layouts[ValidateKind(kind)];
        public static IReadOnlyList<Rect> GetLocalFootprints(int kind)=>Footprints[ValidateKind(kind)];
        public Vector2 Origin{get;private set;}
        public int Kind{get;private set;}
        public Rect WorldBounds=>new Rect(Origin+LocalBounds.position,LocalBounds.size);
        public bool IsBuilt=>campRoot!=null;
        public int RenderedPropCount{get;private set;}
        private GameObject campRoot;
        private Mesh groundMesh;
        private Camera view;
        private readonly List<SpriteRenderer> renderers=new List<SpriteRenderer>();
        private static readonly int HeroProperty=Shader.PropertyToID("_DistrictHeroWorld");

        private static PropPlacement[] BuildLayout(int kind)
        {
            var result=new List<PropPlacement>(kind==0?OrcCore:OgreCore);
            // Three broken woodland/rock shoulders, not an enclosing hedge or palisade.
            // The rear shoulder sits opposite the authored camp cluster.
            var sign=kind==0?-1f:1f;
            AddShoulder(sign*17f,14f,sign);
            AddShoulder(-17,-13,-1);
            AddShoulder(16,-14,1);
            // A few trees behind the shelter join it to the landscape without closing a route.
            var side=kind==0?1f:-1f;
            result.Add(new PropPlacement(101,side*10.8f,18.6f,4.2f,.65f,.65f));
            result.Add(new PropPlacement(102,side*18.8f,18.2f,4.7f,.7f,.7f));
            result.Add(new PropPlacement(300,side*18.8f,15.7f,3.8f,0,0));
            return result.ToArray();

            void AddShoulder(float x,float y,float mirror)
            {
                result.Add(new PropPlacement(100,x,y,4.8f,.7f,.7f));
                result.Add(new PropPlacement(101,x+mirror*2.5f,y+2.5f,5.1f,.7f,.7f));
                result.Add(new PropPlacement(102,x-mirror*2.5f,y+1.6f,4.3f,.65f,.65f));
                result.Add(new PropPlacement(103,x+mirror*.8f,y-2.9f,4.5f,.7f,.7f));
                result.Add(new PropPlacement(200,x-mirror*2.1f,y-2.4f,3f,2f,1.6f));
                result.Add(new PropPlacement(201,x+mirror*2.2f,y-.3f,2.5f,1.6f,1.4f));
                result.Add(new PropPlacement(202,x-mirror*3.3f,y+.1f,2.1f,1.4f,1.1f));
                result.Add(new PropPlacement(300,x-mirror*1.4f,y-3.2f,3.7f,0,0));
                result.Add(new PropPlacement(300,x+mirror*2.1f,y-1.7f,3.4f,0,0));
                result.Add(new PropPlacement(300,x-mirror*3.2f,y+2.3f,3.2f,0,0));
            }
        }

        public void Build(Vector2 origin,Camera camera,Stage3CampArt art,int kind)
        {
            ValidateKind(kind);
            if(camera==null||art==null||!art.IsReady)
                throw new ArgumentException("Camp presentation requires a camera and imported approved art.");
            Clear();Origin=origin;Kind=kind;view=camera;
            campRoot=new GameObject(kind==0?"Open orc camp scenery":"Open ogre ruins scenery");
            campRoot.transform.SetParent(transform,false);
            BuildGround(art.groundMaterial);
            foreach(var p in GetLayout(kind))
            {
                var sprite=art.Frame(kind,p.SpriteIndex);
                if(sprite==null)throw new InvalidOperationException("Missing camp perimeter sprite: "+p.SpriteIndex);
                var child=new GameObject("Camp prop "+p.SpriteIndex);
                child.transform.SetParent(campRoot.transform,false);
                var renderer=child.AddComponent<SpriteRenderer>();
                renderer.sprite=sprite;renderer.sharedMaterial=art.spriteMaterial;
                renderer.spriteSortPoint=SpriteSortPoint.Pivot;
                var scale=p.Width/sprite.bounds.size.x;
                child.transform.localScale=Vector3.one*scale;
                var center=Origin+p.Offset;var foot=center+Vector2.down*p.Footprint.y*.5f;
                var ground=new Vector3(foot.x,foot.y,Stage3TacticalPresentation.GrassSurfaceZ(foot));
                var padding=sprite.pivot.y/sprite.pixelsPerUnit*scale;
                var depth=padding*Mathf.Abs(camera.transform.up.z)/Mathf.Max(.1f,Mathf.Abs(camera.transform.forward.z))+.035f;
                child.transform.SetPositionAndRotation(ground-camera.transform.forward*depth,camera.transform.rotation);
                renderer.sortingOrder=-Mathf.RoundToInt(center.y*100);
                renderers.Add(renderer);
                RenderedPropCount++;
            }
        }

        private void LateUpdate()
        {
            if(view==null||campRoot==null)return;
            var hero=Shader.GetGlobalVector(HeroProperty);
            var focus=new Vector3(hero.x,hero.y,hero.z);
            foreach(var renderer in renderers)
            {
                // Existing district shader fades only a small screen-space hero aperture.
                // Enable it solely on a sprite plane actually in front of the hero.
                var inFront=hero.w>.5f&&Vector3.Dot(renderer.bounds.center-focus,view.transform.forward)<-.05f;
                renderer.color=new Color(1,1,1,inFront?.35f:1f);
            }
        }

        private void BuildGround(Material material)
        {
            // Connected, feathered foot trails retain the world's grass pigment. They
            // introduce no collider, baked backdrop or uniformly filled arena plate.
            var vertices=new List<Vector3>();var uv=new List<Vector2>();
            var colors=new List<Color>();var triangles=new List<int>();
            const float step=.5f;
            var center=Kind==0?new Vector2(14,12.5f):new Vector2(-14,12.5f);
            var radii=new Vector2(7.4f,5.5f);
            var tint=Kind==0?new Color(.57f,.43f,.24f):new Color(.51f,.46f,.34f);
            var direction=Kind==0?1f:-1f;
            var path=new[]{new Vector2(0,-19),new Vector2(-direction,-10),new Vector2(direction*2,-2),
                new Vector2(direction*5,4),new Vector2(direction*10,8),new Vector2(direction*14,12)};
            for(var y=LocalBounds.yMin;y<LocalBounds.yMax;y+=step)for(var x=LocalBounds.xMin;x<LocalBounds.xMax;x+=step)
            {
                var first=vertices.Count;
                for(var i=0;i<4;i++)
                {
                    var local=new Vector2(x+(i==1||i==2?step:0),y+(i>=2?step:0));
                    var world=Origin+local;var delta=local-center;
                    var edge=1-new Vector2(delta.x/radii.x,delta.y/radii.y).magnitude;
                    var noise=(Mathf.PerlinNoise(world.x*.6f,world.y*.6f)-.5f)*.16f;
                    var trail=float.PositiveInfinity;
                    for(var j=1;j<path.Length;j++)
                    {
                        var line=path[j]-path[j-1];var along=Mathf.Clamp01(Vector2.Dot(local-path[j-1],line)/line.sqrMagnitude);
                        trail=Mathf.Min(trail,Vector2.Distance(local,path[j-1]+line*along));
                    }
                    var alpha=Mathf.Max(Mathf.SmoothStep(0,.48f,edge+noise)*.78f,
                        (1-Mathf.SmoothStep(1.2f,3.5f,trail+noise*3f))*.72f);
                    vertices.Add(new Vector3(world.x,world.y,Stage3TacticalPresentation.GrassSurfaceZ(world)-.035f));
                    uv.Add(world*.32f);colors.Add(new Color(tint.r,tint.g,tint.b,alpha));
                }
                triangles.Add(first);triangles.Add(first+2);triangles.Add(first+1);
                triangles.Add(first);triangles.Add(first+3);triangles.Add(first+2);
            }
            groundMesh=new Mesh{name="Camp trampled ground"};
            groundMesh.SetVertices(vertices);groundMesh.SetUVs(0,uv);groundMesh.SetColors(colors);
            groundMesh.SetTriangles(triangles,0);groundMesh.RecalculateNormals();groundMesh.RecalculateBounds();
            var ground=new GameObject("Subtle ground beneath camp props");ground.transform.SetParent(campRoot.transform,false);
            ground.AddComponent<MeshFilter>().sharedMesh=groundMesh;
            var renderer=ground.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }

        private static IReadOnlyList<Rect> MakeFootprints(PropPlacement[] layout)
        {
            var rectangles=new List<Rect>();
            for(var i=0;i<layout.Length;i++)
            {
                var rect=layout[i].GroundRect;
                if(rect.width<=0||rect.height<=0)continue;
                var nearest=new Vector2(Mathf.Clamp(0f,rect.xMin,rect.xMax),Mathf.Clamp(0f,rect.yMin,rect.yMax));
                if(!LocalBounds.Contains(rect.min)||!LocalBounds.Contains(rect.max)||
                    nearest.sqrMagnitude<CombatClearRadius*CombatClearRadius||
                    rect.Overlaps(new Rect(-22,-3,44,6))||rect.Overlaps(new Rect(-3,-20,6,20)))
                    throw new InvalidOperationException("Camp prop encroaches on its reserved open field or escape corridor.");
                rectangles.Add(rect);
            }
            return rectangles.AsReadOnly();
        }
        private static int ValidateKind(int kind)
        {if(kind<0||kind>1)throw new ArgumentOutOfRangeException(nameof(kind));return kind;}
        public void Clear()
        {
            if(campRoot!=null){campRoot.SetActive(false);Destroy(campRoot);campRoot=null;}
            if(groundMesh!=null){Destroy(groundMesh);groundMesh=null;}
            RenderedPropCount=0;
            renderers.Clear();view=null;
        }
        private void OnDestroy()=>Clear();
    }
}
