using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public abstract class AIState
    {
        protected AIState(Entity Parent)
        {
            this.Parent = Parent;
        }

        protected Entity Parent { get; set; }
        public abstract void Execute();
        public abstract void SetParameters(params object[] Parameters);
    }
}
