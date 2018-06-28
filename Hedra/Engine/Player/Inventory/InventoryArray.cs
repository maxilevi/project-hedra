using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Player.Inventory
{
    internal delegate void OnItemSetEventHandler(int Index, Item New);

    internal class InventoryArray
    {
        public event OnItemSetEventHandler OnItemSet;
        private readonly Item[] _items;
        private readonly string[][] _restrictions;

        public InventoryArray(int Size)
        {
            _items = new Item[Size];
            _restrictions = new string[Size][];
            for (var i = 0; i < _restrictions.Length; i++)
            {
                _restrictions[i] = new string[0];
            }
        }

        public Item Search(Func<Item, bool> Matches)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null && Matches(_items[i])) return _items[i];
            }
            return null;
        }

        public bool RemoveItem(Item Item)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != Item) continue;

                this.SetItem(i, null);
                return true;
            }
            return false;
        }

        public bool AddItem(Item Item)
        {
            var hasAmount = Item.HasAttribute(CommonAttributes.Amount);
            if (hasAmount)
            {
                for (var i = 0; i < _items.Length; i++)
                {
                    if (_items[i] == null || _items[i].Name != Item.Name) continue;
                    var isFinite = _items[i].GetAttribute<int>(CommonAttributes.Amount) != int.MaxValue;
                    if (isFinite)
                    {
                        _items[i].SetAttribute(CommonAttributes.Amount,
                            _items[i].GetAttribute<int>(CommonAttributes.Amount) +Item.GetAttribute<int>(CommonAttributes.Amount));
                    }
                    return true;
                }
            }
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null) continue;
                this.SetItem(i, Item);
                return true;
            }
            return false;
        }

        public bool Contains(Item Item)
        {
            return _items.Contains(Item);
        }

        public int IndexOf(Item Item)
        {
            return Array.IndexOf(_items, Item);
        }

        public bool CanSetItem(int Index, Item Item)
        {
            return Item == null || _restrictions[Index] != null 
                && (_restrictions[Index].Length == 0 || _restrictions[Index].Contains(Item.EquipmentType));
        }

        public void SetItem(int Index, Item Item)
        {
            if(!this.CanSetItem(Index, Item))
                throw new ArgumentException($" Putting {Item.EquipmentType} in {_restrictions[Index].FirstOrDefault()} is not permitted.");
            _items[Index] = Item;
            OnItemSet?.Invoke(Index, Item);
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                this.SetItem(Items[i].Key, Items[i].Value);
            }
        }

        public Item GetItem(int Index)
        {
            return _items[Index];
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
            if(_restrictions[Index] == null) _restrictions[Index] = new string[0];
            var newRestrictions = _restrictions[Index].ToList();
            newRestrictions.AddRange(Restrictions);
            _restrictions[Index] = newRestrictions.ToArray();
        }

        public void SetRestrictions(int Index, params string[] Restrictions)
        {
            _restrictions[Index] = Restrictions;
        }

        public void SetRestrictions(int Index, params EquipmentType[] Restrictions)
        {
            this.SetRestrictions(Index, Restrictions.ToList().Select(Type => Type.ToString()).ToArray());
        }

        public void Empty()
        {
            for (var i = 0; i < _restrictions.Length; i++)
            {
                _restrictions[i] = new string[0];
                this.SetItem(i, null);
            }
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null) list.Add(new KeyValuePair<int, Item>(i, _items[i]));
            }
            return list.ToArray();
        }

        public int Length => _items.Length;
        public bool HasAvailableSpace => _items.Any(Item => Item == null);

        public Item this[int Index]
        {
            get { return this.GetItem(Index); }
            set { this.SetItem(Index, value); }
        }
    }
}
