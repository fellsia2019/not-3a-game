#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using Not3A.Stage2.WavesAI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroHordeVisualTests
    {
        private static string Evidence => System.Environment.GetEnvironmentVariable("NOT3A_HERO_HORDE_EVIDENCE") ??
            "Artifacts/S3-HERO-COMBAT-20261018/horde-visual";
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private int previousCaptureRate;

        [UnitySetUp] public IEnumerator Load()
        {
            input = new InputTestFixture(); input.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            previousCaptureRate = Time.captureFramerate;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            scene = game.GetComponent<Stage2SceneBootstrap>();
            Assert.That(game.IsInitialized, Is.True);
            game.HeroCombat.AutomaticTick = false;
            game.RunDriver.enabled = false;
            game.RunDriver.ResetRun();
            game.SetPaused(false);
            scene.CameraFollow.enabled = false;
            Directory.CreateDirectory(Evidence);
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.captureFramerate = previousCaptureRate;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown(); Time.timeScale = 1f;
        }

        [UnityTest] public IEnumerator AuthoredSecondHordeAndHeroArrowRenderTogetherWithoutMissingArt()
        {
            var run = game.RunDriver.Run;
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            // Presentation-only setup: finish the real first wave through normal removal events.
            // No defenses, resources, weapon buffs or replacement enemy population are injected.
            Assert.That(Stage3TestCycle.StartNight(run), Is.True);
            for (var step = 0; step < 1000 && run.WaveIndex == 0; step++)
            {
                run.Tick(.05f);
                for (var i = game.Enemies.ActiveCount - 1; i >= 0; i--)
                {
                    var enemy = game.Enemies.GetEnemy(i);
                    enemy.SetAutomaticTick(false);
                    enemy.TakeDamage(enemy.Health);
                }
            }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.WaveIndex, Is.EqualTo(1));
            yield return null; yield return null;

            var entrance = scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance);
            // Beside and in front of the opaque portal: an actor behind it cannot prove arrow readability.
            scene.PlayerMover.transform.position = entrance + Vector2.right * 3f + Vector2.down * 2f;
            var camera = scene.CameraFollow.BoundCamera;
            var approvedRotation = camera.transform.rotation;
            camera.orthographicSize = 5f;
            camera.transform.position = scene.PlayerMover.transform.position - camera.transform.forward * 30f;
            Time.captureFramerate = 20; // One rendered step per manual .05-second rule tick.
            Stage2Shot latestHeroArrow = null;
            game.Enemies.ShotLaunched += shot => latestHeroArrow = shot;
            Assert.That(Stage3TestCycle.StartNight(run), Is.True);
            var seconds = 0f;
            var capturedArrow = false;
            for (var frame = 0; frame < 360; frame++)
            {
                run.Tick(.05f);
                for (var i = game.Enemies.ActiveCount - 1; i >= 0; i--)
                {
                    var enemy = game.Enemies.GetEnemy(i);
                    enemy.SetAutomaticTick(false);
                    enemy.Tick(.05f);
                }
                game.HeroCombat.Tick(.05f);
                seconds += .05f;
                yield return null; // Real sprite binding, walk frames, arrow presentation and ambient visuals.
                if (seconds < 15.5f || latestHeroArrow == null || latestHeroArrow.Progress < .2f ||
                    latestHeroArrow.Progress > .8f || game.Enemies.ActiveCount <= 70) continue;
                capturedArrow = GameObject.Find("Arrow in flight") != null;
                if (capturedArrow) break;
            }
            Assert.That(capturedArrow, Is.True, "A real hero-origin projectile must be in flight around 16 seconds.");
            Assert.That(game.Enemies.ActiveCount, Is.GreaterThan(70));
            Assert.That(Vector2.Distance(latestHeroArrow.Origin, scene.PlayerMover.transform.position), Is.LessThan(.001f));
            Assert.That(game.Building.Defense.TowerCount, Is.Zero);
            var enemies = Enumerable.Range(0, game.Enemies.ActiveCount).Select(game.Enemies.GetEnemy).ToArray();
            foreach (var enemy in enemies)
            {
                var sprites = enemy.GetComponentsInChildren<SpriteRenderer>().Where(r => r.enabled).ToArray();
                Assert.That(sprites.Length, Is.GreaterThan(0), "Every live enemy needs its real skeleton artwork.");
                foreach (var sprite in sprites)
                {
                    Assert.That(sprite.sprite, Is.Not.Null);
                    Assert.That(sprite.sprite.texture, Is.Not.Null);
                    Assert.That(sprite.sharedMaterial.shader.isSupported, Is.True);
                    Assert.That(sprite.sharedMaterial.shader.name, Does.Not.Contain("InternalErrorShader"));
                }
            }

            // Fit the actual population at the approved angle and within the existing 5..10 overview zoom.
            camera.aspect = 1600f / 900f;
            var reference = scene.PlayerMover.transform.position;
            var points = enemies.Select(e => e.transform.position + Vector3.back * .7f)
                .Concat(new[] { reference, (Vector3)entrance, (Vector3)entrance + Vector3.back * 2.5f }).ToArray();
            var horizontal = points.Select(p => Vector3.Dot(p - reference, camera.transform.right)).ToArray();
            var vertical = points.Select(p => Vector3.Dot(p - reference, camera.transform.up)).ToArray();
            var center = reference + camera.transform.right * ((horizontal.Min() + horizontal.Max()) * .5f) +
                camera.transform.up * ((vertical.Min() + vertical.Max()) * .5f);
            camera.orthographicSize = Mathf.Clamp(Mathf.Max((vertical.Max() - vertical.Min()) * .5f + 1f,
                (horizontal.Max() - horizontal.Min()) / (2f * camera.aspect) + 1f), 5f, 10f);
            camera.transform.position = center - camera.transform.forward * 30f;
            var heroViewport = camera.WorldToViewportPoint(reference);
            camera.transform.position += camera.transform.up *
                ((heroViewport.y - Mathf.Clamp(heroViewport.y, .18f, .82f)) * camera.orthographicSize * 2f) +
                camera.transform.right * ((heroViewport.x - Mathf.Clamp(heroViewport.x, .18f, .82f)) *
                    camera.orthographicSize * 2f * camera.aspect);
            var visible = enemies.Count(e =>
            {
                var viewport = camera.WorldToViewportPoint(e.transform.position);
                return viewport.z > 0f && viewport.x > 0f && viewport.x < 1f && viewport.y > 0f && viewport.y < 1f;
            });
            Assert.That(visible, Is.GreaterThan(50), "Overview must show the dense live population.");
            Assert.That(Quaternion.Angle(camera.transform.rotation, approvedRotation), Is.LessThan(.001f));
            Capture(camera, "hero-horde-overview.png", true);

            var overviewZoom = camera.orthographicSize;
            var screenshot = "not requested: batchmode has no reliable full Game-view backbuffer";
            if (!Application.isBatchMode)
            {
                var screenshotPath = Path.Combine(Evidence, "hero-horde-full-game.png");
                ScreenCapture.CaptureScreenshot(screenshotPath);
                yield return null; yield return null; yield return null;
                screenshot = File.Exists(screenshotPath) ? screenshotPath : "full Game-view capture unavailable";
            }
            // Same frozen projectile and real camera; close view makes its equipped-hero origin inspectable.
            camera.orthographicSize = 2.5f;
            var arrowMidpoint = ((Vector3)latestHeroArrow.Origin + (Vector3)latestHeroArrow.Position) * .5f;
            camera.transform.position = arrowMidpoint + Vector3.back * .45f - camera.transform.forward * 30f;
            var withArrow = Capture(camera, "hero-arrow-close.png", true);
            var arrowRenderer = GameObject.Find("Arrow in flight").GetComponent<MeshRenderer>();
            var priorVisibility = arrowRenderer.enabled;
            var visibleArrowPixels = 0;
            try
            {
                arrowRenderer.enabled = false;
                var withoutArrow = Capture(camera, null, true);
                for (var i = 0; i < withArrow.Length; i++)
                    if (Mathf.Abs(withArrow[i].r - withoutArrow[i].r) +
                        Mathf.Abs(withArrow[i].g - withoutArrow[i].g) +
                        Mathf.Abs(withArrow[i].b - withoutArrow[i].b) > 24) visibleArrowPixels++;
            }
            finally { arrowRenderer.enabled = priorVisibility; }
            Assert.That(visibleArrowPixels, Is.GreaterThan(8),
                "The actual arrow mesh must visibly change the final frame, not be hidden behind the portal.");
            File.WriteAllText(Path.Combine(Evidence, "context.txt"),
                "LOCAL PRESENTATION QA, not a performance benchmark or natural first-wave playthrough.\n" +
                "First wave advanced with immediate QA kills only; authored second-wave spawning, movement and hero bow values unchanged.\n" +
                $"second-wave seconds={seconds:F2}; live={enemies.Length}; visible overview={visible}; " +
                $"hero shots={game.HeroCombat.Rules.ShotsFired}; defenses=0; arrow progress={latestHeroArrow.Progress:F3}; " +
                $"visible arrow pixels={visibleArrowPixels}; overview zoom={overviewZoom:F2}; close zoom=2.5; rotation={approvedRotation.eulerAngles}; " +
                $"graphics={SystemInfo.graphicsDeviceName}/{SystemInfo.graphicsDeviceType}; full screenshot={screenshot}\n");
        }

        private static Color32[] Capture(Camera camera, string filename, bool requireHero = false)
        {
            var target = new RenderTexture(1600, 900, 24);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var texture = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                texture.Apply();
                if (filename != null) File.WriteAllBytes(Path.Combine(Evidence, filename), texture.EncodeToPNG());
                var pixels = texture.GetPixels32();
                var magenta = pixels.Count(p => p.r > 245 && p.b > 245 && p.g < 10);
                Assert.That(magenta, Is.LessThan(5), "Actual camera image must not contain missing-shader magenta.");
                if (requireHero)
                {
                    var body=GameObject.Find("Hero C body");
                    Assert.That(body,Is.Not.Null,"Approved adventurer C must replace the marker in this scene.");
                    var renderer=body.GetComponent<SpriteRenderer>();
                    Color32[] without;
                    try { renderer.enabled=false; without=Capture(camera,null,false); }
                    finally { renderer.enabled=true; }
                    var changed=0;
                    for(var i=0;i<pixels.Length;i++)
                        if(Mathf.Abs(pixels[i].r-without[i].r)+Mathf.Abs(pixels[i].g-without[i].g)+
                           Mathf.Abs(pixels[i].b-without[i].b)>12) changed++;
                    Assert.That(changed,Is.GreaterThan(20),"Approved hero must visibly contribute to the rendered frame.");
                }
                return pixels;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.Destroy(target); Object.Destroy(texture);
            }
        }
    }
}
#endif
