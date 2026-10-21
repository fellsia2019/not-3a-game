using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    public sealed class Stage2Inventory
    {
        public readonly struct State
        {
            internal readonly int Wood, Stone, Metal, Gold;
            internal State(int wood, int stone, int metal, int gold)
            { Wood = wood; Stone = stone; Metal = metal; Gold = gold; }
        }
        private readonly int[] amounts = new int[3];
        private int gold;
        public event Action Changed;

        public int Get(MapResourceKind kind) => amounts[Index(kind)];
        public int Gold => gold;

        public bool CanAfford(Stage2ResourceCost cost) => cost.IsValid && CanAfford(cost.Wood, cost.Stone, cost.Metal, cost.Gold);
        public bool TrySpend(Stage2ResourceCost cost) => cost.IsValid && TrySpend(cost.Wood, cost.Stone, cost.Metal, cost.Gold);
        public bool CanCredit(Stage2ResourceCost cost) => cost.IsValid &&
            cost.Wood <= int.MaxValue - amounts[0] && cost.Stone <= int.MaxValue - amounts[1] &&
            cost.Metal <= int.MaxValue - amounts[2] && cost.Gold <= int.MaxValue - gold;
        public bool TryCredit(Stage2ResourceCost cost)
        {
            if (!CanCredit(cost)) return false;
            if (cost.IsZero) return true;
            amounts[0] += cost.Wood; amounts[1] += cost.Stone; amounts[2] += cost.Metal; gold += cost.Gold;
            Changed?.Invoke();
            return true;
        }

        public State CaptureState() => new State(amounts[0], amounts[1], amounts[2], gold);
        public void RestoreState(State state)
        {
            amounts[0] = state.Wood; amounts[1] = state.Stone; amounts[2] = state.Metal; gold = state.Gold;
            Changed?.Invoke();
        }

        public bool TryAddGold(int amount)
        {
            if (amount <= 0 || amount > int.MaxValue - gold) return false;
            gold += amount;
            Changed?.Invoke();
            return true;
        }

        public bool TryAdd(MapResourceKind kind, int amount)
        {
            var index = Index(kind);
            if (amount <= 0 || amount > int.MaxValue - amounts[index]) return false;
            amounts[index] += amount;
            Changed?.Invoke();
            return true;
        }

        public bool CanAfford(int wood, int stone, int metal, int requiredGold = 0) =>
            wood >= 0 && stone >= 0 && metal >= 0 && requiredGold >= 0 &&
            amounts[0] >= wood && amounts[1] >= stone && amounts[2] >= metal && gold >= requiredGold;

        public bool TrySpend(int wood, int stone, int metal, int requiredGold = 0)
        {
            if (!CanAfford(wood, stone, metal, requiredGold)) return false;
            if (wood == 0 && stone == 0 && metal == 0 && requiredGold == 0) return true;
            amounts[0] -= wood;
            amounts[1] -= stone;
            amounts[2] -= metal;
            gold -= requiredGold;
            Changed?.Invoke();
            return true;
        }

        public void Reset(int initialGold = 0)
        {
            if (initialGold < 0) throw new ArgumentOutOfRangeException(nameof(initialGold));
            if (amounts[0] == 0 && amounts[1] == 0 && amounts[2] == 0 && gold == initialGold) return;
            Array.Clear(amounts, 0, amounts.Length);
            gold = initialGold;
            Changed?.Invoke();
        }

        private static int Index(MapResourceKind kind)
        {
            if (kind < MapResourceKind.Wood || kind > MapResourceKind.Metal)
                throw new ArgumentOutOfRangeException(nameof(kind));
            return (int)kind;
        }
    }

    public enum Stage2ResourceSize { Small, Medium, Large }

    public sealed class Stage2ResourceNode
    {
        private readonly float[] harvestRadii;
        public Stage2ResourceNode(Stage2ResourceSettings settings, Vector2Int cell)
            : this(settings, cell, Vector2.zero, Stage2ResourceSize.Small, 0f) { }

        public Stage2ResourceNode(Stage2ResourceSettings settings, Vector2Int cell, Vector2 worldPosition,
            Stage2ResourceSize size, float harvestRadius, int groupId = -1, int variantSeed = 0, float[] stageHarvestRadii = null)
        {
            if (settings == null || !settings.IsValid) throw new ArgumentException("Invalid resource settings.", nameof(settings));
            if (float.IsNaN(worldPosition.x) || float.IsNaN(worldPosition.y) ||
                float.IsInfinity(worldPosition.x) || float.IsInfinity(worldPosition.y) ||
                harvestRadius < 0f || float.IsNaN(harvestRadius) || float.IsInfinity(harvestRadius) ||
                size < Stage2ResourceSize.Small || size > Stage2ResourceSize.Large)
                throw new ArgumentException("Invalid resource placement.");
            Resource = settings.Resource;
            Cell = cell;
            WorldPosition = worldPosition;
            Size = size;
            HarvestRadius = harvestRadius;
            if (stageHarvestRadii != null)
            {
                if (stageHarvestRadii.Length != 4) throw new ArgumentException("Four stock-stage radii required.");
                var previous = harvestRadius + .0001f;
                foreach (var radius in stageHarvestRadii)
                {
                    if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius) || radius > previous)
                        throw new ArgumentException("Stock-stage radii must be positive and nonincreasing.");
                    previous = radius;
                }
                harvestRadii = (float[])stageHarvestRadii.Clone();
            }
            GroupId = groupId;
            VariantSeed = variantSeed;
            Capacity = settings.Capacity;
            GatherSeconds = settings.GatherSeconds;
            RecoverySeconds = settings.RecoverySeconds;
            Remaining = Capacity;
        }

        public MapResourceKind Resource { get; }
        public Vector2Int Cell { get; }
        public Vector2 WorldPosition { get; }
        public Stage2ResourceSize Size { get; }
        public float HarvestRadius { get; }
        public float InteractionRadius => harvestRadii == null ? HarvestRadius : harvestRadii[Stage2ManualResourceSettings.HarvestState(FillFraction)];
        public int GroupId { get; }
        public int VariantSeed { get; }
        public int Capacity { get; }
        public float GatherSeconds { get; }
        public float RecoverySeconds { get; }
        public int Remaining { get; private set; }
        public float RecoveryRemaining { get; private set; }
        public bool IsDepleted => Remaining == 0;
        public float FillFraction => Remaining / (float)Capacity;

        internal bool TryTakeUnit()
            => TryTakeUnits(1) == 1;

        internal int TryTakeUnits(int amount)
        {
            if (IsDepleted || amount <= 0) return 0;
            var taken = Math.Min(amount, Remaining);
            Remaining -= taken;
            if (Remaining == 0) RecoveryRemaining = RecoverySeconds;
            return taken;
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

        internal void RestoreStock(int remaining, float recovery)
        { Remaining = remaining; RecoveryRemaining = recovery; }
    }

    public sealed class Stage2Economy
    {
        public sealed class Snapshot
        {
            internal readonly Stage2ResourceNode[] Nodes;
            internal readonly int[] Remaining;
            internal readonly float[] Recovery;
            internal readonly Stage2Inventory.State Inventory;
            internal Snapshot(Stage2Economy owner)
            {
                Nodes = owner.nodes.ToArray(); Remaining = new int[Nodes.Length]; Recovery = new float[Nodes.Length];
                for (var i = 0; i < Nodes.Length; i++)
                { Remaining[i] = Nodes[i].Remaining; Recovery[i] = Nodes[i].RecoveryRemaining; }
                Inventory = owner.Inventory.CaptureState();
            }
        }
        private readonly Stage2MapDefinition map;
        private readonly int startingGold;
        private readonly Stage2ResourceNode[] anchors = new Stage2ResourceNode[3];
        private readonly List<Stage2ResourceNode> nodes = new List<Stage2ResourceNode>();
        private readonly HashSet<Stage2ResourceNode> ownedNodes = new HashSet<Stage2ResourceNode>();
        private readonly IReadOnlyList<Stage2ResourceNode> nodeView;
        private readonly float interactionRange;
        private readonly Stage2ManualResourceSettings manualResources;
        private float gatherProgress;
        private int axePower = 1, pickaxePower = 1;
        private float axeSpeed = 1f, pickaxeSpeed = 1f;
        private int axeProfessionPower, pickaxeProfessionPower;
        private float axeProfessionSpeed = 1f, pickaxeProfessionSpeed = 1f;
        private float woodReachMultiplier = 1f, mineralReachMultiplier = 1f;

        public int HarvestPower(MapResourceKind kind)
        {
            var tool = kind == MapResourceKind.Wood ? axePower : pickaxePower;
            var bonus = kind == MapResourceKind.Wood ? axeProfessionPower : pickaxeProfessionPower;
            return tool <= 0 ? 0 : (int)Math.Min(int.MaxValue, (long)tool + bonus);
        }
        public float GatherDuration(Stage2ResourceNode node) => node.GatherSeconds /
            (node.Resource == MapResourceKind.Wood ? axeSpeed * axeProfessionSpeed : pickaxeSpeed * pickaxeProfessionSpeed);
        public void SetManualProfessionStats(int woodPower, int mineralPower, float woodSpeed, float mineralSpeed,
            float woodReach, float mineralReach)
        {
            if (woodPower < 0 || mineralPower < 0 || !float.IsFinite(woodSpeed) || woodSpeed < 1 ||
                !float.IsFinite(mineralSpeed) || mineralSpeed < 1 || !float.IsFinite(woodReach) || woodReach < 1 ||
                !float.IsFinite(mineralReach) || mineralReach < 1) throw new ArgumentException("Invalid profession bonuses.");
            if (axeProfessionPower == woodPower && pickaxeProfessionPower == mineralPower &&
                axeProfessionSpeed == woodSpeed && pickaxeProfessionSpeed == mineralSpeed &&
                woodReachMultiplier == woodReach && mineralReachMultiplier == mineralReach) return;
            CancelGather(); axeProfessionPower = woodPower; pickaxeProfessionPower = mineralPower;
            axeProfessionSpeed = woodSpeed; pickaxeProfessionSpeed = mineralSpeed;
            woodReachMultiplier = woodReach; mineralReachMultiplier = mineralReach;
        }
        public void SetManualToolStats(int axe, int pickaxe, float woodSpeed, float mineralSpeed)
        {
            if (axe < 0 || pickaxe < 0 || !float.IsFinite(woodSpeed) || woodSpeed <= 0 ||
                !float.IsFinite(mineralSpeed) || mineralSpeed <= 0) throw new ArgumentException("Invalid manual tool stats.");
            if (axePower == axe && pickaxePower == pickaxe && axeSpeed == woodSpeed && pickaxeSpeed == mineralSpeed) return;
            CancelGather(); axePower = axe; pickaxePower = pickaxe; axeSpeed = woodSpeed; pickaxeSpeed = mineralSpeed;
        }

        public Stage2Economy(Stage2MapDefinition map, Stage2EconomyCatalog catalog)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var validation = map.ValidateDefinition();
            if (!validation.IsValid) throw new ArgumentException(validation.FirstError, nameof(map));
            if (!catalog.TryValidate(out var reason)) throw new ArgumentException(reason, nameof(catalog));
            this.map = map;
            startingGold = catalog.StartingGold;
            GoldPerKill = catalog.GoldPerKill;
            interactionRange = catalog.InteractionRange;
            manualResources = catalog.ManualResources;
            for (var index = 0; index < anchors.Length; index++)
            {
                var kind = (MapResourceKind)index;
                if (!map.TryGetResourceRegion(kind, out var region))
                    throw new ArgumentException($"Missing {kind} source.", nameof(map));
                anchors[index] = new Stage2ResourceNode(catalog.Get(kind), region.Anchor,
                    map.CellToWorld(region.Anchor), Stage2ResourceSize.Small,
                    manualResources != null && kind != MapResourceKind.Wood ? manualResources.smallHarvestRadius : 0f,
                    stageHarvestRadii: manualResources?.HarvestRadii(kind, Stage2ResourceSize.Small));
                nodes.Add(anchors[index]);
                ownedNodes.Add(anchors[index]);
            }
            nodeView = nodes.AsReadOnly();
            Inventory = new Stage2Inventory();
            Inventory.Reset(startingGold);
        }

        public Stage2Inventory Inventory { get; }
        public int GoldPerKill { get; }
        public event Action<MapResourceKind, int> Gathered;
        public event Action<Stage2ResourceNode> NodeGathered;
        public event Action NodesChanged;
        public float GatherProgress => gatherProgress;
        public Stage2ResourceNode CurrentNode { get; private set; }
        public IReadOnlyList<Stage2ResourceNode> Nodes => nodeView;

        public Snapshot CaptureSnapshot() => new Snapshot(this);
        public bool MatchesSnapshot(Snapshot snapshot)
        {
            if (snapshot == null || snapshot.Nodes.Length != nodes.Count) return false;
            for (var i = 0; i < nodes.Count; i++) if (!ReferenceEquals(nodes[i], snapshot.Nodes[i])) return false;
            return true;
        }
        public void RestoreSnapshot(Snapshot snapshot)
        {
            if (!MatchesSnapshot(snapshot)) throw new ArgumentException("Snapshot belongs to a different resource world.", nameof(snapshot));
            CancelGather();
            for (var i = 0; i < nodes.Count; i++) nodes[i].RestoreStock(snapshot.Remaining[i], snapshot.Recovery[i]);
            Inventory.RestoreState(snapshot.Inventory);
        }

        public Stage2ResourceNode GetNode(MapResourceKind kind)
        {
            if (kind < MapResourceKind.Wood || kind > MapResourceKind.Metal)
                throw new ArgumentOutOfRangeException(nameof(kind));
            return anchors[(int)kind];
        }

        public Vector2 WorldPosition(MapResourceKind kind) => GetNode(kind).WorldPosition;

        public void ReplaceAdditionalNodes(IReadOnlyList<Stage2ResourceNode> additional)
        {
            if (additional == null) throw new ArgumentNullException(nameof(additional));
            var next = new HashSet<Stage2ResourceNode>(anchors);
            foreach (var node in additional)
                if (node == null || !map.Contains(node.Cell) || !next.Add(node))
                    throw new ArgumentException("Additional resources must be unique valid world nodes.", nameof(additional));
            CancelGather();
            nodes.Clear(); nodes.AddRange(anchors); ownedNodes.Clear();
            foreach (var node in anchors) ownedNodes.Add(node);
            foreach (var node in additional) { nodes.Add(node); ownedNodes.Add(node); }
            NodesChanged?.Invoke();
        }

        public bool IsInRange(Stage2ResourceNode node, Vector2 playerWorld) => node != null &&
            (playerWorld - node.WorldPosition).sqrMagnitude <=
            (InteractionRange(node) + node.InteractionRadius) * (InteractionRange(node) + node.InteractionRadius);

        public float InteractionRange(Stage2ResourceNode node) =>
            (manualResources == null ? interactionRange : node.Resource == MapResourceKind.Wood ?
                manualResources.treeInteractionRange : manualResources.MineralInteractionRange(node.Size)) *
            (node.Resource == MapResourceKind.Wood ? woodReachMultiplier : mineralReachMultiplier);

        public Stage2ResourceNode NearestAvailableNode(Vector2 playerWorld)
        {
            Stage2ResourceNode nearest = null;
            var best = float.PositiveInfinity;
            foreach (var node in nodes)
            {
                if (node.IsDepleted) continue;
                var distance = Mathf.Max(0f, Vector2.Distance(playerWorld, node.WorldPosition) - node.InteractionRadius);
                if (distance > InteractionRange(node) || distance > best) continue;
                best = distance; nearest = node;
            }
            return nearest;
        }

        public void Tick(float elapsedSeconds, bool activeTime)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!activeTime) return;
            foreach (var node in nodes) node.Tick(elapsedSeconds);
        }

        // The caller supplies the held state and unpaused Preparation/Wave state.
        // A completed strike consumes at most the tool power and remaining stock, emitting one strike fact.
        public bool AdvanceGather(MapResourceKind kind, Vector2 playerWorld, bool held, float elapsedSeconds, bool allowed)
            => AdvanceGather(GetNode(kind), playerWorld, held, elapsedSeconds, allowed);

        public bool AdvanceGather(Stage2ResourceNode node, Vector2 playerWorld, bool held, float elapsedSeconds, bool allowed)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (node == null || !ownedNodes.Contains(node) || !allowed || !held ||
                !IsInRange(node, playerWorld) || node.IsDepleted || HarvestPower(node.Resource) <= 0)
            {
                CancelGather();
                return false;
            }
            if (CurrentNode != node)
            {
                CurrentNode = node;
                gatherProgress = 0f;
            }
            gatherProgress += elapsedSeconds;
            if (gatherProgress < GatherDuration(node)) return false;
            gatherProgress = 0f;
            var grant = Math.Min(HarvestPower(node.Resource), int.MaxValue - Inventory.Get(node.Resource));
            grant = node.TryTakeUnits(grant);
            if (grant == 0) return false;
            Inventory.TryAdd(node.Resource, grant);
            NodeGathered?.Invoke(node);
            Gathered?.Invoke(node.Resource, grant);
            return true;
        }

        public void CancelGather()
        {
            CurrentNode = null;
            gatherProgress = 0f;
        }

        public void Reset()
        {
            CancelGather();
            foreach (var node in nodes) node.Reset();
            Inventory.Reset(startingGold);
        }
    }
}
