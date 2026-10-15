using System;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    // Explicit local review requests let the existing Editor verify its imported assets.
    public static class Stage3SoulSphereReview
    {
        private const string Folder = "Artifacts/S3-SOUL-SPHERE";
        private const string Request = Folder + "/review.request";
        private const string Running = "Not3A.SoulReview.Tests";
        private const string Show = "Not3A.SoulReview.Show";
        private static TestRunnerApi runner;
        private static double nextPoll;
        private static double captureAt;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            if (Application.isBatchMode) return;
            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Results());
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Show,false))
                    captureAt=EditorApplication.timeSinceStartup+10;
            };
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup<nextPoll) return;
            nextPoll=EditorApplication.timeSinceStartup+1;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (captureAt>0 && EditorApplication.timeSinceStartup>captureAt && EditorApplication.isPlaying)
            {
                if (ShaderUtil.anythingCompiling) return;
                // A background Editor can advance wall time before Start's first frame.
                // Do not publish an empty capture while gameplay presentation is pending.
                if (UnityEngine.Object.FindFirstObjectByType<Stage3SoulSphereMotion>()==null) return;
                captureAt=0; SessionState.SetBool(Show,false);
                Capture();
            }
            if (SessionState.GetBool(Running,false) || !File.Exists(Request)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying=false;
                return;
            }
            var command=File.ReadAllText(Request).Trim();
            File.Delete(Request);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                Stage3RapidPilotBuilder.Build3DGameAssets();
                if (command=="validate")
                {
                    SessionState.SetBool(Running,true);
                    File.WriteAllText(Folder+"/status.txt","Running Stage 3 PlayMode suite");
                    runner.Execute(new ExecutionSettings(new Filter {
                        testMode=TestMode.PlayMode,
                        assemblyNames=new[]{"Not3A.Stage3.VisualPilot.PlayModeTests"}
                    }));
                }
                else Open();
            }
            catch(Exception error)
            {
                SessionState.SetBool(Running,false);
                File.WriteAllText(Folder+"/status.txt",error.ToString());
                Debug.LogException(error);
            }
        }

        private static void Open()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Stage3Rapid3DGamePilot.unity");
            SessionState.SetBool(Show,true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying=true;
        }

        private static void Capture()
        {
            var bootstrap=UnityEngine.Object.FindFirstObjectByType<Stage2SceneBootstrap>();
            if (bootstrap==null) return;
            var camera=bootstrap.CameraFollow.BoundCamera;
            SaveCamera(camera,"art/review/soul_sphere_game_overview.png",1600,1000);
            var position=camera.transform.position;
            var size=camera.orthographicSize;
            try
            {
                var focus=bootstrap.ThroneMarker.position+Vector3.back*.55f;
                camera.transform.position=focus-camera.transform.forward*12;
                camera.orthographicSize=2.05f;
                SaveCamera(camera,"art/review/soul_sphere_game_close.png",1200,1000);
            }
            finally { camera.transform.position=position; camera.orthographicSize=size; }
            var meshes=bootstrap.ThroneMarker.GetComponentsInChildren<MeshFilter>();
            int triangles=0;
            foreach(var mesh in meshes) triangles+=mesh.sharedMesh.triangles.Length/3;
            File.WriteAllText(Folder+"/visual-status.txt",$"UTC={DateTime.UtcNow:O}\nplaying={EditorApplication.isPlaying}\nmeshes={meshes.Length}\ntriangles={triangles}\n");
            Debug.Log($"S3_SOUL_REVIEW_CAPTURED meshes={meshes.Length} triangles={triangles}");
        }

        private static void SaveCamera(Camera camera,string path,int width,int height)
        {
            var target=RenderTexture.GetTemporary(width,height,24);
            var oldTarget=camera.targetTexture;
            var oldActive=RenderTexture.active;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=oldTarget; RenderTexture.active=oldActive;
                RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(image);
            }
        }

        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool(Running,false)) return;
                SessionState.SetBool(Running,false);
                TestRunnerApi.SaveResultToFile(result,Folder+"/playmode.xml");
                File.WriteAllText(Folder+"/status.txt",$"{result.ResultState}: passed={result.PassCount} failed={result.FailCount} duration={result.Duration:F2}s");
                File.WriteAllText(Request,"open");
            }
        }
    }
}
