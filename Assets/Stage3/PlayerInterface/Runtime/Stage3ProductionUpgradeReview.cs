using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in development fixture; normal startup never funds/builds/upgrades generators.</summary>
    public static class Stage3ProductionUpgradeReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            var game = owner.GetComponent<Stage2GameComposition>(); var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>(); var binding = owner.GetComponent<Stage3PlayerInterfaceBinding>();
            for (var i = 0; i < 14; i++) yield return null;
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            var production = game.Production; var wallet = game.Gathering.Economy.Inventory;
            wallet.TryCredit(new Stage2ResourceCost(5000, 5000, 5000, 5000)); scene.CameraFollow.enabled = false;
            var camera = scene.CameraFollow.BoundCamera;
            var report = new StringBuilder("Native Mac development-player production upgrades. Explicit QA funds/buildings; frozen clock/automatic production. Normal start unchanged.\n");
            Stage2ProductionBuilding Build(ResourceProductionBuilding kind)
            {
                binding.ClearBuildingSelection();
                var index = Enumerable.Range(0, production.Rules.Points.Count).First(i =>
                    production.Rules.Points[i].Building == kind && !production.Rules.IsOccupied(i));
                scene.PlayerMover.transform.position = production.Rules.Points[index].Position + Vector2.one * 5;
                production.SelectAndBeginPlacement(kind);
                if (production.TryBuildAt(index) != ProductionPlacementResult.Built) throw new InvalidOperationException("Native generator build failed.");
                return production.Rules.Buildings.Last();
            }
            void Select(Stage2ProductionBuilding building)
            {
                var sprite = production.GetBuiltVisual(building).GetComponent<SpriteRenderer>();
                camera.orthographicSize = 5; camera.transform.position = sprite.bounds.center - camera.transform.forward * 30;
                var p = camera.WorldToScreenPoint(sprite.bounds.center); var wanted = new Vector2(Screen.width * .5f, Screen.height * .72f);
                camera.transform.position += sprite.bounds.center - camera.ScreenToWorldPoint(new Vector3(wanted.x, wanted.y, p.z));
                if (!binding.TrySelectBuildingAtScreen(camera.WorldToScreenPoint(sprite.bounds.center)))
                    throw new InvalidOperationException("Native generator visual selection failed.");
            }
            void Upgrade(Stage2ProductionBuilding building)
            {
                var oldLevel = building.Level; var oldGold = wallet.Gold; var progress = building.ProgressSeconds;
                if (binding.TryUpgradeSelectedProduction() != ProductionUpgradeResult.Upgraded)
                    throw new InvalidOperationException("Native generator card upgrade failed.");
                if (building.ProgressSeconds != progress || building.Level != oldLevel + 1)
                    throw new InvalidOperationException("Upgrade changed cycle fraction or wrong level.");
                report.AppendLine($"{building.Entry.DisplayName} {oldLevel}→{building.Level} gold={oldGold}→{wallet.Gold} fraction={progress:0.000} retained");
            }
            var built = new Stage2ProductionBuilding[3];
            for (var i = 0; i < 3; i++) built[i] = Build((ResourceProductionBuilding)i);
            production.enabled = false; Screen.SetResolution(1600, 900, false); yield return new WaitForSecondsRealtime(.4f);
            for (var i = 0; i < 3; i++)
            {
                hud.CloseWindow(); Select(built[i]); yield return Shot(directory, $"{i}-level1-1600x900");
                Upgrade(built[i]); yield return Shot(directory, $"{i}-level2-1600x900");
                for (var step = 0; step < 3; step++) Upgrade(built[i]);
                yield return Shot(directory, $"{i}-level5-1600x900");
            }
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080) })
            {
                Screen.SetResolution(size.x, size.y, false); yield return new WaitForSecondsRealtime(.3f);
                for (var i = 0; i < 3; i++)
                {
                    hud.SetWindow(Stage3PlayerWindow.Construction); hud.SetBuildGroup(1); Select(built[i]);
                    yield return Shot(directory, $"{i}-max-build-{size.x}x{size.y}");
                }
            }
            Screen.SetResolution(1600, 900, false); yield return new WaitForSecondsRealtime(.3f); hud.CloseWindow();
            var interactive = Build(ResourceProductionBuilding.Sawmill); production.enabled = false; Select(interactive); Upgrade(interactive);
            var gold = wallet.Gold; wallet.TrySpend(new Stage2ResourceCost(0, 0, 0, gold));
            yield return Shot(directory, "unfunded-level2"); wallet.TryAddGold(gold); yield return null;
            yield return Shot(directory, "interactive-level2");
            report.AppendLine($"Interactive actual-mouse target: {interactive.Entry.DisplayName} LVL{interactive.Level}; upgrade={hud.SelectedBuildingUpgradeRect}; walletGold={wallet.Gold}");
            File.WriteAllText(Path.Combine(directory, "verification.txt"), report.ToString());
            hud.SelectedBuildingUpgradeRequested += () => Debug.Log($"S3_PRODUCTION_NATIVE_TRANSACTION level={interactive.Level} output={interactive.OutputAmount} gold={wallet.Gold}");
            Debug.Log("S3_PRODUCTION_UPGRADE_NATIVE_READY " + directory);
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
