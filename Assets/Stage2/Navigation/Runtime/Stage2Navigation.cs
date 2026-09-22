using System;
using System.Collections.Generic;
using System.Diagnostics;
using Not3A.Stage2.Map;
using Unity.Profiling;
using UnityEngine;

namespace Not3A.Stage2.Navigation
{
    public enum NavigationStatus
    {
        ReachesThrone, ReachableBlocker, Unreachable, InvalidMap,
        StartOutsideBounds, StartNotWalkable, StartOccupied
    }

    public enum OccupancyResult
    {
        Changed, InvalidMap, InvalidOwner, OutsideBounds, ReservedLandmark,
        NotWalkable, NotBuildable, AlreadyOccupied, Empty, OwnerMismatch
    }

    /// <summary>Immutable route snapshot; check IsCurrent before moving or attacking.</summary>
    public sealed class NavigationPlan
    {
        internal NavigationPlan(Stage2Navigation source, NavigationStatus status, long revision,
            long generation, Vector2Int[] path, Vector2Int? blocker, object owner, string diagnostic)
        {
            Source = source;
            Status = status;
            Revision = revision;
            Generation = generation;
            Path = Array.AsReadOnly(path);
            BlockerCell = blocker;
            BlockerOwner = owner;
            Diagnostic = diagnostic;
        }

        internal Stage2Navigation Source { get; }
        internal long Generation { get; }
        public NavigationStatus Status { get; }
        public long Revision { get; }
        public IReadOnlyList<Vector2Int> Path { get; }
        public Vector2Int? BlockerCell { get; }
        public object BlockerOwner { get; }
        public string Diagnostic { get; }
    }

    /// <summary>Lifetime counters; timings are observations, never frame budgets.</summary>
    public sealed class NavigationDiagnostics
    {
        public long RequestCount { get; internal set; }
        public long FullSearchCount { get; internal set; }
        public long FieldReuseCount { get; internal set; }
        public long PlanCacheHitCount { get; internal set; }
        public long ReplanCount { get; internal set; }
        public int LastExpandedCells { get; internal set; }
        public double LastSearchMilliseconds { get; internal set; }
        public double TotalSearchMilliseconds { get; internal set; }
        public double LastRequestMilliseconds { get; internal set; }
        public double LastCacheMilliseconds { get; internal set; }
        public double LastReplanMilliseconds { get; internal set; }
    }

    /// <summary>
    /// One explicitly owned, main-thread instance per run/map. Snapshots validated authored
    /// data once; occupancy never modifies the ScriptableObject. No scene/global lookup.
    /// </summary>
    public sealed class Stage2Navigation
    {
        private static readonly ProfilerMarker SearchMarker = new ProfilerMarker("Stage2.Navigation.FullSearch");
        private static readonly ProfilerMarker RequestMarker = new ProfilerMarker("Stage2.Navigation.PlanFrom");
        private static readonly ProfilerMarker ReplanMarker = new ProfilerMarker("Stage2.Navigation.Replan");
        private static readonly ProfilerMarker CacheMarker = new ProfilerMarker("Stage2.Navigation.Cache");

        private readonly Vector2Int minimum;
        private readonly int width;
        private readonly int height;
        private readonly bool[] walkable = Array.Empty<bool>();
        private readonly bool[] buildable = Array.Empty<bool>();
        private readonly object[] occupants = Array.Empty<object>();
        private readonly int[] blocks = Array.Empty<int>();
        private readonly int[] steps = Array.Empty<int>();
        private readonly int[] next = Array.Empty<int>();
        private readonly int[] heap = Array.Empty<int>();
        private readonly int[] heapPositions = Array.Empty<int>();
        private int heapCount;
        private long generation;
        private bool fieldReady;

        public Stage2Navigation(Stage2MapDefinition map)
        {
            if (map == null)
            {
                ValidationDiagnostic = "MissingMap: assign a Stage2MapDefinition.";
                return;
            }

            var validation = map.ValidateDefinition();
            if (!validation.IsValid)
            {
                ValidationDiagnostic = validation.Issues[0].ToString();
                return;
            }

            MapHash = map.ComputeDeterministicContentHash();
            minimum = map.CellBounds.Minimum;
            width = map.CellBounds.Size.x;
            height = map.CellBounds.Size.y;
            Entrance = map.EnemyEntrance;
            Throne = map.Throne;
            var count = width * height;
            walkable = new bool[count];
            buildable = new bool[count];
            occupants = new object[count];
            blocks = new int[count];
            steps = new int[count];
            next = new int[count];
            heap = new int[count];
            heapPositions = new int[count];
            for (var index = 0; index < count; index++)
            {
                var cell = map.ReadCell(Cell(index));
                walkable[index] = cell.IsWalkable;
                buildable[index] = cell.IsBuildable;
            }

            IsValid = true;
        }

