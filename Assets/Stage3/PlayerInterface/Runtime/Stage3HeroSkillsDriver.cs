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
        private readonly List<DamageArea> areas=new List<DamageArea>();
        private float autocastCheck;
        public Func<bool> AutoCastBlocked {get;set;}
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
        private sealed class Burn {public int Id;public Target Target;public double Remaining,Tick;public int Damage;public Stage2DamageSchool School;}
        private sealed class DamageArea {public int Id,Damage,Maximum;public Vector2 Centre;public float Radius;public double Remaining,Tick;public Stage2DamageSchool School;}
        public void Initialize(Stage2GameComposition owner,Stage2PlayerMover player,Stage2HeroProgressionDriver progression,Stage2EquipmentDriver gear,Stage2HeroSkillCatalog catalog)
        {
            game=owner;mover=player;hero=progression;equipment=gear;
            Rules=new Stage2HeroSkills(hero.Rules,catalog,equipment.Rules);
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
            recovery.IncomingDamagePreview=amount=>Rules.PreviewWorldDamage(amount,equipment.ProtectionFraction);
            recovery.IncomingDamageConfirmed=amount=>Rules.AbsorbWorldDamage(amount,equipment.ProtectionFraction);
            recovery.Respawned+=OnRespawn;
        }
        private void OnRespawn(){ClearTargets();Rules.ClearBuffs();}
        private void OnEquipmentChanged(){CancelAim();ApplyStats();}
        private void ApplyStats()
        {
            game.HeroCombat.DamageSchool=ActiveWeapon==1?Stage2DamageSchool.Magic:Stage2DamageSchool.Physical;
            if(guild==null)guild=GetComponent<Stage3GuildWorldDriver>();
            if(guild!=null){guild.KillExperienceBonus=Rules.Value(33);guild.WorldDamageBonus=Rules.WorldDamageBonus;}
            var moving=Rules.Rank(14);
            var movePenalty=mover.IsMoving&&moving>0?Mathf.Min(1,.4f+.2f*moving):1;
            // Temporary weapon-specific effects never remain active on the other weapon.
            var magic=ActiveWeapon==1?1+Rules.Value(23):1;
            game.HeroCombat.Rules.SetAbilityBonuses(magic*movePenalty,1);
            mover.SetPersonalSpeedMultiplier(hero.Rules.MovementSpeedMultiplier);
            hero.Rules.SetAdditionalMana(equipment.ManaBonus+Rules.Value(29),1+Rules.Value(30));
        }
        public bool CanCast(int id,out string reason)
        {
            if(!CanControl){reason="Сейчас недоступно";return false;}
            return Rules.CanExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,out reason);
        }
        private static bool IsAreaSkill(int id)=>id==2||id==3||id==4||id==5||id==6||id==9||id==10;
        private bool Collect(int id,Vector2 point)
        {
            targets.Clear();var n=Rules.Catalog.Get(id);
            if(n==null||!float.IsFinite(point.x)||!float.IsFinite(point.y)||Vector2.Distance(mover.transform.position,point)>n.range)return false;
            CollectArea(point,IsAreaSkill(id)?n.radius:1.1f,IsAreaSkill(id)?n.maxTargets:1);
            return targets.Count>0;
        }
        private void CollectArea(Vector2 point,float radius,int maximum)
        {
            targets.Clear();
            for(var i=0;i<game.Enemies.ActiveCount;i++)
            {var unit=game.Enemies.GetEnemy(i);if(unit!=null&&unit.IsAlive&&Vector2.Distance(unit.transform.position,point)<=radius)targets.Add(new Target(unit));}
            if(guild==null)guild=GetComponent<Stage3GuildWorldDriver>();
            if(guild!=null)foreach(var unit in guild.Enemies)if(unit.Alive&&Vector2.Distance(unit.Position,point)<=radius)targets.Add(new Target(unit));
            targets.Sort((a,b)=>(a.Position-point).sqrMagnitude.CompareTo((b.Position-point).sqrMagnitude));
            if(targets.Count>maximum)targets.RemoveRange(maximum,targets.Count-maximum);
        }
        public bool IsTargetValid(int id,Vector2 point)
        {
            if(!CanCast(id,out _))return false;
            return id==7||id==8? id!=8||hero.Rules.Health<hero.Rules.MaximumHealth:Collect(id,point);
        }
        public void ShowTarget(int id,Vector2 point)
        {var n=Rules?.Catalog.Get(id);if(n==null)return;visuals.SetTargetPreview(id,point,IsAreaSkill(id)?n.radius:.4f,IsTargetValid(id,point));}
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
            if(id==7||id==8)
            {
                var success=Rules.TryExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,()=>
                {
                    if(id==7)Rules.ApplyBarrier();
                    else return hero.Rules.RestoreVitals(Rules.Value(8)*(1+Rules.Value(26)),0);
                    return true;
                });
                if(success)Confirm(id,mover.transform.position,0);return success;
            }
            if(!Collect(id,point))return false;
            var hitPoint=IsAreaSkill(id)?point:targets[0].Position;
            var result=Rules.TryExecute(id,ActiveWeapon,equipment.Rules.ActiveWeapon!=null,()=>{var hit=HitTargets(id);if(hit&&(id==4||id==9))CreateDamageArea(id,hitPoint);return hit;});
            if(result)Confirm(id,hitPoint,IsAreaSkill(id)?n.radius:0);
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
                if(!Damage(target,damage,id>=6?Stage2DamageSchool.Magic:Stage2DamageSchool.Physical))continue;LastTargetCount++;LastAppliedDamage+=damage;
                var status=Status(target);
                if((id==2||id==5)&&target.Alive)
                {
                    var existing=burns.Find(b=>b.Id==id&&ReferenceEquals(b.Target.Portal,target.Portal)&&ReferenceEquals(b.Target.World,target.World));
                    if(existing==null){existing=new Burn{Id=id,Target=target};burns.Add(existing);}
                    existing.Damage=PeriodicDamage(id);existing.School=id==5?Stage2DamageSchool.Physical:Stage2DamageSchool.Magic;
                    existing.Remaining=n.duration+Rules.Value(id==2?18:22);existing.Tick=1;
                    if(id==2)status.MarkBurning((float)existing.Remaining);
                    else {status.MarkPoisoned((float)existing.Remaining);status.ApplySlow(n.slowMultiplier,(float)existing.Remaining);status.Weaken(Stage2DamageSchool.Physical,n.vulnerability,(float)existing.Remaining);}
                }
                if(id==3&&target.Alive)
                {
                    status.Freeze(n.controlDuration*(1+Rules.Value(19)));
                    status.ApplySlow(Mathf.Clamp(n.slowMultiplier-Rules.Value(19),.25f,1),n.duration+Rules.Value(20));
                }
                if(id==10&&target.Alive)
                {status.Stun(n.controlDuration);status.Weaken(Stage2DamageSchool.Magic,n.vulnerability,n.duration);}
            }
            return LastTargetCount>0;
        }
        private static Stage2CombatStatuses Status(Target target)=>target.Portal!=null?target.Portal.Statuses:target.World.Statuses;
        private bool Damage(Target t,int amount,Stage2DamageSchool school)=>t.Alive&&(t.Portal!=null?t.Portal.TakeDamage(amount,school):guild!=null&&guild.DamageWithSchool(t.World,amount,school));
        private int PeriodicDamage(int id)
        {
            var n=Rules.Catalog.Get(id);var modifier=id==2?Rules.Value(17):id==9?Rules.Value(27):0;
            return Mathf.Max(1,Mathf.RoundToInt(game.HeroCombat.Rules.EffectiveDamageExact*n.periodicPower*
                n.Power(Rules.Rank(id))/Mathf.Max(.01f,n.Power(1))*(1+modifier)));
        }
        private void CreateDamageArea(int id,Vector2 point)
        {
            var n=Rules.Catalog.Get(id);var duration=n.duration+(id==4?Rules.Value(21):0);
            areas.Add(new DamageArea{Id=id,Centre=point,Radius=n.radius,Remaining=duration,Tick=1,Damage=PeriodicDamage(id),Maximum=n.maxTargets,
                School=id==9?Stage2DamageSchool.Magic:Stage2DamageSchool.Physical});
            visuals.PresentPersistentSkillArea(id,point,n.radius,duration);
        }
        private void TickAutocast(float elapsed)
        {
            autocastCheck=Mathf.Max(0,autocastCheck-elapsed);
            if(autocastCheck>0||IsAiming||!CanControl||(AutoCastBlocked?.Invoke()??false))return;
            autocastCheck=.1f;var id=ActiveWeapon==0?1:6;
            if(!Rules.AutocastEnabled(id)||!CanCast(id,out _)||id==1&&mover.IsMoving)return;
            var origin=(Vector2)mover.transform.position;var range=Mathf.Min(Rules.Catalog.Get(id).range,game.HeroCombat.Rules.EffectiveRange);
            var portal=game.Enemies.FindNearest(origin,range);var best=portal!=null?((Vector2)portal.transform.position-origin).sqrMagnitude:range*range;
            var found=portal!=null;var point=portal!=null?(Vector2)portal.transform.position:origin;
            if(guild==null)guild=GetComponent<Stage3GuildWorldDriver>();
            if(guild!=null)foreach(var enemy in guild.Enemies)
            {if(!enemy.Alive)continue;var distance=(enemy.Position-origin).sqrMagnitude;if(distance>best||found&&distance==best)continue;best=distance;point=enemy.Position;found=true;}
            if(found)TryCast(id,point);
        }
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
        public bool TryUseActiveItem(Stage2ItemType type)=>CanControl&&Rules.TryUseActiveItem(equipment.Rules,type);
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
                var step=Math.Min(elapsed,burn.Remaining);burn.Remaining-=step;burn.Tick-=step;
                while(burn.Tick<=0&&burn.Target.Alive){burn.Tick+=1;Damage(burn.Target,burn.Damage,burn.School);}
                if(burn.Remaining<=0||!burn.Target.Alive)burns.RemoveAt(i);
            }
            for(var i=areas.Count-1;i>=0;i--)
            {
                var area=areas[i];var step=Math.Min(elapsed,area.Remaining);area.Remaining-=step;area.Tick-=step;
                while(area.Tick<=0)
                {
                    area.Tick+=1;CollectArea(area.Centre,area.Radius,area.Maximum);
                    foreach(var target in targets)
                    {if(Damage(target,area.Damage,area.School)&&target.Alive&&area.Id==9)Status(target).MarkBurning((float)Math.Min(1,area.Remaining));}
                }
                if(area.Remaining<=0)areas.RemoveAt(i);
            }
            TickAutocast(elapsed);
            ApplyStats();
        }
        private void Update(){if(AutomaticTick)Tick(Time.deltaTime);}
        private void ClearTargets(){CancelAim();burns.Clear();areas.Clear();autocastCheck=0;visuals?.ClearPresentation();}
        private void ResetRules(){Rules.Reset();ClearTargets();ApplyStats();}
        private void OnDisable(){CancelAim();}
        private void OnDestroy()
        {
            if(game!=null){game.HeroCombat.MovingAttackAllowed=null;game.RunDriver.Run.Reset-=ResetRules;game.RunDriver.Run.Rewound-=ClearTargets;}
            if(equipment!=null)equipment.Rules.Changed-=OnEquipmentChanged;
            if(recovery!=null){recovery.IncomingDamagePreview=null;recovery.IncomingDamageConfirmed=null;recovery.Respawned-=OnRespawn;}
            if(guild!=null)guild.WorldDamageBonus=0;
        }
    }
}
