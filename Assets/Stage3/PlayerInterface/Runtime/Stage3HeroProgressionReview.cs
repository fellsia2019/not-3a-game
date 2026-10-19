using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit development-player fixture. Normal startup never grants XP or changes the camera.</summary>
    public static class Stage3HeroProgressionReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            var game = owner.GetComponent<Stage2GameComposition>();
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var deadline = Time.realtimeSinceStartup + 15;
            while (owner.GetComponent<Stage2HeroProgressionDriver>() == null && Time.realtimeSinceStartup < deadline) yield return null;
            var driver = owner.GetComponent<Stage2HeroProgressionDriver>();
            if (driver == null) throw new InvalidOperationException("Progression review binding did not initialize.");
            for (var i = 0; i < 4; i++) yield return null;
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; driver.AutomaticTick = false;
            game.RunDriver.ResetRun();
            var rules = driver.Rules; var catalog = driver.Catalog;
            var levelUp = owner.GetComponent<Stage3HeroLevelUpPresentation>();
            if (levelUp == null || !levelUp.IsReady) throw new InvalidOperationException("Approved level-up layers did not load.");
            yield return Shot(directory, "01-starter-hud");
            // Declared QA awards, not a change to starter balance or save data.
            rules.AwardExperience(catalog.RequiredXp(1) + catalog.RequiredXp(2) + 140);
            levelUp.ClearEffects(); // Capture the page independently; celebration is exercised below.
            owner.GetComponent<Stage3HeroProgressionEffects>().ClearEffects();
            rules.TryUpgradePassive(Stage2HeroPassive.LightStep);
            yield return null; hud.SetWindow(Stage3PlayerWindow.Hero);
            yield return Shot(directory, "02-development");
            if (Environment.GetEnvironmentVariable("NOT3A_HERO_PROGRESSION_INTERACTIVE_REVIEW") == "1")
            { Debug.Log("S3_HERO_PROGRESSION_INTERACTIVE_READY: QA LVL3, LightStep1, one available point. Normal start remains LVL1."); yield break; }
            rules.TryUpgradeBlink(); yield return null;
            yield return Shot(directory, "03-blink-rank-two");
            hud.CloseWindow();
            var node = game.Gathering.Economy.Nodes.First(n => n.Resource == MapResourceKind.Stone && !n.IsDepleted);
            var hero = scene.PlayerMover;
            var positioned = false;
            for (var i = 0; i < 32; i++)
            {
                var angle = i * Mathf.PI / 16; var point = node.WorldPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.4f;
                if (!driver.CanLand(point)) continue;
                hero.transform.position = new Vector3(point.x, point.y, hero.transform.position.z); positioned = true; break;
            }
            if (!positioned) throw new InvalidOperationException("No clear resource approach for native Blink review.");
            scene.CameraFollow.SnapToTarget(); yield return null;
            Vector2 target = hero.transform.position; var found = false;
            for (var i = 0; i < 32; i++)
            {
                var angle = i * Mathf.PI / 16; var point = (Vector2)hero.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.8f;
                if (!driver.CanLand(point) || game.ContainsScreenPoint(scene.CameraFollow.BoundCamera.WorldToScreenPoint(point))) continue;
                target = point; found = true; break;
            }
            if (!found) throw new InvalidOperationException("No visible Blink landing.");
            var fx = owner.GetComponent<Stage3HeroProgressionEffects>();
            fx.SetBlinkAim(true, target, rules.BlinkRange, true);
            yield return Shot(directory, "04-valid-landing");
            if (driver.CanLand(node.WorldPosition)) throw new InvalidOperationException("Resource base must reject Blink.");
            fx.SetBlinkAim(true, node.WorldPosition, rules.BlinkRange, false);
            yield return Shot(directory, "05-resource-blocked");
            fx.ClearEffects(); var before = hero.transform.position;
            if (!driver.TryBlink(target)) throw new InvalidOperationException("Valid Blink did not execute.");
            yield return Shot(directory, "06-blink-transition", .07f);
            var victim = game.Enemies.Spawn(); victim.SetAutomaticTick(false); victim.TakeDamage(int.MaxValue);
            yield return Shot(directory, "07-live-xp-cooldown", .12f);
            rules.AwardExperience(rules.RequiredXp - rules.XpWithin);
            yield return Shot(directory, "08-level-up-imprint", .07f);
            yield return Shot(directory, "09-level-up-held", .28f);
            yield return Shot(directory, "10-level-up-waves", .65f);
            yield return Shot(directory, "11-level-up-fading", .70f);
            yield return Shot(directory, "12-level-up-clear", 1f);
            if (levelUp.IsPlaying) throw new InvalidOperationException("Level-up celebration failed to expire.");
            rules.AwardExperience(int.MaxValue);
            yield return Shot(directory, "13-level-100", .4f);
            var source = owner.GetComponent<AudioSource>();
            if (!levelUp.HasAudioClip || source == null || source.clip == null || source.loop || source.spatialBlend != 0 ||
                scene.CameraFollow.BoundCamera.GetComponent<AudioListener>() == null)
                throw new InvalidOperationException("Level-up 2D sound cue/listener did not bind or trigger.");
            levelUp.ClearEffects(); hud.SetWindow(Stage3PlayerWindow.Hero);
            yield return Shot(directory, "14-cap-development");
            File.WriteAllText(Path.Combine(directory, "verification.txt"),
                $"Actual Mac development-player renders {Screen.width}x{Screen.height}. Declared QA XP awards; normal start unchanged.\n" +
                $"level={rules.Level} xp={rules.XpWithin}/{rules.RequiredXp} points={rules.AvailablePoints} blinkRank={rules.BlinkRank}\n" +
                $"blinkDistance={Vector2.Distance(before, hero.transform.position):0.000} cooldown={rules.BlinkCooldownRemaining:0.000}\n" +
                "Resource-centre landing rejected; level-up imprint/hold/waves/fade/expiry and level100 rendered.\n" +
                $"audioClipLoaded={levelUp.HasAudioClip} triggeredSource={source != null} clipSeconds={source.clip.length:0.000} loop={source.loop} spatialBlend={source.spatialBlend}\n");
            Debug.Log("S3_HERO_PROGRESSION_REVIEW_OK " + directory);
        }
        private static IEnumerator Shot(string directory, string name, float delay = .15f)
        {
            yield return new WaitForSecondsRealtime(delay); yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