        public bool IsValid { get; }
        public string ValidationDiagnostic { get; } = string.Empty;
        public string MapHash { get; } = string.Empty;
        public Vector2Int Entrance { get; }
        public Vector2Int Throne { get; }
        public long Revision { get; private set; }
        public int OccupiedCount { get; private set; }
        public NavigationDiagnostics Diagnostics { get; } = new NavigationDiagnostics();

        public OccupancyResult TryOccupy(Vector2Int cell, object owner)
        {
            if (!IsValid) return OccupancyResult.InvalidMap;
            if (ReferenceEquals(owner, null)) return OccupancyResult.InvalidOwner;
            if (!TryIndex(cell, out var index)) return OccupancyResult.OutsideBounds;
            if (cell == Entrance || cell == Throne) return OccupancyResult.ReservedLandmark;
            if (!walkable[index]) return OccupancyResult.NotWalkable;
            if (!buildable[index]) return OccupancyResult.NotBuildable;
            if (occupants[index] != null) return OccupancyResult.AlreadyOccupied;
            occupants[index] = owner;
            OccupiedCount++;
            InvalidateOccupancy();
            return OccupancyResult.Changed;
        }

        public OccupancyResult TryRelease(Vector2Int cell, object owner)
        {
            if (!IsValid) return OccupancyResult.InvalidMap;
            if (ReferenceEquals(owner, null)) return OccupancyResult.InvalidOwner;
            if (!TryIndex(cell, out var index)) return OccupancyResult.OutsideBounds;
            if (occupants[index] == null) return OccupancyResult.Empty;
            if (!ReferenceEquals(occupants[index], owner)) return OccupancyResult.OwnerMismatch;
            occupants[index] = null;
            OccupiedCount--;
            InvalidateOccupancy();
            return OccupancyResult.Changed;
        }

        public bool TryGetOccupant(Vector2Int cell, out object owner)
        {
            owner = TryIndex(cell, out var index) ? occupants[index] : null;
            return owner != null;
        }

        /// <summary>One occupancy revision if nonempty; always invalidates prior-run plans.</summary>
        public void Reset()
        {
            if (OccupiedCount > 0)
            {
                Array.Clear(occupants, 0, occupants.Length);
                OccupiedCount = 0;
                InvalidateOccupancy();
            }

            generation++;
            fieldReady = false;
        }

        public bool IsCurrent(NavigationPlan plan) => plan != null &&
            ReferenceEquals(plan.Source, this) && plan.Revision == Revision && plan.Generation == generation;

        /// <summary>
        /// A consumer retains a plan and path index. Call this before using either;
        /// unchanged plans return by identity without search or path allocation.
        /// </summary>
        public NavigationPlan ReplanIfStale(Vector2Int currentCell, NavigationPlan previous)
        {
            if (IsCurrent(previous))
            {
                using (CacheMarker.Auto())
                {
                    var started = Stopwatch.GetTimestamp();
                    Diagnostics.PlanCacheHitCount++;
                    Diagnostics.LastCacheMilliseconds = MillisecondsSince(started);
                    return previous;
                }
            }

            using (ReplanMarker.Auto())
            {
                var started = Stopwatch.GetTimestamp();
                Diagnostics.ReplanCount++;
                var result = PlanFrom(currentCell);
                Diagnostics.LastReplanMilliseconds = MillisecondsSince(started);
                return result;
            }
        }

        public NavigationPlan PlanFrom(Vector2Int start)
        {
            using (RequestMarker.Auto())
            {
                var started = Stopwatch.GetTimestamp();
                Diagnostics.RequestCount++;
                try
                {
                    if (!IsValid) return Failure(NavigationStatus.InvalidMap, ValidationDiagnostic);
                    if (!TryIndex(start, out var index))
                        return Failure(NavigationStatus.StartOutsideBounds, "Start is outside authored CellBounds.");
                    if (!walkable[index])
                        return Failure(NavigationStatus.StartNotWalkable, "Start is not authored walkable.");
                    if (occupants[index] != null)
                        return Failure(NavigationStatus.StartOccupied, "Start is occupied; placement/movement owner must resolve overlap.");

                    if (!fieldReady) BuildField();
                    else Diagnostics.FieldReuseCount++;
                    if (steps[index] == int.MaxValue)
                        return Failure(NavigationStatus.Unreachable, "No authored walkable connection to throne, even after removing occupants.");

                    var path = new List<Vector2Int> { start };
                    while (next[index] >= 0)
                    {
                        index = next[index];
                        if (occupants[index] != null)
                            return Result(NavigationStatus.ReachableBlocker, path.ToArray(), Cell(index), occupants[index]);
                        path.Add(Cell(index));
                    }

                    return Result(NavigationStatus.ReachesThrone, path.ToArray(), null, null);
                }
                finally
                {
                    Diagnostics.LastRequestMilliseconds = MillisecondsSince(started);
                }
            }
        }

