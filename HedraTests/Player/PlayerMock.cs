using System;
using Hedra.Crafting;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.BoatSystem;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.WeaponSystem;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.Bullet;
using Moq;

namespace HedraTests.Player
{
    public class PlayerMock : IPlayer
    {
        public event OnHitLandedEventHandler HitLanded;
        public event OnInteractionEvent Interact;
        public event OnFishing Fishing;
        public event OnDeadEvent OnDeath;
        public SimpleMessageDispatcherMock MessageMock => MessageDispatcher as SimpleMessageDispatcherMock;
        public SimpleCameraMock CameraMock => View as SimpleCameraMock;
        public PlayerMock()
        {
            MessageDispatcher = new SimpleMessageDispatcherMock();
            Movement = new SimpleMovementMock(this);
            Model = new HumanoidModel(this, new HumanoidModelTemplate
            {
                Path = string.Empty,
                Scale = 0
            });
            UI = new UserInterface(this);
            var physicsMock = new Mock<IPhysicsComponent>();
            physicsMock.Setup(P => P.StaticRaycast(It.IsAny<Vector3>())).Returns(false);
            Physics = physicsMock.Object;
        }

        public void SplashEffect(Chunk UnderChunk)
        {
            throw new NotImplementedException();
        }

        public IPhysicsComponent Physics { get; }
        public event OnComponentAdded ComponentAdded;
        public event OnDamagingEventHandler AfterDamaging;
        public event OnDamagingEventHandler BeforeDamaging;
        public event OnDamageModifierEventHandler DamageModifiers;
        public event OnKillEventHandler Kill;
        public event OnDisposedEvent AfterDisposed;
        public event OnAttackEventHandler BeforeAttack;
        public event OnAttackEventHandler AfterAttack;
        public EntityComponentManager ComponentManager { get; }
        public EntityAttributes Attributes { get; } = new EntityAttributes();
        public float AttackDamage { get; set; }
        public float AttackCooldown { get; set; }
        public float RandomFactor { get; set; }
        public float AttackResistance { get; set; }
        public float ManaRegenFactor { get; set; }
        public int Gold { get; set; }
        public void SetGold(int Amount, bool Silent = false)
        {
            throw new NotImplementedException();
        }

        public float DamageEquation { get; }
        public float UnRandomizedDamageEquation { get; }
        public Vector3 LookingDirection { get; }

        public void AttackSurroundings(float Damage, IEntity[] ToIgnore, Action<IEntity> Callback)
        {
            throw new NotImplementedException();
        }

        public void AttackSurroundings(float Damage, IEntity[] ToIgnore)
        {
            throw new NotImplementedException();
        }

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

        public void SetHelmet(Item Item)
        {
            throw new NotImplementedException();
        }

        public void SetChestplate(Item Item)
        {
            throw new NotImplementedException();
        }

        public void SetPants(Item Item)
        {
            throw new NotImplementedException();
        }

        public void SetBoots(Item Item)
        {
            throw new NotImplementedException();
        }

        public void SetHelmet(HelmetPiece Item)
        {
            throw new NotImplementedException();
        }

        public void SetChestplate(ChestPiece Item)
        {
            throw new NotImplementedException();
        }

        public void SetPants(PantsPiece Item)
        {
            throw new NotImplementedException();
        }

        public void SetBoots(BootsPiece Item)
        {
            throw new NotImplementedException();
        }

        public void InvokeBeforeAttackEvent(AttackOptions Options)
        {
            throw new NotImplementedException();
        }

        public void InvokeAfterAttackEvent(AttackOptions Options)
        {
            throw new NotImplementedException();
        }

        public void AttackSurroundings(float Damage, Action<IEntity> Callback)
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

        public void ProcessHit(bool HittedSomething, bool InvokeBeforeAttackEvent = true)
        {
            throw new NotImplementedException();
        }

        public void ProcessHit(bool HittedSomething)
        {
            throw new NotImplementedException();
        }

        public void ResetEquipment()
        {
            throw new NotImplementedException();
        }

        public void Greet()
        {
            throw new NotImplementedException();
        }

        public Item MainWeapon { get; }
        Item[] IHumanoid.GetMainEquipment()
        {
            throw new NotImplementedException();
        }

        public void SetMainEquipment(Item[] Equipment)
        {
            throw new NotImplementedException();
        }

        public void DoIgnoringHitCombo(Action Lambda)
        {
            throw new NotImplementedException();
        }

        public void AddOrDropItem(Item Item)
        {
            throw new NotImplementedException();
        }

        public void RegisterInteraction(InteractableStructure Structure)
        {
        }

        public void RegisterFishing(Item FishedObject)
        {
        }

