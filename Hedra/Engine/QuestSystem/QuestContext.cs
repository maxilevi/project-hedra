using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class QuestContext
    {
        public QuestContext(Vector3 Position)
        {
            this.Position = Position;
        }
        
        public Vector3 Position { get; }
    }

    public enum QuestContextType
    {
        Spawn,
        Village,
        Wilderness
    }
}