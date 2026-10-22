#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroLoadoutPresentationTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Component hud, presentation;
        private int previousRate;
        private static T Read<T>(object owner, string name) => (T)owner.GetType().GetProperty(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);
        private static object Call(object owner, string name, params object[] args) => owner.GetType().GetMethod(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(owner, args);
        private void Window(string name) => Call(hud, "SetWindow", Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType, name));
        [UnitySetUp] public IEnumerator Load()
        {
            previousRate = Time.captureFramerate; Time.captureFramerate = 60; Time.timeScale = 1;
            input = new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 16; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); hud = game.GetComponent("Stage3FantasyHud");
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false;
            game.RunDriver.ResetRun(); yield return null; yield return null;
            Window("None");
            var type = hud.GetType().Assembly.GetType("Not3A.Stage3.VisualPilot.Stage3HeroSkillPresentation");
            presentation = game.GetComponent(type) ?? game.gameObject.AddComponent(type);
            Call(presentation, "Initialize", game);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown(); Time.captureFramerate = previousRate; Time.timeScale = 1;
        }
        private object SetFixture()
        {
            var binding = game.GetComponent("Stage3PlayerInterfaceBinding") as Behaviour;
            if (binding != null) binding.enabled = false;
            var assembly = hud.GetType().Assembly;
            var view = Activator.CreateInstance(assembly.GetType("Not3A.Stage3.PlayerInterface.Stage3HeroLoadoutView"));
            var nodeType = assembly.GetType("Not3A.Stage3.PlayerInterface.Stage3SkillNodeView");
            var nodes = Array.CreateInstance(nodeType, 31);
            var catalog = Resources.Load<Stage2HeroSkillCatalog>("Stage3HeroSkills");
            Assert.That(catalog, Is.Not.Null);
            for (var id = 0; id < 31; id++)
            {
                var node = Activator.CreateInstance(nodeType);
                nodeType.GetField("Id").SetValue(node, id);
                nodeType.GetField("Title").SetValue(node, "Навык " + id);
                nodeType.GetField("RequiredNodeId").SetValue(node, catalog.Get(id).prerequisite);
                nodeType.GetField("IsActive").SetValue(node, id <= 10);
                nodeType.GetField("MaxRank").SetValue(node, id <= 10 ? 4 : 3);
                nodeType.GetField("Weapon").SetValue(node, id == 0 || id >= 11 && id <= 14 ? -1 : id <= 5 || id >= 15 && id <= 22 ? 0 : 1);
                nodes.SetValue(node, id);
            }
            view.GetType().GetField("Nodes").SetValue(view, nodes);
            Call(hud, "SetHeroLoadout", view); return view;
        }
        [Test] public void ElevenSkillSlotsAndFourConsumablesRemainSeparateInsideDock()
        {
            SetFixture(); var dock = Read<Rect>(hud, "BottomDockRect");
            var skills = Enumerable.Range(0, 11).Select(i => (Rect)Call(hud, "SkillSlotRect", i)).ToArray();
            var quick = Enumerable.Range(0, 4).Select(i => (Rect)Call(hud, "QuickSlotRect", i)).ToArray();
            foreach (var r in skills.Concat(quick))
            {
                Assert.That(r.width, Is.GreaterThan(25));
                Assert.That(dock.Contains(r.min) && dock.Contains(r.max), Is.True, r.ToString());
            }
            var all = skills.Concat(quick).ToArray();
            for (var i = 0; i < all.Length; i++) for (var j = i + 1; j < all.Length; j++)
                Assert.That(all[i].Overlaps(all[j]), Is.False, $"overlap {i}/{j}");
            Assert.That(skills[0].center.y, Is.GreaterThan(skills[1].center.y));
            Assert.That(skills[0].center.y, Is.LessThan(skills[6].center.y));
            Assert.That(Read<Rect>(hud, "BlinkSlotRect"), Is.EqualTo(skills[0]));
            for (var i = 0; i < 3; i++) foreach (var r in all)
                Assert.That(r.Overlaps((Rect)Call(hud, "HeroStatusBarRect", i)), Is.False);
        }
        [Test] public void MixedTreeHasDistinctCommonNodesAndSelectionOwnsUpgradeButton()
        {
            SetFixture(); Window("Hero");
            var nodes = Enumerable.Range(0, 31).Select(i => (Rect)Call(hud, "SkillNodeRect", i)).ToArray();
            for (var i = 0; i < nodes.Length; i++)
            {
                Assert.That(nodes[i].width, Is.GreaterThan(0));
                for (var j = i + 1; j < nodes.Length; j++) Assert.That(nodes[i].Overlaps(nodes[j]), Is.False, $"nodes {i}/{j}");
            }
            Assert.That(nodes[0].center.x, Is.LessThan(nodes[1].center.x));
            Assert.That(nodes[1].center.y, Is.LessThan(nodes[6].center.y));
            var scale = Read<float>(hud, "UiScale");
            // Common nodes must have a real horizontal gutter even when vertical bounds almost meet.
            Assert.That(nodes[1].xMin - nodes[12].xMax, Is.GreaterThanOrEqualTo(8f * scale));
            Assert.That(nodes[6].xMin - nodes[14].xMax, Is.GreaterThanOrEqualTo(8f * scale));
            for (var id = 11; id <= 14; id++) Assert.That(nodes[id].center.x, Is.LessThan(nodes[1].center.x));
            Call(hud, "SelectSkill", 9);
            Assert.That(Read<int>(hud, "SelectedSkillId"), Is.EqualTo(9));
            Assert.That(Read<Rect>(hud, "SkillUpgradeRect").width, Is.GreaterThan(0));
            Assert.That(Read<Rect>(hud, "BlinkUpgradeRect"), Is.EqualTo(Rect.zero));
            Call(hud, "SelectSkill", 0);
            Assert.That(Read<Rect>(hud, "BlinkUpgradeRect"), Is.EqualTo(Read<Rect>(hud, "SkillUpgradeRect")));
            Call(hud, "ShowProfessions", true);
            Assert.That(Read<Rect>(hud, "SkillUpgradeRect"), Is.EqualTo(Rect.zero));
            Assert.That((Rect)Call(hud, "SkillNodeRect", 0), Is.EqualTo(Rect.zero));
        }
        [UnityTest] public IEnumerator ActualTreeCaptionBandsDoNotIntersectNodesOrOtherCaptions()
        {
            SetFixture(); Window("Hero");
            var originalWidth = Screen.width; var originalHeight = Screen.height;
            var originalMode = Screen.fullScreenMode;
            try
            {
                foreach (var size in new[] {new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080)})
                {
                    Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                    yield return null; yield return null; yield return null;
                    Debug.Log($"Caption geometry viewport: requested {size.x}x{size.y}, actual {Screen.width}x{Screen.height}");
                    if (Screen.width != size.x || Screen.height != size.y)
                        Assert.Ignore($"Batch Editor GameView cannot apply required viewport {size.x}x{size.y}; actual {Screen.width}x{Screen.height}. " +
                            "Caption assertions require the qualified720/900/1080 native viewports. See native-hud-final frames and Astra review JSON; no smaller viewport is claimed.");
                    AssertActualCaptionGeometry();
                }
            }
            finally { Screen.SetResolution(originalWidth, originalHeight, originalMode); }
        }

        private void AssertActualCaptionGeometry()
        {
            var nodes = Enumerable.Range(0, 31).Select(i => (Rect)Call(hud, "SkillNodeRect", i)).ToArray();
            var captions = Enumerable.Range(0, 11).Select(i => (Rect)Call(hud, "SkillNodeLabelRect", i)).ToArray();
            var graph = Read<Rect>(hud, "SkillTreeRect"); var scale = Read<float>(hud, "UiScale");
            for (var i = 0; i < captions.Length; i++)
            {
                Assert.That(graph.Contains(captions[i].min) && graph.Contains(captions[i].max), Is.True, $"caption outside graph {i}");
                for (var j = 0; j < nodes.Length; j++)
                    Assert.That(captions[i].Overlaps(nodes[j]), Is.False, $"caption {i} / node {j}");
                for (var j = i + 1; j < captions.Length; j++)
                    Assert.That(captions[i].Overlaps(captions[j]), Is.False, $"captions {i}/{j}");
            }
            // The formerly split single-word Focus title has a full column width.
            Assert.That(captions[4].width, Is.GreaterThanOrEqualTo(180f * scale));
            // All four actual Bolt passives have clear space above or below its caption.
            foreach (var id in new[] {23, 24, 29, 30})
                Assert.That(nodes[id].yMax < captions[6].yMin || nodes[id].yMin > captions[6].yMax, Is.True);
        }

        [Test] public void SkillAtlasUsesDistinctMeasuredBoundsRatherThanClippedUniformRows()
        {
            var texture = Resources.Load<Texture2D>("Stage3HeroSkillIcons");
            Assert.That(texture, Is.Not.Null);
            Assert.That(texture.width, Is.EqualTo(1774)); Assert.That(texture.height, Is.EqualTo(887));
            var method = hud.GetType().GetMethod("SkillIconUv", BindingFlags.Public | BindingFlags.Static);
            var boxes = Enumerable.Range(0, 32).Select(i => (Rect)method.Invoke(null, new object[] { i })).ToArray();
            for (var i = 0; i < boxes.Length; i++)
            {
                Assert.That(boxes[i].xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(boxes[i].yMin, Is.GreaterThanOrEqualTo(0));
                Assert.That(boxes[i].xMax, Is.LessThanOrEqualTo(1)); Assert.That(boxes[i].yMax, Is.LessThanOrEqualTo(1));
                for (var j = i + 1; j < boxes.Length; j++) Assert.That(boxes[i].Overlaps(boxes[j]), Is.False);
            }
            // First-row full silhouettes cross the mathematical quarter boundary in the authored source.
            Assert.That(boxes[0].yMin, Is.LessThan(.75f));
        }
        [Test] public void StaffProjectileAppearanceIsCapturedAtReleaseForPortalAndGuildShots()
        {
            var combat = game.GetComponent("Stage3CombatPresentation");
            var equipment = game.GetComponent<Stage2EquipmentDriver>().Rules;
            Assert.That(equipment.TryReceiveLoot(Stage2ItemType.Wand, Stage2ItemQuality.F, 1, out var staff), Is.True);
            Assert.That(equipment.TryEquip(staff.Id), Is.EqualTo(Stage2EquipmentResult.Ready));
            var origin = (Vector2)game.GetComponent<Stage2SceneBootstrap>().PlayerMover.transform.position;
            var enemy = game.Enemies.Spawn(); enemy.SetAutomaticTick(false); enemy.transform.position = origin + Vector2.right * 2;
            var health = enemy.Health;
            Assert.That(game.Enemies.TryHitNearest(origin, 5, 1, 0, .5f), Is.True);
            Assert.That(Read<int>(combat, "ActiveStaffFlights"), Is.EqualTo(1));
            Call(combat, "LaunchGuildShot", origin, origin + Vector2.right * 2, .5f);
            Assert.That(Read<int>(combat, "ActiveGuildStaffFlights"), Is.EqualTo(1));
            Assert.That(enemy.Health, Is.EqualTo(health), "presentation must not deliver a delayed shot early");
            Assert.That(equipment.TrySwitchWeapon(), Is.True);
            Assert.That(game.Enemies.TryHitNearest(origin, 5, 1, 0, .5f), Is.True);
            Call(combat, "LaunchGuildShot", origin, origin + Vector2.right * 2, .5f);
            Assert.That(Read<int>(combat, "ActiveStaffFlights"), Is.EqualTo(1), "existing magic flight keeps its launch appearance after switching");
            Assert.That(Read<int>(combat, "ActiveGuildStaffFlights"), Is.EqualTo(1));
            Assert.That(Read<int>(combat, "ActiveGuildFlights"), Is.EqualTo(2));
            Assert.That(game.Enemies.PendingShotCount, Is.EqualTo(2));
            Assert.That(enemy.Health, Is.EqualTo(health));
            game.Enemies.TickShots(.5f);
            Assert.That(enemy.Health, Is.EqualTo(health - 2));
        }
        [UnityTest] public IEnumerator SkillImpactsAreBoundedIgnoreMalformedInputAndClearWithRunReset()
        {
            Call(presentation, "PresentSkillImpact", 9, new Vector3(float.NaN, 0, 0), 1f);
            Call(presentation, "PresentSkillImpact", 50, Vector3.zero, 1f);
            Assert.That(Read<int>(presentation, "ActiveImpactCount"), Is.Zero);
            for (var i = 0; i < 30; i++) Call(presentation, "PresentSkillImpact", 9, Vector3.zero, 1.5f);
            Assert.That(Read<int>(presentation, "ActiveImpactCount"), Is.EqualTo(12));
            Call(presentation, "SetTargetPreview", 9, Vector3.zero, 1.5f, true);
            Assert.That(Read<float>(presentation, "PreviewRadius"), Is.EqualTo(1.5f));
            Assert.That(Read<bool>(presentation, "HasTargetPreview"), Is.True);
            game.RunDriver.ResetRun();
            Assert.That(Read<int>(presentation, "ActiveImpactCount"), Is.Zero);
            Assert.That(Read<bool>(presentation, "HasTargetPreview"), Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator ModalRejectsNewImpactsPauseFreezesAndDisableClears()
        {
            Call(presentation, "PresentSkillImpact", 10, Vector3.zero, 1f);
            game.RunDriver.Run.SetPaused(true);
            for (var i = 0; i < 40; i++) yield return null;
            Assert.That(Read<int>(presentation, "ActiveImpactCount"), Is.EqualTo(1));
            game.RunDriver.Run.SetPaused(false); Window("Hero");
            Call(presentation, "PresentSkillImpact", 9, Vector3.zero, 1f);
            for (var i = 0; i < 40; i++) yield return null;
            Assert.That(Read<int>(presentation, "ActiveImpactCount"), Is.EqualTo(1));
            Window("None");
            for (var i = 0; i < 40; i++) yield return null;
            Assert.That(Read<int>(presentation, "ActiveImpactCount"), Is.Zero);
            Call(presentation, "SetTargetPreview", 10, Vector3.zero, 2f, true);
            ((Behaviour)presentation).enabled = false;
            Assert.That(Read<bool>(presentation, "HasTargetPreview"), Is.False);
        }
    }
}
#endif
