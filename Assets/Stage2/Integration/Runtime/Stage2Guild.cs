using System;
using System.Collections.Generic;
using Not3A.Stage2.Economy;

namespace Not3A.Stage2.Integration
{
    public enum Stage2GuildAction { Accept, InProgress, TurnIn, Locked, SpotOccupied, Invalid, GoldLimit, Busy }
    public sealed class Stage2GuildSpawn
    {
        public int Generation;
        public int[] EnemyIds;
    }
    public sealed class Stage2GuildContract
    {
        private readonly HashSet<int> remaining;
        public Stage2GuildQuest Quest { get; }
        public int Generation { get; }
        public int Remaining => remaining.Count;
        public bool Cleared => Remaining == 0;
        internal Stage2GuildContract(Stage2GuildQuest q, Stage2GuildSpawn spawn)
        { Quest=q; Generation=spawn.Generation; remaining=new HashSet<int>(spawn.EnemyIds); }
        internal bool Record(int enemyId) => remaining.Remove(enemyId);
    }

    /// <summary>World-owned guild membership and repeat orders. Killing never grants the contract reward.</summary>
    public sealed class Stage2Guild
    {
        private readonly Stage2Inventory wallet;
        private readonly Stage2HeroProgression hero;
        private readonly List<Stage2GuildContract> active = new List<Stage2GuildContract>();
        private readonly int[] successes = new int[7];
        private bool busy;
        private float experienceBonus,goldBonus;
        public void SetRewardBonuses(float experience,float gold)
        {experience=Math.Clamp(experience,0,.3f);gold=Math.Clamp(gold,0,.3f);if(experienceBonus==experience&&goldBonus==gold)return;experienceBonus=experience;goldBonus=gold;Changed?.Invoke();}
        public int RewardGold(Stage2GuildQuest quest)=>(int)Math.Min(int.MaxValue,Math.Floor(quest.gold*(1d+goldBonus)));
        public int RewardExperience(Stage2GuildQuest quest)=>(int)Math.Min(int.MaxValue,Math.Floor(quest.experience*(1d+experienceBonus)));
        public Stage2GuildCatalog Catalog { get; }
        public Stage2GuildRank Rank { get; private set; }
        public IReadOnlyList<Stage2GuildContract> Active { get; }
        public int CompletedForRank => successes[(int)Rank];
        public int CompletedFor(Stage2GuildRank rank) => rank>=0&&rank<=Stage2GuildRank.S?successes[(int)rank]:0;
        public const int RequiredForTrial = 3;
        public event Action Changed;
        public Stage2Guild(Stage2GuildCatalog catalog, Stage2Inventory inventory, Stage2HeroProgression progression)
        {
            if(catalog==null || !catalog.TryValidate(out _)) throw new ArgumentException("Invalid guild catalog.");
            Catalog=catalog; wallet=inventory??throw new ArgumentNullException(nameof(inventory));
            hero=progression??throw new ArgumentNullException(nameof(progression)); Active=active.AsReadOnly();
        }
        public Stage2GuildContract FindActive(int id) => active.Find(c=>c.Quest.id==id);
        public bool BoardAvailable(Stage2GuildRank rank) => rank >= 0 && rank <= Rank;
        public bool Available(Stage2GuildQuest q) => q!=null && (q.trial
            ? Rank < Stage2GuildRank.S && (int)q.rank==(int)Rank+1 && CompletedForRank>=RequiredForTrial
            : q.rank<=Rank);
        public Stage2GuildAction Evaluate(int id)
        {
            if(busy) return Stage2GuildAction.Busy;
            var q=Catalog.Find(id); if(q==null) return Stage2GuildAction.Invalid;
            var contract=FindActive(id);
            if(contract!=null) return !contract.Cleared ? Stage2GuildAction.InProgress :
                RewardGold(q)>int.MaxValue-wallet.Gold ? Stage2GuildAction.GoldLimit : Stage2GuildAction.TurnIn;
            if(!Available(q)) return Stage2GuildAction.Locked;
            foreach(var other in active) if(other.Quest.enemyKind==q.enemyKind) return Stage2GuildAction.SpotOccupied;
            return Stage2GuildAction.Accept;
        }
        // resetSpot must validate/prepare before replacing actors; null means no world mutation.
        public bool TryAccept(int id, Func<Stage2GuildQuest,Stage2GuildSpawn> resetSpot)
        {
            if(resetSpot==null || Evaluate(id)!=Stage2GuildAction.Accept) return false;
            busy=true;
            try
            {
                var q=Catalog.Find(id); var spawn=resetSpot(q);
                if(spawn==null || spawn.Generation<=0 || spawn.EnemyIds==null || spawn.EnemyIds.Length!=q.population) return false;
                var unique=new HashSet<int>(spawn.EnemyIds);
                if(unique.Count!=q.population || unique.Contains(0)) return false;
                foreach(var enemyId in unique) if(enemyId<1) return false;
                active.Add(new Stage2GuildContract(q,spawn)); Changed?.Invoke(); return true;
            }
            finally { busy=false; }
        }
        public bool RecordKill(Stage2GuildEnemyKind kind,int generation,int id)
        {
            if(busy) return false;
            foreach(var c in active)
                if(c.Quest.enemyKind==kind && c.Generation==generation && c.Record(id))
                { Changed?.Invoke(); return true; }
            return false;
        }
        public bool TryTurnIn(int id)
        {
            if(Evaluate(id)!=Stage2GuildAction.TurnIn) return false;
            busy=true;
            try
            {
                var c=FindActive(id); var q=c.Quest;
                var rewardGold=RewardGold(q);var rewardExperience=RewardExperience(q);
                // Commit membership/ownership before notification; a wallet callback cannot claim twice.
                active.Remove(c);
                if(q.trial) Rank=q.rank;
                else if(q.rank==Rank) successes[(int)Rank]=Math.Min(RequiredForTrial,CompletedForRank+1);
                if(!wallet.TryAddGold(rewardGold)) throw new InvalidOperationException("Prevalidated guild credit failed.");
                hero.AwardExperience(rewardExperience); Changed?.Invoke(); return true;
            }
            finally { busy=false; }
        }
        public bool Reset()
        {
            if(busy) return false;
            Rank=Stage2GuildRank.F; Array.Clear(successes,0,successes.Length); active.Clear(); Changed?.Invoke(); return true;
        }
    }
}
