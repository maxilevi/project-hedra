using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage.Necromancer;

namespace Hedra.AnimationEvents.SkillEvents
{
    public class CastLeech : SkillAnimationEvent<Leech>
    {
        public CastLeech(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }
    }
}