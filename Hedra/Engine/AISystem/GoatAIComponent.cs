using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;

namespace Hedra.Engine.AISystem
{
    public class GoatAIComponent : NeutralAIComponent
    {
        public GoatAIComponent(Entity Parent) : base(Parent)
        {
            Roam.Sound = SoundType.Goat;
        }
    }
}