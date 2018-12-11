using System;
using Hedra.AnimationEvents;
using Hedra.Engine.ModuleSystem.AnimationEvents;

namespace Hedra.API
{
    public class AnimationEventRegistry : TypeRegistry
    {
        protected override void DoAdd(string Name, Type ClassType)
        {
            AnimationEventBuilder.Instance.Register(Name, ClassType);
        }

        protected override void DoRemove(string Name, Type ClassType)
        {
            AnimationEventBuilder.Instance.Unregister(Name);
        }

        protected override Type RegistryType { get; } = typeof(AnimationEvent);
    }
}