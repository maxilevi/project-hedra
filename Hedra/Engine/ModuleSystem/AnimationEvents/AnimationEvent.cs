using System;
using Hedra.Engine.EntitySystem;
using OpenTK;

namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    public abstract class AnimationEvent : IDisposable
    {
        public Entity Parent { get; set; }
        public bool Disposed { get; protected set; }

        protected AnimationEvent(Entity Parent)
        {
            this.Parent = Parent;
        }

        public virtual void Build() { }
        public virtual void Update() { }

        public virtual void Dispose()
        {
            this.Disposed = true;
        }
    }
}
