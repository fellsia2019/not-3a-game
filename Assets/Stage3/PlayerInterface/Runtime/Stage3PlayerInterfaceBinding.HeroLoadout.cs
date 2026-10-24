using Not3A.Stage2.Integration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage3HeroSkillsDriver heroSkills;
        private Stage3HeroLoadoutView loadoutView;
        private int armedSkill=-1;
        public Stage3HeroSkillsDriver HeroSkills=>heroSkills;
        public int ArmedSkill=>armedSkill;
        private void BindHeroLoadout()
        {
            var catalog=Resources.Load<Stage2HeroSkillCatalog>("Stage3HeroSkills");
            if(catalog==null||!catalog.TryValidate(out _)){Debug.LogError("S3_HERO_LOADOUT: missing/invalid skill catalog.");return;}
            heroSkills=GetComponent<Stage3HeroSkillsDriver>()??gameObject.AddComponent<Stage3HeroSkillsDriver>();
            heroSkills.Initialize(game,scene.PlayerMover,heroProgression,equipmentDriver,catalog);
            heroSkills.AutoCastBlocked=()=>armedSkill>=0||blinkArmed;
            heroSkills.ControlBlocked=()=>!bound||!isActiveAndEnabled||!presentation.isActiveAndEnabled||Blocked()||IsDraggingBuilding||HasBuildingDraft;
            loadoutView=new Stage3HeroLoadoutView{Nodes=new Stage3SkillNodeView[31],AdventurerNodes=new Stage3SkillNodeView[catalog.Count-31],QuickSlots=new Stage3QuickSlotView[4]};
            for(var i=0;i<catalog.Count;i++)
            {
                var n=catalog.Get(i);
                var view=new Stage3SkillNodeView{Id=i,Weapon=n.weapon,IsActive=n.active,Title=n.title,
                    Description=n.description,MaxRank=n.MaxRank,RequiredNodeId=n.prerequisite};
                if(i<31)loadoutView.Nodes[i]=view;else loadoutView.AdventurerNodes[i-31]=view;
            }
            for(var i=0;i<4;i++)loadoutView.QuickSlots[i]=new Stage3QuickSlotView{Index=i,Title=i<3?equipmentDriver.Catalog.Get((Stage2ItemType)(11+i)).displayName:"Резерв"};
            presentation.SkillUpgradeRequested+=UpgradeSkill;
            presentation.SkillArmRequested+=ArmHeroSkill;
            presentation.WeaponSwitchRequested+=SwitchHeroWeapon;
            presentation.QuickSlotUseRequested+=UseQuickSlot;
            presentation.QuickAccessAssignmentRequested+=AssignQuickAccess;
            presentation.QuickAccessClearRequested+=ClearQuickAccess;
            presentation.SkillAutocastToggleRequested+=ToggleSkillAutocast;
            presentation.WindowChanged+=CancelHeroSkill;
            game.RunDriver.Run.Reset+=CancelHeroSkill;game.RunDriver.Run.Rewound+=CancelHeroSkill;
            if(heroRecovery!=null)heroRecovery.Respawned+=CancelHeroSkill;
            legacy?.SetDevelopmentShortcut("<Keyboard>/f10");
            RefreshHeroLoadout();
        }
        private void UpgradeSkill(int id)
        {
            if(!CanSpendHeroPoints||heroSkills==null)return;
            heroSkills.Rules.TryUpgrade(id);RefreshHeroProgression();RefreshHeroLoadout();
        }
        public void CancelHeroSkill(){armedSkill=-1;heroSkills?.CancelAim();}
        private void ArmHeroSkill(int id)
        {
            if(id==0){CancelHeroSkill();ArmBlink();return;}
            if(heroSkills==null||!heroSkills.CanCast(id,out _))return;
            CancelBlink();ClearBuildingSelection();game.Gathering.CancelGatherUntilRelease();
            if(id==7||id==8){heroSkills.TryCast(id,scene.PlayerMover.transform.position);armedSkill=-1;}
            else {heroSkills.CancelAim();armedSkill=armedSkill==id?-1:id;}
            RefreshHeroLoadout();
        }
        private void SwitchHeroWeapon()
        {
            if(!CanManageEquipment||IsDraggingBuilding||HasBuildingDraft||(presentation.ActiveWindow!=Stage3PlayerWindow.None&&presentation.ActiveWindow!=Stage3PlayerWindow.Inventory))return;
            CancelBlink();CancelHeroSkill();
            equipmentDriver.Rules.TrySwitchWeapon();MarkEquipmentDirty();RefreshEquipment();RefreshHeroLoadout();
        }
        private void UseQuickSlot(int index)
        {
            if(heroSkills==null||loadoutView==null||index<0||index>=4)return;
            var type=equipmentDriver.Rules.QuickAccessType(index);if(!type.HasValue)return;
            var succeeded=heroSkills.TryUseActiveItem(type.Value);
            var slot=loadoutView.QuickSlots[index];
            slot.FeedbackSerial++;
            slot.FeedbackSucceeded=succeeded;
            slot.FeedbackStartedAt=Time.unscaledTime;
            if(succeeded)MarkEquipmentDirty();
            RefreshHeroLoadout();
        }
        public bool TryAssignQuickAccess(int slot,int itemId)
        {
            if(!CanManageEquipment||presentation.ActiveWindow!=Stage3PlayerWindow.Inventory||!equipmentDriver.Rules.TryAssignQuickAccess(slot,itemId))return false;
            ClearQuickFeedback(slot);MarkEquipmentDirty();RefreshEquipment();RefreshHeroLoadout();return true;
        }
        public bool TryClearQuickAccess(int slot)
        {
            if(!CanManageEquipment||presentation.ActiveWindow!=Stage3PlayerWindow.Inventory||!equipmentDriver.Rules.TryClearQuickAccess(slot))return false;
            ClearQuickFeedback(slot);MarkEquipmentDirty();RefreshEquipment();RefreshHeroLoadout();return true;
        }
        private void AssignQuickAccess(int slot,int itemId)=>TryAssignQuickAccess(slot,itemId);
        private void ClearQuickAccess(int slot)=>TryClearQuickAccess(slot);
        private void ToggleSkillAutocast(int id)
        {if(heroSkills!=null&&bound&&presentation.isActiveAndEnabled&&
            (heroSkills.CanControl||presentation.ActiveWindow==Stage3PlayerWindow.Hero&&CanSpendHeroPoints)&&
            heroSkills.Rules.ToggleAutocast(id))RefreshHeroLoadout();}
        private void ClearQuickFeedback(int slot)
        {if(loadoutView==null||slot<0||slot>=4)return;loadoutView.QuickSlots[slot].FeedbackSerial=0;loadoutView.QuickSlots[slot].FeedbackStartedAt=0;}
        private void UpdateHeroLoadout()
        {
            if(heroSkills==null)return;
            var k=Keyboard.current;var mouse=Mouse.current;
            if(!heroSkills.CanControl||k!=null&&k.escapeKey.wasPressedThisFrame||mouse!=null&&mouse.rightButton.wasPressedThisFrame)
                CancelHeroSkill();
            if(k!=null&&k.zKey.wasPressedThisFrame)SwitchHeroWeapon();
            if(heroSkills.CanControl&&k!=null)
            {
                if(k.f1Key.wasPressedThisFrame)UseQuickSlot(0);
                if(k.f2Key.wasPressedThisFrame)UseQuickSlot(1);
                if(k.f3Key.wasPressedThisFrame)UseQuickSlot(2);
                if(k.f4Key.wasPressedThisFrame)UseQuickSlot(3);
            }
            var digit=k==null||presentation.ActiveWindow!=Stage3PlayerWindow.None?-1:k.digit1Key.wasPressedThisFrame?0:k.digit2Key.wasPressedThisFrame?1:k.digit3Key.wasPressedThisFrame?2:k.digit4Key.wasPressedThisFrame?3:k.digit5Key.wasPressedThisFrame?4:-1;
            if(heroSkills.CanControl&&mouse!=null)
            {
                var screen=mouse.position.ReadValue();
                var visible=!presentation.IsPointerOverUi(screen);
                var ray=scene.CameraFollow.BoundCamera.ScreenPointToRay(screen);
                if(visible&&new Plane(Vector3.forward,Vector3.zero).Raycast(ray,out var distance))
                {
                    var point=(Vector2)ray.GetPoint(distance);
                    if(digit>=0)
                    {
                        CancelBlink();CancelHeroSkill();
                        heroSkills.TryCast(1+heroSkills.ActiveWeapon*5+digit,point);
                    }
                    if(armedSkill>=0)
                    {
                        heroSkills.ShowTarget(armedSkill,point);
                        if(mouse.leftButton.wasPressedThisFrame && !(k!=null&&(k.leftAltKey.isPressed||k.rightAltKey.isPressed)))
                        {
                            blinkConsumedPointer=true;bufferedBuildingPress=false;
                            if(heroSkills.TryCast(armedSkill,point)){armedSkill=-1;heroSkills.HideTarget();}
                        }
                    }
                }
                else heroSkills.HideTarget();
            }
            if(blinkArmed&&armedSkill>=0)CancelHeroSkill();
            RefreshHeroLoadout();
        }
        private void RefreshHeroLoadout()
        {
            if(heroSkills==null||loadoutView==null)return;
            var rules=heroSkills.Rules;
            loadoutView.ActiveWeapon=heroSkills.ActiveWeapon;
            guild?.SetRewardBonuses(rules.Value(31),rules.Value(32));
            for(var i=0;i<rules.Catalog.Count;i++)
            {
                var view=i<31?loadoutView.Nodes[i]:loadoutView.AdventurerNodes[i-31];var n=rules.Catalog.Get(i);
                var rank=rules.Rank(i);var rankChanged=view.Rank!=rank;view.Rank=rank;view.RequiredLevel=rules.RequiredLevel(i);
                view.KnowledgeLocked=n.weapon==1&&!rules.StaffMasteryKnown;
                view.SupportsAutocast=n.supportsAutocast&&rank>0&&!view.KnowledgeLocked;
                view.AutocastEnabled=rules.AutocastEnabled(i);
                var canUpgrade=rules.CanUpgrade(i,out var upgradeReason);
                view.CanUpgrade=CanSpendHeroPoints&&canUpgrade;
                view.UpgradeReason=!CanSpendHeroPoints?"Сейчас недоступно":upgradeReason;
                view.CanCast=i==0?heroProgression.CanCast&&heroProgression.Rules.CanBlink:heroSkills.CanCast(i,out _);
                view.CooldownRemaining=rules.Cooldown(i);view.CooldownTotal=rules.CooldownTotal(i)>0?rules.CooldownTotal(i):i==0?heroProgression.Rules.EffectiveBlinkCooldownDuration:n.Cooldown(Mathf.Max(1,rank));
                view.ManaCost=i==0?heroProgression.Rules.EffectiveBlinkManaCost:rank==0?n.Mana(1):rules.ManaCost(i);
                view.Armed=i==0?blinkArmed:armedSkill==i||i==1&&heroSkills.IsAiming;
                if(i==0)view.UnavailableReason=progressionView.BlinkUnavailableReason;
                else {heroSkills.CanCast(i,out var reason);view.UnavailableReason=reason;}
                // Stable description comes from the runtime catalog; no generated tuning is read.
                if(view.EffectText==null||rankChanged)
                {
                    var current=n.Power(Mathf.Max(1,rank));var next=n.Power(Mathf.Min(n.MaxRank,rank+1));
                    view.EffectText=SkillPowerText(i,current,Mathf.Max(1,rank))+(rank>0&&rank<n.MaxRank?" → "+SkillPowerText(i,next,rank+1):"")+SkillStatusText(i,n);
                }
            }
            for(var i=0;i<4;i++)
            {
                var view=loadoutView.QuickSlots[i];
                var type=equipmentDriver.Rules.QuickAccessType(i);
                view.ItemTypeIndex=type.HasValue?(int)type.Value:-1;
                view.Title=type.HasValue?equipmentDriver.Catalog.Get(type.Value).displayName:"Пусто";
                view.Count=type.HasValue?equipmentDriver.Rules.ActiveItemCount(type.Value):0;
                view.CooldownRemaining=type.HasValue?rules.PotionCooldownRemaining:0;view.CooldownTotal=type.HasValue?rules.Catalog.PotionCooldown:0;
                var action=type.HasValue?equipmentDriver.Catalog.ActiveAction(type.Value):Stage2ItemActiveAction.None;
                var needsHealth=heroProgression.Rules.Health<heroProgression.Rules.MaximumHealth;
                var needsMana=heroProgression.Rules.Mana<heroProgression.Rules.MaximumMana;
                var needed=action==Stage2ItemActiveAction.RestoreHealth?needsHealth:action==Stage2ItemActiveAction.RestoreMana?needsMana:needsHealth||needsMana;
                view.CanUse=type.HasValue&&needed&&view.Count>0&&heroSkills.CanControl&&rules.PotionCooldownRemaining<=0;
                view.UnavailableReason=!type.HasValue?null:view.Count==0?"Нет в сумке":rules.PotionCooldownRemaining>0?"Общий КД зелий":!needed?"Восстановление не требуется":!heroSkills.CanControl?"Сейчас недоступно":null;
            }
            presentation.SetHeroLoadout(loadoutView);
        }
        private static string SkillStatusText(int id,Stage2HeroSkillDefinition node)
        {
            string N(float value)=>value.ToString("0.##",DisplayCulture);
            var area="\nРадиус "+N(node.radius)+" · до "+node.maxTargets+" целей";
            if(id==1)return "\nПрицеливание "+N(node.aimTime)+" с · ПКМ: автокаст";
            if(id==2)return area+"\nГорение "+N(node.duration)+" с · раз в 1 с";
            if(id==3)return area+"\nЗаморозка "+N(node.controlDuration)+" с · замедление "+N((1-node.slowMultiplier)*100)+"% на "+N(node.duration)+" с";
            if(id==4)return area+"\nСтрела в области "+N(node.duration)+" с · удар раз в 1 с";
            if(id==5)return area+"\nЯд "+N(node.duration)+" с · замедление "+N((1-node.slowMultiplier)*100)+"% · физ. урон +"+N(node.vulnerability*100)+"%";
            if(id==6)return area+"\nПКМ: автокаст";
            if(id==9)return area+"\nОгонь на земле "+N(node.duration)+" с · раз в 1 с";
            if(id==10)return area+"\nОглушение "+N(node.controlDuration)+" с · маг. урон +"+N(node.vulnerability*100)+"% на "+N(node.duration)+" с";
            if(id==7)return "\nБарьер до "+N(node.duration)+" с";
            return "";
        }
        private string SkillPowerText(int id,float value,int rank)
        {
            if(id==0)return "Дальность "+heroProgression.Catalog.BlinkDisplayRange(rank).ToString("0.#",DisplayCulture);
            if(id==1||id==2||id==3||id==4||id==5||id==6||id==9||id==10)return "Урон ×"+value.ToString("0.##",DisplayCulture);
            if(id==7)return "Поглощение "+value.ToString("0.#",DisplayCulture);
            if(id==8)return "+"+value.ToString("0.#",DisplayCulture)+" HP";
            if(id==11)return "Урон +"+value.ToString("0.#",DisplayCulture);
            if(id==14)return "Урон на ходу "+Mathf.RoundToInt((.4f+.2f*rank)*100)+"%";
            if(id==18||id==20||id==21||id==22)return "+"+value.ToString("0.#",DisplayCulture)+" с";
            if(id==29)return "+"+value.ToString("0.#",DisplayCulture)+" MP";
            if(id==34)return "+"+(value*100).ToString("0.#",DisplayCulture)+"% урона мировым врагам";
            if(id==35)return "−"+(value*100).ToString("0.#",DisplayCulture)+"% урона от мировых врагов";
            return "+"+(value*100).ToString("0.#",DisplayCulture)+"%";
        }
        private void UnbindHeroLoadout()
        {
            if(presentation!=null)
            {
                presentation.SkillUpgradeRequested-=UpgradeSkill;presentation.SkillArmRequested-=ArmHeroSkill;
                presentation.WeaponSwitchRequested-=SwitchHeroWeapon;presentation.QuickSlotUseRequested-=UseQuickSlot;
                presentation.QuickAccessAssignmentRequested-=AssignQuickAccess;presentation.QuickAccessClearRequested-=ClearQuickAccess;
                presentation.SkillAutocastToggleRequested-=ToggleSkillAutocast;
                presentation.WindowChanged-=CancelHeroSkill;presentation.SetHeroLoadout(null);
            }
            if(game?.RunDriver?.Run!=null){game.RunDriver.Run.Reset-=CancelHeroSkill;game.RunDriver.Run.Rewound-=CancelHeroSkill;}
            if(heroRecovery!=null)heroRecovery.Respawned-=CancelHeroSkill;
            if(heroSkills!=null){heroSkills.ControlBlocked=null;heroSkills.AutoCastBlocked=null;}
        }
    }
}
