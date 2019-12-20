using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem.Mob
{
    public class CowAIComponent : FeedableCattleAIComponent
    {
        public CowAIComponent(IEntity Parent) : base(Parent)
        {
        }

        protected override float AlertTime => 8 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.None;
    }
}