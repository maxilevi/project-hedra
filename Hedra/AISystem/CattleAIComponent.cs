using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public abstract class CattleAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected HerdBehaviour Herd { get; }

        protected CattleAIComponent(IEntity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = AlertTime,
                Sound = Sound,
                Radius = Radius
            };
            Herd = new HerdBehaviour(Parent);
        }

        protected abstract float AlertTime { get; }
        protected abstract SoundType Sound { get; }
        protected virtual float Radius => 80;
        
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
