using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Reviewed manual-node artwork. Stock, locations and recovery remain economy-owned.</summary>
    public sealed class Stage3ManualResourceArt : ScriptableObject
    {
        public const int DepletionFrames = 4;
        public Sprite tree;
        public Sprite[] stoneStates;
        public Sprite[] oreStates;
        // Editor-authored from the same opaque artwork extents as the native sprite pivots.
        // The Stage3 economy builder copies these values to its gameplay catalog.
        public float[] stoneHarvestRadii;
        public float[] oreHarvestRadii;
        public Vector2 treeVisibleTop;
        public Vector2[] stoneVisibleTops;
        public Vector2[] oreVisibleTops;

        public Vector2 VisibleTop(MapResourceKind kind, Stage2ResourceSize size, int state)
        {
            if (kind == MapResourceKind.Wood && treeVisibleTop != Vector2.zero) return treeVisibleTop;
            var tops = kind == MapResourceKind.Stone ? stoneVisibleTops : oreVisibleTops;
            var index = Mathf.Clamp((int)size, 0, 2) * DepletionFrames + Mathf.Clamp(state, 0, 3);
            if (kind != MapResourceKind.Wood && tops != null && tops.Length == 12) return tops[index];
            var frame = Frame(kind, size, state);
            return frame != null ? new Vector2(frame.bounds.center.x, frame.bounds.max.y) : Vector2.zero;
        }

        public static int State(float remainingFraction) => Stage2ManualResourceSettings.HarvestState(remainingFraction);

        public Sprite Frame(MapResourceKind kind, Stage2ResourceSize size, int state)
        {
            if (kind == MapResourceKind.Wood) return tree;
            var frames = kind == MapResourceKind.Stone ? stoneStates : oreStates;
            var index = Mathf.Clamp((int)size, 0, 2) * DepletionFrames + Mathf.Clamp(state, 0, 3);
            return frames != null && frames.Length == 12 ? frames[index] : null;
        }
    }
}
