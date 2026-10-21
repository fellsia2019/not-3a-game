using Not3A.Stage2.Integration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Local E at the existing invasion portal; rules and cadence belong to Run.</summary>
    public sealed class Stage3DaytimeGate : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud hud;
        private InputAction interact;
        private bool requiresRelease = true;
        public bool NearGate => scene != null && Vector2.Distance(scene.PlayerMover.transform.position,
            scene.EntranceMarker.position) <= 4f;
        public bool CanInteract => game != null && NearGate && !game.PlayerInputBlocked &&
            !game.RunDriver.Run.IsPaused && !game.RunDriver.Run.IsTerminal && Time.timeScale > 0 &&
            !(hud?.DebugOpen ?? false) && !game.Building.Defense.IsPlacing &&
            !(game.Production?.IsPlacing ?? false) &&
            !(GetComponent<VisualPilot.Stage3WorldTravel>()?.IsPlacingWorkshop ?? false) &&
            !(GetComponent<VisualPilot.Stage3WorldTravel>()?.IsPlacingBaseCircle ?? false) &&
            !(GetComponent<VisualPilot.Stage3Dwelling>()?.IsPlacing ?? false);
        private void Awake()
        {
            game = GetComponent<Stage2GameComposition>(); scene = GetComponent<Stage2SceneBootstrap>();
            hud = GetComponent<Stage2GameHud>();
            interact = new InputAction("Toggle daytime gate", InputActionType.Button, "<Keyboard>/e");
            interact.Enable();
            game.RunDriver.Run.Rewound += Latch; game.RunDriver.Run.Reset += Latch;
        }
        public bool TryInteract()
        {
            if (!CanInteract) return false;
            game.Gathering.CancelGatherUntilRelease();
            var run = game.RunDriver.Run;
            return run.DaytimeGateOpen ? run.CloseDaytimeGate() : run.OpenDaytimeGate();
        }
        private void Latch() => requiresRelease = true;
        private void Update()
        {
            var held = interact.IsPressed();
            if (!held) requiresRelease = false;
            if (!CanInteract) { requiresRelease |= held; if (hud != null) hud.PortalInteractionHint = null; return; }
            var run = game.RunDriver.Run;
            hud.PortalInteractionHint = run.DaytimeGateOpen ? $"E · Закрыть врата · Угроза {run.LastClearedTier}" :
                run.CanOpenDaytimeGate ? $"E · Открыть врата · Угроза {run.LastClearedTier}" :
                run.LastClearedTier == 0 ? "Дневные врата откроются после первой победы" :
                "Дневные врата закрыты до следующего дня";
            if (!requiresRelease && interact.WasPressedThisFrame()) { TryInteract(); requiresRelease = true; }
        }
        private void OnDisable() { if (hud != null) hud.PortalInteractionHint = null; requiresRelease = true; }
        private void OnDestroy()
        {
            interact?.Dispose();
            if (game?.RunDriver?.Run != null)
            { game.RunDriver.Run.Rewound -= Latch; game.RunDriver.Run.Reset -= Latch; }
        }
    }
}
