using System;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    [Serializable]
    public sealed class Stage3TeleportDestinationView
    {
        public string Id, Title;
        public Sprite Medallion, Preview;
    }

    [Serializable]
    public sealed class Stage3TeleportsView
    {
        public string CurrentId, SelectedId;
        public Stage3TeleportDestinationView[] Destinations = Array.Empty<Stage3TeleportDestinationView>();
        public bool CanTravel;
        public string UnavailableReason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3TeleportsView teleportsView = new Stage3TeleportsView();
        private int teleportFirstRow;
        private string teleportRoute = "";

        public bool TeleportsOpen => ActiveWindow == Stage3PlayerWindow.Teleports;
        public Stage3TeleportsView TeleportsView => teleportsView;
        public event Action<string> TeleportDestinationSelected;
        public event Action<string> TeleportRequested;
        public event Action TeleportClosed;

        // Match the approved broad modal; all coordinates remain physical screen pixels.
        public Rect TeleportPanelRect => new Rect((Screen.width - 1200f * UiScale) * .5f,
            (Screen.height - 690f * UiScale) * .5f, 1200f * UiScale, 690f * UiScale);
        public Rect TeleportCloseRect => new Rect(TeleportPanelRect.xMax - 202f * UiScale,
            TeleportPanelRect.y + 12f * UiScale, 172f * UiScale, 56f * UiScale);
        public Rect TeleportActionRect => new Rect(TeleportPanelRect.xMax - 410f * UiScale,
            TeleportPanelRect.yMax - 94f * UiScale, 365f * UiScale, 68f * UiScale);
        private Rect TeleportRowsRect => new Rect(TeleportPanelRect.x + 48f * UiScale,
            TeleportPanelRect.y + 185f * UiScale, 548f * UiScale, 302f * UiScale);
        public Rect TeleportDestinationRect(int index)
        {
            if(index < teleportFirstRow || index >= teleportFirstRow + 2 ||
                index >= (teleportsView.Destinations?.Length ?? 0)) return Rect.zero;
            var r = TeleportRowsRect;
            return new Rect(r.x, r.y + (index - teleportFirstRow) * 151f * UiScale,
                r.width - ((teleportsView.Destinations?.Length ?? 0) > 2 ? 22f * UiScale : 0f), 134f * UiScale);
        }

        public void SetTeleports(Stage3TeleportsView value)
        {
            teleportsView = value ?? new Stage3TeleportsView();
            teleportFirstRow = Mathf.Clamp(teleportFirstRow, 0, Mathf.Max(0, (teleportsView.Destinations?.Length ?? 0) - 2));
            RefreshTeleportRoute();
        }

        public void ShowTeleports(bool show)
        {
            if(show && !TeleportsOpen) teleportFirstRow = 0;
            if(show) SetWindow(Stage3PlayerWindow.Teleports);
            else if(TeleportsOpen) CloseWindow();
        }

        public void SelectTeleportDestination(string id)
        {
            if(FindTeleportDestination(id) == null || teleportsView.SelectedId == id) return;
            teleportsView.SelectedId = id;
            RefreshTeleportRoute();
            TeleportDestinationSelected?.Invoke(id);
        }

        private Stage3TeleportDestinationView FindTeleportDestination(string id)
        {
            if(string.IsNullOrEmpty(id) || teleportsView.Destinations == null) return null;
            foreach(var row in teleportsView.Destinations)
                if(row != null && string.Equals(row.Id, id, StringComparison.Ordinal)) return row;
            return null;
        }

        private void RefreshTeleportRoute()
        {
            var from = FindTeleportDestination(teleportsView.CurrentId);
            var to = FindTeleportDestination(teleportsView.SelectedId);
            teleportRoute = from == null || to == null ? "" : from.Title + "  →  " + to.Title;
        }

        private void DrawTeleports()
        {
            var s = UiScale;
            var panel = TeleportPanelRect;
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(.08f, .06f, .11f, .62f));
            DrawPanel(panel, true);
            var header = new Rect(panel.x + 7f*s, panel.y + 7f*s, panel.width - 14f*s, 76f*s);
            DrawSliced(header, art.buttonNormal, true, .28f*s);
            PaleLabel(new Rect(header.x + 65f*s, header.y, 700f*s, header.height), "Телепорты", 30, TextAnchor.MiddleLeft);
            if(blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            var close = GUI.Button(TeleportCloseRect, GUIContent.none, blankButton);
            PaleLabel(TeleportCloseRect, "Esc · Закрыть", 18, TextAnchor.MiddleCenter);

            var left = TeleportRowsRect;
            Label(new Rect(left.x + 10f*s, panel.y + 107f*s, left.width, 48f*s), "Открытые точки", 27);
            DrawRule(new Rect(left.x + 10f*s, panel.y + 153f*s, left.width - 20f*s, 1f*s));
            DrawRule(new Rect(panel.x + 644f*s, panel.y + 100f*s, 1f*s, 465f*s), false);
            var rows = teleportsView.Destinations;
            var count = rows?.Length ?? 0;
            if(count > 2)
            {
                var e = Event.current;
                if(e.type == EventType.ScrollWheel && left.Contains(e.mousePosition))
                {
                    teleportFirstRow = Mathf.Clamp(teleportFirstRow + (e.delta.y > 0f ? 1 : -1), 0, count - 2);
                    e.Use();
                }
                // Whole-row scrolling keeps native Graphics sprite draws inside the list.
                teleportFirstRow = Mathf.Clamp(Mathf.RoundToInt(GUI.VerticalScrollbar(
                    new Rect(left.xMax - 15f*s, left.y, 15f*s, left.height - 17f*s),
                    teleportFirstRow, 2f, 0f, count)), 0, count - 2);
            }
            string selectedAfterDraw = null;
            for(var i = teleportFirstRow; i < Mathf.Min(count, teleportFirstRow + 2); i++)
            {
                var row = rows[i];
                if(row == null) continue;
                var rect = TeleportDestinationRect(i);
                var selected = string.Equals(row.Id, teleportsView.SelectedId, StringComparison.Ordinal);
                var current = string.Equals(row.Id, teleportsView.CurrentId, StringComparison.Ordinal);
                if(DrawTeleportDestination(rect, row, selected, current)) selectedAfterDraw = row.Id;
            }
            if(count == 0) Label(left, "Нет открытых точек", 22, TextAnchor.MiddleCenter, true);
            DrawRule(new Rect(left.x + 20f*s, panel.y + 527f*s, left.width - 40f*s, 1f*s));
            Label(new Rect(left.x, panel.y + 539f*s, left.width, 35f*s),
                "Выберите точку назначения", 18, TextAnchor.MiddleCenter, true);

            var destination = FindTeleportDestination(teleportsView.SelectedId);
            var detail = new Rect(panel.x + 688f*s, panel.y + 103f*s, 455f*s, 455f*s);
            Label(new Rect(detail.x, detail.y, detail.width, 50f*s), destination?.Title ?? "Точка назначения", 31, TextAnchor.MiddleCenter);
            DrawRule(new Rect(detail.x + 24f*s, detail.y + 55f*s, detail.width - 48f*s, 1f*s));
            const float previewFrameScale = .16f;
            var border = art.windowFrame.border * (previewFrameScale*s);
            var preview = new Rect(detail.x, detail.y + 84f*s, detail.width, 302f*s);
            if(destination?.Preview != null)
            {
                var aspect = destination.Preview.rect.width / destination.Preview.rect.height;
                var width = (preview.height-border.y-border.w-4f*s)*aspect+border.x+border.z+4f*s;
                if(width>preview.width)
                {
                    width=preview.width;
                    preview.height=(width-border.x-border.z-4f*s)/aspect+border.y+border.w+4f*s;
                }
                preview.x += (preview.width-width)*.5f; preview.width=width;
            }
            var imageSafe = Rect.MinMaxRect(preview.x+border.x+2f*s,preview.y+border.w+2f*s,
                preview.xMax-border.z-2f*s,preview.yMax-border.y-2f*s);
            Fill(imageSafe, Pearl);
            if(destination?.Preview != null) DrawIcon(imageSafe, destination.Preview);
            else DrawIcon(Inset(imageSafe, 34f*s), art.teleportIcon);
            DrawSliced(preview,art.windowFrame,false,previewFrameScale*s);
            Label(new Rect(detail.x, preview.yMax + 4f*s, detail.width, 32f*s), "Точка прибытия", 20, TextAnchor.MiddleCenter);
            DrawRule(new Rect(detail.x + 24f*s, preview.yMax + 44f*s, detail.width - 48f*s, 1f*s));

            DrawRule(new Rect(panel.x + 32f*s, panel.yMax - 114f*s, panel.width - 64f*s, 1f*s), false);
            var isCurrent = destination != null && string.Equals(destination.Id, teleportsView.CurrentId, StringComparison.Ordinal);
            var canTravel = destination != null && !isCurrent && teleportsView.CanTravel;
            Label(new Rect(panel.x + 70f*s, panel.yMax - (canTravel ? 94f : 98f)*s,
                660f*s, (canTravel ? 68f : 40f)*s), teleportRoute, 25, TextAnchor.MiddleRight);
            var travel = Button(TeleportActionRect, "Телепортироваться", false, canTravel);
            if(!canTravel)
            {
                var reason = isCurrent ? "Вы уже здесь" : teleportsView.UnavailableReason;
                if(!string.IsNullOrEmpty(reason))
                    Label(new Rect(panel.x + 70f*s, panel.yMax - 56f*s, 660f*s, 24f*s), reason, 14, TextAnchor.MiddleRight, true);
            }
            if(close) CloseWindow();
            else if(selectedAfterDraw != null) SelectTeleportDestination(selectedAfterDraw);
            else if(travel) TeleportRequested?.Invoke(destination.Id);
        }

        private bool DrawTeleportDestination(Rect r, Stage3TeleportDestinationView row, bool selected, bool current)
        {
            var s = UiScale;
            if(selected)
            {
                DrawBevelGradient(Inset(r, 4f*s), new Color(.34f, .88f, .94f), new Color(.70f, .96f, .96f), 18f*s);
                DrawBevelGradient(Inset(r, 6f*s), new Color(.90f, .99f, .98f), new Color(.65f, .87f, .89f), 17f*s);
            }
            DrawSliced(r, art.panelFrame, !selected, .16f*s);
            if(selected)
            {
                DrawLunarJewel(new Vector2(r.x + 8f*s, r.center.y), 11f*s);
                DrawLunarJewel(new Vector2(r.xMax - 8f*s, r.center.y), 11f*s);
            }
            var medal = new Rect(r.x + 23f*s, r.center.y - 64f*s, 128f*s, 128f*s);
            var portrait = row.Medallion != null ? row.Medallion : row.Preview;
            if(portrait != null) DrawRoundSprite(Inset(medal, 15f*s), portrait);
            else
            {
                DrawDisc(Inset(medal, 15f*s), PlumInk);
                DrawIcon(Inset(medal, 25f*s), art.teleportIcon);
            }
            DrawMedallion(medal, selected);
            var text = new Rect(r.x + 177f*s, r.y + (current ? 31f : 41f)*s, r.width - 214f*s, 48f*s);
            Label(text, row.Title, 30, TextAnchor.MiddleLeft);
            if(current) Label(new Rect(text.x, r.y + 78f*s, text.width, 30f*s), "Вы здесь", 21, TextAnchor.MiddleLeft, true);
            return GUI.Button(r, GUIContent.none, blankButton);
        }

    }
}
