using System;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3ResourceSpriteBuilder
    {
        public const string Folder = "Assets/Stage3/ResourceSpritePilot/";
        public const string WoodPrefab = Folder + "WoodConifer.prefab";
        public const string StonePrefab = Folder + "StoneGranite.prefab";
        public const string OrePrefab = Folder + "IronOre.prefab";
        private const string ReviewResource = "Not3A.ResourceSprites.ReviewResource";
        public static bool SourcesPresent => File.Exists(Folder + "WoodConifer.png") &&
            File.Exists(Folder + "StoneGranite.png") && File.Exists(Folder + "IronOre.png");

        [InitializeOnLoadMethod]
        private static void RegisterReview()
        {
            EditorApplication.update += () =>
            {
                var index = SessionState.GetInt(ReviewResource, -1);
                if (index < 0 || !EditorApplication.isPlaying) return;
                var game = Object.FindAnyObjectByType<Stage2GameComposition>();
                if (game == null || !game.IsInitialized ||
                    game.GetComponentInChildren<Stage3ResourceSprite>() == null) return;
                Frame(game, (MapResourceKind)index);
                SessionState.SetInt(ReviewResource, -1);
            };
        }

        [MenuItem("Tools/Stage 3/Resource Sprites/Open Live Review")]
        public static void OpenReview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { FocusWood(); return; }
            if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                .Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Save open scene edits before resource review.");
            EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            SessionState.SetInt(ReviewResource, (int)MapResourceKind.Wood);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Stage 3/Resource Sprites/Frame Wood")]
        public static void FocusWood() => RequestFrame(MapResourceKind.Wood);
        [MenuItem("Tools/Stage 3/Resource Sprites/Frame Stone")]
        public static void FocusStone() => RequestFrame(MapResourceKind.Stone);
        [MenuItem("Tools/Stage 3/Resource Sprites/Frame Iron Ore")]
        public static void FocusOre() => RequestFrame(MapResourceKind.Metal);

        private static void RequestFrame(MapResourceKind kind)
        {
            if (EditorApplication.isPlaying) SessionState.SetInt(ReviewResource, (int)kind);
        }

        private static void Frame(Stage2GameComposition game, MapResourceKind kind)
        {
            var bootstrap = game.GetComponent<Stage2SceneBootstrap>();
            var anchor = game.Gathering.Economy.WorldPosition(kind);
            bootstrap.PlayerMover.transform.position = anchor + new Vector2(-1.1f, -.55f);
            bootstrap.CameraFollow.SnapToTarget();
            Debug.Log($"S3_RESOURCE_SPRITE_REVIEW_READY resource={kind} anchor={anchor}");
        }

        [MenuItem("Tools/Stage 3/Resource Sprites/Build and Wire")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                .Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Stop Play and save open scene edits before resource wiring.");
            BuildPrefabs();
            var scene = EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            Wire(Object.FindAnyObjectByType<Stage3TacticalPresentation>());
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_RESOURCE_SPRITES_WIRED: approved wood, stone and iron images; existing economy anchors.");
        }

        public static void Wire(Stage3TacticalPresentation presentation)
        {
            if (presentation == null) throw new InvalidOperationException("Tactical presentation is missing.");
            var serialized = new SerializedObject(presentation);
            foreach (var item in new[] { ("woodSourceModel", WoodPrefab), ("stoneSourceModel", StonePrefab), ("metalSourceModel", OrePrefab) })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(item.Item2);
                if (prefab == null) throw new InvalidOperationException("Resource sprite prefab missing: " + item.Item2);
                serialized.FindProperty(item.Item1).objectReferenceValue = prefab;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(presentation);
        }

        public static void BuildPrefabs()
        {
            var materialPath = Folder + "ResourceInk.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Not3A/Skeleton Sprite Unlit");
                if (shader == null) throw new InvalidOperationException("Existing sprite shader is missing.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            Build("WoodConifer", WoodPrefab, 1.5f, .03f, material);
            Build("StoneGranite", StonePrefab, 1.45f, .12f, material);
            Build("IronOre", OrePrefab, 1.5f, .12f, material);
            AssetDatabase.SaveAssets();
        }

        private static void Build(string name, string prefabPath, float visibleWidth, float footingFraction, Material material)
        {
            var path = Folder + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Approved resource PNG missing: " + path);
            // Inspect alpha without repainting/cropping the source image. Transparent canvas
            // margins must not make one resource smaller than another after import.
            var original = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Vector2 pivot;
            float pixelsPerUnit;
            try
            {
                if (!original.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Invalid PNG: " + path);
                var minX = original.width; var minY = original.height; var maxX = -1; var maxY = -1;
                var pixels = original.GetPixels32();
                for (var y = 0; y < original.height; y++)
                for (var x = 0; x < original.width; x++)
                {
                    if (pixels[y * original.width + x].a < 128) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
                if (maxX < minX) throw new InvalidOperationException("Empty resource PNG: " + path);
                pivot = new Vector2((minX + maxX + 1f) * .5f / original.width,
                    (minY + (maxY - minY + 1f) * footingFraction) / original.height);
                pixelsPerUnit = (maxX - minX + 1f) / visibleWidth;
            }
            finally { Object.DestroyImmediate(original); }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.isReadable = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Sprite import failed: " + path);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(prefabPath) : new GameObject(name + " sprite");
            try
            {
                var view = root.GetComponent<Stage3ResourceSprite>();
                if (view == null) view = root.AddComponent<Stage3ResourceSprite>();
                var child = root.transform.Find("Sprite artwork");
                if (child == null) { child = new GameObject("Sprite artwork").transform; child.SetParent(root.transform, false); }
                var body = child.GetComponent<SpriteRenderer>();
                if (body == null) body = child.gameObject.AddComponent<SpriteRenderer>();
                body.sprite = sprite;
                body.sharedMaterial = material;
                body.spriteSortPoint = SpriteSortPoint.Pivot;
                body.shadowCastingMode = ShadowCastingMode.Off;
                body.receiveShadows = false;
                var serialized = new SerializedObject(view);
                serialized.FindProperty("body").objectReferenceValue = body;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { if (existing != null) PrefabUtility.UnloadPrefabContents(root); else Object.DestroyImmediate(root); }
        }
    }
}
