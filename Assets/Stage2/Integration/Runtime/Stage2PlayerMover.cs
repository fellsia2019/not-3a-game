using Not3A.Stage1;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent]
    public sealed class Stage2PlayerMover : MonoBehaviour
    {
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField, Min(0.1f)] private float speed = 4.2f;
        [SerializeField, Min(0f)] private float planarEdgeClearance;

        private InputAction moveAction;

        public Stage2MapDefinition MapDefinition => mapDefinition;
        public float Speed => speed;
        public float PlanarEdgeClearance => planarEdgeClearance;

        public void Configure(Stage2MapDefinition definition, float movementSpeed)
        {
            mapDefinition = definition;
            speed = Mathf.Max(0.1f, movementSpeed);
        }

        public void ConfigurePlanarEdgeClearance(float clearance)
        {
            planarEdgeClearance = Mathf.Max(0f, clearance);
        }

        public void ClampToAuthoredBounds()
        {
            if (mapDefinition == null || mapDefinition.CellBounds == null)
            {
                return;
            }

            var clamped = ClampMovement((Vector2)transform.position);
            transform.position = new Vector3(clamped.x, clamped.y, transform.position.z);
        }

        private void OnEnable()
        {
            moveAction = new InputAction("Stage 2 Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Dispose();
            moveAction = null;
        }

        private void Update()
        {
            if (mapDefinition == null || moveAction == null)
            {
                return;
            }

            var input = moveAction.ReadValue<Vector2>();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var next = (Vector2)transform.position + input * (speed * Time.deltaTime);
            next = ClampMovement(next);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }

        private Vector2 ClampMovement(Vector2 world)
        {
            var clamped = mapDefinition.ClampWorld(world, mapDefinition.CellBounds);
            if (!mapDefinition.PlanarGrid || planarEdgeClearance <= 0f) return clamped;

            // Stage 3 draws the hero above its ground pivot. Leave room for its
            // full silhouette while the camera holds at the visible map edge.
            var minimum = mapDefinition.MinimumCell;
            var maximum = mapDefinition.MaximumCell;
            return new Vector2(
                ClampAxis(clamped.x, minimum.x + planarEdgeClearance,
                    maximum.x - planarEdgeClearance),
                ClampAxis(clamped.y, minimum.y + planarEdgeClearance,
                    maximum.y - planarEdgeClearance));
        }

        private static float ClampAxis(float value, float minimum, float maximum)
        {
            return minimum <= maximum ? Mathf.Clamp(value, minimum, maximum)
                : (minimum + maximum) * .5f;
        }
    }
}
