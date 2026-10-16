using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Integration
{
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
        private GUIStyle title, text, muted, number, button, smallButton, centered;
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
        public float Scale => Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), .75f, 1.35f);
        private float Width => Screen.width / Scale;
        private float Height => Screen.height / Scale;
        public Rect DebugRect => new Rect(Width - 408f, 132f, 392f, Height - 148f);
        public Rect ResourcesRect => new Rect(16f, 16f, Mathf.Min(476f, Width * .55f - 24f), 70f);
        public Rect StatusRect => new Rect(Width - 320f, 16f, 304f, 98f);
        public Rect DebugButtonRect => new Rect(Width - 146f, 120f, 130f, 32f);
        public Rect OutcomeRect => new Rect((Width - 400f) * .5f, (Height - 220f) * .42f, 400f, 220f);
        public Rect HintRect => new Rect(BuildRect.x, BuildRect.y - 35f, BuildRect.width, 27f);
        public Rect BuildRect
        {
            get
            {
                var map = Stage2Minimap.Layout(new Vector2(Screen.width, Screen.height));
                var left = Mathf.Max(map.xMax / Scale + 16f, (Width - 520f) * .5f);
                return new Rect(left, Height - 122f, Mathf.Min(520f, Width - left - 16f), 106f);
            }
        }

        public void Initialize(Stage2SceneBootstrap bootstrap, Stage2GameComposition composition, bool developmentTools)
        {
            scene = bootstrap;
            game = composition;
            allowDebug = developmentTools;
        }

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
            if (!isActiveAndEnabled || game == null || !game.IsInitialized) return false;
            var point = new Vector2(screen.x, Screen.height - screen.y) / Scale;
            var run = game.RunDriver.Run;
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
            DrawResources();
            DrawStatus();
            if (game.RunDriver.Run.IsTerminal) DrawOutcome();
            else if (!DebugOpen) DrawBuild();
            if (DebugAvailable)
            {
                if (DebugOpen) DrawDebug();
                else if (GUI.Button(DebugButtonRect, "ДЕБАГ  ·  F3", smallButton)) SetDebugOpen(true);
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
            Label(new Rect(r.x + 14f, r.y + 10f, 150f, 23f), $"ВОЛНА  {run.WaveNumber} / {run.WaveCount}", title);
            Label(new Rect(r.x + 177f, r.y + 13f, 115f, 20f), $"ТРОН  {run.ThroneHealth}/{run.ThroneMaximum}", text);
            var health = new Rect(r.x + 177f, r.y + 37f, 110f, 4f);
            Fill(health, Edge);
            Fill(new Rect(health.x, health.y, health.width * run.ThroneHealth / run.ThroneMaximum, health.height), Gold);
            var state = DebugPaused || run.IsPaused ? "Пауза" : run.Phase == Stage2RunPhase.Preparation
                ? $"Подготовка · {Mathf.CeilToInt(run.PreparationRemaining)} сек."
                : run.Phase == Stage2RunPhase.Wave ? $"Врагов на карте: {run.ActiveEnemies}"
                : run.Phase == Stage2RunPhase.Won ? "Трон защищён" : "Трон разрушен";
            Label(new Rect(r.x + 14f, r.y + 35f, 160f, 20f), state, muted);
            GUI.enabled = run.Phase == Stage2RunPhase.Preparation && !run.IsPaused && !DebugPaused;
            if (GUI.Button(new Rect(r.x + 14f, r.y + 62f, 276f, 27f), "Начать волну  [Space]", smallButton)) StartNextWave();
            GUI.enabled = true;
        }

        private void DrawBuild()
        {
            var r = BuildRect;
            Card(r);
            var defense = game.Building.Defense;
            var selected = defense.SelectedCatalog;
            Label(new Rect(r.x + 14f, r.y + 9f, r.width - 28f, 23f),
                defense.IsPlacing ? $"РАЗМЕЩЕНИЕ · {selected.RoleName}" : "ОБОРОНА", title);
            var half = (r.width - 38f) * .5f;
            GUI.enabled = !DebugPaused && !game.RunDriver.Run.IsPaused;
            if (GUI.Button(new Rect(r.x + 14f, r.y + 38f, half, 28f), "1 · Rapid", smallButton)) game.Building.SelectAndBeginPlacement(0);
            if (GUI.Button(new Rect(r.x + 24f + half, r.y + 38f, half, 28f), "2 · Longshot", smallButton)) game.Building.SelectAndBeginPlacement(1);
            GUI.enabled = true;
            Label(new Rect(r.x + 14f, r.y + 74f, r.width - 28f, 22f),
                $"{selected.RoleName}: {selected.GoldCost} зол. · {selected.WoodCost} дер. · {selected.StoneCost} кам. · {selected.MetalCost} мет.", muted);
            var hint = defense.IsPlacing ? PlacementHint(game.Building.PreviewResult) : NearbyResourceHint();
            if (string.IsNullOrEmpty(hint)) hint = "WASD · движение    B · строить    Колесо · масштаб";
            var hintArea = HintRect;
            Fill(hintArea, Ink);
            Label(hintArea, hint, centered);
        }

        private string NearbyResourceHint()
        {
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
