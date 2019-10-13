using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Input;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;


namespace Hedra.Engine.Player.PagedInterface
{
    public abstract class PagedInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly PagedInventoryArrayInterface _pagedInterface;
        private readonly ArrowSelectorState _selectorState;
        private readonly int _columns;
        private readonly int _rows;

        protected PagedInventoryArrayInterfaceManager(int Columns, int Rows, InventoryInterfaceItemInfo ItemInfoInterface,
            PagedInventoryArrayInterface Interface)
            : base(ItemInfoInterface, Interface)
        {
            _columns = Columns;
            _rows = Rows;
            _pagedInterface = Interface;
            _selectorState = new ArrowSelectorState();
            EventDispatcher.RegisterKeyDown(this, OnKeyDown);
            EventDispatcher.RegisterKeyUp(this, OnKeyUp);
        }

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            var button = (Button) Sender;
            var item = ItemByButton(button);
            if(item == null) return;
            _pagedInterface.SelectedIndex = IndexByButton(button);
            UpdateView();
            SoundPlayer.PlayUISound(SoundType.ButtonClick);
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

        private void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
            if (!Enabled) return;
            var newIndex = _pagedInterface.SelectedIndex;
            var rowIndex = newIndex / _rows;
            
            _selectorState.OnUp = () => newIndex = MoveSelector(newIndex, _rows, newIndex % _rows, (_columns - rowIndex - 1) * _rows + newIndex);
            _selectorState.OnDown = () => newIndex = MoveSelector(newIndex, - _rows, newIndex % _rows, (_columns - rowIndex - 1) * _rows + newIndex);
            _selectorState.OnRight = () => newIndex = MoveSelector(newIndex, +1, 0, _rows * _columns-1);
            _selectorState.OnLeft = () => newIndex = MoveSelector(newIndex, -1, 0, _rows * _columns-1);
            _selectorState.OnEnter = OnEnterPressed;
            
            ArrowSelector.ProcessKeyDown(EventArgs, _selectorState);
            _pagedInterface.SelectedIndex = newIndex;
            UpdateView();
        }

        protected virtual void OnEnterPressed()
        {
        }
        
        private void OnKeyUp(object Sender, KeyEventArgs EventArgs)
        {
            if (!Enabled) return;
            ArrowSelector.ProcessKeyUp(EventArgs, _selectorState);
        }
        
        private int MoveSelector(int Index, int Direction, int Min, int Max)
        {
            SoundPlayer.PlayUISound(SoundType.ButtonClick);
            var newIndex = (int)Mathf.Clamp(Index + Direction, Min, Max);
            if (_pagedInterface.Array[newIndex] == null) return Index;
            return newIndex;
        }

        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
            EventDispatcher.UnregisterKeyUp(this);
        }
    }
}