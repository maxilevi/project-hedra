using Hedra.Engine.ItemSystem;
using Hedra.EntitySystem;

namespace Hedra.Items
{
    public static class InventoryExtensions
    {
        public static void AddOrDropItem(this IHumanoid Owner, Item Item)
        {
            if (!Owner.Inventory.AddItem(Item))
            {
                World.DropItem(Item, Owner.Position);
            }
        }

        public static bool HasItem(this IHumanoid Owner, string Name)
        {
            return Owner.Inventory.Search(I => I.Name == Name.ToLowerInvariant()) != null;
        }

        public static bool HasItem(this IHumanoid Owner, ItemType Type)
        {
            return HasItem(Owner, Type.ToString());
        }
    }
}