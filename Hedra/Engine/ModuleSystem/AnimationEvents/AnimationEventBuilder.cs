using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    public static class AnimationEventBuilder
    {
        private static readonly Dictionary<string, Type> Events;

        public static void Register(string Name, Type Event)
        {
            Events.Add(Name, Event);
        }

        public static AnimationEvent Build(Entity Parent, string Key)
        {
            return (AnimationEvent) Activator.CreateInstance(Events[Key.ToLowerInvariant()], Parent);
        }
    }
}
