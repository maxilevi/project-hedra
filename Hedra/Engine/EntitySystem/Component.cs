using System;
using Hedra.AISystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public abstract class Component<T> : IUpdatable, IComponent<T> where T : IEntity
    {
        protected Component(T Entity)
        {
            Parent = Entity;
            EnsureSingleBehaviour();
        }

        protected bool Disposed { get; private set; }
        protected T Parent { get; }

        public virtual void Draw()
        {
        }

        public virtual void Dispose()
        {
            Disposed = true;
        }

        public abstract void Update();

        private void EnsureSingleBehaviour()
        {
            if (Parent.SearchComponent<IBehaviourComponent>() != null && this is IBehaviourComponent)
                throw new ArgumentException("Entity cannot have more than 2 behaviour components.");
        }
    }
}