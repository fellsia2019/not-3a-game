using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Small app-local AppKit adapter; Windows never loads the macOS library.</summary>
    public static class Stage2MacTrackpad
    {
        public static bool IsAvailable { get; private set; }
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
        private static bool unavailable;
#endif

        public static void Initialize()
        {
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
            if (IsAvailable || unavailable) return;
            try { IsAvailable = Not3ATrackpad_Initialize() != 0; }
            catch (Exception exception) when (exception is DllNotFoundException ||
                exception is EntryPointNotFoundException || exception is BadImageFormatException)
            {
                unavailable = true;
                Debug.LogWarning("Mac trackpad zoom library could not load; mouse zoom remains available. " + exception.Message);
            }
#endif
        }

        public static float ReadScale(bool acceptInput, bool panPreciseScroll = false)
        {
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
            Initialize();
            if (IsAvailable)
            {
                Not3ATrackpad_SetEnabled(acceptInput ? 1 : 0);
                Not3ATrackpad_SetPanEnabled(acceptInput && panPreciseScroll ? 1 : 0);
                return Not3ATrackpad_ReadScale();
            }
#endif
            return 1f;
        }

        public static Vector2 ReadPan()
        {
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
            if (IsAvailable)
            {
                Not3ATrackpad_ReadPan(out var x, out var y);
                return new Vector2(x, y);
            }
#endif
            return Vector2.zero;
        }

        // Source classification prevents a precise trackpad scroll reaching IMGUI from becoming wheel zoom.
        public static bool LastScrollWasPrecise
        {
            get
            {
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
                return IsAvailable && Not3ATrackpad_LastScrollWasPrecise() != 0;
#else
                return false;
#endif
            }
        }

        public static void Stop()
        {
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
            if (IsAvailable) Not3ATrackpad_Shutdown();
#endif
            IsAvailable = false;
        }

#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
        [DllImport("Not3ATrackpad")] private static extern int Not3ATrackpad_Initialize();
        [DllImport("Not3ATrackpad")] private static extern void Not3ATrackpad_SetEnabled(int enabled);
        [DllImport("Not3ATrackpad")] private static extern void Not3ATrackpad_SetPanEnabled(int enabled);
        [DllImport("Not3ATrackpad")] private static extern float Not3ATrackpad_ReadScale();
        [DllImport("Not3ATrackpad")] private static extern void Not3ATrackpad_ReadPan(out float x, out float y);
        [DllImport("Not3ATrackpad")] private static extern int Not3ATrackpad_LastScrollWasPrecise();
        [DllImport("Not3ATrackpad")] private static extern void Not3ATrackpad_Shutdown();
#endif

#if UNITY_EDITOR_OSX
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterCleanup()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += Stop;
            UnityEditor.EditorApplication.pauseStateChanged += _ => Stop();
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode) Stop();
            };
        }
#endif
    }
}
