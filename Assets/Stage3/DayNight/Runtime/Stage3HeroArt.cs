using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed class Stage3HeroArt : ScriptableObject
    {
        public Sprite[] bodyFrames;
        public Sprite[] idleDirections;
        public Sprite[] bowDirections;
        public Material material;
        public Vector2[] gripOffsets;
        public Vector2[] idleGripOffsets;
        // D-071 additive data: legacy sixteen-frame references remain intact.
        public Sprite[] walkFrames;
        public Vector2[] walkGripOffsets;
        public bool mirrorLeftWalk;
        public bool mirrorLeftWork;
        public Sprite[] axeWorkFrames;
        public Sprite[] pickWorkFrames;
        public int WalkFrameCount => ValidFrames(walkFrames, 32) ? 8 : 4;
        public bool HasWork => ValidFrames(axeWorkFrames, 24) && ValidFrames(pickWorkFrames, 24);
        public Sprite Walk(int direction, int frame) => WalkFrameCount == 8
            ? walkFrames[direction * 8 + Mathf.Clamp(frame, 0, 7)]
            : bodyFrames[direction * 4 + Mathf.Clamp(frame, 0, 3)];
        public Vector2 WalkGrip(int direction, int frame) => WalkFrameCount == 8 &&
            walkGripOffsets != null && walkGripOffsets.Length == 32 ? walkGripOffsets[direction * 8 + frame] :
            gripOffsets != null && gripOffsets.Length == 16 ? gripOffsets[direction * 4 + Mathf.Min(frame, 3)] :
            new Vector2(.1f, .55f);
        public Sprite Work(bool axe, int direction, int frame) =>
            (axe ? axeWorkFrames : pickWorkFrames)[direction * 6 + Mathf.Clamp(frame, 0, 5)];
        private static bool ValidFrames(Sprite[] frames, int count) => frames != null &&
            frames.Length == count && System.Array.TrueForAll(frames, frame => frame != null);
        public float strideCells = .9f;
        public float idlePeriodSeconds = 3.6f;
        public float idleBreathScale = .007f;
        public bool HasIdle => idleDirections != null && idleDirections.Length == 4 &&
            idleGripOffsets != null && idleGripOffsets.Length == 4;
        public bool IsValid => bodyFrames != null && bodyFrames.Length == 16 &&
            bowDirections != null && bowDirections.Length == 4 && material != null;
    }
}
