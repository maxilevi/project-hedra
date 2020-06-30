/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 27/04/2016
 * Time: 08:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Items;
using Hedra.WeaponSystem;


namespace Hedra.Engine.Player
{
    public delegate void OnInventoryUpdated();

    public class PlayerInventory : IPlayerInventory
    {
        public const int MainSpaces = 8;
        public const int InventorySpaces = 30;
        public const int BootsHolder = 30;
        public const int PantsHolder = 31;
        public const int ChestplateHolder = 32;
        public const int HelmetHolder = 33;
        public const int PetHolder = 34;
        public const int VehicleHolder = 35;
        public const int RingHolder = 36;
        public const int WeaponHolder = 37;
        public const int FoodHolder = 29;
        public const int GoldHolder = 28;

        public event OnInventoryUpdated InventoryUpdated;
        public event OnItemSetEventHandler ItemSet;
        private readonly IPlayer _player;
        private readonly InventoryArray _items;
        private readonly InventoryArray _mainItems;
        private readonly RestrictionsInterface _restrictions;
        private bool _show;

        public PlayerInventory(IPlayer Player)
        {
            _player = Player;
            _items = new InventoryArray(InventorySpaces);
            _mainItems = new InventoryArray(MainSpaces);
            _restrictions = new RestrictionsInterface(_mainItems);
            _mainItems.OnItemSet += delegate(int Index, Item New)
            {
                switch (Index+InventorySpaces)
                {
                    case WeaponHolder:
                        _player.SetWeapon(New == null ? Weapon.Empty : New.Weapon);
                        break;
                    case HelmetHolder:
                        _player.SetHelmet(New);
                        break;
                    case ChestplateHolder:
                        _player.SetChestplate(New);
                        break;
                    case PantsHolder:
                        _player.SetPants(New);
                        break;
                    case BootsHolder:
                        _player.SetBoots(New);
                        break;
                    case RingHolder:
                        _player.Ring = New;
                        break;
                }
                ItemSet?.Invoke(Index+InventorySpaces, New);
            };
        }

        public void UpdateInventory()
        {
            InventoryUpdated?.Invoke();
        }

        public void ClearInventory()
        {
            _items.Empty();
            _mainItems.Empty();
        }

        public Item Search(Func<Item, bool> Matches)
        {
            var firstResult = _items.Search(Matches);
            if (firstResult != null) return firstResult;
            var secondResult = _mainItems.Search(Matches);
            return secondResult;
        }

        public int IndexOf(Item Item)
        {
            var firstResult = _items.IndexOf(Item);
            if (firstResult != -1) return firstResult;
            var secondResult = _mainItems.IndexOf(Item);
            return secondResult;
        }

        public bool AddItem(Item New)
        {
            var result = _items.AddItem(New);
            UpdateInventory();
            return result;
        }

        public void RemoveItem(Item Old, int Amount = 1)
        {
            if (Old.HasAttribute(CommonAttributes.Amount) && Old.GetAttribute<int>(CommonAttributes.Amount) > Amount)
                Old.SetAttribute(CommonAttributes.Amount, Old.GetAttribute<int>(CommonAttributes.Amount) - Amount);
            else
                _items.RemoveItem(Old);
            UpdateInventory();
        }

        public void SetItem(int Index, Item New)
        {
            var array = Index >= InventorySpaces ? _mainItems : _items;
            var index = ToCorrectItemSpace(Index);
            array.SetItem(index, New);
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                this.SetItem(Items[i].Key, Items[i].Value);
            }
        }

        public KeyValuePair<int, Item>[] ItemsToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null) list.Add(new KeyValuePair<int, Item>(i, _items[i]));
            }
            return list.ToArray();
        }

        public KeyValuePair<int, Item>[] EquipmentItemsToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < _mainItems.Length; i++)
            {
                if (_mainItems[i] != null) list.Add(new KeyValuePair<int, Item>(i + InventorySpaces, _mainItems[i]));
            }
            return list.ToArray();
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            list.AddRange(this.ItemsToArray());
            list.AddRange(this.EquipmentItemsToArray());
            return list.ToArray();
        }
        
        public bool HasRestrictions(int Index, EquipmentType Type)
        {
            return _restrictions.HasRestriction(ToCorrectItemSpace(WeaponHolder), Type.ToString());
        }

        public void AddRestriction(int Index, EquipmentType Type)
        {
            AddRestriction(Index, Type.ToString());
        }

        public void AddRestriction(int Index, string Type)
        {
            _restrictions.AddRestriction(ToCorrectItemSpace(Index), Type);
        }
        
        public void RemoveRestriction(int Index, EquipmentType Type)
        {
            RemoveRestriction(Index, Type.ToString());
        }

        public void RemoveRestriction(int Index, string Type)
        {
            _restrictions.RemoveRestriction(ToCorrectItemSpace(Index), Type);
        }

        public void SetRestrictions(int Index, string[] Restrictions)
        {
            _restrictions.SetRestrictions(ToCorrectItemSpace(Index), Restrictions);
        }

        public string[] GetRestrictions(int Index)
        {
            return _restrictions.GetRestrictions(ToCorrectItemSpace(Index));
        }
        
        private static int ToCorrectItemSpace(int Index)
        {
            return Index >= InventorySpaces ? Index - InventorySpaces : Index;
        }

        public Item Food
        {
            get
            {
                return (this[FoodHolder] != null && this[FoodHolder].IsFood) 
                    ? this[FoodHolder] 
                    : this.Search(I => I.IsFood);
            }
        }
        
        public Item Ammo => Search(I => I.IsAmmo);

        public bool HasAvailableSpace => _items.HasAvailableSpace;
        public Item this[int Index] => (Index >= InventorySpaces ? _mainItems : _items)[ToCorrectItemSpace(Index)];
        public Item MainWeapon => this[WeaponHolder];
        public Item Ring => this[RingHolder];
        public Item Vehicle => this[VehicleHolder];
        public Item Pet => this[PetHolder];
        public Item Helmet => this[HelmetHolder];
        public Item Chest => this[ChestplateHolder];
        public Item Pants => this[PantsHolder];
        public Item Boots => this[BootsHolder];
        public int Length => _items.Length + _mainItems.Length;

        public InventoryArray MainItemsArray => _mainItems;
        public InventoryArray ItemsArray => _items;
    }
}
