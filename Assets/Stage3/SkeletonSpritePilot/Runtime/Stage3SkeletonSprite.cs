using Not3A.Stage2.RunState;
using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Distance-driven sprite playback. Does not move, damage or select targets.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage3SkeletonSprite : MonoBehaviour
    {
        [SerializeField] private Stage3SkeletonSpriteSet walks;
        [SerializeField] private SpriteRenderer body;
        private Stage2Enemy enemy;
        private Stage2RunDriver runDriver;
        private Camera gameCamera;
        private SpriteRenderer feedbackSource;
        private Color restingFeedback;
        private Vector3 previousPosition;
        private float stridePhase;

        public int CurrentFrame { get; private set; }
        public int Direction { get; private set; } = 3;
        public bool IsWalking { get; private set; }
        public SpriteRenderer Body => body;
        public int WalkFrameCount => walks != null ? walks.FrameCount : 0;

        public void Bind(Stage2Enemy actor, Stage2RunDriver run, Camera camera)
        {
            enemy = actor;
            runDriver = run;
            gameCamera = camera;
            previousPosition = actor.transform.position;
            feedbackSource = actor.GetComponent<SpriteRenderer>();
            restingFeedback = feedbackSource != null ? feedbackSource.color : Color.white;
            if (walks == null || !walks.IsValid || body == null)
            {
                Debug.LogError("S3_SKELETON_BINDING_ERROR: sprite set and body are required.", this);
                enabled = false;
                return;
            }
            ApplyFrame();
        }

        private void LateUpdate()
        {
            if (enemy == null || gameCamera == null || runDriver?.Run == null) return;
            var position = enemy.transform.position;
            var delta = position - previousPosition;
            previousPosition = position;
            var run = runDriver.Run;
            var distance = new Vector2(delta.x, delta.y).magnitude;
            IsWalking = !run.IsPaused && !run.IsTerminal && distance > .00001f && distance < 2f;
            if (IsWalking)
            {
                var right = Vector3.Dot(delta, gameCamera.transform.right);
                var up = Vector3.Dot(delta, gameCamera.transform.up);
                Direction = Mathf.Abs(right) >= Mathf.Abs(up) ? (right >= 0f ? 0 : 2) : (up >= 0f ? 1 : 3);
                stridePhase = Mathf.Repeat(stridePhase + distance / walks.StrideCells, 1f);
                CurrentFrame = Mathf.FloorToInt(stridePhase * walks.FrameCount);
            }
            else if (!run.IsPaused && !run.IsTerminal)
            {
                // Stop cycling while besieging a blocker; no attack animation is claimed by this pilot.
                CurrentFrame = 0;
                stridePhase = 0f;
            }
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            body.sprite = walks.Frame(Direction, CurrentFrame);
            body.flipX = walks.MirrorLeft && Direction == 2;
            body.enabled = enemy.IsAlive;
            body.transform.rotation = gameCamera.transform.rotation;
            body.sortingOrder = -Mathf.RoundToInt(enemy.transform.position.y*100f);
            // The original red marker remains hidden but still owns the existing hit flash timer.
            body.color = feedbackSource != null && feedbackSource.color != restingFeedback
                ? new Color(1f, .58f, .58f, 1f) : Color.white;
        }
    }
}
