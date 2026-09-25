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
        private bool requiresRelease;
        private Stage2Economy economy;

        public Stage2MapDefinition MapDefinition => mapDefinition;
        public Stage2EconomyCatalog Catalog => catalog;
        public Transform Player => player;
        public Stage2Economy Economy => economy;

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

        public void ResetRun()
        {
            economy?.Reset();
            activeRun = true;
            paused = false;
            buildMode = false;
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
            var activeTime = activeRun && !paused;
            economy.Tick(Time.deltaTime, activeTime);
            var held = gatherAction != null && gatherAction.IsPressed();
            if (!held) requiresRelease = false;
            var allowed = activeTime && !buildMode && !requiresRelease;
            var nearest = NearestAvailableNode();
            if (!allowed || !held || !nearest.HasValue)
            {
                economy.CancelGather();
                return;
            }
            economy.AdvanceGather(nearest.Value, player.position, true, Time.deltaTime, true);
        }

        private MapResourceKind? NearestAvailableNode()
        {
            MapResourceKind? nearest = null;
            var best = catalog.InteractionRange * catalog.InteractionRange;
            for (var index = 0; index < 3; index++)
            {
                var kind = (MapResourceKind)index;
                if (economy.GetNode(kind).IsDepleted) continue;
                var distance = (economy.WorldPosition(kind) - (Vector2)player.position).sqrMagnitude;
                if (distance > best) continue;
                best = distance;
                nearest = kind;
            }
            return nearest;
        }
    }
}
