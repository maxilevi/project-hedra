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

        static AnimationEventBuilder()
        {
            Events = new Dictionary<string, Type>();
        }

        public static void Register(string Name, Type Event)
        {
            Events.Add(Name, Event);
        }
        
        public static void Unregister(string Name)
        {
            Events.Remove(Name);
        }

        public static AnimationEvent Build(Entity Parent, string Key)
        {
            return (AnimationEvent) Activator.CreateInstance(Events[Key], Parent);
        }
    }
}
