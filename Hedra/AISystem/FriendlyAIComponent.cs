using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem
{
    public class FriendlyAIComponent : BasicAIComponent
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
        
        public override AIType Type => AIType.Friendly;
        
        public override void Dispose()
        {
            base.Dispose();
            Roam.Dispose();
        }
    }
}
