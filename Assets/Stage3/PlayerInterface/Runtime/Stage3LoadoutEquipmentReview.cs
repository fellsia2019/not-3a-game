using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in disposable native D120 loadout/forge/art review. No ordinary-start grants.</summary>
    public static class Stage3LoadoutEquipmentReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while(binding.Equipment==null||!hud.IsReady)yield return null;
            for(var i=0;i<12;i++)yield return null;
            var clockEnabled=game.RunDriver.enabled;var bowTick=game.HeroCombat.AutomaticTick;
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();var heroTick=hero.AutomaticTick;
            var world=binding.GuildWorld;var worldTick=world!=null&&world.AutomaticTick;
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;hero.AutomaticTick=false;
            if(world!=null)world.AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);owner.GetComponent<Stage2GameHud>().SetDebugOpen(false);
            var start=(Vector2)scene.PlayerMover.transform.position;
            var rules=binding.Equipment.Rules;var frames=new List<string>();
            var wallet=game.Gathering.Economy.Inventory;wallet.TryAddGold(5000);
            Stage2EquipmentItem Grant(Stage2ItemType type,Stage2ItemQuality quality=Stage2ItemQuality.F)
            {Require(rules.TryReceiveLoot(type,quality,1,out var item),"declared fixture grant");return item;}
            void Equip(Stage2EquipmentItem item)=>Require(rules.TryEquip(item.Id)==Stage2EquipmentResult.Ready,"actual equip");
            var staff=Grant(Stage2ItemType.Wand);Equip(staff);
            foreach(var type in new[]{Stage2ItemType.Helmet,Stage2ItemType.Chest,Stage2ItemType.Arms,Stage2ItemType.Legs})Equip(Grant(type));
            var amulet=Grant(Stage2ItemType.Amulet,Stage2ItemQuality.D);
            var firstRing=Grant(Stage2ItemType.Ring,Stage2ItemQuality.E);var secondRing=Grant(Stage2ItemType.Ring,Stage2ItemQuality.C);
            Equip(Grant(Stage2ItemType.Protection,Stage2ItemQuality.C));
            var qualityItems=new List<Stage2EquipmentItem>();
            foreach(var type in new[]{Stage2ItemType.Amulet,Stage2ItemType.Ring,Stage2ItemType.Protection})
                foreach(var quality in new[]{Stage2ItemQuality.F,Stage2ItemQuality.D,Stage2ItemQuality.S,Stage2ItemQuality.SSS})
                    qualityItems.Add(Grant(type,quality));
            foreach(var type in new[]{Stage2ItemType.HealthPotion,Stage2ItemType.ManaPotion,Stage2ItemType.MixedPotion})
                {var stock=binding.Merchant.Items.First(i=>i.Type==type);
                for(var i=0;i<3;i++)Require(binding.Merchant.TryTrade(true,stock.Id)==Stage2TradeResult.Ready,"declared fixture bottle purchase");}
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            foreach(var size in sizes)
            {
                yield return Resize(size);
                if(rules.ActiveWeaponType!=Stage2ItemType.Bow)Require(rules.TrySwitchWeapon(),"bow selection");
                hud.SetWindow(Stage3PlayerWindow.Inventory);Call(binding,"SelectEquipmentItem",amulet.Id);
                yield return Shot(directory,size+"-empty-pendant-rings-bow",hud,frames);
                Require(hud.EquipmentView.EquippedItemIds.Length==12,"twelve slots");
                Require(rules.TrySwitchWeapon(),"staff selection");yield return Shot(directory,size+"-empty-pendant-rings-staff",hud,frames);
            }
            Equip(amulet);Equip(firstRing);Equip(secondRing);
            var sssAmulet=qualityItems.First(i=>i.Type==Stage2ItemType.Amulet&&i.Quality==Stage2ItemQuality.SSS);
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.SetWindow(Stage3PlayerWindow.Inventory);
                Call(binding,"SelectEquipmentItem",sssAmulet.Id);yield return Shot(directory,size+"-twelve-equipped-SSS-amulet",hud,frames);
                var potion=rules.Items.First(i=>i.Type==Stage2ItemType.MixedPotion);
                Call(binding,"SelectEquipmentItem",potion.Id);yield return Shot(directory,size+"-neutral-potion-inventory",hud,frames);
            }
            hud.CloseWindow();
            foreach(MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind)))wallet.TryAdd(kind,1000);
            var forge=binding.Forge;forge.BeginPlacement();var site=Vector2.zero;var found=false;
            for(var ring=6;ring<50&&!found;ring+=2)
                for(var x=-ring;x<=ring&&!found;x+=2)for(var y=-ring;y<=ring;y+=2)
                {var candidate=start+new Vector2(x,y);if(forge.EvaluateAt(candidate)!=Stage3ForgePlacementResult.Ready)continue;site=candidate;found=true;break;}
            Require(found&&forge.TryBuildAt(site)==Stage3ForgePlacementResult.Built,"real forge placement");
            Move(scene,hero,forge.InteractionPosition);
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();Require(forge.TryOpen(),"local forge");
                Call(binding,"SelectForgeMode",Stage3ForgeMode.Create);
                foreach(var type in new[]{Stage2ItemType.Amulet,Stage2ItemType.Ring,Stage2ItemType.Protection})
                {Call(binding,"SelectForgeType",(int)type);yield return Shot(directory,size+"-forge-create-"+type,hud,frames);}
                Call(binding,"SelectForgeType",(int)Stage2ItemType.Amulet);Call(binding,"SelectForgeMode",Stage3ForgeMode.Level);
                Call(binding,"SelectForgeItem",sssAmulet.Id);yield return Shot(directory,size+"-forge-SSS-existing-level",hud,frames);
            }
            hud.CloseWindow();var travel=owner.GetComponent<Stage3WorldTravel>();Move(scene,hero,travel.CityPresentation.MerchantPosition);
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();Require(binding.TryOpenMerchant(),"local shop");
                foreach(var type in new[]{Stage2ItemType.HealthPotion,Stage2ItemType.ManaPotion,Stage2ItemType.MixedPotion})
                {
                    var potion=binding.Merchant.Items.First(i=>i.Type==type);hud.SelectMerchantItem(true,potion.Id);
                    yield return Shot(directory,size+"-merchant-neutral-"+type,hud,frames);
                }
            }
            hud.CloseWindow();Move(scene,hero,start);scene.CameraFollow.ConfigureTacticalView(3);
            if(rules.ActiveWeaponType!=Stage2ItemType.Wand)Require(rules.TrySwitchWeapon(),"world staff");
            var avatar=scene.PlayerMover.GetComponent<Stage3HeroAvatar>();
            foreach(var quality in new[]{Stage2ItemQuality.F,Stage2ItemQuality.SSS})
            {
                Equip(Grant(Stage2ItemType.Wand,quality));
                foreach(var size in sizes)
                {
                    yield return Resize(size);
                    var directions=new[]{Vector2.right,Vector2.up,Vector2.left,Vector2.down};
                    for(var i=0;i<directions.Length;i++)
                    {
                        scene.PlayerMover.transform.position=start+directions[i]*.2f;yield return null;yield return null;
                        scene.CameraFollow.SnapToTarget();yield return Shot(directory,size+"-staff-"+quality+"-direction-"+i,hud,frames);
                        Require(avatar.DisplayedEquipmentType==1&&avatar.DisplayedEquipmentQuality==(int)quality,"actual staff renderer");
                        scene.PlayerMover.transform.position=start;yield return null;
                    }
                }
            }
            File.WriteAllText(Path.Combine(directory,"frames.txt"),string.Join("\n",frames));
            File.WriteAllText(Path.Combine(directory,"qualification.txt"),
                "Actual native development-player HUD/forge/merchant/equipped actor captures at720/900/1080. Explicit disposable fixture grants gear (including normally unobtainable SSS) through TryReceiveLoot and buys bottle stacks through merchant rules; grants materials/gold, freezes clock/enemy/combat and changes legal positions/camera. No DTO injection, invented ordinary loot or OS input qualification. Production equip/switch and actual item-view bindings feed frames. Reset and automatic tick restoration follow capture.\n");
            hud.CloseWindow();game.RunDriver.ResetRun();Move(scene,hero,start);scene.CameraFollow.ConfigureTacticalView(8);
            game.RunDriver.enabled=clockEnabled;game.HeroCombat.AutomaticTick=bowTick;hero.AutomaticTick=heroTick;
            if(world!=null)world.AutomaticTick=worldTick;
            Debug.Log("S3_LOADOUT_EQUIPMENT_REVIEW_OK "+directory);
        }
        private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(owner,args);
        private static void Move(Stage2SceneBootstrap scene,Stage2HeroProgressionDriver hero,Vector2 target)
        {
            var position=target;var valid=hero.CanLand(position);
            for(var radius=.2f;radius<8&&!valid;radius+=.2f)for(var n=0;n<32;n++)
            {var a=n*Mathf.PI/16;var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;if(!hero.CanLand(p))continue;position=p;valid=true;break;}
            Require(valid&&scene.PlayerMover.TryTeleportTo(position),"legal capture position");scene.CameraFollow.SnapToTarget();
        }
        private static IEnumerator Resize(Vector2Int size){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.25f);}
        private static IEnumerator Shot(string directory,string name,Stage3FantasyHud hud,List<string> frames)
        {
            hud.GetComponent<Stage3HeroLevelUpPresentation>()?.ClearEffects();hud.GetComponent<Stage3HeroProgressionEffects>()?.ClearEffects();hud.ClearExperienceFeedback();
            for(var i=0;i<4;i++)yield return null;yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));frames.Add(name);yield return null;
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("D120 equipment QA: "+message);}
    }
}
