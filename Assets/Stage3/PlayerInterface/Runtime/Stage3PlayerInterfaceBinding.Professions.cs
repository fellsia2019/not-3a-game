using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage2ProfessionDriver professionDriver;
        private bool professionsDirty = true, professionsWereManageable;
        public Stage2ProfessionDriver Professions => professionDriver;
        private bool CanManageProfessions => bound && professionDriver != null && !game.RunDriver.Run.IsPaused &&
            !game.RunDriver.Run.IsTerminal && !(legacy?.DebugOpen ?? false) && presentation.isActiveAndEnabled;

        private void BindProfessions()
        {
            if (equipmentDriver == null) return;
            var data = Resources.Load<Stage2ProfessionCatalog>("Stage3Professions");
            if (data == null || !data.TryValidate(out _))
            { Debug.LogError("S3_PROFESSIONS_BINDING_ERROR: missing/invalid Stage3Professions catalog."); return; }
            professionDriver = GetComponent<Stage2ProfessionDriver>() ?? gameObject.AddComponent<Stage2ProfessionDriver>();
            professionDriver.Configure(game, data);
            professionDriver.Rules.Changed += MarkProfessionsDirty;
            equipmentDriver.Rules.Changed += MarkProfessionsDirty;
            presentation.WindowChanged += MarkProfessionsDirty;
            presentation.ProfessionTalentUpgradeRequested += UpgradeProfession;
            RefreshProfessions();
            Debug.Log("S3_PROFESSIONS_READY: actual manual-material XP; miner/logger independent, retry preserves current mastery.");
        }
        private void MarkProfessionsDirty() => professionsDirty = true;
        private void UpgradeProfession(int profession, int talent) => TryUpgradeProfession(profession, talent);
        public bool TryUpgradeProfession(int profession, int talent)
        {
            if (!CanManageProfessions || presentation.ActiveWindow != Stage3PlayerWindow.Hero ||
                !presentation.IsProfessionsOpen || profession is < 0 or > 1 || talent is < 0 or > 2) return false;
            var result = professionDriver.Rules.TryUpgrade((Stage2Profession)profession, (Stage2ProfessionTalent)talent);
            RefreshProfessions(); return result;
        }
        private void UpdateProfessions()
        {
            if (professionDriver == null) return;
            var allowed = CanManageProfessions;
            if (professionsWereManageable != allowed) professionsDirty = true;
            if (professionsDirty) RefreshProfessions();
        }
        private void RefreshProfessions()
        {
            if (professionDriver == null) return;
            professionsDirty = false; professionsWereManageable = CanManageProfessions;
            var rules = professionDriver.Rules; var catalog = professionDriver.Catalog;
            var rows = new Stage3ProfessionView[2];
            for (var p = 0; p < 2; p++)
            {
                var profession = (Stage2Profession)p;
                var item = equipmentDriver.Rules.Equipped(p == 0 ? Stage2ItemSlot.Pickaxe : Stage2ItemSlot.Axe);
                var tool = item == null ? default : equipmentDriver.Catalog.Stats(item.Type, item.Quality, item.Level);
                var node = game.Gathering.Economy.GetNode(p == 0 ? MapResourceKind.Stone : MapResourceKind.Wood);
                var baseDuration = node.GatherSeconds;
                var currentPower = item == null ? 0 : tool.HarvestPower + rules.PowerBonus(profession);
                var currentTempo = rules.TempoBonus(profession); var currentReach = rules.ReachBonus(profession);
                var duration = item == null ? 0 : baseDuration / (tool.HarvestSpeed * (1 + currentTempo));
                var talents = new Stage3ProfessionTalentView[3];
                for (var t = 0; t < 3; t++)
                {
                    var talent = (Stage2ProfessionTalent)t; var rank = rules.Rank(profession, talent);
                    var next = Mathf.Min(catalog.MaxTalentRank, rank + 1); var hasNext = rank < catalog.MaxTalentRank;
                    var canUpgrade = rules.CanUpgrade(profession, talent);
                    var currentBonus = t == 0 ? rules.PowerBonus(profession) : t == 1 ? currentTempo : currentReach;
                    var perRank = t == 0 ? catalog.PowerPerRank : t == 1 ? catalog.TempoPerRank : catalog.ReachPerRank;
                    talents[t] = new Stage3ProfessionTalentView {
                        Title = t == 0 ? "Сила удара" : t == 1 ? "Темп работы" : "Досягаемость",
                        Description = t == 0 ? p == 0 ? "Больше камня и металла за удар" : "Больше дерева за удар" :
                            t == 1 ? p == 0 ? "Быстрее завершает удар" : "Быстрее завершает замах" :
                            p == 0 ? "Удобнее доставать до залежи" : "Удобнее доставать до дерева",
                        Rank = rank, MaxRank = catalog.MaxTalentRank, PointCost = catalog.RankPointCost,
                        CanUpgrade = CanManageProfessions && canUpgrade,
                        UnavailableReason = !hasNext ? "Максимальный ранг" : !canUpgrade ? "Недостаточно очков профессии" :
                            !CanManageProfessions ? "Сейчас недоступно" : string.Empty,
                        CurrentPower = currentPower,
                        NextPower = item == null ? 0 : currentPower + (t == 0 && hasNext ? catalog.PowerPerRank : 0),
                        CurrentDuration = duration,
                        NextDuration = item == null ? 0 : baseDuration / (tool.HarvestSpeed *
                            (1 + currentTempo + (t == 1 && hasNext ? catalog.TempoPerRank : 0))),
                        CurrentReachBonus = currentReach,
                        NextReachBonus = currentReach + (t == 2 && hasNext ? catalog.ReachPerRank : 0),
                        CurrentBonus = currentBonus, NextBonus = next * perRank
                    };
                }
                rows[p] = new Stage3ProfessionView {
                    Title = p == 0 ? "Рудокоп" : "Лесоруб", Description = p == 0 ? "Камень и металл" : "Дерево",
                    Mastery = rules.Mastery(profession), MaxMastery = catalog.MaxMastery,
                    XpWithin = rules.XpWithin(profession), RequiredXp = rules.RequiredXp(profession),
                    AvailablePoints = rules.AvailablePoints(profession), Talents = talents,
                    Tool = item == null ? null : ItemView(item)
                };
            }
            presentation.SetProfessions(new Stage3ProfessionsView { Professions = rows });
        }
        private void UnbindProfessions()
        {
            if (professionDriver != null) professionDriver.Rules.Changed -= MarkProfessionsDirty;
            if (equipmentDriver != null) equipmentDriver.Rules.Changed -= MarkProfessionsDirty;
            if (presentation == null) return;
            presentation.WindowChanged -= MarkProfessionsDirty;
            presentation.ProfessionTalentUpgradeRequested -= UpgradeProfession;
        }
    }
}
