using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Blueprint Art")]
    public sealed class Stage3BlueprintArt : ScriptableObject
    {
        // Tight visible source bounds, baked once by Stage3BlueprintArtImporter.
        // Tower, wand, forge. Stage numerals belong to the UI, never the bitmap.
        public Sprite[] Pages = new Sprite[3];
    }
}
