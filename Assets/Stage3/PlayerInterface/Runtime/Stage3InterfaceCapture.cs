using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in development-only captures of real OnGUI windows; no gameplay grants or camera edits.</summary>
    public sealed class Stage3InterfaceCapture : MonoBehaviour
    {
        private IEnumerator Start()
        {
            var interactionDirectory = Environment.GetEnvironmentVariable("NOT3A_RESOURCE_INTERACTION_CAPTURE_DIRECTORY");
            if (Debug.isDebugBuild && !Application.isBatchMode && !string.IsNullOrWhiteSpace(interactionDirectory) &&
                Path.IsPathFullyQualified(interactionDirectory))
            {
                yield return Stage3ResourceInteractionReview.Capture(gameObject, interactionDirectory);
                yield break;
            }
            var manualDirectory = Environment.GetEnvironmentVariable("NOT3A_MANUAL_CAPTURE_DIRECTORY");
            if (Debug.isDebugBuild && !Application.isBatchMode && !string.IsNullOrWhiteSpace(manualDirectory) &&
                Path.IsPathFullyQualified(manualDirectory))
            {
                yield return Stage3ManualResourceReview.Capture(gameObject, manualDirectory);
                yield break;
            }
            var qualityDirectory = Environment.GetEnvironmentVariable("NOT3A_QUALITY_CAPTURE_DIRECTORY");
            if (Debug.isDebugBuild && !Application.isBatchMode && !string.IsNullOrWhiteSpace(qualityDirectory) &&
                Path.IsPathFullyQualified(qualityDirectory))
            {
                yield return CaptureQuality(qualityDirectory);
                yield break;
            }
            var directory = Environment.GetEnvironmentVariable("NOT3A_INTERFACE_CAPTURE_DIRECTORY");
            if (!Debug.isDebugBuild || Application.isBatchMode || string.IsNullOrWhiteSpace(directory) ||
                !Path.IsPathFullyQualified(directory)) yield break;
            var hud = GetComponent<Stage3FantasyHud>();
            while (hud != null && !hud.IsReady) yield return null;
            if (hud == null) yield break;
            var inspectInteractions = Environment.GetEnvironmentVariable("NOT3A_INTERFACE_CAPTURE_INTERACTION") == "1";
            var originalPointer = Mouse.current?.position.ReadValue();
            var originalBackground = InputSystem.settings.backgroundBehavior;
            if (inspectInteractions)
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                if (Mouse.current != null && !Mouse.current.enabled) InputSystem.EnableDevice(Mouse.current);
            }
            try
            {
            Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(2f);
            var windows = new[] { Stage3PlayerWindow.None, Stage3PlayerWindow.Construction,
                Stage3PlayerWindow.Inventory, Stage3PlayerWindow.Research, Stage3PlayerWindow.Hero, Stage3PlayerWindow.Map };
            foreach (var window in windows)
            {
                hud.SetWindow(window);
                yield return new WaitForSecondsRealtime(.4f);
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(directory, window + ".png"));
                yield return null;
            }
            if (inspectInteractions)
            {
                hud.SetWindow(Stage3PlayerWindow.None);
                yield return CaptureHover(directory, "HoverMenu", hud.MenuButtonRect(Stage3PlayerWindow.Research));
                hud.SetWindow(Stage3PlayerWindow.Inventory);
                yield return CaptureHover(directory, "HoverClose", hud.CloseButtonRect);
                hud.SetWindow(Stage3PlayerWindow.Map);
                yield return CaptureHover(directory, "HoverMapZoom", hud.MapZoomInRect);
                hud.SetWindow(Stage3PlayerWindow.Construction);
                hud.SetBuildGroup(1);
                yield return CaptureHover(directory, "HoverBuildTab", hud.BuildTabRect(0));
                hud.SetBuildGroup(2);
                yield return CaptureHover(directory, "HoverWorkshop", hud.BuildChoiceRect(0));
                yield return CaptureHover(directory, "HoverBuildSelect", hud.BuildSelectButtonRect);
                hud.SetBuildGroup(0);
                yield return CaptureHover(directory, "DisabledBuildSelect", hud.BuildSelectButtonRect);
            }
            hud.SetWindow(Stage3PlayerWindow.None);
            File.WriteAllText(Path.Combine(directory,"context.txt"),
                "Visible development player; actual full Game backbuffer including IMGUI.\n"+
                $"Backbuffer: {Screen.width} x {Screen.height}; fullscreen={Screen.fullScreen}; mode={Screen.fullScreenMode}.\n"+
                $"Interface font: {hud.RuntimeFontName}; graphics={SystemInfo.graphicsDeviceType}; colorSpace={QualitySettings.activeColorSpace}.\n"+
                "QA switches windows through the same public UI actions; no resources, towers, kills, time jumps or camera changes injected.\n"+
                "Current unaffordable build cards remain disabled; inventory/progression reserves are not implemented mechanics.\n"+
                (inspectInteractions ? "Interaction review warps the player mouse; if native warp does not update Input System position, it queues a pointer-position-only input event. Actual renderer/control path, no UI-state override or mouse clicks. Background input policy restored and pointer restore requested. See hover-pointers.txt.\n" : ""));
            Debug.Log("S3_PLAYER_INTERFACE_CAPTURES_READY: " + directory);
            }
            finally
            {
                if (inspectInteractions)
                {
                    if (originalPointer.HasValue) Mouse.current?.WarpCursorPosition(originalPointer.Value);
                    InputSystem.settings.backgroundBehavior = originalBackground;
                }
            }
        }

        private IEnumerator CaptureQuality(string directory)
        {
            var game = GetComponent<Stage2GameComposition>();
            var hud = GetComponent<Stage3FantasyHud>();
            while (game != null && (!game.IsInitialized || hud == null || !hud.IsReady)) yield return null;
            var scene = GetComponent<Stage2SceneBootstrap>();
            var world = GetComponent<Stage3ResourceDistrictWorld>();
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (scene == null || world == null || pipeline == null) yield break;
            Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(1f);
            var camera = scene.CameraFollow.BoundCamera;
            var position = camera.transform.position;
            var zoom = camera.orthographicSize;
            var followed = scene.CameraFollow.enabled;
            var paused = game.RunDriver.Run.IsPaused;
            var window = hud.ActiveWindow;
            var samples = pipeline.msaaSampleCount;
            var textures = new System.Collections.Generic.List<Texture2D>();
            var filters = new System.Collections.Generic.List<FilterMode>();
            var biases = new System.Collections.Generic.List<float>();
            foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
                if (texture.name == "ForestLandmark" || texture.name == "StoneLandmark" || texture.name == "IronLandmark" ||
                    texture.name == "sawmill" || texture.name == "quarry" || texture.name == "iron_mine")
                { textures.Add(texture); filters.Add(texture.filterMode); biases.Add(texture.mipMapBias); }
            try
            {
                game.SetPaused(true); hud.CloseWindow(); scene.CameraFollow.enabled = false;
                foreach (var kind in new[] { ResourceProductionBuilding.Sawmill, ResourceProductionBuilding.Quarry, ResourceProductionBuilding.IronMine })
                {
                    ResourceProductionPoint point = default;
                    var found = false;
                    foreach (var candidate in world.ProductionPoints)
                        if (candidate.Building == kind) { point = candidate; found = true; break; }
                    if (!found) continue;
                    foreach (var size in new[] { 2.5f, 5f, 10f })
                    {
                        camera.orthographicSize = size;
                        camera.transform.position = new Vector3(point.Position.x, point.Position.y + .5f, -.3f) - camera.transform.forward * 40f;
                        foreach (var baseline in new[] { true, false })
                        {
                            pipeline.msaaSampleCount = baseline ? 1 : samples;
                            for (var i = 0; i < textures.Count; i++)
                            {
                                textures[i].filterMode = baseline ? FilterMode.Bilinear : filters[i];
                                textures[i].mipMapBias = baseline ? -10f : biases[i];
                            }
                            yield return new WaitForSecondsRealtime(.2f);
                            yield return new WaitForEndOfFrame();
                            var suffix = size == 2.5f ? "close" : size == 5f ? "normal" : "far";
                            ScreenCapture.CaptureScreenshot(Path.Combine(directory, kind + "-" + suffix + (baseline ? "-comparison.png" : "-current.png")));
                            yield return null; yield return null;
                        }
                    }
                }
                File.WriteAllText(Path.Combine(directory, "context.txt"),
                    $"Actual development player backbuffer {Screen.width}x{Screen.height}; graphics={SystemInfo.graphicsDeviceType}; colorSpace={QualitySettings.activeColorSpace}.\n" +
                    $"URP MSAA={samples}; renderScale={pipeline.renderScale}; camera.allowMSAA={camera.allowMSAA}; seed={world.Seed}.\n" +
                    "Opt-in disposable camera review: simulation paused, follow temporarily disabled, camera visits free production pads at zoom2.5/5/10. No grants or buildings. Restored afterward.\n" +
                    "comparison: MSAA1, bilinear, mip bias -10 forcing highest source mip; approximates former sampling, not an old executable. current: authored MSAA/filter/bias. Both retain the requested no-inner-dash geometry.\n" +
                    string.Join("\n", textures.ConvertAll(t => $"{t.name}: {t.width}x{t.height}, mipCount={t.mipmapCount}, filter={t.filterMode}")));
                Debug.Log("S3_QUALITY_CAPTURES_READY: " + directory);
            }
            finally
            {
                pipeline.msaaSampleCount = samples;
                for (var i = 0; i < textures.Count; i++) { textures[i].filterMode = filters[i]; textures[i].mipMapBias = biases[i]; }
                camera.orthographicSize = zoom; camera.transform.position = position;
                scene.CameraFollow.enabled = followed; game.SetPaused(paused); hud.SetWindow(window);
            }
        }

        private static IEnumerator CaptureHover(string directory, string name, Rect guiRect)
        {
            if (Mouse.current == null) yield break;
            var requested = new Vector2(guiRect.center.x, Screen.height - guiRect.center.y);
            Mouse.current.WarpCursorPosition(requested);
            yield return null;
            yield return null;
            var syntheticPosition = Vector2.Distance(Mouse.current.position.ReadValue(), requested) > .75f;
            if (syntheticPosition)
            {
                if (!Mouse.current.enabled) InputSystem.EnableDevice(Mouse.current);
                InputSystem.QueueDeltaStateEvent(Mouse.current.position, requested);
                yield return null;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(.2f);
            File.AppendAllText(Path.Combine(directory,"hover-pointers.txt"),
                $"{name}: focused={Application.isFocused}; mouseEnabled={Mouse.current.enabled}; requested={requested}; actual={Mouse.current.position.ReadValue()}; queuedPositionEvent={syntheticPosition}\n");
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png"));
            yield return null;
        }
    }
}
