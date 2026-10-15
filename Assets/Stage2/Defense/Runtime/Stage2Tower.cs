using System;
using UnityEngine;

namespace Not3A.Stage2.Defense
{
    [DisallowMultipleComponent]
    public sealed class Stage2Tower : MonoBehaviour
    {
        private int health;
        private bool removed;
        private float attackRange;
        private float fireInterval;
        private int attackDamage;
        private float splashRadiusCells;
        private float projectileFlightSeconds;
        private float fireRemaining;
        private Func<Vector2, float, int, float, float, bool> attackTarget;
        private bool attackActive = true;

        public Vector2Int Cell { get; private set; }
        public int Health => health;
        public float AttackRange => attackRange;
        public bool IsAlive => !removed && health > 0;
        public event Action<Stage2Tower> Removed;

        internal void Configure(Vector2Int cell, int maximumHealth, float range, float interval, int damage,
            float splashRadius = 0f, float flightSeconds = 0f)
        {
            if (maximumHealth <= 0 || range <= 0f || interval <= 0f || damage <= 0 ||
                splashRadius < 0f || float.IsNaN(splashRadius) || float.IsInfinity(splashRadius) ||
                flightSeconds < 0f || float.IsNaN(flightSeconds) || float.IsInfinity(flightSeconds))
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            Cell = cell;
            health = maximumHealth;
            attackRange = range;
            fireInterval = interval;
            attackDamage = damage;
            splashRadiusCells = splashRadius;
            projectileFlightSeconds = flightSeconds;
            fireRemaining = 0f;
            removed = false;
        }

        internal void SetAttackTarget(Func<Vector2, float, int, float, float, bool> target) => attackTarget = target;
        internal void SetAttackActive(bool active) => attackActive = active;

        public void Tick(float elapsedSeconds)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!IsAlive || !attackActive || attackTarget == null || elapsedSeconds == 0f) return;
            fireRemaining = Mathf.Max(0f, fireRemaining - elapsedSeconds);
            if (fireRemaining > 0f || !attackTarget(transform.position, attackRange, attackDamage,
                    splashRadiusCells, projectileFlightSeconds)) return;
            fireRemaining = fireInterval;
        }

        public bool TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive) return false;
            health = Mathf.Max(0, health - amount);
            if (health == 0) RemoveOnce();
            return true;
        }

        private void Update() => Tick(Time.deltaTime);

        private void OnDestroy() => RemoveOnce();

        private void RemoveOnce()
        {
            if (removed) return;
            removed = true;
            Removed?.Invoke(this);
        }
    }
}
