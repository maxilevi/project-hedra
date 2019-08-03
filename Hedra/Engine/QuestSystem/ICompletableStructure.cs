using Hedra.Engine.WorldBuilding;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public interface ICompletableStructure : IStructure
    {
        bool Completed { get; }
    }
}