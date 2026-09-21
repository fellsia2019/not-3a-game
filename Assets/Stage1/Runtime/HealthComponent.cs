using System;
using UnityEngine;

namespace Not3A.Stage1
{
    public sealed class HealthComponent : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximum = 5;

        private HealthPool pool;

        public event Action<HealthComponent> Changed;
        public event Action<HealthComponent> Died;

        public int Current => pool?.Current ?? maximum;
        public int Maximum => pool?.Maximum ?? maximum;
        public bool IsDead => pool?.IsEmpty ?? false;

        private void Awake()
        {
            pool = new HealthPool(maximum);
        }

        public void Configure(int maximumHealth)
        {
            maximum = Mathf.Max(1, maximumHealth);
        }

        public void TakeDamage(int amount)
        {
            if (pool == null)
            {
                pool = new HealthPool(maximum);
            }

            var diedNow = pool.ApplyDamage(amount);
            Changed?.Invoke(this);
            if (diedNow)
            {
                Died?.Invoke(this);
            }
        }
    }
}
