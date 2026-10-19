using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>First playable tuning. Values belong to the scene's data asset, not the HUD.</summary>
    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Hero Progression Catalog")]
    public sealed class Stage2HeroProgressionCatalog : ScriptableObject
    {
        [SerializeField, Range(2, 1000)] private int maxLevel = 100;
        [SerializeField, Min(1)] private int firstLevelXp = 100;
        [SerializeField, Min(0)] private int xpIncreasePerLevel = 50;
        [SerializeField, Min(1)] private int xpPerKill = 12;
        [SerializeField, Min(1)] private int pointsPerLevel = 1;
        [SerializeField, Min(1)] private int rankPointCost = 1;
        [SerializeField] private float[] blinkRanges = { 300, 500, 800, 1000 };
        [SerializeField, Min(.01f)] private float rangeUnitsPerWorldUnit = 100;
        [SerializeField, Min(.01f)] private float blinkCooldown = 6;
        [SerializeField, Range(1, 10)] private int passiveMaxRank = 3;
        [SerializeField, Min(1)] private int damagePerRank = 1;
        [SerializeField, Min(0)] private float attackSpeedPerRank = .10f;
        [SerializeField, Min(0)] private float movementSpeedPerRank = .05f;

        public int MaxLevel => maxLevel;
        public int XpPerKill => xpPerKill;
        public int PointsPerLevel => pointsPerLevel;
        public int RankPointCost => rankPointCost;
        public int BlinkMaxRank => blinkRanges.Length;
        public int PassiveMaxRank => passiveMaxRank;
        public int DamagePerRank => damagePerRank;
        public float AttackSpeedPerRank => attackSpeedPerRank;
        public float MovementSpeedPerRank => movementSpeedPerRank;
        public float BlinkCooldown => blinkCooldown;
        public int RequiredXp(int level) => level >= maxLevel ? 0 :
            (int)System.Math.Min(int.MaxValue, (long)firstLevelXp + (long)(Mathf.Max(1, level) - 1) * xpIncreasePerLevel);
        public float BlinkDisplayRange(int rank) => blinkRanges[Mathf.Clamp(rank, 1, BlinkMaxRank) - 1];
        public float BlinkWorldRange(int rank) => BlinkDisplayRange(rank) / rangeUnitsPerWorldUnit;

        public bool TryValidate(out string reason)
        {
            reason = "Progression levels, XP, points, ranks and finite positive Blink ranges/cooldown are required.";
            if (maxLevel < 2 || maxLevel > 1000 || firstLevelXp < 1 || xpIncreasePerLevel < 0 ||
                xpPerKill < 1 || pointsPerLevel < 1 || rankPointCost < 1 || passiveMaxRank < 1 ||
                damagePerRank < 1 || !float.IsFinite(attackSpeedPerRank) || attackSpeedPerRank < 0 ||
                !float.IsFinite(movementSpeedPerRank) || movementSpeedPerRank < 0 ||
                !float.IsFinite(rangeUnitsPerWorldUnit) || rangeUnitsPerWorldUnit <= 0 ||
                !float.IsFinite(blinkCooldown) || blinkCooldown <= 0 || blinkRanges == null || blinkRanges.Length != 4)
                return false;
            var previous = 0f;
            foreach (var range in blinkRanges)
            {
                if (!float.IsFinite(range) || range <= previous) return false;
                previous = range;
            }
            reason = string.Empty;
            return true;
        }
    }
}
