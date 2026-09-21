using System;
using System.Collections.Generic;
using System.Globalization;
using Not3A.Stage1;
using UnityEngine;

namespace Not3A.Stage2.Map
{
    public enum MapSurfaceKind
    {
        Forest = 0,
        InvasionCorridor = 1
    }

    public enum MapResourceKind
    {
        Wood = 0,
        Stone = 1,
        Metal = 2
    }

    public enum MapValidationCode
    {
        InvalidCellBounds,
        InvalidSurfaceKind,
        InvalidSurfaceRegion,
        OverlappingSurfaceRegions,
        MissingRequiredSurface,
        MissingWalkableMask,
        MissingBuildableMask,
        InvalidMaskRegion,
        BuildableCellNotWalkable,
        MissingEntrance,
        MissingThrone,
        DuplicateLandmark,
        LandmarkOutsideBounds,
        LandmarkNotWalkable,
        LandmarkBuildable,
        LandmarkWrongSurface,
        MissingResourceRegion,
        DuplicateResourceRegion,
        InvalidResourceRegion,
        ResourceAnchorOutsideRegion,
        ResourceAnchorOutsideBounds,
        DuplicateResourceAnchor,
        ResourceAnchorNotWalkable,
        ResourceAnchorBuildable,
        ResourceAnchorWrongSurface,
        InvalidCameraBounds,
        CameraBoundsMissingLandmark,
        CameraBoundsMissingResourceAnchor,
        IndistinguishableDebugColors
    }

    public readonly struct MapValidationIssue
    {
        public MapValidationIssue(MapValidationCode code, string message)
        {
            Code = code;
            Message = message ?? string.Empty;
        }

        public MapValidationCode Code { get; }
        public string Message { get; }

        public override string ToString() => $"{Code}: {Message}";
    }

    public sealed class MapValidationResult
    {
        private readonly MapValidationIssue[] issues;

        internal MapValidationResult(List<MapValidationIssue> source)
        {
            issues = source == null ? Array.Empty<MapValidationIssue>() : source.ToArray();
        }

        public bool IsValid => issues.Length == 0;
        public IReadOnlyList<MapValidationIssue> Issues => issues;
        public string FirstError => IsValid ? string.Empty : issues[0].Message;
    }

    [Serializable]
    public sealed class MapCellArea
    {
        [SerializeField] private Vector2Int minimum;
        [SerializeField] private Vector2Int size = Vector2Int.one;

        public MapCellArea(Vector2Int minimum, Vector2Int size)
        {
            this.minimum = minimum;
            this.size = size;
        }

        public Vector2Int Minimum => minimum;
        public Vector2Int Size => size;
        public long CellCount => (long)size.x * size.y;

        public Vector2Int MaximumInclusive => new Vector2Int(
            minimum.x + size.x - 1,
            minimum.y + size.y - 1);

        public bool HasPositiveSize => size.x > 0 && size.y > 0;

        public bool Contains(Vector2Int cell)
        {
            if (!HasPositiveSize)
            {
                return false;
            }

            var maximumX = minimum.x + (long)size.x - 1L;
            var maximumY = minimum.y + (long)size.y - 1L;
            return cell.x >= minimum.x && cell.x <= maximumX &&
                   cell.y >= minimum.y && cell.y <= maximumY;
        }

        public bool Contains(MapCellArea other)
        {
            if (other == null || !HasPositiveSize || !other.HasPositiveSize)
            {
                return false;
            }

            var maximumX = minimum.x + (long)size.x - 1L;
            var maximumY = minimum.y + (long)size.y - 1L;
            var otherMaximumX = other.minimum.x + (long)other.size.x - 1L;
            var otherMaximumY = other.minimum.y + (long)other.size.y - 1L;
            return other.minimum.x >= minimum.x && otherMaximumX <= maximumX &&
                   other.minimum.y >= minimum.y && otherMaximumY <= maximumY;
        }

        public bool Overlaps(MapCellArea other)
        {
            if (other == null || !HasPositiveSize || !other.HasPositiveSize)
            {
                return false;
            }

            var maximumX = minimum.x + (long)size.x - 1L;
            var maximumY = minimum.y + (long)size.y - 1L;
            var otherMaximumX = other.minimum.x + (long)other.size.x - 1L;
            var otherMaximumY = other.minimum.y + (long)other.size.y - 1L;
            return minimum.x <= otherMaximumX && maximumX >= other.minimum.x &&
                   minimum.y <= otherMaximumY && maximumY >= other.minimum.y;
        }
    }

