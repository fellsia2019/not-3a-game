using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Not3A.Stage1.Tests
{
    public sealed class Stage1IntegrationTests : InputTestFixture
    {
        private const string SceneName = "Stage1Graybox";
        private Keyboard testKeyboard;

        public override void Setup()
        {
            base.Setup();
            testKeyboard = InputSystem.AddDevice<Keyboard>();
        }

        [UnitySetUp]
        public IEnumerator LoadGrayboxScene()
        {
            SceneManager.LoadScene(SceneName);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_InputSystemDrivesMovementContextGatheringAndBuildMode()
        {
            var keyboard = testKeyboard;
            SceneManager.LoadScene(SceneName);
            yield return null;
            var game = Object.FindAnyObjectByType<Stage1GameController>();
            var player = Object.FindAnyObjectByType<PlayerMover>().transform;
            var startPosition = player.position;

            Press(keyboard.wKey);
            yield return null;
            yield return null;
            Release(keyboard.wKey);
            yield return null;
            Assert.That(player.position.y, Is.GreaterThan(startPosition.y), "W must move the hero through the Input System.");

            var nodes = Object.FindObjectsByType<GatherableNode>();
            var tree = System.Array.Find(nodes, node => node.Resource == ResourceKind.Wood);
            var stone = System.Array.Find(nodes, node => node.Resource == ResourceKind.Stone);
            Assert.That(tree, Is.Not.Null);
            Assert.That(stone, Is.Not.Null);

            player.position = tree.transform.position + Vector3.left * 0.9f;
            Press(keyboard.dKey);
            yield return new WaitForSeconds(0.45f);
            Release(keyboard.dKey);
            yield return null;
            Assert.That(player.position.x, Is.LessThan(tree.transform.position.x - 0.5f), "The grounded tree block must stop hero movement.");

            player.position = tree.transform.position + Vector3.right * 0.3f;
            yield return null;
            var toolMarker = player.Find("Context Tool").GetComponent<SpriteRenderer>();
            Assert.That(toolMarker.enabled, Is.True);
            var axeColor = toolMarker.color;
            Press(keyboard.eKey);
            yield return new WaitForSeconds(0.82f);
            Release(keyboard.eKey);
            yield return null;
            Assert.That(game.Wood, Is.EqualTo(1));
            Assert.That(game.ObjectiveProgress, Is.EqualTo(1));

            player.position = stone.transform.position + Vector3.right * 0.3f;
            yield return null;
            Assert.That(toolMarker.enabled, Is.True);
            Assert.That(toolMarker.color, Is.Not.EqualTo(axeColor), "Tree and stone must present different contextual tools.");
            Press(keyboard.eKey);
            yield return new WaitForSeconds(1.02f);
            Release(keyboard.eKey);
            yield return null;
            Assert.That(game.Stone, Is.EqualTo(1));
            Assert.That(game.ObjectiveProgress, Is.EqualTo(1), "Stone collection must not advance the wood objective.");

            Press(keyboard.bKey);
            yield return null;
            Release(keyboard.bKey);
            yield return null;
            Assert.That(game.BuildPlacement.IsPlacing, Is.True);

            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);
            yield return null;
            Assert.That(game.BuildPlacement.IsPlacing, Is.False);
        }

        [UnityTest]
        public IEnumerator Scene_PlayerCameraGatherObjectivePlacementAndTowerCompleteTheWave()
        {
            var game = Object.FindAnyObjectByType<Stage1GameController>();
            var camera = Camera.main;
            var mover = Object.FindAnyObjectByType<PlayerMover>();
            var gatherer = Object.FindAnyObjectByType<PlayerGatherer>();
            var navigation = Object.FindAnyObjectByType<DynamicNavigationGrid>();

            Assert.That(game, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.orthographic, Is.True);
            Assert.That(camera.transform.rotation, Is.EqualTo(Quaternion.identity));
            Assert.That(mover, Is.Not.Null);
            Assert.That(mover.GetComponent<HealthComponent>(), Is.Null, "The non-combat hero must not have combat health.");
            Assert.That(gatherer, Is.Not.Null);
            Assert.That(navigation, Is.Not.Null);
            Assert.That(GameObject.Find("Resource Zone"), Is.Null, "Debug resource-zone fill must not leak into the gameplay view.");
            Assert.That(GameObject.Find("Build Zone"), Is.Null, "Debug build-zone fill must not leak into the gameplay view.");
            var resourceNodes = Object.FindObjectsByType<GatherableNode>();
            Assert.That(resourceNodes.Length, Is.GreaterThanOrEqualTo(2));
            foreach (var node in resourceNodes)
            {
                var cell = IsoGrid.WorldToCellCoordinates(node.transform.position);
                Assert.That(cell.x, Is.InRange(-9f, 9f), $"{node.name} must be inside the readable map boundary.");
                Assert.That(cell.y, Is.InRange(-7f, 7f), $"{node.name} must be inside the readable map boundary.");
                Assert.That(node.GetComponent<BoxCollider2D>(), Is.Not.Null, $"{node.name} must physically block the hero at its grounded footprint.");
            }

            var treeNode = System.Array.Find(resourceNodes, node => node.Resource == ResourceKind.Wood);
            var stoneNode = System.Array.Find(resourceNodes, node => node.Resource == ResourceKind.Stone);
            AssertGrounded(treeNode.gameObject, "Green Tree Block");
            AssertGrounded(stoneNode.gameObject, "Gray Stone Block");
            AssertGrounded(mover.gameObject, "Cyan Hero Block");
            var heroHeight = GetVisualBounds(mover.gameObject).size.y;
            Assert.That(GetVisualBounds(treeNode.gameObject).size.y, Is.GreaterThan(heroHeight * 1.35f), "A tree must read as substantially taller than the hero.");
            Assert.That(GetVisualBounds(stoneNode.gameObject).size.y, Is.LessThan(heroHeight), "A stone cluster must read as lower than the hero.");
            var throneObject = Object.FindAnyObjectByType<HealthComponent>().gameObject;
            AssertGrounded(throneObject, "Gold Throne Block");
            Assert.That(GetVisualBounds(throneObject).size.y, Is.GreaterThan(heroHeight * 1.4f), "The throne must remain the large central landmark.");
            Assert.That(throneObject.GetComponent<BoxCollider2D>(), Is.Not.Null, "The throne must have a grounded blocking footprint.");

            game.CollectResource(ResourceKind.Stone, 1);
            Assert.That(game.ObjectiveProgress, Is.Zero);
            game.CollectResource(ResourceKind.Wood, 5);
            Assert.That(game.ObjectiveComplete, Is.True);
            Assert.That(game.Stone, Is.EqualTo(3), "The fixed objective reward must be granted exactly once.");
            game.CollectResource(ResourceKind.Wood, 1);
            Assert.That(game.Stone, Is.EqualTo(3), "Repeated valid collection must not repeat the reward.");

            var woodBeforeInvalidBuild = game.Wood;
            var stoneBeforeInvalidBuild = game.Stone;
            Assert.That(game.BuildPlacement.TryPlaceAt(IsoGrid.CellToWorld(Vector2Int.zero), out var invalidReason), Is.False);
            Assert.That(invalidReason, Does.Contain("занята"));
            Assert.That(game.Wood, Is.EqualTo(woodBeforeInvalidBuild));
            Assert.That(game.Stone, Is.EqualTo(stoneBeforeInvalidBuild));
            Assert.That(game.BuildPlacement.TryPlaceAt(treeNode.transform.position, out var resourceReason), Is.False);
            Assert.That(resourceReason, Does.Contain("занята"));
            Assert.That(game.BuildPlacement.TryPlaceAt(IsoGrid.CellToWorld(new Vector2Int(20, 20)), out var outsideReason), Is.False);
            Assert.That(outsideReason, Does.Contain("Вне карты"));

            var routeCell = new Vector2Int(4, 0);
            Assert.That(game.BuildPlacement.TryPlaceAt(IsoGrid.CellToWorld(routeCell), out var reason), Is.True, reason);
            Assert.That(game.BuildPlacement.PlacedTowerCount, Is.EqualTo(1));
            Assert.That(game.Wood, Is.EqualTo(4));
            Assert.That(game.Stone, Is.EqualTo(2));
            Assert.That(game.BuildPlacement.TryPlaceAt(IsoGrid.CellToWorld(routeCell), out var occupiedReason), Is.False);
            Assert.That(occupiedReason, Does.Contain("занята"));
            Assert.That(game.Wood, Is.EqualTo(4));
            Assert.That(game.Stone, Is.EqualTo(2));
            var placedTower = Object.FindAnyObjectByType<TowerController>();
            Assert.That(placedTower.OccupiedCell, Is.EqualTo(routeCell));
            AssertGrounded(placedTower.gameObject, "Blue Tower Block");
            Assert.That(placedTower.GetComponent<BoxCollider2D>(), Is.Not.Null, "A placed tower must physically block the hero at its base.");
            placedTower.Configure(100f, 0.05f, 100, null);

            game.StartWave();
            var timeout = Time.realtimeSinceStartup + 20f;
            while (game.Phase == RunPhase.Wave && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(game.Phase, Is.EqualTo(RunPhase.Won));
            Assert.That(Object.FindAnyObjectByType<HealthComponent>().Current, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator DynamicNavigation_FullWallIsAttackedAndDestroyedBeforeEnemyResumes()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Object.Destroy(root);
            }
            yield return null;

            var navigation = new GameObject("Test Navigation").AddComponent<DynamicNavigationGrid>();
            navigation.Configure(new Vector2Int(0, -1), new Vector2Int(4, 1), new Vector2Int(0, -1), new Vector2Int(4, 1), new Vector2Int(4, 0), Vector2Int.zero, null);
            for (var y = -1; y <= 1; y++)
            {
                var towerObject = new GameObject($"Blocker {y}");
                towerObject.transform.position = IsoGrid.CellToWorld(new Vector2Int(2, y));
                var health = towerObject.AddComponent<HealthComponent>();
                health.Configure(1);
                var tower = towerObject.AddComponent<TowerController>();
                tower.enabled = false;
                tower.ConfigureNavigation(navigation, new Vector2Int(2, y));
                Assert.That(navigation.RegisterTower(new Vector2Int(2, y), tower), Is.True);
            }

            var throneObject = new GameObject("Test Throne");
            var throne = throneObject.AddComponent<HealthComponent>();
            throne.Configure(5);
            var enemyObject = new GameObject("Test Enemy");
            var enemyHealth = enemyObject.AddComponent<HealthComponent>();
            enemyHealth.Configure(5);
            var enemy = enemyObject.AddComponent<EnemyController>();
            enemy.Configure(navigation, throne, null, 12f, 1, 0.05f, 1);

            var timeout = Time.realtimeSinceStartup + 3f;
            while (throne.Current == throne.Maximum && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(navigation.Revision, Is.GreaterThan(3), "Destroying a blocker must free its occupied cell and revise navigation.");
            Assert.That(throne.Current, Is.LessThan(throne.Maximum), "The enemy must resume and reach the throne after opening the wall.");
        }

        [UnityTest]
        public IEnumerator Scene_UnopposedWaveDestroysThroneAndRestartRestoresInitialState()
        {
            var game = Object.FindAnyObjectByType<Stage1GameController>();
            Assert.That(game, Is.Not.Null);

            game.StartWave();
            var timeout = Time.realtimeSinceStartup + 20f;
            while (game.Phase == RunPhase.Wave && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(game.Phase, Is.EqualTo(RunPhase.Lost));
            game.RestartRun();
            yield return null;
            yield return null;

            var restarted = Object.FindAnyObjectByType<Stage1GameController>();
            Assert.That(restarted, Is.Not.Null);
            Assert.That(restarted.Phase, Is.EqualTo(RunPhase.Preparation));
            Assert.That(restarted.Wood, Is.Zero);
            Assert.That(restarted.Stone, Is.Zero);
            Assert.That(restarted.ObjectiveProgress, Is.Zero);
        }

        private static Bounds GetVisualBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<SpriteRenderer>();
            Assert.That(renderers.Length, Is.GreaterThan(0), $"{root.name} must have a readable primitive silhouette.");
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static void AssertGrounded(GameObject root, string visualName)
        {
            var visual = root.transform.Find(visualName);
            Assert.That(visual, Is.Not.Null, $"{root.name} must expose its simple block as {visualName}.");
            var renderer = visual.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sprite.bounds.size.x, Is.EqualTo(1f).Within(0.001f), "The solid primitive sprite must be exactly one world unit wide before scaling.");
            Assert.That(renderer.sprite.bounds.size.y, Is.EqualTo(1f).Within(0.001f), "The solid primitive sprite must be exactly one world unit high before scaling.");
            Assert.That(renderer.bounds.min.y, Is.EqualTo(root.transform.position.y).Within(0.001f), $"{visualName} must touch the ground at its root position.");
        }
    }
}
