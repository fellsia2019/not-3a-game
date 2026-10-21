using System;
using Not3A.Stage2.Economy;
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
        [SerializeField] private int throneHealthPerUpgrade = 5;
        [SerializeField] private Stage2ResourceCost[] throneUpgradeCosts = Array.Empty<Stage2ResourceCost>();
        // Empty on legacy assets: the original serialized fields remain a one-wave fallback.
        [SerializeField] private Stage2WaveDefinition[] waves = Array.Empty<Stage2WaveDefinition>();
        [SerializeField] private bool continuousDays;
        // Zero preserves finite legacy fixtures; Stage3 explicitly sets its timed night.
        [SerializeField, Min(0)] private float nightDurationSeconds;
        [SerializeField, Min(1)] private int additionalEnemiesPerNight = 60;
        [SerializeField] private bool daytimeGates;
        [SerializeField, Min(0)] private float gateCutoffMinutes = 30f;
        // Kept serialized for existing assets/configuration callers; D105 uses the night profile at runtime.
        [SerializeField, Min(.01f), Tooltip("Legacy fixed interval; daytime replay uses the last CLEARED night's Spawn Interval.")]
        private float daytimeSpawnInterval = 2.4f;
        [SerializeField, Min(1), Tooltip("Legacy cap retained for asset compatibility; current daytime flow has no enemy-count cap.")]
        private int daytimePopulationLimit = 24;
        [SerializeField, Min(0)] private float healthGrowthPerTier = .15f;
        [SerializeField, Min(0)] private float siegeGrowthPerTier = .1f;
        [SerializeField, Min(.01f)] private float sleepBeforeNightMinutes = 10f;

        public float PreparationSeconds => GetWave(0).PreparationSeconds;
        public int EnemyCount => GetWave(0).EnemyCount;
        public float SpawnInterval => GetWave(0).SpawnInterval;
        public int ThroneHealth => throneHealth;
        public int ThroneHealthPerUpgrade => throneHealthPerUpgrade;
        public int MaximumThroneUpgrades => throneUpgradeCosts?.Length ?? 0;
        public bool TryGetThroneUpgradeCost(int completedUpgrades, out Stage2ResourceCost cost)
        {
            cost = default;
            if (completedUpgrades < 0 || completedUpgrades >= MaximumThroneUpgrades) return false;
            cost = throneUpgradeCosts[completedUpgrades]; return true;
        }
        public void ConfigureThroneUpgrades(Stage2ResourceCost[] costs)
        {
            if (costs == null || costs.Length != 0 && costs.Length != 3)
                throw new ArgumentException("Throne reinforcement requires exactly three paid steps (or empty legacy data).", nameof(costs));
            throneUpgradeCosts = (Stage2ResourceCost[])costs.Clone();
        }
        public int WaveCount => waves != null && waves.Length > 0 ? waves.Length : 1;
        public bool ContinuousDays => continuousDays;
        public float NightDurationSeconds => nightDurationSeconds;
        public bool DaytimeGates => continuousDays && daytimeGates;
        public float GateCutoffMinutes => gateCutoffMinutes;
        public float DaytimeSpawnInterval => daytimeSpawnInterval;
        public int DaytimePopulationLimit => daytimePopulationLimit;
        public float SleepBeforeNightMinutes => sleepBeforeNightMinutes;
        public float HealthMultiplier(int tier) => 1f + Math.Max(0, tier - 1) * healthGrowthPerTier;
        public float SiegeMultiplier(int tier) => 1f + Math.Max(0, tier - 1) * siegeGrowthPerTier;
        public void ConfigureDaytimeGates(bool enabled, float cutoffMinutes = 30f, float interval = 2.4f, int limit = 24)
        { daytimeGates = enabled; gateCutoffMinutes = cutoffMinutes; daytimeSpawnInterval = interval; daytimePopulationLimit = limit; }

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

        public void ConfigureClockDurations(float daylightSeconds, float nighttimeSeconds)
        {
            preparationSeconds=daylightSeconds;nightDurationSeconds=nighttimeSeconds;
            if(waves!=null)for(var i=0;i<waves.Length;i++)
                waves[i]=new Stage2WaveDefinition(daylightSeconds,waves[i].EnemyCount,waves[i].SpawnInterval);
        }

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
            if(!Finite(nightDurationSeconds)||nightDurationSeconds<0f)
            {reason="Night duration must be finite and nonnegative (zero retains legacy finite nights).";return false;}
            if (throneHealthPerUpgrade != 5 || MaximumThroneUpgrades != 0 && MaximumThroneUpgrades != 3 ||
                (long)throneHealth + (long)MaximumThroneUpgrades * throneHealthPerUpgrade > int.MaxValue)
            { reason = "Throne has exactly 3 upgrades of +5 HP (or empty legacy data), within integer capacity."; return false; }
            if (throneUpgradeCosts != null) foreach (var cost in throneUpgradeCosts)
                if (!cost.IsValid || cost.IsZero)
                { reason = "Throne upgrades require valid nonzero complete resource prices."; return false; }
            if (daytimeGates && (!continuousDays || !Finite(gateCutoffMinutes) || gateCutoffMinutes < 0 || gateCutoffMinutes >= 720 ||
                !Finite(daytimeSpawnInterval) || daytimeSpawnInterval <= 0 || daytimePopulationLimit < 1 ||
                !Finite(healthGrowthPerTier) || healthGrowthPerTier < 0 || !Finite(siegeGrowthPerTier) || siegeGrowthPerTier < 0 ||
                !Finite(sleepBeforeNightMinutes) || sleepBeforeNightMinutes <= 0 || sleepBeforeNightMinutes >= 720))
            { reason = "Day gates require continuing days and finite valid cadence/cutoff/tier growth."; return false; }
            if (throneHealth <= 0 || (continuousDays && additionalEnemiesPerNight <= 0))
            {
                reason = "Throne health and continuous-night enemy growth must be positive.";
                return false;
            }
            for (var index = 0; index < WaveCount; index++)
            {
                if (!GetWave(index).IsValid)
                {
                    reason = $"Wave {index + 1}: preparation and enemy count must be positive; spawn interval must be finite and nonnegative.";
                    return false;
                }
                if (DaytimeGates && GetWave(index).SpawnInterval <= 0f)
                { reason = "Continuous daytime flow requires a positive night-profile spawn interval."; return false; }
            }
            reason = string.Empty;
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
