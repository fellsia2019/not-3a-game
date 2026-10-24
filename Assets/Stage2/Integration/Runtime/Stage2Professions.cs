using System;
using Not3A.Stage2.Map;

namespace Not3A.Stage2.Integration
{
    /// <summary>Two current-world mastery tracks; no wallet, renderer, input or automatic production.</summary>
    public sealed class Stage2Professions
    {
        private readonly Stage2ProfessionCatalog catalog;
        private readonly Stage2HeroProgression progression;
        private readonly int[] mastery = { 1, 1 }, xp = new int[2];
        private readonly int[,] ranks = new int[2, 3];
        private readonly bool[] capstones = new bool[2];
        private bool upgrading;
        public event Action Changed;
        public event Action BonusesChanged;
        public Stage2Professions(Stage2ProfessionCatalog data, Stage2HeroProgression progression)
        {
            if (data == null || !data.TryValidate(out _)) throw new ArgumentException("Invalid profession catalog.", nameof(data));
            catalog = data;
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));
        }
        public int Mastery(Stage2Profession profession) => mastery[Index(profession)];
        public int XpWithin(Stage2Profession profession) => xp[Index(profession)];
        public int RequiredXp(Stage2Profession profession) => catalog.RequiredXp(Mastery(profession));
        public int AvailablePoints(Stage2Profession profession) { Index(profession); return progression.AvailablePoints; }
        public int Rank(Stage2Profession profession, Stage2ProfessionTalent talent) => ranks[Index(profession), Index(talent)];
        public int PowerBonus(Stage2Profession profession) => Rank(profession, Stage2ProfessionTalent.Strength) * catalog.PowerPerRank;
        public float TempoBonus(Stage2Profession profession) => Rank(profession, Stage2ProfessionTalent.Tempo) * catalog.TempoPerRank;
        public float ReachBonus(Stage2Profession profession) => Rank(profession, Stage2ProfessionTalent.Reach) * catalog.ReachPerRank;
        public bool CanUpgrade(Stage2Profession profession, Stage2ProfessionTalent talent) =>
            !upgrading && Rank(profession, talent) < catalog.TalentMaxRank(talent) && AvailablePoints(profession) >= catalog.RankPointCost;
        public bool HasCapstone(Stage2Profession profession) => capstones[Index(profession)];
        public float CapstoneChance(Stage2Profession profession) => HasCapstone(profession) ? catalog.CapstoneChance : 0;
        public bool CanUpgradeCapstone(Stage2Profession profession) => !upgrading && catalog.CapstoneChance > 0 &&
            !HasCapstone(profession) && Rank(profession, Stage2ProfessionTalent.Strength) >= catalog.TalentMaxRank(Stage2ProfessionTalent.Strength) &&
            AvailablePoints(profession) >= catalog.RankPointCost;
        public bool TryUpgradeCapstone(Stage2Profession profession)
        {
            if (!CanUpgradeCapstone(profession)) return false;
            upgrading = true;
            try
            {
                if (!progression.TrySpendPoints(catalog.RankPointCost)) return false;
                capstones[Index(profession)] = true;
                BonusesChanged?.Invoke(); Changed?.Invoke(); return true;
            }
            finally { upgrading = false; }
        }

        public bool TryUpgrade(Stage2Profession profession, Stage2ProfessionTalent talent)
        {
            if (!CanUpgrade(profession, talent)) return false;
            upgrading = true;
            try
            {
                if (!progression.TrySpendPoints(catalog.RankPointCost)) return false;
                ranks[(int)profession, (int)talent]++;
                BonusesChanged?.Invoke(); Changed?.Invoke(); return true;
            }
            finally { upgrading = false; }
        }
        public void AwardMaterial(MapResourceKind kind, int actualGrant)
        {
            if (actualGrant <= 0 || (int)kind < 0 || (int)kind > 2) return;
            var i = kind == MapResourceKind.Wood ? 1 : 0;
            if (mastery[i] >= catalog.MaxMastery) return;
            long remaining = (long)xp[i] + (long)actualGrant * catalog.XpPerMaterial;
            while (mastery[i] < catalog.MaxMastery && remaining >= catalog.RequiredXp(mastery[i]))
            {
                remaining -= catalog.RequiredXp(mastery[i]++);
            }
            xp[i] = mastery[i] == catalog.MaxMastery ? 0 : (int)remaining;
            Changed?.Invoke();
        }
        public void Reset()
        {
            mastery[0] = mastery[1] = 1; Array.Clear(xp, 0, xp.Length);
            Array.Clear(ranks, 0, ranks.Length); Array.Clear(capstones, 0, capstones.Length);
            BonusesChanged?.Invoke(); Changed?.Invoke();
        }
        private static int Index(Stage2Profession value) => (int)value is >= 0 and < 2 ? (int)value :
            throw new ArgumentOutOfRangeException(nameof(value));
        private static int Index(Stage2ProfessionTalent value) => (int)value is >= 0 and < 3 ? (int)value :
            throw new ArgumentOutOfRangeException(nameof(value));
    }
}
