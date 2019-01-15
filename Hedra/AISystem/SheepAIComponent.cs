using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class SheepAIComponent : CattleAIComponent
    {
        public SheepAIComponent(IEntity Parent) : base(Parent)
        {
            AlertTime = 8 + Utils.Rng.NextFloat() * 12f;
        }
        
        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.Sheep;
    }
}