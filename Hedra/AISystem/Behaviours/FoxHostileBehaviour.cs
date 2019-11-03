using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class FoxHostileBehaviour : HunterHostileBehaviour
    {
        public FoxHostileBehaviour(IEntity Parent) : base(Parent)
        {
        }

        protected override MobType[] HuntTypes => new[]
        {
            MobType.Sheep,
            MobType.Goat
        };
    }
}