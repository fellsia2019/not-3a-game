using System;

namespace Not3A.Stage2.Integration
{
    public sealed partial class Stage2Equipment
    {
        private readonly int[] quickAccess={11,12,13,-1};
        public Stage2ItemType? QuickAccessType(int slot)=>slot>=0&&slot<quickAccess.Length&&quickAccess[slot]>=0
            ?(Stage2ItemType?)quickAccess[slot]:null;
        public int ActiveItemCount(Stage2ItemType type)
        =>ConsumableCount(type);
        public bool HasActiveAction(Stage2ItemType type)=>catalog.ActiveAction(type)!=Stage2ItemActiveAction.None;
        public bool TryAssignQuickAccess(int slot,int itemId)
        {
            var item=Find(itemId);
            if(mutating||slot<0||slot>=quickAccess.Length||item==null||item.Quantity<1||!HasActiveAction(item.Type))return false;
            if(quickAccess[slot]==(int)item.Type)return true;
            quickAccess[slot]=(int)item.Type;Changed?.Invoke();return true;
        }
        public bool TryClearQuickAccess(int slot)
        {
            if(mutating||slot<0||slot>=quickAccess.Length)return false;
            if(quickAccess[slot]<0)return true;
            quickAccess[slot]=-1;Changed?.Invoke();return true;
        }
        private void ResetQuickAccess()
        {quickAccess[0]=11;quickAccess[1]=12;quickAccess[2]=13;quickAccess[3]=-1;}
        public bool TryActivateItem(Stage2ItemType type,Func<bool> effect)
        {
            if(mutating||!HasActiveAction(type)||effect==null)return false;
            if(Stage2EquipmentCatalog.IsConsumable(type))return TryConsume(type,effect);
            var item=items.Find(i=>i.Type==type&&i.Quantity>0);if(item==null)return false;
            mutating=true;
            try {if(!effect())return false;}
            finally {mutating=false;}
            Changed?.Invoke();return true;
        }
    }
}
