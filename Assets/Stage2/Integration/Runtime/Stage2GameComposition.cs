using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.RunState;
using Not3A.Stage2.WavesAI;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Scene-owned wiring only. Each system retains its own rules and data.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage2GameComposition : MonoBehaviour
    {
        [SerializeField] private Stage2SceneBootstrap bootstrap;
        [SerializeField] private Stage2EconomyCatalog economyCatalog;
        [SerializeField] private Stage2DefenseCatalog defenseCatalog;
        [SerializeField] private Stage2DefenseCatalog longshotCatalog;
        [SerializeField] private Stage2EnemyCatalog enemyCatalog;
        [SerializeField] private Stage2RunCatalog runCatalog;
        [SerializeField] private Stage2GatheringDriver gathering;
        [SerializeField] private Stage2BuildDriver building;
        [SerializeField] private Stage2RunDriver runDriver;
        [SerializeField] private bool showDevelopmentHud = true;

        private Stage2GameHud hud;
        private Stage2Navigation navigation;
        private Stage2EnemyGroup enemies;
        private readonly GameObject[] resourceMarkers = new GameObject[3];

        public bool IsInitialized { get; private set; }
        public Stage2Navigation Navigation => navigation;
        public Stage2EnemyGroup Enemies => enemies;
        public Stage2GatheringDriver Gathering => gathering;
        public Stage2BuildDriver Building => building;
        public Stage2RunDriver RunDriver => runDriver;

        public void Configure(Stage2SceneBootstrap sceneBootstrap,
            Stage2EconomyCatalog economyData, Stage2DefenseCatalog defenseData,
            Stage2DefenseCatalog longshotData,
            Stage2EnemyCatalog enemyData, Stage2RunCatalog runData,
            Stage2GatheringDriver economyDriver, Stage2BuildDriver buildDriver,
            Stage2RunDriver stateDriver)
        {
            bootstrap = sceneBootstrap;
            economyCatalog = economyData;
            defenseCatalog = defenseData;
            longshotCatalog = longshotData;
            enemyCatalog = enemyData;
            runCatalog = runData;
            gathering = economyDriver;
            building = buildDriver;
            runDriver = stateDriver;
        }

        public bool Initialize()
        {
            if (IsInitialized) return true;
            if (bootstrap == null || !bootstrap.IsInitialized || gathering == null ||
                building == null || runDriver == null || economyCatalog == null ||
                defenseCatalog == null || longshotCatalog == null ||
                enemyCatalog == null || runCatalog == null)
                return BindingError("MissingOrUninitializedBinding");
            if (!economyCatalog.TryValidate(out var reason)) return BindingError($"InvalidEconomyCatalog {reason}");
            if (!defenseCatalog.TryValidate(out reason)) return BindingError($"InvalidDefenseCatalog {reason}");
            if (!longshotCatalog.TryValidate(out reason)) return BindingError($"InvalidLongshotCatalog {reason}");
            if (!enemyCatalog.TryValidate(out reason)) return BindingError($"InvalidEnemyCatalog {reason}");
            if (!runCatalog.TryValidate(out reason)) return BindingError($"InvalidRunCatalog {reason}");

            var map = bootstrap.MapDefinition;
            var hero = bootstrap.PlayerMover.transform;
            var camera = bootstrap.CameraFollow.BoundCamera;
            navigation = new Stage2Navigation(map);
            if (!navigation.IsValid) return BindingError($"InvalidNavigation {navigation.ValidationDiagnostic}");
            gathering.Configure(map, economyCatalog, hero);
            if (gathering.Economy == null)
                return BindingError("EconomyConfigureFailed");
            if (!building.Configure(map, defenseCatalog, new[] { longshotCatalog },
                    gathering, hero, camera, navigation))
                return BindingError("DefenseConfigureFailed");
            enemies = new Stage2EnemyGroup(map, navigation, enemyCatalog);
            building.SetEnemyProtection(enemies.IsCellProtected);
            if (!runDriver.Configure(runCatalog, gathering, building, hero, navigation, enemies))
                return BindingError("RunConfigureFailed");
            if (!CreateResourceMarkers()) return false;
            IsInitialized = true;
            hud = GetComponent<Stage2GameHud>();
            if (hud == null) hud = gameObject.AddComponent<Stage2GameHud>();
            hud.Initialize(bootstrap, this, showDevelopmentHud);
            building.SetPointerBlocker(ContainsScreenPoint);
            bootstrap.CameraFollow.SetPointerBlocker(ContainsScreenPoint);
            Debug.Log($"S2_INT_GAMEPLAY_READY map={map.name} hash={navigation.MapHash}", this);
            return true;
        }

        public bool ContainsScreenPoint(Vector2 screen)
        {
            if (hud != null && hud.ContainsScreenPoint(screen)) return true;
            var minimap = GetComponent<Stage2Minimap>();
            return minimap != null && minimap.ContainsScreenPoint(screen);
        }

        public void SetPaused(bool value) => runDriver?.SetPaused(value);

        private void Start() => Initialize();

        private void LateUpdate()
        {
            if (!IsInitialized) return;
            for (var index = 0; index < resourceMarkers.Length; index++)
            {
                var marker = resourceMarkers[index];
                var visible = !gathering.Economy.GetNode((MapResourceKind)index).IsDepleted;
                if (marker != null && marker.activeSelf != visible) marker.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            building?.SetPointerBlocker(null);
            if (bootstrap != null && bootstrap.CameraFollow != null)
                bootstrap.CameraFollow.SetPointerBlocker(null);
            enemies?.ResetRun();
            building?.Defense?.ResetRun();
            navigation?.Reset();
        }

        private bool BindingError(string reason)
        {
            Debug.LogError($"S2_INT_GAMEPLAY_BINDING_ERROR {reason}", this);
            return false;
        }

        private bool CreateResourceMarkers()
        {
            var diamond = bootstrap.EntranceMarker.GetComponentInChildren<SpriteRenderer>()?.sprite;
            var solid = bootstrap.ThroneMarker.GetComponentInChildren<SpriteRenderer>()?.sprite;
            if (diamond == null || solid == null) return BindingError("MissingResourceMarkerSprite");

            for (var index = 0; index < 3; index++)
            {
                var kind = (MapResourceKind)index;
                var marker = new GameObject($"{kind} Resource Marker").transform;
                marker.SetParent(transform, false);
                resourceMarkers[index] = marker.gameObject;
                var anchor = gathering.Economy.WorldPosition(kind);
                marker.position = new Vector3(anchor.x, anchor.y, -0.2f);

                var ground = new GameObject("Ground Diamond").AddComponent<SpriteRenderer>();
                ground.transform.SetParent(marker, false);
                ground.sprite = diamond;
                ground.color = new Color(0.08f, 0.13f, 0.14f);
                ground.transform.localScale = new Vector3(1.55f, 0.9f, 1f);
                ground.sortingOrder = 9;

                var symbol = new GameObject("Source Symbol").AddComponent<SpriteRenderer>();
                symbol.transform.SetParent(marker, false);
                symbol.sprite = kind == MapResourceKind.Stone ? diamond : solid;
                symbol.color = kind == MapResourceKind.Wood ? new Color(0.75f, 0.95f, 0.28f) :
                    kind == MapResourceKind.Stone ? new Color(0.9f, 0.9f, 0.82f) :
                    new Color(0.58f, 0.65f, 1f);
                symbol.transform.localPosition = new Vector3(0f, 0.55f, -0.01f);
                symbol.transform.localScale = kind == MapResourceKind.Stone ?
                    new Vector3(1.05f, 0.85f, 1f) :
                    kind == MapResourceKind.Wood ? new Vector3(0.8f, 1.35f, 1f) :
                    new Vector3(0.58f, 1.25f, 1f);
                symbol.sortingOrder = 10;
            }
            return true;
        }

    }
}
