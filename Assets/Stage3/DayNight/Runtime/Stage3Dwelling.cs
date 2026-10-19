using System;
using System.Collections;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.VisualPilot
{
    public enum DwellingPlacementResult
    { NotPlacing, Ready, InvalidGround, Occupied, HeroOverlap, InsufficientResources, CreationFailed, Built }

    /// <summary>One paid home, its ground occupancy and local day-only rest interaction.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage3Dwelling : MonoBehaviour
    {
        [SerializeField] private Stage3DwellingCatalog catalog;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud hud;
        private Stage3ResourceDistrictWorld world;
        private Stage3WorldTravel travel;
        private HeroObstacleMap obstacles;
        private Rect builtObstacle;
        private Stage3DwellingPresentation presentation;
        private InputAction interact, confirm, cancel;
        private bool requiresRelease = true, confirmRequiresRelease = true;

        public Stage3DwellingCatalog Catalog => catalog;
        public bool IsReady { get; private set; }
        public bool IsBuilt { get; private set; }
        public bool IsPlacing { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 InteractionPosition => Position + catalog.entranceOffset;
        public Vector2 PreviewPosition { get; private set; }
        public bool PreviewVisible { get; private set; }
        public DwellingPlacementResult PreviewResult { get; private set; }
        public event Action Changed;
        public event Action PlacementStarting;

        private bool Active => isActiveAndEnabled && IsReady && game != null && !game.RunDriver.Run.IsTerminal &&
            !game.PlayerInputBlocked && !game.RunDriver.Run.IsPaused && Time.timeScale > 0f && (hud == null || !hud.DebugOpen);
        public bool CanBeginPlacement => Active && !IsBuilt &&
            game.Gathering.Economy.Inventory.CanAfford(catalog.woodCost, 0, 0, 0);
        public Stage2WorkshopBuildStatus BuildStatus => IsBuilt ? Stage2WorkshopBuildStatus.Built :
            !Active ? Stage2WorkshopBuildStatus.Unavailable : IsPlacing ? Stage2WorkshopBuildStatus.Placing : Stage2WorkshopBuildStatus.Ready;
        public bool NearEntrance => IsBuilt && scene != null &&
            Vector2.Distance(scene.PlayerMover.transform.position, InteractionPosition) <= catalog.interactionRadius;
        public bool CanSkipDay => Active && IsBuilt && NearEntrance && game.RunDriver.Run.IsDay &&
            !IsPlacing && !game.Building.Defense.IsPlacing &&
            (game.Production == null || !game.Production.IsPlacing) && (travel == null || !travel.IsPlacingWorkshop);

        private IEnumerator Start()
        {
            yield return null;
            game = GetComponent<Stage2GameComposition>(); scene = GetComponent<Stage2SceneBootstrap>();
            if (catalog == null || !catalog.TryValidate(out var reason))
            { Debug.LogError("S3_DWELLING_BINDING_ERROR: incomplete dwelling catalog.", this); yield break; }
            if (game == null || scene == null || !game.IsInitialized || !game.RunDriver.Run.ContinuousDays) yield break;
            hud = GetComponent<Stage2GameHud>(); world = GetComponent<Stage3ResourceDistrictWorld>();
            travel = GetComponent<Stage3WorldTravel>();
            while (world == null || world.Obstacles == null || (travel != null && !travel.IsReady))
            { yield return null; world = GetComponent<Stage3ResourceDistrictWorld>(); travel = GetComponent<Stage3WorldTravel>(); }
            obstacles = world.Obstacles;
            presentation = gameObject.AddComponent<Stage3DwellingPresentation>();
            presentation.Initialize(this, catalog, scene.CameraFollow.BoundCamera);
            game.Building.PlacementStarting += CancelPlacement;
            if (game.Production != null) game.Production.PlacementStarting += CancelPlacement;
            game.RunDriver.Run.Reset += ResetDwelling;
            IsReady = true;
            Changed?.Invoke();
            Debug.Log("S3_DWELLING_READY: paid green-ground dwelling, local day-only rest; no skipped-time production.", this);
        }

        public void BeginPlacement()
        {
            if (!CanBeginPlacement) return;
            game.Building.CancelPlacement(); game.Production?.CancelPlacement(); travel?.CancelWorkshopPlacement();
            CancelPlacement(); PlacementStarting?.Invoke(); IsPlacing = true;
            confirmRequiresRelease = true; requiresRelease = true;
            game.Gathering.SetProductionBuildMode(true);
            Changed?.Invoke();
        }

        public void CancelPlacement()
        {
            if (IsPlacing) game?.Gathering.SetProductionBuildMode(false);
            IsPlacing = PreviewVisible = false; PreviewResult = DwellingPlacementResult.NotPlacing;
            Changed?.Invoke();
        }

        private Vector2 Snap(Vector2 point) => scene.MapDefinition.CellToWorld(scene.MapDefinition.WorldToCell(point));
        private Rect FootprintAt(Vector2 point) => new Rect(point - catalog.footprint * .5f, catalog.footprint);
        private static float DistanceTo(Rect area, Vector2 point) => Vector2.Distance(point,
            new Vector2(Mathf.Clamp(point.x, area.xMin, area.xMax), Mathf.Clamp(point.y, area.yMin, area.yMax)));

        public DwellingPlacementResult EvaluateAt(Vector2 point)
        {
            if (!IsPlacing || !Active || IsBuilt) return DwellingPlacementResult.NotPlacing;
            if (!Finite(point.x) || !Finite(point.y)) return DwellingPlacementResult.InvalidGround;
            point = Snap(point); var area = FootprintAt(point); var map = scene.MapDefinition;
            var min = map.WorldToCell(area.min); var max = map.WorldToCell(area.max);
            for (var y = min.y; y <= max.y; y++) for (var x = min.x; x <= max.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (!map.Contains(cell) || !map.IsWalkable(cell) || map.GetSurface(cell) != MapSurfaceKind.Forest)
                    return DwellingPlacementResult.InvalidGround;
            }
            if (DistanceTo(area, scene.PlayerMover.transform.position) <= scene.PlayerMover.CollisionRadius)
                return DwellingPlacementResult.HeroOverlap;
            foreach (var rect in obstacles.Rectangles) if (area.Overlaps(rect)) return DwellingPlacementResult.Occupied;
            foreach (var circle in obstacles.Circles) if (DistanceTo(area, circle.Center) <= circle.Radius) return DwellingPlacementResult.Occupied;
            foreach (var district in world.Districts)
            {
                var bounds = district.Bounds;
                if (area.Overlaps(new Rect(bounds.xMin, bounds.yMin, bounds.width, bounds.height))) return DwellingPlacementResult.Occupied;
            }
            if (travel != null)
                foreach (var site in travel.SitePositions)
                    if (DistanceTo(area, site) <= 3f) return DwellingPlacementResult.Occupied;
            if (DistanceTo(area, scene.ThroneMarker.position) <= 3f || DistanceTo(area, scene.EntranceMarker.position) <= 3f)
                return DwellingPlacementResult.Occupied;
            foreach (var node in game.Gathering.Economy.Nodes)
                if (DistanceTo(area, node.WorldPosition) <= (node == game.Gathering.Economy.GetNode(node.Resource) ?
                        3f : node.HarvestRadius + .5f)) return DwellingPlacementResult.Occupied;
            var entrance = point + catalog.entranceOffset;
            var cellAtEntrance = map.WorldToCell(entrance);
            if (!map.Contains(cellAtEntrance) || !map.IsWalkable(cellAtEntrance) ||
                DistanceTo(area, entrance) < scene.PlayerMover.CollisionRadius ||
                obstacles.IsBlocked(entrance, scene.PlayerMover.CollisionRadius)) return DwellingPlacementResult.Occupied;
            return game.Gathering.Economy.Inventory.CanAfford(catalog.woodCost, 0, 0, 0) ?
                DwellingPlacementResult.Ready : DwellingPlacementResult.InsufficientResources;
        }

        public DwellingPlacementResult TryBuildAt(Vector2 point)
        {
            var result = EvaluateAt(point);
            if (result != DwellingPlacementResult.Ready) return PreviewResult = result;
            point = Snap(point);
            if (!presentation.PrepareBuild(point)) return PreviewResult = DwellingPlacementResult.CreationFailed;
            if (!game.Gathering.Economy.Inventory.TrySpend(catalog.woodCost, 0, 0, 0))
            { presentation.CancelPreparedBuild(); return PreviewResult = DwellingPlacementResult.InsufficientResources; }
            Position = point; builtObstacle = FootprintAt(point); obstacles.AddRectangle(builtObstacle); IsBuilt = true;
            presentation.CompleteBuild(); CancelPlacement(); requiresRelease = true;
            game.Gathering.CancelGatherUntilRelease();
            return PreviewResult = DwellingPlacementResult.Built;
        }

        public bool TrySkipDay()
        {
            if (!CanSkipDay) return false;
            // End the preparation directly. Never tick economy/production through the skipped duration.
            game.Gathering.CancelGatherUntilRelease(); requiresRelease = true;
            return game.RunDriver.Run.StartWave();
        }

        private void OnEnable()
        {
            interact = new InputAction("Rest until night", InputActionType.Button, "<Keyboard>/e");
            confirm = new InputAction("Place dwelling", InputActionType.Button, "<Mouse>/leftButton");
            cancel = new InputAction("Cancel dwelling", InputActionType.Button, "<Keyboard>/escape");
            cancel.AddBinding("<Mouse>/rightButton"); interact.Enable(); confirm.Enable(); cancel.Enable(); requiresRelease = true;
        }

        private void Update()
        {
            if (!IsReady) return;
            if (!ReferenceEquals(obstacles, world.Obstacles))
            { ResetDwelling(); obstacles = world.Obstacles; requiresRelease = true; }
            var held = interact != null && interact.IsPressed();
            if (!held) requiresRelease = false;
            if (!Active) { requiresRelease |= held; if (IsPlacing) CancelPlacement(); return; }
            if (IsPlacing)
            {
                if (game.Building.Defense.IsPlacing || (game.Production != null && game.Production.IsPlacing) ||
                    (travel != null && travel.IsPlacingWorkshop) || cancel.WasPressedThisFrame()) { CancelPlacement(); return; }
                if (!confirm.IsPressed()) confirmRequiresRelease = false;
                var mouse = Mouse.current; var distance = 0f;
                var ray = mouse == null ? default : scene.CameraFollow.BoundCamera.ScreenPointToRay(mouse.position.ReadValue());
                PreviewVisible = mouse != null && !game.ContainsScreenPoint(mouse.position.ReadValue()) &&
                    new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out distance);
                if (PreviewVisible)
                {
                    PreviewPosition = Snap(ray.GetPoint(distance)); PreviewResult = EvaluateAt(PreviewPosition);
                    if (!confirmRequiresRelease && confirm.WasPressedThisFrame()) TryBuildAt(PreviewPosition);
                }
                else PreviewResult = DwellingPlacementResult.InvalidGround;
            }
            if (!requiresRelease && interact.WasPressedThisFrame()) { TrySkipDay(); requiresRelease = true; }
        }

        private void ResetDwelling()
        {
            if (IsBuilt && obstacles != null) obstacles.RemoveRectangle(builtObstacle);
            IsBuilt = false; Position = Vector2.zero; CancelPlacement(); presentation?.Clear(); requiresRelease = true;
        }

        private void OnDisable()
        {
            interact?.Dispose(); confirm?.Dispose(); cancel?.Dispose(); interact = confirm = cancel = null;
            CancelPlacement();
        }

        private void OnDestroy()
        {
            if (game != null)
            {
                if (game.Building != null) game.Building.PlacementStarting -= CancelPlacement;
                if (game.Production != null) game.Production.PlacementStarting -= CancelPlacement;
                if (game.RunDriver != null && game.RunDriver.Run != null) game.RunDriver.Run.Reset -= ResetDwelling;
            }
            if (IsBuilt && obstacles != null) obstacles.RemoveRectangle(builtObstacle);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
