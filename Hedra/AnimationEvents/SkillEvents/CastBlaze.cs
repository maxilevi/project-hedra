using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage;

namespace Hedra.AnimationEvents.SkillEvents
{
    public class CastBlaze : SkillAnimationEvent<Blaze>
    {
        public CastBlaze(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }
    }
}