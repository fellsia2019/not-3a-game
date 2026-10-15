using System;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3SkeletonSpriteBuilder
    {
        public const string Folder = "Assets/Stage3/SkeletonSpritePilot/";
        public const string SheetPath = Folder + "SkeletonWalk.png";
        public const string PrefabPath = Folder + "SkeletonSprite.prefab";
        public const string ScenePath = "Assets/Scenes/Stage3Rapid3DGamePilot.unity";
        public const string Evidence = "Artifacts/S3-SKELETON-SPRITE/";
        private const string SetPath = Folder + "SkeletonWalks.asset";
        private const float ScreenHeight = .78f;

        [MenuItem("Tools/Stage 3/Skeleton Sprites/Build and Wire")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before importing the sprite pilot.");
            if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                .Any(index => UnityEngine.SceneManagement.SceneManager.GetSceneAt(index).isDirty))
                throw new InvalidOperationException("Save open scene changes before wiring the sprite pilot.");
            BuildPrefab();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var presentation = Object.FindAnyObjectByType<Stage3TacticalPresentation>();
            if (presentation == null) throw new InvalidOperationException("Tactical presentation is missing.");
            var serialized = new SerializedObject(presentation);
            serialized.FindProperty("skeletonSpritePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Stage3SkeletonSprite>(PrefabPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_SKELETON_WIRED frames=24 directions=4 scene=" + ScenePath);
        }

        public static void BuildPrefab()
        {
            Directory.CreateDirectory(Evidence);
            AssetDatabase.ImportAsset(SheetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Skeleton sheet is missing.");
            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            if (texture.width % 6 != 0 || texture.height % 4 != 0)
                throw new InvalidOperationException("Skeleton sheet must contain a regular 6x4 grid.");
            var pixels = texture.GetPixels32();
            var width = texture.width / 6;
            var height = texture.height / 4;
            var bounds = new RectInt[24];
            var heights = new int[24];
            for (var index = 0; index < 24; index++)
            {
                bounds[index] = OpaqueBounds(pixels, texture.width, width, height, index);
                heights[index] = bounds[index].height;
                if (heights[index] < height / 2 || bounds[index].xMin < 3 || bounds[index].xMax > width - 3 ||
                    bounds[index].yMin < 3 || bounds[index].yMax > height - 3)
                    throw new InvalidOperationException("Clipped or empty skeleton frame " + index);
            }
            Array.Sort(heights);
            var pixelsPerUnit = heights[12] / ScreenHeight;
            var set = AssetDatabase.LoadAssetAtPath<Stage3SkeletonSpriteSet>(SetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<Stage3SkeletonSpriteSet>();
                AssetDatabase.CreateAsset(set, SetPath);
            }
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(SetPath).OfType<Sprite>())
                Object.DestroyImmediate(old, true);
            var frames = new Sprite[24];
            var report = new StringBuilder($"sheet={texture.width}x{texture.height} frames=24 ppu={pixelsPerUnit:F3}\n");
            var directionNames = new[] { "Right", "Away", "Left", "Toward" };
            for (var index = 0; index < frames.Length; index++)
            {
                var x = index % 6 * width;
                var y = (3 - index / 6) * height;
                var opaque = bounds[index];
                // Register to the torso rather than the swinging hands; keep the planted foot on the ground.
                double total = 0, weightedX = 0;
                for (var py = opaque.yMin + opaque.height * 4 / 10; py < opaque.yMin + opaque.height * 6 / 10; py++)
                for (var px = opaque.xMin; px < opaque.xMax; px++)
                {
                    var alpha = pixels[(y + py) * texture.width + x + px].a / 255f;
                    if (alpha < .5f) continue;
                    weightedX += px * alpha;
                    total += alpha;
                }
                var anchorX = total > 0 ? (float)(weightedX / total) : opaque.center.x;
                var pivot = new Vector2(anchorX / width, opaque.yMin / (float)height);
                var frame = Sprite.Create(texture, new Rect(x, y, width, height), pivot,
                    pixelsPerUnit, 0, SpriteMeshType.FullRect);
                frame.name = "Skeleton_" + directionNames[index / 6] + "_" + index % 6;
                AssetDatabase.AddObjectToAsset(frame, set);
                frames[index] = frame;
                report.AppendLine($"{frame.name} opaque={opaque} pivot={pivot}");
            }
            set.Configure(frames);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            importer.isReadable = false;
            importer.SaveAndReimport();
            File.WriteAllText(Evidence + "import.txt", report.ToString());

            var bodyMaterial = Material("SkeletonBody.mat", Color.white);
            var shadowMaterial = Material("ContactShadow.mat", new Color(0f, 0f, 0f, .19f));
            var shadowMesh = ShadowMesh();
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("Skeleton Sprite");
            try
            {
                var view = root.GetComponent<Stage3SkeletonSprite>();
                if (view == null) view = root.AddComponent<Stage3SkeletonSprite>();
                var body = root.transform.Find("Animated body");
                if (body == null)
                {
                    body = new GameObject("Animated body").transform;
                    body.SetParent(root.transform, false);
                }
                body.localPosition = new Vector3(0f, 0f, -.025f);
                var renderer = body.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = body.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = frames[18];
                renderer.sharedMaterial = bodyMaterial;
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var shadow = root.transform.Find("Contact shadow");
                if (shadow == null)
                {
                    shadow = new GameObject("Contact shadow").transform;
                    shadow.SetParent(root.transform, false);
                }
                shadow.localPosition = Vector3.back * .012f;
                var shadowFilter = shadow.GetComponent<MeshFilter>();
                if (shadowFilter == null) shadowFilter = shadow.gameObject.AddComponent<MeshFilter>();
                shadowFilter.sharedMesh = shadowMesh;
                var shadowRenderer = shadow.GetComponent<MeshRenderer>();
                if (shadowRenderer == null) shadowRenderer = shadow.gameObject.AddComponent<MeshRenderer>();
                shadowRenderer.sharedMaterial = shadowMaterial;
                shadowRenderer.shadowCastingMode = ShadowCastingMode.Off;
                shadowRenderer.receiveShadows = false;
                var serialized = new SerializedObject(view);
                serialized.FindProperty("walks").objectReferenceValue = set;
                serialized.FindProperty("body").objectReferenceValue = renderer;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                if (existing != null) PrefabUtility.UnloadPrefabContents(root);
                else Object.DestroyImmediate(root);
            }
            AssetDatabase.SaveAssets();
        }

        private static RectInt OpaqueBounds(Color32[] pixels, int sheetWidth, int width, int height, int index)
        {
            var left = width; var right = -1; var bottom = height; var top = -1;
            var ox = index % 6 * width; var oy = (3 - index / 6) * height;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (pixels[(oy + y) * sheetWidth + ox + x].a < 128) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x);
                bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
            }
            return new RectInt(left, bottom, right - left + 1, top - bottom + 1);
        }

        private static Material Material(string name, Color color)
        {
            var path = Folder + name;
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Not3A/Skeleton Sprite Unlit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh ShadowMesh()
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(Folder + "ContactShadow.asset");
            if (mesh != null) return mesh;
            const int count = 20;
            var vertices = new Vector3[count + 1];
            var colors = new Color[count + 1];
            var uv = new Vector2[count + 1];
            var triangles = new int[count * 3];
            for (var i = 0; i <= count; i++) { colors[i] = Color.white; uv[i] = Vector2.one * .5f; }
            for (var i = 0; i < count; i++)
            {
                var angle = 2f * Mathf.PI * i / count;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * .20f, Mathf.Sin(angle) * .13f, 0f);
                triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = (i + 1) % count + 1;
            }
            mesh = new Mesh { name = "Skeleton contact shadow", vertices = vertices, colors = colors, uv = uv, triangles = triangles };
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, Folder + "ContactShadow.asset");
            return mesh;
        }

        public static void BuildWindows()
        {
            BuildAndWire();
            const string output = "Builds/Windows/Stage3SkeletonSprite/Not3A.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            File.WriteAllText(Evidence + "build.txt", $"{report.summary.result} bytes={report.summary.totalSize} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Skeleton sprite build failed.");
        }
    }
}
