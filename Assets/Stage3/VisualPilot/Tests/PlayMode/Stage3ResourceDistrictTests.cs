#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.Profiling;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3ResourceDistrictTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Component world;
        private Type type;
        private float originalTimeScale;
        private int Seed => (int)type.GetProperty("Seed").GetValue(world);
        private IReadOnlyList<ResourceDistrict> Districts =>
            (IReadOnlyList<ResourceDistrict>)type.GetProperty("Districts").GetValue(world);
        private GameObject Scenery => GameObject.Find("Procedural resource districts seed " + Seed);
        private HeroObstacleMap Obstacles => (HeroObstacleMap)type.GetProperty("Obstacles").GetValue(world);

        [UnitySetUp]
        public IEnumerator Load()
        {
            originalTimeScale=Time.timeScale;
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            world=game.GetComponent("Stage3ResourceDistrictWorld");
            Assert.That(world,Is.Not.Null);type=world.GetType();
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null;yield return null;input.TearDown();
            Time.timeScale=originalTimeScale;
        }

        private void Regenerate(int seed)=>type.GetMethod("Regenerate").Invoke(world,new object[]{seed});

        [UnityTest]
        public IEnumerator StoneAndIronBlockActualAcceleratedKeyboardMovement()
        {
            Regenerate(164032); yield return null;
            var mover=scene.PlayerMover;var hud=game.GetComponent<Stage2GameHud>();
            Assert.That(mover.Obstacles,Is.SameAs(Obstacles));
            hud.SetHeroSpeed(10);hud.SetGameSpeed(10);
            foreach(var district in Districts.Where(d=>d.Kind!=MapResourceKind.Wood).Take(2))
            {
                var edge=district.Cells.Where(c=>c.y==district.Center.y).OrderBy(c=>c.x).First();
                var start=(Vector2)edge+Vector2.left*1.4f;
                Assert.That(Obstacles.IsBlocked(start,mover.CollisionRadius),Is.False);
                mover.transform.position=start;
                // A single very long movement cannot jump the whole solid district.
                mover.MoveBy(Vector2.right*100);
                Assert.That(mover.transform.position.x,Is.LessThan(edge.x));
                Assert.That(mover.transform.position.y,Is.EqualTo(start.y).Within(.005f),"A head-on approach must stop, not be redirected past the district.");
                Assert.That(Vector2.Distance(mover.transform.position,edge),Is.LessThan(1.1f));
                Assert.That(Obstacles.IsBlocked(mover.transform.position,mover.CollisionRadius),Is.False);
                var contact=(Vector2)mover.transform.position;
                input.Press(Keyboard.current.dKey);
                for(var i=0;i<8;i++)
                {
                    yield return null;
                    Assert.That(Obstacles.IsBlocked(mover.transform.position,mover.CollisionRadius),Is.False);
                    Assert.That(Vector2.Distance(mover.transform.position,contact),Is.LessThan(.01f),
                        "Holding movement into the same obstacle must not jitter, teleport or tunnel at x10/x10.");
                }
                input.Release(Keyboard.current.dKey);
                // Inspect a reproducible real Game-camera contact frame.
                mover.transform.position=contact;
                Debug.Log($"S3_COLLISION_CONTACT kind={district.Kind} hero={contact:F4} edge={edge}");
                yield return CaptureContact(district.Kind.ToString());
            }
            hud.SetGameSpeed(1);hud.SetHeroSpeed(1);
        }

        [UnityTest]
        public IEnumerator ForestBlocksIndividualTrunksButAllowsAGapBetweenTrees()
        {
            Regenerate(164032);yield return null;
            var mover=scene.PlayerMover;var forest=Districts.First(d=>d.Kind==MapResourceKind.Wood);
            var cells=new HashSet<Vector2Int>(forest.Cells);
            var trunks=Obstacles.Circles.Where(c=>cells.Contains(Vector2Int.RoundToInt(c.Center))).ToArray();
            Assert.That(trunks.Length,Is.GreaterThan(5));
            var testedTrunk=false;var testedGap=false;
            foreach(var trunk in trunks)
            {
                var start=trunk.Center+Vector2.left*(trunk.Radius+mover.CollisionRadius+.15f);
                if(Obstacles.IsBlocked(start,mover.CollisionRadius)) continue;
                mover.transform.position=start;mover.MoveBy(Vector2.right*4);
                Assert.That(mover.transform.position.x,Is.LessThan(trunk.Center.x));
                Assert.That(Obstacles.IsBlocked(mover.transform.position,mover.CollisionRadius),Is.False);
                testedTrunk=true;break;
            }
            foreach(var a in trunks)
            {
                foreach(var b in trunks)
                {
                    var span=b.Center-a.Center;
                    if(span.magnitude<1.2f || span.magnitude>2.4f) continue;
                    var middle=(a.Center+b.Center)*.5f;
                    var perpendicular=new Vector2(-span.y,span.x).normalized*.65f;
                    var from=middle-perpendicular;var to=middle+perpendicular;
                    if(Enumerable.Range(0,21).Any(i=>Obstacles.IsBlocked(Vector2.Lerp(from,to,i/20f),mover.CollisionRadius))) continue;
                    mover.transform.position=from;mover.MoveBy(to-from);
                    Assert.That(Vector2.Distance(mover.transform.position,to),Is.LessThan(.002f));
                    yield return CaptureContact("forest-gap");testedGap=true;break;
                }
                if(testedGap)break;
            }
            Assert.That(testedTrunk,Is.True);Assert.That(testedGap,Is.True);
        }

        [UnityTest]
        public IEnumerator RegenerationReplacesObstaclesAndRecoversHeroWhileRestartKeepsThem()
        {
            Regenerate(164032);yield return null;
            var mover=scene.PlayerMover;var old=Obstacles;
            var oldSnapshot=CollisionSnapshot(old);
            Regenerate(164033);yield return null;
            var futureSolid=Districts.First(d=>d.Kind==MapResourceKind.Stone).Center;
            Regenerate(164032);yield return null;
            Assert.That(CollisionSnapshot(Obstacles),Is.EqualTo(oldSnapshot));
            mover.transform.position=(Vector2)futureSolid;
            Regenerate(164033);yield return null;
            Assert.That(Obstacles,Is.Not.SameAs(old));Assert.That(mover.Obstacles,Is.SameAs(Obstacles));
            Assert.That(Obstacles.IsBlocked(mover.transform.position,mover.CollisionRadius),Is.False);
            Assert.That(Vector2.Distance(mover.transform.position,futureSolid),Is.GreaterThan(.2f));
            var retained=Obstacles;game.RunDriver.ResetRun();yield return null;
            Assert.That(mover.Obstacles,Is.SameAs(retained));
            Assert.That(mover.transform.position.x,Is.EqualTo(18));Assert.That(mover.transform.position.y,Is.EqualTo(28));
            Assert.That(retained.IsBlocked(mover.transform.position,mover.CollisionRadius),Is.False);
            foreach(var district in Districts.Where(d=>d.Kind!=MapResourceKind.Wood))
                foreach(var cell in district.Cells) Assert.That(retained.IsBlocked(cell,.2f),Is.True);
            var map=scene.MapDefinition;
            for(var y=map.MinimumCell.y;y<=map.MaximumCell.y;y++)
            for(var x=map.MinimumCell.x;x<=map.MaximumCell.x;x++)
            {
                var cell=new Vector2Int(x,y);
                if(map.GetSurface(cell)==MapSurfaceKind.InvasionCorridor)
                    Assert.That(retained.IsBlocked(cell,.2f),Is.False,"The combat road must stay clear.");
            }
        }

        private static string CollisionSnapshot(HeroObstacleMap map)=>
            string.Join("|",map.Rectangles.Select(r=>r.ToString("F4",CultureInfo.InvariantCulture)))+"/"+
            string.Join("|",map.Circles.Select(c=>c.Center.ToString("F4",CultureInfo.InvariantCulture)+":"+
                c.Radius.ToString("F4",CultureInfo.InvariantCulture)));

        [UnityTest]
        public IEnumerator LocalRevealEasesToPartialOpacityAndReturnsWithoutOneFrameFlashes()
        {
            Regenerate(164032);yield return null;
            var mover=scene.PlayerMover;mover.enabled=false;
            var forest=Districts.First(d=>d.Kind==MapResourceKind.Wood);
            var plan=((IReadOnlyList<ResourceDistrictLayout>)type.GetProperty("Layouts").GetValue(world)).First(p=>p.District==forest);
            var refresh=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),world,type.GetMethod("RefreshVisibility"));
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;camera.orthographicSize=5;camera.farClipPlane=300;
            var up=new Vector2(camera.transform.up.x,camera.transform.up.y).normalized;
            var found=false;SpriteRenderer faded=null;Vector2 hiddenPoint=default;
            foreach(var tree in plan.Trees.OrderByDescending(t=>t.Scale))
            {
                hiddenPoint=tree.Point+up*(tree.Scale*1.6f);
                mover.transform.position=hiddenPoint;
                camera.transform.position=(Vector3)hiddenPoint-camera.transform.forward*30;
                foreach(var sprite in Scenery.GetComponentsInChildren<SpriteRenderer>())
                { var color=sprite.color;color.a=1;sprite.color=color; }
                refresh(1/60f);
                faded=Scenery.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(s=>s.color.a<.999f);
                if(faded!=null){found=true;break;}
            }
            Assert.That(found,Is.True,"At least one real mature canopy must occlude the hero in this fixture.");
            Assert.That(faded.color.a,Is.GreaterThan(.9f),"Reveal must begin gradually.");
            var previous=faded.color.a;
            for(var i=0;i<60;i++)
            {
                refresh(1/60f);
                Assert.That(Mathf.Abs(faded.color.a-previous),Is.LessThan(.09f));previous=faded.color.a;
                Assert.That(faded.color.a,Is.GreaterThanOrEqualTo(.349f));
            }
            Assert.That(faded.color.a,Is.InRange(.349f,.45f));
            yield return AssertVisibleHero(camera,"partial-reveal");
            Directory.CreateDirectory("Artifacts/S3-DISTRICT-REFINEMENT-20261016/");
            Save(camera,"Artifacts/S3-DISTRICT-REFINEMENT-20261016/partial-canopy-reveal.png");
            mover.transform.position=(Vector2)forest.Center+Vector2.one*40;
            refresh(1/60f);
            Assert.That(Mathf.Abs(faded.color.a-previous),Is.LessThan(.09f),"Leaving one sample must not restore opacity instantly.");
            for(var i=0;i<120;i++)refresh(1/60f);
            Assert.That(faded.color.a,Is.GreaterThan(.995f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ForestMovementAndVisibilityHotPathDoNotAllocateOrRecoverByTeleporting()
        {
            Regenerate(164033);yield return null;
            var mover=scene.PlayerMover;mover.enabled=false;
            var forest=Districts.First(d=>d.Kind==MapResourceKind.Wood);
            mover.transform.position=(Vector2)forest.Center;mover.MoveBy(Vector2.zero);
            var refresh=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),world,type.GetMethod("RefreshVisibility"));
            for(var i=0;i<10;i++){mover.MoveBy(Vector2.right*.02f);refresh(1/60f);}
            var recoveries=mover.OverlapRecoveryCount;
            var allocated=GC.GetAllocatedBytesForCurrentThread();
            var start=System.Diagnostics.Stopwatch.GetTimestamp();
            var maximumJump=0f;
            for(var i=0;i<2000;i++)
            {
                var target=(Vector2)forest.Center+new Vector2(Mathf.Cos(i*.013f),Mathf.Sin(i*.013f))*5;
                var before=(Vector2)mover.transform.position;
                var delta=(target-before).normalized*.07f;
                mover.MoveBy(delta);refresh(1/60f);
                maximumJump=Mathf.Max(maximumJump,Vector2.Distance(before,mover.transform.position));
            }
            var ticks=System.Diagnostics.Stopwatch.GetTimestamp()-start;
            var bytes=GC.GetAllocatedBytesForCurrentThread()-allocated;
            Assert.That(mover.OverlapRecoveryCount,Is.EqualTo(recoveries),"Normal walking cannot invoke teleport recovery.");
            Assert.That(maximumJump,Is.LessThan(.073f));
            Assert.That(bytes,Is.LessThanOrEqualTo(256),"Hot movement/visibility must not allocate managed memory per update.");
            var milliseconds=ticks*1000.0/System.Diagnostics.Stopwatch.Frequency;
            var json="{\"samples\":2000,\"total_cpu_ms\":"+milliseconds.ToString("F4",CultureInfo.InvariantCulture)+
                ",\"mean_cpu_ms\":"+(milliseconds/2000).ToString("F4",CultureInfo.InvariantCulture)+
                ",\"managed_bytes\":"+bytes+",\"walking_recoveries\":"+(mover.OverlapRecoveryCount-recoveries)+
                ",\"maximum_step\":"+maximumJump.ToString("F6",CultureInfo.InvariantCulture)+"}";
            Directory.CreateDirectory("Artifacts/S3-DISTRICT-REFINEMENT-20261016/");
            File.WriteAllText("Artifacts/S3-DISTRICT-REFINEMENT-20261016/hot-path-profile.json",json);
            yield return null;
        }

        private IEnumerator CaptureContact(string name)
        {
            const string folder="Artifacts/S3-DISTRICT-REFINEMENT-20261016/";Directory.CreateDirectory(folder);
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.enabled=false;
            camera.orthographicSize=5f;camera.farClipPlane=300;
            camera.transform.position=scene.PlayerMover.transform.position-camera.transform.forward*30f;
            yield return null;yield return null;
            Save(camera,folder+name+"-contact.png");
        }

        [UnityTest]
        public IEnumerator ProfileActualWalkingFramesAfterWorldCreation()
        {
            Regenerate(164033);yield return null;
            var mover=scene.PlayerMover;var hud=game.GetComponent<Stage2GameHud>();
            hud.SetGameSpeed(1);hud.SetHeroSpeed(1);
            var forest=Districts.First(d=>d.Kind==MapResourceKind.Wood);
            mover.transform.position=(Vector2)forest.Center;mover.MoveBy(Vector2.zero);scene.CameraFollow.SnapToTarget();
            for(var i=0;i<40;i++)yield return null;
            var recoveries=mover.OverlapRecoveryCount;
            var frames=new List<double>(120);var visibility=new List<double>(120);var movement=new List<double>(120);
            var vis=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"ResourceDistrict.Visibility",1);
            var move=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"Hero.GroundMovement",1);
            try
            {
                input.Press(Keyboard.current.wKey);
                for(var i=0;i<120;i++)
                {
                    if(i==60){input.Release(Keyboard.current.wKey);input.Press(Keyboard.current.dKey);}
                    yield return null;frames.Add(Time.unscaledDeltaTime*1000);visibility.Add(vis.LastValue*.000001);movement.Add(move.LastValue*.000001);
                }
            }
            finally { input.Release(Keyboard.current.wKey);input.Release(Keyboard.current.dKey);vis.Dispose();move.Dispose(); }
            Assert.That(mover.OverlapRecoveryCount,Is.EqualTo(recoveries));
            frames.Sort();visibility.Sort();movement.Sort();
            var json="{\"frames\":120,\"frame_p50_ms\":"+frames[60].ToString("F4",CultureInfo.InvariantCulture)+
                ",\"frame_p95_ms\":"+frames[114].ToString("F4",CultureInfo.InvariantCulture)+
                ",\"frame_max_ms\":"+frames[119].ToString("F4",CultureInfo.InvariantCulture)+
                ",\"visibility_p95_ms\":"+visibility[114].ToString("F4",CultureInfo.InvariantCulture)+
                ",\"movement_p95_ms\":"+movement[114].ToString("F4",CultureInfo.InvariantCulture)+
                ",\"walking_recoveries\":"+(mover.OverlapRecoveryCount-recoveries)+",\"environment\":\"Windows Unity PlayMode after 40 warmup frames\"}";
            Directory.CreateDirectory("Artifacts/S3-DISTRICT-REFINEMENT-20261016/");
            File.WriteAllText("Artifacts/S3-DISTRICT-REFINEMENT-20261016/walking-frame-profile.json",json);
        }

        [UnityTest]
        public IEnumerator RealSceneUsesModularSpritesAndCachedMinimapWithoutChangingRules()
        {
            Assert.That(Districts.Count,Is.EqualTo(6));
            Assert.That(Districts.All(d=>d.Cells.Count>=200 && d.Cells.Count<=300),Is.True);
            var sprites=Scenery.GetComponentsInChildren<SpriteRenderer>();
            Assert.That(sprites.Length,Is.GreaterThan(35));
            Assert.That(sprites.Select(s=>s.sprite.name).Distinct().Count(),Is.GreaterThan(10));
            Assert.That(Scenery.GetComponentsInChildren<MeshFilter>().Length,Is.EqualTo(10),"Hybrid scenery owns six ground aprons and four relief meshes.");
            foreach(var filter in Scenery.GetComponentsInChildren<MeshFilter>())
                foreach(var color in filter.sharedMesh.colors)Assert.That(color.a,Is.InRange(0f,1f),"Ground transition alpha must remain valid.");
            Assert.That(Scenery.GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(scene.MapDefinition.ComputeDeterministicContentHash(),Is.EqualTo("ACB42D4354F114AE"));
            Assert.That(scene.MapDefinition.Size,Is.EqualTo(new Vector2Int(250,250)));
            Assert.That(game.Gathering.Economy.WorldPosition(MapResourceKind.Wood),Is.EqualTo(new Vector2(17,32)));
            var minimap=game.GetComponent<Stage2Minimap>();
            Assert.That(minimap.DistrictTerrain,Is.Not.Null);
            Assert.That(minimap.DistrictTerrain.width,Is.EqualTo(512));
            Assert.That(minimap.ContainsScreenPoint(new Vector2(minimap.PanelRect.center.x,Screen.height-minimap.PanelRect.center.y)),Is.True);
            Assert.That(game.RunDriver.Run.StartWave(),Is.True);
            Assert.That(game.Enemies.ActiveCount,Is.GreaterThan(0));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SeedReproducesSpritesDifferentSeedVariesAndRestartKeepsWorld()
        {
            Regenerate(164032);yield return null;yield return null;
            var snapshot=Snapshot();var districts=Districts;
            var inventory=game.Gathering.Economy.Inventory;
            var before=inventory.Gold;
            Regenerate(164032);yield return null;yield return null;
            Assert.That(Snapshot(),Is.EqualTo(snapshot));
            Assert.That(inventory.Gold,Is.EqualTo(before));
            Assert.That(Object.FindObjectsByType<GameObject>().Count(o=>o.name.StartsWith("Procedural resource districts seed ")),Is.EqualTo(1));
            Regenerate(164033);yield return null;yield return null;
            Assert.That(Snapshot(),Is.Not.EqualTo(snapshot));
            var retained=Districts;var seed=Seed;
            game.RunDriver.ResetRun();yield return null;
            Assert.That(Seed,Is.EqualTo(seed));Assert.That(Districts,Is.SameAs(retained));
            Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<Texture2D>().Count(t=>t.name.StartsWith("Generated ")),Is.Zero);
        }

        [UnityTest]
        public IEnumerator CaptureTwoSeedsAndKeepHeroReadableInsideForest()
        {
            const string folder="Artifacts/S3-DISTRICT-REFINEMENT-20261016/";
            Directory.CreateDirectory(folder);
            var camera=scene.CameraFollow.BoundCamera;
            scene.CameraFollow.enabled=false;
            camera.farClipPlane=300f;
            foreach(var seed in new[]{164032,164033})
            {
                Regenerate(seed);yield return null;yield return null;
                foreach(var district in Districts.Take(3))
                {
                    camera.orthographicSize=14f;
                    camera.transform.position=(Vector3)(Vector2)district.Center-camera.transform.forward*30f;
                    yield return null;
                    Save(camera,folder+district.Kind+"-"+seed+".png");
                    if(district.Kind!=MapResourceKind.Wood)
                    {
                        camera.orthographicSize=10;
                        var focus=(Vector3)(Vector2)district.Center+Vector3.forward*(district.Kind==MapResourceKind.Metal?2f:-2f);
                        camera.transform.position=focus-camera.transform.forward*30;
                        yield return null;
                        Save(camera,folder+district.Kind+"-"+seed+"-detail.png");
                    }
                }
            }
            var forest=Districts.First(d=>d.Kind==MapResourceKind.Wood);
            scene.PlayerMover.transform.position=(Vector2)forest.Center;
            camera.orthographicSize=5f;
            camera.transform.position=(Vector3)(Vector2)forest.Center-camera.transform.forward*30f;
            yield return new WaitForSecondsRealtime(.7f);
            Save(camera,folder+"hero-in-forest.png");
            yield return AssertVisibleHero(camera,"forest-center");
        }

        private string Snapshot()=>string.Join("|",Scenery.GetComponentsInChildren<SpriteRenderer>()
            .Select(s=>s.sprite.name+":"+s.transform.position.ToString("F4",CultureInfo.InvariantCulture)+":"+
                s.transform.localScale.ToString("F4",CultureInfo.InvariantCulture)+":"+s.flipX));

        private static int Save(Camera camera,string path)
        {
            var target=new RenderTexture(1280,900,24);var old=camera.targetTexture;var active=RenderTexture.active;
            var image=new Texture2D(1280,900,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1280,900),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
                var cyan=0;
                for(var y=300;y<600;y++) for(var x=500;x<780;x++)
                {
                    var color=image.GetPixel(x,y);
                    if(color.b>.25f && color.b>color.g*.95f && color.g>color.r*1.25f) cyan++;
                }
                return cyan;
            }
            finally{camera.targetTexture=old;RenderTexture.active=active;Object.Destroy(target);Object.Destroy(image);}
        }

        private IEnumerator AssertVisibleHero(Camera camera,string stem)
        {
            var marker=scene.PlayerMover.transform.Find("Hero scale marker").GetComponent<Renderer>();
            yield return null;
            var before=ReadFrame(camera);marker.enabled=false;
            Color32[] without;
            yield return null;
            try { without=ReadFrame(camera); } finally { marker.enabled=true; }
            yield return null;
            var changed=0;
            for(var y=300;y<600;y++)for(var x=500;x<780;x++)
            {
                var a=before[y*1280+x];var b=without[y*1280+x];
                if(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)>24)changed++;
            }
            const string folder="Artifacts/S3-DISTRICT-REFINEMENT-20261016/";
            Directory.CreateDirectory(folder);
            foreach(var pair in new[]{(name:stem+"-with-hero.png",pixels:before),(name:stem+"-without-hero.png",pixels:without)})
            {
                var image=new Texture2D(1280,900,TextureFormat.RGB24,false);
                image.SetPixels32(pair.pixels);image.Apply();File.WriteAllBytes(folder+pair.name,image.EncodeToPNG());Object.Destroy(image);
            }
            Assert.That(changed,Is.GreaterThan(100),"Paired rendered frames must retain a visible hero contribution under partial occlusion.");
        }

        private static Color32[] ReadFrame(Camera camera)
        {
            var target=new RenderTexture(1280,900,24);var old=camera.targetTexture;var active=RenderTexture.active;
            var image=new Texture2D(1280,900,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1280,900),0,0);image.Apply();return image.GetPixels32();
            }
            finally { camera.targetTexture=old;RenderTexture.active=active;Object.Destroy(target);Object.Destroy(image); }
        }
    }
}
#endif
