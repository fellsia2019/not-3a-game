// Unity CLI eval_file, after Stage2DecisionMeasurement reports completed.
if (!UnityEngine.Application.isPlaying) throw new InvalidOperationException("Use Editor Play.");
var c=UnityEngine.Object.FindAnyObjectByType<Not3A.Stage2.Integration.Stage2GameComposition>();
if(c==null || !c.IsInitialized) throw new InvalidOperationException("Missing live composition.");
var previousBackground=UnityEngine.Application.runInBackground;
UnityEngine.Application.runInBackground=true;
c.RunDriver.ResetRun();
var searchesBefore=c.Navigation.Diagnostics.FullSearchCount;
var requestsBefore=c.Navigation.Diagnostics.RequestCount;
var cacheHitsBefore=c.Navigation.Diagnostics.PlanCacheHitCount;
var start=UnityEngine.Time.timeAsDouble;
var lastFrame=-1;
var skipped=0;
var state=0;
var revisionTime=20d;
var token=new object();
var occupied=false;
var frames=new List<object>();
var memory=new List<object>();
var lastMemory=-1d;
var names=new[]{"idle","64-moving-enemies","64-moving-enemies-replan-1hz"};
UnityEditor.EditorApplication.CallbackFunction tick=null;
Action<string> finish=status=>
{
    UnityEditor.EditorApplication.update-=tick;
    var result=new {status,utc=DateTime.UtcNow.ToString("O"),
        mode="Editor Play sampled frames at timeScale=1; not Windows Player GPU profiling",
        cpu=UnityEngine.SystemInfo.processorType,gpu=UnityEngine.SystemInfo.graphicsDeviceName,
        frames,memory,skipped,navigation=c.Navigation.Diagnostics,
        fullSearchDelta=c.Navigation.Diagnostics.FullSearchCount-searchesBefore,
        requestDelta=c.Navigation.Diagnostics.RequestCount-requestsBefore,
        retainedPlanDelta=c.Navigation.Diagnostics.PlanCacheHitCount-cacheHitsBefore};
    System.IO.Directory.CreateDirectory("Artifacts/S2-DecisionBatch");
    System.IO.File.WriteAllText("Artifacts/S2-DecisionBatch/live-load.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
    if(UnityEngine.Application.isPlaying) c.RunDriver.ResetRun();
    UnityEngine.Application.runInBackground=previousBackground;
    UnityEngine.Debug.Log("S2_LIVE_LOAD_PROBE "+status);
};
tick=()=>
{
    if(!UnityEngine.Application.isPlaying || c==null) { finish("aborted");return; }
    if(UnityEngine.Time.frameCount==lastFrame)return;
    if(lastFrame>=0)skipped+=Math.Max(0,UnityEngine.Time.frameCount-lastFrame-1);
    lastFrame=UnityEngine.Time.frameCount;
    var t=UnityEngine.Time.timeAsDouble-start;
    if(t>=30){finish("completed");return;}
    if(t>=10 && state==0){for(var i=0;i<64;i++)c.Enemies.Spawn();state=1;}
    if(t>=20)state=2;
    if(state==2 && t>=revisionTime)
    {
        if(occupied)c.Navigation.TryRelease(new UnityEngine.Vector2Int(-49,49),token);
        else c.Navigation.TryOccupy(new UnityEngine.Vector2Int(-49,49),token);
        occupied=!occupied;revisionTime+=1;
    }
    frames.Add(new {seconds=t,scenario=names[state],ms=UnityEngine.Time.unscaledDeltaTime*1000f,enemies=c.Enemies.ActiveCount});
    if(t-lastMemory>=1)
    {
        memory.Add(new {seconds=t,scenario=names[state],allocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),
            reservedBytes=UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong()});
        lastMemory=t;
    }
};
UnityEditor.EditorApplication.update+=tick;
UnityEngine.Debug.Log("S2_LIVE_LOAD_PROBE_STARTED");
