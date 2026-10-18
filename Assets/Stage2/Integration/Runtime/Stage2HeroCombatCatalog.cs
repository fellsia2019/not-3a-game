using UnityEngine;

namespace Not3A.Stage2.Integration
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Hero Combat Catalog")]
    public sealed class Stage2HeroCombatCatalog : ScriptableObject
    {
        [SerializeField, Min(.01f)] private float attackRange = 5.5f;
        [SerializeField, Min(.01f)] private float fireInterval = 1.1f;
        [SerializeField, Min(1)] private int attackDamage = 2;
        [SerializeField, Min(0f)] private float projectileFlightSeconds = .18f;

        public float AttackRange => attackRange;
        public float FireInterval => fireInterval;
        public int AttackDamage => attackDamage;
        public float ProjectileFlightSeconds => projectileFlightSeconds;

        public bool TryValidate(out string reason)
        {
            if (!Finite(attackRange) || attackRange <= 0f || !Finite(fireInterval) || fireInterval <= 0f ||
                attackDamage <= 0 || !Finite(projectileFlightSeconds) || projectileFlightSeconds < 0f)
            {
                reason = "Hero range/interval/damage must be positive; flight duration must be finite and nonnegative.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
