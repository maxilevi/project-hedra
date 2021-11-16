using System;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Mission.Blocks;

namespace Hedra.Mission
{
    public class MissionObject
    {
        private readonly MissionBlock[] _blocks;
        private readonly MissionSettings _settings;
        private bool _abandoned;
        private IHumanoid _giver;
        private int _index;
        public Func<bool> FailWhen;

        public MissionObject(MissionBlock[] Blocks, DialogObject Dialog, MissionSettings Settings)
        {
            OpeningDialog = Dialog;
            _settings = Settings;
            _blocks = Blocks;
            _index = -1;
        }

        public bool Disposed { get; set; }
        public string QuestType { get; set; }

        public bool CanSave => _settings.CanSave;
        public bool IsStoryline => _settings.IsStoryline;
        public DialogObject OpeningDialog { get; }
        public bool HasLocation => Current.HasLocation;
        public Vector3 Location => Current.Location;
        public string Description => Current.Description;
        public string ShortDescription => Current.ShortDescription;
        public QuestView View { get; private set; }

        public bool IsCompleted => Current.IsCompleted;
        public bool ShowPlaque => true;

        public bool HasNext => _index < _blocks.Length - 1;

        public MissionBlock Current => _index < _blocks.Length && _index >= 0 ? _blocks[_index] : null;
        public IPlayer Owner { get; private set; }

        public event OnMissionStart MissionStart;
        public event OnMissionDispose MissionDispose;
        public event OnMissionEnd MissionEnd;

        public SerializedQuest Serialize()
        {
            return new SerializedQuest
            {
                Name = _settings.Name,
                GiverName = _giver.Name,
                GivenPosition = _giver.Position
            };
        }

        public void Abandon()
        {
            _abandoned = true;
            Dispose();
        }

        public void CleanupAndAdvance()
        {
            Current.Cleanup();
            if (HasNext && !_abandoned)
            {
                Owner.Questing.Start(_giver, this);
            }
            else
            {
                MissionEnd?.Invoke();
                Dispose();
            }
        }

        public void Start(IHumanoid Giver, IPlayer Player)
        {
            Owner = Player;
            _giver = Giver;
            Next();
        }

        public void Update()
        {
            if (FailWhen != null && FailWhen() || Current.IsFailed)
                Owner.Questing.Fail(this);
            Current?.Update();
        }

        private void Next()
        {
            if (_index == -1)
                MissionStart?.Invoke();
            var previous = Current;
            _index++;
            if (Current != null)
            {
                Current.Owner = Owner;
                Current.Giver = _giver;
                Current.Setup();
                previous?.Dispose();
                View?.Dispose();
                View = Current.BuildView();
                Current.Start();
            }
        }

        public void Dispose()
        {
            Disposed = true;
            for (var i = 0; i < _blocks.Length; ++i) _blocks[i].Dispose();
            MissionDispose?.Invoke();
        }
    }
}