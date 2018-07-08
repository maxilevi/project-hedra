﻿using System;
using System.Reflection;
using Hedra.Engine.EntitySystem;
using OpenTK;

[assembly: Obfuscation(Exclude = true)]
namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    internal abstract class AnimationEvent : IDisposable
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