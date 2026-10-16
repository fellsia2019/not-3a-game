using System;
using System.Collections.Generic;
using Not3A.Stage2.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Small real grass and flower meshes, with no collision or rule ownership.</summary>
    internal static class Stage3LandscapeProps
    {
        private const int BorderDecorationCells = 8;

        internal static void Clear(Transform parent)
        {
            var root = parent.Find("Real landscape grass and flowers");
            if (root == null) return;
            root.gameObject.SetActive(false);
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null) UnityEngine.Object.Destroy(filter.sharedMesh);
            UnityEngine.Object.Destroy(root.gameObject);
        }

        internal static void Build(Stage2MapDefinition map, Transform parent, Vector2[] clearances,
            Material grassBlades, Material flowerPetals, Material flowerCenters, float visualLift,
            IReadOnlyList<ResourceDistrict> districts = null)
        {
            var grass = new MeshBuffer();
            var petals = new MeshBuffer();
            var centers = new MeshBuffer();
            var tuftCount = 0;
            var flowerCount = 0;
            var rockCells = new HashSet<Vector2Int>();
            if (districts != null)
                foreach (var district in districts)
                    if (district.Kind != MapResourceKind.Wood)
                        foreach (var cell in district.Cells) rockCells.Add(cell);

            for (var x = map.MinimumCell.x - BorderDecorationCells;
                 x <= map.MaximumCell.x + BorderDecorationCells; x++)
            for (var y = map.MinimumCell.y - BorderDecorationCells;
                 y <= map.MaximumCell.y + BorderDecorationCells; y++)
            {
                var cell = new Vector2Int(x, y);
                if (rockCells.Contains(cell)) continue;
                if (map.Contains(cell) && map.GetSurface(cell) != MapSurfaceKind.Forest) continue;
                var seed = Hash(x, y, 0x6D2B79F5u);
                var px = x + (Unit(Hash(x, y, 31)) - .5f) * .5f;
                var py = y + (Unit(Hash(x, y, 67)) - .5f) * .5f;
                var point = new Vector2(px, py);
                var nearObjective = false;
                foreach (var clearance in clearances)
                    if (Vector2.Distance(point, clearance) < 1.45f) nearObjective = true;
                if (nearObjective) continue;

                if (seed % 3 == 0)
                {
                    AddGrassTuft(grass, point, seed);
                    tuftCount++;
                }
                if (seed % 6 == 2)
                {
                    AddFlower(petals, centers, point + new Vector2(.25f, -.17f), seed);
                    flowerCount++;
                }

            }

            var root = new GameObject("Real landscape grass and flowers");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0f, -visualLift);
            CreatePart(root.transform, "Individual grass blades", grass, grassBlades, false);
            CreatePart(root.transform, "Flower petals", petals, flowerPetals, false);
            CreatePart(root.transform, "Flower centers", centers, flowerCenters, false);
            Debug.Log($"S3_LANDSCAPE_PROPS_READY bushes=0 tufts={tuftCount} flowers={flowerCount} " +
                $"triangles={grass.Triangles + petals.Triangles + centers.Triangles} " +
                "colliders=0");
        }

        private static void CreatePart(Transform parent, string name, MeshBuffer buffer,
            Material material, bool shadows, Color? tint = null)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.AddComponent<MeshFilter>().sharedMesh = buffer.ToMesh(name);
            var renderer = item.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (tint.HasValue)
            {
                var properties = new MaterialPropertyBlock();
                properties.SetColor("_BaseColor", tint.Value);
                renderer.SetPropertyBlock(properties);
            }
        }

        private static void AddGrassTuft(MeshBuffer grass, Vector2 point, uint seed)
        {
            var rng = new System.Random(unchecked((int)seed));
            for (var blade = 0; blade < 7; blade++)
            {
                var angle = (blade + (float)rng.NextDouble() * .4f) * Mathf.PI * 2f / 7f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var side = new Vector3(-direction.y, direction.x, 0f);
                var basePosition = new Vector3(point.x, point.y,
                    Stage3TacticalPresentation.GrassSurfaceZ(point) +
                    Stage3TacticalPresentation.GrassVisualLift - .008f);
                var height = .09f + .10f * (float)rng.NextDouble();
                var left = basePosition - side * .017f;
                var right = basePosition + side * .017f;
                var tip = basePosition + direction * (.03f + height * .4f) + Vector3.back * height;
                grass.AddTriangle(left, right, tip, Vector3.back);
            }
        }

        private static void AddFlower(MeshBuffer petals, MeshBuffer centers, Vector2 point, uint seed)
        {
            var center = new Vector3(point.x, point.y,
                Stage3TacticalPresentation.GrassSurfaceZ(point) +
                Stage3TacticalPresentation.GrassVisualLift - .008f);
            var turn = Unit(seed) * Mathf.PI * 2f;
            for (var i = 0; i < 5; i++)
            {
                var angle = turn + i * Mathf.PI * 2f / 5f;
                var forward = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var side = new Vector3(-forward.y, forward.x, 0f);
                petals.AddTriangle(center, center + forward * .07f + side * .028f,
                    center + forward * .07f - side * .028f, Vector3.back);
            }
            var a = center + new Vector3(-.016f, -.014f, -.005f);
            var b = center + new Vector3(.016f, -.014f, -.005f);
            var c = center + new Vector3(0, .017f, -.005f);
            centers.AddTriangle(a, b, c, Vector3.back);
        }

        private static uint Hash(int x, int y, uint seed)
        {
            unchecked
            {
                var value = (uint)x * 374761393u + (uint)y * 668265263u + seed;
                value = (value ^ (value >> 13)) * 1274126177u;
                return value ^ (value >> 16);
            }
        }

        private static float Unit(uint value) => (value & 0x00FFFFFF) / 16777216f;

        private sealed class MeshBuffer
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<int> indices = new List<int>();
            internal int Triangles => indices.Count / 3;

            internal void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3? fixedNormal)
            {
                var normal = fixedNormal ?? Vector3.Cross(b - a, c - a).normalized;
                AddTriangle(a, b, c, normal, normal, normal);
            }

            internal void AddTriangle(Vector3 a, Vector3 b, Vector3 c,
                Vector3 normalA, Vector3 normalB, Vector3 normalC)
            {
                var start = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                normals.Add(normalA); normals.Add(normalB); normals.Add(normalC);
                indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
            }

            internal Mesh ToMesh(string name)
            {
                var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTriangles(indices, 0);
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
