using Hedra.Engine.Player.Inventory;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryItemInfo : InventoryInterfaceItemInfo
    {
        public CraftingInventoryItemInfo(InventoryItemRenderer Renderer) : base(Renderer)
        {
        }

        protected override void UpdateView()
        {
            base.UpdateView();
        }
    }
}