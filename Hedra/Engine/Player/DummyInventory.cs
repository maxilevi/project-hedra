using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Items;


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
        }

        public void ClearInventory()
        {
        }

        public Item Search(Func<Item, bool> Matches) => null;

        public int IndexOf(Item Item)
        {
            return -1;
        }

        public void SetItem(int Index, Item New)
        {
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
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
        }

        public void AddRestriction(int Index, string Type)
        {
        }

        public void RemoveRestriction(int Index, EquipmentType Type)
        {
        }

        public void RemoveRestriction(int Index, string Type)
        {
        }

        public void SetRestrictions(int Index, string[] Restrictions)
        {
        }

        public string[] GetRestrictions(int Index)
        {
            return new string[0];
        }

        public Item Food => null;
        public Item Ammo => ItemPool.Grab(ItemType.StoneArrow);

        public bool HasAvailableSpace => false;

        public Item MainWeapon => null;
        
        public Item Vehicle => null;
        
        public Item Pet => null;
        
        public Item Helmet => null;
        
        public Item Chest => null;
        
        public Item Pants => null;
        
        public Item Boots => null;

        public int Length => 0;

        public Item this[int Index] => throw new NotImplementedException();
        public InventoryArray MainItemsArray => null;
        public InventoryArray ItemsArray => null;
    }
}