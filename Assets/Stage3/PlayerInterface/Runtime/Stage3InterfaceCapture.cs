using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in development-only captures of real OnGUI windows; no gameplay grants or camera edits.</summary>
    public sealed class Stage3InterfaceCapture : MonoBehaviour
    {
        private IEnumerator Start()
        {
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
