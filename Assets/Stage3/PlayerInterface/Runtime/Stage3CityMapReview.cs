using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Disposable native navigation checks; no grants, discovery or view-data injection.</summary>
    public static class Stage3CityMapReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);for(var i=0;i<16;i++)yield return null;
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var travel=owner.GetComponent<Stage3WorldTravel>();
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();var map=owner.GetComponent<Stage2Minimap>();
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;hero.AutomaticTick=false;
            var guild=owner.GetComponent<Stage3GuildWorldDriver>();if(guild!=null)guild.AutomaticTick=false;
            game.RunDriver.ResetRun();game.SetPaused(false);yield return null;
            var start=(Vector2)scene.PlayerMover.transform.position;var city=travel.CityOrigin;var frames=new List<string>();
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
            Require(!travel.RemoteDiscovered&&!travel.IsResearched,"unopened starting state");
            foreach(var size in sizes)
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.25f);
                var prefix=size.x+"x"+size.y;
                hud.CloseWindow();Move(scene,Clear(hero,start));
                yield return Shot(directory,prefix+"-start-minimap",frames);
                Require(hud.CityMinimapMarkerRect==Rect.zero&&hud.IsCityMinimapOffscreen,"far city has no minimap direction indicator");
                Require(!travel.RemoteDiscovered&&!travel.IsResearched,"drawing must not unlock travel");
                hud.SetWindow(Stage3PlayerWindow.Map);yield return Shot(directory,prefix+"-M-overview",frames);
                Require(hud.MapZoom==1&&hud.CityLargeMapMarkerRect.width>0,"first overview includes city");
                Require(hud.MapViewportRect.Contains(hud.CityLargeMapMarkerRect.min)&&hud.MapViewportRect.Contains(hud.CityLargeMapMarkerRect.max),"map marker safe");
                Require(hud.CityLargeMapLabelRect==Rect.zero,"city marker uses icon only");
                hud.ZoomMap(3);hud.PanMap(new Vector2(-140,75));var center=hud.MapCenter;var zoom=hud.MapZoom;
                hud.CloseWindow();hud.SetWindow(Stage3PlayerWindow.Map);yield return Shot(directory,prefix+"-M-reopened",frames);
                Require(hud.MapZoom==zoom&&Vector2.Distance(hud.MapCenter,center)<.001f,"map navigation preserved");
                hud.ZoomMap(1/hud.MapZoom);hud.CloseWindow();
                Move(scene,Clear(hero,travel.CityPresentation.GuildInteractionPosition));
                yield return Shot(directory,prefix+"-near-city-minimap",frames);
                Require(hud.CityMinimapMarkerRect.width>0&&!hud.IsCityMinimapOffscreen,"local city house visible");
                Move(scene,Clear(hero,city));hud.SetWindow(Stage3PlayerWindow.Map);
                yield return Shot(directory,prefix+"-M-city-hero-overlap",frames);hud.CloseWindow();
                var bearings=new[]{Vector2.up,Vector2.right,Vector2.down,Vector2.left};
                foreach(var direction in bearings)
                {
                    Move(scene,BearingFooting(hero,city,direction));yield return Shot(directory,prefix+"-bearing-"+direction,frames);
                    if(hud.IsCityMinimapOffscreen)
                    { Require(hud.CityMinimapMarkerRect==Rect.zero,"no offscreen city pointer");continue; }
                    Require(hud.CityMinimapMarkerRect.width>0,"local city marker remains visible");
                    var margin=hud.UiScale*5;var inset=new Rect(hud.MinimapRect.x+margin,hud.MinimapRect.y+margin,hud.MinimapRect.width-2*margin,hud.MinimapRect.height-2*margin);
                    Require(inset.Contains(hud.CityMinimapMarkerRect.min)&&inset.Contains(hud.CityMinimapMarkerRect.max),"rim badge inside frame");
                }
            }
            typeof(Stage3WorldTravel).GetMethod("ClearCity",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(travel,null);
            yield return Shot(directory,"clear-city-removes-marker",frames);
            Require(!map.Markers.Any(m=>m.Kind==Stage2MinimapMarkerKind.City)&&hud.CityMinimapMarkerRect==Rect.zero,"clear removes data and presentation");
            File.WriteAllText(Path.Combine(directory,"frames.txt"),string.Join("\n",frames));
            File.WriteAllText(Path.Combine(directory,"qualification.txt"),"Actual Mac development-player native UI. No grants/discovery/DTO injection. Fixture repositions hero and holds automatic clock/combat only for capture. Initial city visible before research/discovery; data clear and preserved zoom/pan asserted. Ordinary restart restores all runtime flags.\n");
            Debug.Log("S3_CITY_MAP_NATIVE_PASS "+directory);
        }
        private static Vector2 Clear(Stage2HeroProgressionDriver hero,Vector2 point)
        {
            if(hero.CanLand(point))return point;
            for(var ring=.5f;ring<14;ring+=.5f)for(var d=0;d<32;d++)
            {var a=d*Mathf.PI/16;var p=point+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*ring;if(hero.CanLand(p))return p;}
            throw new InvalidOperationException("No map review footing near "+point);
        }
        private static Vector2 BearingFooting(Stage2HeroProgressionDriver hero,Vector2 city,Vector2 direction)
        {
            // The real map spans -50..199. At the eastern edge only a local west-facing check is reachable.
            var side=new Vector2(-direction.y,direction.x);
            for(var distance=70;distance>=12;distance--)
            for(var offset=0f;offset<=10;offset+=.5f)
            foreach(var sign in new[]{1,-1})
            {
                var p=city-direction*distance+side*(offset*sign);
                if(hero.CanLand(p))return p;
            }
            throw new InvalidOperationException("No legal city-bearing review footing: "+direction);
        }
        private static void Move(Stage2SceneBootstrap scene,Vector2 p)
        {Require(scene.PlayerMover.TryTeleportTo(p),"legal fixture footing");scene.CameraFollow.SnapToTarget();}
        private static void Require(bool ok,string label){if(!ok)throw new InvalidOperationException("City map QA: "+label);}
        private static IEnumerator Shot(string dir,string name,List<string> frames)
        {for(var i=0;i<4;i++)yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));frames.Add(name);yield return null;}
    }
}
