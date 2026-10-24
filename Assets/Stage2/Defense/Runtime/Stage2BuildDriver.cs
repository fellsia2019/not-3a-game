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
        private Func<bool> inputBlocker;
        private Func<int,bool> roleAvailable;
        public void SetRoleAvailability(Func<int,bool> available)
        {roleAvailable=available;defense?.SetRoleAvailability(available);}
        public bool UseBuildShortcut { get; set; } = true;
        public bool UseRoleShortcuts { get; set; } = true;
        public bool RepeatPlacementWithShift { get; set; }
        public void SetInputBlocker(Func<bool> blocker) => inputBlocker = blocker;

        public Stage2Defense Defense => defense;
        public PlacementResult PreviewResult { get; private set; } = PlacementResult.NotPlacing;
        public SpriteRenderer PreviewRenderer => ghost;
        public bool PointerOverUi { get; private set; }

        public event Action PlacementStarting;

        public void CancelPlacement() { defense?.CancelPlacement(); SyncBuildMode(); }

        public void SetPointerBlocker(Func<Vector2, bool> blocker) => pointerBlocker = blocker;

        public void SelectAndBeginPlacement(int role)
        {
            if (!CanBeginPlacement(role) || !defense.SelectRole(role)) return;
            PlacementStarting?.Invoke();
            CreateGhost();
            defense.BeginPlacement();
            SyncBuildMode();
        }

        public bool CanBeginPlacement(int role)
        {
            if (defense == null || paused || (inputBlocker?.Invoke() ?? false) || phase == Stage2BuildPhase.Inactive || !defense.IsRoleAvailable(role)) return false;
            var data = defense.GetCatalog(role);
            return gathering.Economy.Inventory.CanAfford(data.WoodCost, data.StoneCost, data.MetalCost, data.GoldCost);
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
                defense.SetRoleAvailability(roleAvailable);
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
            if (inputBlocker?.Invoke() ?? false)
            {
                defense.CancelPlacement();
                PreviewResult = PlacementResult.NotPlacing;
                SyncBuildMode();
                return;
            }
            var buildPressed = UseBuildShortcut && buildAction != null && buildAction.WasPressedThisFrame();
            var cancelPressed = cancelAction != null && cancelAction.WasPressedThisFrame();
            // Legacy role keys are opt-out when the current HUD owns 1–5 for hero skills.
            // Neither shortcuts nor the public HUD action may bypass the full-price gate.
            if (UseRoleShortcuts && firstRoleAction != null && firstRoleAction.WasPressedThisFrame() && CanBeginPlacement(0))
            { PlacementStarting?.Invoke(); defense.SelectRole(0); CreateGhost(); }
            if (UseRoleShortcuts && secondRoleAction != null && secondRoleAction.WasPressedThisFrame() && CanBeginPlacement(1))
            { PlacementStarting?.Invoke(); defense.SelectRole(1); CreateGhost(); }
            // Cancellation wins over confirmation in the same input update.
            if (cancelPressed || (buildPressed && defense.IsPlacing)) defense.CancelPlacement();
            else if (buildPressed) SelectAndBeginPlacement(defense.SelectedRoleIndex);

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
                        ? Stage2PlacementVisuals.Valid
                        : Stage2PlacementVisuals.Invalid;
                }
                if (!cancelPressed && confirmAction != null && confirmAction.WasPressedThisFrame())
                {
                    PreviewResult = defense.TryConfirm(out _);
                    var keyboard = Keyboard.current;
                    if (PreviewResult == PlacementResult.Ready && RepeatPlacementWithShift && keyboard != null &&
                        (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed) &&
                        CanBeginPlacement(defense.SelectedRoleIndex))
                        defense.BeginPlacement();
                }
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
