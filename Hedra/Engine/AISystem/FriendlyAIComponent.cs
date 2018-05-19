using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.AISystem
{
    public class FriendlyAIComponent : BaseAIComponent
    {
        public override AIType Type => AIType.Friendly;
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
