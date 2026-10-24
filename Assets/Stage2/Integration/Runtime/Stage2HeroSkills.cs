using System;
using UnityEngine;
namespace Not3A.Stage2.Integration
{
    /// <summary>Personal skill ownership and bounded cadence. Scene code validates targets before committing an effect.</summary>
    public sealed class Stage2HeroSkills
    {
        private readonly Stage2HeroProgression hero;
        private readonly Stage2Equipment equipment;
        public bool StaffMasteryKnown=>equipment==null||equipment.StaffMasteryKnown;
        public Stage2HeroSkillCatalog Catalog {get;}
        private readonly int[] ranks;
        private readonly float[] cooldowns,totals;
        private readonly bool[] autocast;
        private bool executing;
        public float PotionCooldownRemaining {get;private set;}
        public float BarrierRemaining {get;private set;}
        public float Barrier {get;private set;}
        public Stage2HeroSkills(Stage2HeroProgression owner,Stage2HeroSkillCatalog catalog,Stage2Equipment gear=null)
        {
            hero=owner??throw new ArgumentNullException(nameof(owner));
            if(catalog==null||!catalog.TryValidate(out _))throw new ArgumentException("Invalid skills.");
            equipment=gear;Catalog=catalog;ranks=new int[catalog.Count];cooldowns=new float[catalog.Count];totals=new float[catalog.Count];autocast=new bool[catalog.Count];
        }
        public bool AutocastEnabled(int id)=>id>=0&&id<autocast.Length&&autocast[id];
        public bool ToggleAutocast(int id)
        {
            var node=Catalog.Get(id);
            if(executing||node==null||!node.supportsAutocast||Rank(id)==0||node.weapon==1&&!StaffMasteryKnown)return false;
            autocast[id]=!autocast[id];return true;
        }
        public int Rank(int id)=>id==0?hero.BlinkRank:id>=11&&id<=13?hero.PassiveRank((Stage2HeroPassive)(id-11)):id>=0&&id<ranks.Length?ranks[id]:0;
        public float Value(int id)=>Catalog.Get(id)?.weapon==1&&!StaffMasteryKnown?0:Catalog.Get(id)?.Power(Rank(id))??0;
        public float Cooldown(int id)=>id==0?hero.BlinkCooldownRemaining:id>0&&id<31?cooldowns[id]:0;
        public float CooldownTotal(int id)=>id==0?hero.BlinkCooldownTotal:id>0&&id<31?totals[id]:0;
        public float ManaCost(int id)=>hero.WtfForTesting?0:Catalog.Get(id)==null?0:Catalog.Get(id).Mana(Rank(id))*(Catalog.Get(id).weapon==1?1-Value(24):1);
        public int RequiredLevel(int id)=>id<=0||id>=11&&id<=13?1:Mathf.Min(95,Catalog.Get(id).requiredLevel+
            Math.Min(Rank(id),Catalog.Get(id).MaxRank-1)*(id<11?8:4));
        public bool CanUpgrade(int id,out string reason)
        {
            reason=null;var n=Catalog.Get(id);
            if(n==null){reason="Нет такого навыка";return false;}
            if(n.weapon==1&&!StaffMasteryKnown){reason="Изучите свиток «Владение посохом»";return false;}
            if(Rank(id)>=n.MaxRank){reason="Максимальный ранг";return false;}
            if(hero.Level<RequiredLevel(id)){reason="Нужен уровень "+RequiredLevel(id);return false;}
            if(n.prerequisite>=0&&Rank(n.prerequisite)==0){reason="Сначала: "+Catalog.Get(n.prerequisite).title;return false;}
            if(hero.AvailablePoints<1){reason="Нужно очко героя";return false;}return true;
        }
        public bool TryUpgrade(int id)
        {
            if(executing||!CanUpgrade(id,out _))return false;
            executing=true;
            try
            {
                if(id==0)return hero.TryUpgradeBlink();
                if(id>=11&&id<=13)return hero.TryUpgradePassive((Stage2HeroPassive)(id-11));
                ranks[id]++;
                if(hero.TrySpendPoints(1))return true;
                ranks[id]--;return false;
            }
            finally{executing=false;}
        }
        public bool CanExecute(int id,int weapon,bool equipped,out string reason)
        {
            reason=null;var n=Catalog.Get(id);
            if(executing||n==null||!n.active){reason="Сейчас недоступно";return false;}
            if(n.weapon==1&&!StaffMasteryKnown){reason="Изучите свиток «Владение посохом»";return false;}
            if(Rank(id)==0){reason="Навык не изучен";return false;}
            if(n.weapon>=0&&(!equipped||n.weapon!=weapon)){reason=n.weapon==0?"Нужен активный лук":"Нужен активный посох";return false;}
            if(!hero.WtfForTesting&&Cooldown(id)>0){reason="Перезарядка";return false;}
            if(hero.Mana<ManaCost(id)){reason="Недостаточно MP";return false;}return true;
        }
        public bool TryExecute(int id,int weapon,bool equipped,Func<bool> effect)
        {
            if(id<=0||!CanExecute(id,weapon,equipped,out _)||effect==null)return false;
            executing=true;
            try
            {
                if(!hero.TryUseMana(ManaCost(id),effect))return false;
                cooldowns[id]=totals[id]=hero.WtfForTesting?0:Catalog.Get(id).Cooldown(Rank(id));
                return true;
            }
            finally{executing=false;}
        }
        public void ApplyBarrier(){Barrier=Value(7)*(1+Value(25));BarrierRemaining=Catalog.Get(7).duration;}
        public float AbsorbDamage(float amount,float passiveReduction)
        {
            var reduced=Mathf.Max(0,amount)*(1-Mathf.Clamp(passiveReduction,0,.65f));
            var absorbed=Mathf.Min(Barrier,reduced);Barrier-=absorbed;return reduced-absorbed;
        }
        public float PreviewDamage(float amount,float passiveReduction)=>Mathf.Max(0,amount*(1-Mathf.Clamp(passiveReduction,0,.65f))-Barrier);
        public float WorldDamageBonus=>Mathf.Clamp(Value(34),0,.5f);
        private float WorldDamageTakenMultiplier=>1-Mathf.Clamp(Value(35),0,.5f);
        public float PreviewWorldDamage(float amount,float gearReduction)=>PreviewDamage(amount*WorldDamageTakenMultiplier,gearReduction);
        public float AbsorbWorldDamage(float amount,float gearReduction)=>AbsorbDamage(amount*WorldDamageTakenMultiplier,gearReduction);
        public bool TryUsePotion(Stage2Equipment items,Stage2ItemType type)
        {
            return Stage2EquipmentCatalog.IsConsumable(type)&&TryUseActiveItem(items,type);
        }
        public bool TryUseActiveItem(Stage2Equipment items,Stage2ItemType type)
        {
            if(executing||PotionCooldownRemaining>0||items==null)return false;
            var action=items.Catalog.ActiveAction(type);
            if(action==Stage2ItemActiveAction.None)return false;
            var amount=Catalog.PotionRestoration;
            executing=true;
            try
            {
                if(!items.TryActivateItem(type,()=>hero.RestoreVitals(action==Stage2ItemActiveAction.RestoreMana?0:amount,
                    action==Stage2ItemActiveAction.RestoreHealth?0:amount)))return false;
                PotionCooldownRemaining=Catalog.PotionCooldown;return true;
            }
            finally{executing=false;}
        }
        public void Tick(float elapsed,bool active)
        {
            if(!float.IsFinite(elapsed)||elapsed<0)throw new ArgumentOutOfRangeException(nameof(elapsed));
            if(!active)return;
            for(var i=1;i<31;i++)cooldowns[i]=hero.WtfForTesting?0:Mathf.Max(0,cooldowns[i]-elapsed);
            PotionCooldownRemaining=Mathf.Max(0,PotionCooldownRemaining-elapsed);
            BarrierRemaining=Mathf.Max(0,BarrierRemaining-elapsed);if(BarrierRemaining==0)Barrier=0;
        }
        public void ClearBuffs(){BarrierRemaining=Barrier=0;}
        public void Reset(){Array.Clear(ranks,0,ranks.Length);Array.Clear(cooldowns,0,cooldowns.Length);Array.Clear(totals,0,totals.Length);Array.Clear(autocast,0,autocast.Length);PotionCooldownRemaining=0;ClearBuffs();}
    }
}
