using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.AISystem
{
    internal class FriendlyAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }

        public FriendlyAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
        }

        public override void Update()
        {
            Roam.Update();
        }
    }
}
