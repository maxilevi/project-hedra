using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem
{
    public abstract class CompletableStructureDesign<T> : StructureDesign, ICompletableStructureDesign,
        IFindableStructureDesign where T : BaseStructure, ICompletableStructure
    {
        public string GetDescription(IStructure Structure)
        {
            return GetDescription((T)Structure);
        }

        public string GetShortDescription(IStructure Structure)
        {
            return GetShortDescription((T)Structure);
        }

        public abstract string DisplayName { get; }
        protected abstract string GetShortDescription(T Structure);
        protected abstract string GetDescription(T Structure);
    }
}