using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed class Stage3TowerSpriteSet : ScriptableObject
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private Vector2[] releasePixels;
        [SerializeField] private Sprite spoon;
        [SerializeField] private Sprite stone;
        [SerializeField] private Rect bodyVisibleBounds;
        [SerializeField] private float cellWidth;
        [SerializeField] private float frontInset;
        public Sprite Spoon => spoon;
        public Sprite Stone => stone;
        public Rect BodyVisibleBounds => bodyVisibleBounds;
        public float CellWidth => cellWidth;
        public float FrontInset => frontInset;
        public bool IsLayered => spoon != null;
        public bool IsCatapult => IsLayered || (frames != null && frames.Length == 24);
        public Sprite Frame(int direction, int pose) => frames[IsLayered ? 0 : IsCatapult ? direction * 6 + pose : 0];
        public Vector2 ReleasePixel(int direction) => releasePixels[IsCatapult ? direction : 0];
        public void Configure(Sprite[] value, Vector2[] sockets) { frames = value; releasePixels = sockets; }
        public void ConfigureParts(Sprite value, Sprite rock) { spoon=value; stone=rock; }
        public void ConfigureCellFit(Rect bounds, float width, float inset)
        { bodyVisibleBounds=bounds; cellWidth=width; frontInset=inset; }
    }
}
