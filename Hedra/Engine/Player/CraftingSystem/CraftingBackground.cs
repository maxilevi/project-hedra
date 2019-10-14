using Hedra.Crafting;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.EntitySystem;
using Hedra.Localization;
using System.Numerics;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingBackground : InventoryBackground
    {
        public CraftingBackground(Vector2 Position) : base(Position)
        {
        }

        public override void UpdateView(IHumanoid Human)
        {
            var currentStation = CraftingInventory.GetCurrentStation(Human.Position);
            Name.Text = Translations.Get("crafting_menu");;
        }
    }
}