using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage1
{
    public enum ResourceKind
    {
        Wood,
        Stone
    }

    public enum RunPhase
    {
        Preparation,
        Wave,
        Won,
        Lost
    }

    public sealed class ResourceInventory
    {
        public int Wood { get; private set; }
        public int Stone { get; private set; }

        public event Action Changed;

        public int Get(ResourceKind kind) => kind == ResourceKind.Wood ? Wood : Stone;

        public void Add(ResourceKind kind, int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Resource amount must be positive.");
            }

            if (kind == ResourceKind.Wood)
            {
                Wood += amount;
            }
            else
            {
                Stone += amount;
            }

            Changed?.Invoke();
        }

        public bool CanAfford(int wood, int stone) => wood >= 0 && stone >= 0 && Wood >= wood && Stone >= stone;

        public bool TrySpend(int wood, int stone)
        {
            if (!CanAfford(wood, stone))
            {
                return false;
            }

            Wood -= wood;
            Stone -= stone;
            Changed?.Invoke();
            return true;
        }

        public void Reset()
        {
            Wood = 0;
            Stone = 0;
            Changed?.Invoke();
        }
    }

    public sealed class GatherObjective
    {
        public GatherObjective(ResourceKind trackedResource, int target)
        {
            if (target <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(target));
            }

            TrackedResource = trackedResource;
            Target = target;
        }

        public ResourceKind TrackedResource { get; }
        public int Target { get; }
        public int Progress { get; private set; }
        public bool IsComplete { get; private set; }

        public bool RecordCollection(ResourceKind resource, int amount)
        {
            if (IsComplete || resource != TrackedResource || amount <= 0)
            {
                return false;
            }

            Progress = Mathf.Min(Target, Progress + amount);
            if (Progress < Target)
            {
                return false;
            }

            IsComplete = true;
            return true;
        }
    }

    public sealed class HealthPool
    {
        public HealthPool(int maximum)
        {
            if (maximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            Maximum = maximum;
            Current = maximum;
        }

        public int Maximum { get; }
        public int Current { get; private set; }
        public bool IsEmpty => Current == 0;

        public bool ApplyDamage(int amount)
        {
            if (amount <= 0 || IsEmpty)
            {
                return false;
            }

            Current = Mathf.Max(0, Current - amount);
            return IsEmpty;
        }
    }

    public sealed class RunStateMachine
    {
        public RunPhase Phase { get; private set; } = RunPhase.Preparation;

        public bool StartWave()
        {
            if (Phase != RunPhase.Preparation)
            {
                return false;
            }

            Phase = RunPhase.Wave;
            return true;
        }

        public bool Win()
        {
            if (Phase != RunPhase.Wave)
            {
                return false;
            }

            Phase = RunPhase.Won;
            return true;
        }

        public bool Lose()
        {
            if (Phase == RunPhase.Won || Phase == RunPhase.Lost)
            {
                return false;
            }

            Phase = RunPhase.Lost;
            return true;
        }
    }

    public static class IsoGrid
    {
        public static readonly Vector2 CellSize = new Vector2(1.6f, 0.8f);

        public static Vector2 CellToWorld(Vector2Int cell)
        {
            return CellCoordinatesToWorld(cell);
        }

        public static Vector2 CellCoordinatesToWorld(Vector2 cell)
        {
            return new Vector2(
                (cell.x - cell.y) * CellSize.x * 0.5f,
                (cell.x + cell.y) * CellSize.y * 0.5f);
        }

        public static Vector2 WorldToCellCoordinates(Vector2 world)
        {
            return new Vector2(
                world.x / CellSize.x + world.y / CellSize.y,
                world.y / CellSize.y - world.x / CellSize.x);
        }

        public static Vector2Int WorldToCell(Vector2 world)
        {
            var cell = WorldToCellCoordinates(world);
            return new Vector2Int(Mathf.RoundToInt(cell.x), Mathf.RoundToInt(cell.y));
        }

        public static Vector2 Snap(Vector2 world) => CellToWorld(WorldToCell(world));

        public static Vector2 ClampWorldToCellBounds(Vector2 world, Vector2 minimumCell, Vector2 maximumCell)
        {
            var cell = WorldToCellCoordinates(world);
            cell.x = Mathf.Clamp(cell.x, minimumCell.x, maximumCell.x);
            cell.y = Mathf.Clamp(cell.y, minimumCell.y, maximumCell.y);
            return CellCoordinatesToWorld(cell);
        }
    }

    public static class BuildPlacementRules
    {
        public static bool Validate(
            Vector2 position,
            Rect buildZone,
            IReadOnlyList<Vector2> route,
            float routeClearance,
            IReadOnlyList<Vector2> occupied,
            float occupiedClearance,
            out string reason)
        {
            if (!buildZone.Contains(position))
            {
                reason = "Вне зоны строительства";
                return false;
            }

            if (route != null)
            {
                for (var index = 1; index < route.Count; index++)
                {
                    if (DistanceToSegment(position, route[index - 1], route[index]) < routeClearance)
                    {
                        reason = "Маршрут врагов должен оставаться свободным";
                        return false;
                    }
                }
            }

            if (occupied != null)
            {
                for (var index = 0; index < occupied.Count; index++)
                {
                    if (Vector2.Distance(position, occupied[index]) < occupiedClearance)
                    {
                        reason = "Клетка занята";
                        return false;
                    }
                }
            }

            reason = string.Empty;
            return true;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            var segment = end - start;
            if (segment.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector2.Distance(point, start);
            }

            var t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segment.sqrMagnitude);
            return Vector2.Distance(point, start + segment * t);
        }
    }

    public sealed class GridPathPlan
    {
        public GridPathPlan(IReadOnlyList<Vector2Int> path, bool reachesGoal, Vector2Int? blockerCell)
        {
            Path = path;
            ReachesGoal = reachesGoal;
            BlockerCell = blockerCell;
        }

        public IReadOnlyList<Vector2Int> Path { get; }
        public bool ReachesGoal { get; }
        public Vector2Int? BlockerCell { get; }
    }

    public static class GridPathfinder
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.right
        };

        public static GridPathPlan Plan(
            Vector2Int minimum,
            Vector2Int maximum,
            Vector2Int start,
            Vector2Int goal,
            ISet<Vector2Int> blockers)
        {
            var queue = new Queue<Vector2Int>();
            var previous = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new HashSet<Vector2Int> { start };
            queue.Enqueue(start);
            Vector2Int? firstBlocker = null;
            Vector2Int firstApproach = start;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == goal)
                {
                    return new GridPathPlan(Reconstruct(previous, start, goal), true, null);
                }

                foreach (var direction in Directions)
                {
                    var next = current + direction;
                    if (!Contains(minimum, maximum, next))
                    {
                        continue;
                    }

                    if (blockers != null && blockers.Contains(next) && next != start)
                    {
                        if (!firstBlocker.HasValue)
                        {
                            firstBlocker = next;
                            firstApproach = current;
                        }

                        continue;
                    }

                    if (!visited.Add(next))
                    {
                        continue;
                    }

                    previous[next] = current;
                    queue.Enqueue(next);
                }
            }

            var approachPath = firstBlocker.HasValue
                ? Reconstruct(previous, start, firstApproach)
                : new List<Vector2Int>();
            return new GridPathPlan(approachPath, false, firstBlocker);
        }

        private static bool Contains(Vector2Int minimum, Vector2Int maximum, Vector2Int cell)
        {
            return cell.x >= minimum.x && cell.x <= maximum.x &&
                   cell.y >= minimum.y && cell.y <= maximum.y;
        }

        private static List<Vector2Int> Reconstruct(
            IReadOnlyDictionary<Vector2Int, Vector2Int> previous,
            Vector2Int start,
            Vector2Int end)
        {
            var path = new List<Vector2Int> { end };
            var current = end;
            while (current != start)
            {
                current = previous[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
