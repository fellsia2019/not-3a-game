using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage1
{
    public sealed class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private DynamicNavigationGrid navigation;
        [SerializeField] private HealthComponent throne;
        [SerializeField, Min(1)] private int enemyCount = 6;
        [SerializeField, Min(0f)] private float spawnInterval = 0.8f;
        [SerializeField, Min(0.1f)] private float enemySpeed = 2.2f;
        [SerializeField, Min(1)] private int contactDamage = 1;
        [SerializeField, Min(0.05f)] private float siegeInterval = 0.5f;
        [SerializeField, Min(1)] private int siegeDamage = 1;

        private readonly HashSet<EnemyController> activeEnemies = new HashSet<EnemyController>();
        [SerializeField] private Stage1GameController game;
        private Coroutine spawnRoutine;
        private bool spawningFinished;

        public int ActiveCount => activeEnemies.Count;

        public void Configure(
            Stage1GameController controller,
            EnemyController prefab,
            DynamicNavigationGrid grid,
            HealthComponent throneHealth,
            int count,
            float interval,
            float speed,
            int damage)
        {
            game = controller;
            enemyPrefab = prefab;
            navigation = grid;
            throne = throneHealth;
            enemyCount = Mathf.Max(1, count);
            spawnInterval = Mathf.Max(0f, interval);
            enemySpeed = Mathf.Max(0.1f, speed);
            contactDamage = Mathf.Max(1, damage);
        }

        public void BeginWave()
        {
            if (spawnRoutine != null)
            {
                return;
            }

            spawningFinished = false;
            spawnRoutine = StartCoroutine(SpawnWave());
        }

        public void NotifyEnemyRemoved(EnemyController enemy)
        {
            activeEnemies.Remove(enemy);
            if (spawningFinished && activeEnemies.Count == 0)
            {
                game?.NotifyWaveCleared();
            }
        }

        public void StopAndClear()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            activeEnemies.Clear();
        }

        private IEnumerator SpawnWave()
        {
            for (var index = 0; index < enemyCount; index++)
            {
                if (game == null || game.Phase != RunPhase.Wave)
                {
                    yield break;
                }

                var enemy = Instantiate(enemyPrefab, navigation.EntranceWorld, Quaternion.identity);
                enemy.name = $"Enemy {index + 1}";
                enemy.Configure(navigation, throne, this, enemySpeed, contactDamage, siegeInterval, siegeDamage);
                activeEnemies.Add(enemy);
                if (spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            spawningFinished = true;
            spawnRoutine = null;
            if (activeEnemies.Count == 0)
            {
                game?.NotifyWaveCleared();
            }
        }
    }
}
