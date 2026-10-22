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
            heroSkills.ControlBlocked=()=>!bound||!isActiveAndEnabled||!presentation.isActiveAndEnabled||Blocked()||IsDraggingBuilding||HasBuildingDraft;
            loadoutView=new Stage3HeroLoadoutView{Nodes=new Stage3SkillNodeView[31],AdventurerNodes=new Stage3SkillNodeView[3],QuickSlots=new Stage3QuickSlotView[4]};
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
            if(id==4||id==5||id==7||id==8){heroSkills.TryCast(id,scene.PlayerMover.transform.position);armedSkill=-1;}
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
            if(heroSkills==null)return;
            heroSkills.TryUsePotion(index);MarkEquipmentDirty();RefreshHeroLoadout();
        }
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
                // F4 is deliberately empty until an actual fourth item is specified.
            }
            var digit=k==null?-1:k.digit1Key.wasPressedThisFrame?0:k.digit2Key.wasPressedThisFrame?1:k.digit3Key.wasPressedThisFrame?2:k.digit4Key.wasPressedThisFrame?3:k.digit5Key.wasPressedThisFrame?4:-1;
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
                        if(mouse.leftButton.wasPressedThisFrame)
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
                    view.EffectText=SkillPowerText(i,current,Mathf.Max(1,rank))+(rank>0&&rank<n.MaxRank?" → "+SkillPowerText(i,next,rank+1):"");
                }
            }
            for(var i=0;i<4;i++)
            {
                var view=loadoutView.QuickSlots[i];
                view.Count=i<3?equipmentDriver.Rules.ConsumableCount((Stage2ItemType)(11+i)):0;
                view.CooldownRemaining=rules.PotionCooldownRemaining;view.CooldownTotal=rules.Catalog.PotionCooldown;
                var needsHealth=heroProgression.Rules.Health<heroProgression.Rules.MaximumHealth;
                var needsMana=heroProgression.Rules.Mana<heroProgression.Rules.MaximumMana;
                var needed=i==0?needsHealth:i==1?needsMana:needsHealth||needsMana;
                view.CanUse=i<3&&needed&&view.Count>0&&heroSkills.CanControl&&rules.PotionCooldownRemaining<=0;
                view.UnavailableReason=i==3?"Резерв":view.Count==0?"Нет в сумке":rules.PotionCooldownRemaining>0?"Общий КД зелий":!needed?i==0?"HP заполнено":i==1?"MP заполнено":"HP и MP заполнены":!heroSkills.CanControl?"Сейчас недоступно":null;
            }
            presentation.SetHeroLoadout(loadoutView);
        }
        private string SkillPowerText(int id,float value,int rank)
        {
            if(id==0)return "Дальность "+heroProgression.Catalog.BlinkDisplayRange(rank).ToString("0.#",DisplayCulture);
            if(id==1||id==2||id==3||id==6||id==9||id==10)return "Урон ×"+value.ToString("0.##",DisplayCulture);
            if(id==7)return "Поглощение "+value.ToString("0.#",DisplayCulture);
            if(id==8)return "+"+value.ToString("0.#",DisplayCulture)+" HP";
            if(id==11)return "Урон +"+value.ToString("0.#",DisplayCulture);
            if(id==14)return "Урон на ходу "+Mathf.RoundToInt((.4f+.2f*rank)*100)+"%";
            if(id==18||id==20||id==21||id==22)return "+"+value.ToString("0.#",DisplayCulture)+" с";
            if(id==29)return "+"+value.ToString("0.#",DisplayCulture)+" MP";
            return "+"+(value*100).ToString("0.#",DisplayCulture)+"%";
        }
        private void UnbindHeroLoadout()
        {
            if(presentation!=null)
            {
                presentation.SkillUpgradeRequested-=UpgradeSkill;presentation.SkillArmRequested-=ArmHeroSkill;
                presentation.WeaponSwitchRequested-=SwitchHeroWeapon;presentation.QuickSlotUseRequested-=UseQuickSlot;
                presentation.WindowChanged-=CancelHeroSkill;presentation.SetHeroLoadout(null);
            }
            if(game?.RunDriver?.Run!=null){game.RunDriver.Run.Reset-=CancelHeroSkill;game.RunDriver.Run.Rewound-=CancelHeroSkill;}
            if(heroRecovery!=null)heroRecovery.Respawned-=CancelHeroSkill;
            if(heroSkills!=null)heroSkills.ControlBlocked=null;
        }
    }
}
