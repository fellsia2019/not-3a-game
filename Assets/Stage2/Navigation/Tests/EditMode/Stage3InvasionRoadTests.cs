using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        public void ExpandedGreenWorld_PreservesAcceptedCombatCellsAndAnchors()
        {
            var map=Map;
            Assert.That(map.Size,Is.EqualTo(new Vector2Int(250,250)));
            Assert.That(map.MinimumCell,Is.EqualTo(new Vector2Int(-125,-125)));
            Assert.That(map.MaximumCell,Is.EqualTo(new Vector2Int(124,124)));
            Assert.That(map.CameraBounds.Minimum,Is.EqualTo(map.MinimumCell));
            Assert.That(map.CameraBounds.Size,Is.EqualTo(map.Size));
            Assert.That(map.EnemyEntrance,Is.EqualTo(new Vector2Int(-42,-30)));
            Assert.That(map.Throne,Is.EqualTo(new Vector2Int(39,12)));
            var anchors=new[]{new Vector2Int(17,32),new Vector2Int(8,22),new Vector2Int(27,11)};
            Assert.That(map.ResourceRegionCount,Is.EqualTo(anchors.Length));
            for(var i=0;i<anchors.Length;i++)
                Assert.That(map.GetResourceRegion(i).Anchor,Is.EqualTo(anchors[i]));

            var road=new StringBuilder();
            var roadCells=0;
            for(var x=map.MinimumCell.x;x<=map.MaximumCell.x;x++)
            for(var y=map.MinimumCell.y;y<=map.MaximumCell.y;y++)
            {
                var cell=new Vector2Int(x,y);
                if(map.GetSurface(cell)==MapSurfaceKind.InvasionCorridor)
                {
                    road.Append(x).Append(',').Append(y).Append(';');
                    roadCells++;
                }
                if(x>=-50 && x<50 && y>=-50 && y<50) continue;
                Assert.That(map.GetSurface(cell),Is.EqualTo(MapSurfaceKind.Forest));
                Assert.That(map.IsWalkable(cell),Is.True,"New green land must be reachable by the hero.");
                Assert.That(map.IsBuildable(cell),Is.False,"Expansion must not enlarge the combat area.");
            }
            Assert.That(roadCells,Is.EqualTo(1582));
            // Captured from the accepted 100x100 asset before the bounds-only expansion.
            using(var sha=SHA256.Create())
                Assert.That(string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(road.ToString()))
                    .Select(b=>b.ToString("x2"))),
                    Is.EqualTo("627d4127e71c9be0ef77f64e71c9c669aa0964cca3f503713fee58643eb62b64"));
            var nav=new Stage2Navigation(map);
            Assert.That(nav.PlanFrom(map.EnemyEntrance).Path.Count-1,Is.EqualTo(195));
            Assert.That(nav.PlanFrom(new Vector2Int(-100,-100)).Status,
                Is.EqualTo(NavigationStatus.StartNotWalkable));
        }

        [TestCase(-42,-15,true)]
        [TestCase(-24,-12,true)]
        [TestCase(-6,-8,true)]
        [TestCase(12,-6,true)]
        [TestCase(39,-6,true)]
        [TestCase(-33,0,false)]
        [TestCase(-15,-24,false)]
        [TestCase(3,6,false)]
        [TestCase(25,-18,false)]
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
        public void EnemiesFollowBothZigzags_WhileHeroCanWalkResourceLand()
        {
            var map=Map;
            Assert.That(map.ValidateDefinition().IsValid,Is.True);
            Assert.That(map.EnemiesStayOnInvasionCorridor,Is.True);
            Assert.That(map.EnemyEntrance.x,Is.LessThan(-35));
            Assert.That(map.EnemyEntrance.y,Is.LessThan(-25));
            var nav=new Stage2Navigation(map);
            var plan=nav.PlanFrom(map.EnemyEntrance);
            Assert.That(plan.Status,Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(plan.Path.Last(),Is.EqualTo(map.Throne));
            var direct=Mathf.Abs(map.EnemyEntrance.x-map.Throne.x)+Mathf.Abs(map.EnemyEntrance.y-map.Throne.y);
            Assert.That(plan.Path.Count-1,Is.GreaterThan(direct+40),"Both detours must be traversed.");
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
                Assert.That(nav.TryOccupy(new Vector2Int(-44+i,-15),owners[i]),Is.EqualTo(OccupancyResult.Changed));
            }
            var open=nav.ReplanIfStale(map.EnemyEntrance,baseline);
            Assert.That(open.Status,Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(open.Path.Count,Is.GreaterThan(baseline.Path.Count));
            owners[6]=new object();
            Assert.That(nav.TryOccupy(new Vector2Int(-45,-15),owners[6]),Is.EqualTo(OccupancyResult.Changed));
            var siege=nav.ReplanIfStale(map.EnemyEntrance,open);
            Assert.That(siege.Status,Is.EqualTo(NavigationStatus.ReachableBlocker));
            Assert.That(siege.BlockerCell.HasValue,Is.True);
            Assert.That(nav.TryRelease(new Vector2Int(-45,-15),owners[6]),Is.EqualTo(OccupancyResult.Changed));
            Assert.That(nav.ReplanIfStale(map.EnemyEntrance,siege).Status,Is.EqualTo(NavigationStatus.ReachesThrone));
            nav.Reset();
            Assert.That(nav.IsCurrent(open),Is.False);
            Assert.That(nav.PlanFrom(map.EnemyEntrance).Path.Count,Is.EqualTo(baseline.Path.Count));
        }
    }
}
