using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.QuestSystem
{
    public class QuestInventory
    {
        private readonly List<QuestObject> _activeQuests;

        public QuestInventory()
        {
            _activeQuests = new List<QuestObject>();
        }
        
        public void Start(QuestObject Quest)
        {
            _activeQuests.Add(Quest);
        }

        public void SetQuests(QuestObject[] Quests)
        {
            _activeQuests.Clear();
            _activeQuests.AddRange(Quests.ToList());
        }

        public void Empty()
        {
            _activeQuests.Clear();
        }

        public QuestObject[] ActiveQuests => _activeQuests.ToArray();
    }
}