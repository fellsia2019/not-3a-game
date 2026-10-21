using System.Collections.Generic;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage3WorldTravel teleportTravel;
        private Stage3TeleportsView teleportView;
        private string teleportSource,teleportSelection;
        private Stage3CityArt teleportArt;
        private void BindTeleports()
        {
            teleportTravel=GetComponent<Stage3WorldTravel>();if(teleportTravel==null)return;
            teleportArt=Resources.Load<Stage3CityArt>("Stage3CityArt");
            teleportTravel.TeleportMenuRequested+=OpenTeleports;
            teleportTravel.OpeningsChanged+=CloseTeleports;
            presentation.TeleportDestinationSelected+=SelectTeleport;
            presentation.TeleportRequested+=ConfirmTeleport;
            presentation.TeleportClosed+=OnTeleportsClosed;
        }
        private void UnbindTeleports()
        {
            if(teleportTravel!=null){teleportTravel.TeleportMenuRequested-=OpenTeleports;teleportTravel.OpeningsChanged-=CloseTeleports;}
            if(presentation!=null){presentation.TeleportDestinationSelected-=SelectTeleport;presentation.TeleportRequested-=ConfirmTeleport;presentation.TeleportClosed-=OnTeleportsClosed;}
        }
        private void OpenTeleports(string source)
        {
            ClearBuildingSelection();CancelBlink();CancelPlacement();teleportSource=source;teleportSelection=source;
            var rows=new List<Stage3TeleportDestinationView>();
            foreach(var point in teleportTravel.TravelState.Points)
            {
                if(!teleportTravel.TravelState.IsOpened(point.Id))continue;
                rows.Add(new Stage3TeleportDestinationView{Id=point.Id,Title=point.Title,
                    Medallion=point.Id==Stage3TravelState.CityId?teleportArt?.cityArrivalPreview:teleportArt?.baseArrivalPreview,
                    Preview=point.Id==Stage3TravelState.CityId?teleportArt?.cityArrivalPreview:teleportArt?.baseArrivalPreview});
                if(teleportSelection==source&&point.Id!=source)teleportSelection=point.Id;
            }
            teleportView=new Stage3TeleportsView{CurrentId=source,SelectedId=teleportSelection,Destinations=rows.ToArray()};
            presentation.SetTeleports(teleportView);presentation.ShowTeleports(true);UpdateTeleports();
        }
        private void SelectTeleport(string id){teleportSelection=id;UpdateTeleports();}
        private void ConfirmTeleport(string id)
        {
            if(!presentation.TeleportsOpen||id!=teleportSelection)return;
            if(teleportTravel.TryTravelTo(teleportSource,id,true))presentation.ShowTeleports(false);
            else UpdateTeleports();
        }
        private void OnTeleportsClosed(){teleportTravel?.RequireFreshTravelInput();teleportSource=teleportSelection=null;teleportView=null;}
        private void CloseTeleports(){if(presentation!=null)presentation.ShowTeleports(false);OnTeleportsClosed();}
        private void UpdateTeleports()
        {
            if(teleportTravel==null||!presentation.TeleportsOpen||teleportView==null)return;
            if(game.RunDriver.Run.IsPaused||game.RunDriver.Run.IsTerminal||(legacy?.DebugOpen??false)||teleportTravel.NearbyOpenedPoint()!=teleportSource)
            {CloseTeleports();return;}
            teleportView.CanTravel=teleportTravel.CanTravelTo(teleportSource,teleportSelection,true);
            teleportView.UnavailableReason=teleportTravel.LastTravelFailure;
        }
    }
}
