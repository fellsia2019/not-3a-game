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
        [SerializeField] private Stage2EnemyCatalog enemyCatalog;
        [SerializeField] private Stage2RunCatalog runCatalog;
        [SerializeField] private Stage2GatheringDriver gathering;
        [SerializeField] private Stage2BuildDriver building;
        [SerializeField] private Stage2RunDriver runDriver;
        [SerializeField] private bool showDevelopmentHud = true;

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
            Stage2EnemyCatalog enemyData, Stage2RunCatalog runData,
            Stage2GatheringDriver economyDriver, Stage2BuildDriver buildDriver,
            Stage2RunDriver stateDriver)
        {
            bootstrap = sceneBootstrap;
            economyCatalog = economyData;
            defenseCatalog = defenseData;
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
                defenseCatalog == null || enemyCatalog == null || runCatalog == null)
                return BindingError("MissingOrUninitializedBinding");
            if (!economyCatalog.TryValidate(out var reason)) return BindingError($"InvalidEconomyCatalog {reason}");
            if (!defenseCatalog.TryValidate(out reason)) return BindingError($"InvalidDefenseCatalog {reason}");
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
            if (!building.Configure(map, defenseCatalog, gathering, hero, camera, navigation))
                return BindingError("DefenseConfigureFailed");
            enemies = new Stage2EnemyGroup(map, navigation, enemyCatalog);
            building.SetEnemyProtection(enemies.IsCellProtected);
            if (!runDriver.Configure(runCatalog, gathering, building, hero, navigation, enemies))
                return BindingError("RunConfigureFailed");
            if (!CreateResourceMarkers()) return false;
            IsInitialized = true;
            Debug.Log($"S2_INT_GAMEPLAY_READY map={map.name} hash={navigation.MapHash}", this);
            return true;
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

        private static string SourceDirection(Vector2 offset)
        {
            var vertical = offset.y > 0.5f ? "W" : offset.y < -0.5f ? "S" : "";
            var horizontal = offset.x > 0.5f ? "D" : offset.x < -0.5f ? "A" : "";
            return vertical.Length == 0 && horizontal.Length == 0 ? "HERE" :
                vertical.Length == 0 ? horizontal :
                horizontal.Length == 0 ? vertical : vertical + "+" + horizontal;
        }

        private void OnGUI()
        {
            if (!Debug.isDebugBuild || !showDevelopmentHud || !IsInitialized || runDriver.Run == null) return;
            var run = runDriver.Run;
            var inventory = gathering.Economy.Inventory;
            var phase = run.Phase == Stage2RunPhase.Preparation
                ? $"Preparation {Mathf.CeilToInt(run.PreparationRemaining)}s"
                : run.Phase.ToString();
            GUI.Box(new Rect(10, 10, 400, 150), "Stage 2 development readout");
            GUI.Label(new Rect(20, 32, 380, 20),
                $"{phase}  |  Throne {run.ThroneHealth}/{run.ThroneMaximum}  |  Enemies {run.ActiveEnemies}");
            GUI.Label(new Rect(20, 53, 380, 20),
                $"Wood {inventory.Get(MapResourceKind.Wood)}  Stone {inventory.Get(MapResourceKind.Stone)}  Metal {inventory.Get(MapResourceKind.Metal)}");
            GUI.Label(new Rect(20, 74, 380, 20),
                $"Wave {run.WaveNumber}/{run.WaveCount} ({run.WaveEnemyCount} total)  |  Build: {(building.Defense.IsPlacing ? building.PreviewResult.ToString() : "off")}");
            GUI.Label(new Rect(20, 96, 380, 20),
                "WASD move  E gather  B build  click confirm  Esc cancel");
            GUI.Label(new Rect(20, 117, 380, 20), "Space start wave  |  R restart after win/loss");

            if (run.IsTerminal)
            {
                var message = run.Phase == Stage2RunPhase.Lost
                    ? "Throne lost. Press R to restart before gathering."
                    : "Run won. Press R to restart before gathering.";
                GUI.Box(new Rect(Mathf.Max(10f, (Screen.width - 370f) * 0.5f),
                    Mathf.Max(170f, (Screen.height - 52f) * 0.5f), 370f, 52f), message);
                return;
            }

            var sideBySide = Screen.width >= 630;
            var panelX = sideBySide ? Screen.width - 215f : 10f;
            var panelY = sideBySide ? 10f : 165f;
            GUI.Box(new Rect(panelX, panelY, 205, 108), "Resource sources");
            MapResourceKind? nearbyKind = null;
            for (var index = 0; index < 3; index++)
            {
                var kind = (MapResourceKind)index;
                var node = gathering.Economy.GetNode(kind);
                var offset = gathering.Economy.WorldPosition(kind) - (Vector2)bootstrap.PlayerMover.transform.position;
                if (!node.IsDepleted && offset.magnitude <= economyCatalog.InteractionRange)
                    nearbyKind = kind;
                var stock = node.IsDepleted
                    ? $"0/{node.Capacity} ({Mathf.CeilToInt(node.RecoveryRemaining)}s)"
                    : $"{node.Remaining}/{node.Capacity}";
                GUI.Label(new Rect(panelX + 10, panelY + 24 + index * 25, 190, 20),
                    $"{kind}: {stock}  {offset.magnitude:0}m  {SourceDirection(offset)}");

                var sourceScreen = bootstrap.CameraFollow.BoundCamera.WorldToScreenPoint(
                    gathering.Economy.WorldPosition(kind));
                var sourceY = Screen.height - sourceScreen.y;
                var hiddenByReadout = sourceScreen.x < 410f && sourceY < 165f;
                var hiddenBySourcePanel = sideBySide && sourceScreen.x > panelX && sourceY < 125f;
                var outsideScreen = sourceScreen.z <= 0f || sourceScreen.x < 12f ||
                    sourceScreen.x > Screen.width - 12f || sourceY < 12f || sourceY > Screen.height - 12f;
                if (!hiddenByReadout && !hiddenBySourcePanel && !outsideScreen) continue;

                var hintX = Mathf.Clamp(sourceScreen.x - 48f, 10f, Mathf.Max(10f, Screen.width - 106f));
                var hintY = Mathf.Clamp(sourceY - 10f, 165f, Mathf.Max(165f, Screen.height - 32f));
                GUI.Box(new Rect(hintX, hintY, 96f, 22f), $"{kind} {SourceDirection(offset)}");
            }
            if (nearbyKind.HasValue)
                GUI.Box(new Rect(Mathf.Max(10f, (Screen.width - 210f) * 0.5f),
                    Screen.height - 42f, 210f, 30f), $"Hold E to gather {nearbyKind.Value}");
        }
    }
}
