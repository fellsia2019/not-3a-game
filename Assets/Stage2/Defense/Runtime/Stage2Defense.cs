using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using UnityEngine;

namespace Not3A.Stage2.Defense
{
    public enum Stage2BuildPhase { Inactive, Preparation, Wave }
    public enum Stage2TowerActionResult
    {
        Ready, Busy, Inactive, Paused, InvalidTarget, MaximumLevel, CannotAfford,
        Incompatible, InvalidCell, Occupied, HeroProtected, EnemyProtected,
        EnemyStateUnavailable, PreparationOnly, CapacityExceeded
    }

    public enum PlacementResult
    {
        Ready, NotPlacing, Inactive, Paused, InvalidCell, Occupied,
        HeroProtected, EnemyProtected, EnemyStateUnavailable, CannotAfford,
        CreationFailed, RegistrationFailed, PaymentFailed, ResearchRequired
    }

    /// <summary>Main-thread placement transaction. The scene owner supplies run state and actor protection.</summary>
    public sealed class Stage2Defense
    {
        public sealed class Snapshot
        {
            internal readonly TowerState[] Towers;
            internal readonly int Selected;
            internal Snapshot(TowerState[] towers, int selected) { Towers = towers; Selected = selected; }
        }
        internal readonly struct TowerState
        {
            internal readonly int Role, Health, Level;
            internal readonly Stage2ResourceCost Invested;
            internal readonly Vector2Int Cell;
            internal readonly float Cooldown;
            internal TowerState(Stage2Tower tower)
            { Role = tower.RoleIndex; Health = tower.Health; Cell = tower.Cell; Cooldown = tower.CooldownRemaining;
                Level = tower.Level; Invested = tower.Invested; }
        }
        private readonly Stage2MapDefinition map;
        private readonly Stage2Navigation navigation;
        private readonly Stage2Inventory inventory;
        private readonly IReadOnlyList<Stage2DefenseCatalog> catalogs;
        private readonly Func<Vector2> heroWorld;
        private readonly Func<Stage2DefenseCatalog, Stage2Tower> prepareTower;
        private readonly List<Stage2Tower> towers = new List<Stage2Tower>();
        private Func<Vector2Int, bool> enemyProtected;
        private Func<int, bool> roleAvailable;
        private Func<Vector2, float, int, float, float, bool> attackTarget;
        private bool attackActive = true;
        private Vector2Int? previewCell;
        private bool placing;
        private bool paused;
        private bool committing;
        private Stage2BuildPhase phase = Stage2BuildPhase.Inactive;
        private int selectedCatalogIndex;

        public Stage2Defense(Stage2MapDefinition map, Stage2Navigation navigation,
            Stage2Inventory inventory, Stage2DefenseCatalog catalog, Func<Vector2> heroWorld,
            Func<Stage2Tower> prepareTower)
            : this(map, navigation, inventory, new[] { catalog }, heroWorld,
                _ => prepareTower())
        {
            if (prepareTower == null) throw new ArgumentNullException(nameof(prepareTower));
        }

        public Stage2Defense(Stage2MapDefinition map, Stage2Navigation navigation,
            Stage2Inventory inventory, IReadOnlyList<Stage2DefenseCatalog> catalogs,
            Func<Vector2> heroWorld, Func<Stage2DefenseCatalog, Stage2Tower> prepareTower)
        {
            if (map == null || !map.ValidateDefinition().IsValid) throw new ArgumentException("Invalid map.", nameof(map));
            if (navigation == null || !navigation.IsValid || navigation.MapHash != map.ComputeDeterministicContentHash())
                throw new ArgumentException("Navigation must use the same valid map.", nameof(navigation));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (catalogs == null || catalogs.Count == 0) throw new ArgumentException("At least one defense catalog is required.", nameof(catalogs));
            foreach (var item in catalogs)
                if (item == null || !item.TryValidate(out _))
                    throw new ArgumentException("Invalid defense catalog.", nameof(catalogs));
            this.heroWorld = heroWorld ?? throw new ArgumentNullException(nameof(heroWorld));
            this.prepareTower = prepareTower ?? throw new ArgumentNullException(nameof(prepareTower));
            this.map = map;
            this.navigation = navigation;
            this.inventory = inventory;
            this.catalogs = catalogs;
        }

