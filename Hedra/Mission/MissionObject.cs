using System;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using Hedra.Mission.Blocks;
using System.Numerics;
using Hedra.Engine.QuestSystem;

namespace Hedra.Mission
{
    public class MissionObject
    {
        public event OnMissionStart MissionStart;
        public event OnMissionDispose MissionDispose;
        public event OnMissionEnd MissionEnd;
        public bool Disposed { get; set; }
        public string QuestType { get; set; }
        public Func<bool> FailWhen;
        private readonly MissionBlock[] _blocks;
        private int _index;
        private IPlayer _owner;
        private IHumanoid _giver;
        private QuestView _view;
        private readonly MissionSettings _settings;
        private bool _abandoned;

        public MissionObject(MissionBlock[] Blocks, DialogObject Dialog, MissionSettings Settings)
        {
            OpeningDialog = Dialog;
            _settings = Settings;
            _blocks = Blocks;
            _index = -1;
        }

        public SerializedQuest Serialize()
        {
            return new SerializedQuest
            {
                Name = _settings.Name,
                GiverName = _giver.Name,
                GivenPosition = _giver.Position
            };
        }

        public bool CanSave => _settings.CanSave;
        public bool IsStoryline => _settings.IsStoryline;
        public DialogObject OpeningDialog { get; }
        public bool HasLocation => Current.HasLocation;
        public Vector3 Location => Current.Location;
        public string Description => Current.Description;
        public string ShortDescription => Current.ShortDescription;
        public QuestView View => _view;
        public bool IsCompleted => Current.IsCompleted;
        public bool ShowPlaque => true;

        public void Abandon()
        {
            _abandoned = true;
            Dispose();
        }

        public void CleanupAndAdvance()
        {
            Current.Cleanup();
            if(HasNext && !_abandoned)
                _owner.Questing.Start(_giver, this);
            else
            {
                MissionEnd?.Invoke();
                Dispose();
            }
        }

        public void Start(IHumanoid Giver, IPlayer Player)
        {
            _owner = Player;
            _giver = Giver;
            Next();
        }

        public void Update()
        {
            if(FailWhen != null && FailWhen() || Current.IsFailed)
                _owner.Questing.Fail(this);
            Current?.Update();
        }

        private void Next()
        {
            if(_index == -1)
                MissionStart?.Invoke();
            var previous = Current;
            _index++;
            if (Current != null)
            {
                Current.Owner = _owner;
                Current.Giver = _giver;
                Current.Setup();
                previous?.Dispose();
                _view?.Dispose();
                _view = Current.BuildView();
                Current.Start();
            }
        }

        public bool HasNext => _index < _blocks.Length - 1;

        public MissionBlock Current => _index < _blocks.Length && _index >= 0 ? _blocks[_index] : null;
        public IPlayer Owner => _owner;

        public void Dispose()
        {
            Disposed = true;
            for (var i = 0; i < _blocks.Length; ++i)
            {
                _blocks[i].Dispose();
            }
            MissionDispose?.Invoke();
        }
    }
}