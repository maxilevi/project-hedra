using Hedra.Engine.Player.CraftingSystem;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestingInventoryInterfaceManager : CraftingInventoryArrayInterfaceManager
    {
        public QuestingInventoryInterfaceManager(int Columns, int Rows, CraftingInventoryItemInfo ItemInfoInterface, CraftingInventoryArrayInterface Interface) : base(Columns, Rows, ItemInfoInterface, Interface)
        {
        }
    }
}