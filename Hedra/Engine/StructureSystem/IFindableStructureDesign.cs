using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public interface IFindableStructureDesign
    {
        string DisplayName { get; }
        VertexData Icon { get; }
        int PlateauRadius { get; }
        bool IsCompletable { get; }
    }
}