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
        private readonly IPlayer _player;
        private readonly RestrictionsInterface _restrictions;
        private bool _show;

        public PlayerInventory(IPlayer Player)
        {
            _player = Player;
            ItemsArray = new InventoryArray(InventorySpaces);
            MainItemsArray = new InventoryArray(MainSpaces);
            _restrictions = new RestrictionsInterface(MainItemsArray);
            MainItemsArray.OnItemSet += delegate(int Index, Item New)
            {
                switch (Index + InventorySpaces)
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

                ItemSet?.Invoke(Index + InventorySpaces, New);
            };
        }

        public Item Ring => this[RingHolder];

        public event OnInventoryUpdated InventoryUpdated;
        public event OnItemSetEventHandler ItemSet;

        public void UpdateInventory()
        {
            InventoryUpdated?.Invoke();
        }

        public void ClearInventory()
        {
            ItemsArray.Empty();
            MainItemsArray.Empty();
        }

        public Item Search(Func<Item, bool> Matches)
        {
            var firstResult = ItemsArray.Search(Matches);
            if (firstResult != null) return firstResult;
            var secondResult = MainItemsArray.Search(Matches);
            return secondResult;
        }

        public int IndexOf(Item Item)
        {
            var firstResult = ItemsArray.IndexOf(Item);
            if (firstResult != -1) return firstResult;
            var secondResult = MainItemsArray.IndexOf(Item);
            return secondResult;
        }

        public bool AddItem(Item New)
        {
            var result = ItemsArray.AddItem(New);
            UpdateInventory();
            return result;
        }

        public void RemoveItem(Item Old, int Amount = 1)
        {
            if (Old.HasAttribute(CommonAttributes.Amount) && Old.GetAttribute<int>(CommonAttributes.Amount) > Amount)
                Old.SetAttribute(CommonAttributes.Amount, Old.GetAttribute<int>(CommonAttributes.Amount) - Amount);
            else
                ItemsArray.RemoveItem(Old);
            UpdateInventory();
        }

        public void SetItem(int Index, Item New)
        {
            var array = Index >= InventorySpaces ? MainItemsArray : ItemsArray;
            var index = ToCorrectItemSpace(Index);
            array.SetItem(index, New);
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            for (var i = 0; i < Items.Length; i++) SetItem(Items[i].Key, Items[i].Value);
        }

        public KeyValuePair<int, Item>[] ItemsToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < ItemsArray.Length; i++)
                if (ItemsArray[i] != null)
                    list.Add(new KeyValuePair<int, Item>(i, ItemsArray[i]));
            return list.ToArray();
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            list.AddRange(ItemsToArray());
            list.AddRange(EquipmentItemsToArray());
            return list.ToArray();
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

        public Item Food
        {
            get
            {
                return this[FoodHolder] != null && this[FoodHolder].IsFood
                    ? this[FoodHolder]
                    : Search(I => I.IsFood);
            }
        }

        public Item Ammo => Search(I => I.IsAmmo);

        public bool HasAvailableSpace => ItemsArray.HasAvailableSpace;

        public Item this[int Index] =>
            (Index >= InventorySpaces ? MainItemsArray : ItemsArray)[ToCorrectItemSpace(Index)];

        public Item MainWeapon => this[WeaponHolder];
        public Item Vehicle => this[VehicleHolder];
        public Item Pet => this[PetHolder];
        public Item Helmet => this[HelmetHolder];
        public Item Chest => this[ChestplateHolder];
        public Item Pants => this[PantsHolder];
        public Item Boots => this[BootsHolder];
        public int Length => ItemsArray.Length + MainItemsArray.Length;

        public InventoryArray MainItemsArray { get; }

        public InventoryArray ItemsArray { get; }

        public KeyValuePair<int, Item>[] EquipmentItemsToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < MainItemsArray.Length; i++)
                if (MainItemsArray[i] != null)
                    list.Add(new KeyValuePair<int, Item>(i + InventorySpaces, MainItemsArray[i]));
            return list.ToArray();
        }

        public bool HasRestrictions(int Index, EquipmentType Type)
        {
            return _restrictions.HasRestriction(ToCorrectItemSpace(WeaponHolder), Type.ToString());
        }

        private static int ToCorrectItemSpace(int Index)
        {
            return Index >= InventorySpaces ? Index - InventorySpaces : Index;
        }
    }
}