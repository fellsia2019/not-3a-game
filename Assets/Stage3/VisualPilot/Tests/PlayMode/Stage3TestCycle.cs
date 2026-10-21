#if UNITY_EDITOR
using Not3A.Stage2.RunState;
using Not3A.Stage2.Integration;
using System.Linq;
namespace Not3A.Stage3.VisualPilot.Tests
{
    internal static class Stage3TestCycle
    {
        // Explicit prerequisite fixture for old tower/visual tests; no runtime unlock cheat.
        internal static void StudyBlueprint(Stage2GameComposition game,Stage2BlueprintKind kind)
        {
            var equipment=game.GetComponent<Stage2EquipmentDriver>()?.Rules;
            if(equipment?.Blueprints==null||equipment.Blueprints.IsStudied(kind))return;
            var binding=game.GetComponent("Stage3PlayerInterfaceBinding");
            var merchant=(Stage2Merchant)binding.GetType().GetProperty("Merchant").GetValue(binding);
            var paper=merchant.BlueprintItems.Single(item=>item.Kind==kind);
            game.Gathering.Economy.Inventory.TryAddGold(equipment.Blueprints.Catalog.Get(kind).goldPrice);
            if(merchant.TryTradeBlueprint(true,paper.Id)!=Stage2TradeResult.Ready||equipment.Blueprints.TryStudy(paper.Id)!=Stage2BlueprintResult.Ready)
                throw new System.InvalidOperationException("Test prerequisite study failed: "+kind);
        }
        // Explicit fixture clock advance, never a runtime skip control.
        internal static bool StartNight(Stage2Run run)
        {
            if (!run.UsesDaytimeGates || run.Phase != Stage2RunPhase.Preparation) return run.StartWave();
            // The public remaining duration is float; the clock is double. Cross
            // the boundary explicitly rather than landing a fraction before20:00.
            run.Tick(run.PreparationRemaining + .001f);
            return run.Phase == Stage2RunPhase.Wave;
        }
    }
}
#endif
