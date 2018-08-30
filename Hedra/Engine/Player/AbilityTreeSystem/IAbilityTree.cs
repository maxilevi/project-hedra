﻿using System;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public interface IAbilityTree
    {
        void UpdateView();
        void Update();
        void SetPoints(Type AbilityType, int Count);
        void SetPoints(int Index, int Count);
        void Reset();
        byte[] ToArray();
        void FromInformation(PlayerInformation Information);
        int AvailablePoints { get; }
        InventoryArray TreeItems { get; }
        Key OpeningKey { get; }
        bool Show { get; set; }
        bool HasExitAnimation { get; }
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
    }
}