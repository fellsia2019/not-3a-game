using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Scene-owned opened travel points and contextual research interaction.</summary>
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
        private Stage3TravelState.Snapshot checkpoint;
        private Vector2 checkpointWorkshop;

        public bool IsReady => ready;
        public bool IsResearched => state.Researched;
        public bool WorkshopBuilt => state.WorkshopBuilt;
        public event Action WorkshopOpenRequested;
        public bool NearWorkshop=>ready&&state.WorkshopBuilt&&scene!=null&&Vector2.Distance(scene.PlayerMover.transform.position,WorkshopInteractionPosition)<=data.interactionRadius;
        public bool CanStudyAtWorkshop=>NearWorkshop&&!game.RunDriver.Run.IsTerminal&&!game.RunDriver.Run.IsPaused&&Time.timeScale>0f&&
            !(hud?.DebugOpen??false)&&!IsPlacingWorkshop&&!IsPlacingBaseCircle&&!game.Building.Defense.IsPlacing&&!(game.Production?.IsPlacing??false)&&
            !(GetComponent<Stage3Forge>()?.IsPlacing??false)&&!(GetComponent<Stage3Dwelling>()?.IsPlacing??false)&&
            (!game.PlayerInputBlocked||GetComponent<Stage3FantasyHud>()?.ActiveWindow==Stage3PlayerWindow.Research);
        public SpriteRenderer WorkshopVisual => workshop != null ? workshop.GetComponent<SpriteRenderer>() : null;
        public bool RemoteDiscovered => state.RemoteDiscovered;
        public bool BaseVisible => baseCircle != null && baseCircle.activeSelf;
        public bool BaseBuilt => state.BaseBuilt;
        public Vector2 WorkshopPosition { get; private set; }
        public Vector2 BasePosition { get; private set; }
        public Vector2 RemotePosition { get; private set; }
        public Vector2 WorkshopInteractionPosition => WorkshopPosition + Vector2.down *
            (data.workshopFootprint.y * .5f + scene.PlayerMover.CollisionRadius + .25f);
        public Vector2[] SitePositions
        {
            get {if(!ready)return Array.Empty<Vector2>();var sites=new List<Vector2>{WorkshopPosition};
                foreach(var point in state.Points)sites.Add(point.Position);return sites.ToArray();}
        }
        public Vector2[] BuiltSitePositions
        {
            get {if(!ready)return Array.Empty<Vector2>();var sites=new List<Vector2>();
                if(state.WorkshopBuilt)sites.Add(WorkshopPosition);
                foreach(var point in state.Points)if(point.Id!=Stage3TravelState.BaseId||state.BaseBuilt)sites.Add(point.Position);
                return sites.ToArray();}
        }

        public void Initialize(Stage3WorldTravelData settings, Stage2SceneBootstrap bootstrap,
            Stage2GameComposition composition, Stage2Minimap mapView)
        {
            if (settings == null || settings.sealMaterial == null || settings.workshopSprite == null || settings.workshopMaterial == null ||
                settings.workshopWoodCost < 0 || settings.workshopStoneCost < 0)
                throw new ArgumentException("World travel artwork is incomplete.");
            data = settings; scene = bootstrap; game = composition; minimap = mapView;
            hud = game.GetComponent<Stage2GameHud>();
            hud?.BindWorkshopConstruction(data.workshopSprite, () => WorkshopBuildStatus, BeginWorkshopPlacement, CancelWorkshopPlacement,
                data.workshopWoodCost, data.workshopStoneCost);
            game.Building.PlacementStarting += CancelConstructionPlacement;
            if (game.Production != null) game.Production.PlacementStarting += CancelConstructionPlacement;
            game.RunDriver.Run.Reset += ResetProgress;
            game.RunDriver.Run.BeforeWaveStarted += CaptureCheckpoint;
            game.RunDriver.Run.Rewound += RestoreCheckpoint;
        }

        public void BindWorld(Stage3ResourceDistrictWorld world)
        {
            if (data == null) return;
            ClearCity();
            ResetProgress();
            resourceWorld = world;
            obstacles = world == null ? new HeroObstacleMap() : world.Obstacles;
            var chosen = new List<Vector2>();
            WorkshopPosition = FindSite((Vector2)scene.ThroneMarker.position + data.workshopOffsetFromThrone, world, chosen);
            chosen.Add(WorkshopPosition);
            BasePosition = FindSite((Vector2)scene.ThroneMarker.position + data.baseOffsetFromThrone, world, chosen);
            chosen.Add(BasePosition);
            BindCity(world);
            RemotePosition = CityReady ? city.PortalPosition : FindSite(data.worldCirclePosition, world, chosen);
            var points=new List<Stage3TravelPoint>{new Stage3TravelPoint(Stage3TravelState.BaseId,"База",BasePosition),
                new Stage3TravelPoint(Stage3TravelState.CityId,"Город",RemotePosition)};
            chosen.Add(RemotePosition);
            if(data.additionalPoints!=null)foreach(var point in data.additionalPoints)
            {var safe=FindSite(point.position,world,chosen);chosen.Add(safe);points.Add(new Stage3TravelPoint(point.id,point.title,safe));}
            state.Configure(points);
            ClearExtraCircles();
            foreach(var point in state.Points)
                if(point.Id!=Stage3TravelState.BaseId&&point.Id!=Stage3TravelState.CityId)
                {GameObject circle=null;Mesh marking=null;RebuildCircle(ref circle,ref marking,"Teleport "+point.Id,point.Position);extraCircles.Add(circle);extraMeshes.Add(marking);}
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
                var valid = !OverlapsCity(new Rect(point-Vector2.one*data.siteClearance,Vector2.one*data.siteClearance*2f));
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
                vertices[i] = new Vector3(p.x - point.x, p.y - point.y, Stage3TacticalPresentation.GrassSurfaceZ(p) - (name=="World teleport circle" && CityReady ? .05f : .018f));
                if (x == divisions || y == divisions) continue;
                triangles[t++] = i; triangles[t++] = i + divisions + 1; triangles[t++] = i + 1;
                triangles[t++] = i + 1; triangles[t++] = i + divisions + 1; triangles[t++] = i + divisions + 2;
            }
            mesh = new Mesh { name = name + " terrain marking", vertices = vertices, uv = uv, triangles = triangles };
            if(name=="World teleport circle"&&CityReady)
            {var properties=new MaterialPropertyBlock();properties.SetColor("_ContactColor",new Color(.65f,.9f,1f,1f));properties.SetFloat("_OutlinePixels",2.5f);properties.SetFloat("_MaskHigh",.35f);properties.SetFloat("_Opacity",1f);circle.GetComponent<MeshRenderer>().SetPropertyBlock(properties);}
            mesh.RecalculateBounds(); mesh.RecalculateNormals();
            circle.transform.position = point; circle.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private bool Active => ready && game != null && !game.PlayerInputBlocked && !game.RunDriver.Run.IsTerminal && !game.RunDriver.Run.IsPaused && Time.timeScale > 0f;
        private bool InteractionAllowed => Active && !IsPlacingWorkshop && !IsPlacingBaseCircle && !game.Building.Defense.IsPlacing &&
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
            UpdateBaseCircleConstruction();
            UpdateConstruction();
            var held = interact != null && interact.IsPressed();
            if (!held) requiresRelease = false;
            if (!InteractionAllowed)
            {
                requiresRelease |= held;
                if (hud != null && !IsPlacingWorkshop && !IsPlacingBaseCircle) hud.WorldInteractionHint = null;
                return;
            }
            UpdateNetwork();
            if (!requiresRelease && interact != null && interact.WasPressedThisFrame())
            { TryInteract(); requiresRelease = true; }
            if (hud != null) hud.WorldInteractionHint = ContextHint();
        }

        public bool TryInteract()
        {
            if (!InteractionAllowed) return false;
            var hero = (Vector2)scene.PlayerMover.transform.position;
            if (NearWorkshop)
            {
                if(WorkshopOpenRequested!=null)
                {game.Gathering.CancelGatherUntilRelease();WorkshopOpenRequested.Invoke();return true;}
                return TryStudyTeleport();
            }
            var source=NearbyOpenedPoint();
            return source!=null&&TryOpenTeleportMenu(source);
        }

        public bool TryStudyTeleport()
        {
            if(!CanStudyAtWorkshop||!state.Study())return false;
            game.Gathering.CancelGatherUntilRelease();RefreshMarkers();OpeningsChanged?.Invoke();return true;
        }

        private string ContextHint()
        {
            var hero = (Vector2)scene.PlayerMover.transform.position;
            if (state.WorkshopBuilt && Vector2.Distance(hero, WorkshopInteractionPosition) <= data.interactionRadius)
                return WorkshopOpenRequested!=null?"E · открыть исследования":state.Researched ? "Телепортация изучена · постройте круг на базе" : "E · исследовать Телепортацию в исследовательском центре";
            if (state.BaseBuilt && Vector2.Distance(hero, BasePosition) <= CircleInteractionRadius)
                return "Клик / E · открыть телепорты";
            if (Vector2.Distance(hero, RemotePosition) <= data.discoveryRadius)
                return !state.Researched ? "Круг найден · изучите Телепортацию в исследовательском центре" :
                    Vector2.Distance(hero, RemotePosition) <= CircleInteractionRadius ? "Клик / E · открыть телепорты" :
                    "Подойдите к кругу · клик / E";
            return null;
        }

        private void ResetProgress()
        {
            CancelConstructionPlacement();
            if (state.WorkshopBuilt && obstacles != null) obstacles.RemoveRectangle(workshopObstacle);
            if (workshop != null) { workshop.SetActive(false); Destroy(workshop); workshop = null; }
            state.Reset();
            if(state.TryGet(Stage3TravelState.BaseId,out var home))BasePosition=home.Position;
            RequireFreshTravelInput(); OpeningsChanged?.Invoke();
            if (baseCircle != null) baseCircle.SetActive(false);
            if (hud != null) hud.WorldInteractionHint = null;
            RefreshMarkers();
        }
        private void CaptureCheckpoint()
        { checkpoint = state.CaptureSnapshot(); checkpointWorkshop = WorkshopPosition; }
        private void RestoreCheckpoint()
        {
            if (game.RunDriver.Run.PreserveProgressOnRetry)
            {
                CancelConstructionPlacement(); confirmRequiresRelease = true; RequireFreshTravelInput(); OpeningsChanged?.Invoke();
                game.Gathering.CancelGatherUntilRelease(); RefreshMarkers(); return;
            }
            ResetProgress(); WorkshopPosition = checkpointWorkshop;
            state.RestoreSnapshot(checkpoint);
            if(state.TryGet(Stage3TravelState.BaseId,out var home))BasePosition=home.Position;
            if (state.WorkshopBuilt)
            {
                CreateWorkshop(); PlaceWorkshop();
                workshopObstacle = new Rect(WorkshopPosition - data.workshopFootprint * .5f, data.workshopFootprint);
                obstacles.AddRectangle(workshopObstacle);
            }
            if(state.BaseBuilt)RebuildCircle(ref baseCircle,ref baseMesh,"Base teleport circle",BasePosition);
            baseCircle?.SetActive(state.BaseBuilt);
            confirmRequiresRelease = true; requiresRelease = true;
            game.Gathering.CancelGatherUntilRelease(); RefreshMarkers();
            GetComponent<Stage3TacticalPresentation>()?.RefreshSiteClearances();
        }
        private void RefreshMarkers()
        {
            minimap?.SetWorldTravelMarkers(state.WorkshopBuilt ? WorkshopPosition : null,
                state.BaseBuilt ? BasePosition : null, state.RemoteDiscovered ? RemotePosition : null);
            // A known settlement is a navigation landmark, independent of its unopened travel endpoint.
            minimap?.SetCityMarker(CityReady ? CityOrigin : null);
        }

        private void OnDestroy()
        {
            ClearCity(); ClearExtraCircles();
            if (game != null) game.RunDriver.Run.Reset -= ResetProgress;
            if (game != null)
            {
                game.RunDriver.Run.BeforeWaveStarted -= CaptureCheckpoint;
                game.RunDriver.Run.Rewound -= RestoreCheckpoint;
            }
            if (game != null) game.Building.PlacementStarting -= CancelConstructionPlacement;
            if (game != null && game.Production != null) game.Production.PlacementStarting -= CancelConstructionPlacement;
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
