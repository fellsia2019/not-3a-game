using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Opt-in Stage3 lifetime. Retry preserves current mastery; only full Reset clears it.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage2ProfessionDriver : MonoBehaviour
    {
        private Stage2GameComposition game;
        public Stage2Professions Rules { get; private set; }
        public Stage2ProfessionCatalog Catalog { get; private set; }
        public void Configure(Stage2GameComposition owner, Stage2ProfessionCatalog data)
        {
            Unbind(); game = owner; Catalog = data; Rules = new Stage2Professions(data);
            game.Gathering.Economy.Gathered += OnGathered;
            game.RunDriver.Run.Reset += ResetRules;
            Rules.BonusesChanged += ApplyBonuses; ApplyBonuses();
        }
        private void OnGathered(MapResourceKind kind, int grant)
        {
            if (!game.RunDriver.Run.IsPaused && !game.RunDriver.Run.IsTerminal) Rules.AwardMaterial(kind, grant);
        }
        private void ResetRules() => Rules.Reset();
        private void ApplyBonuses()
        {
            game.Gathering.CancelGatherUntilRelease();
            game.Gathering.Economy.SetManualProfessionStats(Rules.PowerBonus(Stage2Profession.Logger),
                Rules.PowerBonus(Stage2Profession.Miner), 1 + Rules.TempoBonus(Stage2Profession.Logger),
                1 + Rules.TempoBonus(Stage2Profession.Miner), 1 + Rules.ReachBonus(Stage2Profession.Logger),
                1 + Rules.ReachBonus(Stage2Profession.Miner));
        }
        private void Unbind()
        {
            if (Rules != null) Rules.BonusesChanged -= ApplyBonuses;
            if (game == null) return;
            game.Gathering.Economy.Gathered -= OnGathered;
            game.RunDriver.Run.Reset -= ResetRules; game = null;
        }
        private void OnDestroy() => Unbind();
    }
}
