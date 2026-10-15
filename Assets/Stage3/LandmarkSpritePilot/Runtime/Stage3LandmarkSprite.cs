using Not3A.Stage2.RunState;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Fixed-view landmark artwork only. Map reservations and run rules remain external.</summary>
    public sealed class Stage3LandmarkSprite : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private bool portal;
        private Camera gameCamera;
        private Stage2RunDriver runDriver;
        private MaterialPropertyBlock properties;
        public SpriteRenderer Body => body;
        public float FlowTime { get; private set; }

        public void Bind(Stage2RunDriver run, Camera camera)
        {
            runDriver = run;
            gameCamera = camera;
            AlignArtwork();
        }

        private void LateUpdate()
        {
            if (body == null || gameCamera == null) return;
            AlignArtwork();
            // Portal retains its existing ambient clock, including pause/outcome semantics.
            if (portal || runDriver?.Run?.IsPaused == true) return;
            FlowTime += Time.deltaTime;
            properties ??= new MaterialPropertyBlock();
            body.GetPropertyBlock(properties);
            properties.SetFloat("_FlowTime", FlowTime);
            body.SetPropertyBlock(properties);
        }

        private void AlignArtwork()
        {
            body.transform.rotation = gameCamera.transform.rotation;
            // A landmark is registered at the centre of its baked base, unlike a unit's foot pivot.
            // Lower image pixels would sit below the XY ground when the quad tilts with the camera.
            // Move along the view ray: screen registration is unchanged, but the entire base clears
            // the opaque ground. Keep ordinary depth testing against other scene objects.
            var belowPivot = body.sprite.pivot.y / body.sprite.pixelsPerUnit;
            var forward = gameCamera.transform.forward;
            var clearance = belowPivot * Mathf.Abs(gameCamera.transform.up.z) / Mathf.Max(.1f,Mathf.Abs(forward.z)) + .03f;
            body.transform.position = transform.position + Vector3.back*.025f - forward*clearance;
            // Sort transparent actors by their real ground anchors, not the artwork's view-ray clearance.
            body.sortingOrder = -Mathf.RoundToInt(transform.position.y*100f);
        }
    }
}
