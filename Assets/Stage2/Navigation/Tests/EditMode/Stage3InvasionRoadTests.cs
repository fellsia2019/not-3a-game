using System.Linq;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Navigation.Tests
{
    public sealed class Stage3InvasionRoadTests
    {
        private static Stage2MapDefinition Map => AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(
            "Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset");

        [Test]
        public void ExpandedGreenWorld_KeepsAcceptedCombatInLowerLeft()
        {
            var map=Map;
            Assert.That(map.Size,Is.EqualTo(new Vector2Int(250,250)));
            Assert.That(map.MinimumCell,Is.EqualTo(new Vector2Int(-50,-50)));
            Assert.That(map.MaximumCell,Is.EqualTo(new Vector2Int(199,199)));
            Assert.That(map.CameraBounds.Minimum,Is.EqualTo(map.MinimumCell));
            Assert.That(map.CameraBounds.Size,Is.EqualTo(map.Size));
            Assert.That(map.EnemyEntrance,Is.EqualTo(new Vector2Int(-42,12)));
            Assert.That(map.Throne,Is.EqualTo(new Vector2Int(55,12)));
            var anchors=new[]{new Vector2Int(17,32),new Vector2Int(8,22),new Vector2Int(27,11)};
            Assert.That(map.ResourceRegionCount,Is.EqualTo(anchors.Length));
            for(var i=0;i<anchors.Length;i++)
                Assert.That(map.GetResourceRegion(i).Anchor,Is.EqualTo(anchors[i]));

            for(var x=map.MinimumCell.x;x<=map.MaximumCell.x;x++)
            for(var y=map.MinimumCell.y;y<=map.MaximumCell.y;y++)
            {
                var cell=new Vector2Int(x,y);
                if(map.GetSurface(cell)==MapSurfaceKind.InvasionCorridor)
                {
                    Assert.That(x-map.MinimumCell.x,Is.LessThan(map.Size.x/2),
                        "The entire combat road must be in the left half of the world.");
                    Assert.That(y-map.MinimumCell.y,Is.LessThan(map.Size.y/2),
                        "The entire combat road must be in the lower half of the world.");
                }
                if(x<75 && y<75) continue;
                Assert.That(map.GetSurface(cell),Is.EqualTo(MapSurfaceKind.Forest));
                Assert.That(map.IsWalkable(cell),Is.True,"New green land must be reachable by the hero.");
                Assert.That(map.IsBuildable(cell),Is.False,"Expansion must not enlarge the combat area.");
            }
            var nav=new Stage2Navigation(map);
            Assert.That(nav.PlanFrom(new Vector2Int(100,100)).Status,
                Is.EqualTo(NavigationStatus.StartNotWalkable));
        }

        [TestCase(-42,4,true)]
        [TestCase(-41,0,true)]
        [TestCase(-41,-6,true)]
        [TestCase(-39,-20,true)]
        [TestCase(-25,-31,false)]
        [TestCase(37,-20,true)]
        [TestCase(37,-17,true)]
        [TestCase(46,12,false)]
        [TestCase(50,12,false)]
        public void EveryStraightReach_IsSevenCellsAcross(int x,int y,bool vertical)
        {
            var normal=vertical?Vector2Int.right:Vector2Int.up;
            for(var offset=-3;offset<=3;offset++)
                Assert.That(Map.GetSurface(new Vector2Int(x,y)+normal*offset),
                    Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            foreach(var offset in new[]{-4,4})
                Assert.That(Map.GetSurface(new Vector2Int(x,y)+normal*offset),
                    Is.EqualTo(MapSurfaceKind.Forest));
        }

        [Test]
        public void EnemiesFollowBothValleysAndCentralRise_WhileHeroCanWalkResourceLand()
        {
            var map=Map;
            Assert.That(map.ValidateDefinition().IsValid,Is.True);
            Assert.That(map.EnemiesStayOnInvasionCorridor,Is.True);
            Assert.That(map.EnemyEntrance.x,Is.LessThan(-35));
            Assert.That(map.EnemyEntrance.y,Is.EqualTo(map.Throne.y));
            var nav=new Stage2Navigation(map);
            var plan=nav.PlanFrom(map.EnemyEntrance);
            Assert.That(plan.Status,Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(plan.Path.Last(),Is.EqualTo(map.Throne));
            var direct=Mathf.Abs(map.EnemyEntrance.x-map.Throne.x)+Mathf.Abs(map.EnemyEntrance.y-map.Throne.y);
            Assert.That(plan.Path.Count-1,Is.GreaterThan(direct+40),"Both detours must be traversed.");
            var cells=plan.Path.ToList();
            var firstValley=cells.FindIndex(c=>c.x<-10 && c.y<-24);
            Assert.That(firstValley,Is.GreaterThan(0));
            var centralRise=cells.FindIndex(firstValley+1,c=>c.x>-8 && c.x<16 && c.y>-10);
            Assert.That(centralRise,Is.GreaterThan(firstValley));
            var secondValley=cells.FindIndex(centralRise+1,c=>c.x>14 && c.y<-27);
            Assert.That(secondValley,Is.GreaterThan(centralRise));
            foreach(var cell in plan.Path)
                Assert.That(map.GetSurface(cell),Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            for(var i=0;i<map.ResourceRegionCount;i++)
            {
                var grass=map.GetResourceRegion(i).Anchor;
                Assert.That(map.GetSurface(grass),Is.EqualTo(MapSurfaceKind.Forest));
                Assert.That(map.IsWalkable(grass),Is.True,"Hero resource travel must stay open.");
                Assert.That(nav.PlanFrom(grass).Status,Is.EqualTo(NavigationStatus.StartNotWalkable));
            }
        }

        [Test]
        public void SixTowerBarrierLeavesDetour_SevenInvokesSiege_ReleaseAndResetRecover()
        {
            var map=Map;
            var nav=new Stage2Navigation(map);
            var baseline=nav.PlanFrom(map.EnemyEntrance);
            var owners=new object[7];
            for(var i=0;i<6;i++)
            {
                owners[i]=new object();
                Assert.That(nav.TryOccupy(new Vector2Int(46,9+i),owners[i]),Is.EqualTo(OccupancyResult.Changed));
            }
            var open=nav.ReplanIfStale(map.EnemyEntrance,baseline);
            Assert.That(open.Status,Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(open.Path.Count,Is.GreaterThan(baseline.Path.Count));
            owners[6]=new object();
            Assert.That(nav.TryOccupy(new Vector2Int(46,15),owners[6]),Is.EqualTo(OccupancyResult.Changed));
            var siege=nav.ReplanIfStale(map.EnemyEntrance,open);
            Assert.That(siege.Status,Is.EqualTo(NavigationStatus.ReachableBlocker));
            Assert.That(siege.BlockerCell.HasValue,Is.True);
            Assert.That(nav.TryRelease(new Vector2Int(46,15),owners[6]),Is.EqualTo(OccupancyResult.Changed));
            Assert.That(nav.ReplanIfStale(map.EnemyEntrance,siege).Status,Is.EqualTo(NavigationStatus.ReachesThrone));
            nav.Reset();
            Assert.That(nav.IsCurrent(open),Is.False);
            Assert.That(nav.PlanFrom(map.EnemyEntrance).Path.Count,Is.EqualTo(baseline.Path.Count));
        }
    }
}
