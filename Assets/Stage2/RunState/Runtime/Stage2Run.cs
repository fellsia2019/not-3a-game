using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage2.RunState
{
    public enum Stage2RunPhase { Preparation, Wave, Won, Lost }
    public enum Stage2ThroneUpgradeResult { Ready, Upgraded, Inactive, MaximumUpgrades, InsufficientResources }

    /// <summary>Authored waves with optional continuing days. UI observes Changed; it does not own rules.</summary>
    public sealed class Stage2Run : IDisposable
    {
        private readonly Stage2RunCatalog catalog;
        private readonly Stage2Navigation navigation;
        private readonly Stage2Economy economy;
        private readonly Stage2Defense defense;
        private readonly Stage2EnemyGroup enemies;
        private readonly System.Random rewardRandom = new System.Random();
        private int remainingToSpawn;
        private float spawnRemaining;
        private bool spawningFinished;
        private bool disposed;
        private bool upgradingThrone;
        private double nextNightMinutes = 20d * 60d;
        private double nightSpawnsEndMinutes;
        private float daytimeSpawnRemaining;
        private sealed class Checkpoint
        {
            public int Wave, Health, ThroneUpgrades;
            public double Minutes;
            public Stage2Economy.Snapshot Economy;
            public Stage2Defense.Snapshot Defense;
        }
        private Checkpoint checkpoint;
        private bool rewinding;

        public Stage2Run(Stage2RunCatalog data, Stage2Navigation sharedNavigation,
            Stage2Economy sharedEconomy, Stage2Defense sharedDefense, Stage2EnemyGroup group)
        {
            if (data == null || !data.TryValidate(out _)) throw new ArgumentException("Invalid run catalog.", nameof(data));
            catalog = data;
            navigation = sharedNavigation ?? throw new ArgumentNullException(nameof(sharedNavigation));
            economy = sharedEconomy ?? throw new ArgumentNullException(nameof(sharedEconomy));
            defense = sharedDefense ?? throw new ArgumentNullException(nameof(sharedDefense));
            enemies = group ?? throw new ArgumentNullException(nameof(group));
            enemies.ReachedThrone += OnReachedThrone;
            enemies.EnemyRemoved += OnEnemyRemoved;
            economy.NodesChanged += OnResourceNodesChanged;
            defense.SetAttackTarget(enemies.TryHitNearest);
            Phase = Stage2RunPhase.Preparation;
            PreparationRemaining = catalog.PreparationSeconds;
            ThroneHealth = catalog.ThroneHealth;
            SyncDefense();
        }

        public Stage2RunPhase Phase { get; private set; }
        public float PreparationRemaining { get; private set; }
        public int ThroneHealth { get; private set; }
        public int ThroneUpgradeCount { get; private set; }
        public int ThroneUpgradeLimit => catalog.MaximumThroneUpgrades;
        public int ThroneHealthPerUpgrade => catalog.ThroneHealthPerUpgrade;
        public int ThroneMaximum => catalog.ThroneHealth + ThroneUpgradeCount * catalog.ThroneHealthPerUpgrade;
        public bool TryGetThroneUpgradeCost(out Stage2ResourceCost cost) => catalog.TryGetThroneUpgradeCost(ThroneUpgradeCount, out cost);
        public Stage2ThroneUpgradeResult EvaluateThroneUpgrade()
        {
            if (disposed || rewinding || upgradingThrone || IsPaused || IsTerminal || ThroneHealth <= 0)
                return Stage2ThroneUpgradeResult.Inactive;
            if (!TryGetThroneUpgradeCost(out var cost)) return Stage2ThroneUpgradeResult.MaximumUpgrades;
            return economy.Inventory.CanAfford(cost) ? Stage2ThroneUpgradeResult.Ready : Stage2ThroneUpgradeResult.InsufficientResources;
        }
        public Stage2ThroneUpgradeResult TryUpgradeThrone()
        {
            var result = EvaluateThroneUpgrade();
            if (result != Stage2ThroneUpgradeResult.Ready) return result;
            upgradingThrone = true;
            try
            {
                TryGetThroneUpgradeCost(out var cost);
                if (!economy.Inventory.TrySpend(cost)) return Stage2ThroneUpgradeResult.InsufficientResources;
                ThroneUpgradeCount++;
                ThroneHealth += catalog.ThroneHealthPerUpgrade;
                // Fill only the added capacity, preserving the absolute damage already taken.
                Changed?.Invoke(); return Stage2ThroneUpgradeResult.Upgraded;
            }
            finally { upgradingThrone = false; }
        }
        public int ActiveEnemies => enemies.ActiveCount;
        public int WaveIndex { get; private set; }
        public int WaveNumber => WaveIndex + 1;
        public int WaveCount => catalog.WaveCount;
        public bool ContinuousDays => catalog.ContinuousDays;
        public bool UsesDaytimeGates => catalog.DaytimeGates;
        public bool UsesTimedNight => UsesDaytimeGates && catalog.NightDurationSeconds > 0f;
        public float NightDuration => UsesTimedNight ? catalog.NightDurationSeconds : PreparationDuration;
        // From dawn through daylight then night. Clock hours have unequal real-time
        // rates, so the analog HUD follows actual600/180-second phase proportions.
        public float DayNightCycleProgress
        {
            get
            {
                var minutes=(WorldMinutes-480d)%1440d;if(minutes<0)minutes+=1440d;
                var total=(double)PreparationDuration+NightDuration;
                return (float)(minutes<720d ? minutes/720d*PreparationDuration/total :
                    (PreparationDuration+(minutes-720d)/720d*NightDuration)/total);
            }
        }
        public bool PreserveProgressOnRetry => UsesDaytimeGates;
        public bool IsDay => ContinuousDays && Phase == Stage2RunPhase.Preparation && (!UsesDaytimeGates || !IsNightTime);
        public bool IsNight => ContinuousDays && Phase == Stage2RunPhase.Wave;
        public bool DaytimeGateOpen { get; private set; }
        public int LastClearedTier => WaveIndex;
        public float DaytimeSpawnInterval => LastClearedTier > 0 ? catalog.GetWave(LastClearedTier - 1).SpawnInterval : 0f;
        public int DaytimeEnemies => enemies.ActiveDaytimeCount;
        public bool CanOpenDaytimeGate => UsesDaytimeGates && !disposed && !IsPaused && !IsTerminal &&
            IsDay && LastClearedTier > 0 && !DaytimeGateOpen && WorldMinutes < nextNightMinutes - catalog.GateCutoffMinutes;
        public bool CanSleepUntilNight => UsesDaytimeGates && !disposed && !IsPaused && !IsTerminal && IsDay &&
            !DaytimeGateOpen && enemies.ActiveCount == 0 && enemies.PendingShotCount == 0 &&
            WorldMinutes < nextNightMinutes - catalog.SleepBeforeNightMinutes;
        public double WorldMinutes { get; private set; } = 8d * 60d;
        public int CalendarDay => (int)Math.Floor(WorldMinutes / 1440d) + 1;
        public int ClockHour => (int)(WorldMinutes % 1440d) / 60;
        public int ClockMinute => (int)(WorldMinutes % 60d);
        public bool IsNightTime => ContinuousDays && (ClockHour < 8 || ClockHour >= 20);
        public float PreparationDuration => catalog.GetWave(WaveIndex).PreparationSeconds;
        public bool IsFinalWave => !ContinuousDays && WaveNumber == WaveCount;
        // Forecast only for a full timed night; it never limits streaming spawns.
        public int WaveEnemyCount => UsesTimedNight ? (int)Math.Min(int.MaxValue,
            Math.Ceiling((double)NightDuration / catalog.GetWave(WaveIndex).SpawnInterval)) : catalog.GetWave(WaveIndex).EnemyCount;
        public bool IsPaused { get; private set; }
        public bool IsTerminal => Phase == Stage2RunPhase.Won || Phase == Stage2RunPhase.Lost;
        public bool SpawningFinished => spawningFinished;
        public event Action Changed;
        public event Action Reset;
        public event Action BeforeWaveStarted;
        public event Action Rewound;
        public event Action<int> ThroneDamaged;
        public bool CanRetry => !disposed && !rewinding && Phase == Stage2RunPhase.Lost &&
            checkpoint != null && (PreserveProgressOnRetry || economy.MatchesSnapshot(checkpoint.Economy));

        public void Tick(float elapsedSeconds)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (disposed || IsPaused || IsTerminal || elapsedSeconds == 0f) return;
            if (UsesDaytimeGates) { TickGateCycle(elapsedSeconds); return; }
            if (ContinuousDays) WorldMinutes += elapsedSeconds * 720d / Math.Max(1d, PreparationDuration);
            enemies.TickShots(elapsedSeconds);
            if (IsTerminal) return;
            if (Phase == Stage2RunPhase.Preparation)
            {
                PreparationRemaining = Mathf.Max(0f, PreparationRemaining - elapsedSeconds);
                if (PreparationRemaining <= 0f) StartWave();
                else Changed?.Invoke();
                return;
            }

            if (remainingToSpawn > 0)
            {
                spawnRemaining -= elapsedSeconds;
                while (remainingToSpawn > 0 && spawnRemaining <= 0f)
                {
                    enemies.Spawn();
                    remainingToSpawn--;
                    spawnRemaining += catalog.GetWave(WaveIndex).SpawnInterval;
                }
                if (remainingToSpawn == 0) spawningFinished = true;
                Changed?.Invoke();
            }
            TryCompleteWave();
        }

        public bool CanStartNextWaveForTesting=>!disposed&&!rewinding&&!upgradingThrone&&!IsPaused&&Phase==Stage2RunPhase.Preparation;
        /// <summary>Explicit development command: move only the clock to the next night, never simulate skipped income or delete live threats.</summary>
        public bool StartNextWaveForTesting()
        {
            if(!CanStartNextWaveForTesting)return false;
            if(UsesDaytimeGates)WorldMinutes=Math.Max(WorldMinutes,nextNightMinutes);
            return StartWave();
        }
        public bool StartWave()
        {
            if (disposed || IsPaused || Phase != Stage2RunPhase.Preparation) return false;
            if (UsesDaytimeGates && (!IsNightTime || WorldMinutes + .000001d < nextNightMinutes)) return false;
            checkpoint = new Checkpoint { Wave = WaveIndex, Health = ThroneHealth, ThroneUpgrades = ThroneUpgradeCount, Minutes = WorldMinutes,
                Economy = PreserveProgressOnRetry ? null : economy.CaptureSnapshot(),
                Defense = PreserveProgressOnRetry ? null : defense.CaptureSnapshot() };
            BeforeWaveStarted?.Invoke(); // Same boundary for timer expiry and local/manual launch, before any spawn.
            // Local day skip advances the clock only; no skipped-time simulation is credited.
            if (ContinuousDays && !UsesDaytimeGates && !IsNightTime)
                WorldMinutes = Math.Floor(WorldMinutes / 1440d) * 1440d + 20d * 60d;
            if (UsesDaytimeGates)
            {
                DaytimeGateOpen = false; daytimeSpawnRemaining = 0f;
                nextNightMinutes = NextNightAfter(WorldMinutes);
            }
            Phase = Stage2RunPhase.Wave;
            PreparationRemaining = 0f;
            var wave = catalog.GetWave(WaveIndex);
            remainingToSpawn = UsesTimedNight ? 0 : wave.EnemyCount - 1;
            spawningFinished = !UsesTimedNight && remainingToSpawn == 0;
            nightSpawnsEndMinutes = UsesTimedNight ? NextDawnAfter(WorldMinutes) : 0d;
            spawnRemaining = wave.SpawnInterval;
            SyncDefense(); // Preview survives Preparation -> Wave.
            SpawnNightEnemy();
            Changed?.Invoke();
            return true;
        }

        public bool OpenDaytimeGate()
        {
            if (!CanOpenDaytimeGate) return false;
            // Temporal anchor only: D087 keeps the current world, including all gains/spending.
            checkpoint = new Checkpoint { Wave = WaveIndex, Minutes = WorldMinutes };
            DaytimeGateOpen = true; Changed?.Invoke(); return true;
        }
        public bool SleepUntilBeforeNight()
        {
            if (!CanSleepUntilNight) return false;
            // Explicit owner-approved sleep: no skipped-time simulation or production grants.
            WorldMinutes = nextNightMinutes - catalog.SleepBeforeNightMinutes;
            PreparationRemaining = (float)(catalog.SleepBeforeNightMinutes * PreparationDuration / 720d);
            economy.CancelGather(); Changed?.Invoke(); return true;
        }
        public bool CloseDaytimeGate()
        {
            if (!UsesDaytimeGates || disposed || IsPaused || IsTerminal || !DaytimeGateOpen) return false;
            DaytimeGateOpen = false; Changed?.Invoke(); return true;
        }
        private static double NextNightAfter(double minutes) =>
            (Math.Floor((minutes - 1200d) / 1440d) + 1d) * 1440d + 1200d;
        private static double NextDawnAfter(double minutes) =>
            (Math.Floor((minutes - 480d) / 1440d) + 1d) * 1440d + 480d;
        private void SpawnNightEnemy()
        {
            if (!UsesDaytimeGates) { enemies.Spawn(); return; }
            var tier = WaveNumber;
            enemies.Spawn(Stage2EnemySource.Night, tier, catalog.HealthMultiplier(tier), catalog.SiegeMultiplier(tier));
        }
        private void TickGateCycle(float elapsed)
        {
            var remaining = (double)elapsed;
            while (remaining > .00000001d && !IsTerminal)
            {
                var rate = 720d / Math.Max(1d, UsesTimedNight && IsNightTime ? NightDuration : PreparationDuration);
                var cutoff = nextNightMinutes - catalog.GateCutoffMinutes;
                var boundary = DaytimeGateOpen && cutoff > WorldMinutes ? cutoff : nextNightMinutes;
                if(UsesTimedNight)boundary=Math.Min(boundary,IsNightTime?NextDawnAfter(WorldMinutes):NextNightAfter(WorldMinutes));
                var part = Math.Min(remaining, Math.Max(0d, (boundary - WorldMinutes) / rate));
                WorldMinutes += part * rate;
                var delta = (float)part;
                enemies.TickShots(delta);
                if (IsTerminal) break;
                if (Phase == Stage2RunPhase.Wave)
                {
                    spawnRemaining -= delta;
                    while (spawnRemaining <= 0f && (UsesTimedNight ? !spawningFinished &&
                        WorldMinutes + spawnRemaining * rate < nightSpawnsEndMinutes - .000001d : remainingToSpawn > 0))
                    {
                        SpawnNightEnemy(); if(!UsesTimedNight)remainingToSpawn--;
                        spawnRemaining += catalog.GetWave(WaveIndex).SpawnInterval;
                    }
                    if (UsesTimedNight ? WorldMinutes + .000001d >= nightSpawnsEndMinutes : remainingToSpawn == 0) spawningFinished = true;
                    TryCompleteWave();
                }
                else if (DaytimeGateOpen)
                {
                    // Repeat the cleared night's difficulty, not its finite enemy count.
                    daytimeSpawnRemaining -= delta;
                    while (daytimeSpawnRemaining <= 0f &&
                        WorldMinutes + daytimeSpawnRemaining * rate < cutoff - .000001d)
                    {
                        var tier = LastClearedTier;
                        enemies.Spawn(Stage2EnemySource.Daytime, tier, catalog.HealthMultiplier(tier), catalog.SiegeMultiplier(tier));
                        daytimeSpawnRemaining += DaytimeSpawnInterval;
                    }
                }
                if (WorldMinutes + .000001d >= cutoff) DaytimeGateOpen = false;
                if (WorldMinutes + .000001d >= nextNightMinutes)
                {
                    WorldMinutes = nextNightMinutes;
                    if (Phase == Stage2RunPhase.Preparation) StartWave();
                    else nextNightMinutes += 1440d; // Do not queue a second horde over an unfinished one.
                }
                remaining -= part;
            }
            PreparationRemaining = Phase == Stage2RunPhase.Preparation ?
                (float)Math.Max(0d, (nextNightMinutes - WorldMinutes) * PreparationDuration / 720d) : 0f;
            Changed?.Invoke();
        }

        public void SetPaused(bool value)
        {
            if (disposed || IsTerminal || IsPaused == value) return;
            IsPaused = value;
            enemies.SetPaused(value);
            if (value) economy.CancelGather();
            SyncDefense();
            Changed?.Invoke();
        }

        public void ResetRun()
        {
            if (disposed) return;
            InvalidateCheckpoint();
            enemies.ResetRun();
            defense.ResetRun();
            navigation.Reset(); // Always invalidates the old run's plan generation.
            economy.Reset();
            Phase = Stage2RunPhase.Preparation;
            IsPaused = false;
            WaveIndex = 0;
            WorldMinutes = 8d * 60d;
            nextNightMinutes = 20d * 60d; DaytimeGateOpen = false; daytimeSpawnRemaining = 0;
            nightSpawnsEndMinutes = 0d;
            PreparationRemaining = catalog.PreparationSeconds;
            ThroneUpgradeCount = 0;
            ThroneHealth = catalog.ThroneHealth;
            remainingToSpawn = 0;
            spawnRemaining = 0f;
            spawningFinished = false;
            SyncDefense();
            Reset?.Invoke();
            Changed?.Invoke();
        }

        public void InvalidateCheckpoint() => checkpoint = null;
        private void OnResourceNodesChanged()
        {
            // A full snapshot requires the same node identities. A temporal anchor
            // does not: streamed/rebuilt resources must not disable a time-only retry.
            if (!PreserveProgressOnRetry) InvalidateCheckpoint();
        }

        public bool RetryFailedWave()
        {
            if (!CanRetry) return false;
            rewinding = true;
            try
            {
                if (PreserveProgressOnRetry)
                {
                    enemies.ResetRun(); economy.CancelGather();
                    WorldMinutes = Math.Floor(checkpoint.Minutes / 1440d) * 1440d + 480d;
                    nextNightMinutes = NextNightAfter(WorldMinutes);
                    Phase = Stage2RunPhase.Preparation; IsPaused = false; ThroneHealth = ThroneMaximum;
                    DaytimeGateOpen = false; daytimeSpawnRemaining = 0f;
                    nightSpawnsEndMinutes = 0d;
                    remainingToSpawn = 0; spawnRemaining = 0; spawningFinished = false;
                    PreparationRemaining = PreparationDuration;
                    Rewound?.Invoke(); SyncDefense(); Changed?.Invoke(); return true;
                }
                if (!defense.RestoreSnapshot(checkpoint.Defense)) return false;
                enemies.ResetRun();
                economy.RestoreSnapshot(checkpoint.Economy);
                WaveIndex = checkpoint.Wave; ThroneUpgradeCount = checkpoint.ThroneUpgrades; ThroneHealth = checkpoint.Health;
                // A full continuing-day preparation starts at dawn, with no skipped-time income.
                WorldMinutes = ContinuousDays ? Math.Floor(checkpoint.Minutes / 1440d) * 1440d + 8d * 60d : checkpoint.Minutes;
                Phase = Stage2RunPhase.Preparation; IsPaused = false;
                PreparationRemaining = PreparationDuration;
                remainingToSpawn = 0; spawnRemaining = 0f; spawningFinished = false;
                Rewound?.Invoke();
                SyncDefense(); Changed?.Invoke();
                return true;
            }
            finally { rewinding = false; }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            enemies.ReachedThrone -= OnReachedThrone;
            enemies.EnemyRemoved -= OnEnemyRemoved;
            economy.NodesChanged -= OnResourceNodesChanged;
            defense.SetAttackTarget(null);
        }

        private void OnReachedThrone(int damage)
        {
            if (disposed || IsTerminal || (!UsesDaytimeGates && Phase != Stage2RunPhase.Wave) || damage <= 0) return;
            var before = ThroneHealth;
            ThroneHealth = Mathf.Max(0, ThroneHealth - damage);
            ThroneDamaged?.Invoke(before - ThroneHealth);
            if (ThroneHealth == 0) End(Stage2RunPhase.Lost);
            else Changed?.Invoke();
        }

        private void OnEnemyRemoved(Stage2Enemy enemy)
        {
            if (disposed) return;
            if ((Phase == Stage2RunPhase.Wave || (UsesDaytimeGates && !IsTerminal)) && enemy.WasKilled && economy.GoldPerKill > 0)
            {
                var reward = economy.GoldRewardForRoll(rewardRandom.NextDouble());
                if (reward > 0) economy.Inventory.TryAddGold(reward);
            }
            TryCompleteWave();
        }

        private void TryCompleteWave()
        {
            if (Phase != Stage2RunPhase.Wave || !spawningFinished || enemies.ActiveCount != 0 ||
                enemies.PendingShotCount != 0 || ThroneHealth <= 0)
                return;
            if (IsFinalWave)
            {
                End(Stage2RunPhase.Won);
                return;
            }

            // An early victory brings dawn. A horde that already reached morning
            // preserves the forward-moving clock and still earns full preparation.
            if (ContinuousDays && !UsesDaytimeGates && IsNightTime)
            {
                var morning = Math.Floor(WorldMinutes / 1440d) * 1440d + 8d * 60d;
                WorldMinutes = morning > WorldMinutes ? morning : morning + 1440d;
            }
            WaveIndex++;
            if (UsesDaytimeGates) nextNightMinutes = NextNightAfter(WorldMinutes);
            Phase = Stage2RunPhase.Preparation;
            PreparationRemaining = catalog.GetWave(WaveIndex).PreparationSeconds;
            remainingToSpawn = 0;
            spawnRemaining = 0f;
            spawningFinished = false;
            if (UsesDaytimeGates) PreparationRemaining = (float)((nextNightMinutes - WorldMinutes) * PreparationDuration / 720d);
            // Recovery preserves the world, health, inventory and renewable-node timers.
            SyncDefense();
            Changed?.Invoke();
        }

        private void End(Stage2RunPhase outcome)
        {
            if (IsTerminal) return;
            Phase = outcome;
            DaytimeGateOpen = false;
            IsPaused = false;
            economy.CancelGather();
            SyncDefense();
            if (outcome == Stage2RunPhase.Lost) enemies.ResetRun();
            Changed?.Invoke();
        }

        private void SyncDefense()
        {
            var buildPhase = Phase == Stage2RunPhase.Preparation ? Stage2BuildPhase.Preparation :
                Phase == Stage2RunPhase.Wave ? Stage2BuildPhase.Wave : Stage2BuildPhase.Inactive;
            defense.SetRunState(buildPhase, IsPaused);
            defense.SetAttackActive(!IsPaused && !IsTerminal);
        }
    }
}
