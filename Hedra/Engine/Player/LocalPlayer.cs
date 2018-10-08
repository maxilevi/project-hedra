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
using Hedra.Engine.ClassSystem;
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
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.BoatSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.Geometry;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
	public class LocalPlayer : Humanoid, IPlayer
	{
		public ICamera View { get; }
		public ChunkLoader Loader { get; }
		public UserInterface UI { get; set; }
		public IVehicle Boat { get; }
		public IVehicle Glider { get; }
		public IPlayerInventory Inventory { get; }
		public EntitySpawner Spawner { get; }
		public IToolbar Toolbar { get; }
		public QuestLog QuestLog { get; }
		public IAbilityTree AbilityTree { get; }
		public PetManager Pet { get; }
		public Chat Chat { get; }
		public Minimap Minimap { get; }
		public Map Map { get; }
		public TradeInventory Trade { get; }
	    public IMessageDispatcher MessageDispatcher { get; set; }
	    public override float FacingDirection => -(View.TargetYaw * Mathf.Degree - 90f);
		public ICollidable[] NearCollisions => StructureAware.NearCollisions;
		private IStructureAware StructureAware { get; }
	    private float _acummulativeHealing;
        private bool _floating;
	    private bool _inCementery;
	    private float _cementeryTime;
	    private float _targetCementeryTime;
	    private Vector3 _previousPosition;
	    private bool _shouldUpdateTime;
	    private float _health;
	    private bool _wasSleeping;
	    private bool _enabled;
	    private float _oldCementeryTime;
	    private float _oldTime;
	    private bool _canInteract;

        public LocalPlayer()
        {
			this.UI = new UserInterface(this);
			this.View = new Camera(this);
			this.Loader = new ChunkLoader(this);
			this.Spawner = new EntitySpawner(this);
			this.Model = new HumanoidModel(this);
			this.Inventory = new PlayerInventory(this);
			this.Toolbar = new Toolbar(this);
			this.Glider = new HangGlider(this);
			this.AbilityTree = new AbilityTree(this);
			this.QuestLog = new QuestLog(this);
			this.Pet = new PetManager(this);
			this.Chat = new Chat(this);
			this.Minimap = new Minimap(this);
			this.Map = new Map(this);
			this.Trade = new TradeInventory(this);
            this.Movement = new PlayerMovement(this);
            this.MessageDispatcher = new VisualMessageDispatcher(this);
	        this.StructureAware = new StructureAware(this);
	        this.Boat = new Boat(this);
            this.BlockPosition = new Vector3(GameSettings.SpawnPoint);
			this.Physics.CanCollide = true;
			this.AttackSpeed = 0.75f;
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
				if (Key.R == Args.Key && !GameSettings.Paused && IsDead)
					Respawn();
			});
			this.SearchComponent<DamageComponent>().Delete = false;
        }

	    public override bool CanInteract
	    {
	        get => _canInteract;
	        set
	        {
	            _canInteract = value;
                if(DmgComponent != null)
	                DmgComponent.Immune = !value;
	        }
	    }

        public override void Draw()
        {
			base.Draw();
			Map.Draw();
            try
            {
                var entities = World.Entities.ToArray();
                for (int i = entities.Length - 1; i > -1; i--)
                {
                    if (!(entities[i] is LocalPlayer) &&
                        (entities[i].Position.Xz - this.Position.Xz).LengthSquared < 256 * 256 ||
                        Pet.Pet == entities[i])
                    {
                        entities[i].Draw();
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException || e is NullReferenceException)
                    Log.WriteLine("Syncronization exception while reading entities.");
                else
                    throw;
            }
        }

        public override void Update()
        {
            base.Update();

            if (this.IsUnderwater && this.IsRiding)
				this.IsRiding = false;

            if (this.IsSleeping != _wasSleeping)
            {
                SkyManager.DaytimeSpeed = _wasSleeping ? 1.0f : 120.0f;
                //GameSettings.DarkEffect = !_wasSleeping;
            }
            _wasSleeping = this.IsSleeping;       
			
			//Dont cull the back chunk so that shadows can render
			Vector2 chunkPos = World.ToChunkSpace(this.Position);
            Chunk underChunk = World.GetChunkAt(this.Position);
			
			//START CEMENTERY
			bool wasInCementery = _inCementery;
			_inCementery = false;

            var structures = World.Structures;
            for (var i = structures.Count- 1; i > -1; i--)
		    {
		        var cementery = structures[i] as Graveyard;           
                if (cementery == null) continue;

		        Vector3 cementaryPosition = cementery.Position;

		        if ((cementaryPosition.Xz - this.Position.Xz).LengthSquared <
		            cementery.Radius * cementery.Radius * .5f * .5f && !cementery.Restored)
		        {
		            _inCementery = true;
		            break;
		        }
		    }
		    
		    if(_inCementery && !wasInCementery){
				_cementeryTime = SkyManager.DayTime;
				if(_cementeryTime < 12000) SkyManager.DayTime += 24000;
				SkyManager.Enabled = false;
				_targetCementeryTime = GraveyardDesign.GraveyardSkyTime;
				_shouldUpdateTime = true;
				SoundManager.PlayUISound(SoundType.DarkSound);
			}
			else if (!_inCementery && wasInCementery){
				_targetCementeryTime = _cementeryTime;
				if(this._cementeryTime < 12000) _targetCementeryTime += 24000;
				_shouldUpdateTime = true;
				SkyManager.Enabled = false;
			}

		    if(_shouldUpdateTime){
				SkyManager.SetTime( Mathf.Lerp(SkyManager.DayTime, _targetCementeryTime, (float) Time.DeltaTime * 2f) );
				if( Math.Abs(SkyManager.DayTime - _targetCementeryTime) < 10 )
				{
					_shouldUpdateTime = false;
					SkyManager.Enabled = true;
					if( SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
				}
			}
			//END CEMENTERY
			
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
			
			if(this.IsRiding){
				if(this.Position.Y < -1f)
					_floating = true;
				if(_floating)
					this.Physics.TargetPosition += Vector3.UnitY * 15 * Time.DeltaTime;
				if(this.Position.Y > 1f)
					_floating = false;
				if(Model.MountModel == null || Model.MountModel != null && Model.MountModel.Disposed) IsRiding = false;
			}

            try
            {
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
            }
            catch (ArgumentException e)
            {
                Log.WriteLine("Syncronization exception while reading entities.");
            }

            if (!this.IsDead)
            {
                if(!this.DmgComponent.HasBeenAttacked)
                    this.Health += HealthRegen * Time.IndependantDeltaTime;
                this.Mana += ManaRegen * Time.IndependantDeltaTime;
                this.Stamina += (float)Time.DeltaTime * 4f;
            }
            this.Rotation = new Vector3(0, this.Rotation.Y, 0);

            var underBlock0 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (0 + IsoSurfaceCreator.WaterQuadOffset));
			var underBlock1 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (1 + IsoSurfaceCreator.WaterQuadOffset));
			var underBlock2 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (2 + IsoSurfaceCreator.WaterQuadOffset));
			var underBlock3 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (3 + IsoSurfaceCreator.WaterQuadOffset));
			int lowestY = World.GetLowestY( (int) View.CameraPosition.X, (int) View.CameraPosition.Z);
			
			//Log.WriteLine(UnderBlock0.Type);
			if(underBlock0.Type != BlockType.Water && ( View.CameraPosition.Y / Chunk.BlockSize >= lowestY + 2 &&  underBlock1.Type != BlockType.Water && underBlock2.Type != BlockType.Water && underBlock3.Type != BlockType.Water)){
				GameSettings.DistortEffect = false;
				GameSettings.UnderWaterEffect = false;
				WorldRenderer.ShowWaterBackfaces = false;
            }
			if( underBlock0.Type == BlockType.Water || (View.CameraPosition.Y / Chunk.BlockSize <= lowestY + 2 && (underBlock1.Type == BlockType.Water || underBlock2.Type == BlockType.Water || underBlock3.Type == BlockType.Water))){
				GameSettings.UnderWaterEffect = true;
				GameSettings.DistortEffect = true;
			    WorldRenderer.ShowWaterBackfaces = true;
            }
            this.View.AddonDistance = IsMoving || IsSwimming || IsTravelling ? 3.0f : 0.0f;

	        StructureAware.Update();
            Loader.Update();
            Inventory.Update();
            AbilityTree.Update();
            Toolbar.Update();
			UI.Update();
			ManageSounds();
			QuestLog.Update();
			Pet.Update();
			Chat.Update();
			Map.Update();
			Trade.Update();
	        Boat.Update();
	        Glider.Update();
            View.Update();
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

	    public void EatFood(){
			if(this.IsDead || this.IsEating || this.IsKnocked || this.IsEating || this.IsAttacking || this.IsClimbing) return;
			this.WasAttacking = false;
			this.IsAttacking = false;
		    
			if(Inventory.Food != null){
				var foodHealth = Inventory.Food.GetAttribute<float>("Saturation");
                var foodAmount = Inventory.Food.GetAttribute<int>(CommonAttributes.Amount);
				Model.SetFood(Inventory.Food);
				Model.Eat(foodHealth);

				if(foodAmount > 1)
				    Inventory.Food.SetAttribute(CommonAttributes.Amount, foodAmount-1);
                else
					this.Inventory.SetItem(this.Inventory.IndexOf(Inventory.Food), null);
			}
			this.Inventory.UpdateInventory();
		}

	    private void ManageDeath()
	    {
	        if (_health <= 0f)
	        {
	            IsDead = true;
                CoroutineManager.StartCoroutine(this.DmgComponent.DisposeCoroutine);
	            Executer.ExecuteOnMainThread(delegate {
	                this.MessageDispatcher.ShowMessageWhile("[R] TO RESPAWN", Color.White,
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
		
		public void ManageSounds(){

		}	
		
		public override float Health{
			get => _health;
		    set
			{
			    value = Mathf.Clamp(value, 0, this.MaxHealth);
                var diff = value - _health;
			    _acummulativeHealing += diff < 0 ? 0 : diff;
                if (_acummulativeHealing > MaxHealth * .05f)
			    {
			        if (Model.Enabled)
			        {
			            var newLabel = new Billboard(2.0f, $"+ {(int) _acummulativeHealing} HP", Color.GreenYellow,
			                FontCache.Get(AssetManager.BoldFamily,
			                    18 + 12 * ((_acummulativeHealing - MaxHealth * .05f) / this.MaxHealth),
			                    FontStyle.Bold), this.Model.HeadPosition);
			            newLabel.Vanish = true;
			            newLabel.Speed = 4;
			        }
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

        public void Unload()
        {
			if(_inCementery){
				_oldCementeryTime = SkyManager.DayTime;
				SkyManager.DayTime = _cementeryTime;
			}
            _oldTime = float.MaxValue;
            if (SkyManager.StackLength > 0)
            {
                _oldTime = SkyManager.PeekTime();
                SkyManager.PopTime();
            }
		}
		
		public void Load()
		{
			if(_inCementery){
				SkyManager.DayTime = _oldCementeryTime;
			}

		    if (_oldTime != float.MaxValue)
		    {
		        SkyManager.DayTime = _oldTime;
		        SkyManager.PushTime();
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
		    var newPosition = newOffset + this.BlockPosition;
		    newPosition = World.FindPlaceablePosition(this, new Vector3(newPosition.X, PhysicsSystem.Physics.HeightAtPosition(newPosition.X, newPosition.Z), newPosition.Z));
            this.Model.Position = newPosition;
		    this.Physics.TargetPosition = newPosition;
		    this.IsKnocked = false;

		    var xpDiff = (int)(XP * .15f);
		    var goldDiff = (int)(Gold * .1f);
		    XP = Math.Max(XP - xpDiff, 0);
		    Gold = (int)(Gold - goldDiff);
		    if (xpDiff > 0)
		    {
		        var xp = new Billboard(6f, $"- {xpDiff} XP", Color.Purple,
		            FontCache.Get(AssetManager.BoldFamily, 18f, FontStyle.Bold), this.Model.HeadPosition + Vector3.UnitY * 1f)
		        {
		            Vanish = true,
		            Speed = 2,
		            FollowFunc = () => this.Model.HeadPosition + Vector3.UnitY * 1f
                };
		    }
		    if (goldDiff > 0)
		    {
		        var gold = new Billboard(6f, $"- {goldDiff} G", Color.Gold,
		            FontCache.Get(AssetManager.BoldFamily, 18f, FontStyle.Bold), this.Model.HeadPosition + Vector3.UnitY * 3f)
		        {
		            Vanish = true,
		            Speed = 2,
		            FollowFunc = () => this.Model.HeadPosition + Vector3.UnitY * 2f
		        };
		    }
		}

	    public void Reset()
	    {
	        Inventory.ClearInventory();
	        ComponentManager.Clear();
		    Chat.Clear();
            //UI.ShowMenu();
            Model.Alpha = 0f;
	        View.TargetPitch = 0f;
	        View.TargetYaw = 0f;
	        View.TargetDistance = 10f;
	        HandLamp.Enabled = false;
            IsTravelling = false;
	        IsKnocked = false;	
		    Spawner.Enabled = true;
		    HandLamp.Enabled = false;
		    View.Reset();
		    IsRiding = false;
		    if (Health <= 0) Respawn();
	    }
		
		public static bool CreatePlayer(string Name, HumanoidModel PreviewModel, ClassDesign ClassType)
        {
			if(Name == string.Empty)
			{
				Instance.MessageDispatcher.ShowNotification("Name cannot be empty", Color.Red, 3f);
				return false;
			}
			if(DataManager.CharacterCount >= 3)
			{
			    Instance.MessageDispatcher.ShowNotification("You cannot have more than 3 characters", Color.Red, 3f);
				return false;
			}
			if(Name.Length > 12)
			{
			    Instance.MessageDispatcher.ShowNotification("Name cannot be that long", Color.Red, 3f);
				return false;
			}
		    var data = new PlayerInformation
		    {
		        Name = Name,
		        RandomFactor = LocalPlayer.NewRandomFactor(),
		        WorldSeed = World.RandomSeed,
		        Class = ClassType
		    };

		    var gold = ItemPool.Grab(ItemType.Gold);
            gold.SetAttribute(CommonAttributes.Amount, 5);
		    data.AddItem(PlayerInventory.GoldHolder, gold );

		    var food = ItemPool.Grab(ItemType.Berry);
            food.SetAttribute(CommonAttributes.Amount, 5);
			data.AddItem( PlayerInventory.FoodHolder, food);

			data.AddItem(PlayerInventory.WeaponHolder, ClassType.StartingItem);

			DataManager.SavePlayer(data);
		    return true;
		}
	}
}
