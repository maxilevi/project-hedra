using System;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using Hedra.WeaponSystem;

namespace Hedra.EntitySystem
{
    public interface IHumanoid : ISkilledAnimableEntity
    {
        IVehicle Boat { get; }
        IMessageDispatcher MessageDispatcher { get; set; }
        IPlayerInventory Inventory { get; }
        CustomizationData Customization { get; set; }
        int ConsecutiveHits { get; }
        bool IsAttacking { get; set; }
        bool IsSprinting { get; }
        bool IsEating { get; set; }
        bool IsCasting { get; set; }
        bool IsSwimming { get; set; }
        bool IsSailing { get; }
        bool IsGliding { get; }
        bool IsTravelling { get; set; }
        bool IsRolling { get; set; }
        bool IsRiding { get; set; }
        bool IsTied { get; set; }
        bool IsClimbing { get; set; }
        bool WasAttacking { get; set; }
        float AttackSpeed { get; set; }
        float BaseAttackSpeed { get; }
        bool CanInteract { get; set; }
        bool IsSleeping { get; set; }
        bool IsJumping { get; }
        float FacingDirection { get; }
        bool IsSitting { get; set; }
        bool HasWeapon { get; }
        bool IsFishing { get; set; }
        HandLamp HandLamp { get; }
        HumanoidModel Model { get; set; }
        MovementManager Movement { get; }
        ClassDesign Class { get; set; }
        float XP { get; set; }
        float MaxXP { get; }
        float MaxMana { get; }
        float Health { get; set; }
        float ManaRegenFactor { get; set; }
        float Stamina { get; set; }
        float StaminaRegen { get; set; }
        int Level { get; set; }
        float AttackPower { get; set; }
        float MaxStamina { get; }
        float BonusHealth { get; set; }
        float DodgeCost { get; set; }
        float RandomFactor { get; set; }
        float AttackResistance { get; set; }
        float Sorcery { get; set; }
        float Accuracy { get; set; }
        float Aggressiveness { get; set; }
        int Gold { get; set; }
        float DamageEquation { get; }
        float UnRandomizedDamageEquation { get; }
        Item Ring { get; set; }
        float BaseSpeed { get; }
        float MaxHealth { get; }
        float ManaRegen { get; }
        float Armor { get; set; }
        float HealthRegen { get; }
        Weapon LeftWeapon { get; }
        Item MainWeapon { get; }
        event OnAttackEventHandler BeforeAttack;
        event OnAttackEventHandler AfterAttack;
        event OnHitLandedEventHandler HitLanded;
        event OnInteractionEvent Interact;
        event OnFishing Fishing;
        void AttackSurroundings(float Damage, IEntity[] ToIgnore, Action<IEntity> Callback);
        void AttackSurroundings(float Damage, IEntity[] ToIgnore);
        void AttackSurroundings(float Damage, Action<IEntity> Callback);
        void AttackSurroundings(float Damage);
        void ApplyEffectWhile(EffectType NewType, Func<bool> Condition);
        void AddBonusAttackSpeedWhile(float BonusAttackSpeed, Func<bool> Condition);
        void AddBonusHealthWhile(float BonusHealth, Func<bool> Condition);
        void ProcessHit(bool HittedSomething);
        void ResetEquipment();
        void Greet();
        float WeaponModifier(Item Weapon);
        void Roll(RollType Type);
        void SetWeapon(Weapon Item);
        void SetHelmet(Item Item);
        void SetChestplate(Item Item);
        void SetPants(Item Item);
        void SetBoots(Item Item);
        void InvokeBeforeAttackEvent(AttackOptions Options);
        void InvokeAfterAttackEvent(AttackOptions Options);
        Item[] GetMainEquipment();
        void SetMainEquipment(Item[] Equipment);
        void DoIgnoringHitCombo(Action Lambda);
        void AddOrDropItem(Item Item);
        void RegisterInteraction(InteractableStructure Structure);
        void RegisterFishing(Item FishedObject);
    }
}