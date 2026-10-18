using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private int mapTouchCount, mapFirstTouchId, mapSecondTouchId;
        private Vector2 previousMapTouchPoint;
        private float previousMapPinchDistance;
        private bool MapTouchOwnsPointer => mapTouchCount > 0;

        /// <summary>Inverse projection using absolute top-left GUI screen coordinates.</summary>
        public Vector2 MapPointToWorld(Vector2 guiPoint)
        {
            if (scene == null || scene.MapDefinition == null || !Finite(guiPoint) ||
                MapViewportRect.width <= 0f || MapViewportRect.height <= 0f) return MapCenter;
            return scene.MapDefinition.CellCoordinatesToWorld(MapCellAtPoint(guiPoint));
        }

        /// <summary>Keep the world beneath the pointer fixed, except where world bounds constrain the view.</summary>
        public void ZoomMapAt(float factor, Vector2 guiPoint)
        {
            var viewport = MapViewportRect;
            if (scene == null || scene.MapDefinition == null || !Finite(guiPoint) ||
                float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0f ||
                viewport.width <= 0f || viewport.height <= 0f || !viewport.Contains(guiPoint)) return;
            var zoom = Mathf.Clamp(MapZoom * factor, 1f, 12f);
            if (zoom == MapZoom) return;
            var anchor = MapCellAtPoint(guiPoint);
            MapZoom = zoom;
            var extent = MapViewExtent();
            var offset = guiPoint - viewport.center;
            mapCenterCells = anchor - new Vector2(offset.x / viewport.width * extent.x,
                -offset.y / viewport.height * extent.y);
            ClampMapCenter();
        }

        private Vector2 MapCellAtPoint(Vector2 guiPoint)
        {
            var viewport = MapViewportRect;
            var offset = guiPoint - viewport.center;
            var extent = MapViewExtent();
            return mapCenterCells + new Vector2(offset.x / viewport.width * extent.x,
                -offset.y / viewport.height * extent.y);
        }

        private static bool Finite(Vector2 point) => !float.IsNaN(point.x) && !float.IsNaN(point.y) &&
            !float.IsInfinity(point.x) && !float.IsInfinity(point.y);

        private void ResetMapTouchInput()
        {
            mapTouchCount = 0;
            previousMapPinchDistance = 0f;
        }

        // Touch positions use bottom-left screen coordinates; the map itself uses GUI coordinates.
        private void UpdateMapTouchInput()
        {
            if (ActiveWindow != Stage3PlayerWindow.Map || Touchscreen.current == null)
            { ResetMapTouchInput(); return; }
            TouchControl first = null, second = null;
            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed) continue;
                if (first == null) first = touch;
                else { second = touch; break; }
            }
            if (first == null) { ResetMapTouchInput(); return; }
            var firstId = first.touchId.ReadValue();
            var point = ToMapGuiPoint(first.position.ReadValue());
            var count = second == null ? 1 : 2;
            var secondId = second == null ? 0 : second.touchId.ReadValue();
            var distance = 0f;
            if (second != null)
            {
                var other = ToMapGuiPoint(second.position.ReadValue());
                distance = Vector2.Distance(point, other);
                point = (point + other) * .5f;
            }
            if (!Finite(point) || !MapViewportRect.Contains(point))
            { ResetMapTouchInput(); return; }
            var continuing = mapTouchCount == count && mapFirstTouchId == firstId &&
                (count == 1 || mapSecondTouchId == secondId);
            if (continuing)
            {
                PanMap(point - previousMapTouchPoint);
                if (count == 2 && previousMapPinchDistance > 1f && distance > 1f)
                    ZoomMapAt(distance / previousMapPinchDistance, point);
            }
            else if (mapDragging)
            {
                GUIUtility.hotControl = 0;
                mapDragging = false;
            }
            mapTouchCount = count;
            mapFirstTouchId = firstId;
            mapSecondTouchId = secondId;
            previousMapTouchPoint = point;
            previousMapPinchDistance = distance;
        }

        private static Vector2 ToMapGuiPoint(Vector2 screenPoint) =>
            new Vector2(screenPoint.x, Screen.height - screenPoint.y);
    }
}
