using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Player;

namespace Hedra.Engine.QuestSystem
{
    public delegate void OnQuestChanged(QuestObject Object);
    
    public class QuestInventory
    {
        public event OnQuestChanged QuestAccepted;
        public event OnQuestChanged QuestCompleted;
        public event OnQuestChanged QuestAbandoned;
        private readonly IPlayer _player;
        private readonly List<QuestObject> _activeQuests;

        public QuestInventory(IPlayer Player)
        {
            _player = Player;
            _activeQuests = new List<QuestObject>();
            _player.Inventory.InventoryUpdated += CheckForCompleteness;
        }
        
        public void Start(QuestObject Quest)
        {
            Quest.Start(_player);
            QuestAccepted?.Invoke(Quest);
            _activeQuests.Add(Quest);
            CheckForCompleteness();
        }

        public void SetQuests(QuestObject[] Quests)
        {
            _activeQuests.Clear();
            _activeQuests.AddRange(Quests.ToList());
            _activeQuests.ForEach(Q => Q.Start(_player));
        }

        private void CheckForCompleteness()
        {
            for (var i = 0; i < _activeQuests.Count; ++i)
            {
                if (_activeQuests[i].IsQuestCompleted())
                {
                    _activeQuests[i].Trigger();
                    QuestCompleted?.Invoke(_activeQuests[i]);
                }
            }
        }

        public void Abadon(QuestObject Object)
        {
            _activeQuests.Remove(Object);
            QuestAbandoned?.Invoke(Object);
        }
        
        public void Empty()
        {
            _activeQuests.Clear();
        }

        public QuestObject[] ActiveQuests => _activeQuests.ToArray();
    }
}