using UnityEngine;
namespace Not3A.Stage3.PlayerInterface
{
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Guild Art")]
    public sealed class Stage3GuildArt : ScriptableObject
    {
        public Texture2D boardBackground;
        public Sprite orderPaper,detailPaper,rankShield,waxSeal,actionButton,headerPlaque;
        public Sprite orcInk,ogreInk,guildHall,noticeBoard;
        public bool IsReady=>boardBackground!=null&&orderPaper!=null&&detailPaper!=null&&rankShield!=null&&waxSeal!=null&&actionButton!=null&&headerPlaque!=null;
    }
}
