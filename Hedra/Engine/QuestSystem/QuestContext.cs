using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class QuestContext
    {
        public QuestContext(Vector3 Position)
        {
            ContextType = SelectContextType(Position);
        }
        
        public QuestContext(QuestContextType Type)
        {
            ContextType = Type;
        }
        

        private static QuestContextType SelectContextType(Vector3 Position)
        {
            if (World.InRadius<Village>(Position, VillageDesign.MaxVillageRadius).Length > 0)
                return QuestContextType.Village;
            if (World.InRadius<SpawnCampfire>(Position, SpawnCampfireDesign.MaxRadius).Length > 0)
                return QuestContextType.Spawn;
            return QuestContextType.Wilderness;
        }
        
        public QuestContextType ContextType { get; }
    }

    public enum QuestContextType
    {
        Spawn,
        Village,
        Wilderness
    }
}