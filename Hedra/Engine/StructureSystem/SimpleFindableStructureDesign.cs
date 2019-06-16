using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public abstract class SimpleFindableStructureDesign<T> : SimpleStructureDesign<T>, IFindableStructureDesign where T : BaseStructure
    {
        public abstract string DisplayName { get; }
        public abstract VertexData QuestIcon { get; }
        public bool IsCompletable => this is ICompletableStructureDesign;
    }
}