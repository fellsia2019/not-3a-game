using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Four six-frame walks, clockwise from screen-right. Shared by every enemy.</summary>
    public sealed class Stage3SkeletonSpriteSet : ScriptableObject
    {
        public const int FramesPerDirection = 6;
        public const int DirectionCount = 4;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private Sprite[] extendedFrames;
        [SerializeField] private bool mirrorLeft;
        [SerializeField, Min(.1f)] private float strideCells = .85f;

        public bool IsValid => frames != null && frames.Length == FramesPerDirection * DirectionCount
            && System.Array.TrueForAll(frames, frame => frame != null);
        public float StrideCells => strideCells;
        public bool MirrorLeft => FrameCount == 8 && mirrorLeft;
        public int FrameCount => extendedFrames != null && extendedFrames.Length == 32 &&
            System.Array.TrueForAll(extendedFrames, frame => frame != null) ? 8 : FramesPerDirection;
        public Sprite Frame(int direction, int frame) => (FrameCount == 8 ? extendedFrames : frames)[
            Mathf.Clamp(direction, 0, DirectionCount - 1) * FrameCount + Mathf.Clamp(frame, 0, FrameCount - 1)];

        public void Configure(Sprite[] walkFrames) => frames = walkFrames;
        public void ConfigureExtended(Sprite[] walkFrames, bool mirroredLeft = false, float stride = 1.25f)
        { extendedFrames = walkFrames; mirrorLeft = mirroredLeft; strideCells = Mathf.Max(.1f, stride); }
    }
}
