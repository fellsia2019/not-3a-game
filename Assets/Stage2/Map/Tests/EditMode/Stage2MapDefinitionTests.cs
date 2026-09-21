using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Map.Tests
{
    public sealed class Stage2MapDefinitionTests
    {
        private const string CandidateAssetPath = "Assets/Stage2/Map/Data/Stage2Map100x100.asset";

        [Test]
        public void CandidateAsset_DescribesTheComplete100x100Map()
        {
            var definition = LoadCandidate();
            var validation = definition.ValidateDefinition();

            Assert.That(validation.IsValid, Is.True, validation.FirstError);
            Assert.That(definition.Size, Is.EqualTo(new Vector2Int(100, 100)));
            Assert.That(definition.MinimumCell, Is.EqualTo(new Vector2Int(-50, -50)));
            Assert.That(definition.MaximumCell, Is.EqualTo(new Vector2Int(49, 49)));
            Assert.That(definition.HasEnemyEntrance, Is.True);
            Assert.That(definition.HasThrone, Is.True);
            Assert.That(definition.ResourceRegionCount, Is.EqualTo(3));
            Assert.That(definition.GetSurface(definition.EnemyEntrance), Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            Assert.That(definition.GetSurface(definition.Throne), Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            Assert.That(definition.GetSurface(new Vector2Int(-45, 30)), Is.EqualTo(MapSurfaceKind.Forest));
            Assert.That(definition.IsWalkable(definition.EnemyEntrance), Is.True);
            Assert.That(definition.IsBuildable(definition.EnemyEntrance), Is.False);
            Assert.That(definition.IsWalkable(definition.Throne), Is.True);
            Assert.That(definition.IsBuildable(definition.Throne), Is.False);
            Assert.That(definition.CameraBounds.Contains(definition.EnemyEntrance), Is.True);
            Assert.That(definition.CameraBounds.Contains(definition.Throne), Is.True);
            Assert.That(definition.ForestDebugColor, Is.Not.EqualTo(definition.CorridorDebugColor));

            AssertResourceRegion(definition, MapResourceKind.Wood);
            AssertResourceRegion(definition, MapResourceKind.Stone);
            AssertResourceRegion(definition, MapResourceKind.Metal);
            var wood = GetRegion(definition, MapResourceKind.Wood);
            var stone = GetRegion(definition, MapResourceKind.Stone);
            var metal = GetRegion(definition, MapResourceKind.Metal);
            Assert.That(Distance(definition.Throne, wood.Anchor), Is.LessThan(Distance(definition.Throne, stone.Anchor)));
            Assert.That(Distance(definition.Throne, stone.Anchor), Is.LessThan(Distance(definition.Throne, metal.Anchor)));
        }

        [Test]
        public void CandidateAsset_CellWorldConversionRoundTripsAcrossTheBounds()
        {
            var definition = LoadCandidate();
            var cells = new[]
            {
                definition.MinimumCell,
                definition.MaximumCell,
                definition.EnemyEntrance,
                definition.Throne,
                new Vector2Int(-13, 27),
                Vector2Int.zero
            };

            foreach (var cell in cells)
            {
                Assert.That(definition.WorldToCell(definition.CellToWorld(cell)), Is.EqualTo(cell), $"Round-trip failed for {cell}.");
            }
        }

        [Test]
        public void CandidateAsset_ReadsAndValidationAreDeterministic()
        {
            var definition = LoadCandidate();
            var firstHash = definition.ComputeDeterministicContentHash();
            var firstValidation = definition.ValidateDefinition();
            var firstCells = ReadRepresentativeCells(definition);

            var secondHash = definition.ComputeDeterministicContentHash();
            var secondValidation = definition.ValidateDefinition();
            var secondCells = ReadRepresentativeCells(definition);

            Assert.That(secondHash, Is.EqualTo(firstHash));
            AssertValidationEqual(firstValidation, secondValidation);
            Assert.That(secondCells, Is.EqualTo(firstCells));
        }

        [Test]
        public void CandidateAsset_DebugMeshUsesDistinctSurfaceColors()
        {
            var definition = LoadCandidate();

            Assert.That(Stage2MapDebugRenderer.TryBuildMesh(definition, out var mesh, out var error), Is.True, error);
            try
            {
                Assert.That(mesh.vertexCount, Is.EqualTo(100 * 100 * 4));
                var colors = mesh.colors32;
                CollectionAssert.Contains(colors, (Color32)definition.ForestDebugColor);
                CollectionAssert.Contains(colors, (Color32)definition.CorridorDebugColor);
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void Validation_RejectsInvalidBoundsWithReason()
        {
            var definition = CreateDefinition(cellBounds: new MapCellArea(Vector2Int.zero, new Vector2Int(0, 10)));
            try
            {
                AssertIssue(definition.ValidateDefinition(), MapValidationCode.InvalidCellBounds, "positive");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Validation_RejectsMissingEntranceAndThroneWithReasons()
        {
            var definition = CreateDefinition(hasEntrance: false, hasThrone: false);
            try
            {
                var result = definition.ValidateDefinition();
                AssertIssue(result, MapValidationCode.MissingEntrance, "entrance");
                AssertIssue(result, MapValidationCode.MissingThrone, "throne");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Validation_RejectsLandmarkOutsideBoundsWithReason()
        {
            var definition = CreateDefinition(entrance: new Vector2Int(10, 4));
            try
            {
                AssertIssue(definition.ValidateDefinition(), MapValidationCode.LandmarkOutsideBounds, "outside");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Validation_RejectsIncompatibleAndOutOfBoundsMasksWithReasons()
        {
            var walkable = new MapCellMask(
                false,
                new[] { new MapCellArea(new Vector2Int(20, 20), Vector2Int.one) },
                null);
            var buildable = new MapCellMask(true, null, ReservedCells());
            var definition = CreateDefinition(walkableMask: walkable, buildableMask: buildable);
            try
            {
                var result = definition.ValidateDefinition();
                AssertIssue(result, MapValidationCode.InvalidMaskRegion, "inside cell bounds");
                AssertIssue(result, MapValidationCode.BuildableCellNotWalkable, "also be walkable");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Validation_RejectsInvalidResourceRegionAndAnchorWithReasons()
        {
            var regions = ValidResourceRegions();
            regions[2] = new MapResourceRegion(
                MapResourceKind.Metal,
                new MapCellArea(new Vector2Int(8, 8), new Vector2Int(4, 4)),
                new Vector2Int(12, 12));
            var definition = CreateDefinition(resourceRegions: regions);
            try
            {
                AssertIssue(definition.ValidateDefinition(), MapValidationCode.InvalidResourceRegion, "inside cell bounds");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Validation_RejectsCameraBoundsThatExcludeAuthoredLandmarks()
        {
            var definition = CreateDefinition(cameraBounds: new MapCellArea(Vector2Int.zero, new Vector2Int(5, 5)));
            try
            {
                var result = definition.ValidateDefinition();
                AssertIssue(result, MapValidationCode.CameraBoundsMissingLandmark, "enemy entrance");
                AssertIssue(result, MapValidationCode.CameraBoundsMissingResourceAnchor, "anchor");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Validation_RejectsCameraBoundsOutsideTheMap()
        {
            var definition = CreateDefinition(cameraBounds: new MapCellArea(new Vector2Int(9, 9), new Vector2Int(2, 2)));
            try
            {
                AssertIssue(definition.ValidateDefinition(), MapValidationCode.InvalidCameraBounds, "inside cell bounds");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void InvalidValidationResult_IsStableAcrossRepeatedReads()
        {
            var definition = CreateDefinition(hasEntrance: false, walkableMask: new MapCellMask(false, null, null));
            try
            {
                var first = definition.ValidateDefinition();
                var second = definition.ValidateDefinition();
                Assert.That(first.IsValid, Is.False);
                AssertValidationEqual(first, second);
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        private static Stage2MapDefinition LoadCandidate()
        {
            var definition = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(CandidateAssetPath);
            Assert.That(definition, Is.Not.Null, $"Missing generated candidate asset at {CandidateAssetPath}.");
            return definition;
        }

        private static Stage2MapDefinition CreateDefinition(
            MapCellArea cellBounds = null,
            bool hasEntrance = true,
            Vector2Int? entrance = null,
            bool hasThrone = true,
            MapCellMask walkableMask = null,
            MapCellMask buildableMask = null,
            MapResourceRegion[] resourceRegions = null,
            MapCellArea cameraBounds = null)
        {
            var definition = ScriptableObject.CreateInstance<Stage2MapDefinition>();
            definition.ConfigureForEditor(
                cellBounds ?? new MapCellArea(Vector2Int.zero, new Vector2Int(10, 10)),
                MapSurfaceKind.Forest,
                new[]
                {
                    new MapSurfaceRegion(
                        MapSurfaceKind.InvasionCorridor,
                        new MapCellArea(new Vector2Int(0, 4), new Vector2Int(10, 2)))
                },
                walkableMask ?? new MapCellMask(true, null, null),
                buildableMask ?? new MapCellMask(true, null, ReservedCells()),
                hasEntrance,
                entrance ?? new Vector2Int(9, 4),
                hasThrone,
                new Vector2Int(0, 4),
                resourceRegions ?? ValidResourceRegions(),
                cameraBounds ?? new MapCellArea(Vector2Int.zero, new Vector2Int(10, 10)),
                Color.green,
                Color.gray);
            return definition;
        }

        private static MapCellArea[] ReservedCells()
        {
            return new[]
            {
                SingleCell(new Vector2Int(9, 4)),
                SingleCell(new Vector2Int(0, 4)),
                SingleCell(new Vector2Int(1, 1)),
                SingleCell(new Vector2Int(4, 1)),
                SingleCell(new Vector2Int(7, 1))
            };
        }

        private static MapResourceRegion[] ValidResourceRegions()
        {
            return new[]
            {
                new MapResourceRegion(MapResourceKind.Wood, new MapCellArea(new Vector2Int(0, 0), new Vector2Int(3, 3)), new Vector2Int(1, 1)),
                new MapResourceRegion(MapResourceKind.Stone, new MapCellArea(new Vector2Int(3, 0), new Vector2Int(3, 3)), new Vector2Int(4, 1)),
                new MapResourceRegion(MapResourceKind.Metal, new MapCellArea(new Vector2Int(6, 0), new Vector2Int(3, 3)), new Vector2Int(7, 1))
            };
        }

        private static MapCellArea SingleCell(Vector2Int cell) => new MapCellArea(cell, Vector2Int.one);

        private static void AssertResourceRegion(Stage2MapDefinition definition, MapResourceKind resource)
        {
            Assert.That(definition.TryGetResourceRegion(resource, out var region), Is.True, $"Missing {resource} region.");
            Assert.That(region.Area.Contains(region.Anchor), Is.True);
            Assert.That(definition.Contains(region.Anchor), Is.True);
            Assert.That(definition.IsWalkable(region.Anchor), Is.True);
            Assert.That(definition.IsBuildable(region.Anchor), Is.False);
            Assert.That(definition.GetSurface(region.Anchor), Is.EqualTo(MapSurfaceKind.Forest));
            Assert.That(definition.CameraBounds.Contains(region.Anchor), Is.True);
        }

        private static MapResourceRegion GetRegion(Stage2MapDefinition definition, MapResourceKind resource)
        {
            Assert.That(definition.TryGetResourceRegion(resource, out var region), Is.True);
            return region;
        }

        private static int Distance(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y);
        }

        private static List<string> ReadRepresentativeCells(Stage2MapDefinition definition)
        {
            var result = new List<string>();
            for (var x = definition.MinimumCell.x; x <= definition.MaximumCell.x; x += 11)
            {
                for (var y = definition.MinimumCell.y; y <= definition.MaximumCell.y; y += 13)
                {
                    var cell = definition.ReadCell(new Vector2Int(x, y));
                    result.Add($"{x},{y}:{cell.Surface}:{cell.IsWalkable}:{cell.IsBuildable}");
                }
            }

            return result;
        }

        private static void AssertIssue(MapValidationResult result, MapValidationCode code, string messageFragment)
        {
            for (var index = 0; index < result.Issues.Count; index++)
            {
                var issue = result.Issues[index];
                if (issue.Code == code && issue.Message.ToLowerInvariant().Contains(messageFragment.ToLowerInvariant()))
                {
                    return;
                }
            }

            Assert.Fail($"Expected {code} containing '{messageFragment}', got: {string.Join(" | ", result.Issues)}");
        }

        private static void AssertValidationEqual(MapValidationResult first, MapValidationResult second)
        {
            Assert.That(second.IsValid, Is.EqualTo(first.IsValid));
            Assert.That(second.Issues.Count, Is.EqualTo(first.Issues.Count));
            for (var index = 0; index < first.Issues.Count; index++)
            {
                Assert.That(second.Issues[index].Code, Is.EqualTo(first.Issues[index].Code));
                Assert.That(second.Issues[index].Message, Is.EqualTo(first.Issues[index].Message));
            }
        }
    }
}
