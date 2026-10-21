using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Open Camp Art")]
    public sealed class Stage3CampArt : ScriptableObject
    {
        // Orc: tent, canopy, fire, barrels, short palisade, logs.
        // Ogre: broken shelter, wall fragment, column, rubble, logs, fire.
        public Sprite[] orc = new Sprite[6];
        public Sprite[] ogre = new Sprite[6];
        public Sprite[] trees, rocks;
        public Sprite lowPlants;
        public Material spriteMaterial, groundMaterial;
        public bool IsReady => Complete(orc)&&Complete(ogre)&&spriteMaterial!=null&&groundMaterial!=null;
        public Sprite Frame(int kind,int index)
        {
            if(index>=300)return lowPlants;
            if(index>=200)return rocks!=null&&rocks.Length>0?rocks[(index-200)%rocks.Length]:null;
            if(index>=100)return trees!=null&&trees.Length>0?trees[(index-100)%trees.Length]:null;
            var frames=kind==0?orc:ogre;
            return frames!=null&&index>=0&&index<frames.Length?frames[index]:null;
        }
        private static bool Complete(Sprite[] frames)
        {
            if(frames==null||frames.Length!=6)return false;
            foreach(var frame in frames)if(frame==null)return false;
            return true;
        }
    }
}
