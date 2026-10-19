using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Automatic bow cadence. The shared enemy group owns selection, flight and damage.</summary>
    public sealed class Stage2HeroCombat
    {
        public readonly struct Snapshot
        {
            internal readonly float Remaining;
            internal readonly int Shots;
            internal Snapshot(float remaining, int shots) { Remaining = remaining; Shots = shots; }
        }
        public Snapshot CaptureSnapshot() => new Snapshot(remaining, ShotsFired);
        public void RestoreSnapshot(Snapshot snapshot) { remaining = snapshot.Remaining; ShotsFired = snapshot.Shots; }
        private readonly Stage2HeroCombatCatalog catalog;
        private readonly Func<Vector2, float, int, float, float, bool> launch;
        private float remaining;
        private int damageBonus;
        private float attackSpeedMultiplier = 1f;

        public Stage2HeroCombat(Stage2HeroCombatCatalog settings,
            Func<Vector2, float, int, float, float, bool> launchNearest)
        {
            if (settings == null || !settings.TryValidate(out _))
                throw new ArgumentException("Invalid hero combat catalog.", nameof(settings));
            catalog = settings;
            launch = launchNearest ?? throw new ArgumentNullException(nameof(launchNearest));
        }

        public int ShotsFired { get; private set; }
        public float CooldownRemaining => remaining;
        public int EffectiveDamage => catalog.AttackDamage + damageBonus;
        public float EffectiveFireInterval => catalog.FireInterval / attackSpeedMultiplier;
        public void SetPersonalBonuses(int extraDamage, float attackSpeed)
        {
            damageBonus = Mathf.Max(0, extraDamage);
            attackSpeedMultiplier = float.IsFinite(attackSpeed) ? Mathf.Max(1, attackSpeed) : 1;
            remaining = Mathf.Min(remaining, EffectiveFireInterval);
        }

        public void Tick(float elapsed, Vector2 origin, bool active)
        {
            if (elapsed < 0f || float.IsNaN(elapsed) || float.IsInfinity(elapsed))
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (!active || elapsed == 0f) return;
            remaining = Mathf.Max(0f, remaining - elapsed);
            if (remaining > 0f || !launch(origin, catalog.AttackRange, EffectiveDamage,
                    0f, catalog.ProjectileFlightSeconds)) return;
            remaining = EffectiveFireInterval;
            ShotsFired++;
        }

        public void Reset() { remaining = 0f; ShotsFired = 0; }
    }
}
