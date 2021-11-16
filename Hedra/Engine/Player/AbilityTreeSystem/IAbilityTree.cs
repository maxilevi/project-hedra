using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public interface IAbilityTree : ISerializableHandler
    {
        int AvailablePoints { get; }
        int UsedPoints { get; }
        Item[] TreeItems { get; }
        InventoryArray MainTree { get; }
        InventoryArray FirstTree { get; }
        InventoryArray SecondTree { get; }
        bool Show { get; set; }
        AbilityTreeBlueprint Specialization { get; }
        bool HasSpecialization { get; }
        bool HasFirstSpecialization { get; }
        bool HasSecondSpecialization { get; }
        bool IsCurrentTreeEnabled { get; }
        int ExtraSkillPoints { get; set; }
        event OnSkillUpdated SkillUpdated;
        void Update();
        void SetPoints(Type AbilityType, int Count);
        void SetPoints(int Index, int Count);
        void Reset();
        void ShowBlueprint(AbilityTreeBlueprint Blueprint, InventoryArray Array, byte[] AbilityTreeArray);
        void LearnSpecialization(AbilityTreeBlueprint Blueprint);
        bool IsTreeEnabled(AbilityTreeBlueprint Blueprint);
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
        event OnSpecializationLearned SpecializationLearned;
        void ConfirmPoints();
        bool IsConfirmed(int Index);
    }
}