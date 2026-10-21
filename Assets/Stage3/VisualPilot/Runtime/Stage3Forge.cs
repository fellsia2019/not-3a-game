using System;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.VisualPilot
{
    public enum Stage3ForgePlacementResult { NotPlacing, Ready, InvalidGround, Occupied, HeroOverlap, InsufficientResources, CreationFailed, Built }

    /// <summary>One resource-paid green-ground forge, local service and existing obstacle/input ownership.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage3Forge : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage3ResourceDistrictWorld world;
        private Stage3ForgePresentation presentation;
        private HeroObstacleMap obstacles;
        private Rect builtObstacle;
        private InputAction interact, confirm, cancel;
        private bool requiresRelease = true, confirmRequiresRelease = true;
        public Stage3ForgeCatalog Catalog { get; private set; }
        public Stage3ForgeArt Art { get; private set; }
        public Stage2EquipmentDriver EquipmentDriver { get; private set; }
        public bool IsReady { get; private set; }
        public bool IsBuilt { get; private set; }
        public bool IsPlacing { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 InteractionPosition => Position + Catalog.EntranceOffset;
        public Vector2 PreviewPosition { get; private set; }
        public bool PreviewVisible { get; private set; }
        public Stage3ForgePlacementResult PreviewResult { get; private set; }
        public event Action OpenRequested, Changed;
        private bool Active => isActiveAndEnabled && IsReady && !game.RunDriver.Run.IsTerminal &&
            !game.RunDriver.Run.IsPaused && Time.timeScale > 0 && !(GetComponent<Stage2GameHud>()?.DebugOpen ?? false);
        private bool OtherDraft => game.Building.Defense.IsPlacing || (game.Production?.IsPlacing ?? false) ||
            (GetComponent<Stage3Dwelling>()?.IsPlacing ?? false) || (GetComponent<Stage3WorldTravel>()?.IsPlacingWorkshop ?? false) || (GetComponent<Stage3WorldTravel>()?.IsPlacingBaseCircle ?? false);
        public bool NearEntrance => IsBuilt && scene != null && Vector2.Distance(scene.PlayerMover.transform.position, InteractionPosition) <= Catalog.InteractionRadius;
        public bool CanBeginPlacement => Active && !game.PlayerInputBlocked && !IsBuilt && game.Gathering.Economy.Inventory.CanAfford(Catalog.BuildCost);
        public bool CanUse => Active && IsBuilt && NearEntrance && !IsPlacing && !OtherDraft &&
            (!game.PlayerInputBlocked || GetComponent<Stage3FantasyHud>().ActiveWindow == Stage3PlayerWindow.Forge);
        public bool CanUpgradeBuilding => CanUse && EquipmentDriver.Rules.EvaluateForgeUpgrade() == Stage2EquipmentResult.Ready;
        public string UpgradeUnavailableReason => !IsBuilt ? "Сначала постройте кузницу" : !NearEntrance ? "Подойдите ко входу в кузницу" :
            EquipmentDriver.Rules.ForgeLevel >= EquipmentDriver.Catalog.ForgeStageCount ? "Максимальная ступень" :
            !CanUse ? "Кузница сейчас недоступна" : EquipmentDriver.Rules.EvaluateForgeUpgrade()==Stage2EquipmentResult.ResearchRequired ?
                "Изучите чертёж кузницы "+(EquipmentDriver.Rules.ForgeLevel==1?"II":"III")+" в мастерской" :
            EquipmentDriver.Rules.Blueprints==null&&EquipmentDriver.Rules.ForgeUpgradeScrolls < 1 ? "Нужен свиток улучшения кузницы" : null;
        public void Initialize(Stage3ForgeCatalog data, Stage3ForgeArt art, Stage2EquipmentDriver equipment)
        {
            if (data == null || !data.TryValidate(out _)) throw new ArgumentException("Invalid forge catalog.");
            game = GetComponent<Stage2GameComposition>(); scene = GetComponent<Stage2SceneBootstrap>();
            world = GetComponent<Stage3ResourceDistrictWorld>(); obstacles = world.Obstacles;
            Catalog = data; Art = art; EquipmentDriver = equipment;
            presentation = gameObject.AddComponent<Stage3ForgePresentation>();
            presentation.Initialize(this, art, scene.CameraFollow.BoundCamera);
            game.Building.PlacementStarting += CancelPlacement;
            if (game.Production != null) game.Production.PlacementStarting += CancelPlacement;
            game.RunDriver.Run.Reset += ResetForge;
            game.RunDriver.Run.Rewound += AfterRetry;
            IsReady = true; Changed?.Invoke();
        }
        public void BeginPlacement()
        {
            if (!CanBeginPlacement) return;
            game.Building.CancelPlacement(); game.Production?.CancelPlacement();
            GetComponent<Stage3WorldTravel>()?.CancelConstructionPlacement(); GetComponent<Stage3Dwelling>()?.CancelPlacement();
            IsPlacing = true; confirmRequiresRelease = requiresRelease = true;
            game.Gathering.SetProductionBuildMode(true); Changed?.Invoke();
        }
        public void CancelPlacement()
        {
            if (IsPlacing && !OtherDraft) game?.Gathering.SetProductionBuildMode(false);
            IsPlacing = PreviewVisible = false; PreviewResult = Stage3ForgePlacementResult.NotPlacing; Changed?.Invoke();
        }
        private Vector2 Snap(Vector2 point) => scene.MapDefinition.CellToWorld(scene.MapDefinition.WorldToCell(point));
        private Rect Footprint(Vector2 point) => new Rect(point - Catalog.Footprint * .5f, Catalog.Footprint);
        private static float Distance(Rect area, Vector2 point) => Vector2.Distance(point,
            new Vector2(Mathf.Clamp(point.x, area.xMin, area.xMax), Mathf.Clamp(point.y, area.yMin, area.yMax)));
        public Stage3ForgePlacementResult EvaluateAt(Vector2 point)
        {
            if (!IsPlacing || !Active || game.PlayerInputBlocked || IsBuilt) return Stage3ForgePlacementResult.NotPlacing;
            if (!float.IsFinite(point.x) || !float.IsFinite(point.y)) return Stage3ForgePlacementResult.InvalidGround;
            point = Snap(point); var area = Footprint(point);
            if(GetComponent<Stage3WorldTravel>()?.OverlapsCity(area)??false)return Stage3ForgePlacementResult.Occupied; var map = scene.MapDefinition;
            var min = map.WorldToCell(area.min); var max = map.WorldToCell(area.max);
            for (var y = min.y; y <= max.y; y++) for (var x = min.x; x <= max.x; x++)
            { var cell = new Vector2Int(x, y); if (!map.Contains(cell) || !map.IsWalkable(cell) || map.GetSurface(cell) != MapSurfaceKind.Forest) return Stage3ForgePlacementResult.InvalidGround; }
            if (Distance(area, scene.PlayerMover.transform.position) <= scene.PlayerMover.CollisionRadius) return Stage3ForgePlacementResult.HeroOverlap;
            foreach (var rect in obstacles.Rectangles) if (area.Overlaps(rect)) return Stage3ForgePlacementResult.Occupied;
            foreach (var circle in obstacles.Circles) if (Distance(area, circle.Center) <= circle.Radius) return Stage3ForgePlacementResult.Occupied;
            foreach (var district in world.Districts)
            { var b = district.Bounds; if (area.Overlaps(new Rect(b.xMin,b.yMin,b.width,b.height))) return Stage3ForgePlacementResult.Occupied; }
            var travel = GetComponent<Stage3WorldTravel>();
            if (travel != null) foreach (var site in travel.SitePositions) if (Distance(area, site) <= 3) return Stage3ForgePlacementResult.Occupied;
            if (Distance(area, scene.ThroneMarker.position) <= 3 || Distance(area, scene.EntranceMarker.position) <= 3) return Stage3ForgePlacementResult.Occupied;
            foreach (var node in game.Gathering.Economy.Nodes)
                if (Distance(area,node.WorldPosition) <= (node == game.Gathering.Economy.GetNode(node.Resource) ? 3 : node.HarvestRadius + .5f)) return Stage3ForgePlacementResult.Occupied;
            var entrance = point + Catalog.EntranceOffset; var entryCell = map.WorldToCell(entrance);
            if (!map.Contains(entryCell) || !map.IsWalkable(entryCell) || Distance(area,entrance) < scene.PlayerMover.CollisionRadius ||
                obstacles.IsBlocked(entrance,scene.PlayerMover.CollisionRadius)) return Stage3ForgePlacementResult.Occupied;
            return game.Gathering.Economy.Inventory.CanAfford(Catalog.BuildCost) ? Stage3ForgePlacementResult.Ready : Stage3ForgePlacementResult.InsufficientResources;
        }
        public Stage3ForgePlacementResult TryBuildAt(Vector2 point)
        {
            var result = EvaluateAt(point); if (result != Stage3ForgePlacementResult.Ready) return PreviewResult = result;
            point = Snap(point);
            if (!presentation.PrepareBuild(point)) return PreviewResult = Stage3ForgePlacementResult.CreationFailed;
            if (!game.Gathering.Economy.Inventory.TrySpend(Catalog.BuildCost))
            { presentation.CancelPreparedBuild(); return PreviewResult = Stage3ForgePlacementResult.InsufficientResources; }
            Position = point; builtObstacle = Footprint(point); obstacles.AddRectangle(builtObstacle); IsBuilt = true;
            presentation.CompleteBuild(); EquipmentDriver.Rules.ActivateForge(); CancelPlacement(); requiresRelease = true;
            game.Gathering.CancelGatherUntilRelease(); return PreviewResult = Stage3ForgePlacementResult.Built;
        }
        public bool TryOpen()
        {
            if (!CanUse) return false;
            game.Gathering.CancelGatherUntilRelease(); requiresRelease = true; OpenRequested?.Invoke(); return true;
        }
        public bool TryUpgradeBuilding()
        {
            if (!CanUpgradeBuilding) return false;
            return EquipmentDriver.Rules.TryUpgradeForge() == Stage2EquipmentResult.Ready;
        }
        private void OnEnable()
        {
            interact = new InputAction("Use forge",InputActionType.Button,"<Keyboard>/e");
            confirm = new InputAction("Place forge",InputActionType.Button,"<Mouse>/leftButton");
            cancel = new InputAction("Cancel forge",InputActionType.Button,"<Keyboard>/escape"); cancel.AddBinding("<Mouse>/rightButton");
            interact.Enable(); confirm.Enable(); cancel.Enable(); requiresRelease = confirmRequiresRelease = true;
        }
        private void Update()
        {
            if (!IsReady) return;
            if (!ReferenceEquals(obstacles,world.Obstacles)) { ResetForge(); obstacles = world.Obstacles; }
            var held = interact.IsPressed(); if (!held) requiresRelease = false;
            if (!Active || game.PlayerInputBlocked) { requiresRelease |= held; if (IsPlacing) CancelPlacement(); return; }
            if (IsPlacing)
            {
                if (OtherDraft || cancel.WasPressedThisFrame()) { CancelPlacement(); return; }
                if (!confirm.IsPressed()) confirmRequiresRelease = false;
                var mouse = Mouse.current; var distance = 0f;
                var ray = mouse == null ? default : scene.CameraFollow.BoundCamera.ScreenPointToRay(mouse.position.ReadValue());
                PreviewVisible = mouse != null && !game.ContainsScreenPoint(mouse.position.ReadValue()) && new Plane(Vector3.forward,Vector3.zero).Raycast(ray,out distance);
                if (PreviewVisible)
                { PreviewPosition = Snap(ray.GetPoint(distance)); PreviewResult = EvaluateAt(PreviewPosition); if (!confirmRequiresRelease && confirm.WasPressedThisFrame()) TryBuildAt(PreviewPosition); }
            }
            if (!requiresRelease && interact.WasPressedThisFrame()) { TryOpen(); requiresRelease = true; }
        }
        private void ResetForge()
        {
            if (IsBuilt && obstacles != null) obstacles.RemoveRectangle(builtObstacle);
            IsBuilt = false; Position = Vector2.zero; CancelPlacement(); presentation?.Clear(); requiresRelease = true;
            EquipmentDriver?.Rules.DeactivateForge();
        }
        private void AfterRetry() { CancelPlacement(); requiresRelease = confirmRequiresRelease = true; }
        private void OnDisable()
        { interact?.Dispose(); confirm?.Dispose(); cancel?.Dispose(); interact = confirm = cancel = null; CancelPlacement(); }
        private void OnDestroy()
        {
            if (game != null)
            {
                game.Building.PlacementStarting -= CancelPlacement;
                if (game.Production != null) game.Production.PlacementStarting -= CancelPlacement;
                game.RunDriver.Run.Reset -= ResetForge; game.RunDriver.Run.Rewound -= AfterRetry;
            }
            if (IsBuilt && obstacles != null) obstacles.RemoveRectangle(builtObstacle);
        }
    }
}
