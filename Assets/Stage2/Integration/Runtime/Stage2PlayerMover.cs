using Not3A.Stage1;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Profiling;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent]
    public sealed class Stage2PlayerMover : MonoBehaviour
    {
        private static readonly ProfilerMarker MoveMarker=new ProfilerMarker("Hero.GroundMovement");
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField, Min(0.1f)] private float speed = 4.2f;
        [SerializeField, Min(0f)] private float planarEdgeClearance;

        private InputAction moveAction;
        private float movementMultiplier = 1f;
        private HeroObstacleMap obstacles;
        private float collisionRadius = .2f;

        public Stage2MapDefinition MapDefinition => mapDefinition;
        public float Speed => speed;
        public float MovementMultiplier => movementMultiplier;
        public void SetMovementMultiplier(float value) => movementMultiplier =
            float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp(value, 1f, 10f);
        public float PlanarEdgeClearance => planarEdgeClearance;
        public HeroObstacleMap Obstacles => obstacles;
        public float CollisionRadius => collisionRadius;
        public int OverlapRecoveryCount { get; private set; }

        public void BindObstacles(HeroObstacleMap value, float radius)
        {
            obstacles = value;
            collisionRadius = Mathf.Max(.01f, radius);
            MoveBy(Vector2.zero);
        }

        public void MoveBy(Vector2 displacement)
        {
            using var sample=MoveMarker.Auto();
            if (mapDefinition == null) return;
            var start = ClampMovement(transform.position);
            if (obstacles != null && obstacles.IsBlocked(start, collisionRadius))
            {
                var minimum = ClampMovement(mapDefinition.MinimumCell);
                var maximum = ClampMovement(mapDefinition.MaximumCell);
                if (!obstacles.TryFindFreePosition(start, collisionRadius,
                    Rect.MinMaxRect(minimum.x, minimum.y, maximum.x, maximum.y), out start)) return;
                OverlapRecoveryCount++;
            }
            var target = ClampMovement(start + displacement);
            var next = obstacles == null ? target : obstacles.Move(start, target - start, collisionRadius);
            next = ClampMovement(next);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }

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

            MoveBy(input * (speed * movementMultiplier * Time.deltaTime));
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
