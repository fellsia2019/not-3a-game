using System;
using System.Globalization;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Actual profession and equipped-tool facts supplied by the scene binding.</summary>
    public sealed class Stage3ProfessionsView
    {
        public Stage3ProfessionView[] Professions;
    }

    public sealed class Stage3ProfessionView
    {
        public string Title, Description;
        public int Mastery, MaxMastery, XpWithin, RequiredXp, AvailablePoints;
        public Stage3ProfessionTalentView[] Talents;
        public Stage3EquipmentItemView Tool;
    }

    public sealed class Stage3ProfessionTalentView
    {
        public string Title, Description, UnavailableReason;
        public int Rank, MaxRank, PointCost;
        public bool CanUpgrade;
        public int CurrentPower, NextPower;
        public float CurrentDuration, NextDuration, CurrentReachBonus, NextReachBonus;
        // Strength is extra material units; tempo and reach are fractions.
        public float CurrentBonus, NextBonus;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3ProfessionsView professions;
        private bool showProfessions;
        private int selectedProfessionIndex, selectedProfessionTalentIndex;
        private Texture2D professionIcons;
        private bool professionIconsLoaded;
        private static readonly CultureInfo ProfessionCulture = CultureInfo.GetCultureInfo("ru-RU");

        public Stage3ProfessionsView ProfessionsView => professions;
        public bool IsProfessionsOpen => ActiveWindow == Stage3PlayerWindow.Hero && showProfessions;
        public int SelectedProfessionIndex => selectedProfessionIndex;
        public int SelectedProfessionTalentIndex => selectedProfessionTalentIndex;
        public event Action<int, int> ProfessionTalentUpgradeRequested;

        public void SetProfessions(Stage3ProfessionsView value)
        {
            professions = value;
            ClampProfessionSelection();
        }

        public void ShowProfessions(bool value) { showProfessions = value; showAdventurer = false; }

        public void SelectProfession(int index)
        {
            if (professions?.Professions == null || index < 0 ||
                index >= professions.Professions.Length || professions.Professions[index] == null) return;
            if (selectedProfessionIndex != index) selectedProfessionTalentIndex = 0;
            selectedProfessionIndex = index;
            ClampProfessionSelection();
        }

        public void SelectProfessionTalent(int index)
        {
            var talents = SelectedProfession?.Talents;
            if (talents == null || index < 0 || index >= talents.Length || talents[index] == null) return;
            selectedProfessionTalentIndex = index;
        }

        private Stage3ProfessionView SelectedProfession =>
            professions?.Professions != null && selectedProfessionIndex >= 0 &&
            selectedProfessionIndex < professions.Professions.Length
                ? professions.Professions[selectedProfessionIndex] : null;

        private Stage3ProfessionTalentView SelectedProfessionTalent =>
            SelectedProfession?.Talents != null && selectedProfessionTalentIndex >= 0 &&
            selectedProfessionTalentIndex < SelectedProfession.Talents.Length
                ? SelectedProfession.Talents[selectedProfessionTalentIndex] : null;

        private void ClampProfessionSelection()
        {
            selectedProfessionIndex = Mathf.Clamp(selectedProfessionIndex, 0,
                Mathf.Max(0, (professions?.Professions?.Length ?? 0) - 1));
            selectedProfessionTalentIndex = Mathf.Clamp(selectedProfessionTalentIndex, 0,
                Mathf.Max(0, (SelectedProfession?.Talents?.Length ?? 0) - 1));
        }

        private void ResetProfessionPresentation()
        {
            showProfessions = false;
            showAdventurer = false;
            selectedSkillId = 0;
            selectedProfessionIndex = selectedProfessionTalentIndex = 0;
        }

        private Rect HeroDevelopmentContentRect
        {
            get
            {
                var r = WindowContentRect;
                return new Rect(r.x, r.y + 58f * UiScale, r.width, Mathf.Max(0, r.height - 58f * UiScale));
            }
        }

        /// <summary>False is the existing combat tab; true is professions.</summary>
        public Rect ProfessionTabRect(bool professionTab) => HeroDevelopmentTabRect(professionTab ? 1 : 0);

        private Rect ProfessionColumn(int index)
        {
            var r = HeroDevelopmentContentRect;
            return index == 0 ? new Rect(r.x - 16f * UiScale, r.y, r.width * .333f, r.height) :
                index == 1 ? new Rect(r.x + r.width * .35f, r.y, r.width * .333f, r.height) :
                new Rect(r.x + r.width * .72f, r.y, r.width * .28f, r.height);
        }

        public Rect ProfessionCardRect(int index)
        {
            if (!IsProfessionsOpen || index < 0 || index >= 2) return Rect.zero;
            var r = ProfessionColumn(0);
            var gap = 18f * UiScale;
            var height = (r.height - 54f * UiScale - gap) / 2f;
            return new Rect(r.x, r.y + 54f * UiScale + index * (height + gap), r.width, height);
        }

        public Rect ProfessionTalentRect(int index)
        {
            if (!IsProfessionsOpen || index < 0 || index >= 3) return Rect.zero;
            var r = ProfessionColumn(1);
            var gap = 12f * UiScale;
            var height = (r.height - 54f * UiScale - 2f * gap) / 3f;
            return new Rect(r.x, r.y + 54f * UiScale + index * (height + gap), r.width, height);
        }

        public Rect ProfessionUpgradeRect
        {
            get
            {
                if (!IsProfessionsOpen || SelectedProfessionTalent == null) return Rect.zero;
                var r = ProfessionColumn(2);
                return new Rect(r.x, r.y + r.height * .55f, r.width, 57f * UiScale);
            }
        }

        private void DrawHeroDevelopmentTabs()
        {
            var r = WindowContentRect; var s = UiScale;
            DrawRule(new Rect(r.x, r.y + 35f * s, r.width, s), false);
            var requested = -1;
            var selected = showProfessions ? 1 : showAdventurer ? 2 : 0;
            var count = heroLoadout != null ? 3 : 2;
            for (var i = 0; i < count; i++)
            {
                var available = i != 2 || heroLoadout?.AdventurerNodes?.Length > 0;
                var title = i == 0 ? "Бой" : i == 1 ? "Добыча" : "Авантюрист";
                if (Button(HeroDevelopmentTabRect(i), title, selected == i, available)) requested = i;
            }
            if (requested >= 0)
            {
                ShowHeroDevelopmentTab(requested);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawProfessions()
        {
            var s = UiScale;
            var left = ProfessionColumn(0);
            var middle = ProfessionColumn(1);
            Label(new Rect(left.x + 15f * s, left.y, left.width, 42f * s), "Профессии", 30);
            if (professions?.Professions == null || professions.Professions.Length == 0)
            {
                Label(HeroDevelopmentContentRect, "Профессии пока недоступны", 23, TextAnchor.MiddleCenter, true);
                return;
            }
            var selected = SelectedProfession;
            Label(new Rect(middle.x + 12f * s, middle.y, middle.width, 42f * s),
                selectedProfessionIndex == 0 ? "Таланты рудокопа" : "Таланты лесоруба", 30);
            var requestedProfession = -1;
            var requestedTalent = -1;
            for (var i = 0; i < Mathf.Min(2, professions.Professions.Length); i++)
            {
                var profession = professions.Professions[i];
                if (profession == null) continue;
                var card = ProfessionCardRect(i);
                if (ProfessionCardButton(card, i == selectedProfessionIndex)) requestedProfession = i;
                DrawProfessionCard(card, profession, i);
            }
            if (selected?.Talents != null)
                for (var i = 0; i < Mathf.Min(3, selected.Talents.Length); i++)
                {
                    var talent = selected.Talents[i];
                    if (talent == null) continue;
                    var card = ProfessionTalentRect(i);
                    if (ProfessionCardButton(card, i == selectedProfessionTalentIndex)) requestedTalent = i;
                    DrawProfessionTalentCard(card, talent, i);
                }
            var upgrade = DrawProfessionTalentDetails(ProfessionColumn(2), selected, SelectedProfessionTalent);
            if (requestedProfession >= 0) SelectProfession(requestedProfession);
            else if (requestedTalent >= 0) SelectProfessionTalent(requestedTalent);
            else if (upgrade) ProfessionTalentUpgradeRequested?.Invoke(selectedProfessionIndex, selectedProfessionTalentIndex);
        }

        private bool ProfessionCardButton(Rect r, bool selected)
        {
            var s = UiScale;
            var color = GUI.color;
            if (selected)
            {
                // Follow the authored corner silhouette instead of a square selection box.
                GUI.color = color * new Color(.15f, 1f, 1f, .50f);
                var glow = new Rect(r.x - 2f * s, r.y - 2f * s, r.width + 4f * s, r.height + 4f * s);
                DrawSliced(glow, art.panelFrame, false, .26f * s);
            }
            GUI.color = color;
            DrawSliced(r, art.panelFrame, true, .24f * s);
            if (selected)
            {
                GUI.color = color * new Color(.15f, 1f, 1f, .70f);
                DrawSliced(r, art.panelFrame, false, .24f * s);
                GUI.color = color;
            }
            if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            return GUI.Button(r, GUIContent.none, blankButton);
        }

        private void DrawProfessionCard(Rect r, Stage3ProfessionView value, int index)
        {
            var s = UiScale;
            var iconSide = Mathf.Min(r.width * .34f, r.height * .70f);
            var icon = new Rect(r.x + 23f * s, r.y + 25f * s, iconSide, iconSide);
            DrawProfessionIcon(icon, index, -1);
            var x = r.x + r.width * .405f;
            var width = r.xMax - x - 19f * s;
            Label(new Rect(x, r.y + 23f * s, width, 34f * s), value.Title, 28);
            Label(new Rect(x, r.y + 59f * s, width, 38f * s), value.Description, 18, TextAnchor.UpperLeft, true);
            var masteryY = r.y + r.height * .43f;
            Label(new Rect(x, masteryY, width, 27f * s),
                $"Мастерство {value.Mastery} / {value.MaxMastery}", 18, TextAnchor.MiddleLeft);
            var track = new Rect(x, masteryY + 29f * s, width, 25f * s);
            DrawProfessionExperienceBar(track, value);
            var atCap = value.MaxMastery > 0 && value.Mastery >= value.MaxMastery;
            Label(new Rect(x, track.yMax + 3f * s, width, 26f * s),
                atCap ? "XP · МАКС." : $"XP  {value.XpWithin} / {value.RequiredXp}", 18, TextAnchor.MiddleCenter);
            var points = new Rect(x - 4f * s, r.yMax - 62f * s, width + 8f * s, 44f * s);
            DrawSliced(points, art.buttonNormal, true);
            FitPaleLabel(ButtonContentRect(points), $"Очки профессии: {value.AvailablePoints}", 17);
        }

        private void DrawProfessionExperienceBar(Rect r, Stage3ProfessionView value)
        {
            DrawStatusBevel(r, PlumInk);
            var atCap = value.MaxMastery > 0 && value.Mastery >= value.MaxMastery;
            var fraction = atCap ? 1f : value.RequiredXp > 0 ? Mathf.Clamp01((float)value.XpWithin / value.RequiredXp) : 0f;
            var track = Inset(r, 4f * UiScale);
            if (fraction <= 0f) return;
            GUI.BeginGroup(new Rect(track.x, track.y, track.width * fraction, track.height));
            DrawBevelGradient(new Rect(0, 0, track.width, track.height),
                new Color(1f, .72f, .16f), new Color(.79f, .27f, .02f), 6f * UiScale);
            GUI.EndGroup();
        }

        private void DrawProfessionTalentCard(Rect r, Stage3ProfessionTalentView talent, int index)
        {
            var s = UiScale;
            var side = Mathf.Min(r.width * .34f, r.height - 32f * s);
            var icon = new Rect(r.x + 22f * s, r.center.y - side * .5f, side, side);
            DrawProfessionIcon(icon, selectedProfessionIndex, index);
            var x = r.x + r.width * .405f;
            var width = r.xMax - x - 15f * s;
            Label(new Rect(x, r.y + 21f * s, width, 34f * s), talent.Title, 27);
            var description = index == 0 ? (selectedProfessionIndex == 0 ? "Больше ресурса за удар" : "Больше дерева за удар") :
                index == 1 ? "Быстрее завершает удар" :
                selectedProfessionIndex == 0 ? "Удобнее доставать до залежи" : "Удобнее доставать до дерева";
            Label(new Rect(x, r.y + 57f * s, width, 35f * s), description, 17, TextAnchor.UpperLeft, true);
            var dotSide = 27f * s;
            var gap = 22f * s;
            var dotY = r.yMax - 70f * s;
            var count = Mathf.Clamp(talent.MaxRank, 0, 3);
            for (var i = 0; i < count; i++)
            {
                var dot = new Rect(x + i * (dotSide + gap), dotY, dotSide, dotSide);
                DrawDisc(dot, Silver);
                DrawDisc(Inset(dot, 2.5f * s), PlumInk);
                DrawDisc(Inset(dot, 5f * s), i < talent.Rank ? Accent : Pearl);
            }
            Label(new Rect(x, dotY + 30f * s, count * (dotSide + gap) - gap, 24f * s),
                $"{talent.Rank} / {talent.MaxRank}", 18, TextAnchor.MiddleCenter);
        }

        private bool DrawProfessionTalentDetails(Rect r, Stage3ProfessionView profession, Stage3ProfessionTalentView talent)
        {
            if (profession == null || talent == null) return false;
            var s = UiScale;
            var cap = talent.Rank >= talent.MaxRank;
            Label(new Rect(r.x, r.y, r.width, 42f * s), talent.Title, 32);
            DrawRule(new Rect(r.x, r.y + 45f * s, r.width, s));
            Label(new Rect(r.x, r.y + 57f * s, r.width, 31f * s),
                cap ? $"Ранг {talent.Rank} · максимум" : $"Ранг {talent.Rank}  →  {talent.Rank + 1}", 23);
            Label(new Rect(r.x, r.y + 95f * s, r.width, 45f * s), talent.Description, 18);
            var stats = new Rect(r.x, r.y + 140f * s, r.width, 130f * s);
            DrawSliced(stats, art.panelFrame, true, .18f * s);
            var row = new Rect(stats.x + 24f * s, stats.y + 12f * s, stats.width - 48f * s, 34f * s);
            var hasTool = profession.Tool != null;
            DrawProfessionStat(row, "Добыча за удар", hasTool ? talent.CurrentPower.ToString() : "—",
                hasTool ? talent.NextPower + " ед." : "—", talent.CurrentPower != talent.NextPower);
            row.y += 36f * s;
            DrawProfessionStat(row, "Время удара", hasTool ? ProfessionNumber(talent.CurrentDuration) : "—",
                hasTool ? ProfessionNumber(talent.NextDuration) + " с" : "—",
                !Mathf.Approximately(talent.CurrentDuration, talent.NextDuration));
            row.y += 36f * s;
            DrawProfessionStat(row, "Бонус досягаемости", ProfessionPercent(talent.CurrentReachBonus),
                ProfessionPercent(talent.NextReachBonus), !Mathf.Approximately(talent.CurrentReachBonus, talent.NextReachBonus));
            var summary = new Rect(r.x, stats.yMax + 7f * s, r.width, 41f * s);
            string source;
            if (selectedProfessionTalentIndex == 0)
                source = hasTool ? $"Сила {(selectedProfessionIndex == 0 ? "кирки" : "топора")}: {talent.CurrentPower - Mathf.RoundToInt(talent.CurrentBonus)}" : "Инструмент не надет";
            else if (selectedProfessionTalentIndex == 1)
                source = "Бонус к темпу надетого инструмента";
            else source = "Бонус к зазору от края ресурса";
            var bonus = selectedProfessionTalentIndex == 0
                ? $"+{ProfessionNumber(talent.CurrentBonus)} → +{ProfessionNumber(talent.NextBonus)}"
                : $"+{ProfessionPercent(talent.CurrentBonus)} → +{ProfessionPercent(talent.NextBonus)}";
            Label(summary, source + "\nБонус таланта: " + bonus, 16, TextAnchor.MiddleCenter);
            var button = ProfessionUpgradeRect;
            var cost = talent.PointCost;
            var tail = cost % 100;
            var noun = tail >= 11 && tail <= 14 ? "очков" : cost % 10 == 1 ? "очко" : cost % 10 >= 2 && cost % 10 <= 4 ? "очка" : "очков";
            var action = cap ? "Максимальный ранг" : (talent.Rank == 0 ? "Изучить" : "Улучшить") + $" · {cost} {noun}";
            var requested = Button(button, action, false, talent.CanUpgrade && !cap);
            Label(new Rect(r.x, button.yMax + 1f * s, r.width, 27f * s),
                !talent.CanUpgrade && !cap ? talent.UnavailableReason : "", 14, TextAnchor.MiddleCenter, true);
            var toolY = button.yMax + 30f * s;
            DrawRule(new Rect(r.x, toolY, r.width, s));
            Label(new Rect(r.x, toolY + 8f * s, r.width, 29f * s), "Надето", 23);
            var tool = new Rect(r.x, toolY + 42f * s, r.width, Mathf.Max(96f * s, r.yMax - toolY - 68f * s));
            DrawSliced(tool, art.panelFrame, true, .18f * s);
            if (hasTool)
            {
                DrawEquipmentCell(new Rect(tool.x + 14f * s, tool.y + 10f * s, tool.height - 20f * s, tool.height - 20f * s), profession.Tool);
                var x = tool.x + tool.height + 12f * s;
                Label(new Rect(x, tool.y + 17f * s, tool.xMax - x - 20f * s, 28f * s), profession.Tool.Name, 20);
                DrawQualityBadge(new Rect(x, tool.yMax - 52f * s, 66f * s, 39f * s), profession.Tool.QualityIndex);
                Label(new Rect(x + 78f * s, tool.yMax - 52f * s, tool.xMax - x - 98f * s, 39f * s),
                    $"LVL {profession.Tool.Level} / 100", 19, TextAnchor.MiddleCenter);
            }
            else Label(Inset(tool, 12f * s), selectedProfessionIndex == 0 ? "Кирка не надета" : "Топор не надет",
                20, TextAnchor.MiddleCenter, true);
            Label(new Rect(r.x, r.yMax - 20f * s, r.width, 22f * s),
                "Мастерство растёт за добытый материал", 14, TextAnchor.MiddleCenter, true);
            return requested;
        }

        private void DrawProfessionStat(Rect row, string title, string current, string next, bool changed)
        {
            Label(new Rect(row.x, row.y, row.width * .61f, row.height), title, 17, TextAnchor.MiddleLeft, !changed);
            var valueRect = new Rect(row.x + row.width * .61f, row.y, row.width * .39f, row.height);
            var style = GetLabelStyle(18, false);
            var original = style.normal.textColor;
            style.normal.textColor = changed ? Accent : MutedInk;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(valueRect, LabelContent(current + " → " + next), style);
            style.normal.textColor = original;
        }

        private static string ProfessionNumber(float value) => value.ToString("0.##", ProfessionCulture);
        private static string ProfessionPercent(float fraction) => (fraction * 100f).ToString("0.#", ProfessionCulture) + "%";

        private void DrawProfessionIcon(Rect r, int professionIndex, int talentIndex)
        {
            if (!professionIconsLoaded)
            {
                professionIconsLoaded = true;
                professionIcons = Resources.Load<Texture2D>("Stage3ProfessionIcons");
            }
            if (professionIcons != null)
            {
                // Atlas: miner, logger, miner strength, miner tempo /
                // miner reach, logger strength, logger tempo, logger reach.
                var index = talentIndex < 0 ? professionIndex :
                    professionIndex == 0 ? talentIndex + 2 : talentIndex + 5;
                var uv = new Rect((index % 4) * .25f, index < 4 ? .5f : 0f, .25f, .5f);
                DrawUiTexture(r, professionIcons, uv);
                return;
            }
            DrawDisc(Inset(r, r.width * .15f), PlumInk);
            DrawEquipmentIcon(Inset(r, r.width * .21f), professionIndex == 0 ? 6 : 7, 0);
            DrawMedallion(r);
        }
    }
}
