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
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI; 
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.Player.ToolbarSystem;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
	public class LocalPlayer : Humanoid, IUpdatable
	{

		public Camera View;
		public ChunkLoader Loader;
		public UserInterface UI;
		public PlayerInventory Inventory;
		public EntitySpawner Spawner;
		public Toolbar Toolbar;
		public QuestLog QuestLog;
		public AbilityTreeSystem.AbilityTree AbilityTree;
		public PetManager Pet;
		public Chat Chat;
		public Minimap Minimap;
		public Map Map;
		public TradeInventory Trade;
		public GliderModel Glider;
	    public override IMessageDispatcher MessageDispatcher { get; set; }
        public ICollidable[] NearCollisions { get; private set; }
	    private float _acummulativeHealing;
        private bool _floating;
	    private bool _inCementery;
	    private float _cementeryTime;
	    private float _targetCementeryTime;
	    private Vector3 _previousPosition;
	    private bool _shouldUpdateTime;
	    private int _emitted;
	    private float _health;
	    private bool _wasSleeping;
	    private bool _enabled;
	    private float _oldCementeryTime;
	    private float _oldTime;
	    private bool _wasPlayingAmbient;

        public LocalPlayer(){
			this.UI = new UserInterface(this);
			this.View = new Camera(this);
			this.Loader = new ChunkLoader(this);
			this.Spawner = new EntitySpawner(this);
			this.Model = new HumanModel(this);
			this.Inventory = new PlayerInventory(this);
			this.Toolbar = new Toolbar(this);
			this.Glider = new GliderModel();
			this.AbilityTree = new AbilityTreeSystem.AbilityTree(this);
			this.QuestLog = new QuestLog(this);
			this.Pet = new PetManager(this);
			this.Chat = new Chat(this);
			this.Minimap = new Minimap(this);
			this.Map = new Map(this);
			this.Trade = new TradeInventory(this);
            this.MessageDispatcher = new VisualMessageDispatcher(this);
            this.BlockPosition = new Vector3(GameSettings.SpawnPoint);
			this.Physics.CanCollide = true;
			this.AttackSpeed = 0.75f;
	        this.AttackPower = 1.0f;

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
	            if (Pet.Pet != null && Victim == Pet.Pet)
	            {
	                pMaxHealth = Pet.Pet.MaxHealth;
	                pHealth = Pet.Pet.Health;

	                Pet.Pet.MaxHealth = Amount + 1;
	                Pet.Pet.Health = Pet.Pet.MaxHealth;
	            }
	        };

	        this.OnAttacking += delegate (Entity Victim, float Amount) {
	            if (Pet.Pet != null && Victim == Pet.Pet)
	            {
	                Pet.Pet.MaxHealth = pMaxHealth;
	                Pet.Pet.Health = pHealth;
	                var Dmg = Pet.Pet.SearchComponent<DamageComponent>();
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
            Chunk backChunk = World.GetChunkAt( chunkPos.ToVector3() - View.CrossDirection * Chunk.Width * 1f );
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
				SoundManager.PlayUISound(SoundType.DarkSound);
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

                if ((nearCollidableStructure.Position.Xz - this.Position.Xz).LengthFast <
                    nearCollidableStructure.Design.Radius && nearCollidableStructure.Design is VillageDesign)
                {
                    SoundtrackManager.PlayTrack(SoundtrackManager.VillageIndex, true);
                    _wasPlayingAmbient = true;
                }

                NearCollisions = nearCollidableStructure.Colliders;
            }
            else if(_wasPlayingAmbient)
            {
                _wasPlayingAmbient = false;
                SoundtrackManager.PlayTrack(SoundtrackManager.LoopableSongsStart);
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
				
				if(_emitted >= 3){
					World.Particles.Emit();
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
                    LocalPlayer player = GameManager.Player;
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
			
			if(this.IsGliding && this.IsUnderwater)
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
				
				World.Particles.Color = Vector4.One;
				World.Particles.Position = this.Glider.Position - Vector3.UnitY * 10f;
				World.Particles.ParticleLifetime = 1f;
				World.Particles.GravityEffect = .0f;
				World.Particles.Direction = Vector3.Zero;
				World.Particles.Scale = new Vector3(.5f,.5f,.5f);
				_emitted++;
				if(_emitted % 4 == 0)
					World.Particles.Emit();

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

            Inventory.Update();
            AbilityTree.Update();
            Toolbar.Update();
			Movement.Update();
			UI.Update();
			ManageSounds();
			QuestLog.Update();
			Pet.Update();
			Chat.Update();
			Map.Update();
			Trade.Update();
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
	            var currentGold = Inventory.Search(I => I.IsGold);
	            if (currentGold != null)
	                currentGold.SetAttribute(CommonAttributes.Amount, value);
	            else
	                Inventory.AddItem(ItemPool.Grab(ItemType.Gold));
	        }
	    }
		public override Item MainWeapon => Inventory.MainWeapon;

	    public void EatFood(){
			if(this.IsDead || this.IsEating || this.Knocked || this.IsEating || this.IsAttacking || this.IsClimbing) return;
			this.WasAttacking = false;
			this.IsAttacking = false;
			this.Model.LeftWeapon.SlowDown = false;
			
			if(Inventory.Food != null){
				var foodHealth = Inventory.Food.GetAttribute<float>("Saturation");
                var foodAmount = Inventory.Food.GetAttribute<int>(CommonAttributes.Amount);
                this.PlayEatAnimation(foodHealth);

				if(foodAmount > 1)
				    Inventory.Food.SetAttribute(CommonAttributes.Amount, foodAmount-1);
                else
					this.Inventory.SetItem(this.Inventory.IndexOf(Inventory.Food), null);
				
			}
			this.Inventory.UpdateInventory();
		}
		
		private void PlayEatAnimation(float FoodHealth){
			if(Model != null){
				Model.Food = new StaticModel();
				var foodData = Inventory.Food.Model.Clone();
				foodData.Scale( Vector3.One * 1.5f );

				Model.Food.Mesh = ObjectMesh.FromVertexData( foodData, Vector3.Zero);
				Model.Food.Mesh.Enabled= true;
			}
			Model?.Food.GatherMeshes();
			Model?.Eat(FoodHealth);
			this.IsEating = true;
		}

	    private void ManageDeath()
	    {
	        if (_health <= 0f)
	        {
	            IsDead = true;
	            CoroutineManager.StartCoroutine(this.DmgComponent.DisposeCoroutine);
	            if (GameSettings.Hardcore)
	                ThreadManager.ExecuteOnMainThread(delegate {
	                    this.MessageDispatcher.ShowMessageWhile("[R] NEW RUN", Color.White,
	                        () => this.Health <= 0);
	                });
	            else
	                ThreadManager.ExecuteOnMainThread(delegate {
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
			get{ return _health; }
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
			                FontCache.Get(AssetManager.Fonts.Families[0],
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

		public bool Enabled{
			get{ return _enabled; }
			set{
				_enabled = value;
				Model.Enabled = false;
			}
		}
		
		public static LocalPlayer Instance => GameManager.Player;

        public void UnLoad(){
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
		
		public void Load(){
			if(_inCementery){
				SkyManager.DayTime = _oldCementeryTime;
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
		    var newPosition = newOffset + this.BlockPosition;
		    newPosition = new Vector3(newPosition.X, PhysicsSystem.Physics.HeightAtPosition(newPosition.X, newPosition.Z), newPosition.Z);
            this.Model.Position = newPosition;
		    this.Physics.TargetPosition = newPosition;
		    this.Knocked = false;
		}
		
		public static bool CreatePlayer(string Name, HumanModel PreviewModel, ClassDesign ClassType){
			if(Name == string.Empty){
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
