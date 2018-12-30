using Hedra.Engine.Player.PagedInterface;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryArrayInterfaceManager : PagedInventoryArrayInterfaceManager
    {
        private readonly CraftingInventoryArrayInterface _craftingInterface;
        private readonly CraftingInventoryItemInfo _itemInfo;
        
        public CraftingInventoryArrayInterfaceManager(int Columns, int Rows, CraftingInventoryItemInfo ItemInfoInterface,
            CraftingInventoryArrayInterface Interface) : base(Columns, Rows, ItemInfoInterface, Interface)
        {
            _craftingInterface = Interface;
            _itemInfo = ItemInfoInterface;
        }

        protected override void OnEnterPressed()
        {
            _itemInfo.Craft();
        }

        public override void UpdateView()
        {
            base.UpdateView();
            if (Enabled)
            {
                _itemInfo?.Show(
                    _craftingInterface.CurrentOutput,
                    _craftingInterface.CurrentRecipe
                );
            }
        }
    }
}