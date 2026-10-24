using System;
using Not3A.Stage2.Economy;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public enum Stage2ItemType { Bow, Wand, Helmet, Chest, Arms, Legs, Pickaxe, Axe, Amulet, Ring, Protection, HealthPotion, ManaPotion, MixedPotion }
    public enum Stage2ItemQuality { F, E, D, C, B, A, S, SSS }
    public enum Stage2ItemActiveAction { None, RestoreHealth, RestoreMana, RestoreVitals }
    public enum Stage2ItemSlot { Head, Chest, Arms, Legs, Weapon, Pickaxe, Axe, Staff, Amulet, RingLeft, RingRight, Protection }

    [Serializable]
    public sealed class Stage2ItemDefinition
    {
        public Stage2ItemType type;
        public string displayName;
        public int damage, armor;
        public float range = 5.5f, interval = 1.1f, flight = .18f;
        public Stage2ResourceCost craftCost, levelCost;
        public Stage2ItemActiveAction activeAction;
        public float damagePercent, protectionPercent, manaBonus;
        public Stage2ItemDefinition(Stage2ItemType kind, string name, int attack, int protection,
            Stage2ResourceCost craft, Stage2ResourceCost level)
        { type = kind; displayName = name; damage = attack; armor = protection; craftCost = craft; levelCost = level; }
    }

    public readonly struct Stage2ItemStats
    {
        public readonly int Damage, Armor, HarvestPower;
        public readonly float Range, Interval, Flight, HarvestSpeed;
        public readonly bool Weapon, Magic;
        public readonly float DamagePercent, ProtectionPercent, ManaBonus;
        public Stage2ItemStats(int damage, int armor, int power, float range, float interval, float flight,
            float harvestSpeed, bool weapon, bool magic, float damagePercent=0, float protectionPercent=0, float manaBonus=0)
        { Damage = damage; Armor = armor; HarvestPower = power; Range = range; Interval = interval;
            Flight = flight; HarvestSpeed = harvestSpeed; Weapon = weapon; Magic = magic;
            DamagePercent=damagePercent;ProtectionPercent=protectionPercent;ManaBonus=manaBonus; }
    }

    /// <summary>Prototype content tuning; item identity, numerical level and quality are independent.</summary>
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Equipment Catalog")]
    public sealed class Stage2EquipmentCatalog : ScriptableObject
    {
        [SerializeField] private int maximumItemLevel = 100;
        [SerializeField] private bool expandedLoadout;
        [SerializeField] private float statGrowthPerLevel = .02f;
        [SerializeField] private int levelsPerToolPower = 25;
        [SerializeField] private int levelsPerCostStep = 10;
        [SerializeField] private float speedGrowthPerQuality = .04f;
        [SerializeField] private float[] qualityPower = { 1f, 1.15f, 1.35f, 1.6f, 1.9f, 2.3f, 2.8f, 3.5f };
        // D090: three scroll-only building stages; SSS is never obtainable by forging.
        [SerializeField] private Stage2ItemQuality[] forgeCaps = { Stage2ItemQuality.D, Stage2ItemQuality.B, Stage2ItemQuality.S };
        [SerializeField] private Stage2ItemDefinition[] definitions = Defaults();
        public int MaximumItemLevel => maximumItemLevel;
        public bool ExpandedLoadout=>expandedLoadout;
        public int ForgeTypeCount=>expandedLoadout?11:8;
        public int ForgeStageCount => forgeCaps.Length;
        public Stage2ItemQuality ForgeCap(int stage) => stage <= 0 ? Stage2ItemQuality.F : forgeCaps[Mathf.Clamp(stage - 1, 0, forgeCaps.Length - 1)];
        public Stage2ItemDefinition Get(Stage2ItemType type) => ValidType(type) ? definitions[(int)type] : null;
        public Stage2ItemActiveAction ActiveAction(Stage2ItemType type)
        {
            var action=Get(type)?.activeAction??Stage2ItemActiveAction.None;
            if(action!=Stage2ItemActiveAction.None)return action;
            // Compatibility for existing serialized potion catalogs predating action metadata.
            return type==Stage2ItemType.HealthPotion?Stage2ItemActiveAction.RestoreHealth:
                type==Stage2ItemType.ManaPotion?Stage2ItemActiveAction.RestoreMana:
                type==Stage2ItemType.MixedPotion?Stage2ItemActiveAction.RestoreVitals:Stage2ItemActiveAction.None;
        }
        public static bool ValidType(Stage2ItemType type) => (int)type >= 0 && (int)type < 14;
        public static bool IsConsumable(Stage2ItemType type)=>(int)type>=11&&(int)type<=13;
        public static Stage2ItemSlot FixedSlot(Stage2ItemType type)=>type==Stage2ItemType.Wand?Stage2ItemSlot.Staff:Slot(type);
        public static bool ValidQuality(Stage2ItemQuality quality) => (int)quality >= 0 && (int)quality < 8;
        public static Stage2ItemSlot Slot(Stage2ItemType type)
        {
            switch (type)
            {
                case Stage2ItemType.Bow: case Stage2ItemType.Wand: return Stage2ItemSlot.Weapon;
                case Stage2ItemType.Helmet: return Stage2ItemSlot.Head;
                case Stage2ItemType.Chest: return Stage2ItemSlot.Chest;
                case Stage2ItemType.Arms: return Stage2ItemSlot.Arms;
                case Stage2ItemType.Legs: return Stage2ItemSlot.Legs;
                case Stage2ItemType.Pickaxe: return Stage2ItemSlot.Pickaxe;
                case Stage2ItemType.Axe: return Stage2ItemSlot.Axe;
                case Stage2ItemType.Amulet:return Stage2ItemSlot.Amulet;
                case Stage2ItemType.Ring:return Stage2ItemSlot.RingLeft;
                case Stage2ItemType.Protection:return Stage2ItemSlot.Protection;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
        public Stage2ItemStats Stats(Stage2ItemType type, Stage2ItemQuality quality, int level)
        {
            if (!ValidType(type) || !ValidQuality(quality) || level < 1 || level > maximumItemLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            var item = Get(type); var multiplier = qualityPower[(int)quality] * (1f + (level - 1) * statGrowthPerLevel);
            var speed = 1f + (int)quality * speedGrowthPerQuality;
            var tool = type == Stage2ItemType.Pickaxe || type == Stage2ItemType.Axe;
            var weapon = type == Stage2ItemType.Bow || type == Stage2ItemType.Wand;
            return new Stage2ItemStats(weapon ? Mathf.Max(1, Mathf.FloorToInt(item.damage * multiplier)) : 0,
                Mathf.FloorToInt(item.armor * multiplier), tool ? 1 + (int)quality / 2 + (level - 1) / levelsPerToolPower : 0,
                weapon ? item.range : 0, weapon ? item.interval / speed : 0, item.flight, tool ? speed : 1, weapon, type == Stage2ItemType.Wand,
                item.damagePercent*multiplier,Mathf.Min(.65f,item.protectionPercent*multiplier),item.manaBonus*multiplier);
        }
        public Stage2ResourceCost CraftCost(Stage2ItemType type) => Get(type).craftCost;
        public Stage2ResourceCost LevelCost(Stage2ItemType type, int currentLevel) =>
            Scale(Get(type).levelCost, 1 + (Mathf.Max(1, currentLevel) - 1) / levelsPerCostStep);
        public Stage2ResourceCost ReforgeCost(Stage2ItemType type, Stage2ItemQuality nextQuality) =>
            Scale(Get(type).craftCost, 1 + (int)nextQuality);
        private static Stage2ResourceCost Scale(Stage2ResourceCost value, int factor) => new Stage2ResourceCost(
            (int)Math.Min(int.MaxValue, (long)value.Wood * factor), (int)Math.Min(int.MaxValue, (long)value.Stone * factor),
            (int)Math.Min(int.MaxValue, (long)value.Metal * factor), (int)Math.Min(int.MaxValue, (long)value.Gold * factor));
        public bool TryValidate(out string reason)
        {
            reason = "Equipment requires8 ordered item types/qualities, level cap100 and the3 approved forge caps D/B/S.";
            if (maximumItemLevel != 100 || !float.IsFinite(statGrowthPerLevel) || statGrowthPerLevel < 0 ||
                !float.IsFinite(speedGrowthPerQuality) || speedGrowthPerQuality < 0 || levelsPerToolPower < 1 || levelsPerCostStep < 1 ||
                definitions == null || definitions.Length != 14 || qualityPower == null || qualityPower.Length != 8 ||
                forgeCaps == null || forgeCaps.Length != 3 || forgeCaps[0] != Stage2ItemQuality.D ||
                forgeCaps[1] != Stage2ItemQuality.B || forgeCaps[2] != Stage2ItemQuality.S) return false;
            var previous = 0f;
            foreach (var power in qualityPower)
            { if (!float.IsFinite(power) || power < 1 || power < previous) return false; previous = power; }
            for (var i = 0; i < definitions.Length; i++)
            {
                var d = definitions[i];
                if (d == null || (int)d.type != i || string.IsNullOrWhiteSpace(d.displayName) ||
                    !Enum.IsDefined(typeof(Stage2ItemActiveAction),d.activeAction) ||
                    !d.craftCost.IsValid || !d.levelCost.IsValid || (!IsConsumable(d.type)&&(d.craftCost.IsZero||d.levelCost.IsZero)) || d.armor < 0 ||
                    !float.IsFinite(d.damagePercent)||d.damagePercent<0||!float.IsFinite(d.protectionPercent)||d.protectionPercent<0||
                    !float.IsFinite(d.manaBonus)||d.manaBonus<0||
                    ((i == 0 || i == 1) && (d.damage < 1 || !float.IsFinite(d.range) || d.range <= 0 ||
                        !float.IsFinite(d.interval) || d.interval <= 0 || !float.IsFinite(d.flight) || d.flight < 0))) return false;
            }
            reason = string.Empty; return true;
        }
        private static Stage2ItemDefinition[] Defaults() => new[] {
            new Stage2ItemDefinition(Stage2ItemType.Bow,"Лук",2,0,new Stage2ResourceCost(4,0,1),new Stage2ResourceCost(1,0,1,1)),
            new Stage2ItemDefinition(Stage2ItemType.Wand,"Посох",2,0,new Stage2ResourceCost(3,2,1),new Stage2ResourceCost(1,1,1,1)) {range=5f,interval=1.2f},
            new Stage2ItemDefinition(Stage2ItemType.Helmet,"Шлем",0,1,new Stage2ResourceCost(1,0,2),new Stage2ResourceCost(0,1,1,1)),
            new Stage2ItemDefinition(Stage2ItemType.Chest,"Нагрудник",0,2,new Stage2ResourceCost(2,0,4),new Stage2ResourceCost(0,1,2,1)),
            new Stage2ItemDefinition(Stage2ItemType.Arms,"Руки",0,1,new Stage2ResourceCost(1,0,2),new Stage2ResourceCost(0,1,1,1)),
            new Stage2ItemDefinition(Stage2ItemType.Legs,"Ноги",0,1,new Stage2ResourceCost(2,0,3),new Stage2ResourceCost(0,1,1,1)),
            new Stage2ItemDefinition(Stage2ItemType.Pickaxe,"Кирка",0,0,new Stage2ResourceCost(2,0,2),new Stage2ResourceCost(1,0,1,1)),
            new Stage2ItemDefinition(Stage2ItemType.Axe,"Топор",0,0,new Stage2ResourceCost(2,0,2),new Stage2ResourceCost(1,0,1,1)),
            new Stage2ItemDefinition(Stage2ItemType.Amulet,"Амулет натиска",0,0,new Stage2ResourceCost(2,2,3),new Stage2ResourceCost(0,1,1,1)){damagePercent=.05f},
            new Stage2ItemDefinition(Stage2ItemType.Ring,"Кольцо сосредоточения",0,0,new Stage2ResourceCost(0,2,3),new Stage2ResourceCost(0,1,1,1)){manaBonus=10},
            new Stage2ItemDefinition(Stage2ItemType.Protection,"Защитная печать",0,0,new Stage2ResourceCost(2,3,3),new Stage2ResourceCost(0,1,2,1)){protectionPercent=.08f},
            new Stage2ItemDefinition(Stage2ItemType.HealthPotion,"Зелье HP",0,0,default,default),
            new Stage2ItemDefinition(Stage2ItemType.ManaPotion,"Зелье MP",0,0,default,default),
            new Stage2ItemDefinition(Stage2ItemType.MixedPotion,"Зелье восстановления",0,0,default,default)
        };
    }
}
