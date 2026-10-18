#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeroMotionVisualTests
    {
        private const string Evidence="Artifacts/S3-HERO-MOTION-20261018/visual";
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Camera camera;
        private Component avatar;
        private SpriteRenderer body,bow;
        private int previousCaptureRate;

        [UnitySetUp] public IEnumerator Load()
        {
            input=new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            previousCaptureRate=Time.captureFramerate; Time.captureFramerate=30;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<8;i++) yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>(); scene=game.GetComponent<Stage2SceneBootstrap>();
            Assert.That(game.IsInitialized,Is.True); game.RunDriver.enabled=false; game.SetPaused(false);
            game.HeroCombat.AutomaticTick=false; scene.CameraFollow.enabled=false; camera=scene.CameraFollow.BoundCamera;
            camera.aspect=1600f/900f; camera.orthographicSize=2.5f;
            avatar=scene.PlayerMover.GetComponent("Stage3HeroAvatar"); Assert.That(avatar,Is.Not.Null);
            body=scene.PlayerMover.transform.Find("Hero C body").GetComponent<SpriteRenderer>();
            bow=scene.PlayerMover.transform.Find("Starter bow equipment").GetComponent<SpriteRenderer>();
            Directory.CreateDirectory(Evidence);
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.captureFramerate=previousCaptureRate;
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale=1f;
        }

        [UnityTest] public IEnumerator PlantedIdleBreathesAndSideWalkingKeepsCorrectEquipmentRegistration()
        {
            var actor=scene.PlayerMover.transform; var start=actor.position;
            var approvedRotation=camera.transform.rotation;
            var directions=new[]{Vector3.right,Vector3.up,Vector3.left,Vector3.down};
            var maxMotion=0f;
            for(var direction=0;direction<4;direction++)
            {
                actor.position=start; yield return null;
                // Four actual walking poses, with no input/gameplay grants or alternate renderer.
                for(var step=0;step<4;step++)
                {
                    actor.position+=directions[direction]*.23f; yield return null;
                    Assert.That(avatar.GetType().GetProperty("IsMoving").GetValue(avatar),Is.True);
                    Assert.That(avatar.GetType().GetProperty("Direction").GetValue(avatar),Is.EqualTo(direction));
                    Frame();
                    if(direction==0 || direction==2) Capture("hero-walk-"+direction+"-frame-"+step+".png");
                }
                var walkSprite=body.sprite;
                yield return null; yield return null;
                Assert.That(avatar.GetType().GetProperty("IsMoving").GetValue(avatar),Is.False);
                Assert.That(body.sprite,Is.Not.SameAs(walkSprite),"Stopping must select a neutral planted-feet pose, not a walking frame.");
                var foot=body.transform.position; var idleSprite=body.sprite;
                var smallest=10f; var largest=0f;
                Color32[] inhale=null,exhale=null;
                for(var tick=0;tick<112;tick++)
                {
                    yield return null;
                    var scale=body.transform.localScale.y; smallest=Mathf.Min(smallest,scale); largest=Mathf.Max(largest,scale);
                    Assert.That(Vector3.Distance(body.transform.position,foot),Is.LessThan(.00001f),"Idle soles must stay registered to ground.");
                    Assert.That(body.sprite,Is.SameAs(idleSprite),"Breathing must not cycle walking feet.");
                    if(tick==24) inhale=Capture("hero-idle-"+direction+"-inhale.png");
                    if(tick==78) exhale=Capture("hero-idle-"+direction+"-exhale.png");
                }
                maxMotion=Mathf.Max(maxMotion,largest-smallest);
                Assert.That(largest-smallest,Is.InRange(.008f,.025f),"Breathing should be present but restrained.");
                var changed=0;
                for(var i=0;i<inhale.Length;i++)
                    if(Mathf.Abs(inhale[i].r-exhale[i].r)+Mathf.Abs(inhale[i].g-exhale[i].g)+Mathf.Abs(inhale[i].b-exhale[i].b)>18) changed++;
                Assert.That(changed,Is.GreaterThan(20),"Idle must produce a visible change in the actual rendered frame.");
                var scaleBefore=body.transform.localScale; var equipmentBefore=bow.transform.position; var rotationBefore=bow.transform.rotation;
                game.SetPaused(true);
                for(var tick=0;tick<10;tick++) yield return null;
                Assert.That(body.transform.localScale,Is.EqualTo(scaleBefore));
                Assert.That(Vector3.Distance(bow.transform.position,equipmentBefore),Is.LessThan(.00001f));
                Assert.That(Quaternion.Angle(bow.transform.rotation,rotationBefore),Is.LessThan(.001f));
                game.SetPaused(false);
            }
            Assert.That(Quaternion.Angle(camera.transform.rotation,approvedRotation),Is.LessThan(.001f));
            File.WriteAllText(Path.Combine(Evidence,"context.txt"),
                "Actual scene, approved camera rotation, manual actor movement for visual QA only; no resources/defenses/waves injected.\n"+
                "Four walking frames in each side direction; four neutral idle directions at inhale/exhale. Ground-pivot and pause checks.\n"+
                $"maximum breath scale range={maxMotion:F4}; camera zoom=2.5; graphics={SystemInfo.graphicsDeviceType}; camera renders exclude HUD.\n");
        }

        private void Frame() => camera.transform.position=scene.PlayerMover.transform.position+Vector3.back*.55f-camera.transform.forward*30f;
        private Color32[] Capture(string name)
        {
            var target=new RenderTexture(1600,900,24); var old=camera.targetTexture; var active=RenderTexture.active;
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            try
            {
                Assert.That(body.sprite,Is.Not.Null); Assert.That(bow.sprite,Is.Not.Null);
                Assert.That(body.sharedMaterial.shader.isSupported,Is.True); Assert.That(bow.sharedMaterial.shader.isSupported,Is.True);
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1600,900),0,0); image.Apply(); var pixels=image.GetPixels32();
                Assert.That(pixels.Count(p=>p.r>245 && p.b>245 && p.g<10),Is.LessThan(5));
                File.WriteAllBytes(Path.Combine(Evidence,name),image.EncodeToPNG()); return pixels;
            }
            finally { camera.targetTexture=old; RenderTexture.active=active; Object.Destroy(target); Object.Destroy(image); }
        }
    }
}
#endif
