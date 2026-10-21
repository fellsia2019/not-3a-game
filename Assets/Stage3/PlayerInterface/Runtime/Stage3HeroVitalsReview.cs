using System;
using System.Collections;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Disposable opt-in data/camera setup for native numerical HUD and development controls.</summary>
    public static class Stage3HeroVitalsReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<12;i++)yield return null;
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var driver=owner.GetComponent<Stage2HeroProgressionDriver>();
            game.RunDriver.enabled=false;driver.AutomaticTick=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.ResetRun();game.SetPaused(false);
            var rules=driver.Rules;var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            scene.PlayerMover.transform.position=Clear(driver,scene.PlayerMover.transform.position);scene.CameraFollow.SnapToTarget();
            foreach(var size in sizes)
            {
                yield return Resize(size);hud.CloseWindow();rules.Reset();hud.ClearExperienceFeedback();
                yield return Shot(directory,Name(size)+"-hud-full");
                Require(driver.TryBlink(Near(driver,scene.PlayerMover.transform.position)),"valid first blink");
                Require(rules.Mana==0&&rules.BlinkCooldownRemaining==5,"100MP cost /5s cooldown");
                yield return Shot(directory,Name(size)+"-hud-empty");
                driver.Tick(5);Require(rules.Mana==25&&rules.BlinkCooldownRemaining==0&&!rules.CanBlink,"regen and mana gate");
                yield return Shot(directory,Name(size)+"-hud-quarter");
                driver.Tick(15);Require(rules.Mana==100&&rules.CanBlink,"full recovery");
                yield return Shot(directory,Name(size)+"-hud-ready");
                rules.AwardExperience(int.MaxValue);owner.GetComponent<Stage3HeroLevelUpPresentation>().ClearEffects();hud.ClearExperienceFeedback();
                hud.SetWindow(Stage3PlayerWindow.Hero);yield return Shot(directory,Name(size)+"-ranks-start");
                while(rules.TryUpgradeBlink()){}yield return Shot(directory,Name(size)+"-ranks-max");
            }
            yield return Resize(new Vector2Int(1600,900));hud.CloseWindow();rules.Reset();
            scene.CameraFollow.ConfigureTacticalView(5);yield return new WaitForSecondsRealtime(.2f);
            yield return Shot(directory,"zoom-5");scene.CameraFollow.ZoomByPinchScale(.5f);yield return new WaitForSecondsRealtime(1.1f);
            yield return Shot(directory,"zoom-10");scene.CameraFollow.ZoomByPinchScale(.001f);yield return new WaitForSecondsRealtime(1.1f);
            Require(Mathf.Abs(scene.CameraFollow.BoundCamera.orthographicSize-15)<.03f,"new15 zoom clamp");yield return Shot(directory,"zoom-15");
            foreach(var cell in new[]{scene.MapDefinition.MinimumCell,scene.MapDefinition.MaximumCell})
            {
                scene.PlayerMover.transform.position=scene.MapDefinition.CellToWorld(cell);scene.PlayerMover.ClampToAuthoredBounds();scene.CameraFollow.SnapToTarget();
                yield return Shot(directory,cell==scene.MapDefinition.MinimumCell?"zoom-15-edge-min":"zoom-15-edge-max");
            }
            // Exercise the same development commands as the buttons; this is not a mouse-input proof.
            game.RunDriver.ResetRun();scene.CameraFollow.ConfigureTacticalView(5);
            scene.PlayerMover.transform.position=Clear(driver,scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)+Vector2.down*3);
            scene.CameraFollow.SnapToTarget();var tools=owner.GetComponent<Stage2GameHud>();
            tools.SetDebugOpen(true);yield return Shot(directory,"debug-wtf-off");tools.SetDebugOpen(false);
            Require(driver.TryBlink(Near(driver,scene.PlayerMover.transform.position)),"ordinary cast before WTF");
            tools.SetWtfMode(true);Require(rules.Mana==0&&rules.BlinkCooldownRemaining==0,"WTF clears cooldown without mana refund");
            for(var i=0;i<2;i++)Require(driver.TryBlink(Near(driver,scene.PlayerMover.transform.position)),"consecutive zero-mana WTF cast");
            Require(rules.Mana==0&&rules.BlinkCooldownRemaining==0,"WTF zero mana/cooldown");tools.SetDebugOpen(true);yield return Shot(directory,"debug-wtf-on");
            Require(tools.StartNextWave()&&game.RunDriver.Run.Phase==Not3A.Stage2.RunState.Stage2RunPhase.Wave,"first-day debug launch");
            yield return Shot(directory,"debug-wave-started");tools.SetDebugOpen(false);
            // Remove XP/rank/vital QA state; leave a declared first-day fixture for actual Q/F3 input.
            game.RunDriver.ResetRun();driver.AutomaticTick=true;scene.CameraFollow.ConfigureTacticalView(5);
            scene.PlayerMover.transform.position=Clear(driver,scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)+Vector2.down*3);
            scene.CameraFollow.SnapToTarget();yield return null;
            var target=Near(driver,scene.PlayerMover.transform.position);var screen=scene.CameraFollow.BoundCamera.WorldToScreenPoint(target);
            var originScreen=scene.CameraFollow.BoundCamera.WorldToScreenPoint(scene.PlayerMover.transform.position);
            File.WriteAllText(Path.Combine(directory,"input-controls.txt"),$"target screen bottom-left: {screen.x:0.##},{screen.y:0.##}; hero ground: {originScreen.x:0.##},{originScreen.y:0.##}; add titlebar when using native window coordinates.\n");
            File.WriteAllText(Path.Combine(directory,"verification.txt"),"Disposable QA: clock/combat held, manually ticked regen, explicit temporary XP/rank grants for18HUD/page frames plus3zoom frames. All XP/ranks reset afterwards. Actual-input fixture: first day08:00,5gold,100HP/100MP,normal5MP/s regen,hero near entrance; clock held pending F3 launch. Normal startup never receives QA grants/freezing.\n");
            Debug.Log("S3_HERO_VITALS_INPUT_READY "+directory);
        }
        private static Vector2 Clear(Stage2HeroProgressionDriver driver,Vector2 origin)
        {for(var y=-5;y<=5;y++)for(var x=-5;x<=5;x++){var point=origin+new Vector2(x,y);if(driver.CanLand(point))return point;}throw new InvalidOperationException("No legal QA footing.");}
        private static Vector2 Near(Stage2HeroProgressionDriver driver,Vector2 origin)
        {for(var i=0;i<32;i++){var point=origin+new Vector2(Mathf.Cos(i*Mathf.PI/16),Mathf.Sin(i*Mathf.PI/16))*1.5f;if(driver.CanLand(point))return point;}throw new InvalidOperationException("No legal QA landing.");}
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("Vitals QA: "+message);}
        private static string Name(Vector2Int size)=>size.x+"x"+size.y;
        private static IEnumerator Resize(Vector2Int size){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.25f);}
        private static IEnumerator Shot(string directory,string name)
        {yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return null;}
    }
}
