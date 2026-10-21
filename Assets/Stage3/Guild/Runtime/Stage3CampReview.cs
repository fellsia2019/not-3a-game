using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Disposable opt-in native camp/city appearance fixture; ordinary startup never executes this.</summary>
    public static class Stage3CampReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<24;i++)yield return null;
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();var world=owner.GetComponent<Stage3GuildWorldDriver>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var frames=new List<string>();
            if(world==null||!world.Ready)throw new InvalidOperationException("Native camp environment is not ready.");
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;hero.AutomaticTick=false;world.AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);hud.CloseWindow();yield return null;
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
                var prefix=size.x+"x"+size.y;
                for(var kind=0;kind<2;kind++)
                {
                    var center=world.SpotPosition(kind);var name=kind==0?"orc":"ogre";
                    Move(scene,hero,center+Vector2.down*6);
                    Center(scene,center+Vector2.up*3,18);
                    yield return Shot(directory,prefix+"-"+name+"-overview",frames);
                    var side=kind==0?1:-1;
                    Move(scene,hero,center+new Vector2(side*8,8));
                    Center(scene,center+new Vector2(side*12,12),8);
                    yield return Shot(directory,prefix+"-"+name+"-props",frames);
                }
                var city=travel.CityOrigin;
                Move(scene,hero,city+Vector2.down*9);Center(scene,city+new Vector2(0,-1),19);
                yield return Shot(directory,prefix+"-city-overview",frames);
                Center(scene,city+new Vector2(-1,-4),14.5f);
                yield return Shot(directory,prefix+"-city-civic",frames);
                Move(scene,hero,travel.CityPresentation.GuildInteractionPosition+Vector2.down*2);
                Center(scene,travel.CityPresentation.GuildInteractionPosition+Vector2.up*3,11);
                yield return Shot(directory,prefix+"-city-guild",frames);
                Move(scene,hero,travel.RemotePosition+Vector2.down*2);Center(scene,travel.RemotePosition+Vector2.up*2,8);
                yield return Shot(directory,prefix+"-city-portal",frames);
                Move(scene,hero,travel.CityPresentation.MerchantPosition+Vector2.down*2);
                Center(scene,travel.CityPresentation.MerchantPosition+Vector2.up*2,9);
                yield return Shot(directory,prefix+"-city-shop",frames);
            }
            Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
            game.RunDriver.Run.StartNextWaveForTesting();
            for(var kind=0;kind<2;kind++)
            {
                Move(scene,hero,world.SpotPosition(kind)+Vector2.down*6);
                Center(scene,world.SpotPosition(kind)+Vector2.up*3,18);
                yield return Shot(directory,(kind==0?"orc":"ogre")+"-night",frames);
            }
            game.RunDriver.ResetRun();yield return null;
            Move(scene,hero,world.SpotPosition(0)+Vector2.down*6);
            scene.CameraFollow.enabled=true;scene.CameraFollow.ConfigureTacticalView(10);scene.CameraFollow.SnapToTarget();
            yield return Shot(directory,"input-camp-ready",frames);
            File.WriteAllText(Path.Combine(directory,"frames.txt"),string.Join("\n",frames));
            File.WriteAllText(Path.Combine(directory,"qualification.txt"),"Actual Mac player. Opt-in fixture holds clock/AI/bow/regen, moves hero with legal landing and explicitly centres camera for honest layout comparison. No item/XP/contract grants or UI data injection. Night uses real clock testing command. Final view permits actual movement, but AI remains held for safe observation. Ordinary restart clears fixture.\n");
            Debug.Log("S3_CAMPS_NATIVE_READY "+directory);
        }
        private static void Center(Stage2SceneBootstrap scene,Vector2 center,float zoom)
        {
            var follow=scene.CameraFollow;follow.ConfigureTacticalView(zoom);follow.enabled=false;
            follow.BoundCamera.transform.position=new Vector3(center.x,center.y,0)-follow.BoundCamera.transform.forward*20;
        }
        private static void Move(Stage2SceneBootstrap scene,Stage2HeroProgressionDriver hero,Vector2 p)
        {
            if(!hero.CanLand(p))
            {
                var found=false;
                for(var r=.5f;r<=10&&!found;r+=.5f)for(var i=0;i<32;i++)
                {var a=i*Mathf.PI/16;var q=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;if(!hero.CanLand(q))continue;p=q;found=true;break;}
                if(!found)throw new InvalidOperationException("No native camp/city footing.");
            }
            if(!scene.PlayerMover.TryTeleportTo(p))throw new InvalidOperationException("Camp/city native move rejected.");
        }
        private static IEnumerator Shot(string directory,string name,List<string> frames)
        {
            for(var i=0;i<6;i++)yield return null;yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));frames.Add(name);yield return null;
        }
    }
}
