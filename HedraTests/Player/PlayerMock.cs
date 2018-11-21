using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.BoatSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.WeaponSystem;
using OpenTK;

namespace HedraTests.Player
{
    public class PlayerMock : IPlayer
    {
        public event OnHitLandedEventHandler OnHitLanded;
        public SimpleMessageDispatcherMock MessageMock => MessageDispatcher as SimpleMessageDispatcherMock;
        public SimpleCameraMock CameraMock => View as SimpleCameraMock;
        public PlayerMock()
        {
            MessageDispatcher = new SimpleMessageDispatcherMock();
            Movement = new SimpleMovementMock(this);
            Model = new HumanoidModel(this, new HumanoidModelTemplate
            {
                Colors = new ColorTemplate[0],
                Name = string.Empty,
                Path = string.Empty,
                Scale = 0
            });
        }

        public void SplashEffect(Chunk UnderChunk)
        {
            throw new NotImplementedException();
        }

        public IPhysicsComponent Physics { get; }
        public event OnAttackEventHandler AfterAttacking;
        public event OnAttackEventHandler BeforeAttacking;
        public EntityComponentManager ComponentManager { get; }
        public float AttackDamage { get; set; }
        public float AttackCooldown { get; set; }
        public float RandomFactor { get; set; }
        public float AttackResistance { get; set; }
        public float ManaRegenFactor { get; set; }
        public int Gold { get; set; }
        public float DamageEquation { get; }
        public bool IsTravelling { get; set; }
        public bool IsFishing { get; set; }

        public void Roll(RollType Type)
        {
            throw new NotImplementedException();
        }

        public void SetWeapon(Weapon Item)
        {
            throw new NotImplementedException();
        }

        public void AttackSurroundings(float Damage, Action<Entity> Callback)
        {
            throw new NotImplementedException();
        }

        public void AttackSurroundings(float Damage)
        {
            throw new NotImplementedException();
        }

        public void ApplyEffectWhile(EffectType NewType, Func<bool> Condition)
        {
            throw new NotImplementedException();
        }

        public void AddBonusAttackSpeedWhile(float BonusAttackSpeed, Func<bool> Condition)
        {
            throw new NotImplementedException();
        }

        public void AddBonusHealthWhile(float BonusHealth, Func<bool> Condition)
        {
            throw new NotImplementedException();
        }

        public void ProcessHit(bool HittedSomething)
        {
            throw new NotImplementedException();
        }

        public void Greet()
        {
            throw new NotImplementedException();
        }

        public Item MainWeapon { get; }
        public float BaseSpeed { get; }
        public bool Destroy { get; set; }
        public float Stamina { get; set; }
        public int Level { get; set; } = 1;
        public float AttackPower { get; set; }
        public float MaxStamina { get; set; }
        public float AddonHealth { get; set; }
        public float DodgeCost { get; set; }
        public float MaxOxygen { get; set; }
        public int MobId { get; set; }
        public int MobSeed { get; set; }
        public Vector3 Orientation { get; set; }
        public bool Removable { get; set; }
        public Vector3 BlockPosition { get; set; }
        public bool PlaySpawningAnimation { get; set; }
        public float Speed { get; set; }
        public float MaxMana { get; set; } = 100;
        public float Health { get; set; }
        public float Mana { get; set; } = 100;
        public bool InUpdateRange { get; }
        public bool IsActive { get; set; }
        public bool IsBoss { get; set; }
        public bool IsDead { get; set; }
        public bool IsFlying { get; set; }
        public bool IsSailing { get; set; }
        public bool IsFriendly { get; }
        public bool IsGrounded { get; set; }
        public bool IsHumanoid { get; }
        public bool IsImmune { get; }
        public bool IsInvisible { get; set; }
        public bool IsStatic { get; }
        public bool IsUnderwater { get; set; }
        public bool IsKnocked { get; }
        public float MaxHealth { get; set; }
        public float ManaRegen { get; }
        public float HealthRegen { get; }
        public float WeaponModifier(Item Weapon)
        {
            throw new NotImplementedException();
        }

