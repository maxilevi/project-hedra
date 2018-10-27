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
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    /// <summary>
    /// Description of TradeSystem.
    /// </summary>
    public class TradeInventory : PlayerInterface
    {
        public const int MerchantSpaces = 20;
        public const int TradeRadius = 12;
        public bool IsTrading { get; private set; }
        private readonly LocalPlayer _player;
        private readonly InventoryArray _merchantItems;
        private readonly InventoryArray _playerItems;
        private readonly InventoryArrayInterface _playerItemsInterface;
        private readonly InventoryArrayInterface _merchantItemsInterface;
        private readonly InventoryStateManager _stateManager;
        private readonly TradeInventoryArrayInterfaceManager _interfaceManager;
        private readonly InventoryBackground _playerBackground;
        private readonly InventoryBackground _merchantBackground;
        private TradeComponent _tradeComponent;
        private Humanoid _trader;
        private bool _show;

        public TradeInventory(LocalPlayer Player){
            _player = Player;
            _merchantItems = new InventoryArray(MerchantSpaces);
            _playerItems = new InventoryArray(PlayerInventory.InventorySpaces);
            _stateManager = new InventoryStateManager(_player);
            _playerItemsInterface = new InventoryArrayInterface(_playerItems, 0, _playerItems.Length, 5, Vector2.One)
            {
                Position = Vector2.UnitX * .5f + Vector2.UnitY * -.1f
            };
            _merchantItemsInterface = new InventoryArrayInterface(_merchantItems, 0, _merchantItems.Length, 5, Vector2.One)
            {
                Position = Vector2.UnitX * -.5f + Vector2.UnitY * -.1f
            };
            var itemInfoInterface = new TradeInventoryInterfaceItemInfo(_playerItemsInterface.Renderer)
            {
                Position = Vector2.UnitY * .1f
            };
            _interfaceManager = new TradeInventoryArrayInterfaceManager(itemInfoInterface, _playerItemsInterface, _merchantItemsInterface);
            _playerBackground = new InventoryBackground(Vector2.UnitX * .5f + Vector2.UnitY * .55f);
            _merchantBackground = new InventoryBackground(Vector2.UnitX * -.5f + Vector2.UnitY * .55f);
            _interfaceManager.OnTransactionComplete += (Item, Price) => this.UpdateTraders();
            _stateManager.OnStateChange += State =>
            {
                base.Invoke(State);
            };
        }

        public void Trade(Humanoid Trader)
        {
            IsTrading = true;
            _trader = Trader;
            _tradeComponent = Trader.SearchComponent<TradeComponent>();
            this.SetActive(true);
            _playerItems.SetItems(_player.Inventory.ItemsToArray());
            _merchantItems.SetItems(_tradeComponent.Items.ToArray());
            this._interfaceManager.SetTraders(_player, _trader);
            this.UpdateView();
        }

        public void Cancel()
        {
            this._interfaceManager.SetTraders(null, null);
            this.SetActive(false);
            IsTrading = false;
            _tradeComponent = null;
            _playerItems.Empty();
            _merchantItems.Empty();
            this.UpdateView();
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
            this.UpdateView();
        }

        public void ClearInventory()
        {
            _merchantItems.Empty();
            _playerItems.Empty();
        }

        public void UpdateTraders()
        {
            for (var i = 0; i < _playerItems.Length; i++)
            {
                if(_player.Inventory[i]?.IsGold ?? false) continue;

                _player.Inventory.SetItem(i, null);
                _player.Inventory.SetItem(i, _playerItems[i]);
            }

            _tradeComponent.Items.Clear();
            for (var i = 0; i < _merchantItems.Length; i++)
            {
                _tradeComponent.Items.Add(i, _merchantItems[i]);
            }
            _tradeComponent.TransactionComplete();
            this.UpdateInventory();
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
                UpdateManager.CursorShown = true;
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
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, (float)Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float)Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw, (float)Math.Acos(-_player.Orientation.X),
                    (float)Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    (float)Time.DeltaTime * 16f);
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
            this.UpdateInventory();
            this.SetInventoryState(_show);
        }

        private void TradeWithNearestMerchant()
        {
            var merchant = World.InRadius<Humanoid>(_player.Position, TradeRadius).FirstOrDefault(H => H.SearchComponent<TradeComponent>() != null);
            if(merchant != null) this.Trade(merchant);
        }

        public override Key OpeningKey => Key.E;

        public override bool Show
        {
            get { return _show; }
            set
            {
                if (value) this.TradeWithNearestMerchant();
                else this.Cancel();
            }
        }
    }
}
