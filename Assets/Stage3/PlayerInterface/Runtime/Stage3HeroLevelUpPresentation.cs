using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Approved spectral shield celebration. Scene-owned feedback, never an input or pause owner.</summary>
    public sealed class Stage3HeroLevelUpPresentation : MonoBehaviour
    {
        private Stage3FantasyHud hud;
        private Texture2D crest,waves;
        private AudioClip optionalSound;
        private AudioSource audioSource;
        private bool soundPlayed;
        private string levelLabel,pointsLabel;
        public bool IsPlaying { get; private set; }
        public bool IsReady => hud!=null&&crest!=null&&waves!=null;
        public int DisplayedLevel { get; private set; }
        public int DisplayedPoints { get; private set; }
        public float Elapsed { get; private set; }
        public float Duration => 2.25f;
        public bool HasAudioClip => optionalSound!=null;

        public void Initialize(Stage3FantasyHud presentation)
        {
            hud=presentation;
            crest=Resources.Load<Texture2D>("Stage3HeroLevelUpCrest");
            waves=Resources.Load<Texture2D>("Stage3HeroLevelUpWaves");
            if(optionalSound==null)optionalSound=Resources.Load<AudioClip>("Stage3HeroLevelUpSound");
            hud?.BindHeroLevelUpPresentation(this);
        }
        public void SetAudioClip(AudioClip clip)=>optionalSound=clip;

        public void PlayLevelUp(int newLevel,int gainedPoints)
        {
            if(newLevel<1)return;
            DisplayedPoints=IsPlaying?DisplayedPoints+Mathf.Max(0,gainedPoints):Mathf.Max(0,gainedPoints);
            DisplayedLevel=Mathf.Clamp(newLevel,1,100);
            levelLabel=DisplayedLevel.ToString();
            pointsLabel="+"+DisplayedPoints+" "+PointWord(DisplayedPoints);
            if(!IsPlaying){Elapsed=0;soundPlayed=false;}
            else Elapsed=Mathf.Min(Elapsed,.16f);
            IsPlaying=true;
        }
        private static string PointWord(int count)
        {
            var hundred=count%100;if(hundred>=11&&hundred<=14)return "ОЧКОВ";
            return count%10==1?"ОЧКО":count%10>=2&&count%10<=4?"ОЧКА":"ОЧКОВ";
        }
        public void ClearEffects()
        {
            IsPlaying=false;Elapsed=0;DisplayedLevel=0;DisplayedPoints=0;soundPlayed=false;
            if(audioSource!=null)audioSource.Stop();
        }
        private void Update()
        {
            if(!IsPlaying||hud==null)return;
            if(hud.HeroPresentationPaused||hud.IsModalOpen||!hud.isActiveAndEnabled)
            {if(audioSource!=null&&audioSource.isPlaying)audioSource.Pause();return;}
            if(audioSource!=null&&soundPlayed)audioSource.UnPause();
            if(!soundPlayed)
            {
                soundPlayed=true;
                if(optionalSound!=null)
                {
                    if(audioSource==null){audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;audioSource.loop=false;}
                    audioSource.clip=optionalSound;audioSource.Play();
                }
            }
            Elapsed=Mathf.Min(Duration,Elapsed+Time.unscaledDeltaTime);
            if(Elapsed>=Duration)IsPlaying=false;
        }
        public void DrawOverlay()
        {
            if(!IsPlaying||!IsReady||hud.IsModalOpen||!hud.isActiveAndEnabled)return;
            hud.DrawHeroLevelUpArtwork(crest,waves,levelLabel,pointsLabel,Elapsed,Duration);
        }
        private void OnDisable()=>ClearEffects();
        private void OnDestroy()
        {
            if(hud!=null)hud.BindHeroLevelUpPresentation(null);
            if(audioSource!=null)Destroy(audioSource);
        }
    }
}
