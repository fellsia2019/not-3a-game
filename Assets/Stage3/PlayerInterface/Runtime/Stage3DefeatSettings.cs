using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Presentation timing only; no damage, checkpoint or run tuning.</summary>
    public sealed class Stage3DefeatSettings : ScriptableObject
    {
        public Shader fractureShader;
        public float hitSeconds = .65f;
        public float shakeSeconds = .22f;
        public float shakePixels = 2.2f;
        public float whiteoutSeconds = 1.6f;
        public float whiteHoldSeconds = .18f;
        public float retryFadeSeconds = .3f;
    }
}
