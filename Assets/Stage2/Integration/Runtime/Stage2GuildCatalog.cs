using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public enum Stage2GuildRank { F, E, D, C, B, A, S }
    public enum Stage2GuildEnemyKind { Orc, Ogre }

    [Serializable]
    public sealed class Stage2GuildQuest
    {
        public int id;
        public Stage2GuildRank rank;
        public Stage2GuildEnemyKind enemyKind;
        public bool trial;
        public string title, location, objective;
        public int gold, experience, population, health, damage, killExperience;
        public float speed, attackInterval, attackWindup, reach, aggroRadius, leashRadius, radius;
        public bool IsValid => id > 0 && rank >= 0 && rank <= Stage2GuildRank.S &&
            enemyKind >= 0 && enemyKind <= Stage2GuildEnemyKind.Ogre && (!trial || rank > 0) &&
            !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(location) &&
            !string.IsNullOrWhiteSpace(objective) && gold > 0 && experience > 0 && population > 0 &&
            population <= 24 && health > 0 && damage > 0 && killExperience > 0 &&
            FinitePositive(speed) && FinitePositive(attackInterval) && FinitePositive(attackWindup) &&
            attackWindup < attackInterval && FinitePositive(reach) && FinitePositive(aggroRadius) &&
            FinitePositive(leashRadius) && leashRadius > aggroRadius && FinitePositive(radius);
        private static bool FinitePositive(float n) => float.IsFinite(n) && n > 0;
    }

    [CreateAssetMenu(menuName="Not 3A/Stage 3/Guild")]
    public sealed class Stage2GuildCatalog : ScriptableObject
    {
        [SerializeField] private float interactionRadius = 3f;
        [SerializeField] private Stage2GuildQuest[] quests = Defaults();
        public float InteractionRadius => interactionRadius;
        public int Count => quests?.Length ?? 0;
        public Stage2GuildQuest GetAt(int index) => quests[index];
        public Stage2GuildQuest Find(int id)
        { if(quests != null) foreach(var q in quests) if(q != null && q.id == id) return q; return null; }
        public bool TryValidate(out string reason)
        {
            reason = null;
            if(!float.IsFinite(interactionRadius) || interactionRadius <= 0 || Count == 0)
            { reason = "Guild needs a local radius and orders."; return false; }
            var ids = new System.Collections.Generic.HashSet<int>();
            var ordinary = new int[7]; var trials = new int[7];
            foreach(var q in quests)
            {
                if(q == null || !q.IsValid || !ids.Add(q.id))
                { reason = "Invalid or duplicate guild order."; return false; }
                if(q.trial) trials[(int)q.rank]++; else ordinary[(int)q.rank]++;
            }
            for(var i=0;i<7;i++) if(ordinary[i] == 0 || (i > 0 && trials[i] != 1))
            { reason = "Each rank needs orders and exactly one next-rank trial."; return false; }
            return true;
        }

        // Initial authored trial balance. Serialized into Stage3Guild.asset by the installer.
        private static Stage2GuildQuest[] Defaults()
        {
            var result = new System.Collections.Generic.List<Stage2GuildQuest>();
            for(var r=0;r<7;r++)
            {
                for(var kind=0;kind<2;kind++) result.Add(Create(r,kind,false));
                if(r>0) result.Add(Create(r,1,true));
            }
            return result.ToArray();
        }
        private static Stage2GuildQuest Create(int rank, int kind, bool trial)
        {
            var heavy = kind == 1;
            var scale = Mathf.Pow(1.45f,rank) * (trial ? 1.15f : 1f);
            return new Stage2GuildQuest {
                id = rank * 10 + (trial ? 3 : kind + 1), rank=(Stage2GuildRank)rank,
                enemyKind=(Stage2GuildEnemyKind)kind, trial=trial,
                title=trial ? "Испытание на ранг " + (Stage2GuildRank)rank : heavy ? "Стоянка огров" : "Лагерь орков",
                location=heavy ? "Каменистая поляна" : "Лесная опушка",
                objective=heavy ? "Победить огров на стоянке" : "Победить орков в лагере",
                gold=(trial ? 100 : heavy ? 80 : 50)*(rank+1),
                experience=(trial ? 150 : heavy ? 120 : 80)*(rank+1),
                population=heavy ? 1+rank/3 : 3+rank/2,
                health=Mathf.CeilToInt((heavy ? 18 : 8)*scale),
                damage=Mathf.CeilToInt((heavy ? 12 : 6)*Mathf.Pow(1.2f,rank)),
                killExperience=(heavy ? 4 : 2)*(rank+1), speed=heavy ? 1.25f : 1.7f,
                attackInterval=heavy ? 2.2f : 1.6f, attackWindup=heavy ? .55f : .35f,
                reach=heavy ? 1.55f : 1.15f, radius=heavy ? .48f : .32f,
                aggroRadius=8f, leashRadius=14f
            };
        }
    }
}
