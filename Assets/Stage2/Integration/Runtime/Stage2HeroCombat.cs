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
            internal readonly float Fraction;
            internal Snapshot(float remaining, int shots,float fraction) { Remaining = remaining; Shots = shots;Fraction=fraction; }
        }
        public Snapshot CaptureSnapshot() => new Snapshot(remaining, ShotsFired,damageFraction);
        public void RestoreSnapshot(Snapshot snapshot) { remaining = snapshot.Remaining; ShotsFired = snapshot.Shots;damageFraction=snapshot.Fraction; }
        private readonly Stage2HeroCombatCatalog catalog;
        private readonly Func<Vector2, float, int, float, float, bool> launch;
        private float remaining;
        private int damageBonus;
        private float attackSpeedMultiplier = 1f;
        private bool equipmentProfile, equipped = true;
        private int weaponDamage;
        private float weaponRange, weaponInterval, weaponFlight;
        private float equipmentDamageMultiplier=1,abilityDamageMultiplier=1,abilityTempo=1,damageFraction;

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
        public float EffectiveDamageExact=>(equipmentProfile&&!equipped?0:(equipmentProfile?weaponDamage:catalog.AttackDamage)+damageBonus)*equipmentDamageMultiplier*abilityDamageMultiplier;
        public int EffectiveDamage => Mathf.FloorToInt(EffectiveDamageExact);
        public float EffectiveRange => equipmentProfile ? equipped ? weaponRange : 0 : catalog.AttackRange;
        public float EffectiveFireInterval => (equipmentProfile ? weaponInterval : catalog.FireInterval) / (attackSpeedMultiplier*abilityTempo);
        public void SetEquipmentDamageMultiplier(float value)=>equipmentDamageMultiplier=float.IsFinite(value)?Mathf.Max(1,value):1;
        public void SetAbilityBonuses(float damage,float tempo)
        {abilityDamageMultiplier=float.IsFinite(damage)?Mathf.Max(.1f,damage):1;abilityTempo=float.IsFinite(tempo)?Mathf.Clamp(tempo,1,2):1;}
        public void SetEquippedWeapon(bool enabled, int damage, float range, float interval, float flight)
        {
            if (enabled && (damage <= 0 || !float.IsFinite(range) || range <= 0 ||
                !float.IsFinite(interval) || interval <= 0 || !float.IsFinite(flight) || flight < 0))
                throw new ArgumentException("Invalid equipped weapon profile.");
            equipmentProfile = true; equipped = enabled; weaponDamage = damage;
            weaponRange = range; weaponInterval = enabled ? interval : catalog.FireInterval; weaponFlight = flight;
            // Preserve an in-flight cadence. Unequip/re-equip cannot reset the attack timer.
        }
        public void SetPersonalBonuses(int extraDamage, float attackSpeed)
        {
            damageBonus = Mathf.Max(0, extraDamage);
            attackSpeedMultiplier = float.IsFinite(attackSpeed) ? Mathf.Max(1, attackSpeed) : 1;
            remaining = Mathf.Min(remaining, EffectiveFireInterval);
        }

        public void Tick(float elapsed, Vector2 origin, bool active, bool canLaunch = true)
        {
            if (elapsed < 0f || float.IsNaN(elapsed) || float.IsInfinity(elapsed))
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (!active || elapsed == 0f) return;
            remaining = Mathf.Max(0f, remaining - elapsed);
            if (!canLaunch || equipmentProfile && !equipped) return;
            var exact=EffectiveDamageExact+damageFraction;var damage=Mathf.Max(1,Mathf.FloorToInt(exact+.00001f));
            if (remaining > 0f || !launch(origin, EffectiveRange, damage,
                    0f, equipmentProfile ? weaponFlight : catalog.ProjectileFlightSeconds)) return;
            damageFraction=Mathf.Max(0,exact-damage);
            remaining = EffectiveFireInterval;
            ShotsFired++;
        }

        public void Reset() { remaining = 0f; ShotsFired = 0; damageFraction=0; }
    }
}
