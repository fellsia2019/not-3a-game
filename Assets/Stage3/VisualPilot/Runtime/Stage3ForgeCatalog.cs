using Not3A.Stage2.Economy;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Forge Building Catalog")]
    public sealed class Stage3ForgeCatalog : ScriptableObject
    {
        [SerializeField] private Stage2ResourceCost buildCost = new Stage2ResourceCost(20, 15, 8);
        [SerializeField] private Vector2 footprint = new Vector2(3.8f, 2.1f);
        [SerializeField] private Vector2 entranceOffset = new Vector2(0f, -1.6f);
        [SerializeField] private float interactionRadius = 1.7f;
        [SerializeField] private float worldWidth = 4.2f;
        public Stage2ResourceCost BuildCost => buildCost;
        public Vector2 Footprint => footprint;
        public Vector2 EntranceOffset => entranceOffset;
        public float InteractionRadius => interactionRadius;
        public float WorldWidth => worldWidth;
        public bool TryValidate(out string reason)
        {
            reason = "Forge requires a resource-only cost and finite positive footprint/width/range.";
            if (!buildCost.IsValid || buildCost.IsZero || buildCost.Gold != 0 || !Positive(footprint.x) || !Positive(footprint.y) ||
                !Positive(interactionRadius) || !Positive(worldWidth) || !float.IsFinite(entranceOffset.x) || !float.IsFinite(entranceOffset.y)) return false;
            reason = string.Empty; return true;
        }
        private static bool Positive(float value) => float.IsFinite(value) && value > 0;
    }
}
