using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage.Necromancer;

namespace Hedra.AnimationEvents.SkillEvents
{
    public class SpawnSkeleton : SkillAnimationEvent<RaiseSkeleton>
    {
        public SpawnSkeleton(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }
    }
}