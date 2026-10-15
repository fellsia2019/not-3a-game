using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Static resource artwork; gathering and visibility belong to the existing node.</summary>
    public sealed class Stage3ResourceSprite : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        private Camera viewCamera;
        public SpriteRenderer Body => body;

        public void Bind(Camera camera)
        {
            viewCamera = camera;
            Align();
        }

        private void LateUpdate() => Align();

        private void Align()
        {
            if (viewCamera == null || body == null || body.sprite == null) return;
            body.transform.rotation = viewCamera.transform.rotation;
            // Keep the image's painted footing at the node's ground anchor on screen.
            // View-ray clearance lifts the below-pivot pixels above the opaque terrain.
            var belowPivot = body.sprite.pivot.y / body.sprite.pixelsPerUnit;
            var forward = viewCamera.transform.forward;
            var clearance = belowPivot * Mathf.Abs(viewCamera.transform.up.z) /
                Mathf.Max(.1f, Mathf.Abs(forward.z)) + .05f;
            body.transform.position = transform.position + Vector3.back * .025f - forward * clearance;
            body.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
        }
    }
}