        public Weapon LeftWeapon { get; set; }
        public MobType MobType { get; set; }
        public IMessageDispatcher MessageDispatcher { get; set; }
        public ICamera View { get; set; } = new SimpleCameraMock();
        public ChunkLoader Loader { get; }
        public UserInterface UI { get; set; }
        public IPlayerInventory Inventory { get; set; }
        public EntitySpawner Spawner { get; }
        public IToolbar Toolbar { get; set; }
        public IVehicle Boat { get; }
        public QuestLog QuestLog { get; }
        public IAbilityTree AbilityTree { get; set; }
        public PetManager Pet { get; }
        public Chat Chat { get; }
        public Minimap Minimap { get; }
        public Map Map { get; }
        public TradeInventory Trade { get; }
        public IVehicle Glider { get; }
        public int ConsecutiveHits { get; }
        public bool IsAttacking { get; set; }
        public bool IsEating { get; set; }
        public bool IsCasting { get; set; }
        public bool IsSwimming { get; set; }
        public bool IsGliding { get; set; }
        public bool IsTied { get; set; }
        public bool IsRolling { get; set; }
        public bool IsMoving { get; set; }
        public bool IsRiding { get; set; }
        public bool IsClimbing { get; set; }
        public bool WasAttacking { get; set; }
        public float AttackSpeed { get; set; }
        public float BaseAttackSpeed { get; }
        public bool CanInteract { get; set; }
        public bool IsSleeping { get; set; }
        public bool IsJumping { get; }
        public float FacingDirection { get; }
        public bool IsSitting { get; set; }
        public bool HasWeapon => LeftWeapon != null;
        public HandLamp HandLamp { get; }
        public HumanoidModel Model { get; set; }
        public MovementManager Movement { get; }
        public ClassDesign Class { get; set; }
        public float XP { get; set; }
        public float MaxXP { get; }

        BaseUpdatableModel IEntity.Model
        {
            get => Model;
            set => Model = value as HumanoidModel;
        }

        public string Name { get; set; }
        public float Oxygen { get; set; }
        public Vector3 Position { get; set; }
        public ICollidable[] NearCollisions { get; }
        public bool Enabled { get; set; }
        public void Respawn()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public Vector3 Rotation { get; set; }
        public Vector3 Size { get; }
        public string Type { get; set; }
        public void ShowIcon(CacheItem? IconType)
        {
            throw new NotImplementedException();
        }

        public void ShowIcon(CacheItem? IconType, float Seconds)
        {
            throw new NotImplementedException();
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound = true, bool PushBack = true)
        {
            throw new NotImplementedException();
        }

        public bool InAttackRange(IEntity Target, float RadiusModifier = 1)
        {
            throw new NotImplementedException();
        }

        public void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition)
        {
            Speed += BonusSpeed;
        }

        public void AddBonusSpeedForSeconds(float BonusSpeed, float Seconds)
        {
            Speed += BonusSpeed;
        }

        public void AddComponentWhile(IComponent<IEntity> Component, Func<bool> Condition)
        {
            throw new NotImplementedException();
        }

        public void AddComponentForSeconds(IComponent<IEntity> Component, float Seconds)
        {
            throw new NotImplementedException();
        }

        public void AddComponent(IComponent<IEntity> Component)
        {
            throw new NotImplementedException();
        }

        public void RemoveComponent(IComponent<IEntity> Component)
        {
            throw new NotImplementedException();
        }

        public T SearchComponent<T>()
        {
            throw new NotImplementedException();
        }

        public T[] GetComponents<T>()
        {
            throw new NotImplementedException();
        }

        public void UpdateEnvironment()
        {
            throw new NotImplementedException();
        }

        public void KnockForSeconds(float Time)
        {
            throw new NotImplementedException();
        }

        public void SpawnAnimation()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        void IEntity.Update()
        {
            throw new NotImplementedException();
        }

        public void InvokeBeforeAttack(IEntity Invoker, float Damage)
        {
            throw new NotImplementedException();
        }

        public void InvokeAfterAttack(IEntity Invoker, float Damage)
        {
            throw new NotImplementedException();
        }

        void IUpdatable.Update()
        {
            throw new NotImplementedException();
        }
    }
}