#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3MerchantTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component binding,hud,travel;
        private Stage2EquipmentDriver equipment;
        private Stage2Merchant merchant;
        private object Call(object owner,string method,params object[] args)=>owner.GetType().GetMethod(method).Invoke(owner,args);
        private T Get<T>(object owner,string name)=>(T)owner.GetType().GetProperty(name).GetValue(owner);
        private T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name).GetValue(owner);
        private Vector2 ShopPosition=>Get<Vector2>(Get<object>(travel,"CityPresentation"),"MerchantPosition");
        private bool Open()=> (bool)Call(binding,"TryOpenMerchant");
        private bool Trade(bool buy,int id)=>(bool)Call(binding,"TryMerchantTrade",buy,id);
        private void Select(bool buy,int id)=>Call(hud,"SelectMerchantItem",buy,id);
        private object View=>Get<object>(hud,"MerchantView");
        private Stage2Inventory Wallet=>game.Gathering.Economy.Inventory;
        [UnitySetUp] public IEnumerator Load()
        {
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");travel=game.GetComponent("Stage3WorldTravel");
            equipment=game.GetComponent<Stage2EquipmentDriver>();merchant=Get<Stage2Merchant>(binding,"Merchant");
            Assert.That(merchant,Is.Not.Null);game.RunDriver.ResetRun();scene.PlayerMover.transform.position=ShopPosition;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;input.TearDown();Time.timeScale=1f;
        }
        [UnityTest] public IEnumerator ShopMouseClickOpensDualInventoryAndEscapeConsumesHeldClickWithoutPausingWorld()
        {
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;camera.orthographicSize=6;
            camera.transform.position=(Vector3)(ShopPosition+Vector2.up*1.5f)-camera.transform.forward*30;
            yield return null;
            var shop=Get<SpriteRenderer>(Get<object>(travel,"CityPresentation"),"ShopVisual");
            input.Set(Mouse.current.position,(Vector2)camera.WorldToScreenPoint(shop.bounds.center));yield return null;
            input.Press(Mouse.current.leftButton);yield return null;yield return null;
            Assert.That(Get<bool>(hud,"MerchantOpen"),Is.True);Assert.That(game.RunDriver.Run.IsPaused,Is.False);Assert.That(game.PlayerInputBlocked,Is.True);
            Assert.That(Field<Array>(View,"PlayerItems").Length,Is.Zero);Assert.That(Field<Array>(View,"MerchantItems").Length,Is.EqualTo(15));
            input.Press(Keyboard.current.escapeKey);yield return null;input.Release(Keyboard.current.escapeKey);yield return null;yield return null;
            Assert.That(Get<bool>(hud,"MerchantOpen"),Is.False);Assert.That(game.PlayerInputBlocked,Is.False);
            input.Release(Mouse.current.leftButton);yield return null;
        }
        [UnityTest] public IEnumerator PaidBuyAndSaleRefreshBothInventoriesAndCannotReplayAStaleSelection()
        {
            Wallet.TryAddGold(100);var before=Wallet.Gold;var materials=new[]{Wallet.Get(MapResourceKind.Wood),Wallet.Get(MapResourceKind.Stone),Wallet.Get(MapResourceKind.Metal)};
            Assert.That(Open(),Is.True);var item=merchant.Items[0];var buy=merchant.Catalog.BuyPrice(item);var sell=merchant.Catalog.SellPrice(item);
            Select(true,item.Id);Assert.That(Trade(true,item.Id),Is.True);Assert.That(equipment.Rules.Find(item.Id),Is.SameAs(item));
            Assert.That(Field<Array>(View,"PlayerItems").Length,Is.EqualTo(1));Assert.That(Field<Array>(View,"MerchantItems").Length,Is.EqualTo(14));
            Assert.That(Field<int>(View,"Gold"),Is.EqualTo(before-buy));Assert.That(Trade(true,item.Id),Is.False);Assert.That(Wallet.Gold,Is.EqualTo(before-buy));
            Select(false,item.Id);Assert.That(Trade(false,item.Id),Is.True);Assert.That(merchant.Find(item.Id),Is.SameAs(item));
            Assert.That(Field<Array>(View,"PlayerItems").Length,Is.Zero);Assert.That(Field<Array>(View,"MerchantItems").Length,Is.EqualTo(15));
            Assert.That(Wallet.Gold,Is.EqualTo(before-buy+sell));
            Assert.That(new[]{Wallet.Get(MapResourceKind.Wood),Wallet.Get(MapResourceKind.Stone),Wallet.Get(MapResourceKind.Metal)},Is.EqualTo(materials));
            Call(hud,"CloseWindow");Assert.That(Trade(true,merchant.Items[0].Id),Is.False);yield return null;
            Assert.That(Open(),Is.True);Assert.That(merchant.Find(item.Id),Is.SameAs(item));Assert.That(Field<int>(View,"Gold"),Is.EqualTo(before-buy+sell));
        }
        [UnityTest] public IEnumerator UnfundedFarPausedAndHiddenWindowAttemptsCannotSpendOrTransfer()
        {
            var item=merchant.Items[0];var gold=Wallet.Gold;Assert.That(Open(),Is.True);Select(true,item.Id);
            Assert.That(Field<bool>(View,"CanExecute"),Is.False);Assert.That(Trade(true,item.Id),Is.False);Assert.That(Wallet.Gold,Is.EqualTo(gold));
            Wallet.TryAddGold(100);yield return null;scene.PlayerMover.transform.position=ShopPosition+Vector2.down*10;yield return null;
            Assert.That(Field<bool>(View,"CanExecute"),Is.False);Assert.That(Field<string>(View,"UnavailableReason"),Does.Contain("Подойдите"));
            Assert.That(Trade(true,item.Id),Is.False);scene.PlayerMover.transform.position=ShopPosition;yield return null;
            game.SetPaused(true);yield return null;Assert.That(Get<bool>(hud,"MerchantOpen"),Is.False);Assert.That(Trade(true,item.Id),Is.False);
            Assert.That(merchant.Find(item.Id),Is.SameAs(item));Assert.That(Wallet.Gold,Is.EqualTo(gold+100));game.SetPaused(false);
        }
        [UnityTest] public IEnumerator DefeatRetryKeepsCurrentTradeWhileFullResetRestocksAndClearsTheWindow()
        {
            Wallet.TryAddGold(100);Assert.That(Open(),Is.True);var item=merchant.Items[1];Select(true,item.Id);Assert.That(Trade(true,item.Id),Is.True);
            var gold=Wallet.Gold;var run=game.RunDriver.Run;
            Assert.That(Stage3TestCycle.StartNight(run),Is.True);
            for(var i=0;i<10&&!run.IsTerminal;i++){var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Lost));yield return null;Assert.That(Get<bool>(hud,"MerchantOpen"),Is.False);
            Assert.That(run.RetryFailedWave(),Is.True);yield return null;
            Assert.That(equipment.Rules.Find(item.Id),Is.SameAs(item));Assert.That(merchant.Find(item.Id),Is.Null);Assert.That(merchant.Items.Count,Is.EqualTo(10));Assert.That(Wallet.Gold,Is.EqualTo(gold));
            scene.PlayerMover.transform.position=ShopPosition;yield return null;Assert.That(Open(),Is.True);
            game.RunDriver.ResetRun();yield return null;
            Assert.That(Get<bool>(hud,"MerchantOpen"),Is.False);Assert.That(merchant.Items.Count,Is.EqualTo(11));Assert.That(equipment.Rules.Find(item.Id),Is.Null);
            Assert.That(merchant.Items.All(i=>i.Level==1&&i.Quality==Stage2ItemQuality.F),Is.True);Assert.That(Wallet.Gold,Is.EqualTo(5));
        }
    }
}
#endif
