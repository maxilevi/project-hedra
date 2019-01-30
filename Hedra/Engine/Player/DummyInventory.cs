using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public class DummyInventory : IPlayerInventory
    {
        
        public bool AddItem(Item New)
        {
            return false;
        }

        public void RemoveItem(Item Old, int Amount = 1)
        {
        }

        public event OnInventoryUpdated InventoryUpdated;

        public void UpdateInventory()
        {
            throw new NotImplementedException();
        }

        public void ClearInventory()
        {
            throw new NotImplementedException();
        }

        public Item Search(Func<Item, bool> Matches)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(Item Item)
        {
            return -1;
        }

        public void SetItem(int Index, Item New)
        {
            throw new NotImplementedException();
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<int, Item>[] ItemsToArray()
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            throw new NotImplementedException();
        }

        public void AddRestriction(int Index, EquipmentType Type)
        {
            throw new NotImplementedException();
        }

        public void AddRestriction(int Index, string Type)
        {
            throw new NotImplementedException();
        }

        public Item Food => throw new NotImplementedException();
        public Item Ammo => ItemPool.Grab(ItemType.StoneArrow);

        public bool HasAvailableSpace => throw new NotImplementedException();
        
        public Item MainWeapon => throw new NotImplementedException();
        
        public Item Vehicle => throw new NotImplementedException();
        
        public Item Pet => throw new NotImplementedException();
        
        public int Length => throw new NotImplementedException();

        public Item this[int Index] => throw new NotImplementedException();
        public InventoryArray MainItemsArray { get; }
        public InventoryArray ItemsArray { get; }
    }
}