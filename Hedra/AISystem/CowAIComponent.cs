using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class CowAIComponent : CattleAIComponent
    {
        public CowAIComponent(Entity Parent) : base(Parent)
        {
        }

        protected override float AlertTime => 18;
        protected override SoundType Sound => SoundType.None;
    }
}