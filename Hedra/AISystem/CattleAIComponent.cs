using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public abstract class CattleAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected HerdBehaviour Herd { get; }

        protected CattleAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = AlertTime,
                Sound = Sound
            };
            Herd = new HerdBehaviour(Parent);
        }

        protected abstract float AlertTime { get; }
        protected abstract SoundType Sound { get; }
        
        public override void Update()
        {
            if (!Herd.Enabled)
            {
                Roam.Update();
            }
            else
            {
                Herd.Update();
            }
        }
    }
}
