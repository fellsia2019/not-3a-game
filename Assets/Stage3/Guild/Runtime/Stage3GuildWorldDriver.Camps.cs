using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed partial class Stage3GuildWorldDriver
    {
        private readonly Rect[] campBounds=new Rect[2];
        private readonly List<Rect> campSolids=new List<Rect>();
        private readonly Stage3CampPresentation[] campPresentations=new Stage3CampPresentation[2];
        private HeroObstacleMap campGround;
        private readonly Dictionary<Stage2ResourceNode,(int kind,int layout)> campResources=new();
        public IEnumerable<Stage2ResourceNode> HarvestableCampResources=>campResources.Keys;
        public bool IsCampResource(Stage2ResourceNode node)=>campResources.ContainsKey(node);
        public IReadOnlyList<Stage2ResourceNode> CreateCampResources(Stage2EconomyCatalog catalog)
        {
            campResources.Clear();var result=new List<Stage2ResourceNode>();
            if(!Ready||catalog==null)return result;
            for(var kind=0;kind<2;kind++)
            {
                var layout=Stage3CampPresentation.GetLayout(kind);
                for(var index=0;index<layout.Count;index++)
                {
                    var prop=layout[index];if(prop.SpriteIndex<100||prop.SpriteIndex>=300)continue;
                    var resource=prop.SpriteIndex<200?MapResourceKind.Wood:MapResourceKind.Stone;
                    var point=spots[kind]+prop.Offset;
                    var radius=Mathf.Max(prop.Footprint.x,prop.Footprint.y)*.5f;
                    var node=new Stage2ResourceNode(catalog.Get(resource),scene.MapDefinition.WorldToCell(point),point,
                        Stage2ResourceSize.Small,radius,10000+kind*100+index,prop.SpriteIndex);
                    result.Add(node);campResources.Add(node,(kind,index));
                }
            }
            return result;
        }
        public void BindCampResources(Stage3ResourceInteractionMarker marker)
        {
            foreach(var pair in campResources)
                campPresentations[pair.Value.kind].BindResource(pair.Value.layout,pair.Key,marker);
        }
        public IReadOnlyList<Rect> ReservedCampBounds=>campBounds;
        public Stage3CampPresentation CampPresentation(int kind)=>campPresentations[Mathf.Clamp(kind,0,1)];
        public bool OverlapsCamp(Rect area)
        {
            if(!Ready)return false;
            foreach(var bounds in campBounds)if(bounds.Overlaps(area))return true;
            return false;
        }

        // Called before manual-resource generation so the whole encounter, rather than
        // just today's enemy spawn, remains clear on every generated world.
        public bool PrepareCampEnvironment(Stage2SceneBootstrap context,Stage3ResourceDistrictWorld resources,Stage3WorldTravel cityTravel)
        {
            if(context==null||resources?.Obstacles==null||cityTravel==null||!cityTravel.CityReady)return false;
            if(Ready&&ReferenceEquals(campGround,resources.Obstacles))return true;
            var previous=(Vector2[])spots.Clone();
            ClearCampEnvironment();scene=context;world=resources;travel=cityTravel;ground=resources.Obstacles;
            var chosen=new List<Rect>();
            for(var kind=0;kind<2;kind++)
            {
                var found=false;
                for(var ring=0;ring<40&&!found;ring++)for(var d=0;d<48;d++)
                {
                    var angle=(d+kind*24)*Mathf.PI/24;
                    var point=travel.CityOrigin+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(48+ring*4);
                    point=new Vector2(Mathf.Round(point.x),Mathf.Round(point.y));
                    var bounds=new Rect(point+Stage3CampPresentation.LocalBounds.position,Stage3CampPresentation.LocalBounds.size);
                    if(!ClearRegion(bounds,chosen))continue;
                    spots[kind]=point;campBounds[kind]=bounds;chosen.Add(bounds);found=true;break;
                }
                if(!found)throw new InvalidOperationException("No open forest territory for exploration camp "+kind);
            }
            var art=Resources.Load<Stage3CampArt>("Stage3CampArt");
            if(art==null||!art.IsReady)throw new InvalidOperationException("Import the approved modular Stage3CampArt before playing this slice.");
            campGround=ground;
            for(var kind=0;kind<2;kind++)
            {
                var root=new GameObject(kind==0?"Orc open camp":"Ogre open ruins");root.transform.SetParent(transform,false);
                var presentation=root.AddComponent<Stage3CampPresentation>();
                presentation.Build(spots[kind],scene.CameraFollow.BoundCamera,art,kind);campPresentations[kind]=presentation;
                foreach(var prop in Stage3CampPresentation.GetLayout(kind))
                {
                    // Wild trees/rocks belong to the ordinary depletion-aware resource
                    // collision map; only built camp structures are permanent obstacles.
                    if(prop.SpriteIndex>=100&&prop.SpriteIndex<300)continue;
                    var local=prop.GroundRect;if(local.width<=0||local.height<=0)continue;
                    var solid=new Rect(spots[kind]+local.position,local.size);campSolids.Add(solid);campGround.AddRectangle(solid);
                }
            }
            // Regeneration replaces terrain only: preserve kills, IDs, health and the
            // contract generation while relocating surviving actors to their new field.
            shots.Clear();
            foreach(var actor in actors)
            {
                var kind=(int)actor.Rules.Profile.enemyKind;
                actor.Rules.RelocateHome(spots[kind]+actor.Rules.Home-previous[kind]);
                if(actor.Visual!=null)actor.Visual.transform.position=actor.Rules.Position;
            }
            Ready=true;
            Debug.Log($"S3_CAMP_ENVIRONMENTS_READY orcs={spots[0]} ogres={spots[1]} solids={campSolids.Count}");
            return true;
        }

        private bool ClearRegion(Rect area,List<Rect> chosen)
        {
            var padded=new Rect(area.position-Vector2.one*3,area.size+Vector2.one*6);
            var city=travel.CityBounds;
            var cityApproach=new Rect(city.position-Vector2.one*20f,city.size+Vector2.one*40f);
            if(cityApproach.Overlaps(padded))return false;
            foreach(var other in chosen)
                if(new Rect(other.position-Vector2.one*6,other.size+Vector2.one*12).Overlaps(padded))return false;
            if(world.Districts!=null)foreach(var district in world.Districts)
            {
                var b=district.Bounds;
                if(padded.Overlaps(new Rect(b.xMin-3,b.yMin-3,b.width+6,b.height+6)))return false;
            }
            foreach(var site in travel.SitePositions)if(padded.Contains(site))return false;
            var composition=game??GetComponent<Stage2GameComposition>();
            if(composition!=null)for(var kind=0;kind<3;kind++)
                if(padded.Contains(composition.Gathering.Economy.WorldPosition((MapResourceKind)kind)))return false;
            var map=scene.MapDefinition;
            if(!map.Contains(map.WorldToCell(padded.min))||!map.Contains(map.WorldToCell(padded.max)))return false;
            foreach(var solid in ground.Rectangles)if(padded.Overlaps(solid))return false;
            // Check terrain plus existing structural obstacles at sub-cell intervals.
            // Manual circles were unbound by the caller and will be generated afterwards.
            for(var y=padded.yMin;y<=padded.yMax;y+=1f)for(var x=padded.xMin;x<=padded.xMax;x+=1f)
            {
                var p=new Vector2(x,y);var cell=scene.MapDefinition.WorldToCell(p);
                if(!scene.MapDefinition.Contains(cell)||!scene.MapDefinition.IsWalkable(cell)||
                    scene.MapDefinition.GetSurface(cell)!=MapSurfaceKind.Forest||ground.IsBlocked(p,.8f))return false;
            }
            return true;
        }
        private void ClearCampEnvironment()
        {
            if(campGround!=null)foreach(var solid in campSolids)campGround.RemoveRectangle(solid);
            campSolids.Clear();campGround=null;Ready=false;
            campResources.Clear();
            for(var kind=0;kind<campPresentations.Length;kind++)
            {
                var presentation=campPresentations[kind];
                if(presentation!=null){presentation.Clear();Destroy(presentation.gameObject);}
                campPresentations[kind]=null;campBounds[kind]=Rect.zero;
            }
        }
    }
}
