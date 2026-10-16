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
}
