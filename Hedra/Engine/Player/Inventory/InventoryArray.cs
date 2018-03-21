using System;
using System.Linq;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnItemSetEventHandler(int Index, Item New);

    public class InventoryArray
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
                _items[i] = null;
            }
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
