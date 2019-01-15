using System;
using Hedra.AISystem;

namespace Hedra.API
{
    public class AIRegistry : TypeRegistry
    {
        protected override void DoAdd(string Name, Type ClassType)
        {
            AIFactory.Instance.Register(Name, ClassType);
        }

        protected override void DoRemove(string Name, Type ClassType)
        {
            AIFactory.Instance.Unregister(Name);
        }

        protected override Type RegistryType { get; } = typeof(BasicAIComponent);
    }
}