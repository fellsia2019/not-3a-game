#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3LoadoutEquipmentPresentationTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Component hud,binding;
        private int previousRate;
        private static T Read<T>(object target,string property)=>(T)target.GetType().GetProperty(property).GetValue(target);
        private static object Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(target,args);
        private void Window(string name)=>Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,name));
        [UnitySetUp] public IEnumerator Load()
        {
            previousRate=Time.captureFramerate;Time.captureFramerate=60;Time.timeScale=1;
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<18;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();
            hud=game.GetComponent("Stage3FantasyHud");binding=game.GetComponent("Stage3PlayerInterfaceBinding");
            Assert.That(Read<bool>(hud,"IsReady"),Is.True);
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.ResetRun();yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;input.TearDown();Time.captureFramerate=previousRate;Time.timeScale=1;
        }
        private static void Contains(Rect outer,Rect inner,string context)
        {
            Assert.That(inner.width,Is.GreaterThan(0),context);Assert.That(inner.height,Is.GreaterThan(0),context);
            Assert.That(inner.xMin,Is.GreaterThanOrEqualTo(outer.xMin-.001f),context);
            Assert.That(inner.yMin,Is.GreaterThanOrEqualTo(outer.yMin-.001f),context);
            Assert.That(inner.xMax,Is.LessThanOrEqualTo(outer.xMax+.001f),context);
            Assert.That(inner.yMax,Is.LessThanOrEqualTo(outer.yMax+.001f),context);
        }
        [UnityTest] public IEnumerator TwelveProductionSlotsKeepBothWeaponsAndIndependentRingTargets()
        {
            var rules=game.GetComponent<Stage2EquipmentDriver>().Rules;
            Assert.That(rules.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.C,1,out var staff),Is.True);
            Assert.That(rules.TryEquip(staff.Id),Is.EqualTo(Stage2EquipmentResult.Ready));
            Window("Inventory");yield return null;yield return null;
            var view=Read<object>(hud,"EquipmentView");var ids=(int[])view.GetType().GetField("EquippedItemIds").GetValue(view);
            Assert.That(ids.Length,Is.EqualTo(12));Assert.That(ids[4],Is.EqualTo(rules.EquippedAt((Stage2ItemSlot)4).Id));Assert.That(ids[7],Is.EqualTo(staff.Id));
            Assert.That(ids[4],Is.Not.EqualTo(ids[7]));
            var content=Read<Rect>(hud,"WindowContentRect");var slots=new Rect[12];
            for(var i=0;i<12;i++){slots[i]=(Rect)Call(hud,"EquipmentSlotRect",i);Contains(content,slots[i],"slot "+i);}
            var toggle=Read<Rect>(hud,"EquipmentWeaponSwitchRect");Contains(content,toggle,"weapon switch");
            for(var i=0;i<12;i++)
            {
                Assert.That(slots[i].Overlaps(toggle),Is.False,"switch overlaps slot "+i);
                for(var j=i+1;j<12;j++)Assert.That(slots[i].Overlaps(slots[j]),Is.False,"overlapping slots "+i+" / "+j);
            }
            Assert.That(slots[9].y,Is.EqualTo(slots[10].y));Assert.That(slots[9].x,Is.LessThan(slots[10].x));
            Call(hud,"RequestWeaponSwitch");yield return null;yield return null;
            Assert.That(rules.ActiveWeaponType,Is.EqualTo(Stage2ItemType.Bow));
            view=Read<object>(hud,"EquipmentView");ids=(int[])view.GetType().GetField("EquippedItemIds").GetValue(view);
            Assert.That(ids[7],Is.EqualTo(staff.Id),"switching never unequips the staff");
        }
        [UnityTest] public IEnumerator ElevenForgeChoicesFitContentAndPotionCaptionStaysUngraded()
        {
            Window("Forge");yield return null;
            var names=(string[])hud.GetType().GetField("EquipmentTypeNames",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            Assert.That(names.Length,Is.EqualTo(11));Assert.That(names[1],Is.EqualTo("Посох"));
            var content=Read<Rect>(hud,"WindowContentRect");var previous=Rect.zero;
            for(var i=0;i<11;i++)
            {
                var rect=(Rect)Call(hud,"ForgeTypeRect",i);Contains(content,rect,"forge type "+i);
                if(i>0)Assert.That(rect.Overlaps(previous),Is.False);previous=rect;
            }
            var rules=game.GetComponent<Stage2EquipmentDriver>().Rules;
            game.Gathering.Economy.Inventory.TryAddGold(1000);
            var shop=Read<Stage2Merchant>(binding,"Merchant");var stock=shop.Items.First(i=>i.Type==Stage2ItemType.MixedPotion);
            for(var i=0;i<3;i++)Assert.That(shop.TryTrade(true,stock.Id),Is.EqualTo(Stage2TradeResult.Ready));
            Window("Inventory");yield return null;yield return null;
            var view=Read<object>(hud,"EquipmentView");var bag=(Array)view.GetType().GetField("BagItems").GetValue(view);object potion=null;
            foreach(var item in bag)if((int)item.GetType().GetField("TypeIndex").GetValue(item)==13)potion=item;
            Assert.That(potion,Is.Not.Null);Assert.That((bool)potion.GetType().GetField("Consumable").GetValue(potion),Is.True);
            Assert.That((int)potion.GetType().GetField("Quantity").GetValue(potion),Is.EqualTo(3));
            var caption=(string)hud.GetType().GetMethod("ItemCellCaption",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new[]{potion});
            Assert.That(caption,Is.EqualTo("HP + MP"));Assert.That(caption,Does.Not.Contain("LVL"));
        }
        [Test] public void ImportedNewArtHasEveryRankAndContainedSourceBoundsWithDistinctPotionCells()
        {
            var art=Resources.Load<ScriptableObject>("Stage3ForgeArt");Assert.That(art,Is.Not.Null);
            var type=art.GetType();var lookup=type.GetMethod("TryGetIcon");var bounds=type.GetMethod("UiVisibleBounds");
            Assert.That(((Rect[])type.GetField("uiVisibleBounds").GetValue(art)).Length,Is.EqualTo(88));
            Assert.That(((Rect[])type.GetField("potionVisibleBounds").GetValue(art)).Length,Is.EqualTo(3));
            foreach(var item in new[]{1,8,9,10,11,12,13})for(var rank=0;rank<(item<11?8:1);rank++)
            {
                var args=new object[]{item,rank,null,default(Rect)};Assert.That((bool)lookup.Invoke(art,args),Is.True,"missing icon "+item+" / "+rank);
                Assert.That(args[2],Is.InstanceOf<Texture2D>());var cell=(Rect)args[3];Contains(new Rect(0,0,1,1),cell,"source cell");
                var visible=(Rect)bounds.Invoke(art,new object[]{item,rank,cell});Contains(cell,visible,"visible silhouette "+item+" / "+rank);
                Assert.That(visible.width*visible.height,Is.GreaterThan(.001f),"nonempty art");
            }
        }
    }
}
#endif
