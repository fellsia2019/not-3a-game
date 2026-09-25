using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using UnityEngine;

namespace Not3A.Stage2.WavesAI
{
    /// <summary>Explicit run-owned population; pass IsCellProtected to Defense.</summary>
    public sealed class Stage2EnemyGroup
    {
        private readonly Stage2MapDefinition map;
        private readonly Stage2Navigation navigation;
        private readonly Stage2EnemyCatalog catalog;
        private readonly List<Stage2Enemy> enemies = new List<Stage2Enemy>();
        private bool paused;

        public Stage2EnemyGroup(Stage2MapDefinition definition, Stage2Navigation sharedNavigation,
            Stage2EnemyCatalog settings)
        {
            if (definition == null || !definition.ValidateDefinition().IsValid)
                throw new ArgumentException("Invalid map.", nameof(definition));
            if (sharedNavigation == null || !sharedNavigation.IsValid ||
                sharedNavigation.MapHash != definition.ComputeDeterministicContentHash())
                throw new ArgumentException("Navigation must use the same valid map.", nameof(sharedNavigation));
            if (settings == null || !settings.TryValidate(out _))
                throw new ArgumentException("Invalid enemy catalog.", nameof(settings));
            map = definition;
            navigation = sharedNavigation;
            catalog = settings;
        }

        public int ActiveCount => enemies.Count;
        public Stage2Enemy GetEnemy(int index) => enemies[index];
        public event Action<int> ReachedThrone;
        public event Action<Stage2Enemy> EnemyRemoved;

        public bool IsCellProtected(Vector2Int cell)
        {
            foreach (var enemy in enemies)
                if (enemy != null && enemy.Protects(cell)) return true;
            return false;
        }

        public bool TryHitNearest(Vector2 world, float range, int damage)
        {
            if (paused || range <= 0f || damage <= 0) return false;
            Stage2Enemy nearest = null;
            var bestSquared = range * range;
            for (var index = 0; index < enemies.Count; index++)
            {
                var candidate = enemies[index];
                if (candidate == null || !candidate.IsAlive) continue;
                var squared = ((Vector2)candidate.transform.position - world).sqrMagnitude;
                if (squared > bestSquared) continue;
                // Spawn order wins an exact distance tie.
                if (nearest != null && squared == bestSquared) continue;
                nearest = candidate;
                bestSquared = squared;
            }
            return nearest != null && nearest.TakeDamage(damage);
        }

        public Stage2Enemy Spawn() => SpawnAt(map.EnemyEntrance);

        public Stage2Enemy SpawnAt(Vector2Int startCell)
        {
            var enemy = UnityEngine.Object.Instantiate(catalog.EnemyPrefab);
            try
            {
                enemy.Configure(map, navigation, catalog, startCell);
                enemy.SetPaused(paused);
                enemy.ReachedThrone += OnReachedThrone;
                enemy.Removed += OnRemoved;
                enemies.Add(enemy);
                enemy.gameObject.SetActive(true);
                return enemy;
            }
            catch
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(enemy.gameObject);
                else UnityEngine.Object.DestroyImmediate(enemy.gameObject);
                throw;
            }
        }

        public void SetPaused(bool value)
        {
            paused = value;
            foreach (var enemy in enemies) if (enemy != null) enemy.SetPaused(value);
        }

        public void ResetRun()
        {
            while (enemies.Count > 0)
            {
                var enemy = enemies[enemies.Count - 1];
                enemies.RemoveAt(enemies.Count - 1);
                if (enemy == null) continue;
                enemy.ReachedThrone -= OnReachedThrone;
                enemy.Removed -= OnRemoved;
                enemy.Despawn();
            }
            paused = false;
        }

        private void OnReachedThrone(Stage2Enemy _, int damage) => ReachedThrone?.Invoke(damage);

        private void OnRemoved(Stage2Enemy enemy)
        {
            enemy.ReachedThrone -= OnReachedThrone;
            enemy.Removed -= OnRemoved;
            if (enemies.Remove(enemy)) EnemyRemoved?.Invoke(enemy);
        }
    }
}
