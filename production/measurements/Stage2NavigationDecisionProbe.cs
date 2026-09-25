// Unity CLI eval_file statements. Run after the live replay, in Editor Play.
// Uses isolated Navigation/Defense/AI instances; never changes the scene's shared services.
if (!UnityEngine.Application.isPlaying) throw new InvalidOperationException("Use Editor Play.");
var map = UnityEditor.AssetDatabase.LoadAssetAtPath<Not3A.Stage2.Map.Stage2MapDefinition>("Assets/Stage2/Map/Data/Stage2Map100x100.asset");
var towerData = UnityEditor.AssetDatabase.LoadAssetAtPath<Not3A.Stage2.Defense.Stage2DefenseCatalog>("Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset");
var enemyData = UnityEditor.AssetDatabase.LoadAssetAtPath<Not3A.Stage2.WavesAI.Stage2EnemyCatalog>("Assets/Stage2/WavesAI/Data/Stage2EnemyPrototype.asset");
var samples = new List<object>();
var roles = new List<object>();
for (var scenario = 0; scenario < 8; scenario++)
{
    for (var sample = -3; sample < 30; sample++)
    {
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var nav = new Not3A.Stage2.Navigation.Stage2Navigation(map);
        var count = 0;
        if (scenario == 1 || scenario == 2)
        {
            var radius = scenario == 1 ? 1 : 4;
            for (var y=-radius; y<=radius; y++) { nav.TryOccupy(new UnityEngine.Vector2Int(0,y),new object()); count++; }
        }
        if (scenario == 3 || scenario == 4)
        {
            var walls = scenario == 3 ? new[]{0} : new[]{25,0,-25};
            for (var w=0; w<walls.Length; w++)
                for (var y=map.MinimumCell.y; y<=map.MaximumCell.y; y++)
                {
                    if (scenario==4 && y==(w==1 ? map.MinimumCell.y : map.MaximumCell.y)) continue;
                    nav.TryOccupy(new UnityEngine.Vector2Int(walls[w],y),new object()); count++;
                }
        }
        if (scenario >= 5)
        {
            var center=scenario==7 ? map.EnemyEntrance : map.Throne;
            var offsets=scenario==7
                ? new[]{UnityEngine.Vector2Int.left,UnityEngine.Vector2Int.up,UnityEngine.Vector2Int.down}
                : scenario==5
                    ? new[]{UnityEngine.Vector2Int.right,UnityEngine.Vector2Int.up,UnityEngine.Vector2Int.down}
                    : new[]{UnityEngine.Vector2Int.right,UnityEngine.Vector2Int.up,UnityEngine.Vector2Int.down,UnityEngine.Vector2Int.left};
            foreach(var offset in offsets)
            {
                if(nav.TryOccupy(center+offset,new object())!=Not3A.Stage2.Navigation.OccupancyResult.Changed)
                    throw new InvalidOperationException("Invalid compact-layout cell.");
                count++;
            }
        }
        var plan=nav.PlanFrom(map.EnemyEntrance);
        var searchMs=nav.Diagnostics.LastSearchMilliseconds;
        var allocated=GC.GetAllocatedBytesForCurrentThread()-allocatedBefore;
        nav.TryOccupy(new UnityEngine.Vector2Int(-49,49),new object());
        var replanned=nav.ReplanIfStale(map.EnemyEntrance,plan);
        var replanMs=nav.Diagnostics.LastReplanMilliseconds;
        if (sample>=0) samples.Add(new { scenario=new[]{"open","three-tower-wall","nine-tower-wall","full-100-wall","zigzag-297","throne-three-sides","throne-ring-four","entrance-cap-three"}[scenario],
            sample, occupied=count, status=plan.Status.ToString(),steps=plan.Path.Count-1,
            searchMs,replanMs,managedAllocatedBytes=allocated>0 ? (long?)allocated : null,
            allocationCounterAvailable=allocated>0,blocker=plan.BlockerCell?.ToString() });
    }
}
for (var variant=0; variant<2; variant++)
{
    var data=UnityEngine.Object.Instantiate(enemyData);
    data.Configure(enemyData.EnemyPrefab,enemyData.MoveSpeed,enemyData.Health,
        enemyData.SiegeInterval,enemyData.SiegeDamage*(variant+1),enemyData.ThroneDamage);
    var nav=new Not3A.Stage2.Navigation.Stage2Navigation(map);
    var inventory=new Not3A.Stage2.Economy.Stage2Inventory();
    inventory.TryAdd(Not3A.Stage2.Map.MapResourceKind.Wood,towerData.WoodCost*3);
    inventory.TryAdd(Not3A.Stage2.Map.MapResourceKind.Stone,towerData.StoneCost*3);
    inventory.TryAdd(Not3A.Stage2.Map.MapResourceKind.Metal,towerData.MetalCost*3);
    var defense=new Not3A.Stage2.Defense.Stage2Defense(map,nav,inventory,towerData,
        ()=>map.CellToWorld(map.Throne),()=>UnityEngine.Object.Instantiate(towerData.TowerPrefab));
    defense.SetRunState(Not3A.Stage2.Defense.Stage2BuildPhase.Preparation,false);
    foreach(var offset in new[]{UnityEngine.Vector2Int.left,UnityEngine.Vector2Int.up,UnityEngine.Vector2Int.down})
    {
        defense.BeginPlacement(); defense.SetPreviewCell(map.EnemyEntrance+offset);
        if(defense.TryConfirm(out _)!=Not3A.Stage2.Defense.PlacementResult.Ready)
            throw new InvalidOperationException("Could not prepare paid entrance cap.");
    }
    defense.SetAttackActive(false); // Isolate siege from offensive tower damage.
    var beforeStatus=nav.PlanFrom(map.EnemyEntrance).Status.ToString();
    var group=new Not3A.Stage2.WavesAI.Stage2EnemyGroup(map,nav,data);
    var enemy=group.SpawnAt(map.EnemyEntrance);
    enemy.SetAutomaticTick(false);
    var steps=0;
    while(defense.TowerCount==3 && steps<1000) { enemy.Tick(0.05f); steps++; }
    roles.Add(new {variant=variant==0 ? "baseline" : "temporary-double-siege-sensitivity",
        damage=data.SiegeDamage,simulatedSeconds=steps*0.05f,destroyed=defense.TowerCount<3,
        routeBefore=beforeStatus,routeAfter=nav.PlanFrom(map.EnemyEntrance).Status.ToString(),
        paidTowers=3,remainingWood=inventory.Get(Not3A.Stage2.Map.MapResourceKind.Wood),
        remainingStone=inventory.Get(Not3A.Stage2.Map.MapResourceKind.Stone),
        remainingMetal=inventory.Get(Not3A.Stage2.Map.MapResourceKind.Metal),
        limitation="fixed-step paid three-tower entrance cap; tower fire suppressed to isolate siege; not a human balance test or approved enemy tuning"});
    group.ResetRun(); defense.ResetRun(); UnityEngine.Object.Destroy(data);
}
var report=new {utc=DateTime.UtcNow.ToString("O"),mode="Editor Play, isolated synchronous diagnostics",
    mapHash=map.ComputeDeterministicContentHash(),cpu=UnityEngine.SystemInfo.processorType,
    gpu=UnityEngine.SystemInfo.graphicsDeviceName,ramMB=UnityEngine.SystemInfo.systemMemorySize,
    warmups=3,samplesPerScenario=30,samples,roles};
System.IO.Directory.CreateDirectory("Artifacts/S2-DecisionBatch");
System.IO.File.WriteAllText("Artifacts/S2-DecisionBatch/navigation-role-measurements.json",
    Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
return new {samples=samples.Count,roles};
