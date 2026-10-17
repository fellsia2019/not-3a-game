using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Explicit, disposable review run. Temporary resources/buildings never serialize.</summary>
    public static class Stage3ProductionReview
    {
        private const string Key = "Not3A.ProductionReviewRequested";
        private const string Folder = "Artifacts/S3-RESOURCE-PRODUCTION-20261016/final-contact";
        [InitializeOnLoadMethod] private static void Resume()
        { if (SessionState.GetBool(Key, false)) EditorApplication.update += AwaitWorld; }
        [MenuItem("Tools/Stage 3/Resource Districts/Review Built Production")]
        public static void OpenAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start review outside Play.");
            EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            SessionState.SetBool(Key, true); EditorApplication.update -= AwaitWorld; EditorApplication.update += AwaitWorld;
            EditorApplication.isPlaying = true;
        }
        private static void AwaitWorld()
        {
            if (!EditorApplication.isPlaying) return;
            var game = Object.FindAnyObjectByType<Stage2GameComposition>();
            if (game?.Production?.Rules?.Points.Count != 6) return;
            EditorApplication.update -= AwaitWorld; SessionState.SetBool(Key, false);
            game.StartCoroutine(Capture(game));
        }
        private static IEnumerator Capture(Stage2GameComposition game)
        {
            Directory.CreateDirectory(Folder);
            var scene = game.GetComponent<Stage2SceneBootstrap>(); var world = game.GetComponent<Stage3ResourceDistrictWorld>();
            var camera = scene.CameraFollow.BoundCamera; var hero = scene.PlayerMover; var hud = game.GetComponent<Stage2GameHud>();
            var seed = world.Seed; var oldZoom = camera.orthographicSize; var oldCamera = camera.transform.position;
            var oldFollow = scene.CameraFollow.enabled;
            try
            {
                world.Regenerate(164032); scene.CameraFollow.enabled = false; camera.orthographicSize = 5;
                hud.AddAllResources(100); hud.SelectBuildCategory(Stage2BuildCategory.Generators);
                foreach (var kind in new[] { ResourceProductionBuilding.Sawmill, ResourceProductionBuilding.Quarry, ResourceProductionBuilding.IronMine })
                {
                    var production = game.Production;
                    var index = Enumerable.Range(0, production.Rules.Points.Count).First(i => production.Rules.Points[i].Building == kind);
                    var point = production.Rules.Points[index];
                    hero.transform.position = Vector3.zero;
                    production.SelectAndBeginPlacement(kind);
                    if (production.TryBuildAt(index) != Not3A.Stage2.Economy.ProductionPlacementResult.Built) throw new InvalidOperationException("Review building failed.");
                    foreach (var side in new[] { -1f, 1f })
                    {
                        var position = point.Position + Vector2.up * (side < 0 ? -1.0f : .75f);
                        if (hero.Obstacles.IsBlocked(position, hero.CollisionRadius))
                            hero.Obstacles.TryFindFreePosition(position, hero.CollisionRadius, new Rect(point.Position - Vector2.one * 3, Vector2.one * 6), out position);
                        hero.transform.position = position;
                        camera.transform.position = new Vector3(point.Position.x, point.Position.y + .5f, -.3f) - camera.transform.forward * 30;
                        yield return new WaitForSecondsRealtime(.8f);
                        Save(camera, Folder + "/" + kind + (side < 0 ? "-front.png" : "-behind.png"));
                        if (side < 0)
                        {
                            yield return new WaitForEndOfFrame();
                            ScreenCapture.CaptureScreenshot(Folder + "/" + kind + "-hud.png");
                            yield return null;
                        }
                    }
                }
                File.WriteAllText(Folder + "/complete.txt", DateTime.UtcNow.ToString("O"));
                Debug.Log("S3_PRODUCTION_REVIEW_CAPTURED " + Folder);
            }
            finally
            {
                game.RunDriver.ResetRun(); world.Regenerate(seed); hud.SelectBuildCategory(Stage2BuildCategory.Towers);
                camera.orthographicSize = oldZoom; camera.transform.position = oldCamera; scene.CameraFollow.enabled = oldFollow;
                if (Environment.GetEnvironmentVariable("NOT3A_MANUAL_PRODUCTION_REVIEW") == "1")
                {
                    world.Regenerate(164032); hud.AddAllResources(100);
                    var point = game.Production.Rules.Points.First(p => p.Building == ResourceProductionBuilding.Sawmill);
                    hero.transform.position = point.Position + Vector2.down;
                    scene.CameraFollow.enabled = false; camera.orthographicSize = 5;
                    camera.transform.position = new Vector3(point.Position.x, point.Position.y + .5f, -.3f) - camera.transform.forward * 30;
                    Debug.Log("S3_PRODUCTION_MANUAL_REVIEW_READY funded=true buildings=0");
                }
            }
        }
        private static void Save(Camera camera, string path)
        {
            var target = new RenderTexture(1280, 900, 24); var old = camera.targetTexture; var active = RenderTexture.active;
            var image = new Texture2D(1280, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 900), 0, 0); image.Apply(); File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { camera.targetTexture = old; RenderTexture.active = active; Object.Destroy(target); Object.Destroy(image); }
        }
    }
}
