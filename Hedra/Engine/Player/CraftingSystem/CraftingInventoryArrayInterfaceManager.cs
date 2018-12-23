using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK.Input;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly CraftingInventoryArrayInterface _craftingInterface;
        private readonly CraftingInventoryItemInfo _itemInfo;
        
        public CraftingInventoryArrayInterfaceManager(CraftingInventoryItemInfo ItemInfoInterface,
            CraftingInventoryArrayInterface Interface)
            : base(ItemInfoInterface, Interface)
        {
            _craftingInterface = Interface;
            _itemInfo = ItemInfoInterface;
        }

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            var button = (Button) Sender;
            var item = ItemByButton(button);
            if(item == null) return;
            _craftingInterface.SelectedRecipeIndex = IndexByButton(button);
            UpdateView();
            SoundPlayer.PlayUISound(SoundType.ButtonClick);
        }
        
        public override void UpdateView()
        {
            base.UpdateView();
            if(Enabled)
                _itemInfo?.Show(_craftingInterface.Array[_craftingInterface.SelectedRecipeIndex]);
        }

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
        }

        protected override void HoverEnter(object Sender, MouseEventArgs EventArgs)
        {
        }

        protected override void HoverExit(object Sender, MouseEventArgs EventArgs)
        {
        }
    }
}