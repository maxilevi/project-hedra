using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK.Input;

namespace Hedra.Engine.Player.PagedInterface
{
    public abstract class PagedInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly PagedInventoryArrayInterface _pagedInterface;
        private readonly int _columns;
        private readonly int _rows;
        private bool _enterPressed;
        private bool _leftPressed;
        private bool _rightPressed;
        private bool _upPressed;
        private bool _downPressed;

        protected PagedInventoryArrayInterfaceManager(int Columns, int Rows, InventoryInterfaceItemInfo ItemInfoInterface,
            PagedInventoryArrayInterface Interface)
            : base(ItemInfoInterface, Interface)
        {
            _columns = Columns;
            _rows = Rows;
            _pagedInterface = Interface;
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
            switch (EventArgs.Key)
            {
                case Key.Up:
                    if(!_upPressed)
                        newIndex = MoveSelector(newIndex, _rows, newIndex % _rows, (_columns - rowIndex - 1) * _rows + newIndex);
                    _upPressed = true;
                    break;
                case Key.Down:
                    if(!_downPressed)
                        newIndex = MoveSelector(newIndex, - _rows, newIndex % _rows, (_columns - rowIndex - 1) * _rows + newIndex);
                    _downPressed = true;
                    break;
                case Key.Right:
                    if(!_rightPressed)
                        newIndex = MoveSelector(newIndex, +1, 0, _rows * _columns-1);
                    _rightPressed = true;
                    break;
                case Key.Left:
                    if(!_leftPressed)
                        newIndex = MoveSelector(newIndex, -1, 0, _rows * _columns-1);
                    _leftPressed = true;
                    break;
                case Key.Enter:
                    if (!_enterPressed)
                        OnEnterPressed();
                    _enterPressed = true;
                    break;
                default:
                    return;
            }
            _pagedInterface.SelectedIndex = newIndex;
            UpdateView();
        }

        protected virtual void OnEnterPressed()
        {
        }
        
        private void OnKeyUp(object Sender, KeyEventArgs EventArgs)
        {
            if (!Enabled) return;
            switch (EventArgs.Key)
            {
                case Key.Enter:
                    _enterPressed = false;
                    break;
                case Key.Right:
                    _rightPressed = false;
                    break;
                case Key.Left:
                    _leftPressed = false;
                    break;
                case Key.Down:
                    _downPressed = false;
                    break;
                case Key.Up:
                    _upPressed = false;
                    break;
                default:
                    return;
            }
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