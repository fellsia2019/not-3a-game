using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Opt-in disposable development-player check of physical approaches and the actual overhead marker.</summary>
    public static class Stage3ResourceInteractionReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            var game = owner.GetComponent<Stage2GameComposition>();
            while (game == null || !game.IsInitialized || game.Gathering.Economy.Nodes.Count <= 3) yield return null;
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hero = scene.PlayerMover;
            var marker = owner.GetComponent<Stage3ResourceInteractionMarker>();
            var collisions = owner.GetComponent<Stage3ManualResourceCollisions>();
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var camera = scene.CameraFollow.BoundCamera;
            var economy = game.Gathering.Economy;
            var priorHero = hero.transform.position; var priorCamera = camera.transform.position;
            var priorZoom = camera.orthographicSize; var priorFollow = scene.CameraFollow.enabled;
            var priorRate = Time.captureFramerate; var priorCombat = game.HeroCombat.AutomaticTick;
            var priorBackground = InputSystem.settings.backgroundBehavior;
            var keyboard = InputSystem.AddDevice<Keyboard>("Resource interaction QA keyboard");
            var context = new StringBuilder($"Actual development-player Game backbuffer {Screen.width}x{Screen.height}; {SystemInfo.graphicsDeviceType}.\n" +
                "Disposable opt-in QA: synthetic Input System WASD/E; camera/hero setup and model strikes for depletion. No physical keyboard claim. Run reset afterward.\n");
            Directory.CreateDirectory(directory);
            try
            {
                Time.captureFramerate = 60;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                scene.CameraFollow.enabled = false; game.HeroCombat.AutomaticTick = false;
                game.SetPaused(false); hud.SetWindow(Stage3PlayerWindow.None);
                foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                foreach (var size in new[] { Stage2ResourceSize.Small, Stage2ResourceSize.Medium, Stage2ResourceSize.Large })
                {
                    if (kind == MapResourceKind.Wood && size != Stage2ResourceSize.Small) continue;
                    var choice = Approach(economy, collisions, hero, kind, size);
                    hero.transform.position = choice.start;
                    Frame(camera, choice.node.WorldPosition, 5f);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                    var recoveryCount = hero.OverlapRecoveryCount;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(choice.key));
                    for (var i = 0; i < 30; i++) yield return null;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; yield return null;
                    var distance = Vector2.Distance(hero.transform.position, choice.node.WorldPosition);
                    Require(Mathf.Abs(distance - collisions.ColliderRadius(choice.node) - hero.CollisionRadius) < .01f,
                        kind + " " + size + " stops at footing");
                    Require(hero.OverlapRecoveryCount == recoveryCount, "No collision recovery teleport");
                    Require(marker.IsVisible && marker.Target == choice.node && game.Gathering.InteractionTarget == choice.node,
                        "Marker uses exact E target");
                    Require(marker.ToolIsAxe == (kind == MapResourceKind.Wood), "Correct tool glyph");
                    foreach (var zoom in size == Stage2ResourceSize.Small ? new[] { 2.5f, 5f, 10f } : new[] { 5f })
                    {
                        Frame(camera, choice.node.WorldPosition, zoom);
                        yield return Shot(directory, $"marker-{kind}-{size}-zoom-{zoom.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}");
                    }
                    var before = economy.Inventory.Get(kind);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                    for (var i = 0; i < 100 && economy.Inventory.Get(kind) == before; i++) yield return null;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                    Require(economy.Inventory.Get(kind) == before + 1, "E pays one at physical contact");
                    context.AppendLine($"{kind} {size}: contact distance={distance:F4}; footing={collisions.ColliderRadius(choice.node):F4}; exact marker/E; +1; no overlap recovery.");
                }
                var tree = Approach(economy, collisions, hero, MapResourceKind.Wood, Stage2ResourceSize.Small, true);
                hero.transform.position = tree.node.WorldPosition + Vector2.up * .45f;
                Frame(camera, tree.node.WorldPosition, 5f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                for (var i = 0; i < 50; i++) yield return null;
                var view = owner.GetComponentsInChildren<Stage3ResourceSprite>(true).Single(v => v.Node == tree.node);
                Require(view.Body.color.a < .6f && marker.IsVisible && marker.Target == tree.node, "Faded tree retains axe target");
                yield return Shot(directory, "marker-faded-tree");
                hud.SetWindow(Stage3PlayerWindow.Inventory); yield return null; yield return null;
                Require(!marker.IsVisible, "Inventory hides marker");
                hud.SetWindow(Stage3PlayerWindow.None); yield return null; yield return null;
                game.SetPaused(true); yield return null; yield return null;
                Require(!marker.IsVisible, "Pause hides marker");
                game.SetPaused(false); yield return null;
                var depleted = tree.node;
                while (!depleted.IsDepleted) economy.AdvanceGather(depleted, depleted.WorldPosition, true, depleted.GatherSeconds, true);
                economy.CancelGather(); yield return null; yield return null;
                Require(collisions.ColliderRadius(depleted) == 0f && !marker.IsVisible, "Depletion opens site and removes marker");
                yield return Shot(directory, "depleted-tree-open-site");
                context.AppendLine("Foreground tree alpha<0.6 retains axe; inventory/pause hide marker; depleted isolated tree loses footing/marker. All checks PASS.");
                File.WriteAllText(Path.Combine(directory, "context.txt"), context.ToString());
                Debug.Log("S3_RESOURCE_INTERACTION_CAPTURE_COMPLETE " + directory);
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard); hud.SetWindow(Stage3PlayerWindow.None);
                game.RunDriver.ResetRun(); game.HeroCombat.AutomaticTick = priorCombat;
                hero.transform.position = priorHero; camera.orthographicSize = priorZoom; camera.transform.position = priorCamera;
                scene.CameraFollow.enabled = priorFollow; Time.captureFramerate = priorRate;
                InputSystem.settings.backgroundBehavior = priorBackground;
            }
        }

        private static (Stage2ResourceNode node, Vector2 start, Key key) Approach(Stage2Economy economy,
            Stage3ManualResourceCollisions collisions, Stage2PlayerMover hero, MapResourceKind kind, Stage2ResourceSize size,
            bool isolatedTree = false)
        {
            foreach (var node in economy.Nodes.Where(n => n.Resource == kind && n.Size == size && !n.IsDepleted))
            {
                if (isolatedTree && (node.GroupId < 0 || economy.Nodes.Count(n => n.Resource == kind && n.GroupId == node.GroupId) != 1)) continue;
                foreach (var direction in new[] { Vector2.down, Vector2.left, Vector2.up, Vector2.right })
                {
                    var radius = collisions.ColliderRadius(node);
                    var start = node.WorldPosition + direction * (radius + hero.CollisionRadius + .6f);
                    if (hero.Obstacles.IsBlocked(start, hero.CollisionRadius)) continue;
                    var contact = hero.Obstacles.Move(start, -direction * .7f, hero.CollisionRadius);
                    if (Mathf.Abs(Vector2.Distance(contact, node.WorldPosition) - radius - hero.CollisionRadius) > .01f ||
                        economy.NearestAvailableNode(contact) != node) continue;
                    return (node, start, direction == Vector2.down ? Key.W : direction == Vector2.up ? Key.S :
                        direction == Vector2.left ? Key.D : Key.A);
                }
            }
            throw new InvalidOperationException("No clear QA approach to " + kind + " " + size);
        }
        private static void Require(bool passed, string message)
        {
            if (!passed) throw new InvalidOperationException("Resource interaction QA: " + message);
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
    }
}
