using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Disposable opt-in development-player QA, using actual enemy arrivals and the R binding.</summary>
    public static class Stage3DefeatReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            var game = owner.GetComponent<Stage2GameComposition>();
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var effect = owner.GetComponent<Stage3DefeatPresentation>();
            while (game == null || !game.IsInitialized || effect == null)
            { yield return null; game = owner.GetComponent<Stage2GameComposition>(); effect = owner.GetComponent<Stage3DefeatPresentation>(); }
            for (var i = 0; i < 8; i++) yield return null;
            var camera = scene.CameraFollow.BoundCamera; var actor = scene.PlayerMover.transform;
            var previousHero = actor.position; var previousCamera = camera.transform.position;
            var previousZoom = camera.orthographicSize; var previousFollow = scene.CameraFollow.enabled;
            var previousCombat = game.HeroCombat.AutomaticTick; var previousRate = Time.captureFramerate;
            var previousBackground = InputSystem.settings.backgroundBehavior;
            var keyboard = InputSystem.AddDevice<Keyboard>("Defeat review keyboard");
            Directory.CreateDirectory(directory);
            var context = new StringBuilder($"Actual development-player Game backbuffer {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceType}.\n" +
                "Disposable opt-in QA. Damage uses actual EnemyGroup.SpawnAt(throne) and Enemy.Tick arrival; R uses synthetic InputSystem keyboard. Camera/hero/inventory setup is temporary, not a natural playthrough.\n");
            void Require(bool value, string message)
            { if (!value) throw new InvalidOperationException("Defeat player QA: " + message); context.AppendLine("PASS " + message); }
            void Arrive()
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            void Frame(Vector2 target)
            { camera.orthographicSize = 5f; camera.transform.position = (Vector3)target + Vector3.back * .55f - camera.transform.forward * 30f; }
            var stock = game.Gathering.Economy.Inventory;
            try
            {
                Time.captureFramerate = 60; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                game.HeroCombat.AutomaticTick = false; scene.CameraFollow.enabled = false;
                game.RunDriver.ResetRun();
                stock.TryAdd(MapResourceKind.Wood, 17); stock.TryAdd(MapResourceKind.Stone, 9); stock.TryAdd(MapResourceKind.Metal, 8);
                actor.position = scene.ThroneMarker.position + new Vector3(5f, -2f, 0);
                Frame((Vector2)scene.ThroneMarker.position + Vector2.right * 2f);
                yield return null; yield return null;
                var savedHero = actor.position; var savedWood = stock.Get(MapResourceKind.Wood);
                game.RunDriver.Run.StartWave(); yield return Shot(directory, "00-before-hit");
                // The first backbuffer readback can stall a development player for longer
                // than the intentionally short shake. Sample a settled frame after it.
                for (var i = 0; i < 8; i++) yield return null;
                var cameraPosition = camera.transform.position;
                Arrive(); yield return null;
                Require(effect.HitSequence == 1 && effect.HasVisibleFractures, "visible first-hit radial pulse");
                Require(camera.transform.position == cameraPosition, "damage preserves logical camera transform");
                Require(effect.ShakePixels.sqrMagnitude > 0f, "visible damage produces short render shake");
                yield return Shot(directory, "01-first-hit");
                yield return new WaitForSecondsRealtime(.25f); yield return Shot(directory, "02-first-hit-fading");
                yield return new WaitForSecondsRealtime(.8f);
                Require(effect.ActiveHitCount == 0 && !effect.HasVisibleFractures, "nonfatal fractures completely expire");
                yield return Shot(directory, "03-hit-cleared");
                Arrive(); yield return Shot(directory, "04-second-hit");
                yield return new WaitForSecondsRealtime(.8f);
                Frame((Vector2)scene.ThroneMarker.position + Vector2.one * 100f); yield return null;
                for (var i = 0; i < 8; i++) yield return null;
                Arrive(); yield return null;
                Require(!effect.HasVisibleFractures && effect.ShakePixels.sqrMagnitude > 0f, "offscreen damage shakes without false-origin fractures");
                yield return Shot(directory, "05-offscreen-hit");
                Arrive(); Arrive();
                Require(game.RunDriver.Run.IsTerminal && !game.RunDriver.CanRetry, "fatal arrival stops game and gates early retry");
                stock.TryAdd(MapResourceKind.Wood, 71);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                Require(game.RunDriver.Run.IsTerminal, "R cannot bypass unfinished loss transition");
                yield return LossShot(effect, directory, "06-offscreen-fatal", .25f);
                yield return LossShot(effect, directory, "07-offscreen-white", 1.66f);
                Require(effect.Whiteout >= .999f, "offscreen fatal reaches solid white");
                yield return LossShot(effect, directory, "08-offscreen-retry", 2.2f);
                yield return Retry(game, keyboard);
                Require(!game.RunDriver.Run.IsTerminal && game.RunDriver.Run.WaveNumber == 1 && actor.position == savedHero,
                    "real R binding restores failed wave and saved hero");
                Require(stock.Get(MapResourceKind.Wood) == savedWood && effect.ActiveHitCount == 0 && effect.Whiteout == 0,
                    "retry removes failed-future stock and clears effect");
                Frame((Vector2)scene.ThroneMarker.position + Vector2.right * 2f);
                yield return Shot(directory, "09-retried-world");
                for (var i = 0; i < 8; i++) yield return null;
                stock.TryAdd(MapResourceKind.Wood, 3); savedWood += 3;
                game.RunDriver.Run.StartWave(); stock.TryAdd(MapResourceKind.Wood, 50);
                // Prepare the simultaneous contacts before measuring their short feedback;
                // four development-player prefab instantiations can stall one frame > .65s.
                var heavyEnemies = Enumerable.Range(0, game.RunDriver.Run.ThroneMaximum - 1)
                    .Select(_ => game.Enemies.SpawnAt(scene.MapDefinition.Throne)).ToArray();
                foreach (var enemy in heavyEnemies) enemy.SetAutomaticTick(false);
                for (var i = 0; i < 8; i++) yield return null;
                foreach (var enemy in heavyEnemies) enemy.Tick(.01f);
                yield return null;
                Require(effect.ActiveHitCount == heavyEnemies.Length && effect.HasVisibleFractures && effect.LastHitDamageFraction >= .79f,
                    $"heavy visible damage keeps distinct transient pulses and increases strength (hits={effect.ActiveHitCount}, visible={effect.HasVisibleFractures}, fraction={effect.LastHitDamageFraction}, sequence={effect.HitSequence}, hp={game.RunDriver.Run.ThroneHealth}, delta={Time.unscaledDeltaTime})");
                yield return Shot(directory, "10-heavy-visible-hit");
                Arrive();
                yield return LossShot(effect, directory, "11-visible-fatal-early", .25f);
                yield return LossShot(effect, directory, "12-visible-fatal-fracture", .9f);
                yield return LossShot(effect, directory, "13-visible-white", 1.66f);
                yield return LossShot(effect, directory, "14-visible-retry", 2.2f);
                yield return Retry(game, keyboard);
                Require(stock.Get(MapResourceKind.Wood) == savedWood && game.RunDriver.Run.WaveNumber == 1,
                    "second defeat retries newly captured preparation rather than the older checkpoint");
                Require(!effect.RenderViewOverridden, "render view restored after transition and retry");
                if (Environment.GetEnvironmentVariable("NOT3A_DEFEAT_REVIEW_NATIVE_CLICK") == "1")
                {
                    // Leave a third real loss on screen for an ordinary native pointer click.
                    game.RunDriver.Run.StartWave();
                    for (var hit = 0; hit < game.RunDriver.Run.ThroneMaximum; hit++) Arrive();
                    while (!game.RunDriver.CanRetry) yield return null;
                    yield return Shot(directory, "15-native-button-ready");
                    Debug.Log("S3_DEFEAT_NATIVE_BUTTON_READY");
                    var timeout = Time.realtimeSinceStartup + 240f;
                    while (game.RunDriver.Run.IsTerminal && Time.realtimeSinceStartup < timeout) yield return null;
                    Require(!game.RunDriver.Run.IsTerminal && stock.Get(MapResourceKind.Wood) == savedWood,
                        "native pointer retry restores the same checkpoint");
                    yield return Shot(directory, "16-native-button-restored");
                    Debug.Log("S3_DEFEAT_NATIVE_BUTTON_PASSED");
                }
                File.WriteAllText(Path.Combine(directory, "context.txt"), context.ToString());
                Debug.Log("S3_DEFEAT_REWIND_PLAYER_CAPTURE_COMPLETE " + directory);
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard); game.RunDriver.ResetRun(); game.HeroCombat.AutomaticTick = previousCombat;
                actor.position = previousHero; camera.orthographicSize = previousZoom; camera.transform.position = previousCamera;
                scene.CameraFollow.enabled = previousFollow; Time.captureFramerate = previousRate;
                InputSystem.settings.backgroundBehavior = previousBackground;
            }
        }
        private static IEnumerator Retry(Stage2GameComposition game, Keyboard keyboard)
        {
            var timeout = Time.realtimeSinceStartup + 8f;
            while (!game.RunDriver.CanRetry && Time.realtimeSinceStartup < timeout) yield return null;
            if (!game.RunDriver.CanRetry) throw new InvalidOperationException("Defeat player QA: retry did not become ready.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            for (var i = 0; i < 5 && game.RunDriver.Run.IsTerminal; i++) yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
        }
        private static IEnumerator LossShot(Stage3DefeatPresentation effect, string directory, string name, float age)
        {
            while (effect.LossElapsed < age) yield return null;
            yield return Shot(directory, name);
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
