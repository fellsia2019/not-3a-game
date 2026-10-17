using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage2.Map
{
    [Serializable]
    public sealed class ResourceDistrictSettings
    {
        [Min(1)] public int minimumCells = 200;
        [Min(1)] public int maximumCells = 300;
        [Min(1)] public int districtsPerResource = 2;
        [Min(1)] public int roadClearance = 6;
        [Min(1)] public int districtSpacing = 5;
        [Min(1)] public int worldEdgeMargin = 4;
        [Min(1)] public int anchorClearance = 7;

        public void Validate()
        {
            if (minimumCells < 1 || maximumCells < minimumCells || maximumCells > 10000 ||
                districtsPerResource < 1 || districtsPerResource > 20 || roadClearance < 1 ||
                districtSpacing < 1 || worldEdgeMargin < 1 || anchorClearance < 1)
                throw new ArgumentException("Invalid resource district settings.");
        }
    }

    public sealed class ResourceDistrict
    {
        internal ResourceDistrict(MapResourceKind kind, Vector2Int center, int seed, List<Vector2Int> cells,
            bool cutsTerrain = true)
        {
            Kind = kind; Center = center; Seed = seed; CutsTerrain = cutsTerrain;
            Cells = cells.AsReadOnly();
            var min = center; var max = center;
            foreach (var cell in cells) { min = Vector2Int.Min(min, cell); max = Vector2Int.Max(max, cell); }
            Bounds = new RectInt(min, max - min + Vector2Int.one);
        }
        public MapResourceKind Kind { get; }
        public Vector2Int Center { get; }
        public int Seed { get; }
        public bool CutsTerrain { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }
        public RectInt Bounds { get; }
    }

    /// <summary>Seeded connected footprints. No scene objects, global Random or authored-map mutation.</summary>
    public static class ResourceDistrictGenerator
    {
        private static readonly Vector2Int[] Neighbors = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        public static IReadOnlyList<ResourceDistrict> Generate(Stage2MapDefinition map,
            ResourceDistrictSettings settings, int seed, Vector2 heroStart)
            => Generate(map, settings, seed, heroStart, null);

        /// <summary>Authored local footprints keep their composition; only safe origins vary by seed.</summary>
        public static IReadOnlyList<ResourceDistrict> Generate(Stage2MapDefinition map,
            ResourceDistrictSettings settings, int seed, Vector2 heroStart,
            IReadOnlyDictionary<MapResourceKind, IReadOnlyList<Vector2Int>> authoredFootprints)
        {
            if (map == null || !map.PlanarGrid) throw new ArgumentException("A planar map is required.");
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            var size = map.Size;
            var minimum = map.MinimumCell;
            var available = new bool[size.x, size.y];
            for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
                available[x,y] = x >= settings.worldEdgeMargin && y >= settings.worldEdgeMargin &&
                    x < size.x - settings.worldEdgeMargin && y < size.y - settings.worldEdgeMargin;
            void Reserve(Vector2Int cell, int radius)
            {
                for (var x = Mathf.Max(0, cell.x - minimum.x - radius); x <= Mathf.Min(size.x - 1, cell.x - minimum.x + radius); x++)
                for (var y = Mathf.Max(0, cell.y - minimum.y - radius); y <= Mathf.Min(size.y - 1, cell.y - minimum.y + radius); y++)
                    available[x,y] = false;
            }
            for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
            {
                var cell = minimum + new Vector2Int(x,y);
                if (map.GetSurface(cell) != MapSurfaceKind.Forest || !map.IsWalkable(cell)) Reserve(cell, settings.roadClearance);
            }
            Reserve(map.EnemyEntrance, settings.anchorClearance);
            Reserve(map.Throne, settings.anchorClearance);
            Reserve(map.WorldToCell(heroStart), settings.anchorClearance);
            for (var i = 0; i < map.ResourceRegionCount; i++) Reserve(map.GetResourceRegion(i).Anchor, settings.anchorClearance);

            var rng = new System.Random(seed);
            var result = new List<ResourceDistrict>();
            // The first set is nearer the existing start, then use the wider exploration world.
            for (var set = 0; set < settings.districtsPerResource; set++)
            for (var resource = 0; resource < 3; resource++)
            {
                ResourceDistrict accepted = null;
                var area = rng.Next(settings.minimumCells, settings.maximumCells + 1);
                var shapeSeed = rng.Next();
                var authored = authoredFootprints != null && authoredFootprints.TryGetValue(
                    (MapResourceKind)resource, out _);
                var localShape = authored
                    ? new List<Vector2Int>(authoredFootprints[(MapResourceKind)resource])
                    : MakeShape(area, shapeSeed);
                if (localShape.Count == 0 || new HashSet<Vector2Int>(localShape).Count != localShape.Count)
                    throw new ArgumentException("An authored footprint must contain distinct reserved cells.");
                for (var attempt = 0; attempt < 3000; attempt++)
                {
                    Vector2Int center;
                    if (set == 0 && attempt < 600)
                    {
                        var angle = rng.NextDouble() * Math.PI * 2;
                        var distance = 25 + rng.NextDouble() * 27;
                        center = map.WorldToCell(heroStart) + new Vector2Int(
                            (int)Math.Round(Math.Cos(angle) * distance), (int)Math.Round(Math.Sin(angle) * distance));
                    }
                    else center = minimum + new Vector2Int(rng.Next(size.x), rng.Next(size.y));
                    var valid = true;
                    foreach (var offset in localShape)
                    {
                        var p = center + offset - minimum;
                        if (p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y || !available[p.x,p.y])
                        { valid = false; break; }
                    }
                    if (!valid) continue;
                    var cells = new List<Vector2Int>(area);
                    foreach (var offset in localShape) cells.Add(center + offset);
                    accepted = new ResourceDistrict((MapResourceKind)resource, center, shapeSeed, cells, !authored);
                    foreach (var cell in cells) Reserve(cell, settings.districtSpacing);
                    break;
                }
                if (accepted == null) throw new InvalidOperationException("Cannot fit resource districts without violating reserved areas.");
                result.Add(accepted);
            }
            return result.AsReadOnly();
        }

        private readonly struct Frontier : IComparable<Frontier>
        {
            public Frontier(Vector2Int cell, double score) { Cell = cell; Score = score; }
            public Vector2Int Cell { get; }
            public double Score { get; }
            public int CompareTo(Frontier other)
            {
                var score = Score.CompareTo(other.Score);
                if (score != 0) return score;
                var y = Cell.y.CompareTo(other.Cell.y);
                return y != 0 ? y : Cell.x.CompareTo(other.Cell.x);
            }
        }

        private static List<Vector2Int> MakeShape(int count, int seed)
        {
            var random = new System.Random(seed);
            var aspect = .72 + random.NextDouble() * .75;
            var turn = random.NextDouble() * Math.PI;
            var phase = random.NextDouble() * Math.PI * 2;
            var visited = new HashSet<Vector2Int>();
            var frontier = new SortedSet<Frontier>();
            var result = new List<Vector2Int>(count);
            void Add(Vector2Int cell)
            {
                if (!visited.Add(cell)) return;
                var x = (cell.x * Math.Cos(turn) - cell.y * Math.Sin(turn)) / aspect;
                var y = (cell.x * Math.Sin(turn) + cell.y * Math.Cos(turn)) * aspect;
                var angle = Math.Atan2(y, x);
                var edge = 1 + .17 * Math.Sin(angle * 3 + phase) + .10 * Math.Sin(angle * 5 - phase);
                frontier.Add(new Frontier(cell, (x*x + y*y) / (edge*edge)));
            }
            Add(Vector2Int.zero);
            while (result.Count < count)
            {
                var next = frontier.Min;
                frontier.Remove(next);
                result.Add(next.Cell);
                foreach (var direction in Neighbors) Add(next.Cell + direction);
            }
            result.Sort((a,b) => a.y == b.y ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            return result;
        }
    }
}
