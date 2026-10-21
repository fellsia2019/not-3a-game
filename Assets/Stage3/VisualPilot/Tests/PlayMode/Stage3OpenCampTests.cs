#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3OpenCampTests
    {
        private Stage2GameComposition game;private Stage2SceneBootstrap scene;
        private Component world,resources,travel,binding;private Stage2Guild guild;
        private static T Read<T>(object o,string name)=>(T)o.GetType().GetProperty(name).GetValue(o);
        private static object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name).Invoke(o,args);
        private static void Set(object o,string name,object value)=>o.GetType().GetProperty(name).SetValue(o,value);
        private IEnumerable<Stage2GuildEnemy> Units=>Read<IReadOnlyList<Stage2GuildEnemy>>(world,"Enemies");
        [UnitySetUp]public IEnumerator Setup()
        {
            Time.timeScale=1;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<20;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            world=game.GetComponent("Stage3GuildWorldDriver");resources=game.GetComponent("Stage3ResourceDistrictWorld");
            travel=game.GetComponent("Stage3WorldTravel");binding=game.GetComponent("Stage3PlayerInterfaceBinding");
            Assert.That(Read<bool>(world,"Ready"),Is.True);guild=Read<Stage2Guild>(binding,"Guild");
            Set(world,"AutomaticTick",false);game.HeroCombat.AutomaticTick=false;game.RunDriver.enabled=false;
            game.GetComponent<Stage2HeroProgressionDriver>().AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);yield return null;yield return null;Time.timeScale=1;}
        private void VerifyOpenGround()
        {
            var bounds=Read<IReadOnlyList<Rect>>(world,"ReservedCampBounds");
            Assert.That(bounds.Count,Is.EqualTo(2));Assert.That(bounds[0].Overlaps(bounds[1]),Is.False);
            var ground=scene.PlayerMover.Obstacles;
            for(var kind=0;kind<2;kind++)
            {
                var center=(Vector2)Call(world,"SpotPosition",kind);var area=bounds[kind];
                var city=Read<Rect>(travel,"CityBounds");
                Assert.That(area.Overlaps(new Rect(city.position-Vector2.one*20,city.size+Vector2.one*40)),Is.False,"Wild camps must not sit against a city doorstep.");
                var presentation=Call(world,"CampPresentation",kind);
                Assert.That(Read<bool>(presentation,"IsBuilt"),Is.True);
                Assert.That(Read<Rect>(presentation,"WorldBounds"),Is.EqualTo(area));
                Assert.That(Read<int>(presentation,"RenderedPropCount"),Is.GreaterThanOrEqualTo(6));
                for(var y=-9;y<=9;y++)for(var x=-9;x<=9;x++)
                    if(x*x+y*y<=81)Assert.That(ground.IsBlocked(center+new Vector2(x,y),.6f),Is.False,"Battle field must stay open, not only spawn points.");
                foreach(var direction in new[]{Vector2.left,Vector2.right,Vector2.down})
                {
                    var perpendicular=new Vector2(-direction.y,direction.x);
                    for(var lane=-2.5f;lane<=2.5f;lane+=.5f)
                    {
                        var start=center+perpendicular*lane;var step=direction*(direction.y==0?23f:21f);
                        var reached=ground.Move(start,step,.6f);
                        Assert.That(Vector2.Distance(reached,start+step),Is.LessThan(.02f),"Every retreat is broad enough to reposition, not a one-cell slit.");
                    }
                }
                foreach(var node in game.Gathering.Economy.Nodes)
                    Assert.That(area.Contains(node.WorldPosition),Is.False,"Generated trees, rock and ore must never fill the arena.");
                foreach(var unit in Units.Where(u=>(int)u.Profile.enemyKind==kind&&u.Alive))
                {Assert.That(Vector2.Distance(center,unit.Home),Is.LessThan(10));Assert.That(ground.IsBlocked(unit.Position,unit.Profile.radius),Is.False);}
            }
        }
        [UnityTest]public IEnumerator SeveralWorldSeedsKeepWideFieldsAndThreeBroadSweptRetreatRoutes()
        {
            VerifyOpenGround();
            foreach(var seed in new[]{164032,7307,99991})
            {
                Call(resources,"Regenerate",seed);for(var i=0;i<3;i++)yield return null;
                VerifyOpenGround();
                Assert.That(game.GetComponentsInChildren<Component>().Count(c=>c.GetType().Name=="Stage3CampPresentation"),Is.EqualTo(2));
            }
        }
        [UnityTest]public IEnumerator SolidSceneryBlocksSweepsButResetRetryAndRepeatedBindingNeverDuplicateIt()
        {
            VerifyOpenGround();var ground=scene.PlayerMover.Obstacles;
            var count=ground.Rectangles.Count;var presentations=new object[2];
            for(var kind=0;kind<2;kind++)
            {
                presentations[kind]=Call(world,"CampPresentation",kind);
                var center=(Vector2)Call(world,"SpotPosition",kind);
                var type=presentations[kind].GetType();
                var locals=(IReadOnlyList<Rect>)type.GetMethod("GetLocalFootprints").Invoke(null,new object[]{kind});
                foreach(var local in locals)
                {
                    var solid=new Rect(center+local.position,local.size);
                    Assert.That(ground.IsBlocked(solid.center,.35f),Is.True);
                    var start=new Vector2(solid.center.x,solid.yMin-1f);
                    if(ground.IsBlocked(start,.35f))continue;
                    Assert.That(ground.Move(start,solid.center-start,.35f).y,Is.LessThan(solid.yMin),"Visible solid props cannot be walked through.");
                }
            }
            Assert.That(Call(world,"PrepareCampEnvironment",scene,resources,travel),Is.True);
            Assert.That(ground.Rectangles.Count,Is.EqualTo(count));
            game.RunDriver.Run.StartNextWaveForTesting();
            for(var n=0;n<game.RunDriver.Run.ThroneMaximum;n++)
            {var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(game.RunDriver.Run.Phase,Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(game.RunDriver.Run.RetryFailedWave(),Is.True);yield return null;
            for(var kind=0;kind<2;kind++)Assert.That(Call(world,"CampPresentation",kind),Is.SameAs(presentations[kind]));
            for(var i=0;i<2;i++){game.RunDriver.ResetRun();yield return null;}
            Assert.That(ground.Rectangles.Count,Is.EqualTo(count));VerifyOpenGround();
        }
        [UnityTest]public IEnumerator AuthoredCityConnectsEntryGuildShopAndPortalForNormalHeroFooting()
        {
            var city=Read<object>(travel,"CityPresentation");var area=Read<Rect>(travel,"CityBounds");
            var origin=Read<Vector2>(travel,"CityOrigin");
            var entry=origin+(Vector2)city.GetType().GetProperty("EntranceOffset").GetValue(null);
            var goals=new[]{Read<Vector2>(city,"GuildInteractionPosition"),Read<Vector2>(city,"MerchantPosition"),Read<Vector2>(travel,"RemotePosition")};
            var ground=scene.PlayerMover.Obstacles;
            foreach(var radius in new[]{.35f,.6f})
            {
                const float step=.5f;
                var min=area.min+Vector2.one*.65f;var w=Mathf.FloorToInt((area.width-1.3f)/step)+1;var h=Mathf.FloorToInt((area.height-1.3f)/step)+1;
                var free=new bool[w,h];
                for(var y=0;y<h;y++)for(var x=0;x<w;x++)free[x,y]=!ground.IsBlocked(min+new Vector2(x,y)*step,radius);
                var start=new Vector2Int(Mathf.RoundToInt((entry.x-min.x)/step),Mathf.RoundToInt((entry.y-min.y)/step));
                Assert.That(free[start.x,start.y],Is.True,"City entry cannot be enclosed by garden props.");
                var seen=new bool[w,h];var queue=new Queue<Vector2Int>();queue.Enqueue(start);seen[start.x,start.y]=true;
                while(queue.Count>0)
                {
                    var point=queue.Dequeue();
                    foreach(var d in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})
                    {
                        var n=point+d;
                        if(n.x<0||n.y<0||n.x>=w||n.y>=h||seen[n.x,n.y]||!free[n.x,n.y])continue;
                        seen[n.x,n.y]=true;queue.Enqueue(n);
                    }
                }
                foreach(var goal in goals)
                {
                    Assert.That(ground.IsBlocked(goal,radius),Is.False,"Service action point itself must stay free.");
                    var reached=false;
                    for(var y=0;y<h&&!reached;y++)for(var x=0;x<w;x++)
                    {
                        if(!seen[x,y])continue;var p=min+new Vector2(x,y)*step;
                        if(Vector2.Distance(p,goal)>.8f)continue;
                        if(Vector2.Distance(ground.Move(p,goal-p,radius),goal)<.01f){reached=true;break;}
                    }
                    Assert.That(reached,Is.True,"Each service must be reachable from the actual city entrance, not just have a free isolated action point.");
                }
                yield return null;
            }
        }
        [UnityTest]public IEnumerator WorldRegenerationPreservesEarnedKillsCurrentContractAndSurvivorHealth()
        {
            var city=Read<object>(travel,"CityPresentation");var approach=Read<Vector2>(city,"GuildInteractionPosition");
            scene.PlayerMover.transform.position=approach+Vector2.down;
            Assert.That(Call(binding,"TryOpenGuild"),Is.True);Assert.That(Call(binding,"TryGuildAction",1),Is.True);
            var hud=game.GetComponent("Stage3FantasyHud");Call(hud,"CloseWindow");
            var contract=guild.FindActive(1);var group=Units.Where(u=>u.Generation==contract.Generation).ToArray();
            Assert.That(Call(world,"Damage",group[0],group[0].Health),Is.True);
            Assert.That(Call(world,"Damage",group[1],1),Is.True);
            var survivors=Units.Where(u=>u.Alive).ToDictionary(u=>u.Id,u=>u.Health);var remaining=contract.Remaining;
            Call(resources,"Regenerate",37621);for(var i=0;i<4;i++)yield return null;
            Assert.That(guild.FindActive(1),Is.SameAs(contract));Assert.That(contract.Remaining,Is.EqualTo(remaining));
            Assert.That(Units.Where(u=>u.Alive).ToDictionary(u=>u.Id,u=>u.Health),Is.EquivalentTo(survivors));
            Assert.That(Units.Where(u=>u.Alive&&u.Generation==contract.Generation).Count(),Is.EqualTo(remaining));
            VerifyOpenGround();
        }
    }
}
#endif
