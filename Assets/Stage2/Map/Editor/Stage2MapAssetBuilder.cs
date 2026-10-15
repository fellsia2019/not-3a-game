using System;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage2.Map.Editor
{
    public static class Stage2MapAssetBuilder
    {
        public const string CandidateAssetPath = "Assets/Stage2/Map/Data/Stage2Map100x100.asset";
        public const string CornerAssetPath = "Assets/Stage2/Map/Data/Stage2Corner32x32.asset";

        [MenuItem("Tools/Stage 2/Create or Update Corner Playable Map")]
        public static void CreateOrUpdateCornerPlayable()
        {
            var definition = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(CornerAssetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<Stage2MapDefinition>();
                definition.name = "Stage2Corner32x32";
                AssetDatabase.CreateAsset(definition, CornerAssetPath);
            }

            var entrance = new Vector2Int(-14, -12);
            var throne = new Vector2Int(-5, -12);
            var wood = new Vector2Int(10, -10);
            var stone = new Vector2Int(-10, 10);
            var metal = new Vector2Int(10, 10);
            var bounds = new MapCellArea(new Vector2Int(-16, -16), new Vector2Int(32, 32));
            definition.ConfigureForEditor(
                bounds,
                MapSurfaceKind.Forest,
                new[] { new MapSurfaceRegion(MapSurfaceKind.InvasionCorridor,
                    new MapCellArea(new Vector2Int(-16, -14), new Vector2Int(19, 6))) },
                new MapCellMask(true, null, null),
                new MapCellMask(true, null, new[]
                {
                    SingleCell(entrance), SingleCell(throne), SingleCell(wood),
                    SingleCell(stone), SingleCell(metal)
                }),
                true, entrance, true, throne,
                new[]
                {
                    new MapResourceRegion(MapResourceKind.Wood,
                        new MapCellArea(new Vector2Int(7, -13), new Vector2Int(7, 7)), wood),
                    new MapResourceRegion(MapResourceKind.Stone,
                        new MapCellArea(new Vector2Int(-13, 7), new Vector2Int(7, 7)), stone),
                    new MapResourceRegion(MapResourceKind.Metal,
                        new MapCellArea(new Vector2Int(7, 7), new Vector2Int(7, 7)), metal)
                },
                bounds,
                new Color(0.18f, 0.34f, 0.19f, 1f),
                new Color(0.34f, 0.37f, 0.38f, 1f));

            var validation = definition.ValidateDefinition();
            if (!validation.IsValid)
                throw new InvalidOperationException($"Stage 2 corner map is invalid: {validation.Issues[0]}");
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = definition;
            Debug.Log($"S2_MAP_ASSET_OK path={CornerAssetPath} size=32x32 hash={definition.ComputeDeterministicContentHash()}");
        }

        private static readonly Vector2Int MapMinimum = new Vector2Int(-50, -50);
        private static readonly Vector2Int MapSize = new Vector2Int(100, 100);
        private static readonly Vector2Int Entrance = new Vector2Int(49, 0);
        private static readonly Vector2Int Throne = new Vector2Int(-40, 0);
        private static readonly Vector2Int WoodAnchor = new Vector2Int(-36, 14);
        private static readonly Vector2Int StoneAnchor = new Vector2Int(-15, 28);
        private static readonly Vector2Int MetalAnchor = new Vector2Int(10, 42);

        [MenuItem("Tools/Stage 2/Create or Update 100x100 Map Candidate")]
        public static void CreateOrUpdateCandidate()
        {
            var definition = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(CandidateAssetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<Stage2MapDefinition>();
                definition.name = "Stage2 Map 100x100 Candidate";
                AssetDatabase.CreateAsset(definition, CandidateAssetPath);
            }

            var reservedBuildCells = new[]
            {
                SingleCell(Entrance),
                SingleCell(Throne),
                SingleCell(WoodAnchor),
                SingleCell(StoneAnchor),
                SingleCell(MetalAnchor)
            };

            definition.ConfigureForEditor(
                new MapCellArea(MapMinimum, MapSize),
                MapSurfaceKind.Forest,
                new[]
                {
                    new MapSurfaceRegion(
                        MapSurfaceKind.InvasionCorridor,
                        new MapCellArea(new Vector2Int(-50, -4), new Vector2Int(100, 9)))
                },
                new MapCellMask(true, null, null),
                new MapCellMask(true, null, reservedBuildCells),
                true,
                Entrance,
                true,
                Throne,
                new[]
                {
                    new MapResourceRegion(
                        MapResourceKind.Wood,
                        new MapCellArea(new Vector2Int(-44, 8), new Vector2Int(16, 14)),
                        WoodAnchor),
                    new MapResourceRegion(
                        MapResourceKind.Stone,
                        new MapCellArea(new Vector2Int(-24, 20), new Vector2Int(18, 16)),
                        StoneAnchor),
                    new MapResourceRegion(
                        MapResourceKind.Metal,
                        new MapCellArea(new Vector2Int(0, 34), new Vector2Int(20, 15)),
                        MetalAnchor)
                },
                new MapCellArea(MapMinimum, MapSize),
                new Color(0.18f, 0.34f, 0.19f, 1f),
                new Color(0.34f, 0.37f, 0.38f, 1f));

            var validation = definition.ValidateDefinition();
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Stage 2 map candidate is invalid: {validation.Issues[0]}");
            }

            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = definition;
            Debug.Log($"S2_MAP_ASSET_OK path={CandidateAssetPath} size={definition.Size.x}x{definition.Size.y} hash={definition.ComputeDeterministicContentHash()}");
        }

        private static MapCellArea SingleCell(Vector2Int cell) => new MapCellArea(cell, Vector2Int.one);
    }
}
