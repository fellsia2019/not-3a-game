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

        private InputAction moveAction;

        public Stage2MapDefinition MapDefinition => mapDefinition;
        public float Speed => speed;

        public void Configure(Stage2MapDefinition definition, float movementSpeed)
        {
            mapDefinition = definition;
            speed = Mathf.Max(0.1f, movementSpeed);
        }

        public void ClampToAuthoredBounds()
        {
            if (mapDefinition == null || mapDefinition.CellBounds == null)
            {
                return;
            }

            var clamped = IsoGrid.ClampWorldToCellBounds(
                transform.position,
                mapDefinition.CellBounds.Minimum,
                mapDefinition.CellBounds.MaximumInclusive);
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
            next = IsoGrid.ClampWorldToCellBounds(
                next,
                mapDefinition.CellBounds.Minimum,
                mapDefinition.CellBounds.MaximumInclusive);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }
    }
}
