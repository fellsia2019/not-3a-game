#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    // Capture readiness in the first frame BEFORE rendering, rather than waiting
    // several frames for the scene under test to repair its initial presentation.
    [DefaultExecutionOrder(30000)]
    public sealed class Stage3StartupFrameProbe : MonoBehaviour
    {
        public Action Observe;
        private void LateUpdate()
        {
            var observe=Observe;Observe=null;observe?.Invoke();
        }
    }

    public sealed class Stage3StartupTests
    {
        private const string PlayableScene="Assets/Scenes/Stage3Rapid3DGamePilot.unity";
        private const string LegacyScene="Assets/Scenes/Stage2VerticalSlice.unity";
        private bool observed;
        private readonly List<string> failures=new List<string>();
        private float previousTimeScale;

        [SetUp]
        public void SetUp() {previousTimeScale=Time.timeScale;Time.timeScale=1f;}

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;Time.timeScale=previousTimeScale;
        }

        [UnityTest]
        public IEnumerator DirectStartAndReloadHaveFinalWorldAndHudInTheirFirstFrame()
        {
            Assert.That(Array.Find(EditorBuildSettings.scenes,entry=>entry.enabled)?.path,Is.EqualTo(PlayableScene),
                "A normal Unity build must enter the current prototype, not the legacy Stage2 fixture.");
            for(var launch=0;launch<2;launch++)
            {
                yield return FirstFrame(PlayableScene,true);
                var game=Object.FindAnyObjectByType<Stage2GameComposition>();
                var scene=game.GetComponent<Stage2SceneBootstrap>();
                Assert.That(game.RunDriver.Run.IsPaused,Is.False,"Direct start must not wait in a new menu/pause.");
                Assert.That(scene.PlayerMover.enabled,Is.True);
                Assert.That(game.Building.Defense.TowerCount,Is.Zero,"Start must not inject review buildings.");
                var preparation=game.RunDriver.Run.PreparationRemaining;
                yield return new WaitForSeconds(.05f);
                Assert.That(game.RunDriver.Run.PreparationRemaining,Is.LessThan(preparation),"The normal timer continues.");
            }
        }

        [UnityTest]
        public IEnumerator LegacyStage2KeepsItsOwnVisibleMarkersAndHud()
        {
            yield return FirstFrame(LegacyScene,false);
        }

        private IEnumerator FirstFrame(string path,bool currentPresentation)
        {
            observed=false;failures.Clear();
            void ObserveScene(Scene loaded,LoadSceneMode mode)
            {
                if(loaded.path!=path)return;
                var probe=new GameObject("First-frame readiness probe").AddComponent<Stage3StartupFrameProbe>();
                SceneManager.MoveGameObjectToScene(probe.gameObject,loaded);
                probe.Observe=()=>
                {
                    observed=true;
                    try {Inspect(currentPresentation);}
                    catch(Exception exception) {failures.Add(exception.ToString());}
                };
            }
            SceneManager.sceneLoaded+=ObserveScene;
            try
            {
                EditorSceneManager.LoadSceneInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
                for(var frame=0;frame<8&&!observed;frame++)yield return null;
                Assert.That(observed,Is.True,"The first LateUpdate must be observed, not a warmed-up scene.");
                Assert.That(failures,Is.Empty,string.Join("\n",failures));
            }
            finally {SceneManager.sceneLoaded-=ObserveScene;}
        }

        private void Inspect(bool current)
        {
            void Require(bool value,string message) {if(!value)failures.Add(message);}
            bool Ready(Component component) => component!=null &&
                (bool)(component.GetType().GetProperty("IsInitialized")?.GetValue(component) ?? false);
            bool VisibleBody(Component component)
            {
                var body=component?.GetType().GetProperty("Body")?.GetValue(component) as SpriteRenderer;
                return body!=null && body.enabled && body.gameObject.activeInHierarchy && body.sprite!=null;
            }
            var game=Object.FindAnyObjectByType<Stage2GameComposition>();
            Require(game!=null && game.IsInitialized,"Rules must be initialized before the first render.");
            if(game==null)return;
            var scene=game.GetComponent<Stage2SceneBootstrap>();
            var legacy=game.GetComponent<Stage2GameHud>();
            Require(legacy!=null && legacy.HasPlayerPresentation==current,"Exactly the intended HUD must own the first frame.");
            var markerRoots=new[]{scene.PlayerMover.transform,scene.ThroneMarker,scene.EntranceMarker};
            var names=new[]{"Cyan Non-Combat Hero Block","Gold Throne Block","Orange Entrance Diamond"};
            for(var i=0;i<names.Length;i++)
            {
                var marker=markerRoots[i].Find(names[i]);
                var renderer=marker!=null?marker.GetComponent<SpriteRenderer>():null;
                Require(renderer!=null && renderer.enabled!=current,"Wrong initial marker visibility: "+names[i]);
            }
            Require(scene.MapDebugRenderer.GetComponent<MeshRenderer>().enabled!=current,"Wrong initial legacy ground visibility.");
            if(!current)return;
            Require(Ready(game.GetComponent("Stage3TacticalPresentation")),"Final world must be ready in its first frame.");
            Require(VisibleBody(scene.PlayerMover.GetComponent("Stage3HeroAvatar")),"Final hero artwork must already exist.");
            Require(VisibleBody(scene.ThroneMarker.GetComponentInChildren(Type.GetType("Not3A.Stage3.VisualPilot.Stage3LandmarkSprite, Assembly-CSharp"))),"Final throne artwork must already exist.");
            Require(VisibleBody(scene.EntranceMarker.GetComponentInChildren(Type.GetType("Not3A.Stage3.VisualPilot.Stage3LandmarkSprite, Assembly-CSharp"))),"Final portal artwork must already exist.");
            var ui=game.GetComponent("Stage3FantasyHud");
            Require(ui!=null && (bool)ui.GetType().GetProperty("IsReady").GetValue(ui),"Current HUD must already be bound.");
            var minimap=game.GetComponent<Stage2Minimap>();
            Require(minimap!=null && minimap.SuppressPresentation,"Legacy minimap must not flash over the current HUD.");
            foreach(var kind in new[]{"Wood","Stone","Metal"})
            {
                var marker=game.transform.Find(kind+" Resource Marker");
                Require(marker!=null,"Resource rule marker missing: "+kind);
                if(marker==null)continue;
                foreach(var body in marker.GetComponentsInChildren<SpriteRenderer>())
                    if(body.transform.parent==marker)Require(!body.enabled,"Legacy resource glyph is still visible: "+kind);
            }
        }
    }
}
#endif
