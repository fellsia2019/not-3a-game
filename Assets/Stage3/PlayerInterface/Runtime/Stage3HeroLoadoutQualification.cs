using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit disposable development QA only. Ordinary startup never calls this fixture.</summary>
    public static class Stage3HeroLoadoutQualification
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();var game=owner.GetComponent<Stage2GameComposition>();
            var deadline=Time.realtimeSinceStartup+45;
            while(binding==null||binding.HeroSkills==null){if(Time.realtimeSinceStartup>deadline)throw new InvalidOperationException("Loadout binding timeout.");yield return null;binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();}
            var scene=owner.GetComponent<Stage2SceneBootstrap>();var skills=binding.HeroSkills;
            var hero=binding.HeroProgression;var gear=binding.Equipment;var guild=owner.GetComponent<Stage3GuildWorldDriver>();
            game.RunDriver.enabled=false;game.RunDriver.ResetRun();game.SetPaused(false);
            game.HeroCombat.AutomaticTick=false;hero.AutomaticTick=false;skills.AutomaticTick=false;
            if(guild!=null)guild.AutomaticTick=false;
            var camera=scene.CameraFollow.BoundCamera;
            scene.CameraFollow.ConfigureTacticalView(5);
            hero.Rules.AwardExperience(int.MaxValue);
            for(var i=1;i<34;i++)
            {
                // IDs sort by dependency, but common passives follow actives by design.
                if(skills.Rules.CanUpgrade(i,out _))skills.Rules.TryUpgrade(i);
            }
            if(skills.Rules.Rank(14)==0)skills.Rules.TryUpgrade(14);
            foreach(var id in new[]{9,10})while(skills.Rules.CanUpgrade(id,out _))skills.Rules.TryUpgrade(id);
            foreach(var type in new[]{Stage2ItemType.Wand,Stage2ItemType.Amulet,Stage2ItemType.Ring,Stage2ItemType.Ring,Stage2ItemType.Protection})
            {gear.Rules.TryReceiveLoot(type,Stage2ItemQuality.D,12,out var item);gear.Rules.TryEquip(item.Id);}
            game.Gathering.Economy.Inventory.TryAddGold(100000);
            foreach(var type in new[]{Stage2ItemType.HealthPotion,Stage2ItemType.ManaPotion,Stage2ItemType.MixedPotion})
            {
                var item=binding.Merchant.Items.First(i=>i.Type==type);
                for(var n=0;n<3;n++)Require(binding.Merchant.TryTrade(true,item.Id)==Stage2TradeResult.Ready,"real one-bottle purchase");
            }
            yield return null;yield return null;
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"fixture.txt"),"DISPOSABLE OPT-IN QA: level100 through actual XP, ranks purchased with99points, D12gear through declared TryReceiveLoot grants,100000QA gold and3actual purchases per potion. Clock/automaticcombat/regen held for captures; no ordinary profile gets these grants. All native frames use actual bound rules. Reset after capture.\n");
            yield return Stage3HeroLoadoutReview.Capture(owner,directory);
            // Functional checks on the same native player, with actual portal population and hit feedback.
            var origin=(Vector2)scene.PlayerMover.transform.position;var point=origin+Vector2.up*2;
            for(var i=0;i<12;i++)
            {
                var enemy=game.Enemies.Spawn();enemy.SetAutomaticTick(false);
                enemy.transform.position=point+new Vector2((i%4-1.5f)*.15f,(i/4-1)*.15f);
            }
            if(skills.ActiveWeapon!=1)gear.Rules.TrySwitchWeapon();
            hero.Rules.RestoreVitals(0,hero.Rules.MaximumMana);
            var before=game.Enemies.ActiveCount;var mana=hero.Rules.Mana;
            Require(skills.TryCast(9,point),"actual fireball");
            Require(skills.LastTargetCount==4,"bounded four portal targets");
            Require(game.Enemies.ActiveCount>=before-4,"dense horde not cleared");
            Require(hero.Rules.Mana<mana&&skills.Rules.Cooldown(9)>=15,"real spend and floor");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"portal-fireball.png"));
            yield return new WaitForSecondsRealtime(.15f);
            foreach(var enemy in Enumerable.Range(0,game.Enemies.ActiveCount).Select(game.Enemies.GetEnemy).ToArray())enemy.Despawn();
            if(guild!=null&&guild.Enemies.Count>0)
            {
                var unit=guild.Enemies.First(e=>e.Alive);
                // Keep the hit target beside the camera centre, clear of the top clock.
                scene.PlayerMover.transform.position=unit.Position+Vector2.left*2.5f;scene.CameraFollow.SnapToTarget();
                hero.Rules.RestoreVitals(0,hero.Rules.MaximumMana);
                var health=unit.Health;
                Require(skills.TryCast(6,unit.Position),"actual staff hit on neutral");
                Require(unit.Health<health,"neutral damage");
                yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"neutral-bolt.png"));
            }
            File.WriteAllText(Path.Combine(directory,"functional.txt"),"PASS native actual fireball hits4of12portal targets, preserveslivinghorde,spendsMP/CD; actualstaffsinglehitneutralwhenpresent. Earlier rule/PlayModequalification coversinvalid/weapon/potion/death contracts.\n");
            game.RunDriver.ResetRun();skills.AutomaticTick=hero.AutomaticTick=game.HeroCombat.AutomaticTick=true;
            if(guild!=null)guild.AutomaticTick=true;game.RunDriver.enabled=true;
            owner.GetComponent<Stage3FantasyHud>().CloseWindow();
            Debug.Log("S3_HERO_LOADOUT_QUALIFICATION_READY");
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    }
}