        public event Action<Stage2Tower> Built;
        public bool IsPlacing => placing;
        public int TowerCount => towers.Count;
        public Stage2Tower GetTower(int index) => towers[index];
        public Stage2DefenseCatalog GetTowerCatalog(Stage2Tower tower) =>
            tower != null && towers.Contains(tower) ? catalogs[tower.RoleIndex] : null;
        public Vector2Int? PreviewCell => previewCell;
        public int RoleCount => catalogs.Count;
        public int SelectedRoleIndex => selectedCatalogIndex;
        public Stage2DefenseCatalog GetCatalog(int index) => catalogs[index];
        public Stage2DefenseCatalog SelectedCatalog => catalogs[selectedCatalogIndex];

        public Snapshot CaptureSnapshot()
        {
            var saved = new List<TowerState>();
            foreach (var tower in towers) if (tower != null && tower.IsAlive) saved.Add(new TowerState(tower));
            return new Snapshot(saved.ToArray(), selectedCatalogIndex);
        }

        public bool RestoreSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return false;
            var prepared = new List<Stage2Tower>();
            try
            {
                // Finish fallible object creation before replacing the lost attempt's world.
                foreach (var saved in snapshot.Towers)
                {
                    var catalog = catalogs[saved.Role];
                    Stage2Tower tower;
                    try { tower = prepareTower(catalog); }
                    catch (Exception) { return false; }
                    if (tower == null) return false;
                    prepared.Add(tower);
                    if (tower.gameObject.activeInHierarchy) return false;
                    tower.Configure(saved.Cell, catalog.TowerHealth, catalog.AttackRange, catalog.FireInterval,
                        catalog.AttackDamage, catalog.SplashRadiusCells, catalog.ProjectileFlightSeconds, catalog.CanAttack);
                    tower.RoleIndex = saved.Role;
                    for (var level = 1; level < saved.Level; level++)
                    {
                        if (!catalog.TryGetUpgrade(level, out var upgrade)) return false;
                        tower.ApplyUpgrade(upgrade, saved.Invested, 1d, 0f);
                    }
                    tower.SetProgression(saved.Level, saved.Invested);
                    tower.RestoreState(saved.Health, saved.Cooldown);
                    tower.transform.position = map.CellToWorld(saved.Cell);
                    tower.SetAttackTarget(attackTarget); tower.SetAttackActive(false);
                }
                ResetRun(); navigation.Reset();
                foreach (var tower in prepared)
                {
                    if (navigation.TryOccupy(tower.Cell, tower) != OccupancyResult.Changed)
                        throw new InvalidOperationException("Captured tower cell could not be restored.");
                    tower.Removed += OnTowerRemoved; towers.Add(tower);
                }
                selectedCatalogIndex = snapshot.Selected;
                foreach (var tower in prepared) { tower.gameObject.SetActive(true); Built?.Invoke(tower); }
                prepared.Clear();
                return true;
            }
            finally { foreach (var tower in prepared) if (tower != null) DestroyObject(tower.gameObject); }
        }

        public bool SelectRole(int index)
        {
            if (committing || !IsRoleAvailable(index)) return false;
            selectedCatalogIndex = index;
            return true;
        }

        public void SetEnemyProtection(Func<Vector2Int, bool> isProtected) => enemyProtected = isProtected;
        public bool IsRoleAvailable(int index)=>index>=0&&index<catalogs.Count&&(roleAvailable?.Invoke(index)??true);
        public void SetRoleAvailability(Func<int,bool> available)
        {roleAvailable=available;if(!IsRoleAvailable(selectedCatalogIndex))CancelPlacement();}

