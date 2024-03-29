using System;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;

namespace Hedra.EntitySystem
{
    public interface IEntity : IUpdatable, ISearchable, IDisposable, IRenderable, IEntityWithAttributes,
        IObjectWithLifeCycle
    {
        IPhysicsComponent Physics { get; }

        EntityComponentManager ComponentManager { get; }

        float AttackDamage { get; set; }

        float AttackCooldown { get; set; }

        float AttackResistance { get; set; }

        float AttackingSpeedModifier { get; }

        int Level { get; set; }

        bool IsUndead { get; }
        bool IsInsideABuilding { get; set; }

        float MaxOxygen { get; set; }

        int MobId { get; set; }

        int Seed { get; set; }

        Vector3 Orientation { get; set; }

        bool Removable { get; set; }

        bool PlaySpawningAnimation { get; set; }

        float Speed { get; set; }

        float Health { get; set; }

        bool InUpdateRange { get; }

        bool IsFriendly { get; set; }

        bool IsGrounded { get; set; }

        bool IsHumanoid { get; }

        bool IsImmune { get; }

        bool IsInvisible { get; set; }

        bool IsStatic { get; }

        bool IsUnderwater { get; }

        bool IsMoving { get; }

        bool IsAttacking { get; }

        bool IsStuck { get; }

        bool Disposed { get; }

        float MaxHealth { get; set; }

        MobType MobType { get; set; }

        BaseUpdatableModel Model { get; set; }

        string Name { get; set; }

        float Oxygen { get; set; }

        Vector3 Position { get; set; }

        Vector3 Rotation { get; set; }

        Vector3 Size { get; }

        string Type { get; set; }

        bool UpdateWhenOutOfRange { get; set; }

        event OnComponentAdded ComponentAdded;

        event OnDamagingEventHandler AfterDamaging;

        event OnDamagingEventHandler BeforeDamaging;

        event OnDamageModifierEventHandler DamageModifiers;

        event OnKillEventHandler Kill;

        event OnDisposedEvent AfterDisposed;

        void ShowIcon(CacheItem? IconType);

        void ShowIcon(CacheItem? IconType, float Seconds);

        void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound = true, bool PushBack = true);

        void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted, bool PlaySound = true,
            bool PushBack = true);

        void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted, bool PlaySound, bool PushBack,
            DamageType Type);

        void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition, bool ShowParticles = true);

        void AddBonusSpeedForSeconds(float BonusSpeed, float Seconds);

        void AddComponentWhile(IComponent<IEntity> Component, Func<bool> Condition);

        void AddComponentForSeconds(IComponent<IEntity> Component, float Seconds);

        void AddComponent(IComponent<IEntity> Component);

        void RemoveComponent(IComponent<IEntity> Component, bool Dispose = true);

        T SearchComponent<T>();

        void RemoveComponentsOfType<T>() where T : IComponent<IEntity>;

        T[] GetComponents<T>();

        void KnockForSeconds(float Time);

        void Dispose();

        void Draw();

        void Update();

        void InvokeBeforeDamaging(IEntity Invoker, float Damage);

        void InvokeAfterDamaging(IEntity Invoker, float Damage);

        void InvokeDamageModifier(IEntity Invoker, ref float Damage);

        void UpdateCriticalComponents();
    }
}