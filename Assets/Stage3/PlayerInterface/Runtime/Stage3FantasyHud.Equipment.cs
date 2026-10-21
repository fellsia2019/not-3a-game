using System;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed class Stage3EquipmentItemView
    {
        public int Id=-1,TypeIndex,QualityIndex,Level=1;
        public string Name;
        public Stage3SelectedBuildingStat[] Properties;
        public bool CanEquip,IsEquipped;
        public string EquipUnavailableReason,EquippedLabel;
        public Stage3BlueprintItemView Blueprint;
    }
    public sealed class Stage3EquipmentView
    {
        public Stage3EquipmentItemView[] Items;
        public Stage3EquipmentItemView[] BagItems;
        public int[] EquippedItemIds;
        public int SelectedItemId=-1;
        public bool CanUnequip;
        public string UnequipUnavailableReason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3EquipmentView equipment;
        private Stage3ForgeArt equipmentArt;
        private Vector2 equipmentScroll;
        private static readonly string[] EquipmentTypeNames={"Лук","Жезл","Шлем","Нагрудник","Руки","Ноги","Кирка","Топор"};
        private static readonly string[] EquipmentSlotNames={"Шлем","Нагрудник","Руки","Ноги","Оружие","Кирка","Топор"};
        private static readonly string[] EquipmentQualities={"F","E","D","C","B","A","S","SSS"};
        private static readonly Color[] EquipmentQualityColors={new Color32(101,65,43,255),new Color32(133,133,133,255),new Color32(21,156,59,255),new Color32(36,86,220,255),new Color32(145,70,186,255),new Color32(203,66,75,255),new Color32(244,185,0,255),new Color32(255,253,235,255)};
        public Stage3EquipmentView EquipmentView=>equipment;
        public string EquipmentInteractionHint { get; set; }
        public event Action<int> EquipmentItemSelected,EquipItemRequested,UnequipSlotRequested;
        public void SetEquipment(Stage3EquipmentView value)=>equipment=value;
        public void SetEquipmentArt(Stage3ForgeArt value)=>equipmentArt=value;
        public Stage3EquipmentItemView EquippedItem(int slot)
        {
            if(equipment?.EquippedItemIds==null||slot<0||slot>=equipment.EquippedItemIds.Length) return null;
            return FindEquipmentItem(equipment.EquippedItemIds[slot]);
        }
        private Stage3EquipmentItemView FindEquipmentItem(int id)
        {
            if(id<0||equipment?.Items==null) return null;
            foreach(var item in equipment.Items) if(item!=null&&item.Id==id)return item;
            return null;
        }
        private static string QualityName(int index)=>index>=0&&index<8?EquipmentQualities[index]:"—";
        private void DrawEquipmentIcon(Rect rect,Stage3EquipmentItemView item)
        {
            if(item?.Blueprint!=null) DrawBlueprintIcon(rect,item.Blueprint);
            else if(item!=null) DrawEquipmentIcon(rect,item.TypeIndex,item.QualityIndex);
        }
        private void DrawEquipmentIcon(Rect rect,int type,int quality)
        {
            if(equipmentArt!=null&&equipmentArt.TryGetIcon(type,quality,out var texture,out var uv))
            {
                uv=equipmentArt.UiVisibleBounds(type,quality,uv);
                var aspect=texture.width*uv.width/(texture.height*uv.height);
                var width=Mathf.Min(rect.width,rect.height*aspect);var height=width/aspect;
                var target=new Rect(rect.center.x-width*.5f,rect.center.y-height*.5f,width,height);
                if(type==7)
                {
                    for(var band=0;band<2;band++)
                    {
                        var part=equipmentArt.AxeIconBand(quality,band);
                        part=Rect.MinMaxRect(Mathf.Max(part.xMin,uv.xMin),Mathf.Max(part.yMin,uv.yMin),
                            Mathf.Min(part.xMax,uv.xMax),Mathf.Min(part.yMax,uv.yMax));
                        if(part.width<=0f||part.height<=0f)continue;
                        // Both masked bands meet at the same rounded physical pixel.
                        // Fractional adjacent Graphics quads can otherwise expose a
                        // hairline through the blade after visible-bounds fitting.
                        var box=Rect.MinMaxRect(
                            Mathf.Round(target.x+(part.xMin-uv.xMin)/uv.width*width),
                            Mathf.Round(target.y+(uv.yMax-part.yMax)/uv.height*height),
                            Mathf.Round(target.x+(part.xMax-uv.xMin)/uv.width*width),
                            Mathf.Round(target.y+(uv.yMax-part.yMin)/uv.height*height));
                        DrawEquipmentTexture(box,texture,part);
                    }
                }
                else DrawEquipmentTexture(target,texture,uv);
            }
            else if(type==0) DrawIcon(rect,art.bowIcon);
        }
        private void DrawQualityBadge(Rect rect,int quality)
        {
            DrawEquipmentQualityFrame(rect,quality);
            PaleLabel(Inset(rect,4f*UiScale),QualityName(quality),18,TextAnchor.MiddleCenter);
        }
        private Rect EquipmentBagRect=>new Rect(WindowContentRect.x,WindowContentRect.y+92f*UiScale,WindowContentRect.width*.34f,WindowContentRect.height-92f*UiScale);
        public Rect EquipmentItemRect(int index)
        {
            var r=EquipmentBagRect;var s=UiScale;var side=(r.width-4*7f*s-16f*s)/5f;
            return new Rect(r.x+(index%5)*(side+7f*s),r.y+(index/5)*(side+27f*s)-equipmentScroll.y,side,side);
        }
        public Rect EquipmentSlotRect(int slot)
        {
            if(slot<0||slot>6)return Rect.zero;
            var r=WindowContentRect;var s=UiScale;var center=new Rect(r.x+r.width*.37f,r.y,r.width*.34f,r.height);
            var side=Mathf.Min(102f*s,center.width*.23f);var top=center.y+66f*s;
            if(slot<3) return new Rect(center.x,top+slot*(side+35f*s),side,side);
            if(slot==4)return new Rect(center.xMax-side,top,side,side);
            if(slot==3)return new Rect(center.xMax-side,top+2*(side+35f*s),side,side);
            return new Rect(center.center.x+(slot==5?-side-12f*s:12f*s),center.yMax-side-23f*s,side,side);
        }
        public Rect EquipmentActionRect=>new Rect(WindowContentRect.x+WindowContentRect.width*.75f,WindowContentRect.yMax-90f*UiScale,WindowContentRect.width*.25f,58f*UiScale);
        private void DrawEquipmentInventory(Rect content)
        {
            var s=UiScale;var bag=EquipmentBagRect;
            Label(new Rect(content.x,content.y,bag.width,58f*s),"Предметы",42);
            DrawRule(new Rect(content.x,content.y+76f*s,bag.width,1f*s));
            var items=equipment.BagItems??equipment.Items??Array.Empty<Stage3EquipmentItemView>();
            var cell=(bag.width-4*7f*s-16f*s)/5f;var rows=(items.Length+4)/5;
            equipmentScroll=GUI.BeginScrollView(bag,equipmentScroll,new Rect(0,0,bag.width-16f*s,Mathf.Max(bag.height,rows*(cell+27f*s))));
            var selectedId=-1;
            equipmentTextureClip=new Rect(equipmentScroll.x,equipmentScroll.y,bag.width-16f*s,bag.height);
            for(var i=0;i<items.Length;i++)
            {
                var item=items[i];if(item==null)continue;
                var box=new Rect((i%5)*(cell+7f*s),(i/5)*(cell+27f*s),cell,cell);
                if(SurfaceButton(box,false,false))selectedId=item.Id;
                DrawEquipmentCell(box,item,item.Id==equipment.SelectedItemId);
                var label=ItemCellCaption(item);
                Label(new Rect(box.x,box.yMax+1f*s,box.width,22f*s),label,12,TextAnchor.MiddleCenter);
            }
            equipmentTextureClip=null;
            GUI.EndScrollView();
            if(items.Length==0)Label(bag,"Сумка пуста",18,TextAnchor.MiddleCenter,true);
            var center=new Rect(content.x+content.width*.37f,content.y,content.width*.34f,content.height);
            Label(new Rect(center.x,center.y,center.width,42f*s),"Снаряжение",28,TextAnchor.MiddleCenter);
            DrawEquipmentPaperdoll(new Rect(center.x+center.width*.18f,center.y+52f*s,center.width*.64f,center.height-170f*s));
            for(var slot=0;slot<7;slot++)
            {
                var box=EquipmentSlotRect(slot);var item=EquippedItem(slot);
                if(SurfaceButton(box,false,true)&&item!=null)selectedId=item.Id;
                if(item!=null)DrawEquipmentCell(box,item,item.Id==equipment.SelectedItemId);
                else Label(box,"—",24,TextAnchor.MiddleCenter,true);
                Label(new Rect(box.x-10f*s,box.yMax+2f*s,box.width+20f*s,23f*s),EquipmentSlotNames[slot],14,TextAnchor.MiddleCenter);
            }
            Label(new Rect(center.x,EquipmentSlotRect(5).y-31f*s,center.width,27f*s),"Инструменты",22,TextAnchor.MiddleCenter);
            var chosen=FindEquipmentItem(equipment.SelectedItemId);
            DrawEquipmentDetails(new Rect(content.x+content.width*.75f,content.y,content.width*.25f,content.height),chosen);
            if(selectedId>=0)EquipmentItemSelected?.Invoke(selectedId);
        }
        private void DrawEquipmentPaperdoll(Rect rect)
        {
            var hero=art.inventoryHero??art.heroPortrait;DrawIcon(rect,hero);
            var equipped=EquippedItem(4);
            if(hero==null||hero!=art.inventoryHero||equipped==null||equipped.TypeIndex!=0||equipmentArt?.heldEquipment==null)return;
            var held=equipmentArt.heldEquipment.Bow(equipped.QualityIndex,0);if(held==null)return;
            // Same approved front-idle actor and carrying-hand registration as the world avatar.
            // Texture coordinates remain relative to the original 1254px idle sheet.
            var scale=Mathf.Min(rect.width/hero.rect.width,rect.height/hero.rect.height);
            var target=new Rect(rect.center-hero.rect.size*scale*.5f,hero.rect.size*scale);
            var hand=new Vector2(1054f*hero.texture.width/1254f-hero.rect.x,
                hero.rect.yMax-(hero.texture.height-955f*hero.texture.height/1254f));
            var grip=target.position+hand*scale;
            var heldScale=hero.pixelsPerUnit/held.pixelsPerUnit*scale;
            // Mirror the upright projection away from the torso, exactly as in the world front idle.
            var heldRect=new Rect(grip.x-(held.rect.width-held.pivot.x)*heldScale,
                grip.y-(held.rect.height-held.pivot.y)*heldScale,held.rect.width*heldScale,held.rect.height*heldScale);
            DrawUiTexture(heldRect,held.texture,new Rect(held.rect.xMax/held.texture.width,held.rect.y/held.texture.height,
                -held.rect.width/held.texture.width,held.rect.height/held.texture.height));
        }
        private void DrawEquipmentDetails(Rect r,Stage3EquipmentItemView item)
        {
            var s=UiScale;
            if(item==null){Label(r,"Выберите предмет",22,TextAnchor.MiddleCenter,true);return;}
            if(item.Blueprint!=null){DrawBlueprintInventoryDetails(r,item);return;}
            Label(new Rect(r.x,r.y,r.width,52f*s),item.Name,36);
            DrawRule(new Rect(r.x,r.y+72f*s,r.width,s));
            DrawEquipmentCell(new Rect(r.x,r.y+88f*s,r.width*.66f,r.height*.40f),item);
            Label(new Rect(r.x+r.width*.66f,r.y+106f*s,r.width*.34f,26f*s),"Качество",15,TextAnchor.MiddleCenter,true);
            DrawQualityBadge(new Rect(r.x+r.width*.72f,r.y+138f*s,r.width*.22f,52f*s),item.QualityIndex);
            Label(new Rect(r.x,r.y+r.height*.52f,r.width,31f*s),$"LVL {item.Level} / 100",24,TextAnchor.MiddleCenter);
            var props=item.Properties??Array.Empty<Stage3SelectedBuildingStat>();var y=r.y+r.height*.60f;
            DrawRule(new Rect(r.x,y-8f*s,r.width,s));
            for(var i=0;i<props.Length;i++)
            {
                var row=new Rect(r.x,y+i*31f*s,r.width,29f*s);
                Label(new Rect(row.x,row.y,row.width*.62f,row.height),props[i].Label,17,TextAnchor.MiddleLeft,true);
                Label(new Rect(row.x+row.width*.62f,row.y,row.width*.38f,row.height),props[i].Value,18,TextAnchor.MiddleRight);
            }
            var equippedSlot=-1;
            if(equipment.EquippedItemIds!=null)for(var i=0;i<equipment.EquippedItemIds.Length;i++)if(equipment.EquippedItemIds[i]==item.Id)equippedSlot=i;
            var unequip=equippedSlot>=0;var available=unequip?equipment.CanUnequip:item.CanEquip;
            if(Button(EquipmentActionRect,unequip?"Снять":"Надеть",false,available))
            {if(unequip)UnequipSlotRequested?.Invoke(equippedSlot);else EquipItemRequested?.Invoke(item.Id);}
            var reason=available?(item.IsEquipped?item.EquippedLabel:null):unequip?equipment.UnequipUnavailableReason:item.EquipUnavailableReason;
            Label(new Rect(r.x,EquipmentActionRect.yMax+3f*s,r.width,28f*s),reason??"",12,TextAnchor.MiddleCenter,true);
        }
    }
}
