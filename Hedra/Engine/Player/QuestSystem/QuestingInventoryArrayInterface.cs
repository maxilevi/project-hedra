using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestingInventoryArrayInterface : CraftingInventoryArrayInterface
    {
        public QuestingInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns) : base(Player, Array, Rows, Columns)
        {
        }
        
        protected override Translation TitleTranslation => Translation.Create("quests");
        
        protected override Item[] ArrayObjects => new Item[0];
    }
}