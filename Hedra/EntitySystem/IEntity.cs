using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.EntitySystem
{
    public interface IEntity : IUpdatable, ISearchable, IDisposable, IRenderable
    {
        IPhysicsComponent Physics { get; }

        event OnComponentAdded ComponentAdded;
        
        event OnAttackEventHandler AfterAttacking;
        
        event OnAttackEventHandler BeforeAttacking;
        
        EntityComponentManager ComponentManager { get; }
        
        float AttackDamage { get; set; }
        
        float AttackCooldown { get; set; }
        
        float AttackResistance { get; set; }

        float AttackingSpeedModifier { get; }
        
        int Level { get; set; }
        
        float MaxOxygen { get; set; }
        
        int MobId { get; set; }
        
        int Seed { get; set; }
        
        Vector3 Orientation { get; set; }
        
        bool Removable { get; set; }
        
        Vector3 BlockPosition { get; set; }
        
        bool PlaySpawningAnimation { get; set; }
        
        float Speed { get; set; }

        float Health { get; set; }

        bool InUpdateRange { get; }

        bool IsBoss { get; set; }
        
        bool IsDead { get; set; }
        
        bool IsFriendly { get; }
        
        bool IsGrounded { get; set; }
        
        bool IsHumanoid { get; }
        
        bool IsImmune { get; }
        
        bool IsInvisible { get; set; }
        
        bool IsStatic { get; }
        
        bool IsUnderwater { get; }
        
        bool IsKnocked { get; }
        
        bool IsMoving { get; }
        
        bool IsAttacking { get; }
        
        bool IsStuck { get; set; }

        bool Disposed { get; }
        
        float MaxHealth { get; }
        
        MobType MobType { get; set; }
        
        BaseUpdatableModel Model { get; set; }
        
        string Name { get; set; }

        float Oxygen { get; set; }

        Vector3 Position { get; set; }

        Vector3 Rotation { get; set; }
        
        Vector3 Size { get; }

        string Type { get; set; }

        void ShowIcon(CacheItem? IconType);

        void ShowIcon(CacheItem? IconType, float Seconds);

        void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound = true, bool PushBack = true);

        void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition);

        void AddBonusSpeedForSeconds(float BonusSpeed, float Seconds);

        void AddComponentWhile(IComponent<IEntity> Component, Func<bool> Condition);

        void AddComponentForSeconds(IComponent<IEntity> Component, float Seconds);

        void AddComponent(IComponent<IEntity> Component);

        void RemoveComponent(IComponent<IEntity> Component);

        T SearchComponent<T>();

        T[] GetComponents<T>();
        
        void KnockForSeconds(float Time);

        void SpawnAnimation();

        void Dispose();

        void Draw();

        void Update();

        void InvokeBeforeAttack(IEntity Invoker, float Damage);

        void InvokeAfterAttack(IEntity Invoker, float Damage);
    }
}