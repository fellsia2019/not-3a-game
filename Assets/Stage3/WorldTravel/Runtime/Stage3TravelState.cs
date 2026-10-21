using System;
using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public readonly struct Stage3TravelPoint
    {
        public readonly string Id, Title;
        public readonly Vector2 Position;
        public Stage3TravelPoint(string id, string title, Vector2 position)
        { Id=id; Title=title; Position=position; }
    }

    /// <summary>World-owned openings; the UI cannot create destinations or grant discovery.</summary>
    public sealed class Stage3TravelState
    {
        public const string BaseId="base", CityId="city";
        private readonly List<Stage3TravelPoint> points=new List<Stage3TravelPoint>();
        private readonly HashSet<string> opened=new HashSet<string>(StringComparer.Ordinal);
        private Vector2 configuredBasePosition;
        public IReadOnlyList<Stage3TravelPoint> Points => points;
        public Stage3TravelState()
        { Configure(new[]{new Stage3TravelPoint(BaseId,"База",Vector2.zero),new Stage3TravelPoint(CityId,"Город",Vector2.zero)}); }
        public void Configure(IEnumerable<Stage3TravelPoint> definitions)
        {
            var validated=new List<Stage3TravelPoint>(); var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var point in definitions)
            {
                if(string.IsNullOrWhiteSpace(point.Id)||!ids.Add(point.Id)||string.IsNullOrWhiteSpace(point.Title)||
                    float.IsNaN(point.Position.x)||float.IsNaN(point.Position.y)||float.IsInfinity(point.Position.x)||float.IsInfinity(point.Position.y))
                    throw new ArgumentException("Teleport points require unique IDs, titles and finite positions.");
                validated.Add(point);
            }
            points.Clear();points.AddRange(validated);
            configuredBasePosition=TryGet(BaseId,out var home)?home.Position:Vector2.zero;Reset();
        }
        public bool TryGet(string id,out Stage3TravelPoint point)
        { foreach(var candidate in points)if(candidate.Id==id){point=candidate;return true;} point=default;return false; }
        public bool IsOpened(string id) => id!=null&&opened.Contains(id);
        public bool Discover(string id)
        { return id!=BaseId&&TryGet(id,out _)&&opened.Add(id); }
        public void Discover()=>Discover(CityId);
        public bool CanRoute(string from,string to)=>Researched&&from!=to&&IsOpened(from)&&IsOpened(to)&&TryGet(from,out _)&&TryGet(to,out _);
        public readonly struct Snapshot
        {
            internal readonly bool Built, Studied, CircleBuilt;
            internal readonly Vector2 BasePosition;
            internal readonly string[] Opened;
            internal Snapshot(Stage3TravelState state)
            { Built=state.WorkshopBuilt; Studied=state.Researched; CircleBuilt=state.BaseBuilt;
                BasePosition=state.TryGet(BaseId,out var home)?home.Position:Vector2.zero;
                Opened=new string[state.opened.Count];state.opened.CopyTo(Opened); }
        }
        public Snapshot CaptureSnapshot()=>new Snapshot(this);
        public void RestoreSnapshot(Snapshot snapshot)
        {
            WorkshopBuilt=snapshot.Built;Researched=snapshot.Studied;BaseBuilt=snapshot.CircleBuilt;
            SetBasePosition(snapshot.BasePosition);opened.Clear();
            if(snapshot.Opened!=null)foreach(var id in snapshot.Opened)if(TryGet(id,out _))opened.Add(id);
        }
        public bool WorkshopBuilt {get;private set;}
        public bool Researched {get;private set;}
        public bool BaseBuilt {get;private set;}
        public bool RemoteDiscovered=>IsOpened(CityId);
        public bool CanTravel=>Researched&&opened.Count>1;
        public bool BuildWorkshop(){if(WorkshopBuilt)return false;WorkshopBuilt=true;return true;}
        public bool Study(){if(!WorkshopBuilt||Researched)return false;Researched=true;return true;}
        public bool BuildBaseCircle(Vector2 position)
        {
            if(!Researched||BaseBuilt||!TryGet(BaseId,out _)||!Finite(position.x)||!Finite(position.y))return false;
            SetBasePosition(position);BaseBuilt=true;opened.Add(BaseId);return true;
        }
        private void SetBasePosition(Vector2 position)
        {
            for(var i=0;i<points.Count;i++)if(points[i].Id==BaseId)
            {points[i]=new Stage3TravelPoint(BaseId,points[i].Title,position);return;}
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        public void Reset(){WorkshopBuilt=false;Researched=false;BaseBuilt=false;opened.Clear();SetBasePosition(configuredBasePosition);}
    }
}
