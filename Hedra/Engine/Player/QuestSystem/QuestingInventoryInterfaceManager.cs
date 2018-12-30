using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.PagedInterface;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestingInventoryInterfaceManager : PagedInventoryArrayInterfaceManager
    {
        public QuestingInventoryInterfaceManager(int Columns, int Rows, CraftingInventoryItemInfo ItemInfoInterface,
            PagedInventoryArrayInterface Interface) : base(Columns, Rows, ItemInfoInterface, Interface)
        {
        }
    }
}