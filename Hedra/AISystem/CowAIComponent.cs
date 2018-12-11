using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class CowAIComponent : CattleAIComponent
    {
        public CowAIComponent(IEntity Parent) : base(Parent)
        {
            AlertTime = 8 + Utils.Rng.NextFloat() * 12f;
        }

        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.None;
    }
}