using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public sealed class Stage2HeroDrop
    {
        public int Id {get;}
        public Vector2 Position {get;}
        public IReadOnlyList<Stage2EquipmentItem> Equipment {get;}
        public IReadOnlyList<Stage2BlueprintItem> Blueprints {get;}
        public int ItemCount=>Equipment.Count+Blueprints.Count;
        internal Stage2HeroDrop(int id,Vector2 position,List<Stage2EquipmentItem> gear,List<Stage2BlueprintItem> papers)
        {Id=id;Position=position;Equipment=gear.AsReadOnly();Blueprints=papers.AsReadOnly();}
    }

    /// <summary>Current-world recoverable bag ownership. No input, timers, scene or portal-enemy binding.</summary>
    public sealed class Stage2HeroRecovery
    {
        private readonly Stage2HeroProgression hero;
        private readonly Stage2Equipment equipment;
        private readonly List<Stage2HeroDrop> drops=new List<Stage2HeroDrop>();
        private int nextId=1;
        private bool mutating;
        public IReadOnlyList<Stage2HeroDrop> Drops {get;}
        public int DeathCount {get;private set;}
        public int LastXpLost {get;private set;}
        public event Action Changed;
        public event Action Died;
        public Stage2HeroRecovery(Stage2HeroProgression progression,Stage2Equipment ownedItems)
        {
            hero=progression??throw new ArgumentNullException(nameof(progression));
            equipment=ownedItems??throw new ArgumentNullException(nameof(ownedItems));Drops=drops.AsReadOnly();
        }
        public Stage2HeroDrop Find(int id)=>drops.Find(drop=>drop.Id==id);
        public bool TryApplyExplorationDamage(float amount,Vector2 position)
        {
            if(mutating||equipment.ItemMutationActive||!float.IsFinite(amount)||amount<=0||
                !float.IsFinite(position.x)||!float.IsFinite(position.y))return false;
            mutating=true;
            try
            {
                if(amount<hero.Health){hero.ApplyExplorationDamage(amount);return true;}
                var gear=new List<Stage2EquipmentItem>();var papers=new List<Stage2BlueprintItem>();
                if(nextId==int.MaxValue||!equipment.ExtractRecoveryBag(gear,papers))return false;
                if(gear.Count+papers.Count>0)drops.Add(new Stage2HeroDrop(nextId++,position,gear,papers));
                LastXpLost=hero.Reincarnate();DeathCount++;
                equipment.NotifyRecoveryChanged();Changed?.Invoke();Died?.Invoke();return true;
            }
            finally{mutating=false;}
        }
        public bool TryRecover(int id)
        {
            if(mutating||equipment.ItemMutationActive)return false;
            var drop=Find(id);if(drop==null)return false;mutating=true;
            try
            {
                if(!equipment.RestoreRecoveryBag(drop.Equipment,drop.Blueprints))return false;
                drops.Remove(drop);equipment.NotifyRecoveryChanged();Changed?.Invoke();return true;
            }
            finally{mutating=false;}
        }
        public void Reset()
        {
            if(mutating)return;
            drops.Clear();DeathCount=LastXpLost=0;Changed?.Invoke();
        }
    }
}
