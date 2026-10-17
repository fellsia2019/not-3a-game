using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Camera-facing artwork; shots and their clock remain owned by Stage 2.</summary>
    public sealed class Stage3TowerSprite : MonoBehaviour
    {
        [SerializeField] private Stage3TowerSpriteSet artwork;
        [SerializeField] private SpriteRenderer body;
        private Camera viewCamera;
        private SpriteRenderer spoon, stone;
        private Vector2 aim = Vector2.up; // Rest bearing follows the 45-degree grid-facing correction.
        private float pitch = 52f;
        private bool loaded = true;
        public bool IsLayered => artwork.IsLayered;
        public Transform SpoonTransform => spoon == null ? null : spoon.transform;
        public bool LoadedVisible => stone != null && stone.enabled;
        public Sprite LoadedStoneSprite => artwork.Stone;
        public Vector3 LoadedStoneScale => stone.transform.lossyScale;
        public int Direction { get; private set; }
        public int Pose { get; private set; }
        public bool IsCatapult => artwork.IsCatapult;
        public SpriteRenderer Body => body;
        private Transform Anchor => transform.parent;

        public void Bind(Camera camera) { viewCamera = camera; EnsureParts(); Align(); }

        private void EnsureParts()
        {
            if (!IsLayered || spoon != null) return;
            spoon = Part("Articulated spoon",artwork.Spoon);
            stone = Part("Loaded stone",artwork.Stone);
        }

        private SpriteRenderer Part(string name, Sprite sprite)
        {
            var child=transform.Find(name);
            if(child==null) { child=new GameObject(name).transform; child.SetParent(transform,false); }
            var renderer=child.GetComponent<SpriteRenderer>();
            if(renderer==null) renderer=child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite; renderer.sharedMaterial=body.sharedMaterial;
            return renderer;
        }

        public void Face(Vector2 target)
        {
            if (!IsCatapult || viewCamera == null) return;
            var direction = (Vector3)target-Anchor.position;
            aim = ((Vector2)direction).normalized;
            var x = Vector3.Dot(direction,viewCamera.transform.right);
            var y = Vector3.Dot(direction,viewCamera.transform.up);
            Direction = y >= 0f ? (x >= 0f ? 0 : 1) : (x < 0f ? 2 : 3);
            SetPose(Pose);
        }

        public void SetPose(int pose)
        {
            Pose = IsCatapult ? pose : 0;
            body.sprite = artwork.Frame(Direction,Pose);
            Align();
        }

        public void SetThrow(float seconds,float release)
        {
            Pose=seconds<release*.35f?0:seconds<release?1:seconds<release+.20f?3:seconds<1.05f?4:0;
            pitch=Pitch(seconds,release);
            loaded=seconds<release || seconds>=1.05f;
            Align();
        }

        private static float Pitch(float seconds,float release)
        {
            if(seconds<release*.45f) return Mathf.Lerp(52f,10f,Mathf.SmoothStep(0,1,seconds/(release*.45f)));
            if(seconds<release) return Mathf.Lerp(10f,112f,Mathf.SmoothStep(0,1,(seconds-release*.45f)/(release*.55f)));
            if(seconds<release+.11f) return Mathf.Lerp(112f,135f,Mathf.SmoothStep(0,1,(seconds-release)/.11f));
            return Mathf.Lerp(135f,52f,Mathf.SmoothStep(0,1,(seconds-release-.11f)/(.94f-release)));
        }

        public Vector3 SampleRelease(float release)
        {
            var savedPitch=pitch; var savedLoaded=loaded;
            pitch=Pitch(release,release); loaded=false; Align();
            var result=CupPosition;
            pitch=savedPitch; loaded=savedLoaded; Align();
            return result;
        }

        private Vector3 CupPosition => spoon.transform.TransformPoint(new Vector3(275f/480f,258f/480f,0));

        // Release sockets are authored in sheet pixels, measured in the empty-cup
        // release pose. Never show the sheet's detached projectile (pose 2).
        public Vector3 ReleasePosition
        {
            get
            {
                if (viewCamera == null) return transform.position + Vector3.back*1.1f;
                if(IsLayered) return CupPosition;
                var sprite = artwork.Frame(Direction,IsCatapult ? 3 : 0);
                var pixel = artwork.ReleasePixel(Direction);
                var local = (pixel-sprite.pivot)/sprite.pixelsPerUnit;
                return body.transform.TransformPoint(new Vector3(local.x,local.y,0f));
            }
        }

        public void Tint(Color color)
        {
            body.color=color;
            if(spoon!=null) spoon.color=color;
            if(stone!=null) stone.color=color;
        }

        private void LateUpdate() => Align();

        private void Align()
        {
            if (viewCamera == null || body == null || body.sprite == null) return;
            body.transform.rotation = viewCamera.transform.rotation;
            var bounds = artwork.BodyVisibleBounds;
            var fitted = bounds.width > 0f && artwork.CellWidth > 0f;
            var scale = fitted ? artwork.CellWidth/bounds.width : 1f;
            body.transform.localScale = Vector3.one*scale;
            // Register the visible feet, not the padded PNG pivot, at the cell's
            // front edge. One uniform scale also keeps the spoon and stone in proportion.
            var position = fitted
                ? Anchor.position+Vector3.down*(.5f-artwork.FrontInset)-
                    viewCamera.transform.right*(bounds.center.x*scale)-
                    viewCamera.transform.up*(bounds.yMin*scale)
                : Anchor.position+Vector3.back*.025f;
            var depth = fitted ? Mathf.Max(0f,bounds.yMin-body.sprite.bounds.min.y)*scale
                : body.sprite.pivot.y/body.sprite.pixelsPerUnit;
            var clearance = depth*Mathf.Abs(viewCamera.transform.up.z)/
                Mathf.Max(.1f,Mathf.Abs(viewCamera.transform.forward.z))+.03f;
            // Depth-only clearance avoids terrain clipping without shifting screen registration.
            body.transform.position = position-viewCamera.transform.forward*clearance;
            body.sortingOrder = -Mathf.RoundToInt(Anchor.position.y*100f);
            if(!IsLayered) return;
            EnsureParts();
            // The foundation never changes frame, scale or position during a shot.
            // A separate spoon pivots at the painted axle; its cup is also the projectile socket.
            var axle=body.transform.TransformPoint(new Vector3(-.025f,.92f,-.012f));
            var radians=pitch*Mathf.Deg2Rad;
            var target=new Vector2(Vector3.Dot(aim,viewCamera.transform.right),
                Vector3.Dot(aim,viewCamera.transform.up));
            var offset=-target*.75f*Mathf.Cos(radians)+Vector2.up*(.80f*Mathf.Sin(radians));
            // Orthographic 2D articulation keeps the spoon and cup dimensions fixed.
            // Scaling the entire spoon by projected length makes its bowl breathe.
            offset=offset.normalized*.80f;
            var native=new Vector2(275f/480f,258f/480f);
            var angle=Mathf.Atan2(offset.y,offset.x)*Mathf.Rad2Deg-Mathf.Atan2(native.y,native.x)*Mathf.Rad2Deg;
            spoon.transform.SetPositionAndRotation(axle,viewCamera.transform.rotation*Quaternion.Euler(0,0,angle));
            spoon.transform.localScale=Vector3.one*(offset.magnitude/native.magnitude);
            spoon.sortingOrder=body.sortingOrder+1;
            stone.transform.SetPositionAndRotation(CupPosition-viewCamera.transform.forward*.008f,viewCamera.transform.rotation);
            stone.transform.localScale=Vector3.one;
            stone.sortingOrder=body.sortingOrder+2;
            stone.enabled=loaded;
        }
    }
}
