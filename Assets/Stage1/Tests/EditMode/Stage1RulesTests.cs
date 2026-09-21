using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Not3A.Stage1.Tests
{
    public sealed class Stage1RulesTests
    {
        [Test]
        public void Inventory_SpendsOnlyWhenTheFullCostIsAvailable()
        {
            var inventory = new ResourceInventory();
            inventory.Add(ResourceKind.Wood, 5);
            inventory.Add(ResourceKind.Stone, 2);

            Assert.That(inventory.TrySpend(5, 3), Is.False);
            Assert.That(inventory.Wood, Is.EqualTo(5));
            Assert.That(inventory.Stone, Is.EqualTo(2));

            inventory.Add(ResourceKind.Stone, 1);
            Assert.That(inventory.TrySpend(5, 3), Is.True);
            Assert.That(inventory.Wood, Is.Zero);
            Assert.That(inventory.Stone, Is.Zero);
        }

        [Test]
        public void GatherObjective_TracksOnlyConfiguredResourceAndCompletesOnce()
        {
            var objective = new GatherObjective(ResourceKind.Wood, 5);

            Assert.That(objective.RecordCollection(ResourceKind.Stone, 20), Is.False);
            Assert.That(objective.Progress, Is.Zero);
            Assert.That(objective.RecordCollection(ResourceKind.Wood, 4), Is.False);
            Assert.That(objective.RecordCollection(ResourceKind.Wood, 1), Is.True);
            Assert.That(objective.RecordCollection(ResourceKind.Wood, 5), Is.False);
            Assert.That(objective.Progress, Is.EqualTo(5));
            Assert.That(objective.IsComplete, Is.True);
        }

        [Test]
        public void HealthPool_ClampsAtZeroAndReportsDeathOnce()
        {
            var health = new HealthPool(5);

            Assert.That(health.ApplyDamage(2), Is.False);
            Assert.That(health.Current, Is.EqualTo(3));
            Assert.That(health.ApplyDamage(10), Is.True);
            Assert.That(health.Current, Is.Zero);
            Assert.That(health.ApplyDamage(1), Is.False);
        }

        [Test]
        public void RunState_OnlyAllowsTheRequiredPrototypeFlow()
        {
            var state = new RunStateMachine();

            Assert.That(state.Win(), Is.False);
            Assert.That(state.StartWave(), Is.True);
            Assert.That(state.StartWave(), Is.False);
            Assert.That(state.Win(), Is.True);
            Assert.That(state.Lose(), Is.False);
            Assert.That(state.Phase, Is.EqualTo(RunPhase.Won));
        }

        [Test]
        public void IsoGrid_RoundTripsCellsAndSnapsWorldPositions()
        {
            var cell = new Vector2Int(3, -2);
            var world = IsoGrid.CellToWorld(cell);

            Assert.That(IsoGrid.WorldToCell(world), Is.EqualTo(cell));
            Assert.That(IsoGrid.Snap(world + new Vector2(0.09f, -0.07f)), Is.EqualTo(world));
        }

        [Test]
        public void IsoGrid_ClampsWorldPositionToDiamondMapBounds()
        {
            var outside = IsoGrid.CellCoordinatesToWorld(new Vector2(14f, -12f));

            var clamped = IsoGrid.ClampWorldToCellBounds(outside, new Vector2(-9f, -7f), new Vector2(9f, 7f));
            var cell = IsoGrid.WorldToCellCoordinates(clamped);

            Assert.That(cell.x, Is.EqualTo(9f).Within(0.001f));
            Assert.That(cell.y, Is.EqualTo(-7f).Within(0.001f));
        }

        [Test]
        public void GridPathfinder_IsDeterministicAndMazeLengthensTheOpenRoute()
        {
            var minimum = new Vector2Int(0, -2);
            var maximum = new Vector2Int(8, 2);
            var start = new Vector2Int(8, 0);
            var goal = Vector2Int.zero;
            var open = GridPathfinder.Plan(minimum, maximum, start, goal, new HashSet<Vector2Int>());
            var maze = new HashSet<Vector2Int>();
            for (var y = -2; y <= 1; y++) maze.Add(new Vector2Int(6, y));
            for (var y = -1; y <= 2; y++) maze.Add(new Vector2Int(3, y));

            var first = GridPathfinder.Plan(minimum, maximum, start, goal, maze);
            var second = GridPathfinder.Plan(minimum, maximum, start, goal, maze);

            Assert.That(open.ReachesGoal, Is.True);
            Assert.That(first.ReachesGoal, Is.True);
            Assert.That(first.Path.Count, Is.GreaterThan(open.Path.Count));
            Assert.That(second.Path, Is.EqualTo(first.Path));
        }

        [Test]
        public void GridPathfinder_FullWallReturnsReachableBlockerAndRemovingItRestoresPath()
        {
            var blockers = new HashSet<Vector2Int>();
            for (var y = -2; y <= 2; y++) blockers.Add(new Vector2Int(4, y));

            var blocked = GridPathfinder.Plan(new Vector2Int(0, -2), new Vector2Int(8, 2), new Vector2Int(8, 0), Vector2Int.zero, blockers);

            Assert.That(blocked.ReachesGoal, Is.False);
            Assert.That(blocked.BlockerCell.HasValue, Is.True);
            Assert.That(blocked.Path.Count, Is.GreaterThan(0));
            blockers.Remove(blocked.BlockerCell.Value);
            var restored = GridPathfinder.Plan(new Vector2Int(0, -2), new Vector2Int(8, 2), new Vector2Int(8, 0), Vector2Int.zero, blockers);
            Assert.That(restored.ReachesGoal, Is.True);
        }
    }
}
