using System;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [Serializable]
    public struct Stage3ToolGripPose
    {
        public Vector2 grip;
        public float angleDegrees,headDistance;
    }

    /// <summary>Additive equipment layers. Existing body/armor art and gameplay values remain unchanged.</summary>
    public sealed class Stage3HeroEquipmentArt : ScriptableObject
    {
        public Sprite[] bows;
        public Sprite[] axeBodies,pickBodies;
        public Stage3ToolGripPose[] axePoses,pickPoses;
        public bool HasBows=>bows!=null&&bows.Length==32&&Array.TrueForAll(bows,s=>s!=null);
        public Sprite Bow(int quality,int direction)=>HasBows?bows[Mathf.Clamp(quality,0,7)*4+Mathf.Clamp(direction,0,3)]:null;
        public bool TryWork(bool axe,int direction,int frame,out Sprite body,out Stage3ToolGripPose pose)
        {
            var frames=axe?axeBodies:pickBodies;var poses=axe?axePoses:pickPoses;
            var index=Mathf.Clamp(direction,0,3)*6+Mathf.Clamp(frame,0,5);
            body=null;pose=default;
            if(frames==null||frames.Length!=24||poses==null||poses.Length!=24||frames[index]==null)return false;
            body=frames[index];pose=poses[index];return pose.headDistance>0f;
        }
    }
}
