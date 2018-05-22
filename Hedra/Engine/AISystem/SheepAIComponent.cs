using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class SheepAIComponent : BaseAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected HerdBehaviour Herd { get; }

        public SheepAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent);
            Herd = new HerdBehaviour(Parent);
        }

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
