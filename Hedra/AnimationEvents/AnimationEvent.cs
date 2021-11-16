using System;
using Hedra.Engine.SkillSystem;

namespace Hedra.AnimationEvents
{
    public abstract class AnimationEvent : IDisposable
    {
        protected AnimationEvent(ISkilledAnimableEntity Parent)
        {
            this.Parent = Parent;
        }

        public ISkilledAnimableEntity Parent { get; set; }
        public bool Disposed { get; protected set; }

        public virtual void Dispose()
        {
            Disposed = true;
        }

        public virtual void Build()
        {
        }
    }
}