using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.AnimationEvents;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.SkillSystem;

namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    public class AnimationEventBuilder : TypeFactory<AnimationEventBuilder>
    {
        public AnimationEvent Build(ISkilledAnimableEntity Parent, string Key)
        {
            return (AnimationEvent) Activator.CreateInstance(this[Key], Parent);
        }
    }
}
