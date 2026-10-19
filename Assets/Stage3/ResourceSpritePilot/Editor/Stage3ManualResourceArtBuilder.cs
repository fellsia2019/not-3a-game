using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Explicit D-071 import. Original PNGs and all legacy sprite local IDs are retained.</summary>
    public static class Stage3ManualResourceArtBuilder
    {
        public const string Source = "art/source/manual_resources_motion_runtime_2026-10-18/";
        public const string ResourceFolder = "Assets/Stage3/ResourceSpritePilot/ManualArt/";
        public const string ArtPath = ResourceFolder + "ManualResourceArt.asset";
        private const string HeroPath = "Assets/Stage3/DayNight/HeroArt.asset";
        private const string SkeletonPath = "Assets/Stage3/SkeletonSpritePilot/SkeletonWalks.asset";

        [MenuItem("Tools/Stage 3/Manual Resources/Build Approved Art")]
        public static void BuildApprovedArt()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing approved manual resource artwork.");
            EnsureFolder(ResourceFolder.TrimEnd('/'));
            var resources = AssetDatabase.LoadAssetAtPath<Stage3ManualResourceArt>(ArtPath);
            if (resources == null)
            {
                resources = ScriptableObject.CreateInstance<Stage3ManualResourceArt>();
                AssetDatabase.CreateAsset(resources, ArtPath);
            }
            resources.stoneStates = ResourceFrames(resources, Import("stone_states", ResourceFolder, true),
                new[] { 1.45f, 2.45f, 4.05f }, out resources.stoneHarvestRadii);
            resources.oreStates = ResourceFrames(resources, Import("ore_states", ResourceFolder, true),
                new[] { 1.5f, 2.55f, 4.2f }, out resources.oreHarvestRadii);
            var tree = Import("ordinary_tree", ResourceFolder, true);
            var treeRect = new RectInt(0, 0, tree.width, tree.height);
            var treeBounds = OpaqueBounds(tree, treeRect);
            resources.tree = SpriteAsset(resources, "D071 ordinary conifer", tree, treeRect,
                new Vector2(treeBounds.center.x, treeBounds.yMin), treeBounds.width / 1.4f);
            RefreshVisibleTops(resources);
            EditorUtility.SetDirty(resources);
            BindResourcePrefab(Stage3ResourceSpriteBuilder.WoodPrefab, resources, MapResourceKind.Wood);
            BindResourcePrefab(Stage3ResourceSpriteBuilder.StonePrefab, resources, MapResourceKind.Stone);
            BindResourcePrefab(Stage3ResourceSpriteBuilder.OrePrefab, resources, MapResourceKind.Metal);

            var hero = AssetDatabase.LoadAssetAtPath<Stage3HeroArt>(HeroPath);
            if (hero == null) throw new InvalidOperationException("Existing Hero C art is missing: " + HeroPath);
            hero.walkFrames = WalkDirections(hero, "hero_walk_", "Assets/Stage3/DayNight/", 1.15f, out var grips);
            hero.walkGripOffsets = grips;
            hero.mirrorLeftWalk = true;
            hero.strideCells = 1.8f;
            hero.axeWorkFrames = WorkDirections(hero, "hero_axe");
            hero.pickWorkFrames = WorkDirections(hero, "hero_pick");
            hero.mirrorLeftWork = true;
            EditorUtility.SetDirty(hero);

            var skeleton = AssetDatabase.LoadAssetAtPath<Stage3SkeletonSpriteSet>(SkeletonPath);
            if (skeleton == null) throw new InvalidOperationException("Existing skeleton set is missing.");
            skeleton.ConfigureExtended(WalkDirections(skeleton, "skeleton_walk_",
                "Assets/Stage3/SkeletonSpritePilot/", .78f, out _), true);
            EditorUtility.SetDirty(skeleton);
            AssetDatabase.SaveAssets();
            foreach (var name in new[] { "stone_states", "ore_states", "ordinary_tree" })
                EndRead(ResourceFolder + name + ".png");
            foreach (var name in new[] { "hero_walk_right", "hero_walk_back", "hero_walk_front", "hero_axe_work", "hero_pick_work",
                "hero_axe_back", "hero_axe_front", "hero_pick_back", "hero_pick_front" })
                EndRead("Assets/Stage3/DayNight/" + name + ".png");
            foreach (var name in new[] { "right", "back", "front" })
                EndRead("Assets/Stage3/SkeletonSpritePilot/skeleton_walk_" + name + ".png");
            Debug.Log("S3_MANUAL_RESOURCE_ART_READY: three size/four depletion states; separate tree; hero/skeleton eight-frame walks; six-pose tool actions. Legacy references and scene preserved.");
        }

        [MenuItem("Tools/Stage 3/Manual Resources/Refresh Interaction Anchors")]
        public static void RefreshInteractionAnchors()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before refreshing interaction anchors.");
            // Reuse current sprite rect/pivot/PPU and original pixels. No SpriteAsset calls,
            // character imports, geometry changes, prefab wiring or scene edits.
            var readability = new Dictionary<string, bool>();
            try
            {
                foreach (var name in new[] { "stone_states", "ore_states", "ordinary_tree" })
                {
                    var path = ResourceFolder + name + ".png";
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    if (importer == null) throw new InvalidOperationException("Resource texture missing: " + path);
                    readability.Add(path, importer.isReadable);
                    if (!importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
                }
                var art = AssetDatabase.LoadAssetAtPath<Stage3ManualResourceArt>(ArtPath);
                if (art == null) throw new InvalidOperationException("Manual resource art is missing.");
                RefreshVisibleTops(art);
                EditorUtility.SetDirty(art);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                foreach (var entry in readability)
                {
                    var importer = (TextureImporter)AssetImporter.GetAtPath(entry.Key);
                    if (importer.isReadable != entry.Value)
                    { importer.isReadable = entry.Value; importer.SaveAndReimport(); }
                }
            }
            Debug.Log("S3_RESOURCE_INTERACTION_ANCHORS_READY: 25 visible silhouette anchors; source pixels and sprite geometry unchanged.");
        }

        private static void RefreshVisibleTops(Stage3ManualResourceArt art)
        {
            Vector2 Top(Sprite sprite)
            {
                if (sprite == null) throw new InvalidOperationException("Missing resource sprite for interaction anchor.");
                var rect = sprite.rect;
                var bounds = OpaqueBounds(sprite.texture, new RectInt(Mathf.RoundToInt(rect.x),
                    Mathf.RoundToInt(rect.y), Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height)));
                return (new Vector2(bounds.center.x - rect.x, bounds.yMax - rect.y) - sprite.pivot) /
                    sprite.pixelsPerUnit;
            }
            art.treeVisibleTop = Top(art.tree);
            art.stoneVisibleTops = art.stoneStates.Select(Top).ToArray();
            art.oreVisibleTops = art.oreStates.Select(Top).ToArray();
        }

        private static Sprite[] ResourceFrames(ScriptableObject owner, Texture2D texture, float[] widths, out float[] radii)
        {
            var result = new Sprite[12];
            radii = new float[12];
            var cells = GridCells(texture, 4, 3);
            for (var row = 0; row < 3; row++)
            {
                var first = cells[row * 4];
                var full = OpaqueBounds(texture, first);
                var ppu = full.width / widths[row];
                for (var column = 0; column < 4; column++)
                {
                    var rect = cells[row * 4 + column];
                    var bounds = OpaqueBounds(texture, rect);
                    radii[row * 4 + column] = new[] { .65f, 1.1f, 1.8f }[row] * bounds.height / full.height;
                    // One PPU per size keeps depleted masses smaller; never refit each remaining pile.
                    result[row * 4 + column] = SpriteAsset(owner, texture.name + " size" + row + " state" + column,
                        texture, rect, new Vector2(bounds.center.x - rect.x, bounds.yMin - rect.y +
                            radii[row * 4 + column] * .819152f * ppu), ppu);
                }
            }
            return result;
        }

        private static Sprite[] WalkDirections(ScriptableObject owner, string prefix, string folder,
            float standingHeight, out Vector2[] grips)
        {
            var result = new Sprite[32]; grips = new Vector2[32];
            foreach (var pair in new[] { ("right", 0), ("back", 1), ("front", 3) })
            {
                var texture = Import(prefix + pair.Item1, folder, false);
                var cells = GridCells(texture, 4, 2);
                var bounds = cells.Select(cell => OpaqueBounds(texture, cell)).ToArray();
                var heights = bounds.Select(b => b.height).OrderBy(h => h).ToArray();
                var referenceHeight = heights[heights.Length / 2];
                var ppu = referenceHeight / standingHeight;
                var pixels = texture.GetPixels32();
                for (var frame = 0; frame < 8; frame++)
                {
                    var rect = cells[frame]; var visible = bounds[frame];
                    var pivot = new Vector2(TorsoCenter(pixels, texture.width, visible, referenceHeight) - rect.x,
                        visible.yMin - rect.y);
                    var index = pair.Item2 * 8 + frame;
                    result[index] = SpriteAsset(owner, texture.name + " frame" + frame, texture, rect, pivot, ppu);
                    grips[index] = new Vector2(.16f, standingHeight * .48f);
                    if (pair.Item2 == 0)
                    {
                        result[16 + frame] = result[index];
                        grips[16 + frame] = new Vector2(-grips[index].x, grips[index].y);
                    }
                }
            }
            return result;
        }

        private static Sprite[] WorkDirections(ScriptableObject owner, string prefix)
        {
            const string folder = "Assets/Stage3/DayNight/";
            var result = ActorFrames(owner, Import(prefix + "_work", folder, false, true),
                6, 1.15f, true, out _);
            var axe = prefix == "hero_axe";
            // Sole lines are authored against the untouched 1536x1024 sheets. The front
            // tool reaches below the boots, so the opaque image bottom is not a foot anchor.
            var back = ActorFrames(owner, Import(prefix + "_back", folder, false, true),
                3, 1.15f, true, out _, 2, axe ? new[] { 474f, 949f } : new[] { 487f, 977f });
            var front = ActorFrames(owner, Import(prefix + "_front", folder, false, true),
                3, 1.15f, true, out _, 2, axe ? new[] { 475f, 947f } : new[] { 497f, 930f });
            Array.Copy(back, 0, result, 6, 6);
            Array.Copy(result, 0, result, 12, 6);
            Array.Copy(front, 0, result, 18, 6);
            return result;
        }

        private static Sprite[] ActorFrames(ScriptableObject owner, Texture2D texture, int columns,
            float standingHeight, bool working, out Vector2[] grips, int rows = 4, float[] solesFromTop = null)
        {
            var result = new Sprite[columns * rows];
            grips = new Vector2[result.Length];
            // Raised tools cross nominal row boundaries. Each connected actor gets a native
            // sprite mesh, so its rectangular UV area cannot reveal a neighbouring actor.
            var components = ActorComponents(texture, columns, rows);
            var bounds = components.Select(component => component.bounds).ToArray();
            // Work neutral poses define body scale; raised tools must not shrink the character.
            var heights = bounds.Where((_, i) => !working || i % columns == 0).Select(b => b.height).OrderBy(h => h).ToArray();
            var referenceHeight = solesFromTop == null ? heights[heights.Length / 2] :
                Mathf.RoundToInt(bounds[0].yMax - (texture.height - solesFromTop[0] * texture.height / 1024f));
            var ppu = referenceHeight / standingHeight;
            var pixels = texture.GetPixels32();
            var metadata = new SpriteMetaData[result.Length];
            var importData = new WorkImportData { frames = new WorkFrameImport[result.Length] };
            for (var row = 0; row < rows; row++) for (var column = 0; column < columns; column++)
            {
                var index = row * columns + column;
                var rect = components[index].rect;
                var visible = bounds[index];
                var anchorX = TorsoCenter(pixels, texture.width, visible, referenceHeight);
                var soleY = solesFromTop == null ? visible.yMin : texture.height - solesFromTop[row] * texture.height / 1024f;
                var pivot = new Vector2(anchorX - rect.x, soleY - rect.y);
                var name = texture.name + " direction" + row + " frame" + column;
                metadata[index] = new SpriteMetaData { name = name, rect = new Rect(rect.x, rect.y, rect.width, rect.height),
                    alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(pivot.x/rect.width, pivot.y/rect.height) };
                importData.frames[index] = new WorkFrameImport { name = name, rect = rect, strips = components[index].strips };
                // Reviewed after import against the actual carrying hand; new source is independent of legacy grips.
                grips[index] = new Vector2((row == 2 ? -.16f : .16f), standingHeight * .48f);
            }
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.Tight; settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
#pragma warning disable 618
            importer.spritesheet = metadata;
#pragma warning restore 618
            importer.userData = JsonUtility.ToJson(importData);
            importer.SaveAndReimport();
            var imported = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = imported[metadata[i].name];
                if (result[i].vertices.Length != importData.frames[i].strips.Length*4)
                    throw new InvalidOperationException("Imported work mesh was not applied: " + result[i].name);
            }
            return result;
        }

        [Serializable] public sealed class WorkImportData { public WorkFrameImport[] frames; }
        [Serializable] public sealed class WorkFrameImport
        { public string name; public RectInt rect; public RectInt[] strips; }

        private sealed class ActorComponent
        {
            public RectInt bounds, rect;
            public RectInt[] strips;
        }

        private static ActorComponent[] ActorComponents(Texture2D texture, int columns, int rows)
        {
            var pixels = texture.GetPixels32(); var visited = new bool[pixels.Length];
            var components = new List<ActorComponent>(); var queue = new Queue<int>();
            for (var seed = 0; seed < pixels.Length; seed++)
            {
                if (visited[seed] || pixels[seed].a < 128) continue;
                var points = new List<Vector2Int>();
                var minX = texture.width; var minY = texture.height; var maxX = 0; var maxY = 0;
                visited[seed] = true; queue.Enqueue(seed); var count = 0;
                while (queue.Count > 0)
                {
                    var p = queue.Dequeue(); var x = p % texture.width; var y = p / texture.width;
                    count++; minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y);
                    points.Add(new Vector2Int(x, y));
                    for (var dy = -1; dy <= 1; dy++) for (var dx = -1; dx <= 1; dx++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1) continue;
                        var nx = x + dx; var ny = y + dy;
                        if (nx < 0 || nx >= texture.width || ny < 0 || ny >= texture.height)
                        { continue; }
                        var next = ny * texture.width + nx;
                        if (pixels[next].a < 128) { continue; }
                        if (visited[next]) continue;
                        visited[next] = true; queue.Enqueue(next);
                    }
                }
                if (count < pixels.Length * .0005f) continue;
                var left = Mathf.Max(0, minX - 2); var bottom = Mathf.Max(0, minY - 2);
                var right = Mathf.Min(texture.width, maxX + 3); var top = Mathf.Min(texture.height, maxY + 3);
                var rect = new RectInt(left, bottom, right-left, top-bottom);
                // Tight scanline mesh follows this actor, even where neighbouring rows overlap.
                // Native UVs still sample the untouched source; no bitmap is masked or repainted.
                var mask = new bool[rect.width * rect.height];
                var ownPixels = new HashSet<int>(points.Select(point => point.y*texture.width+point.x));
                foreach (var point in points)
                    for (var dy = -2; dy <= 2; dy++) for (var dx = -2; dx <= 2; dx++)
                    {
                        var x = point.x+dx-left; var y = point.y+dy-bottom;
                        if (x < 0 || x >= rect.width || y < 0 || y >= rect.height) continue;
                        var sourcePixel = (bottom+y)*texture.width+left+x;
                        if (pixels[sourcePixel].a >= 128 && !ownPixels.Contains(sourcePixel)) continue;
                        mask[y*rect.width+x] = true;
                    }
                var strips = new List<RectInt>();
                for (var y = 0; y < rect.height; y++)
                {
                    var x = 0;
                    while (x < rect.width)
                    {
                        if (!mask[y*rect.width+x]) { x++; continue; }
                        var start = x;
                        while (x < rect.width && mask[y*rect.width+x]) x++;
                        strips.Add(new RectInt(left+start, bottom+y, x-start, 1));
                    }
                }
                components.Add(new ActorComponent { bounds = new RectInt(minX, minY, maxX-minX+1, maxY-minY+1),
                    rect = rect, strips = strips.ToArray() });
            }
            if (components.Count != columns * rows)
                throw new InvalidOperationException(texture.name + ": expected " + columns * rows +
                    " separate actors, found " + components.Count + ". Review source before importing.");
            // Ground contact, not top of a raised weapon, determines the authored row.
            var ordered = components.OrderByDescending(c => c.bounds.yMin).ToArray();
            for (var row = 0; row < rows; row++)
            {
                var group = ordered.Skip(row * columns).Take(columns).OrderBy(c => c.bounds.center.x).ToArray();
                Array.Copy(group, 0, ordered, row * columns, columns);
            }
            return ordered;
        }

        private static float TorsoCenter(Color32[] pixels, int width, RectInt bounds, int standingPixels)
        {
            double weightedX = 0, total = 0;
            var bottom = bounds.yMin + Mathf.RoundToInt(standingPixels * .43f);
            var top = Mathf.Min(bounds.yMax, bounds.yMin + Mathf.RoundToInt(standingPixels * .65f));
            for (var y = bottom; y < top; y++) for (var x = bounds.xMin; x < bounds.xMax; x++)
            {
                var pixel = pixels[y * width + x];
                if (pixel.a < 128) continue;
                weightedX += (x + .5f) * pixel.a; total += pixel.a;
            }
            return total > 0 ? (float)(weightedX / total) : bounds.center.x;
        }

        private static RectInt[] GridCells(Texture2D texture, int columns, int rows)
        {
            // Keep source pixels untouched. Native slices follow actual transparent gutters,
            // which can differ from an exactly equal grid in the generated production sheets.
            var pixels = texture.GetPixels32(); var rowInk = new int[texture.height];
            for (var y = 0; y < texture.height; y++) for (var x = 0; x < texture.width; x++)
                if (pixels[y * texture.width + x].a >= 128) rowInk[y]++;
            var yCuts = Cuts(rowInk, rows);
            var result = new RectInt[columns * rows];
            for (var row = 0; row < rows; row++)
            {
                var bottom = yCuts[rows - row - 1]; var top = yCuts[rows - row];
                var columnInk = new int[texture.width];
                for (var y = bottom; y < top; y++) for (var x = 0; x < texture.width; x++)
                    if (pixels[y * texture.width + x].a >= 128) columnInk[x]++;
                var xCuts = Cuts(columnInk, columns);
                for (var column = 0; column < columns; column++)
                    result[row * columns + column] = new RectInt(xCuts[column], bottom,
                        xCuts[column + 1] - xCuts[column], top - bottom);
            }
            return result;
        }

        private static int[] Cuts(int[] ink, int divisions)
        {
            var cuts = new int[divisions + 1]; cuts[divisions] = ink.Length;
            var step = ink.Length / (float)divisions;
            for (var i = 1; i < divisions; i++)
            {
                var desired = Mathf.RoundToInt(i * step); var selected = -1; var distance = int.MaxValue;
                var radius = Mathf.FloorToInt(step * .3f);
                for (var p = Mathf.Max(1, desired - radius); p <= Mathf.Min(ink.Length - 2, desired + radius); p++)
                    if (ink[p - 1] == 0 && ink[p] == 0 && ink[p + 1] == 0 && Mathf.Abs(p - desired) < distance)
                    { selected = p; distance = Mathf.Abs(p - desired); }
                if (selected < 0) throw new InvalidOperationException("No clear sprite gutter near " + desired + "; production sheet needs review.");
                cuts[i] = selected;
            }
            return cuts;
        }

        private static RectInt OpaqueBounds(Texture2D texture, RectInt rect)
        {
            var pixels = texture.GetPixels32();
            var left = rect.xMax; var right = rect.xMin; var bottom = rect.yMax; var top = rect.yMin;
            for (var y = rect.yMin; y < rect.yMax; y++) for (var x = rect.xMin; x < rect.xMax; x++)
            {
                if (pixels[y * texture.width + x].a < 128) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1);
                bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1);
            }
            if (left >= right || bottom >= top) throw new InvalidOperationException("Empty art frame " + texture.name + " " + rect);
            if (left <= rect.xMin || right >= rect.xMax || bottom <= rect.yMin || top >= rect.yMax)
                throw new InvalidOperationException("Art touches its slice boundary: " + texture.name + " " + rect);
            return new RectInt(left, bottom, right - left, top - bottom);
        }

        private static Texture2D Import(string name, string destinationFolder, bool mipmaps, bool workSprites = false)
        {
            var source = Source + name + ".png";
            var destination = destinationFolder + name + ".png";
            if (!File.Exists(source)) throw new InvalidOperationException("Approved production image missing: " + source);
            File.Copy(source, destination, true);
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(destination);
            importer.textureType = workSprites ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true; importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = mipmaps;
            importer.filterMode = mipmaps ? FilterMode.Trilinear : FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(destination);
        }

        private static Sprite SpriteAsset(ScriptableObject owner, string name, Texture2D texture,
            RectInt rect, Vector2 pivot, float ppu)
        {
            var path = AssetDatabase.GetAssetPath(owner);
            var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(s => s.name == name);
            var sprite = Sprite.Create(texture, new Rect(rect.x, rect.y, rect.width, rect.height),
                new Vector2(pivot.x / rect.width, pivot.y / rect.height), ppu, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            if (existing == null) { AssetDatabase.AddObjectToAsset(sprite, owner); return sprite; }
            EditorUtility.CopySerialized(sprite, existing); Object.DestroyImmediate(sprite);
            EditorUtility.SetDirty(existing); return existing;
        }

        internal static void ApplyWorkMesh(Sprite sprite, WorkFrameImport frame)
        {
            var strips = frame.strips; var rect = frame.rect; var name = frame.name;

                if (strips.Length * 4 > ushort.MaxValue)
                    throw new InvalidOperationException("Native sprite mesh exceeds the vertex limit: " + name);
                var vertices = new Vector2[strips.Length*4]; var triangles = new ushort[strips.Length*6];
                // OverrideGeometry expects rect-local pixels; Unity applies pivot/PPU itself.
                var origin = new Vector2(rect.x, rect.y);
                for (var i = 0; i < strips.Length; i++)
                {
                    var strip = strips[i]; var v = i*4; var t = i*6;
                    vertices[v] = new Vector2(strip.xMin, strip.yMin)-origin;
                    vertices[v+1] = new Vector2(strip.xMax, strip.yMin)-origin;
                    vertices[v+2] = new Vector2(strip.xMax, strip.yMax)-origin;
                    vertices[v+3] = new Vector2(strip.xMin, strip.yMax)-origin;
                    triangles[t] = (ushort)v; triangles[t+1] = (ushort)(v+1); triangles[t+2] = (ushort)(v+2);
                    triangles[t+3] = (ushort)v; triangles[t+4] = (ushort)(v+2); triangles[t+5] = (ushort)(v+3);
                }
                sprite.OverrideGeometry(vertices, triangles);
                if (sprite.vertices.Length != vertices.Length || sprite.triangles.Length != triangles.Length)
                    throw new InvalidOperationException("Unity rejected the native actor mesh: " + name);
        }

        private static void BindResourcePrefab(string path, Stage3ManualResourceArt art, MapResourceKind kind)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var component = root.GetComponent<Stage3ResourceSprite>();
                var serialized = new SerializedObject(component);
                serialized.FindProperty("art").objectReferenceValue = art;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                component.Body.sprite = art.Frame(kind, Stage2ResourceSize.Small, 0);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void EndRead(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.isReadable = false; importer.SaveAndReimport();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }

    internal sealed class Stage3ManualWorkSpritePostprocessor : AssetPostprocessor
    {
        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            if (!assetPath.StartsWith("Assets/Stage3/DayNight/hero_axe_", StringComparison.Ordinal) &&
                !assetPath.StartsWith("Assets/Stage3/DayNight/hero_pick_", StringComparison.Ordinal)) return;
            if (string.IsNullOrEmpty(assetImporter.userData)) return;
            var data = JsonUtility.FromJson<Stage3ManualResourceArtBuilder.WorkImportData>(assetImporter.userData);
            if (data?.frames == null) return;
            foreach (var sprite in sprites)
            {
                var frame = data.frames.FirstOrDefault(candidate => candidate.name == sprite.name);
                if (frame != null) Stage3ManualResourceArtBuilder.ApplyWorkMesh(sprite, frame);
            }
        }
    }

}
