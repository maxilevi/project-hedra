using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;

namespace Hedra.AISystem
{
    public class PugAIComponent : CattleAIComponent
    {
        public PugAIComponent(Entity Parent) : base(Parent)
        {
        }
        
        protected override float AlertTime => 12;
        protected override SoundType Sound => SoundType.None;
    }
}