using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage1
{
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 4f;
        [SerializeField] private Rect movementBounds = new Rect(-9f, -5f, 18f, 10f);
        [SerializeField] private bool useIsoCellBounds;
        [SerializeField] private Vector2 minimumCell = new Vector2(-9f, -7f);
        [SerializeField] private Vector2 maximumCell = new Vector2(9f, 7f);
        [SerializeField, Min(0.05f)] private float collisionRadius = 0.24f;
        [SerializeField] private Stage1GameController game;

        private InputAction moveAction;

        public void Configure(Stage1GameController controller, float movementSpeed, Rect bounds)
        {
            game = controller;
            speed = movementSpeed;
            movementBounds = bounds;
            useIsoCellBounds = false;
        }

        public void ConfigureIsoBounds(Stage1GameController controller, float movementSpeed, Vector2 minCell, Vector2 maxCell)
        {
            game = controller;
            speed = movementSpeed;
            minimumCell = minCell;
            maximumCell = maxCell;
            useIsoCellBounds = true;
        }

        private void OnEnable()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
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
            if (moveAction == null || (game != null && game.HasEnded))
            {
                return;
            }

            var input = moveAction.ReadValue<Vector2>();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var next = (Vector2)transform.position + input * (speed * Time.deltaTime);
            if (useIsoCellBounds)
            {
                next = IsoGrid.ClampWorldToCellBounds(next, minimumCell, maximumCell);
            }
            else
            {
                next.x = Mathf.Clamp(next.x, movementBounds.xMin, movementBounds.xMax);
                next.y = Mathf.Clamp(next.y, movementBounds.yMin, movementBounds.yMax);
            }

            if (Physics2D.OverlapCircle(next, collisionRadius) == null)
            {
                transform.position = new Vector3(next.x, next.y, transform.position.z);
            }
        }
    }
}
