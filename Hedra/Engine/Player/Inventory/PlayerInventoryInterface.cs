using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.Input;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering.UI;
using Hedra.Input;
using Hedra.Sound;
using Hedra.WeaponSystem;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    public class PlayerInventoryInterface : PlayerInterface
    {
        private readonly IPlayer _player;
        private readonly InventoryArrayInterface _itemsArrayInterface;
        private readonly InventoryArrayInterface _leftMainItemsArrayInterface;
        private readonly InventoryArrayInterface _rightMainItemsArrayInterface;
        private readonly InventoryArrayInterfaceManager _interfaceManager;
        private readonly InventoryBackground _inventoryBackground;
        private readonly InventoryStateManager _stateManager;
        private bool _show;

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
            var itemInfoInterface = new InventoryInterfaceItemInfo(_itemsArrayInterface.Renderer)
            {
                Position = Vector2.UnitX * .6f + Vector2.UnitY * .1f
            };
            _interfaceManager = new InventoryArrayInterfaceManager(itemInfoInterface, _itemsArrayInterface,
                _leftMainItemsArrayInterface, _rightMainItemsArrayInterface);
            _stateManager.OnStateChange += Invoke;
            _player.Inventory.InventoryUpdated += UpdateInventory;
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
        }

        public void Update()
        {
            if (_show)
            {
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float) Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw, (float) Math.Acos(-_player.Orientation.X),
                    (float) Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    (float) Time.DeltaTime * 16f);
                _inventoryBackground.UpdateView(_player);
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
                this.UpdateInventory();
                this.SetInventoryState(_show);
                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
            }
        }
        
        protected override bool HasExitAnimation => true;
    }
}