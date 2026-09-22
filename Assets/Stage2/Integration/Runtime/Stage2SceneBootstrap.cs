using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public readonly struct Stage2IntegrationValidationResult
    {
        public Stage2IntegrationValidationResult(string code, string reason)
        {
            Code = code ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public string Code { get; }
        public string Reason { get; }
        public bool IsValid => string.IsNullOrEmpty(Code);
    }

    [DisallowMultipleComponent]
    public sealed class Stage2SceneBootstrap : MonoBehaviour
    {
        public const string DiagnosticPrefix = "S2_INT_VALIDATION_ERROR";

        [Header("Authored Map")]
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField] private Stage2MapDebugRenderer mapDebugRenderer;

        [Header("Player And Camera")]
        [SerializeField] private Stage2PlayerMover playerMover;
        [SerializeField] private Stage2CameraFollow cameraFollow;

        [Header("Landmarks")]
        [SerializeField] private Transform entranceMarker;
        [SerializeField] private Transform throneMarker;

        public Stage2MapDefinition MapDefinition => mapDefinition;
        public Stage2MapDebugRenderer MapDebugRenderer => mapDebugRenderer;
        public Stage2PlayerMover PlayerMover => playerMover;
        public Stage2CameraFollow CameraFollow => cameraFollow;
        public Transform EntranceMarker => entranceMarker;
        public Transform ThroneMarker => throneMarker;
        public bool IsInitialized { get; private set; }
        public string AuthoredMapHash { get; private set; } = string.Empty;
        public Stage2IntegrationValidationResult LastValidation { get; private set; }

        public void Configure(
            Stage2MapDefinition definition,
            Stage2MapDebugRenderer debugRenderer,
            Stage2PlayerMover mover,
            Stage2CameraFollow follow,
            Transform entrance,
            Transform throne)
        {
            mapDefinition = definition;
            mapDebugRenderer = debugRenderer;
            playerMover = mover;
            cameraFollow = follow;
            entranceMarker = entrance;
            throneMarker = throne;
        }

        public Stage2IntegrationValidationResult ValidateBindings()
        {
            if (mapDefinition == null)
            {
                return Invalid("MissingMapReference", "The Stage 2 map definition serialized reference is missing.");
            }

            var mapValidation = mapDefinition.ValidateDefinition();
            if (!mapValidation.IsValid)
            {
                var firstIssue = mapValidation.Issues[0];
                return Invalid($"MapDefinition.{firstIssue.Code}", firstIssue.Message);
            }

            if (mapDebugRenderer == null)
            {
                return Invalid("MissingDebugRenderer", "The Stage2MapDebugRenderer serialized reference is missing.");
            }

            if (mapDebugRenderer.MapDefinition != mapDefinition)
            {
                return Invalid("DebugRendererMapMismatch", "The debug renderer must reference the same authored map definition as the composition root.");
            }

            if (playerMover == null)
            {
                return Invalid("MissingPlayerMover", "The Stage 2 player mover serialized reference is missing.");
            }

            if (playerMover.MapDefinition != mapDefinition)
            {
                return Invalid("PlayerMapMismatch", "Player bounds must come from the composition root's authored map definition.");
            }

            if (cameraFollow == null)
            {
                return Invalid("MissingCameraFollow", "The Stage 2 camera follow serialized reference is missing.");
            }

            if (cameraFollow.MapDefinition != mapDefinition)
            {
                return Invalid("CameraMapMismatch", "Camera bounds must come from the composition root's authored map definition.");
            }

            if (cameraFollow.Target != playerMover.transform)
            {
                return Invalid("CameraTargetMismatch", "The fixed Stage 2 camera must follow the serialized non-combat player marker.");
            }

            var boundCamera = cameraFollow.BoundCamera;
            if (boundCamera == null || !boundCamera.orthographic || cameraFollow.transform.rotation != Quaternion.identity)
            {
                return Invalid("InvalidCameraConfiguration", "The Stage 2 camera must be orthographic with identity rotation.");
            }

            if (entranceMarker == null)
            {
                return Invalid("MissingEntranceMarker", "The enemy-entrance marker serialized reference is missing.");
            }

            if (throneMarker == null)
            {
                return Invalid("MissingThroneMarker", "The throne marker serialized reference is missing.");
            }

            if (entranceMarker == throneMarker)
            {
                return Invalid("DuplicateLandmarkMarker", "Entrance and throne must use different scene marker transforms.");
            }

            return default;
        }

        public bool Initialize()
        {
            LastValidation = ValidateBindings();
            if (!LastValidation.IsValid)
            {
                IsInitialized = false;
                DisableBoundMotion();
                Debug.LogError($"{DiagnosticPrefix} code={LastValidation.Code} reason={LastValidation.Reason}", this);
                return false;
            }

            AuthoredMapHash = mapDefinition.ComputeDeterministicContentHash();
            SetWorldPosition(entranceMarker, mapDefinition.CellToWorld(mapDefinition.EnemyEntrance));
            SetWorldPosition(throneMarker, mapDefinition.CellToWorld(mapDefinition.Throne));
            playerMover.ClampToAuthoredBounds();
            cameraFollow.SnapToTarget();
            mapDebugRenderer.Rebuild();
            IsInitialized = true;
            Debug.Log($"S2_INT_STARTUP_OK map={mapDefinition.name} size={mapDefinition.Size.x}x{mapDefinition.Size.y} hash={AuthoredMapHash}", this);
            return true;
        }

        private void Awake()
        {
            Initialize();
        }

        private void DisableBoundMotion()
        {
            if (playerMover != null)
            {
                playerMover.enabled = false;
            }

            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
            }
        }

        private static Stage2IntegrationValidationResult Invalid(string code, string reason)
        {
            return new Stage2IntegrationValidationResult(code, reason);
        }

        private static void SetWorldPosition(Transform target, Vector2 position)
        {
            target.position = new Vector3(position.x, position.y, target.position.z);
        }
    }
}
