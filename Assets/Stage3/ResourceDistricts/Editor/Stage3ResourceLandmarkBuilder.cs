using System;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Imports reviewed source artwork without changing source PNGs or the saved scene.</summary>
    public static class Stage3ResourceLandmarkBuilder
    {
        private const string ArtPath = "Assets/Stage3/ResourceDistricts/ResourceDistrictArt.asset";
        private const string Folder = "Assets/Stage3/ResourceDistricts/Landmarks";
        private const string ReviewKey = "Not3A.ResourceLandmark.ReviewRequested";

        [InitializeOnLoadMethod]
        private static void ResumeExplicitReview()
        {
            if (!SessionState.GetBool(ReviewKey, false)) return;
            EditorApplication.update += AwaitWorld;
        }

        private static void AwaitWorld()
        {
            if (!EditorApplication.isPlaying) return;
            var world = Object.FindAnyObjectByType<Stage3ResourceDistrictWorld>();
            if (world == null || world.Districts == null) return;
            SessionState.SetBool(ReviewKey, false);
            EditorApplication.update -= AwaitWorld;
            Stage3ResourceDistrictReview.RequestCapture();
        }

        [MenuItem("Tools/Stage 3/Resource Districts/Import Authored Landmarks And Review")]
        public static void ImportAuthoredAndReview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing authored resource artwork.");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Stage3/ResourceDistricts", "Landmarks");
            var sprite = Import("art/concepts/resource_landmarks_2026-10-16/stone_landmark_v1.png", "StoneLandmark.png");
            var asset = AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>(ArtPath);
            var quarry = new Stage3ResourceLandmark
            {
                kind = MapResourceKind.Stone,
                layers = new[] { new Stage3ResourceLandmarkLayer { sprite = sprite, width = 18,
                    groundImageAreas = new[] { ImageRect(220,625,625,850), ImageRect(430,780,1010,885),
                        ImageRect(840,650,1210,715) } } },
                groundContour = GroundPolygon(new[] { new Vector2(55,480), new Vector2(150,850), new Vector2(450,960),
                    new Vector2(1120,930), new Vector2(1490,730), new Vector2(1510,450), new Vector2(1350,200),
                    new Vector2(700,80), new Vector2(190,180) }),
                productionPoint = GroundPixel(new Vector2(445,715)),
                productionClearance = new Vector2(3.1f,2.1f),
                solidGround = SolidStrips(GroundPolygon(new[] { new Vector2(70,460), new Vector2(220,530),
                    new Vector2(490,560), new Vector2(735,690), new Vector2(890,700), new Vector2(1090,650),
                    new Vector2(1430,550), new Vector2(1490,420), new Vector2(1260,220), new Vector2(700,130),
                    new Vector2(260,190) }))
            };
            var definitions = new List<Stage3ResourceLandmark>(asset.landmarks);
            definitions.RemoveAll(d => d.kind == MapResourceKind.Stone);
            definitions.Add(quarry);
            var ironSource = "art/concepts/resource_landmarks_2026-10-16/iron_landmark_v1.png";
            if (File.Exists(ironSource))
            {
                definitions.RemoveAll(d => d.kind == MapResourceKind.Metal);
                definitions.Add(CreateIron(Import(ironSource,"IronLandmark.png")));
            }
            var forestSource = "art/concepts/resource_landmarks_2026-10-16/forest_landmark_v1.png";
            if (File.Exists(forestSource))
            {
                definitions.RemoveAll(d => d.kind == MapResourceKind.Wood);
                definitions.Add(CreateForest(Import(forestSource,"ForestLandmark.png")));
            }
            asset.landmarks = definitions.ToArray(); asset.landmarkScale = .75f;
            EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            SessionState.SetBool(ReviewKey, true);
            EditorApplication.update -= AwaitWorld;
            EditorApplication.update += AwaitWorld;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        private static Stage3ResourceLandmark CreateForest(Sprite sprite)
        {
            // Ground contacts are authored against visible trunks/stumps in the approved composition.
            var mature = new[] { new Vector2(93,550),new Vector2(160,600),new Vector2(210,650),
                new Vector2(270,700),new Vector2(305,677),new Vector2(350,650),new Vector2(385,597),
                new Vector2(450,580),new Vector2(475,590),new Vector2(553,614),new Vector2(620,620),
                new Vector2(700,665),new Vector2(740,670),new Vector2(805,574),new Vector2(850,590),
                new Vector2(920,650),new Vector2(987,610),new Vector2(1060,565),new Vector2(1115,650),
                new Vector2(1160,585),new Vector2(1220,715),new Vector2(1267,706),new Vector2(1325,635),
                new Vector2(1430,650),new Vector2(1475,690),new Vector2(155,338),new Vector2(220,442),
                new Vector2(256,428),new Vector2(310,448),new Vector2(350,460),new Vector2(418,480),
                new Vector2(475,488),new Vector2(545,402),new Vector2(607,446),new Vector2(700,480),
                new Vector2(757,405),new Vector2(825,395),new Vector2(895,420),new Vector2(970,426),
                new Vector2(1035,365),new Vector2(1100,485),new Vector2(1165,440),new Vector2(1230,420),
                new Vector2(1290,469),new Vector2(1350,443),new Vector2(1410,440),new Vector2(1495,486),
                new Vector2(250,245),new Vector2(292,210),new Vector2(350,225),new Vector2(445,190),
                new Vector2(543,238),new Vector2(603,248),new Vector2(664,280),new Vector2(750,290),
                new Vector2(824,225),new Vector2(895,215),new Vector2(970,180),new Vector2(1050,180),
                new Vector2(1135,170),new Vector2(1210,250),new Vector2(1295,275),new Vector2(1368,272),
                new Vector2(595,500),new Vector2(490,645),new Vector2(430,715) };
            var young = new[] { new Vector2(355,785),new Vector2(380,850),new Vector2(425,885),
                new Vector2(510,790),new Vector2(565,820),new Vector2(610,857),new Vector2(647,770),
                new Vector2(690,843),new Vector2(745,810),new Vector2(810,856),new Vector2(857,855),
                new Vector2(925,922) };
            var circles = new List<Stage3LandmarkGroundCircle>();
            foreach (var p in mature) circles.Add(GroundCircle(p.x,p.y,.24f));
            foreach (var p in young) circles.Add(GroundCircle(p.x,p.y,.14f));
            return new Stage3ResourceLandmark
            {
                kind = MapResourceKind.Wood,
                layers = new[] { new Stage3ResourceLandmarkLayer { sprite = sprite, width = 18,
                    groundImageAreas = new[] { ImageRect(865,605,1280,870),ImageRect(890,800,1140,940),
                        ImageRect(720,585,850,740),ImageRect(530,425,700,585),ImageRect(250,215,430,345),
                        ImageRect(430,650,540,760),ImageRect(650,500,750,600),ImageRect(1270,455,1410,545) } } },
                groundContour = GroundPolygon(new[] { new Vector2(20,550),new Vector2(130,790),new Vector2(460,950),
                    new Vector2(1050,970),new Vector2(1460,740),new Vector2(1510,460),new Vector2(1410,265),
                    new Vector2(1130,125),new Vector2(500,130),new Vector2(180,270) }),
                productionPoint = GroundPixel(new Vector2(1045,775)),
                productionClearance = new Vector2(3,2),
                groundCircles = circles.ToArray()
            };
        }

        private static Stage3ResourceLandmark CreateIron(Sprite sprite) => new Stage3ResourceLandmark
        {
            kind = MapResourceKind.Metal,
            layers = new[] { new Stage3ResourceLandmarkLayer { sprite = sprite, width = 18,
                groundImageAreas = new[] { ImageRect(960,230,1190,475),ImageRect(1100,400,1215,480),
                    ImageRect(1115,480,1220,580),ImageRect(1005,555,1120,640),ImageRect(900,690,1060,770) } } },
            groundContour = GroundPolygon(new[] { new Vector2(40,400),new Vector2(180,680),new Vector2(600,920),
                new Vector2(900,990),new Vector2(1400,880),new Vector2(1510,470),new Vector2(1350,240),
                new Vector2(830,110),new Vector2(400,110),new Vector2(100,270) }),
            productionPoint = GroundPixel(new Vector2(1080,350)),
            productionClearance = new Vector2(2,1.5f),
            solidGround = SolidStrips(GroundPolygon(new[] { new Vector2(105,370),new Vector2(210,630),
                new Vector2(500,810),new Vector2(790,830),new Vector2(940,675),new Vector2(1010,550),
                new Vector2(1000,460),new Vector2(930,375),new Vector2(850,310),new Vector2(920,220),
                new Vector2(410,160) })),
            groundCircles = new[] { GroundCircle(1370,440,.9f),GroundCircle(1360,660,1.2f),
                GroundCircle(1210,880,1.2f),GroundCircle(980,880,.9f),GroundCircle(1300,340,1.0f) }
        };

        private static Stage3LandmarkGroundCircle GroundCircle(float x,float y,float radius) =>
            new Stage3LandmarkGroundCircle { center = GroundPixel(new Vector2(x,y)), radius = radius };

        private static Sprite Import(string source, string fileName)
        {
            var path = Folder + "/" + fileName;
            File.Copy(source, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1536f / 18f;
            importer.alphaIsTransparency = true; importer.isReadable = true;
            // Single-image world art needs prefiltered minification across the tactical zoom range.
            importer.mipmapEnabled = true; importer.filterMode = FilterMode.Trilinear;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.5f,.35f); settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Rect ImageRect(float left,float top,float right,float bottom) =>
            new Rect(left/1536,(1024-bottom)/1024,(right-left)/1536,(bottom-top)/1024);

        private static Vector2 GroundPixel(Vector2 pixel) => new Vector2((pixel.x - 768) / (1536f / 18),
            (1024 - pixel.y - 1024 * .35f) / (1536f / 18) / Mathf.Cos(35 * Mathf.Deg2Rad));
        private static Vector2[] GroundPolygon(Vector2[] pixels)
        {
            var result = new Vector2[pixels.Length];
            for (var i = 0; i < result.Length; i++) result[i] = GroundPixel(pixels[i]);
            return result;
        }
        private static Rect[] SolidStrips(Vector2[] polygon)
        {
            var min = polygon[0].y; var max = min;
            foreach (var p in polygon) { min = Mathf.Min(min,p.y); max = Mathf.Max(max,p.y); }
            const float step = .25f;
            var result = new List<Rect>();
            for (var y = min; y < max; y += step)
            {
                var sample = Mathf.Min(y + step * .5f,max - .0001f);
                var intersections = new List<float>();
                for (var i = 0; i < polygon.Length; i++)
                {
                    var a = polygon[i]; var b = polygon[(i+1)%polygon.Length];
                    if ((a.y <= sample && b.y > sample) || (b.y <= sample && a.y > sample))
                        intersections.Add(Mathf.Lerp(a.x,b.x,(sample-a.y)/(b.y-a.y)));
                }
                intersections.Sort();
                for (var i = 0; i + 1 < intersections.Count; i += 2)
                    result.Add(new Rect(intersections[i], y, intersections[i+1]-intersections[i],Mathf.Min(step,max-y)));
            }
            return result.ToArray();
        }
    }
}
