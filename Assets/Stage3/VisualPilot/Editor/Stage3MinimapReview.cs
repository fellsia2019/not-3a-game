using System.Collections;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3MinimapReview
    {
        private const string CaptureRequest = "Not3A.Minimap.CaptureRequest";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update += () =>
            {
                if (!SessionState.GetBool(CaptureRequest, false) || !EditorApplication.isPlaying) return;
                var map = Object.FindAnyObjectByType<Stage2Minimap>();
                if (map == null || map.Terrain == null) return;
                SessionState.SetBool(CaptureRequest, false);
                map.StartCoroutine(Capture());
            };
        }

        public static void OpenAndCapture()
        {
            SessionState.SetBool(CaptureRequest, true);
            Stage3CartoonWorldBuilder.OpenReview();
        }

        [MenuItem("Tools/Stage 3/Minimap/Capture Live Game")]
        public static void RequestCapture() => SessionState.SetBool(CaptureRequest, true);

        private static IEnumerator Capture()
        {
            // GUI overlays are available at end-of-frame in the interactive Editor.
            // Camera.Render() omits them; batch mode has no displayed Game surface.
            yield return new WaitForSecondsRealtime(.5f);
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            const string folder = "Artifacts/S3-MINIMAP-20261015/";
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(folder + "game-with-minimap.png", image.EncodeToPNG());
            Debug.Log($"S3_MINIMAP_CAPTURED {image.width}x{image.height} {folder}game-with-minimap.png");
            Object.Destroy(image);
        }
    }
}