        private void InvalidateOccupancy()
        {
            Revision++;
            fieldReady = false;
        }

        private NavigationPlan Failure(NavigationStatus status, string reason) =>
            new NavigationPlan(this, status, Revision, generation, Array.Empty<Vector2Int>(), null, null, reason);

        private NavigationPlan Result(NavigationStatus status, Vector2Int[] path, Vector2Int? blocker, object owner) =>
            new NavigationPlan(this, status, Revision, generation, path, blocker, owner, string.Empty);

        // Reverse Dijkstra shared by EVERY start. Cost is lexicographic:
        // (occupied cells to enter, cardinal steps). An open route always wins;
        // otherwise its first occupant has a reachable, unoccupied approach.
        private void BuildField()
        {
            using (SearchMarker.Auto())
            {
                var started = Stopwatch.GetTimestamp();
                Diagnostics.FullSearchCount++;
                Diagnostics.LastExpandedCells = 0;
                heapCount = 0;
                for (var i = 0; i < walkable.Length; i++)
                {
                    blocks[i] = steps[i] = int.MaxValue;
                    next[i] = heapPositions[i] = -1;
                }

                TryIndex(Throne, out var goal);
                blocks[goal] = steps[goal] = 0;
                PushOrDecrease(goal);
                while (heapCount > 0)
                {
                    var current = Pop();
                    Diagnostics.LastExpandedCells++;
                    var x = current % width;
                    var y = current / width;
                    if (x > 0) Relax(current - 1, current);
                    if (y + 1 < height) Relax(current + width, current);
                    if (y > 0) Relax(current - width, current);
                    if (x + 1 < width) Relax(current + 1, current);
                }

                fieldReady = true;
                Diagnostics.LastSearchMilliseconds = MillisecondsSince(started);
                Diagnostics.TotalSearchMilliseconds += Diagnostics.LastSearchMilliseconds;
            }
        }

        private void Relax(int from, int towardGoal)
        {
            if (!walkable[from] || heapPositions[from] == -2) return;
            var candidateBlocks = blocks[towardGoal] + (occupants[towardGoal] != null ? 1 : 0);
            var candidateSteps = steps[towardGoal] + 1;
            if (candidateBlocks > blocks[from] ||
                (candidateBlocks == blocks[from] && candidateSteps > steps[from])) return;
            if (candidateBlocks == blocks[from] && candidateSteps == steps[from])
            {
                // Equal routes choose the next cell with lower y, then lower x.
                if (towardGoal < next[from]) next[from] = towardGoal;
                return;
            }

            blocks[from] = candidateBlocks;
            steps[from] = candidateSteps;
            next[from] = towardGoal;
            PushOrDecrease(from);
        }

        // Indexed heap: bounded by authored cell count, no per-search node allocations.
        private bool Less(int a, int b) => blocks[a] != blocks[b] ? blocks[a] < blocks[b] :
            steps[a] != steps[b] ? steps[a] < steps[b] : a < b;

        private void PushOrDecrease(int cell)
        {
            var position = heapPositions[cell];
            if (position < 0) position = heapCount++;
            while (position > 0)
            {
                var parent = (position - 1) / 2;
                if (!Less(cell, heap[parent])) break;
                heap[position] = heap[parent];
                heapPositions[heap[position]] = position;
                position = parent;
            }

            heap[position] = cell;
            heapPositions[cell] = position;
        }

        private int Pop()
        {
            var result = heap[0];
            var last = heap[--heapCount];
            var position = 0;
            while (position * 2 + 1 < heapCount)
            {
                var child = position * 2 + 1;
                if (child + 1 < heapCount && Less(heap[child + 1], heap[child])) child++;
                if (!Less(heap[child], last)) break;
                heap[position] = heap[child];
                heapPositions[heap[position]] = position;
                position = child;
            }

            if (heapCount > 0)
            {
                heap[position] = last;
                heapPositions[last] = position;
            }

            heapPositions[result] = -2;
            return result;
        }

        private bool TryIndex(Vector2Int cell, out int index)
        {
            var x = (long)cell.x - minimum.x;
            var y = (long)cell.y - minimum.y;
            index = -1;
            if (x < 0 || y < 0 || x >= width || y >= height) return false;
            index = (int)(y * width + x);
            return true;
        }

        private Vector2Int Cell(int index) => new Vector2Int(minimum.x + index % width, minimum.y + index / width);
        private static double MillisecondsSince(long started) =>
            (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
    }
}
