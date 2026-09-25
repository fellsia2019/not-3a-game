using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using UnityEngine;

namespace Not3A.Stage2.Defense
{
    public enum Stage2BuildPhase { Inactive, Preparation, Wave }

    public enum PlacementResult
    {
        Ready, NotPlacing, Inactive, Paused, InvalidCell, Occupied,
        HeroProtected, EnemyProtected, EnemyStateUnavailable, CannotAfford,
        CreationFailed, RegistrationFailed, PaymentFailed
    }

    /// <summary>Main-thread placement transaction. The scene owner supplies run state and actor protection.</summary>
    public sealed class Stage2Defense
    {
        private readonly Stage2MapDefinition map;
        private readonly Stage2Navigation navigation;
        private readonly Stage2Inventory inventory;
        private readonly Stage2DefenseCatalog catalog;
        private readonly Func<Vector2> heroWorld;
        private readonly Func<Stage2Tower> prepareTower;
        private readonly List<Stage2Tower> towers = new List<Stage2Tower>();
        private Func<Vector2Int, bool> enemyProtected;
        private Func<Vector2, float, int, bool> attackTarget;
        private bool attackActive = true;
        private Vector2Int? previewCell;
        private bool placing;
        private bool paused;
        private bool committing;
        private Stage2BuildPhase phase = Stage2BuildPhase.Inactive;

        public Stage2Defense(Stage2MapDefinition map, Stage2Navigation navigation,
            Stage2Inventory inventory, Stage2DefenseCatalog catalog, Func<Vector2> heroWorld,
            Func<Stage2Tower> prepareTower)
        {
            if (map == null || !map.ValidateDefinition().IsValid) throw new ArgumentException("Invalid map.", nameof(map));
            if (navigation == null || !navigation.IsValid || navigation.MapHash != map.ComputeDeterministicContentHash())
                throw new ArgumentException("Navigation must use the same valid map.", nameof(navigation));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (catalog == null || !catalog.TryValidate(out _)) throw new ArgumentException("Invalid defense catalog.", nameof(catalog));
            this.heroWorld = heroWorld ?? throw new ArgumentNullException(nameof(heroWorld));
            this.prepareTower = prepareTower ?? throw new ArgumentNullException(nameof(prepareTower));
            this.map = map;
            this.navigation = navigation;
            this.inventory = inventory;
            this.catalog = catalog;
        }

        public event Action<Stage2Tower> Built;
        public bool IsPlacing => placing;
        public int TowerCount => towers.Count;
        public Vector2Int? PreviewCell => previewCell;

        public void SetEnemyProtection(Func<Vector2Int, bool> isProtected) => enemyProtected = isProtected;

        public void SetAttackTarget(Func<Vector2, float, int, bool> hitNearest)
        {
            attackTarget = hitNearest;
            foreach (var tower in towers) if (tower != null) tower.SetAttackTarget(hitNearest);
        }

        public void SetAttackActive(bool active)
        {
            attackActive = active;
            foreach (var tower in towers) if (tower != null) tower.SetAttackActive(active);
        }

        public void SetRunState(Stage2BuildPhase currentPhase, bool isPaused)
        {
            phase = currentPhase;
            paused = isPaused;
            if (paused || phase == Stage2BuildPhase.Inactive) CancelPlacement();
        }

        public bool BeginPlacement()
        {
            if (committing || paused || phase == Stage2BuildPhase.Inactive) return false;
            placing = true;
            previewCell = null;
            return true;
        }

        public void SetPreviewCell(Vector2Int cell)
        {
            if (placing) previewCell = cell;
        }

        public void CancelPlacement()
        {
            placing = false;
            previewCell = null;
        }

        public PlacementResult EvaluatePreview() => previewCell.HasValue
            ? Evaluate(previewCell.Value) : PlacementResult.NotPlacing;

        public PlacementResult Evaluate(Vector2Int cell)
        {
            if (!placing || committing) return PlacementResult.NotPlacing;
            return EvaluateCore(cell);
        }