    [Serializable]
    public sealed class MapCellMask
    {
        [SerializeField] private bool defaultValue;
        [SerializeField] private MapCellArea[] enabledAreas = Array.Empty<MapCellArea>();
        [SerializeField] private MapCellArea[] disabledAreas = Array.Empty<MapCellArea>();

        public MapCellMask(bool defaultValue, MapCellArea[] enabledAreas, MapCellArea[] disabledAreas)
        {
            this.defaultValue = defaultValue;
            this.enabledAreas = Clone(enabledAreas);
            this.disabledAreas = Clone(disabledAreas);
        }

        public bool DefaultValue => defaultValue;
        internal IReadOnlyList<MapCellArea> EnabledAreas => enabledAreas ?? Array.Empty<MapCellArea>();
        internal IReadOnlyList<MapCellArea> DisabledAreas => disabledAreas ?? Array.Empty<MapCellArea>();

        public bool Contains(Vector2Int cell)
        {
            var value = defaultValue;
            if (enabledAreas != null)
            {
                for (var index = 0; index < enabledAreas.Length; index++)
                {
                    if (enabledAreas[index] != null && enabledAreas[index].Contains(cell))
                    {
                        value = true;
                    }
                }
            }

            if (disabledAreas != null)
            {
                for (var index = 0; index < disabledAreas.Length; index++)
                {
                    if (disabledAreas[index] != null && disabledAreas[index].Contains(cell))
                    {
                        return false;
                    }
                }
            }

            return value;
        }

        private static MapCellArea[] Clone(MapCellArea[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<MapCellArea>();
            }

            var clone = new MapCellArea[source.Length];
            Array.Copy(source, clone, source.Length);
            return clone;
        }
    }

    [Serializable]
    public sealed class MapSurfaceRegion
    {
        [SerializeField] private MapSurfaceKind surface;
        [SerializeField] private MapCellArea area;

        public MapSurfaceRegion(MapSurfaceKind surface, MapCellArea area)
        {
            this.surface = surface;
            this.area = area;
        }

        public MapSurfaceKind Surface => surface;
        public MapCellArea Area => area;
    }

    [Serializable]
    public sealed class MapResourceRegion
    {
        [SerializeField] private MapResourceKind resource;
        [SerializeField] private MapCellArea area;
        [SerializeField] private Vector2Int anchor;

        public MapResourceRegion(MapResourceKind resource, MapCellArea area, Vector2Int anchor)
        {
            this.resource = resource;
            this.area = area;
            this.anchor = anchor;
        }

        public MapResourceKind Resource => resource;
        public MapCellArea Area => area;
        public Vector2Int Anchor => anchor;
    }

    public readonly struct MapCellData
    {
        public MapCellData(MapSurfaceKind surface, bool isWalkable, bool isBuildable)
        {
            Surface = surface;
            IsWalkable = isWalkable;
            IsBuildable = isBuildable;
        }

        public MapSurfaceKind Surface { get; }
        public bool IsWalkable { get; }
        public bool IsBuildable { get; }
    }

    [CreateAssetMenu(fileName = "Stage2MapDefinition", menuName = "Not 3A/Stage 2/Map Definition")]
    public sealed class Stage2MapDefinition : ScriptableObject
    {
        private const long MaximumValidationCellCount = 1_000_000L;

        [Header("Cell Space")]
        [SerializeField] private MapCellArea cellBounds = new MapCellArea(Vector2Int.zero, Vector2Int.one);
        [SerializeField] private MapSurfaceKind defaultSurface = MapSurfaceKind.Forest;
        [SerializeField] private MapSurfaceRegion[] surfaceRegions = Array.Empty<MapSurfaceRegion>();

        [Header("Masks")]
        [SerializeField] private MapCellMask walkableMask = new MapCellMask(true, null, null);
        [SerializeField] private MapCellMask buildableMask = new MapCellMask(true, null, null);

        [Header("Landmarks")]
        [SerializeField] private bool hasEnemyEntrance;
        [SerializeField] private Vector2Int enemyEntrance;
        [SerializeField] private bool hasThrone;
        [SerializeField] private Vector2Int throne;

