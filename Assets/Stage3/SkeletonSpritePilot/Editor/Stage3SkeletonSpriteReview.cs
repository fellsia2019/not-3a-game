using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Explicit Editor-only live inspection. Ordinary spawn/movement; no saved tuning changes.</summary>
    public static class Stage3SkeletonSpriteReview
    {
        private const string Active = "Not3A.SkeletonSprite.LiveReview";
        private static double terminalSince;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool(Active, false);
            };
        }

        [MenuItem("Tools/Stage 3/Skeleton Sprites/Open Looping Live Review")]
        public static void OpenReview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new System.InvalidOperationException("Save the open scene before starting the live sprite review.");
            EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            SessionState.SetBool(Active, true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        private static void Update()
        {
            if (!SessionState.GetBool(Active, false) || !EditorApplication.isPlaying) return;
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            if (composition == null || !composition.IsInitialized) return;
            var run = composition.RunDriver.Run;
            if (run.IsTerminal)
            {
                if (terminalSince == 0) terminalSince = EditorApplication.timeSinceStartup;
                if (EditorApplication.timeSinceStartup - terminalSince > 2) composition.RunDriver.ResetRun();
                return;
            }
            terminalSince = 0;
            if (run.Phase != Stage2RunPhase.Preparation) return;
            var bootstrap = composition.GetComponent<Stage2SceneBootstrap>();
            // Show both landmarks and continue with the same early-wave action as Space.
            bootstrap.PlayerMover.transform.position = (bootstrap.EntranceMarker.position+bootstrap.ThroneMarker.position)*.5f;
            bootstrap.CameraFollow.SnapToTarget();
            run.StartWave();
            Debug.Log("S3_SKELETON_LIVE_REVIEW: ordinary waves, camera between portal/objective; repeats after outcome until Play Mode stops.");
        }
    }
}
