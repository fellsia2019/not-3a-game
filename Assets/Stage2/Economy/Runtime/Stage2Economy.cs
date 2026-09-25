using System;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    public sealed class Stage2Inventory
    {
        private readonly int[] amounts = new int[3];
        public event Action Changed;

        public int Get(MapResourceKind kind) => amounts[Index(kind)];

        public bool TryAdd(MapResourceKind kind, int amount)
        {
            var index = Index(kind);
            if (amount <= 0 || amount > int.MaxValue - amounts[index]) return false;
            amounts[index] += amount;
            Changed?.Invoke();
            return true;
        }

        public bool CanAfford(int wood, int stone, int metal) =>
            wood >= 0 && stone >= 0 && metal >= 0 &&
            amounts[0] >= wood && amounts[1] >= stone && amounts[2] >= metal;

        public bool TrySpend(int wood, int stone, int metal)
        {
            if (!CanAfford(wood, stone, metal)) return false;
            if (wood == 0 && stone == 0 && metal == 0) return true;
            amounts[0] -= wood;
            amounts[1] -= stone;
            amounts[2] -= metal;
            Changed?.Invoke();
            return true;
        }

        public void Reset()
        {
            if (amounts[0] == 0 && amounts[1] == 0 && amounts[2] == 0) return;
            Array.Clear(amounts, 0, amounts.Length);
            Changed?.Invoke();
        }

        private static int Index(MapResourceKind kind)
        {
            if (kind < MapResourceKind.Wood || kind > MapResourceKind.Metal)
                throw new ArgumentOutOfRangeException(nameof(kind));
            return (int)kind;
        }
    }

    public sealed class Stage2ResourceNode
    {
        public Stage2ResourceNode(Stage2ResourceSettings settings, Vector2Int cell)
        {
            if (settings == null || !settings.IsValid) throw new ArgumentException("Invalid resource settings.", nameof(settings));
            Resource = settings.Resource;
            Cell = cell;
            Capacity = settings.Capacity;
            GatherSeconds = settings.GatherSeconds;
            RecoverySeconds = settings.RecoverySeconds;
            Remaining = Capacity;
        }

        public MapResourceKind Resource { get; }
        public Vector2Int Cell { get; }
        public int Capacity { get; }
        public float GatherSeconds { get; }
        public float RecoverySeconds { get; }
        public int Remaining { get; private set; }
        public float RecoveryRemaining { get; private set; }
        public bool IsDepleted => Remaining == 0;

        internal bool TryTakeUnit()
        {
            if (IsDepleted) return false;
            Remaining--;
            if (Remaining == 0) RecoveryRemaining = RecoverySeconds;
            return true;
        }

        public void Tick(float activeSeconds)
        {
            if (activeSeconds < 0f || float.IsNaN(activeSeconds) || float.IsInfinity(activeSeconds))
                throw new ArgumentOutOfRangeException(nameof(activeSeconds));
            if (!IsDepleted || activeSeconds == 0f) return;
            RecoveryRemaining = Mathf.Max(0f, RecoveryRemaining - activeSeconds);
            if (RecoveryRemaining > 0f) return;
            Remaining = Capacity;
        }

        public void Reset()
        {
            Remaining = Capacity;
            RecoveryRemaining = 0f;
        }
    }

    public sealed class Stage2Economy
    {
        private readonly Stage2MapDefinition map;
        private readonly Stage2ResourceNode[] nodes = new Stage2ResourceNode[3];
        private readonly float interactionRangeSquared;
        private int currentKind = -1;
        private float gatherProgress;

        public Stage2Economy(Stage2MapDefinition map, Stage2EconomyCatalog catalog)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var validation = map.ValidateDefinition();
            if (!validation.IsValid) throw new ArgumentException(validation.FirstError, nameof(map));
            if (!catalog.TryValidate(out var reason)) throw new ArgumentException(reason, nameof(catalog));
            this.map = map;
            interactionRangeSquared = catalog.InteractionRange * catalog.InteractionRange;
            for (var index = 0; index < nodes.Length; index++)
            {
                var kind = (MapResourceKind)index;
                if (!map.TryGetResourceRegion(kind, out var region))
                    throw new ArgumentException($"Missing {kind} source.", nameof(map));
                nodes[index] = new Stage2ResourceNode(catalog.Get(kind), region.Anchor);
            }
            Inventory = new Stage2Inventory();
        }

        public Stage2Inventory Inventory { get; }
        public event Action<MapResourceKind, int> Gathered;
        public float GatherProgress => gatherProgress;

        public Stage2ResourceNode GetNode(MapResourceKind kind)
        {
            if (kind < MapResourceKind.Wood || kind > MapResourceKind.Metal)
                throw new ArgumentOutOfRangeException(nameof(kind));
            return nodes[(int)kind];
        }

        public Vector2 WorldPosition(MapResourceKind kind) => map.CellToWorld(GetNode(kind).Cell);

        public void Tick(float elapsedSeconds, bool activeTime)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!activeTime) return;
            foreach (var node in nodes) node.Tick(elapsedSeconds);
        }

        // The caller supplies the held state and unpaused Preparation/Wave state.
        // A completed interaction consumes exactly one unit and emits one fact.
        public bool AdvanceGather(MapResourceKind kind, Vector2 playerWorld, bool held, float elapsedSeconds, bool allowed)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            var node = GetNode(kind);
            var inRange = ((Vector2)playerWorld - WorldPosition(kind)).sqrMagnitude <= interactionRangeSquared;
            if (!allowed || !held || !inRange || node.IsDepleted)
            {
                CancelGather();
                return false;
            }
            if (currentKind != (int)kind)
            {
                currentKind = (int)kind;
                gatherProgress = 0f;
            }
            gatherProgress += elapsedSeconds;
            if (gatherProgress < node.GatherSeconds) return false;
            gatherProgress = 0f;
            if (Inventory.Get(kind) == int.MaxValue || !node.TryTakeUnit()) return false;
            Inventory.TryAdd(kind, 1);
            Gathered?.Invoke(kind, 1);
            return true;
        }

        public void CancelGather()
        {
            currentKind = -1;
            gatherProgress = 0f;
        }

        public void Reset()
        {
            CancelGather();
            foreach (var node in nodes) node.Reset();
            Inventory.Reset();
        }
    }
}
