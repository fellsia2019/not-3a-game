using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.VisualPilot
{
    public enum WorkshopPlacementResult { NotPlacing, Ready, InvalidGround, Occupied, HeroOverlap, Built }

    public sealed partial class Stage3WorldTravel
    {
        private InputAction confirmWorkshop, cancelWorkshop;
        private SpriteRenderer workshopPreview;
        private Stage2PlacementMark workshopOutline;
        private Material workshopPreviewMaterial, workshopGroundMaterial;
        private bool confirmRequiresRelease;
        public bool IsPlacingWorkshop { get; private set; }
        public WorkshopPlacementResult WorkshopPreviewResult { get; private set; }
        public Vector2 WorkshopPreviewPosition { get; private set; }
        public Vector2 WorkshopPlacementSize => Vector2.Max(data.workshopFootprint,
            new Vector2(data.workshopWidth, data.workshopFootprint.y)) + Vector2.one * (scene.PlayerMover.CollisionRadius * 2f);
        private bool ConstructionAllowed => Active && (hud == null || !hud.DebugOpen);
        public Stage2WorkshopBuildStatus WorkshopBuildStatus => state.WorkshopBuilt ? Stage2WorkshopBuildStatus.Built :
            !ConstructionAllowed ? Stage2WorkshopBuildStatus.Unavailable :
            IsPlacingWorkshop ? Stage2WorkshopBuildStatus.Placing : Stage2WorkshopBuildStatus.Ready;

        public void BeginWorkshopPlacement()
        {
            if (!ConstructionAllowed || state.WorkshopBuilt) return;
            game.Building.CancelPlacement(); game.Production?.CancelPlacement(); CancelConstructionPlacement();
            GetComponent<Stage3Dwelling>()?.CancelPlacement();GetComponent<Stage3Forge>()?.CancelPlacement();
            PlacementStarting?.Invoke();
            IsPlacingWorkshop = true; confirmRequiresRelease = true; requiresRelease = true;
            game.Gathering.SetProductionBuildMode(true);
            if (workshopPreviewMaterial == null) workshopPreviewMaterial = Stage2PlacementVisuals.Material(true);
            if (workshopGroundMaterial == null) workshopGroundMaterial = Stage2PlacementVisuals.Material(false);
            var previewObject = new GameObject("Research workshop preview"); previewObject.transform.SetParent(transform, false);
            workshopPreview = previewObject.AddComponent<SpriteRenderer>();
            workshopPreview.sprite = data.workshopSprite; workshopPreview.sharedMaterial = workshopPreviewMaterial;
            workshopOutline = Stage2PlacementVisuals.Outline(transform, WorkshopPlacementSize, workshopGroundMaterial, "Research workshop footprint");
            previewObject.SetActive(false); workshopOutline.gameObject.SetActive(false);
        }

        public void CancelWorkshopPlacement()
        {
            if (IsPlacingWorkshop) game?.Gathering.SetProductionBuildMode(false);
            IsPlacingWorkshop = false; WorkshopPreviewResult = WorkshopPlacementResult.NotPlacing;
            if (workshopPreview != null) { workshopPreview.gameObject.SetActive(false); Destroy(workshopPreview.gameObject); workshopPreview = null; }
            if (workshopOutline != null) { workshopOutline.gameObject.SetActive(false); Destroy(workshopOutline.gameObject); workshopOutline = null; }
            if (hud != null) hud.WorldInteractionHint = null;
        }

        private Vector2 SnapWorkshop(Vector2 point) => scene.MapDefinition.CellToWorld(scene.MapDefinition.WorldToCell(point));
        private Rect PlacementArea(Vector2 point) => new Rect(point - WorkshopPlacementSize * .5f, WorkshopPlacementSize);
        private static float DistanceTo(Rect area, Vector2 point) => Vector2.Distance(point,
            new Vector2(Mathf.Clamp(point.x, area.xMin, area.xMax), Mathf.Clamp(point.y, area.yMin, area.yMax)));

        public WorkshopPlacementResult EvaluateWorkshopAt(Vector2 point)
        {
            if (!IsPlacingWorkshop || !ConstructionAllowed || state.WorkshopBuilt) return WorkshopPlacementResult.NotPlacing;
            if (float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.x) || float.IsInfinity(point.y))
                return WorkshopPlacementResult.InvalidGround;
            point = SnapWorkshop(point);
            var area = PlacementArea(point); var map = scene.MapDefinition;
            if(OverlapsCity(area))return WorkshopPlacementResult.Occupied;
            var min = map.WorldToCell(area.min); var max = map.WorldToCell(area.max);
            for (var y = min.y; y <= max.y; y++) for (var x = min.x; x <= max.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (!map.Contains(cell) || !map.IsWalkable(cell) || map.GetSurface(cell) != MapSurfaceKind.Forest)
                    return WorkshopPlacementResult.InvalidGround;
            }
            if (DistanceTo(area, scene.PlayerMover.transform.position) <= scene.PlayerMover.CollisionRadius)
                return WorkshopPlacementResult.HeroOverlap;
            foreach (var rect in obstacles.Rectangles) if (area.Overlaps(rect)) return WorkshopPlacementResult.Occupied;
            foreach (var circle in obstacles.Circles) if (circle.Radius>0&&DistanceTo(area, circle.Center) <= circle.Radius) return WorkshopPlacementResult.Occupied;
            if (resourceWorld != null)
                foreach (var district in resourceWorld.Districts)
                {
                    var bounds = district.Bounds;
                    if (area.Overlaps(new Rect(bounds.xMin, bounds.yMin, bounds.width, bounds.height))) return WorkshopPlacementResult.Occupied;
                }
            if ((state.BaseBuilt && DistanceTo(area, BasePosition) <= data.circleDiameter * .5f) ||
                DistanceTo(area, RemotePosition) <= data.circleDiameter * .5f ||
                DistanceTo(area, scene.ThroneMarker.position) <= data.siteClearance ||
                DistanceTo(area, scene.EntranceMarker.position) <= data.siteClearance) return WorkshopPlacementResult.Occupied;
            foreach (var node in game.Gathering.Economy.Nodes)
                if(!node.IsDepleted)
                if (DistanceTo(area, node.WorldPosition) <= (node == game.Gathering.Economy.GetNode(node.Resource) ?
                        data.siteClearance : node.InteractionRadius + .5f))
                    return WorkshopPlacementResult.Occupied;
            // Keep the front interaction point reachable after construction.
            var approach = point + Vector2.down * (data.workshopFootprint.y * .5f + scene.PlayerMover.CollisionRadius + .25f);
            var approachCell = map.WorldToCell(approach);
            if (!map.Contains(approachCell) || !map.IsWalkable(approachCell) || obstacles.IsBlocked(approach, scene.PlayerMover.CollisionRadius))
                return WorkshopPlacementResult.Occupied;
            return WorkshopPlacementResult.Ready;
        }

        public WorkshopPlacementResult TryBuildWorkshopAt(Vector2 point)
        {
            var result = EvaluateWorkshopAt(point);
            if (result != WorkshopPlacementResult.Ready) return WorkshopPreviewResult = result;
            WorkshopPosition = SnapWorkshop(point);
            CreateWorkshop(); PlaceWorkshop(); state.BuildWorkshop();
            workshopObstacle = new Rect(WorkshopPosition - data.workshopFootprint * .5f, data.workshopFootprint);
            obstacles.AddRectangle(workshopObstacle);
            CancelWorkshopPlacement(); requiresRelease = true; game.Gathering.CancelGatherUntilRelease(); RefreshMarkers();
            GetComponent<Stage3TacticalPresentation>()?.RefreshSiteClearances();
            return WorkshopPreviewResult = WorkshopPlacementResult.Built;
        }

        private void EnableConstructionInput()
        {
            confirmWorkshop = new InputAction("Build research workshop", InputActionType.Button, "<Mouse>/leftButton");
            cancelWorkshop = new InputAction("Cancel research workshop", InputActionType.Button, "<Keyboard>/escape");
            cancelWorkshop.AddBinding("<Mouse>/rightButton"); confirmWorkshop.Enable(); cancelWorkshop.Enable();
        }
        private void DisableConstructionInput()
        {
            confirmWorkshop?.Dispose(); cancelWorkshop?.Dispose(); confirmWorkshop = cancelWorkshop = null;
            CancelConstructionPlacement();
        }
        private void ReleaseConstructionMaterials()
        {
            if (workshopPreviewMaterial != null) Destroy(workshopPreviewMaterial);
            if (workshopGroundMaterial != null) Destroy(workshopGroundMaterial);
        }
        private void UpdateConstruction()
        {
            if (!IsPlacingWorkshop) return;
            if (!ConstructionAllowed || game.Building.Defense.IsPlacing || (game.Production != null && game.Production.IsPlacing))
            { CancelWorkshopPlacement(); return; }
            if (cancelWorkshop != null && cancelWorkshop.WasPressedThisFrame()) { CancelWorkshopPlacement(); return; }
            if (confirmWorkshop != null && !confirmWorkshop.IsPressed()) confirmRequiresRelease = false;
            var mouse = Mouse.current;
            var distance = 0f;
            var ray = mouse == null ? default : scene.CameraFollow.BoundCamera.ScreenPointToRay(mouse.position.ReadValue());
            var visible = mouse != null && !game.ContainsScreenPoint(mouse.position.ReadValue()) &&
                new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out distance);
            workshopPreview.gameObject.SetActive(visible); workshopOutline.gameObject.SetActive(visible);
            if (!visible)
            {
                WorkshopPreviewResult = WorkshopPlacementResult.InvalidGround;
                if (hud != null) hud.WorldInteractionHint = "Выберите свободное место на зелёной земле · Esc / ПКМ отменяет";
                return;
            }
            WorkshopPreviewPosition = SnapWorkshop(ray.GetPoint(distance));
            WorkshopPreviewResult = EvaluateWorkshopAt(WorkshopPreviewPosition);
            PositionWorkshop(workshopPreview.transform, WorkshopPreviewPosition); workshopPreview.sortingOrder = 30000;
            var color = WorkshopPreviewResult == WorkshopPlacementResult.Ready ? Stage2PlacementVisuals.Valid : Stage2PlacementVisuals.Invalid;
            workshopPreview.color = color; workshopOutline.SetTint(new Color(color.r, color.g, color.b, .9f));
            workshopOutline.transform.position = new Vector3(WorkshopPreviewPosition.x, WorkshopPreviewPosition.y,
                Stage3TacticalPresentation.GrassSurfaceZ(WorkshopPreviewPosition) - .025f);
            if (hud != null) hud.WorldInteractionHint = WorkshopPreviewResult == WorkshopPlacementResult.Ready ?
                "ЛКМ · построить мастерскую    Esc / ПКМ · отменить" : WorkshopPreviewResult == WorkshopPlacementResult.HeroOverlap ?
                "Место занято героем · отойдите или выберите соседнее" : WorkshopPreviewResult == WorkshopPlacementResult.Occupied ?
                "Место или вход заняты ресурсом, зданием либо кругом · выберите соседнее" :
                "Весь фундамент должен быть на зелёной земле · Esc / ПКМ отменяет";
            if (!confirmRequiresRelease && confirmWorkshop != null && confirmWorkshop.WasPressedThisFrame()) TryBuildWorkshopAt(WorkshopPreviewPosition);
        }
    }
}
