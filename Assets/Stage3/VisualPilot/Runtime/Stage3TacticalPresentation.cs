using System.Collections;
using System.Collections.Generic;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>One flat XY rules plane, real height along -Z, one shared tactical camera.
    /// Temporary actor solids are scale references, not final character/content production.</summary>
    public sealed class Stage3TacticalPresentation : MonoBehaviour
    {
        public const int NonWalkableBorderCells = 128;
        public const float GrassVisualLift = .08f;
        public const float GrassApronReach = .40f;
        [SerializeField] private Material grass;
        [SerializeField] private Stage3RoadsideArt roadsideArt;
        [SerializeField] private Stage3ResourceDistrictArt resourceDistrictArt;
        [SerializeField] private Stage3WorldTravelData worldTravelData;
        [SerializeField] private Material grassVerge;
        [SerializeField] private Material paving;
        [SerializeField] private Material neutral;
        [SerializeField] private Stage3SkeletonSprite skeletonSpritePrefab;
        [SerializeField] private Material hero;
        [SerializeField] private Stage3HeroArt heroArt;
        [SerializeField] private bool enableDayNightPresentation;
        [SerializeField] private Material dayNightGround;
        [SerializeField] private Material throne;
        [SerializeField] private GameObject soulSpherePrefab;
        [SerializeField] private Material entrance;
        [SerializeField] private GameObject entrancePortalPrefab;
        [SerializeField] private Material woodSource;
        [SerializeField] private Material stoneSource;
        [SerializeField] private Material metalSource;
        [SerializeField] private GameObject woodSourceModel;
        [SerializeField] private GameObject stoneSourceModel;
        [SerializeField] private GameObject metalSourceModel;
        [SerializeField] private Material grassBlades;
        [SerializeField] private Material flowerPetals;
        [SerializeField] private Material flowerCenters;
        private Stage2SceneBootstrap bootstrap;
        private Stage2GameComposition composition;
        private Stage3WorldTravel worldTravel;
        private GameObject terrain;
        private Mesh terrainMesh;
        private GameObject grassEdge;
        private Mesh grassEdgeMesh;
        private GameObject borderGround;
        private Mesh borderMesh;
        private GameObject constructionGrid;
        private Mesh constructionGridMesh;
        private Material constructionGridMaterial;
        private Transform heroVisual;
        private GameObject preview;
        private int previewRole = -1;
        private Material spritePreviewMaterial, meshPreviewMaterial;
        private Stage2PlacementMark previewFootprint;
        private MaterialPropertyBlock tint;
        private readonly List<GameObject> manualVisuals = new List<GameObject>();
        private Vector2 manualHeroStart;
        private bool manualResourcesReady;

        private IEnumerator Start()
        {
            tint = new MaterialPropertyBlock();
            yield return null;
            bootstrap = GetComponent<Stage2SceneBootstrap>();
            composition = GetComponent<Stage2GameComposition>();
            if (bootstrap == null || composition == null || !composition.IsInitialized) yield break;
            manualHeroStart = bootstrap.PlayerMover.transform.position;
            var minimap = GetComponent<Stage2Minimap>();
            if (minimap == null) minimap = gameObject.AddComponent<Stage2Minimap>();
            minimap.Initialize(bootstrap, composition);
            if (worldTravelData != null)
            {
                worldTravel = gameObject.AddComponent<Stage3WorldTravel>();
                worldTravel.Initialize(worldTravelData, bootstrap, composition, minimap);
            }
            Stage3ResourceDistrictWorld districtWorld = null;
            if (resourceDistrictArt != null)
            {
                districtWorld = gameObject.AddComponent<Stage3ResourceDistrictWorld>();
                districtWorld.Initialize(bootstrap.MapDefinition, resourceDistrictArt,
                    bootstrap.CameraFollow.BoundCamera, bootstrap.PlayerMover.transform, minimap, RebuildLandscape);
            }
            else { worldTravel?.BindWorld(null); RebuildLandscape(null); }
            RebuildManualResources();
            manualResourcesReady = true;
            RefreshSiteClearances();
            Stage3RoadsideDecoration.Build(bootstrap.MapDefinition,transform,roadsideArt,bootstrap.CameraFollow.BoundCamera,
                LandscapeClearances());
            HideSprites(bootstrap.PlayerMover.gameObject);
            if (heroArt != null && heroArt.IsValid)
                bootstrap.PlayerMover.gameObject.AddComponent<Stage3HeroAvatar>().Initialize(heroArt,composition,bootstrap.CameraFollow.BoundCamera);
            else
                heroVisual = Solid(bootstrap.PlayerMover.transform, "Hero scale marker", PrimitiveType.Capsule,
                    new Vector3(0,0,-.43f), new Vector3(.38f,.43f,.38f), hero);
            UpdateHeroGroundHeight(true);
            HideSprites(bootstrap.ThroneMarker.gameObject);
            if (soulSpherePrefab != null)
            {
                var objective = Instantiate(soulSpherePrefab, bootstrap.ThroneMarker, false);
                var sprite = objective.GetComponent<Stage3LandmarkSprite>();
                if (sprite != null) sprite.Bind(composition.RunDriver, bootstrap.CameraFollow.BoundCamera);
            }
            else
            {
                Solid(bootstrap.ThroneMarker, "Throne base", PrimitiveType.Cylinder,
                    new Vector3(0,0,-.08f), new Vector3(1.0f,.08f,1.0f), neutral);
                Solid(bootstrap.ThroneMarker, "Throne objective marker", PrimitiveType.Cube,
                    new Vector3(0,0,-.57f), new Vector3(.55f,.85f,.55f), throne);
            }
            HideSprites(bootstrap.EntranceMarker.gameObject);
            if (entrancePortalPrefab != null)
            {
                var portal=Instantiate(entrancePortalPrefab,bootstrap.EntranceMarker,false);
                var towardGoal=bootstrap.MapDefinition.Throne-bootstrap.MapDefinition.EnemyEntrance;
                var quarterTurns=Mathf.RoundToInt(Mathf.Atan2(towardGoal.y,towardGoal.x)
                    *Mathf.Rad2Deg/90f);
                // Keep the square 3x3 foundation grid-aligned. The authored
                // aperture is 45 degrees from this quarter-turn bearing.
                portal.transform.localRotation=Quaternion.AngleAxis(quarterTurns*90f,Vector3.forward);
                portal.GetComponent<Stage3EntrancePortal>().Bind(composition.RunDriver);
                var sprite = portal.GetComponent<Stage3LandmarkSprite>();
                if (sprite != null) sprite.Bind(composition.RunDriver, bootstrap.CameraFollow.BoundCamera);
            }
            else
                Solid(bootstrap.EntranceMarker, "Entrance disc", PrimitiveType.Cylinder,
                    new Vector3(0,0,-.035f), new Vector3(1.05f,.035f,1.05f), entrance);
            foreach (var tower in FindObjectsByType<Stage2Tower>(FindObjectsSortMode.None)) OnTowerBuilt(tower);
            composition.Building.Defense.Built += OnTowerBuilt;
            gameObject.AddComponent<Stage3CombatPresentation>().Initialize(composition, neutral);
            if (enableDayNightPresentation)
                gameObject.AddComponent<Stage3DayNightPresentation>().Initialize(composition,dayNightGround);
            Debug.Log("S3_TACTICAL_VIEW_READY elevation=55 planarGrid=true heightAxis=-Z");
        }

        private void RebuildManualResources()
        {
            GetComponent<Stage3ManualResourceCollisions>()?.ClearBindings();
            var interactionMarker = GetComponent<Stage3ResourceInteractionMarker>();
            if (interactionMarker == null) interactionMarker = gameObject.AddComponent<Stage3ResourceInteractionMarker>();
            interactionMarker.ClearBindings();
            interactionMarker.Bind(bootstrap.CameraFollow.BoundCamera, composition.Gathering);
            var economy = composition.Gathering.Economy;
            var districtWorld = GetComponent<Stage3ResourceDistrictWorld>();
            if (composition.Gathering.Catalog.ManualResources != null)
            {
                bool Excluded(Vector2 point, float radius)
                {
                    if (Vector2.Distance(point, manualHeroStart) < 4f + radius ||
                        Vector2.Distance(point, bootstrap.EntranceMarker.position) < 5f + radius ||
                        Vector2.Distance(point, bootstrap.ThroneMarker.position) < 5f + radius) return true;
                    for (var i = 0; i < 3; i++)
                        if (Vector2.Distance(point, economy.WorldPosition((MapResourceKind)i)) < 3f + radius) return true;
                    if (worldTravel != null)
                        foreach (var site in worldTravel.SitePositions)
                            if (Vector2.Distance(point, site) < 4f + radius) return true;
                    if (districtWorld?.Districts != null)
                        foreach (var district in districtWorld.Districts)
                        {
                            var bounds = district.Bounds;
                            if (point.x >= bounds.xMin - radius - 2f && point.x <= bounds.xMax + radius + 2f &&
                                point.y >= bounds.yMin - radius - 2f && point.y <= bounds.yMax + radius + 2f) return true;
                        }
                    return districtWorld?.Obstacles?.IsBlocked(point, radius + .4f) ?? false;
                }
                var additional = Stage2ManualResourceGenerator.Generate(bootstrap.MapDefinition,
                    composition.Gathering.Catalog, districtWorld != null ? districtWorld.Seed : 164032, Excluded);
                economy.ReplaceAdditionalNodes(additional);
            }
            foreach (var visual in manualVisuals)
                if (visual != null) { visual.SetActive(false); Destroy(visual); }
            manualVisuals.Clear();
            foreach (var node in economy.Nodes)
            {
                var i = (int)node.Resource;
                var primary = node == economy.GetNode(node.Resource);
                var marker = primary ? transform.Find($"{node.Resource} Resource Marker") : null;
                if (marker != null) HideSprites(marker.gameObject);
                var root = new GameObject($"Manual {node.Resource} {node.Size} stock {node.Capacity} group {node.GroupId}");
                root.transform.SetParent(marker != null ? marker : transform, false);
                root.transform.position = new Vector3(node.WorldPosition.x, node.WorldPosition.y, GrassSurfaceZ(node.WorldPosition));
                manualVisuals.Add(root);
                var model = i == 0 ? woodSourceModel : i == 1 ? stoneSourceModel : metalSourceModel;
                if (model != null)
                {
                    var visual = Instantiate(model, root.transform, false);
                    var sprite = visual.GetComponent<Stage3ResourceSprite>();
                    if (sprite != null)
                    {
                        sprite.Bind(bootstrap.CameraFollow.BoundCamera, node, bootstrap.PlayerMover.transform);
                        interactionMarker.Register(sprite);
                    }
                }
                else
                    Solid(root.transform, "Resource scale marker", i == 1 ? PrimitiveType.Sphere : PrimitiveType.Cylinder,
                        new Vector3(0,0,i == 1 ? -.32f : -.45f),
                        new Vector3(.65f,i == 1 ? .64f : .45f,.65f),
                        i == 0 ? woodSource : i == 1 ? stoneSource : metalSource);
            }
            var manualSettings = composition.Gathering.Catalog.ManualResources;
            if (manualSettings != null)
            {
                var obstacles = districtWorld?.Obstacles ?? bootstrap.PlayerMover.Obstacles ?? new HeroObstacleMap();
                var collisions = GetComponent<Stage3ManualResourceCollisions>();
                if (collisions == null) collisions = gameObject.AddComponent<Stage3ManualResourceCollisions>();
                collisions.Bind(economy, obstacles, bootstrap.PlayerMover, manualSettings.treeTrunkCollisionRadius);
                if (!ReferenceEquals(bootstrap.PlayerMover.Obstacles, obstacles))
                    bootstrap.PlayerMover.BindObstacles(obstacles, bootstrap.PlayerMover.CollisionRadius);
            }
            Debug.Log($"S3_MANUAL_RESOURCES_READY nodes={economy.Nodes.Count} seed={districtWorld?.Seed ?? 164032}");
        }

        private void RebuildLandscape(IReadOnlyList<ResourceDistrict> districts)
        {
            ReleaseSurface();
            BuildSurface(districts);
            BuildConstructionGrid();
            if (manualResourcesReady) RebuildManualResources();
            Stage3LandscapeProps.Clear(transform);
            BuildSiteProps(districts);
        }

        public void RefreshSiteClearances()
        {
            Stage3LandscapeProps.Clear(transform);
            BuildSiteProps(GetComponent<Stage3ResourceDistrictWorld>()?.Districts);
        }

        private void BuildSiteProps(IReadOnlyList<ResourceDistrict> districts)
        {
            Stage3LandscapeProps.Build(bootstrap.MapDefinition, transform,
                LandscapeClearances(), grassBlades, flowerPetals, flowerCenters, GrassVisualLift, districts);
        }

        private Vector2[] LandscapeClearances()
        {
            var points = new List<Vector2> { bootstrap.EntranceMarker.position, bootstrap.ThroneMarker.position };
            foreach (var node in composition.Gathering.Economy.Nodes) points.Add(node.WorldPosition);
            if (worldTravel != null)
                foreach (var point in worldTravel.SitePositions)
                {
                    points.Add(point);
                    points.Add(point + Vector2.left * .75f); points.Add(point + Vector2.right * .75f);
                    points.Add(point + Vector2.up * .75f); points.Add(point + Vector2.down * .75f);
                }
            return points.ToArray();
        }

        private void OnTowerBuilt(Stage2Tower tower)
        {
            tower.transform.localScale = Vector3.one;
            var sprite = tower.GetComponentInChildren<Stage3TowerSprite>();
            if (sprite != null)
            {
                var placeholder = tower.GetComponent<SpriteRenderer>();
                if (placeholder != null) placeholder.enabled = false;
                sprite.Bind(bootstrap.CameraFollow.BoundCamera);
                return;
            }
            HideSprites(tower.gameObject);
            if (tower.GetComponentInChildren<MeshRenderer>() == null)
                Solid(tower.transform, "Longshot scale marker", PrimitiveType.Cylinder,
                    new Vector3(0,0,-.65f), new Vector3(.58f,.65f,.58f), neutral);
            Solid(tower.transform, "Occupied cell footing", PrimitiveType.Cube,
                new Vector3(0,0,-.015f), new Vector3(.94f,.03f,.94f), paving);
        }

        private void LateUpdate()
        {
            if (composition == null || !composition.IsInitialized) return;
            UpdateHeroGroundHeight(false);
            for (var i=0; i<composition.Enemies.ActiveCount; i++)
            {
                var enemy = composition.Enemies.GetEnemy(i);
                if (enemy == null || enemy.GetComponentInChildren<Stage3SkeletonSprite>() != null ||
                    enemy.GetComponentInChildren<MeshRenderer>() != null) continue;
                HideSprites(enemy.gameObject);
                enemy.transform.localScale = Vector3.one;
                if (skeletonSpritePrefab != null)
                {
                    var sprite = Instantiate(skeletonSpritePrefab, enemy.transform, false);
                    sprite.Bind(enemy, composition.RunDriver, bootstrap.CameraFollow.BoundCamera);
                    continue;
                }
                Solid(enemy.transform, "Enemy scale marker", PrimitiveType.Capsule,
                    new Vector3(0,0,-.32f), new Vector3(.34f,.32f,.34f), neutral);
                enemy.gameObject.AddComponent<Stage3MeshFeedback>();
            }
            UpdatePreview();
        }

        private void UpdateHeroGroundHeight(bool immediate)
        {
            if (heroVisual == null || bootstrap == null) return;
            var map = bootstrap.MapDefinition;
            var world = bootstrap.PlayerMover.transform.position;
            var cell = map.WorldToCell(world);
            var target = -.43f + (map.Contains(cell) && map.GetSurface(cell) == MapSurfaceKind.Forest
                ? GrassSurfaceZ(world) : 0f);
            var position = heroVisual.localPosition;
            position.z = immediate ? target : Mathf.MoveTowards(position.z, target, Time.deltaTime * 1.5f);
            heroVisual.localPosition = position;
        }

        private void UpdatePreview()
        {
            var driver = composition.Building;
            var defense = driver.Defense;
            // Keep the whole legal construction area visible even while the pointer
            // is on the selected HUD card; only the cursor ghost hides over UI.
            if (constructionGrid != null) constructionGrid.SetActive(defense.IsPlacing);
            if (driver.PreviewRenderer != null) driver.PreviewRenderer.enabled = false;
            if (!defense.IsPlacing || !defense.PreviewCell.HasValue || driver.PointerOverUi)
            {
                if (preview != null) preview.SetActive(false);
                return;
            }
            if (preview == null || previewRole != defense.SelectedRoleIndex)
            {
                if (preview != null) Destroy(preview);
                preview = new GameObject("Tactical mesh placement preview");
                if (spritePreviewMaterial == null) spritePreviewMaterial = Stage2PlacementVisuals.Material(true);
                if (meshPreviewMaterial == null) meshPreviewMaterial = Stage2PlacementVisuals.Material(false);
                previewRole = defense.SelectedRoleIndex;
                var model = defense.SelectedCatalog.TowerPrefab.transform.Find("Pilot ground-aligned mesh");
                var spriteModel = defense.SelectedCatalog.TowerPrefab.GetComponentInChildren<Stage3TowerSprite>(true);
                if (spriteModel != null)
                {
                    var sprite = Instantiate(spriteModel,preview.transform,false);
                    sprite.Bind(bootstrap.CameraFollow.BoundCamera);
                    foreach (var renderer in sprite.GetComponentsInChildren<SpriteRenderer>()) renderer.sharedMaterial = spritePreviewMaterial;
                }
                else if (model != null) Instantiate(model.gameObject, preview.transform, false);
                else Solid(preview.transform, "Longshot preview", PrimitiveType.Cylinder,
                    new Vector3(0,0,-.65f), new Vector3(.58f,.65f,.58f), neutral);
                previewFootprint = Stage2PlacementVisuals.Outline(preview.transform, Vector2.one * .96f, meshPreviewMaterial, "Selected cell footprint");
                previewFootprint.transform.localPosition = Vector3.back * .06f;
                foreach (var renderer in preview.GetComponentsInChildren<MeshRenderer>())
                { renderer.sharedMaterial = meshPreviewMaterial; renderer.shadowCastingMode = ShadowCastingMode.Off; }
            }
            preview.SetActive(true);
            preview.transform.position = bootstrap.MapDefinition.CellToWorld(defense.PreviewCell.Value);
            var status = defense.EvaluatePreview() == PlacementResult.Ready ? Stage2PlacementVisuals.Valid : Stage2PlacementVisuals.Invalid;
            tint.SetColor("_Color", status);
            foreach (var renderer in preview.GetComponentsInChildren<MeshRenderer>()) renderer.SetPropertyBlock(tint);
            previewFootprint.SetTint(new Color(status.r,status.g,status.b,.9f));
            var previewSprite = preview.GetComponentInChildren<Stage3TowerSprite>();
            if (previewSprite != null) previewSprite.Tint(status);
        }

        private void BuildConstructionGrid()
        {
            var map = bootstrap.MapDefinition;
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            bool BuildCell(Vector2Int cell) => map.Contains(cell) &&
                map.GetSurface(cell)==MapSurfaceKind.InvasionCorridor &&
                map.IsBuildable(cell) && map.IsWalkable(cell) &&
                cell!=map.EnemyEntrance && cell!=map.Throne;
            void Strip(Vector2 cell, Vector2 a, Vector2 b, float width, Color color)
            {
                var normal = new Vector2(-(b-a).y, (b-a).x).normalized * width*.5f;
                var first = vertices.Count;
                foreach (var point in new[] { a-normal, b-normal, b+normal, a+normal })
                {
                    var world = map.CellCoordinatesToWorld(cell + point);
                    vertices.Add(new Vector3(world.x, world.y, -.045f));
                    colors.Add(color);
                }
                triangles.AddRange(new[] { first,first+1,first+2,first,first+2,first+3 });
            }
            for (var y=map.MinimumCell.y; y<=map.MaximumCell.y; y++)
            for (var x=map.MinimumCell.x; x<=map.MaximumCell.x; x++)
            {
                var cell = new Vector2Int(x,y);
                if (!BuildCell(cell)) continue;
                var corners = new[] { new Vector2(-.499f,-.499f),new Vector2(.499f,-.499f),
                    new Vector2(.499f,.499f),new Vector2(-.499f,.499f) };
                var neighbors = new[] { Vector2Int.down,Vector2Int.right,Vector2Int.up,Vector2Int.left };
                for (var edge=0; edge<4; edge++)
                {
                    var shared=BuildCell(cell+neighbors[edge]);
                    // Each shared boundary is drawn once, rather than two cell frames.
                    if(shared && (edge==1 || edge==2)) continue;
                    var a=corners[edge]; var b=corners[(edge+1)%4];
                    // Outer strokes remain inside the road, including reserved holes.
                    var inset=shared?Vector2.zero:new Vector2(-(b-a).y,(b-a).x).normalized*.018f;
                    Strip(cell,a+inset,b+inset,.035f,new Color(.10f,.13f,.13f,.72f));
                    Strip(cell,a+inset,b+inset,.012f,new Color(.91f,.95f,.91f,.82f));
                }
            }
            constructionGridMesh = new Mesh { name="Tower construction cell grid", indexFormat=IndexFormat.UInt32 };
            constructionGridMesh.SetVertices(vertices); constructionGridMesh.SetColors(colors);
            constructionGridMesh.SetUVs(0,new List<Vector2>(new Vector2[vertices.Count]));
            constructionGridMesh.SetTriangles(triangles,0); constructionGridMesh.RecalculateBounds();
            constructionGridMaterial = Stage2PlacementVisuals.Material(false);
            constructionGridMaterial.renderQueue=(int)RenderQueue.Transparent-10;
            constructionGrid = new GameObject("Tower construction cell grid");
            constructionGrid.transform.SetParent(transform,false);
            constructionGrid.AddComponent<MeshFilter>().sharedMesh=constructionGridMesh;
            var renderer=constructionGrid.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=constructionGridMaterial;
            renderer.sortingOrder=-30000;
            renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            constructionGrid.SetActive(false);
        }

        private void BuildSurface(IReadOnlyList<ResourceDistrict> districts)
        {
            bootstrap.MapDebugRenderer.GetComponent<MeshRenderer>().enabled = false;
            var map = bootstrap.MapDefinition;
            // The border is visible terrain, but lies outside authored cells:
            // player movement, building and navigation cannot enter it.
            var minimum = map.MinimumCell;
            var maximum = map.MaximumCell;
            var minX = minimum.x - NonWalkableBorderCells - .5f;
            var minY = minimum.y - NonWalkableBorderCells - .5f;
            var maxX = maximum.x + NonWalkableBorderCells + .5f;
            var maxY = maximum.y + NonWalkableBorderCells + .5f;
            borderMesh = new Mesh { name = "Non-walkable map border" };
            var borderVertices = new List<Vector3>();
            var borderUV = new List<Vector2>();
            var borderTriangles = new List<int>();
            void BorderQuad(float left,float bottom,float right,float top)
            {
                var first=borderVertices.Count;
                foreach(var point in new[]{new Vector2(left,bottom),new Vector2(left,top),new Vector2(right,top),new Vector2(right,bottom)})
                {
                    borderVertices.Add(new Vector3(point.x,point.y,.02f));
                    borderUV.Add((point+Vector2.one*.5f)/4f);
                }
                borderTriangles.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
            }
            // The outside ring must not secretly put a flat grass sheet below excavations.
            BorderQuad(minX,minY,minimum.x-.5f,maxY);
            BorderQuad(maximum.x+.5f,minY,maxX,maxY);
            BorderQuad(minimum.x-.5f,minY,maximum.x+.5f,minimum.y-.5f);
            BorderQuad(minimum.x-.5f,maximum.y+.5f,maximum.x+.5f,maxY);
            borderMesh.SetVertices(borderVertices);
            // One seamless pigment tile covers 4x4 cells, including the
            // non-walkable visual border beyond the authored map.
            borderMesh.SetUVs(0,borderUV);
            borderMesh.SetTriangles(borderTriangles,0);
            borderMesh.RecalculateNormals();
            borderMesh.RecalculateBounds();
            borderGround = new GameObject("Non-walkable map border");
            borderGround.AddComponent<MeshFilter>().sharedMesh = borderMesh;
            var borderRenderer = borderGround.AddComponent<MeshRenderer>();
            borderRenderer.sharedMaterial = grass;
            borderRenderer.shadowCastingMode = ShadowCastingMode.Off;
            var borderTint = new MaterialPropertyBlock();
            borderTint.SetColor("_BaseColor", Color.white);
            borderRenderer.SetPropertyBlock(borderTint);
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var normals = new List<Vector3>();
            var triangles = new[] { new List<int>(), new List<int>() };
            var edgeVertices = new List<Vector3>();
            var edgeUV = new List<Vector2>();
            var edgeTriangles = new List<int>();
            var reliefCells=new HashSet<Vector2Int>();
            if(districts!=null)foreach(var district in districts)
                if(district.CutsTerrain && district.Kind!=MapResourceKind.Wood)
                    foreach(var cell in district.Cells)reliefCells.Add(cell);
            for (var x=map.MinimumCell.x; x<=map.MaximumCell.x; x++)
            for (var y=map.MinimumCell.y; y<=map.MaximumCell.y; y++)
            {
                if(reliefCells.Contains(new Vector2Int(x,y)))continue;
                var ground = map.GetSurface(new Vector2Int(x,y)) == MapSurfaceKind.Forest ? 0 : 1;
                // A 100x100 world keeps the same textured grass/normal function,
                // while one quad per distant cell avoids 16 off-screen quads.
                var divisions = ground == 0 && map.Size.x < 64 ? 4 : 1;
                for (var dx = 0; dx < divisions; dx++)
                for (var dy = 0; dy < divisions; dy++)
                {
                    var a = new Vector2(x-.5f+dx/(float)divisions,
                        y-.5f+dy/(float)divisions);
                    var b = a + new Vector2(0f,1f/divisions);
                    var c = a + new Vector2(1f/divisions,1f/divisions);
                    var d = a + new Vector2(1f/divisions,0f);
                    var first = vertices.Count;
                    foreach (var point in new[] { a,b,c,d })
                    {
                        vertices.Add(new Vector3(point.x,point.y,
                            ground == 0 ? GrassSurfaceZ(point) : 0f));
                        var tilePoint = point+Vector2.one*.5f;
                        if (ground == 1 && grassVerge != null)
                            tilePoint = new Vector2(tilePoint.x*.61f-tilePoint.y,tilePoint.x*.61f+tilePoint.y)*.70710678f;
                        uv.Add(tilePoint/4f);
                        normals.Add(ground == 0 ? GrassSurfaceNormal(point) : Vector3.back);
                    }
                    triangles[ground].AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
                }
                if (ground == 0)
                {
                    if (IsLowerNeighbor(map, x-1, y))
                        AddGrassApron(edgeVertices, edgeTriangles, edgeUV,
                            new Vector2(x-.5f,y+.5f), new Vector2(x-.5f,y-.5f));
                    if (IsLowerNeighbor(map, x+1, y))
                        AddGrassApron(edgeVertices, edgeTriangles, edgeUV,
                            new Vector2(x+.5f,y-.5f), new Vector2(x+.5f,y+.5f));
                    if (IsLowerNeighbor(map, x, y-1))
                        AddGrassApron(edgeVertices, edgeTriangles, edgeUV,
                            new Vector2(x-.5f,y-.5f), new Vector2(x+.5f,y-.5f));
                    if (IsLowerNeighbor(map, x, y+1))
                        AddGrassApron(edgeVertices, edgeTriangles, edgeUV,
                            new Vector2(x+.5f,y+.5f), new Vector2(x-.5f,y+.5f));
                }
                else if(grassVerge != null)
                {
                    // The outside world is visual grass too. Its border has no
                    // authored Forest cell from which the usual apron can grow.
                    if(x==map.MaximumCell.x)
                        AddGrassApron(edgeVertices,edgeTriangles,edgeUV,
                            new Vector2(x+.5f,y+.5f),new Vector2(x+.5f,y-.5f));
                    if(x==map.MinimumCell.x)
                        AddGrassApron(edgeVertices,edgeTriangles,edgeUV,
                            new Vector2(x-.5f,y-.5f),new Vector2(x-.5f,y+.5f));
                    if(y==map.MaximumCell.y)
                        AddGrassApron(edgeVertices,edgeTriangles,edgeUV,
                            new Vector2(x-.5f,y+.5f),new Vector2(x+.5f,y+.5f));
                    if(y==map.MinimumCell.y)
                        AddGrassApron(edgeVertices,edgeTriangles,edgeUV,
                            new Vector2(x+.5f,y-.5f),new Vector2(x-.5f,y-.5f));
                }
            }
            terrainMesh = new Mesh { name = "Stage 3 visual ground", indexFormat = IndexFormat.UInt32,
                subMeshCount = 2 };
            terrainMesh.SetVertices(vertices); terrainMesh.SetUVs(0,uv);
            terrainMesh.SetNormals(normals);
            terrainMesh.SetTriangles(triangles[0],0); terrainMesh.SetTriangles(triangles[1],1);
            terrainMesh.RecalculateBounds();
            terrain = new GameObject("Tactical grass and paved build cells");
            terrain.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
            var renderer = terrain.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] {grass,paving};
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            grassEdgeMesh = new Mesh { name = "Visual grass verge" };
            grassEdgeMesh.SetVertices(edgeVertices);
            grassEdgeMesh.SetUVs(0, edgeUV);
            if (grassVerge != null)
            {
                var colors = new Color[edgeVertices.Count];
                for (var i=0;i<colors.Length;i++) colors[i] = new Color(1,1,1,i%2==0 ? 1f : 0f);
                grassEdgeMesh.colors = colors;
            }
            grassEdgeMesh.SetTriangles(edgeTriangles, 0);
            grassEdgeMesh.RecalculateNormals(); grassEdgeMesh.RecalculateBounds();
            grassEdge = new GameObject("Visual grass verge");
            grassEdge.AddComponent<MeshFilter>().sharedMesh = grassEdgeMesh;
            var edgeRenderer = grassEdge.AddComponent<MeshRenderer>();
            edgeRenderer.sharedMaterial = grassVerge != null ? grassVerge : grass;
            edgeRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static bool IsLowerNeighbor(Stage2MapDefinition map, int x, int y)
        {
            var cell = new Vector2Int(x, y);
            return map.Contains(cell) && map.GetSurface(cell) != MapSurfaceKind.Forest;
        }

        internal static float GrassApronDistance(Vector2 point)
        {
            var broad = (Mathf.PerlinNoise((point.x + 67.1f) * .9f,
                (point.y + 31.7f) * .9f) - .5f) * .24f;
            var detail = (Mathf.PerlinNoise((point.x + 11.3f) * 5.1f,
                (point.y + 79.5f) * 5.1f) - .5f) * .10f;
            return Mathf.Clamp(GrassApronReach + broad + detail, .23f, .65f);
        }

        // Presentation-only humps. Rules, cell picking, navigation and tower feet
        // remain on the original flat XY plane.
        internal static float GrassSurfaceZ(Vector2 point)
        {
            var broad = Mathf.PerlinNoise((point.x+53.2f)*.34f,
                (point.y+17.8f)*.34f);
            var fine = Mathf.PerlinNoise((point.x+11.6f)*.86f,
                (point.y+81.3f)*.86f);
            return -GrassVisualLift - .12f * Mathf.Clamp01(broad*.75f+fine*.25f);
        }

        private static Vector3 GrassSurfaceNormal(Vector2 point)
        {
            const float step = .03f;
            var dx = (GrassSurfaceZ(point+Vector2.right*step)-
                GrassSurfaceZ(point-Vector2.right*step))/(2f*step);
            var dy = (GrassSurfaceZ(point+Vector2.up*step)-
                GrassSurfaceZ(point-Vector2.up*step))/(2f*step);
            return new Vector3(dx,dy,-1f).normalized;
        }

        private static void AddGrassApron(List<Vector3> vertices, List<int> triangles,
            List<Vector2> uv, Vector2 start, Vector2 end)
        {
            const int segments = 12;
            var first = vertices.Count;
            var tangent = (end - start).normalized;
            var outward = new Vector2(tangent.y, -tangent.x);
            for (var i = 0; i <= segments; i++)
            {
                var inner = Vector2.Lerp(start, end, i / (float)segments);
                var outer = inner + outward * GrassApronDistance(inner);
                vertices.Add(new Vector3(inner.x, inner.y, GrassSurfaceZ(inner)));
                vertices.Add(new Vector3(outer.x, outer.y, -.006f));
                uv.Add(new Vector2((inner.x + .5f) / 4f, (inner.y + .5f) / 4f));
                uv.Add(new Vector2((outer.x + .5f) / 4f, (outer.y + .5f) / 4f));
                if (i == segments) continue;
                var index = first + 2 * i;
                triangles.AddRange(new[] { index, index+2, index+3,
                    index, index+3, index+1 });
            }
        }

        private static void HideSprites(GameObject target)
        {
            foreach (var sprite in target.GetComponentsInChildren<SpriteRenderer>()) sprite.enabled = false;
        }

        private static Transform Solid(Transform parent, string name, PrimitiveType shape,
            Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(shape);
            part.name = name;
            Destroy(part.GetComponent<Collider>()); // Visuals never own movement or cell occupancy.
            part.transform.SetParent(parent,false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(-90f,0,0);
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            return part.transform;
        }

        private void OnDestroy()
        {
            Stage3LandscapeProps.Clear(transform);
            if (composition != null && composition.Building?.Defense != null)
                composition.Building.Defense.Built -= OnTowerBuilt;
            ReleaseSurface();
            if (preview != null) Destroy(preview);
            if (spritePreviewMaterial != null) Destroy(spritePreviewMaterial);
            if (meshPreviewMaterial != null) Destroy(meshPreviewMaterial);
        }

        private void OnDisable()
        {
            if (constructionGrid != null) constructionGrid.SetActive(false);
            if (preview != null) preview.SetActive(false);
        }

        private void ReleaseSurface()
        {
            if (terrain != null) { terrain.SetActive(false);Destroy(terrain); }
            if (terrainMesh != null) Destroy(terrainMesh);
            if (grassEdge != null) { grassEdge.SetActive(false);Destroy(grassEdge); }
            if (grassEdgeMesh != null) Destroy(grassEdgeMesh);
            if (borderGround != null) { borderGround.SetActive(false);Destroy(borderGround); }
            if (borderMesh != null) Destroy(borderMesh);
            if (constructionGrid != null) { constructionGrid.SetActive(false); Destroy(constructionGrid); }
            if (constructionGridMesh != null) Destroy(constructionGridMesh);
            if (constructionGridMaterial != null) Destroy(constructionGridMaterial);
            constructionGrid=null; constructionGridMesh=null; constructionGridMaterial=null;
            terrain=null;terrainMesh=null;grassEdge=null;grassEdgeMesh=null;borderGround=null;borderMesh=null;
        }
    }
}
