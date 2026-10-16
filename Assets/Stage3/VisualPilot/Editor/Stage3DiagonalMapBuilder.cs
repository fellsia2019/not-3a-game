using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Authors the accepted seven-wide double zigzag; retains the original builder entry point.</summary>
    internal static class Stage3DiagonalMapBuilder
    {
        internal static readonly Vector2Int Entrance = new Vector2Int(-42,-30);
        internal static readonly Vector2Int Objective = new Vector2Int(39,12);
        internal static readonly Vector2Int HeroStart = new Vector2Int(18,28);
        internal static readonly Vector2Int RapidShowcase = Objective + new Vector2Int(3,-1);
        internal static readonly Vector2Int LongshotShowcase = Objective + new Vector2Int(-1,3);
        private static readonly Vector2Int[] Route = {
            Entrance, new Vector2Int(-42,0), new Vector2Int(-24,0),
            new Vector2Int(-24,-24), new Vector2Int(-6,-24),
            new Vector2Int(-6,6), new Vector2Int(12,6),
            new Vector2Int(12,-18), new Vector2Int(39,-18), Objective
        };

        [MenuItem("Tools/Stage 3/Invasion Road/Apply Accepted Double Zigzag")]
        public static void ApplyAcceptedRoute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before changing the authored map.");
            var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(Stage3RapidPilotBuilder.TacticalMapPath);
            if (map == null) throw new InvalidOperationException("Stage 3 map is missing.");
            Build(map);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_INVASION_ROUTE_APPLIED: seven-wide horizontal double zigzag; entrance lower-left.");
        }

        internal static void Build(Stage2MapDefinition map)
        {
            var bounds=new MapCellArea(new Vector2Int(-125,-125),new Vector2Int(250,250));
            var corridor=new List<MapSurfaceRegion>();
            var buildAreas=new List<MapCellArea>();
            // Union the square-ended, seven-cell strips, then emit disjoint row
            // spans for the existing map format. Adjacent arms remain separated by grass.
            for(var y=bounds.Minimum.y;y<=bounds.MaximumInclusive.y;y++)
            {
                for(var x=bounds.Minimum.x;x<=bounds.MaximumInclusive.x;x++)
                {
                    if(!OnRoad(x,y))continue;
                    var first=x;
                    while(x<bounds.MaximumInclusive.x && OnRoad(x+1,y))x++;
                    var area=new MapCellArea(new Vector2Int(first,y),new Vector2Int(x-first+1,1));
                    corridor.Add(new MapSurfaceRegion(MapSurfaceKind.InvasionCorridor,area));
                    buildAreas.Add(area);
                }
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
            map.ConfigureForEditor(bounds,MapSurfaceKind.Forest,corridor.ToArray(),
                new MapCellMask(true,null,null),new MapCellMask(false,buildAreas.ToArray(),exclusions),
                true,Entrance,true,Objective,resources,bounds,
                map.ForestDebugColor,map.CorridorDebugColor);
            map.UsePlanarGridForEditor();
            map.KeepEnemiesOnInvasionCorridorForEditor();
            var validation=map.ValidateDefinition();
            if(!validation.IsValid)throw new InvalidOperationException(validation.FirstError);
            EditorUtility.SetDirty(map);
        }

        private static bool OnRoad(int x,int y)
        {
            const int halfWidth=3;
            for(var i=1;i<Route.Length;i++)
            {
                var a=Route[i-1]; var b=Route[i];
                if(x>=Mathf.Min(a.x,b.x)-halfWidth && x<=Mathf.Max(a.x,b.x)+halfWidth &&
                   y>=Mathf.Min(a.y,b.y)-halfWidth && y<=Mathf.Max(a.y,b.y)+halfWidth)return true;
            }
            return false;
        }

        private static MapResourceRegion Resource(MapResourceKind kind,Vector2Int anchor)
        {
            return new MapResourceRegion(kind,
                new MapCellArea(anchor-new Vector2Int(3,3),new Vector2Int(7,7)),anchor);
        }
    }
}
