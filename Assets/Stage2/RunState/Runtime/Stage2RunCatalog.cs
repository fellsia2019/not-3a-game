using System;
using UnityEngine;

namespace Not3A.Stage2.RunState
{
    [Serializable]
    public struct Stage2WaveDefinition
    {
        [SerializeField, Min(0.01f)] private float preparationSeconds;
        [SerializeField, Min(1)] private int enemyCount;
        [SerializeField, Min(0f)] private float spawnInterval;

        public Stage2WaveDefinition(float preparation, int enemies, float interval)
        {
            preparationSeconds = preparation;
            enemyCount = enemies;
            spawnInterval = interval;
        }

        public float PreparationSeconds => preparationSeconds;
        public int EnemyCount => enemyCount;
        public float SpawnInterval => spawnInterval;
        public bool IsValid => preparationSeconds > 0f && !float.IsInfinity(preparationSeconds) &&
            enemyCount > 0 && spawnInterval >= 0f && !float.IsInfinity(spawnInterval);
    }

    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Run Catalog")]
    public sealed class Stage2RunCatalog : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float preparationSeconds = 180f;
        [SerializeField, Min(1)] private int enemyCount = 6;
        [SerializeField, Min(0f)] private float spawnInterval = 0.8f;
        [SerializeField, Min(1)] private int throneHealth = 5;
        // Empty on legacy assets: the original serialized fields remain a one-wave fallback.
        [SerializeField] private Stage2WaveDefinition[] waves = Array.Empty<Stage2WaveDefinition>();
        [SerializeField] private bool continuousDays;
        [SerializeField, Min(1)] private int additionalEnemiesPerNight = 60;

        public float PreparationSeconds => GetWave(0).PreparationSeconds;
        public int EnemyCount => GetWave(0).EnemyCount;
        public float SpawnInterval => GetWave(0).SpawnInterval;
        public int ThroneHealth => throneHealth;
        public int WaveCount => waves != null && waves.Length > 0 ? waves.Length : 1;
        public bool ContinuousDays => continuousDays;

        public Stage2WaveDefinition GetWave(int index)
        {
            if (index < 0 || (!continuousDays && index >= WaveCount)) throw new ArgumentOutOfRangeException(nameof(index));
            var authored = waves != null && waves.Length > 0 ? waves[Math.Min(index, waves.Length - 1)] :
                new Stage2WaveDefinition(preparationSeconds, enemyCount, spawnInterval);
            if (index < WaveCount) return authored;
            var count = Math.Min(int.MaxValue, authored.EnemyCount +
                (long)(index - WaveCount + 1) * additionalEnemiesPerNight);
            return new Stage2WaveDefinition(authored.PreparationSeconds, (int)count, authored.SpawnInterval);
        }

        public void ConfigureContinuousDays(bool enabled, int extraEnemiesPerNight = 60)
        { continuousDays = enabled; additionalEnemiesPerNight = extraEnemiesPerNight; }

        public void ConfigureWaves(Stage2WaveDefinition[] sequence, int hitPoints)
        {
            if (sequence == null || sequence.Length == 0) throw new ArgumentException("A wave sequence is required.", nameof(sequence));
            waves = (Stage2WaveDefinition[])sequence.Clone();
            throneHealth = hitPoints;
        }

        public void Configure(float preparation, int enemies, float spawnSeconds, int hitPoints)
        {
            preparationSeconds = preparation;
            enemyCount = enemies;
            spawnInterval = spawnSeconds;
            throneHealth = hitPoints;
            waves = Array.Empty<Stage2WaveDefinition>();
        }

        public bool TryValidate(out string reason)
        {
            if (throneHealth <= 0 || (continuousDays && additionalEnemiesPerNight <= 0))
            {
                reason = "Throne health and continuous-night enemy growth must be positive.";
                return false;
            }
            for (var index = 0; index < WaveCount; index++)
                if (!GetWave(index).IsValid)
                {
                    reason = $"Wave {index + 1}: preparation and enemy count must be positive; spawn interval must be finite and nonnegative.";
                    return false;
                }
            reason = string.Empty;
            return true;
        }
    }
}
