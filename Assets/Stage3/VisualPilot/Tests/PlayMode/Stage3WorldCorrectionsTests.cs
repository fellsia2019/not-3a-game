#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3WorldCorrectionsTests
    {
        private Stage2GameComposition game;private Stage2SceneBootstrap scene;
        private Component travel,world,binding,hud;private InputTestFixture input;
        private static T Read<T>(object o,string p)=>(T)o.GetType().GetProperty(p).GetValue(o);
        private static object Call(object o,string m,params object[] a)=>o.GetType().GetMethod(m).Invoke(o,a);
        private IEnumerable<Stage2GuildEnemy> Units=>Read<IReadOnlyList<Stage2GuildEnemy>>(world,"Enemies");
        [UnitySetUp]public IEnumerator Setup()
        {
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();Time.timeScale=1;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var n=0;n<20;n++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            travel=game.GetComponent("Stage3WorldTravel");world=game.GetComponent("Stage3GuildWorldDriver");
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");
            world.GetType().GetProperty("AutomaticTick").SetValue(world,false);
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;
            game.GetComponent<Stage2HeroProgressionDriver>().AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {foreach(var r in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(r);yield return null;yield return null;input.TearDown();Time.timeScale=1;}
        [UnityTest]public IEnumerator CampTreesAndRocksYieldActualStockAndRemoveTheirVisibleSolidThenReset()
        {
            var nodes=Read<IEnumerable<Stage2ResourceNode>>(world,"HarvestableCampResources").ToArray();
            Assert.That(nodes.Length,Is.EqualTo(46));var economy=game.Gathering.Economy;
            economy.SetManualToolStats(999,999,1,1);
            foreach(var kind in new[]{MapResourceKind.Wood,MapResourceKind.Stone})
            {
                var node=nodes.First(n=>n.Resource==kind);Assert.That(economy.Nodes.Contains(node),Is.True);
                var sprite=game.GetComponentsInChildren<MonoBehaviour>().Single(c=>c.GetType().Name=="Stage3ResourceSprite"&&Read<Stage2ResourceNode>(c,"Node")==node);
                var body=Read<SpriteRenderer>(sprite,"Body");var old=body.sprite;var pose=body.transform.localToWorldMatrix;
                var before=economy.Inventory.Get(kind);var actual=0;
                void Grant(Stage2ResourceNode n,int amount){if(n==node)actual+=amount;}
                economy.ResourceGranted+=Grant;
                Assert.That(economy.AdvanceGather(node,node.WorldPosition,true,100,true),Is.True);
                economy.ResourceGranted-=Grant;yield return null;yield return null;
                Assert.That(node.IsDepleted,Is.True);Assert.That(actual,Is.EqualTo(node.Capacity));
                Assert.That(economy.Inventory.Get(kind)-before,Is.EqualTo(actual));Assert.That(body.enabled,Is.False);
                Assert.That(body.sprite,Is.SameAs(old));Assert.That(body.transform.localToWorldMatrix,Is.EqualTo(pose));
                Assert.That(scene.PlayerMover.Obstacles.IsBlocked(node.WorldPosition,.1f),Is.False,"No permanent decorative footprint may remain.");
            }
            game.RunDriver.ResetRun();yield return null;yield return null;
            Assert.That(nodes.All(n=>!n.IsDepleted),Is.True);
            foreach(var node in nodes)
            {
                var sprite=game.GetComponentsInChildren<MonoBehaviour>().Single(c=>c.GetType().Name=="Stage3ResourceSprite"&&Read<Stage2ResourceNode>(c,"Node")==node);
                Assert.That(Read<SpriteRenderer>(sprite,"Body").enabled,Is.True);
            }
        }
        [UnityTest]public IEnumerator CampPackStandsBesideItsShelterAndLethalFirstHitAlertsAllAlliesOnly()
        {
            var pack=Units.Where(u=>u.Profile.enemyKind==Stage2GuildEnemyKind.Orc).ToArray();
            var centre=(Vector2)Call(world,"SpotPosition",0);var expected=centre+new Vector2(9,7);
            Assert.That(pack.All(u=>Vector2.Distance(u.Home,expected)<4),Is.True);
            Assert.That(pack.Max(u=>pack.Max(v=>Vector2.Distance(u.Home,v.Home))),Is.LessThan(5));
            scene.PlayerMover.transform.position=expected+Vector2.down*4;
            Assert.That(Call(world,"Damage",pack[0],pack[0].Health),Is.True);
            Assert.That(pack.Skip(1).All(u=>u.Engaged),Is.True);
            Assert.That(Units.Where(u=>u.Profile.enemyKind==Stage2GuildEnemyKind.Ogre).All(u=>!u.Engaged),Is.True);
            scene.PlayerMover.transform.position=expected+Vector2.down*40;
            for(var n=0;n<160;n++)Call(world,"Tick",.1f);
            Assert.That(pack.Skip(1).All(u=>!u.Engaged&&Vector2.Distance(u.Position,u.Home)<.11f),Is.True);
            yield return null;
        }
        [UnityTest]public IEnumerator EmptyFutureSitesAndDepletedResourceDoNotBlockBuildingsOrRegrowInsideThem()
        {
            var workshop=Read<Vector2>(travel,"WorkshopPosition");var futureCircle=Read<Vector2>(travel,"BasePosition");
            scene.PlayerMover.transform.position=workshop+Vector2.down*12;
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Wood,100);
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Stone,100);
            game.Gathering.Economy.Inventory.TryAdd(MapResourceKind.Metal,100);
            Stage3TestCycle.FundBaseConstruction(game);Call(travel,"BeginWorkshopPlacement");Assert.That(Call(travel,"EvaluateWorkshopAt",futureCircle).ToString(),Is.EqualTo("Ready"));
            Call(travel,"CancelConstructionPlacement");
            var dwelling=game.GetComponent("Stage3Dwelling");Call(dwelling,"BeginPlacement");
            Assert.That(Call(dwelling,"EvaluateAt",workshop).ToString(),Is.EqualTo("Ready"));
            Assert.That(Call(dwelling,"EvaluateAt",futureCircle).ToString(),Is.EqualTo("Ready"));Call(dwelling,"CancelPlacement");
            Stage3TestCycle.StudyBlueprint(game, Stage2BlueprintKind.ForgeConstruction);
            var forge=game.GetComponent("Stage3Forge");Call(forge,"BeginPlacement");
            Assert.That(Call(forge,"EvaluateAt",workshop).ToString(),Is.EqualTo("Ready"));Call(forge,"CancelPlacement");
            var economy=game.Gathering.Economy;
            var source=game.Gathering.Catalog.Get(MapResourceKind.Wood);
            var node=new Stage2ResourceNode(source,scene.MapDefinition.WorldToCell(workshop),workshop,Stage2ResourceSize.Small,0);
            economy.ReplaceAdditionalNodes(economy.Nodes.Skip(3).Concat(new[]{node}).ToArray());
            var collisions=game.GetComponent("Stage3ManualResourceCollisions");
            Call(collisions,"Bind",economy,scene.PlayerMover.Obstacles,scene.PlayerMover,.18f);
            Stage3TestCycle.FundBaseConstruction(game);Call(travel,"BeginWorkshopPlacement");Assert.That(Call(travel,"EvaluateWorkshopAt",workshop).ToString(),Is.EqualTo("Occupied"));
            economy.SetManualToolStats(999,999,1,1);Assert.That(economy.AdvanceGather(node,workshop,true,100,true),Is.True);
            Call(collisions,"RefreshNow");Assert.That(Call(travel,"EvaluateWorkshopAt",workshop).ToString(),Is.EqualTo("Ready"));
            Assert.That(Call(travel,"TryBuildWorkshopAt",workshop).ToString(),Is.EqualTo("Built"));
            economy.Tick(node.RecoverySeconds+1,true);Assert.That(node.IsDepleted,Is.True,"Reclaimed building ground must not sprout a hidden tree.");
            yield return null;
        }
        [UnityTest]public IEnumerator ClickingActualSealEdgeOpensChoiceWithoutTravelAndGuardsRemoteClick()
        {
            var site=Read<Vector2>(travel,"WorkshopPosition");scene.PlayerMover.transform.position=site+Vector2.down*10;
            Stage3TestCycle.FundBaseConstruction(game);Call(travel,"BeginWorkshopPlacement");Assert.That(Call(travel,"TryBuildWorkshopAt",site).ToString(),Is.EqualTo("Built"));
            scene.PlayerMover.transform.position=Read<Vector2>(travel,"WorkshopInteractionPosition");
            Assert.That(Call(travel,"TryStudyTeleport"),Is.True);
            var home=Read<Vector2>(travel,"BasePosition");Call(travel,"BeginBaseCirclePlacement");
            Assert.That(Call(travel,"TryBuildBaseCircleAt",home).ToString(),Is.EqualTo("Built"));
            var city=Read<Vector2>(travel,"RemotePosition");scene.PlayerMover.transform.position=city;yield return null;
            scene.CameraFollow.enabled=false;var camera=scene.CameraFollow.BoundCamera;camera.orthographicSize=5;
            foreach(var point in new[]{home,city})
            {
                var near=point+Vector2.left*1.5f;scene.PlayerMover.transform.position=near;
                camera.transform.position=(Vector3)point-camera.transform.forward*30;yield return null;
                var z=(float)travel.GetType().Assembly.GetType("Not3A.Stage3.VisualPilot.Stage3TacticalPresentation")
                    .GetMethod("GrassSurfaceZ",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{point});
                var hit=new Vector3(point.x,point.y,z-(point==city ? .05f : .018f));
                var screen=(Vector2)camera.WorldToScreenPoint(hit);
                Assert.That(Call(travel,"TryOpenTeleportAtScreen",screen),Is.True);
                Assert.That(Read<bool>(hud,"TeleportsOpen"),Is.True);Assert.That((Vector2)scene.PlayerMover.transform.position,Is.EqualTo(near));
                Call(hud,"CloseWindow");scene.PlayerMover.transform.position=point+Vector2.left*8;
                Assert.That(Call(travel,"TryOpenTeleportAtScreen",screen),Is.False);
            }
        }
    }
}
#endif
