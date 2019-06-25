/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 01:29 a.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.CraftingSystem;
using OpenTK;
using Hedra.Engine.Sound;
using Hedra.Engine.Rendering;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI; 
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.FoodSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.BoatSystem;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Sound;
using OpenTK.Input;
using KeyEventArgs = Hedra.Engine.Events.KeyEventArgs;

namespace Hedra.Engine.Player
{
    public class LocalPlayer : Humanoid, IPlayer
    {
        public event OnInteractionEvent Interact;
        public ICamera View { get; }
        public ChunkLoader Loader { get; }
        public UserInterface UI { get; set; }
        public IVehicle Boat { get; }
        public IVehicle Glider { get; }
        public IPlayerInventory Inventory { get; }
        public RealmHandler Realms { get; }
        private PlayerInventoryInterface InventoryInterface { get; }
        public QuestInventory Questing { get; }
        private QuestInterface QuestInterface { get; }
        public CraftingInventory Crafting { get; }
        private CraftingInterface CraftingInterface { get; }
        public EntitySpawner Spawner { get; }
        public IToolbar Toolbar { get; }
        public IAbilityTree AbilityTree { get; }
        public IStructureAware StructureAware { get; }
        public PetManager Pet { get; }
        public Chat Chat { get; }
        public Minimap Minimap { get; }
        public Map Map { get; }
        public TradeInventory Trade { get; }
        public override Vector3 LookingDirection => View.LookingDirection;
        public override float FacingDirection => -(View.TargetYaw * Mathf.Degree - 90f);
        public CollisionGroup[] NearCollisions => StructureAware.NearCollisions;
        private IAmbientEffectHandler AmbientEffects { get; }
        private float _acummulativeHealing;
        private Vector3 _previousPosition;
        private float _health;
        private bool _wasSleeping;
        private bool _enabled;
        private bool _canInteract;
        private DamageComponent _damageHandler;

        public LocalPlayer()
        {
            this.UI = new UserInterface(this);
            this.View = new Camera(this);
            this.Loader = new ChunkLoader(this);
            this.Spawner = new EntitySpawner(this);
            this.Model = new HumanoidModel(this);
            this.StructureAware = new StructureAware(this);
            this.Inventory = new PlayerInventory(this);
            this.InventoryInterface = new PlayerInventoryInterface(this);
            this.Crafting = new CraftingInventory(this.Inventory);
            this.CraftingInterface = new CraftingInterface(this);
            this.Toolbar = new Toolbar(this);
            this.Glider = new HangGlider(this);
            this.AbilityTree = new AbilityTree(this);
            this.Questing = new QuestInventory(this);
            this.QuestInterface = new QuestInterface(this);
            this.Pet = new PetManager(this);
            this.Chat = new Chat(this);
            this.Minimap = new Minimap(this);
            this.Map = new Map(this);
            this.Trade = new TradeInventory(this);
            this.Movement = new PlayerMovement(this);
            this.MessageDispatcher = new VisualMessageDispatcher(this);
            this.Boat = new Boat(this);
            this.AmbientEffects = new AmbientEffectHandler(this);
            this.Realms = new RealmHandler();
            this.Physics.CollidesWithStructures = true;
            this.AttackPower = 1.0f;

            this.SetupHandlers();

            World.AddEntity(this);
            DrawManager.Add(this);
            UpdateManager.Add(this);
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
                if(Controls.Interact == Args.Key)
                    Interact?.Invoke();
            }, EventPriority.Low);

            Kill += A =>
            {
                A.Victim.ShowText($"+{(int)Math.Ceiling(A.Experience)} XP", Color.Violet, 20);
            };
            
