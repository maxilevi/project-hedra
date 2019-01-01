using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.QuestSystem.Designs;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public static class QuestPool
    {
        private static readonly List<QuestDesign> QuestDesigns = new List<QuestDesign>
        {
            new CollectDesign(),
            new SpeakDesign()
        };
        
        public static QuestDesign Grab(QuestTier Tier = QuestTier.Any)
        {
            return QuestDesigns.First(D => (D.Tier == Tier || Tier == QuestTier.Any) && Tier != QuestTier.Auxiliary);
        }

        public static QuestDesign Grab(string Name)
        {
            return QuestDesigns.First(D => D.Name == Name);
        }

        public static bool Exists(string Name)
        {
            return QuestDesigns.Any(D => D.Name == Name);
        }
        
        public static QuestDesign[] Designs => QuestDesigns.ToArray();
    }
}