using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Two scene-owned ground markings and one contextual research interaction.</summary>
    [DisallowMultipleComponent]
    public sealed partial class Stage3WorldTravel : MonoBehaviour
    {
        private Stage3WorldTravelData data;
        private Stage2SceneBootstrap scene;
        private Stage2GameComposition game;
        private Stage2GameHud hud;
        private Stage2Minimap minimap;
        private HeroObstacleMap obstacles;
        private Stage3ResourceDistrictWorld resourceWorld;
        private Rect workshopObstacle;
        private GameObject workshop, baseCircle, worldCircle;
        private Mesh baseMesh, worldMesh;
        private InputAction interact;
        private bool requiresRelease = true, ready;
        private readonly Stage3TravelState state = new Stage3TravelState();

        public bool IsReady => ready;
        public bool IsResearched => state.Researched;
        public bool WorkshopBuilt => state.WorkshopBuilt;
        public bool RemoteDiscovered => state.RemoteDiscovered;
        public bool BaseVisible => baseCircle != null && baseCircle.activeSelf;
        public Vector2 WorkshopPosition { get; private set; }
        public Vector2 BasePosition { get; private set; }
        public Vector2 RemotePosition { get; private set; }
        public Vector2 WorkshopInteractionPosition => WorkshopPosition + Vector2.down *
            (data.workshopFootprint.y * .5f + scene.PlayerMover.CollisionRadius + .25f);
        public Vector2[] SitePositions => ready ? new[] { WorkshopPosition, BasePosition, RemotePosition } : Array.Empty<Vector2>();

        public void Initialize(Stage3WorldTravelData settings, Stage2SceneBootstrap bootstrap,
            Stage2GameComposition composition, Stage2Minimap mapView)
        {
            if (settings == null || settings.sealMaterial == null || settings.workshopSprite == null || settings.workshopMaterial == null)
                throw new ArgumentException("World travel artwork is incomplete.");
            data = settings; scene = bootstrap; game = composition; minimap = mapView;
            hud = game.GetComponent<Stage2GameHud>();
            hud?.BindWorkshopConstruction(data.workshopSprite, () => WorkshopBuildStatus, BeginWorkshopPlacement, CancelWorkshopPlacement);
            game.Building.PlacementStarting += CancelWorkshopPlacement;
            if (game.Production != null) game.Production.PlacementStarting += CancelWorkshopPlacement;
            game.RunDriver.Run.Reset += ResetProgress;
        }

        public void BindWorld(Stage3ResourceDistrictWorld world)
        {
            if (data == null) return;
            ResetProgress();
            resourceWorld = world;
            obstacles = world == null ? new HeroObstacleMap() : world.Obstacles;
            var chosen = new List<Vector2>();
            WorkshopPosition = FindSite((Vector2)scene.ThroneMarker.position + data.workshopOffsetFromThrone, world, chosen);
            chosen.Add(WorkshopPosition);
            BasePosition = FindSite((Vector2)scene.ThroneMarker.position + data.baseOffsetFromThrone, world, chosen);
            chosen.Add(BasePosition);
            RemotePosition = FindSite(data.worldCirclePosition, world, chosen);
            if (world == null) scene.PlayerMover.BindObstacles(obstacles, scene.PlayerMover.CollisionRadius);
            RebuildCircle(ref baseCircle, ref baseMesh, "Base teleport circle", BasePosition);
            RebuildCircle(ref worldCircle, ref worldMesh, "World teleport circle", RemotePosition);
            ready = true;
            ResetProgress();
            Debug.Log($"S3_WORLD_TRAVEL_READY workshopSite={WorkshopPosition} base={BasePosition} world={RemotePosition} workshopBuilt=false researchRequired=true");
        }

        private Vector2 FindSite(Vector2 preferred, Stage3ResourceDistrictWorld world, List<Vector2> chosen)
        {
            for (var radius = 0; radius <= 30; radius++)
            for (var y = -radius; y <= radius; y++)
            for (var x = -radius; x <= radius; x++)
            {
                if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radius) continue;
                var point = preferred + new Vector2(x, y);
                var valid = true;
                foreach (var other in chosen)
                    if (Vector2.Distance(point, other) < data.siteClearance * 2f) { valid = false; break; }
                if (!valid) continue;
                var range = Mathf.CeilToInt(data.siteClearance);
                for (var dy = -range; dy <= range && valid; dy++)
                for (var dx = -range; dx <= range; dx++)
                {
                    var p = point + new Vector2(dx, dy);
                    var cell = scene.MapDefinition.WorldToCell(p);
                    if (!scene.MapDefinition.Contains(cell) || !scene.MapDefinition.IsWalkable(cell) ||
                        scene.MapDefinition.GetSurface(cell) != MapSurfaceKind.Forest || obstacles.IsBlocked(p, scene.PlayerMover.CollisionRadius))
                    { valid = false; break; }
                }
                if (!valid) continue;
                if (world != null)
                    foreach (var district in world.Districts)
                    {
                        var bounds = district.Bounds;
                        var expanded = new Rect(bounds.xMin - data.siteClearance - 2f, bounds.yMin - data.siteClearance - 2f,
                            bounds.width + data.siteClearance * 2f + 4f, bounds.height + data.siteClearance * 2f + 4f);
                        if (expanded.Contains(point)) { valid = false; break; }
                    }
                if (!valid || Vector2.Distance(point, scene.ThroneMarker.position) < 4f ||
                    Vector2.Distance(point, scene.EntranceMarker.position) < 4f) continue;
                for (var i = 0; i < 3; i++)
                    if (Vector2.Distance(point, game.Gathering.Economy.WorldPosition((MapResourceKind)i)) < data.siteClearance + 2f)
                    { valid = false; break; }
                if (valid) return point;
            }
            throw new InvalidOperationException("No safe green site for world travel near " + preferred);
        }

        private void CreateWorkshop()
        {
            workshop = new GameObject("Research workshop"); workshop.transform.SetParent(transform, false);
            var renderer = workshop.AddComponent<SpriteRenderer>();
            renderer.sprite = data.workshopSprite; renderer.sharedMaterial = data.workshopMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private void PlaceWorkshop()
            => PositionWorkshop(workshop.transform, WorkshopPosition);

        private void PositionWorkshop(Transform target, Vector2 position)
        {
            var camera = scene.CameraFollow.BoundCamera;
            var sprite = data.workshopSprite;
            var foot = position + Vector2.down * (data.workshopFootprint.y * .5f);
            var below = sprite.pivot.y / sprite.pixelsPerUnit;
            var clearance = below * Mathf.Abs(camera.transform.up.z) / Mathf.Max(.1f, Mathf.Abs(camera.transform.forward.z)) + .03f;
            target.SetPositionAndRotation(new Vector3(foot.x, foot.y,
                Stage3TacticalPresentation.GrassSurfaceZ(foot)) - camera.transform.forward * clearance, camera.transform.rotation);
            target.GetComponent<SpriteRenderer>().sortingOrder = -Mathf.RoundToInt(position.y * 100f);
        }

        private void RebuildCircle(ref GameObject circle, ref Mesh mesh, string name, Vector2 point)
        {
            if (circle == null)
            {
                circle = new GameObject(name); circle.transform.SetParent(transform, false);
                circle.AddComponent<MeshFilter>();
                var renderer = circle.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = data.sealMaterial; renderer.sortingOrder = short.MinValue + 10;
                renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            }
            if (mesh != null) Destroy(mesh);
            const int divisions = 12;
            var vertices = new Vector3[(divisions + 1) * (divisions + 1)];
            var uv = new Vector2[vertices.Length]; var triangles = new int[divisions * divisions * 6]; var t = 0;
            for (var y = 0; y <= divisions; y++) for (var x = 0; x <= divisions; x++)
            {
                var i = y * (divisions + 1) + x;
                uv[i] = new Vector2((float)x / divisions, (float)y / divisions);
                var p = point + (uv[i] - Vector2.one * .5f) * data.circleDiameter;
                vertices[i] = new Vector3(p.x - point.x, p.y - point.y, Stage3TacticalPresentation.GrassSurfaceZ(p) - .018f);
                if (x == divisions || y == divisions) continue;
                triangles[t++] = i; triangles[t++] = i + divisions + 1; triangles[t++] = i + 1;
                triangles[t++] = i + 1; triangles[t++] = i + divisions + 1; triangles[t++] = i + divisions + 2;
            }
            mesh = new Mesh { name = name + " terrain marking", vertices = vertices, uv = uv, triangles = triangles };
            mesh.RecalculateBounds(); mesh.RecalculateNormals();
            circle.transform.position = point; circle.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private bool Active => ready && game != null && !game.PlayerInputBlocked && !game.RunDriver.Run.IsTerminal && !game.RunDriver.Run.IsPaused && Time.timeScale > 0f;
        private bool InteractionAllowed => Active && !IsPlacingWorkshop && !game.Building.Defense.IsPlacing &&
            (game.Production == null || !game.Production.IsPlacing) && (hud == null || !hud.DebugOpen);

        private void OnEnable()
        {
            interact = new InputAction("World interaction", InputActionType.Button, "<Keyboard>/e");
            interact.Enable(); requiresRelease = true;
            EnableConstructionInput();
        }
        private void OnDisable()
        {
            interact?.Dispose(); interact = null;
            DisableConstructionInput();
            if (hud != null) hud.WorldInteractionHint = null;
        }
        private void Update()
        {
            if (!ready) return;
            UpdateConstruction();
            var held = interact != null && interact.IsPressed();
            if (!held) requiresRelease = false;
            if (!InteractionAllowed)
            {
                requiresRelease |= held;
                if (hud != null && !IsPlacingWorkshop) hud.WorldInteractionHint = null;
                return;
            }
            if (!state.RemoteDiscovered && Vector2.Distance(scene.PlayerMover.transform.position, RemotePosition) <= data.discoveryRadius)
            { state.Discover(); RefreshMarkers(); }
            if (!requiresRelease && interact != null && interact.WasPressedThisFrame())
            { TryInteract(); requiresRelease = true; }
            if (hud != null) hud.WorldInteractionHint = ContextHint();
        }

        public bool TryInteract()
        {
            if (!InteractionAllowed) return false;
            var hero = (Vector2)scene.PlayerMover.transform.position;
            if (state.WorkshopBuilt && Vector2.Distance(hero, WorkshopInteractionPosition) <= data.interactionRadius && state.Study())
            {
                game.Gathering.CancelGatherUntilRelease(); baseCircle.SetActive(true); RefreshMarkers();
                return true;
            }
            if (!state.CanTravel) return false;
            var fromBase = Vector2.Distance(hero, BasePosition) <= data.interactionRadius;
            var fromWorld = Vector2.Distance(hero, RemotePosition) <= data.interactionRadius;
            if (!fromBase && !fromWorld) return false;
            var target = fromBase ? RemotePosition : BasePosition;
            if (obstacles.IsBlocked(target, scene.PlayerMover.CollisionRadius) ||
                !scene.MapDefinition.Contains(scene.MapDefinition.WorldToCell(target))) return false;
            game.Gathering.CancelGatherUntilRelease();
            scene.PlayerMover.transform.position = new Vector3(target.x, target.y, scene.PlayerMover.transform.position.z);
            scene.CameraFollow.SnapToTarget();
            return true;
        }

        private string ContextHint()
        {
            var hero = (Vector2)scene.PlayerMover.transform.position;
            if (state.WorkshopBuilt && Vector2.Distance(hero, WorkshopInteractionPosition) <= data.interactionRadius)
                return state.Researched ? "Телепортация изучена · круг на базе открыт" : "E · исследовать Телепортацию в мастерской";
            if (state.Researched && Vector2.Distance(hero, BasePosition) <= data.interactionRadius)
                return state.RemoteDiscovered ? "E · переместиться к найденному кругу" : "Найдите парный круг в мире, чтобы открыть путь";
            if (Vector2.Distance(hero, RemotePosition) <= data.discoveryRadius)
                return !state.Researched ? "Круг найден · изучите Телепортацию в мастерской" :
                    Vector2.Distance(hero, RemotePosition) <= data.interactionRadius ? "E · вернуться к кругу на базе" :
                    "Войдите в центр круга · E для перехода";
            return null;
        }

        private void ResetProgress()
        {
            CancelWorkshopPlacement();
            if (state.WorkshopBuilt && obstacles != null) obstacles.RemoveRectangle(workshopObstacle);
            if (workshop != null) { workshop.SetActive(false); Destroy(workshop); workshop = null; }
            state.Reset(); requiresRelease = true;
            if (baseCircle != null) baseCircle.SetActive(false);
            if (hud != null) hud.WorldInteractionHint = null;
            RefreshMarkers();
        }
        private void RefreshMarkers() => minimap?.SetWorldTravelMarkers(state.WorkshopBuilt ? WorkshopPosition : null,
            state.Researched ? BasePosition : null, state.RemoteDiscovered ? RemotePosition : null);

        private void OnDestroy()
        {
            if (game != null) game.RunDriver.Run.Reset -= ResetProgress;
            if (game != null) game.Building.PlacementStarting -= CancelWorkshopPlacement;
            if (game != null && game.Production != null) game.Production.PlacementStarting -= CancelWorkshopPlacement;
            if (state.WorkshopBuilt && obstacles != null) obstacles.RemoveRectangle(workshopObstacle);
            if (hud != null) hud.BindWorkshopConstruction(null, null, null, null);
            ReleaseConstructionMaterials();
            if (minimap != null) minimap.SetWorldTravelMarkers(null, null, null);
            if (workshop != null) Destroy(workshop);
            if (baseCircle != null) Destroy(baseCircle);
            if (worldCircle != null) Destroy(worldCircle);
            if (baseMesh != null) Destroy(baseMesh);
            if (worldMesh != null) Destroy(worldMesh);
        }
    }
}
