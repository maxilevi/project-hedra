using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public abstract class FindableStructureDesign : StructureDesign, IFindableStructureDesign
    {
        public abstract string DisplayName { get; }
    }
}