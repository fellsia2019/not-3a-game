using System.Collections;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Opt-in scene binding; the UI observes rules and gates player input without pausing the world.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-300)]
    public sealed class Stage3PlayerInterfaceBinding : MonoBehaviour
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
            presentation.ClockText = () => $"{game.RunDriver.Run.ClockHour:00}:{game.RunDriver.Run.ClockMinute:00}";
            presentation.CancelPlacementRequested += CancelPlacement;
            legacy.BindPlayerPresentation(presentation.IsPointerOverUi,
                () => ToLegacyRect(presentation.ResourcesRect), () => ToLegacyRect(presentation.BottomRect),
                () => ToLegacyRect(presentation.StatusRect));
            if (minimap != null) minimap.SuppressPresentation = true;
            game.BindPlayerInputBlocker(Blocked);
            scene.PlayerMover.SetInputBlocker(Blocked);
            game.Gathering.SetInputBlocker(Blocked);
            game.Building.SetInputBlocker(Blocked);
            scene.CameraFollow.SetOverlayTrackpadInput(presentation.AcceptsMapTrackpad, presentation.ApplyMapTrackpadGesture);
            game.Building.UseBuildShortcut = false;
            game.Building.PlacementStarting += OpenDefense;
            if (game.Production != null) game.Production.PlacementStarting += OpenProduction;
            world = GetComponent<Stage3ResourceDistrictWorld>();
            observedGround = world != null ? world.Obstacles : null;
            bound = true;
            Debug.Log("S3_PLAYER_INTERFACE_READY: approved lunar porcelain HUD; nonmodal construction and independent windows.");
        }

        private bool Blocked() => presentation != null && presentation.isActiveAndEnabled && presentation.IsModalOpen;

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
            }
        }

        private void OpenDefense() => OpenConstruction(0);
        private void OpenProduction() => OpenConstruction(1);
        private void OpenConstruction(int group)
        {
            presentation.SetWindow(Stage3PlayerWindow.Construction);
            presentation.SetBuildGroup(group);
        }

        private void CancelPlacement()
        {
            game.Building.CancelPlacement();
            game.Production?.CancelPlacement();
            GetComponent<Stage3WorldTravel>()?.CancelWorkshopPlacement();
            GetComponent<Stage3Dwelling>()?.CancelPlacement();
            game.Gathering.CancelGatherUntilRelease();
        }

        private void OnDestroy()
        {
            if (!bound) return;
            if (presentation != null) presentation.CancelPlacementRequested -= CancelPlacement;
            if (lighting != null) lighting.NightCondition = null;
            if (legacy != null) legacy.BindPlayerPresentation(null);
            if (minimap != null) minimap.SuppressPresentation = false;
            if (scene != null && scene.PlayerMover != null) scene.PlayerMover.SetInputBlocker(null);
            if (scene != null && scene.CameraFollow != null) scene.CameraFollow.SetOverlayTrackpadInput(null, null);
            if (game != null)
            {
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
