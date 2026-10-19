using System;
using Not3A.Stage2.RunState;
using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Opt-in scene lifetime: confirmed kills, personal stat binding and the existing pre-wave boundary.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage2HeroProgressionDriver : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2PlayerMover mover;
        private Stage2Run run;
        private Stage2HeroProgression.Snapshot checkpoint;
        public Stage2HeroProgression Rules { get; private set; }
        public Stage2HeroProgressionCatalog Catalog { get; private set; }
        public bool AutomaticTick { get; set; } = true;
        public Func<bool> ControlBlocked { get; set; }
        public bool CanCast => Rules != null && mover != null && run != null &&
            !run.IsPaused && !run.IsTerminal && !(ControlBlocked?.Invoke() ?? false);
        public event Action<Vector2, Vector2> Blinked;

        public void Configure(Stage2GameComposition owner, Stage2PlayerMover player, Stage2HeroProgressionCatalog catalog)
        {
            Unbind();
            game = owner; mover = player; Catalog = catalog;
            Rules = new Stage2HeroProgression(catalog);
            run = game.RunDriver.Run;
            game.Enemies.EnemyRemoved += OnEnemyRemoved;
            run.BeforeWaveStarted += CaptureCheckpoint;
            run.Rewound += RestoreCheckpoint;
            run.Reset += ResetRules;
            Rules.Changed += ApplyStats;
            checkpoint = Rules.CaptureSnapshot();
            ApplyStats();
        }
        public bool TryBlink(Vector2 cursor)
        {
            if (!CanCast) return false;
            var start = (Vector2)mover.transform.position;
            if (!Rules.TryBlink(start, cursor, Teleport, out var target)) return false;
            game.Gathering.CancelGatherUntilRelease();
            Blinked?.Invoke(start, target); return true;
        }
        public bool CanLand(Vector2 target)
        {
            if (mover == null || !mover.CanTeleportTo(target)) return false;
            var defense = game.Building.Defense;
            for (var i = 0; i < defense.TowerCount; i++)
            {
                var tower = defense.GetTower(i);
                if (tower != null && tower.IsAlive && Overlaps(new Rect((Vector2)tower.transform.position - Vector2.one * .5f, Vector2.one), target))
                    return false;
            }
            if (game.Production?.Rules != null)
                foreach (var building in game.Production.Rules.Buildings)
                    if (Overlaps(building.Footprint, target)) return false;
            return true;
        }
        private bool Overlaps(Rect area, Vector2 point)
        {
            var nearest = new Vector2(Mathf.Clamp(point.x, area.xMin, area.xMax), Mathf.Clamp(point.y, area.yMin, area.yMax));
            return (point - nearest).sqrMagnitude < mover.CollisionRadius * mover.CollisionRadius;
        }
        private bool Teleport(Vector2 target) => CanLand(target) && mover.TryTeleportTo(target);
        public void Tick(float elapsed) => Rules?.Tick(elapsed, run != null && !run.IsPaused && !run.IsTerminal);
        private void Update() { if (AutomaticTick) Tick(Time.deltaTime); }
        private void OnEnemyRemoved(Stage2Enemy enemy)
        {
            // Reset/despawn/arrival are not kills; the population removes once before emitting this event.
            if (enemy.WasKilled) Rules.AwardExperience(Catalog.XpPerKill);
        }
        private void ApplyStats()
        {
            mover.SetPersonalSpeedMultiplier(Rules.MovementSpeedMultiplier);
            game.HeroCombat?.Rules.SetPersonalBonuses(Rules.DamageBonus, Rules.AttackSpeedMultiplier);
        }
        private void CaptureCheckpoint() => checkpoint = Rules.CaptureSnapshot();
        private void RestoreCheckpoint() => Rules.RestoreSnapshot(checkpoint);
        private void ResetRules() => Rules.Reset();
        private void Unbind()
        {
            if (Rules != null) Rules.Changed -= ApplyStats;
            if (run == null) return;
            game.Enemies.EnemyRemoved -= OnEnemyRemoved;
            run.BeforeWaveStarted -= CaptureCheckpoint; run.Rewound -= RestoreCheckpoint; run.Reset -= ResetRules;
            run = null;
        }
        private void OnDestroy() => Unbind();
    }
}
