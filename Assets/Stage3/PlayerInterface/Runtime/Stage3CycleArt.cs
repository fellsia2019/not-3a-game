using UnityEngine;
namespace Not3A.Stage3.PlayerInterface
{
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Cycle Dial Art")]
    public sealed class Stage3CycleArt : ScriptableObject
    {
        public Sprite Face,Hand;
        public Vector2 HandPivot;
        public Shader HandShader;
        public bool IsReady=>Face!=null&&Hand!=null&&HandShader!=null;
    }
}
