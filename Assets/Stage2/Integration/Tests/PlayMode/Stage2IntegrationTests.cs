using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Not3A.Stage1;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.Navigation;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Integration.Tests
{
    public sealed class Stage2IntegrationTests
    {
        private const string SceneName = "Stage2VerticalSlice";
        private InputTestFixture input;
        private Keyboard testKeyboard;

        [UnitySetUp]
        public IEnumerator LoadStage2Scene()
        {
            // UnitySetUp runs before NUnit SetUp in this Test Framework version.
            // Create the isolated input system before scene OnEnable binds actions.
            input = new InputTestFixture();
            input.Setup();
            testKeyboard = InputSystem.AddDevice<Keyboard>();
            SceneManager.LoadScene(SceneName);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ReleaseSceneInput()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Object.Destroy(root);
            }

            yield return null;
            input?.TearDown();
            input = null;
        }

        [UnityTest]
        public IEnumerator Scene_LoadsWithExplicitValidMapAndSharedGameplaySystems()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.IsInitialized, Is.True, bootstrap.LastValidation.Reason);
            Assert.That(bootstrap.MapDefinition, Is.Not.Null);
            Assert.That(bootstrap.MapDefinition.Size, Is.EqualTo(new Vector2Int(32, 32)));
            Assert.That(bootstrap.MapDefinition.ValidateDefinition().IsValid, Is.True);
            Assert.That(bootstrap.MapDebugRenderer.MapDefinition, Is.SameAs(bootstrap.MapDefinition));
            Assert.That(bootstrap.PlayerMover.MapDefinition, Is.SameAs(bootstrap.MapDefinition));
            Assert.That(bootstrap.CameraFollow.MapDefinition, Is.SameAs(bootstrap.MapDefinition));
            Assert.That(bootstrap.AuthoredMapHash, Is.EqualTo(bootstrap.MapDefinition.ComputeDeterministicContentHash()));
            var camera = bootstrap.CameraFollow.BoundCamera;
            foreach (var landmark in new[] { bootstrap.EntranceMarker, bootstrap.ThroneMarker })
            {
                var viewport = camera.WorldToViewportPoint(landmark.position);
                Assert.That(viewport.x, Is.InRange(0.05f, 0.95f), $"{landmark.name} must be visible at the start.");
                Assert.That(viewport.y, Is.InRange(0.05f, 0.95f), $"{landmark.name} must be visible at the start.");
            }
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            Assert.That(composition, Is.Not.Null);
            Assert.That(composition.IsInitialized, Is.True);
            Assert.That(composition.Navigation, Is.Not.Null);
            Assert.That(composition.Navigation.MapHash, Is.EqualTo(bootstrap.AuthoredMapHash));
            Assert.That(composition.Gathering.MapDefinition, Is.SameAs(bootstrap.MapDefinition));
            Assert.That(composition.Gathering.Player, Is.SameAs(bootstrap.PlayerMover.transform));
            Assert.That(composition.Building.Defense, Is.Not.Null);
            Assert.That(composition.Enemies, Is.Not.Null);
            Assert.That(composition.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(composition.Gathering.Economy.Inventory, Is.Not.Null);
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Has.None.Null, "Scene must not contain missing scripts.");
            }

            Assert.That(Object.FindAnyObjectByType<Stage1GameController>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<GatherableNode>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<TowerController>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<EnemyController>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<WaveSpawner>(), Is.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AuthoredResourceMarkers_HideAtDepletionAndReturnOnRecoveryAndRestart()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var map = Object.FindAnyObjectByType<Stage2SceneBootstrap>().MapDefinition;
            var economy = composition.Gathering.Economy;
            var colors = new Color[3];
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var marker = composition.transform.Find($"{kind} Resource Marker");
                Assert.That(marker, Is.Not.Null, $"{kind} must be visible in the scene.");
                Assert.That(Vector2.Distance(marker.position, map.CellToWorld(economy.GetNode(kind).Cell)),
                    Is.LessThan(0.001f));
                var symbol = marker.Find("Source Symbol").GetComponent<SpriteRenderer>();
                Assert.That(symbol.sprite, Is.Not.Null);
                Assert.That(symbol.enabled, Is.True);
                Assert.That(marker.GetComponentInChildren<Collider2D>(), Is.Null);
                colors[(int)kind] = symbol.color;
                var node = economy.GetNode(kind);
                for (var unit = 0; unit < node.Capacity; unit++)
                    Assert.That(economy.AdvanceGather(kind, economy.WorldPosition(kind), true,
                        node.GatherSeconds, true), Is.True);
            }
            Assert.That(colors[0], Is.Not.EqualTo(colors[1]));
            Assert.That(colors[1], Is.Not.EqualTo(colors[2]));
            yield return null;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                Assert.That(composition.transform.Find($"{kind} Resource Marker").gameObject.activeSelf,
                    Is.False, $"{kind} marker must disappear when its source is empty.");

            var wood = economy.GetNode(MapResourceKind.Wood);
            economy.Tick(wood.RecoverySeconds, true);
            yield return null;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                Assert.That(composition.transform.Find($"{kind} Resource Marker").gameObject.activeSelf,
                    Is.True, $"{kind} marker must return when its source recovers.");

            var stone = economy.GetNode(MapResourceKind.Stone);
            for (var unit = 0; unit < stone.Capacity; unit++)
                Assert.That(economy.AdvanceGather(MapResourceKind.Stone,
                    economy.WorldPosition(MapResourceKind.Stone), true, stone.GatherSeconds, true), Is.True);
            yield return null;
            Assert.That(composition.transform.Find("Stone Resource Marker").gameObject.activeSelf, Is.False);
            composition.RunDriver.ResetRun();
            yield return null;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                Assert.That(composition.transform.Find($"{kind} Resource Marker").gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator MetalSource_GathersThroughHeldEInActiveRun()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var economy = composition.Gathering.Economy;
            var metal = economy.GetNode(MapResourceKind.Metal);
            var player = Object.FindAnyObjectByType<Stage2SceneBootstrap>().PlayerMover.transform;
            Assert.That(composition.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            player.position = economy.WorldPosition(MapResourceKind.Metal);
            yield return null; // Clear the input-release latch after scene initialization.

            input.Press(testKeyboard.eKey);
            yield return new WaitForSeconds(metal.GatherSeconds + 0.2f);
            input.Release(testKeyboard.eKey);
            yield return null;

            Assert.That(economy.Inventory.Get(MapResourceKind.Metal), Is.EqualTo(1));
            Assert.That(metal.Remaining, Is.EqualTo(metal.Capacity - 1));
        }

        [UnityTest]
        public IEnumerator RealMap_GatherPayBuildRouteWaveAndRestart()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var map = Object.FindAnyObjectByType<Stage2SceneBootstrap>().MapDefinition;
            var economy = composition.Gathering.Economy;
            var defense = composition.Building.Defense;
            var run = composition.RunDriver.Run;
            var routeCell = new Vector2Int(-13, -12);
            var open = composition.Navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(open.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(open.Path, Does.Contain(routeCell));

            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                Assert.That(map.TryGetResourceRegion(kind, out var region), Is.True);
                Assert.That(economy.GetNode(kind).Cell, Is.EqualTo(region.Anchor));
                Assert.That(economy.AdvanceGather(kind, economy.WorldPosition(kind), true,
                    economy.GetNode(kind).GatherSeconds, true), Is.True);
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(1));
            }

            Assert.That(run.PreparationRemaining, Is.InRange(179f, 180f));
            Assert.That(economy.Inventory.Gold, Is.EqualTo(5));
            Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                economy.WorldPosition(MapResourceKind.Wood), true,
                economy.GetNode(MapResourceKind.Wood).GatherSeconds, true), Is.True);

            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(routeCell);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(tower, Is.Not.Null);
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(composition.Navigation.TryGetOccupant(routeCell, out var owner), Is.True);
            Assert.That(owner, Is.SameAs(tower));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(economy.Inventory.Get(MapResourceKind.Stone), Is.Zero);
            Assert.That(economy.Inventory.Get(MapResourceKind.Metal), Is.EqualTo(1));
            Assert.That(economy.Inventory.Gold, Is.Zero);
            var diverted = composition.Navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(diverted.Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            Assert.That(diverted.Path.Contains(routeCell), Is.False);
            Assert.That(diverted.Path.Count, Is.GreaterThan(open.Path.Count));

            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            Assert.That(composition.Enemies.ActiveCount, Is.EqualTo(1));
            composition.RunDriver.ResetRun();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(composition.Enemies.ActiveCount, Is.Zero);
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(economy.Inventory.Gold, Is.EqualTo(5));
            Assert.That(composition.Navigation.PlanFrom(map.EnemyEntrance).Path.Count, Is.EqualTo(open.Path.Count));
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealMap_LongshotRequiresItsPriceAndDivertsRoute()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var map = Object.FindAnyObjectByType<Stage2SceneBootstrap>().MapDefinition;
            var economy = composition.Gathering.Economy;
            var defense = composition.Building.Defense;
            var cell = new Vector2Int(-13, -12);
            var before = composition.Navigation.PlanFrom(map.EnemyEntrance);
            Assert.That(defense.RoleCount, Is.EqualTo(2));
            Assert.That(defense.SelectRole(1), Is.True);
            var longshot = defense.SelectedCatalog;
            Assert.That(longshot.RoleName, Is.EqualTo("Longshot"));
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                Assert.That(economy.AdvanceGather(kind, economy.WorldPosition(kind), true,
                    economy.GetNode(kind).GatherSeconds, true), Is.True);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(cell);
            Assert.That(defense.EvaluatePreview(), Is.EqualTo(PlacementResult.CannotAfford));
            Assert.That(composition.Navigation.Revision, Is.Zero);
            Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                economy.WorldPosition(MapResourceKind.Wood), true,
                economy.GetNode(MapResourceKind.Wood).GatherSeconds, true), Is.True);
            for (var i = 0; i < 2; i++)
                Assert.That(economy.AdvanceGather(MapResourceKind.Stone,
                    economy.WorldPosition(MapResourceKind.Stone), true,
                    economy.GetNode(MapResourceKind.Stone).GatherSeconds, true), Is.True);
            economy.Tick(economy.GetNode(MapResourceKind.Stone).RecoverySeconds, true);
            for (var i = 0; i < 2; i++)
                Assert.That(economy.AdvanceGather(MapResourceKind.Stone,
                    economy.WorldPosition(MapResourceKind.Stone), true,
                    economy.GetNode(MapResourceKind.Stone).GatherSeconds, true), Is.True);
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(tower.name, Does.StartWith(longshot.TowerPrefab.name));
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(economy.Inventory.Get(MapResourceKind.Stone), Is.Zero);
            Assert.That(economy.Inventory.Get(MapResourceKind.Metal), Is.EqualTo(1));
            Assert.That(composition.Navigation.PlanFrom(map.EnemyEntrance).Path.Count,
                Is.GreaterThan(before.Path.Count));
            composition.RunDriver.ResetRun();
            Assert.That(defense.SelectedRoleIndex, Is.Zero);
            Assert.That(composition.Navigation.OccupiedCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealMap_OneRapidKeepsFiringAcrossFirstWave()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var economy = composition.Gathering.Economy;
            var defense = composition.Building.Defense;
            var run = composition.RunDriver.Run;
            economy.Inventory.TryAdd(MapResourceKind.Wood, 2);
            economy.Inventory.TryAdd(MapResourceKind.Stone, 1);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(new Vector2Int(-13, -12));
            Assert.That(defense.TryConfirm(out var rapid), Is.EqualTo(PlacementResult.Ready));
            Assert.That(run.StartWave(), Is.True);
            for (var step = 0; step < 80 && !run.IsTerminal; step++)
            {
                run.Tick(0.2f);
                for (var enemy = composition.Enemies.ActiveCount - 1; enemy >= 0; enemy--)
                    composition.Enemies.GetEnemy(enemy).Tick(0.2f);
                if (rapid.IsAlive) rapid.Tick(0.2f);
            }
            Assert.That(economy.Inventory.Gold, Is.GreaterThanOrEqualTo(2),
                "A surviving Rapid at the entrance should repeatedly attack across the first wave.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealMap_PaidTowersDefeatAllWavesAndRunCanRestart()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var economy = composition.Gathering.Economy;
            var defense = composition.Building.Defense;
            var run = composition.RunDriver.Run;
            var towers = new Stage2Tower[3];
            for (var index = 0; index < 2; index++)
            {
                Assert.That(defense.SelectRole(index == 1 ? 1 : 0), Is.True);
                foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                    Assert.That(economy.AdvanceGather(kind, economy.WorldPosition(kind), true,
                        economy.GetNode(kind).GatherSeconds, true), Is.True);
                if (index != 1)
                    Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                        economy.WorldPosition(MapResourceKind.Wood), true,
                        economy.GetNode(MapResourceKind.Wood).GatherSeconds, true), Is.True);
                if (index == 1)
                {
                    economy.Tick(economy.GetNode(MapResourceKind.Wood).RecoverySeconds, true);
                    Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                        economy.WorldPosition(MapResourceKind.Wood), true,
                        economy.GetNode(MapResourceKind.Wood).GatherSeconds, true), Is.True);
                    Assert.That(economy.AdvanceGather(MapResourceKind.Stone,
                        economy.WorldPosition(MapResourceKind.Stone), true,
                        economy.GetNode(MapResourceKind.Stone).GatherSeconds, true), Is.True);
                    economy.Tick(economy.GetNode(MapResourceKind.Stone).RecoverySeconds, true);
                    for (var i = 0; i < 3; i++)
                        Assert.That(economy.AdvanceGather(MapResourceKind.Stone,
                            economy.WorldPosition(MapResourceKind.Stone), true,
                            economy.GetNode(MapResourceKind.Stone).GatherSeconds, true), Is.True);
                }
                Assert.That(defense.BeginPlacement(), Is.True);
                defense.SetPreviewCell(new Vector2Int(-13 + index * 3, -12));
                Assert.That(defense.TryConfirm(out towers[index]), Is.EqualTo(PlacementResult.Ready));
            }
            Assert.That(towers[0].name, Does.Contain("Blocker"));
            Assert.That(towers[1].name, Does.Contain("Longshot"));
            Assert.That(economy.Inventory.Gold, Is.Zero);

            Assert.That(run.WaveCount, Is.EqualTo(3));
            for (var wave = 1; wave <= run.WaveCount; wave++)
            {
                Assert.That(run.WaveNumber, Is.EqualTo(wave));
                Assert.That(run.StartWave(), Is.True);
                for (var step = 0; step < 400 && run.Phase == Stage2RunPhase.Wave; step++)
                {
                    run.Tick(0.2f);
                    for (var index = composition.Enemies.ActiveCount - 1; index >= 0; index--)
                        composition.Enemies.GetEnemy(index).Tick(0.2f);
                    foreach (var tower in towers) if (tower != null && tower.IsAlive) tower.Tick(0.2f);
                }
                if (wave == 1)
                {
                    Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
                    Assert.That(economy.Inventory.Gold, Is.GreaterThanOrEqualTo(5));
                    economy.Tick(economy.GetNode(MapResourceKind.Wood).RecoverySeconds, true);
                    Assert.That(defense.SelectRole(0), Is.True);
                    for (var unit = 0; unit < 2; unit++)
                        Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                            economy.WorldPosition(MapResourceKind.Wood), true,
                            economy.GetNode(MapResourceKind.Wood).GatherSeconds, true), Is.True);
                    Assert.That(economy.AdvanceGather(MapResourceKind.Stone,
                        economy.WorldPosition(MapResourceKind.Stone), true,
                        economy.GetNode(MapResourceKind.Stone).GatherSeconds, true), Is.True);
                    Assert.That(defense.BeginPlacement(), Is.True);
                    defense.SetPreviewCell(new Vector2Int(-7, -12));
                    Assert.That(defense.TryConfirm(out towers[2]), Is.EqualTo(PlacementResult.Ready));
                    Assert.That(economy.Inventory.Gold, Is.EqualTo(1));
                }
                if (wave < run.WaveCount)
                {
                    Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
                    Assert.That(defense.TowerCount, Is.EqualTo(3));
                    Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
                }
            }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Won),
                "The authored map must allow a paid defense to finish the complete sequence.");
            Assert.That(run.ThroneHealth, Is.GreaterThan(0));
            composition.RunDriver.ResetRun();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.WaveNumber, Is.EqualTo(1));
            Assert.That(composition.Enemies.ActiveCount, Is.Zero);
            Assert.That(defense.TowerCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealMap_UnopposedWaveDamagesThroneAndEndsInLoss()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var run = composition.RunDriver.Run;
            Assert.That(run.StartWave(), Is.True);
            for (var step = 0; step < 250 && !run.IsTerminal; step++)
            {
                run.Tick(0.5f);
                for (var index = composition.Enemies.ActiveCount - 1; index >= 0; index--)
                    composition.Enemies.GetEnemy(index).Tick(0.5f);
            }
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Lost));
            Assert.That(run.ThroneHealth, Is.Zero);
            composition.RunDriver.ResetRun();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.ThroneHealth, Is.EqualTo(run.ThroneMaximum));
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealMap_FullWallForcesSiegeAndDestructionReopensRoute()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var map = Object.FindAnyObjectByType<Stage2SceneBootstrap>().MapDefinition;
            var economy = composition.Gathering.Economy;
            var defense = composition.Building.Defense;
            var navigation = composition.Navigation;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
                Assert.That(economy.AdvanceGather(kind, economy.WorldPosition(kind), true,
                    economy.GetNode(kind).GatherSeconds, true), Is.True);
            Assert.That(economy.AdvanceGather(MapResourceKind.Wood,
                economy.WorldPosition(MapResourceKind.Wood), true,
                economy.GetNode(MapResourceKind.Wood).GatherSeconds, true), Is.True);
            Assert.That(defense.BeginPlacement(), Is.True);
            var breachCell = new Vector2Int(0, -12);
            defense.SetPreviewCell(breachCell);
            Assert.That(defense.TryConfirm(out var blocker), Is.EqualTo(PlacementResult.Ready));
            for (var y = map.MinimumCell.y; y <= map.MaximumCell.y; y++)
            {
                if (y == breachCell.y) continue;
                Assert.That(navigation.TryOccupy(new Vector2Int(0, y), new object()),
                    Is.EqualTo(OccupancyResult.Changed));
            }

            var fromEast = new Vector2Int(1, -12);
            var blocked = navigation.PlanFrom(fromEast);
            Assert.That(blocked.Status, Is.EqualTo(NavigationStatus.ReachableBlocker));
            Assert.That(blocked.BlockerCell, Is.EqualTo(breachCell));
            Assert.That(composition.RunDriver.Run.StartWave(), Is.True);
            defense.SetAttackActive(false); // Isolate siege from the tower's offensive fire.
            var siegeEnemy = composition.Enemies.SpawnAt(fromEast);
            for (var step = 0; step < 30 && blocker.IsAlive; step++) siegeEnemy.Tick(0.5f);
            Assert.That(blocker.IsAlive, Is.False);
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(navigation.TryGetOccupant(breachCell, out _), Is.False);
            Assert.That(navigation.PlanFrom(fromEast).Status, Is.EqualTo(NavigationStatus.ReachesThrone));
            composition.RunDriver.ResetRun();
            yield return null;
        }

        [UnityTest]
        public IEnumerator DebugRenderer_UsesAuthoredForestAndCorridorColors()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var renderer = bootstrap.MapDebugRenderer;
            var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh, Is.Not.Null);
            Assert.That(mesh.vertexCount, Is.EqualTo(32 * 32 * 4));

            var colors = mesh.colors32;
            var forest = (Color32)bootstrap.MapDefinition.ForestDebugColor;
            var corridor = (Color32)bootstrap.MapDefinition.CorridorDebugColor;
            Assert.That(forest, Is.Not.EqualTo(corridor));
            Assert.That(Array.Exists(colors, color => color.Equals(forest)), Is.True);
            Assert.That(Array.Exists(colors, color => color.Equals(corridor)), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EntranceAndThrone_AreDistinctAuthoredLandmarks()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var definition = bootstrap.MapDefinition;
            Assert.That(Vector2.Distance(bootstrap.EntranceMarker.position, definition.CellToWorld(definition.EnemyEntrance)), Is.LessThan(0.001f));
            Assert.That(Vector2.Distance(bootstrap.ThroneMarker.position, definition.CellToWorld(definition.Throne)), Is.LessThan(0.001f));
            Assert.That(bootstrap.EntranceMarker, Is.Not.SameAs(bootstrap.ThroneMarker));

            var entranceVisual = bootstrap.EntranceMarker.GetComponentInChildren<SpriteRenderer>();
            var throneVisual = bootstrap.ThroneMarker.GetComponentInChildren<SpriteRenderer>();
            Assert.That(entranceVisual, Is.Not.Null);
            Assert.That(throneVisual, Is.Not.Null);
            Assert.That(entranceVisual.color, Is.Not.EqualTo(throneVisual.color));
            Assert.That(entranceVisual.bounds.size, Is.Not.EqualTo(throneVisual.bounds.size));
            yield return null;
        }

        [UnityTest]
        public IEnumerator InputSystem_WasdMovesNonCombatPlayerWithoutMutatingMap()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var mover = bootstrap.PlayerMover;
            var player = mover.transform;
            var initialHash = bootstrap.MapDefinition.ComputeDeterministicContentHash();
            Assert.That(mover.enabled, Is.True);

            yield return AssertKeyMoves(player, testKeyboard.wKey, Vector2.up);
            yield return AssertKeyMoves(player, testKeyboard.aKey, Vector2.left);
            yield return AssertKeyMoves(player, testKeyboard.sKey, Vector2.down);
            yield return AssertKeyMoves(player, testKeyboard.dKey, Vector2.right);
            Assert.That(player.GetComponent<HealthComponent>(), Is.Null, "The Stage 2 shell must not give the hero a combat role.");
            Assert.That(bootstrap.MapDefinition.ComputeDeterministicContentHash(), Is.EqualTo(initialHash), "Runtime movement must not mutate authored map data.");
        }

        [UnityTest]
        public IEnumerator Player_RemainsInsideExtremeAuthoredCellBounds()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var definition = bootstrap.MapDefinition;
            var player = bootstrap.PlayerMover.transform;

            foreach (var corner in OutsideCorners(definition.CellBounds))
            {
                player.position = definition.CellToWorld(corner);
                yield return null;
                AssertInside(definition.CellBounds, IsoGrid.WorldToCellCoordinates(player.position));
            }
        }

        [UnityTest]
        public IEnumerator Camera_FollowsPlayerAndRemainsInsideAuthoredCameraBounds()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var definition = bootstrap.MapDefinition;
            var camera = bootstrap.CameraFollow.BoundCamera;
            var player = bootstrap.PlayerMover.transform;
            Assert.That(camera, Is.SameAs(Camera.main));
            Assert.That(camera.orthographic, Is.True);
            Assert.That(camera.transform.rotation, Is.EqualTo(Quaternion.identity));

            foreach (var corner in OutsideCorners(definition.CameraBounds))
            {
                var before = (Vector2)camera.transform.position;
                player.position = definition.CellToWorld(corner);
                var expected = IsoGrid.ClampWorldToCellBounds(
                    player.position, definition.CameraBounds.Minimum, definition.CameraBounds.MaximumInclusive);
                yield return null;
                yield return null;
                Assert.That(Vector2.Distance(camera.transform.position, expected), Is.LessThan(Vector2.Distance(before, expected)), "Camera must actually follow, not merely remain stationary in bounds.");
                AssertInside(definition.CameraBounds, IsoGrid.WorldToCellCoordinates(camera.transform.position));
                Assert.That(camera.transform.rotation, Is.EqualTo(Quaternion.identity));
            }

            camera.transform.position = new Vector3(1000f, 1000f, -10f);
            yield return null;
            yield return null;
            AssertInside(definition.CameraBounds, IsoGrid.WorldToCellCoordinates(camera.transform.position));
        }

        [UnityTest]
        public IEnumerator MissingMapReference_ReportsExplicitCodeAndReason()
        {
            var root = new GameObject("Missing Map Validation Probe");
            root.SetActive(false);
            var bootstrap = root.AddComponent<Stage2SceneBootstrap>();
            LogAssert.Expect(
                LogType.Error,
                new Regex("S2_INT_VALIDATION_ERROR code=MissingMapReference reason=The Stage 2 map definition serialized reference is missing\\."));

            root.SetActive(true);
            Assert.That(bootstrap.IsInitialized, Is.False);
            Assert.That(bootstrap.LastValidation.Code, Is.EqualTo("MissingMapReference"));
            Assert.That(bootstrap.LastValidation.Reason, Is.Not.Empty);
            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InvalidMapDefinition_ReportsMapValidationCodeAndReason()
        {
            var invalidDefinition = ScriptableObject.CreateInstance<Stage2MapDefinition>();
            JsonUtility.FromJsonOverwrite("{\"cellBounds\":{\"size\":{\"x\":0,\"y\":0}}}", invalidDefinition);

            var root = new GameObject("Invalid Map Validation Probe");
            root.SetActive(false);
            var bootstrap = root.AddComponent<Stage2SceneBootstrap>();
            bootstrap.Configure(invalidDefinition, null, null, null, null, null);
            LogAssert.Expect(
                LogType.Error,
                new Regex("S2_INT_VALIDATION_ERROR code=MapDefinition\\.InvalidCellBounds reason=Cell bounds size must be positive\\."));

            root.SetActive(true);
            Assert.That(bootstrap.IsInitialized, Is.False);
            Assert.That(bootstrap.LastValidation.Code, Is.EqualTo("MapDefinition.InvalidCellBounds"));
            Assert.That(bootstrap.LastValidation.Reason, Does.Contain("positive"));
            Object.Destroy(root);
            Object.Destroy(invalidDefinition);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChangedDefinition_DrivesRendererPlayerAndIndependentCameraBounds()
        {
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var original = bootstrap.MapDefinition;
            var originalHash = original.ComputeDeterministicContentHash();
            var changed = Object.Instantiate(original);
            // Modify only a transient copy, never the authored candidate on disk.
            JsonUtility.FromJsonOverwrite(
                "{\"cellBounds\":{\"minimum\":{\"x\":-18,\"y\":-18},\"size\":{\"x\":36,\"y\":36}}," +
                "\"cameraBounds\":{\"minimum\":{\"x\":-16,\"y\":-16},\"size\":{\"x\":30,\"y\":30}}," +
                "\"forestDebugColor\":{\"r\":0.1,\"g\":0.5,\"b\":0.2,\"a\":1}}", changed);
            try
            {
                Assert.That(changed.ValidateDefinition().IsValid, Is.True);
                BindMap(bootstrap, changed);
                Assert.That(bootstrap.Initialize(), Is.True);
                var mesh = bootstrap.MapDebugRenderer.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(mesh.vertexCount, Is.EqualTo(changed.Size.x * changed.Size.y * 4));
                Assert.That(Array.Exists(mesh.colors32, color => color.Equals((Color32)changed.ForestDebugColor)), Is.True);

                bootstrap.PlayerMover.transform.position = changed.CellToWorld(new Vector2Int(-100, -100));
                yield return null;
                yield return null;
                Assert.That(Vector2.Distance(bootstrap.PlayerMover.transform.position, changed.CellToWorld(changed.MinimumCell)), Is.LessThan(0.001f));
                bootstrap.CameraFollow.SnapToTarget();
                Assert.That(Vector2.Distance(bootstrap.CameraFollow.transform.position, changed.CellToWorld(changed.CameraBounds.Minimum)), Is.LessThan(0.001f));
                Assert.That(original.ComputeDeterministicContentHash(), Is.EqualTo(originalHash));
                Assert.That(changed.ComputeDeterministicContentHash(), Is.EqualTo(bootstrap.AuthoredMapHash));
            }
            finally
            {
                BindMap(bootstrap, original);
                Object.Destroy(changed);
            }
        }

        private static void BindMap(Stage2SceneBootstrap bootstrap, Stage2MapDefinition definition)
        {
            bootstrap.MapDebugRenderer.Configure(definition);
            bootstrap.PlayerMover.Configure(definition, bootstrap.PlayerMover.Speed);
            bootstrap.CameraFollow.Configure(bootstrap.PlayerMover.transform, definition, 7f);
            bootstrap.Configure(definition, bootstrap.MapDebugRenderer, bootstrap.PlayerMover,
                bootstrap.CameraFollow, bootstrap.EntranceMarker, bootstrap.ThroneMarker);
        }

        private IEnumerator AssertKeyMoves(Transform player, ButtonControl key, Vector2 direction)
        {
            var before = (Vector2)player.position;
            input.Press(key);
            yield return null;
            yield return null;
            input.Release(key);
            yield return null;
            var delta = (Vector2)player.position - before;
            Assert.That(Vector2.Dot(delta, direction), Is.GreaterThan(0f), $"{key.name} must move the non-combat marker through the Input System.");
            Assert.That(Mathf.Abs(Vector2.Dot(delta, new Vector2(-direction.y, direction.x))), Is.LessThan(0.001f));
        }

        private static Vector2Int[] OutsideCorners(MapCellArea bounds)
        {
            var min = bounds.Minimum - Vector2Int.one * 250;
            var max = bounds.MaximumInclusive + Vector2Int.one * 250;
            return new[] { min, new Vector2Int(min.x, max.y), max, new Vector2Int(max.x, min.y) };
        }

        private static void AssertInside(MapCellArea bounds, Vector2 cell)
        {
            const float tolerance = 0.0001f;
            Assert.That(cell.x, Is.InRange(bounds.Minimum.x - tolerance, bounds.MaximumInclusive.x + tolerance));
            Assert.That(cell.y, Is.InRange(bounds.Minimum.y - tolerance, bounds.MaximumInclusive.y + tolerance));
        }
    }
}
