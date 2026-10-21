using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Opt-in current-world item lifetime. Loss rewind preserves it; full Reset starts anew.</summary>
    [DisallowMultipleComponent]
    public sealed class Stage2EquipmentDriver : MonoBehaviour
    {
        private Stage2GameComposition game;
        public Stage2Equipment Rules { get; private set; }
        public Stage2EquipmentCatalog Catalog { get; private set; }
        public void Configure(Stage2GameComposition owner, Stage2EquipmentCatalog data)
        {
            Unbind(); game = owner; Catalog = data;
            Rules = new Stage2Equipment(data, game.Gathering.Economy.Inventory);
            Rules.Changed += ApplyEquipment;
            game.RunDriver.Run.Reset += ResetEquipment;
            ApplyEquipment();
        }
        private void ResetEquipment() => Rules.Reset();
        private void ApplyEquipment()
        {
            game.Gathering.CancelGatherUntilRelease();
            var weapon = Rules.Equipped(Stage2ItemSlot.Weapon);
            if (weapon == null) game.HeroCombat.Rules.SetEquippedWeapon(false, 0, 0, 1, 0);
            else
            {
                var stats = Catalog.Stats(weapon.Type, weapon.Quality, weapon.Level);
                game.HeroCombat.Rules.SetEquippedWeapon(true, stats.Damage, stats.Range, stats.Interval, stats.Flight);
            }
            var axe = Tool(Stage2ItemSlot.Axe); var pickaxe = Tool(Stage2ItemSlot.Pickaxe);
            game.Gathering.Economy.SetManualToolStats(axe.HarvestPower, pickaxe.HarvestPower, axe.HarvestSpeed, pickaxe.HarvestSpeed);
        }
        private Stage2ItemStats Tool(Stage2ItemSlot slot)
        {
            var item = Rules.Equipped(slot);
            return item == null ? new Stage2ItemStats(0, 0, 0, 0, 0, 0, 1, false, false) : Catalog.Stats(item.Type, item.Quality, item.Level);
        }
        private void Unbind()
        {
            if (Rules != null) Rules.Changed -= ApplyEquipment;
            if (game != null) game.RunDriver.Run.Reset -= ResetEquipment;
        }
        private void OnDestroy() => Unbind();
    }
}
