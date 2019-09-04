using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Mission
{
    public interface IMissionDesign
    {
        MissionObject Build(Vector3 Position, IHumanoid Giver, IPlayer Owner);
        bool CanGive(Vector3 Position);
        QuestTier Tier { get; }
        string Name { get; }
    }
}