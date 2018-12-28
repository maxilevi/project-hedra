using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestInventoryItemInfo : CraftingInventoryItemInfo
    {
        public QuestInventoryItemInfo(IPlayer Player, InventoryItemRenderer Renderer) : base(Player, Renderer)
        {
        }
    }
}