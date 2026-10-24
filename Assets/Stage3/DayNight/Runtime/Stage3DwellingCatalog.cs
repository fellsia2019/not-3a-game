using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Dwelling")]
    public sealed class Stage3DwellingCatalog : ScriptableObject
    {
        public int woodCost = 20;
        public int stoneCost = 20;
        public Vector2 footprint = new Vector2(3.2f, 2f);
        public Vector2 entranceOffset = new Vector2(.65f, -1.4f);
        public float interactionRadius = 1.4f;
        public Sprite sprite;
        public Material material;
        public float worldWidth = 3.2f;

        public bool TryValidate(out string reason)
        {
            if (woodCost <= 0 || stoneCost < 0 || !Positive(footprint.x) || !Positive(footprint.y) ||
                !Positive(interactionRadius) || !Positive(worldWidth) ||
                !Finite(entranceOffset.x) || !Finite(entranceOffset.y) || sprite == null || material == null)
            {
                reason = "Dwelling requires positive price/footprint/range/width, a finite entrance offset and approved sprite/material.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Positive(float value) => Finite(value) && value > 0f;
    }
}
