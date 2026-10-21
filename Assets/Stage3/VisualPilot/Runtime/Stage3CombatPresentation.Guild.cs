using System.Collections.Generic;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed partial class Stage3CombatPresentation
    {
        private sealed class GuildFlight
        {
            public Transform Body;
            public LineRenderer Trail;
            public Vector3 Start,End;
            public float Duration,Elapsed;
            public int LaunchedFrame;
        }
        private Stage3GuildWorldDriver guildWorld;
        private Stage2GameHud guildLegacyHud;
        private readonly List<GuildFlight> guildFlights=new List<GuildFlight>();
        public int ActiveGuildFlights=>guildFlights.Count;
        private void ObserveGuildShots()
        {
            if(guildWorld!=null||composition==null)return;
            guildWorld=composition.GetComponent<Stage3GuildWorldDriver>();if(guildWorld==null)return;
            guildLegacyHud=composition.GetComponent<Stage2GameHud>();
            guildWorld.ShotLaunched+=LaunchGuildShot;
            composition.RunDriver.Run.Reset+=ClearGuildShots;
        }
        private void LaunchGuildShot(Vector2 origin,Vector2 target,float duration)
        {
            if(duration<=0f||material==null||arrowMesh==null)return;
            var start=(Vector3)origin+Vector3.back*.43f;
            if(heroAvatar!=null&&Vector2.Distance(origin,heroAvatar.transform.position)<.01f)start=heroAvatar.ReleasePosition;
            var body=MeshObject("Exploration arrow in flight",arrowMesh,new Color(1f,.79f,.30f));
            body.localScale=Vector3.one*.72f;
            var flight=new GuildFlight{Body=body,Trail=Line("Exploration shot trail",new Color(1f,.86f,.4f),.035f,7),
                Start=start,End=new Vector3(target.x,target.y,Stage3TacticalPresentation.GrassSurfaceZ(target)-.35f),Duration=duration,LaunchedFrame=Time.frameCount};
            guildFlights.Add(flight);DrawGuildFlight(flight);
        }
        private static Vector3 GuildFlightPoint(GuildFlight shot,float t)=>Vector3.Lerp(shot.Start,shot.End,t)+Vector3.back*.12f*4f*t*(1f-t);
        private void DrawGuildFlight(GuildFlight flight)
        {
            var t=Mathf.Clamp01(flight.Elapsed/flight.Duration);
            flight.Body.position=GuildFlightPoint(flight,t);
            var delta=GuildFlightPoint(flight,Mathf.Min(1,t+.01f))-GuildFlightPoint(flight,Mathf.Max(0,t-.01f));
            if(delta.sqrMagnitude>.000001f)flight.Body.rotation=Quaternion.LookRotation(delta,Vector3.back);
            for(var i=0;i<7;i++)flight.Trail.SetPosition(i,GuildFlightPoint(flight,Mathf.Max(0,t-i*.025f)));
        }
        private void AdvanceGuildShots()
        {
            if(guildWorld==null||!guildWorld.AutomaticTick||composition.RunDriver.Run.IsTerminal||guildLegacyHud!=null&&guildLegacyHud.DebugOpen)return;
            for(var i=guildFlights.Count-1;i>=0;i--)
            {
                var flight=guildFlights[i];if(flight.LaunchedFrame==Time.frameCount)continue;flight.Elapsed+=Time.deltaTime;
                if(flight.Elapsed>=flight.Duration){Destroy(flight.Body.gameObject);Destroy(flight.Trail.gameObject);guildFlights.RemoveAt(i);}
                else DrawGuildFlight(flight);
            }
        }
        private void ClearGuildShots()
        {
            foreach(var flight in guildFlights){if(flight.Body!=null)Destroy(flight.Body.gameObject);if(flight.Trail!=null)Destroy(flight.Trail.gameObject);}
            guildFlights.Clear();
        }
        private void ReleaseGuildShots()
        {
            if(guildWorld!=null)guildWorld.ShotLaunched-=LaunchGuildShot;
            if(composition?.RunDriver?.Run!=null)composition.RunDriver.Run.Reset-=ClearGuildShots;
            ClearGuildShots();guildWorld=null;guildLegacyHud=null;
        }
    }
}
