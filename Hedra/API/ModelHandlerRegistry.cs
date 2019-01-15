using System;
using Hedra.Engine.ModuleSystem.ModelHandlers;
using Hedra.ModelHandlers;

namespace Hedra.API
{
    public class ModelHandlerRegistry : TypeRegistry
    {
        protected override void DoAdd(string Key, Type Value)
        {
            ModelHandlerFactory.Instance.Register(Key, Value);
        }

        protected override void DoRemove(string Key, Type Value)
        {
            ModelHandlerFactory.Instance.Unregister(Key);
        }

        protected override Type RegistryType { get; } = typeof(ModelHandler);
    }
}