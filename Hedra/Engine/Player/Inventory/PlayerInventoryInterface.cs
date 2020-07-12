using System;
using System.Collections.Generic;
using Hedra.AISystem;
using Hedra.Core;
using Hedra.Engine.Input;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering.UI;
using Hedra.Input;
using Hedra.Localization;
using Hedra.Sound;
using Hedra.WeaponSystem;
using System.Numerics;
using Hedra.Items;
using Hedra.Numerics;
using Newtonsoft.Json;
using Silk.NET.Input.Common;


namespace Hedra.Engine.Player.Inventory
{
    public class PlayerInventoryInterface : PlayerInterface
    {
        private readonly IPlayer _player;
        private readonly InventoryArrayInterface _itemsArrayInterface;
        private readonly InventoryArrayInterface _leftMainItemsArrayInterface;
        private readonly InventoryArrayInterface _rightMainItemsArrayInterface;
        private readonly InventoryArrayInterface _extraSpaceItemsArrayInterface;
        private readonly InventoryArrayInterfaceManager _interfaceManager;
        private readonly InventoryBackground _inventoryBackground;
        private readonly InventoryStateManager _stateManager;
        private readonly InventoryCompanionInfo _companionInterface;
        private bool _show;
        private Item _bagItem;

        public PlayerInventoryInterface(IPlayer Player)
        {
            _player = Player;
            _stateManager = new InventoryStateManager(_player);
            _inventoryBackground = new InventoryBackground(Vector2.UnitY * .65f);
            _itemsArrayInterface = new InventoryArrayInterface(_player.Inventory.ItemsArray, 0, _player.Inventory.ItemsArray.Length, 10, Vector2.One)
            {
                Position = Vector2.UnitY * -.5f
            };
            _leftMainItemsArrayInterface = new InventoryArrayInterface(_player.Inventory.MainItemsArray, 0, 4, 1, Vector2.One,
                new [] { "Assets/UI/InventorySlotBoots.png", "Assets/UI/InventorySlotPants.png", "Assets/UI/InventorySlotChest.png", "Assets/UI/InventorySlotHelmet.png" })
            {
                Position = Vector2.UnitY * .05f + Vector2.UnitX * -.25f + Vector2.UnitY * .05f
            };
            _rightMainItemsArrayInterface = new InventoryArrayInterface(_player.Inventory.MainItemsArray, 4, 4, 1, Vector2.One,
                new[] { "Assets/UI/InventorySlotPet.png", "Assets/UI/InventorySlotGlider.png", "Assets/UI/InventorySlotRing.png", "Assets/UI/InventorySlotWeapon.png" })
            {
                Position = Vector2.UnitY * .05f + Vector2.UnitX * +.25f + Vector2.UnitY * .05f
            };
            _extraSpaceItemsArrayInterface = new InventoryArrayInterface(new InventoryArray(HoldingBagHandler.Size), 0, HoldingBagHandler.Size, 6, Vector2.One)
            {
                Position = Vector2.UnitY * -.45f + Vector2.UnitX * .6f
            };
            _companionInterface = new InventoryCompanionInfo
            {
                Position = Vector2.UnitX * -.65f + Vector2.UnitY * .1f
            };
            var itemInfoInterface = new InventoryInterfaceItemInfo
            {
                Position = Vector2.UnitX * .6f + Vector2.UnitY * .1f
            };
            _interfaceManager = new InventoryArrayInterfaceManager(itemInfoInterface, _itemsArrayInterface,
                _leftMainItemsArrayInterface, _rightMainItemsArrayInterface, _extraSpaceItemsArrayInterface);
            _stateManager.OnStateChange += Invoke;
            _player.Inventory.InventoryUpdated += UpdateInventory;
            _player.Companion.CompanionChanged += _companionInterface.UpdateStats;
            _interfaceManager.OnItemMove += OnItemMove;
        }

        private void UpdateInventory()
        {
            _interfaceManager.UpdateView();
        }

        private void SetInventoryState(bool State)
        {        
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
                Cursor.Show = true;
            }
            else
            {
                _stateManager.ReleaseState();
            }
            SetCompanionState(State);
        }

        private void SetCompanionState(bool State)
        {
            if (_player.Companion.Item == null) return;
            if (State)
                _companionInterface.Show(_player.Companion.Item, _player.Companion.Entity);
            else
                _companionInterface.Hide();
            
        }

        private void UpdateCompanionUI()
        {
            if(_companionInterface.Enabled)
                _companionInterface.UpdateStats(_player.Companion.Item, _player.Companion.Entity);
            if (_player.Companion.Item == _companionInterface.ShowingCompanion) return;
            
            if(_player.Companion.Item == null)
                _companionInterface.Hide();
            else
                _companionInterface.Show(_player.Companion.Item, _player.Companion.Entity);
        }

        public void Update()
        {
            if (_show)
            {
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, Time.DeltaTime * 16f);
                _player.View.TargetYaw = 
                    Mathf.Lerp(_player.View.TargetYaw, (float) Math.Atan2(-_player.Orientation.Z, -_player.Orientation.X), Time.DeltaTime * 16f);
                _player.View.CameraHeight = 
                    Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4, Time.DeltaTime * 16f);
                _inventoryBackground.UpdateView(_player);
                UpdateCompanionUI();
            }
        }

        public void ShowInventoryForItem(Item Bag)
        {
            if (Bag != _bagItem)
            {
                _bagItem = Bag;
                _extraSpaceItemsArrayInterface.SetArray(Bag.GetAttribute<InventoryArray>("Inventory"));
                _extraSpaceItemsArrayInterface.Enabled = true;
            }
            else
            {
                _bagItem = null;
                _extraSpaceItemsArrayInterface.Enabled = false;
            }
        }

        private void OnItemMove(InventoryArray PreviousArray, InventoryArray NewArray, int Index, Item Item)
        {
            if (_extraSpaceItemsArrayInterface.Array == NewArray || _extraSpaceItemsArrayInterface.Array == PreviousArray)
            {
                if (NewArray != null)
                {
                    var newItem = NewArray[Index];
                    /* We should not be able to store holding bags in holding bags */
                    if (newItem == _bagItem)
                    {
                        PreviousArray.AddItem(newItem);
                        NewArray.SetItem(Index, null);
                        UpdateInventory();
                    }
                    else
                    {
                        HoldingBagHandler.SaveInventory(_bagItem, _extraSpaceItemsArrayInterface.Array);
                    }
                }
                else
                {
                    HoldingBagHandler.SaveInventory(_bagItem, _extraSpaceItemsArrayInterface.Array);
                }
            }
        }

        public override Key OpeningKey => Controls.InventoryOpen;
        public override bool Show
        {
            get => _show;
            set
            {
                if(_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                _itemsArrayInterface.Enabled = _show;
                _leftMainItemsArrayInterface.Enabled = _show;
                _rightMainItemsArrayInterface.Enabled = _show;
                _inventoryBackground.Enabled = _show;
                _interfaceManager.Enabled = _show;
                _extraSpaceItemsArrayInterface.Enabled = false;
                this.UpdateInventory();
                this.SetInventoryState(_show);
                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
            }
        }
        
        protected override bool HasExitAnimation => true;
    }
}