using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Presentation-only adapter for the existing enemy hit tint.</summary>
    public sealed class Stage3MeshFeedback : MonoBehaviour
    {
        private SpriteRenderer source;
        private MeshRenderer[] targets;
        private MaterialPropertyBlock properties;
        private void Start()
        {
            source = GetComponent<SpriteRenderer>();
            targets = GetComponentsInChildren<MeshRenderer>();
            properties = new MaterialPropertyBlock();
        }
        private void LateUpdate()
        {
            if (source == null) return;
            properties.SetColor("_BaseColor", source.color);
            foreach (var target in targets) if (target != null) target.SetPropertyBlock(properties);
        }
    }
}
