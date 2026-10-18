using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Automatic bow cadence. The shared enemy group owns selection, flight and damage.</summary>
    public sealed class Stage2HeroCombat
    {
        private readonly Stage2HeroCombatCatalog catalog;
        private readonly Func<Vector2, float, int, float, float, bool> launch;
        private float remaining;

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

        public void Tick(float elapsed, Vector2 origin, bool active)
        {
            if (elapsed < 0f || float.IsNaN(elapsed) || float.IsInfinity(elapsed))
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (!active || elapsed == 0f) return;
            remaining = Mathf.Max(0f, remaining - elapsed);
            if (remaining > 0f || !launch(origin, catalog.AttackRange, catalog.AttackDamage,
                    0f, catalog.ProjectileFlightSeconds)) return;
            remaining = catalog.FireInterval;
            ShotsFired++;
        }

        public void Reset() { remaining = 0f; ShotsFired = 0; }
    }
}
