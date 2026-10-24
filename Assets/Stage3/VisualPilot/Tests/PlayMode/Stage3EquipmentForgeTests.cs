#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
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
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3EquipmentForgeTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2EquipmentDriver equipment;
        private Component forge,binding,hud;
        private int previousRate;
        private static T Read<T>(object owner,string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner,string name,params object[] args) => owner.GetType().GetMethod(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Invoke(owner,args);
        private void Window(string name)
        {var type=hud.GetType().GetProperty("ActiveWindow").PropertyType;Call(hud,"SetWindow",Enum.Parse(type,name));}
        [UnitySetUp] public IEnumerator Load()
        {
            previousRate=Time.captureFramerate;Time.captureFramerate=60;Time.timeScale=1;
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<14;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            equipment=game.GetComponent<Stage2EquipmentDriver>();forge=game.GetComponent("Stage3Forge");
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");
            Assert.That(equipment,Is.Not.Null);Assert.That(Read<bool>(forge,"IsReady"),Is.True);
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.ResetRun();yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;input.TearDown();Time.captureFramerate=previousRate;Time.timeScale=1;
        }
        private void Stock()
        {Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintKind.ForgeConstruction);foreach(MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind)))game.Gathering.Economy.Inventory.TryAdd(kind,300);game.Gathering.Economy.Inventory.TryAddGold(300);}
        private Vector2 LegalSite()
        {
            var origin=(Vector2)scene.PlayerMover.transform.position;
            for(var ring=5;ring<45;ring+=2)for(var x=-ring;x<=ring;x+=2)for(var y=-ring;y<=ring;y+=2)
            {var p=origin+new Vector2(x,y);if(Call(forge,"EvaluateAt",p).ToString()=="Ready")return p;}
            Assert.Fail("No legal forge site in the real scene.");return default;
        }
        private IEnumerator BuildAndApproach()
        {
            Stock();
            // This fixture verifies forge/equipment behavior after the new recipe prerequisite.
            var shop=Read<Stage2Merchant>(binding,"Merchant");
            var recipe=shop.BlueprintItems.FirstOrDefault(p=>p.Kind==Stage2BlueprintKind.WandRecipe);
            if(recipe!=null){shop.TryTradeBlueprint(true,recipe.Id);equipment.Rules.Blueprints.TryStudy(recipe.Id);}
            Call(forge,"BeginPlacement");var site=LegalSite();
            Assert.That(Call(forge,"TryBuildAt",site).ToString(),Is.EqualTo("Built"));
            scene.PlayerMover.transform.position=Read<Vector2>(forge,"InteractionPosition");scene.CameraFollow.SnapToTarget();yield return null;yield return null;
        }
        [UnityTest] public IEnumerator RealConstructionPaysOnceProtectsGroundAndStartsAtDWithoutScrolls()
        {
            Assert.That(equipment.Rules.Items.Count,Is.EqualTo(3));Stock();Call(forge,"BeginPlacement");
            Assert.That(Call(forge,"EvaluateAt",(Vector2)scene.EntranceMarker.position).ToString(),Is.EqualTo("InvalidGround"));
            Assert.That(Call(forge,"EvaluateAt",new Vector2(float.NaN,0)).ToString(),Is.EqualTo("InvalidGround"));
            var site=LegalSite();var count=scene.PlayerMover.Obstacles.Rectangles.Count;var wallet=game.Gathering.Economy.Inventory;
            var wood=wallet.Get(MapResourceKind.Wood);var catalog=Read<object>(forge,"Catalog");var cost=Read<Stage2ResourceCost>(catalog,"BuildCost");
            Assert.That(Call(forge,"TryBuildAt",site).ToString(),Is.EqualTo("Built"));
            Assert.That(wallet.Get(MapResourceKind.Wood),Is.EqualTo(wood-cost.Wood));
            Assert.That(scene.PlayerMover.Obstacles.Rectangles.Count,Is.EqualTo(count+1));
            Assert.That(scene.PlayerMover.Obstacles.IsBlocked(Read<Vector2>(forge,"Position"),scene.PlayerMover.CollisionRadius),Is.True);
            Assert.That(scene.PlayerMover.Obstacles.IsBlocked(Read<Vector2>(forge,"InteractionPosition"),scene.PlayerMover.CollisionRadius),Is.False);
            Assert.That(Call(forge,"TryBuildAt",site).ToString(),Is.EqualTo("NotPlacing"));
            Assert.That(equipment.Rules.MaximumQuality,Is.EqualTo(Stage2ItemQuality.D));Assert.That(equipment.Rules.ForgeUpgradeScrolls,Is.Zero);
            scene.PlayerMover.transform.position=Read<Vector2>(forge,"InteractionPosition");yield return null;
            Assert.That(Read<string>(forge,"UpgradeUnavailableReason"),Does.Contain("чертёж"));
            Assert.That(Call(forge,"TryUpgradeBuilding"),Is.False);
        }
        [UnityTest] public IEnumerator LocalEOpensActualForgeAndBindingCreatesPaidItemWithoutAutoEquipping()
        {
            yield return BuildAndApproach();input.Release(Keyboard.current.eKey);yield return null;
            input.Press(Keyboard.current.eKey);yield return null;yield return null;
            Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("Forge"));
            var original=equipment.Rules.Equipped(Stage2ItemSlot.Weapon);var count=equipment.Rules.Items.Count;
            Assert.That(Call(binding,"TryForgeOperation"),Is.True);Assert.That(equipment.Rules.Items.Count,Is.EqualTo(count+1));
            Assert.That(equipment.Rules.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(original));
            input.Release(Keyboard.current.eKey);Window("None");yield return null;
            Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("None"));
        }
        [UnityTest] public IEnumerator CraftedStaffUsesSeparateSlotAndLiveCombatProfileWithTwelveSlots()
        {
            yield return BuildAndApproach();
            Stage3TestCycle.StudyBlueprint(game,Stage2BlueprintKind.ForgeII);
            Assert.That(Call(forge,"TryUpgradeBuilding"),Is.True);
            Assert.That(Call(forge,"TryOpen"),Is.True);
            Call(binding,"SelectForgeType",1);Assert.That(Call(binding,"TryForgeOperation"),Is.True);
            var wand=equipment.Rules.Items.Last();Assert.That(wand.Type,Is.EqualTo(Stage2ItemType.Wand));
            Window("Inventory");yield return null;Assert.That(Call(binding,"TryEquipItem",wand.Id),Is.True);
            Assert.That(equipment.Rules.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(wand));
            var stats=equipment.Catalog.Stats(wand.Type,wand.Quality,wand.Level);
            Assert.That(game.HeroCombat.Rules.EffectiveRange,Is.EqualTo(stats.Range));
            Assert.That(game.HeroCombat.Rules.EffectiveDamage,Is.EqualTo(stats.Damage));
            var view=Read<object>(hud,"EquipmentView");Assert.That(((int[])view.GetType().GetField("EquippedItemIds").GetValue(view)).Length,Is.EqualTo(12));
            Assert.That(Call(binding,"TryUnequipSlot",7),Is.True);Assert.That(game.HeroCombat.Rules.EffectiveDamage,Is.EqualTo(equipment.Catalog.Stats(Stage2ItemType.Bow,Stage2ItemQuality.F,1).Damage));
            Assert.That(equipment.Rules.Find(wand.Id),Is.SameAs(wand));
        }
        [UnityTest] public IEnumerator BagExcludesBothWeaponsAndUnequippingStaffKeepsBow()
        {
            Window("Inventory");yield return null;
            Stage2EquipmentItem[] Owned()=>equipment.Rules.Items.ToArray();
            int[] BagIds()
            {
                var view=Read<object>(hud,"EquipmentView");
                var rows=(Array)view.GetType().GetField("BagItems").GetValue(view);
                return rows.Cast<object>().Select(row=>(int)row.GetType().GetField("Id").GetValue(row)).ToArray();
            }
            Assert.That(BagIds(),Is.Empty,"All three starting items are already equipped.");
            var bow=equipment.Rules.Equipped(Stage2ItemSlot.Weapon);
            Window("None");yield return BuildAndApproach();
            Stage3TestCycle.StudyBlueprint(game,Stage2BlueprintKind.ForgeII);Assert.That(Call(forge,"TryUpgradeBuilding"),Is.True);
            Call(forge,"TryOpen");Call(binding,"SelectForgeType",1);
            Assert.That(Call(binding,"TryForgeOperation"),Is.True);var wand=equipment.Rules.Items.Last();
            Window("Inventory");yield return null;
            Assert.That(BagIds(),Is.EqualTo(new[]{wand.Id}));
            Assert.That(Call(binding,"TryEquipItem",wand.Id),Is.True);
            Assert.That(BagIds(),Is.Empty,"Both weapons are equipped in distinct slots.");
            var viewAfterEquip=Read<object>(hud,"EquipmentView");
            Assert.That(((int[])viewAfterEquip.GetType().GetField("EquippedItemIds").GetValue(viewAfterEquip))[7],Is.EqualTo(wand.Id));
            Assert.That(Call(binding,"TryUnequipSlot",7),Is.True);
            Assert.That(BagIds(),Is.EqualTo(new[]{wand.Id}));
            Assert.That(Call(binding,"TryEquipItem",bow.Id),Is.True);
            Assert.That(BagIds(),Is.EqualTo(new[]{wand.Id}));
            Assert.That(Owned().Length,Is.EqualTo(4));Assert.That(Owned().Select(i=>i.Id).Distinct().Count(),Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator ReforgedPickaxeChangesRealHarvestAndUnequippedToolCannotGather()
        {
            yield return BuildAndApproach();Call(forge,"TryOpen");Call(binding,"SelectForgeType",6);
            var mode=Read<object>(hud,"ForgeView").GetType().GetField("Mode").FieldType;
            Call(binding,"SelectForgeMode",Enum.Parse(mode,"Reforge"));
            Assert.That(Call(binding,"TryForgeOperation"),Is.True);Assert.That(Call(binding,"TryForgeOperation"),Is.True);
            var pickaxe=equipment.Rules.Equipped(Stage2ItemSlot.Pickaxe);Assert.That(pickaxe.Quality,Is.EqualTo(Stage2ItemQuality.D));
            Assert.That(Call(binding,"TryForgeOperation"),Is.False);
            var economy=game.Gathering.Economy;var node=economy.Nodes.First(n=>n.Resource==MapResourceKind.Metal&&n.Size==Stage2ResourceSize.Medium);
            var before=economy.Inventory.Get(MapResourceKind.Metal);var remain=node.Remaining;
            Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,economy.GatherDuration(node),true),Is.True);
            Assert.That(economy.Inventory.Get(MapResourceKind.Metal)-before,Is.EqualTo(2));Assert.That(remain-node.Remaining,Is.EqualTo(2));
            Window("Inventory");yield return null;Assert.That(Call(binding,"TryUnequipSlot",5),Is.True);
            Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,10,true),Is.False);
            Assert.That(Call(binding,"TryEquipItem",pickaxe.Id),Is.True);
            Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,economy.GatherDuration(node),true),Is.True);
        }
        [UnityTest] public IEnumerator DefeatKeepsForgeCurrentItemsAndInvestmentButFullResetClearsThem()
        {
            yield return BuildAndApproach();Call(forge,"TryOpen");Call(binding,"SelectForgeType",1);Call(binding,"TryForgeOperation");
            var item=equipment.Rules.Items.Last();Window("Inventory");Call(binding,"TryEquipItem",item.Id);Window("None");
            var run=game.RunDriver.Run;Assert.That(Stage3TestCycle.StartNight(run),Is.True);
            equipment.Rules.TryLevel(item.Id);var gold=game.Gathering.Economy.Inventory.Gold;var level=item.Level;
            var position=Read<Vector2>(forge,"Position");var rectangles=scene.PlayerMover.Obstacles.Rectangles.Count;
            for(var i=0;i<10&&!run.IsTerminal;i++){var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Lost));Assert.That(run.RetryFailedWave(),Is.True);
            Assert.That(Read<bool>(forge,"IsBuilt"),Is.True);Assert.That(Read<Vector2>(forge,"Position"),Is.EqualTo(position));
            Assert.That(equipment.Rules.Find(item.Id),Is.SameAs(item));Assert.That(item.Level,Is.EqualTo(level));
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold));Assert.That(scene.PlayerMover.Obstacles.Rectangles.Count,Is.EqualTo(rectangles));
            game.RunDriver.ResetRun();yield return null;
            Assert.That(Read<bool>(forge,"IsBuilt"),Is.False);Assert.That(equipment.Rules.ForgeLevel,Is.Zero);
            Assert.That(equipment.Rules.Items.Count,Is.EqualTo(3));Assert.That(equipment.Rules.Find(item.Id),Is.Null);
        }
        [UnityTest] public IEnumerator RemotePauseModalAndDebugCannotUseForgeOrSpendResources()
        {
            yield return BuildAndApproach();var origin=scene.PlayerMover.transform.position;
            scene.PlayerMover.transform.position+=Vector3.right*15;Assert.That(Call(forge,"TryOpen"),Is.False);
            Window("Forge");yield return null;var count=equipment.Rules.Items.Count;
            Assert.That(Call(binding,"TryForgeOperation"),Is.False);Assert.That(equipment.Rules.Items.Count,Is.EqualTo(count));
            Window("None");scene.PlayerMover.transform.position=origin;game.SetPaused(true);Assert.That(Call(forge,"TryOpen"),Is.False);game.SetPaused(false);
            Window("Inventory");Assert.That(Call(forge,"TryOpen"),Is.False);Window("None");
            game.GetComponent<Stage2GameHud>().SetDebugOpen(true);Assert.That(Call(forge,"TryOpen"),Is.False);game.GetComponent<Stage2GameHud>().SetDebugOpen(false);
            Assert.That(Call(forge,"TryOpen"),Is.True);
        }
    }
}
#endif
