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
        }
        
        protected override float AlertTime => 8 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.Sheep;
    }
}