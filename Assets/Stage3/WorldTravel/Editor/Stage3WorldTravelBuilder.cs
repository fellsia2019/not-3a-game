using System;
using System.Collections;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3WorldTravelBuilder
    {
        public const string Folder = "Assets/Stage3/WorldTravel/";
        public const string DataPath = Folder + "WorldTravelData.asset";
        private const string ScenePath = "Assets/Scenes/Stage3Rapid3DGamePilot.unity";
        private const string ReviewKey = "Not3A.WorldTravel.Review";

        [MenuItem("Tools/Stage 3/World Travel/Build and Wire")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before building travel artwork.");
            var seal = Import("art/concepts/research_and_travel_2026-10-17/teleport_circles_v4_seal.png", "TeleportSeal.png", false);
            var workshop = Import("art/concepts/research_and_travel_2026-10-17/research_workshop_v1.png", "ResearchWorkshop.png", true);
            var data = AssetDatabase.LoadAssetAtPath<Stage3WorldTravelData>(DataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<Stage3WorldTravelData>(); AssetDatabase.CreateAsset(data, DataPath); }
            data.sealMaterial = Material("TeleportSeal.mat", "Not3A/Ground Teleport Seal");
            data.sealMaterial.SetTexture("_MainTex", seal); EditorUtility.SetDirty(data.sealMaterial);
            data.workshopMaterial = Material("ResearchWorkshop.mat", "Not3A/Skeleton Sprite Unlit");
            if (data.workshopSprite == null)
            {
                var pixels = workshop.GetPixels32(); var rows = new int[workshop.height]; var columns = new int[workshop.width];
                for (var y = 0; y < workshop.height; y++) for (var x = 0; x < workshop.width; x++)
                    if (pixels[y * workshop.width + x].a >= 128) { rows[y]++; columns[x]++; }
                var left = Array.FindIndex(columns, n => n >= 6); var right = Array.FindLastIndex(columns, n => n >= 6) + 1;
                var bottom = Array.FindIndex(rows, n => n >= 6);
                var pivot = new Vector2((left + right) * .5f / workshop.width, (float)bottom / workshop.height);
                data.workshopSprite = Sprite.Create(workshop, new Rect(0, 0, workshop.width, workshop.height), pivot,
                    (right - left) / data.workshopWidth, 0, SpriteMeshType.FullRect);
                data.workshopSprite.name = "Approved research workshop"; AssetDatabase.AddObjectToAsset(data.workshopSprite, data);
            }
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
            var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + "ResearchWorkshop.png");
            importer.isReadable = false; importer.SaveAndReimport();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            // Opening another scene unloads assets that have no scene reference yet.
            data = AssetDatabase.LoadAssetAtPath<Stage3WorldTravelData>(DataPath);
            if (data == null) throw new InvalidOperationException("Travel data was not saved.");
            var presentation = Object.FindAnyObjectByType<Stage3TacticalPresentation>();
            var serialized = new SerializedObject(presentation);
            serialized.FindProperty("worldTravelData").objectReferenceValue = data;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            EditorSceneManager.OpenScene(ScenePath);
            var saved = new SerializedObject(Object.FindAnyObjectByType<Stage3TacticalPresentation>());
            if (saved.FindProperty("worldTravelData").objectReferenceValue == null)
                throw new InvalidOperationException("Travel data reference did not survive the scene save.");
            Debug.Log("S3_WORLD_TRAVEL_WIRED approvedSeal=v4 researchRequired=true sourceBytesPreserved=true");
        }

        private static Texture2D Import(string source, string name, bool readable)
        {
            var path = Folder + name; File.Copy(source, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default; importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = readable;
            importer.isReadable = readable; importer.sRGBTexture = true; importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static Material Material(string name, string shader)
        {
            var path = Folder + name; var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var found = Shader.Find(shader); if (found == null) throw new InvalidOperationException("Missing shader " + shader);
            material = new Material(found); AssetDatabase.CreateAsset(material, path); return material;
        }

        [InitializeOnLoadMethod]
        private static void RegisterReview()
        {
            EditorApplication.update += () =>
            {
                if (!SessionState.GetBool(ReviewKey, false) || !EditorApplication.isPlaying) return;
                var travel = Object.FindAnyObjectByType<Stage3WorldTravel>();
                if (travel == null || !travel.IsReady) return;
                var scene = travel.GetComponent<Stage2SceneBootstrap>();
                scene.PlayerMover.transform.position = travel.WorkshopInteractionPosition + Vector2.down * .5f;
                travel.GetComponent<Stage2GameHud>().SelectBuildCategory(Stage2BuildCategory.Research);
                scene.CameraFollow.SnapToTarget(); SessionState.SetBool(ReviewKey, false);
                CaptureCurrentGame();
                Debug.Log("S3_WORLD_TRAVEL_LIVE_REVIEW_READY: Research tab open; build the workshop first, then E studies Teleportation. No workshop/research granted.");
            };
            EditorApplication.playModeStateChanged += state =>
            { if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool(ReviewKey, false); };
        }

        [MenuItem("Tools/Stage 3/World Travel/Capture Current Game")]
        public static void CaptureCurrentGame()
        {
            if (!EditorApplication.isPlaying) return;
            var travel = Object.FindAnyObjectByType<Stage3WorldTravel>();
            if (travel != null && travel.IsReady) travel.StartCoroutine(Capture(travel));
        }

        private static IEnumerator Capture(Stage3WorldTravel travel)
        {
            yield return new WaitForEndOfFrame();
            var stage = travel.IsResearched ? "researched" : travel.WorkshopBuilt ? "built" : travel.IsPlacingWorkshop ? "preview" : "menu";
            var frame = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var path = "art/review/workshop_build_" + stage + "_2026-10-17.png";
                File.WriteAllBytes(path, frame.EncodeToPNG()); Debug.Log("S3_WORKSHOP_BUILD_CAPTURE " + path);
            }
            finally { Object.Destroy(frame); }
        }

        [MenuItem("Tools/Stage 3/World Travel/Open Live Review")]
        public static void OpenReview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the open scene before live review.");
            EditorSceneManager.OpenScene(ScenePath); SessionState.SetBool(ReviewKey, true);
            EditorApplication.ExecuteMenuItem("Window/General/Game"); EditorApplication.isPlaying = true;
        }
    }
}
