using System;
using UnityEngine;

namespace Not3A.Stage2.WavesAI
{
    public enum Stage2DamageSchool { Physical, Magic }

    /// <summary>Per-enemy timed effects shared by portal and exploration actors. No world ownership.</summary>
    public sealed class Stage2CombatStatuses
    {
        private float slowTime,slow=1,physicalTime,magicTime,physical,magic;
        private double physicalRemainder,magicRemainder;
        public float FrozenRemaining {get;private set;}
        public float StunnedRemaining {get;private set;}
        public float BurningRemaining {get;private set;}
        public float PoisonRemaining {get;private set;}
        public bool Immobilized=>FrozenRemaining>0||StunnedRemaining>0;
        public float SlowMultiplier=>slowTime>0?slow:1;
        public float PhysicalVulnerability=>physicalTime>0?physical:0;
        public float MagicVulnerability=>magicTime>0?magic:0;
        private static bool Valid(float value)=>float.IsFinite(value)&&value>0;
        public void ApplySlow(float multiplier,float duration)
        {if(!Valid(duration)||!float.IsFinite(multiplier))return;slow=Mathf.Min(SlowMultiplier,Mathf.Clamp(multiplier,.25f,1));slowTime=Mathf.Max(slowTime,duration);}
        public void Freeze(float duration){if(Valid(duration))FrozenRemaining=Mathf.Max(FrozenRemaining,duration);}
        public void Stun(float duration){if(Valid(duration))StunnedRemaining=Mathf.Max(StunnedRemaining,duration);}
        public void MarkBurning(float duration){if(Valid(duration))BurningRemaining=Mathf.Max(BurningRemaining,duration);}
        public void MarkPoisoned(float duration){if(Valid(duration))PoisonRemaining=Mathf.Max(PoisonRemaining,duration);}
        public void Weaken(Stage2DamageSchool school,float strength,float duration)
        {
            if(!Valid(strength)||!Valid(duration))return;
            strength=Mathf.Clamp01(strength);
            if(school==Stage2DamageSchool.Physical){physical=Mathf.Max(PhysicalVulnerability,strength);physicalTime=Mathf.Max(physicalTime,duration);}
            else {magic=Mathf.Max(MagicVulnerability,strength);magicTime=Mathf.Max(magicTime,duration);}
        }
        public int AdjustDamage(int amount,Stage2DamageSchool school)
        {
            if(amount<=0)return 0;
            var bonus=school==Stage2DamageSchool.Physical?PhysicalVulnerability:MagicVulnerability;
            if(bonus<=0)return amount;
            // Preserve small vulnerabilities at starter damage1 without rounding every hit up.
            var exact=amount*(1d+bonus)+(school==Stage2DamageSchool.Physical?physicalRemainder:magicRemainder);
            var result=(int)Math.Min(int.MaxValue,Math.Floor(exact+0.000001));
            if(school==Stage2DamageSchool.Physical)physicalRemainder=exact-result;else magicRemainder=exact-result;
            return result;
        }
        public void Tick(float elapsed)
        {
            if(!float.IsFinite(elapsed)||elapsed<0)throw new ArgumentOutOfRangeException(nameof(elapsed));
            slowTime=Mathf.Max(0,slowTime-elapsed);if(slowTime==0)slow=1;
            physicalTime=Mathf.Max(0,physicalTime-elapsed);magicTime=Mathf.Max(0,magicTime-elapsed);
            if(physicalTime==0)physicalRemainder=0;if(magicTime==0)magicRemainder=0;
            FrozenRemaining=Mathf.Max(0,FrozenRemaining-elapsed);StunnedRemaining=Mathf.Max(0,StunnedRemaining-elapsed);
            BurningRemaining=Mathf.Max(0,BurningRemaining-elapsed);PoisonRemaining=Mathf.Max(0,PoisonRemaining-elapsed);
        }
        public void Reset()
        {slowTime=physicalTime=magicTime=physical=magic=FrozenRemaining=StunnedRemaining=BurningRemaining=PoisonRemaining=0;slow=1;physicalRemainder=magicRemainder=0;}
    }
}
