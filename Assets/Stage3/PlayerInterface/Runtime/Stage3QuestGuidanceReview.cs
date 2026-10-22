using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit development-player fixture for accepted quest navigation; never runs on ordinary startup.</summary>
    public static class Stage3QuestGuidanceReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            for (var i = 0; i < 20; i++) yield return null;
            var binding = owner.GetComponent<Stage3PlayerInterfaceBinding>();
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var game = owner.GetComponent<Stage2GameComposition>();
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hero = owner.GetComponent<Stage2HeroProgressionDriver>();
            var travel = owner.GetComponent<Stage3WorldTravel>();
            var world = binding.GuildWorld;
            Require(world != null && world.Ready, "guild ready");
            game.RunDriver.enabled = false; hero.AutomaticTick = false;
            game.HeroCombat.AutomaticTick = false; world.AutomaticTick = false;
            game.RunDriver.ResetRun(); game.SetPaused(false); owner.GetComponent<Stage2GameHud>().SetDebugOpen(false);
            yield return null;
            var start = (Vector2)scene.PlayerMover.transform.position;
            var board = travel.CityPresentation.GuildInteractionPosition;
            Move(scene, hero, board + Vector2.down);
            Require(binding.TryOpenGuild(), "local guild");
            hud.SelectGuildQuest(1); Require(binding.TryGuildAction(1), "orc acceptance");
            hud.SelectGuildQuest(2); Require(binding.TryGuildAction(2), "ogre acceptance"); hud.CloseWindow();
            var frames = new List<string>();
            var sizes = new[] { new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) };
            foreach (var size in sizes)
            {
                Screen.SetResolution(size.x, size.y, false); yield return new WaitForSecondsRealtime(.3f);
                binding.TrackGuildQuest(1); Move(scene, hero, start);
                hud.SetWindow(Stage3PlayerWindow.Map); hud.ZoomMap(.001f); hud.RecenterMap();
                yield return Shot(directory, size + "-map-two-orders-orc-selected", hud, frames);
                Require(hud.GuildQuestMapPinCount == 2, "both accepted pins");
                binding.TrackGuildQuest(2);
                yield return Shot(directory, size + "-map-two-orders-ogre-selected", hud, frames);
                hud.CloseWindow();
                Require(binding.TryGetTrackedGuildTarget(out var target, out _), "accepted target");
                var map = scene.MapDefinition;
                var cell = map.WorldToCellCoordinates(target); var min = (Vector2)map.MinimumCell;
                var far = min + new Vector2(cell.x - min.x > map.Size.x * .5f ? 5 : map.Size.x - 5,
                    cell.y - min.y > map.Size.y * .5f ? 5 : map.Size.y - 5);
                Move(scene, hero, map.CellCoordinatesToWorld(far));
                yield return Shot(directory, size + "-minimap-far-bearing", hud, frames);
                Require(hud.IsGuildQuestMinimapOffscreen && hud.GuildQuestMinimapArrowRect.width > 0, "far accepted arrow");
                var bearingCells = map.WorldToCellCoordinates(target) - map.WorldToCellCoordinates(scene.PlayerMover.transform.position);
                Require(Vector2.Dot(new Vector2(bearingCells.x, -bearingCells.y).normalized, hud.GuildQuestMinimapDirection) > .999f, "native projected bearing");
                Require(!hud.IsCityMinimapOffscreen || hud.CityMinimapMarkerRect == Rect.zero, "no distant city cue");
                Move(scene, hero, target + Vector2.down * 4);
                yield return Shot(directory, size + "-minimap-local-parchment", hud, frames);
                Require(!hud.IsGuildQuestMinimapOffscreen && hud.GuildQuestMinimapMarkerRect.width > 0, "local accepted paper");
            }
            foreach (var id in new[] { 1, 2 })
            {
                var contract = binding.Guild.FindActive(id);
                foreach (var enemy in world.Enemies.Where(e => e.Alive && e.Generation == contract.Generation && e.Profile.enemyKind == contract.Quest.enemyKind).ToArray())
                    Require(world.Damage(enemy, enemy.Health), "confirmed clear");
            }
            binding.TrackGuildQuest(1);
            foreach (var size in sizes)
            {
                Screen.SetResolution(size.x, size.y, false); yield return new WaitForSecondsRealtime(.3f);
                hud.CloseWindow(); Move(scene, hero, start);
                yield return Shot(directory, size + "-minimap-guild-turn-in-bearing", hud, frames);
                hud.SetWindow(Stage3PlayerWindow.Map); hud.ZoomMap(.001f); hud.RecenterMap();
                yield return Shot(directory, size + "-map-two-guild-checks", hud, frames);
                Require(hud.GuildQuestMapPinCount == 2 && hud.IsGuildQuestTurnIn, "both completed orders remain selectable");
                Require(!hud.GuildQuestMapMarkerRect(1).Overlaps(hud.CityLargeMapMarkerRect) &&
                    !hud.GuildQuestMapMarkerRect(2).Overlaps(hud.CityLargeMapMarkerRect), "guild checks keep city house readable");
                hud.CloseWindow(); Move(scene, hero, board + Vector2.down);
                yield return Shot(directory, size + "-minimap-guild-check-near-hero", hud, frames);
            }
            Require(binding.TryOpenGuild(), "turnin window"); hud.SelectGuildQuest(1);
            Require(binding.TryGuildAction(1), "first turnin");
            Require(binding.TrackedGuildQuestId == 2, "next active target");
            hud.SelectGuildQuest(2); Require(binding.TryGuildAction(2), "last turnin"); hud.CloseWindow();
            yield return Shot(directory, "no-orders-no-quest-arrow", hud, frames);
            Require(hud.GuildQuestMinimapArrowRect == Rect.zero && hud.GuildQuestMinimapMarkerRect == Rect.zero, "no stale guidance");
            // Leave an explicit real-input fixture after the no-orders assertion.
            Require(binding.TryOpenGuild(), "input fixture board");
            hud.SelectGuildQuest(1); Require(binding.TryGuildAction(1), "input fixture orc");
            hud.SelectGuildQuest(2); Require(binding.TryGuildAction(2), "input fixture ogre");
            hud.SetWindow(Stage3PlayerWindow.Map); hud.ZoomMap(.001f); hud.RecenterMap();
            yield return Shot(directory, "input-ready-two-quest-pins", hud, frames);
            var heroBeforePointer = scene.PlayerMover.transform.position;
            var firstPin = hud.GuildQuestMapMarkerRect(1).center;
            var secondPin = hud.GuildQuestMapMarkerRect(2).center;
            Pointer(hud, EventType.MouseDown, firstPin); Pointer(hud, EventType.MouseUp, firstPin);
            Require(binding.TrackedGuildQuestId == 1, "production pointer handler selects clicked order");
            Pointer(hud, EventType.MouseDown, secondPin);
            Pointer(hud, EventType.MouseDrag, secondPin + Vector2.right * 20f * hud.UiScale);
            Pointer(hud, EventType.MouseUp, secondPin);
            Require(binding.TrackedGuildQuestId == 1, "production pointer handler ignores dragged order");
            Pointer(hud, EventType.MouseDown, secondPin); Pointer(hud, EventType.MouseUp, secondPin);
            Require(binding.TrackedGuildQuestId == 2, "production pointer handler selects second order");
            Require(scene.PlayerMover.transform.position == heroBeforePointer && binding.Guild.Active.Count == 2,
                "pointer selection never moves hero or changes contracts");
            File.WriteAllText(Path.Combine(directory, "scripted-pointer.txt"),
                "PASS: production HandleGuildMapPointer consumed synthetic Unity Event down/up/drag sequences using actual native-drawn marker rectangles. Click1 selects1; drag2 keeps1; click2 selects2; hero/contracts unchanged. This is an internal Unity event-path check, NOT physical OS mouse input or full pan-dispatch qualification. OS input remains separately required.\n");
            File.WriteAllText(Path.Combine(directory, "input-ready.txt"),
                $"Native map input fixture. Current tracked order={binding.TrackedGuildQuestId}. Click order1 top-left-origin rect {hud.GuildQuestMapMarkerRect(1)}; ivory bearing should rotate from ogres to orcs without hero movement. Order2 rect {hud.GuildQuestMapMarkerRect(2)}. Drag >6px starting on order2 and release: keep order1 selected, pan normally. A later simple click order2 selects it.\n");
            File.WriteAllText(Path.Combine(directory, "frames.txt"), string.Join("\n", frames));
            File.WriteAllText(Path.Combine(directory, "qualification.txt"),
                "Native development-player captures, 720/900/1080. Real guild acceptance, confirmed kills, local turn-ins; fixture freezes clock/bow/enemy simulation and legally teleports hero for appearance coverage. Programmatic tracking verifies appearance; actual mouse pin selection is separate input qualification. No route, discovery or travel mutation. Ordinary startup never runs this fixture.\n");
            Debug.Log("S3_QUEST_GUIDANCE_REVIEW_READY " + directory);
        }
        private static void Move(Stage2SceneBootstrap scene, Stage2HeroProgressionDriver hero, Vector2 target)
        {
            var p = target;
            if (!hero.CanLand(p))
            {
                var found = false;
                for (var ring = .25f; ring <= 12 && !found; ring += .25f)
                    for (var n = 0; n < 32; n++)
                    {
                        var angle = n * Mathf.PI / 16;
                        var candidate = target + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring;
                        if (!hero.CanLand(candidate)) continue;
                        p = candidate; found = true; break;
                    }
                Require(found, "legal capture footing");
            }
            Require(scene.PlayerMover.TryTeleportTo(p), "capture teleport");
            scene.CameraFollow.ConfigureTacticalView(8); scene.CameraFollow.SnapToTarget();
        }
        private static IEnumerator Shot(string directory, string name, Stage3FantasyHud hud, List<string> frames)
        {
            hud.GetComponent<Stage3HeroLevelUpPresentation>()?.ClearEffects();
            hud.GetComponent<Stage3HeroProgressionEffects>()?.ClearEffects(); hud.ClearExperienceFeedback();
            for (var i = 0; i < 4; i++) yield return null;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); frames.Add(name);
            yield return null;
        }
        private static void Pointer(Stage3FantasyHud hud, EventType type, Vector2 position)
        {
            var saved = Event.current;
            try
            {
                Event.current = new Event { type = type, button = 0, mousePosition = position };
                typeof(Stage3FantasyHud).GetMethod("HandleGuildMapPointer", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            }
            finally { Event.current = saved; }
        }
        private static void Require(bool value, string message)
        { if (!value) throw new InvalidOperationException("Quest guidance QA: " + message); }
    }
}
