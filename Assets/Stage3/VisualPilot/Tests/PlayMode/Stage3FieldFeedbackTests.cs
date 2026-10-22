#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3FieldFeedbackTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Component feedback, hud, guild;
        private int previousRate;
        private static T Read<T>(object owner, string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner, string name, params object[] args) => owner.GetType().GetMethod(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(owner, args);
        private void Window(string name) => Call(hud, "SetWindow", Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType, name));
        [UnitySetUp] public IEnumerator Load()
        {
            previousRate = Time.captureFramerate; Time.captureFramerate = 60; Time.timeScale = 1;
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 16; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            feedback = game.GetComponent("Stage3FieldFeedbackPresentation");
            hud = game.GetComponent("Stage3FantasyHud"); guild = game.GetComponent("Stage3GuildWorldDriver");
            Assert.That(feedback, Is.Not.Null);
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false;
            game.RunDriver.ResetRun(); yield return null; yield return null;
            Window("None");
            if (guild != null) guild.GetType().GetProperty("AutomaticTick").SetValue(guild, false);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown(); Time.captureFramerate = previousRate; Time.timeScale = 1;
        }
        [UnityTest] public IEnumerator PortalAndGuildLethalHitsShowActualHpLossAndNoMana()
        {
            var baseline = Read<long>(feedback, "ConfirmedDamageTotal");
            var portal = game.Enemies.Spawn(); portal.SetAutomaticTick(false);
            var hp = portal.Health;
            Assert.That(portal.TakeDamage(int.MaxValue), Is.True);
            Assert.That(Read<long>(feedback, "ConfirmedDamageTotal") - baseline, Is.EqualTo(hp));
            Assert.That(Read<int>(feedback, "LastActualAmount"), Is.EqualTo(hp));
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.EqualTo(1));
            Assert.That(Read<int>(feedback, "VisibleManaBars"), Is.Zero);
            Assert.That(portal.TakeDamage(3), Is.False);
            Assert.That(Read<long>(feedback, "ConfirmedDamageTotal") - baseline, Is.EqualTo(hp));
            var units = Read<IReadOnlyList<Stage2GuildEnemy>>(guild, "Enemies");
            var enemy = units.First(e => e.Alive); var guildHp = enemy.Health;
            Assert.That(Call(guild, "Damage", enemy, int.MaxValue), Is.True);
            Assert.That(Read<long>(feedback, "ConfirmedDamageTotal") - baseline, Is.EqualTo(hp + guildHp));
            Assert.That(Read<int>(feedback, "LastActualAmount"), Is.EqualTo(guildHp));
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.EqualTo(2));
            yield return null;
        }
        [UnityTest] public IEnumerator ManualGrantUsesActualRemainingStockAndCancelledWorkCreatesNothing()
        {
            var economy = game.Gathering.Economy;
            var node = economy.Nodes.First(n => n.Resource == MapResourceKind.Stone && n.Remaining > 0);
            economy.SetManualToolStats(5, 5, 1, 1);
            while (node.Remaining > 2) Call(node, "TryTakeUnits", Math.Min(node.Remaining - 2, 20));
            var before = Read<long>(feedback, "ConfirmedGatherTotal");
            Assert.That(economy.AdvanceGather(node, node.WorldPosition, true, .001f, true), Is.False);
            economy.CancelGather();
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.Zero);
            var actual = node.Remaining;
            Assert.That(economy.AdvanceGather(node, node.WorldPosition, true, economy.GatherDuration(node), true), Is.True);
            Assert.That(Read<long>(feedback, "ConfirmedGatherTotal") - before, Is.EqualTo(actual));
            Assert.That(Read<int>(feedback, "LastActualAmount"), Is.EqualTo(actual));
            Assert.That(Read<bool>(feedback, "LastWasGather"), Is.True);
            Assert.That(economy.AdvanceGather(node, node.WorldPosition, true, 10, true), Is.False);
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.EqualTo(1));
            game.RunDriver.ResetRun();
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator DensePortalHitsAreBoundedAndZeroDamageHasNoFeedback()
        {
            for (var i = 0; i < 35; i++)
            {
                var enemy = game.Enemies.Spawn(); enemy.SetAutomaticTick(false);
                Assert.That(enemy.TakeDamage(0), Is.False);
                enemy.TakeDamage(1);
            }
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.EqualTo(24));
            game.Enemies.ResetRun();
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator PauseAndModalFreezeExistingFeedbackAndModalDamageDoesNotReplay()
        {
            var enemy = game.Enemies.Spawn(); enemy.SetAutomaticTick(false); enemy.TakeDamage(1);
            yield return null;
            game.RunDriver.Run.SetPaused(true);
            var age = Read<float>(feedback, "OldestFeedbackAge");
            for (var i = 0; i < 5; i++) yield return null;
            Assert.That(Read<float>(feedback, "OldestFeedbackAge"), Is.EqualTo(age));
            game.RunDriver.Run.SetPaused(false); Window("Inventory");
            for (var i = 0; i < 5; i++) yield return null;
            Assert.That(Read<float>(feedback, "OldestFeedbackAge"), Is.EqualTo(age));
            var other = game.Enemies.Spawn(); other.SetAutomaticTick(false); other.TakeDamage(1);
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.EqualTo(1));
            Window("None");
            for (var i = 0; i < 60; i++) yield return null;
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.Zero);
        }
        [UnityTest] public IEnumerator HudExclusionAndDisableClearTransientFeedback()
        {
            var r = Read<Rect>(hud, "WorldHudRect");
            Assert.That(Call(feedback, "IsClearOfHud", r), Is.False);
            Assert.That(Call(feedback, "IsClearOfHud", Read<Rect>(hud, "CycleDialRect")), Is.False);
            Assert.That(Call(feedback, "IsClearOfHud", new Rect(-5, 100, 20, 20)), Is.False);
            var enemy = game.Enemies.Spawn(); enemy.SetAutomaticTick(false); enemy.TakeDamage(1);
            ((Behaviour)feedback).enabled = false;
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.Zero);
            ((Behaviour)feedback).enabled = true; yield return null;
            Assert.That(Read<int>(feedback, "ActiveFloatingCount"), Is.Zero);
        }

        [Test] public void HealthBarsKeepIsolatedSizeButSeparateTheWidePortalRowsAtEveryResolution()
        {
            var method = feedback.GetType().GetMethod("LayoutHealthBars", BindingFlags.Public | BindingFlags.Static);
            foreach (var scale in new[] { .8f, 1f, 1.2f })
            {
                var points = new[] { new Vector2(0, 100), new Vector2(36, 100), new Vector2(72, 100),
                    new Vector2(108, 100), new Vector2(400, 100) }.Select(p => p * scale).ToArray();
                var bars = (Rect[])method.Invoke(null, new object[] { points, new[] { .7f, .7f, .7f, .7f, .7f }, scale });
                Assert.That(bars.All(r => r.width > 0), Is.True);
                Assert.That(bars[4].width, Is.EqualTo(50 * scale).Within(.001f));
                for (var i = 0; i < 3; i++)
                    Assert.That(bars[i + 1].xMin - bars[i].xMax, Is.GreaterThanOrEqualTo(4 * scale - .001f));
                Assert.That(bars[0].width, Is.EqualTo(32 * scale).Within(.001f));
            }
        }

        [Test] public void VeryDenseHealthBarsRetainInjuredPriorityAndNeverMerge()
        {
            var method = feedback.GetType().GetMethod("LayoutHealthBars", BindingFlags.Public | BindingFlags.Static);
            var points = Enumerable.Range(0, 80).Select(i => new Vector2(i * 3, 100 + i % 2 * 2)).ToArray();
            var fractions = Enumerable.Repeat(.9f, points.Length).ToArray(); fractions[40] = .1f;
            var bars = (Rect[])method.Invoke(null, new object[] { points, fractions, 1f });
            Assert.That(bars[40].width, Is.GreaterThanOrEqualTo(24), "Most injured target receives a readable bar first.");
            var shown = bars.Where(r => r.width > 0).ToArray();
            Assert.That(shown.Length, Is.LessThan(points.Length));
            foreach (var bar in shown) Assert.That(bar.width, Is.InRange(24, 50));
            for (var a = 0; a < shown.Length; a++) for (var b = a + 1; b < shown.Length; b++)
                Assert.That(shown[a].Overlaps(shown[b]), Is.False, "Dense bars must never form a false shared HP line.");
        }
    }
}
#endif
