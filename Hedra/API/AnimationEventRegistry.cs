using System;
using Hedra.Engine.ModuleSystem.AnimationEvents;

namespace Hedra.API
{
    public class AnimationEventRegistry : ModRegistry
    {
        protected override void DoAdd(string Name, Type ClassType)
        {
            AnimationEventBuilder.Register(Name, ClassType);
        }

        protected override void DoRemove(string Name)
        {
            AnimationEventBuilder.Unregister(Name);
        }

        protected override Type RegistryType { get; } = typeof(AnimationEvent);
    }
}