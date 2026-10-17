using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    public enum ProductionPlacementResult { NotPlacing, Ready, Built, Inactive, InvalidPoint, WrongType, Occupied, HeroBlocked, InsufficientResources, MissingVisual, InvalidFootprint }
    public sealed class Stage2ProductionBuilding
    {
        internal Stage2ProductionBuilding(int pointIndex, Stage2ProductionEntry entry, Rect footprint)
        { PointIndex = pointIndex; Entry = entry; Footprint = footprint; }
        public int PointIndex { get; }
        public Stage2ProductionEntry Entry { get; }
        public Rect Footprint { get; }
        public double ProgressSeconds { get; internal set; }
    }

    /// <summary>Locked-point construction and active-time resource production. No scene lifecycle.</summary>
    public sealed class Stage2Production
    {
        private readonly Stage2ProductionCatalog catalog;
        private readonly Stage2Inventory inventory;
        private ResourceProductionPoint[] points = Array.Empty<ResourceProductionPoint>();
        private readonly List<Stage2ProductionBuilding> buildings = new List<Stage2ProductionBuilding>();
        private IReadOnlyList<ResourceProductionPoint> pointView;
        private readonly IReadOnlyList<Stage2ProductionBuilding> buildingView;
        private bool active;
        private bool purchasing;
        public Stage2Production(Stage2ProductionCatalog catalog, Stage2Inventory inventory)
        {
            if (catalog == null || !catalog.TryValidate(out _)) throw new ArgumentException("Invalid production catalog.");
            this.catalog = catalog; this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            pointView = Array.AsReadOnly(points);
            buildingView = buildings.AsReadOnly();
        }
        public IReadOnlyList<ResourceProductionPoint> Points => pointView;
        public IReadOnlyList<Stage2ProductionBuilding> Buildings => buildingView;
        public void SetActive(bool value) => active = value;
        public void BindPoints(IReadOnlyList<ResourceProductionPoint> values)
        {
            Reset(); points = new ResourceProductionPoint[values.Count];
            for (var i = 0; i < points.Length; i++) points[i] = values[i];
            pointView = Array.AsReadOnly(points);
        }
        public void Reset() => buildings.Clear();
        public bool IsOccupied(int index) { foreach (var building in buildings) if (building.PointIndex == index) return true; return false; }
        public Rect Footprint(int index, Stage2ProductionEntry entry) => new Rect(points[index].Position - entry.FootprintSize * .5f, entry.FootprintSize);
        public ProductionPlacementResult Evaluate(int index, ResourceProductionBuilding kind, Vector2 hero, float radius)
        {
            if (!active) return ProductionPlacementResult.Inactive;
            if (index < 0 || index >= points.Length) return ProductionPlacementResult.InvalidPoint;
            var point = points[index];
            if (!point.Accepts(kind)) return ProductionPlacementResult.WrongType;
            if (IsOccupied(index)) return ProductionPlacementResult.Occupied;
            var entry = catalog.Get(kind);
            if (!entry.IsValid) return ProductionPlacementResult.MissingVisual;
            var footprint = Footprint(index, entry);
            if (footprint.xMin < point.Clearance.xMin || footprint.xMax > point.Clearance.xMax ||
                footprint.yMin < point.Clearance.yMin || footprint.yMax > point.Clearance.yMax)
                return ProductionPlacementResult.InvalidFootprint;
            var nearest = new Vector2(Mathf.Clamp(hero.x, footprint.xMin, footprint.xMax), Mathf.Clamp(hero.y, footprint.yMin, footprint.yMax));
            if ((hero - nearest).sqrMagnitude <= radius * radius) return ProductionPlacementResult.HeroBlocked;
            return inventory.CanAfford(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost)
                ? ProductionPlacementResult.Ready : ProductionPlacementResult.InsufficientResources;
        }
        public ProductionPlacementResult TryBuild(int index, ResourceProductionBuilding kind, Vector2 hero, float radius,
            Func<bool> prepareInactiveVisual, out Stage2ProductionBuilding built)
        {
            built = null;
            if (purchasing) return ProductionPlacementResult.Inactive;
            var result = Evaluate(index, kind, hero, radius);
            if (result != ProductionPlacementResult.Ready) return result;
            purchasing = true;
            try
            {
                if (prepareInactiveVisual == null) return ProductionPlacementResult.MissingVisual;
                bool prepared;
                try { prepared = prepareInactiveVisual(); }
                catch (Exception) { return ProductionPlacementResult.MissingVisual; }
                if (!prepared) return ProductionPlacementResult.MissingVisual;
                result = Evaluate(index, kind, hero, radius);
                if (result != ProductionPlacementResult.Ready) return result;
                var entry = catalog.Get(kind);
                var candidate = new Stage2ProductionBuilding(index, entry, Footprint(index, entry));
                if (!inventory.TrySpend(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost))
                    return ProductionPlacementResult.InsufficientResources;
                buildings.Add(candidate); built = candidate; return ProductionPlacementResult.Built;
            }
            finally { purchasing = false; }
        }
        public void Tick(float elapsed)
        {
            if (elapsed < 0 || float.IsNaN(elapsed) || float.IsInfinity(elapsed)) throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (!active || elapsed == 0) return;
            foreach (var building in buildings)
            {
                var entry = building.Entry;
                var total = building.ProgressSeconds + elapsed;
                var cycles = Math.Floor(total / entry.IntervalSeconds);
                building.ProgressSeconds = total % entry.IntervalSeconds;
                if (cycles < 1) continue;
                var space = int.MaxValue - inventory.Get(entry.Resource);
                var produced = (int)Math.Min(space, cycles * entry.OutputAmount);
                if (produced > 0) inventory.TryAdd(entry.Resource, produced);
                // Overflow output is discarded; it cannot turn into a deferred burst later.
            }
        }
    }
}
