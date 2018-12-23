using System;
using Hedra.Engine.ItemSystem;
using Hedra.ItemHandlers;

namespace Hedra.API
{
    public class ItemHandlerRegistry : TypeRegistry
    {
        protected override void DoAdd(string Key, Type Value)
        {
            ItemHandlerFactory.Instance.Register(Key, Value);
        }

        protected override void DoRemove(string Key, Type Value)
        {
            ItemHandlerFactory.Instance.Unregister(Key);
        }

        protected override Type RegistryType { get; } = typeof(ItemHandler);
    }
}