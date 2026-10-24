using System;
using System.Linq;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Economy.Tests
{
    public sealed class Stage2ManualResourceTests
    {
        private Stage2MapDefinition map;
        private Stage2EconomyCatalog catalog;

        [SetUp]
        public void Load()
        {
            map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage3/VisualPilot/Stage3PlanarCorner.asset");
            Assert.That(map, Is.Not.Null);
            catalog = ScriptableObject.CreateInstance<Stage2EconomyCatalog>();
            catalog.Configure(1.5f, Enumerable.Range(0, 3).Select(i =>
                new Stage2ResourceSettings((MapResourceKind)i, 5, .8f, 30f)).ToArray());
        }

        [TearDown] public void Cleanup() => Object.DestroyImmediate(catalog);

        private Stage2ResourceNode Node(MapResourceKind kind, Stage2ResourceSize size, int capacity, Vector2 offset)
        {
            var anchor = map.GetResourceRegion(0).Anchor;
            var position = map.CellToWorld(anchor) + offset;
            return new Stage2ResourceNode(new Stage2ResourceSettings(kind, capacity, .8f, 30f),
                map.WorldToCell(position), position, size, 1f);
        }

        [TestCase(MapResourceKind.Stone,Stage2ResourceSize.Medium,35,35,1)]
        [TestCase(MapResourceKind.Stone,Stage2ResourceSize.Large,120,6,0)]
        [TestCase(MapResourceKind.Stone,Stage2ResourceSize.Small,5,5,0)]
        [TestCase(MapResourceKind.Metal,Stage2ResourceSize.Medium,35,6,0)]
        public void MiningCapstoneFinishesOnlyMediumStoneAndCreditsActualStock(MapResourceKind kind,Stage2ResourceSize size,
            int capacity,int expected,int expectedRolls)
        {
            var rolls=0;var model=new Stage2Economy(map,catalog,()=>{rolls++;return .05;});
            model.SetManualProfessionStats(5,5,1,1,1,1);model.SetManualCapstoneChances(.1f,.1f);
            var node=Node(kind,size,capacity,Vector2.right*3);model.ReplaceAdditionalNodes(new[]{node});
            var facts=0;var strikes=0;model.Gathered+=(_,amount)=>facts+=amount;model.NodeGathered+=_=>strikes++;
            Assert.That(model.AdvanceGather(node,node.WorldPosition,true,.4f,true),Is.False);Assert.That(rolls,Is.Zero);
            Assert.That(model.AdvanceGather(node,node.WorldPosition,true,.4f,true),Is.True);
            Assert.That(model.Inventory.Get(kind),Is.EqualTo(expected));Assert.That(facts,Is.EqualTo(expected));
            Assert.That(node.Remaining,Is.EqualTo(capacity-expected));Assert.That(strikes,Is.EqualTo(1));Assert.That(rolls,Is.EqualTo(expectedRolls));
            if(node.IsDepleted)
            {
                Assert.That(node.RecoveryRemaining,Is.EqualTo(node.RecoverySeconds));
                Assert.That(model.AdvanceGather(node,node.WorldPosition,true,10,true),Is.False);Assert.That(facts,Is.EqualTo(expected));
            }
        }
        [Test] public void LoggingCapstoneTakesExactlyOneExtraInRangeTreeWithoutChainOrReentrantHarvest()
        {
            var rolls=0;var model=new Stage2Economy(map,catalog,()=>{rolls++;return 0;});
            model.SetManualProfessionStats(5,5,1,1,1,1);model.SetManualCapstoneChances(.1f,.1f);
            var first=Node(MapResourceKind.Wood,Stage2ResourceSize.Small,5,Vector2.right*3);
            var second=Node(MapResourceKind.Wood,Stage2ResourceSize.Small,5,Vector2.right*3.3f);
            var third=Node(MapResourceKind.Wood,Stage2ResourceSize.Small,5,Vector2.right*3.6f);
            var distant=Node(MapResourceKind.Wood,Stage2ResourceSize.Small,5,Vector2.up*20);
            model.ReplaceAdditionalNodes(new[]{first,second,third,distant});
            var grants=0;var actual=0;var nested=false;
            model.ResourceGranted+=(_,amount)=>{grants++;actual+=amount;};
            model.Inventory.Changed+=()=>nested|=model.AdvanceGather(third,third.WorldPosition,true,1,true);
            Assert.That(model.AdvanceGather(first,first.WorldPosition,true,.8f,true),Is.True);
            Assert.That(first.IsDepleted&&second.IsDepleted,Is.True);Assert.That(third.Remaining,Is.EqualTo(5));
            Assert.That(distant.Remaining,Is.EqualTo(5));Assert.That(rolls,Is.EqualTo(1));Assert.That(nested,Is.False);
            Assert.That(grants,Is.EqualTo(2));Assert.That(actual,Is.EqualTo(10));Assert.That(model.Inventory.Get(MapResourceKind.Wood),Is.EqualTo(10));
        }
        [Test] public void CapstoneRollFailureAndFullWalletCannotConsumeHiddenStock()
        {
            var rolls=0;var model=new Stage2Economy(map,catalog,()=>{rolls++;return .9;});
            var node=Node(MapResourceKind.Stone,Stage2ResourceSize.Medium,35,Vector2.right*3);model.ReplaceAdditionalNodes(new[]{node});
            model.SetManualProfessionStats(5,5,1,1,1,1);model.SetManualCapstoneChances(.1f,.1f);
            Assert.That(model.AdvanceGather(node,node.WorldPosition,true,.8f,false),Is.False);
            Assert.That(model.AdvanceGather(node,node.WorldPosition+Vector2.up*50,true,.8f,true),Is.False);
            Assert.That(rolls,Is.Zero);
            Assert.That(model.AdvanceGather(node,node.WorldPosition,true,.8f,true),Is.True);Assert.That(node.Remaining,Is.EqualTo(29));
            Assert.That(model.Inventory.TryAdd(MapResourceKind.Stone,int.MaxValue-6),Is.True);
            Assert.That(model.AdvanceGather(node,node.WorldPosition,true,.8f,true),Is.False);
            Assert.That(node.Remaining,Is.EqualTo(29));Assert.That(rolls,Is.EqualTo(1));
        }

        [TestCase(Stage2ResourceSize.Small, 5)]
        [TestCase(Stage2ResourceSize.Medium, 35)]
        [TestCase(Stage2ResourceSize.Large, 120)]
        public void EveryStrikePaysImmediatelyUntilIndependentReserveIsEmpty(Stage2ResourceSize size, int capacity)
        {
            var model = new Stage2Economy(map, catalog);
            var first = Node(MapResourceKind.Stone, size, capacity, Vector2.right * 3f);
            var second = Node(MapResourceKind.Stone, size, capacity, Vector2.left * 3f);
            model.ReplaceAdditionalNodes(new[] { first, second });
            var facts = 0; var contactFacts = 0;
            model.Gathered += (_, amount) => facts += amount;
            model.NodeGathered += n => { Assert.That(n, Is.SameAs(first)); contactFacts++; };
            for (var i = 1; i <= capacity; i++)
            {
                Assert.That(model.AdvanceGather(first, first.WorldPosition, true, .8f, true), Is.True);
                Assert.That(model.Inventory.Get(MapResourceKind.Stone), Is.EqualTo(i), "No delayed lump payout.");
                Assert.That(first.Remaining, Is.EqualTo(capacity - i));
                Assert.That(second.Remaining, Is.EqualTo(capacity), "Same-kind sources are independent.");
            }
            Assert.That(facts, Is.EqualTo(capacity)); Assert.That(contactFacts, Is.EqualTo(capacity));
            Assert.That(model.AdvanceGather(first, first.WorldPosition, true, 20f, true), Is.False);
            model.Tick(60f, false); Assert.That(first.IsDepleted, Is.True);
            model.Tick(30f, true);
            Assert.That(first.Remaining, Is.EqualTo(capacity));
            Assert.That(model.Inventory.Get(MapResourceKind.Stone), Is.EqualTo(capacity), "Recovery does not pay.");
            model.Reset();
            Assert.That(model.Nodes.Count, Is.EqualTo(5)); Assert.That(model.CurrentNode, Is.Null);
            Assert.That(model.Inventory.Get(MapResourceKind.Stone), Is.Zero);
        }

        [Test]
        public void SwitchingSameKindTargetsResetsPartialStrikeButPreservesCompletedStock()
        {
            var model = new Stage2Economy(map, catalog);
            var a = Node(MapResourceKind.Metal, Stage2ResourceSize.Medium, 35, Vector2.right * 4f);
            var b = Node(MapResourceKind.Metal, Stage2ResourceSize.Large, 120, Vector2.left * 4f);
            model.ReplaceAdditionalNodes(new[] { a, b });
            Assert.That(model.AdvanceGather(a, a.WorldPosition, true, .8f, true), Is.True);
            Assert.That(model.AdvanceGather(a, a.WorldPosition, true, .4f, true), Is.False);
            Assert.That(model.AdvanceGather(b, b.WorldPosition, true, .4f, true), Is.False);
            Assert.That(model.AdvanceGather(a, a.WorldPosition, true, .4f, true), Is.False);
            model.CancelGather(); model.Tick(300f, true);
            Assert.That(a.Remaining, Is.EqualTo(34), "Partial depletion must not silently replenish.");
            Assert.That(b.Remaining, Is.EqualTo(120));
            Assert.That(model.Inventory.Get(MapResourceKind.Metal), Is.EqualTo(1));
            Assert.That(model.AdvanceGather(a, a.WorldPosition, true, .8f, false), Is.False);
            Assert.That(model.AdvanceGather(a, a.WorldPosition + Vector2.up * 10f, true, .8f, true), Is.False);
            Assert.That(model.AdvanceGather(a, a.WorldPosition + Vector2.right * 2f, true, .8f, true), Is.True,
                "Large forms are reachable from the edge, not just their center.");
            var foreign = Node(MapResourceKind.Metal, Stage2ResourceSize.Small, 5, Vector2.up * 3f);
            Assert.That(model.AdvanceGather(foreign, foreign.WorldPosition, true, 1f, true), Is.False);
            Assert.That(foreign.Remaining, Is.EqualTo(5));
        }

        [Test]
        public void SameSeedReplaysNaturalTreeGroupsAndAllMineralSizesWithoutTouchingUnityRandom()
        {
            var settings = new Stage2ManualResourceSettings(); catalog.ConfigureManualResources(settings);
            var oldRandom = UnityEngine.Random.state;
            var expectedNext = UnityEngine.Random.value;
            UnityEngine.Random.state = oldRandom;
            var exclusionEdge = map.MinimumCell.x + 30f;
            var a = Stage2ManualResourceGenerator.Generate(map, catalog, 164032, (p, radius) => p.x < exclusionEdge);
            Assert.That(UnityEngine.Random.value, Is.EqualTo(expectedNext));
            var b = Stage2ManualResourceGenerator.Generate(map, catalog, 164032, (p, radius) => p.x < exclusionEdge);
            var c = Stage2ManualResourceGenerator.Generate(map, catalog, 164033, (p, radius) => p.x < exclusionEdge);
            Assert.That(a.Select(n => (n.WorldPosition, n.VariantSeed)), Is.EqualTo(b.Select(n => (n.WorldPosition, n.VariantSeed))));
            Assert.That(a[0].WorldPosition, Is.Not.EqualTo(c[0].WorldPosition));
            var groups = a.Where(n => n.Resource == MapResourceKind.Wood).GroupBy(n => n.GroupId).ToArray();
            Assert.That(groups.Length, Is.EqualTo(settings.treeGroups));
            Assert.That(groups.Select(group => group.Count()), Does.Contain(1));
            Assert.That(groups.Select(group => group.Count()), Does.Contain(8));
            foreach (var group in groups)
            {
                var trees = group.ToArray(); Assert.That(trees.Length, Is.InRange(1, 8));
                Assert.That(trees.All(n => n.Size == Stage2ResourceSize.Small && n.Capacity == 5), Is.True);
                for (var i = 0; i < trees.Length; i++)
                for (var j = i + 1; j < trees.Length; j++)
                    Assert.That(Vector2.Distance(trees[i].WorldPosition, trees[j].WorldPosition),
                        Is.InRange(settings.treeSpacing - .0001f, settings.treeGroupRadius * 2f + .0001f));
                if (trees.Length >= 3)
                {
                    var ab = trees[1].WorldPosition - trees[0].WorldPosition;
                    Assert.That(trees.Skip(2).Any(n => Mathf.Abs(ab.x * (n.WorldPosition.y - trees[0].WorldPosition.y) -
                        ab.y * (n.WorldPosition.x - trees[0].WorldPosition.x)) > .01f), Is.True, "No straight planted row.");
                }
            }
            foreach (var n in a)
            {
                Assert.That(n.WorldPosition.x, Is.GreaterThanOrEqualTo(exclusionEdge));
                Assert.That(map.IsWalkable(n.Cell), Is.True);
                Assert.That(map.GetSurface(n.Cell), Is.EqualTo(MapSurfaceKind.Forest));
            }
            foreach (var kind in new[] { MapResourceKind.Stone, MapResourceKind.Metal })
            {
                Assert.That(a.Count(n => n.Resource == kind && n.Size == Stage2ResourceSize.Small), Is.EqualTo(settings.smallDepositsPerKind));
                Assert.That(a.Count(n => n.Resource == kind && n.Capacity == 35), Is.EqualTo(settings.mediumDepositsPerKind));
                Assert.That(a.Count(n => n.Resource == kind && n.Capacity == 120), Is.EqualTo(settings.largeDepositsPerKind));
            }
            UnityEngine.Random.state = oldRandom;
        }

        [Test]
        public void ManualToolReachWidensLargerMineralsAndLeavesTreeAndLegacyRangesUnchanged()
        {
            var legacy = new Stage2Economy(map, catalog);
            Assert.That(legacy.IsInRange(legacy.GetNode(MapResourceKind.Wood),
                legacy.WorldPosition(MapResourceKind.Wood) + Vector2.right * 1.4f), Is.True);
            catalog.ConfigureManualResources(new Stage2ManualResourceSettings());
            var model = new Stage2Economy(map, catalog);
            Assert.That(model.GetNode(MapResourceKind.Stone).HarvestRadius, Is.EqualTo(.65f));
            Assert.That(model.GetNode(MapResourceKind.Wood).HarvestRadius, Is.Zero);
            var point = map.CellToWorld(new Vector2Int(90, 90));
            var tree = new Stage2ResourceNode(catalog.Get(MapResourceKind.Wood), map.WorldToCell(point),
                point, Stage2ResourceSize.Small, 0f);
            var rockPoint = point + Vector2.right * 10f;
            var rock = new Stage2ResourceNode(new Stage2ResourceSettings(MapResourceKind.Stone, 35, .8f, 30f),
                map.WorldToCell(rockPoint), rockPoint, Stage2ResourceSize.Medium, 1.1f);
            model.ReplaceAdditionalNodes(new[] { tree, rock });
            Assert.That(model.IsInRange(tree, point + Vector2.up * .55f), Is.True);
            Assert.That(model.IsInRange(tree, point + Vector2.up * .7f), Is.False);
            Assert.That(model.IsInRange(rock, rockPoint + Vector2.right * 1.9f), Is.True);
            Assert.That(model.IsInRange(rock, rockPoint + Vector2.right * 2f), Is.False);
            Assert.That(model.NearestAvailableNode(rockPoint + Vector2.right * 1.9f), Is.SameAs(rock));
            Assert.That(model.NearestAvailableNode(rockPoint + Vector2.right * 2f), Is.Null);
        }

        [Test]
        public void DepletedFootprintShrinksReachButKeepsTheFullSiteReserved()
        {
            catalog.ConfigureManualResources(new Stage2ManualResourceSettings());
            var model = new Stage2Economy(map, catalog);
            var point = map.CellToWorld(new Vector2Int(90, 90));
            var node = new Stage2ResourceNode(new Stage2ResourceSettings(MapResourceKind.Stone, 35, .8f, 30f),
                map.WorldToCell(point), point, Stage2ResourceSize.Medium, 1.1f,
                stageHarvestRadii: new[] { 1.1f, 1f, .85f, .7f });
            model.ReplaceAdditionalNodes(new[] { node });
            Assert.That(model.IsInRange(node, point + Vector2.right * 1.8f), Is.True);
            for (var i = 0; i < 31; i++) model.AdvanceGather(node, point, true, .8f, true);
            Assert.That(node.InteractionRadius, Is.EqualTo(.7f));
            Assert.That(node.HarvestRadius, Is.EqualTo(1.1f), "Placement must retain the maximum recovering footprint.");
            Assert.That(model.IsInRange(node, point + Vector2.right * 1.8f), Is.False);
            Assert.That(model.IsInRange(node, point + Vector2.right * 1f), Is.True);
            node.Reset(); Assert.That(node.InteractionRadius, Is.EqualTo(1.1f));
        }

        [Test]
        public void ReplacingWorldInvalidatesOldTargetWithoutRewardsAndRejectsDuplicateOwnership()
        {
            var model = new Stage2Economy(map, catalog);
            var old = Node(MapResourceKind.Wood, Stage2ResourceSize.Small, 5, Vector2.right * 4f);
            model.ReplaceAdditionalNodes(new[] { old });
            model.AdvanceGather(old, old.WorldPosition, true, .4f, true);
            var replacement = Node(MapResourceKind.Wood, Stage2ResourceSize.Small, 5, Vector2.left * 4f);
            model.ReplaceAdditionalNodes(new[] { replacement });
            Assert.That(model.CurrentNode, Is.Null); Assert.That(model.GatherProgress, Is.Zero);
            Assert.That(model.AdvanceGather(old, old.WorldPosition, true, .8f, true), Is.False);
            Assert.Throws<ArgumentException>(() => model.ReplaceAdditionalNodes(new[] { replacement, replacement }));
            Assert.That(model.Nodes.Last(), Is.SameAs(replacement), "Rejected replacement is atomic.");
            Assert.That(model.Inventory.Get(MapResourceKind.Wood), Is.Zero);
        }

        [TestCase(MapResourceKind.Stone, Stage2ResourceSize.Medium, .75f)]
        [TestCase(MapResourceKind.Stone, Stage2ResourceSize.Large, 1f)]
        [TestCase(MapResourceKind.Metal, Stage2ResourceSize.Medium, .75f)]
        [TestCase(MapResourceKind.Metal, Stage2ResourceSize.Large, 1f)]
        public void LargerMineralComfortReachFollowsAllStockStagesFromEverySide(MapResourceKind kind, Stage2ResourceSize size, float gap)
        {
            catalog.ConfigureManualResources(new Stage2ManualResourceSettings());
            var model = new Stage2Economy(map, catalog);
            var point = map.CellToWorld(new Vector2Int(90, 90));
            var node = new Stage2ResourceNode(new Stage2ResourceSettings(kind, 120, .8f, 30f),
                map.WorldToCell(point), point, size, 1.1f, stageHarvestRadii: new[] { 1.1f, 1f, .85f, .7f });
            model.ReplaceAdditionalNodes(new[] { node });
            foreach (var remaining in new[] { 120, 80, 50, 10 })
            {
                while (node.Remaining > remaining) model.AdvanceGather(node, point, true, .8f, true);
                model.CancelGather();
                foreach (var side in new[] { Vector2.left, Vector2.up, Vector2.right, Vector2.down })
                {
                    var comfortable = point + side * (node.InteractionRadius + gap);
                    Assert.That(model.IsInRange(node, comfortable), Is.True);
                    Assert.That(model.NearestAvailableNode(comfortable), Is.SameAs(node));
                    Assert.That(model.IsInRange(node, point + side * (node.InteractionRadius + gap + .2f)), Is.False);
                }
            }
        }
    }
}
