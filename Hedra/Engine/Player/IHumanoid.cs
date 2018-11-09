using System;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.Player
{
    public interface IHumanoid : IEntity
    {
        event OnHitLandedEventHandler OnHitLanded;
        IMessageDispatcher MessageDispatcher { get; set; }
        int ConsecutiveHits { get; }
        bool IsAttacking {get; set;}
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
        void Attack(float Damage, Action<Entity> Callback);
        void Attack(float Damage);
        void ApplyEffectWhile(EffectType NewType, Func<bool> Condition);
        void ProcessHit(bool HittedSomething);
        void Greet();
        Item MainWeapon { get; }
        float BaseSpeed { get; }
        float MaxHealth { get; }
        float ManaRegen { get; }
        float HealthRegen { get; }
        float WeaponModifier(Item Weapon);
        Weapon LeftWeapon { get; }
        void Roll(RollType Type);
    }
}