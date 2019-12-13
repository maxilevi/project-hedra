using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public delegate void OnQuestChanged(MissionObject Object);
    
    public class QuestInventory
    {
        public event OnQuestChanged QuestFailed;
        public event OnQuestChanged QuestAccepted;
        public event OnQuestChanged QuestCompleted;
        public event OnQuestChanged QuestAbandoned;
        public event OnQuestChanged QuestLoaded;
        private readonly IPlayer _player;
        private readonly List<MissionObject> _activeQuests;
        private int _storylineQuestCompleted;

        public QuestInventory(IPlayer Player)
        {
            _player = Player;
            _activeQuests = new List<MissionObject>();
            _player.Inventory.InventoryUpdated += CheckForCompleteness;
            _player.StructureAware.StructureEnter += _ => CheckForCompleteness();
            _player.StructureAware.StructureCompleted += _ => CheckForCompleteness();
            _player.StructureAware.StructureLeave += _ => CheckForCompleteness();
            _player.Kill += _ => CheckForCompleteness();
            _player.OnInteract += CheckForCompleteness;
            _player.OnMove += CheckForCompleteness;
        }
        
        public void Start(IHumanoid Giver, MissionObject Quest)
        {
            /* Don't give storyline quest if player already has it */
            if(_activeQuests.Any(Q => Q.IsStoryline)) return;
            Quest.Start(Giver, _player);
            _activeQuests.Insert(0, Quest);
            QuestAccepted?.Invoke(Quest);
            CheckForCompleteness();
        }

        public bool Has(MissionObject Mission)
        {
            return _activeQuests.Contains(Mission);
        }

        public void Update()
        {
            for (var i = 0; i < _activeQuests.Count; ++i)
            {
                _activeQuests[i].Update();
            }
        }

        public void Trigger()
        {
            CheckForCompleteness();
        }
        
        private void CheckForCompleteness()
        {
            for (var i = _activeQuests.Count-1; i > -1; --i)
            {
                if (_activeQuests[i].IsCompleted)
                {
                    var quest = _activeQuests[i];
                    _activeQuests.RemoveAt(i);
                    QuestCompleted?.Invoke(quest);
                    quest.CleanupAndAdvance();
                }
            }
        }

        public void SetSerializedQuests(SerializedQuest[] Quests)
        {
            Empty();
            for (var i = 0; i < Quests.Length; ++i)
            {
                var design = MissionPool.Grab(Quests[i].Name);
                var entity = new Humanoid
                {
                    Name = Quests[i].GiverName,
                    Position = Quests[i].GivenPosition
                };
                var quest = design.Build(entity.Position, entity, _player);
                this.Start(entity, quest);
                entity.Dispose();
            }
            //_storylineQuestCompleted
        }
        
        public SerializedQuest[] GetSerializedQuests()
        {
            var list = new List<SerializedQuest>();
            list.AddRange(_activeQuests.Where(Q => Q.CanSave).Select(Q => Q.Serialize()));
            //list.Add(SerializedQuest.From);
            return list.ToArray();
        }
        
        public void Abandon(MissionObject Object)
        {
            Object.Abandon();
            _activeQuests.Remove(Object);
            QuestAbandoned?.Invoke(Object);
        }

        public void Fail(MissionObject Object)
        {
            Object.Abandon();
            _activeQuests.Remove(Object);
            QuestFailed?.Invoke(Object);
        }
        
        public void Empty()
        {
            _activeQuests.Clear();
        }

        public MissionObject[] ActiveQuests => _activeQuests.ToArray();

        public bool StartedStoryline => _activeQuests.Any(Q => Q.IsStoryline);
    }
}