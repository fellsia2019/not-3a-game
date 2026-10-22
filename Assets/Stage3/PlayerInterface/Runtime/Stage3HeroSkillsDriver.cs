using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.WavesAI;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Small scene adapter for personal skill effects. Existing populations own damage, kills and XP.</summary>
    [DisallowMultipleComponent,DefaultExecutionOrder(80)]
    public sealed class Stage3HeroSkillsDriver:MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2PlayerMover mover;
        private Stage2HeroProgressionDriver hero;
        private Stage2EquipmentDriver equipment;
        private Stage3GuildWorldDriver guild;
        private Stage3HeroSkillPresentation visuals;
        private Stage2HeroRecoveryDriver recovery;
        private readonly List<Target> targets=new List<Target>(16);
        private readonly List<Burn> burns=new List<Burn>();
        private int aimId=-1;
        private Target aimedTarget;
        private float aimRemaining;
        public Stage2HeroSkills Rules {get;private set;}
        public Func<bool> ControlBlocked {get;set;}
        public bool AutomaticTick {get;set;}=true;
        public bool IsAiming=>aimId>=0;
        public int ActiveWeapon=>equipment?.Rules.ActiveWeaponType==Stage2ItemType.Wand?1:0;
        public bool CanControl=>Rules!=null&&game!=null&&!game.RunDriver.Run.IsPaused&&!game.RunDriver.Run.IsTerminal&&
            !(ControlBlocked?.Invoke()??false)&&!(game.GetComponent<Stage2GameHud>()?.DebugOpen??false);
        public float LastAppliedDamage {get;private set;}
        public int LastTargetCount {get;private set;}
        public event Action<int> CastConfirmed;
        private readonly struct Target
        {
            public readonly Stage2Enemy Portal;
            public readonly Stage2GuildEnemy World;
            public Target(Stage2Enemy p){Portal=p;World=null;}
            public Target(Stage2GuildEnemy w){Portal=null;World=w;}
            public bool Alive=>Portal!=null?Portal.IsAlive:World!=null&&World.Alive;
            public Vector2 Position=>Portal!=null?(Vector2)Portal.transform.position:World?.Position??Vector2.zero;
        }
        private sealed class Burn {public Target Target;public float Remaining,Tick;public int Damage;}
        public void Initialize(Stage2GameComposition owner,Stage2PlayerMover player,Stage2HeroProgressionDriver progression,Stage2EquipmentDriver gear,Stage2HeroSkillCatalog catalog)
        {
            game=owner;mover=player;hero=progression;equipment=gear;
            Rules=new Stage2HeroSkills(hero.Rules,catalog);
            visuals=GetComponent<Stage3HeroSkillPresentation>()??gameObject.AddComponent<Stage3HeroSkillPresentation>();
            visuals.Initialize(game);
            game.HeroCombat.MovingAttackAllowed=()=>Rules.Rank(14)>0&&!IsAiming;
            game.RunDriver.Run.Reset+=ResetRules;
            game.RunDriver.Run.Rewound+=ClearTargets;
            equipment.Rules.Changed+=OnEquipmentChanged;
            BindRecovery();ApplyStats();
        }
        private void BindRecovery()
        {
            if(recovery!=null)return;
            recovery=game.GetComponent<Stage2HeroRecoveryDriver>();if(recovery==null)return;
            recovery.IncomingDamagePreview=amount=>Rules.PreviewDamage(amount,equipment.ProtectionFraction);
            recovery.IncomingDamageConfirmed=amount=>Rules.AbsorbDamage(amount,equipment.ProtectionFraction);
            recovery.Respawned+=OnRespawn;
        }
        private void OnRespawn(){ClearTargets();Rules.ClearBuffs();}
        private void OnEquipmentChanged(){CancelAim();ApplyStats();}
        private void ApplyStats()
        {
            if(guild==null)guild=GetComponent<Stage3GuildWorldDriver>();
            if(guild!=null)guild.KillExperienceBonus=Rules.Value(33);
            var moving=Rules.Rank(14);
            var movePenalty=mover.IsMoving&&moving>0?Mathf.Min(1,.4f+.2f*moving):1;
            // Temporary weapon-specific effects never remain active on the other weapon.
            var magic=ActiveWeapon==1?1+Rules.Value(23):1;
            var focus=ActiveWeapon==0&&Rules.FocusRemaining>0?1+Rules.Value(4):1;
            game.HeroCombat.Rules.SetAbilityBonuses(magic*movePenalty,focus);
            mover.SetPersonalSpeedMultiplier(hero.Rules.MovementSpeedMultiplier*(ActiveWeapon==0&&Rules.WindRemaining>0?1+Rules.Value(5):1));
            hero.Rules.SetAdditionalMana(equipment.ManaBonus+Rules.Value(29),1+Rules.Value(30));
        }
        public bool CanCast(int id,out string reason)
        {
            if(!CanControl){reason="Сейчас недоступно";return false;}
            return Rules.CanExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,out reason);
        }
        private bool Collect(int id,Vector2 point)
        {
            targets.Clear();var n=Rules.Catalog.Get(id);
            if(n==null||!float.IsFinite(point.x)||!float.IsFinite(point.y)||
                Vector2.Distance(mover.transform.position,point)>n.range)return false;
            var radius=id==9||id==10?n.radius:1.1f;
            for(var i=0;i<game.Enemies.ActiveCount;i++)
            {var unit=game.Enemies.GetEnemy(i);if(unit!=null&&unit.IsAlive&&Vector2.Distance(unit.transform.position,point)<=radius)targets.Add(new Target(unit));}
            if(guild==null)guild=GetComponent<Stage3GuildWorldDriver>();
            if(guild!=null)foreach(var unit in guild.Enemies)if(unit.Alive&&Vector2.Distance(unit.Position,point)<=radius)targets.Add(new Target(unit));
            targets.Sort((a,b)=>(a.Position-point).sqrMagnitude.CompareTo((b.Position-point).sqrMagnitude));
            var maximum=id==9||id==10?n.maxTargets:1;
            if(targets.Count>maximum)targets.RemoveRange(maximum,targets.Count-maximum);
            return targets.Count>0;
        }
        public bool IsTargetValid(int id,Vector2 point)
        {
            if(!CanCast(id,out _))return false;
            return id==4||id==5||id==7||id==8? id!=8||hero.Rules.Health<hero.Rules.MaximumHealth:Collect(id,point);
        }
        public void ShowTarget(int id,Vector2 point)
        {var n=Rules?.Catalog.Get(id);if(n==null)return;visuals.SetTargetPreview(id,point,id==9||id==10?n.radius:.4f,IsTargetValid(id,point));}
        public void HideTarget()=>visuals?.ClearTargetPreview();
        public bool TryCast(int id,Vector2 point)
        {
            if(!CanCast(id,out _)||id==0)return false;
            var n=Rules.Catalog.Get(id);
            if(id==1)
            {
                if(mover.IsMoving||!Collect(id,point))return false;
                aimId=id;aimedTarget=targets[0];aimRemaining=n.aimTime*(1-Rules.Value(16));game.Gathering.CancelGatherUntilRelease();
                return true;
            }
            if(id==4||id==5||id==7||id==8)
            {
                var success=Rules.TryExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,()=>
                {
                    if(id==4)Rules.ApplyFocus();else if(id==5)Rules.ApplyWind();else if(id==7)Rules.ApplyBarrier();
                    else return hero.Rules.RestoreVitals(Rules.Value(8)*(1+Rules.Value(26)),0);
                    return true;
                });
                if(success)Confirm(id,mover.transform.position,0);return success;
            }
            if(!Collect(id,point))return false;
            var hitPoint=id==9||id==10?point:targets[0].Position;
            var result=Rules.TryExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,()=>HitTargets(id));
            if(result)Confirm(id,hitPoint,id==9||id==10?n.radius:0);
            return result;
        }
        private bool HitTargets(int id)
        {
            var n=Rules.Catalog.Get(id);var multiplier=Rules.Value(id);
            multiplier*=1+(id==1?Rules.Value(15):id==2?Rules.Value(17):id==9?Rules.Value(27):id==10?Rules.Value(28):0);
            // Staff passive already participates in EffectiveDamageExact, so it is never added twice.
            var damage=Mathf.Max(1,Mathf.RoundToInt(game.HeroCombat.Rules.EffectiveDamageExact*multiplier));
            LastTargetCount=0;LastAppliedDamage=0;
            foreach(var target in targets)
            {
                if(!Damage(target,damage))continue;LastTargetCount++;LastAppliedDamage+=damage;
                if(id==2&&target.Alive)
                {
                    var existing=burns.Find(b=>ReferenceEquals(b.Target.Portal,target.Portal)&&ReferenceEquals(b.Target.World,target.World));
                    if(existing==null){existing=new Burn{Target=target};burns.Add(existing);}
                    existing.Damage=Mathf.Max(1,Mathf.RoundToInt(game.HeroCombat.Rules.EffectiveDamageExact*.35f*(1+Rules.Value(17))));
                    existing.Remaining=n.duration+Rules.Value(18);existing.Tick=1;
                }
                if(id==3&&target.Alive)
                {
                    var slow=Mathf.Clamp(.65f-Rules.Value(19),.35f,1);var duration=n.duration+Rules.Value(20);
                    if(target.Portal!=null)target.Portal.ApplySlow(slow,duration);else target.World.ApplySlow(slow,duration);
                }
            }
            return LastTargetCount>0;
        }
        private bool Damage(Target t,int amount)=>t.Alive&&(t.Portal!=null?t.Portal.TakeDamage(amount):guild!=null&&guild.Damage(t.World,amount));
        private void Confirm(int id,Vector2 position,float radius)
        {game.Gathering.CancelGatherUntilRelease();visuals.PresentSkillImpact(id,position,radius);CastConfirmed?.Invoke(id);ApplyStats();}
        public void CancelAim(){aimId=-1;aimRemaining=0;HideTarget();}
        public bool TrySwitchWeapon()
        {
            if(!CanControl)return false;CancelAim();
            return equipment.Rules.TrySwitchWeapon();
        }
        public bool TryUsePotion(int index)
        {
            if(!CanControl||index<0||index>2)return false;
            return Rules.TryUsePotion(equipment.Rules,(Stage2ItemType)(11+index));
        }
        public void Tick(float elapsed)
        {
            if(Rules==null)return;BindRecovery();
            var active=!game.RunDriver.Run.IsPaused&&!game.RunDriver.Run.IsTerminal&&!(game.GetComponent<Stage2GameHud>()?.DebugOpen??false);
            Rules.Tick(elapsed,active);if(!active)return;
            if(aimId>=0)
            {
                if(!CanControl||mover.IsMoving||!aimedTarget.Alive||Vector2.Distance(mover.transform.position,aimedTarget.Position)>Rules.Catalog.Get(aimId).range)CancelAim();
                else
                {
                    aimRemaining-=elapsed;
                    if(aimRemaining<=0)
                    {
                        var id=aimId;targets.Clear();targets.Add(aimedTarget);var point=aimedTarget.Position;
                        CancelAim();if(Rules.TryExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,()=>HitTargets(id)))Confirm(id,point,0);
                    }
                }
            }
            for(var i=burns.Count-1;i>=0;i--)
            {
                var burn=burns[i];if(!burn.Target.Alive){burns.RemoveAt(i);continue;}
                var step=Mathf.Min(elapsed,burn.Remaining);burn.Remaining-=step;burn.Tick-=step;
                while(burn.Tick<=0&&burn.Target.Alive){burn.Tick+=1;Damage(burn.Target,burn.Damage);}
                if(burn.Remaining<=0||!burn.Target.Alive)burns.RemoveAt(i);
            }
            ApplyStats();
        }
        private void Update(){if(AutomaticTick)Tick(Time.deltaTime);}
        private void ClearTargets(){CancelAim();burns.Clear();visuals?.ClearPresentation();}
        private void ResetRules(){Rules.Reset();ClearTargets();ApplyStats();}
        private void OnDisable(){CancelAim();}
        private void OnDestroy()
        {
            if(game!=null){game.HeroCombat.MovingAttackAllowed=null;game.RunDriver.Run.Reset-=ResetRules;game.RunDriver.Run.Rewound-=ClearTargets;}
            if(equipment!=null)equipment.Rules.Changed-=OnEquipmentChanged;
            if(recovery!=null){recovery.IncomingDamagePreview=null;recovery.IncomingDamageConfirmed=null;recovery.Respawned-=OnRespawn;}
        }
    }
}
