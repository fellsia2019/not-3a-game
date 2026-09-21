using UnityEngine;

namespace Not3A.Stage1
{
    public sealed class IsoSorting : MonoBehaviour
    {
        [SerializeField] private int offset;
        [SerializeField] private SpriteRenderer[] renderers;

        public void Configure(int sortingOffset, SpriteRenderer[] spriteRenderers)
        {
            offset = sortingOffset;
            renderers = spriteRenderers;
            Apply();
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            if (renderers == null)
            {
                return;
            }

            var baseOrder = offset - Mathf.RoundToInt(transform.position.y * 100f);
            for (var index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                {
                    renderers[index].sortingOrder = baseOrder + index;
                }
            }
        }
    }
}
