#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
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
    public sealed class Stage3ThroneUpgradeTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component binding, hud;
        private int oldRate;
        private Stage2Run Run => game.RunDriver.Run;
        private Stage2Inventory Wallet => game.Gathering.Economy.Inventory;
        private static T Read<T>(object owner, string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner, string name, params object[] args) => owner.GetType().GetMethod(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(owner, args);
        private void Window(string name) => Call(hud, "SetWindow", Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType, name));
        [UnitySetUp] public IEnumerator Load()
        {
            oldRate = Time.captureFramerate; Time.captureFramerate = 60; Time.timeScale = 1;
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 14; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            binding = game.GetComponent("Stage3PlayerInterfaceBinding"); hud = game.GetComponent("Stage3FantasyHud");
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            Wallet.TryCredit(new Stage2ResourceCost(5000, 5000, 5000, 5000)); scene.CameraFollow.enabled = false;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.captureFramerate = oldRate; Time.timeScale = 1;
        }
        private void Select()
        {
            var marker = scene.ThroneMarker.GetComponentsInChildren<Component>().First(c => c.GetType().Name == "Stage3LandmarkSprite");
            var sprite = Read<SpriteRenderer>(marker, "Body"); var camera = scene.CameraFollow.BoundCamera;
            camera.orthographicSize = 5; camera.transform.position = sprite.bounds.center - camera.transform.forward * 30;
            var p = camera.WorldToScreenPoint(sprite.bounds.center); var wanted = new Vector2(Screen.width * .5f, Screen.height * .72f);
            camera.transform.position += sprite.bounds.center - camera.ScreenToWorldPoint(new Vector3(wanted.x, wanted.y, p.z));
            Assert.That(Call(binding, "TrySelectBuildingAtScreen", (Vector2)camera.WorldToScreenPoint(sprite.bounds.center)), Is.True);
            Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("Throne"));
        }
        private object View => Read<object>(hud, "SelectedBuildingView");
        private T Field<T>(string name) => (T)View.GetType().GetField(name).GetValue(View);
        [UnityTest] public IEnumerator RealThroneCardPaysThreePricesInWorldAndBuildDockAndShowsCap()
        {
            Assert.That(Run.ThroneUpgradeLimit, Is.EqualTo(3)); Assert.That(Run.ThroneMaximum, Is.EqualTo(5));
            for (var step = 0; step < 3; step++)
            {
                Window(step == 1 ? "Construction" : "None"); Select(); yield return null;
                Run.TryGetThroneUpgradeCost(out var cost); var gold = Wallet.Gold; var stone = Wallet.Get(MapResourceKind.Stone);
                Assert.That(Field<bool>("UpgradeAvailable"), Is.True);
                Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("Upgraded"));
                Assert.That(Run.ThroneHealth, Is.EqualTo(10 + step * 5)); Assert.That(Run.ThroneMaximum, Is.EqualTo(10 + step * 5));
                Assert.That(Wallet.Gold, Is.EqualTo(gold - cost.Gold)); Assert.That(Wallet.Get(MapResourceKind.Stone), Is.EqualTo(stone - cost.Stone));
            }
            yield return null;
            Assert.That(Field<string>("Subtitle"), Is.EqualTo("Усиления 3 / 3"));
            Assert.That(Field<bool>("UpgradeAvailable"), Is.False); Assert.That(Field<string>("UpgradeUnavailableReason"), Is.EqualTo("Максимальное усиление"));
            Assert.That(Field<string>("UpgradePrice"), Is.Null.Or.Empty);
            var capped = Wallet.Gold; Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("MaximumUpgrades"));
            Assert.That(Wallet.Gold, Is.EqualTo(capped));
        }
        [UnityTest] public IEnumerator CardGatesFundsPauseModalAndKeepsUpgradeOnRetryButNotFullReset()
        {
            Select(); Wallet.TrySpend(new Stage2ResourceCost(Wallet.Get(MapResourceKind.Wood), Wallet.Get(MapResourceKind.Stone), Wallet.Get(MapResourceKind.Metal), Wallet.Gold));
            yield return null; Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("InsufficientResources"));
            Wallet.TryCredit(new Stage2ResourceCost(500, 500, 500, 500));
            game.SetPaused(true); Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("Inactive"));
            game.SetPaused(false); Window("Inventory"); Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("Inactive"));
            Window("None"); Assert.That(Stage3TestCycle.StartNight(Run), Is.True); Select();
            Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("Upgraded")); var gold = Wallet.Gold;
            for (var i = 0; i < 20 && !Run.IsTerminal; i++)
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(Call(binding, "TryUpgradeSelectedThrone").ToString(), Is.EqualTo("Inactive"));
            Assert.That(Run.RetryFailedWave(), Is.True); yield return null;
            Assert.That(Run.ThroneUpgradeCount, Is.EqualTo(1)); Assert.That(Run.ThroneHealth, Is.EqualTo(10)); Assert.That(Wallet.Gold, Is.EqualTo(gold));
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(Run.ThroneUpgradeCount, Is.Zero); Assert.That(Run.ThroneMaximum, Is.EqualTo(5));
        }
    }
}
#endif
