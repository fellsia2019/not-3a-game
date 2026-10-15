#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3SkeletonSpriteTests
    {
        private InputTestFixture input;
        private Stage2GameComposition composition;

        [UnitySetUp]
        public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup();
            InputSystem.AddDevice<Keyboard>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.ResetRun();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null;
            input?.TearDown();
        }

        private static Component View(GameObject enemy) => enemy.GetComponentsInChildren<MonoBehaviour>()
            .Single(component => component.GetType().Name == "Stage3SkeletonSprite");

        private static int Value(Component view, string name) => (int)view.GetType().GetProperty(name).GetValue(view);

        [UnityTest]
        public IEnumerator RealWave_WalksFromPortalPausesTakesDamageAndRestarts()
        {
            Assert.That(composition.RunDriver.Run.StartWave(), Is.True);
            yield return null; yield return null;
            var enemy = composition.Enemies.GetEnemy(0);
            var bootstrap = composition.GetComponent<Stage2SceneBootstrap>();
            Assert.That(Vector2.Distance(enemy.transform.position, bootstrap.EntranceMarker.position), Is.LessThan(1f));
            var view = View(enemy.gameObject);
            var body = enemy.GetComponentsInChildren<SpriteRenderer>().Single(renderer => renderer.enabled);
            Assert.That(body.sprite.name, Does.StartWith("Skeleton_"));
            Assert.That(enemy.GetComponent<SpriteRenderer>().enabled, Is.False);
            Assert.That(view.GetComponentsInChildren<Collider>(), Is.Empty);
            Assert.That(view.GetComponentsInChildren<Collider2D>(), Is.Empty);
            var start = enemy.transform.position;
            var seen = new System.Collections.Generic.HashSet<string>();
            for (var i = 0; i < 24; i++)
            {
                seen.Add(body.sprite.name);
                yield return new WaitForSeconds(.025f);
            }
            Assert.That(Vector2.Distance(start, enemy.transform.position), Is.GreaterThan(.5f));
            Assert.That(seen.Count, Is.GreaterThanOrEqualTo(3), "Real translation must advance different walk frames.");
            composition.RunDriver.SetPaused(true);
            yield return null;
            var pausedPosition = enemy.transform.position;
            var pausedSprite = body.sprite;
            yield return new WaitForSeconds(.15f);
            Assert.That(enemy.transform.position, Is.EqualTo(pausedPosition));
            Assert.That(body.sprite, Is.SameAs(pausedSprite));
            composition.RunDriver.SetPaused(false);
            var health = enemy.Health;
            Assert.That(enemy.TakeDamage(1), Is.True);
            yield return null;
            Assert.That(enemy.Health, Is.EqualTo(health - 1));
            Assert.That(body.color, Is.Not.EqualTo(Color.white), "Keep existing hit feedback on the visible sprite.");
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(composition.Enemies.ActiveCount, Is.Zero);
            Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                .Count(renderer => renderer.enabled && renderer.sprite != null && renderer.sprite.name.StartsWith("Skeleton_")), Is.Zero);
            Assert.That(composition.RunDriver.Run.StartWave(), Is.True);
            yield return null; yield return null;
            Assert.That(View(composition.Enemies.GetEnemy(0).gameObject), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DirectionsAndIdle_UseRegisteredTransparentSprites()
        {
            composition.RunDriver.Run.StartWave();
            yield return null; yield return null;
            var enemy = composition.Enemies.GetEnemy(0);
            enemy.SetAutomaticTick(false);
            var view = View(enemy.gameObject);
            var body = enemy.GetComponentsInChildren<SpriteRenderer>().Single(renderer => renderer.enabled);
            var camera = composition.GetComponent<Stage2SceneBootstrap>().CameraFollow.BoundCamera;
            var directions = new[] { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
            for (var index = 0; index < 4; index++)
            {
                enemy.transform.position += directions[index] * .15f;
                yield return null;
                Assert.That(Value(view, "Direction"), Is.EqualTo(index));
                Assert.That(Quaternion.Angle(body.transform.rotation, camera.transform.rotation), Is.LessThan(.01f));
            }
            yield return null;
            Assert.That(Value(view, "CurrentFrame"), Is.Zero, "Stationary siege does not walk in place.");
            var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(image.LoadImage(File.ReadAllBytes("Assets/Stage3/SkeletonSpritePilot/SkeletonWalk.png")), Is.True);
                Assert.That(image.GetPixel(0, 0).a, Is.Zero);
                Assert.That(image.GetPixels32().Count(pixel => pixel.a > 220), Is.GreaterThan(100000));
                Assert.That(body.sprite.pivot.y / body.sprite.pixelsPerUnit, Is.LessThan(.15f));
            }
            finally { Object.Destroy(image); }
        }

        [UnityTest]
        public IEnumerator CaptureActualPortalWalk()
        {
            var bootstrap = composition.GetComponent<Stage2SceneBootstrap>();
            var camera = bootstrap.CameraFollow.BoundCamera;
            bootstrap.CameraFollow.enabled = false;
            var landmarks = bootstrap.EntranceMarker.GetComponentsInChildren<SpriteRenderer>()
                .Any(renderer => renderer.enabled && renderer.sprite != null && renderer.sprite.name == "PortalCartoon");
            var evidence = landmarks ? "Artifacts/S3-LANDMARK-SPRITES/" : "Artifacts/S3-SKELETON-SPRITE/";
            var focus = landmarks ? (bootstrap.EntranceMarker.position + bootstrap.ThroneMarker.position) * .5f + Vector3.back*.7f
                : bootstrap.EntranceMarker.position + new Vector3(-1.8f, -1.9f, -.35f);
            camera.transform.position = focus - camera.transform.forward * 14f;
            camera.orthographicSize = landmarks ? 5.3f : 3.2f;
            Directory.CreateDirectory(evidence+"frames");
            var oldRate = Time.captureFramerate;
            try
            {
                Time.captureFramerate = 15;
                composition.RunDriver.Run.StartWave();
                for (var index = 0; index < 60; index++)
                {
                    yield return null;
                    Capture(camera, $"{evidence}frames/frame_{index:D3}.png", 960, 540);
                    if (index == 18)
                    {
                        if (landmarks)
                        {
                            Capture(camera,"art/review/landmark_sprites_game_overview.png",1600,900);
                            camera.orthographicSize=2.6f;
                            camera.transform.position=bootstrap.EntranceMarker.position+Vector3.back*1.1f-camera.transform.forward*14;
                            Capture(camera,"art/review/portal_sprite_game_close.png",1200,1000);
                            camera.transform.position=bootstrap.ThroneMarker.position+Vector3.back*.6f-camera.transform.forward*14;
                            Capture(camera,"art/review/soul_objective_sprite_game_close.png",1200,1000);
                            camera.transform.position=focus-camera.transform.forward*14;
                            camera.orthographicSize=5.3f;
                        }
                        else
                        {
                        Capture(camera, "art/review/skeleton_sprite_portal_close.png", 1600, 900);
                        camera.orthographicSize = 5f;
                        Capture(camera, "art/review/skeleton_sprite_portal_default.png", 1600, 900);
                        camera.orthographicSize = 3.2f;
                        }
                    }
                }
                Assert.That(composition.Enemies.ActiveCount, Is.GreaterThan(0));
                File.WriteAllText(evidence+"motion.txt",
                    $"60 frames, 15 simulation fps; actual Game camera, ordinary portal spawns and enemy movement.\nGPU={SystemInfo.graphicsDeviceName}\nCamera size={camera.orthographicSize}; landmarkSprites={landmarks}.\n");
            }
            finally { Time.captureFramerate = oldRate; }
        }

        private static void Capture(Camera camera, string path, int width, int height)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var target = RenderTexture.GetTemporary(width, height, 24);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(image);
            }
        }
    }
}
#endif
