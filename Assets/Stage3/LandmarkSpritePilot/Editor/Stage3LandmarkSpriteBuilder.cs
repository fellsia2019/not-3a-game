using System;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3LandmarkSpriteBuilder
    {
        public const string Folder = "Assets/Stage3/LandmarkSpritePilot/";
        public const string PortalTexture = Folder + "PortalCartoon.png";
        public const string SoulTexture = Folder + "SoulObjectiveCartoon.png";
        public const string PortalPrefab = Folder + "PortalSprite.prefab";
        public const string SoulPrefab = Folder + "SoulObjectiveSprite.prefab";
        public const string Evidence = "Artifacts/S3-LANDMARK-SPRITES/";

        public static void BuildPrefabs()
        {
            Directory.CreateDirectory(Evidence);
            Build(PortalTexture, PortalPrefab, true, 3.25f, new Vector2(.5f,.22f));
            Build(SoulTexture, SoulPrefab, false, 3.8f, new Vector2(.5f,.38f));
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Stage 3/Landmark Sprites/Build and Wire")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount)
                    .Any(index=>UnityEngine.SceneManagement.SceneManager.GetSceneAt(index).isDirty))
                throw new InvalidOperationException("Stop Play Mode and save open scene edits before wiring sprites.");
            BuildPrefabs();
            var scene = EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            var presentation = Object.FindAnyObjectByType<Stage3TacticalPresentation>();
            if (presentation == null) throw new InvalidOperationException("Stage 3 tactical presentation is missing.");
            var serialized = new SerializedObject(presentation);
            serialized.FindProperty("entrancePortalPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefab);
            serialized.FindProperty("soulSpherePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(SoulPrefab);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Evidence+"import.txt","Portal and soul sprite prefabs wired; original PNGs unchanged.\nMap/spawn/objective rules and 3x3 exclusions unchanged.\n");
            Debug.Log("S3_LANDMARK_SPRITES_WIRED portal=true soul=true");
        }

        private static void Build(string texturePath,string prefabPath,bool portal,float width,Vector2 pivot)
        {
            AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (texture == null || importer == null) throw new InvalidOperationException("Missing landmark art: "+texturePath);
            importer.GetSourceTextureWidthAndHeight(out var pixelWidth, out _);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelWidth/width;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            if (sprite == null) throw new InvalidOperationException("Sprite import failed: "+texturePath);
            var materialPath = Folder+(portal?"PortalFlow.mat":"SoulFlow.mat");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Not3A/Landmark Sprite Flow");
                if (shader == null) throw new InvalidOperationException("Landmark sprite shader missing.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material,materialPath);
            }
            material.SetFloat("_Kind",portal?0:1);
            material.SetFloat("_Speed",portal ? .75f : 1f);
            material.SetVector("_FlowCenter",portal?new Vector4(.51f,.56f,0,0):new Vector4(.5f,.38f,0,0));
            material.SetVector("_FlowRadii",portal?new Vector4(.18f,.22f,0,0):new Vector4(.43f,.27f,0,0));
            EditorUtility.SetDirty(material);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var root = existing != null ? PrefabUtility.LoadPrefabContents(prefabPath) : new GameObject(portal?"Portal sprite":"Soul objective sprite");
            try
            {
                var view = root.GetComponent<Stage3LandmarkSprite>();
                if (view == null) view = root.AddComponent<Stage3LandmarkSprite>();
                var child = root.transform.Find("Sprite artwork");
                if (child == null) { child = new GameObject("Sprite artwork").transform; child.SetParent(root.transform,false); }
                child.localPosition = Vector3.back*.025f;
                var renderer = child.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = child.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite; renderer.sharedMaterial = material;
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;
                renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
                var serialized = new SerializedObject(view);
                serialized.FindProperty("body").objectReferenceValue = renderer;
                serialized.FindProperty("portal").boolValue = portal;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (portal)
                {
                    var clock = root.GetComponent<Stage3EntrancePortal>();
                    if (clock == null) clock = root.AddComponent<Stage3EntrancePortal>();
                    clock.Configure(renderer);
                }
                PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
            }
            finally { if(existing != null) PrefabUtility.UnloadPrefabContents(root); else Object.DestroyImmediate(root); }
        }

        public static void BuildWindows()
        {
            BuildAndWire();
            const string output = "Builds/Windows/Stage3LandmarkSprites/Not3A.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{Stage3SkeletonSpriteBuilder.ScenePath}, locationPathName=output,
                target=BuildTarget.StandaloneWindows64, options=BuildOptions.Development
            });
            File.WriteAllText(Evidence+"build.txt",$"{report.summary.result} bytes={report.summary.totalSize} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings}");
            if(report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Landmark sprite build failed.");
        }
    }
}
