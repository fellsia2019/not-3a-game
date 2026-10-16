using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Map.Tests
{
    public sealed class ResourceDistrictLayoutTests
    {
        private static readonly Vector2 Up=new Vector2(-.574f,.819f);
        private static readonly ResourceDistrictShapeSettings Shape=new ResourceDistrictShapeSettings();
        private static IReadOnlyList<ResourceDistrict> Generate(int seed)=>ResourceDistrictGenerator.Generate(
            AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset"),
            new ResourceDistrictSettings(),seed,new Vector2(18,28));

        [TestCase(1)] [TestCase(37)] [TestCase(-851)] [TestCase(164032)] [TestCase(164033)]
        public void ForestHasScaleAwareClearancesYoungAndMatureTreesAndReproducibleOpenings(int seed)
        {
            foreach(var district in Generate(seed).Where(d=>d.Kind==MapResourceKind.Wood))
            {
                var plan=new ResourceDistrictLayout(district,Shape,Up);
                var repeat=new ResourceDistrictLayout(district,Shape,Up);
                Assert.That(plan.Trees.Count,Is.GreaterThan(5));
                Assert.That(plan.Trees.Select(t=>t.Point),Is.EqualTo(repeat.Trees.Select(t=>t.Point)));
                Assert.That(plan.Trees.Max(t=>t.Scale)-plan.Trees.Min(t=>t.Scale),Is.GreaterThan(.25f));
                foreach(var a in plan.Trees)foreach(var b in plan.Trees)
                {
                    if(a.Point==b.Point)continue;
                    Assert.That(Vector2.Distance(a.Point,b.Point),
                        Is.GreaterThanOrEqualTo(Shape.treeCrownRadius*(a.Scale+b.Scale)+Shape.minimumTreeGap-.001f));
                }
                Assert.That(plan.Heights,Is.Empty);
            }
        }

        [Test]
        public void StoneHasCoherentTerracesAndMineStairsConnectEveryDescentAcrossManySeeds()
        {
            for(var seed=0;seed<80;seed++)foreach(var district in Generate(seed))
            {
                if(district.Kind==MapResourceKind.Wood)continue;
                var plan=new ResourceDistrictLayout(district,Shape,Up);
                Assert.That(plan.Heights.Count,Is.EqualTo(district.Cells.Count));
                if(district.Kind==MapResourceKind.Stone)
                {
                    Assert.That(plan.Heights.Values.Distinct().Count(),Is.GreaterThanOrEqualTo(3));
                    Assert.That(plan.Heights.Values.Min(),Is.GreaterThan(0));continue;
                }
                Assert.That(plan.Heights.Values.Min(),Is.LessThan(-2.7f));
                Assert.That(plan.Stairs.Count,Is.GreaterThanOrEqualTo(2),$"seed {seed}");
                foreach(var stair in plan.Stairs)
                {
                    Assert.That(stair.Top,Is.GreaterThan(stair.Bottom));
                    Assert.That(plan.Heights[stair.Cell],Is.EqualTo(stair.Top));
                    Assert.That(plan.Heights[stair.Cell+stair.Direction],Is.EqualTo(stair.Bottom));
                    Assert.That(Mathf.Abs(stair.Direction.x)+Mathf.Abs(stair.Direction.y),Is.EqualTo(1));
                    var back=stair.Cell-stair.Direction;
                    var previous=plan.Stairs.FirstOrDefault(s=>s.Cell==back);
                    if(previous.Top>previous.Bottom)Assert.That(previous.Bottom,Is.EqualTo(stair.Top));
                    else if(plan.Heights.ContainsKey(back))Assert.That(plan.Heights[back],Is.EqualTo(stair.Top));
                    else Assert.That(stair.Cell,Is.EqualTo(plan.MineEntrance));
                }
            }
        }
    }
}
