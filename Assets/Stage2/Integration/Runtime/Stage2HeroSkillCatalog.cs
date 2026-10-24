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
        public bool supportsAutocast;
        public float periodicPower,controlDuration,slowMultiplier=1,vulnerability;
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
            reason="Skills require36ordered nodes,11actives,20combatpassives,5adventurertalents and bounded fire/lightning.";
            if(nodes==null||nodes.Length!=36||!float.IsFinite(potionCooldown)||potionCooldown<=0||
                !float.IsFinite(potionRestoration)||potionRestoration<=0)return false;
            for(var i=0;i<nodes.Length;i++)
            {
                var n=nodes[i];
                if(n==null||n.id!=i||string.IsNullOrEmpty(n.title)||n.active!=(i<11)||n.weapon < -1||n.weapon>1||
                    n.requiredLevel<1||n.requiredLevel>100||n.prerequisite>=i||n.prerequisite < -1||
                    n.power==null||n.power.Length!=(i<11?4:i>=31?5:3)||n.mana==null||n.mana.Length!=4||
                    n.cooldown==null||n.cooldown.Length!=4||!float.IsFinite(n.duration)||n.duration<0||
                    !float.IsFinite(n.range)||n.range<=0||!float.IsFinite(n.radius)||n.radius<0||
                    !float.IsFinite(n.aimTime)||n.aimTime<0||!float.IsFinite(n.periodicPower)||n.periodicPower<0||
                    !float.IsFinite(n.controlDuration)||n.controlDuration<0||!float.IsFinite(n.slowMultiplier)||n.slowMultiplier<.25f||n.slowMultiplier>1||
                    !float.IsFinite(n.vulnerability)||n.vulnerability<0||n.vulnerability>1||n.maxTargets<1||n.maxTargets>6)return false;
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
        {
            if(nodes==null||nodes.Length!=31&&nodes.Length!=34&&nodes.Length!=36)return;
            var previous=nodes.Length;var current=Defaults();Array.Resize(ref nodes,current.Length);
            for(var i=previous;i<current.Length;i++)nodes[i]=current[i];
            for(var i=31;i<current.Length;i++)
            {
                if(nodes[i]?.power?.Length!=3)continue;
                Array.Resize(ref nodes[i].power,5);
                nodes[i].power[3]=current[i].power[3];nodes[i].power[4]=current[i].power[4];
            }
        }
        private static Stage2HeroSkillDefinition A(int id,string name,int weapon,int level,int prerequisite,float[] power,float[] mana,float[] cd,string description)
            =>new Stage2HeroSkillDefinition{id=id,title=name,weapon=weapon,requiredLevel=level,prerequisite=prerequisite,
                power=power,mana=mana,cooldown=cd,active=true,description=description,duration=id==7?8:3,
                supportsAutocast=id==1||id==6,radius=id==2||id==3||id==5?1.2f:id==6?.8f:id==9?2f:1.6f,
                periodicPower=id==2||id==4?.35f:id==5?.3f:id==9?.45f:0,
                controlDuration=id==3||id==10?1:0,slowMultiplier=id==3?.65f:id==5?.8f:1,
                vulnerability=id==5||id==10?.2f:0};
        private static Stage2HeroSkillDefinition P(int id,string name,int weapon,int level,int prerequisite,float a,float b,float c,string description)
            =>new Stage2HeroSkillDefinition{id=id,title=name,weapon=weapon,requiredLevel=level,prerequisite=prerequisite,
                power=new[]{a,b,c},active=false,description=description};
        private static Stage2HeroSkillDefinition Adventurer(int id,string name,int level,int prerequisite,string description)
            =>new Stage2HeroSkillDefinition{id=id,title=name,weapon=-1,requiredLevel=level,prerequisite=prerequisite,
                power=new[]{.05f,.1f,.15f,.2f,.25f},active=false,description=description};
        private static Stage2HeroSkillDefinition[] Defaults()=>new[]{
            A(0,"Блинк",-1,1,-1,new[]{1f,1,1,1},new[]{100f,75,50,25},new[]{5f,4,3,2},"Скачок к курсору. Общий навык обоих видов оружия."),
            A(1,"Прицельный выстрел",0,2,0,new[]{2.5f,3,3.5f,4},new[]{12f,12,11,10},new[]{7f,6,5,4},"Сильный выстрел по одной цели. Движение отменяет прицеливание."),
            A(2,"Пылающая стрела",0,10,1,new[]{1.2f,1.4f,1.6f,1.8f},new[]{18f,18,17,16},new[]{10f,9,8,7},"Малая область: физический удар и магическое горение поражённых врагов."),
            A(3,"Ледяная стрела",0,20,1,new[]{1.3f,1.5f,1.7f,2},new[]{16f,16,15,14},new[]{10f,9,8,7},"Малая область: замораживает врагов на 1 секунду, затем сохраняет замедление."),
            A(4,"Пробивающая стрела",0,35,2,new[]{2f,2.5f,3f,3.5f},new[]{20f,20,18,16},new[]{20f,18,16,14},"Магическая стрела остаётся в области попадания на 3 секунды и продолжает атаковать врагов."),
            A(5,"Ядовитая стрела",0,50,3,new[]{.8f,1f,1.2f,1.4f},new[]{15f,15,14,12},new[]{18f,16,14,12},"Отравляет врагов в области, замедляет и повышает получаемый физический урон."),
            A(6,"Чародейский снаряд",1,2,0,new[]{2f,2.4f,2.8f,3.2f},new[]{12f,12,11,10},new[]{5f,4.5f,4,3.5f},"Магический удар с небольшим всплеском. ПКМ включает автокаст по ближайшей цели."),
            A(7,"Личный барьер",1,10,6,new[]{15f,25,35,45},new[]{25f,25,23,20},new[]{20f,18,16,14},"Временный личный барьер поглощает урон, не защищает трон."),
            A(8,"Восстановление",1,20,6,new[]{20f,30,40,50},new[]{25f,25,23,20},new[]{20f,18,16,14},"Восстанавливает HP героя за MP."),
            A(9,"Огненный шар",1,35,7,new[]{2.5f,3,3.5f,4},new[]{35f,35,32,30},new[]{25f,22,18,15},"Взрыв в увеличенной области и огонь на земле ещё 3 секунды. До четырёх целей за тик."),
            A(10,"Грозовой разряд",1,50,8,new[]{3f,3.5f,4,4.5f},new[]{40f,40,36,32},new[]{30f,25,20,15},"Удар молнии: оглушение на 1 секунду и +20% получаемого магического урона на 3 секунды."),
            P(11,"Точный удар",-1,1,0,1,2,3,"Дополнительный урон обычных атак обоих видов оружия."),
            P(12,"Быстрые руки",-1,1,0,.1f,.2f,.3f,"Темп обычных атак обоих видов оружия."),
            P(13,"Лёгкий шаг",-1,1,0,.05f,.1f,.15f,"Постоянная скорость движения."),
            P(14,"Стрельба на ходу",-1,50,5,1,2,3,"Разрешает обычные атаки в движении: 60%, 80%, 100% урона. Прицеливание требует остановки."),
            P(15,"Верный прицел",0,5,1,.1f,.2f,.3f,"Усиливает прицельный выстрел."),
            P(16,"Собранность",0,5,1,.1f,.2f,.3f,"Сокращает время прицеливания."),
            P(17,"Жар наконечника",0,14,2,.1f,.2f,.3f,"Усиливает пылающую стрелу и её горение."),
            P(18,"Тлеющая рана",0,14,2,1,2,3,"Продлевает горение на одну, две или три секунды."),
            P(19,"Холодный наконечник",0,24,3,.05f,.1f,.15f,"Усиливает замедление и немного продлевает заморозку ледяной стрелы."),
            P(20,"Цепкий холод",0,24,3,1,2,3,"Продлевает замедление."),
            P(21,"Неугасимый след",0,39,4,1,2,3,"Продлевает действие пробивающей стрелы."),
            P(22,"Стойкий яд",0,54,5,1,2,3,"Продлевает отравление, замедление и физическую уязвимость ядовитой стрелы."),
            P(23,"Магический резонанс",1,5,6,.05f,.1f,.15f,"Усиливает магические атаки посоха."),
            P(24,"Бережная магия",1,5,6,.05f,.1f,.15f,"Снижает MP-стоимость навыков посоха."),
            P(25,"Прочный барьер",1,14,7,.1f,.2f,.3f,"Увеличивает ёмкость личного барьера."),
            P(26,"Целительная сила",1,24,8,.1f,.2f,.3f,"Усиливает восстановление HP."),
            P(27,"Жар стихии",1,39,9,.1f,.2f,.3f,"Усиливает огненный шар, сохраняя его КД."),
            P(28,"Проводник",1,54,10,.1f,.2f,.3f,"Усиливает грозовой разряд, сохраняя его КД."),
            P(29,"Запас маны",1,5,6,10,20,30,"Увеличивает максимальную MP, без мгновенного восполнения."),
            P(30,"Поток маны",1,5,6,.1f,.2f,.3f,"Ускоряет восстановление MP."),
            Adventurer(31,"Опыт авантюриста",5,0,"Больше XP за фактическую сдачу контракта."),
            Adventurer(32,"Щедрый контракт",15,31,"Больше золота за фактическую сдачу контракта."),
            Adventurer(33,"Опыт следопыта",15,31,"Больше XP за мировых орков и огров; не влияет на портальных врагов."),
            Adventurer(34,"Охотник на чудовищ",10,31,"Больше урона мировым врагам от обычных атак и навыков. Не действует на портальные орды."),
            Adventurer(35,"Стойкость странника",20,34,"Меньше урона от мировых врагов. Сочетается с защитой снаряжения, не расходует MP.")
        };
    }
}
