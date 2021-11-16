using System;
using Hedra.Engine.Core;
using Hedra.Items;

namespace Hedra.Engine.ItemSystem
{
    public class ItemHandlerFactory : TypeFactory<ItemHandlerFactory>
    {
        public ItemHandler Build(string Key)
        {
            return (ItemHandler)Activator.CreateInstance(this[Key]);
        }
    }
}