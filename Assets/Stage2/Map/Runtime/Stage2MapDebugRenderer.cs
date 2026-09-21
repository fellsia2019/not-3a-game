using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage2.Map
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class Stage2MapDebugRenderer : MonoBehaviour
    {
        [SerializeField] private Stage2MapDefinition mapDefinition;
        [SerializeField] private int sortingOrder = -1000;

        private Mesh generatedMesh;
        private Material generatedMaterial;

        public Stage2MapDefinition MapDefinition => mapDefinition;

        public void Configure(Stage2MapDefinition definition)
        {
            mapDefinition = definition;
            Rebuild();
        }

        public void Rebuild()
        {
            ReleaseGeneratedObjects();
            if (mapDefinition == null)
            {
                return;
            }

            if (!TryBuildMesh(mapDefinition, out generatedMesh, out var error))
            {
                Debug.LogError($"Stage 2 map debug renderer cannot build: {error}", this);
                return;
            }

            generatedMesh.hideFlags = HideFlags.DontSave;
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = generatedMesh;

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("Stage 2 map debug renderer requires the built-in Sprites/Default shader.", this);
                return;
            }

            generatedMaterial = new Material(shader)
            {
                name = "Stage 2 Map Debug Material",
                hideFlags = HideFlags.DontSave
            };
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = generatedMaterial;
            meshRenderer.sortingOrder = sortingOrder;
        }

        public static bool TryBuildMesh(Stage2MapDefinition definition, out Mesh mesh, out string error)
        {
            mesh = null;
            if (definition == null)
            {
                error = "Map definition is missing.";
                return false;
            }

            var validation = definition.ValidateDefinition();
            if (!validation.IsValid)
            {
                error = validation.FirstError;
                return false;
            }

            var cellCount = checked(definition.Size.x * definition.Size.y);
            var vertices = new List<Vector3>(checked(cellCount * 4));
            var colors = new List<Color32>(checked(cellCount * 4));
            var triangles = new List<int>(checked(cellCount * 6));
            var halfWidth = Not3A.Stage1.IsoGrid.CellSize.x * 0.5f;
            var halfHeight = Not3A.Stage1.IsoGrid.CellSize.y * 0.5f;

            for (var xOffset = 0; xOffset < definition.Size.x; xOffset++)
            {
                for (var yOffset = 0; yOffset < definition.Size.y; yOffset++)
                {
                    var cell = new Vector2Int(
                        definition.MinimumCell.x + xOffset,
                        definition.MinimumCell.y + yOffset);
                    var center = definition.CellToWorld(cell);
                    var firstVertex = vertices.Count;
                    vertices.Add(new Vector3(center.x - halfWidth, center.y, 0f));
                    vertices.Add(new Vector3(center.x, center.y + halfHeight, 0f));
                    vertices.Add(new Vector3(center.x + halfWidth, center.y, 0f));
                    vertices.Add(new Vector3(center.x, center.y - halfHeight, 0f));

                    var color = (Color32)(definition.GetSurface(cell) == MapSurfaceKind.Forest
                        ? definition.ForestDebugColor
                        : definition.CorridorDebugColor);
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);

                    triangles.Add(firstVertex);
                    triangles.Add(firstVertex + 1);
                    triangles.Add(firstVertex + 2);
                    triangles.Add(firstVertex);
                    triangles.Add(firstVertex + 2);
                    triangles.Add(firstVertex + 3);
                }
            }

            mesh = new Mesh
            {
                name = $"{definition.name} Debug Surface",
                indexFormat = vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0, true);
            mesh.RecalculateBounds();
            error = string.Empty;
            return true;
        }

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Rebuild();
            }
        }

        private void OnDisable()
        {
            ReleaseGeneratedObjects();
        }

        private void ReleaseGeneratedObjects()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh == generatedMesh)
            {
                meshFilter.sharedMesh = null;
            }

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.sharedMaterial == generatedMaterial)
            {
                meshRenderer.sharedMaterial = null;
            }

            DestroyGeneratedObject(generatedMesh);
            DestroyGeneratedObject(generatedMaterial);
            generatedMesh = null;
            generatedMaterial = null;
        }

        private static void DestroyGeneratedObject(Object generatedObject)
        {
            if (generatedObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedObject);
            }
            else
            {
                DestroyImmediate(generatedObject);
            }
        }
    }
}
