#if UNITY_EDITOR
using System;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3EquipmentRulesTests
    {
        private Stage2EquipmentCatalog catalog;
        private Stage2Inventory wallet;
        private Stage2Equipment rules;
        [SetUp] public void Setup()
        {
            catalog = ScriptableObject.CreateInstance<Stage2EquipmentCatalog>(); wallet = new Stage2Inventory();
            foreach (MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind))) wallet.TryAdd(kind,100000);
            wallet.TryAddGold(100000); rules = new Stage2Equipment(catalog,wallet);
        }
        [TearDown] public void Cleanup() => Object.DestroyImmediate(catalog);
        private int[] Stock() => new[] {wallet.Get(MapResourceKind.Wood),wallet.Get(MapResourceKind.Stone),wallet.Get(MapResourceKind.Metal),wallet.Gold};
        [Test] public void FreshEquipmentHasThreeActualStarterToolsAndNoArmorOrShieldType()
        {
            Assert.That(catalog.TryValidate(out var reason),Is.True,reason);
            Assert.That(Enum.GetValues(typeof(Stage2ItemType)).Length,Is.EqualTo(8));
            Assert.That(Enum.GetValues(typeof(Stage2ItemSlot)).Length,Is.EqualTo(7));
            Assert.That(rules.Items.Select(i=>i.Type),Is.EquivalentTo(new[]{Stage2ItemType.Bow,Stage2ItemType.Pickaxe,Stage2ItemType.Axe}));
            Assert.That(rules.Items.All(i=>i.Level==1&&i.Quality==Stage2ItemQuality.F&&rules.IsEquipped(i)),Is.True);
            Assert.That(rules.ForgeLevel,Is.Zero); Assert.That(rules.ForgeUpgradeScrolls,Is.Zero);
        }
        [Test] public void CraftIsPaidOnceAndReentrantWalletListenerCannotBuyAnotherItem()
        {
            var stock=Stock(); Assert.That(rules.TryCraft(Stage2ItemType.Wand,out _),Is.EqualTo(Stage2EquipmentResult.ForgeRequired));
            Assert.That(Stock(),Is.EqualTo(stock)); rules.ActivateForge();
            var nested=Stage2EquipmentResult.Ready; wallet.Changed+=()=>nested=rules.TryCraft(Stage2ItemType.Wand,out _);
            Assert.That(rules.TryCraft(Stage2ItemType.Wand,out var item),Is.EqualTo(Stage2EquipmentResult.Ready));
            Assert.That(nested,Is.EqualTo(Stage2EquipmentResult.Busy)); Assert.That(rules.Items.Count,Is.EqualTo(4));
            var cost=catalog.CraftCost(Stage2ItemType.Wand);
            Assert.That(Stock(),Is.EqualTo(new[]{stock[0]-cost.Wood,stock[1]-cost.Stone,stock[2]-cost.Metal,stock[3]-cost.Gold}));
            Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.F)); Assert.That(item.Level,Is.EqualTo(1));
        }
        [Test] public void LevelAndReforgePreserveInstanceAndKeepTheirTwoAxesIndependent()
        {
            rules.ActivateForge(); var item=rules.Items[0]; var id=item.Id;
            Assert.That(rules.TryLevel(id),Is.EqualTo(Stage2EquipmentResult.Ready)); var level=item.Level;
            Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.F));
            Assert.That(rules.TryReforge(id),Is.EqualTo(Stage2EquipmentResult.Ready));
            Assert.That(item.Id,Is.EqualTo(id)); Assert.That(rules.Find(id),Is.SameAs(item));
            Assert.That(item.Level,Is.EqualTo(level)); Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.E));
            Assert.That(rules.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(item));
        }
        [Test] public void ForgeHasThreeScrollOnlyStagesAndNeverPaysResourcesOrGoldForThem()
        {
            rules.ActivateForge(); var stock=Stock();
            Assert.That(rules.MaximumQuality,Is.EqualTo(Stage2ItemQuality.D));
            Assert.That(rules.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.UpgradeScrollRequired));
            rules.TryReceiveForgeScrolls(2);
            Assert.That(rules.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.Ready)); Assert.That(rules.MaximumQuality,Is.EqualTo(Stage2ItemQuality.B));
            Assert.That(rules.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.Ready)); Assert.That(rules.MaximumQuality,Is.EqualTo(Stage2ItemQuality.S));
            Assert.That(rules.ForgeLevel,Is.EqualTo(3)); Assert.That(rules.ForgeUpgradeScrolls,Is.Zero);
            Assert.That(rules.TryUpgradeForge(),Is.EqualTo(Stage2EquipmentResult.MaximumForgeStage)); Assert.That(Stock(),Is.EqualTo(stock));
        }
        [Test] public void SequentialQualityCapsRejectSkippedStagesAndSSSCannotBeForged()
        {
            rules.ActivateForge(); var item=rules.Items[0];
            rules.TryReforge(item.Id); rules.TryReforge(item.Id); Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.D));
            var stock=Stock(); Assert.That(rules.TryReforge(item.Id),Is.EqualTo(Stage2EquipmentResult.ForgeQualityLimit)); Assert.That(Stock(),Is.EqualTo(stock));
            rules.TryReceiveForgeScrolls(2); rules.TryUpgradeForge(); rules.TryReforge(item.Id); rules.TryReforge(item.Id);
            Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.B));
            rules.TryUpgradeForge(); rules.TryReforge(item.Id); rules.TryReforge(item.Id); Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.S));
            stock=Stock(); Assert.That(rules.TryReforge(item.Id),Is.EqualTo(Stage2EquipmentResult.DropOnlyQuality)); Assert.That(Stock(),Is.EqualTo(stock));
            Assert.That(rules.Items.Any(i=>i.Quality==Stage2ItemQuality.SSS),Is.False);
        }
        [Test] public void LootCanReceiveSSSWithoutMakingItCraftableAndInvalidItemsDoNotEnterBag()
        {
            Assert.That(rules.TryReceiveLoot(Stage2ItemType.Bow,Stage2ItemQuality.SSS,100,out var item),Is.True);
            Assert.That(rules.TryEquip(item.Id),Is.EqualTo(Stage2EquipmentResult.Ready)); rules.ActivateForge();
            Assert.That(rules.TryReforge(item.Id),Is.EqualTo(Stage2EquipmentResult.DropOnlyQuality));
            Assert.That(rules.TryReceiveLoot((Stage2ItemType)8,Stage2ItemQuality.F,1,out _),Is.False);
            Assert.That(rules.TryReceiveLoot(Stage2ItemType.Bow,(Stage2ItemQuality)8,1,out _),Is.False);
            Assert.That(rules.TryReceiveLoot(Stage2ItemType.Bow,Stage2ItemQuality.F,101,out _),Is.False);
            Assert.That(rules.Items.Count,Is.EqualTo(4));
        }
        [Test] public void LevelsReach100WithoutChangingQualityAndMaxLevelDoesNotSpend()
        {
            rules.ActivateForge(); var item=rules.Items[0];
            for(var i=1;i<100;i++) Assert.That(rules.TryLevel(item.Id),Is.EqualTo(Stage2EquipmentResult.Ready));
            var stock=Stock(); Assert.That(item.Level,Is.EqualTo(100)); Assert.That(item.Quality,Is.EqualTo(Stage2ItemQuality.F));
            Assert.That(rules.TryLevel(item.Id),Is.EqualTo(Stage2EquipmentResult.MaximumLevel)); Assert.That(Stock(),Is.EqualTo(stock));
        }
        [Test] public void BowAndWandShareOneSlotAndUnequipKeepsBothOwned()
        {
            rules.ActivateForge(); var bow=rules.Items[0]; rules.TryCraft(Stage2ItemType.Wand,out var wand);
            rules.TryEquip(wand.Id); Assert.That(rules.Equipped(Stage2ItemSlot.Weapon),Is.SameAs(wand)); Assert.That(rules.IsEquipped(bow),Is.False);
            rules.TryUnequip(Stage2ItemSlot.Weapon); Assert.That(rules.Equipped(Stage2ItemSlot.Weapon),Is.Null);
            Assert.That(rules.Find(wand.Id),Is.SameAs(wand)); Assert.That(rules.Find(bow.Id),Is.SameAs(bow));
        }
        [Test] public void FullResetRemovesForgeLootAndScrollsButDoesNotRecycleOldItemIds()
        {
            var id=rules.Items[0].Id; rules.ActivateForge(); rules.TryReceiveForgeScrolls(2); rules.TryUpgradeForge(); rules.TryCraft(Stage2ItemType.Wand,out _);
            rules.Reset(); Assert.That(rules.ForgeLevel,Is.Zero); Assert.That(rules.ForgeUpgradeScrolls,Is.Zero);
            Assert.That(rules.Items.Count,Is.EqualTo(3)); Assert.That(rules.Find(id),Is.Null);
            Assert.That(rules.Items.All(i=>i.Level==1&&i.Quality==Stage2ItemQuality.F),Is.True);
        }
        [Test] public void WeaponProfileCombinesPassivesWithoutSplashOrSwapCooldownReset()
        {
            var data=ScriptableObject.CreateInstance<Stage2HeroCombatCatalog>();
            try
            {
                var damage=0;var range=0f;var splash=-1f;
                var combat=new Stage2HeroCombat(data,(origin,r,d,s,f)=>{damage=d;range=r;splash=s;return true;});
                combat.SetPersonalBonuses(3,1); combat.SetEquippedWeapon(true,10,4,1,.18f); combat.Tick(.01f,Vector2.zero,true);
                Assert.That(damage,Is.EqualTo(13)); Assert.That(range,Is.EqualTo(4)); Assert.That(splash,Is.Zero);
                combat.SetEquippedWeapon(false,0,0,1,0); combat.SetEquippedWeapon(true,20,5,1,.18f);
                combat.Tick(.01f,Vector2.zero,true); Assert.That(combat.ShotsFired,Is.EqualTo(1));
                combat.Tick(1,Vector2.zero,true); Assert.That(combat.ShotsFired,Is.EqualTo(2)); Assert.That(damage,Is.EqualTo(23));
                combat.SetEquippedWeapon(false,0,0,1,0);combat.Tick(10,Vector2.zero,true);Assert.That(combat.ShotsFired,Is.EqualTo(2));
            }
            finally{Object.DestroyImmediate(data);}
        }
        [Test] public void StrongToolsGrantOnlyRemainingStockAndOneStrikeEventWithCorrectTiming()
        {
            var map=AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            var data=ScriptableObject.CreateInstance<Stage2EconomyCatalog>();
            try
            {
                data.Configure(2,new[]{new Stage2ResourceSettings(MapResourceKind.Wood,5,1,30),new Stage2ResourceSettings(MapResourceKind.Stone,5,1,30),new Stage2ResourceSettings(MapResourceKind.Metal,5,1,30)});
                var economy=new Stage2Economy(map,data);var node=economy.GetNode(MapResourceKind.Wood);var strikes=0;var grant=0;
                economy.NodeGathered+=n=>strikes++;economy.Gathered+=(k,n)=>grant+=n;
                economy.SetManualToolStats(2,3,2,1);
                Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,.49f,true),Is.False);
                Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,.02f,true),Is.True);Assert.That(node.Remaining,Is.EqualTo(3));
                economy.SetManualToolStats(4,3,1,1); // Tool change discards the unfinished prior stroke.
                Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,.99f,true),Is.False);
                Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,.02f,true),Is.True);
                Assert.That(node.Remaining,Is.Zero);Assert.That(grant,Is.EqualTo(5));Assert.That(strikes,Is.EqualTo(2));
                var metal=economy.GetNode(MapResourceKind.Metal);economy.Inventory.TryAdd(MapResourceKind.Metal,int.MaxValue-1);
                Assert.That(economy.AdvanceGather(metal,metal.WorldPosition,true,1,true),Is.True);
                Assert.That(metal.Remaining,Is.EqualTo(4));Assert.That(economy.Inventory.Get(MapResourceKind.Metal),Is.EqualTo(int.MaxValue));
                Assert.That(economy.AdvanceGather(metal,metal.WorldPosition,true,1,true),Is.False);Assert.That(metal.Remaining,Is.EqualTo(4));
            }
            finally{Object.DestroyImmediate(data);}
        }
    }
}
#endif
