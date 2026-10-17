using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    [Serializable]
    public sealed class Stage2ProductionEntry
    {
        public ResourceProductionBuilding building;
        public string displayName;
        public int woodCost, stoneCost, metalCost, goldCost;
        public int outputAmount = 1;
        public float intervalSeconds = 5;
        public Sprite sprite;
        public float visualWidth = 3;
        public Vector2 footprintSize = new Vector2(1.4f, .8f);
        public ResourceProductionBuilding Building => building;
        public string DisplayName => displayName;
        public int WoodCost => woodCost;
        public int StoneCost => stoneCost;
        public int MetalCost => metalCost;
        public int GoldCost => goldCost;
        public int OutputAmount => outputAmount;
        public float IntervalSeconds => intervalSeconds;
        public Sprite Sprite => sprite;
        public float VisualWidth => visualWidth;
        public Vector2 FootprintSize => footprintSize;
        public MapResourceKind Resource => (MapResourceKind)building;
        public bool IsValid => (int)building >= 0 && (int)building < 3 && !string.IsNullOrWhiteSpace(displayName) &&
            woodCost >= 0 && stoneCost >= 0 && metalCost >= 0 && goldCost >= 0 && outputAmount > 0 &&
            Positive(intervalSeconds) && Positive(visualWidth) && Positive(footprintSize.x) && Positive(footprintSize.y) && sprite != null;
        private static bool Positive(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Production Catalog")]
    public sealed class Stage2ProductionCatalog : ScriptableObject
    {
        [SerializeField] private Stage2ProductionEntry[] entries = Array.Empty<Stage2ProductionEntry>();
        [SerializeField] private Material spriteMaterial;
        public IReadOnlyList<Stage2ProductionEntry> Entries => entries;
        public Material SpriteMaterial => spriteMaterial;
        public void Configure(Stage2ProductionEntry[] values, Material material)
        { entries = (Stage2ProductionEntry[])values.Clone(); spriteMaterial = material; }
        public Stage2ProductionEntry Get(ResourceProductionBuilding kind)
        {
            foreach (var entry in entries) if (entry != null && entry.Building == kind) return entry;
            throw new ArgumentOutOfRangeException(nameof(kind));
        }
        public bool TryValidate(out string reason)
        {
            reason = "Production needs three unique valid entries with actual sprites and a sprite material.";
            if (entries == null || entries.Length != 3 || spriteMaterial == null) return false;
            var mask = 0;
            foreach (var entry in entries)
            {
                if (entry == null || !entry.IsValid || (mask & (1 << (int)entry.Building)) != 0) return false;
                mask |= 1 << (int)entry.Building;
            }
            reason = string.Empty; return true;
        }
    }
}
