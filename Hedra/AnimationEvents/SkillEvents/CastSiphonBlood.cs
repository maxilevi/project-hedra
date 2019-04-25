using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage.Necromancer;

namespace Hedra.AnimationEvents.SkillEvents
{
    public class CastSiphonBlood : SkillAnimationEvent<SiphonBlood>
    {
        public CastSiphonBlood(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }
    }
}