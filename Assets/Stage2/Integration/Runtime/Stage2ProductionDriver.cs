using System;
using System.Collections.Generic;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Integration
{
    [DisallowMultipleComponent, DefaultExecutionOrder(-90)]
    public sealed class Stage2ProductionDriver : MonoBehaviour
    {
        private Stage2GatheringDriver gathering;
        private Stage2BuildDriver towers;
        private Stage2Run run;
        private Stage2PlayerMover hero;
        private Camera view;
        private Stage2MapDefinition map;
        private Stage2GameComposition composition;
        private HeroObstacleMap obstacles;
        private Func<Vector2, bool> pointerBlocker;
        private readonly List<GameObject> visuals = new List<GameObject>();
        private SpriteRenderer preview;
        private Stage2PlacementMark previewOutline;
        private readonly List<Stage2PlacementMark> padMarkers = new List<Stage2PlacementMark>();
        private Material groundMaterial;
        private Material previewMaterial;
        private const int MaskSize = 96;
        private readonly Dictionary<Sprite, bool[]> masks = new Dictionary<Sprite, bool[]>();
        private InputAction confirm, cancel;
        private Stage2Production.Snapshot checkpoint;
        public Stage2ProductionCatalog Catalog { get; private set; }
        public Stage2Production Rules { get; private set; }
        public bool IsPlacing { get; private set; }
        public ResourceProductionBuilding SelectedBuilding { get; private set; }
        public ProductionPlacementResult PreviewResult { get; private set; } = ProductionPlacementResult.NotPlacing;
        public int PreviewPointIndex { get; private set; } = -1;
        public bool PointerOverUi { get; private set; }
        public event Action PlacementStarting;
        public ProductionUpgradeResult EvaluateUpgrade(Stage2ProductionBuilding building) =>
            Active ? Rules.EvaluateUpgrade(building) : ProductionUpgradeResult.Inactive;
        public ProductionUpgradeResult TryUpgrade(Stage2ProductionBuilding building)
        {
            SyncActivity();
            return Rules.TryUpgrade(building);
        }

        public GameObject GetBuiltVisual(Stage2ProductionBuilding building)
        {
            if (Rules == null || building == null) return null;
            for (var i = 0; i < Rules.Buildings.Count; i++)
                if (ReferenceEquals(Rules.Buildings[i], building)) return i < visuals.Count ? visuals[i] : null;
            return null;
        }

        public void Configure(Stage2ProductionCatalog catalog, Stage2SceneBootstrap scene, Stage2GameComposition game)
        {
            composition = game;
            Catalog = catalog; map = scene.MapDefinition; hero = scene.PlayerMover; view = scene.CameraFollow.BoundCamera;
            gathering = game.Gathering; towers = game.Building; run = game.RunDriver.Run; pointerBlocker = game.ContainsScreenPoint;
            Rules = new Stage2Production(catalog, gathering.Economy.Inventory);
            foreach (var entry in catalog.Entries)
            {
                var sprite = entry.Sprite; var texture = sprite.texture; var rect = sprite.rect;
                var pixels = texture.GetPixels32(); var mask = new bool[MaskSize * MaskSize];
                for (var y = 0; y < MaskSize; y++) for (var x = 0; x < MaskSize; x++)
                {
                    var px = Mathf.Min(texture.width - 1, (int)(rect.x + (x + .5f) * rect.width / MaskSize));
                    var py = Mathf.Min(texture.height - 1, (int)(rect.y + (y + .5f) * rect.height / MaskSize));
                    mask[y * MaskSize + x] = pixels[py * texture.width + px].a > 40;
                }
                masks[sprite] = mask;
            }
            towers.PlacementStarting += CancelPlacement;
            run.Reset += ResetRun;
            run.BeforeWaveStarted += CaptureCheckpoint;
            run.Rewound += RestoreCheckpoint;
            run.Changed += SyncActivity;
            SyncActivity();
        }
        public void BindWorld(IReadOnlyList<ResourceProductionPoint> points, HeroObstacleMap groundObstacles)
        {
            ResetRun(); ClearMarkers(); obstacles = groundObstacles; Rules.BindPoints(points);
            if (groundMaterial == null) groundMaterial = Stage2PlacementVisuals.Material(false);
            foreach (var point in points) padMarkers.Add(Stage2PlacementVisuals.Pad(transform, point, groundMaterial));
            SyncActivity();
        }
        public bool CanBeginPlacement(ResourceProductionBuilding building)
        {
            if (Rules == null || !Active) return false;
            if (!Rules.IsBuildingAvailable(building)) return false;
            var entry = Catalog.Get(building);
            return entry.IsValid && gathering.Economy.Inventory.CanAfford(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost);
        }
        public void SelectAndBeginPlacement(ResourceProductionBuilding building)
        {
            if (!CanBeginPlacement(building)) return;
            PlacementStarting?.Invoke();
            towers.CancelPlacement(); CancelPlacement(); SelectedBuilding = building; IsPlacing = true;
            gathering.SetProductionBuildMode(true);
            if (previewMaterial == null) previewMaterial = Stage2PlacementVisuals.Material(true);
            if (groundMaterial == null) groundMaterial = Stage2PlacementVisuals.Material(false);
            var entry = Catalog.Get(building);
            var point = new ResourceProductionPoint(0, entry.Resource, Vector2.zero, entry.FootprintSize, 0);
            var instance = CreateVisual(entry, point, "Production placement preview", previewMaterial);
            preview = instance.GetComponent<SpriteRenderer>(); preview.color = Stage2PlacementVisuals.Invalid;
            previewOutline = Stage2PlacementVisuals.Outline(transform, entry.FootprintSize, groundMaterial, "Production placement footprint");
            previewOutline.gameObject.SetActive(false);
        }
        public void CancelPlacement()
        {
            IsPlacing = false; PreviewPointIndex = -1; PreviewResult = ProductionPlacementResult.NotPlacing;
            if (preview != null) { preview.gameObject.SetActive(false); Destroy(preview.gameObject); preview = null; }
            if (previewOutline != null) { previewOutline.gameObject.SetActive(false); Destroy(previewOutline.gameObject); previewOutline = null; }
            gathering?.SetProductionBuildMode(false);
            SyncMarkers();
        }
        public ProductionPlacementResult TryBuildAt(int pointIndex)
        {
            if (!IsPlacing || Rules == null) return ProductionPlacementResult.NotPlacing;
            SyncActivity();
            var result = Evaluate(pointIndex);
            if (result != ProductionPlacementResult.Ready) return PreviewResult = result;
            GameObject candidate = null;
            try
            {
                result = Rules.TryBuild(pointIndex, SelectedBuilding, hero.transform.position, hero.CollisionRadius, () =>
                {
                    candidate = CreateVisual(Catalog.Get(SelectedBuilding), Rules.Points[pointIndex], "Production " + SelectedBuilding, Catalog.SpriteMaterial);
                    return candidate != null && !candidate.activeSelf && candidate.GetComponent<SpriteRenderer>().sprite != null;
                }, out var building);
                if (result == ProductionPlacementResult.Built)
                {
                    obstacles.AddRectangle(building.Footprint); visuals.Add(candidate); candidate.SetActive(true); candidate = null;
                    padMarkers[pointIndex].gameObject.SetActive(false);
                    CancelPlacement();
                }
                return PreviewResult = result;
            }
            finally { if (candidate != null) Destroy(candidate); }
        }
        private ProductionPlacementResult Evaluate(int pointIndex)
        {
            var result = Rules.Evaluate(pointIndex, SelectedBuilding, hero.transform.position, hero.CollisionRadius);
            if (result != ProductionPlacementResult.Ready) return result;
            if (obstacles == null) return ProductionPlacementResult.InvalidFootprint;
            var rect = Rules.Footprint(pointIndex, Catalog.Get(SelectedBuilding));
            // Landmarks reserve green ground away from the invasion corridor; never alter navigation.
            for (var x = Mathf.FloorToInt(rect.xMin); x <= Mathf.CeilToInt(rect.xMax); x++)
            for (var y = Mathf.FloorToInt(rect.yMin); y <= Mathf.CeilToInt(rect.yMax); y++)
            {
                var cell = map.WorldToCell(new Vector2(x, y));
                if (!map.CellBounds.Contains(cell) || map.ReadCell(cell).Surface != MapSurfaceKind.Forest)
                    return ProductionPlacementResult.InvalidFootprint;
            }
            return result;
        }
        private GameObject CreateVisual(Stage2ProductionEntry entry, ResourceProductionPoint point, string name, Material material)
        {
            if (entry.Sprite == null || material == null) return null;
            var instance = new GameObject(name); instance.SetActive(false); instance.transform.SetParent(transform, false);
            var renderer = instance.AddComponent<SpriteRenderer>(); renderer.sprite = entry.Sprite; renderer.sharedMaterial = material;
            PositionVisual(instance.transform, entry, point.Position);
            // The pad artwork is a coherent image; constructed objects always composite over it.
            renderer.sortingOrder = 30000;
            renderer.color = Color.white;
            return instance;
        }
        private void PositionVisual(Transform target, Stage2ProductionEntry entry, Vector2 position)
        {
            var scale = entry.VisualWidth / entry.Sprite.bounds.size.x;
            var belowPivot = entry.Sprite.pivot.y / entry.Sprite.pixelsPerUnit * scale;
            var clearance = belowPivot * Mathf.Abs(view.transform.up.z) / Mathf.Max(.1f, Mathf.Abs(view.transform.forward.z)) + .10f;
            target.localScale = Vector3.one * scale;
            target.SetPositionAndRotation(new Vector3(position.x, position.y, -.22f) - view.transform.forward * clearance, view.transform.rotation);
        }
        public void ResetRun()
        {
            CancelPlacement();
            if (Rules != null && obstacles != null)
                foreach (var building in Rules.Buildings) obstacles.RemoveRectangle(building.Footprint);
            foreach (var instance in visuals) if (instance != null) { instance.SetActive(false); Destroy(instance); }
            visuals.Clear(); Rules?.Reset();
            foreach (var mark in padMarkers) if (mark != null) mark.gameObject.SetActive(true);
            checkpoint = null;
        }
        private void CaptureCheckpoint() => checkpoint = Rules?.CaptureSnapshot();
        private void RestoreCheckpoint()
        {
            if (run.PreserveProgressOnRetry) { CancelPlacement(); SyncActivity(); return; }
            var saved = checkpoint;
            ResetRun();
            if (saved == null) return;
            Rules.RestoreSnapshot(saved);
            foreach (var building in Rules.Buildings)
            {
                var instance = CreateVisual(building.Entry, Rules.Points[building.PointIndex],
                    "Production " + building.Entry.Building, Catalog.SpriteMaterial);
                obstacles.AddRectangle(building.Footprint); visuals.Add(instance); instance.SetActive(true);
            }
            SyncMarkers();
        }
        private bool Active => run != null && !run.IsPaused && !run.IsTerminal &&
            (composition == null || !composition.PlayerInputBlocked);
        private void SyncActivity()
        {
            Rules?.SetActive(Active);
            if (!Active) CancelPlacement();
        }
        private void OnEnable()
        {
            confirm = new InputAction("Confirm generator", InputActionType.Button, "<Mouse>/leftButton");
            cancel = new InputAction("Cancel generator", InputActionType.Button, "<Keyboard>/escape");
            cancel.AddBinding("<Mouse>/rightButton"); confirm.Enable(); cancel.Enable();
        }
        private void OnDisable() { confirm?.Dispose(); cancel?.Dispose(); confirm = cancel = null; CancelPlacement(); }
        private void OnDestroy()
        {
            if (towers != null) towers.PlacementStarting -= CancelPlacement;
            if (run != null)
            {
                run.Reset -= ResetRun; run.Changed -= SyncActivity;
                run.BeforeWaveStarted -= CaptureCheckpoint; run.Rewound -= RestoreCheckpoint;
            }
            ResetRun(); ClearMarkers();
            if (previewMaterial != null) Destroy(previewMaterial);
            if (groundMaterial != null) Destroy(groundMaterial);
        }
        private void Update()
        {
            if (Rules == null) return;
            SyncActivity();
            if (!IsPlacing) return;
            if (cancel != null && cancel.WasPressedThisFrame()) { CancelPlacement(); return; }
            PointerOverUi = Mouse.current != null && pointerBlocker != null && pointerBlocker(Mouse.current.position.ReadValue());
            PreviewPointIndex = -1;
            if (!PointerOverUi && Mouse.current != null)
            {
                var mouse = Mouse.current.position.ReadValue(); var best = 70f * 70f;
                for (var i = 0; i < Rules.Points.Count; i++)
                {
                    var point = view.WorldToScreenPoint(Rules.Points[i].Position);
                    var distance = ((Vector2)point - mouse).sqrMagnitude;
                    if (point.z <= 0 || distance > best) continue;
                    best = distance; PreviewPointIndex = i;
                }
            }
            PreviewResult = PreviewPointIndex < 0 ? ProductionPlacementResult.InvalidPoint : Evaluate(PreviewPointIndex);
            SyncMarkers();
            if (preview != null)
            {
                var ray = Mouse.current == null ? default : view.ScreenPointToRay(Mouse.current.position.ReadValue());
                var visible = !PointerOverUi && Mouse.current != null && new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out _);
                preview.gameObject.SetActive(visible); previewOutline.gameObject.SetActive(visible);
                if (visible)
                {
                    new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance);
                    var position = PreviewPointIndex >= 0 ? Rules.Points[PreviewPointIndex].Position : (Vector2)ray.GetPoint(distance);
                    var entry = Catalog.Get(SelectedBuilding);
                    PositionVisual(preview.transform, entry, position);
                    var color = PreviewResult == ProductionPlacementResult.Ready ? Stage2PlacementVisuals.Valid : Stage2PlacementVisuals.Invalid;
                    preview.color = color; previewOutline.SetTint(new Color(color.r,color.g,color.b,.9f));
                    previewOutline.transform.position = new Vector3(position.x,position.y,-.27f);
                    var size = PreviewPointIndex >= 0 ? Rules.Points[PreviewPointIndex].Clearance.size : entry.FootprintSize;
                    previewOutline.transform.localScale = new Vector3(size.x/entry.FootprintSize.x,size.y/entry.FootprintSize.y,1);
                }
            }
            if (!PointerOverUi && confirm != null && confirm.WasPressedThisFrame()) TryBuildAt(PreviewPointIndex);
        }
        private void SyncMarkers()
        {
            for (var i = 0; i < padMarkers.Count; i++)
            {
                if (padMarkers[i] == null) continue;
                var underPreview = IsPlacing && !PointerOverUi && i == PreviewPointIndex && Rules.Points[i].Accepts(SelectedBuilding);
                padMarkers[i].gameObject.SetActive(!Rules.IsOccupied(i) && !underPreview);
            }
        }
        private void ClearMarkers()
        {
            foreach (var mark in padMarkers) if (mark != null) { mark.gameObject.SetActive(false); Destroy(mark.gameObject); }
            padMarkers.Clear();
        }
        private void LateUpdate()
        {
            if (Rules == null) return;
            SyncActivity(); Rules.Tick(Time.deltaTime);
            foreach (var instance in visuals)
            {
                var renderer = instance.GetComponent<SpriteRenderer>();
                var sprite = renderer.sprite;
                var covered = false;
                for (var sample = 0; sample < 3; sample++)
                {
                    var focus = hero.transform.position + Vector3.back * (.2f + sample * .25f);
                    var local = renderer.transform.InverseTransformPoint(focus);
                    // Positive local depth is behind the billboard, so a hero in front
                    // never fades a building merely because their screen rectangles meet.
                    if (local.z <= 0) continue;
                    var pixel = new Vector2(local.x, local.y) * sprite.pixelsPerUnit + sprite.pivot;
                    var rect = sprite.rect;
                    if (pixel.x < 0 || pixel.y < 0 || pixel.x >= rect.width || pixel.y >= rect.height) continue;
                    var x = Mathf.Clamp((int)(pixel.x / rect.width * MaskSize), 0, MaskSize - 1);
                    var y = Mathf.Clamp((int)(pixel.y / rect.height * MaskSize), 0, MaskSize - 1);
                    if (masks[sprite][y * MaskSize + x]) covered = true;
                }
                var color = renderer.color;
                color.a = Mathf.MoveTowards(color.a, covered ? .42f : 1, Time.unscaledDeltaTime * 4);
                renderer.color = color;
            }
        }
    }
}
