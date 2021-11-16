using System;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem
{
    public class AIFactory : RegistryFactory<AIFactory, string, Type>
    {
        public BasicAIComponent Build(Entity Parent, string Key)
        {
            return (BasicAIComponent)Activator.CreateInstance(this[Key], Parent);
        }
    }
}