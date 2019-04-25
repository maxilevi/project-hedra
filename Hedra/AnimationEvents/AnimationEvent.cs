using System;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;

namespace Hedra.AnimationEvents
{
    public abstract class AnimationEvent : IDisposable
    {
        public ISkilledAnimableEntity Parent { get; set; }
        public bool Disposed { get; protected set; }

        protected AnimationEvent(ISkilledAnimableEntity Parent)
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
