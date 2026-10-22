using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.WavesAI;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Explicit disposable development capture; never runs without its environment opt-in.</summary>
    public static class Stage3FieldFeedbackReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>();
            var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();
            var field=owner.GetComponent<Stage3FieldFeedbackPresentation>();
            Stage3GuildWorldDriver guild=null;
            var deadline=Time.realtimeSinceStartup+30;
            while(Time.realtimeSinceStartup<deadline)
            {
                guild=owner.GetComponent<Stage3GuildWorldDriver>();
                if(guild!=null&&guild.Ready&&hud!=null&&hud.IsReady&&guild.Enemies.Count>0)break;
                yield return null;
            }
            if(guild==null||!guild.Ready)throw new InvalidOperationException("Field review world not ready.");
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;guild.AutomaticTick=false;
            var hero=owner.GetComponent<Stage2HeroProgressionDriver>();if(hero!=null)hero.AutomaticTick=false;
            var frames=new List<string>();var facts=new List<string>();
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
                hud.CloseWindow();game.SetPaused(false);game.Enemies.ResetRun();field.ClearFeedback();
                // The entrance itself sits near the map/camera edge; stage the real portal
                // actors a short way down the road so the overhead UI can be reviewed.
                var center=scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)+Vector2.down*6;
                scene.PlayerMover.transform.position=center+Vector2.right*2.5f;
                scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
                var portal=new List<Stage2Enemy>();
                for(var i=0;i<8;i++)
                {
                    var enemy=game.Enemies.Spawn(Stage2EnemySource.Night,1,4,1);enemy.SetAutomaticTick(false);
                    enemy.transform.position=center+new Vector2((i%4-1.5f)*1.2f,(i/4)*1.4f);portal.Add(enemy);
                }
                yield return null;yield return null;
                foreach(var enemy in portal)enemy.TakeDamage(1);
                game.SetPaused(true);
                yield return Shot("portal",size.x,size.y);
                if(size.x==1600)
                {
                    scene.CameraFollow.ConfigureTacticalView(15);scene.CameraFollow.SnapToTarget();
                    yield return Shot("portal-wide",size.x,size.y);
                }
                game.SetPaused(false);game.Enemies.ResetRun();
                for(var kind=0;kind<2;kind++)
                {
                    field.ClearFeedback();
                    var enemies=guild.Enemies.Where(e=>e.Alive&&(int)e.Profile.enemyKind==kind).Take(3).ToArray();
                    if(enemies.Length==0)throw new InvalidOperationException("Missing live camp actors for field review.");
                    scene.PlayerMover.transform.position=enemies[0].Position+Vector2.right*2.5f;
                    scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
                    yield return null;yield return null;
                    foreach(var enemy in enemies)guild.Damage(enemy,1);
                    game.SetPaused(true);yield return Shot(kind==0?"orc":"ogre",size.x,size.y);game.SetPaused(false);
                }
                foreach(var kind in new[]{MapResourceKind.Wood,MapResourceKind.Stone,MapResourceKind.Metal})
                {
                    field.ClearFeedback();
                    // Mineral nodes need not belong to a tree group. Use a real, unobstructed
                    // contact and favour ordinary isolated resources for a readable UI frame.
                    var candidates=game.Gathering.Economy.Nodes.Where(n=>n.Resource==kind&&!n.IsDepleted)
                        .OrderBy(n=>n.Size).ToArray();
                    Not3A.Stage2.Economy.Stage2ResourceNode node=null;var p=Vector2.zero;
                    foreach(var candidate in candidates)
                    {
                        foreach(var direction in new[]{Vector2.right,Vector2.left,Vector2.down})
                        {
                            var point=candidate.WorldPosition+direction*(candidate.InteractionRadius+.3f);
                            if(!game.Gathering.Economy.IsInRange(candidate,point)||hero!=null&&!hero.CanLand(point))continue;
                            var crowded=candidates.Any(other=>!ReferenceEquals(other,candidate)&&
                                Vector2.Distance(other.WorldPosition,candidate.WorldPosition)<2.5f);
                            if(crowded)continue;
                            node=candidate;p=point;break;
                        }
                        if(node!=null)break;
                    }
                    if(node==null)
                    {
                        node=candidates.First();p=node.WorldPosition+Vector2.right*(node.InteractionRadius+.3f);
                    }
                    scene.PlayerMover.transform.position=p;
                    scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
                    yield return null;yield return null;
                    if(!game.Gathering.Economy.AdvanceGather(node,p,true,game.Gathering.Economy.GatherDuration(node),true))
                        throw new InvalidOperationException("Native field review did not grant "+kind);
                    game.SetPaused(true);yield return Shot(kind.ToString().ToLowerInvariant(),size.x,size.y);game.SetPaused(false);
                }
            }
            // Red-portal combat is primarily a night activity: qualify the actual night
            // lighting via the existing explicit test-wave command, never a fake tint.
            Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
            game.SetPaused(false);game.RunDriver.ResetRun();yield return null;yield return null;
            if(!game.RunDriver.Run.StartNextWaveForTesting())throw new InvalidOperationException("Night field review could not start test wave.");
            game.Enemies.ResetRun();
            var nightCenter=scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)+Vector2.down*6;
            scene.PlayerMover.transform.position=nightCenter+Vector2.right*2.5f;
            scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
            var nightEnemies=new List<Stage2Enemy>();
            for(var i=0;i<8;i++)
            {
                var enemy=game.Enemies.Spawn(Stage2EnemySource.Night,1,4,1);enemy.SetAutomaticTick(false);
                enemy.transform.position=nightCenter+new Vector2((i%4-1.5f)*1.2f,(i/4)*1.4f);nightEnemies.Add(enemy);
            }
            yield return new WaitForSecondsRealtime(.5f);
            foreach(var enemy in nightEnemies)enemy.TakeDamage(1);
            game.SetPaused(true);yield return Shot("portal-night",1600,900);
            scene.CameraFollow.ConfigureTacticalView(15);scene.CameraFollow.SnapToTarget();
            yield return Shot("portal-wide-night",1600,900);
            game.SetPaused(false);game.Enemies.ResetRun();field.ClearFeedback();
            var nightOgre=guild.Enemies.First(e=>e.Alive&&(int)e.Profile.enemyKind==1);
            scene.PlayerMover.transform.position=nightOgre.Position+Vector2.right*2.5f;
            scene.CameraFollow.ConfigureTacticalView(5);scene.CameraFollow.SnapToTarget();
            yield return null;yield return null;
            guild.Damage(nightOgre,1);game.SetPaused(true);yield return Shot("ogre-night",1600,900);
            File.WriteAllText(Path.Combine(directory,"frames.txt"),string.Join("\n",frames));
            File.WriteAllText(Path.Combine(directory,"facts.txt"),string.Join("\n",facts));
            File.WriteAllText(Path.Combine(directory,"qualification.txt"),
                "Explicit disposable native QA fixture. Real damage and manual grant APIs; clock/AI held, hero and portal actor locations arranged only for visual review. Real enemy HP and actual amount, no mana source. Captures freeze effects after actual events. Restart ordinary process to remove fixture state.\n");
            Debug.Log("S3_FIELD_FEEDBACK_NATIVE_READY "+directory);
            IEnumerator Shot(string label,int width,int height)
            {
                var name=width+"x"+height+"-"+label;
                yield return null;yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));frames.Add(name+".png");
                facts.Add(name+" hpBars="+field.VisibleHealthBars+" mpBars="+field.VisibleManaBars+" floating="+field.ActiveFloatingCount+
                    " actual="+field.LastActualAmount+" gather="+field.LastWasGather+" hpRect="+field.LastHealthBarRect+" numberRect="+field.LastFloatingRect);
                yield return null;
            }
        }
    }
}
