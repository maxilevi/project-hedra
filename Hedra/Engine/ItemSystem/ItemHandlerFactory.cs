using System;
using Hedra.Engine.Core;
using Hedra.ItemHandlers;

namespace Hedra.Engine.ItemSystem
{
    public class ItemHandlerFactory : TypeFactory<ItemHandlerFactory>
    {
        public ItemHandler Build(string Key)
        {
            return (ItemHandler) Activator.CreateInstance(this[Key]);
        }
    }
}