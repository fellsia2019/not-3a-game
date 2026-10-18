using Not3A.Stage2.RunState;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent, DefaultExecutionOrder(100)]
    public sealed class Stage2HeroCombatDriver : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2Run run;
        public Stage2HeroCombat Rules { get; private set; }
        public Stage2HeroCombatCatalog Catalog { get; private set; }
        public bool AutomaticTick { get; set; } = true;

        public void Configure(Stage2HeroCombatCatalog catalog, Stage2GameComposition owner)
        {
            if (run != null) run.Reset -= ResetCombat;
            game = owner;
            Catalog = catalog;
            Rules = new Stage2HeroCombat(catalog, owner.Enemies.TryHitNearest);
            run = owner.RunDriver.Run;
            run.Reset += ResetCombat;
        }

        public void Tick(float elapsed)
        {
            if (Rules == null || game == null || run == null) return;
            var hud = game.GetComponent<Stage2GameHud>();
            var active = game.IsInitialized && !run.IsPaused && !run.IsTerminal &&
                !game.Gathering.IsInteracting && (hud == null || !hud.DebugOpen);
            Rules.Tick(elapsed, game.Gathering.Player.position, active);
        }

        private void Update() { if (AutomaticTick) Tick(Time.deltaTime); }
        private void ResetCombat() => Rules?.Reset();
        private void OnDestroy() { if (run != null) run.Reset -= ResetCombat; }
    }
}
