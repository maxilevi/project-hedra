using System;
using Hedra.Engine.ItemSystem;
using Hedra.Items;

namespace Hedra.API
{
    public class ItemHandlerRegistry : TypeRegistry
    {
        protected override Type RegistryType { get; } = typeof(ItemHandler);

        protected override void DoAdd(string Key, Type Value)
        {
            ItemHandlerFactory.Instance.Register(Key, Value);
        }

        protected override void DoRemove(string Key, Type Value)
        {
            ItemHandlerFactory.Instance.Unregister(Key);
        }
    }
}