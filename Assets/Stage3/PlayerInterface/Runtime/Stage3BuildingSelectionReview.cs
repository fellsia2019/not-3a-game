using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit opt-in disposable player QA. Never enabled in an ordinary game session.</summary>
    public static class Stage3BuildingSelectionReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            var game = owner.GetComponent<Stage2GameComposition>();
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var binding = owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while (!game.IsInitialized || !hud.IsReady) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
            Directory.CreateDirectory(directory);
            var report = new StringBuilder($"Actual development player {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceType}.\n" +
                "Disposable opt-in setup: stocked construction, fixed camera and scripted selection of real built objects. Not a natural playthrough.\n");
            void Require(bool value, string message)
            { if (!value) throw new InvalidOperationException("Building selection QA: " + message); report.AppendLine("PASS " + message); }
            var camera = scene.CameraFollow.BoundCamera;
            var originalRate = Time.captureFramerate;
            var originalFollow = scene.CameraFollow.enabled;
            var originalCombat = game.HeroCombat.AutomaticTick;
            var originalCamera = camera.transform.position;
            var originalSize = camera.orthographicSize;
            var originalHero = scene.PlayerMover.transform.position;
            void Frame(Vector3 position)
            {
                camera.orthographicSize = 5f;
                camera.transform.position = position - camera.transform.forward * 30f;
                var target = new Vector2(Screen.width * .55f, Screen.height * .65f);
                var projected = camera.WorldToScreenPoint(position);
                camera.transform.position += position - camera.ScreenToWorldPoint(new Vector3(target.x, target.y, projected.z));
            }
            void Select(Renderer visual, Stage3BuildingSelectionKind expected)
            {
                Frame(visual.bounds.center);
                var center = (Vector2)camera.WorldToScreenPoint(visual.bounds.center);
                var selected = binding.TrySelectBuildingAtScreen(center) && binding.SelectedBuildingKind == expected;
                // Coherent production art can have a transparent center. Click an actual
                // opaque texel rather than treating its padded image rectangle as a model.
                if (!selected && visual is SpriteRenderer spriteRenderer && spriteRenderer.sprite.texture.isReadable)
                {
                    var sprite = spriteRenderer.sprite; var rect = sprite.textureRect;
                    for (var y = 3; y <= 17 && !selected; y++)
                        for (var x = 3; x <= 17 && !selected; x++)
                        {
                            var u = x / 20f; var v = y / 20f;
                            var px = Mathf.Clamp((int)(rect.x + u * rect.width), 0, sprite.texture.width - 1);
                            var py = Mathf.Clamp((int)(rect.y + v * rect.height), 0, sprite.texture.height - 1);
                            if (sprite.texture.GetPixel(px, py).a < .6f) continue;
                            var bounds = sprite.bounds;
                            var world = visual.transform.TransformPoint(new Vector3(
                                Mathf.Lerp(bounds.min.x, bounds.max.x, u), Mathf.Lerp(bounds.min.y, bounds.max.y, v), 0));
                            selected = binding.TrySelectBuildingAtScreen(camera.WorldToScreenPoint(world)) && binding.SelectedBuildingKind == expected;
                        }
                }
                Require(selected,
                    "select live " + expected + " by its visible model");
            }
            try
            {
                Time.captureFramerate = 60; scene.CameraFollow.enabled = false; game.HeroCombat.AutomaticTick = false;
                game.RunDriver.ResetRun();
                var inventory = game.Gathering.Economy.Inventory;
                foreach (MapResourceKind resource in Enum.GetValues(typeof(MapResourceKind))) inventory.TryAdd(resource, 300);
                inventory.TryAddGold(300);
                scene.PlayerMover.transform.position = scene.ThroneMarker.position + Vector3.right * 5f;
                Frame(scene.ThroneMarker.position);
                yield return Shot(directory, "00-normal");
                Require(hud.AbilityReserveRect(9).width > 0 && hud.NavigationRibbonRect.yMax == hud.BottomRect.yMin,
                    "ten reserves and separate connected ribbon");
                var defense = game.Building.Defense; defense.SelectRole(0);
                Require(defense.BeginPlacement(), "tower placement begins");
                var legal = scene.MapDefinition.Throne;
                var found = false;
                for (var radius = 1; radius < 16 && !found; radius++)
                    for (var x = -radius; x <= radius && !found; x++)
                        for (var y = -radius; y <= radius && !found; y++)
                        {
                            var cell = scene.MapDefinition.Throne + new Vector2Int(x, y);
                            if (defense.Evaluate(cell) != PlacementResult.Ready) continue;
                            legal = cell; found = true;
                        }
                Require(found, "actual legal tower site");
                defense.SetPreviewCell(legal); Require(defense.TryConfirm(out var tower) == PlacementResult.Ready, "paid tower built once");
                yield return null; yield return null;
                var towerBody = tower.GetComponentsInChildren<SpriteRenderer>().First(r => r.enabled && r.gameObject.activeInHierarchy);
                Select(towerBody, Stage3BuildingSelectionKind.Tower);
                yield return Shot(directory, "01-tower");
                tower.TakeDamage(2); yield return null;
                Require(hud.SelectedBuildingView.Stats.Any(s => s.Label == "Здоровье" && s.Value == tower.Health + "/" + tower.MaximumHealth), "live tower HP");
                yield return Shot(directory, "02-tower-damaged");
                binding.ClearBuildingSelection();
                hud.SetWindow(Stage3PlayerWindow.Construction);
                yield return Shot(directory, "03-construction");
                hud.CloseWindow();
                var travel = owner.GetComponent<Stage3WorldTravel>();
                travel.BeginWorkshopPlacement();
                Require(travel.TryBuildWorkshopAt(travel.WorkshopPosition) == WorkshopPlacementResult.Built, "actual workshop built");
                yield return null;
                Select(travel.WorkshopVisual, Stage3BuildingSelectionKind.Workshop);
                yield return Shot(directory, "04-workshop");
                hud.SetWindow(Stage3PlayerWindow.Research);
                Require(!hud.HasSelectedBuilding && !travel.IsResearched, "research overview clears selection without purchasing");
                yield return Shot(directory, "05-research");
                hud.CloseWindow();
                var production = game.Production;
                var pointIndex = 0;
                scene.PlayerMover.transform.position = scene.ThroneMarker.position + Vector3.right * 5f;
                production.SelectAndBeginPlacement(production.Rules.Points[pointIndex].Building);
                Require(production.TryBuildAt(pointIndex) == Stage2.Economy.ProductionPlacementResult.Built, "actual generator built");
                hud.CloseWindow();
                yield return null;
                var building = production.Rules.Buildings[0];
                Select(production.GetBuiltVisual(building).GetComponent<Renderer>(), Stage3BuildingSelectionKind.Production);
                yield return Shot(directory, "06-production");
                binding.ClearBuildingSelection();
                var dwelling = owner.GetComponent<Stage3Dwelling>();
                dwelling.BeginPlacement(); var homeFound = false;
                for (var x = -14; x <= 14 && !homeFound; x += 2)
                    for (var y = -14; y <= 14 && !homeFound; y += 2)
                    {
                        var site = (Vector2)scene.ThroneMarker.position + new Vector2(x, y);
                        if (dwelling.EvaluateAt(site) != DwellingPlacementResult.Ready) continue;
                        homeFound = dwelling.TryBuildAt(site) == DwellingPlacementResult.Built;
                    }
                Require(homeFound, "actual dwelling built"); yield return null;
                Select(owner.GetComponent<Stage3DwellingPresentation>().BuiltVisual, Stage3BuildingSelectionKind.Dwelling);
                yield return Shot(directory, "07-dwelling");
                binding.ClearBuildingSelection();
                var dome = scene.ThroneMarker.GetComponentInChildren<Stage3LandmarkSprite>();
                Select(dome.Body, Stage3BuildingSelectionKind.Throne);
                yield return Shot(directory, "08-throne");
                game.SetPaused(true); yield return null;
                Require(!hud.HasSelectedBuilding, "pause clears selection"); game.SetPaused(false);
                Select(towerBody, Stage3BuildingSelectionKind.Tower);
                game.RunDriver.ResetRun(); yield return null;
                Require(!hud.HasSelectedBuilding, "reset clears stale selection");
                if (Environment.GetEnvironmentVariable("NOT3A_BUILDING_SELECTION_NATIVE_CLICK") == "1")
                {
                    travel.BeginWorkshopPlacement();
                    Require(travel.TryBuildWorkshopAt(travel.WorkshopPosition) == WorkshopPlacementResult.Built, "native-button workshop setup");
                    yield return null;
                    Select(travel.WorkshopVisual, Stage3BuildingSelectionKind.Workshop);
                    yield return Shot(directory, "09-native-close-ready");
                    Debug.Log("S3_BUILDING_NATIVE_CLOSE_READY");
                    var deadline = Time.realtimeSinceStartup + 120f;
                    while (hud.HasSelectedBuilding && Time.realtimeSinceStartup < deadline) yield return null;
                    Require(!hud.HasSelectedBuilding, "native close button clears card and binding");
                    Select(travel.WorkshopVisual, Stage3BuildingSelectionKind.Workshop);
                    yield return Shot(directory, "10-native-research-ready");
                    Debug.Log("S3_BUILDING_NATIVE_RESEARCH_READY");
                    deadline = Time.realtimeSinceStartup + 120f;
                    while (hud.ActiveWindow != Stage3PlayerWindow.Research && Time.realtimeSinceStartup < deadline) yield return null;
                    Require(hud.ActiveWindow == Stage3PlayerWindow.Research && !travel.IsResearched && !hud.HasSelectedBuilding,
                        "native research button opens overview and never buys remotely");
                    yield return Shot(directory, "11-native-research-opened");
                }
                File.WriteAllText(Path.Combine(directory, "verification.txt"), report.ToString());
                Debug.Log("S3_BUILDING_SELECTION_REVIEW_OK " + directory);
            }
            finally
            {
                game.SetPaused(false); game.RunDriver.ResetRun(); binding.ClearBuildingSelection(); hud.CloseWindow();
                camera.transform.position = originalCamera; camera.orthographicSize = originalSize;
                scene.PlayerMover.transform.position = originalHero; scene.CameraFollow.enabled = originalFollow;
                game.HeroCombat.AutomaticTick = originalCombat; Time.captureFramerate = originalRate;
            }
        }

        private static IEnumerator Shot(string directory, string name)
        {
            for (var i = 0; i < 4; i++) yield return null;
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
            UnityEngine.Object.Destroy(image); yield return null;
        }
    }
}
