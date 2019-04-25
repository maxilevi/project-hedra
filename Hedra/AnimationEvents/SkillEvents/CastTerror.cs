using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage.Necromancer;

namespace Hedra.AnimationEvents.SkillEvents
{
    public class CastTerror : SkillAnimationEvent<Terror>
    {
        public CastTerror(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }
    }
}