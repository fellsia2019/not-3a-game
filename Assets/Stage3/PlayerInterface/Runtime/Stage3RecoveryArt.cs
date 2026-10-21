using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Recovery Art")]
    public sealed class Stage3RecoveryArt : ScriptableObject
    {
        public Sprite WorldBag, MapIcon, WorldMedallion;
        public Material WorldMaterial;
        public bool IsReady => WorldBag!=null && MapIcon!=null && WorldMedallion!=null && WorldMaterial!=null;
    }
}
