using Hedra.Engine.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem.Mob
{
    public class GoatAIComponent : NeutralAIComponent
    {
        public GoatAIComponent(Entity Parent) : base(Parent)
        {
            Roam.Sound = SoundType.Goat;
        }
    }
}