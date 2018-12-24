using System;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Player;
using Hedra.WeaponSystem;

namespace Hedra.EntitySystem
{
    public interface IHumanoid : IEntity
    {
        event OnHitLandedEventHandler OnHitLanded;
        IMessageDispatcher MessageDispatcher { get; set; }
        IPlayerInventory Inventory { get; }
        int ConsecutiveHits { get; }
        bool IsAttacking { get; set; }
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
        float Mana { get; set; }
        float ManaRegenFactor { get; set; }
        float Stamina { get; set; }
        int Level { get; set; }
        float AttackPower { get; set; }
        float MaxStamina { get; }
        float AddonHealth { get; set; }
        float DodgeCost { get; set; }    
        float RandomFactor { get; set; }
        float AttackResistance { get; }
        int Gold { get; set; }
        float DamageEquation { get; }
        void AttackSurroundings(float Damage, Action<IEntity> Callback);
        void AttackSurroundings(float Damage);
        void ApplyEffectWhile(EffectType NewType, Func<bool> Condition);
        void AddBonusAttackSpeedWhile(float BonusAttackSpeed, Func<bool> Condition);
        void AddBonusHealthWhile(float BonusHealth, Func<bool> Condition);
        void ProcessHit(bool HittedSomething);
        void ResetEquipment();
        void Greet();
        Item MainWeapon { get; }
        Item Ring { get; set; }
        float BaseSpeed { get; }
        float MaxHealth { get; }
        float ManaRegen { get; }
        float HealthRegen { get; }
        float WeaponModifier(Item Weapon);
        Weapon LeftWeapon { get; }
        void Roll(RollType Type);
        void SetWeapon(Weapon Item);
        void SetHelmet(HelmetPiece Item);
        void SetChestplate(ChestPiece Item);
        void SetPants(PantsPiece Item);
        void SetBoots(BootsPiece Item);
    }
}