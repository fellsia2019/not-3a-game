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
        public float strideCells = .9f;
        public float idlePeriodSeconds = 3.6f;
        public float idleBreathScale = .007f;
        public bool HasIdle => idleDirections != null && idleDirections.Length == 4 &&
            idleGripOffsets != null && idleGripOffsets.Length == 4;
        public bool IsValid => bodyFrames != null && bodyFrames.Length == 16 &&
            bowDirections != null && bowDirections.Length == 4 && material != null;
    }
}
