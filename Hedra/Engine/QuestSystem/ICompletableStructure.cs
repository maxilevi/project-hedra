using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.QuestSystem
{
    public interface ICompletableStructure : IStructure
    {
        bool Completed { get; }
        ItemDescription DeliveryItem { get; }
        QuestReward Reward { get; }
    }
}