        [Header("Resource Placement")]
        [SerializeField] private MapResourceRegion[] resourceRegions = Array.Empty<MapResourceRegion>();

        [Header("Camera And Debug")]
        [SerializeField] private MapCellArea cameraBounds = new MapCellArea(Vector2Int.zero, Vector2Int.one);
        [SerializeField] private Color forestDebugColor = new Color(0.18f, 0.34f, 0.19f, 1f);
        [SerializeField] private Color corridorDebugColor = new Color(0.34f, 0.37f, 0.38f, 1f);

        public MapCellArea CellBounds => cellBounds;
        public Vector2Int MinimumCell => cellBounds.Minimum;
        public Vector2Int MaximumCell => cellBounds.MaximumInclusive;
        public Vector2Int Size => cellBounds.Size;
        public bool HasEnemyEntrance => hasEnemyEntrance;
        public Vector2Int EnemyEntrance => enemyEntrance;
        public bool HasThrone => hasThrone;
        public Vector2Int Throne => throne;
        public MapCellArea CameraBounds => cameraBounds;
        public Color ForestDebugColor => forestDebugColor;
        public Color CorridorDebugColor => corridorDebugColor;
        public int ResourceRegionCount => resourceRegions == null ? 0 : resourceRegions.Length;

        public bool Contains(Vector2Int cell) => cellBounds != null && cellBounds.Contains(cell);

        public Vector2 CellToWorld(Vector2Int cell) => IsoGrid.CellToWorld(cell);

        public Vector2Int WorldToCell(Vector2 world) => IsoGrid.WorldToCell(world);

        public MapCellData ReadCell(Vector2Int cell)
        {
            EnsureInsideBounds(cell);
            return new MapCellData(GetSurfaceUnchecked(cell), walkableMask.Contains(cell), buildableMask.Contains(cell));
        }

        public MapSurfaceKind GetSurface(Vector2Int cell)
        {
            EnsureInsideBounds(cell);
            return GetSurfaceUnchecked(cell);
        }

        public bool IsWalkable(Vector2Int cell)
        {
            EnsureInsideBounds(cell);
            return walkableMask.Contains(cell);
        }

        public bool IsBuildable(Vector2Int cell)
        {
            EnsureInsideBounds(cell);
            return buildableMask.Contains(cell);
        }

