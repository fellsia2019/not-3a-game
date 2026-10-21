using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage2HeroRecoveryDriver heroRecovery;
        private Stage3RecoveryView recoveryView;
        private bool recoveryDirty=true;
        public Stage2HeroRecoveryDriver HeroRecovery=>heroRecovery;
        private void BindHeroRecovery()
        {
            if(heroProgression==null||equipmentDriver==null)return;
            heroRecovery=GetComponent<Stage2HeroRecoveryDriver>()??gameObject.AddComponent<Stage2HeroRecoveryDriver>();
            heroRecovery.Configure(game,scene.PlayerMover,heroProgression,equipmentDriver.Rules,()=>
            {
                var travel=GetComponent<Stage3WorldTravel>();
                return travel!=null&&travel.IsReady?(Vector2?)travel.BasePosition:null;
            });
            heroRecovery.ControlBlocked=()=>!bound||!isActiveAndEnabled||!presentation.isActiveAndEnabled||Blocked()||
                IsDraggingBuilding||HasBuildingDraft||blinkArmed||(legacy?.DebugOpen??false);
            heroRecovery.Rules.Changed+=MarkRecoveryDirty;
            heroRecovery.Respawned+=AfterHeroRespawn;
            UpdateHeroRecovery();
            Debug.Log("S3_HERO_RECOVERY_READY: exploration-only damage, base reincarnation, current-level XP penalty and bag-instance recovery.");
        }
        private void MarkRecoveryDirty()=>recoveryDirty=true;
        private void UpdateHeroRecovery()
        {
            if(heroRecovery?.Rules==null)return;
            if(recoveryDirty||recoveryView==null)
            {
                var drops=heroRecovery.Rules.Drops;
                var views=new Stage3RecoveryDropView[drops.Count];
                for(var i=0;i<views.Length;i++)views[i]=new Stage3RecoveryDropView{Id=drops[i].Id,Position=drops[i].Position,ItemCount=drops[i].ItemCount};
                recoveryView=new Stage3RecoveryView{Drops=views};recoveryDirty=false;
            }
            recoveryView.NearbyDropId=heroRecovery.NearbyDrop?.Id??-1;
            recoveryView.CanRecover=heroRecovery.CanRecover;
            presentation.SetRecovery(recoveryView);
        }
        private void AfterHeroRespawn()
        {
            presentation.CloseWindow();CancelPlacement();CancelBlink();ClearBuildingSelection();ClearEquipmentSelection();
            heroProgressionEffects?.ClearEffects();heroLevelUp?.ClearEffects();
            scene.CameraFollow.SnapToTarget();MarkRecoveryDirty();RefreshHeroProgression();UpdateHeroRecovery();
        }
        private void UnbindHeroRecovery()
        {
            if(heroRecovery==null)return;
            heroRecovery.Rules.Changed-=MarkRecoveryDirty;heroRecovery.Respawned-=AfterHeroRespawn;heroRecovery.ControlBlocked=null;
        }
    }
}
