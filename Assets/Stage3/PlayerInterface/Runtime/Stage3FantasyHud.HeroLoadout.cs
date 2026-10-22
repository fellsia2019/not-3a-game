using System;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Actual learned skills and consumable stock, supplied by the gameplay binding.</summary>
    public sealed class Stage3HeroLoadoutView
    {
        public int ActiveWeapon;
        public Stage3SkillNodeView[] Nodes;
        public Stage3QuickSlotView[] QuickSlots;
        public Stage3SkillNodeView[] AdventurerNodes;
    }

    public sealed class Stage3SkillNodeView
    {
        public int Id, Rank, MaxRank, RequiredLevel, RequiredNodeId = -1, Weapon = -1;
        public bool IsActive, CanUpgrade, CanCast, Armed;
        public float CooldownRemaining, CooldownTotal, ManaCost;
        public string Title, Description, EffectText, UnavailableReason, UpgradeReason;
    }

    public sealed class Stage3QuickSlotView
    {
        public int Index, Count;
        public float CooldownRemaining, CooldownTotal;
        public bool CanUse;
        public string Title, UnavailableReason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3HeroLoadoutView heroLoadout;
        private int selectedSkillId;
        private bool showAdventurer;
        private Texture2D heroSkillIcons;
        private bool heroLoadoutIconsLoaded;
        public Stage3HeroLoadoutView HeroLoadoutView => heroLoadout;
        public int SelectedSkillId => selectedSkillId;
        public bool IsAdventurerOpen => ActiveWindow == Stage3PlayerWindow.Hero && showAdventurer;
        public event Action<int> SkillUpgradeRequested, SkillArmRequested, QuickSlotUseRequested;
        public event Action WeaponSwitchRequested;

        public void SetHeroLoadout(Stage3HeroLoadoutView value)
        {
            heroLoadout = value;
            if (showAdventurer && (value?.AdventurerNodes == null || value.AdventurerNodes.Length == 0))
                showAdventurer = false;
            if (FindSkill(selectedSkillId) == null) selectedSkillId = showAdventurer && value?.AdventurerNodes?.Length > 0
                ? value.AdventurerNodes[0].Id : 0;
        }

        public void SelectSkill(int id)
        {
            if (FindSkill(id) != null) selectedSkillId = id;
        }

        private Stage3SkillNodeView FindSkill(int id)
        {
            var nodes = showAdventurer ? heroLoadout?.AdventurerNodes : heroLoadout?.Nodes;
            if (nodes != null) foreach (var node in nodes) if (node != null && node.Id == id) return node;
            return null;
        }

        private Stage3SkillNodeView FindCombatSkill(int id)
        {
            if (heroLoadout?.Nodes != null)
                foreach (var node in heroLoadout.Nodes) if (node != null && node.Id == id) return node;
            return null;
        }

        private Stage3QuickSlotView FindQuickSlot(int index)
        {
            if (heroLoadout?.QuickSlots != null)
                foreach (var slot in heroLoadout.QuickSlots) if (slot != null && slot.Index == index) return slot;
            return null;
        }

        private Rect LoadoutWeaponArea => new Rect(HeroDockContentRect.x, HeroDockContentRect.y,
            HeroDockContentRect.width * .255f, HeroDockContentRect.height);
        private Rect LoadoutSkillsArea => new Rect(LoadoutWeaponArea.xMax, HeroDockContentRect.y,
            HeroDockContentRect.width * .385f, HeroDockContentRect.height);
        private Rect LoadoutQuickArea => new Rect(LoadoutSkillsArea.xMax + 6f * UiScale, HeroDockContentRect.y,
            HeroDockContentRect.width * .14f - 12f * UiScale, HeroDockContentRect.height);
        private Rect LoadoutStatusArea => Rect.MinMaxRect(LoadoutQuickArea.xMax + 12f * UiScale,
            HeroDockContentRect.y, HeroDockContentRect.xMax, HeroDockContentRect.yMax);

        public Rect SkillSlotRect(int id)
        {
            if (id < 0 || id > 10) return Rect.zero;
            var r = LoadoutSkillsArea; var s = UiScale;
            var gap = 5f * s;
            var side = Mathf.Min(72f * s, Mathf.Min((r.width - 64f * s - gap * 4) / 5f, (r.height - gap) / 2f));
            var start = r.x + 64f * s;
            var top = r.center.y - side - gap * .5f;
            if (id == 0) return new Rect(r.x + 2f * s, r.center.y - side * .5f, side, side);
            var index = id - 1;
            return new Rect(start + index % 5 * (side + gap), top + index / 5 * (side + gap), side, side);
        }

        public Rect QuickSlotRect(int index)
        {
            if (index < 0 || index > 3) return Rect.zero;
            var r = LoadoutQuickArea; var gap = 7f * UiScale;
            var side = Mathf.Min((r.width - gap) * .5f, (r.height - gap) * .5f);
            var x = r.center.x - side - gap * .5f;
            var y = r.center.y - side - gap * .5f;
            return new Rect(x + index % 2 * (side + gap), y + index / 2 * (side + gap), side, side);
        }

        public Rect WeaponSwitchRect
        {
            get { var r = LoadoutWeaponArea; return new Rect(r.xMax - 32f * UiScale, r.center.y - 28f * UiScale, 30f * UiScale, 56f * UiScale); }
        }

        public Rect HeroDevelopmentTabRect(int index)
        {
            if (ActiveWindow != Stage3PlayerWindow.Hero || index < 0 || index > 2) return Rect.zero;
            var r = WindowContentRect; var s = UiScale;
            var width = Mathf.Min(220f * s, r.width * .18f);
            return new Rect(r.center.x - width * 1.5f + index * width, r.y - 9f * s, width, 44f * s);
        }

        private Rect SkillTreeRect
        {
            get { var r = HeroDevelopmentContentRect; return new Rect(r.x - 24f * UiScale, r.y + 64f * UiScale,
                r.width * .755f, r.height - 64f * UiScale); }
        }
        private Rect SkillDetailsRect
        {
            get { var r = HeroDevelopmentContentRect; return Rect.MinMaxRect(SkillTreeRect.xMax + 18f * UiScale,
                SkillTreeRect.y, r.xMax + 12f * UiScale, r.yMax); }
        }

        public Rect SkillNodeRect(int id)
        {
            if (ActiveWindow != Stage3PlayerWindow.Hero || showProfessions || heroLoadout == null) return Rect.zero;
            var r = SkillTreeRect; var s = UiScale;
            var node = FindSkill(id); if (node == null) return Rect.zero;
            float x, y, side;
            if (showAdventurer)
            {
                var index = Array.IndexOf(heroLoadout.AdventurerNodes, node);
                if (index < 0) return Rect.zero;
                var columns = 3;
                x = r.x + r.width * ((index % columns + .5f) / columns);
                y = r.y + r.height * ((index / columns + .5f) / Mathf.Max(1, Mathf.CeilToInt(heroLoadout.AdventurerNodes.Length / 3f)));
                side = 90f * s;
            }
            else if (id == 0)
            { x = r.x + 78f * s; y = r.center.y; side = 110f * s; }
            else if (id >= 11 && id <= 14)
            {
                x = r.x + (id % 2 == 1 ? 142f : 140f) * s;
                y = r.center.y + (id <= 12 ? -1 : 1) * (id % 2 == 1 ? 155f : 102f) * s;
                side = 44f * s;
            }
            else
            {
                var active = id <= 10;
                var weapon = id <= 5 || id >= 15 && id <= 22 ? 0 : 1;
                var column = active ? (id - 1) % 5 : ((id - (weapon == 0 ? 15 : 23)) / 2);
                var start = r.x + 256f * s;
                var pitch = (r.xMax - 104f * s - start) / 4f;
                x = start + column * pitch;
                y = r.y + r.height * (weapon == 0 ? .20f : .70f);
                side = active ? 94f * s : 42f * s;
                if (!active)
                {
                    // Satellites follow their real prerequisite, not their catalogue array position.
                    var parent = FindCombatSkill(node.RequiredNodeId);
                    if (parent != null && parent.IsActive && parent.Id > 0)
                    {
                        var anchor = SkillNodeRect(parent.Id).center;
                        var sibling = 0; var total = 0;
                        foreach (var candidate in heroLoadout.Nodes)
                            if (candidate != null && !candidate.IsActive && candidate.Id >= 15 && candidate.RequiredNodeId == parent.Id)
                            { if (candidate.Id < id) sibling++; total++; }
                        if (total > 2)
                        {
                            x = anchor.x + (sibling % 2 == 0 ? -24f : 24f) * s;
                            y = anchor.y + (sibling < 2 ? -70f : 124f) * s;
                        }
                        else { x = anchor.x + 57f * s; y = anchor.y + (sibling == 0 ? -70f : 124f) * s; }
                    }
                    else { x += 57f * s; y += ((id - (weapon == 0 ? 15 : 23)) % 2 == 0 ? -70f : 124f) * s; }
                }
            }
            return new Rect(x - side * .5f, y - side * .5f, side, side);
        }

        // A reserved caption band separates names from the lower passive satellites.
        public Rect SkillNodeLabelRect(int id)
        {
            var node = FindSkill(id); var r = SkillNodeRect(id);
            if (r.width <= 0 || node == null || !node.IsActive && !showAdventurer) return Rect.zero;
            var width = (id == 0 ? 68f : 180f) * UiScale;
            return new Rect(r.center.x - width * .5f, r.yMax + 6f * UiScale, width, 43f * UiScale);
        }

        public Rect SkillUpgradeRect
        {
            get
            {
                if (ActiveWindow != Stage3PlayerWindow.Hero || showProfessions || FindSkill(selectedSkillId) == null) return Rect.zero;
                var r = SkillDetailsRect; var s = UiScale;
                return new Rect(r.x + 24f * s, r.yMax - 79f * s, r.width - 48f * s, 49f * s);
            }
        }

        private void LoadHeroLoadoutIcons()
        {
            if (heroLoadoutIconsLoaded) return;
            heroLoadoutIconsLoaded = true;
            heroSkillIcons = Resources.Load<Texture2D>("Stage3HeroSkillIcons");
        }

        // Connected opaque silhouette bounds measured in the original1774x887 source.
        // The generated rows are not exactly uniform: equal UV cells cut medallions and leak neighbours.
        private static readonly RectInt[] SkillIconPixels =
        {
            new RectInt(13,22,201,203),new RectInt(229,21,204,204),new RectInt(452,21,204,205),new RectInt(675,22,202,204),
            new RectInt(897,22,202,204),new RectInt(1118,21,203,204),new RectInt(1340,21,203,204),new RectInt(1562,21,201,205),
            new RectInt(12,236,202,204),new RectInt(228,236,204,204),new RectInt(452,235,204,205),new RectInt(676,235,201,205),
            new RectInt(897,236,202,204),new RectInt(1118,236,202,204),new RectInt(1339,236,203,204),new RectInt(1562,236,201,204),
            new RectInt(12,445,201,201),new RectInt(228,445,204,202),new RectInt(453,445,203,202),new RectInt(676,445,201,202),
            new RectInt(897,445,201,202),new RectInt(1117,445,203,202),new RectInt(1339,445,203,202),new RectInt(1562,445,201,202),
            new RectInt(12,652,202,205),new RectInt(228,652,204,205),new RectInt(451,651,205,207),new RectInt(674,652,202,206),
            new RectInt(894,653,203,205),new RectInt(1116,653,203,205),new RectInt(1338,653,204,205),new RectInt(1562,653,201,205)
        };

        public static Rect SkillIconUv(int id)
        {
            if (id < 0 || id >= SkillIconPixels.Length) return Rect.zero;
            var p = SkillIconPixels[id];
            return new Rect(p.x / 1774f, 1f - p.yMax / 887f, p.width / 1774f, p.height / 887f);
        }

        private void DrawSkillArtwork(Rect r, int id)
        {
            LoadHeroLoadoutIcons();
            if (heroSkillIcons != null && id >= 0 && id <= 36)
            {
                var index = id <= 30 ? id : 31;
                var uv = SkillIconUv(index);
                var aspect = heroSkillIcons.width * uv.width / (heroSkillIcons.height * uv.height);
                var width = Mathf.Min(r.width, r.height * aspect); var height = width / aspect;
                DrawUiTexture(new Rect(r.center.x - width * .5f, r.center.y - height * .5f, width, height), heroSkillIcons, uv);
            }
            else if (id == 0) DrawProgressionIcon(r, 0);
            else if (id >= 11 && id <= 13) DrawProgressionIcon(r, id - 10);
            else DrawIcon(r, id <= 5 ? art.bowIcon : art.teleportIcon);
        }
    }
}
