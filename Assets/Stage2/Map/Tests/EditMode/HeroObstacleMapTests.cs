using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Not3A.Stage2.Map.Tests
{
    public sealed class HeroObstacleMapTests
    {
        [TestCase(1f)] [TestCase(100f)] [TestCase(1000f)]
        public void CompleteSweepStopsAtTrunkEvenWhenEndPointIsBeyondIt(float distance)
        {
            var map = new HeroObstacleMap(); map.AddCircle(Vector2.zero, .24f);
            var end = map.Move(Vector2.left, Vector2.right * distance, .2f);
            Assert.That(end.x, Is.EqualTo(-.441f).Within(.002f));
            Assert.That(map.IsBlocked(end,.2f), Is.False);
        }

        [Test]
        public void ForestGapAndTangentArePassableWithoutACanopyWall()
        {
            var map = new HeroObstacleMap();
            map.AddCircle(new Vector2(0,.65f),.24f); map.AddCircle(new Vector2(0,-.65f),.24f);
            Assert.That(map.Move(new Vector2(-2,0),new Vector2(4,0),.2f), Is.EqualTo(new Vector2(2,0)));
            var tangent = new HeroObstacleMap(); tangent.AddCircle(Vector2.zero,.25f);
            Assert.That(tangent.Move(new Vector2(-2,.5f),new Vector2(4,0),.25f), Is.EqualTo(new Vector2(2,.5f)));
        }

        [Test]
        public void PackedCellsBlockInteriorAndSlideWithoutSnaggingAtRowSeams()
        {
            var map = new HeroObstacleMap(); var cells = new List<Vector2Int>();
            for (var y=-10;y<=10;y++) for(var x=0;x<3;x++) cells.Add(new Vector2Int(x,y));
            map.AddSolidCells(cells);
            var position = map.Move(new Vector2(-2,-5),new Vector2(100,10),.2f);
            Assert.That(position.x,Is.EqualTo(-.701f).Within(.003f));
            Assert.That(position.y,Is.EqualTo(5).Within(.003f));
            for(var i=0;i<60;i++)
            {
                position=map.Move(position,new Vector2(.1f,.03f),.2f);
                Assert.That(map.IsBlocked(position,.2f),Is.False);
                Assert.That(position.x,Is.EqualTo(-.701f).Within(.004f));
            }
            Assert.That(position.y,Is.GreaterThan(6.7f));
        }

        [Test]
        public void RoundHeroCanPassNearRoundedCornerWithoutAnInvisibleSquareCorner()
        {
            var map=new HeroObstacleMap();map.AddSolidCells(new[]{Vector2Int.zero});
            var start=new Vector2(-1,.68f);var delta=new Vector2(.32f,0);
            Assert.That(map.Move(start,delta,.2f),Is.EqualTo(start+delta));
        }

        [Test]
        public void PerpendicularWallsStopBothAxesAndAllowReversingOut()
        {
            var map=new HeroObstacleMap();var cells=new List<Vector2Int>();
            for(var i=-4;i<=4;i++){cells.Add(new Vector2Int(0,i));cells.Add(new Vector2Int(i,0));}
            map.AddSolidCells(cells);
            var end=map.Move(new Vector2(-2,-2),new Vector2(20,20),.2f);
            Assert.That(map.IsBlocked(end,.2f),Is.False);
            Assert.That(end.x,Is.EqualTo(-.7f).Within(.004f));
            Assert.That(end.y,Is.EqualTo(-.7f).Within(.004f));
            var reverse=map.Move(end,new Vector2(-2,-2),.2f);
            Assert.That(reverse.x,Is.LessThan(-2.6f)); Assert.That(reverse.y,Is.LessThan(-2.6f));
        }

        [Test]
        public void ReplacementRecoversAnOverlappingSpawnInsideAllowedWorld()
        {
            var map=new HeroObstacleMap();var cells=new List<Vector2Int>();
            for(var x=-8;x<=8;x++) for(var y=-8;y<=8;y++) cells.Add(new Vector2Int(x,y));
            map.AddSolidCells(cells);
            var allowed=Rect.MinMaxRect(-12,-12,12,12);
            Assert.That(map.TryFindFreePosition(Vector2.zero,.2f,allowed,out var point),Is.True);
            Assert.That(map.IsBlocked(point,.2f),Is.False);
            Assert.That(allowed.Contains(point),Is.True);
            Assert.That(point.magnitude,Is.LessThan(10));
            Assert.That(map.TryFindFreePosition(point,.2f,allowed,out var unchanged),Is.True);
            Assert.That(unchanged,Is.EqualTo(point));
        }

        [Test]
        public void RepeatedMixedContactsNeverPenetrateOrTunnel()
        {
            var map=new HeroObstacleMap();var cells=new List<Vector2Int>();
            for(var x=-5;x<=5;x++) for(var y=-5;y<=5;y++) cells.Add(new Vector2Int(x,y));
            map.AddSolidCells(cells);
            for(var i=-4;i<=4;i++) map.AddCircle(new Vector2(i*1.2f,-6),.8f);
            map.AddCircle(new Vector2(-7,2),.3f);map.AddCircle(new Vector2(-7,3),.3f);
            var rng=new System.Random(55);var p=new Vector2(-10,-10);
            for(var i=0;i<2000;i++)
            {
                var delta=new Vector2((float)rng.NextDouble()-.5f,(float)rng.NextDouble()-.5f)*30;
                var next=map.Move(p,delta,.2f);
                Assert.That(map.IsBlocked(next,.2f),Is.False,$"movement {i}: {p} -> {next}");
                p=Vector2.ClampMagnitude(next,15);
                if(map.IsBlocked(p,.2f)) p=new Vector2(-10,-10);
            }
        }
    }
}
