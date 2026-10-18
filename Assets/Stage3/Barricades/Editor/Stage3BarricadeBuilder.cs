using System;
using Not3A.Stage2.Defense;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Imports the approved, face-on A artwork without modifying its pixels.
    /// Scene wiring remains owned by the integration builder.</summary>
    public static class Stage3BarricadeBuilder
    {
        public const string Folder = "Assets/Stage3/Barricades/";
        public const string TexturePath = Folder + "WoodenBarricadeA.png";
        public const string PrefabPath = Folder + "WoodenBarricadeA.prefab";
        public const string SpriteSetPath = Folder + "WoodenBarricadeSprites.asset";
        public const string CatalogPath = Folder + "WoodenBarricadeCatalog.asset";
        public const float CellWidth = .90f;
        public const float FrontInset = .02f;

        [MenuItem("Tools/Stage 3/Barricade/Build Visual Assets")]
        public static void BuildAssets()
        {
            var prefab = BuildVisuals();
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<Stage2DefenseCatalog>();
                // Owner-delegated provisional balance; later Inspector tuning survives an art rebuild.
                catalog.Configure(prefab, 1, 0, 0, 12, 1f, 1f, 0);
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("towerPrefab").objectReferenceValue = prefab;
            serialized.FindProperty("roleName").stringValue = "Частокол";
            serialized.FindProperty("canAttack").boolValue = false;
            serialized.FindProperty("attackDamage").intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            if (!catalog.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_BARRICADE_ART_READY: approved A, camera-facing, .90 cell width, shared preview/built registration.");
        }

        public static Stage2Tower BuildVisuals()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing barricade artwork.");
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("Place the unchanged approved transparent front-facing sprite at " + TexturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.5f, 0f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = 1024f;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
            if (sprite == null) throw new InvalidOperationException("Barricade sprite import failed.");
            var bounds = VisibleBounds(texture, sprite);
            var set = AssetDatabase.LoadAssetAtPath<Stage3TowerSpriteSet>(SpriteSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<Stage3TowerSpriteSet>();
                AssetDatabase.CreateAsset(set, SpriteSetPath);
            }
            set.Configure(new[] { sprite }, new[] { Vector2.zero });
            set.ConfigureParts(null, null);
            set.ConfigureCellFit(bounds, CellWidth, FrontInset);
            EditorUtility.SetDirty(set);

            var shader = Shader.Find("Not3A/Skeleton Sprite Unlit");
            if (shader == null) throw new InvalidOperationException("Existing sprite shader missing.");
            var material = MaterialAsset("BarricadeInk.mat", shader, Color.white);
            var shadeMaterial = MaterialAsset("BarricadeShadow.mat", shader, new Color(0f, 0f, 0f, .23f));
            var old = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = old == null ? new GameObject("Wooden Barricade A") : PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (root.GetComponent<Stage2Tower>() == null) root.AddComponent<Stage2Tower>();
                var placeholder = root.GetComponent<SpriteRenderer>();
                if (placeholder == null) placeholder = root.AddComponent<SpriteRenderer>();
                placeholder.enabled = false;
                var art = Child(root.transform, "Barricade artwork");
                var renderer = art.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = art.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = material;
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var view = art.GetComponent<Stage3TowerSprite>();
                if (view == null) view = art.gameObject.AddComponent<Stage3TowerSprite>();
                Set(view, "artwork", set);
                Set(view, "body", renderer);

                var shadow = Child(root.transform, "Barricade contact shadow");
                // Ground-plane shadow stays inside the same cell and meets the front feet.
                shadow.localPosition = new Vector3(0f, -.5f + FrontInset + .11f, -.014f);
                var filter = shadow.GetComponent<MeshFilter>();
                if (filter == null) filter = shadow.gameObject.AddComponent<MeshFilter>();
                filter.sharedMesh = ContactShadow();
                var shade = shadow.GetComponent<MeshRenderer>();
                if (shade == null) shade = shadow.gameObject.AddComponent<MeshRenderer>();
                shade.sharedMaterial = shadeMaterial;
                shade.sortingOrder = -10000;
                shade.shadowCastingMode = ShadowCastingMode.Off;
                shade.receiveShadows = false;
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                if (old == null) Object.DestroyImmediate(root);
                else PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            importer.isReadable = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Stage2Tower>(PrefabPath);
        }

        private static Rect VisibleBounds(Texture2D texture, Sprite sprite)
        {
            var pixels = texture.GetPixels32();
            var rows = new int[texture.height];
            var columns = new int[texture.width];
            var transparent = 0;
            for (var y = 0; y < texture.height; y++)
            for (var x = 0; x < texture.width; x++)
            {
                var alpha = pixels[y * texture.width + x].a;
                if (alpha >= 128) { rows[y]++; columns[x]++; }
                if (alpha == 0) transparent++;
            }
            if (transparent < pixels.Length / 10)
                throw new InvalidOperationException("Barricade source must be an isolated transparent sprite, not an approval sheet.");
            var rowMinimum = Mathf.Max(4, Mathf.CeilToInt(texture.width * .005f));
            var columnMinimum = Mathf.Max(4, Mathf.CeilToInt(texture.height * .005f));
            var left = Array.FindIndex(columns, n => n >= columnMinimum);
            var right = Array.FindLastIndex(columns, n => n >= columnMinimum) + 1;
            var bottom = Array.FindIndex(rows, n => n >= rowMinimum);
            var top = Array.FindLastIndex(rows, n => n >= rowMinimum) + 1;
            if (left < 0 || bottom < 0) throw new InvalidOperationException("Barricade artwork has no visible body.");
            return new Rect((left - sprite.pivot.x) / sprite.pixelsPerUnit,
                (bottom - sprite.pivot.y) / sprite.pixelsPerUnit,
                (right - left) / sprite.pixelsPerUnit, (top - bottom) / sprite.pixelsPerUnit);
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) return child;
            child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static Material MaterialAsset(string name, Shader shader, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + name);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, Folder + name); }
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh ContactShadow()
        {
            var path = Folder + "BarricadeShadow.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            const int count = 24;
            var vertices = new Vector3[count + 1];
            var colors = new Color[count + 1];
            var triangles = new int[count * 3];
            colors[0] = Color.white;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * CellWidth * .5f, Mathf.Sin(angle) * .11f, 0f);
                colors[i + 1] = new Color(1f, 1f, 1f, 0f);
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % count + 1;
            }
            mesh = new Mesh { name = "Barricade ground contact", vertices = vertices,
                colors = colors, uv = new Vector2[count + 1], triangles = triangles };
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
