using System;
using Not3A.Stage2.RunState;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent, DefaultExecutionOrder(100)]
    public sealed class Stage2HeroCombatDriver : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2Run run;
        private Stage2PlayerMover mover;
        private Stage2HeroCombat.Snapshot checkpoint;
        public Stage2HeroCombat Rules { get; private set; }
        public Stage2HeroCombatCatalog Catalog { get; private set; }
        public bool AutomaticTick { get; set; } = true;
        // Optional world integration shares this cadence; the default remains the portal enemy group.
        public Func<Vector2,float,int,float,float,bool> TargetingOverride { get; set; }

        public void Configure(Stage2HeroCombatCatalog catalog, Stage2GameComposition owner)
        {
            if (run != null) Unbind();
            game = owner;
            mover = owner.Gathering.Player.GetComponent<Stage2PlayerMover>();
            Catalog = catalog;
            Rules = new Stage2HeroCombat(catalog, (origin,range,damage,splash,flight) =>
                TargetingOverride != null ? TargetingOverride(origin,range,damage,splash,flight) :
                owner.Enemies.TryHitNearest(origin,range,damage,splash,flight));
            run = owner.RunDriver.Run;
            run.Reset += ResetCombat;
            run.BeforeWaveStarted += CaptureCheckpoint;
            run.Rewound += RestoreCheckpoint;
        }

        public void Tick(float elapsed)
        {
            if (Rules == null || game == null || run == null) return;
            var hud = game.GetComponent<Stage2GameHud>();
            var active = game.IsInitialized && !run.IsPaused && !run.IsTerminal &&
                !game.Gathering.IsInteracting && (hud == null || !hud.DebugOpen);
            Rules.Tick(elapsed, game.Gathering.Player.position, active, mover == null || !mover.IsMoving);
        }

        private void Update() { if (AutomaticTick) Tick(Time.deltaTime); }
        private void ResetCombat() => Rules?.Reset();
        private void CaptureCheckpoint() => checkpoint = Rules.CaptureSnapshot();
        private void RestoreCheckpoint() { if (!run.PreserveProgressOnRetry) Rules.RestoreSnapshot(checkpoint); }
        private void Unbind()
        { run.Reset -= ResetCombat; run.BeforeWaveStarted -= CaptureCheckpoint; run.Rewound -= RestoreCheckpoint; }
        private void OnDestroy() { if (run != null) Unbind(); }
    }
}
