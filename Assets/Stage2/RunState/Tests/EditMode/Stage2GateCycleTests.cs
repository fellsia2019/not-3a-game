using System.Linq;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.RunState.Tests
{
    public sealed class Stage2GateCycleTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2Economy economy;
        private Stage2Defense defense;
        private Stage2EnemyGroup enemies;
        private Stage2RunCatalog data;
        private Stage2Run run;
        [SetUp] public void Setup()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            navigation = new Stage2Navigation(map);
            economy = new Stage2Economy(map, AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset"));
            var towers = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            defense = new Stage2Defense(map, navigation, economy.Inventory, towers, () => map.CellToWorld(map.Throne), () => Object.Instantiate(towers.TowerPrefab));
            enemies = new Stage2EnemyGroup(map, navigation, AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset"));
            defense.SetEnemyProtection(enemies.IsCellProtected);
            data = ScriptableObject.CreateInstance<Stage2RunCatalog>();
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(12, 2, .2f), new Stage2WaveDefinition(12, 3, .15f) }, 3);
            data.ConfigureContinuousDays(true); data.ConfigureDaytimeGates(true, 30, .5f, 3);
            run = new Stage2Run(data, navigation, economy, defense, enemies);
        }
        [TearDown] public void Cleanup()
        { run.Dispose(); enemies.ResetRun(); defense.ResetRun(); Object.DestroyImmediate(data); }
        private void KillAll()
        { while (enemies.ActiveCount > 0) enemies.GetEnemy(0).TakeDamage(int.MaxValue); }
        private void FirstClearThenDawn()
        {
            run.Tick(12); run.Tick(.3f); KillAll();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            var dawn = (System.Math.Floor(run.WorldMinutes / 1440) + 1) * 1440 + 480;
            run.Tick((float)((dawn - run.WorldMinutes) / 60) + .001f);
            Assert.That(run.ClockHour, Is.EqualTo(8)); Assert.That(run.LastClearedTier, Is.EqualTo(1));
        }
        [Test] public void NoEarlyStartOrEarlyDawnAndPauseFreezesClock()
        {
            Assert.That(run.StartWave(), Is.False); Assert.That(run.OpenDaytimeGate(), Is.False);
            run.SetPaused(true); run.Tick(12); Assert.That(run.ClockHour, Is.EqualTo(8));
            run.SetPaused(false); run.Tick(12); run.Tick(.3f);
            var time = run.WorldMinutes; KillAll();
            Assert.That(run.WorldMinutes, Is.EqualTo(time)); Assert.That(run.IsDay, Is.False);
            Assert.That(run.PreparationRemaining, Is.GreaterThan(12)); Assert.That(run.StartWave(), Is.False);
        }
        [Test] public void GateToggleKeepsCadenceWithoutRemovingOrRewardingLiveEnemies()
        {
            FirstClearThenDawn(); Assert.That(run.OpenDaytimeGate(), Is.True); run.Tick(.01f);
            var first = enemies.GetEnemy(0); first.SetAutomaticTick(false); first.Tick(.01f); first.TakeDamage(1);
            var position = first.transform.position; var hp = first.Health; var gold = economy.Inventory.Gold;
            Assert.That(run.CloseDaytimeGate(), Is.True); run.Tick(1);
            Assert.That(first.transform.position, Is.EqualTo(position)); Assert.That(first.Health, Is.EqualTo(hp));
            Assert.That(enemies.ActiveCount, Is.EqualTo(1)); Assert.That(economy.Inventory.Gold, Is.EqualTo(gold));
            Assert.That(run.OpenDaytimeGate(), Is.True); run.Tick(.01f); Assert.That(enemies.ActiveCount, Is.EqualTo(1));
            run.Tick(.6f); Assert.That(enemies.ActiveCount, Is.EqualTo(4), "Flow must pass both the night count and the obsolete live cap.");
            first.TakeDamage(int.MaxValue); run.Tick(.2f); Assert.That(enemies.ActiveCount, Is.EqualTo(4));
            enemies.GetEnemy(0).TakeDamage(int.MaxValue); run.Tick(.01f); Assert.That(enemies.ActiveCount, Is.EqualTo(3));
            Assert.That(economy.Inventory.Gold, Is.EqualTo(gold + 2 * economy.GoldPerKill));
            Assert.That(Enumerable.Range(0, enemies.ActiveCount).All(i => enemies.GetEnemy(i).Source == Stage2EnemySource.Daytime && enemies.GetEnemy(i).ThreatLevel == 1), Is.True);
        }
        [Test] public void ExplicitTestLaunchStartsFirstNightOnceWithoutSkippedIncomeAndOrdinaryStartStaysLocked()
        {
            var gold=economy.Inventory.Gold;var wood=economy.Inventory.Get(MapResourceKind.Wood);var boundaries=0;
            run.BeforeWaveStarted+=()=>boundaries++;
            Assert.That(run.StartWave(),Is.False);Assert.That(run.ClockHour,Is.EqualTo(8));
            run.SetPaused(true);Assert.That(run.StartNextWaveForTesting(),Is.False);Assert.That(run.ClockHour,Is.EqualTo(8));run.SetPaused(false);
            Assert.That(run.StartNextWaveForTesting(),Is.True);Assert.That(run.ClockHour,Is.EqualTo(20));
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Wave));Assert.That(run.WaveNumber,Is.EqualTo(1));
            Assert.That(enemies.ActiveCount,Is.EqualTo(1));Assert.That(boundaries,Is.EqualTo(1));
            Assert.That(economy.Inventory.Gold,Is.EqualTo(gold));Assert.That(economy.Inventory.Get(MapResourceKind.Wood),Is.EqualTo(wood));
            Assert.That(run.StartNextWaveForTesting(),Is.False);Assert.That(enemies.ActiveCount,Is.EqualTo(1));
        }
        [Test] public void ExplicitTestLaunchClosesGateAndRetainsDaytimeEnemiesWhileStartingNextTier()
        {
            FirstClearThenDawn();run.OpenDaytimeGate();run.Tick(.01f);var old=enemies.GetEnemy(0);var gold=economy.Inventory.Gold;
            Assert.That(run.StartNextWaveForTesting(),Is.True);Assert.That(run.DaytimeGateOpen,Is.False);
            Assert.That(old.IsAlive,Is.True);Assert.That(run.DaytimeEnemies,Is.EqualTo(1));Assert.That(enemies.ActiveCount,Is.EqualTo(2));
            Assert.That(run.WaveNumber,Is.EqualTo(2));Assert.That(run.ClockHour,Is.EqualTo(20));Assert.That(economy.Inventory.Gold,Is.EqualTo(gold));
        }
        [Test] public void CutoffRetainsDayCreepsAndMixedNightCompletesOnlyAfterAllThreatsResolve()
        {
            FirstClearThenDawn(); run.OpenDaytimeGate(); run.Tick(.01f); var old = enemies.GetEnemy(0);
            run.Tick(run.PreparationRemaining - .5f + .0001f);
            Assert.That(run.DaytimeGateOpen, Is.False); Assert.That(run.OpenDaytimeGate(), Is.False);
            Assert.That(old.IsAlive, Is.True); var daytimeCount = run.DaytimeEnemies;
            run.Tick(.5f); Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave)); run.Tick(.5f);
            var night = Enumerable.Range(0, enemies.ActiveCount).Select(enemies.GetEnemy).First(e => e.Source == Stage2EnemySource.Night);
            Assert.That(night.ThreatLevel, Is.EqualTo(2)); Assert.That(night.MaximumHealth, Is.GreaterThan(old.MaximumHealth));
            for (var i = enemies.ActiveCount - 1; i >= 0; i--) if (enemies.GetEnemy(i).Source == Stage2EnemySource.Night) enemies.GetEnemy(i).TakeDamage(int.MaxValue);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave)); Assert.That(run.DaytimeEnemies, Is.EqualTo(daytimeCount));
            KillAll(); Assert.That(run.LastClearedTier, Is.EqualTo(2)); Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
        }
        [Test] public void DayDefeatKeepsResourcesCurrentTowerAndDamageButClearsThreatsAndReturnsMorning()
        {
            FirstClearThenDawn(); run.OpenDaytimeGate(); run.Tick(.01f);
            foreach (MapResourceKind kind in System.Enum.GetValues(typeof(MapResourceKind))) economy.Inventory.TryAdd(kind, 50);
            defense.BeginPlacement(); defense.SetPreviewCell(Vector2Int.zero);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready)); tower.TakeDamage(1);
            enemies.GetEnemy(0).TakeDamage(int.MaxValue); run.Tick(.5f);
            enemies.TryHitNearest(map.CellToWorld(map.EnemyEntrance), 50, 1, 0, 100);
            var wood = economy.Inventory.Get(MapResourceKind.Wood); var gold = economy.Inventory.Gold; var hp = tower.Health;
            for (var i = 0; i < 3; i++) { var enemy = enemies.SpawnAt(map.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost)); Assert.That(run.CanRetry, Is.True);
            Assert.That(run.RetryFailedWave(), Is.True); Assert.That(run.ClockHour, Is.EqualTo(8));
            Assert.That(run.WaveNumber, Is.EqualTo(2)); Assert.That(run.LastClearedTier, Is.EqualTo(1));
            Assert.That(run.ThroneHealth, Is.EqualTo(run.ThroneMaximum)); Assert.That(run.PreparationRemaining, Is.EqualTo(12));
            Assert.That(defense.GetTower(0), Is.SameAs(tower)); Assert.That(tower.Health, Is.EqualTo(hp));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(wood)); Assert.That(economy.Inventory.Gold, Is.EqualTo(gold));
            Assert.That(enemies.ActiveCount, Is.Zero); Assert.That(enemies.PendingShotCount, Is.Zero); Assert.That(run.DaytimeGateOpen, Is.False);
            run.OpenDaytimeGate(); run.Tick(.01f); enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            Assert.That(economy.Inventory.Gold, Is.EqualTo(gold + economy.GoldPerKill));
        }
        [Test] public void LargeClockStepConservesTimeAndDoesNotQueueOverlappingNights()
        {
            run.Tick(36); Assert.That(run.WorldMinutes, Is.EqualTo(2640).Within(.0001));
            Assert.That(run.WaveNumber, Is.EqualTo(1)); Assert.That(enemies.ActiveCount, Is.EqualTo(2));
            KillAll(); var time = run.WorldMinutes; Assert.That(run.WaveNumber, Is.EqualTo(2));
            Assert.That(run.WorldMinutes, Is.EqualTo(time)); Assert.That(run.PreparationRemaining, Is.EqualTo(24));
        }
        [Test] public void ResourcePopulationRefreshCannotInvalidateATemporalRetryAnchor()
        {
            run.Tick(12);
            economy.ReplaceAdditionalNodes(System.Array.Empty<Stage2ResourceNode>());
            for (var i = 0; i < 3; i++)
            { var enemy = enemies.SpawnAt(map.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(run.CanRetry, Is.True);
            Assert.That(run.RetryFailedWave(), Is.True);
            Assert.That(run.ClockHour, Is.EqualTo(8)); Assert.That(run.WaveNumber, Is.EqualTo(1));
        }
        [TestCase(2)] [TestCase(3)]
        public void RetryDayGateRepeatsLastClearedNightDifficultyWithoutAnEnemyQuota(int failedTier)
        {
            var authored=AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>("Assets/Stage3/CombatSlice/HordeRun.asset");
            data.ConfigureWaves(Enumerable.Range(0,authored.WaveCount).Select(authored.GetWave).ToArray(),3);
            var counts=new int[4];var clearedHealth=0;
            for(var cleared=1;cleared<failedTier;cleared++)
            {
                Assert.That(run.StartNextWaveForTesting(),Is.True);clearedHealth=enemies.GetEnemy(0).MaximumHealth;
                var wave=data.GetWave(cleared-1);
                for(var i=0;i<counts.Length;i++){run.Tick(wave.SpawnInterval);counts[i]=enemies.ActiveCount;}
                run.Tick(wave.EnemyCount*wave.SpawnInterval+.01f);KillAll();
            }
            Assert.That(run.StartNextWaveForTesting(),Is.True);
            var morningDay=run.CalendarDay;
            for(var i=0;i<3;i++)
            {var enemy=enemies.SpawnAt(map.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Lost));Assert.That(run.RetryFailedWave(),Is.True);
            Assert.That(run.CalendarDay,Is.EqualTo(morningDay));Assert.That(run.ClockHour,Is.EqualTo(8));
            Assert.That(run.LastClearedTier,Is.EqualTo(failedTier-1));Assert.That(run.WaveNumber,Is.EqualTo(failedTier));
            Assert.That(run.OpenDaytimeGate(),Is.True);run.Tick(.001f);
            var replay=enemies.GetEnemy(0);var profile=data.GetWave(failedTier-2);
            Assert.That(replay.ThreatLevel,Is.EqualTo(failedTier-1),"Replay only the last WON night, never the failed next tier.");
            Assert.That(replay.MaximumHealth,Is.EqualTo(clearedHealth));
            for(var i=0;i<counts.Length;i++)
            {run.Tick(profile.SpawnInterval);Assert.That(enemies.ActiveCount,Is.EqualTo(counts[i]),"Use the cleared night's cadence.");}
            run.Tick(profile.EnemyCount*profile.SpawnInterval*2);
            Assert.That(enemies.ActiveCount,Is.GreaterThan(profile.EnemyCount*2));
            Assert.That(enemies.ActiveCount,Is.GreaterThan(24),"Neither night quota nor old live cap may stop the flow.");
            Assert.That(run.DaytimeGateOpen,Is.True);var alive=enemies.ActiveCount;
            Assert.That(run.CloseDaytimeGate(),Is.True);run.Tick(profile.SpawnInterval*2);
            Assert.That(enemies.ActiveCount,Is.EqualTo(alive));KillAll();
            Assert.That(run.LastClearedTier,Is.EqualTo(failedTier-1));Assert.That(run.WaveNumber,Is.EqualTo(failedTier));
        }
        [Test] public void FirstNightFailureDoesNotUnlockUnclearedDaytimeFarm()
        {
            Assert.That(run.OpenDaytimeGate(),Is.False);Assert.That(run.StartNextWaveForTesting(),Is.True);
            for(var i=0;i<3;i++)
            {var enemy=enemies.SpawnAt(map.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(run.RetryFailedWave(),Is.True);Assert.That(run.LastClearedTier,Is.Zero);
            Assert.That(run.OpenDaytimeGate(),Is.False);run.ResetRun();Assert.That(run.OpenDaytimeGate(),Is.False);
        }
        [Test] public void UnboundedFlowRequiresPositiveCadenceWhileFiniteLegacyWavesCanBeInstant()
        {
            data.ConfigureWaves(new[]{new Stage2WaveDefinition(12,2,0)},3);data.ConfigureDaytimeGates(false);
            Assert.That(data.TryValidate(out _),Is.True);data.ConfigureDaytimeGates(true);
            Assert.That(data.TryValidate(out var reason),Is.False);Assert.That(reason,Does.Contain("positive"));
        }
        [Test] public void SleepEndsTenMinutesBeforeNightAndRefusesOpenGateLiveEnemiesAndPendingAttacks()
        {
            FirstClearThenDawn(); run.OpenDaytimeGate(); Assert.That(run.SleepUntilBeforeNight(), Is.False);
            run.Tick(.01f); run.CloseDaytimeGate(); Assert.That(run.SleepUntilBeforeNight(), Is.False);
            enemies.TryHitNearest(map.CellToWorld(map.EnemyEntrance), 50, 1, 0, 100);
            KillAll(); Assert.That(run.SleepUntilBeforeNight(), Is.False);
            enemies.TickShots(100); var gold = economy.Inventory.Gold;
            Assert.That(run.SleepUntilBeforeNight(), Is.True);
            Assert.That(run.ClockHour, Is.EqualTo(19)); Assert.That(run.ClockMinute, Is.EqualTo(50));
            Assert.That(economy.Inventory.Gold, Is.EqualTo(gold)); Assert.That(run.ActiveEnemies, Is.Zero);
            Assert.That(run.OpenDaytimeGate(), Is.False); Assert.That(run.SleepUntilBeforeNight(), Is.False);
            run.Tick(run.PreparationRemaining); Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(run.SleepUntilBeforeNight(), Is.False);
        }

        private void TimedClock()
        {
            data.ConfigureWaves(new[]{new Stage2WaveDefinition(600,2,20),new Stage2WaveDefinition(600,3,10)},3);
            data.ConfigureClockDurations(600,180);run.ResetRun();
        }
        [Test] public void TenMinuteDayThreeMinuteNightKeepsSpawningPastQuotaAndOnlyEndsAtDawn()
        {
            TimedClock();Assert.That(run.PreparationRemaining,Is.EqualTo(600));run.Tick(599);
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Preparation));run.Tick(1);
            Assert.That(run.ClockHour,Is.EqualTo(20));Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Wave));
            run.Tick(60);Assert.That(enemies.ActiveCount,Is.EqualTo(4));KillAll();
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Wave));Assert.That(run.SpawningFinished,Is.False);
            run.Tick(120);Assert.That(run.ClockHour,Is.EqualTo(8));Assert.That(run.ClockMinute,Is.Zero);
            Assert.That(run.SpawningFinished,Is.True);Assert.That(enemies.ActiveCount,Is.EqualTo(5));
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Wave),"Dawn retains living threats.");
            run.Tick(20);Assert.That(enemies.ActiveCount,Is.EqualTo(5),"No new spawn after dawn.");KillAll();
            Assert.That(run.LastClearedTier,Is.EqualTo(1));Assert.That(run.IsDay,Is.True);
        }
        [Test] public void TimedNightPauseAndRetryKeepTheSamePendingTierAndFullDay()
        {
            TimedClock();run.StartNextWaveForTesting();run.Tick(10);var time=run.WorldMinutes;
            run.SetPaused(true);run.Tick(180);Assert.That(run.WorldMinutes,Is.EqualTo(time));run.SetPaused(false);
            for(var i=0;i<3;i++){var enemy=enemies.SpawnAt(map.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(run.RetryFailedWave(),Is.True);Assert.That(run.ClockHour,Is.EqualTo(8));
            Assert.That(run.PreparationRemaining,Is.EqualTo(600));Assert.That(run.WaveNumber,Is.EqualTo(1));
            Assert.That(run.OpenDaytimeGate(),Is.False);run.StartNextWaveForTesting();run.Tick(180);
            Assert.That(enemies.ActiveCount,Is.EqualTo(9));Assert.That(run.SpawningFinished,Is.True);
        }
        [Test]public void AnalogCycleUsesTenToThreeProportionsAndFreezesResetsAndSleepsWithClock()
        {
            TimedClock();Assert.That(run.DayNightCycleProgress,Is.Zero);
            run.Tick(300);Assert.That(run.DayNightCycleProgress,Is.EqualTo(300f/780f).Within(.00001));
            run.SetPaused(true);run.Tick(300);Assert.That(run.DayNightCycleProgress,Is.EqualTo(300f/780f).Within(.00001));
            run.SetPaused(false);Assert.That(run.SleepUntilBeforeNight(),Is.True);
            Assert.That(run.DayNightCycleProgress,Is.EqualTo((600f-600f*10/720)/780).Within(.00001));
            run.Tick(run.PreparationRemaining+.001f);Assert.That(run.DayNightCycleProgress,Is.EqualTo(600f/780f).Within(.00001));
            run.Tick(90);Assert.That(run.DayNightCycleProgress,Is.EqualTo(690f/780f).Within(.00001));
            run.Tick(90);Assert.That(run.DayNightCycleProgress,Is.EqualTo(0).Within(.00001));
            Assert.That(run.ActiveEnemies,Is.GreaterThan(0),"Dial returns to dawn without erasing live creeps.");
            run.ResetRun();Assert.That(run.DayNightCycleProgress,Is.Zero);
        }
        [Test] public void LargeTimedClockStepUsesBothDurationsAndDoesNotQueueOverlappingNights()
        {
            TimedClock();run.Tick(1560);Assert.That(run.WorldMinutes,Is.EqualTo(3360).Within(.0001));
            Assert.That(run.WaveNumber,Is.EqualTo(1));Assert.That(enemies.ActiveCount,Is.EqualTo(9));
            Assert.That(run.SpawningFinished,Is.True);KillAll();Assert.That(run.LastClearedTier,Is.EqualTo(1));
            Assert.That(run.PreparationRemaining,Is.EqualTo(600).Within(.001));
        }
        [Test] public void TimedDayCutoffIs1930AndRetainsFarmEnemiesBeforeTheStreamingNight()
        {
            TimedClock();run.StartNextWaveForTesting();run.Tick(180);KillAll();
            Assert.That(run.OpenDaytimeGate(),Is.True);run.Tick(575);
            Assert.That(run.ClockHour,Is.EqualTo(19));Assert.That(run.ClockMinute,Is.EqualTo(30));
            Assert.That(run.DaytimeGateOpen,Is.False);var alive=enemies.ActiveCount;
            Assert.That(alive,Is.GreaterThan(2));Assert.That(run.OpenDaytimeGate(),Is.False);
            run.Tick(25);Assert.That(run.ClockHour,Is.EqualTo(20));Assert.That(run.WaveNumber,Is.EqualTo(2));
            Assert.That(enemies.ActiveCount,Is.EqualTo(alive+1));Assert.That(run.SpawningFinished,Is.False);
        }
    }
}
