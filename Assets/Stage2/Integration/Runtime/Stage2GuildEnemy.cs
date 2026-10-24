using System;
using UnityEngine;
using Not3A.Stage2.WavesAI;

namespace Not3A.Stage2.Integration
{
    /// <summary>Small exploration encounter actor; no portal routing, throne or wave membership.</summary>
    public sealed class Stage2GuildEnemy
    {
        private bool engaged,returning;
        private float cooldown,windup;
        public Stage2CombatStatuses Statuses {get;}=new Stage2CombatStatuses();
        public void ApplySlow(float multiplier,float duration)
        {if(Alive)Statuses.ApplySlow(multiplier,duration);}
        public int Id { get; }
        public int Generation { get; }
        public Stage2GuildQuest Profile { get; }
        public Vector2 Home { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 Facing { get; private set; } = Vector2.down;
        public int Health { get; private set; }
        public bool Alive => Health>0;
        public bool Attacking => windup>0;
        public bool Engaged => Alive&&engaged;
        public bool Returning => Alive&&returning;
        public bool Alert(Vector2 hero)
        {
            if(!Alive||returning||!Finite(hero)||Vector2.Distance(hero,Home)>Profile.leashRadius)return false;
            engaged=true;return true;
        }
        public Stage2GuildEnemy(int id,int generation,Stage2GuildQuest profile,Vector2 home)
        {
            if(id<1||generation<1||profile==null||!profile.IsValid||!Finite(home))throw new ArgumentException("Invalid camp actor.");
            Id=id;Generation=generation;Profile=profile;Home=Position=home;Health=profile.health;
        }
        // World regeneration replaces terrain, not earned kills/contract generations or health.
        public void RelocateHome(Vector2 home)
        {
            if(!Finite(home))throw new ArgumentException("Invalid camp home.",nameof(home));
            Home=Position=home;Velocity=Vector2.zero;windup=0;engaged=returning=false;
        }
        private static bool Finite(Vector2 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y);
        public bool TakeDamage(int amount)=>TakeDamage(amount,Stage2DamageSchool.Physical);
        public bool TakeDamage(int amount,Stage2DamageSchool school)
        {
            if(!Alive||amount<=0)return false;
            Health=Math.Max(0,Health-Statuses.AdjustDamage(amount,school));
            if(!Alive){Velocity=Vector2.zero;windup=0;}else{engaged=true;returning=false;}
            return true;
        }
        public void Tick(float elapsed,Vector2 hero,Func<Vector2,Vector2,float,Vector2> move,Action<int> hit)
        {
            if(!float.IsFinite(elapsed)||elapsed<0)throw new ArgumentOutOfRangeException(nameof(elapsed));
            if(!Alive||elapsed==0||!Finite(hero))return;
            var immobilized=Statuses.Immobilized;Statuses.Tick(elapsed);
            if(immobilized){Velocity=Vector2.zero;windup=0;return;}
            Velocity=Vector2.zero;cooldown=Mathf.Max(0,cooldown-elapsed);
            var distance=(hero-Position).magnitude;
            if(engaged&&Vector2.Distance(hero,Home)>Profile.leashRadius){engaged=false;returning=true;windup=0;}
            if(!engaged&&!returning&&distance<=Profile.aggroRadius)engaged=true;
            if(windup>0)
            {
                windup=Mathf.Max(0,windup-elapsed);Facing=(hero-Position).normalized;
                // The caller supplies the current hero position for EACH actor (death may already have teleported him).
                if(windup==0&&engaged&&distance<=Profile.reach)hit?.Invoke(Profile.damage);
                return;
            }
            if(engaged&&distance<=Profile.reach)
            {
                Facing=(hero-Position).normalized;
                if(cooldown==0){windup=Profile.attackWindup;cooldown=Profile.attackInterval;}
                return;
            }
            if(!engaged&&!returning)return;
            var target=engaged?hero:Home;var delta=target-Position;
            if(!engaged&&delta.magnitude<.1f){returning=false;return;}
            var step=delta.normalized*Mathf.Min(delta.magnitude,Profile.speed*Statuses.SlowMultiplier*elapsed);
            var next=move==null?Position+step:move(Position,step,Profile.radius);
            if(!Finite(next))return;
            Velocity=(next-Position)/elapsed;Position=next;
            if(Velocity.sqrMagnitude>.0001f)Facing=Velocity.normalized;
        }
    }
}
