using System;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    [Serializable]
    public sealed class Stage2ResourceSettings
    {
        [SerializeField] private MapResourceKind resource;
        [SerializeField, Min(1)] private int capacity = 1;
        [SerializeField, Min(0.01f)] private float gatherSeconds = 1f;
        [SerializeField, Min(0.01f)] private float recoverySeconds = 30f;

        public MapResourceKind Resource => resource;
        public int Capacity => capacity;
        public float GatherSeconds => gatherSeconds;
        public float RecoverySeconds => recoverySeconds;

        public Stage2ResourceSettings(MapResourceKind resource, int capacity, float gatherSeconds, float recoverySeconds)
        {
            this.resource = resource;
            this.capacity = capacity;
            this.gatherSeconds = gatherSeconds;
            this.recoverySeconds = recoverySeconds;
        }

        public bool IsValid => resource >= MapResourceKind.Wood && resource <= MapResourceKind.Metal &&
            capacity > 0 && FinitePositive(gatherSeconds) && FinitePositive(recoverySeconds);

        private static bool FinitePositive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    [CreateAssetMenu(menuName = "Not 3A/Stage 2/Economy Catalog")]
    public sealed class Stage2EconomyCatalog : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float interactionRange = 1.5f;
        [SerializeField] private Stage2ResourceSettings[] resources = Array.Empty<Stage2ResourceSettings>();
        [SerializeField, Min(0)] private int startingGold = 5;
        [SerializeField, Min(0)] private int goldPerKill = 1;

        public float InteractionRange => interactionRange;
        public int StartingGold => startingGold;
        public int GoldPerKill => goldPerKill;

        public void Configure(float range, Stage2ResourceSettings[] settings, int initialGold = 5, int killGold = 1)
        {
            interactionRange = range;
            resources = settings == null ? Array.Empty<Stage2ResourceSettings>() : (Stage2ResourceSettings[])settings.Clone();
            startingGold = initialGold;
            goldPerKill = killGold;
        }

        public bool TryValidate(out string reason)
        {
            if (interactionRange <= 0f || float.IsNaN(interactionRange) || float.IsInfinity(interactionRange))
            {
                reason = "Interaction range must be positive and finite.";
                return false;
            }
            if (startingGold < 0 || goldPerKill < 0)
            {
                reason = "Gold values must be nonnegative.";
                return false;
            }
            if (resources == null || resources.Length != 3)
            {
                reason = "Exactly one wood, stone and metal setting is required.";
                return false;
            }
            var seen = new bool[3];
            foreach (var setting in resources)
            {
                if (setting == null || !setting.IsValid)
                {
                    reason = "Each resource needs positive capacity and finite positive gather/recovery times.";
                    return false;
                }
                var index = (int)setting.Resource;
                if (seen[index])
                {
                    reason = $"Duplicate {setting.Resource} setting.";
                    return false;
                }
                seen[index] = true;
            }
            reason = string.Empty;
            return true;
        }

        public Stage2ResourceSettings Get(MapResourceKind kind)
        {
            if (kind < MapResourceKind.Wood || kind > MapResourceKind.Metal || resources == null)
                throw new ArgumentOutOfRangeException(nameof(kind));
            foreach (var setting in resources)
                if (setting != null && setting.Resource == kind) return setting;
            throw new InvalidOperationException($"No {kind} setting exists in this catalog.");
        }
    }
}
