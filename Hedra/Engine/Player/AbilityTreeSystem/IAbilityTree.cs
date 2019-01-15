using System;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public interface IAbilityTree
    {
        void Update();
        void SetPoints(Type AbilityType, int Count);
        void SetPoints(int Index, int Count);
        void Reset();
        byte[] ToArray();
        void FromInformation(PlayerInformation Information);
        int AvailablePoints { get; }
        InventoryArray TreeItems { get; }
        bool Show { get; set; }
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
    }
}