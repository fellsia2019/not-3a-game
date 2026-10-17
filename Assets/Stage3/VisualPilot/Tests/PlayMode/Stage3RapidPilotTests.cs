#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
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
    public sealed class Stage3RapidPilotTests
    {
        private InputTestFixture input;

        [UnitySetUp]
        public IEnumerator LoadPilot()
        {
            input = new InputTestFixture();
            input.Setup();
            InputSystem.AddDevice<Keyboard>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3RapidSpritePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator CleanUp()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null;
            input?.TearDown();
        }

        private static Color32[] CapturePortal(Stage2SceneBootstrap bootstrap, bool objective = false)
        {
            var camera=bootstrap.CameraFollow.BoundCamera;
            var position=camera.transform.position;
            var size=camera.orthographicSize;
            var oldTarget=camera.targetTexture;
            var oldActive=RenderTexture.active;
            var target=RenderTexture.GetTemporary(320,320,24);
            var image=new Texture2D(320,320,TextureFormat.RGB24,false);
            try
            {
                var focus=(objective ? bootstrap.ThroneMarker.position : bootstrap.EntranceMarker.position)
                    + Vector3.back*(objective ? .6f : 1.4f);
                camera.transform.position=focus-camera.transform.forward*12;
                camera.orthographicSize=2.6f;
                camera.targetTexture=target;
                camera.Render();
                RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,320,320),0,0);
                image.Apply();
                return image.GetPixels32();
            }
            finally
            {
                camera.transform.position=position;
                camera.orthographicSize=size;
                camera.targetTexture=oldTarget;
                RenderTexture.active=oldActive;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(image);
            }
        }

        private static int ChangedRedPixels(Color32[] before,Color32[] after)
        {
            var changed=0;
            for(var i=0;i<before.Length;i++)
            {
                var a=before[i]; var b=after[i];
                if((a.r>40 && a.r>a.g*2 && a.r>a.b*2) ||
                    (b.r>40 && b.r>b.g*2 && b.r>b.b*2))
                    if(Mathf.Abs(a.r-b.r)>12)changed++;
            }
            return changed;
        }

        private static void AssertBaseVisible(Stage2SceneBootstrap bootstrap, SpriteRenderer artwork,
            bool objective, params Vector2[] samples)
        {
            var visible=CapturePortal(bootstrap,objective);
            artwork.enabled=false;
            Color32[] ground;
            try { ground=CapturePortal(bootstrap,objective); }
            finally { artwork.enabled=true; }
            var camera=bootstrap.CameraFollow.BoundCamera;
            var position=camera.transform.position; var size=camera.orthographicSize;
            try
            {
                var focus=(objective?bootstrap.ThroneMarker.position:bootstrap.EntranceMarker.position)
                    +Vector3.back*(objective?.6f:1.4f);
                camera.transform.position=focus-camera.transform.forward*12;
                camera.orthographicSize=2.6f;
                foreach(var uv in samples)
                {
                    var sprite=artwork.sprite;
                    var local=new Vector3((uv.x*sprite.rect.width-sprite.pivot.x)/sprite.pixelsPerUnit,
                        (uv.y*sprite.rect.height-sprite.pivot.y)/sprite.pixelsPerUnit,0);
                    var point=camera.WorldToViewportPoint(artwork.transform.TransformPoint(local));
                    Assert.That(point.x,Is.InRange(.01f,.99f));
                    Assert.That(point.y,Is.InRange(.01f,.99f));
                    var x=Mathf.RoundToInt(point.x*319); var y=Mathf.RoundToInt(point.y*319);
                    var difference=0;
                    for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++)
                    {
                        var a=visible[(y+dy)*320+x+dx]; var b=ground[(y+dy)*320+x+dx];
                        difference+=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);
                    }
                    Assert.That(difference,Is.GreaterThan(100),
                        $"The foreground step/anchor at sprite UV {uv} must render above the ground.");
                }
            }
            finally { camera.transform.position=position;camera.orthographicSize=size; }
        }

        [UnityTest]
        public IEnumerator CurvedMap_PlacesLandmarksResourcesAndBuildArea()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap=Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var map=bootstrap.MapDefinition;
            Assert.That(map.ValidateDefinition().IsValid,Is.True);
            Assert.That(map.Size,Is.EqualTo(new Vector2Int(250,250)));
            Assert.That(map.EnemyEntrance,Is.EqualTo(new Vector2Int(-42,12)));
            Assert.That(map.Throne,Is.EqualTo(new Vector2Int(55,12)));
            Assert.That(map.EnemiesStayOnInvasionCorridor,Is.True);
            Assert.That(map.GetSurface(new Vector2Int(-39,-20)),Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            Assert.That(map.GetSurface(new Vector2Int(2,-4)),Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            Assert.That(map.GetSurface(new Vector2Int(-15,-12)),Is.EqualTo(MapSurfaceKind.Forest));
            Assert.That(map.GetSurface(new Vector2Int(20,20)),Is.EqualTo(MapSurfaceKind.Forest));
            for(var index=0;index<3;index++)
            {
                var anchor=map.GetResourceRegion(index).Anchor;
                Assert.That(map.GetSurface(anchor),Is.EqualTo(MapSurfaceKind.Forest));
                Assert.That(map.IsWalkable(anchor),Is.True);
                Assert.That(map.IsBuildable(anchor),Is.False);
            }
            Assert.That(map.GetSurface(map.Throne+new Vector2Int(3,-1)),
                Is.EqualTo(MapSurfaceKind.InvasionCorridor));
            Assert.That(map.GetSurface(map.Throne+new Vector2Int(-20,8)),
                Is.EqualTo(MapSurfaceKind.Forest));
            Assert.That(bootstrap.PlayerMover.transform.position.x,Is.EqualTo(18f));
            Assert.That(bootstrap.PlayerMover.transform.position.y,Is.EqualTo(28f));
            var camera=bootstrap.CameraFollow.BoundCamera;
            Assert.That(camera.orthographicSize,Is.EqualTo(5f).Within(.01f));
            bootstrap.PlayerMover.transform.position=bootstrap.EntranceMarker.position;
            bootstrap.CameraFollow.SnapToTarget();
            var portalOnScreen=camera.WorldToViewportPoint(bootstrap.EntranceMarker.position);
            Assert.That(portalOnScreen.x,Is.InRange(.1f,.9f));
            Assert.That(portalOnScreen.y,Is.InRange(.1f,.9f));
        }

        [UnityTest]
        public IEnumerator CurvedRoad_EnemyTraversesWholeRoadAndPortalIsVisible()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition=Object.FindAnyObjectByType<Stage2GameComposition>();
            var bootstrap=composition.GetComponent<Stage2SceneBootstrap>();
            var map=bootstrap.MapDefinition;
            var run=composition.RunDriver.Run;
            composition.RunDriver.enabled=false;
            run.ResetRun();
            var camera=bootstrap.CameraFollow.BoundCamera;
            bootstrap.CameraFollow.enabled=false;
            camera.orthographicSize=30f;
            camera.farClipPlane=150f;
            camera.transform.position=new Vector3(6.5f,-11f,0)-camera.transform.forward*60f;
            SaveRoadView(camera,"route-overview.png",1600,900);
            camera.orthographicSize=5f;
            camera.transform.position=bootstrap.EntranceMarker.position-camera.transform.forward*20f;
            SaveRoadView(camera,"portal-lower-left.png",1200,900);
            var artwork=bootstrap.EntranceMarker.GetComponentsInChildren<SpriteRenderer>().Single(r=>r.enabled);
            Assert.That(artwork,Is.Not.Null);
            Assert.That(Vector3.Dot(artwork.transform.forward,camera.transform.forward),Is.GreaterThan(.999f));
            var visible=camera.WorldToViewportPoint(bootstrap.EntranceMarker.position);
            Assert.That(visible.x,Is.InRange(.1f,.9f));
            Assert.That(visible.y,Is.InRange(.1f,.9f));
            AssertBaseVisible(bootstrap,artwork,false,new Vector2(.47f,.13f));
            Assert.That(run.StartWave(),Is.True);
            var enemy=composition.Enemies.GetEnemy(0);
            enemy.SetAutomaticTick(false);
            var visited=new HashSet<Vector2Int>();
            var health=run.ThroneHealth;
            for(var i=0;i<400 && enemy!=null && enemy.IsAlive;i++)
            {
                visited.Add(enemy.CurrentCell);
                Assert.That(map.GetSurface(enemy.CurrentCell),Is.EqualTo(MapSurfaceKind.InvasionCorridor));
                enemy.Tick(1f);
                if(i%20==0)yield return null;
            }
            Assert.That(visited.Count,Is.GreaterThan(120),"The ordinary enemy must traverse both detours.");
            Assert.That(run.ThroneHealth,Is.LessThan(health));
            File.WriteAllText("Artifacts/S3-CURVED-ROAD-20261016/traversal.txt",
                $"visitedCells={visited.Count}\nportal={map.EnemyEntrance}\nthrone={map.Throne}\nwidth=7\nheroGrassWalkable={map.IsWalkable(map.GetResourceRegion(0).Anchor)}\n");
        }

        [UnityTest]
        public IEnumerator ExpandedGreenWorld_MovementRenderingAndMinimapReachNewLand()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap=Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var map=bootstrap.MapDefinition;
            var game=bootstrap.GetComponent<Stage2GameComposition>();
            var minimap=bootstrap.GetComponent<Stage2Minimap>();
            Assert.That(minimap.Terrain.width,Is.EqualTo(512));
            Assert.That(minimap.Terrain.height,Is.EqualTo(512));
            var mesh=GameObject.Find("Tactical grass and paved build cells").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh.bounds.min.x,Is.EqualTo(-50.5f).Within(.001f));
            Assert.That(mesh.bounds.max.x,Is.EqualTo(199.5f).Within(.001f));
            Assert.That(mesh.bounds.min.y,Is.EqualTo(-50.5f).Within(.001f));
            Assert.That(mesh.bounds.max.y,Is.EqualTo(199.5f).Within(.001f));
            var districtWorld=game.GetComponent("Stage3ResourceDistrictWorld");
            var districts=(IReadOnlyList<ResourceDistrict>)districtWorld.GetType().GetProperty("Districts").GetValue(districtWorld);
            var reliefCells=districts.Where(d=>d.CutsTerrain && d.Kind!=MapResourceKind.Wood).Sum(d=>d.Cells.Count);
            Assert.That(mesh.vertexCount,Is.EqualTo((250*250-reliefCells)*4),
                "The full world remains covered, with precisely the approved relief footprints cut out of the grass sheet.");

            bootstrap.PlayerMover.transform.position=new Vector3(48.5f,75f,0f);
            bootstrap.CameraFollow.SnapToTarget();
            var camera=bootstrap.CameraFollow.BoundCamera;
            var size=camera.orthographicSize;
            input.Press(Keyboard.current.dKey);
            yield return new WaitForSeconds(.5f);
            input.Release(Keyboard.current.dKey);
            Assert.That(bootstrap.PlayerMover.transform.position.x,Is.GreaterThan(49.5f),
                "WASD must cross the old map boundary without clamping.");
            Assert.That(camera.orthographicSize,Is.EqualTo(size).Within(.01f));
            var hero=minimap.Markers.Single(m=>m.Kind==Stage2MinimapMarkerKind.Hero);
            Assert.That(hero.World.x,Is.GreaterThan(49.5f));
            var onMap=minimap.WorldToMapPoint(hero.World);
            Assert.That(minimap.MapRect.Contains(onMap),Is.True);
            var inventory=game.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood,2);
            inventory.TryAdd(MapResourceKind.Stone,1);
            var gold=inventory.Gold;
            var wood=inventory.Get(MapResourceKind.Wood);
            var stone=inventory.Get(MapResourceKind.Stone);
            Assert.That(game.Building.Defense.BeginPlacement(),Is.True);
            var green=new Vector2Int(75,75);
            game.Building.Defense.SetPreviewCell(green);
            Assert.That(game.Building.Defense.TryConfirm(out var rejected),Is.EqualTo(PlacementResult.InvalidCell));
            Assert.That(rejected,Is.Null);
            Assert.That(inventory.Gold,Is.EqualTo(gold));
            Assert.That(inventory.Get(MapResourceKind.Wood),Is.EqualTo(wood));
            Assert.That(inventory.Get(MapResourceKind.Stone),Is.EqualTo(stone));

            var folder=(System.Environment.GetEnvironmentVariable("NOT3A_DISTRICT_EVIDENCE") ??
                "Artifacts/S3-CURVED-ROAD-20261016").TrimEnd('/','\\')+"/";
            SaveRoadView(camera,"new-green-land.png",1200,900,folder);
            bootstrap.CameraFollow.enabled=false;
            camera.orthographicSize=150f;
            camera.farClipPlane=600f;
            var center=(Vector2)map.MinimumCell+(Vector2)(map.Size-Vector2Int.one)*.5f;
            camera.transform.position=(Vector3)center-camera.transform.forward*300f;
            SaveRoadView(camera,"world-250-overview.png",1600,1000,folder);
            Debug.Log($"S3_WORLD_250_OK cells={map.CellBounds.CellCount} terrainVertices={mesh.vertexCount} " +
                $"terrainBytes={UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh)} " +
                $"minimap={minimap.Terrain.width}x{minimap.Terrain.height} curvedRoad=true");
        }

        private static void SaveRoadView(Camera camera,string name,int width,int height,
            string folder="Artifacts/S3-CURVED-ROAD-20261016/")
        {
            Directory.CreateDirectory(folder);
            var previousTarget=camera.targetTexture; var previousActive=RenderTexture.active;
            var target=RenderTexture.GetTemporary(width,height,24);
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                File.WriteAllBytes(folder+name,image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=previousTarget; RenderTexture.active=previousActive;
                RenderTexture.ReleaseTemporary(target); Object.DestroyImmediate(image);
            }
        }

        [UnityTest]
        public IEnumerator ResourceSprites_FitAnchorsAndFollowDepletionRecoveryAndReset()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var camera = composition.GetComponent<Stage2SceneBootstrap>().CameraFollow.BoundCamera;
            var economy = composition.Gathering.Economy;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var marker = composition.transform.Find($"{kind} Resource Marker");
                Assert.That(marker, Is.Not.Null);
                var renderers = marker.GetComponentsInChildren<SpriteRenderer>().Where(item => item.enabled).ToArray();
                Assert.That(renderers.Length, Is.EqualTo(1), kind.ToString());
                Assert.That(marker.GetComponentsInChildren<MeshRenderer>(), Is.Empty, "Do not leave the 3D resource under the sprite.");
                var body = renderers[0];
                var anchor = economy.WorldPosition(kind);
                Assert.That((Vector2)marker.position, Is.EqualTo(anchor));
                Assert.That(Quaternion.Angle(body.transform.rotation, camera.transform.rotation), Is.LessThan(.1f));
                var visualRoot = body.transform.parent;
                Assert.That((Vector2)visualRoot.position, Is.EqualTo(anchor));
                Assert.That(visualRoot.position.z, Is.InRange(-.205f, -.075f), "The footing follows existing grass relief.");
                var screenAnchor = (Vector2)camera.WorldToScreenPoint(visualRoot.position);
                var screenFooting = (Vector2)camera.WorldToScreenPoint(body.transform.position);
                var pixelTolerance = camera.pixelHeight / (2f * camera.orthographicSize) * .03f + .1f;
                Assert.That(Vector2.Distance(screenAnchor, screenFooting), Is.LessThan(pixelTolerance), "The painted footing stays registered at the grass surface.");
                Assert.That(body.sortingOrder, Is.EqualTo(-Mathf.RoundToInt(marker.position.y * 100f)));
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(body.sprite.texture)) as TextureImporter;
                Assert.That(importer.alphaIsTransparency, Is.True);
                Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None));
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
                var png = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    png.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(body.sprite.texture)));
                    var pixels = png.GetPixels32();
                    var xs = Enumerable.Range(0, pixels.Length).Where(i => pixels[i].a >= 128).Select(i => i % png.width).ToArray();
                    var ys = Enumerable.Range(0, pixels.Length).Where(i => pixels[i].a >= 128).Select(i => i / png.width).ToArray();
                    Assert.That((xs.Max() - xs.Min() + 1f) / body.sprite.pixelsPerUnit, Is.InRange(1.4f, 1.55f));
                    Assert.That((ys.Max() - ys.Min() + 1f) / body.sprite.pixelsPerUnit,
                        kind == MapResourceKind.Wood ? Is.InRange(1.85f, 2.15f) : Is.InRange(1.1f, 1.35f));
                }
                finally { Object.DestroyImmediate(png); }
                Assert.That(marker.GetComponentsInChildren<Collider>(), Is.Empty);
                Assert.That(renderers[0].sharedMaterials.Length, Is.EqualTo(1), kind.ToString());
                foreach (var material in renderers[0].sharedMaterials)
                {
                    Assert.That(material.shader.isSupported, Is.True, material.name);
                    Assert.That(body.sprite.texture, Is.Not.Null);
                }
                var node = economy.GetNode(kind);
                for (var unit = 0; unit < node.Capacity; unit++)
                    Assert.That(economy.AdvanceGather(kind, anchor, true, node.GatherSeconds, true), Is.True);
                yield return null;
                Assert.That(marker.gameObject.activeSelf, Is.False, kind.ToString());
                economy.Tick(node.RecoverySeconds, true);
                yield return null;
                Assert.That(marker.gameObject.activeSelf, Is.True, kind.ToString());
            }
            composition.RunDriver.ResetRun();
            yield return null;
            foreach (var kind in new[] { MapResourceKind.Wood, MapResourceKind.Stone, MapResourceKind.Metal })
            {
                var marker = composition.transform.Find($"{kind} Resource Marker");
                Assert.That(marker.gameObject.activeSelf, Is.True);
                Assert.That(marker.GetComponentsInChildren<SpriteRenderer>().Count(item => item.enabled), Is.EqualTo(1));
                Assert.That(economy.GetNode(kind).Remaining, Is.EqualTo(economy.GetNode(kind).Capacity));
            }
        }

        [UnityTest]
        public IEnumerator ResourceSprites_RenderInPlayableCameraAtBothZoomLevels()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var camera = bootstrap.CameraFollow.BoundCamera;
            var economy = composition.Gathering.Economy;
            var timing = new System.Text.StringBuilder();
            Directory.CreateDirectory("art/review");
            Directory.CreateDirectory("Artifacts/S3-RESOURCE-SOURCES");
            foreach (var kind in new[] { MapResourceKind.Stone, MapResourceKind.Wood, MapResourceKind.Metal })
            {
                var anchor = economy.WorldPosition(kind);
                bootstrap.PlayerMover.transform.position = new Vector3(anchor.x - 1.1f, anchor.y - .55f, 0);
                bootstrap.CameraFollow.SnapToTarget();
                yield return null;
                var marker = composition.transform.Find($"{kind} Resource Marker");
                var renderer = marker.GetComponentsInChildren<SpriteRenderer>().Single(item => item.enabled);
                Assert.That(renderer, Is.Not.Null);
                var viewport = camera.WorldToViewportPoint(renderer.bounds.center);
                Assert.That(viewport.x, Is.InRange(.2f, .8f), kind.ToString());
                Assert.That(viewport.y, Is.InRange(.2f, .8f), kind.ToString());
                camera.orthographicSize = 5f;
                SaveResourceFrame(camera, $"art/review/resource_{kind.ToString().ToLowerInvariant()}_game_default.png");
                AssertResourceVisible(camera, renderer);
                var with = new double[48];
                var without = new double[48];
                var target = RenderTexture.GetTemporary(1280, 720, 24);
                var previous = camera.targetTexture;
                var active = RenderTexture.active;
                var pixel = new Texture2D(1, 1, TextureFormat.RGB24, false);
                var watch = new System.Diagnostics.Stopwatch();
                try
                {
                    camera.targetTexture = target;
                    RenderTexture.active = target;
                    for (var round = 0; round < 4; round++)
                    for (var slot = 0; slot < 2; slot++)
                    {
                        var visible = (round + slot) % 2 == 0;
                        renderer.enabled = visible;
                        for (var sample = 0; sample < 16; sample++)
                        {
                            watch.Restart();
                            camera.Render();
                            pixel.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
                            pixel.Apply();
                            watch.Stop();
                            if (sample >= 4)
                                (visible ? with : without)[round * 12 + sample - 4] =
                                    watch.Elapsed.TotalMilliseconds;
                        }
                    }
                    System.Array.Sort(with);
                    System.Array.Sort(without);
                    timing.AppendLine($"{kind}: spriteTris={renderer.sprite.triangles.Length / 3} " +
                        $"renderOnMedianMs={with[24]:F3} renderOffMedianMs={without[24]:F3} " +
                        $"renderOnP95Ms={with[45]:F3} renderOffP95Ms={without[45]:F3}");
                }
                finally
                {
                    renderer.enabled = true;
                    camera.targetTexture = previous;
                    RenderTexture.active = active;
                    RenderTexture.ReleaseTemporary(target);
                    Object.DestroyImmediate(pixel);
                }
                camera.orthographicSize = 2.5f;
                SaveResourceFrame(camera, $"art/review/resource_{kind.ToString().ToLowerInvariant()}_game_close.png");
                AssertResourceVisible(camera, renderer);
                camera.orthographicSize = 5f;
            }
            File.WriteAllText("Artifacts/S3-RESOURCE-SOURCES/render-comparison.txt",
                $"Camera.Render plus synchronous 1px readback in playable Stage 3 scene, Editor, 1280x720, four reversed-order rounds, 4 warmups and 12 samples per state per round. GPU={SystemInfo.graphicsDeviceName}. Hardware-specific observation, not standalone FPS.\n{timing}");
        }

        private static void AssertResourceVisible(Camera camera, SpriteRenderer renderer)
        {
            var target = RenderTexture.GetTemporary(320, 320, 24);
            var previous = camera.targetTexture;
            var active = RenderTexture.active;
            var image = new Texture2D(320, 320, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 320, 320), 0, 0); image.Apply();
                var visible = image.GetPixels32();
                renderer.enabled = false;
                camera.Render();
                image.ReadPixels(new Rect(0, 0, 320, 320), 0, 0); image.Apply();
                var ground = image.GetPixels32();
                var changed = 0;
                for (var i = 0; i < ground.Length; i++)
                    if (Mathf.Abs(visible[i].r - ground[i].r) + Mathf.Abs(visible[i].g - ground[i].g) +
                        Mathf.Abs(visible[i].b - ground[i].b) > 40) changed++;
                Assert.That(changed, Is.GreaterThan(500), renderer.sprite.name + " must render above the terrain.");
            }
            finally
            {
                renderer.enabled = true;
                camera.targetTexture = previous;
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(image);
            }
        }

        private static void SaveResourceFrame(Camera camera, string path)
        {
            var target = RenderTexture.GetTemporary(1600, 900, 24);
            var previous = camera.targetTexture;
            var active = RenderTexture.active;
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(image);
            }
        }

        [UnityTest]
        public IEnumerator EntrancePortal_PreservesSpawnPlacementPauseAndReset()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap=Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var composition=Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.ResetRun();
            var portal=bootstrap.EntranceMarker.GetComponentsInChildren<MonoBehaviour>()
                .SingleOrDefault(component=>component.GetType().Name=="Stage3EntrancePortal");
            Assert.That(portal,Is.Not.Null);
            var entrance=bootstrap.MapDefinition.EnemyEntrance;
            var origin=bootstrap.MapDefinition.CellToWorld(entrance);
            Assert.That((Vector2)portal.transform.position,Is.EqualTo(origin));
            var towardGoal=((Vector2)(bootstrap.MapDefinition.Throne-entrance)).normalized;
            Assert.That(Vector2.Dot((Vector2)portal.transform.right,towardGoal),Is.GreaterThan(.7f));
            Assert.That(portal.GetComponentsInChildren<Collider>(),Is.Empty);
            foreach(var renderer in portal.GetComponentsInChildren<MeshRenderer>())
            {
                Assert.That(renderer.bounds.min.x,Is.GreaterThanOrEqualTo(origin.x-1.5f));
                Assert.That(renderer.bounds.max.x,Is.LessThanOrEqualTo(origin.x+1.5f));
                Assert.That(renderer.bounds.min.y,Is.GreaterThanOrEqualTo(origin.y-1.5f));
                Assert.That(renderer.bounds.max.y,Is.LessThanOrEqualTo(origin.y+1.5f));
                foreach(var material in renderer.sharedMaterials)
                    Assert.That(material.shader.isSupported,Is.True,material.name);
            }
            for(var y=-1;y<=1;y++) for(var x=-1;x<=1;x++)
            {
                var cell=entrance+new Vector2Int(x,y);
                Assert.That(bootstrap.MapDefinition.IsBuildable(cell),Is.False);
                Assert.That(bootstrap.MapDefinition.IsWalkable(cell),Is.True);
            }
            Assert.That(bootstrap.MapDefinition.IsBuildable(entrance+Vector2Int.right*2),Is.True);
            Renderer vortex;
            var artwork=portal.GetComponentsInChildren<SpriteRenderer>().SingleOrDefault();
            if(artwork != null)
            {
                Assert.That(artwork.sprite.name,Is.EqualTo("PortalCartoon"));
                Assert.That(artwork.bounds.size.x,Is.EqualTo(3.25f).Within(.03f));
                Assert.That(artwork.sharedMaterial.shader.isSupported,Is.True);
                Assert.That(portal.GetComponentsInChildren<MeshFilter>(),Is.Empty,
                    "The new portal uses sprite artwork rather than retained hidden model parts.");
                AssertBaseVisible(bootstrap,artwork,false,new Vector2(.47f,.13f));
                vortex=artwork;
            }
            else
            {
            var stone=portal.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="PortalStone");
            Assert.That(stone.bounds.size.x,Is.InRange(2.9f,3f));
            Assert.That(stone.bounds.size.y,Is.InRange(2.9f,3f));
            vortex=portal.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="PortalVortex");
            // The opening turns diagonally while the square foundation stays on the grid.
            Assert.That(vortex.bounds.size.x,Is.InRange(1.3f,1.6f));
            Assert.That(vortex.bounds.size.y,Is.InRange(1.3f,1.6f));
            Assert.That(vortex.bounds.size.x,Is.EqualTo(vortex.bounds.size.y).Within(.02f));
            }
            var block=new MaterialPropertyBlock();
            float FlowTime() { vortex.GetPropertyBlock(block); return block.GetFloat("_FlowTime"); }
            var initialTime=FlowTime();
            var initialFrame=CapturePortal(bootstrap);
            yield return new WaitForSeconds(.4f);
            Assert.That(FlowTime(),Is.GreaterThan(initialTime));
            var movingPixels=ChangedRedPixels(initialFrame,CapturePortal(bootstrap));
            Assert.That(movingPixels,Is.GreaterThan(100),"The rendered red aperture must visibly circulate.");
            Debug.Log($"S3_PORTAL_RENDER_MOTION changedRedPixels={movingPixels}");
            composition.RunDriver.SetPaused(true);
            var pausedTime=FlowTime();
            var pausedFrame=CapturePortal(bootstrap);
            yield return new WaitForSeconds(.2f);
            Assert.That(FlowTime(),Is.EqualTo(pausedTime));
            Assert.That(ChangedRedPixels(pausedFrame,CapturePortal(bootstrap)),Is.Zero,
                "Pausing the run must freeze the visible vortex.");
            composition.RunDriver.SetPaused(false);
            Assert.That(composition.RunDriver.Run.StartWave(),Is.True);
            Assert.That(composition.Enemies.ActiveCount,Is.EqualTo(1));
            Assert.That((Vector2)composition.Enemies.GetEnemy(0).transform.position,Is.EqualTo(origin));
            yield return new WaitForSeconds(.3f);
            Assert.That((Vector2)composition.Enemies.GetEnemy(0).transform.position,Is.Not.EqualTo(origin));
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(composition.Enemies.ActiveCount,Is.Zero);
            Assert.That(bootstrap.EntranceMarker.GetComponentsInChildren<MonoBehaviour>()
                .Count(component=>component.GetType().Name=="Stage3EntrancePortal"),Is.EqualTo(1));
            Assert.That(FlowTime(),Is.GreaterThan(pausedTime));
        }

        [UnityTest]
        public IEnumerator SoulObjective_ReservesNineCellsAndWaveStaysHorizontal()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap=Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var map=bootstrap.MapDefinition;
            for(var y=-1;y<=1;y++) for(var x=-1;x<=1;x++)
                Assert.That(map.IsBuildable(map.Throne+new Vector2Int(x,y)),Is.False);
            Assert.That(map.IsBuildable(map.Throne+new Vector2Int(-2,0)),Is.True);
            var artwork=bootstrap.ThroneMarker.GetComponentsInChildren<SpriteRenderer>()
                .SingleOrDefault(renderer=>renderer.enabled);
            if(artwork != null)
            {
                var composition=bootstrap.GetComponent<Stage2GameComposition>();
                Assert.That(artwork.sprite.name,Is.EqualTo("SoulObjectiveCartoon"));
                Assert.That(artwork.bounds.size.x,Is.EqualTo(3.8f).Within(.03f));
                Assert.That(artwork.sharedMaterial.shader.isSupported,Is.True);
                Assert.That(bootstrap.ThroneMarker.GetComponentsInChildren<MeshFilter>(),Is.Empty);
                Assert.That(bootstrap.ThroneMarker.GetComponentsInChildren<Collider>(),Is.Empty);
                AssertBaseVisible(bootstrap,artwork,true,new Vector2(.30f,.26f),
                    new Vector2(.71f,.24f),new Vector2(.50f,.16f));
                var first=CapturePortal(bootstrap,true);
                yield return new WaitForSeconds(.6f);
                var moving=CapturePortal(bootstrap,true);
                var changed=0;
                for(var pixel=0;pixel<first.Length;pixel++)
                    if(first[pixel].b>first[pixel].r*1.3f &&
                        Mathf.Abs(first[pixel].b-moving[pixel].b)>6)changed++;
                Assert.That(changed,Is.GreaterThan(100),"Actual blue energy pixels must visibly flow.");
                composition.RunDriver.SetPaused(true);
                var paused=CapturePortal(bootstrap,true);
                yield return new WaitForSeconds(.2f);
                Assert.That(CapturePortal(bootstrap,true),Is.EqualTo(paused));
                composition.RunDriver.SetPaused(false);
                Debug.Log($"S3_SOUL_SPRITE_OK footprint=3x3 flatArtwork=true changedBluePixels={changed}");
                yield break;
            }
            var renderers=bootstrap.ThroneMarker.GetComponentsInChildren<MeshRenderer>();
            var stone=renderers.Single(r=>r.name=="StoneBase");
            var dome=renderers.Single(r=>r.name=="Dome");
            var sphere=renderers.Single(r=>r.name=="Sphere");
            var wave=renderers.Single(r=>r.name=="OrbitingWave");
            Assert.That(stone.bounds.size.x,Is.InRange(2.90f,3.05f));
            Assert.That(stone.bounds.size.y,Is.InRange(2.90f,3.05f));
            Assert.That(sphere.bounds.size.x,Is.EqualTo(.98f).Within(.02f));
            Assert.That(dome.sharedMaterial.shader.name,Is.EqualTo("Not3A/Smooth Soul Ice"));
            Assert.That(dome.sharedMaterial.GetFloat("_CenterOpacity"),Is.LessThan(.04f));
            foreach(var renderer in renderers)
                foreach(var material in renderer.sharedMaterials)
                    Assert.That(material.shader.isSupported,Is.True,material.name);
            var stoneRotation=stone.transform.rotation;
            var waveRotation=wave.transform.rotation;
            var plane=wave.bounds.center.z;
            // Sample more than a complete revolution, including all former tipping angles.
            for(var i=0;i<64;i++)
            {
                yield return new WaitForSeconds(.25f);
                Assert.That(wave.bounds.size.z,Is.LessThan(.025f),"FBX axes must not tip the orbit into a vertical loop.");
                Assert.That(wave.bounds.center.z,Is.EqualTo(plane).Within(.002f));
                Assert.That(Quaternion.Angle(stoneRotation,stone.transform.rotation),Is.LessThan(.01f));
            }
            Assert.That(Quaternion.Angle(waveRotation,wave.transform.rotation),Is.GreaterThan(15f));
            Assert.That(bootstrap.ThroneMarker.GetComponentsInChildren<Collider>(),Is.Empty);
            Debug.Log("S3_SOUL_OBJECTIVE_OK footprint=3x3 horizontalWave=true separateTransparentLayers=true");
        }

        [UnityTest]
        public IEnumerator LandscapeVerge_DecorationIsBoundedAndHasNoRuleColliders()
        {
            const float ExpectedVisualLift = .08f;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var props = GameObject.Find("Real landscape grass and flowers");
            Assert.That(props, Is.Not.Null);
            var roadside=GameObject.Find("Cartoon roadside ruins and shrubs");
            Assert.That(roadside,Is.Not.Null);
            Assert.That(roadside.GetComponentsInChildren<Collider>(),Is.Empty);
            Assert.That(roadside.GetComponentsInChildren<Collider2D>(),Is.Empty);
            Assert.That(roadside.GetComponentsInChildren<SpriteRenderer>().Length,Is.InRange(8,160));
            Assert.That(props.GetComponentsInChildren<Collider>(), Is.Empty,
                "Decorative foliage must never affect movement or placement.");
            var meshes = props.GetComponentsInChildren<MeshFilter>();
            Assert.That(meshes.Length, Is.EqualTo(3));
            foreach (var filter in meshes)
                Assert.That(filter.sharedMesh.vertexCount, Is.GreaterThan(0), filter.name);
            var triangles = meshes.Sum(filter => filter.sharedMesh.triangles.Length / 3);
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var map = composition.GetComponent<Stage2SceneBootstrap>().MapDefinition;
            // Keep the former 100x100 trial's density ceiling, including its
            // eight-cell decorative border, as the authored world grows.
            var decoratedCells = (map.Size.x + 16) * (map.Size.y + 16);
            Assert.That(triangles / (float)decoratedCells, Is.LessThan(6f),
                "Keep foliage density bounded across the expanded green world.");
            Assert.That(props.transform.position.z,
                Is.EqualTo(-ExpectedVisualLift).Within(.001f));
            var terrain = GameObject.Find("Tactical grass and paved build cells")
                .GetComponent<MeshFilter>().sharedMesh;
            var grassMaterial = GameObject.Find("Tactical grass and paved build cells")
                .GetComponent<MeshRenderer>().sharedMaterials[0];
            Assert.That(grassMaterial.GetTextureScale("_BaseMap"),
                Is.EqualTo(new Vector2(.5f, .5f)), "Grass brushwork should match the concept's larger scale.");
            var terrainVertices = terrain.vertices;
              Assert.That(terrain.GetTriangles(0).All(index =>
                  terrainVertices[index].z <= -ExpectedVisualLift), Is.True);
              Assert.That(terrain.GetTriangles(0).Any(index =>
                  terrainVertices[index].z < -ExpectedVisualLift - .04f), Is.True,
                  "Visible grass should have real low rolling relief.");
            Assert.That(terrain.GetTriangles(1).All(index =>
                Mathf.Abs(terrainVertices[index].z) < .001f), Is.True);
            var verge = GameObject.Find("Visual grass verge");
            Assert.That(verge, Is.Not.Null);
            Assert.That(verge.GetComponentsInChildren<Collider>(), Is.Empty);
            var vergeMesh = verge.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(vergeMesh.triangles.Length, Is.GreaterThan(0));
              Assert.That(vergeMesh.bounds.min.z, Is.LessThan(-ExpectedVisualLift - .04f));
            Assert.That(vergeMesh.bounds.max.z, Is.InRange(-.007f, -.005f));
            var vergeMaterial = verge.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.That(vergeMaterial.GetTexture("_BaseMap"),Is.SameAs(grassMaterial.GetTexture("_BaseMap")));
            if (vergeMaterial != grassMaterial)
            {
                Assert.That(vergeMaterial.shader.name,Is.EqualTo("Not3A/Cartoon Grass Verge"));
                Assert.That(vergeMesh.colors.Any(color=>color.a==0f),Is.True);
                Assert.That(vergeMesh.colors.Any(color=>color.a==1f),Is.True);
            }
            Assert.That(composition.Building.Defense.TowerCount, Is.EqualTo(2));
            Assert.That(map.Size,Is.EqualTo(new Vector2Int(250,250)));
            foreach(var decoration in roadside.GetComponentsInChildren<SpriteRenderer>())
            {
                var cell=map.WorldToCell(decoration.transform.position);
                Assert.That(!map.Contains(cell) || map.GetSurface(cell)==MapSurfaceKind.Forest,Is.True,
                    "Decorative ruins/shrubs must stay off all buildable paving.");
                Assert.That(Vector2.Distance(decoration.transform.position,map.CellToWorld(map.Throne)),Is.GreaterThan(2.8f));
                Assert.That(Vector2.Distance(decoration.transform.position,map.CellToWorld(map.EnemyEntrance)),Is.GreaterThan(2.8f));
            }
            Assert.That(map.GetSurface(map.Throne+new Vector2Int(3,-1)), Is.Not.EqualTo(MapSurfaceKind.Forest));
            Assert.That(map.GetSurface(map.GetResourceRegion(0).Anchor), Is.EqualTo(MapSurfaceKind.Forest));
            Debug.Log($"S3_LANDSCAPE_VERGE_OK meshes={meshes.Length} triangles={triangles} " +
                $"lift={ExpectedVisualLift} colliders=0 bushes=0");
        }

        [UnityTest]
        public IEnumerator RuntimeMeshPilot_KeepsAtlasAndPaidTowerAcrossRestart()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            Assert.That(composition.IsInitialized, Is.True);
            var defense = composition.Building.Defense;
            Assert.That(defense.TowerCount, Is.EqualTo(2), "Showcase uses normal paid construction for both models.");
            Stage2Tower tower = null;
            foreach (var candidate in Object.FindObjectsByType<Stage2Tower>())
                if (candidate.name.Contains("Rapid")) tower = candidate;
            Assert.That(tower, Is.Not.Null);
            if (TowerSprite(tower) != null) AssertTowerArtwork(tower);
            else
            {
            var mesh = tower.GetComponentsInChildren<MeshRenderer>();
            Assert.That(mesh.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(mesh[0].sharedMaterials.Length, Is.EqualTo(1));
            Assert.That(mesh[0].sharedMaterial.GetTexture("_BaseMap"), Is.Not.Null);
            Assert.That(mesh[0].bounds.size.z, Is.InRange(1.8f, 2.5f), "Height stands perpendicular to the shared ground.");
            var heightAxis = mesh[0].transform.TransformDirection(Vector3.forward).normalized;
            Assert.That(Vector3.Dot(heightAxis, Vector3.back), Is.GreaterThan(.999f), "FBX vertical must not lean along the ground.");
            Assert.That(tower.GetComponent<SpriteRenderer>().enabled, Is.False);
            }
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(defense.TowerCount, Is.Zero);
            Assert.That(Object.FindAnyObjectByType<Stage2Tower>(), Is.Null);
            var catalog = defense.SelectedCatalog;
            var inventory = composition.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood, catalog.WoodCost);
            inventory.TryAdd(MapResourceKind.Stone, catalog.StoneCost);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(composition.GetComponent<Stage2SceneBootstrap>()
                .MapDefinition.Throne+new Vector2Int(3,-1));
            Assert.That(defense.TryConfirm(out var rebuilt), Is.EqualTo(PlacementResult.Ready));
            if (TowerSprite(rebuilt) != null) AssertTowerArtwork(rebuilt);
            else Assert.That(rebuilt.GetComponentInChildren<MeshRenderer>().sharedMaterial.GetTexture("_BaseMap"), Is.Not.Null);
            Assert.That(inventory.Gold, Is.Zero);
            Assert.That(composition.RunDriver.Run.StartWave(), Is.True);
            yield return null;
            Assert.That(composition.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
        }

        [UnityTest]
        public IEnumerator TacticalView_MovementAndViewportStayOnMap()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var mover = bootstrap.PlayerMover;
            var start = mover.transform.position;
            var camera = bootstrap.CameraFollow.BoundCamera;
            var startingZoom = camera.orthographicSize;
            input.Press(Keyboard.current.wKey);
            for (var frame=0; frame<5; frame++) yield return null;
            input.Release(Keyboard.current.wKey);
            Assert.That(mover.transform.position.y, Is.GreaterThan(start.y));
            Assert.That(mover.transform.position.z, Is.EqualTo(0f));
            Assert.That(camera.orthographicSize, Is.EqualTo(startingZoom).Within(.001f),
                "WASD must not change camera zoom.");
            var map = bootstrap.MapDefinition;
            var startingAspect = camera.aspect;
            foreach (var aspect in new[]{startingAspect,2.4f})
            foreach (var zoom in new[]{2.5f,5f,10f})
            foreach (var cell in new[]{map.MinimumCell,map.MaximumCell})
            {
                camera.aspect = aspect;
                camera.orthographicSize = zoom;
                mover.transform.position = map.CellToWorld(cell);
                bootstrap.CameraFollow.SnapToTarget();
                foreach (var corner in new[]{Vector2.zero,Vector2.one,new Vector2(0,1),new Vector2(1,0)})
                {
                    var ray = camera.ViewportPointToRay(corner);
                    Assert.That(new Plane(Vector3.forward,Vector3.zero).Raycast(ray,out var distance), Is.True);
                    var ground = ray.GetPoint(distance);
                    Assert.That(ground.x, Is.InRange(map.MinimumCell.x-.5f,map.MaximumCell.x+.5f));
                    Assert.That(ground.y, Is.InRange(map.MinimumCell.y-.5f,map.MaximumCell.y+.5f));
                }
            }
            camera.aspect = 16f/9f;
            camera.orthographicSize = 10f;
            mover.transform.position = map.CellToWorld(map.MinimumCell);
            mover.ClampToAuthoredBounds();
            bootstrap.CameraFollow.SnapToTarget();
            SaveResourceFrame(camera,"art/review/camera_clamp_southwest.png");
            mover.transform.position = map.CellToWorld(map.MaximumCell);
            mover.ClampToAuthoredBounds();
            bootstrap.CameraFollow.SnapToTarget();
            SaveResourceFrame(camera,"art/review/camera_clamp_northeast.png");
            camera.aspect = startingAspect;
            camera.orthographicSize = startingZoom;
        }

        [UnityTest]
        public IEnumerator Catapult_MeshPreviewFootprintAndRepeatedPlacementStayIndependentOfRules()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.ResetRun();
            yield return null;
            var defense = composition.Building.Defense;
            var inventory = composition.Gathering.Economy.Inventory;
            defense.SelectRole(1);
            var catalog = defense.SelectedCatalog;
            Assert.That(catalog.TowerPrefab.name, Does.Contain("Longshot"));
            if (TowerSprite(catalog.TowerPrefab) != null)
            {
                yield return SpritePlacement(composition);
                yield break;
            }
            var authoredMeshes = catalog.TowerPrefab.GetComponentsInChildren<MeshFilter>(true).Select(part=>part.sharedMesh).ToArray();
            var authoredMesh = authoredMeshes[0];
            var authoredMaterial = catalog.TowerPrefab.GetComponentInChildren<MeshRenderer>(true).sharedMaterial;
            Assert.That(authoredMaterial.GetTexture("_BaseMap"), Is.Not.Null);
            inventory.TryAdd(MapResourceKind.Wood, catalog.WoodCost * 24);
            inventory.TryAdd(MapResourceKind.Stone, catalog.StoneCost * 24);
            defense.BeginPlacement();
            var targetCell=composition.GetComponent<Stage2SceneBootstrap>()
                .MapDefinition.Throne+new Vector2Int(-2,2);
            defense.SetPreviewCell(targetCell);
            yield return null;
            var preview = GameObject.Find("Tactical mesh placement preview");
            Assert.That(preview, Is.Not.Null);
            Assert.That(preview.GetComponentInChildren<MeshFilter>().sharedMesh, Is.SameAs(authoredMesh));
            var previewBounds = ModelBounds(preview.transform);
            Assert.That(defense.TryConfirm(out var first), Is.EqualTo(PlacementResult.Ready));
            yield return null;
            var renderer = first.GetComponentInChildren<MeshRenderer>();
            var bounds = ModelBounds(first.transform);
            Assert.That(bounds.size.x, Is.LessThan(1f));
            Assert.That(bounds.size.y, Is.LessThan(1f));
            Assert.That(bounds.size.z, Is.InRange(1.4f, 1.8f));
            Assert.That(Vector3.Distance(bounds.size,previewBounds.size), Is.LessThan(.0001f),
                "World-preserving yaw reparenting may round matrix floats, but must not change model size.");
            Assert.That(bounds.max.z, Is.LessThanOrEqualTo(.001f));
            Assert.That(first.GetComponentsInChildren<Collider>().Length, Is.Zero);
            Assert.That(first.GetComponentsInChildren<Collider2D>().Length, Is.Zero);
            // Profile the same real scene, first with one catapult, then with 24 paid copies.
            for (var frame = 0; frame < 15; frame++) yield return null;
            var baseline = Time.realtimeSinceStartupAsDouble;
            for (var frame = 0; frame < 60; frame++) yield return null;
            var baselineMs = (Time.realtimeSinceStartupAsDouble - baseline) * 1000 / 60;
            var objective=composition.GetComponent<Stage2SceneBootstrap>().MapDefinition.Throne;
            for (var y = objective.y-9; y <= objective.y+9 && defense.TowerCount < 24; y++)
            for (var x = objective.x-9; x <= objective.x+9 && defense.TowerCount < 24; x++)
            {
                defense.BeginPlacement();
                defense.SetPreviewCell(new Vector2Int(x, y));
                if (defense.EvaluatePreview() != PlacementResult.Ready) continue;
                Assert.That(defense.TryConfirm(out var copy), Is.EqualTo(PlacementResult.Ready));
                CollectionAssert.AreEquivalent(authoredMeshes,copy.transform.Find("Pilot ground-aligned mesh")
                    .GetComponentsInChildren<MeshFilter>(true).Select(part=>part.sharedMesh).ToArray());
                Assert.That(copy.GetComponentInChildren<MeshRenderer>().sharedMaterial, Is.SameAs(authoredMaterial));
            }
            defense.CancelPlacement();
            Assert.That(defense.TowerCount, Is.EqualTo(24));
            for (var frame = 0; frame < 15; frame++) yield return null;
            var loaded = Time.realtimeSinceStartupAsDouble;
            for (var frame = 0; frame < 60; frame++) yield return null;
            var loadedMs = (Time.realtimeSinceStartupAsDouble - loaded) * 1000 / 60;
            Debug.Log($"S3_LONGSHOT_PROFILE towers=24 meshVertices={authoredMesh.vertexCount} " +
                $"meshBytes={UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(authoredMesh)} " +
                $"baselineFrameMs={baselineMs:F3} loadedFrameMs={loadedMs:F3} " +
                $"GPU={SystemInfo.graphicsDeviceName} (Editor diagnostic, not a release performance budget)");
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(defense.TowerCount, Is.Zero);
        }

        private static Component TowerSprite(Stage2Tower tower) => tower.GetComponentsInChildren<MonoBehaviour>(true)
            .SingleOrDefault(component=>component.GetType().Name=="Stage3TowerSprite");

        private static SpriteRenderer TowerArtwork(Stage2Tower tower) =>
            (SpriteRenderer)TowerSprite(tower).GetType().GetProperty("Body").GetValue(TowerSprite(tower));

        private static void AssertTowerArtwork(Stage2Tower tower)
        {
            var body=TowerArtwork(tower);
            Assert.That(body.sprite,Is.Not.Null);
            Assert.That(body.enabled,Is.True);
            Assert.That(body.sprite.texture.width,Is.GreaterThanOrEqualTo(1024));
            Assert.That(body.sharedMaterial.shader.name,Is.EqualTo("Not3A/Skeleton Sprite Unlit"));
            Assert.That(tower.GetComponentsInChildren<MeshRenderer>().All(renderer=>renderer.name=="Tower contact shadow"),Is.True);
            Assert.That(tower.GetComponent<SpriteRenderer>().enabled,Is.False);
            Assert.That(tower.GetComponentsInChildren<Collider>(),Is.Empty);
            Assert.That(tower.GetComponentsInChildren<Collider2D>(),Is.Empty);
        }

        private static IEnumerator SpritePlacement(Stage2GameComposition composition)
        {
            var defense=composition.Building.Defense;
            var inventory=composition.Gathering.Economy.Inventory;
            var catalog=defense.SelectedCatalog;
            inventory.TryAdd(MapResourceKind.Wood,catalog.WoodCost*24);
            inventory.TryAdd(MapResourceKind.Stone,catalog.StoneCost*24);
            var beforeWood=inventory.Get(MapResourceKind.Wood);
            defense.BeginPlacement();
            defense.SetPreviewCell(composition.GetComponent<Stage2SceneBootstrap>().MapDefinition.Throne+new Vector2Int(-2,2));
            yield return null;
            var preview=GameObject.Find("Tactical mesh placement preview");
            Assert.That(preview,Is.Not.Null);
            var previewBody=preview.GetComponentInChildren<SpriteRenderer>();
            var source=TowerArtwork(catalog.TowerPrefab).sprite;
            Assert.That(previewBody.sprite,Is.SameAs(source));
            Assert.That(defense.TryConfirm(out var first),Is.EqualTo(PlacementResult.Ready));
            yield return null;
            AssertTowerArtwork(first);
            Assert.That(TowerArtwork(first).sprite,Is.SameAs(source));
            Assert.That(TowerArtwork(first).bounds.size,Is.EqualTo(previewBody.bounds.size));
            Assert.That(inventory.Get(MapResourceKind.Wood),Is.EqualTo(beforeWood-catalog.WoodCost));
            var map=composition.GetComponent<Stage2SceneBootstrap>().MapDefinition;
            for(var y=map.Throne.y-9;y<=map.Throne.y+9 && defense.TowerCount<24;y++)
            for(var x=map.Throne.x-9;x<=map.Throne.x+9 && defense.TowerCount<24;x++)
            {
                defense.BeginPlacement(); defense.SetPreviewCell(new Vector2Int(x,y));
                if(defense.EvaluatePreview()!=PlacementResult.Ready) continue;
                Assert.That(defense.TryConfirm(out var copy),Is.EqualTo(PlacementResult.Ready));
                Assert.That(TowerArtwork(copy).sprite.texture,Is.SameAs(source.texture));
                Assert.That(TowerArtwork(copy).sharedMaterial,Is.SameAs(TowerArtwork(first).sharedMaterial));
            }
            defense.CancelPlacement();
            Assert.That(defense.TowerCount,Is.EqualTo(24));
            composition.RunDriver.ResetRun(); yield return null;
            Assert.That(Object.FindAnyObjectByType<Stage2Tower>(),Is.Null);
        }

        private static IEnumerator SpriteTracking(Stage2GameComposition composition,Stage2Tower catapult)
        {
            var body=TowerArtwork(catapult);
            var rootPosition=catapult.transform.position;
            var rootRotation=catapult.transform.rotation;
            var enemy=composition.Enemies.SpawnAt(catapult.Cell+new Vector2Int(2,2));
            enemy.SetAutomaticTick(false);
            var bearings=new[]{new Vector2(1,1),new Vector2(-1,1),new Vector2(-1,-1),new Vector2(1,-1)};
            var names=new System.Collections.Generic.HashSet<int>();
            var view=TowerSprite(catapult);
            var foundation=body.sprite;
            foreach(var bearing in bearings)
            {
                enemy.transform.position=rootPosition+(Vector3)bearing*2f;
                yield return null; yield return null;
                names.Add((int)view.GetType().GetProperty("Direction").GetValue(view));
                Assert.That(body.sprite,Is.SameAs(foundation),"Target tracking must never replace the planted masonry.");
                Assert.That(catapult.transform.position,Is.EqualTo(rootPosition));
                Assert.That(catapult.transform.rotation,Is.EqualTo(rootRotation));
                Assert.That(body.transform.rotation,Is.EqualTo(Camera.main.transform.rotation));
            }
            Assert.That(names.Count,Is.EqualTo(4));
            composition.RunDriver.SetPaused(true);
            var frozen=body.sprite;
            enemy.transform.position=rootPosition+Vector3.up*2f;
            yield return null; yield return null;
            Assert.That(body.sprite,Is.SameAs(frozen));
            Assert.That(composition.Enemies.PendingShotCount,Is.Zero);
            composition.RunDriver.ResetRun(); yield return null;
            Assert.That(Object.FindAnyObjectByType<Stage2Tower>(),Is.Null);
        }

        private static IEnumerator SpriteCombat(Stage2GameComposition composition,Stage2Tower catapult,Stage2Tower rapid)
        {
            var body=TowerArtwork(catapult);
            var idle=body.sprite;
            var rapidBody=TowerArtwork(rapid);
            var rapidSprite=rapidBody.sprite;
            var rapidPosition=rapidBody.transform.position;
            var enemy=composition.Enemies.SpawnAt(catapult.Cell+new Vector2Int(2,-2));
            enemy.SetAutomaticTick(false);
            var hp=enemy.Health;
            catapult.Tick(.01f);
            Assert.That(enemy.Health,Is.EqualTo(hp));
            Assert.That(GameObject.Find("Stone in flight").GetComponent<MeshRenderer>().enabled,Is.False);
            composition.RunDriver.Run.Tick(.09f); yield return null; yield return null;
            var spoon=catapult.transform.Find("Tower artwork/Articulated spoon");
            var stone=catapult.transform.Find("Tower artwork/Loaded stone").GetComponent<SpriteRenderer>();
            Assert.That(spoon,Is.Not.Null); Assert.That(stone.enabled,Is.True);
            Assert.That(body.sprite,Is.SameAs(idle),"Foundation stays identical during windup.");
            composition.RunDriver.SetPaused(true);
            var pausedSprite=body.sprite;
            var pausedPosition=body.transform.position;
            var pausedArm=spoon.rotation;
            composition.RunDriver.Run.Tick(5f); yield return null; yield return null;
            Assert.That(body.sprite,Is.SameAs(pausedSprite));
            Assert.That(body.transform.position,Is.EqualTo(pausedPosition));
            Assert.That(spoon.rotation,Is.EqualTo(pausedArm));
            Assert.That(enemy.Health,Is.EqualTo(hp));
            composition.RunDriver.SetPaused(false);
            composition.RunDriver.Run.Tick(.13f); yield return null; yield return null;
            Assert.That(body.sprite,Is.SameAs(idle),"Foundation stays identical at release.");
            Assert.That(stone.enabled,Is.False,"The cup is empty at actual release.");
            Assert.That(Quaternion.Angle(spoon.rotation,pausedArm),Is.GreaterThan(5f));
            var projectile=GameObject.Find("Stone in flight");
            Assert.That(projectile.GetComponent<MeshRenderer>().enabled,Is.True);
            var painted=projectile.GetComponentInChildren<SpriteRenderer>();
            Assert.That(painted,Is.Not.Null);
            Assert.That(painted.enabled,Is.True);
            Assert.That(painted.sprite,Is.SameAs(stone.sprite),"Release must retain the painted loaded stone identity.");
            Assert.That(Vector3.Distance(painted.transform.lossyScale,stone.transform.lossyScale),Is.LessThan(.001f),
                "The fitted tower's stone must retain its size when it leaves the cup.");
            var motion=catapult.GetComponent("Stage3TowerShotMotion");
            var release=(Vector3)motion.GetType().GetProperty("ReleasePosition").GetValue(motion);
            Assert.That(Vector3.Distance(projectile.transform.position,release),Is.LessThan(.002f));
            Assert.That(Vector3.Distance(projectile.transform.position,stone.transform.position),Is.LessThan(.015f),"Projectile starts in the actual animated cup.");
            composition.RunDriver.Run.Tick(.36f);
            Assert.That(enemy.Health,Is.EqualTo(hp-2));
            Assert.That(GameObject.Find("Stone area impact"),Is.Not.Null);
            rapid.Tick(.01f); yield return null; yield return null;
            Assert.That(GameObject.Find("Arrow in flight"),Is.Not.Null);
            Assert.That(rapidBody.sprite,Is.SameAs(rapidSprite));
            Assert.That(rapidBody.transform.position,Is.EqualTo(rapidPosition));
            Assert.That(rapidBody.color,Is.EqualTo(Color.white));
            composition.RunDriver.Run.Tick(.2f);
            Assert.That(GameObject.Find("Arrow impact"),Is.Not.Null);
            composition.RunDriver.ResetRun(); yield return null;
            foreach(var name in new[]{"Stone in flight","Arrow in flight","Stone area impact","Arrow impact"})
                Assert.That(GameObject.Find(name),Is.Null);
        }

        private static Bounds ModelBounds(Transform root)
        {
            Transform model = null;
            foreach (Transform child in root)
                if (child.name.StartsWith("Pilot ground-aligned mesh")) model = child;
            Assert.That(model, Is.Not.Null);
            var renderers = model.GetComponentsInChildren<MeshRenderer>();
            var bounds = renderers[0].bounds;
            foreach (var item in renderers) bounds.Encapsulate(item.bounds);
            return bounds;
        }

        [UnityTest]
        public IEnumerator Catapult_FrameRotatesAboutItsCenterAndTracksEnemiesBetweenShots()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition=Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.enabled=false;
            composition.RunDriver.Run.SetPaused(false);
            var towers=Object.FindObjectsByType<Stage2Tower>(FindObjectsSortMode.None);
            foreach (var tower in towers) tower.enabled=false;
            var catapult=towers.Single(tower=>tower.name.Contains("Longshot"));
            if (TowerSprite(catapult) != null)
            {
                yield return SpriteTracking(composition,catapult);
                yield break;
            }
            var nodes=catapult.GetComponentsInChildren<Transform>(true);
            var pivot=nodes.Single(node=>node.name == "Catapult centered yaw");
            var frame=nodes.Single(node=>node.name == "CatapultFrame");
            var arm=nodes.Single(node=>node.name == "CatapultArm");
            var stone=nodes.Single(node=>node.name == "LoadedStone");
            var foundation=nodes.Single(node=>node.name == "CatapultBase");
            var foundationMatrix=foundation.localToWorldMatrix;
            var originalPivot=pivot.position;
            var sampleLocal=frame.GetComponent<MeshFilter>().sharedMesh.bounds.center+Vector3.right*.2f;
            var sample=frame.TransformPoint(sampleLocal);
            var sampleHeight=sample.z;
            var radius=Vector2.Distance(sample,pivot.position);
            var target=composition.Enemies.SpawnAt(catapult.Cell+new Vector2Int(2,0));
            target.SetAutomaticTick(false);
            int health=target.Health;
            Assert.That(frame.IsChildOf(pivot), Is.True);
            Assert.That(arm.IsChildOf(pivot), Is.True);
            Assert.That(foundation.IsChildOf(pivot), Is.False, "Foundation and parapet must never rotate.");
            Assert.That(Vector2.Distance(pivot.position,catapult.transform.position), Is.LessThan(.0001f));
            foreach (var direction in new[]{Vector2.right,Vector2.up,Vector2.left,Vector2.down})
            {
                target.transform.position=catapult.transform.position+(Vector3)direction*2f;
                yield return new WaitForSeconds(.6f);
                var cup=stone.TransformPoint(stone.GetComponent<MeshFilter>().sharedMesh.bounds.center);
                var forward=((Vector2)(arm.position-cup)).normalized;
                Assert.That(Vector2.Dot(forward,direction), Is.GreaterThan(.998f), "Aim must follow the live enemy before firing.");
                Assert.That(pivot.position, Is.EqualTo(originalPivot));
                Assert.That(foundation.localToWorldMatrix, Is.EqualTo(foundationMatrix));
                sample=frame.TransformPoint(sampleLocal);
                Assert.That(sample.z, Is.EqualTo(sampleHeight).Within(.0001f), "Yaw cannot tilt the frame.");
                Assert.That(Vector2.Distance(sample,pivot.position), Is.EqualTo(radius).Within(.0001f));
            }
            composition.RunDriver.SetPaused(true);
            var pausedRotation=pivot.rotation;
            target.transform.position=catapult.transform.position+Vector3.up*2f;
            yield return new WaitForSeconds(.12f);
            Assert.That(pivot.rotation, Is.EqualTo(pausedRotation));
            Assert.That(target.Health, Is.EqualTo(health));
            Assert.That(composition.Enemies.PendingShotCount, Is.Zero, "Aiming alone cannot fire or damage.");
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(Object.FindAnyObjectByType<Stage2Tower>(), Is.Null);
            Debug.Log("S3_CATAPULT_YAW_OK four bearings; centered vertical axis; fixed base/parapet; read-only tracking; pause/reset.");
        }

        [UnityTest]
        public IEnumerator CombatFlight_PoseImpactPauseAndRestartAreConnectedToRules()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.enabled=false;
            composition.RunDriver.Run.SetPaused(false); // Deterministic flight samples below.
            var towers = Object.FindObjectsByType<Stage2Tower>(FindObjectsSortMode.None);
            foreach (var tower in towers) tower.enabled = false;
            Stage2Tower catapult = null, rapid = null;
            foreach (var tower in towers)
                if (tower.name.Contains("Longshot")) catapult = tower; else rapid = tower;
            if (TowerSprite(catapult) != null)
            {
                yield return SpriteCombat(composition,catapult,rapid);
                yield break;
            }
            Transform arm = null, stone = null;
            foreach (var child in catapult.GetComponentsInChildren<Transform>())
                if (child.name == "CatapultArm") arm = child; else if (child.name == "LoadedStone") stone = child;
            Assert.That(arm, Is.Not.Null); Assert.That(stone, Is.Not.Null);
            var idle = arm.localRotation;
            var rapidModel=rapid.transform.Find("Pilot ground-aligned mesh");
            var rapidPosition=rapidModel.localPosition;
            var rapidRotation=rapidModel.localRotation;
            var rapidColor=rapid.GetComponent<SpriteRenderer>().color;
            var catapultColor=catapult.GetComponent<SpriteRenderer>().color;
            var target = composition.Enemies.SpawnAt(catapult.Cell+new Vector2Int(2,-2));
            target.SetAutomaticTick(false);
            var hp = target.Health;
            catapult.Tick(.01f);
            Assert.That(composition.Enemies.PendingShotCount, Is.EqualTo(1));
            Assert.That(target.Health, Is.EqualTo(hp));
            Assert.That(GameObject.Find("Stone in flight"), Is.Not.Null);
            Assert.That(stone.gameObject.activeSelf, Is.True, "Keep the stone in the cup during windup.");
            Assert.That(GameObject.Find("Stone in flight").GetComponent<MeshRenderer>().enabled, Is.False);
            composition.RunDriver.Run.Tick(.06f);
            yield return null; yield return null;
            Assert.That(Quaternion.Angle(idle, arm.localRotation), Is.GreaterThan(0f));
            composition.RunDriver.SetPaused(true);
            var frozen = GameObject.Find("Stone in flight").transform.position;
            var frozenArm = arm.localRotation;
            composition.RunDriver.Run.Tick(5f);
            yield return null; yield return null;
            Assert.That(GameObject.Find("Stone in flight").transform.position, Is.EqualTo(frozen));
            Assert.That(arm.localRotation, Is.EqualTo(frozenArm));
            Assert.That(target.Health, Is.EqualTo(hp));
            composition.RunDriver.SetPaused(false);
            composition.RunDriver.Run.Tick(.16f); // Exact release at .22 seconds.
            yield return null; yield return null;
            Assert.That(stone.gameObject.activeSelf, Is.False);
            var released=GameObject.Find("Stone in flight");
            Assert.That(released.GetComponent<MeshRenderer>().enabled, Is.True);
            var cupCenter=stone.TransformPoint(stone.GetComponent<MeshFilter>().sharedMesh.bounds.center);
            Assert.That(Vector3.Distance(released.transform.position,cupCenter), Is.LessThan(.002f),
                "The projectile must start inside the moving cup, without teleporting.");
            Assert.That(catapult.GetComponent<SpriteRenderer>().color, Is.EqualTo(catapultColor));
            Assert.That(catapult.GetComponent("Stage3MeshFeedback"), Is.Null);
            composition.RunDriver.Run.Tick(.36f);
            Assert.That(target.Health, Is.EqualTo(hp-2));
            Assert.That(GameObject.Find("Stone area impact"), Is.Not.Null);
            rapid.Tick(.01f);
            Assert.That(GameObject.Find("Arrow in flight"), Is.Not.Null);
            yield return null; yield return null;
            Assert.That(rapidModel.localPosition, Is.EqualTo(rapidPosition));
            Assert.That(rapidModel.localRotation, Is.EqualTo(rapidRotation));
            Assert.That(rapid.GetComponent<SpriteRenderer>().color, Is.EqualTo(rapidColor));
            Assert.That(rapid.GetComponent("Stage3MeshFeedback"), Is.Null);
            composition.RunDriver.Run.Tick(.2f);
            Assert.That(GameObject.Find("Arrow impact"), Is.Not.Null);
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(composition.Enemies.PendingShotCount, Is.Zero);
            foreach (var name in new[]{"Stone in flight","Arrow in flight","Stone area impact","Arrow impact"})
                Assert.That(GameObject.Find(name), Is.Null, name);
        }

        [UnityTest]
        public IEnumerator TacticalView_TrackpadPinchSharesWheelLimitsAndClearsOnDisable()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap=Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var follow=bootstrap.CameraFollow;
            var camera=follow.BoundCamera;
            var hero=bootstrap.PlayerMover.transform.position;
            var importer=AssetImporter.GetAtPath("Assets/Plugins/macOS/libNot3ATrackpad.dylib") as PluginImporter;
            Assert.That(importer,Is.Not.Null);
            Assert.That(importer.GetCompatibleWithAnyPlatform(),Is.False);
            Assert.That(importer.GetEditorData("OS"),Is.EqualTo("OSX"));
            Assert.That(importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX),Is.True);
            Assert.That(importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64),Is.False);
