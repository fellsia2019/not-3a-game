using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage1
{
    public sealed class DynamicNavigationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2Int mapMinimum = new Vector2Int(-9, -7);
        [SerializeField] private Vector2Int mapMaximum = new Vector2Int(9, 7);
        [SerializeField] private Vector2Int pathMinimum = new Vector2Int(0, -2);
        [SerializeField] private Vector2Int pathMaximum = new Vector2Int(9, 2);
        [SerializeField] private Vector2Int entrance = new Vector2Int(9, 0);
        [SerializeField] private Vector2Int goal = Vector2Int.zero;
        [SerializeField] private Vector2Int[] additionalForbiddenBuildCells;

        private readonly Dictionary<Vector2Int, TowerController> towers = new Dictionary<Vector2Int, TowerController>();
        private readonly HashSet<Vector2Int> forbiddenBuildCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> blockerScratch = new HashSet<Vector2Int>();

        public int Revision { get; private set; }
        public Vector2Int Entrance => entrance;
        public Vector2Int Goal => goal;
        public Vector2 EntranceWorld => IsoGrid.CellToWorld(entrance);

        public void Configure(
            Vector2Int minimumMapCell,
            Vector2Int maximumMapCell,
            Vector2Int minimumPathCell,
            Vector2Int maximumPathCell,
            Vector2Int entranceCell,
            Vector2Int goalCell,
            IEnumerable<Vector2Int> nonBuildableCells)
        {
            mapMinimum = minimumMapCell;
            mapMaximum = maximumMapCell;
            pathMinimum = minimumPathCell;
            pathMaximum = maximumPathCell;
            entrance = entranceCell;
            goal = goalCell;
            var additional = new List<Vector2Int>();
            if (nonBuildableCells != null)
            {
                additional.AddRange(nonBuildableCells);
            }

            additionalForbiddenBuildCells = additional.ToArray();
            RebuildForbiddenCells();
        }

        private void Awake()
        {
            RebuildForbiddenCells();
        }

        private void RebuildForbiddenCells()
        {
            forbiddenBuildCells.Clear();
            forbiddenBuildCells.Add(entrance);
            forbiddenBuildCells.Add(goal);
            if (additionalForbiddenBuildCells != null)
            {
                foreach (var cell in additionalForbiddenBuildCells)
                {
                    forbiddenBuildCells.Add(cell);
                }
            }
        }

        public bool ValidateBuildCell(Vector2Int cell, out string reason)
        {
            if (!Contains(mapMinimum, mapMaximum, cell))
            {
                reason = "Вне карты";
                return false;
            }

            if (forbiddenBuildCells.Contains(cell) || towers.ContainsKey(cell))
            {
                reason = "Клетка занята";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool RegisterTower(Vector2Int cell, TowerController tower)
        {
            if (tower == null || towers.ContainsKey(cell))
            {
                return false;
            }

            towers.Add(cell, tower);
            Revision++;
            return true;
        }

        public void UnregisterTower(Vector2Int cell, TowerController tower)
        {
            if (towers.TryGetValue(cell, out var registered) && registered == tower)
            {
                towers.Remove(cell);
                Revision++;
            }
        }

        public GridPathPlan PlanFrom(Vector2Int start, out TowerController blocker)
        {
            blockerScratch.Clear();
            foreach (var pair in towers)
            {
                if (pair.Value != null && !pair.Value.IsDestroyed)
                {
                    blockerScratch.Add(pair.Key);
                }
            }

            var plan = GridPathfinder.Plan(pathMinimum, pathMaximum, start, goal, blockerScratch);
            blocker = null;
            if (plan.BlockerCell.HasValue)
            {
                towers.TryGetValue(plan.BlockerCell.Value, out blocker);
            }

            return plan;
        }

        private static bool Contains(Vector2Int minimum, Vector2Int maximum, Vector2Int cell)
        {
            return cell.x >= minimum.x && cell.x <= maximum.x &&
                   cell.y >= minimum.y && cell.y <= maximum.y;
        }
    }
}
