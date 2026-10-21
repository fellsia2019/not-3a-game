using System.Collections.Generic;

namespace Not3A.Stage2.Integration
{
    public sealed partial class Stage2Equipment
    {
        // Recovery keeps the same instances/IDs and never touches equipped slots,
        // learned knowledge, forge state or the separate resource/gold wallet.
        internal bool ExtractRecoveryBag(List<Stage2EquipmentItem> gear,List<Stage2BlueprintItem> papers)
        {
            if(!BeginItemMutation())return false;
            try
            {
                foreach(var item in items)if(!IsEquipped(item))gear.Add(item);
                items.RemoveAll(item=>!IsEquipped(item));Blueprints?.ExtractRecoveryPapers(papers);
            }
            finally{EndItemMutation();}
            return true;
        }
        internal bool RestoreRecoveryBag(IReadOnlyList<Stage2EquipmentItem> gear,IReadOnlyList<Stage2BlueprintItem> papers)
        {
            if(!BeginItemMutation())return false;
            try
            { foreach(var item in gear)items.Add(item);Blueprints?.RestoreRecoveryPapers(papers); }
            finally{EndItemMutation();}
            return true;
        }
        internal void NotifyRecoveryChanged(){Changed?.Invoke();Blueprints?.NotifyRecoveryChanged();}
    }
}
