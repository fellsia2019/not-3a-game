using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Authors the player's seven-wide curved road; retains the original builder entry point.</summary>
    internal static class Stage3DiagonalMapBuilder
    {
        internal static readonly Vector2Int Entrance = new Vector2Int(-42,12);
        internal static readonly Vector2Int Objective = new Vector2Int(55,12);
        internal static readonly Vector2Int HeroStart = new Vector2Int(18,28);
        internal static readonly Vector2Int RapidShowcase = Objective + new Vector2Int(3,-1);
        internal static readonly Vector2Int LongshotShowcase = Objective + new Vector2Int(-1,3);
        private static readonly Vector2[][] Curves = {
            new Vector2[]{Entrance,new Vector2(-42,0),new Vector2(-39,-16),new Vector2(-39,-27)},
            new[]{new Vector2(-39,-27),new Vector2(-39,-32),new Vector2(-30,-31),new Vector2(-16,-31)},
            new[]{new Vector2(-16,-31),new Vector2(-6,-32),new Vector2(-3,-29),new Vector2(-3,-20)},
            new[]{new Vector2(-3,-20),new Vector2(-3,-11),new Vector2(-9,-5),new Vector2(2,-4)},
            new[]{new Vector2(2,-4),new Vector2(15,-2),new Vector2(13,-14),new Vector2(13,-25)},
            new[]{new Vector2(13,-25),new Vector2(13,-32),new Vector2(9,-34),new Vector2(18,-34)},
            new[]{new Vector2(18,-34),new Vector2(37,-34),new Vector2(37,-32),new Vector2(37,-21)},
            new[]{new Vector2(37,-21),new Vector2(37,-8),new Vector2(33,12),new Vector2(39,12)},
            new Vector2[]{new Vector2(39,12),new Vector2(44,12),new Vector2(50,12),Objective}
        };

        [MenuItem("Tools/Stage 3/Invasion Road/Apply Accepted Curved Road")]
        public static void ApplyAcceptedRoute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before changing the authored map.");
            var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>(Stage3RapidPilotBuilder.TacticalMapPath);
            if (map == null) throw new InvalidOperationException("Stage 3 map is missing.");
            Build(map);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_INVASION_ROUTE_APPLIED: seven-wide curved road from player sketch; lower-left district.");
        }

        internal static void Build(Stage2MapDefinition map)
        {
            var bounds=new MapCellArea(new Vector2Int(-50,-50),new Vector2Int(250,250));
            var corridor=new List<MapSurfaceRegion>();
            var buildAreas=new List<MapCellArea>();
            var road=RasterizeRoad();
            // Bake the curved stroke into the existing cell data. Runtime terrain,
            // building and enemy navigation share the same disjoint row spans.
            for(var y=bounds.Minimum.y;y<=bounds.MaximumInclusive.y;y++)
            {
                for(var x=bounds.Minimum.x;x<=bounds.MaximumInclusive.x;x++)
                {
                    if(!road.Contains(new Vector2Int(x,y)))continue;
                    var first=x;
                    while(x<bounds.MaximumInclusive.x && road.Contains(new Vector2Int(x+1,y)))x++;
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

        private static HashSet<Vector2Int> RasterizeRoad()
        {
            const float radius=3.5f;
            var road=new HashSet<Vector2Int>();
            foreach(var curve in Curves)
            {
                var length=Vector2.Distance(curve[0],curve[1])+Vector2.Distance(curve[1],curve[2])+
                    Vector2.Distance(curve[2],curve[3]);
                var steps=Mathf.Max(1,Mathf.CeilToInt(length*8f));
                for(var sample=0;sample<=steps;sample++)
                {
                    var t=sample/(float)steps;
                    var u=1f-t;
                    var point=u*u*u*curve[0]+3f*u*u*t*curve[1]+3f*u*t*t*curve[2]+t*t*t*curve[3];
                    for(var x=Mathf.CeilToInt(point.x-radius);x<=Mathf.FloorToInt(point.x+radius);x++)
                    for(var y=Mathf.CeilToInt(point.y-radius);y<=Mathf.FloorToInt(point.y+radius);y++)
                        if((new Vector2(x,y)-point).sqrMagnitude<=radius*radius)
                            road.Add(new Vector2Int(x,y));
                }
            }
            return road;
        }

        private static MapResourceRegion Resource(MapResourceKind kind,Vector2Int anchor)
        {
            return new MapResourceRegion(kind,
                new MapCellArea(anchor-new Vector2Int(3,3),new Vector2Int(7,7)),anchor);
        }
    }
}
