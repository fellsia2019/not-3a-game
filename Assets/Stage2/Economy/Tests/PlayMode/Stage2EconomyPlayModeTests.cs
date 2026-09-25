#if UNITY_EDITOR
using System.Collections;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage2.Economy.Tests
{
    public sealed class Stage2EconomyPlayModeTests
    {
        private InputTestFixture input;
        private Keyboard keyboard;
        private GameObject player;
        private GameObject owner;
        private Stage2GatheringDriver driver;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            input = new InputTestFixture();
            input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            var map = AssetDatabase.LoadAssetAtPath<Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
            var data = AssetDatabase.LoadAssetAtPath<Stage2EconomyCatalog>("Assets/Stage2/Economy/Data/Stage2EconomyPrototype.asset");
            player = new GameObject("Test Hero");
            owner = new GameObject("Test Economy");
            driver = owner.AddComponent<Stage2GatheringDriver>();
            driver.Configure(map, data, player.transform);
            Assert.That(driver.Economy, Is.Not.Null);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(owner);
            Object.Destroy(player);
            yield return null;
            input?.TearDown();
        }

        [UnityTest]
        public IEnumerator HeldInput_GathersAllThreeResourcesAtTheirMapAnchors()
        {
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var node = driver.Economy.GetNode(kind);
                player.transform.position = driver.Economy.WorldPosition(kind);
                input.Press(keyboard.eKey);
                yield return new WaitForSeconds(node.GatherSeconds + 0.1f);
                input.Release(keyboard.eKey);
                yield return null;
                Assert.That(driver.Economy.Inventory.Get(kind), Is.EqualTo(1));
                Assert.That(node.Remaining, Is.EqualTo(node.Capacity - 1));
            }
        }

        [UnityTest]
        public IEnumerator PauseAndRestart_BlockHeldInputAndClearProgressAndInventory()
        {
            var kind = MapResourceKind.Wood;
            player.transform.position = driver.Economy.WorldPosition(kind);
            input.Press(keyboard.eKey);
            yield return new WaitForSeconds(driver.Economy.GetNode(kind).GatherSeconds + 0.1f);
            Assert.That(driver.Economy.Inventory.Get(kind), Is.EqualTo(1));
            driver.SetRunActivity(true, true);
            var frozen = driver.Economy.GetNode(kind).Remaining;
            yield return new WaitForSeconds(driver.Economy.GetNode(kind).GatherSeconds + 0.1f);
            Assert.That(driver.Economy.GetNode(kind).Remaining, Is.EqualTo(frozen));
            driver.SetRunActivity(true, false);
            driver.ResetRun();
            yield return new WaitForSeconds(driver.Economy.GetNode(kind).GatherSeconds + 0.1f);
            Assert.That(driver.Economy.Inventory.Get(kind), Is.Zero, "Held input from prior run must not gather.");
            Assert.That(driver.Economy.GetNode(kind).Remaining, Is.EqualTo(driver.Economy.GetNode(kind).Capacity));
            input.Release(keyboard.eKey);
            yield return null;
            input.Press(keyboard.eKey);
            yield return new WaitForSeconds(driver.Economy.GetNode(kind).GatherSeconds + 0.1f);
            Assert.That(driver.Economy.Inventory.Get(kind), Is.EqualTo(1));
        }
    }
}
#endif
