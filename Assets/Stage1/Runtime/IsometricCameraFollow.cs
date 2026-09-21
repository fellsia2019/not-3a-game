using UnityEngine;

namespace Not3A.Stage1
{
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 followBoundsX = new Vector2(-4f, 4f);
        [SerializeField] private Vector2 followBoundsY = new Vector2(-2f, 2f);
        [SerializeField, Min(0f)] private float smoothing = 8f;

        public void Configure(Transform followTarget, Vector2 boundsX, Vector2 boundsY, float smooth)
        {
            target = followTarget;
            followBoundsX = boundsX;
            followBoundsY = boundsY;
            smoothing = smooth;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = new Vector3(
                Mathf.Clamp(target.position.x, followBoundsX.x, followBoundsX.y),
                Mathf.Clamp(target.position.y, followBoundsY.x, followBoundsY.y),
                transform.position.z);
            var t = smoothing <= 0f ? 1f : 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
    }
}
