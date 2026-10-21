using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using UnityEngine;

namespace Not3A.Stage2.WavesAI
{
    /// <summary>Run-owned shot facts. Neither flight nor impact requires a renderer or collider.</summary>
    public sealed class Stage2Shot
    {
        public Vector2 Origin { get; internal set; }
        public Vector2 Position { get; internal set; }
        public float Duration { get; internal set; }
        public float Elapsed { get; internal set; }
        public float SplashRadiusCells { get; internal set; }
        public float Progress => Duration <= 0f ? 1f : Mathf.Clamp01(Elapsed / Duration);
        internal Stage2Enemy Target;
        internal int Damage;
    }

    /// <summary>Explicit run-owned population; pass IsCellProtected to Defense.</summary>
    public sealed class Stage2EnemyGroup
    {
        private readonly Stage2MapDefinition map;
        private readonly Stage2Navigation navigation;
        private readonly Stage2EnemyCatalog catalog;
        private readonly List<Stage2Enemy> enemies = new List<Stage2Enemy>();
        private bool paused;
        private readonly List<Stage2Shot> shots = new List<Stage2Shot>();

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
        public int ActiveDaytimeCount
        { get { var count = 0; foreach (var enemy in enemies) if (enemy != null && enemy.Source == Stage2EnemySource.Daytime) count++; return count; } }
        public Stage2Enemy GetEnemy(int index) => enemies[index];
        public event Action<int> ReachedThrone;
        public event Action<Stage2Enemy> EnemyRemoved;
        public event Action<Stage2Shot> ShotLaunched;
        public event Action<Stage2Shot> ShotImpacted;
        public event Action ShotsCleared;
        public int PendingShotCount => shots.Count;

        public bool IsCellProtected(Vector2Int cell)
        {
            foreach (var enemy in enemies)
                if (enemy != null && enemy.Protects(cell)) return true;
            return false;
        }

        public bool TryHitNearest(Vector2 world, float range, int damage, float splashRadiusCells)
            => TryHitNearest(world, range, damage, splashRadiusCells, 0f);

        public bool TryHitNearest(Vector2 world, float range, int damage, float splashRadiusCells, float flightSeconds)
        {
            if (paused || range <= 0f || damage <= 0 || splashRadiusCells < 0f ||
                float.IsNaN(splashRadiusCells) || float.IsInfinity(splashRadiusCells) ||
                flightSeconds < 0f || float.IsNaN(flightSeconds) || float.IsInfinity(flightSeconds)) return false;
            var nearest=FindNearest(world,range);
            if (nearest == null) return false;
            var shot = new Stage2Shot { Origin = world, Position = nearest.transform.position,
                Target = nearest, Damage = damage, SplashRadiusCells = splashRadiusCells, Duration = flightSeconds };
            if (flightSeconds > 0f) shots.Add(shot);
            ShotLaunched?.Invoke(shot);
            if (flightSeconds == 0f) ResolveShot(shot);
            return true;
        }

        // Read-only target query shared by combat and continuous visual aiming.
        public Stage2Enemy FindNearest(Vector2 world, float range)
        {
            if (paused || range <= 0f) return null;
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
            return nearest;
        }

        public void TickShots(float elapsedSeconds)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (paused || elapsedSeconds == 0f) return;
            for (var index = shots.Count - 1; index >= 0; index--)
            {
                var shot = shots[index];
                if (shot.Target != null && shot.Target.IsAlive) shot.Position = shot.Target.transform.position;
                shot.Elapsed = Mathf.Min(shot.Duration, shot.Elapsed + elapsedSeconds);
                if (shot.Progress < 1f) continue;
                shots.RemoveAt(index);
                ResolveShot(shot);
            }
        }

        private void ResolveShot(Stage2Shot shot)
        {
            // Emit even if another shot already killed the target: the projectile still lands.
            ShotImpacted?.Invoke(shot);
            if (shot.SplashRadiusCells == 0f)
            {
                if (shot.Target != null && shot.Target.IsAlive) shot.Target.TakeDamage(shot.Damage);
                return;
            }

            // The blast follows the selected enemy's position in map-cell space.
            // Iterate backwards because a lethal hit removes its enemy immediately.
            var center = map.WorldToCellCoordinates(shot.Position);
            var radiusSquared = shot.SplashRadiusCells * shot.SplashRadiusCells;
            for (var index = enemies.Count - 1; index >= 0; index--)
            {
                var candidate = enemies[index];
                if (candidate == null || !candidate.IsAlive) continue;
                var position = map.WorldToCellCoordinates(candidate.transform.position);
                if ((position - center).sqrMagnitude <= radiusSquared)
                    candidate.TakeDamage(shot.Damage);
            }
        }

        public Stage2Enemy Spawn() => SpawnAt(map.EnemyEntrance);
        public Stage2Enemy Spawn(Stage2EnemySource source, int tier, float healthMultiplier, float siegeMultiplier)
            => SpawnAt(map.EnemyEntrance, source, tier, healthMultiplier, siegeMultiplier);

        public Stage2Enemy SpawnAt(Vector2Int startCell, Stage2EnemySource source = Stage2EnemySource.Night,
            int tier = 1, float healthMultiplier = 1f, float siegeMultiplier = 1f)
        {
            var enemy = UnityEngine.Object.Instantiate(catalog.EnemyPrefab);
            try
            {
                enemy.ConfigureFromValidatedGroup(map, navigation, catalog, startCell);
                enemy.SetThreat(source, tier, healthMultiplier, siegeMultiplier);
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
            shots.Clear();
            ShotsCleared?.Invoke();
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
