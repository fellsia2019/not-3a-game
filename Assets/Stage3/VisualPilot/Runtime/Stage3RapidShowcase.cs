using System;
using System.Collections;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Places both paid visual pilots for immediate inspection in the isolated game scene.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage3RapidShowcase : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null; // Let Stage 2 composition initialize its normal run first.
            var composition = GetComponent<Stage2GameComposition>();
            if (composition == null || !composition.IsInitialized)
            {
                Debug.LogError("S3_RAPID_GAME_ERROR: Stage 2 game composition was not initialized.", this);
                yield break;
            }

            var defense = composition.Building.Defense;
            var inventory = composition.Gathering.Economy.Inventory;
            for (var role = 0; role < 2; role++)
            {
                defense.SelectRole(role);
                var catalog = defense.SelectedCatalog;
                AddMissing(inventory, MapResourceKind.Wood, catalog.WoodCost);
                AddMissing(inventory, MapResourceKind.Stone, catalog.StoneCost);
                AddMissing(inventory, MapResourceKind.Metal, catalog.MetalCost);
                if (inventory.Gold < catalog.GoldCost)
                    inventory.TryAddGold(catalog.GoldCost - inventory.Gold);
                var target=composition.GetComponent<Stage2SceneBootstrap>().MapDefinition.Throne;
                var cell=target + (role==0 ? new Vector2Int(3,-1) : new Vector2Int(-1,3));
                if (!defense.BeginPlacement())
                {
                    Debug.LogError("S3_RAPID_GAME_ERROR: pilot placement could not begin.", this);
                    yield break;
                }
                defense.SetPreviewCell(cell);
                if (defense.TryConfirm(out var tower) != PlacementResult.Ready || tower == null)
                {
                    Debug.LogError("S3_RAPID_GAME_ERROR: pilot tower purchase failed.", this);
                    yield break;
                }
                Debug.Log($"{(role == 0 ? "S3_RAPID_GAME_READY" : "S3_LONGSHOT_GAME_READY")} cell={cell} prefab={tower.name}", this);
            }
            var capturePath = Environment.GetEnvironmentVariable("NOT3A_PILOT_CAPTURE");
            if (!string.IsNullOrEmpty(capturePath))
            {
                Application.runInBackground = true;
                yield return new WaitForSecondsRealtime(2f);
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(capturePath);
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NOT3A_COMBAT_CAPTURE")))
            {
                Application.runInBackground = true;
                yield return new WaitForSecondsRealtime(2f);
                composition.RunDriver.Run.StartWave();
            }
        }

        private static void AddMissing(Stage2Inventory inventory, MapResourceKind kind, int required)
        {
            var missing = required - inventory.Get(kind);
            if (missing > 0) inventory.TryAdd(kind, missing);
        }
    }
}
