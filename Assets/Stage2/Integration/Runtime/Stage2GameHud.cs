using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Integration
{
    public enum Stage2BuildCategory { Towers, Generators, Research, Home }
    public enum Stage2WorkshopBuildStatus { Unavailable, Ready, Placing, Built }

    /// <summary>Scene-owned player HUD and development tools over the existing gameplay rules.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-200)]
    public sealed class Stage2GameHud : MonoBehaviour
    {
        private Stage2SceneBootstrap scene;
        private Stage2GameComposition game;
        private bool allowDebug;
        private bool ownsClock;
        private float previousTimeScale;
        private InputAction toggle;
        private Vector2 scroll;
        private string feedback = "Изменения действуют только в текущем запуске.";
        private Func<Vector2, bool> playerPresentation;
        private Func<Rect> playerResourcesRect, playerBuildRect, playerStatusRect;
        public bool HasPlayerPresentation => playerPresentation != null;
        public void BindPlayerPresentation(Func<Vector2, bool> pointerHitTest, Func<Rect> resources = null,
            Func<Rect> construction = null, Func<Rect> status = null)
        {
            playerPresentation = pointerHitTest;
            playerResourcesRect = resources; playerBuildRect = construction; playerStatusRect = status;
        }
        public string WorldInteractionHint { get; set; }
        public string DwellingInteractionHint { get; set; }
        private Sprite dwellingIcon;
        private Func<Stage2WorkshopBuildStatus> dwellingStatus;
        private Func<bool> canSelectDwelling;
        private Action beginDwelling, cancelDwelling;
        private int dwellingWoodCost;
        public Stage2WorkshopBuildStatus DwellingStatus => dwellingStatus?.Invoke() ?? Stage2WorkshopBuildStatus.Unavailable;
        public bool CanSelectDwelling => canSelectDwelling?.Invoke() ?? false;
        public void BindDwellingConstruction(Sprite icon, Func<Stage2WorkshopBuildStatus> status,
            Func<bool> canSelect, Action begin, Action cancel, int woodCost)
        { dwellingIcon = icon; dwellingStatus = status; canSelectDwelling = canSelect;
            beginDwelling = begin; cancelDwelling = cancel; dwellingWoodCost = woodCost; }
        private Sprite workshopIcon;
        private Func<Stage2WorkshopBuildStatus> workshopStatus;
        private Action beginWorkshop, cancelWorkshop;
        public Stage2WorkshopBuildStatus WorkshopStatus => workshopStatus?.Invoke() ?? Stage2WorkshopBuildStatus.Unavailable;
        public bool CanSelectWorkshop => WorkshopStatus == Stage2WorkshopBuildStatus.Ready || WorkshopStatus == Stage2WorkshopBuildStatus.Placing;
        // The Stage 3 scene supplies its construction actions without coupling this assembly to its artwork/runtime.
        public void BindWorkshopConstruction(Sprite icon, Func<Stage2WorkshopBuildStatus> status, Action begin, Action cancel)
        { workshopIcon = icon; workshopStatus = status; beginWorkshop = begin; cancelWorkshop = cancel; }
        private GUIStyle title, text, muted, number, button, smallButton, centered, smallButtonText;
        private Texture2D buttonNormal, buttonHover, buttonActive;
        private static readonly Color Ink = new Color(.055f, .085f, .095f, .97f);
        private static readonly Color Edge = new Color(.38f, .46f, .41f);
        private static readonly Color Gold = new Color(.91f, .75f, .40f);
        private static readonly Color Light = new Color(.93f, .94f, .86f);

        public bool DebugAvailable => allowDebug && Debug.isDebugBuild;
        public bool DebugOpen { get; private set; }
        public bool DebugPaused { get; private set; }
        public int GameSpeed { get; private set; } = 1;
        public int HeroSpeed { get; private set; } = 1;
        public Stage2BuildCategory BuildCategory { get; private set; } = Stage2BuildCategory.Towers;
        public float Scale => Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), .75f, 1.35f);
        private float Width => Screen.width / Scale;
        private float Height => Screen.height / Scale;
        public Rect DebugRect => new Rect(Width - 408f, 132f, 392f, Height - 148f);
        public Rect ResourcesRect => playerResourcesRect?.Invoke() ?? new Rect(16f, 16f, Mathf.Min(476f, Width * .55f - 24f), 70f);
        public Rect StatusRect => playerStatusRect?.Invoke() ?? new Rect(Width - 320f, 16f, 304f, 98f);
        public Rect DebugButtonRect => new Rect(Width - 146f, 120f, 130f, 32f);
        public Rect OutcomeRect => new Rect((Width - 400f) * .5f, (Height - 220f) * .42f, 400f, 220f);
        public Rect HintRect => new Rect(BuildRect.x, BuildRect.y - 35f, BuildRect.width, 27f);
        public Rect BuildRect
        {
            get
            {
                if (playerBuildRect != null) return playerBuildRect();
                var map = Stage2Minimap.Layout(new Vector2(Screen.width, Screen.height));
                var left = Mathf.Max(map.xMax / Scale + 16f, (Width - 620f) * .5f);
                return new Rect(left, Height - 194f, Mathf.Min(620f, Width - left - 16f), 178f);
            }
        }
        private Rect TabRect(int index)
        {
            var count = dwellingIcon != null ? 4 : 3;
            var width = (BuildRect.width - 24f - (count - 1) * 8f) / count;
            return new Rect(BuildRect.x + 12f + index * (width + 8f), BuildRect.y + 10f, width, 27f);
        }
        public Rect TowersTabRect => workshopIcon != null || dwellingIcon != null ? TabRect(0) : new Rect(BuildRect.x + 12f, BuildRect.y + 10f, 132f, 27f);
        public Rect GeneratorsTabRect => workshopIcon != null || dwellingIcon != null ? TabRect(1) : new Rect(BuildRect.x + 152f, BuildRect.y + 10f, 152f, 27f);
        public Rect ResearchTabRect => TabRect(2);
        public Rect DwellingTabRect => TabRect(3);
        public Rect BuildChoiceRect(int index)
        {
            var count = BuildCategory == Stage2BuildCategory.Towers ? Mathf.Max(1, game?.Building?.Defense?.RoleCount ?? 2) :
                BuildCategory == Stage2BuildCategory.Research || BuildCategory == Stage2BuildCategory.Home ? 1 : 3;
            var width = (BuildRect.width - 24f - (count - 1) * 8f) / count;
            return new Rect(BuildRect.x + 12f + index * (width + 8f), BuildRect.y + 47f, width, 88f);
        }

        public void SelectBuildCategory(Stage2BuildCategory category)
        {
            if (game == null || (category == Stage2BuildCategory.Generators && game.Production == null)) return;
            if (category == Stage2BuildCategory.Research && workshopIcon == null) return;
            if (category == Stage2BuildCategory.Home && dwellingIcon == null) return;
            if (BuildCategory == category) return;
            game.Building.CancelPlacement();
            game.Production?.CancelPlacement();
            cancelWorkshop?.Invoke();
            cancelDwelling?.Invoke();
            BuildCategory = category;
        }

        public void SelectTower(int role)
        {
            if (!CanSelectTower(role)) return;
            SelectBuildCategory(Stage2BuildCategory.Towers);
            game.Building.SelectAndBeginPlacement(role);
        }

        public void SelectGenerator(ResourceProductionBuilding building)
        {
            if (!CanSelectGenerator(building)) return;
            SelectBuildCategory(Stage2BuildCategory.Generators);
            game.Production.SelectAndBeginPlacement(building);
        }

        public bool CanSelectTower(int role) => game != null && game.Building != null && game.Building.CanBeginPlacement(role);
        public bool CanSelectGenerator(ResourceProductionBuilding building) =>
            game != null && game.Production != null && game.Production.CanBeginPlacement(building);

        public void SelectWorkshop()
        {
            if (!CanSelectWorkshop) return;
            SelectBuildCategory(Stage2BuildCategory.Research);
            beginWorkshop?.Invoke();
        }

        public void SelectDwelling()
        {
            if (!CanSelectDwelling) return;
            SelectBuildCategory(Stage2BuildCategory.Home);
            beginDwelling?.Invoke();
        }

        public void Initialize(Stage2SceneBootstrap bootstrap, Stage2GameComposition composition, bool developmentTools)
        {
            if (game != null && game.Building != null) game.Building.PlacementStarting -= ShowTowerCategory;
            scene = bootstrap;
            game = composition;
            allowDebug = developmentTools;
            game.Building.PlacementStarting += ShowTowerCategory;
        }

        private void ShowTowerCategory() { cancelWorkshop?.Invoke(); cancelDwelling?.Invoke(); BuildCategory = Stage2BuildCategory.Towers; }

        public void SetDebugOpen(bool value) => DebugOpen = DebugAvailable && value;

        public void SetHeroSpeed(int multiplier)
        {
            if (!DebugAvailable || scene == null) return;
            HeroSpeed = Mathf.Clamp(multiplier, 1, 10);
            scene.PlayerMover.SetMovementMultiplier(HeroSpeed);
        }

        public void SetGameSpeed(int multiplier)
        {
            if (!DebugAvailable) return;
            TakeClock();
            GameSpeed = Mathf.Clamp(multiplier, 1, 10);
            ApplyClock();
        }

        public void SetDebugPaused(bool value)
        {
            if (!DebugAvailable || game == null || game.RunDriver.Run.IsTerminal) return;
            TakeClock();
            DebugPaused = value;
            game.SetPaused(value);
            ApplyClock();
        }

        public bool StartNextWave() => game != null && game.RunDriver.Run.StartWave();

        public void RestartRun()
        {
            if (game == null) return;
            DebugPaused = false;
            game.RunDriver.ResetRun();
            ApplyClock();
        }

        public bool AddGold(int amount)
        {
            if (!DebugAvailable || game == null || amount <= 0) return false;
            var inventory = game.Gathering.Economy.Inventory;
            var grant = Math.Min(amount, int.MaxValue - inventory.Gold);
            var added = inventory.TryAddGold(grant);
            feedback = added ? $"Добавлено золото: {grant}." : "Достигнут предел золота.";
            return added;
        }

        public bool AddAllResources(int amount)
        {
            if (!DebugAvailable || game == null || amount <= 0) return false;
            var inventory = game.Gathering.Economy.Inventory;
            var added = false;
            for (var i = 0; i < 3; i++)
            {
                var kind = (MapResourceKind)i;
                added |= inventory.TryAdd(kind, Math.Min(amount, int.MaxValue - inventory.Get(kind)));
            }
            feedback = added ? $"Дерево, камень и металл: +{amount} (до предела)." : "Достигнут предел ресурсов.";
            return added;
        }

        // Input System coordinates are bottom-left pixels; layout is top-left scaled GUI units.
        public bool ContainsScreenPoint(Vector2 screen)
        {
            if (game == null || !game.IsInitialized) return false;
            var point = new Vector2(screen.x, Screen.height - screen.y) / Scale;
            var run = game.RunDriver.Run;
            if (playerPresentation != null)
                return (isActiveAndEnabled && DebugAvailable && (DebugOpen ? DebugRect : DebugButtonRect).Contains(point)) ||
                    (isActiveAndEnabled && run.IsTerminal && OutcomeRect.Contains(point)) || playerPresentation(screen);
            if (!isActiveAndEnabled) return false;
            return ResourcesRect.Contains(point) || StatusRect.Contains(point) ||
                (DebugAvailable && (DebugOpen ? DebugRect : DebugButtonRect).Contains(point)) ||
                (run.IsTerminal && OutcomeRect.Contains(point)) ||
                (!run.IsTerminal && !DebugOpen && (BuildRect.Contains(point) || HintRect.Contains(point)));
        }

        private void OnEnable()
        {
            toggle = new InputAction("Development menu", InputActionType.Button, "<Keyboard>/f3");
            toggle.Enable();
        }

        private void Update()
        {
            if (toggle != null && toggle.WasPressedThisFrame()) SetDebugOpen(!DebugOpen);
            if (game != null && game.Building.Defense != null && game.Building.Defense.IsPlacing)
                BuildCategory = Stage2BuildCategory.Towers;
            else if (game != null && game.Production != null && game.Production.IsPlacing)
                BuildCategory = Stage2BuildCategory.Generators;
            else if (WorkshopStatus == Stage2WorkshopBuildStatus.Placing)
                BuildCategory = Stage2BuildCategory.Research;
            else if (DwellingStatus == Stage2WorkshopBuildStatus.Placing)
                BuildCategory = Stage2BuildCategory.Home;
            // External run-state pause must also freeze hero movement while this UI owns the clock.
            if (ownsClock) ApplyClock();
        }

        private void TakeClock()
        {
            if (ownsClock) return;
            previousTimeScale = Time.timeScale;
            ownsClock = true;
        }

        private void ApplyClock()
        {
            if (ownsClock) Time.timeScale = DebugPaused || (game != null && game.RunDriver.Run.IsPaused) ? 0f : GameSpeed;
        }

        private void OnDisable()
        {
            toggle?.Dispose();
            toggle = null;
            if (ownsClock) Time.timeScale = previousTimeScale;
            ownsClock = false;
            if (DebugPaused && game != null && game.RunDriver != null) game.SetPaused(false);
            if (scene != null && scene.PlayerMover != null) scene.PlayerMover.SetMovementMultiplier(1f);
            DebugOpen = DebugPaused = false;
            GameSpeed = HeroSpeed = 1;
        }

        private void OnDestroy()
        {
            if (game != null && game.Building != null) game.Building.PlacementStarting -= ShowTowerCategory;
            if (buttonNormal != null) Destroy(buttonNormal);
            if (buttonHover != null) Destroy(buttonHover);
            if (buttonActive != null) Destroy(buttonActive);
        }

        private void OnGUI()
        {
            if (game == null || !game.IsInitialized || game.RunDriver.Run == null) return;
            EnsureStyles();
            var matrix = GUI.matrix;
            var color = GUI.color;
            var enabled = GUI.enabled;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * Scale);
            GUI.color = Color.white;
            if (playerPresentation == null)
            {
                DrawResources();
                DrawStatus();
            }
            if (game.RunDriver.Run.IsTerminal) DrawOutcome();
            else if (!DebugOpen && playerPresentation == null) DrawBuild();
            if (DebugAvailable)
            {
                if (DebugOpen) DrawDebug();
                else if (playerPresentation == null && GUI.Button(DebugButtonRect, "ДЕБАГ  ·  F3", smallButton)) SetDebugOpen(true);
            }
            GUI.enabled = enabled;
            GUI.color = color;
            GUI.matrix = matrix;
        }

        private void DrawResources()
        {
            var area = ResourcesRect;
            Card(area);
            var inventory = game.Gathering.Economy.Inventory;
            var names = new[] { "ЗОЛОТО", "ДЕРЕВО", "КАМЕНЬ", "МЕТАЛЛ" };
            var accents = new[] { Gold, new Color(.62f, .78f, .43f), new Color(.79f, .80f, .76f), new Color(.55f, .73f, .83f) };
            var cell = (area.width - 24f) / 4f;
            for (var i = 0; i < 4; i++)
            {
                var x = area.x + 14f + cell * i;
                Fill(new Rect(x, area.y + 14f, 3f, 40f), accents[i]);
                Label(new Rect(x + 10f, area.y + 11f, cell - 12f, 18f), names[i], muted);
                Label(new Rect(x + 10f, area.y + 29f, cell - 12f, 29f),
                    (i == 0 ? inventory.Gold : inventory.Get((MapResourceKind)(i - 1))).ToString(), number);
            }
        }

        private void DrawStatus()
        {
            var r = StatusRect;
            var run = game.RunDriver.Run;
            Card(r);
            Label(new Rect(r.x + 14f, r.y + 10f, 150f, 23f), run.ContinuousDays ?
                $"{(run.IsNight ? "НОЧЬ" : "ДЕНЬ")}  {run.WaveNumber}" : $"ВОЛНА  {run.WaveNumber} / {run.WaveCount}", title);
            Label(new Rect(r.x + 177f, r.y + 13f, 115f, 20f), $"ТРОН  {run.ThroneHealth}/{run.ThroneMaximum}", text);
            var health = new Rect(r.x + 177f, r.y + 37f, 110f, 4f);
            Fill(health, Edge);
            Fill(new Rect(health.x, health.y, health.width * run.ThroneHealth / run.ThroneMaximum, health.height), Gold);
            var state = DebugPaused || run.IsPaused ? "Пауза" : run.Phase == Stage2RunPhase.Preparation
                ? run.ContinuousDays ? $"До ночи  {Mathf.CeilToInt(run.PreparationRemaining) / 60:00}:{Mathf.CeilToInt(run.PreparationRemaining) % 60:00}" :
                    $"Подготовка · {Mathf.CeilToInt(run.PreparationRemaining)} сек."
                : run.Phase == Stage2RunPhase.Wave ? $"Врагов на карте: {run.ActiveEnemies}"
                : run.Phase == Stage2RunPhase.Won ? "Трон защищён" : "Трон разрушен";
            Label(new Rect(r.x + 14f, r.y + 35f, 160f, 20f), state, muted);
            if (run.ContinuousDays)
                Label(new Rect(r.x + 14f, r.y + 62f, 276f, 27f), run.IsNight ?
                    "Рассвет — после уничтожения всей орды" : "E у входа в жилище · ждать ночи", centered);
            else
            {
                GUI.enabled = run.Phase == Stage2RunPhase.Preparation && !run.IsPaused && !DebugPaused;
                if (GUI.Button(new Rect(r.x + 14f, r.y + 62f, 276f, 27f), "Начать волну  [Space]", smallButton)) StartNextWave();
            }
            GUI.enabled = true;
        }

        private void DrawBuild()
        {
            var r = BuildRect;
            Card(r);
            var defense = game.Building.Defense;
            GUI.enabled = !DebugPaused && !game.RunDriver.Run.IsPaused;
            if (GUI.Button(TowersTabRect, defense.RoleCount > 2 ? "Оборона" : "Башни", smallButton)) SelectBuildCategory(Stage2BuildCategory.Towers);
            var buttonsEnabled = GUI.enabled;
            GUI.enabled &= game.Production != null;
            if (GUI.Button(GeneratorsTabRect, "Генераторы", smallButton)) SelectBuildCategory(Stage2BuildCategory.Generators);
            GUI.enabled = buttonsEnabled;
            if (workshopIcon != null && GUI.Button(ResearchTabRect, "Исследования", smallButton)) SelectBuildCategory(Stage2BuildCategory.Research);
            if (dwellingIcon != null && GUI.Button(DwellingTabRect, "Жилище", smallButton)) SelectBuildCategory(Stage2BuildCategory.Home);
            var tab = BuildCategory == Stage2BuildCategory.Towers ? TowersTabRect :
                BuildCategory == Stage2BuildCategory.Research ? ResearchTabRect :
                BuildCategory == Stage2BuildCategory.Home ? DwellingTabRect : GeneratorsTabRect;
            Fill(new Rect(tab.x, tab.yMax + 3f, tab.width, 2f), Gold);
            string description;
            string hoverDescription = null;
            var inventory = game.Gathering.Economy.Inventory;
            if (BuildCategory == Stage2BuildCategory.Home && dwellingIcon != null)
            {
                var choice = BuildChoiceRect(0);
                GUI.enabled = buttonsEnabled && CanSelectDwelling;
                if (GUI.Button(choice, GUIContent.none, smallButton)) SelectDwelling();
                GUI.color = GUI.enabled || DwellingStatus == Stage2WorkshopBuildStatus.Built ? Color.white : new Color(.5f,.5f,.5f,.65f);
                DrawSprite(new Rect(choice.x + 8f, choice.y + 4f, 78f, 78f), dwellingIcon);
                GUI.color = Color.white;
                Label(new Rect(choice.x + 94f, choice.y + 8f, choice.width - 102f, 24f), "Жилище героя", smallButtonText);
                Label(new Rect(choice.x + 94f, choice.y + 37f, choice.width - 102f, 20f), "Позволяет дождаться ночи у входа", muted);
                Label(new Rect(choice.x + 94f, choice.y + 63f, choice.width - 102f, 18f),
                    DwellingStatus == Stage2WorkshopBuildStatus.Built ? "Построено" : Price(dwellingWoodCost,0,0,0), muted);
                GUI.enabled = buttonsEnabled;
                if (DwellingStatus == Stage2WorkshopBuildStatus.Placing)
                    Fill(new Rect(choice.x + 2f, choice.yMax - 4f, choice.width - 4f, 2f), Gold);
                description = DwellingStatus == Stage2WorkshopBuildStatus.Built ?
                    "Днём: E у входа · ночью ожидание недоступно" :
                    $"Одно жилище · {dwellingWoodCost} дерева · свободная зелёная земля";
            }
            else if (BuildCategory == Stage2BuildCategory.Research && workshopIcon != null)
            {
                var choice = BuildChoiceRect(0);
                GUI.enabled = buttonsEnabled && CanSelectWorkshop;
                if (GUI.Button(choice, GUIContent.none, smallButton)) SelectWorkshop();
                GUI.enabled = buttonsEnabled;
                DrawSprite(new Rect(choice.x + 8f, choice.y + 4f, 78f, 78f), workshopIcon);
                Label(new Rect(choice.x + 94f, choice.y + 8f, choice.width - 102f, 24f), "Исследовательская мастерская", smallButtonText);
                Label(new Rect(choice.x + 94f, choice.y + 37f, choice.width - 102f, 20f), "Открывает исследование Телепортации", muted);
                Label(new Rect(choice.x + 94f, choice.y + 63f, choice.width - 102f, 18f),
                    WorkshopStatus == Stage2WorkshopBuildStatus.Built ? "Построена" : "Бесплатно", muted);
                if (WorkshopStatus == Stage2WorkshopBuildStatus.Placing)
                    Fill(new Rect(choice.x + 2f, choice.yMax - 4f, choice.width - 4f, 2f), Gold);
                description = WorkshopStatus == Stage2WorkshopBuildStatus.Built ?
                    "Подойдите к мастерской и нажмите E для исследования" : "Место постройки — свободная зелёная земля";
            }
            else if (BuildCategory == Stage2BuildCategory.Generators && game.Production != null)
            {
                var production = game.Production;
                for (var i = 0; i < production.Catalog.Entries.Count; i++)
                {
                    var entry = production.Catalog.Entries[i];
                    var choice = BuildChoiceRect(i);
                    GUI.enabled = buttonsEnabled && CanSelectGenerator(entry.Building);
                    if (GUI.Button(choice, GUIContent.none, smallButton)) SelectGenerator(entry.Building);
                    GUI.color = GUI.enabled ? Color.white : new Color(.5f, .5f, .5f, .65f);
                    DrawSprite(new Rect(choice.x + 6f, choice.y + 6f, 48f, 48f), entry.Sprite);
                    GUI.color = Color.white;
                    Label(new Rect(choice.x + 58f, choice.y + 5f, choice.width - 63f, 33f), entry.DisplayName, smallButtonText);
                    Label(new Rect(choice.x + 58f, choice.y + 42f, choice.width - 63f, 18f),
                        $"+{entry.OutputAmount} {ResourceName(entry.Resource)} / {entry.IntervalSeconds:0.#} с", muted);
                    var price = Price(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost);
                    Label(new Rect(choice.x + 7f, choice.y + 65f, choice.width - 14f, 18f), price, muted);
                    if (MouseOver(choice)) hoverDescription =
                        (inventory.CanAfford(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost) ? "" : "Не хватает ресурсов · ") +
                        entry.DisplayName + ": " + price;
                    GUI.enabled = buttonsEnabled;
                    if (production.IsPlacing && production.SelectedBuilding == entry.Building)
                        Fill(new Rect(choice.x + 2f, choice.yMax - 4f, choice.width - 4f, 2f), Gold);
                }
                var selected = production.Catalog.Get(production.SelectedBuilding);
                description = $"{selected.DisplayName}: {Price(selected.WoodCost, selected.StoneCost, selected.MetalCost, selected.GoldCost)}";
                if (workshopIcon == null) Label(new Rect(r.x + 320f, r.y + 14f, r.width - 332f, 22f),
                    $"Работает: {production.Rules.Buildings.Count}", muted);
            }
            else
            {
                for (var i = 0; i < defense.RoleCount; i++)
                {
                    var entry = defense.GetCatalog(i);
                    var choice = BuildChoiceRect(i);
                    GUI.enabled = buttonsEnabled && CanSelectTower(i);
                    var label = i < 2 ? $"{i + 1} · {entry.RoleName}" : entry.RoleName;
                    if (GUI.Button(choice, label, button)) SelectTower(i);
                    var price = Price(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost);
                    Label(new Rect(choice.x + 7f, choice.y + 65f, choice.width - 14f, 18f), price, centered);
                    if (MouseOver(choice)) hoverDescription =
                        (inventory.CanAfford(entry.WoodCost, entry.StoneCost, entry.MetalCost, entry.GoldCost) ? "" : "Не хватает ресурсов · ") +
                        entry.RoleName + ": " + price + (entry.CanAttack ? "" : " · блокирует путь, без урона");
                    GUI.enabled = buttonsEnabled;
                    if (defense.SelectedRoleIndex == i)
                        Fill(new Rect(choice.x + 2f, choice.yMax - 4f, choice.width - 4f, 2f), Gold);
                }
                var selected = defense.SelectedCatalog;
                description = $"{selected.RoleName}: {Price(selected.WoodCost, selected.StoneCost, selected.MetalCost, selected.GoldCost)}" +
                    (selected.CanAttack ? "" : " · блокирует путь, без урона");
                if (workshopIcon == null) Label(new Rect(r.x + 320f, r.y + 14f, r.width - 332f, 22f), "На серой дороге", muted);
            }
            GUI.enabled = true;
            Label(new Rect(r.x + 14f, r.y + 147f, r.width - 28f, 22f), hoverDescription ?? description, muted);
            var hint = game.Production != null && game.Production.IsPlacing ? ProductionHint(game.Production.PreviewResult) :
                defense.IsPlacing ? PlacementHint(game.Building.PreviewResult) : NearbyResourceHint();
            if (string.IsNullOrEmpty(hint) && game.Production != null && game.Production.PreviewResult == ProductionPlacementResult.Built)
                hint = "Генератор построен · добыча поступает автоматически";
            if (string.IsNullOrEmpty(hint) && BuildCategory == Stage2BuildCategory.Generators)
                hint = "Выберите генератор, затем его площадку на месторождении";
            if (string.IsNullOrEmpty(hint)) hint = "WASD · движение    B · строить    Колесо · масштаб";
            var hintArea = HintRect;
            Fill(hintArea, Ink);
            Label(hintArea, hint, centered);
        }

        private bool MouseOver(Rect area)
        {
            if (Mouse.current == null) return false;
            var mouse = Mouse.current.position.ReadValue();
            return area.Contains(new Vector2(mouse.x, Screen.height - mouse.y) / Scale);
        }

        private static string ResourceName(MapResourceKind kind) => kind == MapResourceKind.Wood ? "дер." :
            kind == MapResourceKind.Stone ? "кам." : "мет.";

        private static string Price(int wood, int stone, int metal, int gold)
        {
            var result = "";
            void Append(int value, string name)
            {
                if (value <= 0) return;
                if (result.Length > 0) result += " · ";
                result += value + " " + name;
            }
            Append(gold, "зол."); Append(wood, "дер."); Append(stone, "кам."); Append(metal, "мет.");
            return result.Length == 0 ? "Бесплатно" : result;
        }

        private static void DrawSprite(Rect area, Sprite sprite)
        {
            if (sprite == null) return;
            var rect = sprite.rect;
            var factor = Mathf.Min(area.width / rect.width, area.height / rect.height);
            var size = rect.size * factor;
            var fitted = new Rect(area.center - size * .5f, size);
            GUI.DrawTextureWithTexCoords(fitted, sprite.texture,
                new Rect(rect.x / sprite.texture.width, rect.y / sprite.texture.height,
                    rect.width / sprite.texture.width, rect.height / sprite.texture.height));
        }

        private static string ProductionHint(ProductionPlacementResult result)
        {
            switch (result)
            {
                case ProductionPlacementResult.Ready: return "ЛКМ · построить на площадке    Esc / ПКМ · отменить";
                case ProductionPlacementResult.InsufficientResources: return "Недостаточно ресурсов · цена указана внизу";
                case ProductionPlacementResult.WrongType: return "Здесь нужен генератор другого ресурса";
                case ProductionPlacementResult.Occupied: return "На этой площадке уже есть генератор";
                case ProductionPlacementResult.HeroBlocked: return "Отойдите от площадки перед строительством";
                case ProductionPlacementResult.Inactive: return "Строительство сейчас недоступно";
                default: return "Наведите на площадку нужного месторождения · Esc / ПКМ отменяет";
            }
        }

        private string NearbyResourceHint()
        {
            if (!string.IsNullOrEmpty(DwellingInteractionHint))
                return DebugPaused ? "Игра на паузе · продолжить в дебаг-меню" : DwellingInteractionHint;
            if (!string.IsNullOrEmpty(WorldInteractionHint))
                return DebugPaused ? "Игра на паузе · продолжить в дебаг-меню" : WorldInteractionHint;
            var economy = game.Gathering.Economy;
            var best = game.Gathering.Catalog.InteractionRange;
            var result = "";
            var names = new[] { "дерево", "камень", "металл" };
            for (var i = 0; i < 3; i++)
            {
                var kind = (MapResourceKind)i;
                var distance = Vector2.Distance(economy.WorldPosition(kind), scene.PlayerMover.transform.position);
                if (economy.GetNode(kind).IsDepleted || distance > best) continue;
                best = distance;
                result = $"Удерживайте E · добывать {names[i]}";
            }
            return DebugPaused ? "Игра на паузе · продолжить в дебаг-меню" : result;
        }

        private static string PlacementHint(PlacementResult result)
        {
            switch (result)
            {
                case PlacementResult.Ready: return "ЛКМ · построить    Esc / ПКМ · отменить";
                case PlacementResult.CannotAfford: return "Недостаточно ресурсов · Esc / ПКМ отменяет строительство";
                case PlacementResult.Occupied: return "Клетка занята · выберите другую";
                case PlacementResult.HeroProtected: return "Отойдите от места строительства";
                case PlacementResult.EnemyProtected: return "Здесь проходит враг · выберите другую клетку";
                default: return "Выберите свободную клетку на серой дороге · Esc отменяет";
            }
        }

        private void DrawOutcome()
        {
            var r = OutcomeRect;
            Card(r);
            Fill(new Rect(r.x + 24f, r.y + 25f, 40f, 3f), Gold);
            Label(new Rect(r.x + 24f, r.y + 44f, r.width - 48f, 35f),
                game.RunDriver.Run.Phase == Stage2RunPhase.Won ? "ПОБЕДА" : "ПОРАЖЕНИЕ", number);
            Label(new Rect(r.x + 24f, r.y + 89f, r.width - 48f, 48f),
                game.RunDriver.Run.Phase == Stage2RunPhase.Won ? "Все волны отбиты. Трон под защитой." : "Трон пал. Попробуйте другую оборону.", text);
            if (GUI.Button(new Rect(r.x + 24f, r.y + 158f, r.width - 48f, 38f), "Новый забег  [R]", button)) RestartRun();
        }

        private void DrawDebug()
        {
            var r = DebugRect;
            Card(r);
            Label(new Rect(r.x + 18f, r.y + 13f, 270f, 28f), "ИНСТРУМЕНТЫ РАЗРАБОТКИ", title);
            if (GUI.Button(new Rect(r.xMax - 47f, r.y + 10f, 32f, 30f), "×", button)) SetDebugOpen(false);
            var view = new Rect(r.x + 14f, r.y + 52f, r.width - 28f, r.height - 64f);
            scroll = GUI.BeginScrollView(view, scroll, new Rect(0, 0, view.width - 20f, 508f));
            var w = view.width - 20f;
            Label(new Rect(4, 0, w, 20), "ЗАБЕГ", muted);
            GUI.enabled = !game.RunDriver.Run.IsTerminal;
            if (GUI.Button(new Rect(0, 26, w, 32), DebugPaused ? "Продолжить игру" : "Пауза", button)) SetDebugPaused(!DebugPaused);
            GUI.enabled = game.RunDriver.Run.Phase == Stage2RunPhase.Preparation && !game.RunDriver.Run.IsPaused;
            var waveLabel = game.RunDriver.Run.Phase == Stage2RunPhase.Wave ? "Волна уже идёт" :
                game.RunDriver.Run.IsTerminal ? "Забег завершён" : "Запустить следующую волну";
            if (GUI.Button(new Rect(0, 66, w, 32), waveLabel, button)) StartNextWave();
            GUI.enabled = true;
            DrawSpeed(new Rect(0, 114, w, 60), "Скорость игры  (×1–×10)", GameSpeed, SetGameSpeed);
            DrawSpeed(new Rect(0, 182, w, 60), "Скорость героя  (×1–×10)", HeroSpeed, SetHeroSpeed);
            Label(new Rect(0, 246, w, 30), "Скорость героя умножается на скорость игры.", muted);
            Label(new Rect(0, 282, w, 20), "ЗОЛОТО", muted);
            if (GUI.Button(new Rect(0, 306, w * .5f - 5f, 32), "+100", button)) AddGold(100);
            if (GUI.Button(new Rect(w * .5f + 5f, 306, w * .5f - 5f, 32), "+1 000", button)) AddGold(1000);
            Label(new Rect(0, 352, w, 20), "ДЕРЕВО + КАМЕНЬ + МЕТАЛЛ", muted);
            if (GUI.Button(new Rect(0, 376, w * .5f - 5f, 32), "+100 каждого", button)) AddAllResources(100);
            if (GUI.Button(new Rect(w * .5f + 5f, 376, w * .5f - 5f, 32), "+1 000 каждого", button)) AddAllResources(1000);
            if (GUI.Button(new Rect(0, 422, w, 34), "Перезапустить забег", button)) RestartRun();
            Label(new Rect(0, 466, w, 40), feedback, muted);
            GUI.EndScrollView();
        }

        private void DrawSpeed(Rect r, string caption, int value, Action<int> set)
        {
            Label(new Rect(r.x, r.y, r.width - 64f, 25f), caption, text);
            Label(new Rect(r.xMax - 64f, r.y, 64f, 25f), $"×{value}", title);
            var next = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(r.x + 4f, r.y + 37f, r.width - 8f, 22f), value, 1f, 10f));
            if (next != value) set(next);
        }

        private void EnsureStyles()
        {
            if (text != null) return;
            text = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, normal = { textColor = Light } };
            muted = new GUIStyle(text) { fontSize = 13, normal = { textColor = new Color(.68f, .75f, .73f) } };
            title = new GUIStyle(text) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Gold } };
            number = new GUIStyle(text) { fontSize = 26, fontStyle = FontStyle.Bold };
            centered = new GUIStyle(muted) { alignment = TextAnchor.MiddleCenter };
            buttonNormal = Solid(new Color(.16f, .23f, .24f));
            buttonHover = Solid(new Color(.25f, .34f, .33f));
            buttonActive = Solid(new Color(.33f, .39f, .30f));
            button = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, border = new RectOffset(0, 0, 0, 0),
                normal = { background = buttonNormal, textColor = Light },
                hover = { background = buttonHover, textColor = Color.white },
                active = { background = buttonActive, textColor = Gold } };
            smallButton = new GUIStyle(button) { fontSize = 14 };
            smallButtonText = new GUIStyle(text) { fontSize = 14, fontStyle = FontStyle.Bold, wordWrap = true };
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }

        private static void Card(Rect r)
        {
            Fill(new Rect(r.x + 3f, r.y + 4f, r.width, r.height), new Color(0, 0, 0, .22f));
            Fill(r, Edge);
            Fill(new Rect(r.x + 1f, r.y + 1f, r.width - 2f, r.height - 2f), Ink);
            Fill(new Rect(r.x + 1f, r.y + 1f, r.width - 2f, 2f), Gold * new Color(1, 1, 1, .75f));
        }

        private static void Fill(Rect r, Color color)
        {
            GUI.color = color; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white;
        }

        private static void Label(Rect r, string value, GUIStyle style) => GUI.Label(r, value, style);
    }
}
