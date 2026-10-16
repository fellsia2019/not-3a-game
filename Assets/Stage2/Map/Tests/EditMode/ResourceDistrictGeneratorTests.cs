using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Map.Tests
{
    public sealed class ResourceDistrictGeneratorTests
    {
        private Stage2MapDefinition Map => AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(
            "Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset");
        private static readonly Vector2 Hero = new Vector2(18,28);

        [TestCase(1)] [TestCase(37)] [TestCase(-851)] [TestCase(164032)] [TestCase(int.MinValue)]
        public void LayoutsAreConnectedSeparatedInBoundsAndRespectReservations(int seed)
        {
            var map = Map; var settings = new ResourceDistrictSettings();
            var hash = map.ComputeDeterministicContentHash();
            var districts = ResourceDistrictGenerator.Generate(map,settings,seed,Hero);
            Assert.That(districts.Count, Is.EqualTo(6));
            var all = new HashSet<Vector2Int>();
            foreach (var district in districts)
            {
                Assert.That(district.Cells.Count, Is.InRange(200,300));
                Assert.That(district.Cells.Distinct().Count(), Is.EqualTo(district.Cells.Count));
                var cells = new HashSet<Vector2Int>(district.Cells);
                var reached = new HashSet<Vector2Int> { district.Center };
                var queue = new Queue<Vector2Int>(); queue.Enqueue(district.Center);
                while (queue.Count > 0)
                {
                    var cell = queue.Dequeue();
                    foreach (var direction in new[] { Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left })
                        if (cells.Contains(cell+direction) && reached.Add(cell+direction)) queue.Enqueue(cell+direction);
                }
                Assert.That(reached.Count, Is.EqualTo(cells.Count));
                foreach (var cell in cells)
                {
                    Assert.That(map.Contains(cell), Is.True);
                    Assert.That(all.Add(cell), Is.True, "Districts cannot overlap.");
                    Assert.That(map.GetSurface(cell), Is.EqualTo(MapSurfaceKind.Forest));
                    for (var dx=-settings.roadClearance; dx<=settings.roadClearance; dx++)
                    for (var dy=-settings.roadClearance; dy<=settings.roadClearance; dy++)
                    {
                        var near=cell+new Vector2Int(dx,dy);
                        if(map.Contains(near)) Assert.That(map.GetSurface(near), Is.EqualTo(MapSurfaceKind.Forest));
                    }
                    Assert.That(Chebyshev(cell,map.WorldToCell(Hero)), Is.GreaterThan(settings.anchorClearance));
                    for(var i=0;i<map.ResourceRegionCount;i++)
                        Assert.That(Chebyshev(cell,map.GetResourceRegion(i).Anchor), Is.GreaterThan(settings.anchorClearance));
                }
                foreach (var previous in districts)
                {
                    if (ReferenceEquals(previous,district)) break;
                    foreach(var a in previous.Cells)
                    foreach(var b in cells) Assert.That(Chebyshev(a,b), Is.GreaterThan(settings.districtSpacing));
                }
            }
            Assert.That(map.ComputeDeterministicContentHash(),Is.EqualTo(hash));
        }

        [Test]
        public void SameSeedReproducesWhileOtherSeedChangesPositionsAndShapesWithoutGlobalRandom()
        {
            var state=UnityEngine.Random.state;
            try
            {
                UnityEngine.Random.InitState(987);
                var before=UnityEngine.Random.state;
                var a=ResourceDistrictGenerator.Generate(Map,new ResourceDistrictSettings(),7,Hero);
                var b=ResourceDistrictGenerator.Generate(Map,new ResourceDistrictSettings(),7,Hero);
                var c=ResourceDistrictGenerator.Generate(Map,new ResourceDistrictSettings(),8,Hero);
                Assert.That(UnityEngine.Random.state,Is.EqualTo(before));
                for(var i=0;i<a.Count;i++)
                {
                    Assert.That(a[i].Center,Is.EqualTo(b[i].Center));
                    Assert.That(a[i].Cells,Is.EqualTo(b[i].Cells));
                    Assert.That(a[i].Cells.Select(p=>p-a[i].Center),Is.Not.EqualTo(c[i].Cells.Select(p=>p-c[i].Center)));
                }
            }
            finally { UnityEngine.Random.state=state; }
        }

        [Test]
        public void SizeCanBeIncreasedThroughSettingsAndInvalidSettingsFailExplicitly()
        {
            var settings=new ResourceDistrictSettings { minimumCells=700,maximumCells=900,districtsPerResource=1 };
            var districts=ResourceDistrictGenerator.Generate(Map,settings,12,Hero);
            Assert.That(districts.All(d=>d.Cells.Count>=700 && d.Cells.Count<=900),Is.True);
            Assert.Throws<ArgumentException>(()=>ResourceDistrictGenerator.Generate(Map,
                new ResourceDistrictSettings {minimumCells=300,maximumCells=200},12,Hero));
        }

        private static int Chebyshev(Vector2Int a,Vector2Int b)=>Mathf.Max(Mathf.Abs(a.x-b.x),Mathf.Abs(a.y-b.y));
    }
}
