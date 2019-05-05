using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public abstract class FindableStructureDesign : StructureDesign, IFindableStructureDesign
    {
        public abstract string DisplayName { get; }
        public abstract VertexData QuestIcon { get; }
        public bool IsCompletable => false;
    }
}