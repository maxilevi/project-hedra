using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem
{
    public abstract class SimpleCompletableStructureDesign<T> : SimpleFindableStructureDesign<T>, ICompletableStructureDesign where T : BaseStructure, ICompletableStructure
    {
        protected abstract string GetDescription(T Structure);
        protected abstract string GetShortDescription(T Structure);
        public string GetDescription(IStructure Structure) => GetDescription((T) Structure);
        public string GetShortDescription(IStructure Structure) => GetShortDescription((T) Structure);
    }
}