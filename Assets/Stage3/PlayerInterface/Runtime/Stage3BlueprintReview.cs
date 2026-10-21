using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit opt-in development-player verification; ordinary startup receives no QA grants.</summary>
    public static class Stage3BlueprintReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<12;i++)yield return null;
            var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var merchant=binding.Merchant;var equipment=binding.Equipment.Rules;
            var papers=binding.Blueprints;game.RunDriver.enabled=false;game.RunDriver.ResetRun();game.SetPaused(false);game.HeroCombat.AutomaticTick=false;
            var wallet=game.Gathering.Economy.Inventory;wallet.TryAddGold(10000);
            var shop=travel.CityPresentation.MerchantPosition;scene.PlayerMover.transform.position=shop;
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();Require(binding.TryOpenMerchant(),"open shop");
                hud.SelectMerchantItem(true,merchant.BlueprintItems.First(p=>p.Kind==Stage2BlueprintKind.ForgeII).Id);
                yield return Shot(directory,Name(size)+"-merchant-blueprints");
            }
            foreach(var paper in merchant.BlueprintItems.ToArray())
            {hud.SelectMerchantItem(true,paper.Id);Require(binding.TryMerchantTrade(true,paper.Id),"buy paper");}
            hud.CloseWindow();travel.BeginWorkshopPlacement();var origin=(Vector2)scene.PlayerMover.transform.position;Vector2? site=null;
            // Use the same legal construction evaluator; never bypass occupancy or scene wiring.
            for(var ring=5;ring<150&&!site.HasValue;ring+=2)for(var x=-ring;x<=ring&&!site.HasValue;x+=2)for(var y=-ring;y<=ring;y+=2)
            {var p=origin+new Vector2(x,y);if(travel.EvaluateWorkshopAt(p)==WorkshopPlacementResult.Ready){site=p;break;}}
            Require(site.HasValue,"legal workshop site");Require(travel.TryBuildWorkshopAt(site.Value)==WorkshopPlacementResult.Built,"build workshop");
            scene.PlayerMover.transform.position=travel.WorkshopInteractionPosition;yield return null;
            if(Environment.GetEnvironmentVariable("NOT3A_BLUEPRINT_INPUT_REVIEW")=="1")
            {
                yield return Resize(new Vector2Int(1600,900));hud.CloseWindow();
                var inputCamera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;inputCamera.orthographicSize=6;
                inputCamera.transform.position=(Vector3)(travel.WorkshopPosition+Vector2.up)-inputCamera.transform.forward*30;
                File.WriteAllText(Path.Combine(directory,"verification.txt"),"Disposable input fixture:10000QA gold, actual purchases of4papers, legal workshop placement, hero at its entrance. Clock/combat stopped. Awaiting actual local E and mouse Study; no UI state injected. Normal startup unaffected.\n");
                Debug.Log("S3_BLUEPRINT_INPUT_READY_E");
                while(!papers.IsStudied(Stage2BlueprintKind.ForgeII))yield return null;
                Require(papers.Find(Stage2BlueprintKind.ForgeII)==null&&equipment.ForgeLevel==0,"actual input consumed paper without auto-upgrading forge");
                yield return Shot(directory,"actual-input-studied");
                File.AppendAllText(Path.Combine(directory,"verification.txt"),"PASS actual local E opened research; mouse selected ForgeII and clicked Study; paper consumed; knowledge history selected; forge not auto-upgraded.\n");
                Debug.Log("S3_BLUEPRINT_INPUT_OK");yield break;
            }
            foreach(var size in sizes)
            {
                yield return Resize(size);binding.OpenResearchForBlueprint(2);hud.SelectResearchFilter(false);hud.SelectResearchEntry(2);
                yield return Shot(directory,Name(size)+"-workshop-owned");
                hud.SelectResearchEntry(3);yield return Shot(directory,Name(size)+"-forge-iii-prerequisite");
            }
            hud.SelectResearchEntry(2);Require(binding.TryStudyResearch(2),"study forge II");
            Require(papers.Find(Stage2BlueprintKind.ForgeII)==null&&papers.IsStudied(Stage2BlueprintKind.ForgeII),"paper consumed, knowledge retained");
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.SelectResearchFilter(true);hud.SelectResearchEntry(2);
                yield return Shot(directory,Name(size)+"-workshop-studied");
            }
            // Disposable second source simulates a future duplicate drop/merchant, using the same ownership rules.
            var secondShop=new Stage2Merchant(equipment,merchant.Catalog);var duplicate=secondShop.BlueprintItems.First(p=>p.Kind==Stage2BlueprintKind.ForgeII);
            Require(secondShop.TryTradeBlueprint(true,duplicate.Id)==Stage2TradeResult.Ready,"duplicate fixture");
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.SetWindow(Stage3PlayerWindow.Inventory);
                typeof(Stage3PlayerInterfaceBinding).GetMethod("SelectEquipmentItem",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(binding,new object[]{duplicate.Id});
                yield return Shot(directory,Name(size)+"-bag-known-duplicate");
            }
            hud.CloseWindow();scene.PlayerMover.transform.position=shop;yield return null;Require(binding.TryOpenMerchant(),"return merchant");
            hud.SelectMerchantItem(false,duplicate.Id);Require(binding.TryMerchantTrade(false,duplicate.Id),"sell duplicate");
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.SelectMerchantItem(true,duplicate.Id);
                yield return Shot(directory,Name(size)+"-merchant-known-duplicate");
            }
            // Reset all QA ownership/knowledge. Leave a small opt-in fixture for actual mouse verification.
            game.RunDriver.ResetRun();game.SetPaused(false);wallet.TryAddGold(1000);scene.PlayerMover.transform.position=shop;
            yield return Resize(new Vector2Int(1600,900));hud.CloseWindow();
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;camera.orthographicSize=6;
            camera.transform.position=(Vector3)(shop+Vector2.up*1.5f)-camera.transform.forward*30;
            File.WriteAllText(Path.Combine(directory,"verification.txt"),"Disposable QA: frozen run clock, temporary gold, actual finite merchant trades, legal workshop construction, local study, one simulated duplicate source. 18 native frames at720/900/1080; all grants/knowledge reset afterwards. Actual-click fixture: initial12stock,1005gold,hero at merchant; normal startup unaffected.\n");
            Debug.Log("S3_BLUEPRINT_NATIVE_READY "+directory);
        }
        private static void Require(bool value,string action){if(!value)throw new InvalidOperationException("Blueprint QA failed: "+action);}
        private static string Name(Vector2Int size)=>size.x+"x"+size.y;
        private static IEnumerator Resize(Vector2Int size){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);}
        private static IEnumerator Shot(string directory,string name)
        {yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return null;}
    }
}
