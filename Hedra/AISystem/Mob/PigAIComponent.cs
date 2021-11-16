using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem.Mob
{
    public class PigAIComponent : FeedableCattleAIComponent
    {
        public PigAIComponent(IEntity Parent) : base(Parent)
        {
        }

        protected override float AlertTime => 12 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.None;
    }
}