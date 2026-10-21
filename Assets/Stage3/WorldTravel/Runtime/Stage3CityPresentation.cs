using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Modular approved city artwork. Site, collision, discovery and services belong to world integration.</summary>
    public sealed class Stage3CityPresentation : MonoBehaviour
    {
        private readonly struct Placement
        {
            public readonly int ArtIndex;
            public readonly Vector2 Offset, Footprint;
            public readonly float Width;
            public Placement(int artIndex, float x, float y, float width, float footprintX, float footprintY)
            { ArtIndex=artIndex; Offset=new Vector2(x,y); Width=width; Footprint=new Vector2(footprintX,footprintY); }
        }

        // -1 shop, -2 temple, -3 guild, -4 notice board; otherwise an isolated sprite from the twelve-piece kit.
        // One layout owns both the visible bases and the footprint data consumed by integration.
        private static readonly Placement[] Layout =
        {
            // Civic composition follows city_v4_open_temple: guild left, modest temple
            // right, well in the foreground. The blue shop anchors the upper east lane.
            new Placement(-1,14,11,8f,6f,3f),
            new Placement(-2,9.2f,1.2f,5.6f,4.4f,3f),
            new Placement(-3,-12,-1,11.8f,8.8f,5.2f),
            new Placement(-4,-4.6f,-3.8f,1.05f,.84f,.3f),
            // Staggered residential blocks frame the civic space rather than divide it.
            new Placement(0,-18.3f,15.6f,5.2f,3.7f,2.8f),
            new Placement(2,-12,17,5.4f,3.9f,2.9f),
            new Placement(1,-4.2f,13.6f,5.2f,3.7f,2.8f),
            new Placement(0,2,17.2f,5.2f,3.7f,2.8f),
            new Placement(2,9.4f,15.8f,5.3f,3.8f,2.9f),
            new Placement(1,19.3f,16.7f,5.2f,3.7f,2.8f),
            new Placement(1,-18,6.5f,5f,3.6f,2.7f),
            new Placement(0,-9.8f,10.8f,5f,3.6f,2.7f),
            new Placement(2,.6f,7.9f,5.4f,3.9f,2.9f),
            new Placement(0,19.5f,5.4f,4.7f,3.4f,2.6f),
            new Placement(0,-18,-12.8f,5.2f,3.7f,2.8f),
            new Placement(2,-10.8f,-14.2f,5.4f,3.9f,2.9f),
            new Placement(1,10.5f,-14.1f,5.2f,3.7f,2.8f),
            new Placement(0,18,-11.5f,5.2f,3.7f,2.8f),
            new Placement(4,0,-8,3.4f,2f,2f),
            new Placement(5,-3.1f,-9.8f,1.8f,1.6f,.55f),
            new Placement(5,4.1f,-10.1f,1.8f,1.6f,.55f),
            new Placement(5,13.4f,-5.2f,1.7f,1.5f,.55f),
            new Placement(7,-16.9f,-3.9f,1.5f,1f,1f),
            new Placement(7,-6.2f,-3.7f,1.4f,1f,1f),
            new Placement(7,-2.3f,-7.1f,1.25f,.85f,.85f),
            new Placement(7,2.2f,-7.4f,1.25f,.85f,.85f),
            new Placement(7,17.4f,8.5f,1.4f,1f,1f),
            new Placement(7,12.2f,-.4f,1.3f,.9f,.9f),
            new Placement(8,14,7.5f,.68f,0,0),
            new Placement(9,-6,-6,.65f,0,0),
            new Placement(10,4,-3,.62f,0,0),
            new Placement(9,-12,10.8f,.65f,0,0),
            new Placement(10,8,8,.62f,0,0)
        };
        // Continuous planted edges define the blocks. The two public approaches and
        // the whole front of the temple court remain open: no fence across a service.
        private static readonly Placement[] Gardens =
        {
            new Placement(0,-21,16.7f,4.2f,2f,1.5f),new Placement(1,-17,17.7f,4.3f,2.4f,1.2f),
            new Placement(0,-14.4f,15.9f,3.6f,2f,1.5f),new Placement(1,-10.7f,18.2f,4.3f,2.4f,1.2f),
            new Placement(0,-6.4f,17.5f,3.9f,2.1f,1.5f),new Placement(1,-3.3f,17.4f,4.2f,2.4f,1.2f),
            new Placement(0,.3f,17.5f,3.7f,2f,1.5f),new Placement(1,4.1f,18,4.2f,2.4f,1.2f),
            new Placement(0,7.4f,16.5f,3.8f,2.1f,1.5f),new Placement(1,14.6f,18,4.1f,2.3f,1.2f),
            new Placement(0,21,16.5f,3.6f,1.7f,1.4f),new Placement(0,-21,11.3f,4f,1.8f,1.5f),
            new Placement(1,-16.9f,10.2f,3.3f,1.8f,1.1f),new Placement(0,-13.1f,8.1f,3.6f,1.9f,1.5f),
            new Placement(1,-8.4f,8.2f,4f,2.2f,1.2f),new Placement(0,-5,8.2f,3.6f,1.9f,1.5f),
            new Placement(1,-2.1f,10.8f,3.8f,2.1f,1.2f),new Placement(0,4.6f,9,4f,2.1f,1.5f),
            new Placement(1,1.3f,4.7f,4.8f,2.8f,1.1f),new Placement(0,5.1f,4.3f,3.6f,1.9f,1.5f),
            new Placement(0,9.5f,11.5f,3.7f,2f,1.5f),new Placement(1,19.3f,11.9f,3.6f,2f,1.1f),
            new Placement(0,21.2f,9.6f,3.6f,1.6f,1.5f),new Placement(1,19.8f,4.7f,3.7f,2f,1.2f),
            // Garden behind/beside the civic buildings; their fronts have clear paving.
            new Placement(0,-20.3f,2.2f,4.3f,2.1f,1.6f),new Placement(0,-18.7f,-1.7f,3.9f,1.9f,1.5f),
            new Placement(1,-10.1f,3.4f,4.1f,2.3f,1.2f),new Placement(0,-5.9f,2.6f,3.9f,2f,1.5f),
            new Placement(1,-4.2f,.3f,3.8f,2.1f,1.2f),
            new Placement(0,5.2f,2,4.5f,2.4f,1.7f),new Placement(1,7.4f,4.2f,3.7f,2f,1.1f),
            new Placement(0,14.2f,1.4f,4.4f,2.3f,1.6f),new Placement(1,17.1f,-.8f,4.2f,2.2f,1.2f),
            new Placement(0,16.9f,-4.9f,4.1f,2f,1.6f),
            // Foreground planted crescents leave the entrance and plaza unobstructed.
            new Placement(0,-19.8f,-7.1f,4.1f,2f,1.5f),new Placement(1,-16.2f,-8.7f,4.4f,2.5f,1.2f),
            new Placement(0,-12.8f,-9.8f,4.2f,2.2f,1.6f),new Placement(1,-9.1f,-10.4f,4f,2.3f,1.2f),
            new Placement(0,-6.2f,-12.2f,4.1f,2.1f,1.5f),new Placement(1,-4.5f,-14.8f,3.6f,1.8f,1.1f),
            new Placement(0,-21,-14.8f,3.6f,1.7f,1.4f),new Placement(1,-15.1f,-15.7f,3.9f,2.2f,1.1f),
            new Placement(0,-7.8f,-15.7f,3.7f,1.8f,1.4f),
            new Placement(0,6.7f,-12.8f,4f,2.1f,1.5f),new Placement(1,10.4f,-10.7f,4.5f,2.5f,1.2f),
            new Placement(0,14.1f,-9.3f,4.2f,2.2f,1.5f),new Placement(1,18.1f,-8.4f,3.9f,2.1f,1.1f),
            new Placement(0,20.9f,-12.9f,3.8f,1.7f,1.4f),new Placement(1,14.5f,-15.5f,4f,2.3f,1.1f),
            new Placement(0,7.4f,-16.1f,3.6f,1.8f,1.3f),
            // A few short rear/side residential boundaries, never a front public barrier.
            new Placement(2,-17,17.6f,3.3f,2.4f,.5f),new Placement(2,-3.2f,17.2f,3.3f,2.4f,.5f),
            new Placement(3,-14.9f,9.9f,2.8f,.5f,1.9f),new Placement(2,1.5f,4.8f,3.5f,2.6f,.5f),
            new Placement(3,21,5.6f,2.8f,.5f,1.9f),new Placement(2,-13.2f,-15.4f,3.4f,2.5f,.5f),
            new Placement(2,14,-15.3f,3.4f,2.5f,.5f),
            new Placement(4,-4,-5.3f,.75f,.35f,.35f),new Placement(4,-7,-11.2f,.75f,.35f,.35f),
            new Placement(4,5.4f,-11.8f,.75f,.35f,.35f),new Placement(4,14.9f,-7.1f,.75f,.35f,.35f),
            new Placement(4,-13.4f,5.3f,.75f,.35f,.35f),new Placement(4,5.9f,7,.75f,.35f,.35f),
            new Placement(4,18.2f,6.1f,.75f,.35f,.35f),
            new Placement(5,-18.4f,17.5f,4.1f,0,0),new Placement(5,-6.4f,17.1f,3.8f,0,0),
            new Placement(5,5.6f,17.4f,3.8f,0,0),new Placement(5,19.4f,16.6f,3.8f,0,0),
            new Placement(5,-19.5f,10.7f,3.8f,0,0),new Placement(5,-11.7f,8.9f,3.4f,0,0),
            new Placement(5,-5.6f,9.2f,3.6f,0,0),new Placement(5,3.5f,5,4f,0,0),
            new Placement(5,19.8f,4.8f,3.7f,0,0),new Placement(5,-19,1.8f,3.8f,0,0),
            new Placement(5,-5.4f,1.4f,3.6f,0,0),new Placement(5,5.7f,2.5f,4f,0,0),
            new Placement(5,15.5f,-.9f,3.9f,0,0),new Placement(5,16.3f,-4.5f,3.8f,0,0),
            new Placement(5,-17.3f,-8.1f,4.1f,0,0),new Placement(5,-10.9f,-10,4.1f,0,0),
            new Placement(5,-5.5f,-13.5f,3.6f,0,0),new Placement(5,-18.6f,-15.2f,3.7f,0,0),
            new Placement(5,8.3f,-12.1f,3.8f,0,0),new Placement(5,15.5f,-9,4f,0,0),
            new Placement(5,18.7f,-14.2f,3.6f,0,0)
        };
        private static readonly Vector2[] GardenTrees =
        {
            new Vector2(-21,17),new Vector2(-14.6f,17.5f),new Vector2(-6.5f,18),
            new Vector2(.1f,18),new Vector2(7.6f,18),new Vector2(15,18),new Vector2(21,17),
            new Vector2(-21,12),new Vector2(-14.8f,8.5f),new Vector2(-5.4f,10),
            new Vector2(5,10.5f),new Vector2(8.8f,14),new Vector2(21,12.8f),
            new Vector2(-21,3.1f),new Vector2(-19.5f,-1.3f),new Vector2(-5.1f,2.8f),
            new Vector2(4.8f,3.8f),new Vector2(15.2f,3.5f),new Vector2(19,-1.5f),
            new Vector2(-20.7f,-7),new Vector2(-13.5f,-9.4f),new Vector2(-6.7f,-12.4f),
            new Vector2(-20,-15.8f),new Vector2(-14,-16),new Vector2(-7,-16),
            new Vector2(6.7f,-15.9f),new Vector2(11.7f,-10.1f),new Vector2(20.7f,-8.2f),
            new Vector2(20.5f,-15),new Vector2(14.5f,-16)
        };
        private static readonly Vector2[][] Streets =
        {
            // One principal route bends past the civic plaza into the upper quarter.
            new[]{new Vector2(0,-17),new Vector2(0,-13),new Vector2(-2,-6),new Vector2(-3.6f,-1),new Vector2(-4.6f,4),new Vector2(-6.6f,10),new Vector2(-8,19)},
            new[]{new Vector2(-19,-5.2f),new Vector2(-11,-5.4f),new Vector2(-3,-6),new Vector2(4,-5.6f),new Vector2(7,-4.2f)},
            new[]{new Vector2(-20,3.7f),new Vector2(-16,5.1f),new Vector2(-15,10),new Vector2(-12,12.3f),new Vector2(-7,12.6f)},
            new[]{new Vector2(-7,12.6f),new Vector2(-1,12.1f),new Vector2(6,13.5f),new Vector2(13,14.2f),new Vector2(21,14.4f)},
            new[]{new Vector2(-3.6f,-1),new Vector2(-1.2f,2.4f),new Vector2(4,1.7f),new Vector2(7,5.8f),new Vector2(14,7.5f),new Vector2(21,5)},
            new[]{new Vector2(7,5.8f),new Vector2(7.1f,10),new Vector2(6,13.5f),new Vector2(6,18.5f)},
            new[]{new Vector2(-20,-15.4f),new Vector2(-16,-16.2f),new Vector2(-10,-16),new Vector2(-4,-16),new Vector2(0,-15)},
            new[]{new Vector2(0,-15),new Vector2(5,-16),new Vector2(11,-16.1f),new Vector2(17,-14.6f),new Vector2(21,-14)},
            new[]{new Vector2(13.6f,-6.5f),new Vector2(17,-7),new Vector2(21,-10),new Vector2(21,-14)}
        };
        private static readonly Rect[] Footprints = CreateFootprints();
        private static readonly IReadOnlyList<Rect> ReadOnlyFootprints = Array.AsReadOnly(Footprints);
        public static Rect LocalBounds => new Rect(-22,-17,44,36);
        public static Vector2 PortalOffset => new Vector2(7.4f,-2.6f);
        // Decorative board only; access belongs to the guild entrance.
        public static Vector2 GuildBoardOffset => new Vector2(-4.6f,-3.8f);
        public static Vector2 GuildInteractionOffset => new Vector2(-10.2f,-4.6f);
        public static Vector2 MerchantOffset => new Vector2(14,7.5f);
        public static Vector2 EntranceOffset => new Vector2(0,-16);
        public static IReadOnlyList<Rect> LocalFootprints => ReadOnlyFootprints;
        public Vector2 Origin { get; private set; }
        public Vector2 PortalPosition => Origin + PortalOffset;
        public Vector2 MerchantPosition => Origin + MerchantOffset;
        public Vector2 GuildBoardPosition => Origin + GuildBoardOffset;
        public Vector2 GuildInteractionPosition => Origin + GuildInteractionOffset;
        public Rect GuildFootprint
        {
            get
            {
                foreach(var p in Layout)
                    if(p.ArtIndex==-3)return new Rect(Origin+p.Offset-p.Footprint*.5f,p.Footprint);
                return Rect.zero;
            }
        }
        public bool IsBuilt => cityRoot != null;
        public SpriteRenderer ShopVisual { get; private set; }
        public SpriteRenderer TempleVisual { get; private set; }
        public SpriteRenderer GuildVisual { get; private set; }
        public SpriteRenderer GuildBoardVisual { get; private set; }
        private GameObject cityRoot;
        private Mesh pavingMesh;

        private static Rect[] CreateFootprints()
        {
            var result=new List<Rect>();
            foreach(var p in Layout)
                if(p.Footprint.x>0f) result.Add(new Rect(p.Offset-p.Footprint*.5f,p.Footprint));
            foreach(var p in Gardens)
                if(p.Footprint.x>0f)result.Add(new Rect(p.Offset-p.Footprint*.5f,p.Footprint));
            foreach(var point in GardenTrees)result.Add(new Rect(point-Vector2.one*.32f,Vector2.one*.64f));
            return result.ToArray();
        }

        public void Build(Vector2 origin, Camera camera, Stage3CityArt art)
        {
            if(camera==null || art==null || !art.IsReady)
                throw new ArgumentException("City presentation requires camera and imported modular city art.");
            Clear(); Origin=origin;
            cityRoot=new GameObject("City A — modular presentation");
            cityRoot.transform.SetParent(transform,false);
            BuildPaving(art.pavingMaterial);
            foreach(var p in Layout)
            {
                var sprite=p.ArtIndex==-1?art.shop:p.ArtIndex==-2?art.temple:p.ArtIndex==-3?art.guild:p.ArtIndex==-4?art.noticeBoard:art.kit[p.ArtIndex];
                if(sprite==null)continue;
                var renderer=CreateSprite(p.ArtIndex==-1?"Trading house":p.ArtIndex==-2?"Quiet temple":p.ArtIndex==-3?"Adventurers guild":p.ArtIndex==-4?"Guild notice board":"City piece "+p.ArtIndex,
                    sprite,p.ArtIndex==-1||p.ArtIndex==-3?art.shopMaterial:art.spriteMaterial,p,origin,camera);
                if(p.ArtIndex==-1)ShopVisual=renderer;
                if(p.ArtIndex==-2)TempleVisual=renderer;
                if(p.ArtIndex==-3)GuildVisual=renderer;
                if(p.ArtIndex==-4)
                {
                    GuildBoardVisual=renderer;
                    renderer.color=new Color(.78f,.74f,.65f,1f); // Quiet decoration, never a service signal.
                }
            }
            if(art.gardens!=null && art.gardens.Length==6)
                foreach(var p in Gardens)
                {
                    var sprite=art.gardens[p.ArtIndex];if(sprite==null)continue;
                    CreateSprite("City garden mass "+p.ArtIndex,sprite,art.spriteMaterial,p,origin,camera);
                }
            if(art.gardenTrees!=null && art.gardenTrees.Length>0)
                for(var i=0;i<GardenTrees.Length;i++)
                {
                    var tree=art.gardenTrees[i%art.gardenTrees.Length];if(tree==null)continue;
                    var p=GardenTrees[i];
                    CreateSprite("City garden tree",tree,art.spriteMaterial,
                        new Placement(0,p.x,p.y,4.3f+(i%3)*.3f,.64f,.64f),origin,camera);
                }
            if(ShopVisual!=null && art.kit[11]!=null)
            {
                var badge=new GameObject("Merchant role — scales").AddComponent<SpriteRenderer>();
                badge.transform.SetParent(cityRoot.transform,false);
                badge.sprite=art.kit[11];badge.sharedMaterial=art.spriteMaterial;
                var width=1.05f;
                badge.transform.localScale=Vector3.one*(width/badge.sprite.bounds.size.x);
                var shopTop=ShopVisual.transform.TransformPoint(new Vector3(ShopVisual.sprite.bounds.center.x,
                    ShopVisual.sprite.bounds.max.y,0));
                badge.transform.SetPositionAndRotation(shopTop+camera.transform.up*.35f-camera.transform.forward*.04f,camera.transform.rotation);
                badge.sortingOrder=ShopVisual.sortingOrder+1;
            }
        }

        private SpriteRenderer CreateSprite(string label, Sprite sprite, Material material, Placement p, Vector2 origin, Camera camera)
        {
            var obj=new GameObject(label);obj.transform.SetParent(cityRoot.transform,false);
            var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;
            var scale=p.Width/sprite.bounds.size.x;
            obj.transform.localScale=Vector3.one*scale;
            var position=origin+p.Offset;
            var foot=position+Vector2.down*p.Footprint.y*.5f;
            var ground=new Vector3(foot.x,foot.y,Stage3TacticalPresentation.GrassSurfaceZ(foot));
            var padding=sprite.pivot.y/sprite.pixelsPerUnit*scale;
            var depth=padding*Mathf.Abs(camera.transform.up.z)/Mathf.Max(.1f,Mathf.Abs(camera.transform.forward.z))+.035f;
            obj.transform.SetPositionAndRotation(ground-camera.transform.forward*depth,camera.transform.rotation);
            renderer.sortingOrder=-Mathf.RoundToInt(position.y*100f);
            return renderer;
        }

        private void BuildPaving(Material material)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var triangles=new List<int>();
            // One shared-UV surface, with curved courts and narrow feathered turf margins.
            // No rectangular pavement cut-outs or overlapping street junctions.
            const float step=.4f;
            for(var y=LocalBounds.yMin;y<LocalBounds.yMax;y+=step)for(var x=LocalBounds.xMin;x<LocalBounds.xMax;x+=step)
            {
                if(PavingDistance(new Vector2(x+step*.5f,y+step*.5f))<-.8f)continue;
                var first=vertices.Count;
                for(var corner=0;corner<4;corner++)
                {
                    var local=new Vector2(x+(corner==1||corner==2?step:0),y+(corner>=2?step:0));
                    var world=Origin+local;
                    vertices.Add(new Vector3(world.x,world.y,Stage3TacticalPresentation.GrassSurfaceZ(world)-.018f));
                    uv.Add(local*.16f);
                    var organic=Mathf.Sin(local.x*3.7f+local.y*1.4f)*.08f+Mathf.Sin(local.y*5.3f-local.x)*.05f;
                    colors.Add(new Color(1,1,1,Mathf.SmoothStep(0,1,(PavingDistance(local)+organic+.20f)/.48f)));
                }
                triangles.Add(first);triangles.Add(first+2);triangles.Add(first+1);
                triangles.Add(first);triangles.Add(first+3);triangles.Add(first+2);
            }
            pavingMesh=new Mesh{name="City connected paving"};
            pavingMesh.SetVertices(vertices);pavingMesh.SetUVs(0,uv);pavingMesh.SetColors(colors);pavingMesh.SetTriangles(triangles,0);pavingMesh.RecalculateNormals();pavingMesh.RecalculateBounds();
            var paving=new GameObject("Walkable streets and temple court");paving.transform.SetParent(cityRoot.transform,false);
            paving.AddComponent<MeshFilter>().sharedMesh=pavingMesh;
            var renderer=paving.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }

        private static float PavingDistance(Vector2 point)
        {
            // Broad connected civic floor, with the portal in a distinct side court.
            // No transit street crosses the seal: the temple branch ends at the court.
            var distance=EllipseDistance(point,new Vector2(0,-7),new Vector2(8.8f,5.4f));
            distance=Mathf.Max(distance,EllipseDistance(point,new Vector2(0,-2.8f),new Vector2(9.6f,4.4f)));
            distance=Mathf.Max(distance,EllipseDistance(point,new Vector2(-10.7f,-5.2f),new Vector2(4.8f,3.8f)));
            distance=Mathf.Max(distance,EllipseDistance(point,new Vector2(8.8f,-3.4f),new Vector2(6.8f,4.5f)));
            distance=Mathf.Max(distance,EllipseDistance(point,new Vector2(14,7.8f),new Vector2(4.1f,2.4f)));
            for(var i=0;i<Streets.Length;i++)
                for(var j=1;j<Streets[i].Length;j++)
                    distance=Mathf.Max(distance,(i==0?1.6f:i==1?1.8f:i==4?1.35f:1.05f)-
                        Vector2.Distance(point,ClosestOnSegment(point,Streets[i][j-1],Streets[i][j])));
            // Narrow individual front paths, not continuous pads joining every house into a plaza.
            foreach(var house in Layout)
            {
                if(house.ArtIndex<0||house.ArtIndex>3)continue;
                var threshold=house.Offset+Vector2.down*(house.Footprint.y*.5f+.3f);
                var nearest=threshold;var best=float.PositiveInfinity;
                foreach(var street in Streets)for(var j=1;j<street.Length;j++)
                {
                    var candidate=ClosestOnSegment(threshold,street[j-1],street[j]);
                    var delta=(candidate-threshold).sqrMagnitude;
                    if(delta<best){best=delta;nearest=candidate;}
                }
                distance=Mathf.Max(distance,.85f-Vector2.Distance(point,ClosestOnSegment(point,threshold,nearest)));
            }
            // Turf reaches underneath the large foliage, with soft edges rather than tiny pot holes.
            foreach(var garden in Gardens)
            {
                if(garden.ArtIndex>1)continue;
                var radii=new Vector2(garden.Width*.46f,Mathf.Max(1f,garden.Footprint.y*.8f));
                distance=Mathf.Min(distance,-EllipseDistance(point,garden.Offset,radii));
            }
            return distance;
        }

        private static float EllipseDistance(Vector2 point,Vector2 center,Vector2 radii)
        {
            var offset=point-center;
            return (1-new Vector2(offset.x/radii.x,offset.y/radii.y).magnitude)*Mathf.Min(radii.x,radii.y);
        }
        private static Vector2 ClosestOnSegment(Vector2 point,Vector2 a,Vector2 b)
        {
            var ab=b-a;if(ab.sqrMagnitude<.00001f)return a;
            return a+ab*Mathf.Clamp01(Vector2.Dot(point-a,ab)/ab.sqrMagnitude);
        }

        public void Clear()
        {
            if(cityRoot!=null){cityRoot.SetActive(false);Destroy(cityRoot);cityRoot=null;}
            if(pavingMesh!=null){Destroy(pavingMesh);pavingMesh=null;}
            ShopVisual=null;TempleVisual=null;GuildVisual=null;GuildBoardVisual=null;
        }
        private void OnDestroy()=>Clear();
    }
}
