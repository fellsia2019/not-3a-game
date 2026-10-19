using System;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    [Serializable]
    public struct Stage2ResourceCost
    {
        [SerializeField, Min(0)] private int wood, stone, metal, gold;
        public int Wood => wood;
        public int Stone => stone;
        public int Metal => metal;
        public int Gold => gold;
        public bool IsValid => wood >= 0 && stone >= 0 && metal >= 0 && gold >= 0;
        public bool IsZero => wood == 0 && stone == 0 && metal == 0 && gold == 0;
        public Stage2ResourceCost(int wood, int stone, int metal, int gold = 0)
        { this.wood = wood; this.stone = stone; this.metal = metal; this.gold = gold; }
        public Stage2ResourceCost Half => new Stage2ResourceCost(wood / 2, stone / 2, metal / 2, gold / 2);
        public bool TryAdd(Stage2ResourceCost other, out Stage2ResourceCost total)
        {
            total = default;
            if (!IsValid || !other.IsValid || other.wood > int.MaxValue - wood ||
                other.stone > int.MaxValue - stone || other.metal > int.MaxValue - metal ||
                other.gold > int.MaxValue - gold) return false;
            total = new Stage2ResourceCost(wood + other.wood, stone + other.stone, metal + other.metal, gold + other.gold);
            return true;
        }
    }
}
