#if UNITY_EDITOR
using System;
using System.Linq;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3GuildRulesTests
    {
        private Stage2Guild guild;private Stage2GuildCatalog data;private Stage2Inventory wallet;
        private Stage2HeroProgression hero;private int generation,id,resets;
        [SetUp]public void Setup()
        {
            data=ScriptableObject.CreateInstance<Stage2GuildCatalog>();wallet=new Stage2Inventory();
            hero=new Stage2HeroProgression(Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression"));
            guild=new Stage2Guild(data,wallet,hero);generation=id=resets=0;
        }
        [TearDown]public void Cleanup()=>Object.DestroyImmediate(data);
        private Stage2GuildSpawn Spawn(Stage2GuildQuest q)
        {resets++;return new Stage2GuildSpawn{Generation=++generation,EnemyIds=Enumerable.Range(id+=100,q.population).ToArray()};}
        private Stage2GuildContract Accept(int quest)
        {Assert.That(guild.TryAccept(quest,Spawn),Is.True);return guild.FindActive(quest);}
        private void Clear(Stage2GuildContract c)
        {for(var i=0;i<c.Quest.population;i++)Assert.That(guild.RecordKill(c.Quest.enemyKind,c.Generation,id+i),Is.True);}
        private void Complete(int quest){var c=Accept(quest);Clear(c);Assert.That(guild.TryTurnIn(quest),Is.True);}
        [Test]public void ClearOnlyMarksReadyAndRewardRequiresOneTurnIn()
        {
            var c=Accept(1);var gold=wallet.Gold;var xp=hero.XpWithin;
            Assert.That(guild.TryTurnIn(1),Is.False);Clear(c);
            Assert.That(wallet.Gold,Is.EqualTo(gold));Assert.That(hero.XpWithin,Is.EqualTo(xp));Assert.That(guild.CompletedForRank,Is.Zero);
            Assert.That(guild.Evaluate(1),Is.EqualTo(Stage2GuildAction.TurnIn));Assert.That(guild.TryTurnIn(1),Is.True);
            Assert.That(wallet.Gold,Is.EqualTo(gold+c.Quest.gold));Assert.That(guild.CompletedForRank,Is.EqualTo(1));
            Assert.That(guild.TryTurnIn(1),Is.False);Assert.That(resets,Is.EqualTo(1),"Turn-in must not refill a camp.");
        }
        [Test]public void RepeatsCountButThirdTurnInOnlyUnlocksTrial()
        {
            Complete(1);Complete(1);Complete(1);
            Assert.That(guild.Rank,Is.EqualTo(Stage2GuildRank.F));Assert.That(guild.CompletedForRank,Is.EqualTo(3));
            Assert.That(guild.Evaluate(11),Is.EqualTo(Stage2GuildAction.Locked));Assert.That(guild.Evaluate(13),Is.EqualTo(Stage2GuildAction.Accept));
            var trial=Accept(13);Clear(trial);Assert.That(guild.Rank,Is.EqualTo(Stage2GuildRank.F));
            Assert.That(guild.TryTurnIn(13),Is.True);Assert.That(guild.Rank,Is.EqualTo(Stage2GuildRank.E));
            Assert.That(guild.CompletedForRank,Is.Zero);Assert.That(guild.BoardAvailable(Stage2GuildRank.E),Is.True);
        }
        [Test]public void CompleteProgressionReachesSWithNoExtraTrialAndOldRanksNeverAdvanceNewRank()
        {
            for(var rank=0;rank<6;rank++)
            {
                for(var n=0;n<3;n++)Complete(rank*10+1);
                Complete((rank+1)*10+3);Assert.That((int)guild.Rank,Is.EqualTo(rank+1));
                Complete(1);Assert.That(guild.CompletedForRank,Is.Zero);
            }
            for(var n=0;n<3;n++)Complete(61);
            Assert.That(guild.Rank,Is.EqualTo(Stage2GuildRank.S));Assert.That(guild.Evaluate(73),Is.EqualTo(Stage2GuildAction.Invalid));
        }
        [Test]public void GenerationsAndTypedSpotsRejectDuplicateStaleAndUnrelatedKills()
        {
            var c=Accept(1);var first=id;
            Assert.That(guild.RecordKill(Stage2GuildEnemyKind.Ogre,c.Generation,first),Is.False);
            Assert.That(guild.RecordKill(Stage2GuildEnemyKind.Orc,c.Generation-1,first),Is.False);
            Assert.That(guild.RecordKill(Stage2GuildEnemyKind.Orc,c.Generation,first),Is.True);
            Assert.That(guild.RecordKill(Stage2GuildEnemyKind.Orc,c.Generation,first),Is.False);
            Assert.That(guild.TryAccept(1,Spawn),Is.False);var other=Accept(2);Assert.That(guild.Active.Count,Is.EqualTo(2));
            Clear(other);Assert.That(c.Remaining,Is.EqualTo(c.Quest.population-1));
            for(var i=1;i<c.Quest.population;i++)guild.RecordKill(c.Quest.enemyKind,c.Generation,first+i);
            guild.TryTurnIn(1);var repeat=Accept(1);
            Assert.That(guild.RecordKill(Stage2GuildEnemyKind.Orc,c.Generation,first),Is.False);Assert.That(repeat.Remaining,Is.EqualTo(repeat.Quest.population));
        }
        [Test]public void FailedAcceptanceOverflowAndReentrantCallbacksCannotAwardOrRefillTwice()
        {
            Assert.That(guild.TryAccept(1,_=>null),Is.False);Assert.That(guild.Active,Is.Empty);
            var c=Accept(1);Clear(c);wallet.TryAddGold(int.MaxValue);
            Assert.That(guild.TryTurnIn(1),Is.False);Assert.That(guild.FindActive(1),Is.SameAs(c));
            wallet.Reset();var nested=false;
            wallet.Changed+=()=>{nested|=guild.TryTurnIn(1);nested|=guild.TryAccept(1,Spawn);nested|=guild.Reset();};
            Assert.That(guild.TryTurnIn(1),Is.True);Assert.That(nested,Is.False);Assert.That(resets,Is.EqualTo(1));
        }
        [Test]public void FullResetClearsMembershipContractsAndCounters()
        {
            for(var n=0;n<3;n++)Complete(1);Complete(13);Accept(11);
            guild.Reset();Assert.That(guild.Rank,Is.EqualTo(Stage2GuildRank.F));Assert.That(guild.Active,Is.Empty);
            Assert.That(guild.CompletedForRank,Is.Zero);Assert.That(guild.Evaluate(13),Is.EqualTo(Stage2GuildAction.Locked));
        }
        [Test]public void MeleeWindupAllowsEscapeAndLeashReturnsWithoutHealing()
        {
            var q=data.Find(1);var unit=new Stage2GuildEnemy(1,1,q,Vector2.zero);var hits=0;
            unit.Tick(.01f,Vector2.right*.5f,null,d=>hits++);Assert.That(unit.Attacking,Is.True);
            unit.Tick(q.attackWindup,Vector2.right*3,null,d=>hits++);Assert.That(hits,Is.Zero);
            unit.TakeDamage(1);unit.Tick(1,Vector2.right*5,null,d=>hits++);Assert.That(unit.Position.x,Is.GreaterThan(0));
            for(var n=0;n<100;n++)unit.Tick(.1f,Vector2.right*50,null,d=>hits++);
            Assert.That(Vector2.Distance(unit.Position,unit.Home),Is.LessThan(.11f));Assert.That(unit.Health,Is.EqualTo(q.health-1));
        }
        [Test]public void MeleeUsesFreshHeroPositionAfterEarlierFatalStrike()
        {
            var q=data.Find(1);var first=new Stage2GuildEnemy(1,1,q,Vector2.zero);var second=new Stage2GuildEnemy(2,1,q,Vector2.zero);
            var heroPosition=Vector2.right*.5f;var hits=0;
            Action<int> hit=d=>{hits++;heroPosition=Vector2.right*100;};
            first.Tick(.01f,heroPosition,null,hit);second.Tick(.01f,heroPosition,null,hit);
            first.Tick(q.attackWindup,heroPosition,null,hit);second.Tick(q.attackWindup,heroPosition,null,hit);
            Assert.That(hits,Is.EqualTo(1));Assert.That(second.Attacking,Is.False);
        }
        [Test]public void ExplorationMovementCannotCrossGatheringFootprintsEvenAtLargeSteps()
        {
            var q=data.Find(1);var unit=new Stage2GuildEnemy(1,1,q,Vector2.zero);
            var ground=new HeroObstacleMap();ground.AddRectangle(new Rect(1,-2,1,4));
            unit.Tick(3,Vector2.right*5,ground.Move,_=>Assert.Fail("Melee across blocked ground"));
            Assert.That(unit.Position.x,Is.LessThan(1-q.radius));Assert.That(ground.IsBlocked(unit.Position,q.radius),Is.False);
        }
        [Test]public void RangedHitAlertsAnActorOutsideItsIdleAggroRadiusWithoutHealingIt()
        {
            var q=data.Find(1);var unit=new Stage2GuildEnemy(1,1,q,Vector2.zero);
            var target=Vector2.right*(q.aggroRadius+1);unit.Tick(.1f,target,null,null);
            Assert.That(unit.Engaged,Is.False);unit.TakeDamage(1);unit.Tick(.1f,target,null,null);
            Assert.That(unit.Engaged,Is.True);Assert.That(unit.Position.x,Is.GreaterThan(0));Assert.That(unit.Health,Is.EqualTo(q.health-1));
        }
    }
}
#endif
