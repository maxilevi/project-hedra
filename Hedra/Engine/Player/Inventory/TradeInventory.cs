/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/08/2017
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Input;
using Hedra.Localization;
using Hedra.Numerics;
using Silk.NET.Input;

namespace Hedra.Engine.Player.Inventory
{
    /// <summary>
    ///     Description of TradeSystem.
    /// </summary>
    public class TradeInventory : PlayerInterface
    {
        public const int MerchantSpaces = 20;
        public const int TradeRadius = 12;
        private readonly TradeInventoryArrayInterfaceManager _interfaceManager;
        private readonly InventoryBackground _merchantBackground;
        private readonly InventoryArray _merchantItems;
        private readonly InventoryArrayInterface _merchantItemsInterface;
        private readonly LocalPlayer _player;
        private readonly InventoryBackground _playerBackground;
        private readonly InventoryArray _playerItems;
        private readonly InventoryArrayInterface _playerItemsInterface;
        private readonly InventoryStateManager _stateManager;
        private bool _show;
        private TradeComponent _tradeComponent;
        private Humanoid _trader;

        public TradeInventory(LocalPlayer Player)
        {
            _player = Player;
            _merchantItems = new InventoryArray(MerchantSpaces);
            _playerItems = new InventoryArray(PlayerInventory.InventorySpaces);
            _stateManager = new InventoryStateManager(_player);
            _playerItemsInterface = new InventoryArrayInterface(_playerItems, 0, _playerItems.Length, 5, Vector2.One)
            {
                Position = Vector2.UnitX * .5f + Vector2.UnitY * -.1f
            };
            _merchantItemsInterface =
                new InventoryArrayInterface(_merchantItems, 0, _merchantItems.Length, 5, Vector2.One)
                {
                    Position = Vector2.UnitX * -.5f + Vector2.UnitY * -.1f
                };
            var itemInfoInterface = new TradeInventoryInterfaceItemInfo
            {
                Position = Vector2.UnitY * .1f
            };
            _interfaceManager =
                new TradeInventoryArrayInterfaceManager(itemInfoInterface, _playerItemsInterface,
                    _merchantItemsInterface);
            _playerBackground = new InventoryBackground(Vector2.UnitX * .5f + Vector2.UnitY * .55f);
            _merchantBackground = new InventoryBackground(Vector2.UnitX * -.5f + Vector2.UnitY * .55f);
            _interfaceManager.OnTransactionComplete += (Item, Price, Type) =>
            {
                UpdateTraders(Item, Type);
                TransactionComplete?.Invoke(Item, Price, Type);
            };
            _stateManager.OnStateChange += State => { Invoke(State); };
        }

        public bool IsTrading { get; private set; }

        public override Key OpeningKey => Controls.Interact;

        public override bool Show
        {
            get => _show;
            set
            {
                if (value) TradeWithNearestMerchant();
                else Cancel();
            }
        }

        protected override bool HasExitAnimation => true;
        public event OnTransactionCompleteEventHandler TransactionComplete;

        public void Trade(Humanoid Trader)
        {
            IsTrading = true;
            _trader = Trader;
            _tradeComponent = Trader.SearchComponent<TradeComponent>();
            SetActive(true);
            _playerItems.SetItems(_player.Inventory.ItemsToArray());
            _merchantItems.SetItems(_tradeComponent.Items.ToArray());
            _interfaceManager.SetTraders(_player, _trader);
            UpdateView();
        }

        public void Cancel()
        {
            _interfaceManager.SetTraders(null, null);
            SetActive(false);
            IsTrading = false;
            _tradeComponent = null;
            _playerItems.Empty();
            _merchantItems.Empty();
            UpdateView();
        }


        public void UpdateView()
        {
            _interfaceManager.UpdateView();
        }

        public void UpdateInventory()
        {
            _playerItems.Empty();
            _merchantItems.Empty();
            _playerItems.SetItems(_player.Inventory.ItemsToArray());
            _merchantItems.SetItems(_tradeComponent.Items.ToArray());
            UpdateView();
            _player.Inventory.UpdateInventory();
        }

        public void ClearInventory()
        {
            _merchantItems.Empty();
            _playerItems.Empty();
        }

        public void UpdateTraders(Item Item, TransactionType Type)
        {
            for (var i = 0; i < _playerItems.Length; i++)
            {
                if (_player.Inventory[i]?.IsGold ?? false) continue;

                _player.Inventory.SetItem(i, null);
                _player.Inventory.SetItem(i, _playerItems[i]);
            }

            _tradeComponent.Items.Clear();
            for (var i = 0; i < _merchantItems.Length; i++) _tradeComponent.Items.Add(i, _merchantItems[i]);
            _tradeComponent.TransactionComplete(Item, Type);
            UpdateInventory();
        }

        private void SetInventoryState(bool State)
        {
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
                _player.View.PositionDelegate = () => (_player.Position + _trader.Position) / 2;
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
                    Mathf.Lerp(_player.View.TargetDistance, 10f, Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw,
                    (float)Math.Atan2(-_player.Orientation.Z, -_player.Orientation.X),
                    Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    Time.DeltaTime * 16f);
                _playerBackground.UpdateView(_player);
                _merchantBackground.UpdateView(_trader);
            }
        }

        private void SetActive(bool Value)
        {
            if (_show == Value || _stateManager.GetState() != _show) return;
            _show = Value;
            _merchantItemsInterface.Enabled = _show;
            _playerItemsInterface.Enabled = _show;
            _interfaceManager.Enabled = _show;
            _playerBackground.Enabled = _show;
            _merchantBackground.Enabled = _show;
            UpdateInventory();
            SetInventoryState(_show);
        }

        private void TradeWithNearestMerchant()
        {
            var merchant = World.InRadius<Humanoid>(_player.Position, TradeRadius)
                .FirstOrDefault(H => H.SearchComponent<TradeComponent>() != null);
            if (merchant != null) Trade(merchant);
        }
    }
}