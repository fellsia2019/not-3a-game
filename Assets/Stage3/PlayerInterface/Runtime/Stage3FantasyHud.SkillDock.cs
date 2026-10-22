using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        public void RequestWeaponSwitch() => WeaponSwitchRequested?.Invoke();

        private void DrawHeroLoadoutDock()
        {
            var r = LoadoutWeaponArea; var s = UiScale;
            RefreshHeroProgressionLabels();
            var portraitSide = Mathf.Min(122f * s, r.width * .43f);
            var portrait = new Rect(r.x, r.center.y - portraitSide * .5f, portraitSide, portraitSide);
            DrawRoundSprite(Inset(portrait, 8f * s), art.heroPortrait); DrawMedallion(portrait);
            if (heroProgression != null)
            {
                var level = new Rect(portrait.center.x - 43f * s, portrait.yMax - 6f * s, 86f * s, 27f * s);
                DrawPanel(level); Label(level, heroLevelLabel, 14, TextAnchor.MiddleCenter);
            }
            var weaponWidth = Mathf.Min(51f * s, (WeaponSwitchRect.x - portrait.xMax - 14f * s) * .5f);
            for (var i = 0; i < 2; i++)
            {
                var weapon = new Rect(portrait.xMax + 3f * s + i * (weaponWidth + 5f * s), r.center.y - 44f * s, weaponWidth, 88f * s);
                var item = EquippedItem(i == 0 ? 4 : 7);
                DrawEquipmentCell(weapon, item);
                if (heroLoadout.ActiveWeapon == i)
                {
                    Stroke(Inset(weapon, 2f * s), Accent, 1.5f * s);
                    Fill(new Rect(weapon.x + 6f * s, weapon.yMax - 3f * s, weapon.width - 12f * s, 2f * s), Accent);
                }
                Label(new Rect(weapon.x - 2f * s, weapon.yMax + 3f * s, weapon.width + 4f * s, 20f * s),
                    i == 0 ? "Лук" : "Посох", 13, TextAnchor.MiddleCenter);
            }
            var swap = WeaponSwitchRect;
            if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            var switchRequested = GUI.Button(swap, GUIContent.none, blankButton);
            Label(new Rect(swap.x, swap.y - 5f * s, swap.width, 22f * s), "↕", 22, TextAnchor.MiddleCenter);
            DrawPanel(new Rect(swap.x, swap.y + 17f * s, swap.width, 28f * s));
            Label(new Rect(swap.x, swap.y + 17f * s, swap.width, 28f * s), "Z", 18, TextAnchor.MiddleCenter);
            DrawRule(new Rect(r.xMax, r.y + 8f * s, s, r.height - 16f * s));

            var arm = -1; var use = -1;
            for (var id = 0; id <= 10; id++)
            {
                var node = FindCombatSkill(id);
                if (DrawLoadoutSkillSlot(SkillSlotRect(id), node, id)) arm = id;
            }
            DrawRule(new Rect(LoadoutQuickArea.x - 4f * s, r.y + 8f * s, s, r.height - 16f * s));
            for (var index = 0; index < 4; index++)
                if (DrawQuickSlot(QuickSlotRect(index), FindQuickSlot(index), index)) use = index;
            DrawRule(new Rect(LoadoutQuickArea.xMax + 6f * s, r.y + 8f * s, s, r.height - 16f * s));
            if (heroProgression != null)
            {
                DrawLiveHeroStatus(0, "HP", heroProgression.Health, heroProgression.MaximumHealth, heroHealthLabel, new Color(.82f, .08f, .20f));
                DrawLiveHeroStatus(1, "MP", heroProgression.Mana, heroProgression.MaximumMana, heroManaLabel, new Color(.02f, .65f, .94f));
                var xp = HeroStatusBarRect(2);
                Label(new Rect(HeroStatusRect.x, xp.y, 42f * s, xp.height), "XP", 18, TextAnchor.MiddleLeft);
                DrawExperienceBar(xp); DrawExperienceFeedback();
            }
            else
            {
                DrawUnavailableHeroStatus(0, "HP", new Color(.82f, .08f, .20f));
                DrawUnavailableHeroStatus(1, "MP", new Color(.02f, .65f, .94f));
                DrawUnavailableHeroStatus(2, "XP", ExperienceOrange);
            }
            // Dispatch after drawing one stable snapshot; gameplay owns every validity check.
            if (switchRequested) RequestWeaponSwitch();
            if (arm >= 0) SkillArmRequested?.Invoke(arm);
            if (use >= 0) QuickSlotUseRequested?.Invoke(use);
        }

        private bool DrawLoadoutSkillSlot(Rect r, Stage3SkillNodeView node, int id)
        {
            var s = UiScale;
            var current = id == 0 || (id <= 5 ? 0 : 1) == heroLoadout.ActiveWeapon;
            var usable = node != null && current && (node.CanCast || node.Armed);
            var old = GUI.color;
            GUI.color = old * (!current ? new Color(.45f, .44f, .52f, 1f) : node == null || node.Rank <= 0 ? new Color(.60f, .57f, .63f, 1f) : Color.white);
            DrawDisc(Inset(r, r.width * .13f), PlumInk);
            if (node != null) DrawSkillArtwork(Inset(r, r.width * .12f), id);
            DrawMedallion(r, node?.Armed == true);
            GUI.color = old;
            if (node != null)
            {
                DrawSkillCooldown(Inset(r, r.width * .16f), node.CooldownRemaining, node.CooldownTotal);
                if (node.Rank <= 0) DrawGuildLock(new Rect(r.center.x - 8f * s, r.center.y - 10f * s, 16f * s, 20f * s));
            }
            if (current)
            {
                var key = new Rect(r.xMax - 23f * s, r.yMax - 23f * s, 22f * s, 24f * s);
                DrawDisc(key, Silver); DrawDisc(Inset(key, 1.5f * s), PlumInk);
                PaleLabel(key, id == 0 ? "Q" : ((id - 1) % 5 + 1).ToString(), 16, TextAnchor.MiddleCenter);
            }
            if (node?.CooldownRemaining > 0)
                PaleLabel(new Rect(r.x, r.center.y - 10f * s, r.width, 22f * s), Mathf.CeilToInt(node.CooldownRemaining) + "с", 18, TextAnchor.MiddleCenter);
            if (r.Contains(Event.current.mousePosition) && node != null)
            {
                var reason = !current ? "Z · Переключить оружие" : node.UnavailableReason;
                DrawLoadoutTooltip(r, node.Title + (node.Rank <= 0 ? " · Не изучено" : "") + "\n" +
                    (string.IsNullOrEmpty(reason) ? node.Description : reason));
            }
            var enabled = GUI.enabled; GUI.enabled = enabled && usable;
            if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            var clicked = GUI.Button(r, GUIContent.none, blankButton);
            GUI.enabled = enabled;
            return clicked;
        }

        private bool DrawQuickSlot(Rect r, Stage3QuickSlotView slot, int index)
        {
            var s = UiScale;
            DrawSliced(r, art.panelFrame, true, .16f * s);
            var old = GUI.color;
            if (slot == null || slot.Count <= 0) GUI.color = old * new Color(.60f, .57f, .63f, 1f);
            if (index < 3) DrawEquipmentIcon(Inset(r, 7f * s), index + 11, 0);
            else if (index == 3) Label(Inset(r, 12f * s), "+", 30, TextAnchor.MiddleCenter, true);
            GUI.color = old;
            DrawSkillCooldown(Inset(r, 6f * s), slot?.CooldownRemaining ?? 0, slot?.CooldownTotal ?? 0);
            var key = new Rect(r.x + 4f * s, r.y + 3f * s, 27f * s, 22f * s);
            Fill(key, PlumInk); PaleLabel(key, "F" + (index + 1), 14, TextAnchor.MiddleCenter);
            if (index < 3)
            {
                var count = new Rect(r.xMax - 27f * s, r.yMax - 26f * s, 23f * s, 22f * s);
                Fill(count, PlumInk); PaleLabel(count, (slot?.Count ?? 0).ToString(), 16, TextAnchor.MiddleCenter);
            }
            if (slot?.CooldownRemaining > 0)
                PaleLabel(new Rect(r.x, r.center.y - 10f * s, r.width, 22f * s), Mathf.CeilToInt(slot.CooldownRemaining) + "с", 16, TextAnchor.MiddleCenter);
            if (r.Contains(Event.current.mousePosition))
                DrawLoadoutTooltip(r, slot == null ? "Резерв" : slot.Title +
                    (string.IsNullOrEmpty(slot.UnavailableReason) ? "" : "\n" + slot.UnavailableReason));
            var enabled = GUI.enabled; GUI.enabled = enabled && slot?.CanUse == true;
            if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            var clicked = GUI.Button(r, GUIContent.none, blankButton);
            GUI.enabled = enabled;
            return clicked;
        }

        private void DrawSkillCooldown(Rect r, float remaining, float total)
        {
            if (remaining <= 0 || total <= 0) return;
            EnsureCooldownSectors();
            var step = Mathf.Clamp(Mathf.CeilToInt(remaining / total * 63f), 1, 63);
            DrawUiTexture(r, cooldownSectors, new Rect(step % 8 / 8f, (7 - step / 8) / 8f, 1f / 8, 1f / 8));
        }

        private void DrawLoadoutTooltip(Rect anchor, string text)
        {
            var s = UiScale;
            var tooltip = new Rect(anchor.center.x - 175f * s, WorldHudRect.y - 95f * s, 350f * s, 87f * s);
            tooltip.x = Mathf.Clamp(tooltip.x, 12f * s, Screen.width - tooltip.width - 12f * s);
            DrawPanel(tooltip); Label(Inset(tooltip, 12f * s), text, 17, TextAnchor.MiddleCenter);
        }
    }
}
