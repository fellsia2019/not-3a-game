using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage1
{
    public sealed class PlayerGatherer : MonoBehaviour
    {
        [SerializeField] private Stage1GameController game;
        [SerializeField] private BuildPlacement buildPlacement;
        [SerializeField] private GatherableNode[] nodes;
        [SerializeField] private SpriteRenderer toolMarker;
        [SerializeField, Min(0.1f)] private float interactionRange = 1.5f;

        private InputAction interactAction;
        private GatherableNode currentNode;
        private float progress;

        public void Configure(
            Stage1GameController controller,
            BuildPlacement placement,
            GatherableNode[] gatherableNodes,
            SpriteRenderer marker,
            float range)
        {
            game = controller;
            buildPlacement = placement;
            nodes = gatherableNodes;
            toolMarker = marker;
            interactionRange = range;
        }

        private void OnEnable()
        {
            interactAction = new InputAction("Gather", InputActionType.Button, "<Keyboard>/e");
            interactAction.Enable();
        }

        private void OnDisable()
        {
            interactAction?.Dispose();
            interactAction = null;
        }

        private void Update()
        {
            if (game == null || game.HasEnded || (buildPlacement != null && buildPlacement.IsPlacing))
            {
                ClearContext();
                return;
            }

            var nearest = FindNearestNode();
            if (nearest != currentNode)
            {
                currentNode = nearest;
                progress = 0f;
            }

            if (currentNode == null)
            {
                ClearContext();
                return;
            }

            ShowTool(currentNode.Resource);
            var tool = currentNode.Resource == ResourceKind.Wood ? "топор" : "кирка";
            var resource = currentNode.Resource == ResourceKind.Wood ? "дерево" : "камень";
            if (interactAction != null && interactAction.IsPressed())
            {
                progress += Time.deltaTime;
                game.SetContextHint($"Добыча: {tool} · {resource} {Mathf.Clamp01(progress / currentNode.GatherSeconds):P0}");
                if (progress >= currentNode.GatherSeconds)
                {
                    progress = 0f;
                    if (currentNode.TryGather(out var kind, out var amount))
                    {
                        game.CollectResource(kind, amount);
                    }

                    if (currentNode.IsDepleted)
                    {
                        currentNode = null;
                    }
                }
            }
            else
            {
                progress = 0f;
                game.SetContextHint($"Удерживайте E: {tool} · {resource}");
            }
        }

        private GatherableNode FindNearestNode()
        {
            GatherableNode nearest = null;
            var bestDistance = interactionRange;
            if (nodes == null)
            {
                return null;
            }

            foreach (var node in nodes)
            {
                if (node == null || !node.gameObject.activeInHierarchy || node.IsDepleted)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, node.transform.position);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    nearest = node;
                }
            }

            return nearest;
        }

        private void ShowTool(ResourceKind resource)
        {
            if (toolMarker == null)
            {
                return;
            }

            toolMarker.enabled = true;
            toolMarker.color = resource == ResourceKind.Wood
                ? new Color(0.83f, 0.55f, 0.24f)
                : new Color(0.68f, 0.75f, 0.8f);
            toolMarker.transform.localRotation = Quaternion.Euler(0f, 0f, resource == ResourceKind.Wood ? -35f : 35f);
        }

        private void ClearContext()
        {
            currentNode = null;
            progress = 0f;
            if (toolMarker != null)
            {
                toolMarker.enabled = false;
            }
        }
    }
}
