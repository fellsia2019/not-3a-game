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
        public bool CanEquip,IsEquipped,Consumable,CanUse,CanAssignQuickAccess;
        public int Quantity=1;
        public string EquipUnavailableReason,EquippedLabel;
        public Stage3BlueprintItemView Blueprint;
    }
    public sealed class Stage3EquipmentView
    {
        public Stage3EquipmentItemView[] Items;
        public Stage3EquipmentItemView[] BagItems;
        public int[] EquippedItemIds;
        public int SelectedItemId=-1,ActiveWeapon;
        public bool CanUnequip;
        public string UnequipUnavailableReason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3EquipmentView equipment;
        private Stage3ForgeArt equipmentArt;
        private Vector2 equipmentScroll;
        private static readonly string[] EquipmentTypeNames={"Лук","Посох","Шлем","Нагрудник","Руки","Ноги","Кирка","Топор","Амулет натиска","Кольцо","Защитная печать"};
        private static readonly string[] EquipmentSlotNames={"Шлем","Грудь","Руки","Ноги","Лук","Кирка","Топор","Посох","Амулет","Кольцо","Кольцо","Защитная печать"};
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
        private const int EquipmentBagColumns=6;
        private float equipmentNativeScrollbarSpace=20f;
        private float EquipmentScrollbarSpace=>Mathf.Max(16f*UiScale,equipmentNativeScrollbarSpace);
        private float EquipmentCellSide(Rect bag)=>(bag.width-Mathf.Max(0f,EquipmentScrollbarSpace-16f*UiScale)-(EquipmentBagColumns-1)*9f*UiScale-24f*UiScale)/EquipmentBagColumns;
        private Rect EquipmentLoadoutRect=>new Rect(WindowContentRect.x,WindowContentRect.y,WindowContentRect.width*.335f,WindowContentRect.height);
        private Rect EquipmentBagRect
        {
            get
            {
                var r=new Rect(WindowContentRect.x+WindowContentRect.width*.36f,WindowContentRect.y+62f*UiScale,WindowContentRect.width*.365f,WindowContentRect.height-178f*UiScale);
                var side=(r.width-(EquipmentBagColumns-1)*9f*UiScale-24f*UiScale)/EquipmentBagColumns;
                var pitch=side+25f*UiScale;
                r.height=Mathf.Max(1,Mathf.FloorToInt((r.height-8f*UiScale)/pitch))*pitch+8f*UiScale;
                r.width+=Mathf.Max(0f,EquipmentScrollbarSpace-16f*UiScale);
                return r;
            }
        }
        private float EquipmentSlotSide=>Mathf.Min(90f*UiScale,EquipmentLoadoutRect.width*.215f);
        public Rect EquipmentWeaponSwitchRect=>new Rect(EquipmentLoadoutRect.center.x-92f*UiScale,
            EquipmentLoadoutRect.yMax-EquipmentSlotSide-58f*UiScale,184f*UiScale,28f*UiScale);
        public Rect EquipmentItemRect(int index)
        {
            if(index<0)return Rect.zero;
            var r=EquipmentBagRect;var s=UiScale;var side=EquipmentCellSide(r);
            return new Rect(r.x+4f*s+(index%EquipmentBagColumns)*(side+9f*s),r.y+4f*s+(index/EquipmentBagColumns)*(side+25f*s)-equipmentScroll.y,side,side);
        }
        public Rect EquipmentSlotRect(int slot)
        {
            if(slot<0||slot>11)return Rect.zero;
            var r=EquipmentLoadoutRect;var s=UiScale;var side=EquipmentSlotSide;
            var top=r.y+62f*s;var stride=(EquipmentWeaponSwitchRect.y-10f*s-top-side)/3f;
            if(slot<4)return new Rect(r.x,top+slot*stride,side,side);
            var rightRow=slot==4?0:slot==7?1:slot==8?2:slot==11?3:-1;
            if(rightRow>=0)return new Rect(r.xMax-side,top+rightRow*stride,side,side);
            var bottomColumn=slot==9?0:slot==10?1:slot==5?2:3;
            var gap=(r.width-4f*side)/3f;
            return new Rect(r.x+bottomColumn*(side+gap),r.yMax-side-24f*s,side,side);
        }
        public Rect EquipmentActionRect=>new Rect(WindowContentRect.x+WindowContentRect.width*.75f,WindowContentRect.yMax-90f*UiScale,WindowContentRect.width*.25f,58f*UiScale);
        private void DrawEquipmentInventory(Rect content)
        {
            equipmentNativeScrollbarSpace=GUI.skin.verticalScrollbar.fixedWidth+GUI.skin.verticalScrollbar.margin.left+2f;
            var s=UiScale;var bag=EquipmentBagRect;var loadout=EquipmentLoadoutRect;
            Label(new Rect(loadout.x,content.y,loadout.width,48f*s),"Снаряжение",34);
            Label(new Rect(bag.x,content.y,bag.width,48f*s),"Сумка",34);
            DrawRule(new Rect(bag.x,content.y+49f*s,bag.width,s));
            var items=equipment.BagItems??equipment.Items??Array.Empty<Stage3EquipmentItemView>();
            var cell=EquipmentCellSide(bag);
            var visibleRows=Mathf.Max(1,Mathf.FloorToInt(bag.height/(cell+25f*s)));
            var rows=Mathf.Max(visibleRows,(items.Length+EquipmentBagColumns-1)/EquipmentBagColumns);
            equipmentScroll=GUI.BeginScrollView(bag,equipmentScroll,new Rect(0,0,bag.width-EquipmentScrollbarSpace,Mathf.Max(bag.height,rows*(cell+25f*s)+8f*s)),false,true,GUIStyle.none,GUI.skin.verticalScrollbar);
            equipmentGuiOrigin=bag.position-equipmentScroll;
            var selectedId=-1;
            equipmentTextureClip=new Rect(equipmentScroll.x,equipmentScroll.y,bag.width-EquipmentScrollbarSpace,bag.height);
            for(var i=0;i<rows*EquipmentBagColumns;i++)
            {
                var box=new Rect(4f*s+(i%EquipmentBagColumns)*(cell+9f*s),4f*s+(i/EquipmentBagColumns)*(cell+25f*s),cell,cell);
                if(box.y<equipmentScroll.y||box.yMax+23f*s>equipmentScroll.y+bag.height)continue;
                var item=i<items.Length?items[i]:null;
                if(item==null){DrawBlueprintNeutralFrame(box);continue;}
                if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
                if(GUI.Button(box,GUIContent.none,blankButton))selectedId=item.Id;
                DrawEquipmentCell(box,item,item.Id==equipment.SelectedItemId);
                Label(new Rect(box.x,box.yMax+s,box.width,22f*s),ItemCellCaption(item),12,TextAnchor.MiddleCenter);
            }
            equipmentTextureClip=null;
            GUI.EndScrollView();
            equipmentGuiOrigin=Vector2.zero;
            equipmentScroll.y=Mathf.Round(equipmentScroll.y/(cell+25f*s))*(cell+25f*s);
            var dollTop=loadout.y+60f*s;
            DrawEquipmentPaperdoll(new Rect(loadout.x+EquipmentSlotSide*.92f,dollTop,
                loadout.width-EquipmentSlotSide*1.84f,EquipmentWeaponSwitchRect.y-dollTop-5f*s));
            for(var slot=0;slot<12;slot++)
            {
                var box=EquipmentSlotRect(slot);var item=EquippedItem(slot);
                if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
                if(GUI.Button(box,GUIContent.none,blankButton)&&item!=null)selectedId=item.Id;
                if(item!=null)DrawEquipmentCell(box,item,item.Id==equipment.SelectedItemId);
                else DrawEmptyEquipmentSlot(box,slot);
                if(slot==(equipment.ActiveWeapon==1?7:4))
                {
                    // Active weapon gets a compact cyan jewel, separate from the inspected item's frame.
                    DrawLunarJewel(new Vector2(box.xMax-3f*s,box.y+4f*s),9f*s);
                    DrawGlyphLine(new Vector2(box.x+9f*s,box.yMax-3f*s),new Vector2(box.xMax-9f*s,box.yMax-3f*s),2f*s,Accent);
                }
                Label(new Rect(box.x-6f*s,box.yMax+2f*s,box.width+12f*s,21f*s),EquipmentSlotNames[slot],slot==11?12:14,TextAnchor.MiddleCenter);
            }
            if(Button(EquipmentWeaponSwitchRect,"Z · Сменить"))RequestWeaponSwitch();
            DrawEquipmentDetails(new Rect(content.x+content.width*.75f,content.y,content.width*.25f,content.height),FindEquipmentItem(equipment.SelectedItemId));
            DrawInventoryQuickAccess(FindEquipmentItem(equipment.SelectedItemId));
            if(selectedId>=0)EquipmentItemSelected?.Invoke(selectedId);
        }
        public Rect InventoryQuickSlotRect(int index)
        {
            if(index<0||index>3)return Rect.zero;
            var s=UiScale;var bag=EquipmentBagRect;
            return new Rect(bag.x+4f*s+index*72f*s,WindowContentRect.yMax-68f*s,64f*s,64f*s);
        }

        private void DrawInventoryQuickAccess(Stage3EquipmentItemView selected)
        {
            var s=UiScale;var bag=EquipmentBagRect;
            HudReferenceLabel(new Rect(bag.x,WindowContentRect.yMax-112f*s,bag.width,23f*s),"Быстрый доступ",18,TextAnchor.MiddleLeft,true);
            Label(new Rect(bag.x,WindowContentRect.yMax-88f*s,bag.width,18f*s),
                selected?.CanAssignQuickAccess==true?"ЛКМ · Назначить выбранный предмет    ПКМ · Очистить":"Выберите применяемый предмет    ПКМ · Очистить",13,TextAnchor.MiddleLeft,true);
            for(var i=0;i<4;i++)
            {
                var r=InventoryQuickSlotRect(i);var slot=FindQuickSlot(i);var assigned=slot!=null&&slot.ItemTypeIndex>=0;
                DrawHudPlainTile(r,Color.clear);
                if(assigned)DrawQuickAccessArtwork(new Rect(r.center.x-17f*s,r.y+12f*s,40f*s,50f*s),slot);
                DrawHudKeycap(new Rect(r.x+3f*s,r.y+3f*s,25f*s,21f*s),"F"+(i+1),true,15);
                if(assigned)DrawHudKeycap(new Rect(r.xMax-27f*s,r.yMax-24f*s,24f*s,21f*s),slot.Count.ToString(),true,16);
                var hover=GUI.enabled&&r.Contains(CurrentGuiPointer);
                if(hover&&(assigned||selected?.CanAssignQuickAccess==true))DrawReferenceState(Inset(r,s),0,1f);
                if(hover&&Event.current.type==EventType.MouseDown&&Event.current.button==1)
                {if(assigned)QuickAccessClearRequested?.Invoke(i);Event.current.Use();}
                if(hover&&assigned)DrawLoadoutTooltip(r,slot.Title+"\nПКМ · Очистить ячейку");
                var enabled=GUI.enabled;GUI.enabled=enabled&&selected?.CanAssignQuickAccess==true;
                if(blankButton==null)blankButton=new GUIStyle(GUIStyle.none);
                if(GUI.Button(r,GUIContent.none,blankButton))QuickAccessAssignmentRequested?.Invoke(i,selected.Id);
                GUI.enabled=enabled;
            }
        }

        private void DrawEmptyEquipmentSlot(Rect box,int slot)
        {
            DrawBlueprintNeutralFrame(box);
            var c=box.center;var u=Mathf.Min(box.width,box.height)/90f;
            var ink=new Color(.51f,.38f,.35f,.28f);
            if(slot==9||slot==10)
            {
                var old=GUI.color;GUI.color=ink;
                var style=GetLabelStyle(40,false);style.alignment=TextAnchor.MiddleCenter;
                GUI.Label(box,LabelContent("○"),style);GUI.color=old;
                var gem=c+Vector2.up*18f*u;
                DrawGlyphLine(gem+Vector2.up*5f*u,gem+Vector2.left*5f*u,3f*u,ink);
                DrawGlyphLine(gem+Vector2.left*5f*u,gem+Vector2.down*5f*u,3f*u,ink);
                DrawGlyphLine(gem+Vector2.down*5f*u,gem+Vector2.right*5f*u,3f*u,ink);
                DrawGlyphLine(gem+Vector2.right*5f*u,gem+Vector2.up*5f*u,3f*u,ink);
            }
            else if(slot==8)
            {
                // Neck chain and suspended pendant: deliberately distinct from both ring slots.
                DrawGlyphLine(c+new Vector2(-17,-24)*u,c+new Vector2(-10,-4)*u,3f*u,ink);
                DrawGlyphLine(c+new Vector2(-10,-4)*u,c+new Vector2(0,2)*u,3f*u,ink);
                DrawGlyphLine(c+new Vector2(17,-24)*u,c+new Vector2(10,-4)*u,3f*u,ink);
                DrawGlyphLine(c+new Vector2(10,-4)*u,c+new Vector2(0,2)*u,3f*u,ink);
                var top=c+new Vector2(0,2)*u;var left=c+new Vector2(-10,14)*u;var bottom=c+new Vector2(0,27)*u;var right=c+new Vector2(10,14)*u;
                DrawGlyphLine(top,left,4f*u,ink);DrawGlyphLine(left,bottom,4f*u,ink);
                DrawGlyphLine(bottom,right,4f*u,ink);DrawGlyphLine(right,top,4f*u,ink);
            }
            else if(slot==11)
            {
                for(var i=0;i<8;i++)
                {
                    var a=i*Mathf.PI/4f;var b=(i+1)*Mathf.PI/4f;
                    DrawGlyphLine(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*24f*u,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*24f*u,2f*u,ink);
                    DrawGlyphLine(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*15f*u,c-new Vector2(Mathf.Cos(a),Mathf.Sin(a))*15f*u,u,ink);
                }
            }
            else
            {
                var type=slot==0?2:slot==1?3:slot==2?4:slot==3?5:slot==4?0:slot==7?1:slot==5?6:7;
                var old=GUI.color;GUI.color=new Color(.64f,.54f,.49f,.20f);
                DrawEquipmentIcon(EquipmentIconSafeRect(box),type,0);GUI.color=old;
            }
        }
        private void DrawEquipmentPaperdoll(Rect rect)
        {
            var hero=art.inventoryHero??art.heroPortrait;DrawIcon(rect,hero);
            var equipped=EquippedItem(equipment?.ActiveWeapon==1?7:4);
            if(hero==null||hero!=art.inventoryHero||equipped==null||equipmentArt==null)return;
            if(equipped.TypeIndex==1&&equipmentArt.staffAccessories!=null)
            {DrawPaperdollStaff(rect,hero,equipped.QualityIndex);return;}
            if(equipped.TypeIndex!=0||equipmentArt.heldEquipment==null)return;
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
        private void DrawPaperdollStaff(Rect rect,Sprite hero,int quality)
        {
            if(!equipmentArt.TryGetIcon(1,quality,out var texture,out var uv))return;
            uv=equipmentArt.UiVisibleBounds(1,quality,uv);
            var scale=Mathf.Min(rect.width/hero.rect.width,rect.height/hero.rect.height);
            var target=new Rect(rect.center-hero.rect.size*scale*.5f,hero.rect.size*scale);
            var hand=new Vector2(1054f*hero.texture.width/1254f-hero.rect.x,
                hero.rect.yMax-(hero.texture.height-955f*hero.texture.height/1254f));
            var grip=target.position+hand*scale;
            var heldScale=hero.pixelsPerUnit/equipmentArt.StaffPixelsPerUnit*scale;
            var source=equipmentArt.StaffGripPixels(quality);
            var heldRect=new Rect(grip.x-(source.x-uv.x*texture.width)*heldScale,
                grip.y-(uv.yMax*texture.height-source.y)*heldScale,uv.width*texture.width*heldScale,uv.height*texture.height*heldScale);
            var matrix=GUI.matrix;
            GUIUtility.RotateAroundPivot(-Stage3ForgeArt.StaffUprightAngle,grip);
            DrawUiTexture(heldRect,texture,uv);GUI.matrix=matrix;
        }
        private void DrawConsumableInventoryDetails(Rect r,Stage3EquipmentItemView item)
        {
            var s=UiScale;
            FitText(new Rect(r.x,r.y,r.width,52f*s),item.Name,30,false);
            DrawRule(new Rect(r.x,r.y+72f*s,r.width,s));
            var side=Mathf.Min(r.width*.75f,r.height*.38f);
            DrawEquipmentIcon(new Rect(r.center.x-side*.5f,r.y+90f*s,side,side),item);
            Label(new Rect(r.x,r.y+r.height*.52f,r.width,31f*s),"В сумке: "+item.Quantity,23,TextAnchor.MiddleCenter);
            var props=item.Properties??Array.Empty<Stage3SelectedBuildingStat>();var y=r.y+r.height*.60f;
            for(var i=0;i<props.Length;i++)
            {
                DrawConsumableProperty(new Rect(r.x,y+i*54f*s,r.width,54f*s),props[i]);
            }
            Label(new Rect(r.x,EquipmentActionRect.y,r.width,EquipmentActionRect.height+28f*s),
                "Выберите ячейку F1–F4 под сумкой,\nчтобы назначить этот предмет.",16,TextAnchor.MiddleCenter,true);
        }
        private void DrawConsumableProperty(Rect row,Stage3SelectedBuildingStat property)
        {
            // A combined restoration value needs the full detail width at 720p.
            // Reserve two lines inside this detail row; inventory cells stay fixed.
            var titleHeight=24f*UiScale;
            Label(new Rect(row.x,row.y,row.width,titleHeight),property.Label,16,TextAnchor.MiddleLeft,true);
            Label(new Rect(row.x,row.y+titleHeight,row.width,row.height-titleHeight),property.Value,18,TextAnchor.MiddleRight);
        }
        private void DrawEquipmentDetails(Rect r,Stage3EquipmentItemView item)
        {
            var s=UiScale;
            if(item==null){Label(r,"Выберите предмет",22,TextAnchor.MiddleCenter,true);return;}
            if(item.Blueprint!=null){DrawBlueprintInventoryDetails(r,item);return;}
            if(item.Consumable){DrawConsumableInventoryDetails(r,item);return;}
            FitText(new Rect(r.x,r.y,r.width,52f*s),item.Name,32,false);
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
