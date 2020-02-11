using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.Game;

namespace Hedra.AISystem
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

        public override void Draw()
        {
            base.Draw();
            if (GameSettings.DebugAI)
            {
                DrawDebugCollision();
                if (Retaliate.Enabled)
                    Retaliate.Draw();
                else
                    Roam.Draw();
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            Roam.Dispose();
            Retaliate.Dispose();
        }

        public override AIType Type => AIType.Neutral;
    }
}