        public Item[] GetMainEquipment { get; }
        public Item Ring { get; set; }
        public float BaseSpeed { get; }
        public bool Destroy { get; set; }
        public float Stamina { get; set; }
        public float StaminaRegen { get; set; }
        public float AttackingSpeedModifier { get; }
        public int Level { get; set; } = 1;
        public float AttackPower { get; set; }
        public float MaxStamina { get; set; }
        public float BonusHealth { get; set; }
        public float DodgeCost { get; set; }
        public float MaxOxygen { get; set; }
        public int MobId { get; set; }
        public int Seed { get; set; }
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
        public bool IsFriendly { get; set; }
        public bool IsGrounded { get; set; }
        public bool IsHumanoid { get; }
        public bool IsImmune { get; }
        public bool IsInvisible { get; set; }
        public bool IsStatic { get; }
        public bool IsUnderwater { get; set; }
        public bool IsKnocked { get; }
        public bool Disposed { get; }
        public float MaxHealth { get; set; }
        public float ManaRegen { get; }
        public float Armor { get; set; }
        public float HealthRegen { get; }
        public float WeaponModifier(Item Weapon)
        {
            throw new NotImplementedException();
        }

        public Weapon LeftWeapon { get; set; }
        public MobType MobType { get; set; }
        public event OnMoveEvent OnMove;
        public event OnRespawnEvent OnRespawn;
        public IMessageDispatcher MessageDispatcher { get; set; }
        public ICamera View { get; set; } = new SimpleCameraMock();
        public ChunkLoader Loader { get; }
        public UserInterface UI { get; set; }
        public IPlayerInventory Inventory { get; set; }
        public CustomizationData Customization { get; set; }
        public MobSpawner Spawner { get; }
        public IToolbar Toolbar { get; set; }
        public IVehicle Boat { get; }
        public QuestInterface QuestInterface { get; }
        public IAbilityTree AbilityTree { get; set; }
        public EquipmentHandler Equipment { get; }
        public RealmHandler Realms { get; }
        public CompanionHandler Companion { get; }
        public Chat Chat { get; }
        public Minimap Minimap { get; }
        public Map Map { get; }
        public TradeInventory Trade { get; }
        public IVehicle Glider { get; }
        public int ConsecutiveHits { get; }
        public bool IsAttacking { get; set; }
        public bool IsStuck { get; set; }
        public bool IsSprinting { get; }
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
        public bool InAttackStance { get; set; }
        public bool IsUndead { get; }
        public float AttackSpeed { get; set; }
        public float BaseAttackSpeed { get; }
        public bool IsInsideABuilding { get; set; }
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
        public void SetXP(float Amount, bool Silent)
        {
            throw new NotImplementedException();
        }

        public float MaxXP { get; }

        BaseUpdatableModel IEntity.Model
        {
            get => Model;
            set => Model = value as HumanoidModel;
        }

        public string Name { get; set; }
        public float Oxygen { get; set; }
        public Vector3 Position { get; set; }
        public CollisionGroup[] NearCollisions { get; }
        public CraftingInventory Crafting { get; }
        public QuestInventory Questing { get; }
        public IStructureAware StructureAware { get; }
        public void ShowInventoryFor(Item Bag)
        {
            
        }

        public bool InterfaceOpened { get; }
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

        public void HideInterfaces()
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

        public void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted, bool PlaySound = true,
            bool PushBack = true)
        {
            throw new NotImplementedException();
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted, bool PlaySound, bool PushBack,
            DamageType Type)
        {
            throw new NotImplementedException();
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted)
        {
            throw new NotImplementedException();
        }

        public void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition, bool ShowParticles = true)
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

        public void RemoveComponent(IComponent<IEntity> Component, bool Dispose = true)
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

        public void RemoveComponentsOfType<T>() where T : IComponent<IEntity>
        {
            throw new NotImplementedException();
        }

        public T[] GetComponents<T>()
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

        public void InvokeBeforeDamaging(IEntity Invoker, float Damage)
        {
            throw new NotImplementedException();
        }

        public void InvokeAfterDamaging(IEntity Invoker, float Damage)
        {
            throw new NotImplementedException();
        }

        public void InvokeDamageModifier(IEntity Invoker, ref float Damage)
        {
            throw new NotImplementedException();
        }

        public void UpdateCriticalComponents()
        {
            throw new NotImplementedException();
        }

        public bool UpdateWhenOutOfRange { get; set; }

        void IUpdatable.Update()
        {
            throw new NotImplementedException();
        }

        public bool CanCastSkill => true;
        public void SetSkillPoints(Type Skill, int Points)
        {
            AbilityTree.SetPoints(Skill, Points);
        }

        public T SearchSkill<T>() where T : AbstractBaseSkill
        {
            throw new NotImplementedException();
        }

        public Animation AnimationBlending { get; }
        public void ResetModel()
        {
            throw new NotImplementedException();
        }

        public void PlayAnimation(Animation Animation)
        {
            throw new NotImplementedException();
        }

        public void BlendAnimation(Animation Animation)
        {
            throw new NotImplementedException();
        }

        public bool CaptureMovement { get; set; }
        public void Orientate()
        {
            throw new NotImplementedException();
        }
    }
}