using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private Stage3GuildWorldDriver guildMapSource;
        private Stage3GuildWorldDriver GuildMapSource
        {
            get
            {
                if(guildMapSource==null&&game!=null)guildMapSource=game.GetComponent<Stage3GuildWorldDriver>();
                return guildMapSource;
            }
        }
        private void DrawGuildMapMarkers()
        {
            var source=GuildMapSource;if(source==null)return;
            var enemies=source.Enemies;
            for(var i=0;i<enemies.Count;i++)
            {
                var enemy=enemies[i];if(enemy==null||!enemy.Alive)continue;
                var p=WorldToLargeMapPoint(enemy.Position)-MapViewportRect.position;
                if(p.x<0||p.y<0||p.x>MapViewportRect.width||p.y>MapViewportRect.height)continue;
                DrawWorldMapMarker(p,Stage2MinimapMarkerKind.Enemy,UiScale);
            }
        }
        private void DrawGuildMinimapMarkers(Rect inner,Rect uv)
        {
            var source=GuildMapSource;if(source==null||scene?.MapDefinition==null)return;
            var map=scene.MapDefinition;var enemies=source.Enemies;
            for(var i=0;i<enemies.Count;i++)
            {
                var enemy=enemies[i];if(enemy==null||!enemy.Alive)continue;
                var cell=map.WorldToCellCoordinates(enemy.Position)-(Vector2)map.MinimumCell+Vector2.one*.5f;
                var unit=new Vector2((cell.x/map.Size.x-uv.x)/uv.width,(cell.y/map.Size.y-uv.y)/uv.height);
                if((unit-Vector2.one*.5f).sqrMagnitude>.235f)continue;
                var p=new Vector2(inner.x+unit.x*inner.width,inner.yMax-unit.y*inner.height);
                DrawWorldMapMarker(p,Stage2MinimapMarkerKind.Enemy,UiScale*.65f);
            }
        }
    }
}
