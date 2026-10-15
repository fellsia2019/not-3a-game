using System;
using System.IO;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Builds an isolated visual trial; the Stage 2 scene and catalog stay intact.</summary>
    public static class Stage3RapidPilotBuilder
    {
        private const string Folder = "Assets/Stage3/VisualPilot/";
        private const string SourceScene = "Assets/Scenes/Stage2VerticalSlice.unity";
        private const string PilotScene = "Assets/Scenes/Stage3RapidSpritePilot.unity";
        private const string ProbeScene = "Assets/Scenes/Stage3RapidRuntime3DProbe.unity";
        private const string Game3DScene = "Assets/Scenes/Stage3Rapid3DGamePilot.unity";
        private const string BaselineCatalog = "Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset";
        private const string SpritePath = Folder + "RapidWatchtowerSprite.png";
        private const string ModelPath = Folder + "RapidWatchtower.fbx";
        private const string PrefabPath = Folder + "RapidWatchtowerSprite.prefab";
        private const string CatalogPath = Folder + "RapidWatchtowerPilotCatalog.asset";
        private const string Game3DPrefabPath = Folder + "RapidWatchtower3DGame.prefab";
        private const string Game3DCatalogPath = Folder + "RapidWatchtower3DGameCatalog.asset";
        private const string ReviewFolder = "art/review";
        private const string Game3DBuild = "Builds/Windows/Not3AStage3RapidPilot.exe";
        internal const string TacticalMapPath = Folder + "Stage3PlanarCorner.asset";
        private const string TacticalZoomPath = Folder + "TacticalZoomSettings.asset";

        // Explicit, short-lived local request for the already open interactive Editor.
        // -executeMethod is not forwarded to an existing Editor by Unity's launcher.
        private const string ReviewRequest = "Artifacts/S3-RAPID-PILOT/open-game.request";
        private const string ReviewPending = "Not3A.Stage3.OpenGamePending";

        [InitializeOnLoadMethod]
        private static void RegisterPlayableReviewRequest()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += ConsumePlayableReviewRequest;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode ||
                    !SessionState.GetBool(ReviewPending, false)) return;
                SessionState.SetBool(ReviewPending, false);
                EditorApplication.ExecuteMenuItem("Window/General/Game");
                SessionState.SetBool("Not3A.Stage3.ZoomInspectorOpened",
                    EditorApplication.ExecuteMenuItem("Window/General/Inspector"));
                Selection.activeObject = Require<Stage2CameraZoomSettings>(TacticalZoomPath);
                // First import of the translucent objective can compile URP variants
                // after Play starts; wait until its materials have rendered.
                double captureAt = EditorApplication.timeSinceStartup + 6;
                void CaptureStartedGame()
                {
                    if (EditorApplication.timeSinceStartup < captureAt) return;
                    EditorApplication.update -= CaptureStartedGame;
                    var scene = SceneManager.GetActiveScene();
                    var cameraFollow = UnityEngine.Object.FindFirstObjectByType<Stage2CameraFollow>();
                    bool ready = EditorApplication.isPlaying && scene.path == Game3DScene &&
                        UnityEngine.Object.FindFirstObjectByType<Stage3TacticalPresentation>() != null &&
                        UnityEngine.Object.FindFirstObjectByType<Stage2Tower>() != null;
                    if (ready && cameraFollow?.ZoomSettings != null)
                    {
                        EditorApplication.ExecuteMenuItem("Window/General/Inspector");
                        Selection.activeObject = cameraFollow.ZoomSettings;
                    }
                    bool tuningVisible = cameraFollow != null && cameraFollow.ZoomSettings != null &&
                        Selection.activeObject == cameraFollow.ZoomSettings;
                    File.WriteAllText("Artifacts/S3-RAPID-PILOT/editor-game-status.txt",
                        $"utc={DateTime.UtcNow:O}\nscene={scene.path}\nplaying={EditorApplication.isPlaying}\nready={ready}\n" +
                        $"zoomSettingsSelected={tuningVisible}\nselectedAsset={AssetDatabase.GetAssetPath(Selection.activeObject)}\n" +
                        $"zoomStep={cameraFollow?.ZoomSettings?.StepMultiplier}\n");
                    if (ready)
                    {
                        ScreenCapture.CaptureScreenshot(Path.GetFullPath("art/review/stage3_editor_game.png"));
                        Debug.Log("S3_EDITOR_GAME_READY: playable scene, tactical presentation and tower verified in the interactive Editor.");
                    }
                }
                EditorApplication.update += CaptureStartedGame;
            };
        }

        private static void ConsumePlayableReviewRequest()
        {
            if (!File.Exists(ReviewRequest)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ConsumePlayableReviewRequest;
                return;
            }
            bool fresh = DateTime.UtcNow - File.GetLastWriteTimeUtc(ReviewRequest) < TimeSpan.FromMinutes(5);
            if (!fresh)
            {
                File.Delete(ReviewRequest);
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.playModeStateChanged += ResumePlayableReviewRequest;
                    EditorApplication.isPlaying = false;
                }
                else EditorApplication.delayCall += ConsumePlayableReviewRequest;
                return;
            }
            File.Delete(ReviewRequest);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build3DGameAssets();
            EditorSceneManager.OpenScene(Game3DScene, OpenSceneMode.Single);
            SessionState.SetBool(ReviewPending, true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        private static void ResumePlayableReviewRequest(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= ResumePlayableReviewRequest;
            EditorApplication.delayCall += ConsumePlayableReviewRequest;
        }

        public static void RebuildRevision()
        {
            BuildAndVerify();
            Build3DGamePlayer();
        }

        [MenuItem("Tools/Stage 3/Build Rapid Playable Windows Pilot")]
        public static void Build3DGamePlayer()
        {
            Build3DGameAssets();
            BuildCurrentGamePlayer();
        }

        // Build the saved playable scene without regenerating its approved camera or models.
        public static void BuildCurrentGamePlayer()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Game3DBuild) ?? "Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { Game3DScene },
                locationPathName = Game3DBuild,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Stage 3 Rapid game build failed: {report.summary.result}");
            Debug.Log($"S3_RAPID_GAME_BUILD_OK path={Game3DBuild} bytes={report.summary.totalSize}");
        }

        [MenuItem("Tools/Stage 3/Open Rapid in Playable Game")]
        public static void Open3DGamePilot()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before opening the pilot game.");
            if (SceneManager.GetActiveScene().isDirty &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build3DGameAssets();
            EditorSceneManager.OpenScene(Game3DScene, OpenSceneMode.Single);
            SessionState.SetBool(ReviewPending, true);
            EditorApplication.delayCall += () =>
            {
                EditorApplication.isPlaying = true;
                Debug.Log("S3_RAPID_GAME_OPEN: playable pilot scene entered Play Mode.");
            };
        }

        [MenuItem("Tools/Stage 3/Open Rapid 3D Model Probe")]
        public static void Open3DProbe()
        {
            if (!File.Exists(ProbeScene))
                throw new InvalidOperationException("Rapid 3D probe scene is missing; build the visual pilot first.");
            EditorSceneManager.OpenScene(ProbeScene, OpenSceneMode.Single);
            EditorApplication.delayCall += () =>
            {
                var tower = GameObject.Find("Rapid Watchtower FBX (runtime 3D geometry probe)");
                if (tower == null) return;
                Selection.activeGameObject = tower;
                SceneView.lastActiveSceneView?.FrameSelected();
                SceneView.RepaintAll();
                Debug.Log("S3_RAPID_3D_PROBE_OPEN: model selected and framed in Scene view.");
            };
        }

        [MenuItem("Tools/Stage 3/Build Rapid Visual Pilot")]
        public static void BuildAndVerify()
        {
            ConfigureAtlasImport();
            ConfigureSpriteImport();
            var sprite = Require<Sprite>(SpritePath);
            Require<GameObject>(ModelPath);
            var baseline = Require<Stage2DefenseCatalog>(BaselineCatalog);
            var prefab = BuildSpritePrefab(sprite);
            BuildPilotCatalog(baseline, prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var catalog = Require<Stage2DefenseCatalog>(CatalogPath);
            if (AssetDatabase.GetAssetPath(catalog) != CatalogPath)
                throw new InvalidOperationException("Pilot catalog has no persistent asset path.");
            BuildSpriteScene(catalog);
            VerifyPlacementAndCapture(catalog);
            Build3DProbe();
            AssetDatabase.SaveAssets();
            Debug.Log("S3_RAPID_PILOT_OK: Blender FBX and sprite imported; isolated sprite scene purchased a tower; 2D and 3D review captures saved.");
        }

        private static T Require<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException($"Required pilot asset missing: {path}");
            return asset;
        }

        private static void ConfigureSpriteImport()
        {
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Rapid sprite PNG did not import as a texture.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = Require<Texture2D>(SpritePath).height / 1.95f;
            var spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            spriteSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            spriteSettings.spritePivot = new Vector2(0.5f, 0.14f);
            importer.SetTextureSettings(spriteSettings);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Stage2Tower BuildSpritePrefab(Sprite sprite)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
                try
                {
                    var renderer = contents.GetComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sharedMaterial = GetOrCreateMaterial("PilotSpriteUnlit.mat", "Sprites/Default", Color.white);
                    PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
                return Require<GameObject>(PrefabPath).GetComponent<Stage2Tower>();
            }
            var root = new GameObject("Rapid Watchtower Visual Pilot");
            try
            {
                root.AddComponent<Stage2Tower>();
                var renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 250;
                renderer.sharedMaterial = GetOrCreateMaterial("PilotSpriteUnlit.mat", "Sprites/Default", Color.white);
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
            return Require<GameObject>(PrefabPath).GetComponent<Stage2Tower>();
        }

        private static Stage2DefenseCatalog BuildPilotCatalog(Stage2DefenseCatalog baseline, Stage2Tower prefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(CatalogPath);
            if (existing != null) return existing;
            var catalog = UnityEngine.Object.Instantiate(baseline);
            catalog.name = "RapidWatchtowerPilotCatalog";
            catalog.Configure(prefab, baseline.WoodCost, baseline.StoneCost, baseline.MetalCost,
                baseline.TowerHealth, baseline.AttackRange, baseline.FireInterval,
                baseline.AttackDamage, baseline.GoldCost);
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }

        internal static void Build3DGameAssets()
        {
            ConfigureAtlasImport();
            ConfigureSpriteImport();
            var prefab = Build3DGamePrefab();
            var longshotCatalog = Stage3LongshotBuilder.BuildCatalog();
            var baseline = Require<Stage2DefenseCatalog>(BaselineCatalog);
            if (AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(Game3DCatalogPath) == null)
            {
                var catalog = UnityEngine.Object.Instantiate(baseline);
                catalog.name = "RapidWatchtower3DGameCatalog";
                catalog.Configure(prefab, baseline.WoodCost, baseline.StoneCost, baseline.MetalCost,
                    baseline.TowerHealth, baseline.AttackRange, baseline.FireInterval,
                    baseline.AttackDamage, baseline.GoldCost);
                AssetDatabase.CreateAsset(catalog, Game3DCatalogPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var gameCatalog = Require<Stage2DefenseCatalog>(Game3DCatalogPath);
            if (AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(TacticalMapPath) == null)
            {
                var map = UnityEngine.Object.Instantiate(Require<Stage2MapDefinition>(
                    "Assets/Stage2/Map/Data/Stage2Corner32x32.asset"));
                map.name = "Stage3PlanarCorner";
                map.UsePlanarGridForEditor();
                AssetDatabase.CreateAsset(map, TacticalMapPath);
            }
            Stage3DiagonalMapBuilder.Build(Require<Stage2MapDefinition>(TacticalMapPath));
            if (AssetDatabase.LoadAssetAtPath<Stage2CameraZoomSettings>(TacticalZoomPath) == null)
            {
                var settings = ScriptableObject.CreateInstance<Stage2CameraZoomSettings>();
                settings.name = "Tactical Zoom Settings";
                AssetDatabase.CreateAsset(settings, TacticalZoomPath);
            }
            CreateTacticalMaterials();
            var cartoonTowers = File.Exists(Stage3CartoonWorldBuilder.Folder+"RapidCartoon.png");
            if (cartoonTowers) Stage3CartoonWorldBuilder.BuildAssets();
            Stage3SoulSphereBuilder.BuildPrefab();
            Stage3EntrancePortalBuilder.BuildPrefab();
            if (Stage3ResourceSpriteBuilder.SourcesPresent) Stage3ResourceSpriteBuilder.BuildPrefabs();
            else Stage3ResourceSourceBuilder.BuildPrefabs();
            if (File.Exists(Stage3SkeletonSpriteBuilder.SheetPath)) Stage3SkeletonSpriteBuilder.BuildPrefab();
            if (File.Exists(Stage3LandmarkSpriteBuilder.PortalTexture)) Stage3LandmarkSpriteBuilder.BuildPrefabs();
            Stage3EntrancePortalBuilder.ReserveFootprint(Require<Stage2MapDefinition>(TacticalMapPath));
            Stage3SoulSphereBuilder.ReserveFootprint(Require<Stage2MapDefinition>(TacticalMapPath));
            AssetDatabase.SaveAssets();
            if (!File.Exists(Game3DScene))
            {
                var source = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
                if (!EditorSceneManager.SaveScene(source, Game3DScene, true))
                    throw new InvalidOperationException("Could not copy the Stage 2 scene for the 3D game pilot.");
            }
            var scene = EditorSceneManager.OpenScene(Game3DScene, OpenSceneMode.Single);
            gameCatalog = Require<Stage2DefenseCatalog>(Game3DCatalogPath);
            longshotCatalog = Require<Stage2DefenseCatalog>(Stage3LongshotBuilder.CatalogPath);
            if (cartoonTowers)
            {
                gameCatalog = Require<Stage2DefenseCatalog>(Stage3CartoonWorldBuilder.RapidCatalog);
                longshotCatalog = Require<Stage2DefenseCatalog>(Stage3CartoonWorldBuilder.LongshotCatalog);
            }
            var composition = UnityEngine.Object.FindAnyObjectByType<Stage2GameComposition>();
            var building = UnityEngine.Object.FindAnyObjectByType<Stage2BuildDriver>();
            if (composition == null || building == null)
                throw new InvalidOperationException("3D game copy has no gameplay composition/build driver.");
            SetReference(composition, "defenseCatalog", gameCatalog);
            SetReference(composition, "longshotCatalog", longshotCatalog);
            SetReference(building, "catalog", gameCatalog);
            if (composition.GetComponent<Stage3RapidShowcase>() == null)
                composition.gameObject.AddComponent<Stage3RapidShowcase>();
            ConfigureTacticalScene(composition, building);
            // The Stage 2 flat sprites needed no light. Scope mesh lighting to this pilot.
            var keyObject = GameObject.Find("Pilot mesh key light");
            if (keyObject == null) keyObject = new GameObject("Pilot mesh key light");
            keyObject.transform.rotation = Quaternion.LookRotation(new Vector3(.45f,.6f,1f), Vector3.up);
            var key = keyObject.GetComponent<Light>();
            if (key == null) key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.92f, 0.82f);
            key.intensity = 1.15f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = .8f;
            key.shadowBias = .015f;
            key.shadowNormalBias = .12f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.43f);
            Stage3SoulSphereBuilder.ConfigureGlow(composition.GetComponent<Stage2SceneBootstrap>().CameraFollow.BoundCamera);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, Game3DScene))
                throw new InvalidOperationException("Could not save the playable 3D pilot scene.");
        }

        private static Stage2Tower Build3DGamePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Game3DPrefabPath);
            var model = Require<GameObject>(ModelPath);
            var sprite = Require<Sprite>(SpritePath);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(Game3DPrefabPath)
                : new GameObject("Rapid Watchtower 3D Game Pilot");
            try
            {
                if (root.GetComponent<Stage2Tower>() == null) root.AddComponent<Stage2Tower>();
                var previewSource = root.GetComponent<SpriteRenderer>();
                if (previewSource == null) previewSource = root.AddComponent<SpriteRenderer>();
                previewSource.sprite = sprite;
                previewSource.sortingOrder = 250;
                previewSource.enabled = false; // Build preview still uses its sprite; paid tower shows FBX.
                for (var index = root.transform.childCount - 1; index >= 0; index--)
                    UnityEngine.Object.DestroyImmediate(root.transform.GetChild(index).gameObject);
                var child = (GameObject)PrefabUtility.InstantiatePrefab(model);
                child.name = "Rapid Watchtower Mesh";
                var visualPivot = new GameObject("Pilot ground-aligned mesh").transform;
                visualPivot.SetParent(root.transform, false);
                visualPivot.localPosition = new Vector3(0f, 0f, -0.03f);
                visualPivot.localRotation = Quaternion.AngleAxis(25f, Vector3.forward) * Quaternion.Euler(-90f, 0f, 0f);
                // Preserve the FBX root's Blender-to-Unity axis conversion.
                child.transform.SetParent(visualPivot, false);
                ApplyPilotMaterials(child);
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, Game3DPrefabPath);
            }
            finally
            {
                if (existing != null) PrefabUtility.UnloadPrefabContents(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
            return Require<GameObject>(Game3DPrefabPath).GetComponent<Stage2Tower>();
        }

        private static void CreateTacticalMaterials()
        {
            ConfigureLandscapeTextureImport();
            foreach (var ground in new[] { "Grass", "Paving" })
            {
                // Original reproducible surface pigments. Bushes and rocks are
                // separate 3D props if approved, never painted into these maps.
                var texture = Require<Texture2D>(Folder + (ground == "Grass"
                    ? "LandscapeGrass.png" : "LandscapeDefenseEarth.png"));
                var shaderName = "Universal Render Pipeline/Lit";
                var material = GetOrCreateMaterial("Tactical"+ground+".mat", shaderName, Color.white);
                var shader = Shader.Find(shaderName);
                if (shader == null) throw new InvalidOperationException($"Pilot shader missing: {shaderName}");
                if (material.shader != shader) material.shader = shader;
                material.SetTexture("_BaseMap",texture);
                material.SetTextureScale("_BaseMap", ground == "Grass"
                    ? new Vector2(.5f,.5f) : Vector2.one);
                material.SetFloat("_Smoothness", .08f);
                EditorUtility.SetDirty(material);
            }
            GetOrCreateMaterial("TacticalNeutral.mat","Universal Render Pipeline/Lit",Color.white);
            GetOrCreateMaterial("TacticalHero.mat","Universal Render Pipeline/Lit",new Color(.12f,.72f,.85f));
            GetOrCreateMaterial("TacticalThrone.mat","Universal Render Pipeline/Lit",new Color(.88f,.57f,.12f));
            GetOrCreateMaterial("TacticalEntrance.mat","Universal Render Pipeline/Lit",new Color(.8f,.20f,.055f));
            GetOrCreateMaterial("TacticalWood.mat","Universal Render Pipeline/Lit",new Color(.48f,.27f,.09f));
            GetOrCreateMaterial("TacticalStone.mat","Universal Render Pipeline/Lit",new Color(.55f,.60f,.64f));
            GetOrCreateMaterial("TacticalMetal.mat","Universal Render Pipeline/Lit",new Color(.28f,.37f,.50f));
            GetOrCreateMaterial("LandscapeGrassBlades.mat","Universal Render Pipeline/Lit",new Color(.32f,.57f,.12f));
            GetOrCreateMaterial("LandscapeFlowerPetals.mat","Universal Render Pipeline/Lit",new Color(.94f,.94f,.82f));
            GetOrCreateMaterial("LandscapeFlowerCenters.mat","Universal Render Pipeline/Lit",new Color(.94f,.68f,.14f));
        }

        // Keep the source brushwork sharp at the tactical zoom; Unity's default
        // non-power-of-two scaling and block compression blur the painted grass.
        public static void ConfigureLandscapeTextureImport()
        {
            foreach (var name in new[] { "LandscapeGrass.png", "LandscapeDefenseEarth.png" })
            {
                var importer = AssetImporter.GetAtPath(Folder + name) as TextureImporter;
                if (importer == null) throw new InvalidOperationException($"Missing landscape texture: {name}");
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureTacticalScene(Stage2GameComposition composition, Stage2BuildDriver building)
        {
            var bootstrap = composition.GetComponent<Stage2SceneBootstrap>();
            var map = Require<Stage2MapDefinition>(TacticalMapPath);
            var mover = bootstrap.PlayerMover;
            var camera = bootstrap.CameraFollow;
            bootstrap.MapDebugRenderer.Configure(map);
            mover.Configure(map,mover.Speed);
            mover.ConfigurePlanarEdgeClearance(1f);
            mover.transform.position = map.CellToWorld(Stage3DiagonalMapBuilder.HeroStart);
            camera.Configure(mover.transform,map,7f);
            camera.ConfigureTacticalView(5f);
            camera.ConfigureTacticalZoom(Require<Stage2CameraZoomSettings>(TacticalZoomPath));
            camera.BoundCamera.farClipPlane = 80f;
            bootstrap.Configure(map,bootstrap.MapDebugRenderer,mover,camera,
                bootstrap.EntranceMarker,bootstrap.ThroneMarker);
            bootstrap.EntranceMarker.localScale = Vector3.one;
            bootstrap.ThroneMarker.localScale = Vector3.one;
            SetReference(building,"mapDefinition",map);
            var presentation = composition.GetComponent<Stage3TacticalPresentation>();
            if (presentation == null) presentation = composition.gameObject.AddComponent<Stage3TacticalPresentation>();
            var properties = new[] { "grass", "paving", "neutral", "hero", "throne", "entrance", "woodSource", "stoneSource", "metalSource" };
            var names = new[] { "Grass", "Paving", "Neutral", "Hero", "Throne", "Entrance", "Wood", "Stone", "Metal" };
            for (var i=0; i<properties.Length; i++)
                SetReference(presentation,properties[i],Require<Material>(Folder+"Tactical"+names[i]+".mat"));
            SetReference(presentation,"grassBlades",Require<Material>(Folder+"LandscapeGrassBlades.mat"));
            if (File.Exists(Stage3CartoonWorldBuilder.Folder+"GrassVerge.mat"))
                SetReference(presentation,"grassVerge",Require<Material>(Stage3CartoonWorldBuilder.Folder+"GrassVerge.mat"));
            if (File.Exists(Stage3CartoonWorldBuilder.Folder+"RoadsideArt.asset"))
                SetReference(presentation,"roadsideArt",Require<Stage3RoadsideArt>(Stage3CartoonWorldBuilder.Folder+"RoadsideArt.asset"));
            SetReference(presentation,"flowerPetals",Require<Material>(Folder+"LandscapeFlowerPetals.mat"));
            SetReference(presentation,"flowerCenters",Require<Material>(Folder+"LandscapeFlowerCenters.mat"));
            SetReference(presentation,"soulSpherePrefab",Require<GameObject>(File.Exists(Stage3LandmarkSpriteBuilder.SoulTexture)
                ? Stage3LandmarkSpriteBuilder.SoulPrefab : Stage3SoulSphereBuilder.PrefabPath));
            SetReference(presentation,"entrancePortalPrefab",Require<GameObject>(File.Exists(Stage3LandmarkSpriteBuilder.PortalTexture)
                ? Stage3LandmarkSpriteBuilder.PortalPrefab : Stage3EntrancePortalBuilder.PrefabPath));
            if (Stage3ResourceSpriteBuilder.SourcesPresent) Stage3ResourceSpriteBuilder.Wire(presentation);
            else
            {
                SetReference(presentation,"woodSourceModel",Require<GameObject>(Stage3ResourceSourceBuilder.WoodPrefabPath));
                SetReference(presentation,"stoneSourceModel",Require<GameObject>(Stage3ResourceSourceBuilder.StonePrefabPath));
                SetReference(presentation,"metalSourceModel",Require<GameObject>(Stage3ResourceSourceBuilder.IronOrePrefabPath));
            }
            SetReference(presentation,"skeletonSpritePrefab",AssetDatabase.LoadAssetAtPath<Stage3SkeletonSprite>(Stage3SkeletonSpriteBuilder.PrefabPath));
            EditorUtility.SetDirty(bootstrap);
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(mover);
        }

        private static void BuildSpriteScene(Stage2DefenseCatalog catalog)
        {
            if (!File.Exists(SourceScene)) throw new InvalidOperationException("Stage 2 source scene is missing.");
            if (!File.Exists(PilotScene))
            {
                var source = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
                if (!EditorSceneManager.SaveScene(source, PilotScene, true))
                    throw new InvalidOperationException("Could not copy the Stage 2 scene for visual trial.");
            }
            var scene = EditorSceneManager.OpenScene(PilotScene, OpenSceneMode.Single);
            catalog = Require<Stage2DefenseCatalog>(CatalogPath);
            var composition = UnityEngine.Object.FindAnyObjectByType<Stage2GameComposition>();
            var building = UnityEngine.Object.FindAnyObjectByType<Stage2BuildDriver>();
            if (composition == null || building == null)
                throw new InvalidOperationException("Pilot copy has no gameplay composition/build driver.");
            SetReference(composition, "defenseCatalog", catalog);
            SetReference(building, "catalog", catalog);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, PilotScene))
                throw new InvalidOperationException("Could not save the isolated pilot scene.");
        }

        private static void SetReference(UnityEngine.Object owner, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(owner);
            var property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"Missing serialized field {propertyName}");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(owner);
            serialized.Update();
            if (serialized.FindProperty(propertyName).objectReferenceValue != value)
                throw new InvalidOperationException($"Could not bind {propertyName} on {owner.name}.");
        }

        private static void VerifyPlacementAndCapture(Stage2DefenseCatalog catalog)
        {
            EditorSceneManager.OpenScene(PilotScene, OpenSceneMode.Single);
            catalog = Require<Stage2DefenseCatalog>(CatalogPath);
            var bootstrap = UnityEngine.Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var composition = UnityEngine.Object.FindAnyObjectByType<Stage2GameComposition>();
            if (bootstrap == null || composition == null || !bootstrap.Initialize() || !composition.Initialize())
                throw new InvalidOperationException("Pilot game scene did not initialize.");
            if (composition.Building.Defense.SelectedCatalog != catalog)
                throw new InvalidOperationException("Pilot scene is not using its pilot catalog.");
            var inventory = composition.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood, catalog.WoodCost);
            inventory.TryAdd(MapResourceKind.Stone, catalog.StoneCost);
            inventory.TryAdd(MapResourceKind.Metal, catalog.MetalCost);
            inventory.TryAddGold(catalog.GoldCost);
            composition.Building.SetRunState(Stage2BuildPhase.Preparation, false);
            var defense = composition.Building.Defense;
            if (!defense.BeginPlacement()) throw new InvalidOperationException("Pilot placement did not start.");
            var map = bootstrap.MapDefinition;
            var candidate = new Vector2Int(-9, -11);
            defense.SetPreviewCell(candidate);
            if (defense.EvaluatePreview() != PlacementResult.Ready)
                throw new InvalidOperationException($"Pilot cell {candidate} is unavailable: {defense.EvaluatePreview()}");
            if (defense.TryConfirm(out var tower) != PlacementResult.Ready || tower == null ||
                defense.TowerCount != 1 || tower.GetComponent<SpriteRenderer>()?.sprite != Require<Sprite>(SpritePath))
                throw new InvalidOperationException("Pilot tower purchase or art binding failed.");
            var camera = bootstrap.CameraFollow.BoundCamera;
            var center = map.CellToWorld(candidate);
            camera.transform.position = new Vector3(center.x, center.y + 0.2f, -10f);
            Capture(camera, ReviewFolder + "/rapid_sprite_in_stage2.png");
            defense.ResetRun();
        }

        private static void Build3DProbe()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var importedModel = Require<GameObject>(ModelPath);
            var tower = (GameObject)PrefabUtility.InstantiatePrefab(importedModel);
            tower.name = "Rapid Watchtower FBX (runtime 3D geometry probe)";
            tower.transform.position = Vector3.zero;
            ApplyPilotMaterials(tower);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ground.name = "One-cell ground scale reference";
            ground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ground.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
            ground.transform.position = new Vector3(0f, -0.015f, 0f);
            var groundMaterial = GetOrCreateMaterial("PilotGrayGround.mat", "Universal Render Pipeline/Lit",
                new Color(0.34f, 0.37f, 0.38f));
            groundMaterial.SetTexture("_BaseMap", Texture2D.whiteTexture);
            EditorUtility.SetDirty(groundMaterial);
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMaterial;

            var lightObject = new GameObject("Soft directional light");
            lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;

            var cameraObject = new GameObject("Pilot Orthographic 3D Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 1.55f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.09f);
            cameraObject.transform.position = new Vector3(3.5f, 3.1f, -4.5f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            cameraObject.AddComponent<AudioListener>();
            if (!EditorSceneManager.SaveScene(scene, ProbeScene))
                throw new InvalidOperationException("Could not save runtime 3D probe scene.");
            Capture(camera, ReviewFolder + "/rapid_runtime3d_probe.png");
        }

        private static void ConfigureAtlasImport()
        {
            var atlasPath = Folder + "RapidWatchtowerAlbedo.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void ApplyPilotMaterials(GameObject tower)
        {
            var atlasPath = Folder + "RapidWatchtowerAlbedo.png";
            var material = GetOrCreateMaterial("PilotWatchtowerAtlas.mat", "Universal Render Pipeline/Lit", Color.white);
            material.SetTexture("_BaseMap", Require<Texture2D>(atlasPath));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.23f);
            EditorUtility.SetDirty(material);
            foreach (var renderer in tower.GetComponentsInChildren<MeshRenderer>())
                renderer.sharedMaterials = new[] { material };
        }

        private static Material GetOrCreateMaterial(string filename, string shaderName, Color color)
        {
            var path = Folder + filename;
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException($"Pilot shader missing: {shaderName}");
            var material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void Capture(Camera camera, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ReviewFolder);
            var target = new RenderTexture(960, 600, 24);
            var texture = new Texture2D(960, 600, TextureFormat.RGBA32, false);
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 960, 600), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
