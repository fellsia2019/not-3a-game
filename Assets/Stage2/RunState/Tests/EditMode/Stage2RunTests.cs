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
    public sealed class Stage2RunTests
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2Economy economy;
        private Stage2Defense defense;
        private Stage2EnemyGroup enemies;
        private Stage2RunCatalog data;
        private Stage2Run run;

        [SetUp]
        public void SetUp()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            var economyData = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset");
            var defenseData = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
            var enemyData = AssetDatabase.LoadAssetAtPath<Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
            var authoredRun = AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>("Assets/Stage2/RunState/Data/Stage2RunPrototype.asset");
            Assert.That(authoredRun.TryValidate(out var reason), Is.True, reason);
            Assert.That(authoredRun.WaveCount, Is.EqualTo(3));
            for (var wave = 0; wave < authoredRun.WaveCount; wave++)
                Assert.That(authoredRun.GetWave(wave).PreparationSeconds, Is.EqualTo(180f));
            navigation = new Stage2Navigation(map);
            economy = new Stage2Economy(map, economyData);
            defense = new Stage2Defense(map, navigation, economy.Inventory, defenseData,
                () => map.CellToWorld(map.Throne), () => Object.Instantiate(defenseData.TowerPrefab));
            enemies = new Stage2EnemyGroup(map, navigation, enemyData);
            defense.SetEnemyProtection(enemies.IsCellProtected);
            data = ScriptableObject.CreateInstance<Stage2RunCatalog>();
            data.Configure(1f, 2, 0.25f, 2);
            run = new Stage2Run(data, navigation, economy, defense, enemies);
        }

        [TearDown]
        public void TearDown()
        {
            run?.Dispose();
            enemies?.ResetRun();
            defense?.ResetRun();
            if (data != null) Object.DestroyImmediate(data);
        }

        [Test]
        public void PreparationTimerAndEarlyStart_EnterOneWaveOnlyOnce()
        {
            var changes = 0;
            run.Changed += () => changes++;
            run.Tick(0.5f);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.PreparationRemaining, Is.EqualTo(0.5f));
            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.StartWave(), Is.False);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(enemies.ActiveCount, Is.EqualTo(1));
            run.Tick(0.25f);
            Assert.That(run.SpawningFinished, Is.True);
            Assert.That(enemies.ActiveCount, Is.EqualTo(2));
            Assert.That(changes, Is.GreaterThan(0));
        }

        [Test]
        public void LegacyCatalogAndSequence_ValidateEveryWaveAndKeepLegacyValues()
        {
            Assert.That(data.WaveCount, Is.EqualTo(1));
            Assert.That(data.GetWave(0).EnemyCount, Is.EqualTo(2));
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(1f, 1, 0f),
                new Stage2WaveDefinition(float.NaN, 2, 0.5f) }, 2);
            Assert.That(data.TryValidate(out var reason), Is.False);
            Assert.That(reason, Does.Contain("Wave 2"));
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(1f, 1, float.PositiveInfinity) }, 2);
            Assert.That(data.TryValidate(out _), Is.False);
            data.Configure(2f, 3, 0f, 2);
            Assert.That(data.TryValidate(out _), Is.True);
            Assert.That(data.WaveCount, Is.EqualTo(1));
            Assert.That(data.GetWave(0).EnemyCount, Is.EqualTo(3));
        }

        [Test]
        public void WaveClear_PreservesPaidTowerInventoryHealthAndNodeRecovery()
        {
            ConfigureSequence();
            var wood = economy.GetNode(MapResourceKind.Wood);
            for (var i = 0; i < wood.Capacity; i++)
                Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                    economy.WorldPosition(MapResourceKind.Wood), true, wood.GatherSeconds, true), Is.True);
            economy.Inventory.TryAdd(MapResourceKind.Stone, 2);
            economy.Inventory.TryAdd(MapResourceKind.Metal, 2);
            defense.BeginPlacement();
            defense.SetPreviewCell(Vector2Int.zero);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            tower.TakeDamage(1);
            var towerHealth = tower.Health;
            var woodInventory = economy.Inventory.Get(MapResourceKind.Wood);
            var revision = navigation.Revision;
            economy.Tick(1f, true);
            var recovery = wood.RecoveryRemaining;
            run.StartWave();
            var contact = enemies.SpawnAt(map.Throne + Vector2Int.right);
            contact.SetAutomaticTick(false);
            contact.Tick(1f);
            contact.Tick(0.01f);
            ClearEnemies();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.WaveNumber, Is.EqualTo(2));
            Assert.That(run.ThroneHealth, Is.EqualTo(1));
            Assert.That(run.PreparationRemaining, Is.EqualTo(3f));
            Assert.That(run.SpawningFinished, Is.False);
            Assert.That(tower.Health, Is.EqualTo(towerHealth));
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(navigation.Revision, Is.EqualTo(revision));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(woodInventory));
            Assert.That(economy.Inventory.Gold, Is.GreaterThanOrEqualTo(0));
            Assert.That(wood.Remaining, Is.Zero);
            Assert.That(wood.RecoveryRemaining, Is.EqualTo(recovery));
            economy.Tick(recovery, true);
            Assert.That(wood.Remaining, Is.EqualTo(wood.Capacity));
        }

        [Test]
        public void ThreeWaves_PauseEarlyStartSpawnCompletionAndFinalWin()
        {
            ConfigureSequence();
            var terminalFacts = 0;
            run.Changed += () => { if (run.IsTerminal) terminalFacts++; };
            run.Tick(1f);
            ClearEnemies();
            Assert.That(run.WaveNumber, Is.EqualTo(2));
            Assert.That(run.IsTerminal, Is.False);
            run.SetPaused(true);
            run.Tick(100f);
            Assert.That(run.StartWave(), Is.False);
            Assert.That(run.PreparationRemaining, Is.EqualTo(3f));
            run.SetPaused(false);
            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.StartWave(), Is.False);
            ClearEnemies();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave), "The second spawn is pending.");
            run.Tick(0.5f);
            ClearEnemies();
            Assert.That(run.WaveNumber, Is.EqualTo(3));
            Assert.That(run.IsFinalWave, Is.True);
            Assert.That(terminalFacts, Is.Zero);
            run.Tick(4f);
            ClearEnemies();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Won));
            Assert.That(terminalFacts, Is.EqualTo(1));
            run.Tick(100f);
            Assert.That(terminalFacts, Is.EqualTo(1));
            run.ResetRun();
            Assert.That(run.WaveNumber, Is.EqualTo(1));
            Assert.That(run.PreparationRemaining, Is.EqualTo(1f));
            Assert.That(run.ThroneHealth, Is.EqualTo(2));
            Assert.That(run.SpawningFinished, Is.False);
        }

        [Test]
        public void LethalContactOnFinalWave_LosesInsteadOfWinning()
        {
            ConfigureSequence();
            run.StartWave();
            ClearEnemies();
            run.StartWave();
            run.Tick(0.5f);
            ClearEnemies();
            run.StartWave();
            // The current one enemy cannot clear the wave before the contact resolves.
            var contact = enemies.SpawnAt(map.Throne + Vector2Int.right);
            contact.SetAutomaticTick(false);
            var next = enemies.SpawnAt(map.Throne + Vector2Int.right);
            next.SetAutomaticTick(false);
            enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            contact.Tick(1f);
            contact.Tick(0.01f);
            next.Tick(1f);
            next.Tick(0.01f);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(run.WaveNumber, Is.EqualTo(3));
            Assert.That(enemies.ActiveCount, Is.Zero);
            Assert.That(run.StartWave(), Is.False);
        }

        private void ConfigureSequence()
        {
            run.Dispose();
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(1f, 1, 0f),
                new Stage2WaveDefinition(3f, 2, 0.5f), new Stage2WaveDefinition(4f, 1, 0f) }, 2);
            run = new Stage2Run(data, navigation, economy, defense, enemies);
        }

        private void ClearEnemies()
        {
            while (enemies.ActiveCount > 0) enemies.GetEnemy(0).TakeDamage(int.MaxValue);
        }

        [Test]
        public void AutomaticStartAndLastEnemyDeath_WinExactlyOnceAfterSpawningFinishes()
        {
            var terminalFacts = 0;
            run.Changed += () => { if (run.IsTerminal) terminalFacts++; };
            run.Tick(1f);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            var first = enemies.GetEnemy(0);
            first.TakeDamage(first.Health);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave), "A later spawn is still pending.");
            run.Tick(0.25f);
            var second = enemies.GetEnemy(0);
            second.TakeDamage(second.Health);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Won));
            Assert.That(terminalFacts, Is.EqualTo(1));
            Assert.That(run.StartWave(), Is.False);
            run.Tick(10f);
            Assert.That(terminalFacts, Is.EqualTo(1));
            Assert.That(run.ThroneHealth, Is.EqualTo(run.ThroneMaximum));
        }

        [Test]
        public void LastThroneHit_LossWinsOverClearAndClearsAllActors()
        {
            run.StartWave();
            var contact = enemies.SpawnAt(new Vector2Int(map.Throne.x + 1, map.Throne.y));
            contact.SetAutomaticTick(false);
            contact.Tick(1f);
            contact.Tick(0.01f);
            Assert.That(run.ThroneHealth, Is.EqualTo(1));
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            var second = enemies.SpawnAt(new Vector2Int(map.Throne.x + 1, map.Throne.y));
            second.SetAutomaticTick(false);
            second.Tick(1f);
            second.Tick(0.01f);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(run.ThroneHealth, Is.Zero);
            Assert.That(enemies.ActiveCount, Is.Zero);
            Assert.That(run.StartWave(), Is.False);
            Assert.That(defense.BeginPlacement(), Is.False);
        }

        [Test]
        public void PauseAndRestart_DiscardPreviewAndInvalidatePriorRunState()
        {
            var cell = new Vector2Int(0, 0);
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                economy.Inventory.TryAdd(kind, 2);
            defense.BeginPlacement();
            defense.SetPreviewCell(cell);
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.Ready));
            var oldPlan = navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(new Vector2Int(1, 0));
            run.SetPaused(true);
            Assert.That(defense.IsPlacing, Is.False);
            var remaining = run.PreparationRemaining;
            run.Tick(100f);
            Assert.That(run.PreparationRemaining, Is.EqualTo(remaining));
            Assert.That(run.StartWave(), Is.False);
            run.SetPaused(false);
            Assert.That(defense.IsPlacing, Is.False);
            run.StartWave();
            Assert.That(enemies.ActiveCount, Is.EqualTo(1));
            run.ResetRun();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.IsPaused, Is.False);
            Assert.That(run.PreparationRemaining, Is.EqualTo(data.PreparationSeconds));
            Assert.That(run.ThroneHealth, Is.EqualTo(data.ThroneHealth));
            Assert.That(enemies.ActiveCount, Is.Zero);
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(navigation.OccupiedCount, Is.Zero);
            Assert.That(navigation.IsCurrent(oldPlan), Is.False);
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                Assert.That(economy.Inventory.Get(kind), Is.Zero);
                Assert.That(economy.GetNode(kind).Remaining, Is.EqualTo(economy.GetNode(kind).Capacity));
            }
            Assert.That(defense.TryConfirm(out _), Is.EqualTo(PlacementResult.NotPlacing));
            Assert.That(economy.Inventory.Gold, Is.EqualTo(5));
        }

        [Test]
        public void KillCreditsOneGold_ThroneContactAndRestartDoNot()
        {
            Assert.That(economy.Inventory.Gold, Is.EqualTo(5));
            run.StartWave();
            var killed = enemies.GetEnemy(0);
            killed.TakeDamage(killed.Health);
            Assert.That(economy.Inventory.Gold, Is.EqualTo(6));
            var contact = enemies.SpawnAt(map.Throne + Vector2Int.right);
            contact.SetAutomaticTick(false);
            contact.Tick(1f);
            contact.Tick(0.01f);
            Assert.That(economy.Inventory.Gold, Is.EqualTo(6));
            run.ResetRun();
            Assert.That(economy.Inventory.Gold, Is.EqualTo(5));
        }

        [Test]
        public void ContinuousDays_KeepWorldAndAdvanceBeyondAuthoredSequenceWithGrowingNights()
        {
            run.Dispose();
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(1200f, 2, .1f),
                new Stage2WaveDefinition(1200f, 3, .1f), new Stage2WaveDefinition(1200f, 4, .1f) }, 2);
            data.ConfigureContinuousDays(true, 2);
            run = new Stage2Run(data, navigation, economy, defense, enemies);
            economy.Inventory.TryAdd(MapResourceKind.Wood, 7);
            var expectedCounts = new[] { 2, 3, 4, 6, 8, 10 };
            for (var day = 0; day < expectedCounts.Length; day++)
            {
                Assert.That(run.IsDay, Is.True);
                Assert.That(run.PreparationRemaining, Is.EqualTo(1200f));
                Assert.That(run.PreparationDuration, Is.EqualTo(1200f));
                Assert.That(run.WaveEnemyCount, Is.EqualTo(expectedCounts[day]));
                Assert.That(run.IsFinalWave, Is.False);
                run.SetPaused(true); run.Tick(1200f);
                Assert.That(run.PreparationRemaining, Is.EqualTo(1200f));
                run.SetPaused(false); run.Tick(1199f);
                Assert.That(run.IsDay, Is.True);
                run.Tick(1f);
                Assert.That(run.IsNight, Is.True);
                Assert.That(run.StartWave(), Is.False, "A night cannot be skipped or relaunched.");
                run.Tick(10f);
                Assert.That(run.SpawningFinished, Is.True);
                Assert.That(enemies.ActiveCount, Is.EqualTo(expectedCounts[day]));
                for (var i = enemies.ActiveCount - 1; i >= 0; i--)
                { var enemy = enemies.GetEnemy(i); enemy.TakeDamage(enemy.Health); }
                Assert.That(run.WaveNumber, Is.EqualTo(day + 2));
                Assert.That(run.IsTerminal, Is.False);
                Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.EqualTo(7));
            }
            run.ResetRun();
            Assert.That(run.IsDay, Is.True);
            Assert.That(run.WaveNumber, Is.EqualTo(1));
            Assert.That(run.PreparationRemaining, Is.EqualTo(1200f));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
        }

        [Test]
        public void Clock_DaySkipAndEarlyVictoryAdvanceToNightAndNextMorningWithoutIncome()
        {
            ConfigureClockRun();
            Assert.That(run.ClockHour, Is.EqualTo(8));
            Assert.That(run.CalendarDay, Is.EqualTo(1));
            run.Tick(600f);
            Assert.That(run.ClockHour, Is.EqualTo(14));
            var clock = run.WorldMinutes;
            run.SetPaused(true); run.Tick(600f);
            Assert.That(run.WorldMinutes, Is.EqualTo(clock));
            Assert.That(run.StartWave(), Is.False);
            run.SetPaused(false);
            var gold = economy.Inventory.Gold;
            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.ClockHour, Is.EqualTo(20));
            Assert.That(economy.Inventory.Gold, Is.EqualTo(gold));
            run.Tick(10f);
            for (var i = enemies.ActiveCount - 1; i >= 0; i--)
            { var enemy = enemies.GetEnemy(i); enemy.TakeDamage(enemy.Health); }
            Assert.That(run.IsDay, Is.True);
            Assert.That(run.ClockHour, Is.EqualTo(8));
            Assert.That(run.CalendarDay, Is.EqualTo(2));
            Assert.That(run.PreparationRemaining, Is.EqualTo(1200f));
            run.ResetRun();
            Assert.That(run.ClockHour, Is.EqualTo(8));
            Assert.That(run.CalendarDay, Is.EqualTo(1));
        }

        [Test]
        public void Clock_OverrunningHordeContinuesPastDawnWithoutOverlapOrBackwardTime()
        {
            ConfigureClockRun();
            Assert.That(run.StartWave(), Is.True);
            run.Tick(1200f);
            Assert.That(run.ClockHour, Is.EqualTo(8));
            Assert.That(run.CalendarDay, Is.EqualTo(2));
            Assert.That(run.IsNightTime, Is.False);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(enemies.ActiveCount, Is.EqualTo(2));
            Assert.That(run.StartWave(), Is.False);
            run.Tick(300f);
            Assert.That(run.ClockHour, Is.EqualTo(11));
            var clock = run.WorldMinutes;
            for (var i = enemies.ActiveCount - 1; i >= 0; i--)
            { var enemy = enemies.GetEnemy(i); enemy.TakeDamage(enemy.Health); }
            Assert.That(run.WorldMinutes, Is.EqualTo(clock));
            Assert.That(run.PreparationRemaining, Is.EqualTo(1200f));
            run.Tick(1199f);
            Assert.That(run.IsDay, Is.True);
            Assert.That(enemies.ActiveCount, Is.Zero);
            run.Tick(1f);
            Assert.That(run.IsNight, Is.True);
            Assert.That(run.IsNightTime, Is.True);
            Assert.That(run.ClockHour, Is.EqualTo(23));
        }

        private void ConfigureClockRun()
        {
            run.Dispose();
            data.ConfigureWaves(new[] { new Stage2WaveDefinition(1200f, 2, .1f) }, 2);
            data.ConfigureContinuousDays(true, 1);
            run = new Stage2Run(data, navigation, economy, defense, enemies);
        }

        [Test]
        public void ContinuousGrowth_ValidatesAndDoesNotOverflowOrChangeFiniteLegacy()
        {
            data.ConfigureContinuousDays(true, 0);
            Assert.That(data.TryValidate(out _), Is.False);
            data.ConfigureContinuousDays(true, int.MaxValue);
            Assert.That(data.TryValidate(out _), Is.True);
            Assert.That(data.GetWave(100).EnemyCount, Is.EqualTo(int.MaxValue));
            data.ConfigureContinuousDays(false);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => data.GetWave(1));
        }
    }
}
