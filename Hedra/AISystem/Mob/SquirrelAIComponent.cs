using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem.Mob
{
    public class SquirrelAIComponent : CattleAIComponent
    {
        public SquirrelAIComponent(IEntity Parent) : base(Parent)
        {
        }

        protected override float AlertTime => 4 + Utils.Rng.NextFloat() * 4f;
        protected override SoundType Sound => SoundType.None;
    }
}