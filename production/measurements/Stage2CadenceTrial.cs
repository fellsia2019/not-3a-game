// Unity CLI eval_file statement body, clean Stage2VerticalSlice in Editor Play.
// Real WASD/E/Space at timeScale=1; public paid-build API; no asset/scene saves.
if (!Application.isPlaying || Math.Abs(Time.timeScale - 1f) > 0.001f)
    throw new InvalidOperationException("Enter Editor Play at timeScale=1.");
var composition = UnityEngine.Object.FindAnyObjectByType<Not3A.Stage2.Integration.Stage2GameComposition>();
var bootstrap = UnityEngine.Object.FindAnyObjectByType<Not3A.Stage2.Integration.Stage2SceneBootstrap>();
if (composition == null || !composition.IsInitialized) throw new InvalidOperationException("Missing Stage 2 composition.");
foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
    if (device.name == "S2CadenceKeyboard") throw new InvalidOperationException("A cadence probe is already running.");
var folder = System.IO.Path.GetFullPath("Artifacts/S2-Cadence");
System.IO.Directory.CreateDirectory(folder);
var previousBackground = Application.runInBackground;
Application.runInBackground = true;
var keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("S2CadenceKeyboard");
composition.RunDriver.ResetRun();
var hero = bootstrap.PlayerMover.transform;
var origin = hero.position;
var economy = composition.Gathering.Economy;
var defense = composition.Building.Defense;
var run = composition.RunDriver.Run;
var map = bootstrap.MapDefinition;
var started = Time.timeAsDouble;
var runStarted = started;
var stageStarted = started;
var lastStatus = started - 10;
var lastFrame = -1;
var skippedFrames = 0;
var policy = "early-start-with-paid-replacement";
var stage = "travel";
var source = 0;
var builds = 0;
var recovering = false;
var lastPhase = "";
var replacementComplete = false;
// Match the earlier one-wave defense near the throne for a comparable cadence.
var towerStartX = -35;
var towerStepX = 4;
var initialTower = (Not3A.Stage2.Defense.Stage2Tower)null;
var events = new List<object>();
var segments = new List<object>();
var runs = new List<object>();
var gatheredTotals = new int[3];
var buildCount = 0;
var sourceWorlds = new Vector2[3];
for (var i = 0; i < 3; i++) sourceWorlds[i] = economy.WorldPosition((Not3A.Stage2.Map.MapResourceKind)i);
Func<object> snapshot = () => new {
    t = Time.timeAsDouble - started, runSeconds = Time.timeAsDouble - runStarted,
    policy, stage, phase = run.Phase.ToString(), wave = run.WaveNumber, waves = run.WaveCount,
    remaining = run.PreparationRemaining, throne = run.ThroneHealth, towers = defense.TowerCount,
    hero = hero.position.ToString(),
    wood = economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Wood),
    stone = economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Stone),
    metal = economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Metal),
    gathered = (int[])gatheredTotals.Clone(), buildCount,
    nodes = new[] { economy.GetNode(Not3A.Stage2.Map.MapResourceKind.Wood),
        economy.GetNode(Not3A.Stage2.Map.MapResourceKind.Stone), economy.GetNode(Not3A.Stage2.Map.MapResourceKind.Metal) }
        .Select(n => new { kind = n.Resource.ToString(), n.Remaining, n.RecoveryRemaining }).ToArray()
};
Action<UnityEngine.InputSystem.Key[]> send = keys => UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys));
Action<Vector2> move = target => {
    var delta = target - (Vector2)hero.position;
    var keys = new List<UnityEngine.InputSystem.Key>();
    if (Math.Abs(delta.x) > 0.08f) keys.Add(delta.x > 0 ? UnityEngine.InputSystem.Key.D : UnityEngine.InputSystem.Key.A);
    if (Math.Abs(delta.y) > 0.08f) keys.Add(delta.y > 0 ? UnityEngine.InputSystem.Key.W : UnityEngine.InputSystem.Key.S);
    send(keys.ToArray());
};
Action<Not3A.Stage2.Map.MapResourceKind,int> gathered = (kind, amount) => {
    gatheredTotals[(int)kind] += amount;
    events.Add(new { fact = "gather", kind = kind.ToString(), amount, state = snapshot() });
};
Action<Not3A.Stage2.Defense.Stage2Tower> built = tower => {
    buildCount++;
    events.Add(new { fact = "paid-build", cell = tower.Cell.ToString(), state = snapshot() });
};
economy.Gathered += gathered;
defense.Built += built;
UnityEditor.EditorApplication.CallbackFunction update = null;
Action<string> finish = status => {
    UnityEditor.EditorApplication.update -= update;
    economy.Gathered -= gathered;
    defense.Built -= built;
    if (keyboard.added) UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
    Application.runInBackground = previousBackground;
    var report = new { status, utc = DateTime.UtcNow.ToString("O"),
        mode = "Editor Play WASD/E/Space replay, timeScale=1", unity = Application.unityVersion,
        cpu = SystemInfo.processorType, gpu = SystemInfo.graphicsDeviceName,
        mapHash = map.ComputeDeterministicContentHash(), heroSpeed = bootstrap.PlayerMover.Speed,
        replacementComplete, towerStartX, towerStepX, events, segments, runs, skippedFrames,
        limits = "Known-coordinate scripted policy, not a human playtest. Builds use public transactions at 1-second spacing. One tower is deliberately destroyed after wave 1 through TakeDamage to measure recovery; this is not natural siege evidence. No inventory injection, rewards, healing, source reset or time acceleration." };
    System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "live-cadence.json"), Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));
    System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "status.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new { status, state = snapshot() }));
    if (Application.isPlaying && composition != null) composition.RunDriver.ResetRun();
    Debug.Log("S2_CADENCE_TRIAL " + status);
};
update = () => {
    try {
        if (!Application.isPlaying || composition == null) { finish("aborted"); return; }
        if (Time.frameCount == lastFrame) return;
        if (lastFrame >= 0) skippedFrames += Math.Max(0, Time.frameCount - lastFrame - 1);
        lastFrame = Time.frameCount;
        var now = Time.timeAsDouble;
        if (now - started > 420) { finish("timeout"); return; }
        if (now - lastStatus > 5) {
            System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "status.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new { status = "running", state = snapshot() }));
            lastStatus = now;
        }
        var phase = run.Phase + "-" + run.WaveNumber;
        if (phase != lastPhase) {
            events.Add(new { fact = "phase", state = snapshot() });
            lastPhase = phase;
        }
        if (run.IsTerminal) {
            runs.Add(snapshot());
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            if (policy == "unopposed-timed-start") {
                if (run.Phase != Not3A.Stage2.RunState.Stage2RunPhase.Lost) { finish("unexpected-unopposed-outcome"); return; }
                finish("completed"); return;
            }
            if (run.Phase != Not3A.Stage2.RunState.Stage2RunPhase.Won || !replacementComplete) {
                finish("defended-policy-failed"); return;
            }
            composition.RunDriver.ResetRun();
            events.Add(new { fact = "restart", state = snapshot() });
            if (run.WaveNumber != 1 || defense.TowerCount != 0 || composition.Enemies.ActiveCount != 0 ||
                economy.Inventory.Get(Not3A.Stage2.Map.MapResourceKind.Wood) != 0 ||
                run.ThroneHealth != run.ThroneMaximum) { finish("restart-failed"); return; }
            policy = "unopposed-timed-start"; stage = "wait"; runStarted = now;
            gatheredTotals = new int[3]; buildCount = 0; lastPhase = "";
            return;
        }
        if (policy == "unopposed-timed-start") return;
        if (stage == "travel") {
            if (Vector2.Distance(hero.position, sourceWorlds[source]) > 0.18f) { move(sourceWorlds[source]); return; }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new { phase = recovering ? "recovery" : "initial", action = "travel-to-source", source, seconds = now - stageStarted });
            stage = "gather"; stageStarted = now; return;
        }
        if (stage == "gather") {
            if (economy.Inventory.Get((Not3A.Stage2.Map.MapResourceKind)source) < (recovering ? 1 : 3)) {
                send(new[] { UnityEngine.InputSystem.Key.E }); return;
            }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new { phase = recovering ? "recovery" : "initial", action = "gather", source, seconds = now - stageStarted });
            source++; stage = source < 3 ? "travel" : "return"; stageStarted = now; return;
        }
        if (stage == "return") {
            if (Vector2.Distance(hero.position, origin) > 0.18f) { move(origin); return; }
            send(Array.Empty<UnityEngine.InputSystem.Key>());
            segments.Add(new { phase = recovering ? "recovery" : "initial", action = "return", seconds = now - stageStarted });
            stage = "build"; stageStarted = now; builds = 0; return;
        }
        if (stage == "build") {
            if (now - stageStarted < 1) return;
            defense.BeginPlacement();
            defense.SetPreviewCell(new Vector2Int(towerStartX + builds * towerStepX, 0));
            if (defense.TryConfirm(out var tower) != Not3A.Stage2.Defense.PlacementResult.Ready)
                throw new InvalidOperationException("Paid tower placement failed.");
            if (!recovering && builds == 0) initialTower = tower;
            builds++; stageStarted = now;
            if (builds < (recovering ? 1 : 3)) return;
            if (recovering) {
                replacementComplete = true;
                events.Add(new { fact = "replacement-complete", state = snapshot() });
            }
            stage = "start"; return;
        }
        if (stage == "start") {
            send(new[] { UnityEngine.InputSystem.Key.Space }); stage = "release"; return;
        }
        if (stage == "release") { send(Array.Empty<UnityEngine.InputSystem.Key>()); stage = "wave"; return; }
        if (stage == "wave" && run.Phase == Not3A.Stage2.RunState.Stage2RunPhase.Preparation) {
            if (run.WaveNumber == 2 && !recovering) {
                if (initialTower == null || !initialTower.IsAlive) throw new InvalidOperationException("Expected first paid tower to survive wave 1.");
                initialTower.TakeDamage(initialTower.Health);
                events.Add(new { fact = "controlled-paid-tower-loss", state = snapshot() });
                recovering = true; source = 0; stage = "travel"; stageStarted = now; return;
            }
            stage = "start";
        }
    } catch (Exception error) {
        events.Add(new { fact = "exception", message = error.ToString() });
        finish("error");
    }
};
UnityEditor.EditorApplication.update += update;
Debug.Log("S2_CADENCE_TRIAL started");
