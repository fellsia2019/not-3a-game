using UnityEngine;

namespace Not3A.Stage2.WavesAI
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Enemy Catalog")]
    public sealed class Stage2EnemyCatalog : ScriptableObject
    {
        [SerializeField] private Stage2Enemy enemyPrefab;
        [SerializeField, Min(0.01f)] private float moveSpeed = 2f;
        [SerializeField, Min(1)] private int health = 3;
        [SerializeField, Min(0.01f)] private float siegeInterval = 0.5f;
        [SerializeField, Min(1)] private int siegeDamage = 1;
        [SerializeField, Min(1)] private int throneDamage = 1;

        public Stage2Enemy EnemyPrefab => enemyPrefab;
        public float MoveSpeed => moveSpeed;
        public int Health => health;
        public float SiegeInterval => siegeInterval;
        public int SiegeDamage => siegeDamage;
        public int ThroneDamage => throneDamage;

        public void Configure(Stage2Enemy prefab, float speed, int hitPoints,
            float attackInterval, int attackDamage, int contactDamage)
        {
            enemyPrefab = prefab;
            moveSpeed = speed;
            health = hitPoints;
            siegeInterval = attackInterval;
            siegeDamage = attackDamage;
            throneDamage = contactDamage;
        }

        public bool TryValidate(out string reason)
        {
            if (enemyPrefab == null || enemyPrefab.gameObject.activeSelf ||
                moveSpeed <= 0f || float.IsNaN(moveSpeed) || float.IsInfinity(moveSpeed) ||
                health <= 0 || siegeInterval <= 0f || float.IsNaN(siegeInterval) ||
                float.IsInfinity(siegeInterval) || siegeDamage <= 0 || throneDamage <= 0)
            {
                reason = "Inactive enemy prefab and finite positive movement/combat values are required.";
                return false;
            }
            reason = string.Empty;
            return true;
        }
    }
}
