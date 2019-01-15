using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class PugAIComponent : CattleAIComponent
    {
        public PugAIComponent(IEntity Parent) : base(Parent)
        {
            AlertTime = 6 + Utils.Rng.NextFloat() * 12f;
        }
        
        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.None;
        protected override float Radius => 120;
    }
}