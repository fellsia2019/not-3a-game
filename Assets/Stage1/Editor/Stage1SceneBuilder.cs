using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Not3A.Stage1;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Not3A.Stage1.Editor
{
    [InitializeOnLoad]
    public static class Stage1SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Stage1Graybox.unity";
        private const string GeneratedFolder = "Assets/Stage1/Generated";
        private const string PrefabFolder = "Assets/Stage1/Prefabs";
        private const string DiamondAssetPath = GeneratedFolder + "/GrayboxDiamond.asset";
        private const string SolidAssetPath = GeneratedFolder + "/GrayboxSolid.asset";
        private const string EnemyPrefabPath = PrefabFolder + "/Enemy.prefab";
        private const string TowerPrefabPath = PrefabFolder + "/Tower.prefab";
        private const string BuildPath = "Builds/Windows/Not3AGraybox.exe";
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string AutoOpenSessionKey = "Not3A.Stage1.GrayboxAutoOpened";
        private static readonly Vector2Int MapMinimumCell = new Vector2Int(-10, -8);
        private static readonly Vector2Int MapMaximumCell = new Vector2Int(10, 8);
        private static readonly Vector2 MovementMinimumCell = new Vector2(-9.35f, -7.35f);
        private static readonly Vector2 MovementMaximumCell = new Vector2(9.35f, 7.35f);
        private static readonly Vector2Int PathMinimumCell = new Vector2Int(0, -2);
        private static readonly Vector2Int PathMaximumCell = new Vector2Int(9, 2);
        private static readonly Vector2Int EntranceCell = new Vector2Int(9, 0);
        private static readonly Vector2Int ThroneCell = Vector2Int.zero;

        private static Sprite solidSprite;
        private static Sprite diamondSprite;

        static Stage1SceneBuilder()
        {
            EditorApplication.delayCall += EnsureSceneExists;
        }

        [MenuItem("Tools/Stage 1/Rebuild Graybox Scene")]
        public static void RebuildScene()
        {
            BuildScene(true);
        }

        [MenuItem("Tools/Stage 1/Open Graybox Scene", priority = 0)]
        public static void OpenGrayboxScene()
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

        [MenuItem("Tools/Stage 1/Build Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            if (!File.Exists(ScenePath))
            {
                BuildScene(false);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(BuildPath) ?? "Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Stage 1 Windows build failed: {report.summary.result}");
            }

            Debug.Log($"STAGE1_BUILD_OK path={BuildPath} bytes={report.summary.totalSize}");
        }

        private static void EnsureSceneExists()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += EnsureSceneExists;
                return;
            }

            if (!File.Exists(ScenePath))
            {
                BuildScene(false);
            }

            OpenGrayboxFromUntouchedSampleScene();
        }

        private static void OpenGrayboxFromUntouchedSampleScene()
        {
            if (Application.isBatchMode || SessionState.GetBool(AutoOpenSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(AutoOpenSessionKey, true);
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == SampleScenePath && !activeScene.isDirty)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log("STAGE1_SCENE_OPENED: replaced the untouched SampleScene view with Stage1Graybox.");
            }
        }

        private static void BuildScene(bool openAfterBuild)
        {
            EnsureFolder("Assets/Stage1", "Generated");
            EnsureFolder("Assets/Stage1", "Prefabs");
            solidSprite = CreateOrLoadSolidSprite();
            diamondSprite = CreateOrLoadDiamondSprite();

            var enemyPrefab = CreateEnemyPrefab();
            var towerPrefab = CreateTowerPrefab();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage1Graybox";

            CreateMapTiles();

            CreateZoneMarker("Enemy Entrance", IsoGrid.CellToWorld(EntranceCell), new Vector2(1.3f, 1.3f), new Color(0.9f, 0.23f, 0.16f, 0.9f), -700);

            var controllerObject = new GameObject("Stage 1 Game");
            var controller = controllerObject.AddComponent<Stage1GameController>();

            var throne = CreateThrone();
            var nodes = CreateResourceNodes();
            var navigationObject = new GameObject("Dynamic Enemy Navigation");
            var navigation = navigationObject.AddComponent<DynamicNavigationGrid>();
            navigation.Configure(
                new Vector2Int(-9, -7),
                new Vector2Int(9, 7),
                PathMinimumCell,
                PathMaximumCell,
                EntranceCell,
                ThroneCell,
                nodes.Select(node => IsoGrid.WorldToCell(node.transform.position)));
            var player = CreatePlayer(controller);
            var camera = CreateCamera(player.transform);
            var hud = CreateHud();

            var spawnerObject = new GameObject("One Wave Spawner");
            var spawner = spawnerObject.AddComponent<WaveSpawner>();

            var placementObject = new GameObject("Tower Placement");
            var placement = placementObject.AddComponent<BuildPlacement>();
            var ghostObject = CreateSpriteObject("Tower Ghost", Vector2.zero, diamondSprite, new Color(0.2f, 1f, 0.45f, 0.65f), new Vector2(1.15f, 1.15f), 800);
            var ghost = ghostObject.GetComponent<SpriteRenderer>();
            ghostObject.SetActive(false);

            placement.Configure(
                controller,
                hud,
                camera,
                navigation,
                towerPrefab,
                ghost,
                2,
                1);
            spawner.Configure(controller, enemyPrefab, navigation, throne, 6, 0.8f, 2.2f, 1);
            controller.Configure(hud, throne, spawner, placement, 180f, 5, 2, 0, 0);

            var gatherer = player.GetComponent<PlayerGatherer>();
            var toolMarker = player.transform.Find("Context Tool").GetComponent<SpriteRenderer>();
            gatherer.Configure(controller, placement, nodes, toolMarker, 1.55f);

            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = controllerObject;
            if (openAfterBuild)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Debug.Log("STAGE1_SCENE_OK: Stage1Graybox scene, prefabs, explicit references, and build settings generated.");
        }

        private static EnemyController CreateEnemyPrefab()
        {
            var root = new GameObject("Enemy");
            var body = CreateChildSprite(root.transform, "Red Enemy Block", solidSprite, new Color(0.9f, 0.2f, 0.14f), new Vector2(0.66f, 1f), new Vector2(0f, 0.5f));
            var health = root.AddComponent<HealthComponent>();
            health.Configure(8);
            var enemy = root.AddComponent<EnemyController>();
            var sorting = root.AddComponent<IsoSorting>();
            sorting.Configure(300, new[] { body });
            var saved = PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            Object.DestroyImmediate(root);
            return saved.GetComponent<EnemyController>();
        }

        private static TowerController CreateTowerPrefab()
        {
            var root = new GameObject("Tower");
            var body = CreateChildSprite(root.transform, "Blue Tower Block", solidSprite, new Color(0.2f, 0.55f, 0.86f), new Vector2(0.86f, 1.4f), new Vector2(0f, 0.7f));
            var flash = CreateChildSprite(root.transform, "Shot Flash", solidSprite, new Color(1f, 0.88f, 0.16f), new Vector2(0.22f, 0.22f), new Vector2(0.56f, 1.2f));
            flash.enabled = false;
            AddBlockingCollider(root, new Vector2(0.86f, 0.5f), new Vector2(0f, 0.25f));
            var health = root.AddComponent<HealthComponent>();
            health.Configure(5);
            var tower = root.AddComponent<TowerController>();
            tower.Configure(4.25f, 0.6f, 2, flash);
            var sorting = root.AddComponent<IsoSorting>();
            sorting.Configure(250, new[] { body, flash });
            var saved = PrefabUtility.SaveAsPrefabAsset(root, TowerPrefabPath);
            Object.DestroyImmediate(root);
            return saved.GetComponent<TowerController>();
        }

        private static void CreateMapTiles()
        {
            var root = new GameObject("Isometric Ground Grid");
            for (var x = MapMinimumCell.x; x <= MapMaximumCell.x; x++)
            {
                for (var y = MapMinimumCell.y; y <= MapMaximumCell.y; y++)
                {
                    var world = IsoGrid.CellToWorld(new Vector2Int(x, y));
                    var isBoundary = x == MapMinimumCell.x || x == MapMaximumCell.x || y == MapMinimumCell.y || y == MapMaximumCell.y;
                    var isPath = x >= PathMinimumCell.x && x <= PathMaximumCell.x && y >= PathMinimumCell.y && y <= PathMaximumCell.y;
                    var shade = isBoundary
                        ? new Color(0.42f, 0.57f, 0.34f)
                        : isPath
                            ? ((x + y) % 2 == 0 ? new Color(0.34f, 0.37f, 0.38f) : new Color(0.28f, 0.31f, 0.32f))
                        : (x + y) % 2 == 0
                            ? new Color(0.19f, 0.31f, 0.2f)
                            : new Color(0.145f, 0.25f, 0.16f);
                    var scale = isBoundary ? Vector2.one * 1.04f : Vector2.one * 0.97f;
                    var sortingOrder = isBoundary ? -990 : -1000;
                    var tile = CreateSpriteObject($"Tile {x},{y}", world, diamondSprite, shade, scale, sortingOrder);
                    tile.transform.SetParent(root.transform, true);
                }
            }
        }

        private static FixedRoute CreateRoute(Vector2[] points)
        {
            var root = new GameObject("Fixed Enemy Route");
            var route = root.AddComponent<FixedRoute>();
            route.Configure(points);
            for (var index = 1; index < points.Length; index++)
            {
                CreateRouteSegment(root.transform, points[index - 1], points[index], index);
            }

            for (var index = 0; index < points.Length; index++)
            {
                var marker = CreateSpriteObject($"Waypoint {index}", points[index], solidSprite, new Color(0.82f, 0.5f, 0.18f, 0.85f), new Vector2(0.34f, 0.34f), -490);
                marker.transform.SetParent(root.transform, true);
            }

            return route;
        }

        private static void CreateRouteSegment(Transform parent, Vector2 start, Vector2 end, int index)
        {
            var delta = end - start;
            var segment = CreateSpriteObject(
                $"Route Segment {index}",
                (start + end) * 0.5f,
                solidSprite,
                new Color(0.72f, 0.39f, 0.13f, 0.62f),
                new Vector2(delta.magnitude, 0.42f),
                -500);
            segment.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            segment.transform.SetParent(parent, true);
        }

        private static HealthComponent CreateThrone()
        {
            var root = new GameObject("Throne");
            var body = CreateChildSprite(root.transform, "Gold Throne Block", solidSprite, new Color(0.94f, 0.68f, 0.16f), new Vector2(1.22f, 1.9f), new Vector2(0f, 0.95f));
            AddBlockingCollider(root, new Vector2(1.1f, 0.62f), new Vector2(0f, 0.31f));
            var health = root.AddComponent<HealthComponent>();
            health.Configure(5);
            var sorting = root.AddComponent<IsoSorting>();
            sorting.Configure(200, new[] { body });
            return health;
        }

        private static GatherableNode[] CreateResourceNodes()
        {
            var nodes = new List<GatherableNode>();
            var treePositions = new[]
            {
                IsoGrid.CellToWorld(new Vector2Int(-7, 0)),
                IsoGrid.CellToWorld(new Vector2Int(-6, 2)),
                IsoGrid.CellToWorld(new Vector2Int(-5, 4)),
                IsoGrid.CellToWorld(new Vector2Int(-3, 5))
            };
            foreach (var position in treePositions)
            {
                nodes.Add(CreateResourceNode(ResourceKind.Wood, position, 3));
            }

            var stonePositions = new[]
            {
                IsoGrid.CellToWorld(new Vector2Int(-8, -1)),
                IsoGrid.CellToWorld(new Vector2Int(-4, 6)),
                IsoGrid.CellToWorld(new Vector2Int(-2, 6))
            };
            foreach (var position in stonePositions)
            {
                nodes.Add(CreateResourceNode(ResourceKind.Stone, position, 3));
            }

            return nodes.ToArray();
        }

        private static GatherableNode CreateResourceNode(ResourceKind kind, Vector2 position, int units)
        {
            var isWood = kind == ResourceKind.Wood;
            var root = new GameObject(isWood ? "Tree" : "Stone");
            root.transform.position = new Vector3(position.x, position.y, 0f);
            var renderers = new List<SpriteRenderer>();
            if (isWood)
            {
                renderers.Add(CreateChildSprite(root.transform, "Green Tree Block", solidSprite, new Color(0.2f, 0.68f, 0.26f), new Vector2(1f, 1.8f), new Vector2(0f, 0.9f)));
                AddBlockingCollider(root, new Vector2(0.9f, 0.56f), new Vector2(0f, 0.28f));
            }
            else
            {
                renderers.Add(CreateChildSprite(root.transform, "Gray Stone Block", solidSprite, new Color(0.58f, 0.64f, 0.68f), new Vector2(1.08f, 0.68f), new Vector2(0f, 0.34f)));
                AddBlockingCollider(root, new Vector2(1.02f, 0.58f), new Vector2(0f, 0.29f));
            }

            var node = root.AddComponent<GatherableNode>();
            node.Configure(kind, units, isWood ? 0.72f : 0.92f);
            var sorting = root.AddComponent<IsoSorting>();
            sorting.Configure(100, renderers.ToArray());
            return node;
        }

        private static GameObject CreatePlayer(Stage1GameController controller)
        {
            var root = new GameObject("Hero (Non-Combat)");
            root.transform.position = new Vector3(-1.1f, -1.5f, 0f);
            var body = CreateChildSprite(root.transform, "Cyan Hero Block", solidSprite, new Color(0.2f, 0.78f, 0.88f), new Vector2(0.66f, 1.14f), new Vector2(0f, 0.57f));
            var marker = CreateChildSprite(root.transform, "Context Tool", solidSprite, new Color(0.83f, 0.55f, 0.24f), new Vector2(0.18f, 0.72f), new Vector2(0.5f, 0.62f));
            marker.enabled = false;
            var mover = root.AddComponent<PlayerMover>();
            mover.ConfigureIsoBounds(controller, 4.2f, MovementMinimumCell, MovementMaximumCell);
            root.AddComponent<PlayerGatherer>();
            var sorting = root.AddComponent<IsoSorting>();
            sorting.Configure(400, new[] { body, marker });
            return root;
        }

        private static Camera CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.1f, 0.11f);
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<IsometricCameraFollow>();
            follow.Configure(target, new Vector2(-4.6f, 4.6f), new Vector2(-2f, 2f), 7f);
            return camera;
        }

        private static Stage1Hud CreateHud()
        {
            var canvasObject = new GameObject("Graybox HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateUiPanel(canvasObject.transform, "Top Bar", new Vector2(0f, 0.86f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.03f, 0.05f, 0.06f, 0.86f));
            var resources = CreateUiText(canvasObject.transform, "Resources", "Дерево: 0    Камень: 0", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(500f, 54f), 30, TextAnchor.MiddleLeft);
            var phase = CreateUiText(canvasObject.transform, "Phase", "Подготовка", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(700f, 54f), 31, TextAnchor.MiddleCenter);
            var objective = CreateUiText(canvasObject.transform, "Objective", "Задача: добыть 5 дерева (0/5)", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(850f, 46f), 25, TextAnchor.MiddleCenter);
            objective.color = new Color(1f, 0.88f, 0.35f);
            var throne = CreateUiText(canvasObject.transform, "Throne", "Трон: 5 / 5", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(340f, 54f), 30, TextAnchor.MiddleRight);

            var contextPanel = CreateUiPanel(canvasObject.transform, "Context Bar", new Vector2(0.12f, 0f), new Vector2(0.88f, 0f), new Vector2(0f, 0f), new Vector2(0f, 116f), new Color(0.03f, 0.05f, 0.06f, 0.84f));
            var context = CreateUiText(contextPanel.transform, "Controls And Context", string.Empty, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 25, TextAnchor.MiddleCenter);
            context.rectTransform.offsetMin = new Vector2(12f, 8f);
            context.rectTransform.offsetMax = new Vector2(-12f, -8f);

            var resultPanel = CreateUiPanel(canvasObject.transform, "Run Result", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.015f, 0.025f, 0.03f, 0.9f));
            var result = CreateUiText(resultPanel.transform, "Result Text", "ПОБЕДА", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(700f, 260f), 58, TextAnchor.MiddleCenter);
            var restart = CreateUiButton(resultPanel.transform, "Restart", "НАЧАТЬ ЗАНОВО", new Vector2(0f, -105f));
            resultPanel.SetActive(false);

            var hud = canvasObject.AddComponent<Stage1Hud>();
            hud.Configure(resources, phase, objective, throne, context, resultPanel, result, restart);
            return hud;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static GameObject CreateZoneMarker(string name, Vector2 position, Vector2 size, Color color, int order)
        {
            return CreateSpriteObject(name, position, diamondSprite, color, size, order);
        }

        private static GameObject CreateSpriteObject(string name, Vector2 position, Sprite sprite, Color color, Vector2 scale, int order)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.position = new Vector3(position.x, position.y, 0f);
            gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return gameObject;
        }

        private static SpriteRenderer CreateChildSprite(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            float rotation = 0f)
        {
            var child = CreateSpriteObject(name, Vector2.zero, sprite, color, scale, 0);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            child.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            return child.GetComponent<SpriteRenderer>();
        }

        private static BoxCollider2D AddBlockingCollider(GameObject root, Vector2 size, Vector2 offset)
        {
            var collider = root.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.offset = offset;
            collider.isTrigger = false;
            return collider;
        }

        private static GameObject CreateUiPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            var image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static Text CreateUiText(
            Transform parent,
            string name,
            string value,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 position,
            Vector2 size,
            int fontSize,
            TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateUiButton(Transform parent, string name, string label, Vector2 position)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(390f, 82f);
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.5f, 0.78f, 1f);
            var text = CreateUiText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private static Sprite CreateOrLoadSolidSprite()
        {
            var existing = AssetDatabase.LoadAllAssetsAtPath(SolidAssetPath).OfType<Sprite>().FirstOrDefault();
            if (existing != null)
            {
                return existing;
            }

            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "GrayboxSolidTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = Enumerable.Repeat(new Color32(255, 255, 255, 255), size * size).ToArray();
            texture.SetPixels32(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, SolidAssetPath);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = "GrayboxSolid";
            AssetDatabase.AddObjectToAsset(sprite, texture);
            AssetDatabase.SaveAssets();
            return sprite;
        }

        private static Sprite CreateOrLoadDiamondSprite()
        {
            var existing = AssetDatabase.LoadAllAssetsAtPath(DiamondAssetPath).OfType<Sprite>().FirstOrDefault();
            if (existing != null)
            {
                return existing;
            }

            var texture = new Texture2D(64, 32, TextureFormat.RGBA32, false)
            {
                name = "GrayboxDiamondTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[64 * 32];
            for (var y = 0; y < 32; y++)
            {
                var normalizedY = Mathf.Abs((y + 0.5f) / 16f - 1f);
                for (var x = 0; x < 64; x++)
                {
                    var normalizedX = Mathf.Abs((x + 0.5f) / 32f - 1f);
                    var inside = normalizedX + normalizedY <= 1f;
                    pixels[y * 64 + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, DiamondAssetPath);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 32f), new Vector2(0.5f, 0.5f), 40f);
            sprite.name = "GrayboxDiamond";
            AssetDatabase.AddObjectToAsset(sprite, texture);
            AssetDatabase.SaveAssets();
            return sprite;
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
