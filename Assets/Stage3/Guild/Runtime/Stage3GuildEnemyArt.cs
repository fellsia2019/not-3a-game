using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed class Stage3GuildEnemyArt : ScriptableObject
    {
        // Front idle/walk A/walk B/strike, then rear equivalents. Authored full silhouettes.
        public Sprite[] orcFrames,ogreFrames;
        public Material material;
        public Sprite Frame(int kind,int index)
        {
            var frames=kind==1?ogreFrames:orcFrames;
            return frames!=null&&index>=0&&index<frames.Length?frames[index]:null;
        }
    }
}
