using UnityEngine;

namespace Not3A.Stage1
{
    public sealed class GatherableNode : MonoBehaviour
    {
        [SerializeField] private ResourceKind resource;
        [SerializeField, Min(1)] private int remainingUnits = 3;
        [SerializeField, Min(0.1f)] private float gatherSeconds = 0.8f;

        public ResourceKind Resource => resource;
        public int RemainingUnits => remainingUnits;
        public float GatherSeconds => gatherSeconds;
        public bool IsDepleted => remainingUnits <= 0;

        public void Configure(ResourceKind kind, int units, float seconds)
        {
            resource = kind;
            remainingUnits = Mathf.Max(1, units);
            gatherSeconds = Mathf.Max(0.1f, seconds);
        }

        public bool TryGather(out ResourceKind kind, out int amount)
        {
            kind = resource;
            amount = 0;
            if (IsDepleted)
            {
                return false;
            }

            remainingUnits--;
            amount = 1;
            if (IsDepleted)
            {
                gameObject.SetActive(false);
            }

            return true;
        }
    }
}
