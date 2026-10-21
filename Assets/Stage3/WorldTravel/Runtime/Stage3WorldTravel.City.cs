using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed partial class Stage3WorldTravel
    {
        private Stage3CityPresentation city;
        private readonly List<Rect> citySolids=new List<Rect>();
        private HeroObstacleMap cityGround;
        public bool CityReady=>city!=null&&city.IsBuilt;
        public Vector2 CityOrigin {get;private set;}
        public Rect CityBounds=>CityReady?new Rect(CityOrigin+Stage3CityPresentation.LocalBounds.position,Stage3CityPresentation.LocalBounds.size):Rect.zero;
        public Stage3CityPresentation CityPresentation=>city;
        public bool OverlapsCity(Rect area)=>CityReady&&CityBounds.Overlaps(area);
        private void BindCity(Stage3ResourceDistrictWorld world)
        {
            ClearCity();var art=Resources.Load<Stage3CityArt>("Stage3CityArt");
            if(art==null||!art.IsReady)return;
            var found=false;
            for(var radius=0;radius<=75&&!found;radius+=3)
            for(var y=-radius;y<=radius&&!found;y+=3)
            for(var x=-radius;x<=radius;x+=3)
            {
                if(radius>0&&Mathf.Max(Mathf.Abs(x),Mathf.Abs(y))!=radius)continue;
                var origin=data.cityOrigin+new Vector2(x,y);var local=Stage3CityPresentation.LocalBounds;
                var bounds=new Rect(origin+local.position,local.size);var valid=true;
                for(var sy=bounds.yMin;sy<=bounds.yMax&&valid;sy+=2f)
                for(var sx=bounds.xMin;sx<=bounds.xMax;sx+=2f)
                {
                    var point=new Vector2(sx,sy);var cell=scene.MapDefinition.WorldToCell(point);
                    if(!scene.MapDefinition.Contains(cell)||!scene.MapDefinition.IsWalkable(cell)||
                        scene.MapDefinition.GetSurface(cell)!=MapSurfaceKind.Forest||obstacles.IsBlocked(point,1f)){valid=false;break;}
                }
                if(world?.Districts!=null)foreach(var district in world.Districts)
                {var b=district.Bounds;if(bounds.Overlaps(new Rect(b.xMin-3,b.yMin-3,b.width+6,b.height+6))){valid=false;break;}}
                if(!valid)continue;
                CityOrigin=origin;found=true;break;
            }
            if(!found)throw new InvalidOperationException("No clear forest district for the city.");
            city=GetComponent<Stage3CityPresentation>();if(city==null)city=gameObject.AddComponent<Stage3CityPresentation>();
            city.Build(CityOrigin,scene.CameraFollow.BoundCamera,art);cityGround=obstacles;
            foreach(var local in Stage3CityPresentation.LocalFootprints)
            {var solid=new Rect(CityOrigin+local.position,local.size);citySolids.Add(solid);obstacles.AddRectangle(solid);}
            Debug.Log($"S3_CITY_READY origin={CityOrigin} portal={city.PortalPosition} merchant={city.MerchantPosition} solids={citySolids.Count}");
        }
        private void ClearCity()
        {
            minimap?.SetCityMarker(null);
            if(cityGround!=null)foreach(var solid in citySolids)cityGround.RemoveRectangle(solid);
            citySolids.Clear();cityGround=null;city?.Clear();
        }
    }
}
