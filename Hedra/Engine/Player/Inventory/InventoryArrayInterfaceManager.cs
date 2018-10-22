﻿using System;
using System.Linq;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
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
    public delegate void OnItemMoveEventHandler(InventoryArray PreviousArray, InventoryArray NewArray, int Index, Item Item);

    public class InventoryArrayInterfaceManager
    {
        public OnItemMoveEventHandler OnItemMove;
        private readonly InventoryInterfaceItemInfo _itemInfoInterface;
        private readonly InventoryArrayInterface[] _interfaces;
        private readonly Button _cancelButton;
        private float _selectedMeshHeight;
        private ObjectMesh _selectedMesh;
        private Button _selectedButton;
        private Item _selectedItem;
        private bool _enabled;
        private bool _willReset;

        public InventoryArrayInterfaceManager(InventoryInterfaceItemInfo ItemInfoInterface, params InventoryArrayInterface[] Interfaces)
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
                    buttons[j].HoverEnter += (Sender, EventArgs) => this.HoverEnter(buttons[k], EventArgs);
                    buttons[j].HoverExit += (Sender, EventArgs) => this.HoverExit(buttons[k], EventArgs);
                }
            }
            _itemInfoInterface = ItemInfoInterface;
            EventDispatcher.RegisterMouseMove(this, this.MouseMove);
            EventDispatcher.RegisterMouseDown(this, this.MouseClick);
        }

        private void MouseClick(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (_selectedButton != null)
            {
                _willReset = true;
                TaskManager.After(10, delegate
                {
                    if(_willReset)
                        this.DropItem(_selectedItem);
                });
            }
        }

        private void MouseMove(object Sender, MouseMoveEventArgs EventArgs)
        {
            var newCoords = Mathf.ToNormalizedDeviceCoordinates(
                new Vector2(EventArgs.Mouse.X, GameSettings.SurfaceHeight - EventArgs.Mouse.Y),
                new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                );
            if (_selectedButton != null)
            {
                _selectedButton.Position = newCoords;
            }
        }

        protected virtual void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            if(EventArgs == null || EventArgs.Button != MouseButton.Left) return;
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var array = this.ArrayByButton(button);
            var item = array[itemIndex];
            if (item == null && _selectedButton == null || button == _selectedButton) return;
            _willReset = false;
            if (item != null && _selectedButton == null)
            {
                this.SetSelectedItem(button, item);
                array[itemIndex] = null;
                this.SetCancelButton(button);
                this.UpdateView();
                SoundManager.PlayUISound(SoundType.ButtonClick);
            }
            else if (_selectedButton != null)
            {
                var newIndex = this.IndexByButton(_selectedButton);
                var newArray = this.ArrayByButton(_selectedButton);
                if (!array.CanSetItem(itemIndex, _selectedItem))
                {
                    this.ShowCannotYieldEquipment(_selectedItem);
                    return;
                }
                array[itemIndex] = _selectedItem;
                newArray[newIndex] = item;                
                this.ResetSelected();
                this.UpdateView();
                OnItemMove?.Invoke(newArray, array, itemIndex, item);
                SoundManager.PlayUISound(SoundType.ButtonClick);
            }
        }

        protected virtual void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs == null || EventArgs.Button != MouseButton.Right) return;
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var array = this.ArrayByButton(button);
            var item = array[itemIndex];
            array[itemIndex] = null;
            if (array.HasRestrictions(itemIndex) && item != null)
                this.PlaceItemInFirstEmptyPosition(item);
            else
                this.PlaceInRestrictionsOrFirstEmpty(itemIndex, array, item);
            
            this.UpdateView();
            SoundManager.PlayUISound(SoundType.ButtonClick);
        }

        private void SetSelectedItem(Button SelectedButton, Item SelectedItem)
        {
            _selectedButton = SelectedButton;
            _selectedItem = SelectedItem;
            var renderer = this.RendererByButton(_selectedButton);
            _selectedMesh = renderer.BuildModel(_selectedItem, out _selectedMeshHeight);
            _selectedButton.Texture.IdPointer = () => renderer.Draw(_selectedMesh, SelectedItem, true, _selectedMeshHeight * InventoryItemRenderer.ZOffsetFactor);
        }

        private void SetCancelButton(Button SelectedButton)
        {
            _cancelButton.Position = SelectedButton.Position;
            _cancelButton.Scale = SelectedButton.Scale;
            _cancelButton.Clickable = false;
            TaskManager.After(10, () => _cancelButton.Clickable = true);
        }

        private void PlaceItemInFirstEmptyPosition(Item Item)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var newArray = _interfaces[i].Array;
                if(newArray.AddItem(Item)) return;
            }
        }

        private void PlaceInRestrictionsOrFirstEmpty(int ItemIndex, InventoryArray Array, Item Item)
        {
            if(Item == null) return;
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var newArray = _interfaces[i].Array;
                for (var j = 0; j < newArray.Length; j++)
                {
                    if (newArray.HasRestrictions(j))
                    {
                        var restrictions = newArray.GetRestrictions(j);
                        for (var k = 0; k < restrictions.Length; k++)
                        {
                            if (restrictions[k] != Item.EquipmentType) continue;
                            this.SwitchItems(ItemIndex, j, Array, newArray);
                            newArray[j] = Item;
                            return;
                        }
                    }
                }
            }
            this.ShowCannotYieldEquipment(Item);
            this.PlaceItemInFirstEmptyPosition(Item);
        }

        private void ShowCannotYieldEquipment(Item Item)
        {
            if (Item.IsEquipment)
            {
                GameManager.Player.MessageDispatcher.ShowNotification("YOU HAVEN'T LEARNED TO\nUSE THAT TYPE OF EQUIPMENT",
                    System.Drawing.Color.Red, 2f, true);
            }
        }

        private void SwitchItems(int Index, int IndexToSwitch, InventoryArray Array, InventoryArray ArrayToSwitch)
        {
            var item = Array[Index];
            Array[Index] = ArrayToSwitch[IndexToSwitch];
            ArrayToSwitch[IndexToSwitch] = item;
        }

        private void DropItem(Item SelectedItem)
        {
            if(SelectedItem == null) return;
            World.DropItem(SelectedItem, LocalPlayer.Instance.Position);//FIXME
            this.ResetSelected();
        }

        protected InventoryArray ArrayByButton(Button Sender)
        {
            return this.InterfaceByButton(Sender).Array;
        }

        protected InventoryArrayInterface InterfaceByButton(Button Sender)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var result = Array.IndexOf(_interfaces[i].Buttons, Sender);
                if (result != -1) return _interfaces[i];
            }
            return null;
        }

        protected int IndexByButton(Button Sender)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var result = Array.IndexOf(_interfaces[i].Buttons, Sender);
                if (result != -1) return result + _interfaces[i].Offset;
            }
            return -1;
        }

        protected int OffsetByButton(Button Sender)
        {
            return this.InterfaceByButton(Sender).Offset;
        }

        protected Item ItemByButton(Button Sender)
        {
            return this.ArrayByButton(Sender)[this.IndexByButton(Sender)];
        }

        protected InventoryItemRenderer RendererByButton(Button Sender)
        {
            return this.InterfaceByButton(Sender).Renderer;
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
            renderer.UpdateView();
            _selectedButton.Texture.IdPointer = () => renderer.Draw(k);
            _selectedButton = null;
            _selectedItem = null;
            _cancelButton.Position = Vector2.Zero;
            _selectedMesh?.Dispose();
            _selectedMesh = null;
        }

        private void HoverEnter(object Sender, MouseEventArgs EventArgs)
        {
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var array = this.ArrayByButton(button);
            var item = array[itemIndex];

            _itemInfoInterface?.Show(item);
        }

        private void HoverExit(object Sender, MouseEventArgs EventArgs)
        {
            _itemInfoInterface?.Hide();
        }

        public bool HasCancelButton { get; set; } = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                for (var i = 0; i < _interfaces.Length; i++)
                {
                    _interfaces[i].Enabled = value;
                }
                if(_itemInfoInterface != null) _itemInfoInterface.Enabled = value;
                if (HasCancelButton)
                {
                    if (_enabled)
                        _cancelButton.Enable();
                    else
                        _cancelButton.Disable();
                }
                this.Cancel();
            }
        }

        public virtual void UpdateView()
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                _interfaces[i].UpdateView();
            }
        }
    }
}
