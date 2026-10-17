using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Explicit review-only startup; no authored state or inventory is serialized.</summary>
    public static class Stage3PlacementReview
    {
        private const string Key="Not3A.PlacementReviewRequested";
        private const string Folder="Artifacts/S3-PLACEMENT-READABILITY-20261016/live";
        [InitializeOnLoadMethod] private static void Resume()
        { if(SessionState.GetBool(Key,false)) EditorApplication.update+=AwaitWorld; }
        [MenuItem("Tools/Stage 3/Resource Districts/Review Placement Readability")]
        public static void OpenReview()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start review outside Play.");
            EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            SessionState.SetBool(Key,true); EditorApplication.update-=AwaitWorld; EditorApplication.update+=AwaitWorld;
            EditorApplication.isPlaying=true;
        }
        private static void AwaitWorld()
        {
            if(!EditorApplication.isPlaying) return;
            var game=Object.FindAnyObjectByType<Stage2GameComposition>();
            if(game?.Production?.Rules?.Points.Count!=6) return;
            EditorApplication.update-=AwaitWorld; SessionState.SetBool(Key,false); game.StartCoroutine(Capture(game));
        }
        private static IEnumerator Capture(Stage2GameComposition game)
        {
            Directory.CreateDirectory(Folder);
            var scene=game.GetComponent<Stage2SceneBootstrap>(); var world=game.GetComponent<Stage3ResourceDistrictWorld>();
            game.RunDriver.ResetRun(); world.Regenerate(164032);
            var camera=scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled=false; camera.orthographicSize=5;
            var hud=game.GetComponent<Stage2GameHud>(); hud.SelectBuildCategory(Stage2BuildCategory.Generators);
            foreach(var kind in new[]{ResourceProductionBuilding.IronMine,ResourceProductionBuilding.Quarry,ResourceProductionBuilding.Sawmill})
            {
                var point=game.Production.Rules.Points.First(p=>p.Building==kind);
                scene.PlayerMover.transform.position=point.Position+Vector2.down*1.2f;
                camera.transform.position=new Vector3(point.Position.x,point.Position.y+.5f,-.3f)-camera.transform.forward*30;
                yield return null; yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Folder+"/"+kind+"-disabled-hud.png"); yield return null;
            }
            Debug.Log("S3_PLACEMENT_MANUAL_REVIEW_READY: no QA funds, six free pad marks; test disabled card, then debug grant for valid preview.");
        }
    }
}
