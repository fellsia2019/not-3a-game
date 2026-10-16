using System;
using System.Collections.Generic;
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
        [SerializeField] private Stage2DefenseCatalog[] additionalCatalogs = Array.Empty<Stage2DefenseCatalog>();
        [SerializeField] private Stage2GatheringDriver gathering;
        [SerializeField] private Transform player;
        [SerializeField] private Camera worldCamera;

        private Stage2Navigation navigation;
        private Stage2Defense defense;
        private SpriteRenderer ghost;
        private InputAction buildAction;
        private InputAction confirmAction;
        private InputAction cancelAction;
        private InputAction firstRoleAction;
        private InputAction secondRoleAction;
        private Stage2BuildPhase phase = Stage2BuildPhase.Inactive;
        private bool paused;
        private Func<Vector2, bool> pointerBlocker;

        public Stage2Defense Defense => defense;
        public PlacementResult PreviewResult { get; private set; } = PlacementResult.NotPlacing;
        public SpriteRenderer PreviewRenderer => ghost;
        public bool PointerOverUi { get; private set; }

        public void SetPointerBlocker(Func<Vector2, bool> blocker) => pointerBlocker = blocker;

        public void SelectAndBeginPlacement(int role)
        {
            if (defense == null || !defense.SelectRole(role)) return;
            CreateGhost();
            defense.BeginPlacement();
            SyncBuildMode();
        }

        public bool TryScreenToCell(Vector2 screen, out Vector2Int cell)
        {
            cell = default;
            if (worldCamera == null || mapDefinition == null) return false;
            var ray = worldCamera.ScreenPointToRay(screen);
            if (!new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance)) return false;
            cell = mapDefinition.WorldToCell(ray.GetPoint(distance));
            return true;
        }

        public bool Configure(Stage2MapDefinition definition, Stage2DefenseCatalog data,
            Stage2GatheringDriver economyDriver, Transform playerTransform,
            Camera camera, Stage2Navigation sharedNavigation)
            => Configure(definition, data, Array.Empty<Stage2DefenseCatalog>(), economyDriver,
                playerTransform, camera, sharedNavigation);

        public bool Configure(Stage2MapDefinition definition, Stage2DefenseCatalog data,
            Stage2DefenseCatalog[] extraCatalogs, Stage2GatheringDriver economyDriver,
            Transform playerTransform, Camera camera, Stage2Navigation sharedNavigation)
        {
            mapDefinition = definition;
            catalog = data;
            additionalCatalogs = extraCatalogs ?? Array.Empty<Stage2DefenseCatalog>();
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
                var roles = new List<Stage2DefenseCatalog> { catalog };
                roles.AddRange(additionalCatalogs);
                defense = new Stage2Defense(mapDefinition, navigation, gathering.Economy.Inventory,
                    roles, () => player.position, selected => Instantiate(selected.TowerPrefab));
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
            firstRoleAction = new InputAction("Stage 2 Rapid Tower", InputActionType.Button, "<Keyboard>/1");
            secondRoleAction = new InputAction("Stage 2 Longshot Tower", InputActionType.Button, "<Keyboard>/2");
            buildAction.Enable();
            confirmAction.Enable();
            cancelAction.Enable();
            firstRoleAction.Enable();
            secondRoleAction.Enable();
        }

        private void OnDisable()
        {
            buildAction?.Dispose();
            confirmAction?.Dispose();
            cancelAction?.Dispose();
            firstRoleAction?.Dispose();
            secondRoleAction?.Dispose();
            buildAction = confirmAction = cancelAction = firstRoleAction = secondRoleAction = null;
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
            PointerOverUi = Mouse.current != null && pointerBlocker != null &&
                pointerBlocker(Mouse.current.position.ReadValue());
            var buildPressed = buildAction != null && buildAction.WasPressedThisFrame();
            var cancelPressed = cancelAction != null && cancelAction.WasPressedThisFrame();
            if (firstRoleAction != null && firstRoleAction.WasPressedThisFrame() && defense.SelectRole(0))
                CreateGhost();
            if (secondRoleAction != null && secondRoleAction.WasPressedThisFrame() && defense.SelectRole(1))
                CreateGhost();
            // Cancellation wins over confirmation in the same input update.
            if (cancelPressed || (buildPressed && defense.IsPlacing)) defense.CancelPlacement();
            else if (buildPressed) defense.BeginPlacement();

            if (defense.IsPlacing && Mouse.current != null)
            {
                if (PointerOverUi)
                {
                    PreviewResult = PlacementResult.InvalidCell;
                    SyncBuildMode();
                    return;
                }
                var screen = Mouse.current.position.ReadValue();
                if (!TryScreenToCell(screen, out var cell)) return;
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
            if (ghost != null) ghost.gameObject.SetActive(placing && !PointerOverUi);
        }

        private void CreateGhost()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            var source = defense.SelectedCatalog.TowerPrefab.GetComponentInChildren<SpriteRenderer>(true);
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
