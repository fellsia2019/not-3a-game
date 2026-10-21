using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit opt-in disposable development-player fixture. Never runs in a normal game.</summary>
    public static class Stage3EquipmentForgeReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while(binding.Equipment==null||!hud.IsReady)yield return null;
            for(var i=0;i<12;i++)yield return null;
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;
            owner.GetComponent<Stage2HeroProgressionDriver>().AutomaticTick=false;
            game.RunDriver.ResetRun();var rules=binding.Equipment.Rules;var forge=binding.Forge;
            File.WriteAllText(Path.Combine(directory,"verification.txt"),
                $"Actual development player {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceType}.\n"+
                "Disposable QA only: automatic clock/combat disabled;3000 of each resource/gold granted; actual paid forge placed at legal site and hero positioned at entrance. Native E and mouse actions awaited. Not a natural playthrough.\n");
            Require(rules.Items.Count==3&&rules.ForgeLevel==0&&rules.ForgeUpgradeScrolls==0,"normal starter state");
            hud.SetWindow(Stage3PlayerWindow.Inventory);yield return Shot(directory,"01-starting-inventory");hud.SetWindow(Stage3PlayerWindow.None);
            foreach(MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind)))game.Gathering.Economy.Inventory.TryAdd(kind,3000);
            game.Gathering.Economy.Inventory.TryAddGold(3000);
            if(rules.Blueprints!=null)
            {
                // Explicit opt-in recipe prerequisite for this older forge-only input fixture.
                var recipe=binding.Merchant.BlueprintItems.First(p=>p.Kind==Stage2BlueprintKind.WandRecipe);
                Require(binding.Merchant.TryTradeBlueprint(true,recipe.Id)==Stage2TradeResult.Ready&&
                    rules.Blueprints.TryStudy(recipe.Id)==Stage2BlueprintResult.Ready,"QA wand recipe prerequisite");
                File.AppendAllText(Path.Combine(directory,"verification.txt"),"QA additionally buys/studies the wand recipe through ownership rules before the forge-only input scenario.\n");
            }
            forge.BeginPlacement();var origin=(Vector2)scene.PlayerMover.transform.position;var built=false;
            for(var ring=5;ring<45&&!built;ring+=2)
            for(var x=-ring;x<=ring&&!built;x+=2)for(var y=-ring;y<=ring&&!built;y+=2)
            {var point=origin+new Vector2(x,y);if(forge.EvaluateAt(point)==Stage3ForgePlacementResult.Ready)built=forge.TryBuildAt(point)==Stage3ForgePlacementResult.Built;}
            Require(built,"paid legal forge");
            scene.PlayerMover.transform.position=forge.InteractionPosition;scene.CameraFollow.SnapToTarget();
            yield return Shot(directory,"02-world-forge");
            Debug.Log("S3_EQUIPMENT_NATIVE_READY_E");
            while(hud.ActiveWindow!=Stage3PlayerWindow.Forge)yield return null;
            yield return Shot(directory,"03-forge-create");
            Controls(directory,hud);Debug.Log("S3_EQUIPMENT_NATIVE_READY_CRAFT_WAND");
            while(rules.Items.Count==3)yield return null;
            var wand=rules.Items.Last();Require(wand.Type==Stage2ItemType.Wand&&wand.Level==1&&wand.Quality==Stage2ItemQuality.F,"native wand craft");
            Require(rules.Equipped(Stage2ItemSlot.Weapon).Type==Stage2ItemType.Bow,"craft does not auto-equip");
            yield return Shot(directory,"04-native-crafted-wand");Debug.Log("S3_EQUIPMENT_NATIVE_READY_LEVEL");
            while(wand.Level==1)yield return null;Require(wand.Level==2,"native numeric level");
            yield return Shot(directory,"05-native-level-2");Debug.Log("S3_EQUIPMENT_NATIVE_READY_REFORGE");
            while(wand.Quality==Stage2ItemQuality.F)yield return null;
            Require(wand.Quality==Stage2ItemQuality.E&&wand.Level==2,"native reforge retains numeric level");
            yield return Shot(directory,"06-native-quality-E");Debug.Log("S3_EQUIPMENT_NATIVE_READY_EQUIP");
            while(rules.Equipped(Stage2ItemSlot.Weapon)!=wand)yield return null;
            Require(hud.ActiveWindow==Stage3PlayerWindow.Inventory&&game.HeroCombat.Rules.EffectiveRange==binding.Equipment.Catalog.Stats(wand.Type,wand.Quality,wand.Level).Range,"native equip updates combat profile");
            yield return Shot(directory,"07-native-wand-equipped");
            while(hud.ActiveWindow!=Stage3PlayerWindow.None)yield return null;
            yield return Shot(directory,"08-world-equipped-wand");
            var visual=owner.GetComponent<Stage3ForgePresentation>().BuiltVisual;
            Require(binding.TrySelectBuildingAtScreen(scene.CameraFollow.BoundCamera.WorldToScreenPoint(visual.bounds.center)),"world forge selected");
            Require(!forge.CanUpgradeBuilding&&rules.ForgeUpgradeScrolls==0,"scroll-less upgrade blocked");
            yield return Shot(directory,"09-scroll-only-building-card");
            File.AppendAllText(Path.Combine(directory,"verification.txt"),
                "PASS native E opens forge; native type/craft creates F1 wand without replacing bow; native level F2; native reforge E2; native inventory select/equip updates actual weapon; no scroll means no building upgrade.\n"+
                "Following layout review uses public window actions and declared QA loot grants: four D1 armor items plus S100 bow and SSS100 wand. No gameplay drop source is enabled.\n");
            foreach(var type in new[]{Stage2ItemType.Helmet,Stage2ItemType.Chest,Stage2ItemType.Arms,Stage2ItemType.Legs})
            {rules.TryReceiveLoot(type,Stage2ItemQuality.D,1,out var armor);rules.TryEquip(armor.Id);}
            rules.TryReceiveLoot(Stage2ItemType.Bow,Stage2ItemQuality.S,100,out _);
            rules.TryReceiveLoot(Stage2ItemType.Wand,Stage2ItemQuality.SSS,100,out _);
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);for(var i=0;i<20;i++)yield return null;
                var prefix=$"{Screen.width}x{Screen.height}-";
                hud.SetWindow(Stage3PlayerWindow.Inventory);yield return Shot(directory,prefix+"inventory");
                hud.SetWindow(Stage3PlayerWindow.None);forge.TryOpen();yield return Shot(directory,prefix+"forge-reforge");
                hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(2);yield return Shot(directory,prefix+"construction-base");
                hud.SetWindow(Stage3PlayerWindow.None);
            }
            Screen.SetResolution(1600,900,FullScreenMode.Windowed);
            Debug.Log("S3_EQUIPMENT_NATIVE_REVIEW_OK "+directory);
        }
        private static void Controls(string directory,Stage3FantasyHud hud)
        {
            File.WriteAllText(Path.Combine(directory,"controls.txt"),
                $"GUI top-left rects; backbuffer={Screen.width}x{Screen.height}\nWand={hud.ForgeTypeRect(1)}\n"+
                $"Level={hud.ForgeModeRect(1)}\nReforge={hud.ForgeModeRect(2)}\nExecute={hud.ForgeExecuteRect}\n"+
                $"Bag wand(index3)={hud.EquipmentItemRect(3)}\nEquip={hud.EquipmentActionRect}\n");
        }
        private static void Require(bool value,string reason)
        {if(!value)throw new InvalidOperationException("Equipment QA: "+reason);}
        private static IEnumerator Shot(string directory,string name)
        {
            yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return null;
        }
    }
}
