using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;
using Hedra.Game;

namespace Hedra.AISystem
{
    public class HostileAIComponent : BasicAIComponent
    {
        public HostileAIComponent(IEntity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
        }

        protected RoamBehaviour Roam { get; }
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public override AIType Type => AIType.Hostile;

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Hostile.Update();
                if (!Hostile.Enabled) Roam.Update();
            }
        }

        public override void Draw()
        {
            if (GameSettings.DebugAI)
            {
                DrawDebugCollision();
                if (Retaliate.Enabled)
                    Retaliate.Draw();
                else if (Hostile.Enabled)
                    Hostile.Draw();
            }
        }

        public void SetTarget(IEntity Entity)
        {
            Hostile.SetTarget(Entity);
        }

        public override void Dispose()
        {
            base.Dispose();
            Roam.Dispose();
            Hostile.Dispose();
            Retaliate.Dispose();
        }
    }
}