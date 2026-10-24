using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Lunar Interface Art")]
    public sealed class Stage3FantasyHudArt : ScriptableObject
    {
        public Sprite windowFrame, panelFrame, buttonNormal, buttonSelected, minimapFrame, medallionFrame;
        public Sprite[] resourceIcons = new Sprite[4];
        public Sprite[] menuIcons = new Sprite[5];
        public Sprite playerNavigationIcon, towerLevelPlate, upgradeReadyIcon, upgradeBlockedIcon;
        public Sprite[] professionCapstoneIcons;
        public Sprite controlPanel, controlAction, controlSlot, controlMedallion, upgradeLightCurtain;
        public Sprite[] adventurerIcons;
        public Sprite heroPortrait, inventoryHero, bowIcon, teleportIcon, sunIcon, closeIcon;
        public Sprite[] defenseIcons = new Sprite[3];
        public Sprite workshopIcon, dwellingIcon;
        public Font font;
        public Shader circularMapShader;
        public bool IsReady => windowFrame != null && panelFrame != null && buttonNormal != null &&
            buttonSelected != null && minimapFrame != null && medallionFrame != null;
    }
}
