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
        public event OnMissionEnd MissionEnd;
        private readonly MissionBlock[] _blocks;
        private int _index;
        private IPlayer _owner;
        private IHumanoid _giver;
        private QuestView _view;
        private readonly MissionSettings _settings;
        
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
                GiverName = _giver.Name
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

        }

        public void CleanupAndAdvance()
        {
            Current.Cleanup();
            if(HasNext)
                _owner.Questing.Start(_giver, this);
        }

        public void Start(IHumanoid Giver, IPlayer Player)
        {
            _owner = Player;
            _giver = Giver;
            Next();
        }

        private void Next()
        {
            _index++;
            if (Current != null)
            {
                Current.Owner = _owner;
                Current.Giver = _giver;
                Current.Setup();
                _view?.Dispose();
                _view = Current.BuildView();
            }
            else
            {
                MissionEnd?.Invoke();
            }
        }

        private bool HasNext => _index < _blocks.Length - 1;

        public MissionBlock Current => _index < _blocks.Length ? _blocks[_index] : null;
        public IPlayer Owner => _owner;

        public void Dispose()
        {
            for (var i = 0; i < _blocks.Length; ++i)
            {
                _blocks[i].Dispose();
            }
        }
    }
}