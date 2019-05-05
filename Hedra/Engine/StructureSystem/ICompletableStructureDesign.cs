using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public interface ICompletableStructureDesign
    {
        string GetShortDescription(IStructure Structure);
        string GetDescription(IStructure Structure);
    }
}