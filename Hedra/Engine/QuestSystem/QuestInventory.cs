using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
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
        public StorySettings Story { get; private set; }
        private readonly IPlayer _player;
        private readonly List<MissionObject> _activeQuests;

        public QuestInventory(IPlayer Player)
        {
            _player = Player;
            Story = new StorySettings();
            _activeQuests = new List<MissionObject>();
            _player.Inventory.InventoryUpdated += CheckForCompleteness;
            _player.StructureAware.StructureEnter += _ => CheckForCompleteness();
            _player.StructureAware.StructureCompleted += _ => CheckForCompleteness();
            _player.StructureAware.StructureLeave += _ => CheckForCompleteness();
            _player.Kill += _ => CheckForCompleteness();
            _player.Interact += _ => CheckForCompleteness();
            _player.Physics.OnMove += CheckForCompleteness;
            SkyManager.TimeChange += CheckForCompleteness;
            QuestCompleted += OnQuestCompleted;
        }
        
        public void Start(IHumanoid Giver, MissionObject Quest)
        {
            /* Don't give storyline quest if player already has it */
            if(Quest.IsStoryline && _activeQuests.Any(Q => Q.IsStoryline)) return;
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

        private void OnQuestCompleted(MissionObject Quest)
        {
            if (Quest.IsStoryline && !Quest.HasNext)
                Story.CompletedSteps++;
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
                if (Quests[i].IsMetadata) continue;
                var design = MissionPool.Grab(Quests[i].Name);
                var entity = new Humanoid
                {
                    Name = Quests[i].GiverName,
                    Position = Quests[i].GivenPosition
                };
                var quest = design.Build(entity, _player);
                this.Start(entity, quest);
                entity.Dispose();
            }

            var metadata = Quests.FirstOrDefault(Q => Q.IsMetadata);
            Story = metadata != null ? StorySettings.FromQuest(metadata) : new StorySettings();
        }
        
        public SerializedQuest[] GetSerializedQuests()
        {
            var list = new List<SerializedQuest>();
            list.AddRange(_activeQuests.Where(Q => Q.CanSave).Select(Q => Q.Serialize()));
            list.Add(SerializedQuest.FromStoryline(Story));
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

        public bool HasStoryQuest => _activeQuests.Any(Q => Q.IsStoryline);
    }
}