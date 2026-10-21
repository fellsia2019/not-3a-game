using System;

namespace Not3A.Stage2.Integration
{
    public sealed partial class Stage2Equipment
    {
        public Stage2Blueprints Blueprints {get;private set;}
        public void EnableBlueprintResearch(Stage2BlueprintCatalog data)
        {
            if(data==null||!data.TryValidate(out _))throw new ArgumentException("Invalid blueprint catalog.");
            if(mutating||Blueprints!=null)throw new InvalidOperationException("Blueprint ownership is configured once before transactions.");
            Blueprints=new Stage2Blueprints(this,wallet,data);Changed?.Invoke();
        }
        internal bool ItemMutationActive=>mutating;
        internal int ReserveInventoryId()
        {
            if(mutating||nextId==int.MaxValue)throw new InvalidOperationException("Cannot reserve inventory identity.");
            return nextId++;
        }
        internal bool BeginItemMutation(){if(mutating)return false;mutating=true;return true;}
        internal void EndItemMutation()=>mutating=false;
        private bool KnowsForgeStage(int stage)=>Blueprints!=null&&Blueprints.IsStudied(stage==2?Stage2BlueprintKind.ForgeII:Stage2BlueprintKind.ForgeIII);
    }
}
