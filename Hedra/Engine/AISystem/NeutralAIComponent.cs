using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class NeutralAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected RetaliateBehaviour Retaliate { get; }

        public NeutralAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
            Retaliate = new RetaliateBehaviour(Parent);
        }

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Roam.Update();
            }
        }
    }
}
