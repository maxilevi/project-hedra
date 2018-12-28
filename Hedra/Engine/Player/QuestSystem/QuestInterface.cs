using System;
using Hedra.Core;
using Hedra.Engine.Input;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform.MacOS;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestInterface : PlayerInterface
    {
        private const int Entries = 5;
        public override Key OpeningKey => Controls.QuestLog;
        private readonly IPlayer _player;
        private bool _show;
        private readonly QuestingInventoryArrayInterface _questItemInterface;
        private readonly InventoryStateManager _stateManager;
        private readonly QuestingInventoryInterfaceManager _interfaceManager;
        private readonly QuestInventoryItemInfo _itemInfo;
        
        public QuestInterface(IPlayer Player)
        {
            _player = Player;
            _stateManager = new InventoryStateManager(_player);
            var interfacePosition = Vector2.UnitX * -.65f;
            _questItemInterface = new QuestingInventoryArrayInterface(_player, new InventoryArray(Entries), Entries, 1)
            {
                Position = interfacePosition,
                Scale = Vector2.One * 1.05f
            };
            _itemInfo = new QuestInventoryItemInfo(_player, _questItemInterface.Renderer)
            {
                Position = Vector2.UnitY * _questItemInterface.Position.Y + interfacePosition.X * -Vector2.UnitX
            };
            _interfaceManager = new QuestingInventoryInterfaceManager(Entries, 1, _itemInfo, _questItemInterface);
            _stateManager.OnStateChange += Invoke;
            
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