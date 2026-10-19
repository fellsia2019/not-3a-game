using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Economy
{
    [DisallowMultipleComponent]
    public sealed class Stage2GatheringDriver : MonoBehaviour
    {
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField] private Stage2EconomyCatalog catalog;
        [SerializeField] private Transform player;

        private InputAction gatherAction;
        private bool activeRun = true;
        private bool paused;
        private bool buildMode;
        private bool productionBuildMode;
        private bool requiresRelease;
        private Stage2Economy economy;
        private Vector2 previousPlayerPosition;
        private System.Func<bool> inputBlocker;
        public void SetInputBlocker(System.Func<bool> blocker) => inputBlocker = blocker;

        public Stage2MapDefinition MapDefinition => mapDefinition;
        public Stage2EconomyCatalog Catalog => catalog;
        public Transform Player => player;
        public Stage2Economy Economy => economy;
        public Stage2ResourceNode GatheringNode => economy?.CurrentNode;
        public Stage2ResourceNode InteractionTarget
        {
            get
            {
                if (!isActiveAndEnabled || economy == null || player == null || !activeRun || paused ||
                    buildMode || productionBuildMode || (inputBlocker?.Invoke() ?? false)) return null;
                var current = economy.CurrentNode;
                return current != null && !current.IsDepleted && economy.IsInRange(current, player.position)
                    ? current : economy.NearestAvailableNode(player.position);
            }
        }
        public bool IsInteracting => buildMode || productionBuildMode ||
            (gatherAction != null && gatherAction.IsPressed());

        public void Configure(Stage2MapDefinition definition, Stage2EconomyCatalog data, Transform playerTransform)
        {
            mapDefinition = definition;
            catalog = data;
            player = playerTransform;
            Initialize();
        }

        public bool Initialize()
        {
            if (mapDefinition == null || catalog == null || player == null)
            {
                Debug.LogError("S2_ECO_BINDING_ERROR: map, catalog and player are required.", this);
                return false;
            }
            try
            {
                economy = new Stage2Economy(mapDefinition, catalog);
                previousPlayerPosition = player.position;
                requiresRelease = true;
                return true;
            }
            catch (System.ArgumentException exception)
            {
                Debug.LogError($"S2_ECO_BINDING_ERROR: {exception.Message}", this);
                return false;
            }
        }

        public void SetRunActivity(bool running, bool isPaused)
        {
            activeRun = running;
            paused = isPaused;
            if (!running || isPaused)
            {
                economy?.CancelGather();
                requiresRelease = true;
            }
        }

        public void SetBuildMode(bool active)
        {
            buildMode = active;
            if (active)
            {
                economy?.CancelGather();
                requiresRelease = true;
            }
        }

        public void SetProductionBuildMode(bool value)
        {
            productionBuildMode = value;
            if (value) { economy?.CancelGather(); requiresRelease = true; }
        }

        public void ResetRun()
        {
            economy?.Reset();
            activeRun = true;
            paused = false;
            buildMode = false;
            productionBuildMode = false;
            requiresRelease = true;
        }

        public void CancelGatherUntilRelease()
        {
            economy?.CancelGather();
            requiresRelease = true;
        }

        private void Start()
        {
            if (economy == null) Initialize();
        }

        private void OnEnable()
        {
            gatherAction = new InputAction("Stage 2 Gather", InputActionType.Button, "<Keyboard>/e");
            gatherAction.Enable();
        }

        private void OnDisable()
        {
            gatherAction?.Dispose();
            gatherAction = null;
            economy?.CancelGather();
            requiresRelease = true;
        }

        private void Update()
        {
            if (economy == null || player == null) return;
            var position = (Vector2)player.position;
            var moved = (position - previousPlayerPosition).sqrMagnitude > .000001f;
            previousPlayerPosition = position;
            var activeTime = activeRun && !paused;
            economy.Tick(Time.deltaTime, activeTime);
            var held = gatherAction != null && gatherAction.IsPressed();
            if (inputBlocker?.Invoke() ?? false)
            {
                economy.CancelGather();
                requiresRelease |= held;
                return;
            }
            if (!held) requiresRelease = false;
            var allowed = activeTime && !buildMode && !productionBuildMode && !requiresRelease &&
                (catalog.ManualResources == null || !moved);
            var nearest = InteractionTarget;
            if (!allowed || !held || nearest == null)
            {
                economy.CancelGather();
                return;
            }
            economy.AdvanceGather(nearest, player.position, true, Time.deltaTime, true);
        }
    }
}
