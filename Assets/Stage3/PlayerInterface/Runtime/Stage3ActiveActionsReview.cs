using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.WavesAI;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Not3A.Stage3.PlayerInterface
{
    // Explicit disposable development-player qualification; never attached to ordinary startup.
    public static class Stage3ActiveActionsReview
    {
        [Serializable] private sealed class Report
        {
            public bool ready;
            public string scope="Opt-in paid transactions and bound HUD commands; OS mouse clicks require separate manual qualification. Ordinary frames precede all QA grants.";
            public List<string> assertions=new List<string>();
            public List<Frame> frames=new List<Frame>();
        }
        [Serializable] private sealed class Frame
        {
            public string file,window,tooltip,forgeName;
            public int width,height,selectedSkill,healthBottles,staffRank,forgeLevel,persistentAreas;
            public bool mastery,bowAutocast,staffAutocast;
            public int[] assignments;
            public Rect assignmentF4,description,effect;
            public float detailBottom,detailAvailableBottom;
        }
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            if(!Debug.isDebugBuild&&!Application.isEditor)yield break;
            if(string.IsNullOrWhiteSpace(directory))throw new ArgumentException("Explicit active-actions QA directory required.");
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var world=owner.GetComponent<Stage3GuildWorldDriver>();
            var deadline=Time.realtimeSinceStartup+45;
            while((!game.IsInitialized||!hud.IsReady||binding.HeroSkills==null)&&Time.realtimeSinceStartup<deadline)yield return null;
            var report=new Report();
            Check(game.IsInitialized&&hud.IsReady&&binding.HeroSkills!=null,"Bound active-actions fixture");
            var skills=binding.HeroSkills.Rules;var driver=binding.HeroSkills;var gear=binding.Equipment.Rules;
            var hero=binding.HeroProgression.Rules;var stock=game.Gathering.Economy.Inventory;
            var visuals=owner.GetComponent<Stage3HeroSkillPresentation>();var camera=scene.CameraFollow.BoundCamera;
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            var path=Path.Combine(directory,"capture-status.json");
            var keep=Environment.GetEnvironmentVariable("NOT3A_ACTIVE_ACTIONS_KEEP_STATE")=="1";
            var oldRun=game.RunDriver.enabled;var oldCombat=game.HeroCombat.AutomaticTick;var oldSkills=driver.AutomaticTick;
            var oldAutocastBlocker=driver.AutoCastBlocked;
            var oldWorld=world.AutomaticTick;var oldFollow=scene.CameraFollow.enabled;var oldRate=Time.captureFramerate;
            var start=scene.PlayerMover.transform.position;var oldCamera=camera.transform.position;var oldZoom=camera.orthographicSize;
            var potionId=-1;
            try
            {
                game.RunDriver.enabled=false;game.SetPaused(false);game.HeroCombat.AutomaticTick=false;
                driver.AutomaticTick=false;world.AutomaticTick=false;Time.captureFramerate=60;
                hud.CloseWindow();Hover(Rect.zero);yield return Settle();
                Check(gear.ConsumableCount(Stage2ItemType.HealthPotion)==0&&!gear.StaffMasteryKnown&&gear.QuickAccessType(3)==null,"Ordinary has no free bottles, mastery or F4 assignment");
                foreach(var size in sizes){yield return Resize(size);yield return Shot(size,"00-ordinary-no-grants");}
                // The only grants are scoped to this explicitly requested disposable review fixture.
                Check(stock.TryCredit(new Stage2ResourceCost(100000,100000,100000,100000)),"Explicit QA resource credit");
                hero.AwardExperience(int.MaxValue);
                Check(!skills.TryUpgrade(6),"Staff learning rejected before mastery even with sufficient level/points");
                scene.PlayerMover.transform.position=travel.CityPresentation.MerchantPosition;yield return Settle();
                Check(binding.TryOpenMerchant(),"Actual local merchant opened");
                var bottle=binding.Merchant.Items.First(i=>i.Type==Stage2ItemType.HealthPotion);
                for(var i=0;i<3;i++)
                {hud.SelectMerchantItem(true,bottle.Id);yield return Settle();Check(binding.TryMerchantTrade(true,bottle.Id),"Bought health potion "+(i+1));}
                var wand=binding.Merchant.Items.First(i=>i.Type==Stage2ItemType.Wand);
                hud.SelectMerchantItem(true,wand.Id);yield return Settle();
                Check(binding.TryMerchantTrade(true,wand.Id),"Bought existing merchant staff");
                foreach(var kind in new[]{Stage2BlueprintKind.WandRecipe,Stage2BlueprintKind.ForgeII,Stage2BlueprintKind.ForgeConstruction})
                {
                    var paper=binding.Merchant.BlueprintItems.Single(p=>p.Kind==kind);
                    hud.SelectMerchantItem(true,paper.Id);yield return Settle();
                    Check(binding.TryMerchantTrade(true,paper.Id),"Bought document "+kind);
                }
                potionId=gear.Items.First(i=>i.Type==Stage2ItemType.HealthPotion).Id;
                hud.SetWindow(Stage3PlayerWindow.Inventory);SelectItem(potionId);yield return Settle();
                Check(!binding.TryEquipItem(wand.Id),"Owned staff cannot equip before mastery");
                HudEvent("QuickAccessAssignmentRequested",3,potionId);yield return Settle();
                Check(gear.QuickAccessType(3)==Stage2ItemType.HealthPotion&&gear.ConsumableCount(Stage2ItemType.HealthPotion)==3,"Bound F4 assignment preserves owned stack");
                foreach(var size in sizes){yield return Resize(size);yield return Shot(size,"01-inventory-assigned-f4");}
                HudEvent("QuickAccessClearRequested",3);yield return Settle();
                Check(gear.QuickAccessType(3)==null&&gear.ConsumableCount(Stage2ItemType.HealthPotion)==3,"Clear does not discard bottles");
                HudEvent("QuickAccessAssignmentRequested",3,potionId);hud.CloseWindow();yield return Settle();
                Check(owner.GetComponent<Stage2HeroRecoveryDriver>().TryApplyExplorationDamage(40),"Actual damage before F4 use");
                yield return KeyPress(Key.F4);yield return Settle();
                Check(gear.ConsumableCount(Stage2ItemType.HealthPotion)==2&&hero.Health>99,"Actual F4 consumes assigned bottle and heals");
                foreach(var size in sizes){yield return Resize(size);yield return Shot(size,"02-world-f4-consumed");}
                hud.SetWindow(Stage3PlayerWindow.Hero);hud.ShowHeroDevelopmentTab(0);yield return Settle();
                hud.SelectSkill(6);
                foreach(var size in sizes)
                {
                    yield return Resize(size);Hover(hud.SkillNodeRect(6));yield return Settle();
                    Check(hud.SelectedSkillId!=6&&hud.HeroLoadoutView.Nodes.Where(n=>n.Weapon==1).All(n=>n.KnowledgeLocked),"Staff nodes concealed and unselectable "+size);
                    Check(string.IsNullOrEmpty(hud.LastLoadoutTooltipText),"No staff hover title before knowledge "+size);
                    yield return Shot(size,"03-staff-locked-hover");
                }
                hud.CloseWindow();Hover(Rect.zero);scene.PlayerMover.transform.position=start;yield return Settle();
                travel.BeginWorkshopPlacement();var workshopSite=FindSite(p=>travel.EvaluateWorkshopAt(p)==WorkshopPlacementResult.Ready);
                Check(travel.TryBuildWorkshopAt(workshopSite)==WorkshopPlacementResult.Built,"Paid actual research centre construction");
                scene.PlayerMover.transform.position=travel.WorkshopInteractionPosition;yield return Settle();
                foreach(var kind in new[]{Stage2BlueprintKind.WandRecipe,Stage2BlueprintKind.ForgeII,Stage2BlueprintKind.ForgeConstruction})
                {
                    binding.OpenResearchForBlueprint((int)kind);yield return Settle();
                    Check(binding.TryStudyResearch((int)kind),"Local actual study consumes "+kind);
                    Check(binding.Blueprints.IsStudied(kind),"Knowledge registered "+kind);
                }
                Check(gear.StaffMasteryKnown,"Mastery opens after actual study");
                foreach(var size in sizes){yield return Resize(size);binding.OpenResearchForBlueprint((int)Stage2BlueprintKind.WandRecipe);yield return Settle();yield return Shot(size,"04-studied-mastery");}
                hud.CloseWindow();scene.PlayerMover.transform.position=start;yield return Settle();
                var forge=binding.Forge;forge.BeginPlacement();var forgeSite=FindSite(p=>forge.EvaluateAt(p)==Stage3ForgePlacementResult.Ready);
                Check(forge.TryBuildAt(forgeSite)==Stage3ForgePlacementResult.Built,"Paid actual forge construction");
                scene.PlayerMover.transform.position=forge.InteractionPosition;yield return Settle();Check(forge.TryOpen(),"Local forge opened");
                Invoke(binding,"SelectForgeType",(int)Stage2ItemType.Wand);
                Check(!binding.TryForgeOperation(),"Staff craft unavailable at forge I despite mastery");
                Check(forge.TryUpgradeBuilding(),"Paid studied forge II upgrade");yield return Settle();
                Check(forge.DisplayName=="Небесная кузница"&&gear.ForgeLevel==2,"Forge II actual name");
                Check(binding.TryForgeOperation(),"Actual staff craft succeeds at forge II");
                foreach(var size in sizes){yield return Resize(size);yield return Shot(size,"05-celestial-forge-crafted");}
                hud.SetWindow(Stage3PlayerWindow.Inventory);yield return Settle();Check(binding.TryEquipItem(wand.Id),"Owned merchant staff equips after mastery");
                hud.CloseWindow();scene.PlayerMover.transform.position=start;yield return Settle();
                for(var id=1;id<=10;id++)Learn(id);
                if(driver.ActiveWeapon!=0)gear.TrySwitchWeapon();yield return Settle();
                HudEvent("SkillAutocastToggleRequested",1);yield return Settle();Check(skills.AutocastEnabled(1),"Bound bow RMB command enables autocast");
                foreach(var size in sizes){yield return Resize(size);Hover(hud.SkillSlotRect(1));yield return Settle();yield return Shot(size,"06-bow-autocast");}
                gear.TrySwitchWeapon();yield return Settle();HudEvent("SkillAutocastToggleRequested",6);yield return Settle();Check(skills.AutocastEnabled(6),"Bound staff RMB command enables autocast");
                foreach(var size in sizes){yield return Resize(size);Hover(hud.SkillSlotRect(6));yield return Settle();yield return Shot(size,"07-staff-autocast");}
                hud.SetWindow(Stage3PlayerWindow.Hero);hud.ShowHeroDevelopmentTab(0);Hover(Rect.zero);
                foreach(var size in sizes)
                {
                    yield return Resize(size);
                    foreach(var id in new[]{4,5,10})
                    {
                        hud.SelectSkill(id);yield return Settle();yield return new WaitForEndOfFrame();
                        Check(hud.SkillDetailsMeasuredBottom<=hud.SkillDetailsAvailableBottom+1,"Measured detail text fits skill "+id+" at "+size);
                        yield return Shot(size,"08-detail-"+id);
                    }
                }
                hud.CloseWindow();Hover(Rect.zero);yield return Settle();scene.CameraFollow.enabled=false;
                driver.AutoCastBlocked=()=>true; // Keep each effect shot independent of enabled autocast preferences.
                // Real durable portal actors use the existing spawn-health multiplier solely in this opt-in fixture.
                var point=(Vector2)start+Vector2.up*2;
                var enemy=game.Enemies.Spawn(Stage2EnemySource.Night,1,100,1);enemy.SetAutomaticTick(false);enemy.transform.position=point;
                foreach(var size in sizes)
                {
                    yield return Resize(size);FrameWorld(point);
                    Prepare(0);Check(driver.TryCast(4,point),"Actual persistent arrow cast");yield return Settle();
                    Check(visuals.ActivePersistentAreaCount>0,"Arrow area presentation present");yield return Shot(size,"09-red-arrow-area");
                    Prepare(1);Check(driver.TryCast(9,point),"Actual ground fire cast");driver.Tick(1f);yield return Settle();
                    Check(enemy.Statuses.BurningRemaining>0&&visuals.ActivePersistentAreaCount>0,"Actual burn and fire area present");
                    Check(!enemy.Statuses.Immobilized&&enemy.Statuses.PoisonRemaining==0&&enemy.Statuses.PhysicalVulnerability==0&&enemy.Statuses.MagicVulnerability==0,"Fire-only shot contains no previous ice/poison/lightning "+size);yield return Shot(size,"10-ground-fire");
                    Prepare(0);Check(driver.TryCast(3,point),"Actual ice freeze cast");Check(driver.TryCast(5,point),"Actual poison cast");
                    if(driver.ActiveWeapon!=1)Check(gear.TrySwitchWeapon(),"Switch staff for combined status shot");
                    hero.RestoreVitals(1000,1000);Check(driver.TryCast(10,point),"Actual lightning cast");yield return Settle();
                    Check(enemy.Statuses.FrozenRemaining>0&&enemy.Statuses.PoisonRemaining>0&&enemy.Statuses.StunnedRemaining>0&&enemy.Statuses.PhysicalVulnerability>0&&enemy.Statuses.MagicVulnerability>0,"Actual portal statuses combined");
                    yield return Shot(size,"11-portal-statuses");
                }
                // Real living world actors retain authored health; independent surviving targets show each status.
                var unit=world.Enemies.Where(e=>e.Alive).OrderByDescending(e=>e.Health).First();
                scene.PlayerMover.transform.position=unit.Position+Vector2.down*3;yield return Settle();
                Prepare(0);Check(driver.TryCast(5,unit.Position),"Actual world poison cast");
                Check(unit.Alive&&unit.Statuses.PoisonRemaining>0,"Living world actor carries actual poison");
                foreach(var size in sizes){yield return Resize(size);FrameWorld(unit.Position);yield return Shot(size,"12-world-poison");}
                // A hidden modal must not keep a gameplay-expired fire area alive visually.
                visuals.ClearPresentation();visuals.PresentPersistentSkillArea(9,point,1,.1f);hud.SetWindow(Stage3PlayerWindow.Inventory);
                yield return new WaitForSeconds(.2f);
                Check(visuals.ActivePersistentAreaCount==0,"Hidden modal does not retain expired persistent area");
                // Longer areas are explicitly cleared before manual handoff, preserving runtime state ownership.
                visuals.ClearPresentation();
                report.ready=true;File.WriteAllText(path,JsonUtility.ToJson(report,true));
                Debug.Log("S3_ACTIVE_ACTIONS_REVIEW_READY: "+report.frames.Count+" native frames; actual transactions and gates asserted.");
            }
            finally
            {
                Time.captureFramerate=oldRate;Hover(Rect.zero);game.SetPaused(false);
                driver.AutoCastBlocked=oldAutocastBlocker;driver.AutomaticTick=oldSkills;world.AutomaticTick=oldWorld;game.HeroCombat.AutomaticTick=oldCombat;
                scene.CameraFollow.enabled=oldFollow;camera.transform.position=oldCamera;camera.orthographicSize=oldZoom;
                scene.PlayerMover.transform.position=start;
                if(keep&&report.ready){hud.SetWindow(Stage3PlayerWindow.Inventory);SelectItem(potionId);}
                else {hud.CloseWindow();game.RunDriver.ResetRun();game.RunDriver.enabled=oldRun;}
                File.WriteAllText(path,JsonUtility.ToJson(report,true));
            }
            if(!keep&&Environment.GetEnvironmentVariable("NOT3A_HUD_REFACTOR_QUIT")=="1")Application.Quit(0);

            void Check(bool value,string message){if(!value)throw new InvalidOperationException("Active actions QA: "+message);if(report!=null)report.assertions.Add(message);}
            void Learn(int id){var parent=skills.Catalog.Get(id).prerequisite;if(parent>=0&&skills.Rank(parent)==0)Learn(parent);if(skills.Rank(id)==0)Check(skills.TryUpgrade(id),"Paid skill rank "+id);}
            void SelectItem(int id)=>Invoke(binding,"SelectEquipmentItem",id);
            void HudEvent(string name,params object[] args)
            {var action=typeof(Stage3FantasyHud).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(hud) as Delegate;Check(action!=null,"Bound HUD event "+name);action.DynamicInvoke(args);}
            Vector2 FindSite(Func<Vector2,bool> valid)
            {for(var radius=5;radius<100;radius+=2)for(var x=-radius;x<=radius;x+=2)for(var y=-radius;y<=radius;y+=2){var p=(Vector2)start+new Vector2(x,y);if(valid(p))return p;}throw new InvalidOperationException("No paid construction site");}
            void Prepare(int weapon)
            {
                Invoke(driver,"ClearTargets");
                for(var i=0;i<game.Enemies.ActiveCount;i++)game.Enemies.GetEnemy(i).Statuses.Reset();
                foreach(var actor in world.Enemies)actor.Statuses.Reset();
                skills.Tick(60,true);hero.RestoreVitals(1000,1000);if(driver.ActiveWeapon!=weapon)gear.TrySwitchWeapon();
            }
            void FrameWorld(Vector2 centre)
            {camera.orthographicSize=5;var p=new Vector3(centre.x,centre.y,Stage3TacticalPresentation.GrassSurfaceZ(centre));camera.transform.position=p-camera.transform.forward*30;var projected=camera.WorldToScreenPoint(p);camera.transform.position+=p-camera.ScreenToWorldPoint(new Vector3(Screen.width*.54f,Screen.height*.57f,projected.z));}
            IEnumerator Shot(Vector2Int size,string state)
            {
                yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
                Check(image.width==size.x&&image.height==size.y,"Native dimensions "+state+" "+size);
                var name=size.x+"x"+size.y+"-"+state+".png";File.WriteAllBytes(Path.Combine(directory,name),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
                report.frames.Add(new Frame{file=name,width=size.x,height=size.y,window=hud.ActiveWindow.ToString(),tooltip=hud.LastLoadoutTooltipText,
                    selectedSkill=hud.SelectedSkillId,healthBottles=gear.ConsumableCount(Stage2ItemType.HealthPotion),mastery=gear.StaffMasteryKnown,
                    staffRank=skills.Rank(6),forgeLevel=gear.ForgeLevel,forgeName=binding.Forge.DisplayName,persistentAreas=visuals.ActivePersistentAreaCount,
                    bowAutocast=skills.AutocastEnabled(1),staffAutocast=skills.AutocastEnabled(6),assignments=Enumerable.Range(0,4).Select(i=>gear.QuickAccessType(i).HasValue?(int)gear.QuickAccessType(i).Value:-1).ToArray(),
                    assignmentF4=hud.InventoryQuickSlotRect(3),description=hud.SkillDescriptionTextRect,effect=hud.SkillEffectTextRect,
                    detailBottom=hud.SkillDetailsMeasuredBottom,detailAvailableBottom=hud.SkillDetailsAvailableBottom});
                File.WriteAllText(path,JsonUtility.ToJson(report,true));yield return null;
            }
        }
        private static object Invoke(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(target,args);
        private static IEnumerator Resize(Vector2Int size){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);yield return Settle();}
        private static IEnumerator Settle(){yield return null;yield return null;yield return null;}
        private static IEnumerator KeyPress(Key key)
        {InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return null;}
        private static void Hover(Rect rect)
        {if(Mouse.current==null)return;var point=rect==Rect.zero?new Vector2(1,Screen.height-1):new Vector2(rect.center.x,Screen.height-rect.center.y);Mouse.current.WarpCursorPosition(point);InputSystem.QueueDeltaStateEvent(Mouse.current.position,point);}
    }
}
