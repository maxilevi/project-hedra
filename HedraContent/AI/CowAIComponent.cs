using Hedra.AISystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;

namespace HedraContent.AI
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