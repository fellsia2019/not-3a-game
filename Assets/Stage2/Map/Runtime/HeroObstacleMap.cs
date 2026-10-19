using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage2.Map
{
    public readonly struct HeroCircleObstacle
    {
        public HeroCircleObstacle(Vector2 center, float radius) { Center = center; Radius = radius; }
        public Vector2 Center { get; }
        public float Radius { get; }
    }

    /// <summary>Ground footprints only. Sweeps a round hero through a complete movement,
    /// including accelerated/debug movement, then slides the remainder along contacts.</summary>
    public sealed class HeroObstacleMap
    {
        private const float Skin = .001f;
        private readonly List<Rect> rectangles = new List<Rect>();
        private readonly List<HeroCircleObstacle> circles = new List<HeroCircleObstacle>();
        public IReadOnlyList<Rect> Rectangles => rectangles.AsReadOnly();
        public IReadOnlyList<HeroCircleObstacle> Circles => circles.AsReadOnly();

        public void AddCircle(Vector2 center, float radius)
            => AddUpdatableCircle(center, radius);

        public int AddUpdatableCircle(Vector2 center, float radius)
        {
            if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius));
            circles.Add(new HeroCircleObstacle(center, radius));
            return circles.Count - 1;
        }

        // Stable slots let a finite resource shrink/disappear without disturbing district circles.
        public void UpdateCircle(int index, Vector2 center, float radius)
        {
            if (index < 0 || index >= circles.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius) ||
                float.IsNaN(center.x) || float.IsNaN(center.y) || float.IsInfinity(center.x) || float.IsInfinity(center.y))
                throw new ArgumentOutOfRangeException(nameof(radius));
            circles[index] = new HeroCircleObstacle(center, radius);
        }

        public void AddRectangle(Rect groundArea)
        {
            if (groundArea.width <= 0 || groundArea.height <= 0 ||
                float.IsNaN(groundArea.x) || float.IsNaN(groundArea.y) ||
                float.IsInfinity(groundArea.x) || float.IsInfinity(groundArea.y))
                throw new ArgumentOutOfRangeException(nameof(groundArea));
            rectangles.Add(groundArea);
        }

        public bool RemoveRectangle(Rect groundArea) => rectangles.Remove(groundArea);

        public void AddSolidCells(IEnumerable<Vector2Int> cells)
        {
            // Merge adjacent cells into horizontal strips. Internal cell seams cannot snag movement.
            var ordered = new List<Vector2Int>(new HashSet<Vector2Int>(cells));
            ordered.Sort((a,b) => a.y == b.y ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            for (var i = 0; i < ordered.Count; i++)
            {
                var start = ordered[i]; var end = start.x;
                while (i + 1 < ordered.Count && ordered[i+1].y == start.y && ordered[i+1].x == end + 1)
                    end = ordered[++i].x;
                rectangles.Add(new Rect(start.x - .5f, start.y - .5f, end - start.x + 1f, 1f));
            }
        }

        public bool IsBlocked(Vector2 point, float radius)
        {
            foreach (var rect in rectangles)
            {
                var nearest = new Vector2(Mathf.Clamp(point.x, rect.xMin, rect.xMax),
                    Mathf.Clamp(point.y, rect.yMin, rect.yMax));
                if ((point - nearest).sqrMagnitude < radius * radius) return true;
            }
            foreach (var circle in circles)
                if (circle.Radius > 0f && (point - circle.Center).sqrMagnitude < (radius + circle.Radius) * (radius + circle.Radius)) return true;
            return false;
        }

        public Vector2 Move(Vector2 position, Vector2 displacement, float radius)
        {
            // Callers recover an overlapping spawn before movement. Never cast out through a solid.
            if (IsBlocked(position, radius)) return position;
            for (var iteration = 0; iteration < 8 && displacement.sqrMagnitude > .00000001f; iteration++)
            {
                var time = 1f; var normal = Vector2.zero;
                foreach (var rect in rectangles) SweepRectangle(position, displacement, rect, radius, ref time, ref normal);
                foreach (var circle in circles)
                    if (circle.Radius > 0f)
                        SweepCircle(position, displacement, circle.Center, radius + circle.Radius, ref time, ref normal);
                if (normal == Vector2.zero)
                {
                    var end = position + displacement;
                    // Float roundoff near two tangent contacts must never turn a clear
                    // sweep origin into an overlapping endpoint on the next frame.
                    return IsBlocked(end, radius) ? position : end;
                }
                var contact = position + displacement * time + normal * Skin;
                // A second touching obstacle can lie in the direction of the separation normal.
                // Back off along the known-clear incoming segment in that case.
                if (IsBlocked(contact, radius))
                    contact = position + displacement * Mathf.Max(0f, time - Skin / displacement.magnitude);
                // The backed-off point can still round inside a neighbouring circle.
                // Keep the last verified clear contact; ordinary movement never recovers
                // by teleporting out of this tight corner on the next frame.
                if (IsBlocked(contact, radius)) return position;
                position = contact;
                displacement *= 1f - time;
                var inward = Vector2.Dot(displacement, normal);
                if (inward < 0f) displacement -= normal * inward;
            }
            return position;
        }

        public bool TryFindFreePosition(Vector2 origin, float radius, Rect allowed, out Vector2 point)
        {
            origin = new Vector2(Mathf.Clamp(origin.x, allowed.xMin, allowed.xMax),
                Mathf.Clamp(origin.y, allowed.yMin, allowed.yMax));
            point = origin;
            if (!IsBlocked(origin, radius + Skin)) return true;
            // Only used after world replacement or an external teleport, never on ordinary free movement.
            const float step = .5f;
            var limit = Mathf.CeilToInt(Mathf.Max(allowed.width, allowed.height) / step) + 1;
            for (var ring = 1; ring <= limit; ring++)
            {
                var found = false; var nearest = float.PositiveInfinity; var best = origin;
                void Consider(int x, int y)
                {
                    var candidate = origin + new Vector2(x, y) * step;
                    if (candidate.x < allowed.xMin || candidate.x > allowed.xMax ||
                        candidate.y < allowed.yMin || candidate.y > allowed.yMax || IsBlocked(candidate, radius + Skin)) return;
                    var distance = (candidate - origin).sqrMagnitude;
                    if (distance >= nearest) return;
                    nearest = distance; best = candidate; found = true;
                }
                for (var x = -ring; x <= ring; x++) { Consider(x, -ring); Consider(x, ring); }
                for (var y = -ring + 1; y < ring; y++) { Consider(-ring, y); Consider(ring, y); }
                if (found) { point = best; return true; }
            }
            return false;
        }

        private static void SweepRectangle(Vector2 p, Vector2 d, Rect rect, float radius,
            ref float time, ref Vector2 normal)
        {
            if (Mathf.Max(p.x, p.x+d.x) + radius < rect.xMin || Mathf.Min(p.x, p.x+d.x) - radius > rect.xMax ||
                Mathf.Max(p.y, p.y+d.y) + radius < rect.yMin || Mathf.Min(p.y, p.y+d.y) - radius > rect.yMax) return;
            // Straight sides of the Minkowski sum, followed by its rounded corners.
            var best = time; var hit = normal;
            void Side(float t, float tangent, float low, float high, Vector2 n)
            {
                if (t >= -.000001f && t <= best && tangent >= low && tangent <= high)
                { best = Mathf.Max(0f,t); hit = n; }
            }
            if (d.x > .000001f) { var t = (rect.xMin-radius-p.x)/d.x; Side(t,p.y+d.y*t,rect.yMin,rect.yMax,Vector2.left); }
            if (d.x < -.000001f) { var t = (rect.xMax+radius-p.x)/d.x; Side(t,p.y+d.y*t,rect.yMin,rect.yMax,Vector2.right); }
            if (d.y > .000001f) { var t = (rect.yMin-radius-p.y)/d.y; Side(t,p.x+d.x*t,rect.xMin,rect.xMax,Vector2.down); }
            if (d.y < -.000001f) { var t = (rect.yMax+radius-p.y)/d.y; Side(t,p.x+d.x*t,rect.xMin,rect.xMax,Vector2.up); }
            SweepCircle(p,d,new Vector2(rect.xMin,rect.yMin),radius,ref best,ref hit);
            SweepCircle(p,d,new Vector2(rect.xMin,rect.yMax),radius,ref best,ref hit);
            SweepCircle(p,d,new Vector2(rect.xMax,rect.yMin),radius,ref best,ref hit);
            SweepCircle(p,d,new Vector2(rect.xMax,rect.yMax),radius,ref best,ref hit);
            time = best; normal = hit;
        }

        private static void SweepCircle(Vector2 p, Vector2 d, Vector2 center, float radius,
            ref float time, ref Vector2 normal)
        {
            var offset = p - center;
            var approach = Vector2.Dot(offset,d);
            if (approach >= 0f) return;
            var lengthSquared = d.sqrMagnitude;
            var discriminant = approach * approach - lengthSquared * (offset.sqrMagnitude - radius * radius);
            if (discriminant <= 0f) return; // A tangent is free movement.
            var t = (-approach - Mathf.Sqrt(discriminant)) / lengthSquared;
            if (t < -.000001f || t > time) return;
            var n = (offset + d * Mathf.Max(0f,t)).normalized;
            if (Vector2.Dot(d,n) >= -.000001f) return;
            time = Mathf.Max(0f,t); normal = n;
        }
    }
}
