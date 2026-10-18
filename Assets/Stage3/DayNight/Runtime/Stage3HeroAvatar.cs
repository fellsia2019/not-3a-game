using Not3A.Stage2.Integration;
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
        private float stride, shotPose, idleSeconds;
        private int direction = 3;
        public SpriteRenderer Body => body;
        public SpriteRenderer Equipment => bow;
        public Vector3 ReleasePosition => bow != null ? bow.transform.position : transform.position + Vector3.back*.43f;
        public int Direction => direction;
        public int Frame { get; private set; }
        public bool IsMoving { get; private set; }

        public void Initialize(Stage3HeroArt artwork, Stage2GameComposition owner, Camera camera)
        {
            art = artwork; game = owner; view = camera; previous = transform.position;
            body = Part("Hero C body"); bow = Part("Starter bow equipment");
            game.Enemies.ShotLaunched += OnShot;
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

        private void OnShot(Stage2Shot shot)
        {
            if (Vector2.Distance(shot.Origin,transform.position) > .01f) return;
            direction = Facing((Vector3)shot.Position-transform.position); shotPose = .24f;
            Apply();
        }

        private void LateUpdate()
        {
            if (art == null || game == null || view == null) return;
            var delta = transform.position-previous; previous = transform.position;
            if (!game.RunDriver.Run.IsPaused)
            {
                shotPose = Mathf.Max(0f,shotPose-Time.deltaTime);
                var distance = ((Vector2)delta).magnitude;
                if (distance > .00001f && distance < 2f)
                {
                    IsMoving = true; idleSeconds = 0f;
                    if (shotPose <= 0f) direction = Facing(delta);
                    stride = Mathf.Repeat(stride+distance/art.strideCells,1f);
                    Frame = Mathf.FloorToInt(stride*4f);
                }
                else
                {
                    IsMoving = false; Frame = 0; stride = 0f;
                    idleSeconds += Time.deltaTime;
                }
            }
            Apply();
        }

        private void Apply()
        {
            if (art == null || !art.IsValid || view == null) return;
            var idle = !IsMoving && art.HasIdle;
            body.sprite = idle ? art.idleDirections[direction] : art.bodyFrames[direction*4+Frame];
            bow.sprite = art.bowDirections[direction];
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
            var gripIndex = art.gripOffsets != null && art.gripOffsets.Length == 16 ? direction*4+Frame : direction;
            var grip = art.gripOffsets != null && art.gripOffsets.Length > gripIndex ? art.gripOffsets[gripIndex] : new Vector2(.1f,.55f);
            if (idle) grip = art.idleGripOffsets[direction];
            bow.transform.SetPositionAndRotation(body.transform.TransformPoint(grip)-view.transform.forward*.005f,
                view.transform.rotation*Quaternion.Euler(0f,0f,shotPose > 0f ? Mathf.Sin(shotPose/.24f*Mathf.PI)*-5f : breath*.6f));
            body.sortingOrder = -Mathf.RoundToInt(world.y*100f)+1;
            bow.sortingOrder = body.sortingOrder+(direction == 1 ? -1 : 1);
        }

        private void OnDestroy() { if (game != null && game.Enemies != null) game.Enemies.ShotLaunched -= OnShot; }
    }
}
