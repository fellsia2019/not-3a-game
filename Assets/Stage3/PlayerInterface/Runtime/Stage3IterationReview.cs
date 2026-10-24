using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in disposable native checks for the current workshop/city/combat correction.</summary>
    public static class Stage3IterationReview
    {
        [Serializable] private sealed class CircleInput
        { public Rect card;public Vector2 valid,invalid;public string instructions; }
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<20;i++)yield return null;
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            var world=binding.GuildWorld;var frames=new List<string>();
            game.RunDriver.enabled=false;hero.AutomaticTick=false;game.HeroCombat.AutomaticTick=false;world.AutomaticTick=false;
            var start=(Vector2)scene.PlayerMover.transform.position;
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
                game.RunDriver.ResetRun();game.SetPaused(false);hud.CloseWindow();Move(scene,Clear(hero,start));
                scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
                var prefix=size.x+"x"+size.y;
                yield return Shot(directory,prefix+"-dawn-far-city-hidden",frames);
                Require(hud.CityMinimapMarkerRect==Rect.zero,"no distant city pointer");
                Require(Mathf.Abs(hud.CycleDialRect.width-120*hud.UiScale)<.01f,"double-size clock");
                game.Gathering.Economy.Inventory.TryAdd(Not3A.Stage2.Map.MapResourceKind.Wood,travel.WorkshopWoodCost);game.Gathering.Economy.Inventory.TryAdd(Not3A.Stage2.Map.MapResourceKind.Stone,travel.WorkshopStoneCost);travel.BeginWorkshopPlacement();Require(travel.TryBuildWorkshopAt(travel.WorkshopPosition)==WorkshopPlacementResult.Built,"workshop construction");
                Move(scene,Clear(hero,travel.WorkshopInteractionPosition));Require(travel.TryInteract(),"local E research route");
                hud.SelectResearchEntry(-1);yield return Shot(directory,prefix+"-starter-blueprint",frames);
                Require(binding.TryStudyResearch(-1),"starter study");Require(!travel.BaseBuilt&&!travel.BaseVisible,"study does not construct circle");
                yield return Shot(directory,prefix+"-studied-unbuilt",frames);
                hud.SetWindow(Stage3PlayerWindow.Construction);hud.SetBuildGroup(2);
                yield return Shot(directory,prefix+"-base-construction-card",frames);
                Require(hud.ActivateBuildChoice(3)&&travel.IsPlacingBaseCircle,"learned construction card selects circle");
                var site=CircleSite(travel);
                if(size.x==1600)
                {
                    // One resolution is deliberately left for actual native mouse input.
                    travel.CancelBaseCirclePlacement();Move(scene,Clear(hero,site+Vector2.down*4));
                    scene.CameraFollow.ConfigureTacticalView(6);scene.CameraFollow.SnapToTarget();
                    for(var i=0;i<5;i++)yield return null;
                    Vector2 GuiPoint(Vector2 p)
                    {var v=scene.CameraFollow.BoundCamera.WorldToScreenPoint(p);return new Vector2(v.x,Screen.height-v.y);}
                    var input=new CircleInput{card=hud.BuildChoiceRect(3),valid=GuiPoint(site),
                        invalid=GuiPoint(scene.PlayerMover.transform.position),
                        instructions="Click circle card; click invalid hero footing (red); drag from invalid footing to valid (green, not released build); fresh click valid constructs. GUI coordinates exclude window title bar."};
                    File.WriteAllText(Path.Combine(directory,"input-circle-ready.json"),JsonUtility.ToJson(input,true));
                    Debug.Log("S3_ITERATION_CIRCLE_MOUSE_READY");
                    var started=Time.realtimeSinceStartup;
                    while(!travel.BaseBuilt&&Time.realtimeSinceStartup-started<180)yield return null;
                    Require(travel.BaseBuilt,"actual mouse base construction");
                }
                else Require(travel.TryBuildBaseCircleAt(site)==WorkshopPlacementResult.Built,"manual circle build");
                hud.CloseWindow();Move(scene,Clear(hero,travel.BasePosition+Vector2.down*3));
                scene.CameraFollow.ConfigureTacticalView(6);scene.CameraFollow.SnapToTarget();
                yield return Shot(directory,prefix+"-base-built",frames);
                Require(travel.BaseBuilt&&travel.BaseVisible,"built base circle visible");
                Move(scene,Clear(hero,travel.CityOrigin+Vector2.down*6));
                scene.CameraFollow.ConfigureTacticalView(27);scene.CameraFollow.SnapToTarget();
                yield return Shot(directory,prefix+"-city-overview",frames);
                Require(hud.CityMinimapMarkerRect.width>0,"nearby city icon remains");
                Move(scene,Clear(hero,new Vector2(travel.CityPresentation.GuildFootprint.center.x,travel.CityPresentation.GuildFootprint.yMin-2)));
                scene.CameraFollow.ConfigureTacticalView(8);scene.CameraFollow.SnapToTarget();
                yield return Shot(directory,prefix+"-guild-approach",frames);
                Require(binding.NearGuild&&binding.TryOpenGuild(),"real building approach opens usable guild");
                yield return Shot(directory,prefix+"-guild-accept-ready",frames);hud.CloseWindow();
                game.RunDriver.Run.Tick(600);
                for(var i=0;i<game.Enemies.ActiveCount;i++)game.Enemies.GetEnemy(i).SetAutomaticTick(false);
                scene.CameraFollow.ConfigureTacticalView(27);Move(scene,Clear(hero,travel.CityOrigin+Vector2.down*6));
                yield return Shot(directory,prefix+"-night-city",frames);
                Require(hud.CyclePhaseLabel=="Ночь","night binding");
            }
            Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
            game.RunDriver.ResetRun();hud.CloseWindow();
            var footprint=travel.CityPresentation.GuildFootprint;
            Move(scene,Clear(hero,new Vector2(footprint.center.x,footprint.yMin-2)));
            scene.CameraFollow.ConfigureTacticalView(8);scene.CameraFollow.SnapToTarget();
            yield return Shot(directory,"input-guild-ready",frames);
            File.WriteAllText(Path.Combine(directory,"qualification.txt"),"Real Mac development player. Fixture holds clock/AI/bow/regen and moves hero only for review; constructs/studies using real rules. No item/XP grants or view DTO injection. Native screenshots are programmatic appearance proof, actual input recorded separately. Ordinary process restart clears all fixture flags.\n");
            File.WriteAllText(Path.Combine(directory,"frames.txt"),string.Join("\n",frames));
            Debug.Log("S3_ITERATION_NATIVE_READY "+directory);
        }
        private static Vector2 CircleSite(Stage3WorldTravel travel)
        {
            var center=travel.BasePosition;
            for(var radius=0;radius<=24;radius++)for(var y=-radius;y<=radius;y++)for(var x=-radius;x<=radius;x++)
            {
                if(Mathf.Max(Mathf.Abs(x),Mathf.Abs(y))!=radius)continue;
                var p=center+new Vector2(x,y);if(travel.EvaluateBaseCircleAt(p)==WorkshopPlacementResult.Ready)return p;
            }
            throw new InvalidOperationException("No native circle site");
        }
        private static Vector2 Clear(Stage2HeroProgressionDriver hero,Vector2 point)
        {
            if(hero.CanLand(point))return point;
            for(var r=.25f;r<=8;r+=.25f)for(var i=0;i<32;i++)
            {var a=i*Mathf.PI/16;var p=point+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;if(hero.CanLand(p))return p;}
            throw new InvalidOperationException("No native review footing");
        }
        private static void Move(Stage2SceneBootstrap scene,Vector2 p)
        {Require(scene.PlayerMover.TryTeleportTo(p),"legal fixture footing");scene.CameraFollow.SnapToTarget();}
        private static void Require(bool ok,string reason){if(!ok)throw new InvalidOperationException("Iteration QA: "+reason);}
        private static IEnumerator Shot(string directory,string name,List<string> frames)
        {for(var i=0;i<5;i++)yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));frames.Add(name);yield return null;}
    }
}
