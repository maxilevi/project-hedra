﻿using Hedra.Engine.CacheSystem;
using System;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public interface IEntity
    {
        PhysicsComponent Physics { get; }
        
        event OnAttackEventHandler OnAttacking;
        
        event OnAttackEventHandler BeforeAttacking;
        
        EntityComponentManager ComponentManager { get; }
        
        float AttackDamage { get; set; }
        
        float AttackCooldown { get; set; }
        
        float AttackResistance { get; set; }
        
        bool Destroy { get; set; }
        
        int Level { get; set; }
        
        float MaxOxygen { get; set; }
        
        int MobId { get; set; }
        
        int MobSeed { get; set; }
        
        Vector3 Orientation { get; set; }
        
        bool Removable { get; set; }
        
        Vector3 BlockPosition { get; set; }
        
        bool PlaySpawningAnimation { get; set; }
        
        float Speed { get; set; }

        float Health { get; set; }

        bool InUpdateRange { get; }

        bool IsActive { get; set; }
        
        bool IsBoss { get; set; }
        
        bool IsDead { get; set; }
        
        bool IsFlying { get; set; }
        
        bool IsFriendly { get; }
        
        bool IsGrounded { get; set; }
        
        bool IsHumanoid { get; }
        
        bool IsImmune { get; }
        
        bool IsInvisible { get; set; }
        
        bool IsStatic { get; }
        
        bool IsUnderwater { get; set; }
        
        bool Knocked { get; }
        
        float MaxHealth { get; }
        
        MobType MobType { get; set; }
        
        BaseUpdatableModel Model { get; set; }
        
        string Name { get; set; }

        float Oxygen { get; set; }

        Vector3 Position { get; set; }

        Vector3 Rotation { get; set; }

        string Type { get; set; }

        void ShowIcon(CacheItem? IconType);

        void ShowIcon(CacheItem? IconType, float Seconds);

        void Damage(float Amount, Entity Damager, out float Exp, bool PlaySound = true);

        bool InAttackRange(Entity Target, float RadiusModifier = 1f);

        void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition);

        void AddBonusSpeedForSeconds(float BonusSpeed, float Seconds);

        void AddComponentWhile(EntityComponent Component, Func<bool> Condition);

        void AddComponentForSeconds(EntityComponent Component, float Seconds);

        void AddComponent(EntityComponent Component);

        void RemoveComponent(EntityComponent Component);

        T SearchComponent<T>() where T : EntityComponent;

        void UpdateEnviroment();

        void KnockForSeconds(float Time);

        void SpawnAnimation();

        void Dispose();

        void Draw();

        void Update();
    }
}