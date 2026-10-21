using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in disposable native visual fixture. No view DTO injection or ordinary-start grants.</summary>
    public static class Stage3RecoveryReview
    {
        [Serializable] private sealed class Frame
        {
            public string name;
            public int width,height,bearing,targetId;
            public Rect pointer;
        }
        [Serializable] private sealed class Manifest
        {
            public string qualification="Automated real-rule fixture; screenshots are visual evidence, not mouse/keyboard input proof.";
            public string grants="One F1 helmet through TryReceiveLoot;41 temporary XP. Clock/combat/regen held only in this opt-in run.";
            public string inputFixture="Ends with the original dropped helmet nearby,100HP/MP,WTFoff. Press real E to recover. Fresh ordinary restart removes all fixture state.";
            public int dropId,itemId;
            public Vector2 deathPosition,respawnPosition;
            public List<Frame> frames=new List<Frame>();
        }
        private static readonly string[] Bearings={"east","southeast","south","southwest","west","northwest","north","northeast"};

        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);
            for(var i=0;i<12;i++)yield return null;
            var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            var hud=owner.GetComponent<Stage3FantasyHud>();
            var game=owner.GetComponent<Stage2GameComposition>();
            var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();
            var tools=owner.GetComponent<Stage2GameHud>();
            var recovery=binding.HeroRecovery;
            Require(recovery?.Rules!=null,"recovery driver bound");
            game.RunDriver.enabled=false;hero.AutomaticTick=false;game.HeroCombat.AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);tools.SetWtfMode(false);tools.SetDebugOpen(false);hud.CloseWindow();
            scene.CameraFollow.ConfigureTacticalView(5);
            var map=scene.MapDefinition;
            var center=map.CellCoordinatesToWorld(((Vector2)map.MinimumCell+(Vector2)map.MaximumCell)*.5f);
            var site=Clear(hero,center,10f);
            var near=Nearby(hero,site);
            Move(scene,site);
            Require(binding.Equipment.Rules.TryReceiveLoot(Stage2ItemType.Helmet,Stage2ItemQuality.F,1,out var item),"temporary spare helmet grant");
            hero.Rules.AwardExperience(41);hud.ClearExperienceFeedback();
            Require(tools.TestHeroDeath(),"same F3 command invokes real death/respawn rules");
            Require(recovery.Rules.DeathCount==1&&recovery.Rules.Drops.Count==1,"one real nonempty drop");
            var drop=recovery.Rules.Drops[0];
            Require(drop.ItemCount==1&&drop.Equipment[0].Id==item.Id,"same granted instance in drop");
            Require(Vector2.Distance(scene.PlayerMover.transform.position,site)>3,"base respawn moved hero away");
            Require(hero.Rules.Health==100&&hero.Rules.Mana==100,"restored real vitals");
            var manifest=new Manifest{dropId=drop.Id,itemId=item.Id,deathPosition=site,respawnPosition=scene.PlayerMover.transform.position};
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();
                Move(scene,manifest.respawnPosition);
                yield return Shot(directory,Name(size)+"-base-respawn",hud,manifest);
                Move(scene,near);yield return Settle();
                Require(recovery.CanRecover&&recovery.NearbyDrop?.Id==drop.Id,"nearby E candidate");
                Require(hud.RecoveryPointerRect==Rect.zero,"visible world target hides pointer");
                var marker=hud.RecoveryWorldMarkerRect(drop.Id);
                Require(marker.width>0&&marker.y>=0&&marker.yMax<hud.WorldHudRect.y,"world marker safe above HUD");
                yield return Shot(directory,Name(size)+"-world-near-E",hud,manifest);
                hud.SetWindow(Stage3PlayerWindow.Map);yield return Settle();
                Require(hud.RecoveryPointerRect==Rect.zero,"fullscreen map hides edge tooltip");
                yield return Shot(directory,Name(size)+"-map",hud,manifest);hud.CloseWindow();
                for(var direction=0;direction<8;direction++)
                {
                    var angle=direction*Mathf.PI*.25f;
                    var screenDirection=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    var delta=WorldBearing(scene.CameraFollow.BoundCamera,screenDirection)*30f;
                    var heroPosition=Clear(hero,site-delta,3f);
                    Move(scene,heroPosition);yield return Settle();
                    var r=hud.RecoveryPointerRect;var s=hud.UiScale;
                    Require(r.width>0&&hud.RecoveryTargetId==drop.Id,"offscreen pointer exists for correct drop");
                    Require(hud.RecoveryPointerBearing==direction,"bearing "+Bearings[direction]+" got "+hud.RecoveryPointerBearing);
                    Require(r.x>=23f*s&&r.xMax<=Screen.width-23f*s,"pointer horizontal safe bounds");
                    Require(r.y>=89f*s&&r.yMax<=hud.WorldHudRect.y-17f*s,"pointer top cards and HUD exclusion");
                    Require(!r.Overlaps(hud.ResourcesRect)&&!r.Overlaps(hud.StatusRect),"no top-card overlap");
                    yield return Shot(directory,Name(size)+"-pointer-"+Bearings[direction],hud,manifest);
                }
            }
            yield return Resize(new Vector2Int(1600,900));Move(scene,near);yield return Settle();
            tools.SetDebugOpen(true);
            // Scroll the existing debug viewport to expose its new final button, without adding any control.
            typeof(Stage2GameHud).GetField("scroll",BindingFlags.Instance|BindingFlags.NonPublic)?.SetValue(tools,new Vector2(0,180));
            yield return Shot(directory,"debug-death-button",hud,manifest);
            tools.SetDebugOpen(false);Move(scene,near);hud.ClearExperienceFeedback();yield return Settle();
            Require(recovery.CanRecover&&recovery.NearbyDrop?.Id==drop.Id,"real E fixture ready");
            var ground=scene.CameraFollow.BoundCamera.WorldToScreenPoint(Stage3HeroRecoveryPresentation.GroundPosition(site));
            File.WriteAllText(Path.Combine(directory,"input-ready-E.txt"),
                $"REAL KEYBOARD E READY\nDrop {drop.Id}; original helmet instance {item.Id}; count {drop.ItemCount}.\n"+
                $"Hero {scene.PlayerMover.transform.position}; drop {site}; screen bottom-left {ground.x:0.##},{ground.y:0.##}.\n"+
                "Close Debug/map if opened, release then press E. Expected: world/map/pointer markers disappear and same helmet returns to bag. No click/input proof is claimed by this fixture.\n");
            File.WriteAllText(Path.Combine(directory,"manifest.json"),JsonUtility.ToJson(manifest,true));
            Debug.Log("S3_HERO_RECOVERY_NATIVE_READY_E "+directory);
        }

        private static Vector2 WorldBearing(Camera camera,Vector2 screen)
        {
            var r=camera.transform.right;var u=camera.transform.up;var det=r.x*u.y-r.y*u.x;
            Require(Mathf.Abs(det)>.05f,"invertible ground projection");
            return new Vector2((screen.x*u.y+screen.y*r.y)/det,(-screen.y*r.x-screen.x*u.x)/det);
        }
        private static Vector2 Clear(Stage2HeroProgressionDriver hero,Vector2 origin,float radius)
        {
            if(hero.CanLand(origin))return origin;
            for(var ring=.25f;ring<=radius;ring+=.25f)for(var i=0;i<32;i++)
            {
                var angle=i*Mathf.PI/16;var p=origin+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*ring;
                if(hero.CanLand(p))return p;
            }
            throw new InvalidOperationException("Recovery QA: no legal fixture footing near "+origin);
        }
        private static Vector2 Nearby(Stage2HeroProgressionDriver hero,Vector2 site)
        {
            for(var i=0;i<32;i++)
            {
                var a=Mathf.PI+i*Mathf.PI/16;var p=site+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*1.35f;
                if(hero.CanLand(p))return p;
            }
            throw new InvalidOperationException("Recovery QA: no nearby E footing");
        }
        private static void Move(Stage2SceneBootstrap scene,Vector2 p)
        {Require(scene.PlayerMover.TryTeleportTo(p),"legal QA reposition");scene.CameraFollow.SnapToTarget();}
        private static IEnumerator Settle(){for(var i=0;i<4;i++)yield return null;yield return new WaitForEndOfFrame();}
        private static void Require(bool value,string text){if(!value)throw new InvalidOperationException("Recovery visual QA: "+text);}
        private static string Name(Vector2Int size)=>size.x+"x"+size.y;
        private static IEnumerator Resize(Vector2Int size){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);}
        private static IEnumerator Shot(string directory,string name,Stage3FantasyHud hud,Manifest manifest)
        {
            yield return Settle();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));
            manifest.frames.Add(new Frame{name=name,width=Screen.width,height=Screen.height,bearing=hud.RecoveryPointerBearing,
                targetId=hud.RecoveryTargetId,pointer=hud.RecoveryPointerRect});
            yield return null;
        }
    }
}
