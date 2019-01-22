using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class CowAIComponent : CattleAIComponent
    {
        public CowAIComponent(IEntity Parent) : base(Parent)
        {
        }

        protected override float AlertTime => 8 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.None;
    }
}