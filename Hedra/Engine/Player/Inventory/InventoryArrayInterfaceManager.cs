using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using Silk.NET.Input;
using SixLabors.ImageSharp;
using Button = Hedra.Engine.Rendering.UI.Button;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnItemMoveEventHandler(InventoryArray PreviousArray, InventoryArray NewArray, int Index,
        Item Item);

    public class InventoryArrayInterfaceManager : IDisposable
    {
        private readonly Button _cancelButton;
        private readonly InventoryArrayInterface[] _interfaces;
        private readonly InventoryInterfaceItemInfo _itemInfoInterface;
        private bool _enabled;
        private Button _selectedButton;
        private InventoryArray _selectedButtonArray;
        private int _selectedButtonIndex;
        private Item _selectedItem;
        private ObjectMesh _selectedMesh;
        private Vector3 _selectedMeshSize;
        private bool _willReset;
        public OnItemMoveEventHandler OnItemMove;

        public InventoryArrayInterfaceManager(InventoryInterfaceItemInfo ItemInfoInterface,
            params InventoryArrayInterface[] Interfaces)
        {
            _interfaces = Interfaces;
            _cancelButton = new Button(Vector2.Zero, Vector2.One, GUIRenderer.TransparentTexture);
            _cancelButton.Click += (S, E) => Cancel();
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var buttons = _interfaces[i].Buttons;
                for (var j = 0; j < buttons.Length; j++)
                {
                    var k = j;
                    buttons[j].Click += (Sender, EventArgs) => Interact(buttons[k], EventArgs);
                    buttons[j].Click += (Sender, EventArgs) => Use(buttons[k], EventArgs);
                    buttons[j].HoverEnter += () => HoverEnter(buttons[k]);
                    buttons[j].HoverExit += () => HoverExit(buttons[k]);
                }
            }

            _itemInfoInterface = ItemInfoInterface;
            EventDispatcher.RegisterMouseMove(this, MouseMove);
            EventDispatcher.RegisterMouseDown(this, MouseClick);
        }

        public bool HasCancelButton { get; set; } = true;

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                for (var i = 0; i < _interfaces.Length; i++) _interfaces[i].Enabled = value;
                if (_itemInfoInterface != null) _itemInfoInterface.Enabled = value;
                if (HasCancelButton)
                {
                    if (_enabled)
                        _cancelButton.Enable();
                    else
                        _cancelButton.Disable();
                }

                Cancel();
            }
        }

        public virtual void Dispose()
        {
            EventDispatcher.UnregisterMouseMove(this);
            EventDispatcher.UnregisterMouseDown(this);
        }

        private void MouseClick(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (_selectedButton != null)
            {
                _willReset = true;
                TaskScheduler.After(.01f, delegate
                {
                    if (_willReset)
                        DropItem(_selectedItem);
                    OnItemMove?.Invoke(_selectedButtonArray, null, _selectedButtonIndex, _selectedItem);
                });
            }
        }

        private void MouseMove(object Sender, MouseMoveEventArgs EventArgs)
        {
            var newCoords = Mathf.ToNormalizedDeviceCoordinates(
                new Vector2(EventArgs.X, GameSettings.SurfaceHeight - EventArgs.Y),
                new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
            );
            if (_selectedButton != null) _selectedButton.Position = newCoords;
        }

        protected virtual void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs.Button != MouseButton.Left) return;
            var button = (Button)Sender;
            var itemIndex = IndexByButton(button);
            var array = ArrayByButton(button);
            var item = array[itemIndex];
            if (item == null && _selectedButton == null || button == _selectedButton) return;
            _willReset = false;
            if (item != null && _selectedButton == null)
            {
                SetSelectedItem(array, itemIndex, button, item);
                array[itemIndex] = null;
                SetCancelButton(button);
                UpdateView();
                SoundPlayer.PlayUISound(SoundType.ButtonClick);
            }
            else if (_selectedButton != null)
            {
                var newIndex = IndexByButton(_selectedButton);
                var newArray = ArrayByButton(_selectedButton);
                if (!array.CanSetItem(itemIndex, _selectedItem))
                {
                    ShowCannotYieldEquipment(_selectedItem);
                    return;
                }

                array[itemIndex] = _selectedItem;
                newArray[newIndex] = item;
                ResetSelected();
                UpdateView();
                OnItemMove?.Invoke(newArray, array, itemIndex, item);
                SoundPlayer.PlayUISound(SoundType.ButtonClick);
            }
        }

        protected virtual void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs.Button != MouseButton.Right) return;
            var button = (Button)Sender;
            var itemIndex = IndexByButton(button);
            var array = ArrayByButton(button);
            var item = array[itemIndex];
            array[itemIndex] = null;
            if (item != null && item.IsConsumable)
            {
                var success = Consume(item);
                if (success && item.GetAttribute(CommonAttributes.Amount, 1) > 1)
                {
                    item.SetAttribute(CommonAttributes.Amount, item.GetAttribute<int>(CommonAttributes.Amount) - 1);
                    array[itemIndex] = item;
                }

                if (!success) array[itemIndex] = item;
            }
            else if (item != null && item.IsFood && array.Length > PlayerInventory.FoodHolder)
            {
                array[itemIndex] = item;
                SwitchItems(itemIndex, PlayerInventory.FoodHolder, array, array);
            }
            else if (array.HasRestrictions(itemIndex) && item != null)
            {
                PlaceItemInFirstEmptyPosition(item);
            }
            else
            {
                PlaceInRestrictionsOrFirstEmpty(itemIndex, array, item);
            }

            UpdateView();
            SoundPlayer.PlayUISound(SoundType.ItemEquip);
        }

        private void SetSelectedItem(InventoryArray Array, int Index, Button SelectedButton, Item SelectedItem)
        {
            _selectedButton = SelectedButton;
            _selectedButtonArray = Array;
            _selectedButtonIndex = Index;
            _selectedItem = SelectedItem;
            var renderer = RendererByButton(_selectedButton);
            _selectedMesh = InventoryItemRenderer.BuildModel(_selectedItem.Model, out _selectedMeshSize);
            _selectedButton.Texture.IdPointer = () =>
                InventoryItemRenderer.Draw(_selectedMesh, SelectedItem, true, _selectedMeshSize);
        }

        private void SetCancelButton(Button SelectedButton)
        {
            _cancelButton.Position = SelectedButton.Position;
            _cancelButton.Scale = SelectedButton.Scale;
            _cancelButton.CanClick = false;
            TaskScheduler.After(.01f, () => _cancelButton.CanClick = true);
        }

        private void PlaceItemInFirstEmptyPosition(Item Item)
        {
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var newArray = _interfaces[i].Array;
                if (newArray.AddItem(Item)) return;
            }
        }

        private void PlaceInRestrictionsOrFirstEmpty(int ItemIndex, InventoryArray Array, Item Item)
        {
            if (Item == null) return;
            for (var i = 0; i < _interfaces.Length; i++)
            {
                var newArray = _interfaces[i].Array;
                for (var j = 0; j < newArray.Length; j++)
                    if (newArray.HasRestrictions(j))
                    {
                        var restrictions = newArray.GetRestrictions(j);
                        for (var k = 0; k < restrictions.Length; k++)
                        {
                            if (restrictions[k] != Item.EquipmentType) continue;
                            SwitchItems(ItemIndex, j, Array, newArray);
                            newArray[j] = Item;
                            return;
                        }
                    }
            }

            ShowCannotYieldEquipment(Item);
            PlaceItemInFirstEmptyPosition(Item);
        }


        private static bool Consume(Item Item)
        {
            return ItemHandlerFactory.Instance.Build(
                Item.GetAttribute<string>(CommonAttributes.Handler)
            ).Consume(GameManager.Player, Item);
        }

        private void ShowCannotYieldEquipment(Item Item)
        {
            if (Item.IsEquipment)
                GameManager.Player.MessageDispatcher.ShowNotification(Translations.Get("cannot_use_equipment"),
                    Color.Red, 2f, true);
        }

        private void SwitchItems(int Index, int IndexToSwitch, InventoryArray Array, InventoryArray ArrayToSwitch)
        {
            var item = Array[Index];
            Array[Index] = ArrayToSwitch[IndexToSwitch];
            ArrayToSwitch[IndexToSwitch] = item;
        }

        private void DropItem(Item SelectedItem)
        {
            if (SelectedItem == null) return;
            World.DropItem(SelectedItem, LocalPlayer.Instance.Position); //FIXME
            ResetSelected();
        }

        protected InventoryArray ArrayByButton(Button Sender)
        {
            return InterfaceByButton(Sender).Array;
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
            return InterfaceByButton(Sender).Offset;
        }

        protected Item ItemByButton(Button Sender)
        {
            return ArrayByButton(Sender)[IndexByButton(Sender)];
        }

        protected InventoryItemRenderer RendererByButton(Button Sender)
        {
            return InterfaceByButton(Sender).Renderer;
        }

        private void Cancel()
        {
            if (_selectedButton == null) return;

            var index = IndexByButton(_selectedButton);
            var array = ArrayByButton(_selectedButton);
            array[index] = _selectedItem;
            ResetSelected();
        }

        private void ResetSelected()
        {
            _selectedButton.Position = _cancelButton.Position;
            var renderer = RendererByButton(_selectedButton);
            var k = IndexByButton(_selectedButton) - OffsetByButton(_selectedButton);
            renderer.UpdateView();
            _selectedButton.Texture.IdPointer = () => renderer.Draw(k);
            _selectedButton = null;
            _selectedItem = null;
            _cancelButton.Position = Vector2.Zero;
            _selectedMesh?.Dispose();
            _selectedMesh = null;
        }

        protected virtual void HoverEnter(object Sender)
        {
            var button = (Button)Sender;
            var itemIndex = IndexByButton(button);
            var array = ArrayByButton(button);
            var item = array[itemIndex];

            _itemInfoInterface?.Show(item);
        }

        protected virtual void HoverExit(object Sender)
        {
            _itemInfoInterface?.Hide();
        }

        public virtual void UpdateView()
        {
            for (var i = 0; i < _interfaces.Length; i++) _interfaces[i].UpdateView();
        }
    }
}