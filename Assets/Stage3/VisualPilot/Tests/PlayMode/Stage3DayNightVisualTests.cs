#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3DayNightVisualTests
    {
        private const string Evidence="Artifacts/S3-DAY-NIGHT-20261018/visual";
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component home;
        private Camera camera;
        private int previousCaptureRate;
        private object Read(string name) => home.GetType().GetProperty(name).GetValue(home);
        private object Call(string name,params object[] args) => home.GetType().GetMethod(name).Invoke(home,args);

        [UnitySetUp] public IEnumerator Load()
        {
            input=new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            previousCaptureRate=Time.captureFramerate; Time.captureFramerate=30;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<8;i++) yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>(); scene=game.GetComponent<Stage2SceneBootstrap>();
            Assert.That(game.IsInitialized,Is.True); Assert.That(game.RunDriver.Run.ContinuousDays,Is.True);
            game.RunDriver.enabled=false; game.SetPaused(false); game.HeroCombat.AutomaticTick=false;
            home=game.GetComponent("Stage3Dwelling"); Assert.That(home,Is.Not.Null); Assert.That(Read("IsReady"),Is.True);
            camera=scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled=false;
            camera.aspect=1600f/900f; Directory.CreateDirectory(Evidence);
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.captureFramerate=previousCaptureRate;
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale=1f;
        }

        [UnityTest] public IEnumerator ApprovedHeroDwellingAndMoonlightRenderAcrossRealDayNightDawn()
        {
            var hud=game.GetComponent<Stage2GameHud>();
            Assert.That(hud.DwellingTabRect.xMin,Is.GreaterThan(hud.ResearchTabRect.xMax));
            Assert.That(hud.DwellingTabRect.xMax,Is.LessThanOrEqualTo(hud.BuildRect.xMax));
            Assert.That(hud.CanSelectDwelling,Is.False);
            hud.AddAllResources(10); hud.SelectDwelling();
            Assert.That(hud.BuildCategory,Is.EqualTo(Stage2BuildCategory.Home));
            Assert.That(Read("IsPlacing"),Is.True);
            var map=scene.MapDefinition; var site=Vector2.zero; var found=false; var nearest=float.PositiveInfinity;
            var origin=(Vector2)scene.ThroneMarker.position;
            for(var y=map.MinimumCell.y+3;y<=map.MaximumCell.y-3;y++)
            for(var x=map.MinimumCell.x+3;x<=map.MaximumCell.x-3;x++)
            {
                var point=map.CellToWorld(new Vector2Int(x,y)); var distance=(point-origin).sqrMagnitude;
                if(distance>=nearest || Call("EvaluateAt",point).ToString()!="Ready") continue;
                site=point; nearest=distance; found=true;
            }
            Assert.That(found,Is.True,"An actual legal green-ground site must exist.");
            Frame(site+Vector2.down*.5f,5f);
            input.Set(Mouse.current.position,(Vector2)camera.WorldToScreenPoint(site));
            yield return null; yield return null;
            var preview=GameObject.Find("Hero dwelling preview"); Assert.That(preview,Is.Not.Null);
            Assert.That(preview.GetComponent<SpriteRenderer>().color.g,Is.GreaterThan(preview.GetComponent<SpriteRenderer>().color.r));
            var registration=preview.transform.position;
            Capture("dwelling-preview-day.png");
            Assert.That(Call("TryBuildAt",site).ToString(),Is.EqualTo("Built"));
            yield return null;
            var built=GameObject.Find("Hero dwelling B"); Assert.That(built,Is.Not.Null);
            Assert.That(Vector3.Distance(registration,built.transform.position),Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(built.transform.rotation,camera.transform.rotation),Is.LessThan(.001f));
            scene.PlayerMover.transform.position=(Vector2)Read("InteractionPosition");
            yield return null; yield return null;
            Assert.That(hud.DwellingInteractionHint,Is.Not.Null);
            var hero=GameObject.Find("Hero C body"); var equipment=GameObject.Find("Starter bow equipment");
            Assert.That(hero,Is.Not.Null); Assert.That(equipment,Is.Not.Null);
            Assert.That(GameObject.Find("Hero scale marker"),Is.Null);
            foreach(var renderer in new[]{hero.GetComponent<SpriteRenderer>(),equipment.GetComponent<SpriteRenderer>(),built.GetComponent<SpriteRenderer>()})
            { Assert.That(renderer.sprite,Is.Not.Null); Assert.That(renderer.sharedMaterial.shader.isSupported,Is.True); }
            var day=Capture("dwelling-hero-day.png");
            var run=game.RunDriver.Run;
            Assert.That(Call("TrySkipDay"),Is.True);
            Assert.That(Stage3TestCycle.StartNight(game.RunDriver.Run),Is.True);
            run.Tick(.05f);
            for(var i=0;i<game.Enemies.ActiveCount;i++) game.Enemies.GetEnemy(i).SetAutomaticTick(false);
            for(var i=0;i<22;i++) yield return null;
            Assert.That(run.IsNight,Is.True);
            Assert.That(Shader.GetGlobalFloat("_Not3ANightAmount"),Is.EqualTo(1f).Within(.001f));
            Assert.That(hud.DwellingInteractionHint,Does.Contain("боя"));
            var night=Capture("dwelling-hero-night.png");
            Assert.That(Mean(night),Is.LessThan(Mean(day)*.85f),"Moonlight must visibly change the world.");
            Assert.That(Mean(night),Is.GreaterThan(.025f),"Night must remain readable.");
            Frame(scene.EntranceMarker.position,5f); yield return null;
            Capture("portal-night.png");
            // QA-only fast completion of the actual first horde, then verify the real dawn transition.
            for(var step=0;step<4000 && run.Phase!=Stage2RunPhase.Preparation;step++)
            {
                run.Tick(.05f);
                for(var i=game.Enemies.ActiveCount-1;i>=0;i--)
                { var enemy=game.Enemies.GetEnemy(i); enemy.SetAutomaticTick(false); enemy.TakeDamage(enemy.Health); }
            }
            Assert.That(run.IsDay,Is.True,"Streaming night stops at dawn, then waits for the remaining enemies.");
            Assert.That(run.WaveNumber,Is.EqualTo(2));
            Assert.That(run.IsDay,Is.True);
            Frame(site+Vector2.down*.5f,5f);
            for(var i=0;i<22;i++) yield return null;
            Assert.That(Shader.GetGlobalFloat("_Not3ANightAmount"),Is.EqualTo(0f).Within(.001f));
            var dawn=Capture("dwelling-hero-dawn.png");
            Assert.That(Mean(dawn),Is.GreaterThan(Mean(night)*1.25f));
            // Four approved directions and independently held equipment, at the approved camera angle.
            var actor=scene.PlayerMover.transform; var center=actor.position;
            var directions=new[]{Vector3.right,Vector3.up,Vector3.left,Vector3.down};
            var avatar=actor.GetComponent("Stage3HeroAvatar");
            for(var direction=0;direction<4;direction++)
            {
                actor.position=center; yield return null;
                actor.position+=directions[direction]*.2f; yield return null;
                Frame(actor.position+Vector3.back*.55f,2.5f);
                Assert.That(avatar.GetType().GetProperty("Direction").GetValue(avatar),Is.EqualTo(direction));
                Capture("hero-C-direction-"+direction+".png");
            }
            File.WriteAllText(Path.Combine(Evidence,"context.txt"),
                "Actual scene camera renders; batch captures exclude IMGUI HUD. QA granted 10 resources for placement, no new gameplay values.\n"+
                "Real day->home rest->night->QA first-horde kills->QA clock advance to dawn (no runtime early dawn). Four direction snapshots are manual actor motion for art registration.\n"+
                $"house site={site}; unchanged camera rotation={camera.transform.eulerAngles}; day luminance={Mean(day):F3}; night={Mean(night):F3}; dawn={Mean(dawn):F3}; graphics={SystemInfo.graphicsDeviceType}\n");
        }

        private void Frame(Vector3 center,float zoom)
        { camera.orthographicSize=zoom; camera.transform.position=center-camera.transform.forward*30f; }
        private static float Mean(Color32[] pixels) => pixels.Average(p=>(p.r*.2126f+p.g*.7152f+p.b*.0722f)/255f);
        private Color32[] Capture(string filename)
        {
            var target=new RenderTexture(1600,900,24); var old=camera.targetTexture; var active=RenderTexture.active;
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1600,900),0,0); image.Apply();
                var pixels=image.GetPixels32();
                Assert.That(pixels.Count(p=>p.r>245 && p.b>245 && p.g<10),Is.LessThan(5));
                File.WriteAllBytes(Path.Combine(Evidence,filename),image.EncodeToPNG()); return pixels;
            }
            finally { camera.targetTexture=old; RenderTexture.active=active; Object.Destroy(target); Object.Destroy(image); }
        }
    }
}
#endif
