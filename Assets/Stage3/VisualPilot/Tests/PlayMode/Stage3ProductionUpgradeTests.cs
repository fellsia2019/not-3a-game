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
    public sealed class Stage3ProductionUpgradeTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2ProductionDriver production;
        private Component binding, hud;
        private int oldRate;
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
            production = game.Production; binding = game.GetComponent("Stage3PlayerInterfaceBinding"); hud = game.GetComponent("Stage3FantasyHud");
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            Wallet.TryCredit(new Stage2ResourceCost(5000, 5000, 5000, 5000));
            scene.CameraFollow.enabled = false; yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.captureFramerate = oldRate; Time.timeScale = 1;
        }
        private Stage2ProductionBuilding Build(ResourceProductionBuilding kind)
        {
            var index = Enumerable.Range(0, production.Rules.Points.Count).First(i => production.Rules.Points[i].Building == kind);
            scene.PlayerMover.transform.position = production.Rules.Points[index].Position + Vector2.one * 5;
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintCatalog.ForProduction(kind));
                production.SelectAndBeginPlacement(kind);
            Assert.That(production.TryBuildAt(index), Is.EqualTo(ProductionPlacementResult.Built));
            return production.Rules.Buildings.Last();
        }
        private void Select(Stage2ProductionBuilding building)
        {
            var camera = scene.CameraFollow.BoundCamera; var sprite = production.GetBuiltVisual(building).GetComponent<SpriteRenderer>();
            camera.orthographicSize = 5; camera.transform.position = sprite.bounds.center - camera.transform.forward * 30;
            var projected = camera.WorldToScreenPoint(sprite.bounds.center);
            var wanted = new Vector2(Screen.width * .5f, Screen.height * .72f);
            camera.transform.position += sprite.bounds.center - camera.ScreenToWorldPoint(new Vector3(wanted.x, wanted.y, projected.z));
            Assert.That(Call(binding, "TrySelectBuildingAtScreen", (Vector2)camera.WorldToScreenPoint(sprite.bounds.center)), Is.True);
            Assert.That(Read<object>(binding, "SelectedBuildingKind").ToString(), Is.EqualTo("Production"));
        }
        [UnityTest] public IEnumerator AllThreeRealCardsUpgradeSeparatelyPayExactCostAndPreserveVisualCollision()
        {
            foreach (ResourceProductionBuilding kind in Enum.GetValues(typeof(ResourceProductionBuilding)))
            {
                Window("None"); var building = Build(kind); production.enabled = false;
                var visual = production.GetBuiltVisual(building); var footprint = building.Footprint;
                production.Rules.Tick(building.IntervalSeconds * .5f); var progress = building.ProgressSeconds;
                Select(building); yield return null;
                for (var next = 2; next <= 5; next++)
                {
                    building.Entry.TryGetUpgrade(building.Level, out var upgrade);
                    var cost = upgrade.cost; var wood = Wallet.Get(MapResourceKind.Wood); var gold = Wallet.Gold;
                    Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("Upgraded"));
                    Assert.That(building.Level, Is.EqualTo(next)); Assert.That(building.OutputAmount, Is.EqualTo(next));
                    Assert.That(Wallet.Get(MapResourceKind.Wood), Is.EqualTo(wood - cost.Wood));
                    Assert.That(Wallet.Gold, Is.EqualTo(gold - cost.Gold));
                    Assert.That(building.ProgressSeconds, Is.EqualTo(progress));
                    Assert.That(production.GetBuiltVisual(building), Is.SameAs(visual)); Assert.That(building.Footprint, Is.EqualTo(footprint));
                    Assert.That(scene.PlayerMover.Obstacles.IsBlocked(footprint.center, scene.PlayerMover.CollisionRadius), Is.True);
                }
                Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("MaximumLevel"));
                var view = Read<object>(hud, "SelectedBuildingView");
                Assert.That((string)view.GetType().GetField("Subtitle").GetValue(view), Is.EqualTo("Уровень 5 / 5"));
                Assert.That((bool)view.GetType().GetField("UpgradeAvailable").GetValue(view), Is.False);
                Assert.That((string)view.GetType().GetField("UpgradeUnavailableReason").GetValue(view), Is.EqualTo("Максимальный уровень"));
                production.enabled = true;
            }
        }
        [UnityTest] public IEnumerator PaidOutputNeverAwardsProfessionXpAndSelectionInBuildModeRespectsPauseAndFunds()
        {
            var building = Build(ResourceProductionBuilding.IronMine); production.enabled = false;
            Window("Construction"); Select(building); yield return null;
            Wallet.TrySpend(new Stage2ResourceCost(Wallet.Get(MapResourceKind.Wood), Wallet.Get(MapResourceKind.Stone), Wallet.Get(MapResourceKind.Metal), Wallet.Gold));
            yield return null; Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("InsufficientResources"));
            Wallet.TryCredit(new Stage2ResourceCost(500, 500, 500, 500));
            game.SetPaused(true); Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("InvalidBuilding"));
            game.SetPaused(false); Select(building);
            Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("Upgraded"));
            var before = Wallet.Get(MapResourceKind.Metal); production.Rules.Tick(building.IntervalSeconds);
            Assert.That(Wallet.Get(MapResourceKind.Metal) - before, Is.EqualTo(2));
            var professions = game.GetComponent<Stage2ProfessionDriver>().Rules;
            Assert.That(professions.XpWithin(Stage2Profession.Miner), Is.Zero);
            Assert.That(professions.Mastery(Stage2Profession.Miner), Is.EqualTo(1));
            Window("Inventory"); Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("InvalidBuilding"));
        }
        [UnityTest] public IEnumerator FailedNightKeepsCurrentProductionUpgradeAndFractionButResetStartsAtLevelOne()
        {
            var building = Build(ResourceProductionBuilding.Quarry); production.enabled = false;
            var run = game.RunDriver.Run; Assert.That(Stage3TestCycle.StartNight(run), Is.True);
            Select(building); Assert.That(Call(binding, "TryUpgradeSelectedProduction").ToString(), Is.EqualTo("Upgraded"));
            production.Rules.Tick(3.25f); var progress = building.ProgressSeconds; var gold = Wallet.Gold;
            for (var i = 0; i < 10 && !run.IsTerminal; i++)
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost)); Assert.That(run.RetryFailedWave(), Is.True);
            Assert.That(production.Rules.Buildings[0], Is.SameAs(building)); Assert.That(building.Level, Is.EqualTo(2));
            Assert.That(building.ProgressSeconds, Is.EqualTo(progress)); Assert.That(Wallet.Gold, Is.EqualTo(gold));
            game.RunDriver.ResetRun(); Assert.That(production.Rules.Buildings.Count, Is.Zero);
            Wallet.TryCredit(new Stage2ResourceCost(100, 100, 100, 100)); building = Build(ResourceProductionBuilding.Quarry);
            Assert.That(building.Level, Is.EqualTo(1)); Assert.That(building.ProgressSeconds, Is.Zero); yield return null;
        }
    }
}
#endif
