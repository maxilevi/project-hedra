using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public class GoatAIComponent : NeutralAIComponent
    {
        public GoatAIComponent(Entity Parent) : base(Parent)
        {
            Roam.Sound = SoundType.Goat;
        }
    }
}