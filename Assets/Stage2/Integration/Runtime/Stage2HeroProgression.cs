using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public enum Stage2HeroPassive { PreciseStrike, QuickHands, LightStep }

    /// <summary>World-owned progression and Blink cadence; no renderer, input or enemy lifetime ownership.</summary>
    public sealed class Stage2HeroProgression
    {
        public readonly struct Snapshot
        {
            internal readonly int Level, Xp, Points, Blink, Damage, Attack, Movement;
            internal readonly float Cooldown;
            internal Snapshot(Stage2HeroProgression r)
            {
                Level = r.Level; Xp = r.XpWithin; Points = r.AvailablePoints; Blink = r.BlinkRank;
                Damage = r.ranks[0]; Attack = r.ranks[1]; Movement = r.ranks[2]; Cooldown = r.BlinkCooldownRemaining;
            }
        }

        private readonly Stage2HeroProgressionCatalog catalog;
        private readonly int[] ranks = new int[3];
        public Stage2HeroProgression(Stage2HeroProgressionCatalog settings)
        {
            if (settings == null || !settings.TryValidate(out _)) throw new ArgumentException("Invalid progression catalog.", nameof(settings));
            catalog = settings;
            Reset();
        }
        public int Level { get; private set; }
        public int XpWithin { get; private set; }
        public int RequiredXp => catalog.RequiredXp(Level);
        public int AvailablePoints { get; private set; }
        public int BlinkRank { get; private set; }
        public float BlinkCooldownRemaining { get; private set; }
        public float BlinkRange => catalog.BlinkWorldRange(BlinkRank);
        public bool CanUpgradeBlink => BlinkRank < catalog.BlinkMaxRank && AvailablePoints >= catalog.RankPointCost;
        public int DamageBonus => ranks[0] * catalog.DamagePerRank;
        public float AttackSpeedMultiplier => 1f + ranks[1] * catalog.AttackSpeedPerRank;
        public float MovementSpeedMultiplier => 1f + ranks[2] * catalog.MovementSpeedPerRank;
        public event Action Changed;
        public event Action<int> ExperienceGained;
        /// <summary>Final level of a single award, coalescing multiple levels into one celebration.</summary>
        public event Action<int> LevelRaised;
        public int PassiveRank(Stage2HeroPassive passive) => ranks[ValidIndex(passive)];
        public bool CanUpgradePassive(Stage2HeroPassive passive) => PassiveRank(passive) < catalog.PassiveMaxRank &&
            AvailablePoints >= catalog.RankPointCost;

        public void AwardExperience(int amount)
        {
            if (amount <= 0 || Level >= catalog.MaxLevel) return;
            var before = Level;
            long remaining = (long)XpWithin + amount;
            while (Level < catalog.MaxLevel && remaining >= catalog.RequiredXp(Level))
            {
                remaining -= catalog.RequiredXp(Level++);
                AvailablePoints += catalog.PointsPerLevel;
            }
            XpWithin = Level == catalog.MaxLevel ? 0 : (int)remaining;
            Changed?.Invoke();
            ExperienceGained?.Invoke(amount);
            if (Level > before) LevelRaised?.Invoke(Level);
        }

        public bool TryUpgradeBlink()
        {
            if (!CanUpgradeBlink) return false;
            AvailablePoints -= catalog.RankPointCost; BlinkRank++; Changed?.Invoke(); return true;
        }
        public bool TryUpgradePassive(Stage2HeroPassive passive)
        {
            if (!CanUpgradePassive(passive)) return false;
            AvailablePoints -= catalog.RankPointCost; ranks[(int)passive]++; Changed?.Invoke(); return true;
        }
        public bool TryGetBlinkLanding(Vector2 origin, Vector2 cursor, Func<Vector2, bool> landingFits, out Vector2 landing)
        {
            landing = origin;
            if (!Finite(origin) || !Finite(cursor) || landingFits == null) return false;
            landing = origin + Vector2.ClampMagnitude(cursor - origin, BlinkRange);
            return (landing - origin).sqrMagnitude > .0001f && landingFits(landing);
        }
        public bool TryBlink(Vector2 origin, Vector2 cursor, Func<Vector2, bool> teleport, out Vector2 landing)
        {
            landing = origin;
            if (BlinkCooldownRemaining > 0 || !TryGetBlinkLanding(origin, cursor, _ => true, out var target) ||
                teleport == null || !teleport(target)) return false;
            landing = target; BlinkCooldownRemaining = catalog.BlinkCooldown; Changed?.Invoke(); return true;
        }
        public void Tick(float elapsed, bool active)
        {
            if (!float.IsFinite(elapsed) || elapsed < 0) throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (!active) return;
            BlinkCooldownRemaining = Mathf.Max(0, BlinkCooldownRemaining - elapsed);
        }
        public Snapshot CaptureSnapshot() => new Snapshot(this);
        public void RestoreSnapshot(Snapshot snapshot)
        {
            Level = snapshot.Level; XpWithin = snapshot.Xp; AvailablePoints = snapshot.Points; BlinkRank = snapshot.Blink;
            ranks[0] = snapshot.Damage; ranks[1] = snapshot.Attack; ranks[2] = snapshot.Movement;
            BlinkCooldownRemaining = snapshot.Cooldown; Changed?.Invoke();
        }
        public void Reset()
        {
            Level = 1; XpWithin = 0; AvailablePoints = 0; BlinkRank = 1; BlinkCooldownRemaining = 0;
            Array.Clear(ranks, 0, ranks.Length); Changed?.Invoke();
        }
        private static int ValidIndex(Stage2HeroPassive passive) => (int)passive is >= 0 and < 3 ? (int)passive :
            throw new ArgumentOutOfRangeException(nameof(passive));
        private static bool Finite(Vector2 v) => float.IsFinite(v.x) && float.IsFinite(v.y);
    }
}
