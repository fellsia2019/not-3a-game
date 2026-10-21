using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Disposable opt-in native QA; grants here never run during ordinary startup.</summary>
    public static class Stage3MerchantReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<12;i++)yield return null;
            var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var merchant=binding.Merchant;var equipment=binding.Equipment.Rules;
            game.RunDriver.enabled=false;game.RunDriver.ResetRun();game.SetPaused(false);game.HeroCombat.AutomaticTick=false;
            var point=travel.CityPresentation.MerchantPosition;scene.PlayerMover.transform.position=point;
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;camera.orthographicSize=6;
            camera.transform.position=(Vector3)(point+Vector2.up*1.5f)-camera.transform.forward*30;
            var wallet=game.Gathering.Economy.Inventory;wallet.TryAddGold(10000);
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();binding.TryOpenMerchant();hud.SelectMerchantItem(true,merchant.Items[0].Id);
                yield return Shot(directory,Name(size)+"-buy-empty-bag");
                var gold=wallet.Gold;wallet.TrySpend(0,0,0,gold);yield return Shot(directory,Name(size)+"-unfunded");wallet.TryAddGold(gold);
                scene.PlayerMover.transform.position=point+Vector2.down*10;
                yield return Shot(directory,Name(size)+"-too-far");scene.PlayerMover.transform.position=point;
            }
            for(var i=0;i<40;i++)equipment.TryReceiveLoot((Stage2ItemType)(i%8),(Stage2ItemQuality)(i/8),i%2==0?1:100,out _);
            equipment.TryReceiveLoot(Stage2ItemType.Bow,Stage2ItemQuality.SSS,100,out var rare);
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();binding.TryOpenMerchant();hud.SelectMerchantItem(false,rare.Id);
                yield return Shot(directory,Name(size)+"-sell-sss");
                Scroll(hud,false);yield return Shot(directory,Name(size)+"-player-scroll");
            }
            foreach(var item in equipment.Items.Where(item=>!equipment.IsEquipped(item)).ToArray())
            {
                hud.SelectMerchantItem(false,item.Id);
                if(!binding.TryMerchantTrade(false,item.Id))throw new InvalidOperationException("QA sale failed "+item.Id);
            }
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();binding.TryOpenMerchant();hud.SelectMerchantItem(true,rare.Id);Scroll(hud,true);
                yield return Shot(directory,Name(size)+"-merchant-scroll");
            }
            wallet.TryAddGold(1000000);
            foreach(var item in merchant.Items.ToArray())
            {
                hud.SelectMerchantItem(true,item.Id);
                if(!binding.TryMerchantTrade(true,item.Id))throw new InvalidOperationException("QA purchase failed "+item.Id);
            }
            foreach(var size in sizes)
            {yield return Resize(size);hud.CloseWindow();binding.TryOpenMerchant();yield return Shot(directory,Name(size)+"-empty-stock");}
            // Clear all visual QA grants before the small actual-input fixture.
            game.RunDriver.ResetRun();game.SetPaused(false);scene.PlayerMover.transform.position=point;
            wallet.TryAddGold(100);equipment.TryReceiveLoot(Stage2ItemType.Helmet,Stage2ItemQuality.E,12,out _);
            yield return Resize(new Vector2Int(1600,900));hud.CloseWindow();
            File.WriteAllText(Path.Combine(directory,"verification.txt"),"Disposable QA: frozen run clock, temporary gold/41 bag item grants across existing qualities for21native frames at720/900/1080. Sales/purchases use actual product transactions. Reset afterwards, then actual-click fixture: finite8F1 stock,105gold,one spare E12helmet. Ordinary startup never grants stock to player or gold.\n");
            Debug.Log("S3_MERCHANT_NATIVE_READY "+directory);
        }
        private static void Scroll(Stage3FantasyHud hud,bool buy)=>typeof(Stage3FantasyHud).GetField(
            buy?"merchantStockScroll":"merchantPlayerScroll",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(hud,new Vector2(0,10000));
        private static string Name(Vector2Int size)=>size.x+"x"+size.y;
        private static IEnumerator Resize(Vector2Int size)
        {Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);}
        private static IEnumerator Shot(string dir,string name)
        {yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return null;}
    }
}
