using System;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.VisualPilot
{
    public sealed partial class Stage3WorldTravel
    {
        public event Action PlacementStarting;
        public bool IsPlacingBaseCircle { get; private set; }
        public bool CanBeginBaseCirclePlacement => isActiveAndEnabled && ConstructionAllowed && state.Researched && !state.BaseBuilt;
        public Vector2 BaseCirclePreviewPosition { get; private set; }
        public bool BaseCirclePreviewVisible { get; private set; }
        public WorkshopPlacementResult BaseCirclePreviewResult { get; private set; }
        public Vector2 BaseCirclePlacementSize => Vector2.one * Mathf.Max(data.circleDiameter, data.siteClearance * 2f);

        // Astra owns the preview artwork; rule/input ownership remains here.
        partial void RefreshBaseCirclePreview(bool visible);
        partial void ClearBaseCirclePreview();

        public void BeginBaseCirclePlacement()
        {
            if (!CanBeginBaseCirclePlacement) return;
            game.Building.CancelPlacement(); game.Production?.CancelPlacement(); CancelConstructionPlacement();
            GetComponent<Stage3Dwelling>()?.CancelPlacement(); GetComponent<Stage3Forge>()?.CancelPlacement();
            PlacementStarting?.Invoke(); IsPlacingBaseCircle = true;
            confirmRequiresRelease = true; RequireFreshTravelInput();
            game.Gathering.SetProductionBuildMode(true);
        }

        public void CancelBaseCirclePlacement()
        {
            if (IsPlacingBaseCircle) game?.Gathering.SetProductionBuildMode(false);
            IsPlacingBaseCircle = BaseCirclePreviewVisible = false;
            BaseCirclePreviewResult = WorkshopPlacementResult.NotPlacing;
            ClearBaseCirclePreview();
            if (hud != null) hud.WorldInteractionHint = null;
        }

        public void CancelConstructionPlacement()
        { CancelWorkshopPlacement(); CancelBaseCirclePlacement(); }

        public WorkshopPlacementResult EvaluateBaseCircleAt(Vector2 point)
        {
            if (!IsPlacingBaseCircle || !CanBeginBaseCirclePlacement) return WorkshopPlacementResult.NotPlacing;
            if (!float.IsFinite(point.x) || !float.IsFinite(point.y)) return WorkshopPlacementResult.InvalidGround;
            point = SnapWorkshop(point);
            var area = new Rect(point - BaseCirclePlacementSize * .5f, BaseCirclePlacementSize);
            if (OverlapsCity(area)) return WorkshopPlacementResult.Occupied;
            var map = scene.MapDefinition; var min = map.WorldToCell(area.min); var max = map.WorldToCell(area.max);
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
            if (resourceWorld != null) foreach (var district in resourceWorld.Districts)
            {
                var b = district.Bounds;
                if (area.Overlaps(new Rect(b.xMin, b.yMin, b.width, b.height))) return WorkshopPlacementResult.Occupied;
            }
            foreach (var site in state.Points)
                if (site.Id != Stage3TravelState.BaseId && DistanceTo(area, site.Position) <= data.circleDiameter * .5f)
                    return WorkshopPlacementResult.Occupied;
            if (DistanceTo(area, WorkshopInteractionPosition) <= scene.PlayerMover.CollisionRadius ||
                DistanceTo(area, scene.ThroneMarker.position) <= data.siteClearance ||
                DistanceTo(area, scene.EntranceMarker.position) <= data.siteClearance) return WorkshopPlacementResult.Occupied;
            foreach (var node in game.Gathering.Economy.Nodes)
                if(!node.IsDepleted)
                if (DistanceTo(area, node.WorldPosition) <= (node == game.Gathering.Economy.GetNode(node.Resource) ?
                    data.siteClearance : node.InteractionRadius + .5f)) return WorkshopPlacementResult.Occupied;
            return WorkshopPlacementResult.Ready;
        }

        public WorkshopPlacementResult TryBuildBaseCircleAt(Vector2 point)
        {
            var result = EvaluateBaseCircleAt(point);
            if (result != WorkshopPlacementResult.Ready) return BaseCirclePreviewResult = result;
            point = SnapWorkshop(point);
            if (!state.BuildBaseCircle(point)) return BaseCirclePreviewResult = WorkshopPlacementResult.NotPlacing;
            BasePosition = point; RebuildCircle(ref baseCircle, ref baseMesh, "Base teleport circle", BasePosition);
            baseCircle.SetActive(true); CancelBaseCirclePlacement(); RequireFreshTravelInput();
            game.Gathering.CancelGatherUntilRelease(); RefreshMarkers(); OpeningsChanged?.Invoke();
            GetComponent<Stage3TacticalPresentation>()?.RefreshSiteClearances();
            return BaseCirclePreviewResult = WorkshopPlacementResult.Built;
        }

        private void UpdateBaseCircleConstruction()
        {
            if (!IsPlacingBaseCircle) return;
            if (!CanBeginBaseCirclePlacement || game.Building.Defense.IsPlacing || (game.Production?.IsPlacing ?? false) ||
                (GetComponent<Stage3Dwelling>()?.IsPlacing ?? false) || (GetComponent<Stage3Forge>()?.IsPlacing ?? false))
            { CancelBaseCirclePlacement(); return; }
            if (cancelWorkshop != null && cancelWorkshop.WasPressedThisFrame()) { CancelBaseCirclePlacement(); return; }
            if (confirmWorkshop != null && !confirmWorkshop.IsPressed()) confirmRequiresRelease = false;
            var mouse = Mouse.current; var distance = 0f;
            var ray = mouse == null ? default : scene.CameraFollow.BoundCamera.ScreenPointToRay(mouse.position.ReadValue());
            BaseCirclePreviewVisible = mouse != null && !game.ContainsScreenPoint(mouse.position.ReadValue()) &&
                new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out distance);
            if (BaseCirclePreviewVisible)
            {
                BaseCirclePreviewPosition = SnapWorkshop(ray.GetPoint(distance));
                BaseCirclePreviewResult = EvaluateBaseCircleAt(BaseCirclePreviewPosition);
            }
            else BaseCirclePreviewResult = WorkshopPlacementResult.InvalidGround;
            RefreshBaseCirclePreview(BaseCirclePreviewVisible);
            if (hud != null) hud.WorldInteractionHint = BaseCirclePreviewResult == WorkshopPlacementResult.Ready ?
                "ЛКМ · построить круг телепортации    Esc / ПКМ · отменить" : BaseCirclePreviewResult == WorkshopPlacementResult.HeroOverlap ?
                "Место занято героем · выберите соседнее" : "Нужно свободное место на зелёной земле · Esc / ПКМ отменяет";
            if (BaseCirclePreviewVisible && !confirmRequiresRelease && confirmWorkshop != null && confirmWorkshop.WasPressedThisFrame())
                TryBuildBaseCircleAt(BaseCirclePreviewPosition);
        }
    }
}
