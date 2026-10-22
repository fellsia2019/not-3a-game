using System.Collections;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in scene binding; the UI observes rules and gates player input without pausing the world.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-300)]
    public sealed partial class Stage3PlayerInterfaceBinding : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud legacy;
        private Stage2Minimap minimap;
        private Stage3FantasyHud presentation;
        private Stage3DayNightPresentation lighting;
        private Stage3ResourceDistrictWorld world;
        private HeroObstacleMap observedGround;
        private bool bound;
        private int displayedClockMinute=int.MinValue;
        private string clockLabel;

        private IEnumerator Start()
        {
            game = GetComponent<Stage2GameComposition>();
            scene = GetComponent<Stage2SceneBootstrap>();
            presentation = GetComponent<Stage3FantasyHud>();
            while (game != null && !game.IsInitialized) yield return null;
            if (game == null || scene == null || presentation == null) yield break;
            legacy = GetComponent<Stage2GameHud>();
            minimap = GetComponent<Stage2Minimap>();
            presentation.Initialize(game, scene, legacy);
            presentation.ClockText = DisplayClock;
            presentation.CycleProgress = () => game.RunDriver.Run.DayNightCycleProgress;
            presentation.IsNightPhase = () => game.RunDriver.Run.IsNightTime;
            presentation.CancelPlacementRequested += CancelPlacement;
            legacy.BindPlayerPresentation(presentation.IsPointerOverUi,
                () => ToLegacyRect(presentation.ResourcesRect), () => ToLegacyRect(presentation.BottomRect),
                () => ToLegacyRect(presentation.StatusRect), () => ToLegacyRect(presentation.DevelopmentAccessRect));
            if (minimap != null) minimap.SuppressPresentation = true;
            game.BindPlayerInputBlocker(Blocked);
            scene.PlayerMover.SetInputBlocker(() => Blocked() || IsDraggingBuilding);
            game.Gathering.SetInputBlocker(() => Blocked() || IsDraggingBuilding || blinkArmed || armedSkill>=0 || (heroSkills?.IsAiming??false) || heroRecovery?.NearbyDrop!=null);
            game.Building.SetInputBlocker(Blocked);
            scene.CameraFollow.SetPointerBlocker(point => IsDraggingBuilding || game.ContainsScreenPoint(point));
            scene.CameraFollow.SetOverlayTrackpadInput(presentation.AcceptsMapTrackpad, presentation.ApplyMapTrackpadGesture);
            game.Building.UseBuildShortcut = false;
            game.Building.PlacementStarting += OpenDefense;
            game.RunDriver.Run.Rewound += AfterRetry;
            if (game.Production != null) game.Production.PlacementStarting += OpenProduction;
            world = GetComponent<Stage3ResourceDistrictWorld>();
            observedGround = world != null ? world.Obstacles : null;
            bound = true;
            BindBuildingSelection();
            BindHeroProgression();
            BindEquipment();
            BindBlueprintResearch();
            BindProfessions();
            BindTeleports();
            BindMerchant();
            BindHeroRecovery();
            BindGuild();
            BindHeroLoadout();
            if (game.RunDriver.Run.UsesDaytimeGates && GetComponent<Stage3DaytimeGate>() == null)
                gameObject.AddComponent<Stage3DaytimeGate>();
            Debug.Log("S3_PLAYER_INTERFACE_READY: approved lunar porcelain HUD; nonmodal construction and independent windows.");
        }

        private bool Blocked() => game != null && (game.RunDriver.Run.IsTerminal || game.RunDriver.Run.IsPaused) ||
            presentation != null && presentation.isActiveAndEnabled && presentation.IsModalOpen;

        private string DisplayClock()
        {
            var run=game.RunDriver.Run;var minute=run.ClockHour*60+run.ClockMinute;
            if(minute!=displayedClockMinute)
            {displayedClockMinute=minute;clockLabel=$"{run.ClockHour:00}:{run.ClockMinute:00}";}
            return clockLabel;
        }

        private Rect ToLegacyRect(Rect pixels) => new Rect(pixels.position / legacy.Scale, pixels.size / legacy.Scale);

        private void Update()
        {
            if (!bound) return;
            if (minimap == null)
            {
                minimap = GetComponent<Stage2Minimap>();
                if (minimap != null) minimap.SuppressPresentation = true;
            }
            if (lighting == null)
            {
                lighting = GetComponent<Stage3DayNightPresentation>();
                if (lighting != null) lighting.NightCondition = () => game.RunDriver.Run.IsNightTime;
            }
            if (world == null) world = GetComponent<Stage3ResourceDistrictWorld>();
            if (world != null && !ReferenceEquals(observedGround, world.Obstacles))
            {
                observedGround = world.Obstacles;
                presentation.CloseWindow();
                ClearBuildingSelection();
            }
            UpdateHeroProgression();
            UpdateHeroLoadout();
            UpdateEquipment();
            UpdateBlueprintResearch();
            UpdateProfessions();
            UpdateTeleports();
            UpdateMerchant();
            UpdateBuildingSelection();
            UpdateBuildingDragPresentation();
            UpdateHeroRecovery();
            UpdateGuild();
        }

        private void OpenDefense() => OpenConstruction(0);
        private void AfterRetry()
        {
            presentation.CloseWindow(); CancelPlacement();
            scene.CameraFollow.SnapToTarget();
            GetComponent<Stage3ManualResourceCollisions>()?.RefreshNow();
        }
        private void OpenProduction() => OpenConstruction(1);
        private void OpenConstruction(int group)
        {
            ClearBuildingSelection();
            presentation.SetWindow(Stage3PlayerWindow.Construction);
            presentation.SetBuildGroup(group);
        }

        private void CancelPlacement()
        {
            game.Building.CancelPlacement();
            game.Production?.CancelPlacement();
            GetComponent<Stage3WorldTravel>()?.CancelConstructionPlacement();
            GetComponent<Stage3Dwelling>()?.CancelPlacement();
            GetComponent<Stage3Forge>()?.CancelPlacement();
            game.Gathering.CancelGatherUntilRelease();
        }

        private void OnEnable() { if (bound) BeginBuildingPointerCapture(); }
        private void OnDisable() { EndBuildingPointerCapture(); ClearBuildingSelection(); CancelBlink();CancelHeroSkill(); }

        private void OnDestroy()
        {
            if (!bound) return;
            UnbindGuild();
            UnbindHeroLoadout();
            UnbindHeroProgression();
            UnbindHeroRecovery();
            UnbindProfessions();
            UnbindTeleports();
            UnbindMerchant();
            UnbindBlueprintResearch();
            UnbindEquipment();
            UnbindBuildingSelection();
            if (presentation != null) presentation.CancelPlacementRequested -= CancelPlacement;
            if (lighting != null) lighting.NightCondition = null;
            if (legacy != null) legacy.BindPlayerPresentation(null);
            if (minimap != null) minimap.SuppressPresentation = false;
            if (scene != null && scene.PlayerMover != null) scene.PlayerMover.SetInputBlocker(null);
            if (scene != null && scene.CameraFollow != null) scene.CameraFollow.SetOverlayTrackpadInput(null, null);
            if (scene != null && scene.CameraFollow != null && game != null)
                scene.CameraFollow.SetPointerBlocker(game.ContainsScreenPoint);
            if (game != null)
            {
                game.RunDriver.Run.Rewound -= AfterRetry;
                game.BindPlayerInputBlocker(null);
                game.Gathering?.SetInputBlocker(null);
                if (game.Building != null)
                {
                    game.Building.PlacementStarting -= OpenDefense;
                    game.Building.SetInputBlocker(null); game.Building.UseBuildShortcut = true;
                }
                if (game.Production != null) game.Production.PlacementStarting -= OpenProduction;
            }
        }
    }
}
