using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    internal static class Stage3RoadsideDecoration
    {
        internal static void Build(Stage2MapDefinition map,Transform parent,Stage3RoadsideArt art,Camera camera,Vector2[] clearances)
        {
            if(art==null || camera==null) return;
            var root=new GameObject("Cartoon roadside ruins and shrubs");
            root.transform.SetParent(parent,false);
            var placed=new List<Vector2>();
            for(var x=map.MinimumCell.x-4;x<=map.MaximumCell.x+4;x++)
            for(var y=map.MinimumCell.y-4;y<=map.MaximumCell.y+4;y++)
            {
                if(placed.Count>=160) continue;
                var cell=new Vector2Int(x,y);
                if(map.Contains(cell) && map.GetSurface(cell)!=MapSurfaceKind.Forest) continue;
                var hash=unchecked((uint)(x*73856093 ^ y*19349663));
                if(hash%3!=0) continue;
                var nearest=20f;
                for(var dx=-4;dx<=4;dx++) for(var dy=-4;dy<=4;dy++)
                {
                    var other=cell+new Vector2Int(dx,dy);
                    if(map.Contains(other) && map.GetSurface(other)!=MapSurfaceKind.Forest)
                        nearest=Mathf.Min(nearest,Mathf.Sqrt(dx*dx+dy*dy));
                }
                if(nearest<1f || nearest>3.3f) continue;
                var point=new Vector2(x,y);
                var clear=true;
                foreach(var reserved in clearances) if(Vector2.Distance(point,reserved)<3.2f) clear=false;
                foreach(var previous in placed) if(Vector2.Distance(point,previous)<1.3f) clear=false;
                if(!clear) continue;
                var index=(int)((hash/7)%6);
                // Tall silhouettes stay entirely outside the buildable paving.
                if(nearest<2f) index=4;
                var item=new GameObject(index<3?"Broken roadside masonry":"Roadside shrub and flowers");
                item.transform.SetParent(root.transform,false);
                var sprite=art.sprites[index];
                var depth=sprite.pivot.y/sprite.pixelsPerUnit;
                var lift=depth*Mathf.Abs(camera.transform.up.z)/Mathf.Abs(camera.transform.forward.z)+.03f;
                item.transform.SetPositionAndRotation(new Vector3(x,y,Stage3TacticalPresentation.GrassSurfaceZ(point))-
                    camera.transform.forward*lift,camera.transform.rotation);
                var renderer=item.AddComponent<SpriteRenderer>();
                renderer.sprite=sprite; renderer.sharedMaterial=art.material;
                renderer.sortingOrder=-Mathf.RoundToInt(y*100f);
                placed.Add(point);
                if(placed.Count>=160) break;
            }
            Debug.Log($"S3_CARTOON_ROADSIDE_READY props={placed.Count} colliders=0 gameplayChanges=0");
        }
    }
}
