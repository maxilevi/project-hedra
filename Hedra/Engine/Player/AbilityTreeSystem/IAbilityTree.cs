using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public interface IAbilityTree
    {
        event OnSkillUpdated SkillUpdated;
        void Update();
        void SetPoints(Type AbilityType, int Count);
        void SetPoints(int Index, int Count);
        void Reset();
        byte[] MainTreeSave { get; }
        byte[] FirstTreeSave { get; }
        byte[] SecondTreeSave { get; }
        void ShowBlueprint(AbilityTreeBlueprint Blueprint, InventoryArray Array, byte[] AbilityTreeArray);
        void FromInformation(PlayerInformation Information);
        int AvailablePoints { get; }
        Item[] TreeItems { get; }
        InventoryArray MainTree { get; }
        InventoryArray FirstTree { get; }
        InventoryArray SecondTree { get; }
        bool Show { get; set; }
        int SpecializationTreeIndex { get; }
        AbilityTreeBlueprint Specialization { get; }
        void LearnSpecialization(AbilityTreeBlueprint Blueprint);
        bool IsTreeEnabled(AbilityTreeBlueprint Blueprint);
        bool HasSpecialization { get; }
        bool HasFirstSpecialization { get; }
        bool HasSecondSpecialization { get; }
        bool IsCurrentTreeEnabled { get; }
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
        int ExtraSkillPoints { get; set; }
    }
}