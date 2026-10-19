using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Integration;
using UnityEngine;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Disposable opt-in player qualification: automatic dense-horde movement and comfortable E reach.</summary>
    public static class Stage3MovementReachReview
    {
        // Explicit development-only observation, with no frame capture or file writes during the walk.
        public static IEnumerator CaptureWalking(GameObject owner, string directory)
        {
            var game=owner.GetComponent<Stage2GameComposition>();
            var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Not3A.Stage3.PlayerInterface.Stage3FantasyHud>();
            while(!game.IsInitialized || !hud.IsReady) yield return null;
            for(var i=0;i<8;i++)yield return null;
            Directory.CreateDirectory(directory);
            var mover=scene.PlayerMover;var origin=mover.transform.position;
            var wasEnabled=mover.enabled;var combat=game.HeroCombat.AutomaticTick;
            var camera=scene.CameraFollow.BoundCamera;var oldZoom=camera.orthographicSize;
            var hudEnabled=hud.enabled;var previousRate=Time.captureFramerate;
            var oldTargetRate=Application.targetFrameRate;var oldVSync=QualitySettings.vSyncCount;
            var oldMultiplier=mover.MovementMultiplier;
            var nativeInput=Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_NATIVE_INPUT")=="1";
            var walkMultiplier=float.TryParse(Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_SPEED"),
                NumberStyles.Float,CultureInfo.InvariantCulture,out var speed)?Mathf.Clamp(speed,1,10):1f;
            var profileZoom=float.TryParse(Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_ZOOM"),
                NumberStyles.Float,CultureInfo.InvariantCulture,out var zoom)?Mathf.Clamp(zoom,2.5f,10f):oldZoom;
            var seconds=float.TryParse(Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_SECONDS"),
                NumberStyles.Float,CultureInfo.InvariantCulture,out var requested)?Mathf.Clamp(requested,5,120):45f;
            var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);
            var description=new StringBuilder();
            var names=new HashSet<string>{"Main Thread","Render Thread","GC Allocated In Frame","GC.Collect",
                "GC.Incremental.Collect","Gfx.WaitForPresentOnGfxThread","WaitForTargetFPS","BehaviourUpdate",
                "ResourceDistrict.Visibility","Hero.GroundMovement","Stage3.ResourceSprites","Stage3.HUD",
                "Camera.Render","GUI.Repaint","RenderLoop.Draw","Draw Calls Count","Triangles Count","GPU Frame Time",
                "Gfx.WaitForRenderThread","Semaphore.WaitForSignal","Gfx.PresentFrame","PresentSwapchain",
                "Gfx.UploadTexture","Gfx.UploadTextureData","Mesh.UploadMeshData","Shader.CompileGPUProgram",
                "Shader.CreateGPUProgram","Shader.DynamicLoadGPUProgram","Mono.JIT","Other Reads","Texture Reads",
                "TimeUpdate.WaitForLastPresentationAndUpdateTime","PostLateUpdate.PresentAfterDraw"};
            var recorders=new List<ProfilerRecorder>();var columns=new List<string>();
            foreach(var handle in handles)
            {
                var info=ProfilerRecorderHandle.GetDescription(handle);
                description.AppendLine(info.Category.Name+" / "+info.Name+" / "+info.UnitType);
                if(!names.Contains(info.Name)) continue;
                var recorder=new ProfilerRecorder(handle,1);recorder.Start();recorders.Add(recorder);columns.Add(info.Name);
            }
            File.WriteAllText(Path.Combine(directory,"available-counters.txt"),description.ToString());
            const int capacity=32768;
            var samples=new double[capacity,7+recorders.Count];var count=0;var travelled=0f;
            try
            {
                game.SetPaused(false);game.HeroCombat.AutomaticTick=false;mover.enabled=nativeInput;mover.SetMovementMultiplier(1);
                hud.CloseWindow();Time.captureFramerate=0;
                if(int.TryParse(Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_VSYNC"),out var vSync))
                    QualitySettings.vSyncCount=Mathf.Clamp(vSync,0,1);
                if(int.TryParse(Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_TARGET_FPS"),out var targetFps))
                    Application.targetFrameRate=Mathf.Clamp(targetFps,30,240);
                if(profileZoom!=oldZoom)scene.CameraFollow.ZoomByPinchScale(oldZoom/profileZoom);
                var route=nativeInput?null:WalkingRoute(scene);var waypoint=1;var direction=1;
                if(Environment.GetEnvironmentVariable("NOT3A_WALK_PROFILE_DISABLE_HUD")=="1")hud.enabled=false;
                for(var i=0;i<120;i++)yield return null;
                if(walkMultiplier>1)
                {var warmup=Time.realtimeSinceStartupAsDouble+3;while(Time.realtimeSinceStartupAsDouble<warmup)yield return null;}
                var started=Time.realtimeSinceStartupAsDouble;var allocated=GC.GetAllocatedBytesForCurrentThread();
                var collections=GC.CollectionCount(0);var recoveries=mover.OverlapRecoveryCount;
                UnityEngine.Debug.Log("S3_WALK_PROFILE_STARTED "+directory);
                while(count<capacity && Time.realtimeSinceStartupAsDouble-started<seconds)
                {
                    var before=(Vector2)mover.transform.position;
                    if(!nativeInput)
                    {
                        var delta=route[waypoint]-before;
                        mover.MoveBy(Vector2.ClampMagnitude(delta,mover.Speed*walkMultiplier*Time.deltaTime));
                        if(Vector2.Distance(mover.transform.position,route[waypoint])<.015f)
                        {
                            if(waypoint==route.Count-1)direction=-1;else if(waypoint==0)direction=1;
                            waypoint+=direction;
                        }
                    }
                    yield return null;
                    travelled+=Vector2.Distance(before,mover.transform.position);
                    var nowAllocated=GC.GetAllocatedBytesForCurrentThread();var nowCollections=GC.CollectionCount(0);
                    samples[count,0]=Time.realtimeSinceStartupAsDouble-started;samples[count,1]=Time.unscaledDeltaTime*1000;
                    samples[count,2]=mover.transform.position.x;samples[count,3]=mover.transform.position.y;
                    samples[count,4]=travelled;samples[count,5]=nowAllocated-allocated;samples[count,6]=nowCollections-collections;
                    for(var i=0;i<recorders.Count;i++)samples[count,7+i]=recorders[i].LastValue;
                    allocated=nowAllocated;collections=nowCollections;count++;
                }
                var csv=new StringBuilder("seconds,frame_ms,x,y,distance,thread_allocated_bytes,gc_collections");
                foreach(var name in columns)csv.Append(',').Append(name);csv.AppendLine();
                for(var row=0;row<count;row++)
                {
                    for(var col=0;col<samples.GetLength(1);col++)
                    {if(col>0)csv.Append(',');csv.Append(samples[row,col].ToString("0.######",CultureInfo.InvariantCulture));}
                    csv.AppendLine();
                }
                File.WriteAllText(Path.Combine(directory,"frames.csv"),csv.ToString());
                File.WriteAllText(Path.Combine(directory,"context.txt"),$"Actual {Application.platform} {SystemInfo.graphicsDeviceType} development player; disposable observation through real MoveBy/collisions; camera follows normally; no screenshots/file writes during measured frames.\nframes={count} distance={travelled:F3} recoveries={mover.OverlapRecoveryCount-recoveries} size={Screen.width}x{Screen.height} targetFPS={Application.targetFrameRate} vSync={QualitySettings.vSyncCount} HUD={hud.enabled} zoom={camera.orthographicSize} speed={walkMultiplier} nativeWASD={nativeInput} seed={owner.GetComponent<Stage3ResourceDistrictWorld>().Seed}\nNative GC Allocated In Frame is authoritative; Mono thread-allocation API may return unsupported zero.\n");
                UnityEngine.Debug.Log("S3_WALK_PROFILE_COMPLETE "+directory);
            }
            finally
            {
                foreach(var recorder in recorders)recorder.Dispose();
                mover.transform.position=origin;mover.enabled=wasEnabled;mover.SetMovementMultiplier(oldMultiplier);
                game.HeroCombat.AutomaticTick=combat;hud.enabled=hudEnabled;Time.captureFramerate=previousRate;
                Application.targetFrameRate=oldTargetRate;QualitySettings.vSyncCount=oldVSync;
                if(profileZoom!=oldZoom)scene.CameraFollow.ZoomByPinchScale(profileZoom/oldZoom);
                camera.orthographicSize=oldZoom;
                scene.CameraFollow.SnapToTarget();
            }
            yield return Shot(directory,"normal-hud");
        }

        private static List<Vector2> WalkingRoute(Stage2SceneBootstrap scene)
        {
            var map=scene.MapDefinition;var mover=scene.PlayerMover;var obstacles=mover.Obstacles;
            var width=map.Size.x;var total=width*map.Size.y;var minimum=map.MinimumCell;
            int Index(Vector2Int cell) => (cell.y-minimum.y)*width+cell.x-minimum.x;
            Vector2Int Cell(int index) => minimum+new Vector2Int(index%width,index/width);
            var start=map.WorldToCell(mover.transform.position);var goal=minimum+new Vector2Int(width-20,map.Size.y/2);
            if(!obstacles.TryFindFreePosition(map.CellToWorld(goal),mover.CollisionRadius+.03f,
                new Rect(map.CellToWorld(goal)-Vector2.one*8,Vector2.one*16),out var free))
                throw new InvalidOperationException("No profiling route goal.");
            goal=map.WorldToCell(free);
            var costs=new int[total];var parents=new int[total];Array.Fill(costs,int.MaxValue);Array.Fill(parents,-1);
            var open=new SortedSet<(float Score,int Cell)>();var startIndex=Index(start);costs[startIndex]=0;
            open.Add((0,startIndex));var closed=new bool[total];var target=Index(goal);
            var offsets=new[]{Vector2Int.right,Vector2Int.up,Vector2Int.left,Vector2Int.down};
            while(open.Count>0)
            {
                var current=open.Min;open.Remove(current);if(closed[current.Cell])continue;closed[current.Cell]=true;
                if(current.Cell==target)break;var cell=Cell(current.Cell);
                foreach(var offset in offsets)
                {
                    var next=cell+offset;if(!map.Contains(next)||!map.IsWalkable(next))continue;
                    var nextIndex=Index(next);if(closed[nextIndex]||costs[nextIndex]<=costs[current.Cell]+1)continue;
                    var world=map.CellToWorld(cell);var destination=map.CellToWorld(next);
                    if(Vector2.Distance(obstacles.Move(world,destination-world,mover.CollisionRadius),destination)>.01f)continue;
                    costs[nextIndex]=costs[current.Cell]+1;parents[nextIndex]=current.Cell;
                    var heuristic=Mathf.Abs(goal.x-next.x)+Mathf.Abs(goal.y-next.y);
                    open.Add((costs[nextIndex]+heuristic*1.01f,nextIndex));
                }
            }
            if(parents[target]<0)throw new InvalidOperationException("No profiling walk route.");
            var route=new List<Vector2>();for(var index=target;index>=0;index=parents[index])route.Add(map.CellToWorld(Cell(index)));
            route.Reverse();return route;
        }

        public static IEnumerator Capture(GameObject owner, string directory)
        {
            yield return Horde(owner, directory);
            yield return Stage3ResourceInteractionReview.Capture(owner, Path.Combine(directory, "resources"), true);
            UnityEngine.Debug.Log("S3_MOVEMENT_REACH_PLAYER_COMPLETE " + directory);
        }

        private static IEnumerator Horde(GameObject owner, string directory)
        {
            var game = owner.GetComponent<Stage2GameComposition>();
            while (game == null || !game.IsInitialized) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var camera = scene.CameraFollow.BoundCamera;
            var position = camera.transform.position; var zoom = camera.orthographicSize;
            var following = scene.CameraFollow.enabled; var combat = game.HeroCombat.AutomaticTick;
            var rate = Time.captureFramerate;
            Directory.CreateDirectory(directory);
            var report = new StringBuilder("Actual Mac development player, automatic Enemy.Update movement at game speed 1.\n" +
                "Disposable QA: initial stock-free wave cleared through scheduled-spawn ticks/explicit kills solely to reach the authored second horde. No forced movement, arrivals, route edits or time-scale acceleration in the observed horde.\n");
            try
            {
                Time.captureFramerate = 0; game.HeroCombat.AutomaticTick = false;
                scene.CameraFollow.enabled = false; game.RunDriver.ResetRun();
                var timings = new double[8];
                for (var i = 0; i < timings.Length; i++)
                {
                    var clock = Stopwatch.StartNew();
                    game.Enemies.Spawn().SetAutomaticTick(false);
                    timings[i] = clock.Elapsed.TotalMilliseconds;
                }
                report.AppendLine($"Eight fresh spawns, no pooling: mean={timings.Average():F4}ms max={timings.Max():F4}ms (actual player; no budget assertion).");
                game.RunDriver.ResetRun(); var run = game.RunDriver.Run;
                run.StartWave(); run.Tick(60f);
                for (var i = game.Enemies.ActiveCount - 1; i >= 0; i--) game.Enemies.GetEnemy(i).TakeDamage(game.Enemies.GetEnemy(i).Health);
                if (run.WaveNumber != 2 || !run.StartWave()) throw new InvalidOperationException("Horde review could not start the authored second wave.");
                var lead = game.Enemies.GetEnemy(0); var origin = lead.transform.position;
                camera.orthographicSize = 10f;
                camera.transform.position = origin + Vector3.back * .55f - camera.transform.forward * 30f;
                yield return Shot(directory, "horde-start");
                var started = Time.timeAsDouble; var still = 0f; var maximumStill = 0f; var previous = lead.transform.position;
                while (Time.timeAsDouble - started < 16d)
                {
                    yield return null;
                    if (lead == null || run.IsTerminal || run.IsPaused) throw new InvalidOperationException("Horde review stopped unexpectedly.");
                    still = (lead.transform.position - previous).sqrMagnitude < 1e-12f ? still + Time.deltaTime : 0f;
                    maximumStill = Mathf.Max(maximumStill, still); previous = lead.transform.position;
                    if (still > .2f) throw new InvalidOperationException($"Lead creep stopped at {lead.CurrentCell}: {lead.Plan?.Status}.");
                    camera.transform.position = lead.transform.position + Vector3.back * .55f - camera.transform.forward * 30f;
                }
                if (game.Enemies.ActiveCount != run.WaveEnemyCount || !run.SpawningFinished)
                    throw new InvalidOperationException("Authored dense horde did not finish spawning.");
                for (var i = 0; i < game.Enemies.ActiveCount; i++)
                {
                    var enemy = game.Enemies.GetEnemy(i);
                    if (enemy.Plan == null || enemy.Plan.Status != Not3A.Stage2.Navigation.NavigationStatus.ReachesThrone)
                        throw new InvalidOperationException("Dense horde contains an actor without a usable route.");
                }
                report.AppendLine($"PASS {game.Enemies.ActiveCount} actual enemies spawned through the normal RunDriver cadence; lead displacement={Vector2.Distance(origin, lead.transform.position):F3}, longest stationary active-time sample={maximumStill:F4}s; all plans reach throne.");
                yield return Shot(directory, "horde-moving");
                File.WriteAllText(Path.Combine(directory, "horde-context.txt"), report.ToString());
                UnityEngine.Debug.Log("S3_DENSE_HORDE_PLAYER_PASS " + directory);
            }
            finally
            {
                game.RunDriver.ResetRun(); game.HeroCombat.AutomaticTick = combat;
                camera.orthographicSize = zoom; camera.transform.position = position;
                scene.CameraFollow.enabled = following; Time.captureFramerate = rate;
            }
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
