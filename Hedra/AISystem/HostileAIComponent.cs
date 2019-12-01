using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Game;
using System.Numerics;

namespace Hedra.AISystem
{
    public class HostileAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public HostileAIComponent(IEntity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
        }

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Hostile.Update();
                if (!Hostile.Enabled)
                {
                    Roam.Update();
                }
            }
        }
        
        public override void Draw()
        {
            if (GameSettings.DebugAI)
            {
                DrawDebugCollision();
                if (Retaliate.Enabled)
                    Retaliate.Draw();
                else if(Hostile.Enabled)
                    Hostile.Draw();
            }
        }
        
        public void SetTarget(IEntity Entity)
        {
            Hostile.SetTarget(Entity);
        }

        public override AIType Type => AIType.Hostile;
    }
}
