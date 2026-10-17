using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed class Stage3ResourceDistrictArt : ScriptableObject
    {
        public ResourceDistrictSettings settings = new ResourceDistrictSettings();
        public ResourceDistrictShapeSettings shape = new ResourceDistrictShapeSettings();
        public bool randomizeWorldSeed = true;
        public int fixedWorldSeed = 164032;
        [Min(.05f)] public float landmarkScale = .75f;
        public Stage3ResourceLandmark[] landmarks = System.Array.Empty<Stage3ResourceLandmark>();
        public Sprite[] trees;
        public Sprite[] rocks;
        public Sprite[] mineParts;
        public Material material;
        public Material geologyMaterial;
        public Material geologyRevealMaterial;
        public Material groundBlendMaterial;
        [Range(.15f,.6f)] public float occludedOpacity = .35f;
        [Min(.02f)] public float fadeOutSeconds = .16f;
        [Min(.02f)] public float fadeInSeconds = .28f;
        [Min(0f)] public float occlusionHoldSeconds = .12f;
        [Min(.1f)] public float revealRadius = .95f;
        [Range(.1f,.9f)] public float revealSoftness = .45f;
        [Min(.2f)] public float groundBlendWidth = 2f;
        [Range(.1f, 1f)] public float treeDensity = .60f;
        [Range(.1f, 1f)] public float rockDensity = .65f;
        [Min(.01f)] public float heroCollisionRadius = .2f;
        [Min(.01f)] public float treeTrunkRadius = .24f;
        [Min(.01f)] public float rockFootprintRadius = 1.2f;
        [Min(.01f)] public float hoistFootprintRadius = .65f;
    }

    [System.Serializable]
    public sealed class Stage3ResourceLandmark
    {
        public MapResourceKind kind;
        public Stage3ResourceLandmarkLayer[] layers = System.Array.Empty<Stage3ResourceLandmarkLayer>();
        // Authoring coordinates use ground XY, before the common landmark scale.
        public Vector2[] groundContour = System.Array.Empty<Vector2>();
        public Rect[] solidGround = System.Array.Empty<Rect>();
        public Stage3LandmarkGroundCircle[] groundCircles = System.Array.Empty<Stage3LandmarkGroundCircle>();
        public Vector2 productionPoint;
        public Vector2 productionClearance = new Vector2(3, 3);
        public float productionFacingDegrees;
    }

    [System.Serializable]
    public sealed class Stage3ResourceLandmarkLayer
    {
        public Sprite sprite;
        public Vector2 groundAnchor;
        [Min(.01f)] public float width = 18;
        public bool groundLayer;
        // Normalized image regions whose pixels depict walkable ground, not occluding volume.
        public Rect[] groundImageAreas = System.Array.Empty<Rect>();
    }

    [System.Serializable]
    public struct Stage3LandmarkGroundCircle
    {
        public Vector2 center;
        [Min(.01f)] public float radius;
    }
}
