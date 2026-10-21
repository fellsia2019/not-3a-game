using Not3A.Stage2.Integration;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Approved versatile adventurer C. The removable bow is current equipment, not a class.</summary>
    public sealed class Stage3HeroAvatar : MonoBehaviour
    {
        private Stage3HeroArt art;
        private Stage2GameComposition game;
        private Camera view;
        private SpriteRenderer body, bow;
        private Vector3 previous;
        private Stage3ForgeArt equipmentArt;
        private readonly Sprite[] equipmentSprites=new Sprite[24];
        private int pickaxeQuality=-1,axeQuality=-1;
        public int DisplayedEquipmentType { get; private set; }=-1;
        public int DisplayedEquipmentQuality { get; private set; }=-1;
        public Vector3 EquipmentGripWorldPosition=>bow!=null?bow.transform.position:transform.position;
        public bool UsesDetachedHarvestTool { get; private set; }
        public void SetHarvestEquipmentVisual(int pickaxeQualityOrMinus1,int axeQualityOrMinus1)
        {
            pickaxeQuality=Mathf.Clamp(pickaxeQualityOrMinus1,-1,7);axeQuality=Mathf.Clamp(axeQualityOrMinus1,-1,7);
            if(body!=null)Apply();
        }
        private int equipmentType=-2,equipmentQuality;
        public void SetEquipmentVisual(Stage3ForgeArt artwork,int typeIndex,int qualityIndex)
        {
            if(equipmentArt!=artwork)
            {
                ReleaseEquipmentSprites();equipmentArt=artwork;
            }
            equipmentType=typeIndex;equipmentQuality=Mathf.Clamp(qualityIndex,0,7);
            if(body!=null)Apply();
        }
        private Sprite EquipmentSprite(int type,int quality)
        {
            // Relaxed front-facing hands carry the bow upright beside the body.
            if(type==0)return equipmentArt?.heldEquipment?.Bow(quality,direction==3?0:direction)??art.bowDirections[direction];
            if(quality<0||equipmentArt==null||type!=1&&type!=6&&type!=7)return null;
            var index=(type==1?0:type==6?8:16)+quality;
            if(equipmentSprites[index]!=null)return equipmentSprites[index];
            if(!equipmentArt.TryGetIcon(type,quality,out var texture,out var uv))return null;
            var rect=new Rect(uv.x*texture.width,uv.y*texture.height,uv.width*texture.width,uv.height*texture.height);
            var pivot=new Vector2(.35f,.35f);var ppu=rect.height/.68f;
            if(type==6)
            {
                pivot=new Vector2(.76f,.36f);ppu=100f;
            }
            else if(type==7)
            {
                var grip=new Vector2((51f+quality*255f)*texture.width/2048f,(768f-608f)*texture.height/768f);
                pivot=new Vector2((grip.x-rect.x)/rect.width,(grip.y-rect.y)/rect.height);ppu=100f;
            }
            var sprite=Sprite.Create(texture,rect,pivot,ppu,0,SpriteMeshType.FullRect);
            if(type==7)
            {
                var vertices=new Vector2[8];var triangles=new ushort[]{0,1,2,0,2,3,4,5,6,4,6,7};
                for(var band=0;band<2;band++)
                {
                    var part=equipmentArt.AxeIconBand(quality,band);
                    var lo=new Vector2(part.xMin*texture.width,part.yMin*texture.height);
                    var hi=new Vector2(part.xMax*texture.width,part.yMax*texture.height);
                    // Sprite.OverrideGeometry takes rect-local pixels; Unity applies pivot/PPU.
                    var origin=rect.min;
                    vertices[band*4]=lo-origin;
                    vertices[band*4+1]=new Vector2(hi.x,lo.y)-origin;
                    vertices[band*4+2]=hi-origin;
                    vertices[band*4+3]=new Vector2(lo.x,hi.y)-origin;
                }
                sprite.OverrideGeometry(vertices,triangles);
            }
            sprite.name="Equipped "+type+" quality "+quality;sprite.hideFlags=HideFlags.DontSave;
            equipmentSprites[index]=sprite;return sprite;
        }
        private void ReleaseEquipmentSprites()
        {
            for(var i=0;i<equipmentSprites.Length;i++)if(equipmentSprites[i]!=null){Destroy(equipmentSprites[i]);equipmentSprites[i]=null;}
        }
        private float stride, shotPose, idleSeconds;
        private int direction = 3;
        private Stage2Economy economy;
        private Stage2ResourceNode workNode, lastHitNode;
        private float contactSeconds;
        public SpriteRenderer Body => body;
        public SpriteRenderer Equipment => bow;
        public Vector3 ReleasePosition => bow != null ? bow.transform.position : transform.position + Vector3.back*.43f;
        public int Direction => direction;
        public int Frame { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsHarvesting => workNode != null && art != null && art.HasWork;
        public MapResourceKind? EquippedHarvestResource => IsHarvesting ? workNode.Resource : (MapResourceKind?)null;
        public int HarvestFrame { get; private set; }
        public int ConfirmedStrikes { get; private set; }
        public int WalkFrameCount => art != null ? art.WalkFrameCount : 0;

        public void Initialize(Stage3HeroArt artwork, Stage2GameComposition owner, Camera camera)
        {
            art = artwork; game = owner; view = camera; previous = transform.position;
            body = Part("Hero C body"); bow = Part("Starter bow equipment");
            game.Enemies.ShotLaunched += OnShot;
            game.RunDriver.Run.Reset += ResetVisual;
            game.RunDriver.Run.Rewound += ResetVisual;
            BindEconomy();
            Apply();
        }

        private SpriteRenderer Part(string name)
        {
            var child = new GameObject(name); child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sharedMaterial = art.material;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot; return renderer;
        }

        private int Facing(Vector3 delta)
        {
            var x = Vector3.Dot(delta,view.transform.right); var y = Vector3.Dot(delta,view.transform.up);
            return Mathf.Abs(x) >= Mathf.Abs(y) ? x >= 0f ? 0 : 2 : y >= 0f ? 1 : 3;
        }

        private void OnShot(Stage2Shot shot) => NotifyExplorationShot(shot.Origin,shot.Position);

        // Presentation of an already-confirmed shot; targeting, range, damage and projectile facts remain in combat rules.
        public void NotifyExplorationShot(Vector2 origin,Vector2 target)
        {
            if (art == null || view == null || game == null || Vector2.Distance(origin,transform.position) > .01f) return;
            if (workNode != null || game.Gathering?.GatheringNode != null) return;
            direction = Facing((Vector3)target-transform.position); shotPose = .24f;
            Apply();
        }

        private void LateUpdate()
        {
            if (art == null || game == null || view == null) return;
            BindEconomy();
            var delta = transform.position-previous; previous = transform.position;
            var run = game.RunDriver.Run;
            if (!run.IsPaused && !run.IsTerminal)
            {
                shotPose = Mathf.Max(0f,shotPose-Time.deltaTime);
                var distance = ((Vector2)delta).magnitude;
                if (distance > .00001f && distance < 2f)
                {
                    IsMoving = true; idleSeconds = 0f;
                    shotPose = 0f; direction = Facing(delta);
                    stride = Mathf.Repeat(stride+distance/art.strideCells,1f);
                    Frame = Mathf.FloorToInt(stride*art.WalkFrameCount);
                }
                else
                {
                    IsMoving = false; Frame = 0; stride = 0f;
                    idleSeconds += Time.deltaTime;
                }
                var gathering = game.Gathering.GatheringNode;
                // A confirmed grant owns the contact pose, including the final depleted unit.
                // This adapter observes economy facts; it never grants a unit or drives gathering.
                contactSeconds = Mathf.Max(0f, contactSeconds-Time.deltaTime);
                workNode = !IsMoving && art.HasWork ? gathering ?? (contactSeconds > 0f ? lastHitNode : null) : null;
                if (workNode != null)
                {
                    direction = Facing((Vector3)workNode.WorldPosition-transform.position);
                    if (contactSeconds > .10f) HarvestFrame = 3;
                    else if (contactSeconds > .05f) HarvestFrame = 4;
                    else if (contactSeconds > 0f) HarvestFrame = 5;
                    else
                    {
                        var progress = Mathf.Clamp01(economy.GatherProgress/economy.GatherDuration(workNode));
                        HarvestFrame = progress < .2f ? 0 : progress < .62f ? 1 : 2;
                    }
                    shotPose = 0f;
                }
            }
            else if (run.IsTerminal) { workNode = null; contactSeconds = 0f; }
            Apply();
        }

        private void BindEconomy()
        {
            var current = game.Gathering?.Economy;
            if (current == economy) return;
            if (economy != null) economy.NodeGathered -= OnGathered;
            economy = current;
            if (economy != null) economy.NodeGathered += OnGathered;
        }

        private void OnGathered(Stage2ResourceNode node)
        {
            if (game.Gathering.GatheringNode != node) return;
            lastHitNode = node; contactSeconds = .18f; ConfirmedStrikes++;
        }

        private void ResetVisual()
        {
            workNode = null; lastHitNode = null;
            contactSeconds = 0f; shotPose = 0f; stride = 0f; idleSeconds = 0f;
            ConfirmedStrikes = 0; HarvestFrame = 0; Frame = 0; IsMoving = false;
            previous = transform.position;
            Apply();
        }

        private void Apply()
        {
            if (art == null || !art.IsValid || view == null) return;
            var working = IsHarvesting;
            var idle = !working && !IsMoving && art.HasIdle;
            var axe=working&&workNode.Resource==MapResourceKind.Wood;
            Sprite cleanBody=null;Stage3ToolGripPose toolPose=default;
            var detached=working&&equipmentArt!=null&&equipmentArt.heldEquipment!=null&&
                equipmentArt.heldEquipment.TryWork(axe,direction,HarvestFrame,out cleanBody,out toolPose);
            body.sprite = detached?cleanBody:working ? art.Work(axe, direction, HarvestFrame) :
                idle ? art.idleDirections[direction] : art.Walk(direction, Frame);
            body.flipX = direction == 2 && (working ? art.mirrorLeftWork :
                !idle && art.WalkFrameCount == 8 && art.mirrorLeftWalk);
            var shownType=detached?(axe?7:6):equipmentType==-2?0:equipmentType;
            var shownQuality=detached?(axe?axeQuality:pickaxeQuality):equipmentQuality;
            bow.sprite=EquipmentSprite(shownType,shownQuality);
            // Clean work bodies retain both hands; the separate tool sits behind their fingers.
            bow.enabled=(!working||detached)&&bow.sprite!=null;
            bow.flipX=detached?direction==2:!working&&
                (direction==2&&(equipmentType==1||equipmentArt?.heldEquipment?.HasBows==true)||
                 direction==3&&shownType==0&&equipmentArt?.heldEquipment?.HasBows==true);
            UsesDetachedHarvestTool=detached&&bow.enabled;
            DisplayedEquipmentType=bow.enabled?shownType:-1;DisplayedEquipmentQuality=bow.enabled?shownQuality:-1;
            var breath = idle ? Mathf.Sin(idleSeconds*Mathf.PI*2f/Mathf.Max(.5f,art.idlePeriodSeconds)) : 0f;
            // A neutral planted-foot sprite breathes around its sole pivot; it never cycles walking feet in place.
            body.transform.localScale = new Vector3(1f,1f+breath*art.idleBreathScale,1f);
            var world = transform.position;
            var map = game.GetComponent<Stage2SceneBootstrap>().MapDefinition;
            var cell = map.WorldToCell(world);
            world.z = map.Contains(cell) && map.GetSurface(cell) == Not3A.Stage2.Map.MapSurfaceKind.Forest ?
                Stage3TacticalPresentation.GrassSurfaceZ(world) : 0f;
            var padding = body.sprite.pivot.y/body.sprite.pixelsPerUnit;
            var clearance = padding*Mathf.Abs(view.transform.up.z)/Mathf.Max(.1f,Mathf.Abs(view.transform.forward.z))+.025f;
            body.transform.SetPositionAndRotation(world-view.transform.forward*clearance,view.transform.rotation);
            var grip = art.WalkGrip(direction, Frame);
            if (idle) grip = art.idleGripOffsets[direction];
            var angle=shotPose>0f?Mathf.Sin(shotPose/.24f*Mathf.PI)*-5f:breath*.6f;
            bow.transform.localScale=Vector3.one;
            if(detached)
            {
                grip=toolPose.grip;
                var sourceAngle=axe?63f:125f;
                if(direction==2)sourceAngle=180f-sourceAngle;
                var sourceDistance=axe?1.76f:1.37f;
                angle=toolPose.angleDegrees-sourceAngle;
                bow.transform.localScale=Vector3.one*(toolPose.headDistance/sourceDistance);
            }
            bow.transform.SetPositionAndRotation(body.transform.TransformPoint(grip)-view.transform.forward*.005f,
                view.transform.rotation*Quaternion.Euler(0f,0f,angle));
            body.sortingOrder = -Mathf.RoundToInt(world.y*100f)+1;
            bow.sortingOrder = body.sortingOrder+(detached||direction==1?-1:1);
        }

        private void OnDestroy()
        {
            ReleaseEquipmentSprites();
            if (game != null && game.Enemies != null) game.Enemies.ShotLaunched -= OnShot;
            if (game != null && game.RunDriver != null)
            { game.RunDriver.Run.Reset -= ResetVisual; game.RunDriver.Run.Rewound -= ResetVisual; }
            if (economy != null) economy.NodeGathered -= OnGathered;
        }
    }
}
