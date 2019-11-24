using Hedra.Engine.Player;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Mission
{
    public interface IMissionDesign
    {
        MissionObject Build(Vector3 Position, IHumanoid Giver, IPlayer Owner);
        bool CanGive(Vector3 Position);
        QuestTier Tier { get; }
        QuestHint Hint { get; }
        string Name { get; }
    }
}