            _damageHandler = SearchComponent<DamageComponent>();
            _damageHandler.Delete = false;
        }

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
            {
                if (!(entities[i] is LocalPlayer) &&
                    (entities[i].Position.Xz - this.Position.Xz).LengthSquared < 256 * 256 ||
                    Pet.Pet == entities[i])
                {
                    entities[i].Draw();
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (this.IsUnderwater && this.IsRiding)
                this.IsRiding = false;

            if (this.IsSleeping != _wasSleeping)
            {
                SkyManager.DaytimeSpeed = _wasSleeping ? 1.0f : 40.0f;
                //GameSettings.DarkEffect = !_wasSleeping;
            }
            _wasSleeping = this.IsSleeping;       
            
            //Dont cull the back chunk so that shadows can render
            Vector2 chunkPos = World.ToChunkSpace(this.Position);
            Chunk underChunk = World.GetChunkAt(this.Position);

            
            if( this.Model.Enabled && (_previousPosition - Model.Human.BlockPosition).LengthFast > 0.25f && Model.Human.IsGrounded && underChunk != null){
                World.Particles.VariateUniformly = true;
                World.Particles.Color = World.GetHighestBlockAt( (int) Model.Human.Position.X, (int) Model.Human.Position.Z).GetColor(underChunk.Biome.Colors);
                World.Particles.Position = Model.Human.Position - Vector3.UnitY;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
                World.Particles.Direction = (-Model.Human.Orientation + Vector3.UnitY * 1.5f) * .15f;
                World.Particles.ParticleLifetime = 1;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
                if(World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                    World.Particles.Color = Vector4.Zero;
                
                if( (int) Time.AccumulatedFrameTime % 2 == 0) World.Particles.Emit();
                
                _previousPosition = Model.Human.BlockPosition;
            }

            var entities = World.Entities.ToArray();
            for (int i = entities.Length - 1; i > -1; i--)
            {
                var player = GameManager.Player;
                if (entities[i] != player && entities[i].InUpdateRange && !GameSettings.Paused &&
                    !GameManager.IsLoading

                    || Pet.Pet == entities[i] || entities[i].IsBoss)
                {

                    entities[i].Update();
                }
                else if (entities[i] != player && entities[i].InUpdateRange && GameSettings.Paused)
                {
                    (entities[i].Model as IAudible)?.StopSound();
                }
            }
            
            this.Rotation = new Vector3(0, this.Rotation.Y, 0);
            this.View.AddedDistance = IsMoving || IsSwimming || IsTravelling ? 3.0f : 0.0f;

            AmbientEffects.Update();
            StructureAware.Update();
            Loader.Update();
            InventoryInterface.Update();
            CraftingInterface.Update();
            QuestInterface.Update();
            AbilityTree.Update();
            Toolbar.Update();
            UI.Update();
            Pet.Update();
            Chat.Update();
            Map.Update();
            Trade.Update();
            Boat.Update();
            Glider.Update();
            View.Update();
            Realms.Update();
        }

        public override int Gold
        {
            get
            {
                return Inventory.Search(I => I.IsGold)?.GetAttribute<int>(CommonAttributes.Amount) ?? 0;
            }
            set
            {
                if (value < 0) return;
                this.ShowText(Model.HeadPosition, $"+ {value - Gold} {Translations.Get("quest_gold")}", Color.Gold, 18);
                var currentGold = Inventory.Search(I => I.IsGold);
                if (currentGold == null)
                {
                    var gold = ItemPool.Grab(ItemType.Gold);
                    gold.SetAttribute(CommonAttributes.Amount, value);
                    Inventory.AddItem(gold);
                }
                else
                {
                    currentGold.SetAttribute(CommonAttributes.Amount, value);
                }
            }
        }
        public override Item MainWeapon => Inventory.MainWeapon;

        public void EatFood()
        {
            if(this.IsDead || this.IsEating || this.IsKnocked || this.IsEating || this.IsAttacking || this.IsClimbing) return;
            this.WasAttacking = false;
            this.IsAttacking = false;
            
            if(Inventory.Food != null)
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
            this.Inventory.UpdateInventory();
        }

        private bool CanEat(Item Food, out bool ShouldSit)
        {
            ShouldSit = Food.HasAttribute(CommonAttributes.EatSitting) 
                             && Food.GetAttribute<bool>(CommonAttributes.EatSitting);
            return (ShouldSit && !IsMoving || !ShouldSit);
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
                Executer.ExecuteOnMainThread(delegate {
                    this.MessageDispatcher.ShowMessageWhile(Translations.Get("to_respawn", Controls.Respawn), Color.White,
                        () => this.Health <= 0 && !GameManager.InStartMenu);
                });
            }
            else
            {
                if (!IsDead) return;
                IsDead = false;
                Model?.Recompose();
            }
        }
 
        public override float Health
        {
            get => _health;
            set
            {
                value = Mathf.Clamp(value, 0, this.MaxHealth);
                var diff = value - _health;
                _acummulativeHealing += diff < 0 ? 0 : diff;
                if (_acummulativeHealing > MaxHealth * .05f)
                {
                    this.ShowText(Model.HeadPosition, $"+ {(int)_acummulativeHealing} HP", Color.GreenYellow, 18 + 12 * ((_acummulativeHealing - MaxHealth * .05f) / this.MaxHealth), 2.0f);
                    _acummulativeHealing = 0;
                }
                if (_health <= 0 && value > 0) this.PlaySpawningAnimation = true;
                _health = value;
                this.ManageDeath();
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set{
                _enabled = value;
                Model.Enabled = value;
            }
        }
        
        public static IPlayer Instance => GameManager.Player;

        public override bool IsGliding => Glider.Enabled;
        
        public override bool IsSailing => Boat.Enabled;

        public bool InterfaceOpened => InventoryInterface.Show || Trade.Show 
                                                               || CraftingInterface.Show 
                                                               || AbilityTree.Show 
                                                               || QuestInterface.Show;
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
            this.PlaySpawningAnimation = true;
            this.IsRiding = false;
            var newOffset = new Vector3( (192f * Utils.Rng.NextFloat() - 96f) * Chunk.BlockSize, 0, (192f * Utils.Rng.NextFloat() - 96f) * Chunk.BlockSize);
            var newPosition = World.FindSpawningPoint(newOffset + this.BlockPosition);
            newPosition = World.FindPlaceablePosition(this, new Vector3(newPosition.X, PhysicsSystem.Physics.HeightAtPosition(newPosition.X, newPosition.Z), newPosition.Z));
            this.Model.Position = newPosition;
            this.Physics.TargetPosition = newPosition;
            this.IsKnocked = false;
            IsRolling = false;

            var xpDiff = (int)(XP * .15f);
            var goldDiff = (int)(Gold * .1f);
            XP = Math.Max(XP - xpDiff, 0);
            Gold = (int)(Gold - goldDiff);
            if (xpDiff > 0)
            {
                var xp = new TextBillboard(6f, $"- {xpDiff} XP", Color.Purple,
                    FontCache.GetBold(18f), () => this.Model.HeadPosition + Vector3.UnitY * 1f)
                {
                    Vanish = true,
                    VanishSpeed = 2
                };
            }
            if (goldDiff > 0)
            {
                var gold = new TextBillboard(6f, $"- {goldDiff} G", Color.Gold,
                    FontCache.GetBold(18f), () => this.Model.HeadPosition + Vector3.UnitY * 2f)
                {
                    Vanish = true,
                    VanishSpeed = 2
                };
            }
        }

        public void Reset()
        {
            IsRiding = false;
            /* Finish removing the mount */
            Pet.Pet?.Update();
            Inventory.ClearInventory();
            ComponentManager.Clear();
            CraftingInterface.Reset();
            QuestInterface.Reset();
            Realms.Reset();
            Minimap.Reset();
            Chat.Clear();
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

        public T SearchSkill<T>() where T : BaseSkill
        {
            return (T) Toolbar.Skills.First(S => S is T);
        }
        
        public static bool CreatePlayer(string Name, ClassDesign ClassType)
        {
            if (Name == string.Empty)
            {
                Instance.MessageDispatcher.ShowNotification(Translations.Get("name_empty"), Color.Red, 3f);
                return false;
            }
            if (DataManager.CharacterCount >= GameSettings.MaxCharacters)
            {
                Instance.MessageDispatcher.ShowNotification(Translations.Get("max_characters", GameSettings.MaxCharacters), Color.Red, GameSettings.MaxCharacters);
                return false;
            }
            const int maxName = 12;
            if (Name.Length > maxName)
            {
                Instance.MessageDispatcher.ShowNotification(Translations.Get("name_long", maxName), Color.Red, 3f);
                return false;
            }

            var information = BuildNewPlayer(Name, ClassType);
            DataManager.SavePlayer(information);
            return true;
        }

        public static PlayerInformation BuildNewPlayer(string Name, ClassDesign ClassType)
        {
            var data = new PlayerInformation
            {
                Name = Name,
                RandomFactor = NewRandomFactor(),
                Class = ClassType
            };

            var gold = ItemPool.Grab(ItemType.Gold);
            gold.SetAttribute(CommonAttributes.Amount, 5);
            data.AddItem(PlayerInventory.GoldHolder, gold);

            var food = ItemPool.Grab(ItemType.HealthPotion);
            food.SetAttribute(CommonAttributes.Amount, 5);
            data.AddItem(PlayerInventory.FoodHolder, food);

            data.AddRecipe(ItemPool.Grab(ItemType.HealthPotionRecipe).Name);
            var items = ClassType.StartingItems;
            for (var i = 0; i < items.Length; ++i)
            {
                data.AddItem(items[i].Key, items[i].Value);
            }
            
            var recipes = ClassType.StartingRecipes;
            for (var i = 0; i < recipes.Length; ++i)
            {
                data.AddRecipe(recipes[i].Name);
            }
            return data;
        }

        public void Dispose()
        {
            UpdateManager.Remove(this);
            DrawManager.Remove(this);
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}
