using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Four six-frame walks, clockwise from screen-right. Shared by every enemy.</summary>
    public sealed class Stage3SkeletonSpriteSet : ScriptableObject
    {
        public const int FramesPerDirection = 6;
        public const int DirectionCount = 4;
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(.1f)] private float strideCells = .85f;

        public bool IsValid => frames != null && frames.Length == FramesPerDirection * DirectionCount
            && System.Array.TrueForAll(frames, frame => frame != null);
        public float StrideCells => strideCells;
        public Sprite Frame(int direction, int frame) => frames[
            Mathf.Clamp(direction, 0, DirectionCount - 1) * FramesPerDirection
            + Mathf.Clamp(frame, 0, FramesPerDirection - 1)];

        public void Configure(Sprite[] walkFrames) => frames = walkFrames;
    }
}
