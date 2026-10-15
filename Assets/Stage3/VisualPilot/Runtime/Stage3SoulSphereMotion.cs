using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Visual orbit only; throne health, blocking and targeting stay in Stage 2 rules.</summary>
    public sealed class Stage3SoulSphereMotion : MonoBehaviour
    {
        [SerializeField] private Transform orbitingWave;
        [SerializeField] private float degreesPerSecond = 24f;

        private void Update()
        {
            if (orbitingWave != null)
                // Rules ground is XY and height is -Z. FBX child-local axes
                // retain Blender conversion, so local Y would tip the ring.
                orbitingWave.RotateAround(transform.position, Vector3.back,
                    degreesPerSecond * Time.deltaTime);
        }

        public void Configure(Transform wave) => orbitingWave = wave;
    }
}
