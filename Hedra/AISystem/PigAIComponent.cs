using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class PigAIComponent : CattleAIComponent
    {
        public PigAIComponent(IEntity Parent) : base(Parent)
        {
            AlertTime = 12 + Utils.Rng.NextFloat() * 12f;
        }
        
        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.None;
    }
}