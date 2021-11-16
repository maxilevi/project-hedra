using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem
{
    public class FriendlyAIComponent : BasicAIComponent
    {
        public FriendlyAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
        }

        protected RoamBehaviour Roam { get; }

        public override AIType Type => AIType.Friendly;

        public override void Update()
        {
            Roam.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
            Roam.Dispose();
        }
    }
}