        public MapResourceRegion GetResourceRegion(int index)
        {
            if (resourceRegions == null || index < 0 || index >= resourceRegions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return resourceRegions[index];
        }

        public bool TryGetResourceRegion(MapResourceKind resource, out MapResourceRegion region)
        {
            if (resourceRegions != null)
            {
                for (var index = 0; index < resourceRegions.Length; index++)
                {
                    var candidate = resourceRegions[index];
                    if (candidate != null && candidate.Resource == resource)
                    {
                        region = candidate;
                        return true;
                    }
                }
            }

            region = null;
            return false;
        }

        public MapValidationResult ValidateDefinition()
        {
            var issues = new List<MapValidationIssue>();
            var boundsAreUsable = ValidateCellBounds(issues);
            ValidateSurfaces(issues, boundsAreUsable);
            ValidateMask("walkable", walkableMask, MapValidationCode.MissingWalkableMask, issues, boundsAreUsable);
            ValidateMask("buildable", buildableMask, MapValidationCode.MissingBuildableMask, issues, boundsAreUsable);
            ValidateMasksTogether(issues, boundsAreUsable);
            ValidateLandmarks(issues, boundsAreUsable);
            ValidateResources(issues, boundsAreUsable);
            ValidateCameraBounds(issues, boundsAreUsable);
            ValidateDebugColors(issues);
            return new MapValidationResult(issues);
        }

        public string ComputeDeterministicContentHash()
        {
            var hash = 14695981039346656037UL;
            HashArea(ref hash, cellBounds);
            HashInt(ref hash, (int)defaultSurface);
            HashSurfaceRegions(ref hash, surfaceRegions);
            HashMask(ref hash, walkableMask);
            HashMask(ref hash, buildableMask);
            HashBool(ref hash, hasEnemyEntrance);
            HashVector(ref hash, enemyEntrance);
            HashBool(ref hash, hasThrone);
            HashVector(ref hash, throne);
            HashResourceRegions(ref hash, resourceRegions);
            HashArea(ref hash, cameraBounds);
            HashColor(ref hash, forestDebugColor);
            HashColor(ref hash, corridorDebugColor);
            return hash.ToString("X16", CultureInfo.InvariantCulture);
        }

#if UNITY_EDITOR
        public void ConfigureForEditor(
            MapCellArea authoredCellBounds,
            MapSurfaceKind authoredDefaultSurface,
            MapSurfaceRegion[] authoredSurfaceRegions,
            MapCellMask authoredWalkableMask,
            MapCellMask authoredBuildableMask,
            bool authoredHasEnemyEntrance,
            Vector2Int authoredEnemyEntrance,
            bool authoredHasThrone,
            Vector2Int authoredThrone,
            MapResourceRegion[] authoredResourceRegions,
            MapCellArea authoredCameraBounds,
            Color authoredForestDebugColor,
            Color authoredCorridorDebugColor)
        {
            cellBounds = authoredCellBounds;
            defaultSurface = authoredDefaultSurface;
            surfaceRegions = authoredSurfaceRegions ?? Array.Empty<MapSurfaceRegion>();
            walkableMask = authoredWalkableMask;
            buildableMask = authoredBuildableMask;
            hasEnemyEntrance = authoredHasEnemyEntrance;
            enemyEntrance = authoredEnemyEntrance;
            hasThrone = authoredHasThrone;
            throne = authoredThrone;
            resourceRegions = authoredResourceRegions ?? Array.Empty<MapResourceRegion>();
            cameraBounds = authoredCameraBounds;
            forestDebugColor = authoredForestDebugColor;
            corridorDebugColor = authoredCorridorDebugColor;
        }
#endif

        private bool ValidateCellBounds(List<MapValidationIssue> issues)
        {
            if (cellBounds == null || !cellBounds.HasPositiveSize)
            {
                AddIssue(issues, MapValidationCode.InvalidCellBounds, "Cell bounds size must be positive.");
                return false;
            }

            var maximumX = cellBounds.Minimum.x + (long)cellBounds.Size.x - 1L;
            var maximumY = cellBounds.Minimum.y + (long)cellBounds.Size.y - 1L;
            if (maximumX > int.MaxValue || maximumX < int.MinValue ||
                maximumY > int.MaxValue || maximumY < int.MinValue ||
                cellBounds.CellCount > MaximumValidationCellCount)
            {
                AddIssue(
                    issues,
                    MapValidationCode.InvalidCellBounds,
                    $"Cell bounds must fit Int32 coordinates and contain at most {MaximumValidationCellCount} cells.");
                return false;
            }

            return true;
        }

        private void ValidateSurfaces(List<MapValidationIssue> issues, bool boundsAreUsable)
        {
            if (!Enum.IsDefined(typeof(MapSurfaceKind), defaultSurface))
            {
                AddIssue(issues, MapValidationCode.InvalidSurfaceKind, $"Default surface value {(int)defaultSurface} is not supported.");
            }

            if (surfaceRegions != null)
            {
                for (var index = 0; index < surfaceRegions.Length; index++)
                {
                    var region = surfaceRegions[index];
                    if (region == null || !Enum.IsDefined(typeof(MapSurfaceKind), region.Surface) ||
                        !IsAreaValidInsideBounds(region.Area, boundsAreUsable))
                    {
                        AddIssue(issues, MapValidationCode.InvalidSurfaceRegion, $"Surface region {index} must use a supported surface and remain inside cell bounds.");
                        continue;
                    }

                    for (var otherIndex = index + 1; otherIndex < surfaceRegions.Length; otherIndex++)
                    {
                        var other = surfaceRegions[otherIndex];
                        if (other != null && region.Area.Overlaps(other.Area))
                        {
                            AddIssue(issues, MapValidationCode.OverlappingSurfaceRegions, $"Surface regions {index} and {otherIndex} overlap; each cell must have one authored surface override.");
                        }
                    }
                }
            }

            if (!boundsAreUsable)
            {
                return;
            }

            var foundForest = false;
            var foundCorridor = false;
            ForEachCell(cell =>
            {
                var surface = GetSurfaceUnchecked(cell);
                foundForest |= surface == MapSurfaceKind.Forest;
                foundCorridor |= surface == MapSurfaceKind.InvasionCorridor;
                return !foundForest || !foundCorridor;
            });

            if (!foundForest)
            {
                AddIssue(issues, MapValidationCode.MissingRequiredSurface, "Map must contain at least one forest cell.");
            }

            if (!foundCorridor)
            {
                AddIssue(issues, MapValidationCode.MissingRequiredSurface, "Map must contain at least one invasion-corridor cell.");
            }
        }

        private void ValidateMask(
            string label,
            MapCellMask mask,
            MapValidationCode missingCode,
            List<MapValidationIssue> issues,
            bool boundsAreUsable)
        {
            if (mask == null)
            {
                AddIssue(issues, missingCode, $"The {label} mask is missing.");
                return;
            }

            ValidateMaskAreas(label, "enabled", mask.EnabledAreas, issues, boundsAreUsable);
            ValidateMaskAreas(label, "disabled", mask.DisabledAreas, issues, boundsAreUsable);
        }

        private void ValidateMaskAreas(
            string maskLabel,
            string areaLabel,
            IReadOnlyList<MapCellArea> areas,
            List<MapValidationIssue> issues,
            bool boundsAreUsable)
        {
            for (var index = 0; index < areas.Count; index++)
            {
                if (!IsAreaValidInsideBounds(areas[index], boundsAreUsable))
                {
                    AddIssue(issues, MapValidationCode.InvalidMaskRegion, $"{maskLabel} mask {areaLabel} area {index} must have positive size and remain inside cell bounds.");
                }
            }
        }

        private void ValidateMasksTogether(List<MapValidationIssue> issues, bool boundsAreUsable)
        {
            if (!boundsAreUsable || walkableMask == null || buildableMask == null)
            {
                return;
            }

            ForEachCell(cell =>
            {
                if (!buildableMask.Contains(cell) || walkableMask.Contains(cell))
                {
                    return true;
                }

                AddIssue(issues, MapValidationCode.BuildableCellNotWalkable, $"Buildable cell {cell} must also be walkable before occupancy.");
                return false;
            });
        }

        private void ValidateLandmarks(List<MapValidationIssue> issues, bool boundsAreUsable)
        {
            if (!hasEnemyEntrance)
            {
                AddIssue(issues, MapValidationCode.MissingEntrance, "Exactly one enemy entrance must be authored.");
            }

            if (!hasThrone)
            {
                AddIssue(issues, MapValidationCode.MissingThrone, "Exactly one throne cell must be authored.");
            }

            if (hasEnemyEntrance && hasThrone && enemyEntrance == throne)
            {
                AddIssue(issues, MapValidationCode.DuplicateLandmark, "Enemy entrance and throne must occupy different cells.");
            }

            if (!boundsAreUsable)
            {
                return;
            }

            if (hasEnemyEntrance)
            {
                ValidateLandmark("Enemy entrance", enemyEntrance, issues);
            }

            if (hasThrone)
            {
                ValidateLandmark("Throne", throne, issues);
            }
        }

        private void ValidateLandmark(string label, Vector2Int cell, List<MapValidationIssue> issues)
        {
            if (!cellBounds.Contains(cell))
            {
                AddIssue(issues, MapValidationCode.LandmarkOutsideBounds, $"{label} cell {cell} is outside cell bounds.");
                return;
            }

            if (walkableMask != null && !walkableMask.Contains(cell))
            {
                AddIssue(issues, MapValidationCode.LandmarkNotWalkable, $"{label} cell {cell} must be walkable.");
            }

            if (buildableMask != null && buildableMask.Contains(cell))
            {
                AddIssue(issues, MapValidationCode.LandmarkBuildable, $"{label} cell {cell} must not be buildable.");
            }

            if (GetSurfaceUnchecked(cell) != MapSurfaceKind.InvasionCorridor)
            {
                AddIssue(issues, MapValidationCode.LandmarkWrongSurface, $"{label} cell {cell} must be on the invasion corridor.");
            }
        }

        private void ValidateResources(List<MapValidationIssue> issues, bool boundsAreUsable)
        {
            var found = new bool[3];
            var anchors = new HashSet<Vector2Int>();
            if (resourceRegions != null)
            {
                for (var index = 0; index < resourceRegions.Length; index++)
                {
                    var region = resourceRegions[index];
                    if (region == null || !Enum.IsDefined(typeof(MapResourceKind), region.Resource))
                    {
                        AddIssue(issues, MapValidationCode.InvalidResourceRegion, $"Resource region {index} must declare a supported resource kind.");
                        continue;
                    }

                    var resourceIndex = (int)region.Resource;
                    if (found[resourceIndex])
                    {
                        AddIssue(issues, MapValidationCode.DuplicateResourceRegion, $"Resource {region.Resource} must have exactly one placement region.");
                    }
                    else
                    {
                        found[resourceIndex] = true;
                    }

                    if (!IsAreaValidInsideBounds(region.Area, boundsAreUsable))
                    {
                        AddIssue(issues, MapValidationCode.InvalidResourceRegion, $"Resource region {index} must have positive size and remain inside cell bounds.");
                        continue;
                    }

                    if (!region.Area.Contains(region.Anchor))
                    {
                        AddIssue(issues, MapValidationCode.ResourceAnchorOutsideRegion, $"{region.Resource} anchor {region.Anchor} must be inside its placement region.");
                    }

                    if (!cellBounds.Contains(region.Anchor))
                    {
                        AddIssue(issues, MapValidationCode.ResourceAnchorOutsideBounds, $"{region.Resource} anchor {region.Anchor} is outside cell bounds.");
                        continue;
                    }

                    if (!anchors.Add(region.Anchor))
                    {
                        AddIssue(issues, MapValidationCode.DuplicateResourceAnchor, $"Resource anchor {region.Anchor} is assigned more than once.");
                    }

                    if (walkableMask != null && !walkableMask.Contains(region.Anchor))
                    {
                        AddIssue(issues, MapValidationCode.ResourceAnchorNotWalkable, $"{region.Resource} anchor {region.Anchor} must be walkable.");
                    }

                    if (buildableMask != null && buildableMask.Contains(region.Anchor))
                    {
                        AddIssue(issues, MapValidationCode.ResourceAnchorBuildable, $"{region.Resource} anchor {region.Anchor} must be reserved from building.");
                    }

                    if (GetSurfaceUnchecked(region.Anchor) != MapSurfaceKind.Forest)
                    {
                        AddIssue(issues, MapValidationCode.ResourceAnchorWrongSurface, $"{region.Resource} anchor {region.Anchor} must be on forest terrain.");
                    }
                }
            }

            for (var resourceIndex = 0; resourceIndex < found.Length; resourceIndex++)
            {
                if (!found[resourceIndex])
                {
                    AddIssue(issues, MapValidationCode.MissingResourceRegion, $"A placement region for {(MapResourceKind)resourceIndex} is required.");
                }
            }
        }

        private void ValidateCameraBounds(List<MapValidationIssue> issues, bool boundsAreUsable)
        {
            if (!IsAreaValidInsideBounds(cameraBounds, boundsAreUsable))
            {
                AddIssue(issues, MapValidationCode.InvalidCameraBounds, "Camera bounds must have positive size and remain inside cell bounds.");
                return;
            }

            if (hasEnemyEntrance && cellBounds.Contains(enemyEntrance) && !cameraBounds.Contains(enemyEntrance))
            {
                AddIssue(issues, MapValidationCode.CameraBoundsMissingLandmark, $"Camera bounds must contain enemy entrance {enemyEntrance}.");
            }

            if (hasThrone && cellBounds.Contains(throne) && !cameraBounds.Contains(throne))
            {
                AddIssue(issues, MapValidationCode.CameraBoundsMissingLandmark, $"Camera bounds must contain throne {throne}.");
            }

            if (resourceRegions == null)
            {
                return;
            }

            for (var index = 0; index < resourceRegions.Length; index++)
            {
                var region = resourceRegions[index];
                if (region != null && cellBounds.Contains(region.Anchor) && !cameraBounds.Contains(region.Anchor))
                {
                    AddIssue(issues, MapValidationCode.CameraBoundsMissingResourceAnchor, $"Camera bounds must contain {region.Resource} anchor {region.Anchor}.");
                }
            }
        }

        private void ValidateDebugColors(List<MapValidationIssue> issues)
        {
            var forest = (Color32)forestDebugColor;
            var corridor = (Color32)corridorDebugColor;
            if (forest.Equals(corridor))
            {
                AddIssue(issues, MapValidationCode.IndistinguishableDebugColors, "Forest and invasion-corridor debug colors must be different.");
            }
        }

        private bool IsAreaValidInsideBounds(MapCellArea area, bool boundsAreUsable)
        {
            return boundsAreUsable && area != null && area.HasPositiveSize && cellBounds.Contains(area);
        }

        private MapSurfaceKind GetSurfaceUnchecked(Vector2Int cell)
        {
            if (surfaceRegions != null)
            {
                for (var index = 0; index < surfaceRegions.Length; index++)
                {
                    var region = surfaceRegions[index];
                    if (region != null && region.Area != null && region.Area.Contains(cell))
                    {
                        return region.Surface;
                    }
                }
            }

            return defaultSurface;
        }

        private void EnsureInsideBounds(Vector2Int cell)
        {
            if (!Contains(cell))
            {
                throw new ArgumentOutOfRangeException(nameof(cell), cell, "Cell is outside the authored map bounds.");
            }
        }

        private void ForEachCell(Func<Vector2Int, bool> visitor)
        {
            for (var xOffset = 0; xOffset < cellBounds.Size.x; xOffset++)
            {
                for (var yOffset = 0; yOffset < cellBounds.Size.y; yOffset++)
                {
                    var cell = new Vector2Int(cellBounds.Minimum.x + xOffset, cellBounds.Minimum.y + yOffset);
                    if (!visitor(cell))
                    {
                        return;
                    }
                }
            }
        }

        private static void AddIssue(List<MapValidationIssue> issues, MapValidationCode code, string message)
        {
            issues.Add(new MapValidationIssue(code, message));
        }

        private static void HashSurfaceRegions(ref ulong hash, MapSurfaceRegion[] regions)
        {
            HashInt(ref hash, regions == null ? -1 : regions.Length);
            if (regions == null)
            {
                return;
            }

            for (var index = 0; index < regions.Length; index++)
            {
                var region = regions[index];
                HashBool(ref hash, region != null);
                if (region != null)
                {
                    HashInt(ref hash, (int)region.Surface);
                    HashArea(ref hash, region.Area);
                }
            }
        }

        private static void HashMask(ref ulong hash, MapCellMask mask)
        {
            HashBool(ref hash, mask != null);
            if (mask == null)
            {
                return;
            }

            HashBool(ref hash, mask.DefaultValue);
            HashAreas(ref hash, mask.EnabledAreas);
            HashAreas(ref hash, mask.DisabledAreas);
        }

        private static void HashAreas(ref ulong hash, IReadOnlyList<MapCellArea> areas)
        {
            HashInt(ref hash, areas.Count);
            for (var index = 0; index < areas.Count; index++)
            {
                HashArea(ref hash, areas[index]);
            }
        }

        private static void HashResourceRegions(ref ulong hash, MapResourceRegion[] regions)
        {
            HashInt(ref hash, regions == null ? -1 : regions.Length);
            if (regions == null)
            {
                return;
            }

            for (var index = 0; index < regions.Length; index++)
            {
                var region = regions[index];
                HashBool(ref hash, region != null);
                if (region != null)
                {
                    HashInt(ref hash, (int)region.Resource);
                    HashArea(ref hash, region.Area);
                    HashVector(ref hash, region.Anchor);
                }
            }
        }

        private static void HashArea(ref ulong hash, MapCellArea area)
        {
            HashBool(ref hash, area != null);
            if (area == null)
            {
                return;
            }

            HashVector(ref hash, area.Minimum);
            HashVector(ref hash, area.Size);
        }

        private static void HashColor(ref ulong hash, Color color)
        {
            var bytes = (Color32)color;
            HashByte(ref hash, bytes.r);
            HashByte(ref hash, bytes.g);
            HashByte(ref hash, bytes.b);
            HashByte(ref hash, bytes.a);
        }

        private static void HashVector(ref ulong hash, Vector2Int value)
        {
            HashInt(ref hash, value.x);
            HashInt(ref hash, value.y);
        }

        private static void HashBool(ref ulong hash, bool value) => HashByte(ref hash, value ? (byte)1 : (byte)0);

        private static void HashInt(ref ulong hash, int value)
        {
            unchecked
            {
                HashByte(ref hash, (byte)value);
                HashByte(ref hash, (byte)(value >> 8));
                HashByte(ref hash, (byte)(value >> 16));
                HashByte(ref hash, (byte)(value >> 24));
            }
        }

        private static void HashByte(ref ulong hash, byte value)
        {
            unchecked
            {
                hash ^= value;
                hash *= 1099511628211UL;
            }
        }
    }
}
