using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Not3A.Stage1
{
    public sealed class Stage1GameController : MonoBehaviour
    {
        [SerializeField] private Stage1Hud hud;
        [SerializeField] private HealthComponent throne;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private BuildPlacement buildPlacement;
        [SerializeField, Min(1f)] private float preparationSeconds = 180f;
        [SerializeField, Min(1)] private int objectiveWoodTarget = 5;
        [SerializeField, Min(1)] private int objectiveStoneReward = 2;
        [SerializeField, Min(0)] private int startingWood;
        [SerializeField, Min(0)] private int startingStone;

        private ResourceInventory inventory;
        private GatherObjective objective;
        private RunStateMachine runState;
        private InputAction startWaveAction;
        private InputAction restartAction;
        private float preparationRemaining;
        private string contextHint;

        public RunPhase Phase => runState?.Phase ?? RunPhase.Preparation;
        public bool HasEnded => Phase == RunPhase.Won || Phase == RunPhase.Lost;
        public bool CanBuild => Phase == RunPhase.Preparation || Phase == RunPhase.Wave;
        public int Wood => inventory?.Wood ?? 0;
        public int Stone => inventory?.Stone ?? 0;
        public int ObjectiveProgress => objective?.Progress ?? 0;
        public bool ObjectiveComplete => objective?.IsComplete ?? false;
        public BuildPlacement BuildPlacement => buildPlacement;

        public void Configure(
            Stage1Hud stageHud,
            HealthComponent throneHealth,
            WaveSpawner spawner,
            BuildPlacement placement,
            float preparation,
            int woodTarget,
            int stoneReward,
            int initialWood,
            int initialStone)
        {
            hud = stageHud;
            throne = throneHealth;
            waveSpawner = spawner;
            buildPlacement = placement;
            preparationSeconds = preparation;
            objectiveWoodTarget = woodTarget;
            objectiveStoneReward = stoneReward;
            startingWood = Mathf.Max(0, initialWood);
            startingStone = Mathf.Max(0, initialStone);
        }

        private void Awake()
        {
            inventory = new ResourceInventory();
            if (startingWood > 0)
            {
                inventory.Add(ResourceKind.Wood, startingWood);
            }

            if (startingStone > 0)
            {
                inventory.Add(ResourceKind.Stone, startingStone);
            }

            objective = new GatherObjective(ResourceKind.Wood, objectiveWoodTarget);
            runState = new RunStateMachine();
            preparationRemaining = preparationSeconds;
            inventory.Changed += RefreshHud;
            if (throne != null)
            {
                throne.Changed += OnThroneChanged;
                throne.Died += OnThroneDied;
            }

            if (hud != null && hud.RestartButton != null)
            {
                hud.RestartButton.onClick.AddListener(RestartRun);
            }
        }

        private void Start()
        {
            hud.HideResult();
            SetContextHint("B — выбрать башню; SPACE — начать волну раньше");
            RefreshHud();
        }

        private void OnEnable()
        {
            startWaveAction = new InputAction("StartWave", InputActionType.Button, "<Keyboard>/space");
            restartAction = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
            startWaveAction.Enable();
            restartAction.Enable();
        }

        private void OnDisable()
        {
            startWaveAction?.Dispose();
            restartAction?.Dispose();
            startWaveAction = null;
            restartAction = null;
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= RefreshHud;
            }

            if (throne != null)
            {
                throne.Changed -= OnThroneChanged;
                throne.Died -= OnThroneDied;
            }

            if (hud != null && hud.RestartButton != null)
            {
                hud.RestartButton.onClick.RemoveListener(RestartRun);
            }
        }

        private void Update()
        {
            if (Phase == RunPhase.Preparation)
            {
                preparationRemaining = Mathf.Max(0f, preparationRemaining - Time.deltaTime);
                if (preparationRemaining <= 0f || (startWaveAction != null && startWaveAction.WasPressedThisFrame()))
                {
                    StartWave();
                }
            }

            if (HasEnded && restartAction != null && restartAction.WasPressedThisFrame())
            {
                RestartRun();
            }

            RefreshPhaseText();
        }

        public void CollectResource(ResourceKind kind, int amount)
        {
            if (HasEnded || amount <= 0)
            {
                return;
            }

            inventory.Add(kind, amount);
            if (objective.RecordCollection(kind, amount))
            {
                inventory.Add(ResourceKind.Stone, objectiveStoneReward);
                SetContextHint($"Задача выполнена: +{objectiveStoneReward} камня");
            }

            RefreshHud();
        }

        public bool CanAffordTower(int wood, int stone) => inventory != null && inventory.CanAfford(wood, stone);
        public bool TrySpendTowerCost(int wood, int stone) => inventory != null && inventory.TrySpend(wood, stone);

        public void StartWave()
        {
            if (!runState.StartWave())
            {
                return;
            }

            waveSpawner.BeginWave();
            SetContextHint("Волна идёт: строительство по-прежнему доступно");
            RefreshHud();
        }

        public void NotifyWaveCleared()
        {
            if (!runState.Win())
            {
                return;
            }

            buildPlacement.CancelPlacement();
            hud.ShowResult("ПОБЕДА\nТрон защищён\nR — начать заново");
            RefreshHud();
        }

        public void RestartRun()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void SetContextHint(string hint)
        {
            contextHint = hint;
            hud?.SetContext(contextHint);
        }

        private void OnThroneChanged(HealthComponent health)
        {
            hud?.SetThrone(health.Current, health.Maximum);
        }

        private void OnThroneDied(HealthComponent _)
        {
            if (!runState.Lose())
            {
                return;
            }

            waveSpawner.StopAndClear();
            buildPlacement.CancelPlacement();
            hud.ShowResult("ПОРАЖЕНИЕ\nТрон разрушен\nR — начать заново");
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (hud == null || inventory == null || objective == null || runState == null)
            {
                return;
            }

            hud.SetResources(inventory.Wood, inventory.Stone);
            hud.SetObjective(objective.IsComplete
                ? $"Задача: добыть {objectiveWoodTarget} дерева — выполнено (+{objectiveStoneReward} камня)"
                : $"Задача: добыть {objectiveWoodTarget} дерева ({objective.Progress}/{objectiveWoodTarget}) → +{objectiveStoneReward} камня");
            if (throne != null)
            {
                hud.SetThrone(throne.Current, throne.Maximum);
            }

            hud.SetContext(contextHint);
            RefreshPhaseText();
        }

        private void RefreshPhaseText()
        {
            if (hud == null || runState == null)
            {
                return;
            }

            switch (Phase)
            {
                case RunPhase.Preparation:
                    var seconds = Mathf.CeilToInt(preparationRemaining);
                    hud.SetPhase($"Подготовка  {seconds / 60:00}:{seconds % 60:00}   SPACE — начать волну");
                    break;
                case RunPhase.Wave:
                    hud.SetPhase($"Волна 1/1   Врагов на карте: {waveSpawner.ActiveCount}");
                    break;
                case RunPhase.Won:
                    hud.SetPhase("Финальная волна отбита");
                    break;
                case RunPhase.Lost:
                    hud.SetPhase("Трон разрушен");
                    break;
            }
        }
    }
}
