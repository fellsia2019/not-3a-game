using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit development-only clock fixture. No view injection or normal-start grants.</summary>
    public static class Stage3CycleReview
    {
        [Serializable]private sealed class Frame
        {public string name,phase;public int width,height;public float progress,degrees;public Rect dial;}
        [Serializable]private sealed class Manifest
        {
            public string qualification="Real rules and bound HUD; clock/combat/regen held only in this opt-in fixture. No resource/XP/item grants. Screenshots are not mouse-input proof.";
            public List<Frame> frames=new List<Frame>();
        }
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<12;i++)yield return null;
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();var hud=owner.GetComponent<Stage3FantasyHud>();
            var tools=owner.GetComponent<Stage2GameHud>();var run=game.RunDriver.Run;var manifest=new Manifest();
            game.RunDriver.enabled=false;hero.AutomaticTick=false;game.HeroCombat.AutomaticTick=false;
            tools.SetDebugOpen(false);game.SetPaused(false);hud.CloseWindow();
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
                game.RunDriver.ResetRun();scene.CameraFollow.SnapToTarget();yield return Settle();CheckGeometry(hud,tools);
                Require(run.DayNightCycleProgress==0&&hud.CyclePhaseLabel=="День","dawn binding");
                yield return Shot(directory,Name(size)+"-dawn",hud,run,manifest);
                run.Tick(300);yield return Shot(directory,Name(size)+"-midday",hud,run,manifest);
                game.SetPaused(true);var frozen=run.DayNightCycleProgress;run.Tick(90);
                Require(run.DayNightCycleProgress==frozen,"pause keeps hand still");game.SetPaused(false);
                Require(run.SleepUntilBeforeNight(),"actual sleep");
                yield return Shot(directory,Name(size)+"-before-night",hud,run,manifest);
                Require(tools.StartNextWave(),"same F3 night command");FreezeEnemies(game);
                Require(Mathf.Abs(run.DayNightCycleProgress-10f/13f)<.00001f,"night at10/13");
                yield return Shot(directory,Name(size)+"-night-start",hud,run,manifest);
                run.Tick(90);FreezeEnemies(game);yield return Shot(directory,Name(size)+"-midnight",hud,run,manifest);
                run.Tick(90);FreezeEnemies(game);
                Require(run.SpawningFinished&&run.ActiveEnemies>0&&!run.IsNightTime,"dawn stops spawns and retains living threats");
                yield return Shot(directory,Name(size)+"-dawn-live-horde",hud,run,manifest);
                game.RunDriver.ResetRun();hud.SetWindow(Stage3PlayerWindow.Inventory);
                yield return Shot(directory,Name(size)+"-inventory",hud,run,manifest);
                hud.SetWindow(Stage3PlayerWindow.Map);yield return Shot(directory,Name(size)+"-map",hud,run,manifest);hud.CloseWindow();
            }
            Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
            game.RunDriver.ResetRun();tools.SetDebugOpen(true);yield return Shot(directory,"debug-access",hud,run,manifest);tools.SetDebugOpen(false);
            File.WriteAllText(Path.Combine(directory,"manifest.json"),JsonUtility.ToJson(manifest,true));
            Debug.Log("S3_CYCLE_NATIVE_REVIEW_OK "+directory);
        }
        private static void CheckGeometry(Stage3FantasyHud hud,Stage2GameHud tools)
        {
            var r=hud.CycleDialRect;var s=hud.UiScale;
            Require(r.x>=0&&r.y>=0&&r.xMax<=Screen.width&&r.yMax<=Screen.height,"whole dial onscreen");
            Require(Mathf.Abs(r.center.x-Screen.width*.5f)<.01f&&Mathf.Abs(r.width-120f*s)<.01f,"enlarged centered dial");
            Require(hud.CycleCorniceRect.y==0&&hud.CycleCorniceRect.width==Screen.width,"flush cornice");
            foreach(var rect in new[]{hud.CycleDialRect,hud.CyclePhaseRect,hud.ResourcesRect,hud.DevelopmentAccessRect})
                Require(hud.IsPointerOverUi(new Vector2(rect.center.x,Screen.height-rect.center.y)),"protected topHUD hit region");
            var gap=new Vector2((hud.ResourcesRect.xMax+hud.CycleDialRect.xMin)*.5f,85f*s);
            Require(!hud.IsPointerOverUi(new Vector2(gap.x,Screen.height-gap.y)),"open world gap remains interactive");
            Require(Vector2.Distance(tools.DebugButtonRect.position*tools.Scale,hud.DevelopmentAccessRect.position)<.01f,"F3 hit region follows visible access");
        }
        private static void FreezeEnemies(Stage2GameComposition game)
        {for(var i=0;i<game.Enemies.ActiveCount;i++)game.Enemies.GetEnemy(i).SetAutomaticTick(false);}
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("Cycle QA: "+message);}
        private static string Name(Vector2Int size)=>size.x+"x"+size.y;
        private static IEnumerator Settle(){for(var i=0;i<6;i++)yield return null;yield return new WaitForEndOfFrame();}
        private static IEnumerator Shot(string directory,string name,Stage3FantasyHud hud,Not3A.Stage2.RunState.Stage2Run run,Manifest manifest)
        {
            yield return Settle();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));
            manifest.frames.Add(new Frame{name=name,width=Screen.width,height=Screen.height,phase=hud.CyclePhaseLabel,
                progress=run.DayNightCycleProgress,degrees=hud.CycleHandDegrees,dial=hud.CycleDialRect});yield return null;
        }
    }
}
