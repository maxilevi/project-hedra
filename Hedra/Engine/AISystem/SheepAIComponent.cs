using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;

namespace Hedra.Engine.AISystem
{
    public class SheepAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected HerdBehaviour Herd { get; }

        public SheepAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 18f,
                Sound = SoundType.Sheep
            };
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
