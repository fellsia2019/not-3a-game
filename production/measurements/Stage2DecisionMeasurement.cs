// Run through Unity CLI eval_file in the clean Stage 2 Editor Play scene.
// Script evaluation installs a temporary callback; no scene or data is saved.

if (!Application.isPlaying || Math.Abs(Time.timeScale - 1f) > 0.001f)
    throw new InvalidOperationException("Run in Editor Play at timeScale=1.");
var composition = UnityEngine.Object.FindAnyObjectByType<Not3A.Stage2.Integration.Stage2GameComposition>();
var bootstrap = UnityEngine.Object.FindAnyObjectByType<Not3A.Stage2.Integration.Stage2SceneBootstrap>();
if (composition == null || !composition.IsInitialized)
    throw new InvalidOperationException("Stage 2 gameplay is not initialized.");
foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
    if (device.name == "S2MeasurementKeyboard")
        throw new InvalidOperationException("A measurement keyboard already exists; stop the previous Play session first.");
var folder = System.IO.Path.GetFullPath("Artifacts/S2-DecisionBatch");
System.IO.Directory.CreateDirectory(folder);
var previousBackground = Application.runInBackground;
Application.runInBackground = true; // Keep the sampled Play session advancing while CLI owns focus.
var keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("S2MeasurementKeyboard");
var hero = bootstrap.PlayerMover.transform;
composition.RunDriver.ResetRun();
var origin = hero.position;
var economy = composition.Gathering.Economy;
var run = composition.RunDriver.Run;
var map = bootstrap.MapDefinition;
var started = Time.timeAsDouble;
var stageStarted = started;
var runStarted = started;
var lastStatus = started - 10;
var stage = "travel";
var route = 0;
var source = 0;
var builds = 0;
var lastFrame = -1;
var skippedFrames = 0;
var lastPhase = run.Phase.ToString();
var memoryStart = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
var maxMemory = memoryStart;
var events = new List<object>();
var segments = new List<object>();
var frames = new List<object>();
var runs = new List<object>();
var sourceWorlds = new Vector2[3];
for (var i = 0; i < 3; i++) sourceWorlds[i] = economy.WorldPosition((Not3A.Stage2.Map.MapResourceKind)i);
var targets = new Vector2[] { sourceWorlds[0], origin, sourceWorlds[1], origin, sourceWorlds[2], origin };
var labels = new[] { "wood-out", "wood-back", "stone-out", "stone-back", "metal-out", "metal-back" };
var geometry = new List<object>();
for (var i = 0; i < 3; i++)
{
    var delta = sourceWorlds[i] - (Vector2)origin;
    var a = Math.Abs(delta.x); var b = Math.Abs(delta.y);
    geometry.Add(new { resource = ((Not3A.Stage2.Map.MapResourceKind)i).ToString(), anchor = economy.GetNode((Not3A.Stage2.Map.MapResourceKind)i).Cell.ToString(),
        worldDistance = delta.magnitude, eightDirectionSeconds = (Math.Max(a,b) + (Math.Sqrt(2)-1)*Math.Min(a,b))/bootstrap.PlayerMover.Speed });
}
Action<UnityEngine.InputSystem.Key[]> send = keys => UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys));
Action<Vector2> move = target =>
{
    var delta = target - (Vector2)hero.position;
    var keys = new List<UnityEngine.InputSystem.Key>();
    if (Math.Abs(delta.x) > 0.08f) keys.Add(delta.x > 0 ? UnityEngine.InputSystem.Key.D : UnityEngine.InputSystem.Key.A);
    if (Math.Abs(delta.y) > 0.08f) keys.Add(delta.y > 0 ? UnityEngine.InputSystem.Key.W : UnityEngine.InputSystem.Key.S);
    send(keys.ToArray());
};
Action<Not3A.Stage2.Map.MapResourceKind,int> gathered = (kind, amount) => events.Add(new { t = Time.timeAsDouble-started,
    stage, fact = "gather", resource = kind.ToString(), amount, phase = run.Phase.ToString() });
Action<Not3A.Stage2.Defense.Stage2Tower> built = tower => events.Add(new { t = Time.timeAsDouble-started,
    stage, fact = "build", cell = tower.Cell.ToString(), phase = run.Phase.ToString() });
