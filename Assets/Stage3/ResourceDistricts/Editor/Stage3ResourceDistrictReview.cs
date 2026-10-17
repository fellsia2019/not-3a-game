using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Explicit Game-camera review; never authors scene, art or project settings.</summary>
    public static class Stage3ResourceDistrictReview
    {
        private static bool running;

        [MenuItem("Tools/Stage 3/Resource Districts/Capture Live Districts")]
        public static void RequestCapture()
        {
            if (!EditorApplication.isPlaying || running) return;
            var world = Object.FindAnyObjectByType<Stage3ResourceDistrictWorld>();
            if (world == null || world.Districts == null) return;
            running = true;
            world.StartCoroutine(Capture(world));
        }

        [MenuItem("Tools/Stage 3/Resource Districts/Capture Live Districts", true)]
        private static bool CanCapture() => EditorApplication.isPlaying && !running;

        private static IEnumerator Capture(Stage3ResourceDistrictWorld world)
        {
            var follow = Object.FindAnyObjectByType<Stage2CameraFollow>();
            var camera = follow.BoundCamera;
            var player = Object.FindAnyObjectByType<Stage2PlayerMover>();
            var originalSeed = world.Seed;
            var originalHero = player.transform.position;
            var originalCamera = camera.transform.position;
            var originalZoom = camera.orthographicSize;
            var originalFar = camera.farClipPlane;
            var originalFollow = follow.enabled;
            var folder = Environment.GetEnvironmentVariable("NOT3A_DISTRICT_REVIEW") ??
                "Artifacts/S3-DISTRICT-REVIEWS/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "/";
            folder = folder.TrimEnd('/', '\\') + "/";
            Directory.CreateDirectory(folder);
            try
            {
                follow.enabled = false;
                camera.farClipPlane = 300;
                foreach (var seed in new[] { 164032, 164033 })
                {
                    world.Regenerate(seed);
                    yield return null;
                    yield return null;
                    foreach (var district in world.Districts.Take(3))
                    foreach (var zoom in new[] { 14f, 10f, 5f, 2.5f })
                    {
                        camera.orthographicSize = zoom;
                        var focus = (Vector3)(Vector2)district.Center + Vector3.forward *
                            (district.Kind == MapResourceKind.Metal ? 2 : district.Kind == MapResourceKind.Stone ? -2 : 0);
                        if (!district.CutsTerrain)
                        {
                            var renderer = world.GetComponentsInChildren<SpriteRenderer>().First(s =>
                                s.transform.parent.name == district.Kind + " authored landmark " + district.Seed);
                            focus = renderer.bounds.center;
                        }
                        camera.transform.position = focus - camera.transform.forward * 30;
                        yield return null;
                        yield return null;
                        Save(camera, folder + district.Kind + "-" + seed + "-" + zoom + ".png");
                    }
                }
                foreach (var point in world.ProductionPoints)
                {
                    player.transform.position = new Vector3(point.Position.x,point.Position.y,0);
                    camera.orthographicSize = 5;
                    camera.transform.position = player.transform.position - camera.transform.forward * 30;
                    yield return new WaitForSecondsRealtime(.7f);
                    Save(camera, folder + point.Resource + "-production-point.png");
                }
                var forest = world.Districts.First(d => d.Kind == MapResourceKind.Wood);
                player.transform.position = (Vector2)forest.Center;
                camera.orthographicSize = 5;
                camera.transform.position = player.transform.position - camera.transform.forward * 30;
                yield return new WaitForSecondsRealtime(.7f);
                Save(camera, folder + "forest-hero.png");
                File.WriteAllText(folder + "complete.txt", DateTime.UtcNow.ToString("O"));
                Debug.Log("S3_DISTRICT_REVIEW_CAPTURED " + folder);
            }
            finally
            {
                if (world != null) world.Regenerate(originalSeed);
                if (player != null) player.transform.position = originalHero;
                if (camera != null)
                {
                    camera.orthographicSize = originalZoom;
                    camera.farClipPlane = originalFar;
                    camera.transform.position = originalCamera;
                }
                if (follow != null) follow.enabled = originalFollow;
                running = false;
            }
        }

        private static void Save(Camera camera, string path)
        {
            var target = new RenderTexture(1280, 900, 24);
            var old = camera.targetTexture;
            var active = RenderTexture.active;
            var image = new Texture2D(1280, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = old;
                RenderTexture.active = active;
                Object.Destroy(target);
                Object.Destroy(image);
            }
        }
    }
}
