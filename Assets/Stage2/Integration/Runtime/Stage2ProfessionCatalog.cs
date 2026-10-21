using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public enum Stage2Profession { Miner, Logger }
    public enum Stage2ProfessionTalent { Strength, Tempo, Reach }

    /// <summary>Trial gathering tuning, separate from hero combat and item levels.</summary>
    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Profession Catalog")]
    public sealed class Stage2ProfessionCatalog : ScriptableObject
    {
        [SerializeField] private int maxMastery = 100;
        [SerializeField, Min(1)] private int firstLevelXp = 20;
        [SerializeField, Min(0)] private int xpIncreasePerLevel = 5;
        [SerializeField, Min(1)] private int xpPerMaterial = 1;
        [SerializeField, Min(1)] private int pointEveryLevels = 5;
        [SerializeField, Min(1)] private int rankPointCost = 1;
        [SerializeField] private int maxTalentRank = 3;
        [SerializeField, Min(1)] private int powerPerRank = 1;
        [SerializeField, Min(0)] private float tempoPerRank = .05f;
        [SerializeField, Min(0)] private float reachPerRank = .10f;
        public int MaxMastery => maxMastery;
        public int XpPerMaterial => xpPerMaterial;
        public int PointEveryLevels => pointEveryLevels;
        public int RankPointCost => rankPointCost;
        public int MaxTalentRank => maxTalentRank;
        public int PowerPerRank => powerPerRank;
        public float TempoPerRank => tempoPerRank;
        public float ReachPerRank => reachPerRank;
        public int RequiredXp(int mastery) => mastery >= maxMastery ? 0 :
            (int)Math.Min(int.MaxValue, (long)firstLevelXp + (long)(Mathf.Max(1, mastery) - 1) * xpIncreasePerLevel);
        public bool TryValidate(out string reason)
        {
            reason = "Profession data requires mastery cap100,3 ranks, positive XP/points and finite nonnegative bonuses.";
            if (maxMastery != 100 || maxTalentRank != 3 || firstLevelXp < 1 || xpIncreasePerLevel < 0 ||
                xpPerMaterial < 1 || pointEveryLevels < 2 || pointEveryLevels > maxMastery ||
                rankPointCost < 1 || powerPerRank < 1 || powerPerRank > int.MaxValue / maxTalentRank ||
                !float.IsFinite(tempoPerRank) || tempoPerRank < 0 || tempoPerRank > 10 ||
                !float.IsFinite(reachPerRank) || reachPerRank < 0 || reachPerRank > 10) return false;
            reason = string.Empty; return true;
        }
    }
}
