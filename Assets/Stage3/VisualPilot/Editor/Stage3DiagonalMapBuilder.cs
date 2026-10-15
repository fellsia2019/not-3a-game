using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Rebuilds only the accepted Stage 3 map layout from authored cells.</summary>
    internal static class Stage3DiagonalMapBuilder
    {
        internal static readonly Vector2Int Entrance = new Vector2Int(46,44);
        internal static readonly Vector2Int Objective = new Vector2Int(39,39);
        internal static readonly Vector2Int HeroStart = new Vector2Int(18,28);
        internal static readonly Vector2Int RapidShowcase = new Vector2Int(42,38);
        internal static readonly Vector2Int LongshotShowcase = new Vector2Int(38,42);

        internal static void Build(Stage2MapDefinition map)
        {
            const int corner=49;
            // Along the diagonal: (a+b)/sqrt(2) <= 30. Across it:
            // |a-b|/sqrt(2) <= 6. Disjoint one-row regions retain the existing
            // cell-authored surface/placement contracts without a polygon system.
            const int alongLimit=42, acrossLimit=8;
            var corridor=new List<MapSurfaceRegion>();
            for(var b=0;b<=corner*2;b++)
            {
                var firstA=Mathf.Max(0,b-acrossLimit);
                var lastA=Mathf.Min(corner*2,Mathf.Min(b+acrossLimit,alongLimit-b));
                if(lastA<firstA)continue;
                corridor.Add(new MapSurfaceRegion(MapSurfaceKind.InvasionCorridor,
                    new MapCellArea(new Vector2Int(corner-lastA,corner-b),
                        new Vector2Int(lastA-firstA+1,1))));
            }

            var wood=new Vector2Int(17,32);
            var stone=new Vector2Int(8,22);
            var metal=new Vector2Int(27,11);
            var resources=new[]
            {
                Resource(MapResourceKind.Wood,wood),
                Resource(MapResourceKind.Stone,stone),
                Resource(MapResourceKind.Metal,metal)
            };
            var exclusions=new[]
            {
                new MapCellArea(Entrance-Vector2Int.one,new Vector2Int(3,3)),
                new MapCellArea(Objective-Vector2Int.one,new Vector2Int(3,3)),
                new MapCellArea(wood,Vector2Int.one),
                new MapCellArea(stone,Vector2Int.one),
                new MapCellArea(metal,Vector2Int.one)
            };
            var bounds=new MapCellArea(new Vector2Int(-50,-50),new Vector2Int(100,100));
            map.ConfigureForEditor(bounds,MapSurfaceKind.Forest,corridor.ToArray(),
                new MapCellMask(true,null,null),new MapCellMask(true,null,exclusions),
                true,Entrance,true,Objective,resources,bounds,
                map.ForestDebugColor,map.CorridorDebugColor);
            map.UsePlanarGridForEditor();
            var validation=map.ValidateDefinition();
            if(!validation.IsValid)throw new InvalidOperationException(validation.FirstError);
            EditorUtility.SetDirty(map);
        }

        private static MapResourceRegion Resource(MapResourceKind kind,Vector2Int anchor)
        {
            return new MapResourceRegion(kind,
                new MapCellArea(anchor-new Vector2Int(3,3),new Vector2Int(7,7)),anchor);
        }
    }
}
