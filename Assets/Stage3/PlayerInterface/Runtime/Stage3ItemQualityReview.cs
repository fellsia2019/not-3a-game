using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in native quality-cell QA. Normal startup never grants items, scrolls or money.</summary>
    public static class Stage3ItemQualityReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            var game = owner.GetComponent<Stage2GameComposition>(); var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>(); var binding = owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while (binding.Equipment == null || !hud.IsReady) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            var rules = binding.Equipment.Rules; var bows = new Stage2EquipmentItem[8];
            for (var q = 0; q < 8; q++)
                if (!rules.TryReceiveLoot(Stage2ItemType.Bow, (Stage2ItemQuality)q, 1, out bows[q]))
                    throw new InvalidOperationException("Quality QA bow grant failed.");
            var armour = new[] { Stage2ItemType.Helmet, Stage2ItemType.Chest, Stage2ItemType.Arms, Stage2ItemType.Legs };
            for (var i = 0; i < armour.Length; i++)
            { rules.TryReceiveLoot(armour[i], (Stage2ItemQuality)(i + 2), 1, out var item); rules.TryEquip(item.Id); }
            rules.TryReceiveLoot(Stage2ItemType.Pickaxe, Stage2ItemQuality.SSS, 1, out var pick); rules.TryEquip(pick.Id);
            rules.TryReceiveLoot(Stage2ItemType.Axe, Stage2ItemQuality.S, 1, out var axe); rules.TryEquip(axe.Id);
            rules.TryReceiveLoot(Stage2ItemType.Bow, Stage2ItemQuality.SSS, 1, out var weapon); rules.TryEquip(weapon.Id);
            game.Gathering.Economy.Inventory.TryCredit(new Stage2ResourceCost(5000, 5000, 5000, 5000));
            var forge = binding.Forge; forge.BeginPlacement(); var origin = (Vector2)scene.PlayerMover.transform.position; var built = false;
            for (var ring = 5; ring < 45 && !built; ring += 2)
            for (var x = -ring; x <= ring && !built; x += 2)
            for (var y = -ring; y <= ring && !built; y += 2)
            { var point = origin + new Vector2(x, y); if (forge.EvaluateAt(point) == Stage3ForgePlacementResult.Ready) built = forge.TryBuildAt(point) == Stage3ForgePlacementResult.Built; }
            if (!built) throw new InvalidOperationException("Quality QA legal forge build failed.");
            if(rules.Blueprints!=null)
                foreach(var kind in new[]{Stage2BlueprintKind.ForgeII,Stage2BlueprintKind.ForgeIII})
                {
                    var paper=binding.Merchant.BlueprintItems.First(p=>p.Kind==kind);
                    if(binding.Merchant.TryTradeBlueprint(true,paper.Id)!=Stage2TradeResult.Ready||rules.Blueprints.TryStudy(paper.Id)!=Stage2BlueprintResult.Ready)
                        throw new InvalidOperationException("Quality QA blueprint prerequisite failed.");
                }
            else rules.TryReceiveForgeScrolls(2);
            rules.TryUpgradeForge();rules.TryUpgradeForge();
            scene.PlayerMover.transform.position = forge.InteractionPosition; scene.CameraFollow.SnapToTarget();
            void Call(string name, params object[] args) => typeof(Stage3PlayerInterfaceBinding).GetMethod(name,
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(binding, args);
            var report = new StringBuilder("Native Mac quality-cell verification. Explicit disposable QA grants:8bow ranks plus mixed loadout, S/SSS tools/weapon,5000materials/gold; forgeII/III prerequisite papers bought/studied through ownership rules (legacy profiles use2scrolls). Paid legal forge. Automatic clock/combat stopped. No normal startup grants or live loot source added.\n");
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) })
            {
                Screen.SetResolution(size.x, size.y, false); yield return new WaitForSecondsRealtime(.35f);
                foreach (var q in new[] { 2, 3, 6, 7 })
                {
                    hud.SetWindow(Stage3PlayerWindow.Inventory); Call("SelectEquipmentItem", bows[q].Id);
                    yield return Shot(directory, $"{size.x}x{size.y}-inventory-{(Stage2ItemQuality)q}");
                }
                hud.CloseWindow(); if (!forge.TryOpen()) throw new InvalidOperationException("Quality QA local forge could not open.");
                Call("SelectForgeType", (int)Stage2ItemType.Bow); Call("SelectForgeMode", Stage3ForgeMode.Reforge); Call("SelectForgeItem", bows[2].Id);
                yield return Shot(directory, $"{size.x}x{size.y}-forge-D-to-C");
                Call("SelectForgeItem", bows[5].Id); yield return Shot(directory, $"{size.x}x{size.y}-forge-A-to-S");
                hud.SetWindow(Stage3PlayerWindow.Hero); hud.ShowProfessions(true); hud.SelectProfession(0);
                yield return Shot(directory, $"{size.x}x{size.y}-miner-SSS-tool");
                hud.SelectProfession(1); yield return Shot(directory, $"{size.x}x{size.y}-logger-S-tool");
                hud.CloseWindow(); yield return Shot(directory, $"{size.x}x{size.y}-HUD-SSS-bow");
                report.AppendLine($"Captured {size.x}x{size.y}: all8bag ranks, selected D/C/S/SSS, mixed equipped slots, actual D→C/A→S forge previews, S/SSS tool views, SSS HUD weapon.");
            }
            Screen.SetResolution(1600, 900, false); yield return new WaitForSecondsRealtime(.35f);
            hud.SetWindow(Stage3PlayerWindow.Inventory); Call("SelectEquipmentItem", bows[3].Id); yield return null;
            report.AppendLine($"Interactive native inventory: CspareBow={bows[3].Id}; selectedC; slotC(index3)={hud.EquipmentItemRect(3)}; action={hud.EquipmentActionRect}; equippedWeapon={rules.Equipped(Stage2ItemSlot.Weapon).Id}");
            File.WriteAllText(Path.Combine(directory, "verification.txt"), report.ToString());
            rules.Changed += () => Debug.Log($"S3_ITEM_QUALITY_NATIVE_EQUIPMENT weapon={rules.Equipped(Stage2ItemSlot.Weapon)?.Id} quality={rules.Equipped(Stage2ItemSlot.Weapon)?.Quality} items={rules.Items.Count}");
            Debug.Log("S3_ITEM_QUALITY_NATIVE_READY " + directory);
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
