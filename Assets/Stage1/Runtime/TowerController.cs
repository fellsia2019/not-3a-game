using UnityEngine;

namespace Not3A.Stage1
{
    [RequireComponent(typeof(HealthComponent))]
    public sealed class TowerController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float range = 4.25f;
        [SerializeField, Min(0.05f)] private float fireInterval = 0.45f;
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField] private SpriteRenderer muzzleFlash;

        private float cooldown;
        private float flashRemaining;
        private HealthComponent health;
        private DynamicNavigationGrid navigation;
        private Vector2Int occupiedCell;

        public HealthComponent Health => health;
        public bool IsDestroyed => health != null && health.IsDead;
        public Vector2Int OccupiedCell => occupiedCell;

        public void Configure(float attackRange, float interval, int shotDamage, SpriteRenderer flash)
        {
            range = attackRange;
            fireInterval = interval;
            damage = shotDamage;
            muzzleFlash = flash;
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = false;
            }
        }

        public void ConfigureNavigation(DynamicNavigationGrid grid, Vector2Int cell)
        {
            navigation = grid;
            occupiedCell = cell;
        }

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }

            navigation?.UnregisterTower(occupiedCell, this);
        }

        private void Update()
        {
            if (flashRemaining > 0f)
            {
                flashRemaining -= Time.deltaTime;
                if (flashRemaining <= 0f && muzzleFlash != null)
                {
                    muzzleFlash.enabled = false;
                }
            }

            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }

            var target = FindTarget();
            if (target == null)
            {
                return;
            }

            target.Health.TakeDamage(damage);
            cooldown = fireInterval;
            flashRemaining = 0.08f;
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = true;
            }
        }

        private EnemyController FindTarget()
        {
            EnemyController best = null;
            var bestDistance = range;
            var enemies = FindObjectsByType<EnemyController>();
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= bestDistance)
                {
                    best = enemy;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void OnDied(HealthComponent _)
        {
            navigation?.UnregisterTower(occupiedCell, this);
            Destroy(gameObject);
        }
    }
}
