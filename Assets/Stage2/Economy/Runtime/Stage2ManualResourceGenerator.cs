using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage2.Economy
{
    [Serializable]
    public sealed class Stage2ManualResourceSettings
    {
        [Min(0)] public int treeGroups = 72;
        [Min(0)] public int smallDepositsPerKind = 48;
        [Min(0)] public int mediumDepositsPerKind = 24;
        [Min(0)] public int largeDepositsPerKind = 8;
        [Min(1)] public int mediumCapacity = 35;
        [Min(1)] public int largeCapacity = 120;
        [Min(.1f)] public float treeGroupRadius = 3.6f;
        [Min(.1f)] public float treeSpacing = 1.3f;
        [Min(.1f)] public float groupSpacing = 9f;
        [Min(.1f)] public float depositSpacing = 2.6f;
        [Min(.1f)] public float edgeMargin = 3f;
        [Min(.1f)] public float roadClearance = 3f;
        [Min(0f)] public float smallHarvestRadius = .65f;
        [Min(0f)] public float mediumHarvestRadius = 1.1f;
        [Min(0f)] public float largeHarvestRadius = 1.8f;
        [Min(.01f)] public float mineralInteractionRange = .35f;
        [Min(.01f)] public float mediumMineralInteractionRange = .85f;
        [Min(.01f)] public float largeMineralInteractionRange = 1.1f;
        [Min(.01f)] public float treeInteractionRange = .6f;
        [Min(.01f)] public float treeTrunkCollisionRadius = .18f;
        public float[] stoneHarvestRadii = Array.Empty<float>();
        public float[] oreHarvestRadii = Array.Empty<float>();

        public bool IsValid => Count(treeGroups) && Count(smallDepositsPerKind) &&
            Count(mediumDepositsPerKind) && Count(largeDepositsPerKind) && mediumCapacity > 0 &&
            largeCapacity >= mediumCapacity && Positive(treeGroupRadius) && Positive(treeSpacing) &&
            treeGroupRadius >= treeSpacing && Positive(groupSpacing) && Positive(depositSpacing) &&
            Positive(edgeMargin) && Positive(roadClearance) && Nonnegative(smallHarvestRadius) &&
            Nonnegative(mediumHarvestRadius) && Nonnegative(largeHarvestRadius) &&
            Positive(mineralInteractionRange) && Positive(mediumMineralInteractionRange) &&
            Positive(largeMineralInteractionRange) && Positive(treeInteractionRange) && Positive(treeTrunkCollisionRadius) &&
            treeTrunkCollisionRadius < treeInteractionRange &&
            ValidRadii(stoneHarvestRadii) && ValidRadii(oreHarvestRadii);
        private static bool Count(int value) => value >= 0 && value <= 1000;
        private static bool Positive(float value) => value > 0f && Nonnegative(value);
        private static bool Nonnegative(float value) => value >= 0f && !float.IsInfinity(value) && !float.IsNaN(value);

        public float Radius(Stage2ResourceSize size) => size == Stage2ResourceSize.Large ? largeHarvestRadius :
            size == Stage2ResourceSize.Medium ? mediumHarvestRadius : smallHarvestRadius;

        public float MineralInteractionRange(Stage2ResourceSize size) => size == Stage2ResourceSize.Large
            ? largeMineralInteractionRange : size == Stage2ResourceSize.Medium
                ? mediumMineralInteractionRange : mineralInteractionRange;

        public static int HarvestState(float fraction) => fraction > .75f ? 0 : fraction > .5f ? 1 : fraction > .15f ? 2 : 3;

        public float[] HarvestRadii(MapResourceKind kind, Stage2ResourceSize size)
        {
            var source = kind == MapResourceKind.Stone ? stoneHarvestRadii : kind == MapResourceKind.Metal ? oreHarvestRadii : null;
            if (source == null || source.Length != 12) return null;
            var result = new float[4]; Array.Copy(source, (int)size * 4, result, 0, 4); return result;
        }

        private bool ValidRadii(float[] values)
        {
            if (values == null || values.Length == 0) return true;
            if (values.Length != 12) return false;
            for (var size = 0; size < 3; size++)
            {
                var previous = Radius((Stage2ResourceSize)size) + .0001f;
                for (var state = 0; state < 4; state++)
                {
                    var value = values[size * 4 + state];
                    if (!Positive(value) || value > previous) return false;
                    previous = value;
                }
            }
            return true;
        }
    }

    /// <summary>Seeded natural scatter. No scene objects, global random state or changes to map masks.</summary>
    public static class Stage2ManualResourceGenerator
    {
        public static IReadOnlyList<Stage2ResourceNode> Generate(Stage2MapDefinition map,
            Stage2EconomyCatalog catalog, int seed, Func<Vector2, float, bool> excluded = null)
        {
            if (map == null || catalog == null || !catalog.TryValidate(out var reason))
                throw new ArgumentException("Valid map and economy catalog required.");
            var settings = catalog.ManualResources;
            var result = new List<Stage2ResourceNode>();
            if (settings == null) return result.AsReadOnly();
            var random = new System.Random(unchecked(seed ^ 0x32d41b7));
            var occupied = new List<Vector2>();
            var centers = new List<Vector2>();
            for (var i = 0; i < map.ResourceRegionCount; i++) occupied.Add(map.CellToWorld(map.GetResourceRegion(i).Anchor));

            Vector2 Center() => map.CellCoordinatesToWorld(new Vector2(
                Mathf.Lerp(map.MinimumCell.x + settings.edgeMargin, map.MaximumCell.x - settings.edgeMargin, (float)random.NextDouble()),
                Mathf.Lerp(map.MinimumCell.y + settings.edgeMargin, map.MaximumCell.y - settings.edgeMargin, (float)random.NextDouble())));

            bool Fits(Vector2 position, float radius)
            {
                var cellPosition = map.WorldToCellCoordinates(position);
                if (cellPosition.x < map.MinimumCell.x + settings.edgeMargin || cellPosition.x > map.MaximumCell.x - settings.edgeMargin ||
                    cellPosition.y < map.MinimumCell.y + settings.edgeMargin || cellPosition.y > map.MaximumCell.y - settings.edgeMargin)
                    return false;
                // Keep every source and its approach off the defense road, including across cell edges.
                var extent = Mathf.CeilToInt(settings.roadClearance + radius);
                var cell = map.WorldToCell(position);
                for (var y = -extent; y <= extent; y++)
                for (var x = -extent; x <= extent; x++)
                {
                    var sample = cell + new Vector2Int(x, y);
                    if (!map.Contains(sample) || !map.IsWalkable(sample) || map.GetSurface(sample) != MapSurfaceKind.Forest)
                        return false;
                }
                return !(excluded?.Invoke(position, radius) ?? false);
            }

            bool Separate(Vector2 position, float distance, List<Vector2> points)
            {
                foreach (var point in points) if ((position - point).sqrMagnitude < distance * distance) return false;
                return true;
            }

            Stage2ResourceNode Make(MapResourceKind kind, Stage2ResourceSize size, Vector2 position, int group)
            {
                var source = catalog.Get(kind);
                var capacity = size == Stage2ResourceSize.Large ? settings.largeCapacity :
                    size == Stage2ResourceSize.Medium ? settings.mediumCapacity : source.Capacity;
                return new Stage2ResourceNode(new Stage2ResourceSettings(kind, capacity, source.GatherSeconds, source.RecoverySeconds),
                    map.WorldToCell(position), position, size, kind == MapResourceKind.Wood ? 0f : settings.Radius(size),
                    group, random.Next(), settings.HarvestRadii(kind, size));
            }

            for (var group = 0; group < settings.treeGroups; group++)
            {
                var count = random.Next(1, 9);
                var placed = false;
                for (var attempt = 0; attempt < 300 && !placed; attempt++)
                {
                    var center = Center();
                    if (!Separate(center, settings.groupSpacing, centers)) continue;
                    var positions = new List<Vector2>();
                    // Uniform disk samples with minimum trunk spacing: deliberately no lattice, rings or rows.
                    for (var sample = 0; sample < 400 && positions.Count < count; sample++)
                    {
                        var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                        var distance = Mathf.Sqrt((float)random.NextDouble()) * settings.treeGroupRadius;
                        var position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                        if (!Fits(position, 0f) ||
                            !Separate(position, settings.treeSpacing, positions) ||
                            !Separate(position, settings.depositSpacing, occupied)) continue;
                        positions.Add(position);
                    }
                    if (positions.Count != count) continue;
                    centers.Add(center);
                    foreach (var position in positions) result.Add(Make(MapResourceKind.Wood, Stage2ResourceSize.Small, position, group));
                    occupied.AddRange(positions); placed = true;
                }
                if (!placed) throw new InvalidOperationException($"Manual tree group {group} has no clear natural placement.");
            }

            for (var kind = MapResourceKind.Stone; kind <= MapResourceKind.Metal; kind++)
            for (var size = Stage2ResourceSize.Small; size <= Stage2ResourceSize.Large; size++)
            {
                var count = size == Stage2ResourceSize.Small ? settings.smallDepositsPerKind :
                    size == Stage2ResourceSize.Medium ? settings.mediumDepositsPerKind : settings.largeDepositsPerKind;
                for (var i = 0; i < count; i++)
                {
                    var placed = false;
                    for (var attempt = 0; attempt < 3000; attempt++)
                    {
                        var position = Center();
                        if (!Fits(position, settings.Radius(size)) || !Separate(position,
                                settings.depositSpacing + settings.Radius(size), occupied)) continue;
                        result.Add(Make(kind, size, position, -1)); occupied.Add(position); placed = true; break;
                    }
                    if (!placed) throw new InvalidOperationException($"Manual {size} {kind} {i} has no clear placement.");
                }
            }
            return result.AsReadOnly();
        }
    }
}
