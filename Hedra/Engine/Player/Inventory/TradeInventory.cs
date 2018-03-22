/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/08/2017
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
	/// <summary>
	/// Description of TradeSystem.
	/// </summary>
	public class TradeInventory
	{
		public const int MerchantSpaces = 20;
        public bool IsTrading { get; private set; }
		private readonly LocalPlayer _player;
	    private readonly InventoryArray _merchantItems;
	    private readonly InventoryArray _playerItems;
	    private readonly InventoryArrayInterface _playerItemsInterface;
	    private readonly InventoryArrayInterface _merchantItemsInterface;
	    private readonly InventoryStateManager _stateManager;
	    private readonly InventoryArrayInterfaceManager _interfaceManager;
	    private readonly InventoryBackground _playerBackground;
	    private readonly InventoryBackground _merchantBackground;
	    private MerchantComponent _merchantComponent;
	    private Entity _trader;
        private bool _show;

        public TradeInventory(LocalPlayer Player){
            _player = Player;
            _merchantItems = new InventoryArray(MerchantSpaces);
            _playerItems = new InventoryArray(PlayerInventory.InventorySpaces);
            _stateManager = new InventoryStateManager(_player);
            _playerItemsInterface = new InventoryArrayInterface(_playerItems, 0, _playerItems.Length, 5)
            {
                Position = Vector2.UnitX * .5f + Vector2.UnitY * -.1f
            };
            _merchantItemsInterface = new InventoryArrayInterface(_merchantItems, 0, _merchantItems.Length, 5)
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
            EventDispatcher.RegisterKeyDown(this, delegate (object Sender, KeyboardKeyEventArgs Args)
            {
                if (Args.Key == Key.Escape)
                    this.Cancel();
            });
        }

	    public void Trade(Entity Trader)
	    {
	        IsTrading = true;
	        _trader = Trader;
	        _merchantComponent = Trader.SearchComponent<MerchantComponent>();
	        Show = true;
	        _playerItems.SetItems(_player.Inventory.MainItemsToArray());
	        _merchantItems.SetItems(_merchantComponent.Items.ToArray());
            this.UpdateView();
        }

	    public void Cancel()
	    {
	        Show = false;
            IsTrading = false;
	        //_trader = null;
            _merchantComponent = null;
	        _playerItems.Empty();
	        _merchantItems.Empty();
	        this.UpdateView();
	    }

	    public void UpdateView()
	    {
	        _playerItemsInterface.UpdateView();
            _merchantItemsInterface.UpdateView();
	    }

        public void UpdateInventory()
        {
            _interfaceManager.UpdateView();
        }

        public void ClearInventory()
        {
            _merchantItems.Empty();
            _playerItems.Empty();
        }

        private void SetInventoryState(bool State)
        {
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.Check = false;
                _player.View.Check = false;
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
                _player.View.Pitch = Mathf.Lerp(_player.View.Pitch, 0f, (float)Time.deltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float)Time.deltaTime * 16f);
                _player.View.Yaw = Mathf.Lerp(_player.View.Yaw, (float)Math.Acos(-_player.Orientation.X),
                    (float)Time.deltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    (float)Time.deltaTime * 16f);
                _playerBackground.UpdateView(_player);
                _merchantBackground.UpdateView(_trader as Humanoid);
            }
        }

        public bool Show
        {
            get { return _show; }
            private set
            {
                if (_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                _merchantItemsInterface.Enabled = _show;
                _playerItemsInterface.Enabled = _show;
                _interfaceManager.Enabled = _show;
                _playerBackground.Enabled = _show;
                _merchantBackground.Enabled = _show;
                this.UpdateInventory();
                this.SetInventoryState(_show);
            }
        }
    }
}
