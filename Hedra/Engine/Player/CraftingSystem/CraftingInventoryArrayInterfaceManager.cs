using Hedra.Engine.Player.Inventory;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        public CraftingInventoryArrayInterfaceManager(InventoryInterfaceItemInfo ItemInfoInterface, params InventoryArrayInterface[] Interfaces)
            : base(ItemInfoInterface, Interfaces)
        {
        }
    }
}