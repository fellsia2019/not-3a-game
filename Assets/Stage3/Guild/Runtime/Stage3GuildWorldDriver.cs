using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Two typed exploration spots. Lifetime belongs to the current world, never a failed portal wave.</summary>
    [DisallowMultipleComponent,DefaultExecutionOrder(110)]
    public sealed partial class Stage3GuildWorldDriver : MonoBehaviour
    {
        private sealed class Actor
        { public Stage2GuildEnemy Rules;public Stage3GuildEnemyVisual Visual;public float Corpse; }
        private sealed class Shot
        { public Stage2GuildEnemy Target;public int Damage;public float Remaining;public Vector2 Origin; }
        private readonly List<Actor> actors=new List<Actor>();
        private readonly List<Shot> shots=new List<Shot>();
        private readonly Vector2[] spots=new Vector2[2];
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage3ResourceDistrictWorld world;
        private Stage3WorldTravel travel;
        private Stage2HeroProgressionDriver hero;
        private Stage2HeroCombatDriver combat;
        private Stage2HeroRecoveryDriver recovery;
        private Stage3HeroAvatar avatar;
        private HeroObstacleMap ground;
        private Stage2Guild guild;
        private int generation,nextId;
        private bool resetPending;
        public bool Ready {get;private set;}
        public bool AutomaticTick {get;set;}=true;
        public bool CombatActive
        {get{if(shots.Count>0)return true;foreach(var a in actors)if(a.Rules.Engaged)return true;return false;}}
        public IReadOnlyList<Stage2GuildEnemy> Enemies=>enemyView;
        private readonly List<Stage2GuildEnemy> enemyView=new List<Stage2GuildEnemy>();
        public Vector2 SpotPosition(int kind)=>spots[Mathf.Clamp(kind,0,1)];
        public event Action<Vector2,Vector2,float> ShotLaunched;
        public void Configure(Stage2GameComposition owner,Stage2SceneBootstrap context,Stage2Guild membership,
            Stage2HeroProgressionDriver progression,Stage2HeroRecoveryDriver damage)
        {
            game=owner;scene=context;guild=membership;hero=progression;recovery=damage;
            world=GetComponent<Stage3ResourceDistrictWorld>();travel=GetComponent<Stage3WorldTravel>();
            combat=GetComponent<Stage2HeroCombatDriver>();avatar=scene.PlayerMover.GetComponent<Stage3HeroAvatar>();
            if(combat!=null)combat.TargetingOverride=LaunchNearest;
            game.RunDriver.Run.Reset+=RequestReset;resetPending=true;
        }
        private void RequestReset(){guild.Reset();resetPending=true;}
        private bool Valid(Vector2 p,float radius)
        {
            var map=scene.MapDefinition;var cell=map.WorldToCell(p);
            return map.Contains(cell)&&map.IsWalkable(cell)&&map.GetSurface(cell)==MapSurfaceKind.Forest&&
                !(ground?.IsBlocked(p,radius)??true)&&!(travel?.CityBounds.Contains(p)??false);
        }
        private bool FindSites() => PrepareCampEnvironment(scene,world,travel);
        private void Update()
        {
            if(game==null)return;
            if(!Ready&&!FindSites())return;
            if(world.Obstacles!=null&&!ReferenceEquals(ground,world.Obstacles))ground=world.Obstacles;
            if(resetPending)
            {
                ClearActors();
                for(var kind=0;kind<2;kind++)
                    for(var i=0;i<guild.Catalog.Count;i++)
                    {var q=guild.Catalog.GetAt(i);if(!q.trial&&q.rank==Stage2GuildRank.F&&(int)q.enemyKind==kind){ResetSpot(q);break;}}
                resetPending=false;
                Debug.Log($"S3_GUILD_WORLD_READY orcs={spots[0]} ogres={spots[1]}");
            }
            if(AutomaticTick)Tick(Time.deltaTime);
        }
        public Stage2GuildSpawn ResetSpot(Stage2GuildQuest q)
        {
            if(!Ready||q==null||!q.IsValid)return null;
            var kind=(int)q.enemyKind;var positions=new List<Vector2>();
            void Consider(Vector2 p)
            {
                if(positions.Count>=q.population||!Valid(p,q.radius+.1f))return;
                foreach(var other in positions)if(Vector2.Distance(p,other)<q.radius*2+.35f)return;
                positions.Add(p);
            }
            // Existing population occupies the meadow, rather than one tiny spawn pile.
            foreach(var offset in CampSpawnOffsets)Consider(spots[kind]+offset);
            for(var ring=0;ring<10&&positions.Count<q.population;ring++)for(var d=0;d<16&&positions.Count<q.population;d++)
            {
                if(ring==0&&d>0)break;
                Consider(spots[kind]+new Vector2(Mathf.Cos(d*Mathf.PI/8),Mathf.Sin(d*Mathf.PI/8))*(ring*.85f));
            }
            if(positions.Count!=q.population)return null; // No mutation until every spawn is valid.
            for(var i=actors.Count-1;i>=0;i--)if(actors[i].Rules.Profile.enemyKind==q.enemyKind)
            {Destroy(actors[i].Visual.gameObject);enemyView.Remove(actors[i].Rules);actors.RemoveAt(i);}
            shots.RemoveAll(s=>s.Target.Profile.enemyKind==q.enemyKind);
            var ids=new int[q.population];var epoch=++generation;
            for(var i=0;i<positions.Count;i++)
            {
                ids[i]=++nextId;var rules=new Stage2GuildEnemy(ids[i],epoch,q,positions[i]);
                var root=new GameObject(q.enemyKind+" exploration "+ids[i]);root.transform.SetParent(transform,false);
                root.transform.position=positions[i];var visual=root.AddComponent<Stage3GuildEnemyVisual>();
                visual.Initialize(kind,scene.CameraFollow.BoundCamera);
                actors.Add(new Actor{Rules=rules,Visual=visual});enemyView.Add(rules);
            }
            return new Stage2GuildSpawn{Generation=epoch,EnemyIds=ids};
        }
        private static readonly Vector2[] CampSpawnOffsets={Vector2.zero,new Vector2(-6,3),new Vector2(6,-3),
            new Vector2(-5,-5),new Vector2(5,5),new Vector2(0,7)};
        private Vector2 Move(Vector2 position,Vector2 displacement,float radius)
        {
            var result=ground.Move(position,displacement,radius);
            // Small local steering around rocks/trunks; never teleport across an obstacle.
            if((result-position).sqrMagnitude<displacement.sqrMagnitude*.1f)
            {
                var best=Vector2.Dot(result-position,displacement.normalized);
                foreach(var angle in SteeringAngles)
                {
                    var a=angle*Mathf.Deg2Rad;var step=new Vector2(displacement.x*Mathf.Cos(a)-displacement.y*Mathf.Sin(a),displacement.x*Mathf.Sin(a)+displacement.y*Mathf.Cos(a));
                    var candidate=ground.Move(position,step,radius);var score=Vector2.Dot(candidate-position,displacement.normalized);
                    if(score>best){best=score;result=candidate;}
                }
            }
            var cell=scene.MapDefinition.WorldToCell(result);
            return scene.MapDefinition.Contains(cell)&&scene.MapDefinition.IsWalkable(cell)?result:position;
        }
        private static readonly float[] SteeringAngles={45,-45,80,-80};
        public void Tick(float elapsed)
        {
            if(!float.IsFinite(elapsed)||elapsed<0)throw new ArgumentOutOfRangeException(nameof(elapsed));
            if(!Ready||elapsed==0||game.RunDriver.Run.IsPaused||game.RunDriver.Run.IsTerminal||
                (game.GetComponent<Stage2GameHud>()?.DebugOpen??false))return;
            for(var i=shots.Count-1;i>=0;i--)
            {
                var shot=shots[i];shot.Remaining-=elapsed;if(shot.Remaining>0)continue;
                shots.RemoveAt(i);Damage(shot.Target,shot.Damage);
            }
            for(var i=actors.Count-1;i>=0;i--)
            {
                var a=actors[i];var unit=a.Rules;
                if(unit.Alive)unit.Tick(elapsed,scene.PlayerMover.transform.position,Move,amount=>
                {
                    if(Vector2.Distance(unit.Position,scene.PlayerMover.transform.position)<=unit.Profile.reach)
                        recovery.TryApplyExplorationDamage(amount);
                });
                else if((a.Corpse+=elapsed)>=.4f)
                {Destroy(a.Visual.gameObject);enemyView.Remove(unit);actors.RemoveAt(i);continue;}
                a.Visual.transform.position=unit.Position;a.Visual.SetState(unit.Velocity,unit.Facing,unit.Attacking,!unit.Alive);
            }
        }
        public bool LaunchNearest(Vector2 origin,float range,int damage,float splash,float flight)
        {
            var portal=game.Enemies.FindNearest(origin,range);
            var best=portal!=null?((Vector2)portal.transform.position-origin).sqrMagnitude:range*range;
            Stage2GuildEnemy selected=null;
            foreach(var a in actors)
            {
                var unit=a.Rules;if(!unit.Alive)continue;var d=(unit.Position-origin).sqrMagnitude;
                if(d>best||d==best&&(portal!=null||selected!=null))continue;selected=unit;best=d;
            }
            if(selected==null)return game.Enemies.TryHitNearest(origin,range,damage,splash,flight);
            avatar?.NotifyExplorationShot(origin,selected.Position);ShotLaunched?.Invoke(origin,selected.Position,flight);
            if(flight<=0)Damage(selected,damage);else shots.Add(new Shot{Target=selected,Damage=damage,Remaining=flight,Origin=origin});
            return true;
        }
        public bool Damage(Stage2GuildEnemy target,int amount)
        {
            if(target==null||!enemyView.Contains(target)||!target.TakeDamage(amount))return false;
            if(!target.Alive)
            {guild.RecordKill(target.Profile.enemyKind,target.Generation,target.Id);hero.Rules.AwardExperience(target.Profile.killExperience);}
            return true;
        }
        private void ClearActors()
        {
            foreach(var a in actors)if(a.Visual!=null)Destroy(a.Visual.gameObject);
            actors.Clear();enemyView.Clear();shots.Clear();
        }
        private void OnDestroy()
        {
            if(game?.RunDriver?.Run!=null)game.RunDriver.Run.Reset-=RequestReset;
            if(combat!=null)combat.TargetingOverride=null;ClearActors();ClearCampEnvironment();
        }
    }
}