        public void SetAttackTarget(Func<Vector2, float, int, float, float, bool> hitNearest)
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
            if (committing || paused || phase == Stage2BuildPhase.Inactive || !IsRoleAvailable(selectedCatalogIndex)) return false;
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
            if (!IsRoleAvailable(selectedCatalogIndex)) return PlacementResult.ResearchRequired;
            if (!map.Contains(cell) || map.GetSurface(cell) != MapSurfaceKind.InvasionCorridor ||
                !map.IsWalkable(cell) || !map.IsBuildable(cell) ||
                cell == map.EnemyEntrance || cell == map.Throne) return PlacementResult.InvalidCell;
            if (navigation.TryGetOccupant(cell, out _)) return PlacementResult.Occupied;
            if (map.WorldToCell(heroWorld()) == cell) return PlacementResult.HeroProtected;
            // Wave without a live movement-state provider must fail closed.
            if (phase == Stage2BuildPhase.Wave && enemyProtected == null)
                return PlacementResult.EnemyStateUnavailable;
            if (enemyProtected != null && enemyProtected(cell)) return PlacementResult.EnemyProtected;
            var catalog = SelectedCatalog;
            if (!inventory.CanAfford(catalog.WoodCost, catalog.StoneCost, catalog.MetalCost, catalog.GoldCost))
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
                var catalog = SelectedCatalog;
                try { prepared = prepareTower(catalog); }
                catch (Exception exception)
                {
                    Debug.LogWarning($"S2_DEF_CREATION_FAILED: {exception.Message}");
                    return PlacementResult.CreationFailed;
                }
                if (prepared == null || prepared.gameObject.activeInHierarchy)
                    return PlacementResult.CreationFailed;
                prepared.Configure(cell, catalog.TowerHealth, catalog.AttackRange,
                    catalog.FireInterval, catalog.AttackDamage, catalog.SplashRadiusCells, catalog.ProjectileFlightSeconds,
                    catalog.CanAttack);
                prepared.RoleIndex = selectedCatalogIndex;
                prepared.SetProgression(1, catalog.BuildCost);
                prepared.SetAttackTarget(attackTarget);
                prepared.SetAttackActive(attackActive);
                prepared.transform.position = map.CellToWorld(cell);

                // Recheck after fallible creation, before either inventory or occupancy changes.
                validity = EvaluateCore(cell);
                if (validity != PlacementResult.Ready) return validity;
                if (navigation.TryOccupy(cell, prepared) != OccupancyResult.Changed)
                    return PlacementResult.RegistrationFailed;
                if (!inventory.TrySpend(catalog.WoodCost, catalog.StoneCost, catalog.MetalCost, catalog.GoldCost))
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

        private Stage2TowerActionResult CheckOwned(Stage2Tower tower)
        {
            if (committing || placing) return Stage2TowerActionResult.Busy;
            if (phase == Stage2BuildPhase.Inactive) return Stage2TowerActionResult.Inactive;
            if (paused) return Stage2TowerActionResult.Paused;
            if (tower == null || !tower.IsAlive || !towers.Contains(tower) ||
                !navigation.TryGetOccupant(tower.Cell, out var owner) || !ReferenceEquals(owner, tower))
                return Stage2TowerActionResult.InvalidTarget;
            return Stage2TowerActionResult.Ready;
        }

        public Stage2TowerActionResult EvaluateUpgrade(Stage2Tower tower)
        {
            var result = CheckOwned(tower);
            if (result != Stage2TowerActionResult.Ready) return result;
            if (!catalogs[tower.RoleIndex].TryGetUpgrade(tower.Level, out var upgrade))
                return catalogs[tower.RoleIndex].UnlimitedUpgrades ? Stage2TowerActionResult.CapacityExceeded : Stage2TowerActionResult.MaximumLevel;
            if (!tower.Invested.TryAdd(upgrade.Cost, out _)) return Stage2TowerActionResult.CapacityExceeded;
            return inventory.CanAfford(upgrade.Cost) ? Stage2TowerActionResult.Ready : Stage2TowerActionResult.CannotAfford;
        }

        public Stage2TowerActionResult TryUpgrade(Stage2Tower tower)
        {
            var result = EvaluateUpgrade(tower);
            if (result != Stage2TowerActionResult.Ready) return result;
            catalogs[tower.RoleIndex].TryGetUpgrade(tower.Level, out var upgrade);
            tower.Invested.TryAdd(upgrade.Cost, out var invested);
            var healthFraction = tower.Health / (double)tower.MaximumHealth;
            var cooldownFraction = tower.CooldownRemaining / tower.FireInterval;
            committing = true;
            try
            {
                if (!inventory.TrySpend(upgrade.Cost)) return Stage2TowerActionResult.CannotAfford;
                tower.ApplyUpgrade(upgrade, invested, healthFraction, cooldownFraction);
                return Stage2TowerActionResult.Ready;
            }
            finally { committing = false; }
        }

