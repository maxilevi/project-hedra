using System;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryArrayInterfaceManager
    {
        private readonly InventoryArrayInterface[] _interfaces;
        private readonly Button _cancelButton;
        private EntityMesh _selectedMesh;
        private Button _selectedButton;
        private Item _selectedItem;
        private bool _enabled;
        private bool _willReset;

        public InventoryArrayInterfaceManager(params InventoryArrayInterface[] Interfaces)
        {
            _interfaces = Interfaces;
            _cancelButton = new Button(Vector2.Zero, Vector2.One, GUIRenderer.TransparentTexture);
            _cancelButton.Click += (S, E) => this.Cancel();
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var buttons = _interfaces[i].Buttons;
                for (var j = 0; j < buttons.Length; j++)
                {
                    var k = j;
                    buttons[j].Click += (Sender, EventArgs) => this.Interact(buttons[k], EventArgs);
                    buttons[j].Click += (Sender, EventArgs) => this.Use(buttons[k], EventArgs);
                    buttons[j].HoverEnter += this.HoverEnter;
                    buttons[j].HoverExit += this.HoverExit;
                }
            }
            EventDispatcher.RegisterMouseMove(this, this.MouseMove);
            EventDispatcher.RegisterMouseDown(this, this.MouseClick);
        }

        private void MouseClick(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (_selectedButton != null)
            {
                _willReset = true;
                TaskManager.Delay(10, delegate
                {
                    if(_willReset)
                        this.DropItem(_selectedItem);
                });
            }
        }

        private void MouseMove(object Sender, MouseMoveEventArgs EventArgs)
        {
            if (_selectedButton != null)
            {
                var newCoords = Mathf.ToNormalizedDeviceCoordinates(EventArgs.Mouse.X, Constants.HEIGHT - EventArgs.Mouse.Y);
                _selectedButton.Position = newCoords;
            }
        }

        private void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            if(EventArgs.Button != MouseButton.Left) return;
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var array = this.ArrayByButton(button);
            var item = array[itemIndex];
            if (item == null && _selectedButton == null || button == _selectedButton) return;
            _willReset = false;
            if (item != null && _selectedButton == null)
            {
                _selectedButton = button;
                _selectedItem = item;
                _cancelButton.Position = button.Position;
                _cancelButton.Scale = button.Scale;
                array[itemIndex] = null;
                var renderer = this.RendererByButton(_selectedButton);
                _selectedMesh = EntityMesh.FromVertexData(item.Model);
                _selectedMesh.UseFog = false;
                _selectedButton.Texture.IdPointer = () => renderer.Draw(_selectedMesh, item);
                _cancelButton.Clickable = false;
                TaskManager.Delay(10, () => _cancelButton.Clickable = true);
                SoundManager.PlaySoundInPlayersLocation(SoundType.ButtonClick);
            }
            else if (_selectedButton != null)
            {
                var newIndex = IndexByButton(_selectedButton);
                var newArray = ArrayByButton(_selectedButton);
                if(!array.CanSetItem(itemIndex, _selectedItem)) return;

                array[itemIndex] = _selectedItem;
                newArray[newIndex] = item;
                this.ResetSelected();
                this.UpdateView();
                SoundManager.PlaySoundInPlayersLocation(SoundType.ButtonClick);
            }
        }

        private void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs.Button != MouseButton.Right) return;
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var array = this.ArrayByButton(button);
            var item = array[itemIndex];
            array[itemIndex] = null;
            if (array.HasRestrictions(itemIndex))
                this.PlaceItemInFirstEmptyPosition(item);          
            else
                this.PlaceInRestrictionsOrFirstEmpty(item);
            this.UpdateView();
            SoundManager.PlaySoundInPlayersLocation(SoundType.ButtonClick);
        }

        private void PlaceItemInFirstEmptyPosition(Item Item)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var newArray = _interfaces[i].Array;
                for (var j = 0; j < newArray.Length; j++)
                {
                    if (newArray[j] != null || newArray.HasRestrictions(j)) continue;
                    newArray[j] = Item;
                    return;
                }
            }
        }

        private void PlaceInRestrictionsOrFirstEmpty(Item Item)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var newArray = _interfaces[i].Array;
                for (var j = 0; j < newArray.Length; j++)
                {
                    if (newArray[j] == null && newArray.HasRestrictions(j))
                    {
                        var restrictions = newArray.GetRestrictions(j);
                        for (var k = 0; k < restrictions.Length; k++)
                        {
                            if (restrictions[k] != Item.WeaponType) continue;
                            newArray[j] = Item;
                            return;
                        }
                    }
                }
            }
            this.PlaceItemInFirstEmptyPosition(Item);
        }

        private void DropItem(Item SelectedItem)
        {
            if(SelectedItem == null) return;
            World.DropItem(SelectedItem, LocalPlayer.Instance.Position);//FIXME
            this.ResetSelected();
        }

        private InventoryArray ArrayByButton(Button Sender)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var result = Array.IndexOf(_interfaces[i].Buttons, Sender);
                if (result != -1) return _interfaces[i].Array;
            }
            return null;
        }

        private int IndexByButton(Button Sender)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var result = Array.IndexOf(_interfaces[i].Buttons, Sender);
                if (result != -1) return result + _interfaces[i].Offset;
            }
            return -1;
        }

        private int OffsetByButton(Button Sender)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var result = Array.IndexOf(_interfaces[i].Buttons, Sender);
                if (result != -1) return _interfaces[i].Offset;
            }
            throw new ArgumentException($"Button not found in button list.");
        }

        private InventoryItemRenderer RendererByButton(Button Sender)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var result = Array.IndexOf(_interfaces[i].Buttons, Sender);
                if (result != -1) return _interfaces[i].Renderer;
            }
            return null;
        }

        private void Cancel()
        {
            if(_selectedButton == null) return;

            var index = IndexByButton(_selectedButton);
            var array = ArrayByButton(_selectedButton);
            array[index] = _selectedItem;
            this.ResetSelected();
        }

        private void ResetSelected()
        {
            _selectedButton.Position = _cancelButton.Position;
            var renderer = this.RendererByButton(_selectedButton);
            var k = this.IndexByButton(_selectedButton) - this.OffsetByButton(_selectedButton);
            _selectedButton.Texture.IdPointer = () => renderer.Draw(k);
            _selectedButton = null;
            _selectedItem = null;
            _cancelButton.Position = Vector2.Zero;
            _selectedMesh?.Dispose();
            _selectedMesh = null;
        }

        private void HoverEnter(object Sender, MouseEventArgs EventArgs)
        {

        }

        private void HoverExit(object Sender, MouseEventArgs EventArgs)
        {

        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if(_enabled)
                    _cancelButton.Enable();
                else
                    _cancelButton.Disable();
                this.Cancel();
            }
        }

        public void UpdateView()
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                _interfaces[i].UpdateView();
            }
        }
    }
}
