#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3CartoonWorldTests
    {
        [UnityTest]
        public IEnumerator ActualScene_NewSurfacesTransparentTowersAndOrdinaryCombat()
        {
            var input=new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>();
            var oldRate=Time.captureFramerate;
            try
            {
                EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
                yield return null; yield return null; yield return null;
                var composition=Object.FindAnyObjectByType<Stage2GameComposition>();
                var bootstrap=composition.GetComponent<Stage2SceneBootstrap>();
                Assert.That(bootstrap.MapDefinition.Size,Is.EqualTo(new Vector2Int(100,100)));
                Assert.That(bootstrap.MapDefinition.EnemyEntrance,Is.EqualTo(new Vector2Int(-42,-30)));
                Assert.That(bootstrap.MapDefinition.Throne,Is.EqualTo(new Vector2Int(39,12)));
                var ground=GameObject.Find("Tactical grass and paved build cells").GetComponent<MeshRenderer>();
                Assert.That(ground.sharedMaterials[0].GetTexture("_BaseMap").name,Is.EqualTo("CartoonMeadowV2"));
                Assert.That(ground.sharedMaterials[1].GetTexture("_BaseMap").name,Is.EqualTo("CartoonPavingV2"));
                foreach(var path in new[]{"RapidGroundedV2","LongshotPartsV2","RoadsidePropsV2"})
                {
                    var png=new Texture2D(2,2,TextureFormat.RGBA32,false);
                    try
                    {
                        png.LoadImage(File.ReadAllBytes("Assets/Stage3/CartoonWorldPilot/"+path+".png"));
                        Assert.That(png.GetPixel(0,0).a,Is.LessThan(.01f));
                        Assert.That(png.GetPixels32().Count(p=>p.a>127),Is.GreaterThan(100000));
                    }
                    finally { Object.Destroy(png); }
                }
                var towers=Object.FindObjectsByType<Stage2Tower>();
                Assert.That(towers.Length,Is.EqualTo(2));
                var catapult=towers.Single(t=>t.name.Contains("Longshot"));
                var body=catapult.GetComponentsInChildren<SpriteRenderer>().First(r=>r.enabled);
                var camera=bootstrap.CameraFollow.BoundCamera;
                bootstrap.CameraFollow.enabled=false;
                var focus=bootstrap.ThroneMarker.position+Vector3.back*.6f;
                camera.transform.position=focus-camera.transform.forward*14f;
                camera.orthographicSize=6f;
                const string output="Artifacts/S3-ASTRA-VISUAL/";
                Directory.CreateDirectory(output+"frames");
                Directory.CreateDirectory("art/review");
                Time.captureFramerate=15;
                composition.RunDriver.Run.StartWave();
                // Advance the long, empty approach through normal rule/movement ticks.
                // Capture the same ten seconds of combat once the first creep is nearby.
                composition.RunDriver.enabled=false;
                composition.RunDriver.Run.SetPaused(false); // OnDisable pauses the run; this test owns the clock.
                bool approached=false;
                for(var step=0;step<3000 && !approached;step++)
                {
                    composition.RunDriver.Run.Tick(.1f);
                    for(var i=composition.Enemies.ActiveCount-1;i>=0;i--)
                    {
                        var enemy=composition.Enemies.GetEnemy(i);
                        enemy.SetAutomaticTick(false);
                        enemy.Tick(.1f);
                        approached|=Vector2.Distance(enemy.transform.position,bootstrap.ThroneMarker.position)<=6f;
                    }
                }
                Assert.That(approached,Is.True,"The wave must traverse the authored winding road.");
                for(var i=0;i<composition.Enemies.ActiveCount;i++)composition.Enemies.GetEnemy(i).SetAutomaticTick(true);
                composition.RunDriver.enabled=true;
                bool fired=false,arrow=false;
                for(var frame=0;frame<150;frame++)
                {
                    yield return null;
                    fired|=GameObject.Find("Stone in flight")!=null;
                    arrow|=GameObject.Find("Arrow in flight")!=null;
                    Capture(camera,output+$"frames/frame_{frame:D3}.png",960,540);
                    if(frame%3==0) { var oldSize=camera.orthographicSize; var oldPosition=camera.transform.position; camera.orthographicSize=1.5f; camera.transform.position=catapult.transform.position+Vector3.back*.8f-camera.transform.forward*14f; Capture(camera,output+$"frames/catapult_{frame:D3}.png",800,700); camera.orthographicSize=oldSize; camera.transform.position=oldPosition; }
                    if(frame==90) Capture(camera,"art/review/astra_world_game_overview.png",1600,900);
                }
                Assert.That(fired,Is.True,"Normal wave must reach and trigger the catapult.");
                Assert.That(arrow,Is.True,"Normal wave must trigger Rapid arrows.");
                foreach(var tower in towers)
                {
                    camera.orthographicSize=2.2f;
                    camera.transform.position=tower.transform.position+Vector3.back*.7f-camera.transform.forward*14f;
                    Capture(camera,"art/review/astra_"+(tower==catapult?"longshot":"rapid")+"_game_close.png",1200,1000);
                }
                File.WriteAllText(output+"capture.txt","150 actual game frames at 15 simulation fps, unchanged map/camera orientation, ordinary paid towers and first wave. Both tower roles fired. GPU="+SystemInfo.graphicsDeviceName);
            }
            finally
            {
                Time.captureFramerate=oldRate;
                foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
                input.TearDown();
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActualCatapult_ContinuousThrow60Fps()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition=Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.enabled=false;
            composition.RunDriver.Run.SetPaused(false);
            var towers=Object.FindObjectsByType<Stage2Tower>();
            foreach(var tower in towers) tower.enabled=false;
            var catapult=towers.Single(t=>t.name.Contains("Longshot"));
            var enemy=composition.Enemies.SpawnAt(catapult.Cell+new Vector2Int(2,-2));
            enemy.SetAutomaticTick(false);
            var bootstrap=composition.GetComponent<Stage2SceneBootstrap>();
            bootstrap.CameraFollow.enabled=false;
            var camera=bootstrap.CameraFollow.BoundCamera;
            camera.orthographicSize=1.8f;
            camera.transform.position=catapult.transform.position+new Vector3(.25f,0,-.8f)-camera.transform.forward*14f;
            const string output="Artifacts/S3-ASTRA-VISUAL/throw60/";
            Directory.CreateDirectory(output);
            var body=catapult.transform.Find("Tower artwork").GetComponent<SpriteRenderer>();
            var foundation=body.sprite;
            var foot=body.transform.position;
            var healthBefore=enemy.Health;
            catapult.Tick(.01f);
            for(var frame=0;frame<90;frame++)
            {
                composition.RunDriver.Run.Tick(1f/60f);
                yield return null;
                Assert.That(body.sprite,Is.SameAs(foundation));
                Assert.That(body.transform.position,Is.EqualTo(foot));
                Capture(camera,output+$"frame_{frame:D3}.png",960,840);
            }
            Assert.That(enemy.Health,Is.LessThan(healthBefore));
            File.WriteAllText(output+"capture.txt","90 actual Game-camera frames, 60 Hz existing run-owned combat clock, ordinary catapult shot and target, fixed camera orientation, no interpolated or repainted frames.");
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null;
        }

        private static void Capture(Camera camera,string path,int width,int height)
        {
            var oldTarget=camera.targetTexture; var oldActive=RenderTexture.active;
            var target=RenderTexture.GetTemporary(width,height,24);
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
                RenderTexture.ReleaseTemporary(target); Object.DestroyImmediate(image);
            }
        }
    }
}
#endif

