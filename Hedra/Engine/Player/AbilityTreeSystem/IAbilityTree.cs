using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Items;


namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public interface IAbilityTree : ISerializableHandler
    {
        event OnSkillUpdated SkillUpdated;
        void Update();
        void SetPoints(Type AbilityType, int Count);
        void SetPoints(int Index, int Count);
        void Reset();
        void ShowBlueprint(AbilityTreeBlueprint Blueprint, InventoryArray Array, byte[] AbilityTreeArray);
        int AvailablePoints { get; }
        int UsedPoints { get; }
        Item[] TreeItems { get; }
        InventoryArray MainTree { get; }
        InventoryArray FirstTree { get; }
        InventoryArray SecondTree { get; }
        bool Show { get; set; }
        AbilityTreeBlueprint Specialization { get; }
        void LearnSpecialization(AbilityTreeBlueprint Blueprint);
        bool IsTreeEnabled(AbilityTreeBlueprint Blueprint);
        bool HasSpecialization { get; }
        bool HasFirstSpecialization { get; }
        bool HasSecondSpecialization { get; }
        bool IsCurrentTreeEnabled { get; }
        event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
        event OnSpecializationLearned SpecializationLearned;
        int ExtraSkillPoints { get; set; }
        void ConfirmPoints();
        bool IsConfirmed(int Index);
    }
}