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
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3ProfessionRuleTests
    {
        private Stage2ProfessionCatalog data;
        private Stage2Professions rules;
        [SetUp] public void Create() { data = ScriptableObject.CreateInstance<Stage2ProfessionCatalog>(); rules = new Stage2Professions(data); }
        [TearDown] public void Dispose() => Object.DestroyImmediate(data);
        [Test] public void MaterialKindsAdvanceIndependentTracksAndOnlyMilestonesAwardPoints()
        {
            rules.AwardMaterial(MapResourceKind.Stone, 55); //20+25, mastery3 XP10.
            rules.AwardMaterial(MapResourceKind.Metal, 55); //30+35, mastery5 XP0.
            Assert.That(rules.Mastery(Stage2Profession.Miner), Is.EqualTo(5));
            Assert.That(rules.XpWithin(Stage2Profession.Miner), Is.Zero);
            Assert.That(rules.AvailablePoints(Stage2Profession.Miner), Is.EqualTo(1));
            Assert.That(rules.Mastery(Stage2Profession.Logger), Is.EqualTo(1));
            rules.AwardMaterial(MapResourceKind.Wood, 19);
            Assert.That(rules.XpWithin(Stage2Profession.Logger), Is.EqualTo(19));
            rules.AwardMaterial(MapResourceKind.Wood, 0); rules.AwardMaterial(MapResourceKind.Wood, -1);
            rules.AwardMaterial((MapResourceKind)99, 500);
            Assert.That(rules.XpWithin(Stage2Profession.Logger), Is.EqualTo(19));
            rules.AwardMaterial(MapResourceKind.Wood, 1);
            Assert.That(rules.Mastery(Stage2Profession.Logger), Is.EqualTo(2));
            Assert.That(rules.AvailablePoints(Stage2Profession.Logger), Is.Zero);
        }
        [Test] public void CapOverflowSpendingAndResetCannotDuplicatePoints()
        {
            rules.AwardMaterial(MapResourceKind.Stone, int.MaxValue);
            Assert.That(rules.Mastery(Stage2Profession.Miner), Is.EqualTo(100));
            Assert.That(rules.RequiredXp(Stage2Profession.Miner), Is.Zero);
            Assert.That(rules.XpWithin(Stage2Profession.Miner), Is.Zero);
            Assert.That(rules.AvailablePoints(Stage2Profession.Miner), Is.EqualTo(20));
            for (var i = 0; i < 3; i++) Assert.That(rules.TryUpgrade(Stage2Profession.Miner, Stage2ProfessionTalent.Strength), Is.True);
            Assert.That(rules.TryUpgrade(Stage2Profession.Miner, Stage2ProfessionTalent.Strength), Is.False);
            rules.AwardMaterial(MapResourceKind.Metal, int.MaxValue);
            Assert.That(rules.AvailablePoints(Stage2Profession.Miner), Is.EqualTo(17));
            Assert.That(rules.TryUpgrade(Stage2Profession.Logger, Stage2ProfessionTalent.Strength), Is.False);
            Assert.That(rules.PowerBonus(Stage2Profession.Miner), Is.EqualTo(3));
            rules.Reset();
            Assert.That(rules.Mastery(Stage2Profession.Miner), Is.EqualTo(1));
            Assert.That(rules.AvailablePoints(Stage2Profession.Miner), Is.Zero);
            Assert.That(rules.PowerBonus(Stage2Profession.Miner), Is.Zero);
        }
        [Test] public void XpChangesDoNotInvalidateUnfinishedWorkButTalentChangesDo()
        {
            var changes = 0; rules.BonusesChanged += () => changes++;
            rules.AwardMaterial(MapResourceKind.Stone, 110); Assert.That(changes, Is.Zero);
            rules.TryUpgrade(Stage2Profession.Miner, Stage2ProfessionTalent.Tempo);
            Assert.That(changes, Is.EqualTo(1)); Assert.That(rules.TempoBonus(Stage2Profession.Miner), Is.EqualTo(.05f));
        }
        [Test] public void CatalogRejectsInvalidCurveAndNonFiniteBonuses()
        {
            var serialized = new SerializedObject(data);
            serialized.FindProperty("tempoPerRank").floatValue = float.NaN; serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(data.TryValidate(out _), Is.False);
            Assert.Throws<ArgumentException>(() => new Stage2Professions(data));
        }
    }

    public sealed class Stage3ProfessionTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2ProfessionDriver driver;
        private Stage2EquipmentDriver items;
        private Component binding, hud;
        private int previousRate;
        private Stage2Economy Economy => game.Gathering.Economy;
        private static T Read<T>(object owner, string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner, string name, params object[] args) => owner.GetType().GetMethod(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(owner, args);
        private void Window(string name) => Call(hud, "SetWindow", Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType, name));
        private void OpenProfessions() { Window("Hero"); Call(hud, "ShowProfessions", true); }
        [UnitySetUp] public IEnumerator Load()
        {
            previousRate = Time.captureFramerate; Time.captureFramerate = 60; Time.timeScale = 1;
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 14; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            driver = game.GetComponent<Stage2ProfessionDriver>(); items = game.GetComponent<Stage2EquipmentDriver>();
            binding = game.GetComponent("Stage3PlayerInterfaceBinding"); hud = game.GetComponent("Stage3FantasyHud");
            Assert.That(driver, Is.Not.Null); game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false;
            game.RunDriver.ResetRun(); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.captureFramerate = previousRate; Time.timeScale = 1;
        }
        private Stage2ResourceNode Node(MapResourceKind kind) => Economy.Nodes.First(n => n.Resource == kind && n.Size == Stage2ResourceSize.Large);
        [UnityTest] public IEnumerator ActualExtractionUsesGrantAndContinuousHeldEDoesNotRequireReleaseAfterXp()
        {
            var collisions = game.GetComponent("Stage3ManualResourceCollisions");
            Stage2ResourceNode target = null;
            foreach (var node in Economy.Nodes.Where(n => n.Resource == MapResourceKind.Stone && n.Size == Stage2ResourceSize.Large))
            {
                var radius = (float)Call(collisions, "ColliderRadius", node) + scene.PlayerMover.CollisionRadius + .08f;
                var point = node.WorldPosition + Vector2.left * radius;
                if (scene.PlayerMover.Obstacles.IsBlocked(point, scene.PlayerMover.CollisionRadius) || Economy.NearestAvailableNode(point) != node) continue;
                target = node; scene.PlayerMover.transform.position = point; break;
            }
            Assert.That(target, Is.Not.Null); yield return null; yield return null;
            var stock = target.Remaining; var wallet = Economy.Inventory.Get(MapResourceKind.Stone);
            input.Press(Keyboard.current.eKey);
            for (var i = 0; i < 110; i++) yield return null;
            input.Release(Keyboard.current.eKey); yield return null;
            var actual = stock - target.Remaining;
            Assert.That(actual, Is.GreaterThanOrEqualTo(2), "XP from a hit must not force E release after every strike.");
            Assert.That(Economy.Inventory.Get(MapResourceKind.Stone) - wallet, Is.EqualTo(actual));
            Assert.That(driver.Rules.XpWithin(Stage2Profession.Miner), Is.EqualTo(actual));
            Assert.That(driver.Rules.XpWithin(Stage2Profession.Logger), Is.Zero);
            Assert.That(game.GetComponent<Stage2HeroProgressionDriver>().Rules.XpWithin, Is.Zero);
        }
        [UnityTest] public IEnumerator ToolAndTalentsConsumeExactStockAndReachUsesTheSameSelectionPredicate()
        {
            driver.Rules.AwardMaterial(MapResourceKind.Stone, int.MaxValue); OpenProfessions();
            for (var t = 0; t < 3; t++) Assert.That(Call(binding, "TryUpgradeProfession", 0, t), Is.True);
            var node = Node(MapResourceKind.Stone);
            Assert.That(Economy.HarvestPower(MapResourceKind.Stone), Is.EqualTo(2));
            Assert.That(Economy.GatherDuration(node), Is.EqualTo(node.GatherSeconds / 1.05f).Within(.0001f));
            var baseGap = Economy.InteractionRange(node) / 1.1f;
            var point = node.WorldPosition + Vector2.left * (node.InteractionRadius + baseGap * 1.05f);
            Assert.That(Economy.IsInRange(node, point), Is.True);
            Assert.That(Economy.NearestAvailableNode(point), Is.SameAs(node));
            var before = node.Remaining; var wallet = Economy.Inventory.Get(MapResourceKind.Stone);
            Assert.That(Economy.AdvanceGather(node, point, true, Economy.GatherDuration(node), true), Is.True);
            Assert.That(before - node.Remaining, Is.EqualTo(2)); Assert.That(Economy.Inventory.Get(MapResourceKind.Stone) - wallet, Is.EqualTo(2));
            Window("Inventory"); var pick = items.Rules.Equipped(Stage2ItemSlot.Pickaxe);
            Assert.That(Call(binding, "TryUnequipSlot", 5), Is.True); Assert.That(Economy.HarvestPower(MapResourceKind.Stone), Is.Zero);
            Assert.That(Economy.AdvanceGather(node, point, true, 10f, true), Is.False);
            Assert.That(Call(binding, "TryEquipItem", pick.Id), Is.True);
            Assert.That(items.Rules.TryReceiveLoot(Stage2ItemType.Pickaxe, Stage2ItemQuality.D, 1, out var stronger), Is.True);
            Assert.That(Call(binding, "TryEquipItem", stronger.Id), Is.True);
            Assert.That(Economy.HarvestPower(MapResourceKind.Stone), Is.EqualTo(3), "D1 tool2 + profession1, not one source replacing the other.");
            Assert.That(Economy.GatherDuration(node), Is.EqualTo(node.GatherSeconds / (1.08f * 1.05f)).Within(.0001f));
            while (node.Remaining > 0) Economy.AdvanceGather(node, node.WorldPosition, true, Economy.GatherDuration(node), true);
            Assert.That(Economy.Inventory.Get(MapResourceKind.Stone) - wallet, Is.EqualTo(before));
            Assert.That(Economy.AdvanceGather(node, node.WorldPosition, true, 10, true), Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator PartialBlockedAndAutomaticWalletChangesGiveNoMasteryAndRankChangeCancelsWork()
        {
            var node = Node(MapResourceKind.Metal); var duration = Economy.GatherDuration(node);
            Assert.That(Economy.AdvanceGather(node, node.WorldPosition, true, duration * .5f, true), Is.False);
            Assert.That(Economy.AdvanceGather(node, node.WorldPosition, false, duration, true), Is.False);
            Assert.That(Economy.AdvanceGather(node, node.WorldPosition, true, duration, false), Is.False);
            Economy.Inventory.TryAdd(MapResourceKind.Metal, 100); Economy.Inventory.TryAddGold(10);
            Assert.That(driver.Rules.XpWithin(Stage2Profession.Miner), Is.Zero);
            driver.Rules.AwardMaterial(MapResourceKind.Stone, 110); OpenProfessions();
            Economy.AdvanceGather(node, node.WorldPosition, true, duration * .5f, true);
            Assert.That(Call(binding, "TryUpgradeProfession", 0, 1), Is.True);
            Assert.That(Economy.GatherProgress, Is.Zero);
            var stock = node.Remaining;
            Assert.That(Economy.AdvanceGather(node, node.WorldPosition, true, duration * .51f, true), Is.False);
            Assert.That(node.Remaining, Is.EqualTo(stock)); yield return null;
        }
        [UnityTest] public IEnumerator ProfessionViewShowsActualIndependentPointsAndPurchasesAreBlockedOutsideThePageOrDuringPause()
        {
            driver.Rules.AwardMaterial(MapResourceKind.Wood, 110); yield return null;
            Assert.That(Call(binding, "TryUpgradeProfession", 1, 0), Is.False);
            OpenProfessions(); yield return null;
            Call(hud, "SelectProfession", 1); Call(hud, "SelectProfessionTalent", 1);
            Assert.That(Read<int>(hud, "SelectedProfessionIndex"), Is.EqualTo(1));
            Assert.That(Read<Rect>(hud, "ProfessionUpgradeRect").width, Is.GreaterThan(0));
            Assert.That(Call(binding, "TryUpgradeProfession", 0, 0), Is.False, "Miner cannot spend logger's point.");
            game.RunDriver.Run.SetPaused(true); yield return null;
            Assert.That(Call(binding, "TryUpgradeProfession", 1, 0), Is.False);
            game.RunDriver.Run.SetPaused(false); yield return null;
            Assert.That(Call(binding, "TryUpgradeProfession", 1, 1), Is.True);
            Assert.That(driver.Rules.TempoBonus(Stage2Profession.Logger), Is.EqualTo(.05f));
            Assert.That(driver.Rules.TempoBonus(Stage2Profession.Miner), Is.Zero);
            var view = Read<object>(hud, "ProfessionsView");
            var rows = (Array)view.GetType().GetField("Professions").GetValue(view);
            Assert.That((int)rows.GetValue(1).GetType().GetField("AvailablePoints").GetValue(rows.GetValue(1)), Is.Zero);
            var tool = rows.GetValue(1).GetType().GetField("Tool").GetValue(rows.GetValue(1));
            Assert.That((int)tool.GetType().GetField("QualityIndex").GetValue(tool), Is.Zero);
            Assert.That((int)tool.GetType().GetField("Level").GetValue(tool), Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DefeatPreservesCurrentMasteryAndRankWhileFullResetClearsBothAndModifiers()
        {
            var run = game.RunDriver.Run; Assert.That(Stage3TestCycle.StartNight(run), Is.True);
            driver.Rules.AwardMaterial(MapResourceKind.Stone, 125); OpenProfessions();
            Assert.That(Call(binding, "TryUpgradeProfession", 0, 0), Is.True); Window("None");
            var node = Node(MapResourceKind.Stone); Economy.AdvanceGather(node, node.WorldPosition, true, 1f, true);
            var stock = node.Remaining; var xp = driver.Rules.XpWithin(Stage2Profession.Miner);
            for (var i = 0; i < 10 && !run.IsTerminal; i++)
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost)); Assert.That(run.RetryFailedWave(), Is.True);
            Assert.That(driver.Rules.Mastery(Stage2Profession.Miner), Is.EqualTo(5));
            Assert.That(driver.Rules.XpWithin(Stage2Profession.Miner), Is.EqualTo(xp));
            Assert.That(driver.Rules.PowerBonus(Stage2Profession.Miner), Is.EqualTo(1));
            Assert.That(node.Remaining, Is.EqualTo(stock)); Assert.That(Economy.HarvestPower(MapResourceKind.Stone), Is.EqualTo(2));
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(driver.Rules.Mastery(Stage2Profession.Miner), Is.EqualTo(1));
            Assert.That(driver.Rules.XpWithin(Stage2Profession.Miner), Is.Zero);
            Assert.That(driver.Rules.AvailablePoints(Stage2Profession.Miner), Is.Zero);
            Assert.That(Economy.HarvestPower(MapResourceKind.Stone), Is.EqualTo(1));
        }
    }
}
#endif
