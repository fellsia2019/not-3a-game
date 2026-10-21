using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Shared asset so zoom tuning in the Inspector survives Play Mode.</summary>
    public sealed class Stage2CameraZoomSettings : ScriptableObject
    {
        [SerializeField, Range(1.05f, 3f)]
        [Tooltip("Во сколько раз меняется масштаб за один шаг колёсика. Изменения сохраняются во время игры.")]
        private float stepMultiplier = 1.4f;

        public float StepMultiplier => Mathf.Clamp(stepMultiplier, 1.05f, 3f);
        [SerializeField,Range(1f,5f)] private float maximumDistanceMultiplier=2f;
        public float MaximumDistanceMultiplier=>Mathf.Clamp(maximumDistanceMultiplier,1f,5f);
    }
}
