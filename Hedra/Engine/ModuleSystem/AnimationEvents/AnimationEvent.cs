using System;
using System.Reflection;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public abstract class AnimationEvent : IDisposable
    {
        public IEntity Parent { get; set; }
        public bool Disposed { get; protected set; }

        protected AnimationEvent(IEntity Parent)
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
