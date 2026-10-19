#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroProgressionRulesTests
    {
        private Stage2HeroProgressionCatalog catalog;
        private Stage2HeroProgression rules;
        [SetUp] public void Setup()
        { catalog = Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression"); rules = new Stage2HeroProgression(catalog); }

        [Test] public void MultiLevelAwardsCarryXpSpendPointsConsecutivelyAndStopAt100()
        {
            Assert.That(catalog.MaxLevel, Is.EqualTo(100)); Assert.That(rules.BlinkRank, Is.EqualTo(1));
            Assert.That(rules.TryUpgradeBlink(), Is.False);
            var celebrations = 0; var lastLevel = 0;
            rules.LevelRaised += level => { celebrations++; lastLevel = level; };
            rules.AwardExperience(catalog.RequiredXp(1) + catalog.RequiredXp(2) + 17);
            Assert.That(rules.Level, Is.EqualTo(3)); Assert.That(rules.XpWithin, Is.EqualTo(17));
            Assert.That(rules.AvailablePoints, Is.EqualTo(2)); Assert.That(celebrations, Is.EqualTo(1));
            Assert.That(lastLevel, Is.EqualTo(3));
            Assert.That(rules.TryUpgradeBlink(), Is.True); Assert.That(rules.BlinkRank, Is.EqualTo(2));
            Assert.That(rules.TryUpgradePassive(Stage2HeroPassive.PreciseStrike), Is.True);
            Assert.That(rules.TryUpgradeBlink(), Is.False); Assert.That(rules.AvailablePoints, Is.Zero);
            rules.AwardExperience(int.MaxValue);
            Assert.That(rules.Level, Is.EqualTo(100)); Assert.That(rules.RequiredXp, Is.Zero); Assert.That(rules.XpWithin, Is.Zero);
            Assert.That(rules.AvailablePoints, Is.EqualTo(97));
            var points = rules.AvailablePoints; rules.AwardExperience(int.MaxValue);
            Assert.That(rules.AvailablePoints, Is.EqualTo(points)); Assert.That(celebrations, Is.EqualTo(2));
        }

        [Test] public void RankCapsAndPersonalBonusesAreEffectiveAndSnapshotDoesNotAliasRanks()
        {
            rules.AwardExperience(int.MaxValue);
            foreach (Stage2HeroPassive passive in Enum.GetValues(typeof(Stage2HeroPassive)))
            {
                for (var i = 0; i < catalog.PassiveMaxRank; i++) Assert.That(rules.TryUpgradePassive(passive), Is.True);
                var points = rules.AvailablePoints;
                Assert.That(rules.TryUpgradePassive(passive), Is.False); Assert.That(rules.AvailablePoints, Is.EqualTo(points));
            }
            Assert.That(rules.DamageBonus, Is.EqualTo(catalog.DamagePerRank * catalog.PassiveMaxRank));
            Assert.That(rules.AttackSpeedMultiplier, Is.GreaterThan(1)); Assert.That(rules.MovementSpeedMultiplier, Is.GreaterThan(1));
            while (rules.TryUpgradeBlink()) { }
            Assert.That(rules.BlinkRank, Is.EqualTo(4));
            var snapshot = rules.CaptureSnapshot(); rules.Reset();
            Assert.That(rules.DamageBonus, Is.Zero); rules.RestoreSnapshot(snapshot);
            Assert.That(rules.PassiveRank(Stage2HeroPassive.PreciseStrike), Is.EqualTo(catalog.PassiveMaxRank));
        }

        [Test] public void BlinkClampsAtRankReachRejectsBlockedOrInvalidLandingAndFreezesCooldownOnPause()
        {
            var origin = new Vector2(20, 20); var moved = origin;
            Assert.That(rules.TryBlink(origin, origin + Vector2.right * 100, p => { moved = p; return true; }, out var landing), Is.True);
            Assert.That(Vector2.Distance(origin, landing), Is.EqualTo(catalog.BlinkWorldRange(1)).Within(.001));
            Assert.That(moved, Is.EqualTo(landing)); Assert.That(rules.TryBlink(origin, origin + Vector2.up, _ => true, out _), Is.False);
            rules.Tick(20, false); Assert.That(rules.BlinkCooldownRemaining, Is.EqualTo(catalog.BlinkCooldown));
            var snapshot = rules.CaptureSnapshot(); rules.Tick(20, true);
            Assert.That(rules.TryBlink(origin, origin + Vector2.up, _ => false, out _), Is.False);
            Assert.That(rules.BlinkCooldownRemaining, Is.Zero);
            Assert.That(rules.TryBlink(origin, new Vector2(float.NaN, 0), _ => true, out _), Is.False);
            Assert.That(rules.TryBlink(origin, origin, _ => true, out _), Is.False);
            rules.RestoreSnapshot(snapshot); Assert.That(rules.BlinkCooldownRemaining, Is.EqualTo(catalog.BlinkCooldown));
        }

        [Test] public void PersonalCombatBonusesChangeDamageAndCadenceWithoutAddingSplash()
        {
            var combatCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<Stage2HeroCombatCatalog>("Assets/Stage3/CombatSlice/HeroBow.asset");
            var damage = 0; var splash = -1f;
            var combat = new Stage2HeroCombat(combatCatalog, (p, r, d, s, t) => { damage = d; splash = s; return true; });
            combat.SetPersonalBonuses(1, 1.3f); combat.Tick(.01f, Vector2.zero, true);
            Assert.That(damage, Is.EqualTo(combatCatalog.AttackDamage + 1)); Assert.That(splash, Is.Zero);
            Assert.That(combat.CooldownRemaining, Is.LessThan(combatCatalog.FireInterval));
        }
    }

    public sealed class Stage3HeroProgressionTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2HeroProgressionDriver driver;
        private Stage2Run Run => game.RunDriver.Run;
        private static object Read(object owner, string property) => owner.GetType().GetProperty(property).GetValue(owner);
        private static object Call(object owner, string method, params object[] args) => owner.GetType().GetMethod(method).Invoke(owner, args);
        [UnitySetUp] public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 4; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            driver = game.GetComponent<Stage2HeroProgressionDriver>(); Assert.That(driver, Is.Not.Null);
            game.HeroCombat.AutomaticTick = false; game.RunDriver.enabled = false; driver.AutomaticTick = false;
            game.RunDriver.ResetRun();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale = 1;
        }

        [UnityTest] public IEnumerator HeroAndTowerKillsAwardOnceGloballyButDespawnAndArrivalDoNot()
        {
            var origin = new Vector2(110, 110); scene.PlayerMover.transform.position = origin;
            var enemy = game.Enemies.Spawn(); enemy.SetAutomaticTick(false); enemy.transform.position = origin + Vector2.right;
            for (var i = 0; i < 10 && enemy.IsAlive; i++)
            { game.HeroCombat.Tick(2); game.Enemies.TickShots(1); }
            Assert.That(enemy.WasKilled, Is.True); Assert.That(driver.Rules.XpWithin, Is.EqualTo(driver.Catalog.XpPerKill));
            enemy.TakeDamage(int.MaxValue); enemy.Despawn();
            var discarded = game.Enemies.Spawn(); discarded.SetAutomaticTick(false); discarded.Despawn();
            var arrival = game.Enemies.SpawnAt(scene.MapDefinition.Throne); arrival.SetAutomaticTick(false); arrival.Tick(.01f);
            Assert.That(driver.Rules.XpWithin, Is.EqualTo(driver.Catalog.XpPerKill));
            var tower = BuildTower();
            var victim = game.Enemies.Spawn(); victim.SetAutomaticTick(false); victim.transform.position = tower.transform.position + Vector3.right;
            for (var i = 0; i < 10 && victim.IsAlive; i++) { tower.Tick(10); game.Enemies.TickShots(2); }
            Assert.That(victim.WasKilled, Is.True); Assert.That(driver.Rules.XpWithin, Is.EqualTo(2 * driver.Catalog.XpPerKill));
            game.RunDriver.ResetRun(); Assert.That(driver.Rules.Level, Is.EqualTo(1)); Assert.That(driver.Rules.XpWithin, Is.Zero);
            yield return null;
        }

        [UnityTest] public IEnumerator BlinkChecksFootprintBuildingsPauseAndUiAndKeepsDebugMovementSeparate()
        {
            var origin = FindClear(new Vector2(110, 110)); scene.PlayerMover.transform.position = origin;
            var target = FindNear(origin); var obstacles = scene.PlayerMover.Obstacles;
            var slot = obstacles.AddUpdatableCircle(target, .1f);
            Assert.That(driver.TryBlink(target), Is.False); Assert.That(driver.Rules.BlinkCooldownRemaining, Is.Zero);
            obstacles.UpdateCircle(slot, target, 0);
            var outside = scene.MapDefinition.CellToWorld(scene.MapDefinition.MinimumCell) - Vector2.one * 10;
            Assert.That(scene.PlayerMover.CanTeleportTo(outside), Is.False);
            var tower = BuildTower(); Assert.That(driver.CanLand(tower.transform.position), Is.False);
            game.SetPaused(true); Assert.That(driver.TryBlink(target), Is.False); game.SetPaused(false);
            var hud = game.GetComponent("Stage3FantasyHud");
            var windowType = hud.GetType().GetProperty("ActiveWindow").PropertyType;
            Call(hud, "SetWindow", Enum.Parse(windowType, "Hero")); Assert.That(driver.TryBlink(target), Is.False);
            Call(hud, "CloseWindow"); Assert.That(driver.TryBlink(target), Is.True);
            Assert.That((Vector2)scene.PlayerMover.transform.position, Is.EqualTo(target));
            scene.PlayerMover.SetMovementMultiplier(5);
            driver.Rules.AwardExperience(driver.Catalog.RequiredXp(1)); driver.Rules.TryUpgradePassive(Stage2HeroPassive.LightStep);
            Assert.That(scene.PlayerMover.EffectiveSpeed, Is.GreaterThan(scene.PlayerMover.Speed));
            Assert.That(scene.PlayerMover.MovementMultiplier, Is.EqualTo(5));
            game.RunDriver.ResetRun(); Assert.That(scene.PlayerMover.EffectiveSpeed, Is.EqualTo(scene.PlayerMover.Speed));
            yield return null;
        }

        [UnityTest] public IEnumerator FailedWaveRestoresXpPointsRanksCooldownAndPersonalStatsWithoutAwardEvents()
        {
            var rules = driver.Rules;
            rules.AwardExperience(driver.Catalog.RequiredXp(1) + 7); Assert.That(rules.TryUpgradePassive(Stage2HeroPassive.PreciseStrike), Is.True);
            var origin = FindClear(new Vector2(110, 110)); scene.PlayerMover.transform.position = origin;
            Assert.That(driver.TryBlink(FindNear(origin)), Is.True); driver.Tick(.5f);
            var remaining = rules.BlinkCooldownRemaining;
            Assert.That(Run.StartWave(), Is.True);
            rules.AwardExperience(2000); rules.TryUpgradeBlink(); rules.TryUpgradePassive(Stage2HeroPassive.LightStep); driver.Tick(30);
            for (var i = 0; i < 30 && !Run.IsTerminal; i++)
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            var awards = 0; rules.ExperienceGained += _ => awards++;
            Assert.That(Run.RetryFailedWave(), Is.True);
            Assert.That(rules.Level, Is.EqualTo(2)); Assert.That(rules.XpWithin, Is.EqualTo(7)); Assert.That(rules.AvailablePoints, Is.Zero);
            Assert.That(rules.BlinkRank, Is.EqualTo(1)); Assert.That(rules.PassiveRank(Stage2HeroPassive.LightStep), Is.Zero);
            Assert.That(rules.DamageBonus, Is.EqualTo(driver.Catalog.DamagePerRank));
            Assert.That(game.HeroCombat.Rules.EffectiveDamage, Is.EqualTo(game.HeroCombat.Catalog.AttackDamage + rules.DamageBonus));
            Assert.That(rules.BlinkCooldownRemaining, Is.EqualTo(remaining)); Assert.That(awards, Is.Zero);
            Assert.That(Run.PreparationRemaining, Is.EqualTo(Run.PreparationDuration));
            Assert.That((bool)Read(game.GetComponent("Stage3HeroLevelUpPresentation"), "IsPlaying"), Is.False);
            yield return null;
        }

        [UnityTest] public IEnumerator ApprovedLevelUpCombinesAwardsFreezesWithoutRestartAndNeverBlocksBlink()
        {
            var effect = game.GetComponent("Stage3HeroLevelUpPresentation"); Assert.That(effect, Is.Not.Null);
            Assert.That((bool)Read(effect, "IsReady"), Is.True);
            Assert.That((bool)Read(effect, "HasAudioClip"), Is.True, "The CC0 choir/gold imprint cue must be included in the build.");
            var rules = driver.Rules; rules.AwardExperience(driver.Catalog.RequiredXp(1));
            rules.AwardExperience(driver.Catalog.RequiredXp(2));
            Assert.That((int)Read(effect, "DisplayedLevel"), Is.EqualTo(3));
            Assert.That((int)Read(effect, "DisplayedPoints"), Is.EqualTo(2));
            Assert.That((bool)Read(effect, "IsPlaying"), Is.True);
            var origin = FindClear(new Vector2(110, 110)); scene.PlayerMover.transform.position = origin;
            Assert.That(driver.TryBlink(FindNear(origin)), Is.True, "The celebration never captures world controls.");
            yield return new WaitForSecondsRealtime(.1f);
            game.SetPaused(true); var elapsed = (float)Read(effect, "Elapsed");
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That((float)Read(effect, "Elapsed"), Is.EqualTo(elapsed));
            var hud = game.GetComponent("Stage3FantasyHud"); var windowType = hud.GetType().GetProperty("ActiveWindow").PropertyType;
            game.SetPaused(false); Call(hud, "SetWindow", Enum.Parse(windowType, "Hero"));
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That((float)Read(effect, "Elapsed"), Is.EqualTo(elapsed));
            Call(hud, "CloseWindow"); yield return new WaitForSecondsRealtime(.1f);
            Assert.That((float)Read(effect, "Elapsed"), Is.GreaterThan(elapsed));
            Assert.That((int)Read(effect, "DisplayedPoints"), Is.EqualTo(2));
            yield return new WaitForSecondsRealtime((float)Read(effect, "Duration") + .2f);
            Assert.That((bool)Read(effect, "IsPlaying"), Is.False);
            Call(hud, "SetWindow", Enum.Parse(windowType, "Hero")); Call(hud, "CloseWindow"); yield return null;
            Assert.That((bool)Read(effect, "IsPlaying"), Is.False, "Opening O cannot replay a finished celebration.");
            rules.AwardExperience(rules.RequiredXp); Assert.That((bool)Read(effect, "IsPlaying"), Is.True);
            game.RunDriver.ResetRun(); Assert.That((bool)Read(effect, "IsPlaying"), Is.False);
        }

        [UnityTest] public IEnumerator HudReflectsProgressAndKeyboardBlinkCastsToWorldCursor()
        {
            var origin = FindClear((Vector2)scene.PlayerMover.transform.position); scene.PlayerMover.transform.position = origin;
            scene.CameraFollow.SnapToTarget(); yield return null;
            var target = FindNear(origin); var screen = scene.CameraFollow.BoundCamera.WorldToScreenPoint(target);
            input.Set(Mouse.current.position, (Vector2)screen);
            input.Press(Keyboard.current.qKey); yield return null; input.Release(Keyboard.current.qKey);
            Assert.That(Vector2.Distance(scene.PlayerMover.transform.position, target), Is.LessThan(.01));
            Assert.That(driver.Rules.BlinkCooldownRemaining, Is.GreaterThan(0));
            driver.Rules.AwardExperience(driver.Catalog.RequiredXp(1) + 11); yield return null;
            var hud = game.GetComponent("Stage3FantasyHud"); var view = Read(hud, "HeroProgressionView");
            Assert.That((int)view.GetType().GetField("Level").GetValue(view), Is.EqualTo(2));
            Assert.That((int)view.GetType().GetField("XpWithin").GetValue(view), Is.EqualTo(11));
            Call(hud, "SetWindow", Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType, "Hero"));
            var purchase = (Action<int>)hud.GetType().GetField("HeroPassiveUpgradeRequested", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hud);
            purchase(0); yield return null;
            Assert.That(driver.Rules.PassiveRank(Stage2HeroPassive.PreciseStrike), Is.EqualTo(1));
            Assert.That((int)view.GetType().GetField("WeaponDamage").GetValue(view), Is.EqualTo(game.HeroCombat.Rules.EffectiveDamage));
        }

        private Vector2 FindClear(Vector2 centre)
        {
            for (var y = -5; y <= 5; y++) for (var x = -5; x <= 5; x++)
            { var p = centre + new Vector2(x, y); if (driver.CanLand(p)) return p; }
            throw new Exception("No nearby clear hero footing.");
        }
        private Vector2 FindNear(Vector2 origin)
        {
            for (var i = 0; i < 16; i++)
            { var p = origin + new Vector2(Mathf.Cos(i * Mathf.PI / 8), Mathf.Sin(i * Mathf.PI / 8)) * 1.5f; if (driver.CanLand(p)) return p; }
            throw new Exception("No nearby Blink landing.");
        }
        private Stage2Tower BuildTower()
        {
            var inventory = game.Gathering.Economy.Inventory;
            foreach (MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind))) inventory.TryAdd(kind, 100);
            inventory.TryAddGold(100); var defense = game.Building.Defense; defense.SelectRole(0); defense.BeginPlacement();
            for (var y = scene.MapDefinition.MinimumCell.y; y <= scene.MapDefinition.MaximumCell.y; y++)
            for (var x = scene.MapDefinition.MinimumCell.x; x <= scene.MapDefinition.MaximumCell.x; x++)
            {
                var cell = new Vector2Int(x, y); if (defense.Evaluate(cell) != PlacementResult.Ready) continue;
                defense.SetPreviewCell(cell); Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
                defense.CancelPlacement(); return tower;
            }
            throw new Exception("No legal tower site.");
        }
    }
}
#endif
