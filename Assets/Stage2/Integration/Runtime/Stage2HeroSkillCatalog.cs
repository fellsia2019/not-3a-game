using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    [Serializable]
    public sealed class Stage2HeroSkillDefinition
    {
        public int id,weapon=-1,requiredLevel=1,prerequisite=-1;
        public string title,description;
        public bool active;
        public float[] power={1,1.2f,1.4f,1.6f},mana={0,0,0,0},cooldown={0,0,0,0};
        public float duration=3,radius=1.6f,range=6,aimTime=.5f;
        public int maxTargets=4;
        public int MaxRank=>power.Length;
        public float Power(int rank)=>rank<=0?0:power[Mathf.Clamp(rank-1,0,power.Length-1)];
        public float Mana(int rank)=>rank<=0?0:mana[Mathf.Clamp(rank-1,0,mana.Length-1)];
        public float Cooldown(int rank)=>rank<=0?0:cooldown[Mathf.Clamp(rank-1,0,cooldown.Length-1)];
    }
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Hero Skill Catalog")]
    public sealed class Stage2HeroSkillCatalog:ScriptableObject
    {
        [SerializeField] private Stage2HeroSkillDefinition[] nodes=Defaults();
        [SerializeField] private float potionCooldown=20,potionRestoration=100;
        public int Count=>nodes.Length;
        public float PotionCooldown=>potionCooldown;
        public float PotionRestoration=>potionRestoration;
        public Stage2HeroSkillDefinition Get(int id)=>id>=0&&id<nodes.Length?nodes[id]:null;
        public bool TryValidate(out string reason)
        {
            reason="Skills require34ordered nodes,11actives,20combatpassives,3adventurertalents and bounded fire/lightning.";
            if(nodes==null||nodes.Length!=34||!float.IsFinite(potionCooldown)||potionCooldown<=0||
                !float.IsFinite(potionRestoration)||potionRestoration<=0)return false;
            for(var i=0;i<nodes.Length;i++)
            {
                var n=nodes[i];
                if(n==null||n.id!=i||string.IsNullOrEmpty(n.title)||n.active!=(i<11)||n.weapon < -1||n.weapon>1||
                    n.requiredLevel<1||n.requiredLevel>100||n.prerequisite>=i||n.prerequisite < -1||
                    n.power==null||n.power.Length!=(i<11?4:3)||n.mana==null||n.mana.Length!=4||
                    n.cooldown==null||n.cooldown.Length!=4||!float.IsFinite(n.duration)||n.duration<0||
                    !float.IsFinite(n.range)||n.range<=0||!float.IsFinite(n.radius)||n.radius<0||
                    !float.IsFinite(n.aimTime)||n.aimTime<0||n.maxTargets<1||n.maxTargets>6)return false;
                foreach(var v in n.power)if(!float.IsFinite(v)||v<0)return false;
                foreach(var v in n.mana)if(!float.IsFinite(v)||v<0)return false;
                foreach(var v in n.cooldown)if(!float.IsFinite(v)||v<0)return false;
            }
            foreach(var id in new[]{9,10})
            {var n=nodes[id];if(n.cooldown[0]<25||n.cooldown[0]>30||n.radius>2||n.maxTargets>4)return false;
                foreach(var v in n.cooldown)if(v<15)return false;}
            reason=null;return true;
        }
        public void MigrateAdventurerDefaults()
        {if(nodes==null||nodes.Length!=31)return;var current=Defaults();Array.Resize(ref nodes,current.Length);for(var i=31;i<current.Length;i++)nodes[i]=current[i];}
        private static Stage2HeroSkillDefinition A(int id,string name,int weapon,int level,int prerequisite,float[] power,float[] mana,float[] cd,string description)
            =>new Stage2HeroSkillDefinition{id=id,title=name,weapon=weapon,requiredLevel=level,prerequisite=prerequisite,
                power=power,mana=mana,cooldown=cd,active=true,description=description,duration=id==4?6:id==5?5:id==7?8:3};
        private static Stage2HeroSkillDefinition P(int id,string name,int weapon,int level,int prerequisite,float a,float b,float c,string description)
            =>new Stage2HeroSkillDefinition{id=id,title=name,weapon=weapon,requiredLevel=level,prerequisite=prerequisite,
                power=new[]{a,b,c},active=false,description=description};
        private static Stage2HeroSkillDefinition[] Defaults()=>new[]{
            A(0,"Блинк",-1,1,-1,new[]{1f,1,1,1},new[]{100f,75,50,25},new[]{5f,4,3,2},"Скачок к курсору. Общий навык обоих видов оружия."),
            A(1,"Прицельный выстрел",0,2,0,new[]{2.5f,3,3.5f,4},new[]{12f,12,11,10},new[]{7f,6,5,4},"Сильный выстрел по одной цели. Движение отменяет прицеливание."),
            A(2,"Пылающая стрела",0,10,1,new[]{1.2f,1.4f,1.6f,1.8f},new[]{18f,18,17,16},new[]{10f,9,8,7},"Стрела поджигает одну цель. Горение не распространяется."),
            A(3,"Ледяная стрела",0,20,1,new[]{1.3f,1.5f,1.7f,2},new[]{16f,16,15,14},new[]{10f,9,8,7},"Стрела замедляет одну цель, без заморозки лагеря."),
            A(4,"Сосредоточенность",0,35,2,new[]{.25f,.35f,.45f,.55f},new[]{20f,20,18,16},new[]{20f,18,16,14},"На время повышает темп обычных атак."),
            A(5,"Попутный ветер",0,50,3,new[]{.25f,.35f,.45f,.55f},new[]{15f,15,14,12},new[]{18f,16,14,12},"Временно ускоряет передвижение."),
            A(6,"Чародейский снаряд",1,2,0,new[]{2f,2.4f,2.8f,3.2f},new[]{12f,12,11,10},new[]{5f,4.5f,4,3.5f},"Магический удар по одной цели."),
            A(7,"Личный барьер",1,10,6,new[]{15f,25,35,45},new[]{25f,25,23,20},new[]{20f,18,16,14},"Временный личный барьер поглощает урон, не защищает трон."),
            A(8,"Восстановление",1,20,6,new[]{20f,30,40,50},new[]{25f,25,23,20},new[]{20f,18,16,14},"Восстанавливает HP героя за MP."),
            A(9,"Огненный шар",1,35,7,new[]{2.5f,3,3.5f,4},new[]{35f,35,32,30},new[]{25f,22,18,15},"Огненный удар по небольшой области, до четырёх целей."),
            A(10,"Грозовой разряд",1,50,8,new[]{3f,3.5f,4,4.5f},new[]{40f,40,36,32},new[]{30f,25,20,15},"Молния по небольшой области, до четырёх целей."),
            P(11,"Точный удар",-1,1,0,1,2,3,"Дополнительный урон обычных атак обоих видов оружия."),
            P(12,"Быстрые руки",-1,1,0,.1f,.2f,.3f,"Темп обычных атак обоих видов оружия."),
            P(13,"Лёгкий шаг",-1,1,0,.05f,.1f,.15f,"Постоянная скорость движения."),
            P(14,"Стрельба на ходу",-1,12,13,1,2,3,"Разрешает обычные атаки в движении: 60%, 80%, 100% урона. Прицеливание требует остановки."),
            P(15,"Верный прицел",0,5,1,.1f,.2f,.3f,"Усиливает прицельный выстрел."),
            P(16,"Собранность",0,5,1,.1f,.2f,.3f,"Сокращает время прицеливания."),
            P(17,"Жар наконечника",0,14,2,.1f,.2f,.3f,"Усиливает пылающую стрелу и её горение."),
            P(18,"Тлеющая рана",0,14,2,1,2,3,"Продлевает горение на одну, две или три секунды."),
            P(19,"Холодный наконечник",0,24,3,.05f,.1f,.15f,"Усиливает замедление ледяной стрелы."),
            P(20,"Цепкий холод",0,24,3,1,2,3,"Продлевает замедление."),
            P(21,"Глубокое дыхание",0,39,4,1,2,3,"Продлевает сосредоточенность."),
            P(22,"Попутный порыв",0,54,5,1,2,3,"Продлевает попутный ветер."),
            P(23,"Магический резонанс",1,5,6,.05f,.1f,.15f,"Усиливает магические атаки посоха."),
            P(24,"Бережная магия",1,5,6,.05f,.1f,.15f,"Снижает MP-стоимость навыков посоха."),
            P(25,"Прочный барьер",1,14,7,.1f,.2f,.3f,"Увеличивает ёмкость личного барьера."),
            P(26,"Целительная сила",1,24,8,.1f,.2f,.3f,"Усиливает восстановление HP."),
            P(27,"Жар стихии",1,39,9,.1f,.2f,.3f,"Усиливает огненный шар, сохраняя его КД."),
            P(28,"Проводник",1,54,10,.1f,.2f,.3f,"Усиливает грозовой разряд, сохраняя его КД."),
            P(29,"Запас маны",1,5,6,10,20,30,"Увеличивает максимальную MP, без мгновенного восполнения."),
            P(30,"Поток маны",1,5,6,.1f,.2f,.3f,"Ускоряет восстановление MP."),
            P(31,"Опыт авантюриста",-1,5,0,.05f,.1f,.15f,"Больше XP за фактическую сдачу контракта."),
            P(32,"Щедрый контракт",-1,15,31,.05f,.1f,.15f,"Больше золота за фактическую сдачу контракта."),
            P(33,"Опыт следопыта",-1,15,31,.05f,.1f,.15f,"Больше XP за мировых орков и огров; не влияет на портальных врагов.")
        };
    }
}
