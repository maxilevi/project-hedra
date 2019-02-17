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
        byte[] MainTreeSave { get; }
        byte[] FirstTreeSave { get; }
        byte[] SecondTreeSave { get; }
        void ShowBlueprint(AbilityTreeBlueprint Blueprint, byte[] AbilityTreeArray);
        void FromInformation(PlayerInformation Information);
        int AvailablePoints { get; }
        InventoryArray TreeItems { get; }
        bool Show { get; set; }
        int SpecializationTreeIndex { get; }
        AbilityTreeBlueprint Specialization { get; }
        bool HasSpecialization { get; }
        bool HasFirstSpecialization { get; }
        bool HasSecondSpecialization { get; }
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
    }
}