using UnityEngine;

namespace Not3A.Stage1
{
    [RequireComponent(typeof(HealthComponent))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private int throneDamage = 1;
        [SerializeField, Min(0.05f)] private float siegeInterval = 0.5f;
        [SerializeField, Min(1)] private int siegeDamage = 1;

        private DynamicNavigationGrid navigation;
        private HealthComponent throne;
        private HealthComponent health;
        private WaveSpawner owner;
        private GridPathPlan plan;
        private TowerController blocker;
        private int pathIndex;
        private int plannedRevision = -1;
        private float siegeCooldown;
        private bool removed;

        public HealthComponent Health => health;

        public void Configure(
            DynamicNavigationGrid grid,
            HealthComponent throneHealth,
            WaveSpawner spawner,
            float moveSpeed,
            int contactDamage,
            float attackInterval,
            int attackDamage)
        {
            navigation = grid;
            throne = throneHealth;
            owner = spawner;
            speed = moveSpeed;
            throneDamage = contactDamage;
            siegeInterval = Mathf.Max(0.05f, attackInterval);
            siegeDamage = Mathf.Max(1, attackDamage);
            transform.position = navigation.EntranceWorld;
            Replan();
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
        }

        private void Update()
        {
            if (removed || navigation == null)
            {
                return;
            }

            if (plannedRevision != navigation.Revision || (blocker != null && blocker.IsDestroyed))
            {
                Replan();
            }

            if (plan == null)
            {
                return;
            }

            if (pathIndex < plan.Path.Count)
            {
                var destination = IsoGrid.CellToWorld(plan.Path[pathIndex]);
                transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, destination) <= 0.02f)
                {
                    pathIndex++;
                }

                return;
            }

            if (plan.ReachesGoal)
            {
                ReachThrone();
                return;
            }

            if (blocker == null || blocker.IsDestroyed)
            {
                Replan();
                return;
            }

            siegeCooldown -= Time.deltaTime;
            if (siegeCooldown <= 0f)
            {
                blocker.Health.TakeDamage(siegeDamage);
                siegeCooldown = siegeInterval;
            }
        }

        private void Replan()
        {
            var start = IsoGrid.WorldToCell(transform.position);
            plan = navigation.PlanFrom(start, out blocker);
            pathIndex = plan.Path.Count > 1 ? 1 : plan.Path.Count;
            plannedRevision = navigation.Revision;
            siegeCooldown = 0f;
        }

        private void ReachThrone()
        {
            if (removed)
            {
                return;
            }

            removed = true;
            throne?.TakeDamage(throneDamage);
            owner?.NotifyEnemyRemoved(this);
            Destroy(gameObject);
        }

        private void OnDied(HealthComponent _)
        {
            if (removed)
            {
                return;
            }

            removed = true;
            owner?.NotifyEnemyRemoved(this);
            Destroy(gameObject);
        }
    }
}
