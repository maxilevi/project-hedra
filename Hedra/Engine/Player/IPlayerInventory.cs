using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public interface IPlayerInventory
    {
        void UpdateInventory();
        void ClearInventory();
        Item Search(Func<Item, bool> Matches);
        int IndexOf(Item Item);
        bool AddItem(Item New);
        void SetItem(int Index, Item New);
        void SetItems(KeyValuePair<int, Item>[] Items);
        KeyValuePair<int, Item>[] ItemsToArray();
        KeyValuePair<int, Item>[] EquipmentItemsToArray();
        KeyValuePair<int, Item>[] ToArray();
        void Update();
        bool HasRestrictions(int Index, EquipmentType Type);
        void AddRestriction(int Index, EquipmentType Type);
        void AddRestriction(int Index, string Type);
        Item Food { get; }
        bool HasAvailableSpace { get; }
        Item MainWeapon { get; }
        Item Ring { get; }
        Item Vehicle { get; }
        Item Pet { get; }
        Item Helmet { get; }
        Item Chest { get; }
        Item Pants { get; }
        Item Boots { get; }
        int Length { get; }
        Key OpeningKey { get; }
        bool Show { get; set; }
        bool HasExitAnimation { get; }
        Item this[int Index] { get; }
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
    }
}