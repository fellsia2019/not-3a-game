using Not3A.Stage2.RunState;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Read-only run presentation. Spawn location and navigation belong to map/rules.</summary>
    public sealed class Stage3EntrancePortal : MonoBehaviour
    {
        [SerializeField] private Renderer vortex;
        private Stage2RunDriver runDriver;
        private MaterialPropertyBlock properties;
        public float FlowTime { get; private set; }

        public void Configure(Renderer surface) => vortex = surface;
        public void Bind(Stage2RunDriver driver) => runDriver = driver;

        private void Update()
        {
            // Ambient circulation also stays alive in the outcome view and prefab
            // previews. Only an explicit gameplay pause freezes its presentation clock.
            if (vortex == null || runDriver?.Run?.IsPaused == true) return;
            FlowTime += Time.deltaTime;
            properties ??= new MaterialPropertyBlock();
            properties.SetFloat("_FlowTime", FlowTime);
            vortex.SetPropertyBlock(properties);
        }
    }
}
