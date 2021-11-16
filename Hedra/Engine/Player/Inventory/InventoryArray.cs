using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Items;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnItemSetEventHandler(int Index, Item New);

    public class InventoryArray
    {
        private readonly string[][] _restrictions;

        public InventoryArray(int Size)
        {
            Items = new Item[Size];
            _restrictions = new string[Size][];
            for (var i = 0; i < _restrictions.Length; i++) _restrictions[i] = new string[0];
        }

        public Item[] Items { get; }

        public int Length => Items.Length;
        public bool HasAvailableSpace => Items.Any(Item => Item == null);

        public Item this[int Index]
        {
            get => GetItem(Index);
            set => SetItem(Index, value);
        }

        public event OnItemSetEventHandler OnItemSet;

        public Item Search(Func<Item, bool> Matches)
        {
            for (var i = 0; i < Items.Length; i++)
                if (Items[i] != null && Matches(Items[i]))
                    return Items[i];
            return null;
        }

        public bool RemoveItem(Item Item)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i] != Item) continue;

                SetItem(i, null);
                return true;
            }

            return false;
        }

        public bool AddItem(Item Item)
        {
            var hasAmount = Item.HasAttribute(CommonAttributes.Amount);
            if (hasAmount)
                for (var i = 0; i < Items.Length; i++)
                {
                    if (Items[i] == null || Items[i].Name != Item.Name) continue;
                    var isFinite = Items[i].GetAttribute<int>(CommonAttributes.Amount) != int.MaxValue;
                    if (isFinite)
                        Items[i].SetAttribute(CommonAttributes.Amount,
                            Items[i].GetAttribute<int>(CommonAttributes.Amount) +
                            Item.GetAttribute<int>(CommonAttributes.Amount));
                    return true;
                }

            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null || !CanSetItem(i, Item)) continue;
                SetItem(i, Item);
                return true;
            }

            return false;
        }

        public bool Contains(Item Item)
        {
            return Items.Contains(Item);
        }

        public int IndexOf(Item Item)
        {
            return Array.IndexOf(Items, Item);
        }

        public bool CanSetItem(int Index, Item Item)
        {
            return Item == null || _restrictions[Index] != null
                && (_restrictions[Index].Length == 0 || _restrictions[Index].Contains(Item.EquipmentType));
        }

        public void SetItem(int Index, Item Item)
        {
            if (!CanSetItem(Index, Item))
                throw new ArgumentException(
                    $" Putting {Item.EquipmentType} in {_restrictions[Index].FirstOrDefault()} is not permitted.");
            Items[Index] = Item;
            OnItemSet?.Invoke(Index, Item);
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            for (var i = 0; i < Items.Length; i++) SetItem(Items[i].Key, Items[i].Value);
        }

        public Item GetItem(int Index)
        {
            return Items[Index];
        }

        public string[] GetRestrictions(int Index)
        {
            return _restrictions[Index];
        }

        public bool HasRestrictions(int Index)
        {
            return _restrictions[Index] == null || _restrictions[Index].Length > 0;
        }

        public void AppendRestriction(int Index, params string[] Restrictions)
        {
            if (_restrictions[Index] == null) _restrictions[Index] = new string[0];
            var newRestrictions = _restrictions[Index].ToList();
            newRestrictions.AddRange(Restrictions);
            _restrictions[Index] = newRestrictions.ToArray();
        }

        public void RemoveRestriction(int Index, string Restriction)
        {
            var restrictionArray = _restrictions[Index];
            if (restrictionArray == null || Array.IndexOf(restrictionArray, Restriction) == -1)
                throw new ArgumentOutOfRangeException(
                    $"Cannot remove a restriction '{Restriction}' from an inexistant array.");
            SetRestrictions(Index, restrictionArray.Where(R => R != Restriction).ToArray());
        }

        public void SetRestrictions(int Index, params string[] Restrictions)
        {
            _restrictions[Index] = Restrictions;
        }

        public void SetRestrictions(int Index, params EquipmentType[] Restrictions)
        {
            SetRestrictions(Index, Restrictions.ToList().Select(Type => Type.ToString()).ToArray());
        }

        public void Empty()
        {
            for (var i = 0; i < _restrictions.Length; i++)
            {
                _restrictions[i] = new string[0];
                SetItem(i, null);
            }
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < Items.Length; i++)
                if (Items[i] != null)
                    list.Add(new KeyValuePair<int, Item>(i, Items[i]));
            return list.ToArray();
        }
    }
}