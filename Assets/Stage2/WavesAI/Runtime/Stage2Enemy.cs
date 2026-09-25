using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using UnityEngine;

namespace Not3A.Stage2.WavesAI
{
    [DisallowMultipleComponent]
    public sealed class Stage2Enemy : MonoBehaviour
    {
        private Stage2MapDefinition map;
        private Stage2Navigation navigation;
        private Stage2EnemyCatalog catalog;
        private NavigationPlan plan;
        private int pathIndex;
        private Vector2Int? stepDestination;
        private float siegeRemaining;
        private int health;
        private bool configured;
        private bool removed;
        private bool paused;
        private bool automaticTick = true;

        public Vector2Int CurrentCell { get; private set; }
        public Vector2Int? StepDestination => stepDestination;
        public NavigationPlan Plan => plan;
        public int Health => health;
        public bool IsAlive => configured && !removed && health > 0;
        public bool IsAtCellCenter => !stepDestination.HasValue;
        public event Action<Stage2Enemy, int> ReachedThrone;
        public event Action<Stage2Enemy> Removed;

        public void Configure(Stage2MapDefinition definition, Stage2Navigation sharedNavigation,
            Stage2EnemyCatalog settings, Vector2Int startCell)
        {
            if (definition == null || !definition.ValidateDefinition().IsValid)
                throw new ArgumentException("Invalid map.", nameof(definition));
            if (sharedNavigation == null || !sharedNavigation.IsValid ||
                sharedNavigation.MapHash != definition.ComputeDeterministicContentHash())
                throw new ArgumentException("Navigation must use the same valid map.", nameof(sharedNavigation));
            if (settings == null || !settings.TryValidate(out _))
                throw new ArgumentException("Invalid enemy catalog.", nameof(settings));
            if (!definition.Contains(startCell) || !definition.IsWalkable(startCell) ||
                sharedNavigation.TryGetOccupant(startCell, out _))
                throw new ArgumentException("Enemy start must be free and walkable.", nameof(startCell));
            map = definition;
            navigation = sharedNavigation;
            catalog = settings;
            CurrentCell = startCell;
            transform.position = definition.CellToWorld(startCell);
            health = settings.Health;
            siegeRemaining = settings.SiegeInterval;
            plan = null;
            pathIndex = 0;
            stepDestination = null;
            paused = false;
            removed = false;
            configured = true;
        }

        public bool Protects(Vector2Int cell) => IsAlive &&
            (CurrentCell == cell || stepDestination == cell);

        public void SetPaused(bool value) => paused = value;
        public void SetAutomaticTick(bool value) => automaticTick = value;

        public bool TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive) return false;
            health = Mathf.Max(0, health - amount);
            if (health == 0) RemoveOnce(false, true);
            return true;
        }

        public void Despawn() => RemoveOnce(false, true);

        public void Tick(float elapsedSeconds)
        {
            if (elapsedSeconds < 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!IsAlive || paused || elapsedSeconds == 0f) return;
            siegeRemaining = Mathf.Max(0f, siegeRemaining - elapsedSeconds);

            // An already-started step owns both endpoints until arrival. A mid-step
            // occupancy revision affects only the decision made at the next center.
            if (stepDestination.HasValue)
            {
                AdvanceStep(elapsedSeconds);
                return;
            }

            var current = navigation.ReplanIfStale(CurrentCell, plan);
            if (!ReferenceEquals(current, plan))
            {
                plan = current;
                pathIndex = 1; // Path[0] is the explicit current cell.
            }
            if (plan.Status != NavigationStatus.ReachesThrone &&
                plan.Status != NavigationStatus.ReachableBlocker) return;

            if (pathIndex < plan.Path.Count)
            {
                var next = plan.Path[pathIndex];
                if (navigation.TryGetOccupant(next, out _))
                {
                    plan = null;
                    return;
                }
                stepDestination = next; // Protection commits before movement begins.
                AdvanceStep(elapsedSeconds);
                return;
            }

            if (plan.Status == NavigationStatus.ReachesThrone && CurrentCell == map.Throne)
            {
                RemoveOnce(true, true);
                return;
            }
            if (plan.Status != NavigationStatus.ReachableBlocker || !plan.BlockerCell.HasValue ||
                plan.BlockerOwner is not Stage2Tower tower || !tower.IsAlive ||
                Math.Abs(CurrentCell.x - plan.BlockerCell.Value.x) +
                Math.Abs(CurrentCell.y - plan.BlockerCell.Value.y) != 1 ||
                !navigation.TryGetOccupant(plan.BlockerCell.Value, out var owner) ||
                !ReferenceEquals(owner, tower))
            {
                plan = null;
                return;
            }

            if (siegeRemaining > 0f) return;
            siegeRemaining = catalog.SiegeInterval;
            tower.TakeDamage(catalog.SiegeDamage);
        }

        private void AdvanceStep(float elapsedSeconds)
        {
            var destination = stepDestination.Value;
            var target = map.CellToWorld(destination);
            var moved = Vector2.MoveTowards(transform.position, target, catalog.MoveSpeed * elapsedSeconds);
            transform.position = new Vector3(moved.x, moved.y, transform.position.z);
            if (moved != target) return;
            CurrentCell = destination;
            stepDestination = null;
            pathIndex++;
        }

        private void Update()
        {
            if (automaticTick) Tick(Time.deltaTime);
        }

        private void OnDestroy() => RemoveOnce(false, false);

        private void RemoveOnce(bool throne, bool destroyObject)
        {
            if (removed) return;
            removed = true;
            stepDestination = null;
            plan = null;
            if (throne) ReachedThrone?.Invoke(this, catalog.ThroneDamage);
            Removed?.Invoke(this);
            if (destroyObject)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
            }
        }
    }
}