economy.Gathered += gathered;
composition.Building.Defense.Built += built;
UnityEditor.EditorApplication.CallbackFunction update = null;
Action<string> finish = status =>
{
    EditorApplication.update -= update;
    economy.Gathered -= gathered;
    composition.Building.Defense.Built -= built;
    if (keyboard.added) UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
    Application.runInBackground = previousBackground;
    var report = new { status, utc = DateTime.UtcNow.ToString("O"), mode = "Editor Play, real WASD/E input replay, timeScale=1",
        unity = Application.unityVersion, cpu = SystemInfo.processorType, gpu = SystemInfo.graphicsDeviceName,
        ramMB = SystemInfo.systemMemorySize, os = SystemInfo.operatingSystem,
        mapHash = map.ComputeDeterministicContentHash(), heroSpeed = bootstrap.PlayerMover.Speed,
        origin = origin.ToString(), geometry, segments, events, frames, runs, skippedFrames,
        allocatedStartBytes = memoryStart, allocatedPeakBytes = maxMemory,
        allocatedEndBytes = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),
        reservedEndBytes = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong(),
        navigation = composition.Navigation.Diagnostics,
        limits = "Automated policy, not human observation. Build confirmations use the public API with fixed 1-second spacing. Frame intervals are sampled Editor frames, not Windows GPU performance; skipped frames are counted." };
    System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"live-measurements.json"), Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));
    System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"status.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new {status, stage}));
    if (Application.isPlaying && composition != null) composition.RunDriver.ResetRun();
    Debug.Log("S2_DECISION_MEASUREMENTS " + status);
};
update = () =>
{
    try
    {
        if (!Application.isPlaying || composition == null) { finish("aborted"); return; }
        if (Time.frameCount == lastFrame) return;
        if (lastFrame >= 0) skippedFrames += Math.Max(0, Time.frameCount-lastFrame-1);
        lastFrame = Time.frameCount;
        var now = Time.timeAsDouble;
        var elapsed = now-started;
        if (elapsed > 440) { finish("timeout"); return; }
        frames.Add(new { t=elapsed, stage, phase=run.Phase.ToString(), ms=Time.unscaledDeltaTime*1000f,
            enemies=composition.Enemies.ActiveCount });
        maxMemory = Math.Max(maxMemory,UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());
        if (now-lastStatus >= 5)
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"status.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new {status="running", stage,
                elapsed, route, source, builds, position=hero.position.ToString(), phase=run.Phase.ToString(), preparation=run.PreparationRemaining}));
            lastStatus=now;
        }
        var phase = run.Phase.ToString();
        if (phase != lastPhase)
        {
            events.Add(new { t=elapsed, stage, fact="phase", phase, throne=run.ThroneHealth });
            lastPhase=phase;
        }
        if (stage=="travel")
        {
            if (Vector2.Distance(hero.position,targets[route]) > 0.18f) { move(targets[route]); return; }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new { name=labels[route], seconds=now-stageStarted, end=hero.position.ToString() });
            route++; stageStarted=now;
            if (route==targets.Length)
            {
                composition.RunDriver.ResetRun();
                stage="circuit-travel"; source=0; runStarted=now;
            }
            return;
        }
        if (stage=="circuit-travel")
        {
            if (Vector2.Distance(hero.position,sourceWorlds[source]) > 0.18f) { move(sourceWorlds[source]); return; }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new {name="circuit-to-"+((Not3A.Stage2.Map.MapResourceKind)source), seconds=now-stageStarted});
            stage="gather"; stageStarted=now;
            return;
        }
        if (stage=="gather")
        {
            if (economy.Inventory.Get((Not3A.Stage2.Map.MapResourceKind)source)<3) { send(new[]{UnityEngine.InputSystem.Key.E}); return; }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new {name="gather-"+((Not3A.Stage2.Map.MapResourceKind)source), seconds=now-stageStarted});
            source++; stageStarted=now;
            stage=source<3 ? "circuit-travel" : "circuit-return";
            return;
        }
        if (stage=="circuit-return")
        {
            if (Vector2.Distance(hero.position,origin)>0.18f) { move(origin); return; }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new {name="circuit-return",seconds=now-stageStarted});
            stage="build"; stageStarted=now;
            return;
        }
        if (stage=="build")
        {
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            if (now-stageStarted<1) return;
            var defense=composition.Building.Defense;
            defense.BeginPlacement(); defense.SetPreviewCell(new Vector2Int(-35+builds*4,0));
            var result=defense.TryConfirm(out var tower);
            if (result!=Not3A.Stage2.Defense.PlacementResult.Ready) { finish("build-failed-"+result); return; }
            builds++; stageStarted=now;
            if (builds==3)
            {
                segments.Add(new {name="early-start-preparation-total",seconds=now-runStarted});
                send(new[]{UnityEngine.InputSystem.Key.Space}); stage="defended-wave";
            }
            return;
        }
        if (stage=="defended-wave")
        {
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            if (!run.IsTerminal) return;
            runs.Add(new {policy="gather-three-each-build-three-early-start",outcome=run.Phase.ToString(),
                seconds=now-runStarted, waveSeconds=now-stageStarted, throne=run.ThroneHealth,
                wood=economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Wood), stone=economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Stone),
                metal=economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Metal), towers=composition.Building.Defense.TowerCount});
            composition.RunDriver.ResetRun(); stage="timed-unopposed"; stageStarted=now; runStarted=now;
            return;
        }
        if (stage=="timed-unopposed")
        {
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            if (!run.IsTerminal) return;
            runs.Add(new {policy="no-actions-automatic-wave",outcome=run.Phase.ToString(),seconds=now-runStarted,
                throne=run.ThroneHealth,wood=economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Wood),
                stone=economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Stone),metal=economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Metal)});
            finish("completed");
        }
    }
    catch (Exception exception) { events.Add(new {fact="probe-exception",error=exception.ToString()}); finish("probe-error"); }
};
EditorApplication.update += update;
Debug.Log("S2_DECISION_MEASUREMENTS_STARTED");
