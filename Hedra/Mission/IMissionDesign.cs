using Hedra.Engine.Player;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Mission
{
    public interface IMissionDesign
    {
        MissionObject Build(IHumanoid Giver, IPlayer Owner);
        bool CanGive(Vector3 Position);
        QuestTier Tier { get; }
        QuestHint Hint { get; }
        QuestPriority Priority { get; }
        string Name { get; }
        bool IsStoryline { get; }
        bool CanSave { get; }
        IMissionDesign Clone { get; }
        MissionDesignSettings Settings { get; set; }
    }
}