using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public interface IFindableStructureDesign
    {
        string DisplayName { get; }
        VertexData QuestIcon { get; }
        bool IsCompletable { get; }
    }
}