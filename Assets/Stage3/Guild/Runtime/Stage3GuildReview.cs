using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in disposable native capture fixture. Uses actual contracts, not injected view data.</summary>
    public static class Stage3GuildReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<20;i++)yield return null;
            var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();var travel=owner.GetComponent<Stage3WorldTravel>();
            var world=binding.GuildWorld;var guild=binding.Guild;var frames=new List<string>();
            Require(world!=null&&world.Ready&&guild!=null,"guild bound");
            game.RunDriver.enabled=false;hero.AutomaticTick=false;game.HeroCombat.AutomaticTick=false;world.AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);owner.GetComponent<Stage2GameHud>().SetDebugOpen(false);
            yield return null;
            var board=travel.CityPresentation.GuildInteractionPosition;
            void AtBoard(){hud.CloseWindow();Move(scene,Clear(hero,board+Vector2.down));Require(binding.TryOpenGuild(),"open board");}
            void ClearOrder(int id)
            {
                var contract=guild.FindActive(id);Require(contract!=null,"active order");
                foreach(var unit in world.Enemies.Where(u=>u.Generation==contract.Generation&&u.Profile.enemyKind==contract.Quest.enemyKind).ToArray())
                    Require(world.Damage(unit,unit.Health),"confirmed kill");
            }
            void Complete(int id)
            {
                AtBoard();hud.SelectGuildQuest(id);Require(binding.TryGuildAction(id),"accept");ClearOrder(id);
                AtBoard();hud.SelectGuildQuest(id);Require(binding.TryGuildAction(id),"turn in");
            }
            Complete(1);Complete(1);Require(guild.CompletedForRank==2,"F2/3");
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();Move(scene,Clear(hero,travel.CityOrigin+Vector2.down*10));
                scene.CameraFollow.ConfigureTacticalView(27);scene.CameraFollow.SnapToTarget();
                yield return Shot(directory,size+"-city",frames);
                scene.CameraFollow.ConfigureTacticalView(6);AtBoard();yield return Shot(directory,size+"-F-board",frames);
                Move(scene,Clear(hero,board+Vector2.down*10));yield return Shot(directory,size+"-remote-disabled",frames);
            }
            Complete(1);Require(guild.CompletedForRank==3&&guild.Rank==Stage2GuildRank.F,"3/3 does not promote");
            foreach(var size in sizes)
            {
                yield return Resize(size);AtBoard();hud.SelectGuildQuest(13);yield return Shot(directory,size+"-trial-available",frames);
                var scroll=typeof(Stage3FantasyHud).GetField("guildScroll",BindingFlags.Instance|BindingFlags.NonPublic);
                scroll.SetValue(hud,new Vector2(0,10000));yield return Shot(directory,size+"-third-paper-scroll",frames);
                scroll.SetValue(hud,Vector2.zero);
            }
            Require(binding.TryGuildAction(13),"trial accepted");ClearOrder(13);
            foreach(var size in sizes)
            {yield return Resize(size);AtBoard();hud.SelectGuildQuest(13);yield return Shot(directory,size+"-trial-turn-in",frames);}
            Require(binding.TryGuildAction(13),"trial turnin");Require(guild.Rank==Stage2GuildRank.E,"promoted to E");
            foreach(var size in sizes)
            {yield return Resize(size);AtBoard();hud.SelectGuildRank(1);yield return Shot(directory,size+"-E-unlocked",frames);}
            // Accept one order of each type. Their real roots/poses are reviewed beside the same unchanged hero.
            AtBoard();hud.SelectGuildQuest(1); // F is still accessible, but the initial E board is selected.
            hud.SelectGuildRank(0);hud.SelectGuildQuest(1);Require(binding.TryGuildAction(1),"orc review acceptance");
            hud.SelectGuildQuest(2);Require(binding.TryGuildAction(2),"ogre review acceptance");hud.CloseWindow();
            foreach(var kind in new[]{Stage2GuildEnemyKind.Orc,Stage2GuildEnemyKind.Ogre})
            {
                var unit=world.Enemies.First(u=>u.Alive&&u.Profile.enemyKind==kind);
                var visual=owner.GetComponentsInChildren<Stage3GuildEnemyVisual>().First(v=>v.name==kind+" exploration "+unit.Id);
                foreach(var size in sizes)
                {
                    yield return Resize(size);Move(scene,Clear(hero,unit.Position+Vector2.down*1.7f));
                    scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
                    visual.SetState(Vector2.zero,Vector2.down,false,false);yield return Shot(directory,size+"-"+kind+"-front",frames);
                    visual.SetState(Vector2.zero,Vector2.up,false,false);yield return Shot(directory,size+"-"+kind+"-rear",frames);
                    visual.SetState(Vector2.zero,Vector2.down,true,false);yield return Shot(directory,size+"-"+kind+"-strike",frames);
                    visual.SetState(Vector2.zero,Vector2.down,false,false);
                }
                if(kind==Stage2GuildEnemyKind.Orc)
                {
                    yield return Resize(new Vector2Int(1600,900));Move(scene,Clear(hero,unit.Position+Vector2.down*3));
                    world.AutomaticTick=true;
                    Require(world.LaunchNearest(scene.PlayerMover.transform.position,8,1,0,.8f),"real world bow launch");
                    yield return new WaitForSeconds(.32f);
                    yield return Shot(directory,"world-bow-in-flight",frames);world.AutomaticTick=false;
                }
            }
            yield return Resize(new Vector2Int(1600,900));hud.SetWindow(Stage3PlayerWindow.Map);
            yield return Shot(directory,"map-world-spots",frames);hud.CloseWindow();
            ClearOrder(1);AtBoard();hud.SelectGuildRank(0);hud.SelectGuildQuest(1);
            yield return Shot(directory,"input-ready-turn-in",frames);
            var action=hud.GuildActionRect;
            hud.CloseWindow();scene.CameraFollow.ConfigureTacticalView(9);scene.CameraFollow.SnapToTarget();
            var click=scene.CameraFollow.BoundCamera.WorldToScreenPoint(travel.CityPresentation.GuildVisual.bounds.center);
            File.WriteAllText(Path.Combine(directory,"input-ready.txt"),$"Real mouse fixture: click GUILD BUILDING bottom-left ({click.x},{click.y}); world notice board is decoration only. Then F board, first order, claim action top-left rect {action}. Expected gold before={game.Gathering.Economy.Inventory.Gold}; claim adds50 once. Current rank E stays E; old F work does not count toward E.\n");
            File.WriteAllText(Path.Combine(directory,"frames.txt"),string.Join("\n",frames));
            File.WriteAllText(Path.Combine(directory,"qualification.txt"),
                "Actual Mac development-player native captures. Fixture repositions hero, freezes clock/bow/AI and resolves kills through real world Damage. No DTO injection or ordinary-start grants. F repeat turn-ins/3/3/trial/claim/E boards use real rules. Front/rear/strike SetState and programmatic paper-list scroll are explicit appearance fixtures. Actual input qualification is recorded separately. Ordinary restart removes all fixture state.\n");
            Debug.Log("S3_GUILD_NATIVE_REVIEW_READY "+directory);
        }
        private static Vector2 Clear(Stage2HeroProgressionDriver hero,Vector2 p)
        {
            if(hero.CanLand(p))return p;
            for(var ring=.25f;ring<6;ring+=.25f)for(var d=0;d<32;d++)
            {var a=d*Mathf.PI/16;var q=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*ring;if(hero.CanLand(q))return q;}
            throw new InvalidOperationException("No guild review footing.");
        }
        private static void Move(Stage2SceneBootstrap scene,Vector2 p)
        {Require(scene.PlayerMover.TryTeleportTo(p),"legal fixture move");scene.CameraFollow.SnapToTarget();}
        private static void Require(bool value,string text){if(!value)throw new InvalidOperationException("Guild QA: "+text);}
        private static IEnumerator Resize(Vector2Int s){Screen.SetResolution(s.x,s.y,false);yield return new WaitForSecondsRealtime(.3f);}
        private static IEnumerator Shot(string dir,string name,List<string> frames)
        {
            var hud=UnityEngine.Object.FindAnyObjectByType<Stage3FantasyHud>();
            hud.GetComponent<Stage3HeroLevelUpPresentation>()?.ClearEffects();hud.GetComponent<Stage3HeroProgressionEffects>()?.ClearEffects();hud.ClearExperienceFeedback();
            for(var i=0;i<4;i++)yield return null;yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));frames.Add(name);yield return null;
        }
    }
}
