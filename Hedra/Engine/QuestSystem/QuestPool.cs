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
            new SpawnQuestDesign(),
            new FindOverworldStructureDesign()
        };
        
        public static QuestDesign Random(Vector3 Position, QuestTier Tier = QuestTier.Any)
        {
            var possibilities = QuestDesigns.Where(D => D.Tier == Tier || Tier == QuestTier.Any).Where(Q => Q.IsAvailable(Position)).ToArray();
            return possibilities[Utils.Rng.Next(0, possibilities.Length)];
        }

        public static QuestDesign Grab(string Name)
        {
            return QuestDesigns.First(D => D.Name == Name);
        }
        
        public static QuestDesign Grab(Quests Name)
        {
            return Grab(Name.ToString());
        }

        public static bool Exists(string Name)
        {
            return QuestDesigns.Any(D => D.Name == Name);
        }
        
        public static QuestDesign[] Designs => QuestDesigns.ToArray();
    }
}