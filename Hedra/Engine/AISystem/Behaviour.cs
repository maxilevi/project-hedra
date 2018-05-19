using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public abstract class Behaviour
    {
        protected Entity Parent { get; }

        protected Behaviour(Entity Parent)
        {
            this.Parent = Parent;
        }

        public virtual void Update() { }
    }
}
