using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Opt-in development-player QA. Uses real model facts, keyboard bindings and backbuffer.</summary>
    public static class Stage3ManualResourceReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            var game = owner.GetComponent<Stage2GameComposition>();
            while (game == null || !game.IsInitialized || game.Gathering.Economy.Nodes.Count <= 3)
                yield return null;
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var actor = scene.PlayerMover.transform;
            var avatar = actor.GetComponent<Stage3HeroAvatar>();
            while (avatar == null || avatar.Body?.sprite == null) { yield return null; avatar = actor.GetComponent<Stage3HeroAvatar>(); }
            var economy = game.Gathering.Economy;
            var camera = scene.CameraFollow.BoundCamera;
            var priorHero = actor.position; var priorCamera = camera.transform.position;
            var priorZoom = camera.orthographicSize; var priorFollow = scene.CameraFollow.enabled;
            var priorCaptureRate = Time.captureFramerate;
            var priorBackground = InputSystem.settings.backgroundBehavior;
            var priorCombat = game.HeroCombat.AutomaticTick;
            var keyboard = InputSystem.AddDevice<Keyboard>("Manual resource QA keyboard");
            Directory.CreateDirectory(directory);
            var context = new StringBuilder($"Actual development-player Game backbuffer {Screen.width}x{Screen.height}; {SystemInfo.graphicsDeviceType}.\n" +
                "Opt-in disposable QA: synthetic Input System keyboard for real E/WASD bindings; model strikes for stock-stage setup. No physical input verification.\n" +
                "Temporarily frames camera and hero; restores state and resets the disposable run afterward. No sprite/frame override.\n");
            try
            {
                Time.captureFramerate = 60;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                scene.CameraFollow.enabled = false; game.HeroCombat.AutomaticTick = false;
                game.SetPaused(true);
                var groups = economy.Nodes.Where(n => n.Resource == MapResourceKind.Wood && n.GroupId >= 0).GroupBy(n => n.GroupId).ToArray();
                foreach (var count in new[] { 1, 4, 8 })
                {
                    var group = groups.FirstOrDefault(g => g.Count() == count);
                    if (group == null) continue;
                    var center = group.Aggregate(Vector2.zero, (p, n) => p + n.WorldPosition) / count;
                    Frame(camera, center, 5f); yield return Shot(directory, "trees-natural-" + count);
                    context.AppendLine($"Tree group {group.Key}: {count} separate nodes, positions=" + string.Join(";", group.Select(n => n.WorldPosition.ToString("F3"))));
                }
                foreach (var kind in new[] { MapResourceKind.Stone, MapResourceKind.Metal })
                foreach (var size in new[] { Stage2ResourceSize.Small, Stage2ResourceSize.Medium, Stage2ResourceSize.Large })
                {
                    var node = economy.Nodes.First(n => n.Resource == kind && n.Size == size);
                    Frame(camera, node.WorldPosition, size == Stage2ResourceSize.Large ? 5f : 2.5f);
                    foreach (var fill in new[] { 1f, .5f, .12f })
                    {
                        var remaining = Mathf.Max(1, Mathf.CeilToInt(node.Capacity * fill));
                        while (node.Remaining > remaining)
                            economy.AdvanceGather(node, node.WorldPosition, true, node.GatherSeconds, true);
                        economy.CancelGather();
                        yield return Shot(directory, $"{kind}-{size}-remaining-{remaining}");
                    }
                }
                game.RunDriver.ResetRun(); game.SetPaused(true);
                foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                {
                    var node = economy.Nodes.First(n => n.Resource == kind &&
                        (kind == MapResourceKind.Wood ? n.GroupId >= 0 : n.Size == Stage2ResourceSize.Medium));
                    actor.position = Approach(economy, node);
                    Frame(camera, (node.WorldPosition + (Vector2)actor.position) * .5f, 2.5f);
                    game.SetPaused(false);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                    var seen = new HashSet<int>();
                    for (var frame = 0; frame < 190 && seen.Count < 6; frame++)
                    {
                        yield return new WaitForEndOfFrame();
                        if (!avatar.IsHarvesting || !seen.Add(avatar.HarvestFrame)) continue;
                        ScreenCapture.CaptureScreenshot(Path.Combine(directory, $"work-{kind}-pose-{avatar.HarvestFrame}.png"));
                    }
                    context.AppendLine($"{kind} E: observed poses={string.Join(",",seen.OrderBy(x=>x))}; paid={economy.Inventory.Get(kind)}; stock={node.Remaining}/{node.Capacity}; contacts={avatar.ConfirmedStrikes}.");
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                    game.RunDriver.ResetRun(); game.SetPaused(true);
                }
                // Four actual held-E approaches, with full and low mineral stock.
                foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                foreach (var low in new[] { false, true })
                foreach (var direction in new[] { 0, 1, 2, 3 })
                {
                    if (kind == MapResourceKind.Wood && low) continue;
                    game.RunDriver.ResetRun(); game.SetPaused(true);
                    var node = economy.Nodes.First(n => n.Resource == kind && (kind == MapResourceKind.Wood
                        ? n.GroupId >= 0 && economy.Nodes.Count(t => t.GroupId == n.GroupId && t.Resource == kind) == 1
                        : n.Size == Stage2ResourceSize.Medium));
                    if (low)
                    {
                        while (node.Remaining > Mathf.CeilToInt(node.Capacity * .12f))
                            economy.AdvanceGather(node, node.WorldPosition, true, node.GatherSeconds, true);
                        economy.CancelGather();
                    }
                    var targetDirection = direction == 0 ? Vector2.right : direction == 1 ? Vector2.up :
                        direction == 2 ? Vector2.left : Vector2.down;
                    actor.position = node.WorldPosition - targetDirection * (node.InteractionRadius + (kind == MapResourceKind.Wood ? .5f : .25f));
                    Frame(camera, (node.WorldPosition + (Vector2)actor.position) * .5f, 3.5f);
                    game.SetPaused(false);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    for (var i = 0; i < 20; i++) yield return null;
                    var contacts = avatar.ConfirmedStrikes;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                    var captured = false;
                    for (var i = 0; i < 120; i++)
                    {
                        yield return new WaitForEndOfFrame();
                        if (avatar.ConfirmedStrikes <= contacts || avatar.HarvestFrame != 3) continue;
                        ScreenCapture.CaptureScreenshot(Path.Combine(directory, $"contact-{kind}-dir-{direction}-{(low ? "low" : "full")}.png"));
                        captured = true; break;
                    }
                    context.AppendLine($"Contact {kind} direction={direction} low={low}: captured={captured}; stock={node.Remaining}/{node.Capacity}.");
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                }
                var walkStart = WalkStart(scene, economy);
                foreach (var key in new[] { Key.D, Key.W, Key.A, Key.S })
                {
                    actor.position = walkStart;
                    game.SetPaused(false);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                    var seen = new HashSet<int>();
                    var sequence = key == Key.D || key == Key.W;
                    var sequenceDirectory = Path.Combine(directory, "hero-" + key + "-sequence");
                    if (sequence) Directory.CreateDirectory(sequenceDirectory);
                    for (var frame = 0; frame < 120 && (seen.Count < avatar.WalkFrameCount || sequence && frame < 60); frame++)
                    {
                        Frame(camera, actor.position, sequence ? 5f : 2.5f);
                        yield return new WaitForEndOfFrame();
                        if (sequence && frame < 60) SaveFrame(sequenceDirectory, frame);
                        if (!avatar.IsMoving || !seen.Add(avatar.Frame)) continue;
                        ScreenCapture.CaptureScreenshot(Path.Combine(directory, $"hero-walk-{key}-pose-{avatar.Frame}.png"));
                    }
                    context.AppendLine($"Hero {key}: observed {seen.Count} phases; frameCount={avatar.WalkFrameCount}; normal speed={scene.PlayerMover.Speed}.");
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                    game.SetPaused(true);
                }
                game.RunDriver.ResetRun(); game.SetPaused(false); game.RunDriver.Run.StartWave();
                for (var i = 0; i < 600 && game.Enemies.ActiveCount == 0; i++) yield return null;
                if (game.Enemies.ActiveCount > 0)
                {
                    var enemy = game.Enemies.GetEnemy(0);
                    var seen = new HashSet<int>();
                    var sequenceDirectory = Path.Combine(directory, "skeleton-sequence");
                    Directory.CreateDirectory(sequenceDirectory);
                    for (var i = 0; i < 180 && (seen.Count < 8 || i < 60) && enemy != null && enemy.IsAlive; i++)
                    {
                        Frame(camera, enemy.transform.position, 5f); yield return new WaitForEndOfFrame();
                        if (i < 60) SaveFrame(sequenceDirectory, i);
                        var sprite = enemy.GetComponentInChildren<Stage3SkeletonSprite>();
                        if (sprite == null || !sprite.IsWalking || !seen.Add(sprite.CurrentFrame)) continue;
                        ScreenCapture.CaptureScreenshot(Path.Combine(directory, $"skeleton-walk-pose-{sprite.CurrentFrame}.png"));
                    }
                    context.AppendLine($"Actual first-wave skeleton: observed {seen.Count} walking phases.");
                }
                File.WriteAllText(Path.Combine(directory, "context.txt"), context.ToString());
                Debug.Log("S3_MANUAL_RESOURCE_PLAYER_CAPTURE_COMPLETE " + directory);
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                game.RunDriver.ResetRun(); game.HeroCombat.AutomaticTick = priorCombat;
                actor.position = priorHero; camera.orthographicSize = priorZoom; camera.transform.position = priorCamera;
                scene.CameraFollow.enabled = priorFollow;
                Time.captureFramerate = priorCaptureRate;
                InputSystem.settings.backgroundBehavior = priorBackground;
            }
        }

        private static Vector2 Approach(Stage2Economy economy, Stage2ResourceNode node)
        {
            for (var i = 0; i < 16; i++)
            {
                var angle = -Mathf.PI * .5f + i * Mathf.PI / 8f;
                var point = node.WorldPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) *
                    (node.InteractionRadius + (node.Resource == MapResourceKind.Wood ? .5f : .25f));
                if (economy.NearestAvailableNode(point) == node) return point;
            }
            return node.WorldPosition;
        }
        private static Vector2 WalkStart(Stage2SceneBootstrap scene, Stage2Economy economy)
        {
            foreach (var node in economy.Nodes.Skip(3))
            {
                var candidate = node.WorldPosition + new Vector2(10f, 10f);
                if (economy.Nodes.Any(n => Vector2.Distance(n.WorldPosition, candidate) < 9f + n.HarvestRadius)) continue;
                var clear = true;
                for (var y = -7; y <= 7 && clear; y++) for (var x = -7; x <= 7; x++)
                {
                    var point = candidate + new Vector2(x, y); var cell = scene.MapDefinition.WorldToCell(point);
                    if (!scene.MapDefinition.Contains(cell) || scene.MapDefinition.GetSurface(cell) != MapSurfaceKind.Forest ||
                        scene.PlayerMover.Obstacles.IsBlocked(point, scene.PlayerMover.CollisionRadius)) { clear = false; break; }
                }
                if (clear) return candidate;
            }
            return scene.PlayerMover.transform.position;
        }
        private static void Frame(Camera camera, Vector2 point, float zoom)
        {
            camera.orthographicSize = zoom;
            camera.transform.position = (Vector3)point + Vector3.back * .55f - camera.transform.forward * 30f;
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
        private static void SaveFrame(string directory, int frame)
        {
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory, frame.ToString("D4") + ".jpg"), texture.EncodeToJPG(90));
            UnityEngine.Object.Destroy(texture);
        }
    }
}
