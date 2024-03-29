using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Input;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Sound;
using Silk.NET.Input;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestInterface : PlayerInterface
    {
        private const int Columns = 5;
        private const int Rows = 1;
        private readonly IPlayer _player;
        private readonly QuestingJournal _questItemInterface;
        private readonly InventoryStateManager _stateManager;
        private bool _show;

        public QuestInterface(IPlayer Player)
        {
            _player = Player;
            _stateManager = new InventoryStateManager(_player);
            _questItemInterface = new QuestingJournal(_player)
            {
                Position = Vector2.UnitX * -.5f
            };
            _stateManager.OnStateChange += Invoke;
            _player.Questing.QuestAccepted += O =>
            {
                if (!O.ShowPlaque) return;
                _player.MessageDispatcher.ShowPlaque(
                    $"{Translations.Get("new_quest")}{Environment.NewLine}{O.ShortDescription}", 1f
                );
            };
            _player.Questing.QuestCompleted += O =>
            {
                if (!O.ShowPlaque) return;
                _player.MessageDispatcher.ShowPlaque(
                    $"{Translations.Get("quest_completed")}{Environment.NewLine}{O.ShortDescription}", 1f
                );
            };
            _player.Questing.QuestAbandoned += O =>
            {
                if (!O.ShowPlaque) return;
                _player.MessageDispatcher.ShowPlaque(
                    $"{Translations.Get("quest_abandoned")}{Environment.NewLine}{O.ShortDescription}", 1f, false
                );
            };
            _player.Questing.QuestFailed += O =>
            {
                if (!O.ShowPlaque) return;
                _player.MessageDispatcher.ShowPlaque(
                    $"{Translations.Get("quest_failed")}{Environment.NewLine}{O.ShortDescription}", 1f, false
                );
            };
        }

        public override Key OpeningKey => Controls.QuestLog;

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

        protected override bool HasExitAnimation => true;

        private void UpdateView()
        {
            _questItemInterface.UpdateView();
        }

        private void SetInventoryState(bool State)
        {
            _questItemInterface.Enabled = State;

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
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw,
                    (float)Math.Atan2(-_player.Orientation.Z, -_player.Orientation.X),
                    Time.DeltaTime * 16f);
            }
        }

        public void Reset()
        {
            _questItemInterface.Reset();
        }
    }
}