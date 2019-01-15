using System;
using Hedra.AISystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public abstract class Component<T> : IUpdatable, IComponent<T> where T : IEntity
    {    
        public bool Drawable { get; }
        protected bool Disposed { get; private set; }
        protected T Parent { get; }

        protected Component(T Entity)
        {
            Parent = Entity;
            Drawable = GetType().GetMethod("Draw")?.DeclaringType != base.GetType().BaseType;
            EnsureSingleBehaviour();
        }

        private void EnsureSingleBehaviour()
        {
            //if(Parent.SearchComponent<IBehaviourComponent>() != null && this is IBehaviourComponent)
            //    throw new ArgumentException("Entity cannot have more than 2 behaviour components.");
        }
        
        public abstract void Update();
        
        public virtual void Draw(){}
        
        public virtual void Dispose()
        {
            this.Disposed = true;
        }
    }
}