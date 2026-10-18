using UnityEngine;

namespace Not3A.Stage2.Defense
{
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
            reason = string.Empty;
            return true;
        }
    }
}
