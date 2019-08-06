using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public abstract class CompletableStructureDesign<T> : StructureDesign, ICompletableStructureDesign, IFindableStructureDesign where T : BaseStructure, ICompletableStructure
    {
        public abstract string DisplayName { get; }
        protected abstract string GetShortDescription(T Structure);
        protected abstract string GetDescription(T Structure);
        public string GetDescription(IStructure Structure) => GetDescription((T) Structure);
        public string GetShortDescription(IStructure Structure) => GetShortDescription((T) Structure);
    }
}