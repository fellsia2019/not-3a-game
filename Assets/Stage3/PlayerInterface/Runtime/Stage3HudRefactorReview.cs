using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit disposable development-player review. Never invoked by ordinary startup.</summary>
    public static class Stage3HudRefactorReview
    {
        [Serializable] private sealed class FrameFact
        {
            public string file, state;
            public int width,height,heroLevel,heroPoints,buildGroup,buildPage,buildPages,visibleCards;
            public int[] combatRanks,adventurerRanks,professionRanks,capstoneRanks;
            public int movingAttackRequiredLevel,movingAttackPrerequisite,resistanceRequiredLevel;
            public int upgradeEffects;
            public float upgradeElapsed;
            public Rect dock,service,construction,skill14,skill5,adventurer35;
            public Rect[] buildCards,quickSlots,navigation,equipmentSlots;
            public Rect tooltip,tooltipText;
            public string tooltipContent,selectedSubtitle,debugProgression;
            public int activeWeapon,weaponFront;
            public float weaponAnimation;
            public Rect[] weaponDiscs;
            public Rect weaponFooter;
        }
        [Serializable] private sealed class Report
        {
            public bool ready;
            public string scope="Disposable opt-in QA; ordinary HUD captured before grants. Actual catalog, transactions and native pixels; no synthetic future buildings.";
            public string error;
            public List<FrameFact> frames=new List<FrameFact>();
        }
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            if(!Debug.isDebugBuild&&!Application.isEditor)yield break;
            if(string.IsNullOrWhiteSpace(directory))throw new ArgumentException("Explicit QA capture directory required.");
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            var travel=owner.GetComponent<Stage3WorldTravel>();
            var deadline=Time.realtimeSinceStartup+45;
            while((game==null||!game.IsInitialized||hud==null||!hud.IsReady||binding.HeroSkills==null)&&Time.realtimeSinceStartup<deadline)yield return null;
            Require(game!=null&&game.IsInitialized&&hud!=null&&hud.IsReady&&binding.HeroSkills!=null,"Bound real Stage3 fixture required");
            if(Environment.GetEnvironmentVariable("NOT3A_ACTIVE_ACTIONS_REVIEW_ONLY")=="1")
            {yield return Stage3ActiveActionsReview.Capture(owner,directory);yield break;}
            if(Environment.GetEnvironmentVariable("NOT3A_HUD_COMPACT_REVIEW_ONLY")=="1")
            {yield return CaptureCompact(owner,directory);yield break;}
            if(Environment.GetEnvironmentVariable("NOT3A_HUD_POLISH_REVIEW_ONLY")=="1")
            {yield return CapturePolish(owner,directory);yield break;}
            Require(hud.Art.playerNavigationIcon!=null&&hud.Art.towerLevelPlate!=null&&hud.Art.controlPanel!=null&&
                hud.Art.controlAction!=null&&hud.Art.controlSlot!=null&&hud.Art.controlMedallion!=null&&hud.Art.upgradeLightCurtain!=null&&
                hud.Art.adventurerIcons?.Length==5&&hud.Art.adventurerIcons.All(icon=>icon!=null),"Approved original D124 assets wired");
            var weaponOnly=Environment.GetEnvironmentVariable("NOT3A_HUD_WEAPON_REVIEW_ONLY")=="1";
            var report=new Report();var reportPath=Path.Combine(directory,"capture-status.json");
            if(weaponOnly)report.scope="Disposable opt-in weapon-only review: actual empty/equipped/active/turn states at3native sizes; ordinary startup has no grants.";
            var camera=scene.CameraFollow.BoundCamera;
            var follow=scene.CameraFollow.enabled;var driver=game.RunDriver.enabled;var combat=game.HeroCombat.AutomaticTick;
            var playerStart=scene.PlayerMover.transform.position;var cameraStart=camera.transform.position;var zoom=camera.orthographicSize;
            var pointerStart=Mouse.current!=null?Mouse.current.position.ReadValue():Vector2.zero;
            var captureRate=Time.captureFramerate;var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            var stock=game.Gathering.Economy.Inventory;var defense=game.Building.Defense;
            var hero=binding.HeroProgression.Rules;var skills=binding.HeroSkills.Rules;var gear=binding.Equipment.Rules;
            Stage2Tower tower=null,barricade=null;
            File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
            try
            {
                // Freeze only the QA clock. Disabling the driver sets pause, so explicitly release it.
                game.RunDriver.enabled=false;game.SetPaused(false);game.HeroCombat.AutomaticTick=false;
                Time.captureFramerate=60;
                foreach(var size in sizes)
                {
                    yield return Resize(size);hud.CloseWindow();yield return Settle();
                    yield return Shot(size,"00-ordinary-hud");
                    if(weaponOnly)continue;
                    hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(0);yield return Settle();
                    Require(hud.VisibleBuildCardCount==2,"Unknown Longshot is absent before study");
                    yield return Shot(size,"01-unknown-hidden");
                }
                var debugTools=owner.GetComponent<Stage2GameHud>();
                foreach(var size in weaponOnly?Array.Empty<Vector2Int>():sizes)
                {
                    yield return Resize(size);hud.CloseWindow();debugTools.SetDebugOpen(true);
                    typeof(Stage2GameHud).GetField("scroll",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(debugTools,new Vector2(0,10000));
                    Require(debugTools.GrantHeroExperience(100),"Explicit debug XP grant");yield return Settle();yield return Shot(size,"16-debug-xp");
                    Require(hero.Level==1&&hero.XpWithin==100,"Actual debug XP, no phantom level");
                    Require(debugTools.GrantHeroLevels(1),"Explicit debug single level");yield return Settle();yield return Shot(size,"17-debug-level");
                    Require(hero.Level==2&&hero.XpWithin==0&&hero.AvailablePoints==1,"Actual level boundary and shared point");
                    Require(debugTools.GrantHeroLevels(10),"Explicit debug ten levels");yield return Settle();yield return Shot(size,"18-debug-ten-levels");
                    Require(hero.Level==12&&hero.AvailablePoints==11,"Actual multi-level grant and shared points");
                    debugTools.SetDebugOpen(false);game.RunDriver.ResetRun();game.SetPaused(false);yield return Settle();
                }
                hud.CloseWindow();Fund();
                while(hero.Level<80)hero.AwardExperience(hero.RequiredXp-hero.XpWithin);
                Require(hero.Level==80,"QA grants level80, not level100");
                for(var id=1;id<=10;id++)Learn(id,1);
                hud.SetWindow(Stage3PlayerWindow.Inventory);
                var equippedTypes=new[]{Stage2ItemType.Helmet,Stage2ItemType.Chest,Stage2ItemType.Arms,Stage2ItemType.Legs,
                    Stage2ItemType.Bow,Stage2ItemType.Pickaxe,Stage2ItemType.Axe,Stage2ItemType.Wand,Stage2ItemType.Amulet,
                    Stage2ItemType.Ring,Stage2ItemType.Ring,Stage2ItemType.Protection};
                for(var i=0;i<equippedTypes.Length;i++)
                {
                    Require(gear.TryReceiveLoot(equippedTypes[i],(Stage2ItemQuality)(i%8),1+i*3,out var item),"QA equipment grant");
                    Require(binding.TryEquipItem(item.Id),"Real equip transaction");
                }
                Require(Enumerable.Range(0,12).All(i=>gear.EquippedAt((Stage2ItemSlot)i)!=null),"Twelve occupied real equipment slots");
                for(var i=0;i<30;i++)Require(gear.TryReceiveLoot((Stage2ItemType)(i%11),(Stage2ItemQuality)(i%8),1+i,out _),"QA bag grant");
                // Local merchant purchase, then local workshop study: buying never unlocks construction.
                hud.CloseWindow();scene.PlayerMover.transform.position=travel.CityPresentation.MerchantPosition;
                yield return Settle();Require(binding.TryOpenMerchant(),"Local real merchant endpoint");yield return Settle();
                var documents=new[]{Stage2BlueprintKind.Longshot,Stage2BlueprintKind.Sawmill,Stage2BlueprintKind.Quarry,Stage2BlueprintKind.IronMine,Stage2BlueprintKind.ForgeConstruction};
                foreach(var kind in documents)
                {
                    var paper=binding.Merchant.BlueprintItems.Single(p=>p.Kind==kind);
                    hud.SelectMerchantItem(true,paper.Id);yield return Settle();
                    Require(binding.TryMerchantTrade(true,paper.Id),"Paid blueprint purchase "+kind);
                }
                hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(1);
                Require(hud.VisibleBuildCardCount==0,"Purchased production documents do not unlock build cards");
                hud.CloseWindow();scene.PlayerMover.transform.position=playerStart;
                travel.BeginWorkshopPlacement();Vector2? site=null;
                for(var ring=5;ring<70&&!site.HasValue;ring+=2)
                for(var x=-ring;x<=ring&&!site.HasValue;x+=2)
                for(var y=-ring;y<=ring;y+=2)
                {
                    var point=(Vector2)playerStart+new Vector2(x,y);
                    if(travel.EvaluateWorkshopAt(point).ToString()=="Ready"){site=point;break;}
                }
                Require(site.HasValue,"Legal workshop site");
                Require(travel.TryBuildWorkshopAt(site.Value).ToString()=="Built","Real workshop construction");
                scene.PlayerMover.transform.position=travel.WorkshopInteractionPosition;yield return Settle();
                Require(travel.TryStudyTeleport(),"Actual pinned teleport research");
                foreach(var kind in documents)
                {
                    binding.OpenResearchForBlueprint((int)kind);yield return Settle();
                    Require(binding.TryStudyResearch((int)kind),"Local consumed study "+kind);
                }
                hud.SetWindow(Stage3PlayerWindow.Hero);hud.ShowHeroDevelopmentTab(1);yield return Settle();
                for(var p=0;p<2;p++)
                {
                    for(var talent=0;talent<3;talent++)
                        for(var rank=0;rank<(talent==2?3:5);rank++)Require(binding.TryUpgradeProfession(p,talent),"Paid profession rank");
                    Require(binding.TryUpgradeProfessionCapstone(p),"Paid real profession capstone");
                }
                foreach(var id in new[]{31,32,33,34,35})Learn(id,5);
                hud.CloseWindow();scene.PlayerMover.transform.position=playerStart;scene.CameraFollow.enabled=false;
                tower=BuildTower();
                var barricadeRole=Enumerable.Range(0,defense.RoleCount).First(i=>!defense.GetCatalog(i).CanAttack);
                barricade=BuildTower(barricadeRole,8);yield return Settle();
                // Long level-up overlay is explicitly allowed to finish before visual layout review.
                yield return new WaitForSecondsRealtime(3f);
                foreach(var size in sizes)
                {
                    yield return Resize(size);FrameTower();Fund();hud.CloseWindow();yield return Settle();
                    if(!weaponOnly)yield return Shot(size,"02-equipped-player-hud");
                    if(hud.HeroLoadoutView.ActiveWeapon!=0){Require(gear.TrySwitchWeapon(),"Actual bow activation");yield return Settle();}
                    while(hud.WeaponSwitchAnimationProgress<1)yield return null;
                    yield return Shot(size,"23-bow-front");
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Z));yield return null;
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return Settle();
                    Require(hud.HeroLoadoutView.ActiveWeapon==1,"Actual Z activates equipped staff");yield return Shot(size,"24-staff-turn");
                    if(weaponOnly)
                    {
                        while(hud.WeaponSwitchAnimationProgress<.5f)yield return null;
                        yield return Shot(size,"27-staff-turn-peak");
                    }
                    while(hud.WeaponSwitchAnimationProgress<1)yield return null;
                    yield return Shot(size,"25-staff-front");
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Z));yield return null;
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return Settle();
                    Require(hud.HeroLoadoutView.ActiveWeapon==0,"Actual Z returns to equipped bow");yield return Shot(size,"26-bow-turn");
                    while(hud.WeaponSwitchAnimationProgress<1)yield return null;
                    if(weaponOnly)continue;
                    hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(1);yield return Settle();
                    Require(hud.VisibleBuildCardCount==3,"Three studied production cards");yield return Shot(size,"02-production-flow-prices");
                    hud.SetBuildGroup(2);hud.SetBuildPage(0);yield return Settle();
                    Require(hud.BuildCardCount==4&&hud.BuildPageCount==2,"Four actual base choices on two pages");yield return Shot(size,"03-base-page-one");
                    hud.SetBuildPage(1);yield return Settle();yield return Shot(size,"04-base-page-two");
                    hud.SetBuildGroup(1);stock.Reset();yield return Settle();yield return Shot(size,"05-known-unfunded");Fund();
                    hud.SetWindow(Stage3PlayerWindow.Hero);hud.ShowHeroDevelopmentTab(0);hud.SelectSkill(14);yield return Settle();yield return Shot(size,"06-combat-moving-attack");
                    hud.ShowHeroDevelopmentTab(1);hud.SelectProfession(0);hud.SelectProfessionTalent(0);yield return Settle();yield return Shot(size,"07-gathering-max-ranks");
                    hud.SelectProfessionCapstone(0);yield return Settle();yield return Shot(size,"07-miner-capstone-detail");
                    Hover(hud.ProfessionCapstoneRect(0));yield return Settle();
                    yield return Shot(size,"19-miner-capstone-hover");Require(!string.IsNullOrEmpty(hud.LastLoadoutTooltipText),"Real native mining tooltip hover");Hover(Rect.zero);
                    hud.SelectProfessionCapstone(1);yield return Settle();yield return Shot(size,"07-logger-capstone-detail");
                    Hover(hud.ProfessionCapstoneRect(1));yield return Settle();
                    yield return Shot(size,"20-logger-capstone-hover");Require(!string.IsNullOrEmpty(hud.LastLoadoutTooltipText),"Real native logging tooltip hover");Hover(Rect.zero);
                    hud.ShowHeroDevelopmentTab(2);hud.SelectSkill(35);yield return Settle();yield return Shot(size,"08-adventurer-five");
                    hud.SetWindow(Stage3PlayerWindow.Inventory);yield return Settle();
                    var first=gear.Items.First(i=>!gear.IsEquipped(i));
                    typeof(Stage3PlayerInterfaceBinding).GetMethod("SelectEquipmentItem",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(binding,new object[]{first.Id});
                    yield return Settle();yield return Shot(size,"09-inventory-selected");
                    Hover(hud.EquipmentItemRect(1));yield return Settle();yield return Shot(size,"09-inventory-hover");Hover(Rect.zero);
                    hud.CloseWindow();scene.PlayerMover.transform.position=travel.CityPresentation.MerchantPosition;yield return Settle();
                    Require(binding.TryOpenMerchant(),"Local merchant reopens");hud.SelectMerchantItem(true,binding.Merchant.Items[0].Id);yield return Settle();yield return Shot(size,"10-merchant-dual-grids");
                    Hover(hud.MerchantItemRect(true,1));yield return Settle();yield return Shot(size,"10-merchant-hover");Hover(Rect.zero);
                    hud.CloseWindow();FrameTower(barricade);yield return Settle();
                    Require(binding.TrySelectBuildingAtScreen(camera.WorldToScreenPoint(barricade.GetComponentInChildren<Stage3TowerSprite>().Body.bounds.center)),"Select real nonupgradable barricade");
                    yield return Settle();Require(hud.SelectedBuildingView.Subtitle=="Преграда"&&!hud.SelectedBuildingView.ShowUpgrade,"Barricade has no artificial level or upgrade");
                    yield return Shot(size,"21-barricade-no-level");
                    binding.ClearBuildingSelection();
                    var workshop=travel.WorkshopVisual;scene.PlayerMover.transform.position=travel.WorkshopInteractionPosition;
                    camera.transform.position=workshop.bounds.center-camera.transform.forward*30;
                    var projectedWorkshop=camera.WorldToScreenPoint(workshop.bounds.center);
                    camera.transform.position+=workshop.bounds.center-camera.ScreenToWorldPoint(new Vector3(Screen.width*.55f,Screen.height*.64f,projectedWorkshop.z));
                    yield return Settle();var workshopPoint=OpaquePoint(workshop);
                    Require(binding.TrySelectBuildingAtScreen(workshopPoint),"Select real single-level workshop point="+workshopPoint+" ui="+game.ContainsScreenPoint(workshopPoint)+" window="+hud.ActiveWindow+" paused="+game.RunDriver.Run.IsPaused);
                    yield return Settle();Require(hud.SelectedBuildingView.Subtitle=="Изучение чертежей"&&!hud.SelectedBuildingView.ShowUpgrade,"Workshop has no artificial level or upgrade");
                    yield return Shot(size,"22-workshop-no-level");
                    hud.CloseWindow();scene.PlayerMover.transform.position=playerStart;FrameTower();binding.ClearBuildingSelection();Fund();yield return Settle();
                    Require(Keyboard.current!=null,"Automation keyboard present");
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.LeftAlt));yield return Settle();
                    Require(binding.EvaluateQuickUpgrade(tower)==Stage2TowerActionResult.Ready,"Actual Alt ready evaluation");yield return Shot(size,"11-alt-ready");
                    stock.Reset();yield return Settle();Require(binding.EvaluateQuickUpgrade(tower)!=Stage2TowerActionResult.Ready,"Actual Alt insufficient resources");yield return Shot(size,"12-alt-blocked");
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return Settle();Fund();
                    Require(binding.TrySelectBuildingAtScreen(TowerPoint()),"Select real tower");hud.ClearBuildingUpgradeEffects();
                    Require(binding.TryUpgradeSelectedBuilding()==Stage2TowerActionResult.Ready,"Paid upgrade emits feedback");
                    Require(hud.BuildingUpgradeEffectCount==1,"Exactly one paid effect");
                    foreach(var time in new[]{.05f,.3f,.9f})
                    {
                        while(hud.BuildingUpgradeEffectCount>0&&hud.BuildingUpgradeEffectElapsed<time)yield return null;
                        game.SetPaused(true);yield return Shot(size,"13-upgrade-"+time.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture));game.SetPaused(false);
                    }
                    hud.ClearBuildingUpgradeEffects();binding.ClearBuildingSelection();
                    var badge=BadgeRect(tower);var dial=hud.CycleDialRect.center;var depth=camera.WorldToScreenPoint(tower.transform.position).z;
                    camera.transform.position+=camera.ScreenToWorldPoint(new Vector3(badge.center.x,Screen.height-badge.center.y,depth))-
                        camera.ScreenToWorldPoint(new Vector3(dial.x,Screen.height-dial.y,depth));
                    Require(hud.CycleDialRect.Contains(BadgeRect(tower).center),"Intentional clock and LVL overlap");yield return Shot(size,"14-clock-over-level");
                }
                // The actual clock's debug command advances to a real night without
                // crediting skipped income; the disabled QA driver spawns no stream.
                if(!weaponOnly)Require(game.RunDriver.Run.StartNextWaveForTesting(),"Real night for upgrade readability");
                foreach(var size in weaponOnly?Array.Empty<Vector2Int>():sizes)
                {
                    yield return Resize(size);hud.CloseWindow();FrameTower();Fund();yield return Settle();
                    Require(binding.TrySelectBuildingAtScreen(TowerPoint()),"Night tower selected");
                    hud.ClearBuildingUpgradeEffects();
                    Require(binding.TryUpgradeSelectedBuilding()==Stage2TowerActionResult.Ready,"Paid night upgrade");
                    binding.ClearBuildingSelection();
                    while(hud.BuildingUpgradeEffectCount>0&&hud.BuildingUpgradeEffectElapsed<.3f)yield return null;
                    game.SetPaused(true);yield return Shot(size,"15-night-upgrade-light");game.SetPaused(false);
                }
                report.ready=true;
                File.WriteAllLines(Path.Combine(directory,"frames.txt"),report.frames.Select(f=>f.file));
                Debug.Log("S3_HUD_REFACTOR_NATIVE_READY: "+report.frames.Count+" frames at "+directory);
            }
            finally
            {
                if(Keyboard.current!=null)InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
                if(Mouse.current!=null)Mouse.current.WarpCursorPosition(pointerStart);
                hud.ClearBuildingUpgradeEffects();game.SetPaused(false);game.RunDriver.ResetRun();hud.CloseWindow();
                scene.PlayerMover.transform.position=playerStart;scene.CameraFollow.enabled=follow;camera.transform.position=cameraStart;camera.orthographicSize=zoom;
                game.HeroCombat.AutomaticTick=combat;game.RunDriver.enabled=driver;Time.captureFramerate=captureRate;
                if(!report.ready)report.error="Capture interrupted or assertion failed; inspect player log. This is not a visual PASS.";
                File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
            }
            if(report.ready&&Environment.GetEnvironmentVariable("NOT3A_HUD_REFACTOR_QUIT")=="1")Application.Quit();

            void Hover(Rect rect)
            {
                if(Mouse.current==null)return;
                var point=rect==Rect.zero?new Vector2(1,Screen.height-1):new Vector2(rect.center.x,Screen.height-rect.center.y);
                // Native IMGUI hover reads OS mouse events in local scroll coordinates.
                // Queuing an Input System position alone does not produce those events.
                Mouse.current.WarpCursorPosition(point);
                InputSystem.QueueDeltaStateEvent(Mouse.current.position,point);
            }
            void Fund()=>Require(stock.TryCredit(new Stage2ResourceCost(100000,100000,100000,100000)),"QA resource credit");
            void Learn(int id,int target)
            {
                var prerequisite=skills.Catalog.Get(id).prerequisite;
                if(prerequisite>=0&&skills.Rank(prerequisite)==0)Learn(prerequisite,1);
                while(skills.Rank(id)<target)Require(skills.TryUpgrade(id),"Paid real skill rank "+id);
            }
            Stage2Tower BuildTower(int role=0,int firstRadius=1)
            {
                defense.SelectRole(role);Require(defense.BeginPlacement(),"Tower placement armed");
                for(var radius=firstRadius;radius<30;radius++)for(var y=-radius;y<=radius;y++)for(var x=-radius;x<=radius;x++)
                {
                    var cell=scene.MapDefinition.Throne+new Vector2Int(x,y);if(defense.Evaluate(cell)!=PlacementResult.Ready)continue;
                    defense.SetPreviewCell(cell);Require(defense.TryConfirm(out var result)==PlacementResult.Ready,"Paid tower placement");return result;
                }
                throw new InvalidOperationException("No valid QA tower site.");
            }
            Vector2 TowerPoint()=>camera.WorldToScreenPoint(tower.GetComponentInChildren<Stage3TowerSprite>().Body.bounds.center);
            Vector2 OpaquePoint(SpriteRenderer visual)
            {
                var sprite=visual.sprite;var rect=sprite.textureRect;
                if(sprite.texture.isReadable)for(var y=5;y<=15;y++)for(var x=5;x<=15;x++)
                {
                    var u=x/20f;var v=y/20f;
                    if(sprite.texture.GetPixel(Mathf.Clamp((int)(rect.x+u*rect.width),0,sprite.texture.width-1),
                        Mathf.Clamp((int)(rect.y+v*rect.height),0,sprite.texture.height-1)).a<.8f)continue;
                    var local=new Vector3(Mathf.Lerp(sprite.bounds.min.x,sprite.bounds.max.x,u),Mathf.Lerp(sprite.bounds.min.y,sprite.bounds.max.y,v),0);
                    if(visual.flipX)local.x=-local.x;if(visual.flipY)local.y=-local.y;
                    return camera.WorldToScreenPoint(visual.transform.TransformPoint(local));
                }
                return camera.WorldToScreenPoint(visual.bounds.center);
            }
            void FrameTower(Stage2Tower target=null)
            {
                var point=(target??tower).GetComponentInChildren<Stage3TowerSprite>().Body.bounds.center;
                camera.orthographicSize=5;camera.transform.position=point-camera.transform.forward*30;
                var projected=camera.WorldToScreenPoint(point);
                camera.transform.position+=point-camera.ScreenToWorldPoint(new Vector3(Screen.width*.53f,Screen.height*.55f,projected.z));
            }
            Rect BadgeRect(Stage2Tower target)
            {
                var left=float.PositiveInfinity;var right=float.NegativeInfinity;var top=float.PositiveInfinity;
                foreach(var renderer in target.GetComponentInChildren<Stage3TowerSprite>().GetComponentsInChildren<SpriteRenderer>())
                {
                    if(!renderer.enabled||renderer.sprite==null||renderer.color.a<=.01f)continue;
                    var b=renderer.sprite.bounds;
                    for(var c=0;c<4;c++)
                    {
                        var p=new Vector3((c&1)==0?b.min.x:b.max.x,(c&2)==0?b.min.y:b.max.y,0);
                        if(renderer.flipX)p.x=-p.x;if(renderer.flipY)p.y=-p.y;
                        var screen=camera.WorldToScreenPoint(renderer.transform.TransformPoint(p));
                        left=Mathf.Min(left,screen.x);right=Mathf.Max(right,screen.x);top=Mathf.Min(top,Screen.height-screen.y);
                    }
                }
                return new Rect((left+right)*.5f-32*hud.UiScale,top-26*hud.UiScale,64*hud.UiScale,22*hud.UiScale);
            }
            IEnumerator Shot(Vector2Int size,string state)
            {
                yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
                Require(image!=null&&image.width==size.x&&image.height==size.y,"Exact native output dimensions");
                var file=size.x+"x"+size.y+"-"+state+".png";File.WriteAllBytes(Path.Combine(directory,file),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
                report.frames.Add(new FrameFact{file=file,state=state,width=size.x,height=size.y,heroLevel=hero.Level,heroPoints=hero.AvailablePoints,
                    buildGroup=hud.BuildGroup,buildPage=hud.BuildPage,buildPages=hud.BuildPageCount,visibleCards=hud.VisibleBuildCardCount,
                    combatRanks=Enumerable.Range(0,31).Select(skills.Rank).ToArray(),adventurerRanks=Enumerable.Range(31,5).Select(skills.Rank).ToArray(),
                    professionRanks=Enumerable.Range(0,6).Select(i=>binding.Professions.Rules.Rank((Stage2Profession)(i/3),(Stage2ProfessionTalent)(i%3))).ToArray(),
                    capstoneRanks=Enumerable.Range(0,2).Select(i=>binding.Professions.Rules.HasCapstone((Stage2Profession)i)?1:0).ToArray(),
                    movingAttackRequiredLevel=skills.Catalog.Get(14).requiredLevel,movingAttackPrerequisite=skills.Catalog.Get(14).prerequisite,resistanceRequiredLevel=skills.Catalog.Get(35).requiredLevel,
                    upgradeEffects=hud.BuildingUpgradeEffectCount,upgradeElapsed=hud.BuildingUpgradeEffectElapsed,dock=hud.BottomDockRect,service=hud.NavigationRibbonRect,construction=hud.ConstructionContentRect,
                    skill14=hud.SkillNodeRect(14),skill5=hud.SkillNodeRect(5),adventurer35=hud.SkillNodeRect(35),
                    buildCards=Enumerable.Range(0,3).Select(hud.BuildChoiceRect).ToArray(),quickSlots=Enumerable.Range(0,4).Select(hud.QuickSlotRect).ToArray(),
                    navigation=new[]{Stage3PlayerWindow.None,Stage3PlayerWindow.Inventory,Stage3PlayerWindow.Construction,Stage3PlayerWindow.Research,Stage3PlayerWindow.Hero,Stage3PlayerWindow.Map}.Select(hud.HudMenuButtonRect).ToArray(),
                    equipmentSlots=Enumerable.Range(0,12).Select(hud.EquipmentSlotRect).ToArray()});
                var fact=report.frames[report.frames.Count-1];
                fact.tooltip=hud.LastLoadoutTooltipRect;fact.tooltipText=hud.LastLoadoutTooltipTextRect;fact.tooltipContent=hud.LastLoadoutTooltipText;
                fact.selectedSubtitle=hud.SelectedBuildingView?.Subtitle;fact.debugProgression=owner.GetComponent<Stage2GameHud>().HeroProgressionDebugStatus;
                fact.activeWeapon=hud.HeroLoadoutView.ActiveWeapon;fact.weaponFront=hud.WeaponFrontIndex;fact.weaponAnimation=hud.WeaponSwitchAnimationProgress;
                fact.weaponDiscs=new[]{hud.WeaponDiscRect(0),hud.WeaponDiscRect(1)};fact.weaponFooter=hud.WeaponSwitchRect;
                File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));yield return null;
            }
        }
        [Serializable] private sealed class PolishFrame
        {
            public string file,inspected,window,detailText,tooltipText;
            public Rect service,details,quickGroup,menuGroup,tooltip; public Rect[] quick,keys,counts,icons,navKeys,navIcons,cards;
            public int[] amounts,serials;public bool[] successes; public float[] cooldowns;
        }
        [Serializable] private sealed class PolishReport
        {public bool ready;public string error="";public List<PolishFrame> frames=new List<PolishFrame>();}
        private static IEnumerator CapturePolish(GameObject owner,string directory)
        {
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var stock=game.Gathering.Economy.Inventory;
            var report=new PolishReport();var reportPath=Path.Combine(directory,"capture-status.json");
            game.RunDriver.enabled=false;game.SetPaused(false);game.HeroCombat.AutomaticTick=false;
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            var start=scene.PlayerMover.transform.position;
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();yield return Settle();yield return Shot(size,"00-ordinary");
                Move(hud.HudMenuIconRect(1).center);yield return Settle();yield return Shot(size,"00b-navigation-hover");
                Move(new Vector2(Screen.width*.5f,Screen.height*.3f));yield return Settle();
                hud.RequestQuickSlotUse(0);yield return new WaitForSecondsRealtime(.16f);yield return Shot(size,"01-zero-refused");
                hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(2);yield return new WaitForSecondsRealtime(.5f);yield return Settle();yield return Shot(size,"02-base-prices");
                Move(hud.BuildChoiceRect(1).center);yield return Settle();yield return Shot(size,"02b-dwelling-hover-no-popup");
            }
            hud.CloseWindow();foreach(var kind in new[]{Not3A.Stage2.Map.MapResourceKind.Wood,Not3A.Stage2.Map.MapResourceKind.Stone,Not3A.Stage2.Map.MapResourceKind.Metal})stock.TryAdd(kind,10000);
            stock.TryAddGold(10000);
            scene.PlayerMover.transform.position=travel.CityPresentation.MerchantPosition;yield return Settle();
            Require(binding.TryOpenMerchant(),"Polish local merchant");
            var documents=new[]{Stage2BlueprintKind.Longshot,Stage2BlueprintKind.Sawmill,Stage2BlueprintKind.Quarry,Stage2BlueprintKind.IronMine,Stage2BlueprintKind.ForgeConstruction};
            foreach(var kind in documents){var paper=binding.Merchant.BlueprintItems.Single(p=>p.Kind==kind);hud.SelectMerchantItem(true,paper.Id);Require(binding.TryMerchantTrade(true,paper.Id),"Polish paid paper");}
            var health=binding.Merchant.Items.First(p=>p.Type==Stage2ItemType.HealthPotion);
            for(var i=0;i<5;i++){hud.SelectMerchantItem(true,health.Id);Require(binding.TryMerchantTrade(true,health.Id),"Polish paid potion");}
            hud.CloseWindow();scene.PlayerMover.transform.position=start;
            travel.BeginWorkshopPlacement();Vector2? site=null;
            for(var ring=5;ring<70&&!site.HasValue;ring+=2)for(var x=-ring;x<=ring&&!site.HasValue;x+=2)for(var y=-ring;y<=ring;y+=2)
            {var point=(Vector2)start+new Vector2(x,y);if(travel.EvaluateWorkshopAt(point)==WorkshopPlacementResult.Ready){site=point;break;}}
            Require(site.HasValue&&travel.TryBuildWorkshopAt(site.Value)==WorkshopPlacementResult.Built,"Polish paid research centre");
            scene.PlayerMover.transform.position=travel.WorkshopInteractionPosition;yield return Settle();
            foreach(var kind in documents){binding.OpenResearchForBlueprint((int)kind);yield return Settle();Require(binding.TryStudyResearch((int)kind),"Polish local study");}
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(0);
                Move(hud.BuildChoiceRect(1).center);yield return Settle();yield return Shot(size,"03-funded-longshot-stats");
                Require(hud.ActivateBuildChoice(1),"Reference real known blueprint selection");
                Move(new Vector2(Screen.width*.5f,Screen.height*.3f));yield return Settle();yield return Shot(size,"03b-construction-active");
                game.Building.CancelPlacement();Move(hud.BuildChoiceRect(1).center);yield return Settle();
                var snapshot=stock.CaptureState();stock.Reset();yield return Settle();yield return Shot(size,"04-unfunded-defense");
                hud.SetBuildGroup(1);Move(hud.BuildChoiceRect(2).center);yield return Settle();yield return Shot(size,"05-three-resource-production");
                stock.RestoreState(snapshot);hud.CloseWindow();Move(hud.QuickSlotRect(0).center);
                binding.HeroSkills.Rules.Tick(21,true);game.GetComponent<Stage2HeroRecoveryDriver>().TryApplyExplorationDamage(40);
                yield return Settle();yield return Shot(size,"06-quick-hover");
                hud.RequestQuickSlotUse(0);yield return new WaitForSecondsRealtime(.16f);yield return Shot(size,"07-quick-success");
                hud.RequestQuickSlotUse(0);yield return new WaitForSecondsRealtime(.16f);yield return Shot(size,"08-quick-cooldown-refused");
                Move(hud.QuickSlotRect(3).center);hud.RequestQuickSlotUse(3);yield return Settle();yield return Shot(size,"09-empty-reserve");
                Move(new Vector2(Screen.width*.5f,Screen.height*.4f));yield return Settle();
            }
            report.ready=true;File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
            if(Environment.GetEnvironmentVariable("NOT3A_HUD_REFACTOR_QUIT")=="1")Application.Quit(0);
            void Move(Vector2 gui)
            {InputSystem.QueueDeltaStateEvent(Mouse.current.position,new Vector2(gui.x,Screen.height-gui.y));InputSystem.Update();}
            IEnumerator Shot(Vector2Int size,string state)
            {
                yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
                Require(image.width==size.x&&image.height==size.y,"Polish exact native dimensions");
                var file=size.x+"x"+size.y+"-"+state+".png";File.WriteAllBytes(Path.Combine(directory,file),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
                var slots=hud.HeroLoadoutView.QuickSlots;
                Require(hud.ActiveWindow!=Stage3PlayerWindow.Construction||string.IsNullOrEmpty(hud.LastLoadoutTooltipText),"Construction details stay left; no hover popup");
                report.frames.Add(new PolishFrame{file=file,inspected=hud.InspectedBuildName,window=hud.ActiveWindow.ToString(),detailText=hud.InspectedBuildDetailText,
                    service=hud.NavigationRibbonRect,details=hud.ConstructionDetailsRect,quickGroup=hud.QuickRibbonRect,menuGroup=hud.MenuRibbonRect,
                    tooltip=hud.LastLoadoutTooltipRect,tooltipText=hud.LastLoadoutTooltipText,
                    quick=Enumerable.Range(0,4).Select(hud.QuickSlotRect).ToArray(),keys=Enumerable.Range(0,4).Select(hud.QuickSlotKeyRect).ToArray(),
                    counts=Enumerable.Range(0,4).Select(hud.QuickSlotCountRect).ToArray(),icons=Enumerable.Range(0,4).Select(hud.QuickSlotIconRect).ToArray(),
                    navKeys=Enumerable.Range(0,6).Select(hud.HudMenuKeyRect).ToArray(),navIcons=Enumerable.Range(0,6).Select(hud.HudMenuIconRect).ToArray(),
                    cards=Enumerable.Range(0,3).Select(hud.BuildChoiceRect).ToArray(),amounts=slots.Select(v=>v.Count).ToArray(),serials=slots.Select(v=>v.FeedbackSerial).ToArray(),
                    successes=slots.Select(v=>v.FeedbackSucceeded).ToArray(),cooldowns=slots.Select(v=>v.CooldownRemaining).ToArray()});
                File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));yield return null;
            }
        }
        [Serializable] private sealed class CompactFrame
        {
            public string file,window,tooltipText,detail;
            public Rect service,minimap,minimapTerrain,minimapOrnament,skill,close,tooltip,returnText;
            public float returnMeasured;
            public Rect[] quick,keys,counts,nav,navIcons,modalTabs;
            public int[] amounts,serials;
        }
        [Serializable] private sealed class CompactReport
        {public bool ready;public List<CompactFrame> frames=new List<CompactFrame>();}
        private static IEnumerator CaptureCompact(GameObject owner,string directory)
        {
            var game=owner.GetComponent<Stage2GameComposition>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var report=new CompactReport();var reportPath=Path.Combine(directory,"capture-status.json");
            game.RunDriver.enabled=false;game.SetPaused(false);game.HeroCombat.AutomaticTick=false;
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();MoveWorld();yield return Settle();yield return Shot(size,"00-ordinary");
                Move(hud.QuickSlotRect(0).center);yield return Settle();yield return Shot(size,"01-quick-hover");
                MoveWorld();hud.RequestQuickSlotUse(0);yield return new WaitForSecondsRealtime(.16f);yield return Shot(size,"02-zero-refused");
                yield return new WaitForSecondsRealtime(.5f);Move(hud.QuickSlotRect(3).center);hud.RequestQuickSlotUse(3);yield return Settle();yield return Shot(size,"03-empty-reserve");
                hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(0);Move(hud.BuildChoiceRect(0).center);yield return Settle();yield return Shot(size,"04-construction-left");
                hud.SetWindow(Stage3PlayerWindow.Inventory);MoveWorld();yield return Settle();yield return Shot(size,"05-inventory-active");
                Move(hud.FullscreenTabRect(Stage3PlayerWindow.Research).center);yield return Settle();yield return Shot(size,"06-modal-research-hover");
                Move(hud.CloseButtonRect.center);yield return Settle();yield return Shot(size,"07-return-player-hover");
                hud.SetWindow(Stage3PlayerWindow.Research);MoveWorld();yield return Settle();yield return Shot(size,"08-research-active");
                hud.SetWindow(Stage3PlayerWindow.Hero);MoveWorld();yield return Settle();yield return Shot(size,"09-hero-active");
                hud.SetWindow(Stage3PlayerWindow.Map);MoveWorld();yield return Settle();yield return Shot(size,"10-map-active");
            }
            report.ready=true;File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
            if(Environment.GetEnvironmentVariable("NOT3A_HUD_REFACTOR_QUIT")=="1")Application.Quit(0);
            void MoveWorld()=>Move(new Vector2(Screen.width*.5f,Screen.height*.36f));
            void Move(Vector2 gui)
            {InputSystem.QueueDeltaStateEvent(Mouse.current.position,new Vector2(gui.x,Screen.height-gui.y));InputSystem.Update();}
            IEnumerator Shot(Vector2Int size,string state)
            {
                yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
                Require(image.width==size.x&&image.height==size.y,"Compact exact native dimensions");
                var file=size.x+"x"+size.y+"-"+state+".png";File.WriteAllBytes(Path.Combine(directory,file),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
                var slots=hud.HeroLoadoutView.QuickSlots;
                Require(slots[3].Count==0&&slots[3].FeedbackSerial==0,"Compact F4 remains empty and silent");
                Require(hud.ActiveWindow!=Stage3PlayerWindow.Construction||string.IsNullOrEmpty(hud.LastLoadoutTooltipText),"Compact construction info remains left");
                Require(!hud.IsModalOpen||hud.FullscreenReturnMeasuredHeight<=hud.FullscreenReturnTextRect.height+.1f,"Multiline return label fits actual padded area");
                report.frames.Add(new CompactFrame{file=file,window=hud.ActiveWindow.ToString(),service=hud.NavigationRibbonRect,
                    minimap=hud.MinimapRect,minimapTerrain=hud.MinimapTerrainRect,minimapOrnament=hud.MinimapOrnamentRect,
                    skill=hud.SkillSlotRect(1),close=hud.CloseButtonRect,tooltip=hud.LastLoadoutTooltipRect,
                    returnText=hud.FullscreenReturnTextRect,returnMeasured=hud.FullscreenReturnMeasuredHeight,
                    tooltipText=hud.LastLoadoutTooltipText,detail=hud.InspectedBuildDetailText,
                    quick=Enumerable.Range(0,4).Select(hud.QuickSlotRect).ToArray(),keys=Enumerable.Range(0,4).Select(hud.QuickSlotKeyRect).ToArray(),
                    counts=Enumerable.Range(0,4).Select(hud.QuickSlotCountRect).ToArray(),
                    nav=new[]{Stage3PlayerWindow.None,Stage3PlayerWindow.Inventory,Stage3PlayerWindow.Construction,Stage3PlayerWindow.Research,Stage3PlayerWindow.Hero,Stage3PlayerWindow.Map}.Select(hud.HudMenuButtonRect).ToArray(),
                    navIcons=Enumerable.Range(0,6).Select(hud.HudMenuIconRect).ToArray(),
                    modalTabs=new[]{Stage3PlayerWindow.None,Stage3PlayerWindow.Inventory,Stage3PlayerWindow.Research,Stage3PlayerWindow.Hero,Stage3PlayerWindow.Map}.Select(hud.FullscreenTabRect).ToArray(),
                    amounts=slots.Select(v=>v.Count).ToArray(),serials=slots.Select(v=>v.FeedbackSerial).ToArray()});
                File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));yield return null;
            }
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("D124 native review: "+message);}
        private static IEnumerator Resize(Vector2Int size){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);}
        private static IEnumerator Settle(){yield return null;yield return null;yield return null;}
    }
}
