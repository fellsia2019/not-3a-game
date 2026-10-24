using System;
using System.Globalization;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Not3A.Stage3.PlayerInterface
{
    public enum Stage3BuildingSelectionKind { None, Tower, Production, Workshop, Dwelling, Throne, Forge }

    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage3BuildingSelectionKind selectionKind;
        private Stage2Tower selectedTower;
        private Stage2ProductionBuilding selectedProduction;
        private Renderer[] selectionRenderers;
        private Stage3SelectedBuildingView selectedView;
        private int selectionStamp = int.MinValue;
        private bool selectionPressArmed;
        private Vector2 selectionPressPosition;
        private Stage2Tower pressedTower, draggedTower;
        private Vector2Int? dragTargetCell;
        private bool dragDropValid;
        private bool bufferedBuildingPress;
        private bool quickUpgradePress;
        private Vector2 bufferedBuildingPressPosition;
        private InputSettings buildingPointerSettings;
        private bool previousPointerMergingDisabled;
        public bool IsDraggingBuilding => draggedTower != null;
        public Stage2Tower DraggedTower => draggedTower;
        public Vector2Int? DragTargetCell => dragTargetCell;
        public bool DragDropValid => dragDropValid;
        public bool DragWillMerge
        {
            get
            {
                if (!dragDropValid || draggedTower == null || !dragTargetCell.HasValue) return false;
                var recipient = TowerAt(dragTargetCell.Value);
                return recipient != null && recipient != draggedTower;
            }
        }
        private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("ru-RU");

        public Stage3BuildingSelectionKind SelectedBuildingKind => selectionKind;
        public bool IsQuickUpgradeMode => WorldBuildingSelectionAvailable && !IsDraggingBuilding && AltHeld;
        private static bool AltHeld => Keyboard.current != null &&
            (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);

        public Stage2TowerActionResult EvaluateQuickUpgrade(Stage2Tower tower) =>
            IsQuickUpgradeMode && !HasBuildingDraft ? game.Building.Defense.EvaluateUpgrade(tower) : Stage2TowerActionResult.Inactive;

        public Stage2TowerActionResult TryQuickUpgradeAtScreen(Vector2 point)
        {
            if (!IsQuickUpgradeMode || game.ContainsScreenPoint(point)) return Stage2TowerActionResult.Inactive;
            var tower = FindTowerAtScreen(point);
            if (tower == null) return Stage2TowerActionResult.InvalidTarget;
            CancelPlacement();
            var result = game.Building.Defense.TryUpgrade(tower);
            if (result == Stage2TowerActionResult.Ready) PlayTowerUpgradeEffect(tower);
            InvalidateBuildingSelection();
            return result;
        }

        private void BindBuildingSelection()
        {
            BeginBuildingPointerCapture();
            presentation.WindowChanged += ClearBuildingSelection;
            presentation.SelectedBuildingCloseRequested += ClearBuildingSelection;
            presentation.SelectedBuildingUpgradeRequested += UpgradeSelectedBuilding;
            presentation.SelectedBuildingSellRequested += SellSelectedBuilding;
            presentation.SelectedBuildingSleepRequested += SleepInSelectedDwelling;
            presentation.SelectedBuildingForgeRequested += OpenSelectedForge;
            game.Gathering.Economy.Inventory.Changed += InvalidateBuildingSelection;
            game.RunDriver.Run.Reset += ClearBuildingSelection;
            game.RunDriver.Run.Rewound += ClearBuildingSelection;
        }

        private void UnbindBuildingSelection()
        {
            EndBuildingPointerCapture();
            if (presentation != null)
            {
                presentation.WindowChanged -= ClearBuildingSelection;
                presentation.SelectedBuildingCloseRequested -= ClearBuildingSelection;
                presentation.SelectedBuildingUpgradeRequested -= UpgradeSelectedBuilding;
                presentation.SelectedBuildingSellRequested -= SellSelectedBuilding;
                presentation.SelectedBuildingSleepRequested -= SleepInSelectedDwelling;
                presentation.SelectedBuildingForgeRequested -= OpenSelectedForge;
            }
            if (game != null && game.RunDriver != null)
            {
                game.RunDriver.Run.Reset -= ClearBuildingSelection;
                game.RunDriver.Run.Rewound -= ClearBuildingSelection;
                game.Gathering.Economy.Inventory.Changed -= InvalidateBuildingSelection;
            }
            ReleaseBuildingDragPresentation();
        }

        private void BeginBuildingPointerCapture()
        {
            if (buildingPointerSettings != null) return;
            buildingPointerSettings = InputSystem.settings;
            previousPointerMergingDisabled = buildingPointerSettings.disableRedundantEventsMerging;
            // Merging a press with later held motion can replace its position before
            // onEvent runs. Capture the actual sequence while this input owner is active.
            buildingPointerSettings.disableRedundantEventsMerging = true;
            InputSystem.onEvent += ObserveBuildingPointerPress;
        }

        private void EndBuildingPointerCapture()
        {
            InputSystem.onEvent -= ObserveBuildingPointerPress;
            if (buildingPointerSettings != null)
                buildingPointerSettings.disableRedundantEventsMerging = previousPointerMergingDisabled;
            buildingPointerSettings = null;
            bufferedBuildingPress = false;
        }

        private void ObserveBuildingPointerPress(InputEventPtr inputEvent, InputDevice device)
        {
            var mouse = Mouse.current;
            if (!bound || !isActiveAndEnabled || mouse == null || device != mouse || bufferedBuildingPress ||
                !(inputEvent.IsA<StateEvent>() || inputEvent.IsA<DeltaStateEvent>()) ||
                !mouse.leftButton.ReadValueFromEvent(inputEvent, out var value) || value < .5f ||
                mouse.leftButton.ReadValue() >= .5f) return;
            // Input System can process down, motion and up before the next MonoBehaviour
            // Update. Preserve the press location before the final position replaces it.
            bufferedBuildingPressPosition = mouse.position.ReadValueFromEvent(inputEvent, out var position)
                ? position : mouse.position.ReadValue();
            bufferedBuildingPress = true;
        }

        private void InvalidateBuildingSelection() => selectionStamp = int.MinValue;
        private void UpgradeSelectedBuilding()
        {
            if (selectionKind == Stage3BuildingSelectionKind.Throne)
            { TryUpgradeSelectedThrone(); return; }
            if (selectionKind == Stage3BuildingSelectionKind.Production)
            { TryUpgradeSelectedProduction(); return; }
            if(selectionKind==Stage3BuildingSelectionKind.Forge)
            {
                var forge=GetComponent<Stage3Forge>();
                if(SelectionAvailable&&!IsDraggingBuilding&&forge!=null&&forge.TryUpgradeBuilding())
                    presentation.PlayBuildingUpgradeEffect(forge.Position,forge.Catalog.Footprint);
                InvalidateBuildingSelection();RefreshBuildingSelection();
            }
            else TryUpgradeSelectedBuilding();
        }
        private void OpenSelectedForge()
        {
            if(SelectionAvailable&&!IsDraggingBuilding&&selectionKind==Stage3BuildingSelectionKind.Forge)
                GetComponent<Stage3Forge>()?.TryOpen();
        }
        private void SellSelectedBuilding() => TrySellSelectedBuilding();
        public bool TrySleepInSelectedDwelling()
        {
            if (!SelectionAvailable || IsDraggingBuilding || selectionKind != Stage3BuildingSelectionKind.Dwelling) return false;
            var dwelling = GetComponent<Stage3Dwelling>();
            var slept = dwelling != null && dwelling.TrySkipDay();
            InvalidateBuildingSelection(); RefreshBuildingSelection();
            return slept;
        }
        private void SleepInSelectedDwelling() => TrySleepInSelectedDwelling();
        public Stage2TowerActionResult TryUpgradeSelectedBuilding()
        {
            if (!SelectionAvailable || IsDraggingBuilding || selectionKind != Stage3BuildingSelectionKind.Tower)
                return Stage2TowerActionResult.InvalidTarget;
            var result = game.Building.Defense.TryUpgrade(selectedTower);
            if (result == Stage2TowerActionResult.Ready) PlayTowerUpgradeEffect(selectedTower);
            InvalidateBuildingSelection(); RefreshBuildingSelection();
            return result;
        }
        public ProductionUpgradeResult TryUpgradeSelectedProduction()
        {
            if (!SelectionAvailable || IsDraggingBuilding || selectionKind != Stage3BuildingSelectionKind.Production)
                return ProductionUpgradeResult.InvalidBuilding;
            var result = game.Production.TryUpgrade(selectedProduction);
            if (result == ProductionUpgradeResult.Upgraded)
                presentation.PlayBuildingUpgradeEffect(selectedProduction.Footprint.center,selectedProduction.Footprint.size);
            InvalidateBuildingSelection(); RefreshBuildingSelection(); return result;
        }
        public Stage2ThroneUpgradeResult TryUpgradeSelectedThrone()
        {
            if (!SelectionAvailable || IsDraggingBuilding || selectionKind != Stage3BuildingSelectionKind.Throne)
                return Stage2ThroneUpgradeResult.Inactive;
            var result = game.RunDriver.Run.TryUpgradeThrone();
            if (result == Stage2ThroneUpgradeResult.Upgraded)
                presentation.PlayBuildingUpgradeEffect(scene.ThroneMarker.position,CellFootprintSize());
            InvalidateBuildingSelection(); RefreshBuildingSelection(); return result;
        }
        private Vector2 CellFootprintSize()
        {
            var map=scene.MapDefinition;var origin=map.CellCoordinatesToWorld(Vector2.zero);
            var x=map.CellCoordinatesToWorld(Vector2.right)-origin;var y=map.CellCoordinatesToWorld(Vector2.up)-origin;
            return new Vector2(Mathf.Abs(x.x)+Mathf.Abs(y.x),Mathf.Abs(x.y)+Mathf.Abs(y.y));
        }
        private void PlayTowerUpgradeEffect(Stage2Tower tower) =>
            presentation.PlayBuildingUpgradeEffect(scene.MapDefinition.CellToWorld(tower.Cell),CellFootprintSize());
        public Stage2TowerActionResult TrySellSelectedBuilding()
        {
            if (!SelectionAvailable || IsDraggingBuilding || selectionKind != Stage3BuildingSelectionKind.Tower)
                return Stage2TowerActionResult.InvalidTarget;
            var result = game.Building.Defense.TrySell(selectedTower);
            if (result == Stage2TowerActionResult.Ready) ClearBuildingSelection();
            else { InvalidateBuildingSelection(); RefreshBuildingSelection(); }
            return result;
        }

        private bool WorldBuildingSelectionAvailable => bound && presentation != null && presentation.isActiveAndEnabled &&
            !blinkArmed && !blinkConsumedPointer &&
            (presentation.ActiveWindow == Stage3PlayerWindow.None || presentation.ActiveWindow == Stage3PlayerWindow.Construction) &&
            !Blocked() && !(legacy != null && legacy.DebugOpen);
        private bool HasBuildingDraft => game.Building.Defense.IsPlacing ||
            (game.Production != null && game.Production.IsPlacing) ||
            (GetComponent<Stage3WorldTravel>()?.IsPlacingWorkshop ?? false) ||
            (GetComponent<Stage3WorldTravel>()?.IsPlacingBaseCircle ?? false) ||
            (GetComponent<Stage3Dwelling>()?.IsPlacing ?? false) ||
            (GetComponent<Stage3Forge>()?.IsPlacing ?? false);
        private bool SelectionAvailable => WorldBuildingSelectionAvailable && !HasBuildingDraft;

        private void UpdateBuildingSelection()
        {
            var pointerPressed = bufferedBuildingPress;
            var pointerPressPosition = bufferedBuildingPressPosition;
            bufferedBuildingPress = false;
            if (!WorldBuildingSelectionAvailable) { ClearBuildingSelection(); return; }
            if (HasBuildingDraft && selectionKind != Stage3BuildingSelectionKind.None) ClearBuildingSelection();
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame ||
                mouse != null && mouse.rightButton.wasPressedThisFrame)
            { ClearBuildingSelection(); return; }
            if (selectionKind != Stage3BuildingSelectionKind.None) RefreshBuildingSelection();
            if (mouse == null) { selectionPressArmed = false; return; }
            var pointer = mouse.position.ReadValue();
            if (IsQuickUpgradeMode)
            {
                if (HasBuildingDraft) CancelPlacement();
                selectionPressArmed = false; pressedTower = null;
                if (pointerPressed || mouse.leftButton.wasPressedThisFrame)
                {
                    quickUpgradePress = true;
                    TryQuickUpgradeAtScreen(pointerPressed ? pointerPressPosition : pointer);
                }
                return;
            }
            // Releasing Alt before the mouse cannot turn an upgrade click into selection.
            if (quickUpgradePress)
            {
                if (!mouse.leftButton.isPressed) quickUpgradePress = false;
                return;
            }
            if (Debug.isDebugBuild && (mouse.leftButton.wasPressedThisFrame || mouse.leftButton.wasReleasedThisFrame) &&
                Environment.GetEnvironmentVariable("NOT3A_BUILDING_ACTIONS_CONSTRUCTION_REVIEW") == "1")
                Debug.Log($"S3_CONSTRUCTION_POINTER frame={Time.frameCount} position={pointer} down={mouse.leftButton.wasPressedThisFrame} up={mouse.leftButton.wasReleasedThisFrame} origin={(pointerPressed ? pointerPressPosition : pointer)} armed={selectionPressArmed} drag={IsDraggingBuilding}");
            if (pointerPressed || mouse.leftButton.wasPressedThisFrame)
            {
                var origin = pointerPressed ? pointerPressPosition : pointer;
                var worldPress = !game.ContainsScreenPoint(origin);
                // Cancel before placement drivers; reveal the card on release, so newly
                // appearing actions cannot capture the same mouse-down underneath the pointer.
                var picked = worldPress ? PickBuildingAtScreen(origin) : default;
                if (picked.Kind != Stage3BuildingSelectionKind.None) CancelPlacement();
                // A placement press on empty ground must not select the newly built
                // object on release and cancel a Shift-repeat blueprint.
                selectionPressArmed = worldPress && (picked.Kind != Stage3BuildingSelectionKind.None || !HasBuildingDraft);
                selectionPressPosition = origin;
                pressedTower = picked.Tower;
            }
            if (!IsDraggingBuilding && selectionPressArmed &&
                Vector2.Distance(selectionPressPosition, pointer) > 8f * presentation.UiScale)
            {
                var source = pressedTower;
                selectionPressArmed = false;
                if (source != null && !game.ContainsScreenPoint(pointer)) TryBeginBuildingDrag(source);
            }
            if (IsDraggingBuilding)
            {
                if (!draggedTower.IsAlive || game.Building.Defense.GetTowerCatalog(draggedTower) == null)
                { ClearBuildingSelection(); return; }
                UpdateBuildingDragTarget(pointer);
                if (mouse.leftButton.wasReleasedThisFrame) TryDropBuildingAtScreen(pointer);
                return;
            }
            if (game.ContainsScreenPoint(pointer)) selectionPressArmed = false;
            if (!mouse.leftButton.wasReleasedThisFrame) return;
            var accept = selectionPressArmed;
            selectionPressArmed = false;
            if (accept) TrySelectBuildingAtScreen(pointer);
            pressedTower = null;
        }

        private Stage2Tower FindTowerAtScreen(Vector2 pointer, Stage2Tower ignore = null)
        {
            Stage2Tower found = null; var nearest = float.PositiveInfinity;
            var defense = game.Building.Defense;
            for (var i = 0; i < defense.TowerCount; i++)
            {
                var tower = defense.GetTower(i);
                if (tower == null || !tower.IsAlive || tower == ignore) continue;
                foreach (var visual in tower.GetComponentsInChildren<Renderer>())
                    if (TryHitBuildingVisual(scene.CameraFollow.BoundCamera, visual, pointer, out var distance) && distance < nearest)
                    { nearest = distance; found = tower; }
            }
            return found;
        }

        private void SelectTower(Stage2Tower tower)
        {
            ClearBuildingSelection();
            selectionKind = Stage3BuildingSelectionKind.Tower; selectedTower = tower;
            selectionRenderers = tower.GetComponentsInChildren<Renderer>();
            RefreshBuildingSelection();
        }

        public bool TryBeginBuildingDrag(Stage2Tower tower)
        {
            if (!SelectionAvailable || tower == null || !tower.IsAlive || game.Building.Defense.GetTowerCatalog(tower) == null)
                return false;
            SelectTower(tower); draggedTower = tower;
            game.Gathering.CancelGatherUntilRelease();
            return true;
        }

        // Development capture observes the same target/presentation after Update, without OS input synthesis.
        internal void PreviewBuildingDragAtScreen(Vector2 pointer)
        { UpdateBuildingDragTarget(pointer); UpdateBuildingDragPresentation(); }

        internal void UpdateBuildingDragTarget(Vector2 pointer)
        {
            dragTargetCell = null; dragDropValid = false;
            if (!IsDraggingBuilding || game.ContainsScreenPoint(pointer) ||
                !game.Building.TryScreenToCell(pointer, out var cell)) return;
            // A visible tower silhouette is the drop target even above its ground cell.
            var hitTower = FindTowerAtScreen(pointer, draggedTower);
            if (hitTower != null) cell = hitTower.Cell;
            dragTargetCell = cell;
            dragDropValid = game.Building.Defense.EvaluateMove(draggedTower, cell) == Stage2TowerActionResult.Ready;
        }

        private Stage2Tower TowerAt(Vector2Int cell)
        {
            var defense = game.Building.Defense;
            for (var i = 0; i < defense.TowerCount; i++)
            { var tower = defense.GetTower(i); if (tower != null && tower.IsAlive && tower.Cell == cell) return tower; }
            return null;
        }

        public Stage2TowerActionResult TryDropBuildingAtScreen(Vector2 pointer)
        {
            if (!IsDraggingBuilding) return Stage2TowerActionResult.InvalidTarget;
            UpdateBuildingDragTarget(pointer);
            var source = draggedTower;
            var result = Stage2TowerActionResult.InvalidCell;
            if (SelectionAvailable && dragTargetCell.HasValue)
                result = game.Building.Defense.TryMove(source, dragTargetCell.Value);
            draggedTower = null; dragTargetCell = null; dragDropValid = false;
            selectionPressArmed = false; pressedTower = null;
            ReleaseBuildingDragPresentation();
            if (source != null && source.IsAlive) { InvalidateBuildingSelection(); RefreshBuildingSelection(); }
            else ClearBuildingSelection();
            return result;
        }

        public void ClearBuildingSelection()
        {
            selectionKind = Stage3BuildingSelectionKind.None;
            selectedTower = null; selectedProduction = null; selectionRenderers = null;
            selectedView = null; selectionStamp = int.MinValue; selectionPressArmed = false;
            pressedTower = null; draggedTower = null; dragTargetCell = null; dragDropValid = false;
            bufferedBuildingPress = false;
            quickUpgradePress = false;
            ReleaseBuildingDragPresentation();
            presentation?.ClearSelectedBuilding();
        }

        private readonly struct BuildingHit
        {
            public readonly Stage3BuildingSelectionKind Kind;
            public readonly Stage2Tower Tower;
            public readonly Stage2ProductionBuilding Production;
            public readonly Renderer[] Renderers;
            public BuildingHit(Stage3BuildingSelectionKind kind, Stage2Tower tower,
                Stage2ProductionBuilding production, Renderer[] renderers)
            { Kind = kind; Tower = tower; Production = production; Renderers = renderers; }
        }

        public bool TrySelectBuildingAtScreen(Vector2 pointer)
        {
            if (!WorldBuildingSelectionAvailable || game.ContainsScreenPoint(pointer) ||
                !float.IsFinite(pointer.x) || !float.IsFinite(pointer.y)) return false;
            var hit = PickBuildingAtScreen(pointer);
            if (hit.Kind == Stage3BuildingSelectionKind.None)
            {
                if (!HasBuildingDraft) ClearBuildingSelection();
                return false;
            }
            CancelPlacement();
            ClearBuildingSelection();
            selectionKind = hit.Kind; selectedTower = hit.Tower; selectedProduction = hit.Production;
            selectionRenderers = hit.Renderers;
            RefreshBuildingSelection();
            return selectionKind != Stage3BuildingSelectionKind.None;
        }

        private BuildingHit PickBuildingAtScreen(Vector2 pointer)
        {
            var camera = scene.CameraFollow.BoundCamera;
            var bestDistance = float.PositiveInfinity;
            var kind = Stage3BuildingSelectionKind.None;
            Stage2Tower tower = null;
            Stage2ProductionBuilding production = null;
            Renderer[] renderers = null;
            void Candidate(Stage3BuildingSelectionKind candidateKind, Renderer[] visuals,
                Stage2Tower candidateTower = null, Stage2ProductionBuilding candidateProduction = null)
            {
                if (visuals == null) return;
                var nearest = float.PositiveInfinity;
                foreach (var renderer in visuals)
                    if (TryHitBuildingVisual(camera, renderer, pointer, out var distance))
                        nearest = Mathf.Min(nearest, distance);
                if (nearest >= bestDistance) return;
                bestDistance = nearest; kind = candidateKind; renderers = visuals;
                tower = candidateTower; production = candidateProduction;
            }
            var defense = game.Building.Defense;
            for (var i = 0; i < defense.TowerCount; i++)
            {
                var item = defense.GetTower(i);
                if (item != null && item.IsAlive)
                    Candidate(Stage3BuildingSelectionKind.Tower, item.GetComponentsInChildren<Renderer>(), item);
            }
            var generators = game.Production;
            if (generators != null && generators.Rules != null)
                foreach (var item in generators.Rules.Buildings)
                {
                    var visual = generators.GetBuiltVisual(item);
                    if (visual != null) Candidate(Stage3BuildingSelectionKind.Production,
                        visual.GetComponentsInChildren<Renderer>(), candidateProduction: item);
                }
            var travel = GetComponent<Stage3WorldTravel>();
            if (travel != null && travel.WorkshopBuilt && travel.WorkshopVisual != null)
                Candidate(Stage3BuildingSelectionKind.Workshop, new Renderer[] { travel.WorkshopVisual });
            var dwelling = GetComponent<Stage3Dwelling>();
            var dwellingVisual = GetComponent<Stage3DwellingPresentation>()?.BuiltVisual;
            if (dwelling != null && dwelling.IsBuilt && dwellingVisual != null)
                Candidate(Stage3BuildingSelectionKind.Dwelling, new Renderer[] { dwellingVisual });
            var forge=GetComponent<Stage3Forge>();
            var forgeVisual=GetComponent<Stage3ForgePresentation>()?.BuiltVisual;
            if(forge!=null&&forge.IsBuilt&&forgeVisual!=null)
                Candidate(Stage3BuildingSelectionKind.Forge,new Renderer[]{forgeVisual});
            var dome = scene.ThroneMarker.GetComponentInChildren<Stage3LandmarkSprite>();
            Candidate(Stage3BuildingSelectionKind.Throne, dome != null && dome.Body != null ?
                new Renderer[] { dome.Body } : scene.ThroneMarker.GetComponentsInChildren<Renderer>());
            return new BuildingHit(kind, tower, production, renderers);
        }

        private static bool UsableVisual(Renderer renderer) => renderer != null && renderer.enabled &&
            renderer.gameObject.activeInHierarchy && !(renderer is ParticleSystemRenderer) &&
            !(renderer is LineRenderer) && !(renderer is TrailRenderer);

        private static bool TryHitBuildingVisual(Camera camera, Renderer renderer, Vector2 pointer, out float distance)
        {
            distance = float.PositiveInfinity;
            if (camera == null || !UsableVisual(renderer)) return false;
            var ray = camera.ScreenPointToRay(pointer);
            if (renderer is SpriteRenderer spriteRenderer)
            {
                var sprite = spriteRenderer.sprite;
                if (sprite == null || spriteRenderer.color.a <= .01f ||
                    !new Plane(renderer.transform.forward, renderer.transform.position).Raycast(ray, out distance)) return false;
                var local = renderer.transform.InverseTransformPoint(ray.GetPoint(distance));
                var bounds = sprite.bounds;
                var uv = new Vector2(Mathf.InverseLerp(bounds.min.x, bounds.max.x, local.x),
                    Mathf.InverseLerp(bounds.min.y, bounds.max.y, local.y));
                if (local.x < bounds.min.x || local.x > bounds.max.x || local.y < bounds.min.y || local.y > bounds.max.y) return false;
                if (spriteRenderer.flipX) uv.x = 1f - uv.x;
                if (spriteRenderer.flipY) uv.y = 1f - uv.y;
                if (!sprite.texture.isReadable) return true;
                var rect = sprite.textureRect;
                var x = Mathf.Clamp(Mathf.FloorToInt(rect.x + uv.x * rect.width), 0, sprite.texture.width - 1);
                var y = Mathf.Clamp(Mathf.FloorToInt(rect.y + uv.y * rect.height), 0, sprite.texture.height - 1);
                return sprite.texture.GetPixel(x, y).a > .15f;
            }
            return renderer.bounds.IntersectRay(ray, out distance);
        }

        private void RefreshBuildingSelection()
        {
            var travel = GetComponent<Stage3WorldTravel>();
            var dwelling = GetComponent<Stage3Dwelling>();
            var forge=GetComponent<Stage3Forge>();
            var valid = selectionKind switch
            {
                Stage3BuildingSelectionKind.Tower => selectedTower != null && selectedTower.IsAlive &&
                    game.Building.Defense.GetTowerCatalog(selectedTower) != null,
                Stage3BuildingSelectionKind.Production => game.Production != null &&
                    game.Production.GetBuiltVisual(selectedProduction) != null,
                Stage3BuildingSelectionKind.Workshop => travel != null && travel.WorkshopBuilt,
                Stage3BuildingSelectionKind.Dwelling => dwelling != null && dwelling.IsBuilt,
                Stage3BuildingSelectionKind.Forge => forge!=null&&forge.IsBuilt,
                Stage3BuildingSelectionKind.Throne => game.RunDriver.Run.ThroneHealth > 0,
                _ => false
            };
            if (!valid) { ClearBuildingSelection(); return; }
            var stamp = selectionKind == Stage3BuildingSelectionKind.Tower ? HashCode.Combine(selectedTower.Health, selectedTower.Level,
                game.Building.Defense.EvaluateUpgrade(selectedTower), game.Building.Defense.EvaluateSell(selectedTower)) :
                selectionKind == Stage3BuildingSelectionKind.Throne ? HashCode.Combine(game.RunDriver.Run.ThroneHealth,
                    game.RunDriver.Run.ThroneMaximum, game.RunDriver.Run.ThroneUpgradeCount, game.RunDriver.Run.EvaluateThroneUpgrade()) :
                selectionKind == Stage3BuildingSelectionKind.Production ? HashCode.Combine(selectedProduction.Level,
                    game.Production.EvaluateUpgrade(selectedProduction)) :
                selectionKind == Stage3BuildingSelectionKind.Workshop ? (travel.IsResearched ? 1 : 0) :
                selectionKind == Stage3BuildingSelectionKind.Forge ? HashCode.Combine(forge.EquipmentDriver.Rules.ForgeLevel,forge.CanUse,forge.CanUpgradeBuilding,forge.UpgradeUnavailableReason) :
                selectionKind == Stage3BuildingSelectionKind.Dwelling ? HashCode.Combine(dwelling.CanSkipDay, dwelling.NearEntrance,
                    game.RunDriver.Run.UsesDaytimeGates ? Stage3DwellingPresentation.GetSleepUnavailableReason(dwelling, game) : null) : 0;
            if (selectedView == null || stamp != selectionStamp)
            {
                selectedView = BuildSelectionView(travel, dwelling);
                selectionStamp = stamp;
                presentation.SetSelectedBuilding(selectedView);
            }
            var found = false; var combined = new Bounds();
            if (selectionRenderers != null)
                foreach (var renderer in selectionRenderers)
                    if (UsableVisual(renderer))
                    { if (found) combined.Encapsulate(renderer.bounds); else combined = renderer.bounds; found = true; }
            if (found) selectedView.WorldBounds = combined;
        }

        private Stage3SelectedBuildingView BuildSelectionView(Stage3WorldTravel travel, Stage3Dwelling dwelling)
        {
            var result = new Stage3SelectedBuildingView {
                Subtitle = "Базовая постройка", ShowUpgrade = true,
                UpgradeUnavailableReason = "Улучшения пока не открыты",
                SellUnavailableReason = "Продажа пока не открыта",
                Hint = "ЛКМ по свободному миру или Esc · снять выбор"
            };
            Stage3SelectedBuildingStat Stat(string label, object value) => new Stage3SelectedBuildingStat(label,
                value is float number ? number.ToString("0.##", DisplayCulture) : value.ToString());
            switch (selectionKind)
            {
                case Stage3BuildingSelectionKind.Tower:
                    var catalog = game.Building.Defense.GetTowerCatalog(selectedTower);
                    result.Title = catalog.RoleName; result.ShowSell = true;
                    result.Subtitle = catalog.MaximumLevel > 1 ? "Уровень " + selectedTower.Level +
                        (!catalog.UnlimitedUpgrades ? " / " + catalog.MaximumLevel : "") : "Преграда";
                    result.ShowUpgrade = selectedTower.CanAttack;
                    result.UpgradeAvailable = game.Building.Defense.EvaluateUpgrade(selectedTower) == Stage2TowerActionResult.Ready;
                    result.SellAvailable = game.Building.Defense.EvaluateSell(selectedTower) == Stage2TowerActionResult.Ready;
                    result.SellRefund = FormatCost(game.Building.Defense.GetSaleRefund(selectedTower));
                    result.SellUnavailableReason = "Недостаточно места в запасах";
                    if (catalog.TryGetUpgrade(selectedTower.Level, out var upgrade))
                    {
                        result.UpgradePrice = FormatCost(upgrade.Cost);
                        result.UpgradeUnavailableReason = "Недостаточно ресурсов";
                        result.UpgradePreview = selectedTower.CanAttack
                            ? $"След. уровень: урон {selectedTower.AttackDamage} → {upgrade.Damage} · HP {selectedTower.MaximumHealth} → {upgrade.Health}"
                            : $"След. уровень: HP {selectedTower.MaximumHealth} → {upgrade.Health}";
                    }
                    else result.UpgradeUnavailableReason = "Максимальный уровень";
                    result.Hint = !selectedTower.CanAttack
                        ? "Частокол не улучшается. Перемещение — днём."
                        : selectedTower.Level >= catalog.MaximumLevel
                            ? "Максимальный уровень. Перемещение — днём."
                            : "Платное улучшение — кнопкой в карточке. Перемещение — днём.";
                    result.WorldPosition = selectedTower.transform.position;
                    var towerArt = selectedTower.GetComponentInChildren<Stage3TowerSprite>();
                    var icon = towerArt != null ? towerArt.Body : selectedTower.GetComponentInChildren<SpriteRenderer>();
                    result.Icon = icon != null ? icon.sprite : null;
                    var defense = game.Building.Defense;
                    for (var i = 0; i < defense.RoleCount; i++)
                        if (defense.GetCatalog(i) == catalog && presentation.Art.defenseIcons != null &&
                            i < presentation.Art.defenseIcons.Length && presentation.Art.defenseIcons[i] != null)
                            result.Icon = presentation.Art.defenseIcons[i];
                    result.Stats = selectedTower.CanAttack ? new[] {
                        Stat("Урон", selectedTower.AttackDamage), Stat("Дальность", selectedTower.AttackRange),
                        Stat("Интервал", selectedTower.FireInterval.ToString("0.##", DisplayCulture) + " с"),
                        Stat("Здоровье", selectedTower.Health + "/" + selectedTower.MaximumHealth) } :
                        new[] { Stat("Здоровье", selectedTower.Health + "/" + selectedTower.MaximumHealth), Stat("Назначение", "Преграда") };
                    break;
                case Stage3BuildingSelectionKind.Production:
                    var entry = selectedProduction.Entry;
                    result.Title = entry.DisplayName; result.Icon = entry.Sprite;
                    result.Subtitle = "Уровень " + selectedProduction.Level + " / " + entry.MaximumLevel;
                    var upgradeResult = game.Production.EvaluateUpgrade(selectedProduction);
                    result.UpgradeAvailable = upgradeResult == ProductionUpgradeResult.Ready;
                    result.UpgradeUnavailableReason = upgradeResult == ProductionUpgradeResult.MaximumLevel ? "Максимальный уровень" :
                        upgradeResult == ProductionUpgradeResult.InsufficientResources ? "Недостаточно ресурсов" : "Сейчас недоступно";
                    if (entry.TryGetUpgrade(selectedProduction.Level, out var productionUpgrade))
                    {
                        result.UpgradePrice = FormatCost(productionUpgrade.cost);
                        result.UpgradePreview = $"След. уровень: +{selectedProduction.OutputAmount} → +{productionUpgrade.outputAmount} за цикл · интервал {entry.IntervalSeconds:0.##} с";
                    }
                    else result.Hint = "Максимальный уровень";
                    result.WorldPosition = selectedProduction.Footprint.center;
                    result.Stats = new[] { Stat("Ресурс", entry.Resource == Stage2.Map.MapResourceKind.Wood ? "Дерево" :
                        entry.Resource == Stage2.Map.MapResourceKind.Stone ? "Камень" : "Металл"),
                        Stat("За цикл", "+" + selectedProduction.OutputAmount), Stat("Интервал", selectedProduction.IntervalSeconds.ToString("0.##", DisplayCulture) + " с"),
                        Stat("Состояние", "Добывает") };
                    break;
                case Stage3BuildingSelectionKind.Workshop:
                    result.Title = "Исследовательский центр"; result.Icon = presentation.Art.workshopIcon;
                    result.Subtitle="Изучение чертежей";result.ShowUpgrade=false;
                    result.WorldPosition = travel.WorkshopPosition; result.ShowResearch = true;
                    result.Stats = new[] { Stat("Изучено чертежей",equipmentDriver?.Rules.Blueprints?.StudiedCount??0),
                        Stat("Телепортация", travel.IsResearched ? "Изучена" : "Не изучена") };
                    result.Hint = "Изучение чертежей у исследовательского центра · E или «Исследования»";
                    break;
                case Stage3BuildingSelectionKind.Dwelling:
                    result.Title = "Жилище"; result.Icon = dwelling.Catalog.sprite; result.ShowUpgrade = false;
                    result.WorldPosition = dwelling.Position;
                    if (game.RunDriver.Run.UsesDaytimeGates)
                    {
                        result.ShowSleep = true; result.SleepAvailable = dwelling.CanSkipDay;
                        result.SleepUnavailableReason = Stage3DwellingPresentation.GetSleepUnavailableReason(dwelling, game);
                        result.Stats = new[] { Stat("Пробуждение", "До начала ночи") };
                        result.Hint = "Сон доступен у входа · только вне боя";
                    }
                    else
                    {
                        result.Stats = new[] { Stat("У входа", dwelling.CanSkipDay ? "E · дождаться ночи" : "Отдых недоступен") };
                        result.Hint = "Назначение улучшений жилища ещё не выбрано";
                    }
                    break;
                case Stage3BuildingSelectionKind.Forge:
                    var forge=GetComponent<Stage3Forge>();var rules=forge.EquipmentDriver.Rules;
                    result.Title=forge.DisplayName;result.Icon=forge.Art?.worldSprite;result.WorldPosition=forge.Position;
                    result.Subtitle="Ступень "+rules.ForgeLevel+" / "+rules.Catalog.ForgeStageCount;
                    result.ShowForge=true;result.ForgeAvailable=forge.CanUse;
                    result.ForgeUnavailableReason=forge.NearEntrance?"Кузница сейчас недоступна":"Подойдите ко входу в кузницу";
                    result.UpgradeAvailable=forge.CanUpgradeBuilding;result.UpgradeUnavailableReason=forge.UpgradeUnavailableReason;
                    result.UpgradePriceLabel=rules.Blueprints==null?null:"Условие";
                    result.UpgradePrice=rules.ForgeLevel<rules.Catalog.ForgeStageCount?(rules.Blueprints==null?"1 свиток":
                        rules.EvaluateForgeUpgrade()==Stage2EquipmentResult.Ready?"Допуск изучен":"Изучите чертёж"):null;
                    result.Stats=rules.Blueprints==null?new[]{Stat("Предел качества",rules.MaximumQuality),Stat("Свитки",rules.ForgeUpgradeScrolls)}:
                        new[]{Stat("Предел качества",rules.MaximumQuality),Stat("Улучшение",rules.ForgeLevel>=rules.Catalog.ForgeStageCount?"Максимум":
                        rules.EvaluateForgeUpgrade()==Stage2EquipmentResult.Ready?"Допуск изучен":"Нужен чертёж")};
                    result.Hint="Изготовление и улучшение предметов · E у входа";
                    break;
                default:
                    var throneRun = game.RunDriver.Run;
                    result.Title = "Трон"; result.Subtitle = "Усиления " + throneRun.ThroneUpgradeCount + " / " + throneRun.ThroneUpgradeLimit;
                    result.WorldPosition = scene.ThroneMarker.position;
                    var dome = scene.ThroneMarker.GetComponentInChildren<Stage3LandmarkSprite>();
                    result.Icon = dome != null && dome.Body != null ? dome.Body.sprite : null;
                    result.Stats = new[] { Stat("Здоровье", throneRun.ThroneHealth + "/" + throneRun.ThroneMaximum) };
                    var throneUpgrade = throneRun.EvaluateThroneUpgrade();
                    result.UpgradeAvailable = throneUpgrade == Stage2ThroneUpgradeResult.Ready;
                    result.UpgradeUnavailableReason = throneUpgrade == Stage2ThroneUpgradeResult.MaximumUpgrades ? "Максимальное усиление" :
                        throneUpgrade == Stage2ThroneUpgradeResult.InsufficientResources ? "Недостаточно ресурсов" : "Сейчас недоступно";
                    if (throneRun.TryGetThroneUpgradeCost(out var thronePrice))
                    {
                        result.UpgradePrice = FormatCost(thronePrice);
                        var delta = throneRun.ThroneHealthPerUpgrade;
                        result.UpgradePreview = $"HP {throneRun.ThroneHealth}/{throneRun.ThroneMaximum} → {throneRun.ThroneHealth + delta}/{throneRun.ThroneMaximum + delta} · +{delta} к текущему и максимуму";
                    }
                    else result.Hint = "Максимальное усиление";
                    break;
            }
            return result;
        }

        private static string FormatCost(Stage2ResourceCost cost)
        {
            var parts = new System.Collections.Generic.List<string>(4);
            if (cost.Wood > 0) parts.Add(cost.Wood + " дер.");
            if (cost.Stone > 0) parts.Add(cost.Stone + " кам.");
            if (cost.Metal > 0) parts.Add(cost.Metal + " мет.");
            if (cost.Gold > 0) parts.Add(cost.Gold + " зол.");
            if (parts.Count == 0) return "0";
            if (parts.Count <= 2) return string.Join(" · ", parts);
            return string.Join(" · ", parts.GetRange(0, 2)) + "\n" + string.Join(" · ", parts.GetRange(2, parts.Count - 2));
        }
    }
}
