using Not3A.Stage1;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class Stage2CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField, Min(0f)] private float smoothing = 7f;

        public Transform Target => target;
        public Stage2MapDefinition MapDefinition => mapDefinition;
        public Camera BoundCamera => GetComponent<Camera>();

        public void Configure(Transform followTarget, Stage2MapDefinition definition, float smooth)
        {
            target = followTarget;
            mapDefinition = definition;
            smoothing = Mathf.Max(0f, smooth);
        }

        public void SnapToTarget()
        {
            ApplyFollow(true);
        }

        private void LateUpdate()
        {
            ApplyFollow(false);
        }

        private void ApplyFollow(bool immediate)
        {
            if (target == null || mapDefinition == null || mapDefinition.CameraBounds == null)
            {
                return;
            }

            var bounds = mapDefinition.CameraBounds;
            var clamped = IsoGrid.ClampWorldToCellBounds(
                target.position,
                bounds.Minimum,
                bounds.MaximumInclusive);
            var desired = new Vector3(clamped.x, clamped.y, transform.position.z);
            var interpolation = immediate || smoothing <= 0f
                ? 1f
                : 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            var next = IsoGrid.ClampWorldToCellBounds(
                Vector3.Lerp(transform.position, desired, interpolation),
                bounds.Minimum,
                bounds.MaximumInclusive);
            transform.position = new Vector3(next.x, next.y, desired.z);
        }
    }
}
