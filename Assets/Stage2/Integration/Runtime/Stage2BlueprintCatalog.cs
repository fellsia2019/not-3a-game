using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    // Append IDs: existing paper identities and knowledge keep their meaning.
    public enum Stage2BlueprintKind { Longshot, WandRecipe, ForgeII, ForgeIII, Sawmill, Quarry, IronMine, ForgeConstruction }
    public enum Stage2BlueprintCategory { Opening, Upgrade }

    [Serializable]
    public sealed class Stage2BlueprintDefinition
    {
        public Stage2BlueprintKind kind;
        public string displayName, shortLabel, result;
        [Min(2)] public int goldPrice;
        public Stage2BlueprintCategory Category => TargetStage > 0 ? Stage2BlueprintCategory.Upgrade : Stage2BlueprintCategory.Opening;
        public int TargetStage => kind == Stage2BlueprintKind.ForgeII ? 2 : kind == Stage2BlueprintKind.ForgeIII ? 3 : 0;
        public int ArtIndex => TargetStage > 0 ? 2 : kind == Stage2BlueprintKind.WandRecipe ? 1 : 0;
    }

    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Blueprint Catalog")]
    public sealed class Stage2BlueprintCatalog : ScriptableObject
    {
        public const int KindCount = 8;
        public static Stage2BlueprintKind ForProduction(Not3A.Stage2.Map.ResourceProductionBuilding building) => building switch {
            Not3A.Stage2.Map.ResourceProductionBuilding.Sawmill => Stage2BlueprintKind.Sawmill,
            Not3A.Stage2.Map.ResourceProductionBuilding.Quarry => Stage2BlueprintKind.Quarry,
            Not3A.Stage2.Map.ResourceProductionBuilding.IronMine => Stage2BlueprintKind.IronMine,
            _ => throw new ArgumentOutOfRangeException(nameof(building)) };
        [SerializeField] private Stage2BlueprintDefinition[] entries = {
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.Longshot, displayName="Чертёж Дальнобойной башни", shortLabel="Дальнобойная", result="Открывает строительство Дальнобойной башни. Постройка оплачивается отдельно ресурсами.", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.WandRecipe, displayName="Владение посохом", shortLabel="Посох", result="Позволяет экипировать посох и изучать его навыки. Открывает ковку посоха в Небесной кузнице (ступень II).", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.ForgeII, displayName="Небесная кузница → II", shortLabel="Кузница II", result="Открывает улучшение до Небесной кузницы (ступень II). Само улучшение выполняется в кузнице.", goldPrice=200 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.ForgeIII, displayName="Кузница → III", shortLabel="Кузница III", result="Открывает улучшение кузницы до ступени III. Само улучшение выполняется в кузнице.", goldPrice=400 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.Sawmill, displayName="Чертёж лесопилки", shortLabel="Лесопилка", result="Открывает строительство лесопилки. Постройка оплачивается отдельно ресурсами.", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.Quarry, displayName="Чертёж каменоломни", shortLabel="Каменоломня", result="Открывает строительство каменоломни. Постройка оплачивается отдельно ресурсами.", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.IronMine, displayName="Чертёж железной шахты", shortLabel="Жел. шахта", result="Открывает строительство железной шахты. Постройка оплачивается отдельно ресурсами.", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.ForgeConstruction, displayName="Чертёж кузницы", shortLabel="Кузница", result="Открывает строительство кузницы. Постройка и изучение её рецептов и улучшений — отдельные действия.", goldPrice=100 }
        };
        public int Count => entries.Length;
        public Stage2BlueprintDefinition Get(Stage2BlueprintKind kind)
        { foreach(var entry in entries) if(entry.kind==kind)return entry; throw new ArgumentOutOfRangeException(nameof(kind)); }
        public bool TryValidate(out string reason)
        {
            reason="Blueprint catalog requires uniquely identified documents and positive gold prices.";
            if(entries==null||entries.Length!=KindCount)return false;
            var seen=new bool[KindCount];
            foreach(var entry in entries)
            {
                if(entry==null||(int)entry.kind<0||(int)entry.kind>=KindCount||seen[(int)entry.kind]||
                    string.IsNullOrWhiteSpace(entry.displayName)||string.IsNullOrWhiteSpace(entry.shortLabel)||
                    string.IsNullOrWhiteSpace(entry.result)||entry.goldPrice<2||entry.goldPrice>1000000)return false;
                seen[(int)entry.kind]=true;
            }
            reason=null;return true;
        }
    }
}
