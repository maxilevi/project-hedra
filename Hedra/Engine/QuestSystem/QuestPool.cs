using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.QuestSystem
{
    public static class QuestPool
    {
        private static readonly List<QuestDesign> _designs;
        
        public static QuestObject Grab(QuestTier Tier = QuestTier.Any)
        {
            return _designs.First(D => D.Tier == Tier || Tier == QuestTier.Any).Build();
        }
    }
}