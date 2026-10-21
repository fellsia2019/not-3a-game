#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3ManualResourceTests
    {
        private InputTestFixture input;
        private Keyboard keyboard;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component avatar;
        private int previousCaptureRate;
        private float previousTimeScale;

        private static T Read<T>(object component, string property) =>
            (T)component.GetType().GetProperty(property).GetValue(component);
        private Component[] ResourceViews() => game.GetComponentsInChildren<MonoBehaviour>(true)
            .Where(component => component.GetType().Name == "Stage3ResourceSprite").Cast<Component>().ToArray();
        private Component View(Stage2ResourceNode node) => ResourceViews().Single(c => Read<Stage2ResourceNode>(c, "Node") == node);
        private Component Footprints => game.GetComponent("Stage3ManualResourceCollisions");
        private float Radius(Stage2ResourceNode node) => (float)Footprints.GetType().GetMethod("ColliderRadius").Invoke(Footprints, new object[] { node });

        // Independent PNG check: a FullRect sprite's transparent atlas padding is not a visible crest.
        private static Vector2 SourceCrest(Sprite sprite)
        {
            var source = new Texture2D(2, 2);
            try
            {
                Assert.That(ImageConversion.LoadImage(source, File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture))), Is.True);
                var pixels = source.GetPixels32(); var rect = sprite.rect;
                var left = (int)rect.xMax; var right = (int)rect.xMin; var top = (int)rect.yMin;
                for (var y = (int)rect.yMin; y < rect.yMax; y++) for (var x = (int)rect.xMin; x < rect.xMax; x++)
                {
                    if (pixels[y * source.width + x].a < 128) continue;
                    left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1); top = Mathf.Max(top, y + 1);
                }
                return (new Vector2((left + right) * .5f - rect.x, top - rect.y) - sprite.pivot) / sprite.pixelsPerUnit;
            }
            finally { Object.DestroyImmediate(source); }
        }

        private (Stage2ResourceNode node, Vector2 start, Vector2 inward) ClearApproach(MapResourceKind kind, Stage2ResourceSize size,
            bool requireComfortableRange = false)
        {
            var economy = game.Gathering.Economy;
            foreach (var node in economy.Nodes.Where(n => n.Resource == kind && n.Size == size && !n.IsDepleted))
            foreach (var outward in new[] { Vector2.left, Vector2.down, Vector2.right, Vector2.up })
            {
                var radius = Radius(node);
                if (radius <= 0f) continue;
                var start = node.WorldPosition + outward * (radius + scene.PlayerMover.CollisionRadius + .6f);
                if (scene.PlayerMover.Obstacles.IsBlocked(start, scene.PlayerMover.CollisionRadius)) continue;
                if (requireComfortableRange && economy.NearestAvailableNode(start) != node) continue;
                var contact = scene.PlayerMover.Obstacles.Move(start, -outward * .7f, scene.PlayerMover.CollisionRadius);
                if (Mathf.Abs(Vector2.Distance(contact, node.WorldPosition) - radius - scene.PlayerMover.CollisionRadius) > .01f ||
                    economy.NearestAvailableNode(contact) != node) continue;
                return (node, start, -outward);
            }
            Assert.Fail($"No clear physical approach to {kind} {size}."); return default;
        }

        [UnitySetUp]
        public IEnumerator Load()
        {
            previousCaptureRate = Time.captureFramerate; previousTimeScale = Time.timeScale;
            Time.captureFramerate = 60; Time.timeScale = 1f;
            input = new InputTestFixture(); input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < 8; i++) yield return null;
            game = Object.FindAnyObjectByType<Stage2GameComposition>();
            Assert.That(game != null && game.IsInitialized, Is.True);
            scene = game.GetComponent<Stage2SceneBootstrap>();
            avatar = scene.PlayerMover.GetComponent("Stage3HeroAvatar"); Assert.That(avatar, Is.Not.Null);
            game.HeroCombat.AutomaticTick = false;
            game.RunDriver.ResetRun(); game.SetPaused(false);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null;
            input.TearDown(); Time.captureFramerate = previousCaptureRate; Time.timeScale = previousTimeScale;
        }

        [UnityTest]
        public IEnumerator SavedSceneBindsEveryIndependentNodeAndNaturalTreeGroupToItsActualSprite()
        {
            var economy = game.Gathering.Economy;
            Assert.That(game.Gathering.Catalog.ManualResources, Is.Not.Null, "Stage3-only distribution is actually wired.");
            Assert.That(economy.Nodes.Count, Is.GreaterThan(250));
            Assert.That(ResourceViews().Length, Is.EqualTo(economy.Nodes.Count));
            foreach (var node in economy.Nodes)
            {
                var body = Read<SpriteRenderer>(View(node), "Body");
                Assert.That(body.enabled && body.sprite != null, Is.True);
                Assert.That(body.transform.parent.lossyScale, Is.EqualTo(Vector3.one));
                Assert.That(node.Remaining, Is.EqualTo(node.Capacity));
                Assert.That(scene.MapDefinition.GetSurface(node.Cell), Is.EqualTo(MapSurfaceKind.Forest));
            }
            var groups = economy.Nodes.Where(n => n.Resource == MapResourceKind.Wood && n.GroupId >= 0).GroupBy(n => n.GroupId);
            Assert.That(groups.All(group => group.Count() >= 1 && group.Count() <= 8), Is.True);
            Assert.That(groups.Any(group => group.Count() >= 3), Is.True);
            var minimap = game.GetComponent<Stage2Minimap>();
            yield return null;
            var resourceMarkers = minimap.Markers.Count(m => (int)m.Kind >= 0 && (int)m.Kind <= 2);
            Assert.That(resourceMarkers, Is.EqualTo(economy.Nodes.Count), "M/minimap use all live manual sources.");
        }

        [UnityTest]
        public IEnumerator EverySourceSizeBlocksSweptMovementAndRemainsHarvestableAtItsContactEdge()
        {
            Assert.That(Footprints, Is.Not.Null);
            Assert.That(Read<int>(Footprints, "Count"), Is.EqualTo(game.Gathering.Economy.Nodes.Count));
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            foreach (var size in new[] { Stage2ResourceSize.Small, Stage2ResourceSize.Medium, Stage2ResourceSize.Large })
            {
                if (kind == MapResourceKind.Wood && size != Stage2ResourceSize.Small) continue;
                var choice = ClearApproach(kind, size);
                var hero = scene.PlayerMover;
                hero.transform.position = choice.start;
                yield return null;
                var recoveries = hero.OverlapRecoveryCount;
                hero.MoveBy(choice.inward * 100f);
                yield return null;
                Assert.That(Vector2.Distance(hero.transform.position, choice.node.WorldPosition),
                    Is.EqualTo(Radius(choice.node) + hero.CollisionRadius).Within(.006f));
                Assert.That(hero.Obstacles.IsBlocked(hero.transform.position, hero.CollisionRadius), Is.False);
                Assert.That(hero.OverlapRecoveryCount, Is.EqualTo(recoveries), "Ordinary contact cannot teleport through a resource.");
                Assert.That(game.Gathering.InteractionTarget, Is.SameAs(choice.node));
                var before = game.Gathering.Economy.Inventory.Get(kind);
                input.Press(keyboard.eKey);
                for (var i = 0; i < 100 && game.Gathering.Economy.Inventory.Get(kind) == before; i++) yield return null;
                input.Release(keyboard.eKey); yield return null;
                Assert.That(game.Gathering.Economy.Inventory.Get(kind), Is.EqualTo(before + 1));
            }
        }

        [UnityTest]
        public IEnumerator LargerMineralsAcceptRealEWithClearSpaceBeforeCollision()
        {
            foreach (var kind in new[] { MapResourceKind.Stone, MapResourceKind.Metal })
            foreach (var size in new[] { Stage2ResourceSize.Medium, Stage2ResourceSize.Large })
            {
                var choice = ClearApproach(kind, size, true);
                var hero = scene.PlayerMover;
                hero.transform.position = choice.start;
                scene.CameraFollow.SnapToTarget();
                yield return null; yield return null;
                Assert.That(Vector2.Distance(hero.transform.position, choice.node.WorldPosition) - Radius(choice.node),
                    Is.GreaterThan(.75f), "E must work well before the hero touches the mineral.");
                Assert.That(hero.Obstacles.IsBlocked(hero.transform.position, hero.CollisionRadius), Is.False);
                Assert.That(game.Gathering.InteractionTarget, Is.SameAs(choice.node));
                var marker = game.GetComponent("Stage3ResourceInteractionMarker");
                Assert.That(Read<bool>(marker, "IsVisible"), Is.True);
                Assert.That(Read<Stage2ResourceNode>(marker, "Target"), Is.SameAs(choice.node));
                var before = game.Gathering.Economy.Inventory.Get(kind);
                input.Press(keyboard.eKey);
                for (var i = 0; i < 100 && game.Gathering.Economy.Inventory.Get(kind) == before; i++) yield return null;
                input.Release(keyboard.eKey); yield return null;
                Assert.That(game.Gathering.Economy.Inventory.Get(kind), Is.EqualTo(before + 1));
            }
        }

        [UnityTest]
        public IEnumerator DepletionOpensTheSiteAndRecoveryWaitsForTheHeroToLeaveBeforeBlockingAgain()
        {
            game.SetPaused(true);
            var choice = ClearApproach(MapResourceKind.Stone, Stage2ResourceSize.Medium);
            var node = choice.node; var economy = game.Gathering.Economy; var hero = scene.PlayerMover;
            hero.transform.position = choice.start;
            while (!node.IsDepleted) economy.AdvanceGather(node, node.WorldPosition, true, node.GatherSeconds, true);
            economy.CancelGather(); yield return null;
            Assert.That(Radius(node), Is.Zero);
            hero.transform.position = node.WorldPosition;
            var recoveries = hero.OverlapRecoveryCount;
            economy.Tick(node.RecoverySeconds, true); yield return null; yield return null;
            Assert.That(node.IsDepleted, Is.False);
            Assert.That(Radius(node), Is.Zero, "A recovering source must let an overlapping hero walk out.");
            Assert.That((Vector2)hero.transform.position, Is.EqualTo(node.WorldPosition));
            hero.MoveBy(-choice.inward * (node.HarvestRadius + hero.CollisionRadius + .3f)); yield return null;
            Assert.That(Radius(node), Is.EqualTo(node.InteractionRadius));
            hero.MoveBy(choice.inward * 100f); yield return null;
            Assert.That(Vector2.Distance(hero.transform.position, node.WorldPosition),
                Is.EqualTo(Radius(node) + hero.CollisionRadius).Within(.006f));
            Assert.That(hero.OverlapRecoveryCount, Is.EqualTo(recoveries));
        }

        [UnityTest]
        public IEnumerator InteractionTargetMatchesHeldEAndClearsForPauseUiAndConstruction()
        {
            var choice = ClearApproach(MapResourceKind.Wood, Stage2ResourceSize.Small);
            scene.PlayerMover.transform.position = choice.start;
            scene.PlayerMover.MoveBy(choice.inward * 100f); yield return null;
            Assert.That(game.Gathering.InteractionTarget, Is.SameAs(choice.node));
            input.Press(keyboard.eKey); for (var i = 0; i < 5; i++) yield return null;
            Assert.That(game.Gathering.GatheringNode, Is.SameAs(game.Gathering.InteractionTarget));
            game.SetPaused(true); yield return null; Assert.That(game.Gathering.InteractionTarget, Is.Null);
            input.Release(keyboard.eKey); game.SetPaused(false); yield return null;
            game.Gathering.SetBuildMode(true); Assert.That(game.Gathering.InteractionTarget, Is.Null);
            game.Gathering.SetBuildMode(false);
            game.Gathering.SetProductionBuildMode(true); Assert.That(game.Gathering.InteractionTarget, Is.Null);
            game.Gathering.SetProductionBuildMode(false);
            input.Press(keyboard.iKey); yield return null; input.Release(keyboard.iKey); yield return null;
            Assert.That(game.Gathering.InteractionTarget, Is.Null);
        }

        [UnityTest]
        public IEnumerator OverheadToolFollowsExactTargetAcrossZoomAndHidesWhenHarvestIsUnavailable()
        {
            var marker = game.GetComponent("Stage3ResourceInteractionMarker");
            Assert.That(marker, Is.Not.Null);
            var clearHome = scene.PlayerMover.transform.position;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var choice = ClearApproach(kind, Stage2ResourceSize.Small);
                scene.PlayerMover.transform.position = choice.start;
                scene.PlayerMover.MoveBy(choice.inward * 100f);
                var camera = scene.CameraFollow.BoundCamera;
                var body = Read<SpriteRenderer>(View(choice.node), "Body");
                var crest = SourceCrest(body.sprite);
                var follow = scene.CameraFollow.enabled;
                scene.CameraFollow.enabled = false;
                foreach (var zoom in new[] { 2.5f, 5f, 10f })
                {
                    camera.orthographicSize = zoom;
                    camera.transform.position = (Vector3)choice.node.WorldPosition + Vector3.back * .55f - camera.transform.forward * 30f;
                    yield return null; yield return null;
                    Assert.That(Read<bool>(marker, "IsVisible"), Is.True);
                    Assert.That(Read<Stage2ResourceNode>(marker, "Target"), Is.SameAs(game.Gathering.InteractionTarget));
                    Assert.That(Read<bool>(marker, "ToolIsAxe"), Is.EqualTo(kind == MapResourceKind.Wood));
                    var screenCrest = camera.WorldToScreenPoint(body.transform.TransformPoint(crest));
                    var badge = Read<Rect>(marker, "ScreenRect");
                    if (screenCrest.y + badge.height + 16f < camera.pixelRect.yMax)
                    {
                        Assert.That(badge.yMin - screenCrest.y, Is.EqualTo(8f).Within(2f),
                            "Marker belongs above painted pixels, not transparent FullRect padding.");
                        Assert.That(badge.center.x, Is.EqualTo(screenCrest.x).Within(2f));
                    }
                }
                game.SetPaused(true); yield return null; yield return null;
                Assert.That(Read<bool>(marker, "IsVisible"), Is.False);
                game.SetPaused(false);
                input.Press(keyboard.iKey); yield return null; input.Release(keyboard.iKey); yield return null;
                Assert.That(Read<bool>(marker, "IsVisible"), Is.False);
                input.Press(keyboard.escapeKey); yield return null; input.Release(keyboard.escapeKey); yield return null;
                scene.PlayerMover.transform.position = clearHome;
                yield return null; yield return null;
                Assert.That(game.Gathering.InteractionTarget, Is.Null);
                Assert.That(Read<bool>(marker, "IsVisible"), Is.False);
                scene.CameraFollow.enabled = follow;
            }
        }

        [UnityTest]
        public IEnumerator ResourceFootingStaysRegisteredAfterAnchorAndCameraChanges()
        {
            game.SetPaused(true);scene.CameraFollow.enabled=false;
            var component=View(game.Gathering.Economy.Nodes.First(n=>n.Resource==MapResourceKind.Stone && n.Size==Stage2ResourceSize.Medium));
            var body=Read<SpriteRenderer>(component,"Body");var camera=scene.CameraFollow.BoundCamera;
            foreach(var rotation in new[]{camera.transform.rotation,Quaternion.Euler(52,0,25)})
            {
                camera.transform.rotation=rotation;component.transform.position+=Vector3.right*2f;
                yield return null;yield return null;
                Assert.That(Quaternion.Angle(body.transform.rotation,rotation),Is.LessThan(.001f));
                var actual=(Vector2)camera.WorldToScreenPoint(body.transform.position);
                var expected=(Vector2)camera.WorldToScreenPoint(component.transform.position+Vector3.back*.025f);
                Assert.That(Vector2.Distance(actual,expected),Is.LessThan(.01f),"Billboard pivot remains on its ground anchor after cache invalidation.");
            }
        }

        [UnityTest]
        public IEnumerator ForegroundOrdinaryTreeFadesOnlyWhileItCoversTheHero()
        {
            game.SetPaused(true);
            var node = game.Gathering.Economy.Nodes.First(n => n.Resource == MapResourceKind.Wood && n.GroupId >= 0);
            var body = Read<SpriteRenderer>(View(node), "Body");
            scene.PlayerMover.transform.position = node.WorldPosition + Vector2.up * .65f;
            yield return new WaitForSecondsRealtime(.2f); yield return null;
            Assert.That(body.color.a, Is.LessThan(.6f), "Foreground crown must leave the hero readable.");
            scene.PlayerMover.transform.position = node.WorldPosition + Vector2.right * 6f;
            yield return new WaitForSecondsRealtime(.2f); yield return null;
            Assert.That(body.color.a, Is.EqualTo(1f).Within(.001f));
            Assert.That(node.Remaining, Is.EqualTo(node.Capacity), "Visibility cannot harvest the tree.");
        }

        [UnityTest]
        public IEnumerator ActualHeroAndEnemyPlaybackUsesEightPhasesAndFreezesOnPause()
        {
            Assert.That(Read<int>(avatar, "WalkFrameCount"), Is.EqualTo(8));
            var start = scene.PlayerMover.transform.position;
            var frames = new System.Collections.Generic.HashSet<int>();
            var images = new System.Collections.Generic.HashSet<Sprite>();
            for (var i = 0; i < 40; i++)
            {
                scene.PlayerMover.transform.position = start + Vector3.right * (.045f * (i + 1));
                yield return null;
                frames.Add(Read<int>(avatar, "Frame")); images.Add(Read<SpriteRenderer>(avatar, "Body").sprite);
            }
            Assert.That(frames.Count, Is.EqualTo(8)); Assert.That(images.Count, Is.EqualTo(8));
            var enemy = game.Enemies.SpawnAt(scene.MapDefinition.EnemyEntrance);
            yield return null; yield return null;
            enemy.SetAutomaticTick(false);
            var skeleton = enemy.GetComponentsInChildren<MonoBehaviour>().Single(c => c.GetType().Name == "Stage3SkeletonSprite");
            var origin = enemy.transform.position;
            frames.Clear(); images.Clear();
            for (var i = 0; i < 40; i++)
            {
                enemy.transform.position = origin + Vector3.down * (.032f * (i + 1));
                yield return null;
                frames.Add(Read<int>(skeleton, "CurrentFrame")); images.Add(Read<SpriteRenderer>(skeleton, "Body").sprite);
            }
            Assert.That(frames.Count, Is.EqualTo(8)); Assert.That(images.Count, Is.EqualTo(8));
            game.SetPaused(true); yield return null;
            var before = Read<SpriteRenderer>(skeleton, "Body").sprite;
            for (var i = 0; i < 12; i++) yield return null;
            Assert.That(Read<SpriteRenderer>(skeleton, "Body").sprite, Is.SameAs(before));
            Assert.That(enemy.IsAlive, Is.True);
        }

        [UnityTest]
        public IEnumerator HeldEUsesMatchingToolAndPaysAtEachConfirmedStrikeForAllThreeResources()
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                input.Release(keyboard.eKey); yield return null;
                var economy = game.Gathering.Economy;
                var node = economy.Nodes.First(n => n.Resource == kind &&
                    (n.GroupId >= 0 || n.Size == Stage2ResourceSize.Medium));
                scene.PlayerMover.transform.position = node.WorldPosition;
                yield return null;
                var before = economy.Inventory.Get(kind);
                var stock = node.Remaining;
                var contacts = Read<int>(avatar, "ConfirmedStrikes");
                input.Press(keyboard.eKey);
                for (var i = 0; i < 12; i++) yield return null;
                Assert.That(game.Gathering.GatheringNode, Is.SameAs(node));
                Assert.That(Read<bool>(avatar, "IsHarvesting"), Is.True);
                Assert.That(Read<MapResourceKind?>(avatar, "EquippedHarvestResource"), Is.EqualTo(kind));
                Assert.That(Read<SpriteRenderer>(avatar, "Equipment").enabled, Is.True, "The actual equipped tool must be visible during harvesting.");
                Assert.That(Read<bool>(avatar, "UsesDetachedHarvestTool"), Is.True, "A baked starter tool cannot represent owned quality.");
                Assert.That(Read<int>(avatar, "DisplayedEquipmentType"), Is.EqualTo(kind == MapResourceKind.Wood ? 7 : 6));
                Assert.That(Read<int>(avatar, "DisplayedEquipmentQuality"), Is.Zero);
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(before), "Anticipation cannot pay early.");
                for (var i = 0; i < 200 && economy.Inventory.Get(kind) < before + 2; i++) yield return null;
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(before + 2));
                Assert.That(node.Remaining, Is.EqualTo(stock - 2));
                Assert.That(Read<int>(avatar, "ConfirmedStrikes"), Is.EqualTo(contacts + 2));
                input.Release(keyboard.eKey);
                for (var i = 0; i < 15; i++) yield return null;
                Assert.That(Read<bool>(avatar, "IsHarvesting"), Is.False);
                Assert.That(Read<SpriteRenderer>(avatar, "Equipment").enabled, Is.True);
            }
        }

        [UnityTest]
        public IEnumerator DepletionChangesMineralSpritesAndRecoveryRestoresWithoutPayment()
        {
            game.SetPaused(true);
            var economy = game.Gathering.Economy;
            foreach (var kind in new[] { MapResourceKind.Stone, MapResourceKind.Metal })
            foreach (var size in new[] { Stage2ResourceSize.Small, Stage2ResourceSize.Medium, Stage2ResourceSize.Large })
            {
                var node = economy.Nodes.First(n => n.Resource == kind && n.Size == size);
                var component = View(node); var body = Read<SpriteRenderer>(component, "Body");
                var full = body.sprite; var seen = new System.Collections.Generic.HashSet<Sprite> { full };
                var before = economy.Inventory.Get(kind);
                for (var i = 0; i < node.Capacity; i++)
                {
                    Assert.That(economy.AdvanceGather(node, node.WorldPosition, true, node.GatherSeconds, true), Is.True);
                    yield return null;
                    if (!node.IsDepleted) seen.Add(body.sprite);
                }
                yield return null;
                Assert.That(seen.Count, Is.GreaterThanOrEqualTo(3), "Distinct coherent depletion drawings, not one scaled sprite.");
                Assert.That(body.enabled && body.gameObject.activeInHierarchy, Is.False, "Legacy anchor parents and independent renderers both hide depleted nodes.");
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(before + node.Capacity));
                economy.Tick(node.RecoverySeconds, true); yield return null; yield return null;
                Assert.That(node.Remaining, Is.EqualTo(node.Capacity));
                Assert.That(body.enabled && body.gameObject.activeInHierarchy, Is.True); Assert.That(body.sprite, Is.SameAs(full));
                Assert.That(economy.Inventory.Get(kind), Is.EqualTo(before + node.Capacity));
                economy.CancelGather();
            }
        }

        [UnityTest]
        public IEnumerator PauseAndFullScreenUiCancelWorkAndRequireFreshEWithoutFreeStock()
        {
            var economy = game.Gathering.Economy;
            var node = economy.Nodes.First(n => n.Resource == MapResourceKind.Stone && n.Size == Stage2ResourceSize.Medium);
            scene.PlayerMover.transform.position = node.WorldPosition;
            yield return null;
            input.Press(keyboard.eKey); for (var i = 0; i < 18; i++) yield return null;
            game.SetPaused(true); yield return null;
            Assert.That(game.Gathering.GatheringNode, Is.Null);
            var frozen = Read<SpriteRenderer>(avatar, "Body").sprite;
            for (var i = 0; i < 12; i++) yield return null;
            Assert.That(Read<SpriteRenderer>(avatar, "Body").sprite, Is.SameAs(frozen));
            game.SetPaused(false); for (var i = 0; i < 60; i++) yield return null;
            Assert.That(node.Remaining, Is.EqualTo(node.Capacity));
            input.Release(keyboard.eKey); yield return null;
            input.Press(keyboard.eKey); for (var i = 0; i < 12; i++) yield return null;
            input.Press(keyboard.iKey); yield return null; input.Release(keyboard.iKey);
            for (var i = 0; i < 60; i++) yield return null;
            Assert.That(game.Gathering.GatheringNode, Is.Null);
            Assert.That(node.Remaining, Is.EqualTo(node.Capacity));
            Assert.That(economy.Inventory.Get(MapResourceKind.Stone), Is.Zero);
            input.Release(keyboard.eKey); input.Press(keyboard.escapeKey); yield return null; input.Release(keyboard.escapeKey);
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(economy.Nodes.All(n => n.Remaining == n.Capacity && n.RecoveryRemaining == 0f), Is.True);
            Assert.That(economy.CurrentNode, Is.Null);
            Assert.That(Read<bool>(avatar, "IsHarvesting"), Is.False);
        }

        [UnityTest]
        public IEnumerator MovingCancelsAnUnfinishedStrikeWithoutImmobilizingTheHero()
        {
            var economy = game.Gathering.Economy;
            var node = economy.Nodes.First(n => n.Resource == MapResourceKind.Metal && n.Size == Stage2ResourceSize.Medium);
            scene.PlayerMover.transform.position = node.WorldPosition;
            yield return null; yield return null;
            input.Press(keyboard.eKey); for (var i = 0; i < 18; i++) yield return null;
            Assert.That(economy.GatherProgress, Is.GreaterThan(0f));
            var before = scene.PlayerMover.transform.position;
            input.Press(keyboard.dKey); for (var i = 0; i < 24; i++) yield return null;
            Assert.That(scene.PlayerMover.transform.position.x, Is.GreaterThan(before.x + .5f));
            Assert.That(economy.GatherProgress, Is.Zero);
            Assert.That(economy.Inventory.Get(MapResourceKind.Metal), Is.Zero);
            input.Release(keyboard.dKey); input.Release(keyboard.eKey);
            yield return null;
            Assert.That(economy.CurrentNode, Is.Null);
        }

        [UnityTest]
        public IEnumerator RegenerationReplacesOldBindingsWithoutDuplicatingViewsOrGrantingStock()
        {
            var world = game.GetComponent("Stage3ResourceDistrictWorld");
            var method = world.GetType().GetMethod("Regenerate");
            var economy = game.Gathering.Economy;
            method.Invoke(world, new object[] { 164032 }); yield return null; yield return null;
            var before = economy.Nodes.Skip(3).Select(n => n.WorldPosition).ToArray();
            var stale = economy.Nodes[3];
            var oldMap = Read<HeroObstacleMap>(Footprints, "BoundMap");
            var oldSlots = oldMap.Circles.Count - economy.Nodes.Count;
            method.Invoke(world, new object[] { 164032 }); yield return null; yield return null;
            Assert.That(economy.Nodes.Skip(3).Select(n => n.WorldPosition), Is.EqualTo(before));
            Assert.That(ResourceViews().Length, Is.EqualTo(economy.Nodes.Count));
            Assert.That(ResourceViews().Any(v => Read<Stage2ResourceNode>(v, "Node") == stale), Is.False);
            Assert.That(economy.AdvanceGather(stale, stale.WorldPosition, true, stale.GatherSeconds, true), Is.False);
            Assert.That(economy.Inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(Read<int>(Footprints, "Count"), Is.EqualTo(economy.Nodes.Count));
            Assert.That(Read<HeroObstacleMap>(Footprints, "BoundMap"), Is.SameAs(scene.PlayerMover.Obstacles));
            Assert.That(oldMap.Circles.Skip(oldSlots).All(circle => circle.Radius == 0f), Is.True,
                "Old resource footing slots are disabled when the generated world is replaced.");
            method.Invoke(world, new object[] { 164033 }); yield return null; yield return null;
            Assert.That(economy.Nodes[3].WorldPosition, Is.Not.EqualTo(before[0]));
        }
    }
}
#endif
