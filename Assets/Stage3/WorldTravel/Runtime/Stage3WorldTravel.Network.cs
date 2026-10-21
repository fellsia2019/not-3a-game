using System;
using System.Collections.Generic;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.VisualPilot
{
    [Serializable] public sealed class Stage3ExtraTravelPoint
    { public string id,title; public Vector2 position; }

    public sealed partial class Stage3WorldTravel
    {
        private readonly List<GameObject> extraCircles=new List<GameObject>();
        private readonly List<Mesh> extraMeshes=new List<Mesh>();
        private void ClearExtraCircles()
        {foreach(var item in extraCircles)if(item!=null)Destroy(item);foreach(var mesh in extraMeshes)if(mesh!=null)Destroy(mesh);extraCircles.Clear();extraMeshes.Clear();}
        private bool pointerRequiresRelease=true;
        public event Action<string> TeleportMenuRequested;
        public event Action OpeningsChanged;
        public Stage3TravelState TravelState=>state;
        public string LastTravelFailure {get;private set;}
        public void RequireFreshTravelInput(){requiresRelease=true;pointerRequiresRelease=true;}
        private bool TravelModeAllowed(bool menu)=>ready&&game!=null&&!game.RunDriver.Run.IsPaused&&
            !game.RunDriver.Run.IsTerminal&&Time.timeScale>0f&&!IsPlacingWorkshop&&!IsPlacingBaseCircle&&!game.Building.Defense.IsPlacing&&
            !(game.Production?.IsPlacing??false)&&!(hud?.DebugOpen??false)&&
            (!game.PlayerInputBlocked||(menu&&(GetComponent<Stage3FantasyHud>()?.TeleportsOpen??false)));
        public string NearbyOpenedPoint()
        {
            var p=(Vector2)scene.PlayerMover.transform.position;
            foreach(var point in state.Points)if(state.IsOpened(point.Id)&&Vector2.Distance(p,point.Position)<=data.interactionRadius)return point.Id;
            return null;
        }
        public bool TryOpenTeleportMenu(string id)
        {
            if(!TravelModeAllowed(false)||!state.Researched||!state.IsOpened(id)||NearbyOpenedPoint()!=id)return false;
            game.Gathering.CancelGatherUntilRelease();RequireFreshTravelInput();TeleportMenuRequested?.Invoke(id);return true;
        }
        public bool CanTravelTo(string from,string to,bool menu=false)
        {
            LastTravelFailure=null;
            if(!TravelModeAllowed(menu)){LastTravelFailure="Сейчас переход недоступен";return false;}
            if(from==to){LastTravelFailure="Вы уже здесь";return false;}
            if(!state.CanRoute(from,to)){LastTravelFailure="Точка не открыта";return false;}
            if(NearbyOpenedPoint()!=from){LastTravelFailure="Подойдите к порталу";return false;}
            state.TryGet(to,out var target);var cell=scene.MapDefinition.WorldToCell(target.Position);
            if(!scene.MapDefinition.Contains(cell)||!scene.MapDefinition.IsWalkable(cell)||obstacles.IsBlocked(target.Position,scene.PlayerMover.CollisionRadius))
            {LastTravelFailure="Место прибытия занято";return false;}
            return true;
        }
        public bool TryTravelTo(string from,string to,bool menu=false)
        {
            if(!CanTravelTo(from,to,menu))return false;
            state.TryGet(to,out var target);game.Gathering.CancelGatherUntilRelease();RequireFreshTravelInput();
            scene.PlayerMover.transform.position=new Vector3(target.Position.x,target.Position.y,scene.PlayerMover.transform.position.z);
            scene.CameraFollow.SnapToTarget();return true;
        }
        private void UpdateNetwork()
        {
            var mouse=Mouse.current;var held=mouse!=null&&mouse.leftButton.isPressed;
            if(!held)pointerRequiresRelease=false;
            if(!InteractionAllowed)return;
            var changed=false;var hero=(Vector2)scene.PlayerMover.transform.position;
            foreach(var point in state.Points)
                if(point.Id!=Stage3TravelState.BaseId&&Vector2.Distance(hero,point.Position)<=data.discoveryRadius)changed|=state.Discover(point.Id);
            if(changed){RefreshMarkers();OpeningsChanged?.Invoke();}
            if(pointerRequiresRelease||mouse==null||!mouse.leftButton.wasPressedThisFrame||game.ContainsScreenPoint(mouse.position.ReadValue()))return;
            var ray=scene.CameraFollow.BoundCamera.ScreenPointToRay(mouse.position.ReadValue());
            if(!new Plane(Vector3.forward,Vector3.zero).Raycast(ray,out var distance))return;
            var hit=(Vector2)ray.GetPoint(distance);var id=NearbyOpenedPoint();
            if(id!=null&&state.TryGet(id,out var current)&&Vector2.Distance(hit,current.Position)<=data.circleDiameter*.5f)TryOpenTeleportMenu(id);
        }
    }
}
