using System;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Input;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInterface : PlayerInterface
    {
        private const int Columns = 4;
        private const int Rows = 4;
        public override Key OpeningKey => Controls.Crafting;
        private readonly IPlayer _player;       
        private readonly CraftingInventoryArrayInterface _recipesItemInterface;
        private readonly InventoryStateManager _stateManager;
        private readonly CraftingInventoryArrayInterfaceManager _interfaceManager;
        private readonly CraftingInventoryItemInfo _itemInfo;
        private bool _show;
        
        
        public CraftingInterface(IPlayer Player)
        {
            _player = Player;
            _stateManager = new InventoryStateManager(_player);
            var interfacePosition = Vector2.UnitX * -.4f + Vector2.UnitY * -.0f;
            _recipesItemInterface = new CraftingInventoryArrayInterface(_player, new InventoryArray(Columns * Rows), Rows, Columns)
            {
                Position = interfacePosition,
                Scale = Vector2.One * 1.05f
            };
            _itemInfo = new CraftingInventoryItemInfo(_player, _recipesItemInterface.Renderer)
            {
                Position = Vector2.UnitY * _recipesItemInterface.Position.Y + interfacePosition.X * -Vector2.UnitX
            };
            _interfaceManager = new CraftingInventoryArrayInterfaceManager(Rows, Columns, _itemInfo, _recipesItemInterface);
            _stateManager.OnStateChange += Invoke;
        }

        public void Reset()
        {
            _recipesItemInterface.Reset();
        }
        
        private void UpdateView()
        {
            _interfaceManager.UpdateView();
            _recipesItemInterface.UpdateView();
        }

        private void SetInventoryState(bool State)
        {
            _recipesItemInterface.Enabled = State;
            _interfaceManager.Enabled = State;
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
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    Time.DeltaTime * 8f);
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, (float)Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float)Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw, (float)Math.Acos(-_player.Orientation.X),
                    Time.DeltaTime * 16f);
                _itemInfo.Update();
            }
        }

        public override bool Show
        {
            get => _show;
            set
            {
                if (_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                SetInventoryState(_show);
                UpdateView();
                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
            }
        }
    }
}