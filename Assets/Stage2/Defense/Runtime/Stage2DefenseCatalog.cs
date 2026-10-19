using System;
using Not3A.Stage2.Economy;
using UnityEngine;

namespace Not3A.Stage2.Defense
{
    [Serializable]
    public struct Stage2TowerUpgrade
    {
        [SerializeField] private Stage2ResourceCost cost;
        [SerializeField, Min(1)] private int health;
        [SerializeField, Min(0.01f)] private float range, interval;
        [SerializeField, Min(0)] private int damage;
        public Stage2ResourceCost Cost => cost;
        public int Health => health;
        public float Range => range;
        public float Interval => interval;
        public int Damage => damage;
        public Stage2TowerUpgrade(Stage2ResourceCost cost, int health, float range, float interval, int damage)
        { this.cost = cost; this.health = health; this.range = range; this.interval = interval; this.damage = damage; }
        public bool IsValid(bool attacks) => cost.IsValid && !cost.IsZero && health > 0 &&
            float.IsFinite(range) && range > 0 && float.IsFinite(interval) && interval > 0 &&
            (attacks ? damage > 0 : damage == 0);
    }

    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Defense Catalog")]
    public sealed class Stage2DefenseCatalog : ScriptableObject
    {
        [SerializeField] private Stage2Tower towerPrefab;
        [SerializeField] private string roleName = "Rapid";
        [SerializeField, Min(0)] private int woodCost = 1;
        [SerializeField, Min(0)] private int stoneCost = 1;
        [SerializeField, Min(0)] private int metalCost = 1;
        [SerializeField, Min(0)] private int goldCost;
        [SerializeField, Min(1)] private int towerHealth = 10;
        [SerializeField] private bool canAttack = true;
        [SerializeField, Min(0.01f)] private float attackRange = 4.25f;
        [SerializeField, Min(0.01f)] private float fireInterval = 0.45f;
        [SerializeField, Min(0)] private int attackDamage = 1;
        [SerializeField, Min(0)] private float splashRadiusCells;
        [SerializeField, Min(0)] private float projectileFlightSeconds;
        [SerializeField] private Stage2TowerUpgrade[] upgrades = Array.Empty<Stage2TowerUpgrade>();

        public Stage2Tower TowerPrefab => towerPrefab;
        public string RoleName => roleName;
        public int WoodCost => woodCost;
        public int StoneCost => stoneCost;
        public int MetalCost => metalCost;
        public int GoldCost => goldCost;
        public int TowerHealth => towerHealth;
        public bool CanAttack => canAttack;
        public float AttackRange => attackRange;
        public float FireInterval => fireInterval;
        public int AttackDamage => attackDamage;
        public float SplashRadiusCells => splashRadiusCells;
        public float ProjectileFlightSeconds => projectileFlightSeconds;
        public Stage2ResourceCost BuildCost => new Stage2ResourceCost(woodCost, stoneCost, metalCost, goldCost);
        public int MaximumLevel => 1 + (upgrades?.Length ?? 0);
        public bool TryGetUpgrade(int currentLevel, out Stage2TowerUpgrade upgrade)
        {
            upgrade = default;
            if (currentLevel < 1 || currentLevel >= MaximumLevel) return false;
            upgrade = upgrades[currentLevel - 1];
            return true;
        }
        public void ConfigureUpgrades(params Stage2TowerUpgrade[] tiers)
            => upgrades = tiers == null ? Array.Empty<Stage2TowerUpgrade>() : (Stage2TowerUpgrade[])tiers.Clone();

        public void Configure(Stage2Tower prefab, int wood, int stone, int metal, int health)
            => Configure(prefab, wood, stone, metal, health, 4.25f, 0.45f, 1);

        public void Configure(Stage2Tower prefab, int wood, int stone, int metal, int health,
            float range, float interval, int damage, int gold = 0, bool attacks = true)
        {
            towerPrefab = prefab;
            woodCost = wood;
            stoneCost = stone;
            metalCost = metal;
            goldCost = gold;
            towerHealth = health;
            attackRange = range;
            fireInterval = interval;
            attackDamage = damage;
            canAttack = attacks;
        }

        public bool TryValidate(out string reason)
        {
            if (towerPrefab == null || towerPrefab.gameObject.activeSelf ||
                string.IsNullOrWhiteSpace(roleName) ||
                woodCost < 0 || stoneCost < 0 || metalCost < 0 || goldCost < 0 ||
                woodCost + (long)stoneCost + metalCost + goldCost == 0 || towerHealth <= 0 ||
                attackRange <= 0f || float.IsNaN(attackRange) || float.IsInfinity(attackRange) ||
                fireInterval <= 0f || float.IsNaN(fireInterval) || float.IsInfinity(fireInterval) ||
                (canAttack ? attackDamage <= 0 : attackDamage != 0) || splashRadiusCells < 0f ||
                float.IsNaN(splashRadiusCells) || float.IsInfinity(splashRadiusCells) ||
                projectileFlightSeconds < 0f || float.IsNaN(projectileFlightSeconds) ||
                float.IsInfinity(projectileFlightSeconds))
            {
                reason = "Inactive defense prefab, positive cost/health, finite attack values and zero damage for passive defenses are required.";
                return false;
            }
            foreach (var upgrade in upgrades ?? Array.Empty<Stage2TowerUpgrade>())
                if (!upgrade.IsValid(canAttack))
                { reason = "Upgrade rows require positive prices/health, finite attack values and role-compatible damage."; return false; }
            reason = string.Empty;
            return true;
        }
    }
}
