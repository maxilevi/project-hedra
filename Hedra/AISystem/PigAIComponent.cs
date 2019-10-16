using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class PigAIComponent : CattleAIComponent
    {
        public PigAIComponent(IEntity Parent) : base(Parent)
        {
        }
        
        protected override float AlertTime => 12 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.None;
    }
}