#if UNITY_EDITOR_OSX
            Assert.That(Stage2MacTrackpad.IsAvailable,Is.True,"The real universal AppKit library must load in Unity.");
#endif
            follow.ZoomByPinchScale(1.25f);
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize,Is.EqualTo(4f).Within(.02f));
            follow.ZoomByPinchScale(.8f);
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize,Is.EqualTo(5f).Within(.02f));
            foreach(var invalid in new[]{0f,-1f,float.NaN,float.PositiveInfinity,1f})
                follow.ZoomByPinchScale(invalid);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(camera.orthographicSize,Is.EqualTo(5f).Within(.02f));
            follow.ZoomByPinchScale(100f);
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(camera.orthographicSize,Is.EqualTo(2.5f).Within(.02f));
            follow.ZoomByPinchScale(.001f);
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(camera.orthographicSize,Is.EqualTo(10f).Within(.02f));
            follow.enabled=false;
            Assert.That(Stage2MacTrackpad.IsAvailable,Is.False);
            follow.ZoomByPinchScale(2f);
            follow.enabled=true;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(camera.orthographicSize,Is.EqualTo(10f).Within(.02f),"No stale pinch or zoom rebase on resume.");
            follow.ZoomByPinchScale(100f);
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(camera.orthographicSize,Is.EqualTo(2.5f).Within(.02f));
            Assert.That(bootstrap.PlayerMover.transform.position,Is.EqualTo(hero));
            Assert.That(Vector3.Angle(camera.transform.forward,Vector3.forward),Is.EqualTo(35f).Within(.01f));
        }

        [UnityTest]
        public IEnumerator TacticalView_InspectorZoomSettingUpdatesLiveAndWheelClamps()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var follow = bootstrap.CameraFollow;
            var camera = follow.BoundCamera;
            var startSize = camera.orthographicSize;
            Assert.That(startSize, Is.EqualTo(5f).Within(.01f));
            Assert.That(AssetDatabase.GetAssetPath(follow.ZoomSettings),
                Is.EqualTo("Assets/Stage3/VisualPilot/TacticalZoomSettings.asset"));
            var editable = Object.Instantiate(follow.ZoomSettings);
            follow.ConfigureTacticalZoom(editable);
            var serialized = new SerializedObject(editable);
            var multiplier = serialized.FindProperty("stepMultiplier");
            multiplier.floatValue = 2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            input.Set(mouse.scroll, new Vector2(0, 1));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return null;
            Assert.That(camera.orthographicSize, Is.LessThan(startSize));
            Assert.That(camera.orthographicSize, Is.GreaterThan(startSize / 2f));
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize, Is.EqualTo(startSize / 2f).Within(.02f));
            Assert.That(camera.orthographicSize, Is.EqualTo(2.5f).Within(.02f));

            input.Set(mouse.scroll, new Vector2(0, -1));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize, Is.EqualTo(startSize).Within(.02f));

            multiplier.floatValue = 1.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            input.Set(mouse.scroll, new Vector2(0, 120));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize, Is.EqualTo(startSize / 1.25f).Within(.02f));

            input.Set(mouse.scroll, new Vector2(0, 1200));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return new WaitForSecondsRealtime(.9f);
            Assert.That(camera.orthographicSize, Is.EqualTo(startSize * .5f).Within(.02f));

            var map = bootstrap.MapDefinition;
            Assert.That(bootstrap.PlayerMover.PlanarEdgeClearance,Is.EqualTo(1f));
            bootstrap.PlayerMover.transform.position = map.CellToWorld(new Vector2Int(0, 0));
            follow.SnapToTarget();
            input.Set(mouse.scroll, new Vector2(0, -2400));
            yield return null;
            input.Set(mouse.scroll, Vector2.zero);
            yield return new WaitForSecondsRealtime(1.1f);
            Assert.That(camera.orthographicSize, Is.EqualTo(startSize * 2f).Within(.02f));
            Assert.That(camera.orthographicSize, Is.EqualTo(10f).Within(.02f));
            Assert.That(map.Contains(new Vector2Int(map.MinimumCell.x-1,map.MinimumCell.y)), Is.False);

            var ground = new Plane(Vector3.forward, Vector3.zero);
            foreach (var cellPosition in new[]{new Vector2Int(0,0), map.MinimumCell, map.MaximumCell})
            {
                bootstrap.PlayerMover.transform.position = map.CellToWorld(cellPosition);
                follow.SnapToTarget();
                yield return null;
                Assert.That(camera.orthographicSize, Is.EqualTo(startSize * 2f).Within(.02f),
                    "Moving to a map edge must not change zoom.");
                if(cellPosition!=Vector2Int.zero)
                {
                    var expected=cellPosition==map.MinimumCell
                        ? map.MinimumCell+Vector2Int.one : map.MaximumCell-Vector2Int.one;
                    Assert.That(Vector2.Distance(bootstrap.PlayerMover.transform.position,
                        map.CellToWorld(expected)),Is.LessThan(.001f));
                }
                var heroViewport = camera.WorldToViewportPoint(bootstrap.PlayerMover.transform.position);
                Assert.That(heroViewport.x, Is.InRange(0f,1f));
                Assert.That(heroViewport.y, Is.InRange(0f,1f));
                if (cellPosition == Vector2Int.zero)
                {
                    Assert.That(heroViewport.x, Is.EqualTo(.5f).Within(.001f));
                    Assert.That(heroViewport.y, Is.EqualTo(.5f).Within(.001f));
                }
                else Assert.That(Mathf.Abs(heroViewport.x-.5f), Is.GreaterThan(.1f),
                    "At the playable edge the camera must stop centering the hero.");
                var heroBounds=GameObject.Find("Hero scale marker").GetComponent<Renderer>().bounds;
                for(var vertex=0;vertex<8;vertex++)
                {
                    var point=camera.WorldToViewportPoint(new Vector3(
                        (vertex&1)==0?heroBounds.min.x:heroBounds.max.x,
                        (vertex&2)==0?heroBounds.min.y:heroBounds.max.y,
                        (vertex&4)==0?heroBounds.min.z:heroBounds.max.z));
                    Assert.That(point.x,Is.InRange(0f,1f),"The full hero must remain in view.");
                    Assert.That(point.y,Is.InRange(0f,1f),"The full hero must remain in view.");
                }
                foreach (var corner in new[]{Vector2.zero, Vector2.one, new Vector2(0,1), new Vector2(1,0)})
                {
                    var ray = camera.ViewportPointToRay(corner);
                    Assert.That(ground.Raycast(ray,out var distance), Is.True);
                    var point = ray.GetPoint(distance);
                    Assert.That(point.x, Is.InRange(map.MinimumCell.x-.5f,map.MaximumCell.x+.5f));
                    Assert.That(point.y, Is.InRange(map.MinimumCell.y-.5f,map.MaximumCell.y+.5f));
                }
            }
            bootstrap.PlayerMover.transform.position = map.CellToWorld(map.MinimumCell);
            follow.SnapToTarget();
            var stopped = camera.transform.position;
            bootstrap.PlayerMover.transform.position = map.CellToWorld(map.MinimumCell+Vector2Int.one);
            follow.SnapToTarget();
            Assert.That(Vector3.Distance(camera.transform.position,stopped), Is.LessThan(.001f),
                "The camera must stay fixed while the hero crosses the last cells.");
            bootstrap.PlayerMover.transform.position = map.CellToWorld(new Vector2Int(0,0));
            follow.SnapToTarget();
            var cell = map.Throne+new Vector2Int(-2,2);
            var screen = camera.WorldToScreenPoint(bootstrap.MapDefinition.CellToWorld(cell));
            Assert.That(bootstrap.GetComponent<Stage2GameComposition>().Building.TryScreenToCell(screen,out var picked), Is.True);
            Assert.That(picked, Is.EqualTo(cell));
            Object.Destroy(editable);
        }

        [UnityTest]
        public IEnumerator TacticalView_ClickUsesGroundPlaneAndRejectsGrass()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var bootstrap = Object.FindAnyObjectByType<Stage2SceneBootstrap>();
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            Assert.That(bootstrap.MapDefinition.PlanarGrid, Is.True);
            var camera = bootstrap.CameraFollow.BoundCamera;
            Assert.That(Vector3.Angle(camera.transform.forward, Vector3.forward), Is.EqualTo(35f).Within(.01f));
            var objective=bootstrap.MapDefinition.Throne;
            bootstrap.PlayerMover.transform.position=bootstrap.MapDefinition.CellToWorld(objective)+Vector2.left*2f;
            bootstrap.CameraFollow.SnapToTarget();
            foreach (var cell in new[] { objective+new Vector2Int(3,-1),
                objective+new Vector2Int(-2,2), objective+new Vector2Int(-6,0) })
            {
                var screen = camera.WorldToScreenPoint(bootstrap.MapDefinition.CellToWorld(cell));
                Assert.That(composition.Building.TryScreenToCell(screen, out var picked), Is.True);
                Assert.That(picked, Is.EqualTo(cell));
            }
            var mouse = InputSystem.AddDevice<Mouse>();
            composition.RunDriver.ResetRun();
            yield return null;
            var defense = composition.Building.Defense;
            var inventory = composition.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood,2); inventory.TryAdd(MapResourceKind.Stone,1);
            var buildCell = objective+new Vector2Int(-2,2);
            input.Set(mouse.position,(Vector2)camera.WorldToScreenPoint(bootstrap.MapDefinition.CellToWorld(buildCell)));
            Assert.That(defense.BeginPlacement(), Is.True);
            yield return null;
            Assert.That(composition.Building.PreviewResult, Is.EqualTo(PlacementResult.Ready));
            input.Press(mouse.leftButton); yield return null; input.Release(mouse.leftButton);
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(Object.FindAnyObjectByType<Stage2Tower>().Cell, Is.EqualTo(buildCell));
            inventory.TryAddGold(5); inventory.TryAdd(MapResourceKind.Wood,2); inventory.TryAdd(MapResourceKind.Stone,1);
            Assert.That(defense.BeginPlacement(), Is.True);
            var grassCell=objective+new Vector2Int(-6,5);
            Assert.That(bootstrap.MapDefinition.GetSurface(grassCell),Is.EqualTo(MapSurfaceKind.Forest));
            input.Set(mouse.position,(Vector2)camera.WorldToScreenPoint(
                bootstrap.MapDefinition.CellToWorld(grassCell)));
            yield return null;
            Assert.That(composition.Building.PreviewResult, Is.Not.EqualTo(PlacementResult.Ready));
            input.Press(mouse.leftButton); yield return null; input.Release(mouse.leftButton);
            Assert.That(defense.TowerCount, Is.EqualTo(1));
            Assert.That(inventory.Gold, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator TacticalView_PaidDefenseWinsFullRunAndRestarts()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            composition.RunDriver.ResetRun();
            yield return null;
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
                var objective=composition.GetComponent<Stage2SceneBootstrap>().MapDefinition.Throne;
                defense.SetPreviewCell(objective+(index==0 ? new Vector2Int(3,-1) : new Vector2Int(-1,3)));
                Assert.That(defense.TryConfirm(out towers[index]), Is.EqualTo(PlacementResult.Ready));
            }
            Assert.That(towers[0].name, Does.Contain("Rapid"));
            Assert.That(towers[1].name, Does.Contain("Longshot"));
            Assert.That(economy.Inventory.Gold, Is.Zero);

            Assert.That(run.WaveCount, Is.EqualTo(3));
            for (var wave = 1; wave <= run.WaveCount; wave++)
            {
                Assert.That(run.WaveNumber, Is.EqualTo(wave));
                Assert.That(run.StartWave(), Is.True);
                for (var step = 0; step < 2000 && run.Phase == Stage2RunPhase.Wave; step++)
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
                    defense.SetPreviewCell(composition.GetComponent<Stage2SceneBootstrap>()
                        .MapDefinition.Throne+new Vector2Int(-4,3));
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
            var bootstrap=composition.GetComponent<Stage2SceneBootstrap>();
            var outcomeFrame=CapturePortal(bootstrap);
            yield return new WaitForSeconds(.4f);
            Assert.That(ChangedRedPixels(outcomeFrame,CapturePortal(bootstrap)),Is.GreaterThan(100),
                "The ambient vortex must keep circulating in the outcome view.");
            composition.RunDriver.ResetRun();
            Assert.That(run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(run.WaveNumber, Is.EqualTo(1));
            Assert.That(composition.Enemies.ActiveCount, Is.Zero);
            Assert.That(defense.TowerCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpritePilot_UsesSamePaidPlacementAndRestartLoop()
        {
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            Assert.That(composition, Is.Not.Null);
            Assert.That(composition.IsInitialized, Is.True);
            var catalog = AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(
                "Assets/Stage3/VisualPilot/RapidWatchtowerPilotCatalog.asset");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Stage3/VisualPilot/RapidWatchtowerSprite.png");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(sprite, Is.Not.Null);
            var defense = composition.Building.Defense;
            Assert.That(defense.SelectedCatalog, Is.SameAs(catalog));
            var inventory = composition.Gathering.Economy.Inventory;
            inventory.TryAdd(MapResourceKind.Wood, catalog.WoodCost);
            inventory.TryAdd(MapResourceKind.Stone, catalog.StoneCost);
            inventory.TryAdd(MapResourceKind.Metal, catalog.MetalCost);
            if (inventory.Gold < catalog.GoldCost) inventory.TryAddGold(catalog.GoldCost - inventory.Gold);
            Assert.That(defense.BeginPlacement(), Is.True);
            defense.SetPreviewCell(new Vector2Int(-9, -11));
            Assert.That(defense.EvaluatePreview(), Is.EqualTo(PlacementResult.Ready));
            Assert.That(defense.TryConfirm(out var tower), Is.EqualTo(PlacementResult.Ready));
            Assert.That(tower.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
            Assert.That(inventory.Get(MapResourceKind.Wood), Is.Zero);
            Assert.That(inventory.Gold, Is.Zero);
            Assert.That(composition.RunDriver.Run.StartWave(), Is.True);
            yield return null;
            Assert.That(composition.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Wave));
            composition.RunDriver.ResetRun();
            yield return null;
            Assert.That(composition.RunDriver.Run.Phase, Is.EqualTo(Stage2RunPhase.Preparation));
            Assert.That(defense.TowerCount, Is.Zero);
        }
    }
}
#endif
