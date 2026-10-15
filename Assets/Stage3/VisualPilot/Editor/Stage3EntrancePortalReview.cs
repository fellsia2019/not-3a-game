using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Explicit local requests only; no work runs without a fresh review.request.</summary>
    public static class Stage3EntrancePortalReview
    {
        private const string Folder="Artifacts/S3-ENTRANCE-PORTAL";
        private const string Request=Folder+"/review.request";
        private const string Running="Not3A.PortalReview.Tests";
        private const string Show="Not3A.PortalReview.Show";
        private const string Motion="Not3A.PortalReview.Motion";
        private const string Reloaded="Not3A.PortalReview.Reloaded";
        private const string Scene="Assets/Scenes/Stage3Rapid3DGamePilot.unity";
        private static TestRunnerApi runner;
        private static double nextPoll,captureAt;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            if(Application.isBatchMode)return;
            runner=ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Results());
            EditorApplication.update+=Poll;
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Show,false))
                    captureAt=EditorApplication.timeSinceStartup+8;
            };
        }

        private static void Poll()
        {
            if(EditorApplication.timeSinceStartup<nextPoll)return;
            nextPoll=EditorApplication.timeSinceStartup+1;
            if(EditorApplication.isCompiling || EditorApplication.isUpdating)return;
            if(captureAt>0 && EditorApplication.timeSinceStartup>captureAt && EditorApplication.isPlaying)
            {
                if(ShaderUtil.anythingCompiling || UnityEngine.Object.FindAnyObjectByType<Stage3EntrancePortal>()==null)return;
                captureAt=0; SessionState.SetBool(Show,false);
                Capture();
            }
            if(SessionState.GetBool(Running,false) || !File.Exists(Request))return;
            if(DateTime.UtcNow-File.GetLastWriteTimeUtc(Request)>TimeSpan.FromMinutes(10))
            {File.Delete(Request);return;}
            if(EditorApplication.isPlayingOrWillChangePlaymode)
            {EditorApplication.isPlaying=false;return;}
            var command=File.ReadAllText(Request).Trim();
            if((command=="validate" || command=="portal-test") && !SessionState.GetBool(Reloaded,false))
            {
                // Refresh before building, then reload so repeated runs also
                // discover tests when the editor has disabled Play-mode reloads.
                SessionState.SetBool(Reloaded,true);
                AssetDatabase.Refresh();
                nextPoll=EditorApplication.timeSinceStartup+5;
                EditorUtility.RequestScriptReload();
                return;
            }
            SessionState.SetBool(Reloaded,false);
            File.Delete(Request);
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            try
            {
                Stage3RapidPilotBuilder.Build3DGameAssets();
                if(command=="validate" || command=="portal-test")
                {
                    SessionState.SetBool(Running,true);
                    File.WriteAllText(Folder+"/status.txt","Running Stage 3, Integration and RunState PlayMode suites");
                    runner.Execute(new ExecutionSettings(new Filter{
                        testMode=TestMode.PlayMode,
                        testNames=command=="portal-test" ? new[]{"Not3A.Stage3.VisualPilot.Tests.Stage3RapidPilotTests.EntrancePortal_PreservesSpawnPlacementPauseAndReset"} : null,
                        assemblyNames=new[]{"Not3A.Stage3.VisualPilot.PlayModeTests",
                            "Not3A.Stage2.Integration.PlayModeTests","Not3A.Stage2.RunState.PlayModeTests"}
                    }));
                }
                else if(command=="build")
                {
                    Directory.CreateDirectory("Builds/Windows/Stage3EntrancePortal");
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                        scenes=new[]{Scene}, locationPathName="Builds/Windows/Stage3EntrancePortal/Not3A.exe",
                        target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
                    });
                    File.WriteAllText(Folder+"/build.txt",$"{report.summary.result} bytes={report.summary.totalSize} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings}");
                    if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Portal build failed.");
                    Open();
                }
                else Open(command=="motion");
            }
            catch(Exception error)
            {
                SessionState.SetBool(Running,false);
                File.WriteAllText(Folder+"/status.txt",error.ToString()); Debug.LogException(error);
            }
        }

        private static void Open(bool captureMotion=false)
        {
            EditorSceneManager.OpenScene(Scene);
            SessionState.SetBool(Show,true);
            SessionState.SetBool(Motion,captureMotion);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying=true;
        }

        private static void Capture()
        {
            var bootstrap=UnityEngine.Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            if(bootstrap==null)return;
            var camera=bootstrap.CameraFollow.BoundCamera;
            var position=camera.transform.position; var size=camera.orthographicSize;
            try
            {
                SaveCamera(camera,"art/review/entrance_portal_game_overview.png",1600,900);
                var focus=bootstrap.EntranceMarker.position+Vector3.back*1.4f;
                camera.transform.position=focus-camera.transform.forward*12;
                camera.orthographicSize=2.6f;
                SaveCamera(camera,"art/review/entrance_portal_game_close.png",1200,1000);
            }
            finally{camera.transform.position=position;camera.orthographicSize=size;}
            var portal=bootstrap.EntranceMarker.GetComponentInChildren<Stage3EntrancePortal>();
            var meshes=portal.GetComponentsInChildren<MeshFilter>();
            var renderers=portal.GetComponentsInChildren<MeshRenderer>();
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            File.WriteAllText(Folder+"/visual-status.txt",
                $"UTC={DateTime.UtcNow:O}\nplaying={EditorApplication.isPlaying}\nmeshes={meshes.Length}\ntriangles={meshes.Sum(m=>m.sharedMesh.triangles.Length/3)}\nworldBounds={bounds}\ncolliders={portal.GetComponentsInChildren<Collider>().Length}\n");
            MeasureCamera(camera,renderers);
            Debug.Log("S3_PORTAL_REVIEW_CAPTURED");
            if(SessionState.GetBool(Motion,false))
            {
                SessionState.SetBool(Motion,false);
                bootstrap.StartCoroutine(CaptureMotion(bootstrap,portal));
            }
        }

        private static IEnumerator CaptureMotion(Stage2SceneBootstrap bootstrap,Stage3EntrancePortal portal)
        {
            Directory.CreateDirectory(Folder+"/motion");
            var camera=bootstrap.CameraFollow.BoundCamera;
            var oldRate=Time.captureFramerate;
            var firstTime=portal.FlowTime;
            Time.captureFramerate=15;
            try
            {
                for(var frame=0;frame<90;frame++)
                {
                    yield return null;
                    var position=camera.transform.position;
                    var size=camera.orthographicSize;
                    try
                    {
                        SaveCamera(camera,$"{Folder}/motion/overview_{frame:D3}.png",960,540);
                        var focus=bootstrap.EntranceMarker.position+Vector3.back*1.4f;
                        camera.transform.position=focus-camera.transform.forward*12;
                        camera.orthographicSize=2.6f;
                        SaveCamera(camera,$"{Folder}/motion/close_{frame:D3}.png",600,500);
                    }
                    finally { camera.transform.position=position;camera.orthographicSize=size; }
                }
                File.WriteAllText(Folder+"/motion-status.txt",
                    $"UTC={DateTime.UtcNow:O}\nframes=90 fps=15\nflowStart={firstTime:F3} flowEnd={portal.FlowTime:F3}\nActual Game camera; fixed simulation step for recording.\n");
            }
            finally { Time.captureFramerate=oldRate; }
        }

        // Bounded local comparison, with synchronous readback. Not a release frame budget.
        private static void MeasureCamera(Camera camera,Renderer[] portal)
        {
            var target=RenderTexture.GetTemporary(1600,900,24);
            var oldTarget=camera.targetTexture; var oldActive=RenderTexture.active;
            var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);
            var watch=new System.Diagnostics.Stopwatch();
            var on=new double[30];var off=new double[30];
            try
            {
                camera.targetTexture=target;RenderTexture.active=target;
                for(var i=0;i<68;i++)
                {
                    bool visible=i%2==0;
                    foreach(var renderer in portal)renderer.enabled=visible;
                    watch.Restart();camera.Render();pixel.ReadPixels(new Rect(0,0,1,1),0,0);pixel.Apply();watch.Stop();
                    if(i>=8)(visible?on:off)[(i-8)/2]=watch.Elapsed.TotalMilliseconds;
                }
                Array.Sort(on);Array.Sort(off);
                File.WriteAllText(Folder+"/render-comparison.txt",
                    $"Camera.Render + synchronous 1px readback, Editor, 1600x900, 4 warmups per state, 30 samples per state\nGPU={SystemInfo.graphicsDeviceName}\nportalOnMedianMs={on[15]:F3}\nportalOffMedianMs={off[15]:F3}\nportalOnP95Ms={on[28]:F3}\nportalOffP95Ms={off[28]:F3}\nNot a standalone or minimum-target frame budget.\n");
            }
            finally
            {
                foreach(var renderer in portal)renderer.enabled=true;
                camera.targetTexture=oldTarget;RenderTexture.active=oldActive;
                RenderTexture.ReleaseTemporary(target);UnityEngine.Object.DestroyImmediate(pixel);
            }
        }

        private static void SaveCamera(Camera camera,string path,int width,int height)
        {
            var target=RenderTexture.GetTemporary(width,height,24);
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=oldTarget;RenderTexture.active=oldActive;
                RenderTexture.ReleaseTemporary(target);UnityEngine.Object.DestroyImmediate(image);
            }
        }

        private sealed class Results:ICallbacks
        {
            public void RunStarted(ITestAdaptor tests){}
            public void TestStarted(ITestAdaptor test){}
            public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result)
            {
                if(!SessionState.GetBool(Running,false))return;
                SessionState.SetBool(Running,false);
                TestRunnerApi.SaveResultToFile(result,Folder+"/playmode.xml");
                var state=result.PassCount+result.FailCount==0 ? "INVALID: no tests executed" : result.ResultState;
                File.WriteAllText(Folder+"/status.txt",$"{state}: passed={result.PassCount} failed={result.FailCount} duration={result.Duration:F2}s");
                // The framework restores its scene after this callback. A later
                // explicit request opens the review, so cleanup cannot race Play.
                nextPoll=EditorApplication.timeSinceStartup+3;
            }
        }
    }
}
