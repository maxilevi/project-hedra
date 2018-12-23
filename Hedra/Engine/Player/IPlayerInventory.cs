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
        KeyValuePair<int, Item>[] ToArray();
        void Update();
        void AddRestriction(int Index, EquipmentType Type);
        void AddRestriction(int Index, string Type);
        Item Food { get; }
        bool HasAvailableSpace { get; }
        Item MainWeapon { get; }
        Item Vehicle { get; }
        Item Pet { get; }
        int Length { get; }
        bool Show { get; set; }
        Item this[int Index] { get; }
    }
}