#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
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
    public sealed class Stage3QuestGuidanceTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component binding, hud, world, travel, resources;
        private Stage2Guild guild;
        private static T Read<T>(object o, string p) => (T)o.GetType().GetProperty(p).GetValue(o);
        private static object Call(object o, string method, params object[] args) => o.GetType().GetMethod(method).Invoke(o, args);
        private static void Set(object o, string property, object value) => o.GetType().GetProperty(property).SetValue(o, value);
        private Vector2 Board => Read<Vector2>(Read<object>(travel, "CityPresentation"), "GuildInteractionPosition");
        private IEnumerable<Stage2GuildEnemy> Units => Read<IReadOnlyList<Stage2GuildEnemy>>(world, "Enemies");
        [UnitySetUp] public IEnumerator Setup()
        {
            input = new InputTestFixture(); input.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>(); Time.timeScale = 1;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity", new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 20; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>(); scene = game.GetComponent<Stage2SceneBootstrap>();
            binding = game.GetComponent("Stage3PlayerInterfaceBinding"); hud = game.GetComponent("Stage3FantasyHud");
            world = game.GetComponent("Stage3GuildWorldDriver"); travel = game.GetComponent("Stage3WorldTravel");
            resources = game.GetComponent("Stage3ResourceDistrictWorld"); guild = Read<Stage2Guild>(binding, "Guild");
            Assert.That(Read<bool>(world, "Ready"), Is.True);
            Set(world, "AutomaticTick", false); game.HeroCombat.AutomaticTick = false; game.RunDriver.enabled = false;
            game.GetComponent<Stage2HeroProgressionDriver>().AutomaticTick = false;
            game.RunDriver.ResetRun(); game.SetPaused(false); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale = 1;
        }
        private void Accept(int id)
        {
            Call(hud, "CloseWindow"); scene.PlayerMover.transform.position = Board + Vector2.down;
            Assert.That(Call(binding, "TryOpenGuild"), Is.True);
            Call(hud, "SelectGuildQuest", id); Assert.That(Call(binding, "TryGuildAction", id), Is.True);
            Call(hud, "CloseWindow");
        }
        private bool Target(out Vector2 target, out bool turnIn)
        {
            object[] args = { Vector2.zero, false };
            var valid = (bool)Call(binding, "TryGetTrackedGuildTarget", args);
            target = (Vector2)args[0]; turnIn = (bool)args[1]; return valid;
        }
        private void Clear(int id)
        {
            var contract = guild.FindActive(id);
            foreach (var enemy in Units.Where(u => u.Alive && u.Generation == contract.Generation && u.Profile.enemyKind == contract.Quest.enemyKind).ToArray())
                Assert.That(Call(world, "Damage", enemy, enemy.Health), Is.True);
        }
        private void OpenMap()
        {
            var windowType = hud.GetType().GetProperty("ActiveWindow").PropertyType;
            Call(hud, "SetWindow", Enum.Parse(windowType, "Map")); Call(hud, "ZoomMap", .001f); Call(hud, "RecenterMap");
        }
        [UnityTest] public IEnumerator AcceptanceTracksLatestClearReturnsToGuildTurnInSelectsRemainingAndResetRemovesTarget()
        {
            Assert.That(Target(out _, out _), Is.False);
            Assert.That(Call(binding, "TrackGuildQuest", 1), Is.False, "An unaccepted order cannot create a navigation target.");
            Accept(1); Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(1));
            Accept(2); Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(2));
            Assert.That(Call(binding, "TrackGuildQuest", 1), Is.True);
            Assert.That(Target(out var camp, out var turnIn), Is.True); Assert.That(turnIn, Is.False);
            Assert.That(Vector2.Distance(camp, Board), Is.GreaterThan(10));
            Clear(1); Assert.That(Target(out var guildTarget, out turnIn), Is.True);
            Assert.That(turnIn, Is.True); Assert.That(Vector2.Distance(guildTarget, Board), Is.LessThan(.001f));
            scene.PlayerMover.transform.position = Board + Vector2.down;
            Assert.That(Call(binding, "TryOpenGuild"), Is.True); Call(hud, "SelectGuildQuest", 1);
            Assert.That(Call(binding, "TryGuildAction", 1), Is.True);
            Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(2));
            Assert.That(Target(out _, out turnIn), Is.True); Assert.That(turnIn, Is.False);
            Clear(2); Call(hud, "SelectGuildQuest", 2); Assert.That(Call(binding, "TryGuildAction", 2), Is.True);
            Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(-1)); Assert.That(Target(out _, out _), Is.False);
            Accept(1); game.RunDriver.ResetRun(); yield return null;
            Assert.That(Target(out _, out _), Is.False); Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(-1));
        }
        [UnityTest] public IEnumerator RetryAndWorldRegenerationKeepSelectedContractAndResolveItsCurrentLiveTarget()
        {
            Accept(1); Accept(2); Assert.That(Call(binding, "TrackGuildQuest", 1), Is.True);
            var contract = guild.FindActive(1);
            var victim = Units.First(u => u.Alive && u.Generation == contract.Generation && u.Profile.enemyKind == contract.Quest.enemyKind);
            Assert.That(Call(world, "Damage", victim, victim.Health), Is.True); var remaining = contract.Remaining;
            game.RunDriver.Run.StartNextWaveForTesting();
            for (var n = 0; n < game.RunDriver.Run.ThroneMaximum; n++)
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            Assert.That(game.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(game.RunDriver.Run.RetryFailedWave(), Is.True);
            Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(1));
            Assert.That(guild.FindActive(1), Is.SameAs(contract)); Assert.That(contract.Remaining, Is.EqualTo(remaining));
            Call(resources, "Regenerate", 37621); for (var i = 0; i < 4; i++) yield return null;
            Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(1));
            Assert.That(Target(out var target, out var turnIn), Is.True); Assert.That(turnIn, Is.False);
            var homes = Units.Where(u => u.Alive && u.Generation == contract.Generation && u.Profile.enemyKind == contract.Quest.enemyKind).Select(u => u.Home).ToArray();
            var mean = homes.Aggregate(Vector2.zero, (sum, p) => sum + p) / homes.Length;
            Assert.That(Vector2.Distance(target, mean), Is.LessThan(.001f), "Guidance must re-resolve the relocated pack.");
            Assert.That(contract.Remaining, Is.EqualTo(remaining));
        }
        [UnityTest] public IEnumerator ActualMapPinsSelectOneContractAndDraggingPinDoesNotChangeTracking()
        {
            if (Application.isBatchMode)
                Assert.Ignore("Native IMGUI draw and pointer geometry require a rendered player/Game view; covered by Stage3QuestGuidanceReview and separate native pointer qualification, not a batch draw PASS.");
            Accept(1); Accept(2); OpenMap();
            for (var i = 0; i < 30 && Read<int>(hud, "GuildQuestMapPinCount") != 2; i++) yield return null;
            Assert.That(Read<int>(hud, "GuildQuestMapPinCount"), Is.EqualTo(2));
            var first = (Rect)Call(hud, "GuildQuestMapMarkerRect", 1);
            var second = (Rect)Call(hud, "GuildQuestMapMarkerRect", 2);
            Assert.That(first.width, Is.GreaterThan(0)); Assert.That(second.width, Is.GreaterThan(0)); Assert.That(first.Overlaps(second), Is.False);
            var viewport = Read<Rect>(hud, "MapViewportRect"); Assert.That(viewport.Contains(first.center), Is.True);
            var heroBefore = scene.PlayerMover.transform.position;
            MapPointer(EventType.MouseDown, first.center); MapPointer(EventType.MouseUp, first.center);
            Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(1));
            MapPointer(EventType.MouseDown, second.center); MapPointer(EventType.MouseDrag, second.center + Vector2.right * 20);
            MapPointer(EventType.MouseUp, second.center);
            Assert.That(Read<int>(binding, "TrackedGuildQuestId"), Is.EqualTo(1), "A pan beginning on a quest pin must not select it.");
            Assert.That(scene.PlayerMover.transform.position, Is.EqualTo(heroBefore), "Map selection never teleports or autopaths.");
            Assert.That(guild.Active.Count, Is.EqualTo(2));
        }
        private void MapPointer(EventType type, Vector2 point)
        {
            var saved = Event.current;
            try
            {
                Event.current = new Event { type = type, button = 0, mousePosition = point };
                hud.GetType().GetMethod("HandleGuildMapPointer", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            }
            finally { Event.current = saved; }
        }
        [UnityTest] public IEnumerator MinimapBearingMatchesTargetProjectionLocalPinReplacesArrowAndNoDistantCityCue()
        {
            if (Application.isBatchMode)
                Assert.Ignore("Native minimap marker/bearing metadata is produced by OnGUI; covered by Stage3QuestGuidanceReview native frames, not a batch draw PASS.");
            Accept(1); Assert.That(Target(out var target, out _), Is.True);
            // Opposite map corner ensures the accepted target and city are outside the local view.
            var targetCell = scene.MapDefinition.WorldToCellCoordinates(target);
            var min = (Vector2)scene.MapDefinition.MinimumCell;
            var size = (Vector2)scene.MapDefinition.Size;
            var farCell = min + new Vector2(targetCell.x - min.x > size.x * .5f ? 3 : size.x - 3,
                targetCell.y - min.y > size.y * .5f ? 3 : size.y - 3);
            scene.PlayerMover.transform.position = scene.MapDefinition.CellCoordinatesToWorld(farCell);
            for (var i = 0; i < 6; i++) yield return null;
            Assert.That(Read<bool>(hud, "IsGuildQuestMinimapOffscreen"), Is.True);
            Assert.That(Read<Rect>(hud, "GuildQuestMinimapArrowRect").width, Is.GreaterThan(0));
            Assert.That(Read<Rect>(hud, "GuildQuestMinimapMarkerRect"), Is.EqualTo(Rect.zero));
            var delta = targetCell - scene.MapDefinition.WorldToCellCoordinates(scene.PlayerMover.transform.position);
            var expected = new Vector2(delta.x, -delta.y).normalized;
            Assert.That(Vector2.Dot(expected, Read<Vector2>(hud, "GuildQuestMinimapDirection")), Is.GreaterThan(.999f));
            Assert.That(Read<Rect>(hud, "CityMinimapMarkerRect"), Is.EqualTo(Rect.zero), "Accepted quest guidance does not restore D113's removed far-city cue.");
            scene.PlayerMover.transform.position = target + Vector2.down * 2;
            for (var i = 0; i < 6; i++) yield return null;
            Assert.That(Read<bool>(hud, "IsGuildQuestMinimapOffscreen"), Is.False);
            Assert.That(Read<Rect>(hud, "GuildQuestMinimapMarkerRect").width, Is.GreaterThan(0));
            Assert.That(Read<Rect>(hud, "GuildQuestMinimapArrowRect"), Is.EqualTo(Rect.zero));
            game.RunDriver.ResetRun(); for (var i = 0; i < 4; i++) yield return null;
            Assert.That(Read<Rect>(hud, "GuildQuestMinimapMarkerRect"), Is.EqualTo(Rect.zero));
            Assert.That(Read<Rect>(hud, "GuildQuestMinimapArrowRect"), Is.EqualTo(Rect.zero));
        }
    }
}
#endif
