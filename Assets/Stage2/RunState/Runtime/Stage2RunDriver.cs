using System;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.WavesAI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.RunState
{
    [DisallowMultipleComponent]
    public sealed class Stage2RunDriver : MonoBehaviour
    {
        [SerializeField] private Stage2RunCatalog catalog;
        [SerializeField] private Stage2GatheringDriver gathering;
        [SerializeField] private Stage2BuildDriver building;
        [SerializeField] private Transform player;

        private Stage2Navigation navigation;
        private Stage2EnemyGroup enemies;
        private Stage2Run run;
        private Vector3 initialPlayerPosition;
        private InputAction startWaveAction;
        private InputAction restartAction;

        public Stage2Run Run => run;

        public bool Configure(Stage2RunCatalog data, Stage2GatheringDriver economyDriver,
            Stage2BuildDriver buildDriver, Transform playerTransform,
            Stage2Navigation sharedNavigation, Stage2EnemyGroup enemyGroup)
        {
            catalog = data;
            gathering = economyDriver;
            building = buildDriver;
            player = playerTransform;
            navigation = sharedNavigation;
            enemies = enemyGroup;
            initialPlayerPosition = player != null ? player.position : Vector3.zero;
            return Initialize();
        }

        public bool Initialize()
        {
            if (catalog == null || gathering == null || gathering.Economy == null ||
                building == null || building.Defense == null || player == null ||
                navigation == null || enemies == null)
            {
                Debug.LogError("S2_RUN_BINDING_ERROR: catalog, initialized Economy/Defense, hero, shared Navigation and enemy group are required.", this);
                return false;
            }
            try
            {
                if (run != null)
                {
                    run.Changed -= SyncConsumers;
                    run.Dispose();
                }
                run = new Stage2Run(catalog, navigation, gathering.Economy,
                    building.Defense, enemies);
                run.Changed += SyncConsumers;
                SyncConsumers();
                return true;
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"S2_RUN_BINDING_ERROR: {exception.Message}", this);
                return false;
            }
        }

        public void SetPaused(bool value) => run?.SetPaused(value);

        public void ResetRun()
        {
            if (run == null) return;
            run.ResetRun();
            gathering.ResetRun(); // Also latches held E until release in the new run.
            player.position = initialPlayerPosition;
            SyncConsumers();
        }

        private void OnEnable()
        {
            // Retain Stage 1's Space/R actions; pause is an explicit UI/run-state call.
            startWaveAction = new InputAction("Stage 2 Start Wave", InputActionType.Button, "<Keyboard>/space");
            restartAction = new InputAction("Stage 2 Restart", InputActionType.Button, "<Keyboard>/r");
            startWaveAction.Enable();
            restartAction.Enable();
            run?.SetPaused(false);
        }

        private void OnDisable()
        {
            run?.SetPaused(true);
            startWaveAction?.Dispose();
            restartAction?.Dispose();
            startWaveAction = restartAction = null;
        }

        private void OnDestroy()
        {
            if (run == null) return;
            run.Changed -= SyncConsumers;
            run.Dispose();
        }

        private void Update()
        {
            if (run == null) return;
            if (run.IsTerminal && restartAction != null && restartAction.WasPressedThisFrame())
            {
                ResetRun();
                return;
            }
            if (!run.ContinuousDays && startWaveAction != null && startWaveAction.WasPressedThisFrame()) run.StartWave();
            run.Tick(Time.deltaTime);
        }

        private void SyncConsumers()
        {
            if (run == null) return;
            var active = !run.IsTerminal;
            gathering.SetRunActivity(active, run.IsPaused);
            var phase = run.Phase == Stage2RunPhase.Preparation ? Stage2BuildPhase.Preparation :
                run.Phase == Stage2RunPhase.Wave ? Stage2BuildPhase.Wave : Stage2BuildPhase.Inactive;
            building.SetRunState(phase, run.IsPaused);
        }
    }
}
