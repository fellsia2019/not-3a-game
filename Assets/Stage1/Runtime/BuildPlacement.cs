using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage1
{
    public sealed class BuildPlacement : MonoBehaviour
    {
        [SerializeField] private Stage1GameController game;
        [SerializeField] private Stage1Hud hud;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private DynamicNavigationGrid navigation;
        [SerializeField] private TowerController towerPrefab;
        [SerializeField] private SpriteRenderer ghost;
        [SerializeField, Min(0)] private int woodCost = 5;
        [SerializeField, Min(0)] private int stoneCost = 3;

        private readonly List<TowerController> placedTowers = new List<TowerController>();
        private InputAction buildAction;
        private InputAction confirmAction;
        private InputAction cancelAction;
        private string currentReason;
        private bool currentValid;

        public bool IsPlacing { get; private set; }
        public int PlacedTowerCount => placedTowers.Count;

        public void Configure(
            Stage1GameController controller,
            Stage1Hud stageHud,
            Camera camera,
            DynamicNavigationGrid grid,
            TowerController prefab,
            SpriteRenderer ghostRenderer,
            int wood,
            int stone)
        {
            game = controller;
            hud = stageHud;
            worldCamera = camera;
            navigation = grid;
            towerPrefab = prefab;
            ghost = ghostRenderer;
            woodCost = wood;
            stoneCost = stone;
            if (ghost != null)
            {
                ghost.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            buildAction = new InputAction("BuildTower", InputActionType.Button, "<Keyboard>/b");
            confirmAction = new InputAction("ConfirmBuild", InputActionType.Button, "<Mouse>/leftButton");
            cancelAction = new InputAction("CancelBuild", InputActionType.Button);
            cancelAction.AddBinding("<Keyboard>/escape");
            cancelAction.AddBinding("<Mouse>/rightButton");
            buildAction.Enable();
            confirmAction.Enable();
            cancelAction.Enable();
        }

        private void OnDisable()
        {
            buildAction?.Dispose();
            confirmAction?.Dispose();
            cancelAction?.Dispose();
            buildAction = null;
            confirmAction = null;
            cancelAction = null;
        }

        private void Update()
        {
            if (buildAction != null && buildAction.WasPressedThisFrame())
            {
                if (IsPlacing)
                {
                    CancelPlacement();
                }
                else
                {
                    BeginPlacement();
                }
            }

            if (!IsPlacing)
            {
                return;
            }

            if (game == null || !game.CanBuild || (cancelAction != null && cancelAction.WasPressedThisFrame()))
            {
                CancelPlacement();
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null || worldCamera == null)
            {
                return;
            }

            var screen = mouse.position.ReadValue();
            var world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
            var snapped = IsoGrid.Snap(world);
            ghost.transform.position = snapped;
            currentValid = ValidatePosition(snapped, out currentReason);
            ghost.color = currentValid
                ? new Color(0.2f, 1f, 0.45f, 0.65f)
                : new Color(1f, 0.25f, 0.2f, 0.65f);
            hud.SetContext(currentValid
                ? $"Башня: {woodCost} дерева + {stoneCost} камня · место допустимо"
                : $"Башня: {currentReason}");

            if (confirmAction != null && confirmAction.WasPressedThisFrame())
            {
                TryPlaceAt(snapped, out _);
            }
        }

        public void BeginPlacement()
        {
            if (game == null || !game.CanBuild)
            {
                return;
            }

            IsPlacing = true;
            if (ghost != null)
            {
                ghost.gameObject.SetActive(true);
            }
        }

        public void CancelPlacement()
        {
            IsPlacing = false;
            if (ghost != null)
            {
                ghost.gameObject.SetActive(false);
            }

            game?.SetContextHint("B — выбрать башню; SPACE — начать волну раньше");
        }

        public bool TryPlaceAt(Vector2 worldPosition, out string reason)
        {
            var snapped = IsoGrid.Snap(worldPosition);
            if (!ValidatePosition(snapped, out reason))
            {
                game?.SetContextHint($"Нельзя построить: {reason}");
                return false;
            }

            if (!game.TrySpendTowerCost(woodCost, stoneCost))
            {
                reason = "Не хватает ресурсов";
                game.SetContextHint($"Нельзя построить: {reason}");
                return false;
            }

            var tower = Instantiate(towerPrefab, snapped, Quaternion.identity);
            tower.name = $"Tower {placedTowers.Count + 1}";
            var cell = IsoGrid.WorldToCell(snapped);
            tower.ConfigureNavigation(navigation, cell);
            if (!navigation.RegisterTower(cell, tower))
            {
                Destroy(tower.gameObject);
                reason = "Клетка занята";
                return false;
            }

            placedTowers.Add(tower);
            CancelPlacement();
            reason = string.Empty;
            return true;
        }

        private bool ValidatePosition(Vector2 position, out string reason)
        {
            if (game == null || !game.CanBuild)
            {
                reason = "Забег завершён";
                return false;
            }

            if (!game.CanAffordTower(woodCost, stoneCost))
            {
                reason = $"Нужно {woodCost} дерева и {stoneCost} камня";
                return false;
            }

            if (navigation == null)
            {
                reason = "Навигация не настроена";
                return false;
            }

            return navigation.ValidateBuildCell(IsoGrid.WorldToCell(position), out reason);
        }
    }
}
