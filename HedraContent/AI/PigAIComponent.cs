using Hedra.AISystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;

namespace HedraContent.AI
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