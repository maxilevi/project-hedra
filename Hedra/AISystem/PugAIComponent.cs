using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class PugAIComponent : CattleAIComponent
    {
        public PugAIComponent(IEntity Parent) : base(Parent)
        {
        }

        protected override float AlertTime => 6 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.None;
        protected override float Radius => 120;
    }
}