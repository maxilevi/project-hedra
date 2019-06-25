using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Items;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public interface IPlayerInventory
    {
        event OnInventoryUpdated InventoryUpdated;
        void UpdateInventory();
        void ClearInventory();
        Item Search(Func<Item, bool> Matches);
        int IndexOf(Item Item);
        bool AddItem(Item New);
        void RemoveItem(Item Old, int Amount = 1);
        void SetItem(int Index, Item New);
        void SetItems(KeyValuePair<int, Item>[] Items);
        KeyValuePair<int, Item>[] ItemsToArray();
        KeyValuePair<int, Item>[] ToArray();
        void AddRestriction(int Index, EquipmentType Type);
        void AddRestriction(int Index, string Type);
        void RemoveRestriction(int Index, EquipmentType Type);
        void RemoveRestriction(int Index, string Type);
        void SetRestrictions(int Index, string[] Restrictions);
        string[] GetRestrictions(int Index);
        Item Food { get; }
        Item Ammo { get; }
        bool HasAvailableSpace { get; }
        Item MainWeapon { get; }
        Item Vehicle { get; }
        Item Pet { get; }
        Item Helmet { get; }
        Item Chest { get; }
        Item Pants { get; }
        Item Boots { get; }
        int Length { get; }
        Item this[int Index] { get; }
        InventoryArray MainItemsArray { get; }
        InventoryArray ItemsArray { get; }
    }

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