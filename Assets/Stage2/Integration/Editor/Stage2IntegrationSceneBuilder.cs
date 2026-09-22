using System;
using System.IO;
using System.Linq;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Not3A.Stage2.Integration.Editor
{
    public static class Stage2IntegrationSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Stage2VerticalSlice.unity";
        public const string Stage1ScenePath = "Assets/Scenes/Stage1Graybox.unity";
        public const string MapAssetPath = "Assets/Stage2/Map/Data/Stage2Map100x100.asset";
        public const string BuildPath = "Builds/Windows/Not3AStage2.exe";

        private const string SolidSpriteAssetPath = "Assets/Stage1/Generated/GrayboxSolid.asset";
        private const string DiamondSpriteAssetPath = "Assets/Stage1/Generated/GrayboxDiamond.asset";

        [MenuItem("Tools/Stage 2/Integration/Rebuild Scene Shell")]
        public static void RebuildSceneShell()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                BuildScene(true);
            }
        }

        [MenuItem("Tools/Stage 2/Integration/Open Vertical Slice Scene")]
        public static void OpenVerticalSliceScene()
        {
            if (!File.Exists(ScenePath))
            {
                BuildScene(true);
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("Tools/Stage 2/Integration/Build Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            if (!File.Exists(ScenePath))
            {
                BuildScene(false);
            }

            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (enabledScenes.Length == 0 || enabledScenes[0] != ScenePath)
            {
                throw new InvalidOperationException($"Stage 2 must be the first enabled build scene. Found: {string.Join(", ", enabledScenes)}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(BuildPath) ?? "Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Stage 2 Windows build failed: {report.summary.result}");
            }

            Debug.Log($"S2_INT_BUILD_OK path={BuildPath} bytes={report.summary.totalSize} scenes={string.Join(" -> ", enabledScenes)}");
        }

        private static void BuildScene(bool openAfterBuild)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Stop Play Mode before rebuilding the Stage 2 scene.");
            }

            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index).isDirty)
                {
                    throw new InvalidOperationException("Save or close modified scenes before rebuilding the Stage 2 scene.");
                }
            }

            var mapDefinition = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(MapAssetPath);
            if (mapDefinition == null)
            {
                throw new InvalidOperationException($"Required Stage 2 map asset is missing at {MapAssetPath}.");
            }

            var validation = mapDefinition.ValidateDefinition();
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Stage 2 map asset is invalid: {validation.Issues[0]}");
            }

            var solidSprite = LoadSprite(SolidSpriteAssetPath);
            var diamondSprite = LoadSprite(DiamondSpriteAssetPath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage2VerticalSlice";

            var mapObject = new GameObject("Authored 100x100 Map Surface", typeof(MeshFilter), typeof(MeshRenderer));
            var debugRenderer = mapObject.AddComponent<Stage2MapDebugRenderer>();
            debugRenderer.Configure(mapDefinition);

            var entranceMarker = CreateEntranceMarker(mapDefinition, diamondSprite);
            var throneMarker = CreateThroneMarker(mapDefinition, solidSprite);
            var player = CreatePlayer(mapDefinition, solidSprite);
            var cameraFollow = CreateCamera(player.transform, mapDefinition);

            var compositionObject = new GameObject("Stage 2 Scene Composition");
            var bootstrap = compositionObject.AddComponent<Stage2SceneBootstrap>();
            bootstrap.Configure(
                mapDefinition,
                debugRenderer,
                player.GetComponent<Stage2PlayerMover>(),
                cameraFollow,
                entranceMarker.transform,
                throneMarker.transform);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save Stage 2 integration scene at {ScenePath}.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(Stage1ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = compositionObject;

            if (openAfterBuild)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Debug.Log(
                $"S2_INT_SCENE_OK path={ScenePath} map={MapAssetPath} " +
                $"size={mapDefinition.Size.x}x{mapDefinition.Size.y} hash={mapDefinition.ComputeDeterministicContentHash()} " +
                $"buildOrder={ScenePath} -> {Stage1ScenePath}");
        }

        private static GameObject CreateEntranceMarker(Stage2MapDefinition definition, Sprite diamondSprite)
        {
            var root = new GameObject("Enemy Entrance Marker");
            root.transform.position = WithZ(definition.CellToWorld(definition.EnemyEntrance), 0f);
            CreateSpriteChild(
                root.transform,
                "Orange Entrance Diamond",
                diamondSprite,
                new Color(0.95f, 0.3f, 0.12f, 1f),
                new Vector2(1.45f, 1.45f),
                Vector2.zero,
                100);
            return root;
        }

        private static GameObject CreateThroneMarker(Stage2MapDefinition definition, Sprite solidSprite)
        {
            var root = new GameObject("Throne Marker");
            root.transform.position = WithZ(definition.CellToWorld(definition.Throne), 0f);
            CreateSpriteChild(
                root.transform,
                "Gold Throne Block",
                solidSprite,
                new Color(0.95f, 0.7f, 0.14f, 1f),
                new Vector2(1.25f, 2f),
                new Vector2(0f, 1f),
                200);
            return root;
        }

        private static GameObject CreatePlayer(Stage2MapDefinition definition, Sprite solidSprite)
        {
            var root = new GameObject("Hero Marker (Non-Combat)");
            var spawnCell = FindPlayerSpawn(definition);
            root.transform.position = WithZ(definition.CellToWorld(spawnCell), 0f);
            CreateSpriteChild(
                root.transform,
                "Cyan Non-Combat Hero Block",
                solidSprite,
                new Color(0.18f, 0.8f, 0.9f, 1f),
                new Vector2(0.66f, 1.14f),
                new Vector2(0f, 0.57f),
                400);
            var mover = root.AddComponent<Stage2PlayerMover>();
            mover.Configure(definition, 4.2f);
            return root;
        }

        private static Stage2CameraFollow CreateCamera(Transform target, Stage2MapDefinition definition)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(target.position.x, target.position.y, -10f);
            cameraObject.transform.rotation = Quaternion.identity;
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.08f, 1f);
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<Stage2CameraFollow>();
            follow.Configure(target, definition, 7f);
            return follow;
        }

        private static Vector2Int FindPlayerSpawn(Stage2MapDefinition definition)
        {
            var origin = definition.Throne;
            var maximumRadius = Mathf.Max(definition.Size.x, definition.Size.y);
            var directions = new[] { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
            for (var radius = 1; radius <= maximumRadius; radius++)
            {
                for (var index = 0; index < directions.Length; index++)
                {
                    var candidate = origin + directions[index] * radius;
                    if (definition.Contains(candidate) &&
                        candidate != definition.EnemyEntrance &&
                        definition.IsWalkable(candidate) &&
                        definition.GetSurface(candidate) == MapSurfaceKind.Forest)
                    {
                        return candidate;
                    }
                }
            }

            throw new InvalidOperationException("The authored Stage 2 map has no walkable forest cell for the temporary player marker.");
        }

        private static Sprite LoadSprite(string assetPath)
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
            {
                throw new InvalidOperationException($"Required Stage 1 regression sprite is missing at {assetPath}.");
            }

            return sprite;
        }

        private static SpriteRenderer CreateSpriteChild(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = WithZ(localPosition, 0f);
            child.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Vector3 WithZ(Vector2 value, float z)
        {
            return new Vector3(value.x, value.y, z);
        }
    }
}
