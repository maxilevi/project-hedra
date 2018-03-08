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
using OpenTK;
using Hedra.Engine.Sound;
using Hedra.Engine.Scenes;
using Hedra.Engine.Rendering;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI; 
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Item;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
	public class LocalPlayer : Humanoid, IUpdatable
	{

	    public const float DefaultSpeed = 1.25f;
		public Camera View;
		public ChunkLoader Loader;
		public UserInterface UI;
		public Inventory Inventory;
		public EntitySpawner Spawner;
		public SkillsBar Skills;
		public QuestLog QuestLog;
		public SkillTree SkillSystem;
		public PetManager Pet;
		public Chat Chat;
		public Minimap Minimap;
		public Map Map;
		public TradeSystem Trade;
		public GliderModel Glider;
	    public VisualMessageDispatcher MessageDispatcher;
        public ICollidable[] NearCollisions { get; private set; }

        private bool _floating;
	    private bool _inCementery;
	    private float _cementeryTime;
	    private float _targetCementeryTime;
	    private Vector3 _previousPosition;
	    private bool _shouldUpdateTime;
	    private int _emitted;
	    private float _health;
	    private bool _wasSleeping;	    

	    public LocalPlayer(){
			this.UI = new UserInterface(this);
			this.View = new Camera(this);
			this.Loader = new ChunkLoader(this);
			this.Spawner = new EntitySpawner(this);
			this.Model = new HumanModel(this);
			this.Inventory = new Inventory(this);
			this.Skills = new SkillsBar(this);
			this.Glider = new GliderModel();
			this.SkillSystem = new SkillTree(this);
			this.QuestLog = new QuestLog(this);
			this.Pet = new PetManager(this);
			this.Chat = new Chat(this);
			this.Minimap = new Minimap(this);
			this.Map = new Map(this);
			this.Trade = new TradeSystem(this);
            this.MessageDispatcher = new VisualMessageDispatcher(this);

            this.BlockPosition = new Vector3(Constants.WORLD_OFFSET * .5f);
			this.Physics.CanCollide = true;
			this.AttackSpeed = 1.25f;
	        this.AttackPower = 1.0f;
			this.Speed = DefaultSpeed;
	        this.DefaultBox.Max = new Vector3(2.5f, 5, 2.5f);

	        this.SetupHandlers();

            World.AddEntity(this);
			DrawManager.Add(this);
			UpdateManager.Add(this);
		}

        #region SetupHandlers
        public void SetupHandlers()
	    {
	        float pHealth = 0, pMaxHealth = 0;

	        this.BeforeAttacking += delegate (Entity Victim, float Amount) {
	            if (Pet.MountEntity != null && Victim == Pet.MountEntity)
	            {
	                pMaxHealth = Pet.MountEntity.MaxHealth;
	                pHealth = Pet.MountEntity.Health;

	                Pet.MountEntity.MaxHealth = Amount + 1;
	                Pet.MountEntity.Health = Pet.MountEntity.MaxHealth;
	            }
	        };

	        this.OnAttacking += delegate (Entity Victim, float Amount) {
	            if (Pet.MountEntity != null && Victim == Pet.MountEntity)
	            {
	                Pet.MountEntity.MaxHealth = pMaxHealth;
	                Pet.MountEntity.Health = pHealth;
	                var Dmg = Pet.MountEntity.SearchComponent<DamageComponent>();
	                for (int i = Dmg.DamageLabels.Count - 1; i > -1; i--)
	                {
	                    if (Dmg.DamageLabels[i].Texture is GUIText)
	                    {
	                        if ((Dmg.DamageLabels[i].Texture as GUIText).Text == ((int)Amount).ToString())
	                        {
	                            Dmg.DamageLabels[i].Dispose();
	                            Dmg.DamageLabels.RemoveAt(i);
	                        }
	                    }
	                }
	            }

	            if ((Victim.Position - Position).LengthSquared > 128 * 128)
	            {
	                Victim.Health += Amount;
	                var dmg = Victim.SearchComponent<DamageComponent>();
	                for (int i = dmg.DamageLabels.Count - 1; i > -1; i--)
	                {
	                    if (!(dmg.DamageLabels[i].Texture is GUIText)) continue;

	                    if (((GUIText)dmg.DamageLabels[i].Texture).Text != ((int)Amount).ToString()) continue;

	                    dmg.DamageLabels[i].Dispose();
	                    dmg.DamageLabels.RemoveAt(i);
	                }
	            }

	            if (Networking.NetworkManager.IsConnected && !Networking.NetworkManager.IsHost)
	                Networking.NetworkManager.RegisterAttack(Victim, Amount);

	        };

	        EventDispatcher.RegisterKeyDown(typeof(ClaimableStructure), delegate (object sender, KeyboardKeyEventArgs e)
	        {
	            if (e.Key != Key.E || GameSettings.Paused) return;

	            for (var i = 0; i < World.Structures.Count; i++)
	            {
	                if (!(World.Structures[i] is ClaimableStructure)) continue;
	                var claimable = (ClaimableStructure)World.Structures[i];

	                if (!((this.Position - claimable.Position).LengthSquared < claimable.ClaimDistance * claimable.ClaimDistance) ||
	                    !(Vector3.Dot((claimable.Position - this.Position).NormalizedFast(),
	                          this.View.LookAtPoint.NormalizedFast()) > .6f)) continue;

	                claimable.Claim(this);
	            }

	            for (var i = 0; i < World.Entities.Count; i++)
	            {
	                var claimableComponent = World.Entities[i].SearchComponent<BerryBushComponent>();
	                claimableComponent?.Interact(this);
	            }
	        });
        }
        #endregion

	    private bool _canInteract;
	    public override bool CanInteract
	    {
	        get { return _canInteract; }
	        set
	        {
	            _canInteract = value;
                if(DmgComponent != null)
	                DmgComponent.Immune = !value;
	        }
	    }

        public override void Draw(){
			if(Oxygen != MaxOxygen && !GameSettings.Paused && UI.GamePanel.Enabled)
				UI.GamePanel.Oxygen = true;
			else
				UI.GamePanel.Oxygen = false;
			
			if(Stamina != MaxStamina && !GameSettings.Paused && UI.GamePanel.Enabled && !this.IsUnderwater)
				UI.GamePanel.Stamina = true;
			else
				UI.GamePanel.Stamina = false;
			
			UI.Draw();
			base.Draw();
			Map.Draw();

            try
            {
                var entities = World.Entities.ToArray();
                for (int i = entities.Length - 1; i > -1; i--)
                {
                    if (!(entities[i] is LocalPlayer) &&
                        (entities[i].Position.Xz - this.Position.Xz).LengthSquared < 256 * 256 ||
                        Pet.MountEntity == entities[i])
                    {
                        entities[i].Draw();
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Log.WriteLine("Syncronization exception while reading entities.");
            }
        }

        public new void Update(){
			base.Update();
			
			if(this.IsUnderwater && this.IsRiding)
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
            Chunk backChunk = World.GetChunkAt( chunkPos.ToVector3() - View.CrossDirection * Chunk.ChunkWidth * 1f );
			if(backChunk?.Mesh != null){
				backChunk.Mesh.DontCull = true;
				//BackChunk.Mesh.OnlyShadows = true;
			}
			
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
				SoundManager.PlaySoundInPlayersLocation(SoundType.DarkSound);
			}
			else if (!_inCementery && wasInCementery){
				_targetCementeryTime = _cementeryTime;
				if(this._cementeryTime < 12000) _targetCementeryTime += 24000;
				_shouldUpdateTime = true;
				SkyManager.Enabled = false;
			}

            List<CollidableStructure> collidableStructures = null;
            lock (World.StructureGenerator.Items)
            {
                collidableStructures = (from item in World.StructureGenerator.Items
                    where (item.Position.Xz - this.Position.Xz).LengthSquared < item.Design.Radius * item.Design.Radius
                              select item).ToList();
            }

            CollidableStructure nearCollidableStructure = collidableStructures.FirstOrDefault();

            this.NearCollisions = null;
            if (nearCollidableStructure != null)
		    {

		        if ((nearCollidableStructure.Position.Xz - this.Position.Xz).LengthFast < nearCollidableStructure.Design.Radius && nearCollidableStructure.Design is VillageDesign)
		            SoundtrackManager.PlayAmbient(SoundtrackManager.VillageIndex);

                NearCollisions = nearCollidableStructure.Colliders;
		    }	    

		    if(_shouldUpdateTime){
				SkyManager.SetTime( Mathf.Lerp(SkyManager.DayTime, _targetCementeryTime, (float) Time.deltaTime * 2f) );
				if( Math.Abs(SkyManager.DayTime - _targetCementeryTime) < 10 ){//Little difference
					_shouldUpdateTime = false;
					SkyManager.Enabled = true;
					if( SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
				}
			}
			//END CEMENTERY
			
			if( this.Model.Enabled && (_previousPosition - Model.Human.BlockPosition).LengthFast > 0.25f && Model.Human.IsGrounded && underChunk != null){
				World.WorldParticles.VariateUniformly = true;
				World.WorldParticles.Color = World.GetHighestBlockAt( (int) Model.Human.Position.X, (int) Model.Human.Position.Z).GetColor(underChunk.Biome.Colors);
				World.WorldParticles.Position = Model.Human.Position - Vector3.UnitY;
				World.WorldParticles.Scale = Vector3.One * .25f;
				World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				World.WorldParticles.Direction = (-Model.Human.Orientation + Vector3.UnitY * 1.5f) * .15f;
				World.WorldParticles.ParticleLifetime = 1;
				World.WorldParticles.GravityEffect = .1f;
				World.WorldParticles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
				if(World.WorldParticles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
					World.WorldParticles.Color = underChunk.Biome.Colors.GrassColor;
				
				if(_emitted >= 3){
					World.WorldParticles.Emit();
					_emitted = 0;
				}
				_emitted++;
			    _previousPosition = Model.Human.BlockPosition;
            }
			
			if(this.IsRiding){
				if(this.Position.Y < -1f)
					_floating = true;
				if(_floating)
					this.Physics.TargetPosition += Vector3.UnitY * 15 * (float) Time.deltaTime;
				if(this.Position.Y > 1f)
					_floating = false;
				if(Model.MountModel == null || Model.MountModel != null && Model.MountModel.Disposed) IsRiding = false;
			}

            try
            {
                var entities = World.Entities.ToArray();
                for (int i = entities.Length - 1; i > -1; i--)
                {
                    LocalPlayer player = SceneManager.Game.LPlayer;
                    if (entities[i] != player && entities[i].InUpdateRange && !GameSettings.Paused &&
                        !SceneManager.Game.IsLoading

                        || Pet.MountEntity == entities[i] || entities[i].IsBoss)
                    {

                        entities[i].Update();
                    }
                    else if (entities[i] != player && entities[i].InUpdateRange && GameSettings.Paused)
                    {
                        (entities[i].Model as IAudible)?.StopSound();
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Log.WriteLine("Syncronization exception while reading entities.");
            }

            if (!this.IsDead)
            {
                this.Health += HealthRegen * Time.FrameTimeSeconds;
                this.Mana += ManaRegen * Time.FrameTimeSeconds;
                this.Stamina += (float)Time.deltaTime * 4f;
            }
            this.Rotation = new Vector3(0, this.Rotation.Y, 0);
			
			Model.FacingDirection = View.Yaw * Mathf.Degree - Mathf.Degree - 25;
			
			if(View.PlayerMode){
				if(IsGrounded && !this.IsFlying)
					View.Position = new Vector3(Model.Position.X, View.Position.Y + (Model.Position.Y - View.Position.Y) * (float) Time.deltaTime * 4f , Model.Position.Z);
				else
					View.Position = new Vector3(Model.Position.X, Model.Position.Y, Model.Position.Z);
			}
			
			if(this.IsGliding && this.IsUnderwater || this.IsGliding && SceneManager.Game.InMenuWorld)
				this.IsGliding = false;
			
			
			if(this.IsGliding && !IsGrounded){		
					
				this.Glider.Enabled = true;
				this.Glider.Position = this.Position + Vector3.UnitY * 5f;
				
				float AngleX = -45 * Mathf.Clamp(View.Pitch, -.5f, .5f);

				this.Glider.BaseMesh.Rotation = new Vector3(AngleX, Model.Model.Rotation.Y, 0);
				this.Glider.RotationPoint = Vector3.UnitY * 5.5f;
				
				this.Physics.GravityDirection = -Vector3.UnitY * .3f;
				this.Physics.VelocityCap = 4.5f;
				this.Physics.Move( this.View.LookAtPoint.NormalizedFast() * 7.5f * 3.5f * 3f * new Vector3(1f,.35f, 1f));
				this.Physics.ResetFall();
				this.Model.TargetRotation = new Vector3(AngleX, -this.Model.FacingDirection, 0);
				this.Model.Model.Rotation = new Vector3(AngleX, -this.Model.FacingDirection, 0);
				this.Model.Glide();
				
				World.WorldParticles.Color = Vector4.One;
				World.WorldParticles.Position = this.Glider.Position - Vector3.UnitY * 10f;
				World.WorldParticles.ParticleLifetime = 1f;
				World.WorldParticles.GravityEffect = .0f;
				World.WorldParticles.Direction = Vector3.Zero;
				World.WorldParticles.Scale = new Vector3(.5f,.5f,.5f);
				_emitted++;
				if(_emitted % 4 == 0)
					World.WorldParticles.Emit();

			}else{
				if(this.Glider.Enabled){//Player was Gliding
					
					this.Model.TargetRotation = new Vector3(0, Model.TargetRotation.Y, 0);
					//this.Glider.BaseMesh.Rotation = new Vector3(0, Model.Model.Rotation.Y, 0);
					this.Glider.Enabled = false;
					this.Physics.GravityDirection = -Vector3.UnitY;
					this.Physics.VelocityCap = float.MaxValue;
				}
			}
			

			Block underBlock0 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (0 + IsoSurfaceCreator.WaterQuadOffset));
			Block underBlock1 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (1 + IsoSurfaceCreator.WaterQuadOffset));
			Block underBlock2 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (2 + IsoSurfaceCreator.WaterQuadOffset));
			Block underBlock3 = World.GetBlockAt(Mathf.DivideVector(View.CameraPosition, new Vector3(1,Chunk.BlockSize,1)) + Vector3.UnitY * (3 + IsoSurfaceCreator.WaterQuadOffset));
			int lowestY = World.GetLowestY( (int) View.CameraPosition.X, (int) View.CameraPosition.Z);
			
			//Log.WriteLine(UnderBlock0.Type);
			if(underBlock0.Type != BlockType.Water && ( View.CameraPosition.Y / Chunk.BlockSize >= lowestY + 2 &&  underBlock1.Type != BlockType.Water && underBlock2.Type != BlockType.Water && underBlock3.Type != BlockType.Water)){
				GameSettings.DistortEffect = false;
				GameSettings.UnderWaterEffect = false;
				WorldRenderer.ShowWaterBackfaces = false;
			    WaterMeshBuffer.ShowBackfaces = false;
            }
			if( underBlock0.Type == BlockType.Water || (View.CameraPosition.Y / Chunk.BlockSize <= lowestY + 2 && (underBlock1.Type == BlockType.Water || underBlock2.Type == BlockType.Water || underBlock3.Type == BlockType.Water))){
				GameSettings.UnderWaterEffect = true;
				GameSettings.DistortEffect = true;
			    WorldRenderer.ShowWaterBackfaces = true;
			    WaterMeshBuffer.ShowBackfaces = true;
            }

			
			if(IsGrounded)
				IsGliding = false;

            this.View.AddonDistance = this.IsMoving || this.IsSwimming || this.IsGliding ? 3.0f : 0.0f;

            //this.Physics.PushAround = !IsAttacking; // If he is attacking dont push 'em	

			Movement.Update();
			UI.Update();
			SoundManager.Update(Position);
			ManageSounds();
			QuestLog.Update();
			Pet.Update();
			Chat.Update();
			Minimap.Update();
			Map.Update();
			Trade.Update();
            View.Update();
        }
		
		public override InventoryItem MainWeapon => Inventory.MainWeapon;

	    public override InventoryItem Ring => Inventory.Ring;

	    public void EatFood(){
			if(this.IsDead || this.IsEating || this.Knocked || this.IsEating || this.IsAttacking || this.IsClimbing) return;
			this.WasAttacking = false;
			this.IsAttacking = false;
			this.Model.LeftWeapon.SlowDown = false;
			
			if(Inventory.Items[Inventory.FoodHolder] != null){
				InventoryItem FoodItem = Inventory.Items[Inventory.FoodHolder];
				float FoodHealth = Item.ItemPool.MaterialInfo(FoodItem.Info.MaterialType).AttackPower;
				this.PlayEatAnimation(FoodHealth);
				if(FoodItem.Info.Damage > 1){
					FoodItem.Info = new Item.ItemInfo(FoodItem.Info.MaterialType, FoodItem.Info.Damage-1);
				}else{
					//It's 1
					this.Inventory.SetItem(null, Inventory.FoodHolder);
				}
			}
			this.Inventory.UpdateInventory();
		}
		
		private void PlayEatAnimation(float FoodHealth){
			if(Model != null){
				Model.Food = new StaticModel();
				VertexData FoodData = Inventory.Items[Inventory.FoodHolder].MeshFile.Clone();
				FoodData.Scale( Vector3.One * 1.5f );
				Model.Food.Mesh = EntityMesh.FromVertexData( FoodData, Vector3.Zero);
				Model.Food.Mesh.Enabled= true;
			}
			Model.Food.Init();
			this.IsEating = true;
			Model.Eat(FoodHealth);
			this.IsEating = true;
		}
		
		public void ManageSounds(){

		}
		
		
		public override float Health{
			get{ return _health; }
			set{
				_health = Mathf.Clamp(value,0,MaxHealth);
				if(_health <= 0f){
					IsDead = true;
					CoroutineManager.StartCoroutine(this.DmgComponent.DisposeCoroutine);
					if(GameSettings.Hardcore)
						ThreadManager.ExecuteOnMainThread( delegate{ this.MessageDispatcher.ShowMessageWhile("[R] NEW RUN", Color.White,
						        () => this.Health <= 0); } );
					else
						ThreadManager.ExecuteOnMainThread( delegate{ this.MessageDispatcher.ShowMessageWhile("[R] TO RESPAWN", Color.White,
                                () => this.Health <= 0 && Constants.CHARACTER_CHOOSED); } );
				}else{
					IsDead = false;
				    Model?.Recompose();
				}
			}
		}
		
		private bool _enabled;
		public bool Enabled{
			get{ return _enabled; }
			set{
				_enabled = value;
				Model.Enabled = false;
			}
		}
		
		public static LocalPlayer Instance => SceneManager.Game.LPlayer;

	    private float _oldCementeryTime;
	    private float _oldSpeed;
	    private float _oldTime;
        public void UnLoad(){
			if(_inCementery){
				_oldCementeryTime = SkyManager.DayTime;
				SkyManager.DayTime = _cementeryTime;
			}
		    if (Model.MountModel != null)
		    {
		        _oldSpeed = this.Speed;
		        this.Speed = Model.MountModel.Parent.SearchComponent<RideComponent>().RiderSpeed;
		    }
            _oldTime = float.MaxValue;
            if (SkyManager.StackLength > 0)
            {
                _oldTime = SkyManager.PeekTime();
                SkyManager.PopTime();
            }
		}
		
		public void Load(){
			if(_inCementery){
				SkyManager.DayTime = _oldCementeryTime;
			}

		    if (Model.MountModel != null)
		    {
		        this.Speed = _oldSpeed;
		    }

		    if (_oldTime != float.MaxValue)
		    {
		        SkyManager.DayTime = _oldTime;
		        SkyManager.PushTime();
            }
        }
		
		public void Respawn(){
			Model.Idle();
			Health = MaxHealth;
			Mana = MaxMana;
		    Stamina = MaxStamina;
		    this.PlaySpawningAnimation = true;
		    this.IsRiding = false;
            var newOffset = new Vector3( (192f * Utils.Rng.NextFloat() - 96f) * Chunk.BlockSize, 0, (192f * Utils.Rng.NextFloat() - 96f) * Chunk.BlockSize);
		    var newPosition = newOffset + this.Model.Position;
		    newPosition = new Vector3(newPosition.X, PhysicsSystem.Physics.HeightAtPosition(newPosition.X, newPosition.Z), newPosition.Z);
            this.Model.Position = newPosition;
		    this.Physics.TargetPosition = newPosition;
		}
		
		public static bool CreatePlayer(string Name, HumanModel PreviewModel, Class ClassType){
			if(Name == String.Empty){
				Instance.MessageDispatcher.ShowNotification("Name cannot be empty", Color.DarkRed, 3f);
				return false;
			}
			if(DataManager.CharacterCount >= 3){
			    Instance.MessageDispatcher.ShowNotification("You cannot have more than 3 characters", Color.DarkRed, 3f);
				return false;
			}
			if(Name.Length > 12){
			    Instance.MessageDispatcher.ShowNotification("Name cannot be that long", Color.DarkRed, 3f);
				return false;
			}
		    var Data = new PlayerData
		    {
		        Name = Name,
		        RandomFactor = Utils.Rng.NextFloat() * .35f + .75f,
		        Color1 = PreviewModel.Color0,
		        Color0 = PreviewModel.Color1,
		        WorldSeed = World.RandomSeed,
		        ClassType = ClassType
		    };

		    Data.Items.Add( new InventoryItem(ItemType.Coin, Item.ItemInfo.Gold(5)), Inventory.CoinHolder );
			var BerryInfo = new ItemInfo(Material.Berry, 5);
			Data.Items.Add( new InventoryItem(ItemType.Food, BerryInfo), Inventory.FoodHolder);
			var ItemInfo = new ItemInfo(Material.Copper, 6);
			ItemInfo.ModelSeed = 2;
			
			
			if(ClassType == Class.Warrior)
				Data.Items.Add( new InventoryItem( ItemType.Sword, ItemInfo), Inventory.WeaponHolder);
			
			else if(ClassType == Class.Archer)
				Data.Items.Add( new InventoryItem( ItemType.Bow, ItemInfo), Inventory.WeaponHolder);
			
			else if(ClassType == Class.Rogue)
				Data.Items.Add( new InventoryItem( ItemType.DoubleBlades, ItemInfo), Inventory.WeaponHolder);
			
			else if(ClassType == Class.Mage || ClassType == Class.Necromancer)
				Data.Items.Add( new InventoryItem( ItemType.ThrowableDagger, ItemInfo), Inventory.WeaponHolder);
			
			DataManager.SavePlayer(Data);
		    return true;
		}
	}
	
	public enum Class{
		None,
		Archer,
		Rogue,
		Warrior,
		Mage,
		Necromancer
	}
}