        public Stage2TowerActionResult EvaluateMerge(Stage2Tower donor, Stage2Tower recipient)
        {
            var result = CheckOwned(donor);
            if (result != Stage2TowerActionResult.Ready) return result;
            result = CheckOwned(recipient);
            if (result != Stage2TowerActionResult.Ready) return result;
            if (!catalogs[donor.RoleIndex].AllowMerging || !catalogs[recipient.RoleIndex].AllowMerging)
                return Stage2TowerActionResult.Incompatible;
            if (donor == recipient || donor.RoleIndex != recipient.RoleIndex || donor.Level != recipient.Level)
                return Stage2TowerActionResult.Incompatible;
            if (!catalogs[recipient.RoleIndex].TryGetUpgrade(recipient.Level, out _))
                return Stage2TowerActionResult.MaximumLevel;
            return donor.Invested.TryAdd(recipient.Invested, out _) ? Stage2TowerActionResult.Ready : Stage2TowerActionResult.CapacityExceeded;
        }

        public Stage2TowerActionResult TryMerge(Stage2Tower donor, Stage2Tower recipient)
        {
            var result = EvaluateMerge(donor, recipient);
            if (result != Stage2TowerActionResult.Ready) return result;
            catalogs[recipient.RoleIndex].TryGetUpgrade(recipient.Level, out var upgrade);
            donor.Invested.TryAdd(recipient.Invested, out var invested);
            var healthFraction = (donor.Health + (double)recipient.Health) / (donor.MaximumHealth + (double)recipient.MaximumHealth);
            var cooldownFraction = Mathf.Max(donor.CooldownRemaining / donor.FireInterval,
                recipient.CooldownRemaining / recipient.FireInterval);
            committing = true;
            try
            {
                recipient.ApplyUpgrade(upgrade, invested, healthFraction, cooldownFraction);
                donor.Retire();
                return Stage2TowerActionResult.Ready;
            }
            finally { committing = false; }
        }

        public Stage2ResourceCost GetSaleRefund(Stage2Tower tower) =>
            tower != null && towers.Contains(tower) ? tower.Invested.Half : default;

        public Stage2TowerActionResult EvaluateSell(Stage2Tower tower)
        {
            var result = CheckOwned(tower);
            if (result != Stage2TowerActionResult.Ready) return result;
            return inventory.CanCredit(tower.Invested.Half) ? Stage2TowerActionResult.Ready : Stage2TowerActionResult.CapacityExceeded;
        }

        public Stage2TowerActionResult TrySell(Stage2Tower tower)
        {
            var result = EvaluateSell(tower);
            if (result != Stage2TowerActionResult.Ready) return result;
            var refund = tower.Invested.Half;
            committing = true;
            try
            {
                tower.Retire();
                inventory.TryCredit(refund);
                return Stage2TowerActionResult.Ready;
            }
            finally { committing = false; }
        }

        public Stage2TowerActionResult EvaluateMove(Stage2Tower tower, Vector2Int cell)
        {
            var result = CheckOwned(tower);
            if (result != Stage2TowerActionResult.Ready) return result;
            if (phase != Stage2BuildPhase.Preparation) return Stage2TowerActionResult.PreparationOnly;
            if (!map.Contains(cell) || map.GetSurface(cell) != MapSurfaceKind.InvasionCorridor ||
                !map.IsWalkable(cell) || !map.IsBuildable(cell) || cell == map.EnemyEntrance || cell == map.Throne)
                return Stage2TowerActionResult.InvalidCell;
            if (cell == tower.Cell) return Stage2TowerActionResult.Ready;
            if (navigation.TryGetOccupant(cell, out _)) return Stage2TowerActionResult.Occupied;
            if (map.WorldToCell(heroWorld()) == cell) return Stage2TowerActionResult.HeroProtected;
            if (enemyProtected != null && enemyProtected(cell)) return Stage2TowerActionResult.EnemyProtected;
            return Stage2TowerActionResult.Ready;
        }

        public Stage2TowerActionResult TryMove(Stage2Tower tower, Vector2Int cell)
        {
            var result = EvaluateMove(tower, cell);
            if (result != Stage2TowerActionResult.Ready || tower.Cell == cell) return result;
            committing = true;
            try
            {
                if (navigation.TryOccupy(cell, tower) != OccupancyResult.Changed) return Stage2TowerActionResult.Occupied;
                navigation.TryRelease(tower.Cell, tower);
                tower.MoveTo(cell, map.CellToWorld(cell));
                return Stage2TowerActionResult.Ready;
            }
            finally { committing = false; }
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
                tower.gameObject.SetActive(false);
                DestroyObject(tower.gameObject);
            }
            towers.Clear();
            selectedCatalogIndex = 0;
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
