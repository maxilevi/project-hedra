using System;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Input;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform.MacOS;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestInterface : PlayerInterface
    {
        private const int Columns = 7;
        private const int Rows = 1;
        public override Key OpeningKey => Controls.QuestLog;
        private readonly IPlayer _player;
        private bool _show;
        private readonly QuestingInventoryArrayInterface _questItemInterface;
        private readonly InventoryStateManager _stateManager;
        private readonly QuestingInventoryInterfaceManager _interfaceManager;
        private readonly QuestInventoryItemInfo _itemInfo;
        private readonly QuestDialog _dialog;
        
        public QuestInterface(IPlayer Player)
        {
            _player = Player;
            _stateManager = new InventoryStateManager(_player);
            var interfacePosition = Vector2.UnitX * -.5f;
            _questItemInterface = new QuestingInventoryArrayInterface(_player, new InventoryArray(Columns * Rows), Rows, Columns)
            {
                Position = Vector2.UnitY * -.1f
            };
            _itemInfo = new QuestInventoryItemInfo(_player, _questItemInterface.Renderer)
            {
                Position = Vector2.UnitY * _questItemInterface.Position.Y + interfacePosition.X * -Vector2.UnitX
            };
            _interfaceManager = new QuestingInventoryInterfaceManager(Rows, Columns, _itemInfo, _questItemInterface);
            _stateManager.OnStateChange += Invoke;
            _dialog = new QuestDialog(_player, _questItemInterface.Renderer);
        }

        public void UpdateView()
        {
            _interfaceManager.UpdateView();
            _questItemInterface.UpdateView();
        }

        private void SetInventoryState(bool State)
        {
            _questItemInterface.Enabled = State;
            _interfaceManager.Enabled = State;
            _dialog.Enabled = false;
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
                _player.View.PositionDelegate = Camera.DefaultDelegate;
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
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, (float) Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float) Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw,
                    0,
                    Time.DeltaTime * 16f);
            }
        }

        public void ShowDialog(IHumanoid Humanoid, QuestObject Object, Action Callback)
        {
            MarkAsShown();
            _player.View.PositionDelegate = () => (_player.Position + Humanoid.Position) / 2;
            _questItemInterface.Enabled = false;
            _interfaceManager.Enabled = false;
            _dialog.Show(Object, Callback);
            
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