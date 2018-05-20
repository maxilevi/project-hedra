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
            Type[] typeList = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(AnimationEventBuilder).Namespace).ToArray();
            foreach (Type type in typeList)
            {
                if (!type.IsSubclassOf(typeof(AnimationEvent))) continue;
                Events.Add(type.Name.ToLowerInvariant(), type);
            }
        }

        public static AnimationEvent Build(Entity Parent, string Key)
        {
            return (AnimationEvent) Activator.CreateInstance(Events[Key.ToLowerInvariant()], Parent);
        }
    }
}
