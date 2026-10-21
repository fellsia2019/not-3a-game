using System;
using System.Collections;
using System.IO;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit disposable native QA; never runs in ordinary startup.</summary>
    public static class Stage3CityTeleportReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);
            for(var i=0;i<12;i++)yield return null;
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var travel=owner.GetComponent<Stage3WorldTravel>();var hud=owner.GetComponent<Stage3FantasyHud>();
            if(!travel.CityReady)throw new InvalidOperationException("Native city missing");
            game.RunDriver.enabled=false;game.RunDriver.ResetRun();game.SetPaused(false);game.HeroCombat.AutomaticTick=false;hud.CloseWindow();
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;
            Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.4f);
            scene.PlayerMover.transform.position=travel.RemotePosition;yield return null;
            foreach(var zoom in new[]{25f,17f})
            {camera.orthographicSize=zoom;camera.transform.position=(Vector3)travel.CityOrigin-camera.transform.forward*30f;yield return Shot(directory,"city-overview-"+zoom);}
            camera.orthographicSize=5.5f;camera.transform.position=(Vector3)(travel.RemotePosition+Vector2.up*1.7f)-camera.transform.forward*30f;
            yield return Shot(directory,"temple-court");
            camera.transform.position=(Vector3)(travel.CityPresentation.MerchantPosition+Vector2.up*2f)-camera.transform.forward*30f;
            yield return Shot(directory,"merchant-shop");
            scene.PlayerMover.transform.position=travel.BasePosition;yield return null;
            owner.GetComponent<Stage2GameHud>().SelectWorkshop();
            var build=travel.TryBuildWorkshopAt(travel.WorkshopPosition);
            if(build!=WorkshopPlacementResult.Built)throw new InvalidOperationException("QA workshop build failed: "+build+" at "+travel.WorkshopPosition+" placing="+travel.IsPlacingWorkshop+" blocked="+game.PlayerInputBlocked);
            scene.PlayerMover.transform.position=travel.WorkshopInteractionPosition;
            if(!travel.TryInteract())throw new InvalidOperationException("QA research page failed");
            hud.SelectResearchEntry(-1);
            if(!owner.GetComponent<Stage3PlayerInterfaceBinding>().TryStudyResearch(-1)||!travel.IsResearched)throw new InvalidOperationException("QA study failed");
            hud.CloseWindow(); travel.BeginBaseCirclePlacement();
            if(travel.TryBuildBaseCircleAt(travel.BasePosition)!=WorkshopPlacementResult.Built)throw new InvalidOperationException("QA manual base-circle build failed");
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
                foreach(var source in new[]{Stage3TravelState.CityId,Stage3TravelState.BaseId})
                {
                    hud.CloseWindow();scene.PlayerMover.transform.position=source==Stage3TravelState.CityId?travel.RemotePosition:travel.BasePosition;
                    camera.orthographicSize=6f;camera.transform.position=scene.PlayerMover.transform.position+Vector3.up*1.5f-camera.transform.forward*30f;
                    if(!travel.TryOpenTeleportMenu(source))throw new InvalidOperationException("QA menu failed "+source);
                    yield return Shot(directory,$"{size.x}x{size.y}-from-{source}");
                    hud.SelectTeleportDestination(source);yield return Shot(directory,$"{size.x}x{size.y}-current-{source}");
                }
            }
            hud.CloseWindow();Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
            scene.PlayerMover.transform.position=travel.RemotePosition;camera.orthographicSize=6;
            camera.transform.position=scene.PlayerMover.transform.position+Vector3.up*1.5f-camera.transform.forward*30f;
            travel.TryOpenTeleportMenu(Stage3TravelState.CityId);
            File.WriteAllText(Path.Combine(directory,"verification.txt"),$"Disposable native QA: frozen clock, free prototype workshop/study through product rules, personally visit city. No funds/items granted. City={travel.CityOrigin}; portal={travel.RemotePosition}; base={travel.BasePosition}. 4cityviews+12menuviews at720/900/1080; current point disabled. End city→base menu for actualmouse confirm.\n");
            Debug.Log("S3_CITY_TELEPORT_NATIVE_READY "+directory);
        }
        private static IEnumerator Shot(string dir,string name)
        {yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return null;}
    }
}
