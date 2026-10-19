using System.Collections.Generic;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Resource footing in the existing swept hero obstacle map; independent of sprite opacity.</summary>
    public sealed class Stage3ManualResourceCollisions : MonoBehaviour
    {
        private sealed class Footprint
        {
            public Stage2ResourceNode Node;
            public int Slot;
            public float Radius;
        }

        private readonly List<Footprint> footprints = new List<Footprint>();
        private readonly Dictionary<Stage2ResourceNode, Footprint> byNode = new Dictionary<Stage2ResourceNode, Footprint>();
        private Stage2Economy economy;
        private HeroObstacleMap obstacles;
        private Stage2PlayerMover hero;
        private float trunkRadius;

        public HeroObstacleMap BoundMap => obstacles;
        public int Count => footprints.Count;
        public float ColliderRadius(Stage2ResourceNode node) => byNode.TryGetValue(node, out var entry) ? entry.Radius : 0f;

        public void Bind(Stage2Economy model, HeroObstacleMap map, Stage2PlayerMover player, float treeRadius)
        {
            ClearBindings();
            economy = model; obstacles = map; hero = player; trunkRadius = treeRadius;
            if (economy == null || obstacles == null || hero == null) return;
            foreach (var node in economy.Nodes)
            {
                var slot = obstacles.AddUpdatableCircle(node.WorldPosition, .001f);
                obstacles.UpdateCircle(slot, node.WorldPosition, 0f);
                var entry = new Footprint { Node = node, Slot = slot };
                footprints.Add(entry); byNode.Add(node, entry);
                Sync(entry);
            }
            economy.NodeGathered += OnGathered;
        }

        public void RefreshNow()
        {
            if (obstacles == null || hero == null) return;
            foreach (var entry in footprints) Sync(entry);
        }
        private void LateUpdate() => RefreshNow();

        private void OnGathered(Stage2ResourceNode node)
        {
            if (byNode.TryGetValue(node, out var entry)) Sync(entry);
        }

        private void Sync(Footprint entry)
        {
            var desired = entry.Node.IsDepleted ? 0f : entry.Node.Resource == MapResourceKind.Wood
                ? trunkRadius : entry.Node.InteractionRadius;
            if (Mathf.Approximately(desired, entry.Radius)) return;
            // Recovery/reset may restore a source around a hero standing in its empty site.
            // Retain the smaller/disabled footing until the hero walks clear; never trap or teleport them.
            if (desired > entry.Radius && Vector2.Distance(hero.transform.position, entry.Node.WorldPosition) <
                desired + hero.CollisionRadius + .003f) return;
            obstacles.UpdateCircle(entry.Slot, entry.Node.WorldPosition, desired);
            entry.Radius = desired;
        }

        public void ClearBindings()
        {
            if (economy != null) economy.NodeGathered -= OnGathered;
            if (obstacles != null)
                foreach (var entry in footprints) obstacles.UpdateCircle(entry.Slot, entry.Node.WorldPosition, 0f);
            footprints.Clear(); byNode.Clear(); economy = null; obstacles = null;
        }

        private void OnDestroy() => ClearBindings();
    }
}
