using System;
using System.Collections;
using System.IO;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Disposable native-input QA, enabled only by an explicit development-player environment variable.</summary>
    public static class Stage3GateCycleReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>(); var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var home=owner.GetComponent<Stage3Dwelling>(); var hud=owner.GetComponent<Stage3FantasyHud>();
            var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while(home==null || !home.IsReady || !hud.IsReady) {yield return null;home=owner.GetComponent<Stage3Dwelling>();}
            for(var i=0;i<12;i++) yield return null;
            game.RunDriver.enabled=false; game.HeroCombat.AutomaticTick=false;
            game.RunDriver.ResetRun(); var run=game.RunDriver.Run;
            var progression=owner.GetComponent<Stage2HeroProgressionDriver>(); progression.AutomaticTick=false;
            // Declared setup: build a real paid house and clear one night, advance to
            // dawn, then wait for actual E and actual selected-card mouse input.
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood,10); home.BeginPlacement();
            var origin=(Vector2)scene.PlayerMover.transform.position; var built=false;
            for(var ring=5;ring<45&&!built;ring+=2)
            for(var x=-ring;x<=ring&&!built;x+=2) for(var y=-ring;y<=ring&&!built;y+=2)
            { var point=origin+new Vector2(x,y); if(home.EvaluateAt(point)==DwellingPlacementResult.Ready) built=home.TryBuildAt(point)==DwellingPlacementResult.Built; }
            Require(built,"real dwelling placement");
            run.Tick(run.PreparationRemaining+.001f);
            while(!run.SpawningFinished)run.Tick(.25f);
            while(game.Enemies.ActiveCount>0) game.Enemies.GetEnemy(0).TakeDamage(int.MaxValue);
            owner.GetComponent<Stage3HeroLevelUpPresentation>()?.ClearEffects();
            owner.GetComponent<Stage3HeroProgressionEffects>()?.ClearEffects();
            Require(run.IsDay && run.LastClearedTier==1,"last tier unlocked at dawn");
            scene.PlayerMover.transform.position=scene.EntranceMarker.position+Vector3.down*2;
            scene.CameraFollow.SnapToTarget();
            File.WriteAllText(Path.Combine(directory,"verification.txt"),
                $"Actual native player {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceType}.\n"+
                "Disposable QA:10wood paid house, simulated first-night clock/kills/dawn, automatic clock/combat disabled. Not a natural playthrough.\n");
            yield return Shot(directory,"01-local-gate-ready");
            Debug.Log("S3_GATE_NATIVE_READY_OPEN "+directory);
            while(!run.DaytimeGateOpen) yield return null;
            run.Tick(.01f); var creep=game.Enemies.GetEnemy(0); creep.SetAutomaticTick(false);
            yield return Shot(directory,"02-open-live-creep");
            Debug.Log("S3_GATE_NATIVE_READY_CLOSE");
            while(run.DaytimeGateOpen) yield return null;
            Require(creep.IsAlive && !run.CanSleepUntilNight,"native close keeps live creep and blocks sleep");
            yield return Shot(directory,"03-closed-live-creep");
            creep.TakeDamage(int.MaxValue);
            scene.PlayerMover.transform.position=home.InteractionPosition; scene.CameraFollow.SnapToTarget();
            yield return null; yield return null;
            var visual=owner.GetComponent<Stage3DwellingPresentation>().BuiltVisual;
            Require(binding.TrySelectBuildingAtScreen(scene.CameraFollow.BoundCamera.WorldToScreenPoint(visual.bounds.center)) &&
                binding.SelectedBuildingKind==Stage3BuildingSelectionKind.Dwelling,"select actual dwelling");
            Require(hud.SelectedBuildingView.SleepAvailable,"sleep card enabled after fight");
            yield return Shot(directory,"04-sleep-card-ready");
            var rect=hud.SelectedBuildingSleepRect;
            File.WriteAllText(Path.Combine(directory,"sleep-button.txt"),$"GUI-top-left rect={rect}; centre={rect.center}; backbuffer={Screen.width}x{Screen.height}\n");
            Debug.Log("S3_GATE_NATIVE_READY_SLEEP "+rect);
            while(run.WorldMinutes%1440<1190) yield return null;
            Require(run.ClockHour==19 && run.ClockMinute==50 && run.ActiveEnemies==0,"native Sleep ends at19:50 outside battle");
            yield return Shot(directory,"05-slept-1950");
            File.AppendAllText(Path.Combine(directory,"verification.txt"),
                "PASS actual E open/close; existing creep retained, sleep refused while alive.\n"+
                "PASS real selected-house Sleep click ends at19:50; no enemy spawn or skipped-time rewards.\n");
            Debug.Log("S3_GATE_NATIVE_REVIEW_OK "+directory);
        }
        private static void Require(bool value,string reason)
        { if(!value) throw new InvalidOperationException("Gate cycle QA: "+reason); }
        private static IEnumerator Shot(string directory,string name)
        {
            yield return new WaitForSecondsRealtime(.2f); yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png")); yield return null;
        }
    }
}
