using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
    public abstract class Component<T> : IUpdatable, IComponent<T> where T : IEntity
    {	
        public bool Renderable { get; }
        protected bool Disposed { get; private set; }
        protected T Parent { get; set; }

        protected Component(T Entity)
        {
            this.Parent = Entity;
            this.Renderable = this.GetType().GetMethod("Draw")?.DeclaringType != base.GetType().BaseType;
        }
		
        public abstract void Update();
		
        public virtual void Draw(){}
		
        public virtual void Dispose()
        {
            this.Disposed = true;
        }
    }
}