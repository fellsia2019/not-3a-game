using System;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Defense
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)] // Build-mode input closes gathering before its frame update.
    public sealed class Stage2BuildDriver : MonoBehaviour
    {
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField] private Stage2DefenseCatalog catalog;
        [SerializeField] private Stage2GatheringDriver gathering;
        [SerializeField] private Transform player;
        [SerializeField] private Camera worldCamera;

        private Stage2Navigation navigation;
        private Stage2Defense defense;
        private SpriteRenderer ghost;
        private InputAction buildAction;
        private InputAction confirmAction;
        private InputAction cancelAction;
        private Stage2BuildPhase phase = Stage2BuildPhase.Inactive;
        private bool paused;

        public Stage2Defense Defense => defense;
        public PlacementResult PreviewResult { get; private set; } = PlacementResult.NotPlacing;

        public bool Configure(Stage2MapDefinition definition, Stage2DefenseCatalog data,
            Stage2GatheringDriver economyDriver, Transform playerTransform,
            Camera camera, Stage2Navigation sharedNavigation)
        {
            mapDefinition = definition;
            catalog = data;
            gathering = economyDriver;
            player = playerTransform;
            worldCamera = camera;
            navigation = sharedNavigation;
            return Initialize();
        }

        public bool Initialize()
        {
            if (mapDefinition == null || catalog == null || gathering == null ||
                gathering.Economy == null || player == null || worldCamera == null || navigation == null)
            {
                Debug.LogError("S2_DEF_BINDING_ERROR: map, catalog, initialized Economy, hero, camera and shared Navigation are required.", this);
                return false;
            }
            try
            {
                defense?.ResetRun();
                defense = new Stage2Defense(mapDefinition, navigation, gathering.Economy.Inventory,
                    catalog, () => player.position, () => Instantiate(catalog.TowerPrefab));
                defense.SetRunState(phase, paused);
                CreateGhost();
                return true;
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"S2_DEF_BINDING_ERROR: {exception.Message}", this);
                return false;
            }
        }

        public void SetEnemyProtection(Func<Vector2Int, bool> isProtected) =>
            defense?.SetEnemyProtection(isProtected);

        public void SetRunState(Stage2BuildPhase currentPhase, bool isPaused)
        {
            phase = currentPhase;
            paused = isPaused;
            defense?.SetRunState(phase, paused);
            SyncBuildMode();
        }

        public void ResetRun()
        {
            defense?.ResetRun();
            phase = Stage2BuildPhase.Inactive;
            paused = false;
            SyncBuildMode();
        }

        private void OnEnable()
        {
            buildAction = new InputAction("Stage 2 Build", InputActionType.Button, "<Keyboard>/b");
            confirmAction = new InputAction("Stage 2 Confirm Build", InputActionType.Button, "<Mouse>/leftButton");
            cancelAction = new InputAction("Stage 2 Cancel Build", InputActionType.Button);
            cancelAction.AddBinding("<Keyboard>/escape");
            cancelAction.AddBinding("<Mouse>/rightButton");
            buildAction.Enable();
            confirmAction.Enable();
            cancelAction.Enable();
        }

        private void OnDisable()
        {
            buildAction?.Dispose();
            confirmAction?.Dispose();
            cancelAction?.Dispose();
            buildAction = confirmAction = cancelAction = null;
            defense?.CancelPlacement();
            SyncBuildMode();
        }

        private void OnDestroy()
        {
            defense?.ResetRun();
            if (ghost != null) Destroy(ghost.gameObject);
        }

        private void Update()
        {
            if (defense == null) return;
            var buildPressed = buildAction != null && buildAction.WasPressedThisFrame();
            var cancelPressed = cancelAction != null && cancelAction.WasPressedThisFrame();
            // Cancellation wins over confirmation in the same input update.
            if (cancelPressed || (buildPressed && defense.IsPlacing)) defense.CancelPlacement();
            else if (buildPressed) defense.BeginPlacement();

            if (defense.IsPlacing && Mouse.current != null)
            {
                var screen = Mouse.current.position.ReadValue();
                var world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
                var cell = mapDefinition.WorldToCell(world);
                defense.SetPreviewCell(cell);
                PreviewResult = defense.EvaluatePreview();
                if (ghost != null)
                {
                    ghost.transform.position = mapDefinition.CellToWorld(cell);
                    ghost.color = PreviewResult == PlacementResult.Ready
                        ? new Color(0.2f, 1f, 0.45f, 0.65f)
                        : new Color(1f, 0.25f, 0.2f, 0.65f);
                }
                if (!cancelPressed && confirmAction != null && confirmAction.WasPressedThisFrame())
                    PreviewResult = defense.TryConfirm(out _);
            }
            else PreviewResult = PlacementResult.NotPlacing;
            SyncBuildMode();
        }

        private void SyncBuildMode()
        {
            var placing = defense != null && defense.IsPlacing;
            gathering?.SetBuildMode(placing);
            if (ghost != null) ghost.gameObject.SetActive(placing);
        }

        private void CreateGhost()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            var source = catalog.TowerPrefab.GetComponentInChildren<SpriteRenderer>(true);
            if (source == null) return;
            var instance = new GameObject("Stage 2 Build Preview");
            ghost = instance.AddComponent<SpriteRenderer>();
            ghost.sprite = source.sprite;
            ghost.sortingOrder = source.sortingOrder;
            ghost.transform.localScale = source.transform.lossyScale;
            instance.SetActive(false);
        }
    }
}
