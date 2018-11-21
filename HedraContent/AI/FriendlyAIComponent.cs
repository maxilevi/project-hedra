using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace HedraContent.AI
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
    }
}
