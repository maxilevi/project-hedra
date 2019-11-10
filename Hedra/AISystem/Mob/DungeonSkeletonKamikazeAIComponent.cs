using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class DungeonSkeletonKamikazeAIComponent : SkeletonKamikazeAIComponent
    {
        public DungeonSkeletonKamikazeAIComponent(Entity Parent) : base(Parent)
        {
            AlterBehaviour<HostileBehaviour>(new DungeonHostileBehaviour(Parent));
            AlterBehaviour<RoamBehaviour>(new DungeonRoamBehaviour(Parent));
            AlterBehaviour<TraverseBehaviour>(new DungeonTraverseBehaviour(Parent));
        }
    }
}