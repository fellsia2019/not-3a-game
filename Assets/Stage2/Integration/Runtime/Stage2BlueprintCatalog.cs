using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    public enum Stage2BlueprintKind { Longshot, WandRecipe, ForgeII, ForgeIII }
    public enum Stage2BlueprintCategory { Opening, Upgrade }

    [Serializable]
    public sealed class Stage2BlueprintDefinition
    {
        public Stage2BlueprintKind kind;
        public string displayName, shortLabel, result;
        [Min(2)] public int goldPrice;
        public Stage2BlueprintCategory Category => (int)kind < 2 ? Stage2BlueprintCategory.Opening : Stage2BlueprintCategory.Upgrade;
        public int TargetStage => kind == Stage2BlueprintKind.ForgeII ? 2 : kind == Stage2BlueprintKind.ForgeIII ? 3 : 0;
        public int ArtIndex => (int)kind < 2 ? (int)kind : 2;
    }

    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Blueprint Catalog")]
    public sealed class Stage2BlueprintCatalog : ScriptableObject
    {
        [SerializeField] private Stage2BlueprintDefinition[] entries = {
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.Longshot, displayName="Чертёж Дальнобойной башни", shortLabel="Дальнобойная", result="Открывает строительство Дальнобойной башни. Постройка оплачивается отдельно ресурсами.", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.WandRecipe, displayName="Чертёж жезла", shortLabel="Жезл", result="Открывает рецепт жезла в кузнице. Ковка оплачивается отдельно ресурсами.", goldPrice=100 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.ForgeII, displayName="Кузница → II", shortLabel="Кузница II", result="Открывает улучшение кузницы до ступени II. Само улучшение выполняется в кузнице.", goldPrice=200 },
            new Stage2BlueprintDefinition { kind=Stage2BlueprintKind.ForgeIII, displayName="Кузница → III", shortLabel="Кузница III", result="Открывает улучшение кузницы до ступени III. Само улучшение выполняется в кузнице.", goldPrice=400 }
        };
        public int Count => entries.Length;
        public Stage2BlueprintDefinition Get(Stage2BlueprintKind kind)
        { foreach(var entry in entries) if(entry.kind==kind)return entry; throw new ArgumentOutOfRangeException(nameof(kind)); }
        public bool TryValidate(out string reason)
        {
            reason="Blueprint catalog requires the four accepted, uniquely identified documents and positive gold prices.";
            if(entries==null||entries.Length!=4)return false;
            var seen=new bool[4];
            foreach(var entry in entries)
            {
                if(entry==null||(int)entry.kind<0||(int)entry.kind>=4||seen[(int)entry.kind]||
                    string.IsNullOrWhiteSpace(entry.displayName)||string.IsNullOrWhiteSpace(entry.shortLabel)||
                    string.IsNullOrWhiteSpace(entry.result)||entry.goldPrice<2||entry.goldPrice>1000000)return false;
                seen[(int)entry.kind]=true;
            }
            reason=null;return true;
        }
    }
}
