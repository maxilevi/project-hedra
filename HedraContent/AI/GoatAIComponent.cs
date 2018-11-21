using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;

namespace HedraContent.AI
{
    public class GoatAIComponent : NeutralAIComponent
    {
        public GoatAIComponent(Entity Parent) : base(Parent)
        {
            Roam.Sound = SoundType.Goat;
        }
    }
}