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
        public event OnQuestChanged QuestAccepted;
        public event OnQuestChanged QuestCompleted;
        public event OnQuestChanged QuestAbandoned;
        public event OnQuestChanged QuestLoaded;
        private readonly IPlayer _player;
        private readonly List<MissionObject> _activeQuests;

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
                    Name = Quests[i].GiverName
                };
                var quest = design.Build(_player.Position, entity, _player);
                this.Start(entity, quest);
                entity.Dispose();
            }
            /*
            _activeQuests.Clear();
            _activeQuests.AddRange(Quests.Select(MissionObject.FromTemplate).ToList());
            _activeQuests.RemoveAll(Q => Q == null);
            _activeQuests.ForEach(Q => Q.Start(_player));
            _activeQuests.ForEach(Q => QuestLoaded?.Invoke(Q));
            */
        }
        
        public SerializedQuest[] GetSerializedQuests()
        {
            return _activeQuests.Where(Q => Q.CanSave).Select(Q => Q.Serialize()).ToArray();
        }
        
        public void Abandon(MissionObject Object)
        {
            Object.Abandon();
            _activeQuests.Remove(Object);
            QuestAbandoned?.Invoke(Object);
        }
        
        public void Empty()
        {
            _activeQuests.Clear();
        }

        public MissionObject[] ActiveQuests => _activeQuests.ToArray();

        public bool StartedStoryline => _activeQuests.Any(Q => Q.IsStoryline);
    }
}