using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/City Art")]
    public sealed class Stage3CityArt : ScriptableObject
    {
        public Sprite shop, temple;
        public Sprite guild, noticeBoard;
        public Sprite baseArrivalPreview, cityArrivalPreview;
        public Sprite[] gardenTrees;
        // Shrub, flowering hedge, two fence orientations, lantern, low ground flowers.
        public Sprite[] gardens = new Sprite[0];
        // Four houses; well, bench, wall/post, planter; merchant, man, woman, role badge.
        public Sprite[] kit = new Sprite[12];
        public Material spriteMaterial, shopMaterial, pavingMaterial;
        public bool IsReady => shop != null && temple != null && kit != null && kit.Length == 12 &&
            spriteMaterial != null && shopMaterial != null && pavingMaterial != null;
    }
}
