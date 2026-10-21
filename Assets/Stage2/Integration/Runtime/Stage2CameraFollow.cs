using Not3A.Stage1;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class Stage2CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField, Min(0f)] private float smoothing = 7f;
        [SerializeField] private bool tacticalView;
        [SerializeField] private Vector3 tacticalOffset = new Vector3(0f, -11.47f, -16.38f);
        [SerializeField] private Stage2CameraZoomSettings zoomSettings;
        private float tacticalStartingSize;
        private float tacticalTargetSize;
        private int zoomDiagnosticsRemaining = 4;
        private int pinchDiagnosticsRemaining = 3;
        private System.Func<Vector2, bool> pointerBlocker;
        private System.Func<Vector2, bool> overlayTrackpadHitTest;
        private System.Action<float, Vector2, Vector2> overlayTrackpadInput;

        public void SetPointerBlocker(System.Func<Vector2, bool> blocker) => pointerBlocker = blocker;

        // The camera polls the app-local native queue once, routing to the visible surface.
        // Callback arguments: pinch ratio, GUI-space pan delta, bottom-left screen pointer.
        public void SetOverlayTrackpadInput(System.Func<Vector2, bool> hitTest,
            System.Action<float, Vector2, Vector2> input)
        {
            overlayTrackpadHitTest = hitTest;
            overlayTrackpadInput = input;
        }

        public Transform Target => target;
        public Stage2MapDefinition MapDefinition => mapDefinition;
        public Camera BoundCamera => GetComponent<Camera>();
        public bool TacticalView => tacticalView;
        public Stage2CameraZoomSettings ZoomSettings => zoomSettings;
        public float MaximumTacticalSize=>tacticalStartingSize*(zoomSettings!=null?zoomSettings.MaximumDistanceMultiplier:2f);

        public void ConfigureTacticalZoom(Stage2CameraZoomSettings settings)
        {
            zoomSettings = settings;
        }

        private void OnEnable()
        {
            if (!tacticalView) return;
            if (tacticalStartingSize <= 0f) tacticalStartingSize = BoundCamera.orthographicSize;
            tacticalTargetSize = Mathf.Clamp(BoundCamera.orthographicSize,
                tacticalStartingSize * .5f, MaximumTacticalSize);
            Stage2MacTrackpad.Initialize();
            ApplyFollow(true);
        }

        private void OnDisable() => Stage2MacTrackpad.Stop();

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) Stage2MacTrackpad.Stop();
        }

        public void ConfigureTacticalView(float startingSize = 5f)
        {
            tacticalView = true;
            transform.rotation = Quaternion.Euler(-35f, 0f, 0f);
            tacticalOffset = -transform.forward * 20f;
            BoundCamera.orthographicSize = startingSize;
            tacticalStartingSize = BoundCamera.orthographicSize;
            tacticalTargetSize = tacticalStartingSize;
            SnapToTarget();
        }

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
            if (tacticalView)
            {
                var camera = BoundCamera;
                tacticalTargetSize=Mathf.Clamp(tacticalTargetSize,tacticalStartingSize*.5f,MaximumTacticalSize);
                var factor = 1f - Mathf.Exp(-7f * Time.deltaTime);
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, tacticalTargetSize, factor);
            }
            ApplyFollow(false);
        }

        private void Update()
        {
            if (!tacticalView || tacticalStartingSize <= 0f) return;
            var pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            var pointerAllowed = pointerBlocker == null || !pointerBlocker(pointer);
            var overlayOwnsInput = overlayTrackpadInput != null && overlayTrackpadHitTest != null && overlayTrackpadHitTest(pointer);
            var pinchScale = Stage2MacTrackpad.ReadScale(CanReceivePinch() && (pointerAllowed || overlayOwnsInput), overlayOwnsInput);
            var pan = Stage2MacTrackpad.ReadPan();
            if (overlayOwnsInput)
            {
                ApplyTrackpadGesture(pinchScale, pan, pointer);
                return;
            }
            if (!pointerAllowed) return;
            if (pinchScale != 1f)
            {
                ZoomByPinchScale(pinchScale);
                if (pinchDiagnosticsRemaining-- > 0)
                    Debug.Log($"S3_PINCH_INPUT scale={pinchScale:0.####} target={tacticalTargetSize:0.###}");
                return;
            }
            if (Mouse.current == null) return;
            var scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll == 0f) return;
            // Input System reports a delta whose units vary by device. A small
            // nonzero wheel event is still a deliberate zoom step.
            var steps = Mathf.Sign(scroll) * Mathf.Max(1f, Mathf.Abs(scroll) / 120f);
            var multiplier = zoomSettings != null ? zoomSettings.StepMultiplier : 1.4f;
            ApplyZoomScale(Mathf.Pow(multiplier, -steps));
            if (zoomDiagnosticsRemaining-- > 0)
                Debug.Log($"S3_ZOOM_INPUT raw={scroll:0.###} steps={steps:0.###} target={tacticalTargetSize:0.###}");
        }

        public void ApplyTrackpadGesture(float scale, Vector2 panDelta, Vector2 screenPoint)
        {
            if (!isActiveAndEnabled || !tacticalView) return;
            if (overlayTrackpadInput != null && overlayTrackpadHitTest != null && overlayTrackpadHitTest(screenPoint))
                overlayTrackpadInput(scale, panDelta, screenPoint);
            else if (pointerBlocker == null || !pointerBlocker(screenPoint)) ZoomByPinchScale(scale);
        }

        /// <summary>Ratio supplied by a pinch gesture: spreading fingers (>1) zooms in.</summary>
        public void ZoomByPinchScale(float scale)
        {
            if (!isActiveAndEnabled || !tacticalView || tacticalStartingSize <= 0f ||
                scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale)) return;
            ApplyZoomScale(1f / Mathf.Max(scale, .000001f));
        }

        private void ApplyZoomScale(float scale)
        {
            tacticalTargetSize = Mathf.Clamp(tacticalTargetSize * scale,
                tacticalStartingSize * .5f, MaximumTacticalSize);
        }

        private static bool CanReceivePinch()
        {
            if (!Application.isFocused || Application.isBatchMode) return false;
#if UNITY_EDITOR
            var focused = UnityEditor.EditorWindow.focusedWindow;
            var hovered = UnityEditor.EditorWindow.mouseOverWindow;
            return !UnityEditor.EditorApplication.isPaused && focused != null && hovered != null &&
                focused.GetType().Name == "GameView" && hovered.GetType().Name == "GameView";
#else
            return true;
#endif
        }

        private void ApplyFollow(bool immediate)
        {
            if (target == null || mapDefinition == null || mapDefinition.CameraBounds == null)
            {
                return;
            }

            var bounds = mapDefinition.CameraBounds;
            if (tacticalView)
            {
                var focus = mapDefinition.ClampWorld((Vector2)target.position, bounds);
                var camera = BoundCamera;
                // At a wide orthographic size, a fixed camera distance puts the
                // lower viewport rays behind the ground plane. Keep the whole
                // viewport in front of it without changing angle or focus.
                var viewDistance = Mathf.Max(tacticalOffset.magnitude, camera.orthographicSize);
                var desiredView = new Vector3(focus.x, focus.y, 0f) - transform.forward * viewDistance;
                transform.position = desiredView;
                // Follow the hero until the visible ground reaches the playable
                // edge. At that point the hero moves across the screen while the
                // camera stays put. The larger decorative border remains hidden.
                var ground = new Plane(Vector3.forward, Vector3.zero);
                var minimumOffset = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                var maximumOffset = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                for (var corner = 0; corner < 4; corner++)
                {
                    var ray = camera.ViewportPointToRay(new Vector3(corner & 1, corner >> 1, 0f));
                    if (!ground.Raycast(ray, out var distance)) return;
                    var offset = (Vector2)ray.GetPoint(distance) - focus;
                    minimumOffset = Vector2.Min(minimumOffset, offset);
                    maximumOffset = Vector2.Max(maximumOffset, offset);
                }

                const float edgeInset = .2f;
                var minimum = (Vector2)bounds.Minimum - Vector2.one * (.5f - edgeInset);
                var maximum = (Vector2)bounds.MaximumInclusive + Vector2.one * (.5f - edgeInset);
                focus.x = ClampViewportAxis(focus.x, minimum.x - minimumOffset.x,
                    maximum.x - maximumOffset.x);
                focus.y = ClampViewportAxis(focus.y, minimum.y - minimumOffset.y,
                    maximum.y - maximumOffset.y);
                transform.position = new Vector3(focus.x, focus.y, 0f) - transform.forward * viewDistance;
                return;
            }
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

        private static float ClampViewportAxis(float value, float minimum, float maximum)
        {
            // A viewport wider than the authored world cannot fit entirely.
            // Center it in that exceptional case instead of reversing the clamp.
            return minimum <= maximum ? Mathf.Clamp(value, minimum, maximum)
                : (minimum + maximum) * .5f;
        }

    }
}
