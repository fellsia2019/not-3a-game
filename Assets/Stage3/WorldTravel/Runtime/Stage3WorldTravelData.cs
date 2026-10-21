using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed class Stage3WorldTravelData : ScriptableObject
    {
        public Material sealMaterial;
        public Sprite workshopSprite;
        public Material workshopMaterial;
        public Vector2 workshopOffsetFromThrone = new Vector2(-5, 7);
        public Vector2 baseOffsetFromThrone = new Vector2(0, 7);
        public Vector2 worldCirclePosition = new Vector2(94, 48);
        public Vector2 cityOrigin = new Vector2(172, 162);
        public Stage3ExtraTravelPoint[] additionalPoints = System.Array.Empty<Stage3ExtraTravelPoint>();
        [Min(1f)] public float circleDiameter = 3.4f;
        [Min(.1f)] public float interactionRadius = .85f;
        [Min(.1f)] public float discoveryRadius = 2.2f;
        public Vector2 workshopFootprint = new Vector2(2.2f, 1.2f);
        [Min(.1f)] public float workshopWidth = 2.6f;
        [Min(1f)] public float siteClearance = 2.4f;
    }
}