        private PlacementResult EvaluateCore(Vector2Int cell)
        {
            if (phase == Stage2BuildPhase.Inactive) return PlacementResult.Inactive;
            if (paused) return PlacementResult.Paused;
            if (!map.Contains(cell) || !map.IsWalkable(cell) || !map.IsBuildable(cell) ||
                cell == map.EnemyEntrance || cell == map.Throne) return PlacementResult.InvalidCell;
            if (navigation.TryGetOccupant(cell, out _)) return PlacementResult.Occupied;
            if (map.WorldToCell(heroWorld()) == cell) return PlacementResult.HeroProtected;
            // Wave without a live movement-state provider must fail closed.
            if (phase == Stage2BuildPhase.Wave && enemyProtected == null)
                return PlacementResult.EnemyStateUnavailable;
            if (enemyProtected != null && enemyProtected(cell)) return PlacementResult.EnemyProtected;
            if (!inventory.CanAfford(catalog.WoodCost, catalog.StoneCost, catalog.MetalCost))
                return PlacementResult.CannotAfford;
            return PlacementResult.Ready;
        }

        public PlacementResult TryConfirm(out Stage2Tower builtTower)
        {
            builtTower = null;
            if (!previewCell.HasValue) return PlacementResult.NotPlacing;
            var cell = previewCell.Value;
            var validity = Evaluate(cell);
            if (validity != PlacementResult.Ready) return validity;

            committing = true;
            Stage2Tower prepared = null;
            try
            {
                try { prepared = prepareTower(); }
                catch (Exception exception)
                {
                    Debug.LogWarning($"S2_DEF_CREATION_FAILED: {exception.Message}");
                    return PlacementResult.CreationFailed;
                }
                if (prepared == null || prepared.gameObject.activeInHierarchy)
                    return PlacementResult.CreationFailed;
                prepared.Configure(cell, catalog.TowerHealth, catalog.AttackRange,
                    catalog.FireInterval, catalog.AttackDamage);
                prepared.SetAttackTarget(attackTarget);
                prepared.SetAttackActive(attackActive);
                prepared.transform.position = map.CellToWorld(cell);

                // Recheck after fallible creation, before either inventory or occupancy changes.
                validity = EvaluateCore(cell);
                if (validity != PlacementResult.Ready) return validity;
                if (navigation.TryOccupy(cell, prepared) != OccupancyResult.Changed)
                    return PlacementResult.RegistrationFailed;
                if (!inventory.TrySpend(catalog.WoodCost, catalog.StoneCost, catalog.MetalCost))
                {
                    navigation.TryRelease(cell, prepared);
                    return PlacementResult.PaymentFailed;
                }

                prepared.Removed += OnTowerRemoved;
                towers.Add(prepared);
                prepared.gameObject.SetActive(true);
                builtTower = prepared;
                prepared = null;
                CancelPlacement();
                Built?.Invoke(builtTower);
                return PlacementResult.Ready;
            }
            finally
            {
                committing = false;
                if (prepared != null) DestroyObject(prepared.gameObject);
            }
        }

        public void ResetRun()
        {
            CancelPlacement();
            for (var index = towers.Count - 1; index >= 0; index--)
            {
                var tower = towers[index];
                if (tower == null) continue;
                tower.Removed -= OnTowerRemoved;
                navigation.TryRelease(tower.Cell, tower);
                DestroyObject(tower.gameObject);
            }
            towers.Clear();
            phase = Stage2BuildPhase.Inactive;
            paused = false;
        }

        private void OnTowerRemoved(Stage2Tower tower)
        {
            tower.Removed -= OnTowerRemoved;
            if (!towers.Remove(tower)) return;
            navigation.TryRelease(tower.Cell, tower);
            DestroyObject(tower.gameObject);
        }

        private static void DestroyObject(GameObject instance)
        {
            if (instance == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(instance);
            else UnityEngine.Object.DestroyImmediate(instance);
        }
    }
}
