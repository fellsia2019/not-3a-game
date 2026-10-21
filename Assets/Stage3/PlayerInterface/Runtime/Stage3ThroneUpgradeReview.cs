using System;
using System.Collections;
using System.IO;
using System.Text;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit development QA only; normal startup never grants funds or damages/upgrades the throne.</summary>
    public static class Stage3ThroneUpgradeReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            var game = owner.GetComponent<Stage2GameComposition>(); var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>(); var binding = owner.GetComponent<Stage3PlayerInterfaceBinding>();
            for (var i = 0; i < 14; i++) yield return null;
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            var run = game.RunDriver.Run; var wallet = game.Gathering.Economy.Inventory;
            wallet.TryCredit(new Stage2ResourceCost(5000, 5000, 5000, 5000)); scene.CameraFollow.enabled = false;
            var camera = scene.CameraFollow.BoundCamera;
            var report = new StringBuilder("Native Mac development-player throne reinforcement. Explicit QA funds/damage, frozen clock. Normal start unchanged.\n");
            void Select()
            {
                var sprite = scene.ThroneMarker.GetComponentInChildren<Stage3LandmarkSprite>().Body;
                camera.orthographicSize = 5; camera.transform.position = sprite.bounds.center - camera.transform.forward * 30;
                var p = camera.WorldToScreenPoint(sprite.bounds.center); var wanted = new Vector2(Screen.width * .5f, Screen.height * .72f);
                camera.transform.position += sprite.bounds.center - camera.ScreenToWorldPoint(new Vector3(wanted.x, wanted.y, p.z));
                if (!binding.TrySelectBuildingAtScreen(camera.WorldToScreenPoint(sprite.bounds.center)))
                    throw new InvalidOperationException("Native throne visual selection failed.");
            }
            void Hit()
            { var enemy = game.Enemies.SpawnAt(scene.MapDefinition.Throne); enemy.SetAutomaticTick(false); enemy.Tick(.01f); }
            void Upgrade()
            {
                var old = run.ThroneHealth; var max = run.ThroneMaximum; var gold = wallet.Gold;
                if (binding.TryUpgradeSelectedThrone() != Stage2ThroneUpgradeResult.Upgraded || run.ThroneHealth != old + 5 || run.ThroneMaximum != max + 5)
                    throw new InvalidOperationException("Native throne paid upgrade/health transfer failed.");
                report.AppendLine($"Upgrade{run.ThroneUpgradeCount}/3 HP{old}/{max}→{run.ThroneHealth}/{run.ThroneMaximum} gold={gold}→{wallet.Gold}");
            }
            Screen.SetResolution(1600, 900, false); yield return new WaitForSecondsRealtime(.4f);
            hud.CloseWindow(); Select(); yield return Shot(directory, "00-base-5of5");
            Hit(); Hit(); yield return new WaitForSecondsRealtime(1.5f); Select(); yield return Shot(directory, "01-damaged-3of5");
            for (var i = 1; i <= 3; i++) { Upgrade(); yield return Shot(directory, $"0{i + 1}-upgrade{i}-{run.ThroneHealth}of{run.ThroneMaximum}"); }
            var cappedGold = wallet.Gold;
            if (binding.TryUpgradeSelectedThrone() != Stage2ThroneUpgradeResult.MaximumUpgrades || wallet.Gold != cappedGold)
                throw new InvalidOperationException("Native throne cap charged a fourth price.");
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080) })
            {
                Screen.SetResolution(size.x, size.y, false); yield return new WaitForSecondsRealtime(.3f);
                hud.SetWindow(Stage3PlayerWindow.Construction); hud.SetBuildGroup(2); Select();
                yield return Shot(directory, $"05-max-build-{size.x}x{size.y}");
            }
            Screen.SetResolution(1600, 900, false); yield return new WaitForSecondsRealtime(.3f);
            hud.CloseWindow(); game.RunDriver.ResetRun(); Select(); yield return Shot(directory, "06-unfunded");
            wallet.TryCredit(new Stage2ResourceCost(5000, 5000, 5000, 5000)); Hit(); Hit();
            yield return new WaitForSecondsRealtime(1.5f); Select(); yield return Shot(directory, "07-interactive-3of5");
            report.AppendLine($"Interactive actual-mouse target: HP{run.ThroneHealth}/{run.ThroneMaximum}; count={run.ThroneUpgradeCount}/3; button={hud.SelectedBuildingUpgradeRect}; walletGold={wallet.Gold}");
            File.WriteAllText(Path.Combine(directory, "verification.txt"), report.ToString());
            hud.SelectedBuildingUpgradeRequested += () => Debug.Log($"S3_THRONE_NATIVE_TRANSACTION count={run.ThroneUpgradeCount} hp={run.ThroneHealth}/{run.ThroneMaximum} gold={wallet.Gold}");
            Debug.Log("S3_THRONE_UPGRADE_NATIVE_READY " + directory);
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
