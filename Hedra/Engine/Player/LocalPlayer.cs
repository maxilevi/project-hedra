/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 01:29 a.m.
 *
 */

using System;
using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.Core;
using Hedra.Crafting;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.FoodSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering.UI;
using Hedra.Sound;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player
{
    public class LocalPlayer : Humanoid, IPlayer
    {
        private const int MinimumRespawnDistance = 32;
        private const int MaximumRespawnDistance = 128;
        private float _acummulativeHealing;
        private bool _canInteract;
        private DamageComponent _damageHandler;
        private bool _enabled;
        private float _health;
        private Vector3 _previousPosition;
        private bool _wasSleeping;

        public LocalPlayer()
        {
            UI = new UserInterface(this);
            View = new Camera(this);
            Loader = new ChunkLoader(this);
            Spawner = new MobSpawner(this);
            Model = new HumanoidModel(this);
            StructureAware = new StructureAware(this);
            Inventory = new PlayerInventory(this);
            Companion = new CompanionHandler(this);
            InventoryInterface = new PlayerInventoryInterface(this);
            Crafting = new CraftingInventory(Inventory);
            CraftingInterface = new CraftingInterface(this);
            Toolbar = new Toolbar(this);
            Glider = new HangGlider(this);
            AbilityTree = new AbilityTree(this);
            Questing = new QuestInventory(this);
            QuestInterface = new QuestInterface(this);
            Chat = new Chat(this);
            Minimap = new Minimap(this);
            Map = new Map(this);
            Trade = new TradeInventory(this);
            Movement = new PlayerMovement(this);
            MessageDispatcher = new VisualMessageDispatcher(this);
            AmbientEffects = new AmbientEffectHandler(this);
            Realms = new RealmHandler();
            Physics.CollidesWithStructures = true;
            AttackPower = 1.0f;

            SetupHandlers();

            World.AddEntity(this);
            DrawManager.Add(this);
            UpdateManager.Add(this);
        }

        public IVehicle Glider { get; }
        private PlayerInventoryInterface InventoryInterface { get; }
        private QuestInterface QuestInterface { get; }
        private CraftingInterface CraftingInterface { get; }
        public MobSpawner Spawner { get; }
        public Map Map { get; }
        private IAmbientEffectHandler AmbientEffects { get; }

        public static IPlayer Instance => GameManager.Player;
        public event OnRespawnEvent OnRespawn;
        public event OnDeadEvent OnDeath;

        public ICamera View { get; }
        public ChunkLoader Loader { get; }
        public UserInterface UI { get; set; }
        public RealmHandler Realms { get; }
        public QuestInventory Questing { get; }
        public CraftingInventory Crafting { get; }
        public IToolbar Toolbar { get; }
        public IAbilityTree AbilityTree { get; }
        public IStructureAware StructureAware { get; }
        public CompanionHandler Companion { get; }
        public Chat Chat { get; }
        public Minimap Minimap { get; }
        public TradeInventory Trade { get; }
        public override Vector3 LookingDirection => View.LookingDirection;
        public override float FacingDirection => -(View.TargetYaw * Mathf.Degree - 90f);
        public CollisionGroup[] NearCollisions => StructureAware.NearCollisions;

        public override bool CanInteract
        {
            get => _canInteract;
            set
            {
                _canInteract = value;
                _damageHandler.Immune = !value;
            }
        }

        public override void Draw()
        {
            base.Draw();
            Map.Draw();
            var entities = World.Entities.ToArray();
            for (var i = entities.Length - 1; i > -1; i--)
                if (!(entities[i] is LocalPlayer) &&
                    (entities[i].Position.Xz() - Position.Xz()).LengthSquared() < 64 * 64 ||
                    Companion.Entity == entities[i])
                    entities[i].Draw();

            if (GameSettings.DebugNavMesh)
            {
                var structs = StructureHandler.GetNearStructures(Position);
                for (var i = 0; i < structs.Length; ++i) structs[i].Draw();
            }
        }

        public override void Update()
        {
            UpdateCriticalComponents();
            base.Update();

            if (IsUnderwater && IsRiding)
                IsRiding = false;

            if (IsSleeping != _wasSleeping) SkyManager.DaytimeSpeed = _wasSleeping ? 1.0f : 40.0f;
            _wasSleeping = IsSleeping;
            var underChunk = World.GetChunkAt(Position);

            if (Model.Enabled && (_previousPosition - Model.Human.Position).LengthFast() > 0.25f &&
                Model.Human.IsGrounded && underChunk != null)
            {
                World.Particles.VariateUniformly = true;
                World.Particles.Color =
                    World.GetHighestBlockAt((int)Model.Human.Position.X, (int)Model.Human.Position.Z)
                        .GetColor(underChunk.Biome.Colors);
                World.Particles.Position = Model.Human.Position - Vector3.UnitY;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-Model.Human.Orientation + Vector3.UnitY * 1.5f) * .15f;
                World.Particles.ParticleLifetime = 1;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
                if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                    World.Particles.Color = Vector4.Zero;

                if ((int)Time.AccumulatedFrameTime % 2 == 0) World.Particles.Emit();

                _previousPosition = Model.Human.Position;
            }

            if (Companion.Entity != null && !Companion.Entity.Disposed)
            {
                Companion.Entity?.Update();
                Companion.Entity?.UpdateCriticalComponents();
            }

            Rotation = new Vector3(0, Rotation.Y, 0);
            View.AddedDistance = IsMoving || IsSwimming || IsTravelling ? 3.0f : 0.0f;
            AmbientEffects.Update();
            StructureAware.Update();
            Loader.Update();
            InventoryInterface.Update();
            CraftingInterface.Update();
            QuestInterface.Update();
            AbilityTree.Update();
            Toolbar.Update();
            UI.Update();
            Companion.Update();
            Chat.Update();
            Map.Update();
            Trade.Update();
            Boat.Update();
            Glider.Update();
            View.Update();
            Realms.Update();
            Spawner.Update();
            Questing.Update();
        }

        public void ShowInventoryFor(Item Bag)
        {
            InventoryInterface.ShowInventoryForItem(Bag);
        }

        public override int Gold
        {
            get => Inventory.Search(I => I.IsGold)?.GetAttribute<int>(CommonAttributes.Amount) ?? 0;
            set => SetGold(value, false);
        }

        public override Item MainWeapon => Inventory.MainWeapon;

        public override float Health
        {
            get => _health;
            set
            {
                value = Mathf.Clamp(value, 0, MaxHealth);
                var diff = value - _health;
                _acummulativeHealing += diff < 0 ? 0 : diff;
                if (_acummulativeHealing > MaxHealth * .05f)
                {
                    this.ShowText(Model.HeadPosition, $"+ {(int)_acummulativeHealing} HP", Color.GreenYellow,
                        18 + 12 * ((_acummulativeHealing - MaxHealth * .05f) / MaxHealth), 2.0f);
                    _acummulativeHealing = 0;
                }

                if (_health <= 0 && value > 0) PlaySpawningAnimation = true;
                _health = value;
                ManageDeath();
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                Model.Enabled = value;
            }
        }

        public override bool IsGliding => Glider.Enabled;

        public override bool IsSailing => Boat.Enabled;

        public bool InterfaceOpened => PlayerInterface.Showing;

        public void HideInterfaces()
        {
            InventoryInterface.Show = false;
            Trade.Show = false;
            CraftingInterface.Show = false;
            AbilityTree.Show = false;
            QuestInterface.Show = false;
        }

        public override bool IsTravelling
        {
            get => IsGliding || IsSailing;
            set
            {
                if (value) throw new ArgumentException("Travelling can't be enabled.");
                if (IsGliding) Glider.Disable();
                if (IsSailing) Boat.Disable();
            }
        }

        public void Respawn()
        {
            Health = MaxHealth;
            Mana = MaxMana;
            Stamina = MaxStamina;
            GameManager.SpawningEffect = true;
            PlaySpawningAnimation = true;
            IsRiding = false;
            var newOffset = Position;
            while ((Position - newOffset).LengthSquared() < MinimumRespawnDistance * MinimumRespawnDistance)
                newOffset = Position + new Vector3(
                    MaximumRespawnDistance * Utils.Rng.NextFloat() * 2 - MaximumRespawnDistance,
                    0,
                    MaximumRespawnDistance * Utils.Rng.NextFloat() * 2 - MaximumRespawnDistance
                );

            var newPosition = World.FindSpawningPoint(newOffset);
            newPosition = World.FindPlaceablePosition(this,
                new Vector3(newPosition.X, PhysicsSystem.Physics.HeightAtPosition(newPosition.X, newPosition.Z),
                    newPosition.Z));
            Model.Position = newPosition;
            Position = newPosition;
            IsKnocked = false;
            IsRolling = false;
            ComponentManager.Clear();

            var xpDiff = (int)(XP * .15f);
            var goldDiff = (int)(Gold * .1f);
            SetXP(Math.Max(XP - xpDiff, 0), true);
            SetGold(Gold - goldDiff, true);
            if (xpDiff > 0)
            {
                var xp = new TextBillboard(6f, $"- {xpDiff} XP", Color.Purple,
                    FontCache.GetBold(18f), () => Model.HeadPosition + Vector3.UnitY * 1f)
                {
                    Vanish = true,
                    VanishSpeed = 2
                };
            }

            if (goldDiff > 0)
            {
                var gold = new TextBillboard(6f, $"- {goldDiff} G", Color.Gold,
                    FontCache.GetBold(18f), () => Model.HeadPosition + Vector3.UnitY * 2f)
                {
                    Vanish = true,
                    VanishSpeed = 2
                };
            }

            OnRespawn?.Invoke();
        }

        public void Reset()
        {
            IsRiding = false;
            /* Finish removing the mount */
            Companion.Entity?.Update();
            Inventory.ClearInventory();
            ComponentManager.Clear();
            CraftingInterface.Reset();
            QuestInterface.Reset();
            Realms.Reset();
            Minimap.Reset();
            Chat.Clear();
            Armor = 0f;
            Model.Alpha = 0f;
            View.TargetPitch = 0f;
            View.TargetYaw = 0f;
            View.TargetDistance = 10f;
            HandLamp.Enabled = false;
            IsRolling = false;
            IsTravelling = false;
            IsKnocked = false;
            IsSleeping = false;
            ShowIcon(null);
            Spawner.Enabled = true;
            HandLamp.Enabled = false;
            DodgeCost = DefaultDodgeCost;
            Toolbar.ResetSkills();

            View.Reset();

            IsRiding = false;
            if (Health <= 0) Respawn();
        }

        public bool CanCastSkill => !IsEating && !IsRiding;

        public void SetSkillPoints(Type Skill, int Points)
        {
            AbilityTree.SetPoints(Skill, Points);
        }

        public T SearchSkill<T>() where T : AbstractBaseSkill
        {
            return (T)Toolbar.Skills.First(S => S is T);
        }

        public void Dispose()
        {
            UpdateManager.Remove(this);
            DrawManager.Remove(this);
            EventDispatcher.UnregisterKeyDown(this);
        }

        private void SetupHandlers()
        {
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs Args)
            {
                if (Controls.Respawn == Args.Key && !GameSettings.Paused && IsDead)
                    Respawn();

                if (Controls.Handlamp == Args.Key && !GameSettings.Paused && CanInteract)
                {
                    HandLamp.Enabled = !HandLamp.Enabled;
                    SoundPlayer.PlaySound(SoundType.NotificationSound, Position);
                }
            }, EventPriority.Low);

            Kill += A => { A.Victim.ShowText($"+{(int)Math.Ceiling(A.Experience)} XP", Color.Violet, 20); };

            _damageHandler = SearchComponent<DamageComponent>();
            _damageHandler.PushOnHit = false;
            _damageHandler.Delete = false;
            _damageHandler.OnDeadEvent += Args => OnDeath?.Invoke(Args);
        }

        private void SetGold(int Amount, bool Silent)
        {
            if (Amount < 0) return;
            var currentGold = Inventory.Search(I => I.IsGold);
            if (!Silent)
            {
                var isLessThanCurrent = Amount < currentGold.GetAttribute<int>(CommonAttributes.Amount);
                var sign = isLessThanCurrent ? "-" : "+";
                this.ShowText(Model.HeadPosition, $"{sign} {Math.Abs(Amount - Gold)} {Translations.Get("quest_gold")}",
                    isLessThanCurrent ? Color.Red : Color.Gold,
                    18);
            }

            if (currentGold == null)
            {
                var gold = ItemPool.Grab(ItemType.Gold);
                gold.SetAttribute(CommonAttributes.Amount, Amount);
                Inventory.AddItem(gold);
            }
            else
            {
                currentGold.SetAttribute(CommonAttributes.Amount, Amount);
            }
        }

        public void EatFood()
        {
            if (IsDead || IsEating || IsKnocked || IsEating || IsAttacking || IsClimbing) return;
            WasAttacking = false;
            IsAttacking = false;

            if (Inventory.Food != null)
            {
                var food = Inventory.Food;
                if (CanEat(food, out var shouldSit))
                {
                    if (shouldSit) IsSitting = true;
                    Model.EatFood(food, OnEatingEnd);
                    Inventory.RemoveItem(food);
                }
                else
                {
                    MessageDispatcher.ShowNotification(Translations.Get("cant_eat_while_moving"), Color.Red, 2f);
                }
            }

            Inventory.UpdateInventory();
        }

        private bool CanEat(Item Food, out bool ShouldSit)
        {
            ShouldSit = Food.HasAttribute(CommonAttributes.EatSitting)
                        && Food.GetAttribute<bool>(CommonAttributes.EatSitting);
            return ShouldSit && !IsMoving || !ShouldSit;
        }

        private void OnEatingEnd(Item Food)
        {
            FoodHandler.ApplyEffects(Food, this);
        }

        private void ManageDeath()
        {
            if (_health <= 0f)
            {
                IsDead = true;
                RoutineManager.StartRoutine(_damageHandler.DisposeCoroutine);
                Executer.ExecuteOnMainThread(delegate
                {
                    MessageDispatcher.ShowMessageWhile(Translations.Get("to_respawn", Controls.Respawn), Color.White,
                        () => Health <= 0 && !GameManager.InStartMenu);
                });
            }
            else
            {
                if (!IsDead) return;
                IsDead = false;
                Model?.Recompose();
            }
        }

        public static bool CreatePlayer(string Name, ClassDesign ClassType, CustomizationData Customization)
        {
            if (Name == string.Empty)
            {
                Instance.MessageDispatcher.ShowNotification(Translations.Get("name_empty"), Color.Red, 3f);
                return false;
            }

            if (DataManager.CharacterCount >= GameSettings.MaxCharacters)
            {
                Instance.MessageDispatcher.ShowNotification(
                    Translations.Get("max_characters", GameSettings.MaxCharacters), Color.Red,
                    GameSettings.MaxCharacters);
                return false;
            }

            const int maxName = 12;
            if (Name.Length > maxName)
            {
                Instance.MessageDispatcher.ShowNotification(Translations.Get("name_long", maxName), Color.Red, 3f);
                return false;
            }

            var information = BuildNewPlayer(Name, ClassType, Customization);
            DataManager.SavePlayer(information);
            return true;
        }

        public static PlayerInformation BuildNewPlayer(string Name, ClassDesign ClassType,
            CustomizationData Customization)
        {
            var data = new PlayerInformation
            {
                Name = Name,
                RandomFactor = NewRandomFactor(),
                Class = ClassType,
                Customization = Customization
            };

            var gold = ItemPool.Grab(ItemType.Gold);
            gold.SetAttribute(CommonAttributes.Amount, 5);
            data.AddItem(PlayerInventory.GoldHolder, gold);

            var food = ItemPool.Grab(ItemType.HealthPotion);
            food.SetAttribute(CommonAttributes.Amount, 5);
            data.AddItem(PlayerInventory.FoodHolder, food);

            data.AddRecipe(ItemPool.Grab(ItemType.HealthPotionRecipe).Name);
            var items = ClassType.StartingItems;
            for (var i = 0; i < items.Length; ++i) data.AddItem(items[i].Key, items[i].Value);

            var recipes = ClassType.StartingRecipes;
            for (var i = 0; i < recipes.Length; ++i) data.AddRecipe(recipes[i].Name);
            return data;
        }
    }
}