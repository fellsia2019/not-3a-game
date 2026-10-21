using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in native QA of all 64 real item projections through the product renderer.</summary>
    public sealed class Stage3ItemIconSafeAreaReview : MonoBehaviour
    {
        private static readonly string[] Qualities = { "F", "E", "D", "C", "B", "A", "S", "SSS" };
        private static readonly string[] Types = { "Bow", "Wand", "Helmet", "Chest", "Arms", "Legs", "Pickaxe", "Axe" };
        private Action<Rect, Stage3EquipmentItemView, bool> drawCell;
        private Stage3EquipmentItemView[] items;
        private GUIStyle heading, label;
        private int firstType;
        private bool visible;

        public static IEnumerator Capture(GameObject owner, string directory)
        {
            // Preserve the existing real inventory/loadout/forge/profession/HUD review first.
            yield return Stage3ItemQualityReview.Capture(owner, directory);
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var review = owner.AddComponent<Stage3ItemIconSafeAreaReview>();
            var method = typeof(Stage3FantasyHud).GetMethod("DrawEquipmentCell", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("Shared equipment cell renderer missing.");
            review.drawCell = (Action<Rect, Stage3EquipmentItemView, bool>)Delegate.CreateDelegate(
                typeof(Action<Rect, Stage3EquipmentItemView, bool>), hud, method);
            review.items = new Stage3EquipmentItemView[64];
            for (var type = 0; type < 8; type++)
            for (var quality = 0; quality < 8; quality++)
                review.items[type * 8 + quality] = new Stage3EquipmentItemView
                { TypeIndex = type, QualityIndex = quality, Level = 1, Name = Types[type] };
            var report = new StringBuilder("Native item safe-area audit: the same product DrawEquipmentCell renders all 64 type/rank combinations.\n");
            report.AppendLine("Grids use QA-only view DTOs, not inventory grants or a live drop source. Existing preceding window captures use the declared disposable quality-review setup.");
            var hudWasEnabled = hud.enabled;
            hud.enabled = false; // Hide its normal page; the audit calls its exact shared cell renderer.
            review.visible = true;
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) })
            {
                Screen.SetResolution(size.x, size.y, false);
                yield return new WaitForSecondsRealtime(.35f);
                for (var page = 0; page < 2; page++)
                {
                    review.firstType = page * 4;
                    yield return null;
                    yield return new WaitForEndOfFrame();
                    ScreenCapture.CaptureScreenshot(Path.Combine(directory, $"{size.x}x{size.y}-all64-page{page + 1}.png"));
                    yield return null;
                }
                report.AppendLine($"{size.x}x{size.y}: all 8 ranks, first page Bow/Wand/Helmet/Chest; second Arms/Legs/Pickaxe/Axe.");
            }
            review.visible = false;
            Destroy(review);
            hud.enabled = hudWasEnabled;
            Screen.SetResolution(1600, 900, false);
            yield return new WaitForSecondsRealtime(.35f);
            hud.SetWindow(Stage3PlayerWindow.Inventory);
            File.WriteAllText(Path.Combine(directory, "safe-areas.txt"), report.ToString());
            Debug.Log("S3_ITEM_SAFE_AREAS_READY " + directory);
        }

        private void OnGUI()
        {
            if (!visible || drawCell == null || items == null) return;
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
                label = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                heading.normal.textColor = label.normal.textColor = new Color(.2f, .1f, .25f);
            }
            var oldDepth = GUI.depth;
            var oldColor = GUI.color;
            GUI.depth = -1000;
            GUI.color = new Color(.95f, .92f, .86f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(0, 10, Screen.width, 35), "ITEM ICON SAFE-AREA AUDIT | product renderer | LVL 1", heading);
            var side = Mathf.Floor(Mathf.Min((Screen.width - 170f - 7f * 14f) / 8f, (Screen.height - 140f) / 4f - 32f));
            for (var quality = 0; quality < 8; quality++)
                GUI.Label(new Rect(150f + quality * (side + 14f), 65f, side, 30f), Qualities[quality], label);
            for (var row = 0; row < 4; row++)
            {
                var y = 110f + row * (side + 32f);
                GUI.Label(new Rect(6f, y, 132f, side), Types[firstType + row], label);
                for (var quality = 0; quality < 8; quality++)
                    drawCell(new Rect(150f + quality * (side + 14f), y, side, side), items[(firstType + row) * 8 + quality], false);
            }
            GUI.Label(new Rect(0, Screen.height - 31f, Screen.width, 24f),
                "QA only: inspect visible silhouette centring, padding, aspect and complete contours for every item/rank.", label);
            GUI.color = oldColor;
            GUI.depth = oldDepth;
        }
    }
}
