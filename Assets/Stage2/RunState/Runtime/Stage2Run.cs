using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage2.RunState
{
    public enum Stage2RunPhase { Preparation, Wave, Won, Lost }

    /// <summary>Authored waves with optional continuing days. UI observes Changed; it does not own rules.</summary>
    public sealed class Stage2Run : IDisposable
    {
        private readonly Stage2RunCatalog catalog;
        private readonly Stage2Navigation navigation;
        private readonly Stage2Economy economy;
        private readonly Stage2Defense defense;
        private readonly Stage2EnemyGroup enemies;
        private int remainingToSpawn;
        private float spawnRemaining;
        private bool spawningFinished;
        private bool disposed;

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
            defense.SetAttackTarget(enemies.TryHitNearest);
            Phase = Stage2RunPhase.Preparation;
            PreparationRemaining = catalog.PreparationSeconds;
            ThroneHealth = catalog.ThroneHealth;
            SyncDefense();
        }

        public Stage2RunPhase Phase { get; private set; }
        public float PreparationRemaining { get; private set; }
        public int ThroneHealth { get; private set; }
        public int ThroneMaximum => catalog.ThroneHealth;
        public int ActiveEnemies => enemies.ActiveCount;
        public int WaveIndex { get; private set; }
        public int WaveNumber => WaveIndex + 1;
        public int WaveCount => catalog.WaveCount;
        public bool ContinuousDays => catalog.ContinuousDays;
        public bool IsDay => ContinuousDays && Phase == Stage2RunPhase.Preparation;
        public bool IsNight => ContinuousDays && Phase == Stage2RunPhase.Wave;
        public double WorldMinutes { get; private set; } = 8d * 60d;
        public int CalendarDay => (int)Math.Floor(WorldMinutes / 1440d) + 1;
        public int ClockHour => (int)(WorldMinutes % 1440d) / 60;
        public int ClockMinute => (int)(WorldMinutes % 60d);
        public bool IsNightTime => ContinuousDays && (ClockHour < 8 || ClockHour >= 20);
        public float PreparationDuration => catalog.GetWave(WaveIndex).PreparationSeconds;
        public bool IsFinalWave => !ContinuousDays && WaveNumber == WaveCount;
        public int WaveEnemyCount => catalog.GetWave(WaveIndex).EnemyCount;
        public bool IsPaused { get; private set; }
        public bool IsTerminal => Phase == Stage2RunPhase.Won || Phase == Stage2RunPhase.Lost;
        public bool SpawningFinished => spawningFinished;
        public event Action Changed;
        public event Action Reset;

        public void Tick(float elapsedSeconds)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (disposed || IsPaused || IsTerminal || elapsedSeconds == 0f) return;
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

        public bool StartWave()
        {
            if (disposed || IsPaused || Phase != Stage2RunPhase.Preparation) return false;
            // Local day skip advances the clock only; no skipped-time simulation is credited.
            if (ContinuousDays && !IsNightTime)
                WorldMinutes = Math.Floor(WorldMinutes / 1440d) * 1440d + 20d * 60d;
            Phase = Stage2RunPhase.Wave;
            PreparationRemaining = 0f;
            var wave = catalog.GetWave(WaveIndex);
            remainingToSpawn = wave.EnemyCount - 1;
            spawningFinished = remainingToSpawn == 0;
            spawnRemaining = wave.SpawnInterval;
            SyncDefense(); // Preview survives Preparation -> Wave.
            enemies.Spawn();
            Changed?.Invoke();
            return true;
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
            enemies.ResetRun();
            defense.ResetRun();
            navigation.Reset(); // Always invalidates the old run's plan generation.
            economy.Reset();
            Phase = Stage2RunPhase.Preparation;
            IsPaused = false;
            WaveIndex = 0;
            WorldMinutes = 8d * 60d;
            PreparationRemaining = catalog.PreparationSeconds;
            ThroneHealth = catalog.ThroneHealth;
            remainingToSpawn = 0;
            spawnRemaining = 0f;
            spawningFinished = false;
            SyncDefense();
            Reset?.Invoke();
            Changed?.Invoke();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            enemies.ReachedThrone -= OnReachedThrone;
            enemies.EnemyRemoved -= OnEnemyRemoved;
            defense.SetAttackTarget(null);
        }

        private void OnReachedThrone(int damage)
        {
            if (disposed || Phase != Stage2RunPhase.Wave || damage <= 0) return;
            ThroneHealth = Mathf.Max(0, ThroneHealth - damage);
            if (ThroneHealth == 0) End(Stage2RunPhase.Lost);
            else Changed?.Invoke();
        }

        private void OnEnemyRemoved(Stage2Enemy enemy)
        {
            if (disposed) return;
            if (Phase == Stage2RunPhase.Wave && enemy.WasKilled && economy.GoldPerKill > 0)
                economy.Inventory.TryAddGold(economy.GoldPerKill);
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
            if (ContinuousDays && IsNightTime)
            {
                var morning = Math.Floor(WorldMinutes / 1440d) * 1440d + 8d * 60d;
                WorldMinutes = morning > WorldMinutes ? morning : morning + 1440d;
            }
            WaveIndex++;
            Phase = Stage2RunPhase.Preparation;
            PreparationRemaining = catalog.GetWave(WaveIndex).PreparationSeconds;
            remainingToSpawn = 0;
            spawnRemaining = 0f;
            spawningFinished = false;
            // Recovery preserves the world, health, inventory and renewable-node timers.
            SyncDefense();
            Changed?.Invoke();
        }

        private void End(Stage2RunPhase outcome)
        {
            if (IsTerminal) return;
            Phase = outcome;
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
