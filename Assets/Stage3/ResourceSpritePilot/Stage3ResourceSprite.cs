using Not3A.Stage2.Economy;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Static resource artwork; gathering and visibility belong to the existing node.</summary>
    public sealed class Stage3ResourceSprite : MonoBehaviour
    {
        private static readonly Unity.Profiling.ProfilerMarker SpriteUpdateMarker=new Unity.Profiling.ProfilerMarker("Stage3.ResourceSprites");
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private Stage3ManualResourceArt art;
        private Camera viewCamera;
        private Stage2ResourceNode node;
        private Transform hero;
        private bool aligned;
        private Sprite alignedSprite;
        private Vector3 alignedAnchor;
        private Quaternion alignedRotation;
        public SpriteRenderer Body => body;
        public Stage2ResourceNode Node => node;
        public int DepletionFrame { get; private set; }
        public Vector3 VisibleTopWorld => body == null || body.sprite == null ? transform.position :
            body.transform.TransformPoint(art != null && node != null
                ? (Vector3)art.VisibleTop(node.Resource, node.Size, DepletionFrame)
                : new Vector3(body.sprite.bounds.center.x, body.sprite.bounds.max.y, 0));

        public void Bind(Camera camera)
        {
            viewCamera = camera;
            Align();
        }

        public void Bind(Camera camera, Stage2ResourceNode resourceNode)
        {
            node = resourceNode;
            Bind(camera);
        }

        public void Bind(Camera camera, Stage2ResourceNode resourceNode, Transform player)
        {
            hero = player;
            Bind(camera, resourceNode);
        }

        private void LateUpdate() => Align();

        private void Align()
        {
            using var sample=SpriteUpdateMarker.Auto();
            if (node != null && body != null)
            {
                var visible=!node.IsDepleted;
                if(body.enabled!=visible)body.enabled=visible;
                DepletionFrame = Stage3ManualResourceArt.State(node.FillFraction);
                var frame = art != null ? art.Frame(node.Resource, node.Size, DepletionFrame) : null;
                if (frame != null && body.sprite!=frame) body.sprite = frame;
            }
            if (viewCamera == null || body == null || body.sprite == null) return;
            var sprite=body.sprite;var rotation=viewCamera.transform.rotation;var anchor=transform.position;
            // Keep the image's painted footing at the node's ground anchor on screen.
            // View-ray clearance lifts the below-pivot pixels above the opaque terrain.
            if(!aligned || alignedSprite!=sprite || alignedAnchor!=anchor || alignedRotation!=rotation)
            {
                var belowPivot = sprite.pivot.y / sprite.pixelsPerUnit;
                var forward = rotation*Vector3.forward;
                var clearance = belowPivot * Mathf.Abs((rotation*Vector3.up).z) /
                    Mathf.Max(.1f, Mathf.Abs(forward.z)) + .05f;
                body.transform.SetPositionAndRotation(anchor+Vector3.back*.025f-forward*clearance,rotation);
                body.sortingOrder = -Mathf.RoundToInt(anchor.y * 100f);
                aligned=true;alignedSprite=sprite;alignedAnchor=anchor;alignedRotation=rotation;
            }
            UpdateForegroundFade(rotation);
        }

        private void UpdateForegroundFade(Quaternion rotation)
        {
            var obscuresHero = false;
            if (hero != null && node != null && node.Resource == Not3A.Stage2.Map.MapResourceKind.Wood &&
                body.enabled && body.sortingOrder > -Mathf.RoundToInt(hero.position.y * 100f) + 1)
            {
                var bounds = body.sprite.bounds;
                if(viewCamera.orthographic)
                {
                    // Camera-facing artwork and an orthographic camera preserve overlap
                    // under this affine transform; no six world-to-screen calls per tree.
                    var matrix=body.transform.worldToLocalMatrix;
                    var feet=matrix.MultiplyPoint3x4(hero.position);
                    var head=matrix.MultiplyPoint3x4(hero.position+(rotation*Vector3.up)*1.15f);
                    var halfWidth=Mathf.Abs(matrix.MultiplyVector((rotation*Vector3.right)*(.22f*1.15f)).x);
                    obscuresHero=feet.x+halfWidth>bounds.min.x && feet.x-halfWidth<bounds.max.x &&
                        head.y>bounds.min.y && feet.y<bounds.max.y;
                }
                else
                {
                    var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                    var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                    for (var y = 0; y < 2; y++) for (var x = 0; x < 2; x++)
                    {
                        var corner = body.transform.TransformPoint(new Vector3(x == 0 ? bounds.min.x : bounds.max.x,
                            y == 0 ? bounds.min.y : bounds.max.y, 0));
                        var screen = (Vector2)viewCamera.WorldToScreenPoint(corner);
                        min = Vector2.Min(min, screen); max = Vector2.Max(max, screen);
                    }
                    var feet = (Vector2)viewCamera.WorldToScreenPoint(hero.position);
                    var head = (Vector2)viewCamera.WorldToScreenPoint(hero.position + viewCamera.transform.up * 1.15f);
                    var halfWidth = (head.y - feet.y) * .22f;
                    obscuresHero = feet.x + halfWidth > min.x && feet.x - halfWidth < max.x &&
                        head.y > min.y && feet.y < max.y;
                }
            }
            var color = body.color;
            var alpha = Mathf.MoveTowards(color.a, obscuresHero ? .45f : 1f, Time.unscaledDeltaTime * 5f);
            if(color.a!=alpha) {color.a=alpha;body.color=color;}
        }
    }
}
