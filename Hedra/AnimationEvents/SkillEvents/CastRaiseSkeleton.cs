using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage.Necromancer;

namespace Hedra.AnimationEvents.SkillEvents
{
    public class CastRaiseSkeleton : SkillAnimationEvent<RaiseSkeleton>
    {
        public CastRaiseSkeleton(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }
    }
}