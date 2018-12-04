using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class PigAIComponent : CattleAIComponent
    {
        public PigAIComponent(Entity Parent) : base(Parent)
        {
        }
        
        protected override float AlertTime => 22;
        protected override SoundType Sound => SoundType.None;
    }
}