/*
 * Author: Zaphyk
 * Date: 08/02/2016
 * Time: 02:19 a.m.
 *
 */
using Hedra.Engine.Item;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using System.Drawing;
using System.Collections;
using Hedra.Engine.QuestSystem.Objectives;

namespace Hedra.Engine.Scenes
{
	/// <summary>
	/// The Scene containing the main game
	/// </summary>
	public class GameScene : IScene
	{
	    public int Id { get; set; } = 1;
		public LocalPlayer LPlayer;
		public PlayerData CurrentData;
		private Texture _loadingScreen;
	    private GUIText _playerText;
	    private float _rotationY;
	    private bool _isNewRun;
	    private float _savedHealth, _savedMana;
        public bool IsLoading;

		public void LoadScene()
		{
		    World.Load();
			Log.WriteLine("World Created Successfully!");
			LPlayer = new LocalPlayer();
			Log.WriteLine("Player Created Successfully!");
			//World.ChunkGenerationQueue.Player = LPlayer;
			Log.WriteLine("Added Generation Queue");

			LPlayer.Model.Enabled = false;
			LPlayer.UI.ShowMenu();
			LPlayer.UI.ChrChooser.Disable(); 
			LPlayer.UI.Menu.Enable();
			
			Log.WriteLine("Setted up GUI");

			LPlayer.Enabled = false;
			Constants.DEBUG = false;

			_loadingScreen = new Texture(Color.FromArgb(255,29,29,29), Color.FromArgb(255,59,59,59), Vector2.Zero, Vector2.One, GradientType.TOP_BOT);
			_loadingScreen.Disable();

		    _playerText = new GUIText("", new Vector2(0, 0), Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 14, FontStyle.Bold));


            Log.WriteLine("Created GUI Notification Successfully");

			CoroutineManager.StartCoroutine(MenuCoroutine);
		}
		
		public bool InMenuWorld => World.Seed == World.MenuSeed;

	    public bool InMenu => LPlayer.UI.Menu.Enabled && !LPlayer.UI.Hide && World.Seed != World.MenuSeed;

	    public void LoadMenu(){
			Constants.REDIRECT_NET = false;
			Constants.REDIRECT_NEW_RUN = false;
	        World.Recreate(World.MenuSeed);
            LocalPlayer.Instance.UI.ShowMenu();
          	Constants.CHARACTER_CHOOSED = false;
          	CoroutineManager.StartCoroutine(SceneManager.Game.MenuCoroutine);
          	LocalPlayer.Instance.Model.Enabled = false;
          	Enviroment.SkyManager.SetTime(12000);
          	LocalPlayer.Instance.View.Pitch = 0f;
          	LocalPlayer.Instance.HandLamp.Enabled = false;
          	LocalPlayer.Instance.View.Yaw = 0f;
          	LocalPlayer.Instance.View.TargetDistance = 10f;
	        LocalPlayer.Instance.IsGliding = false;
	        LocalPlayer.Instance.Glider.Enabled = false;
          	CoroutineManager.StartCoroutine(MenuCoroutine);
		}
		
		public IEnumerator MenuCoroutine(){
			while(!Constants.CHARACTER_CHOOSED){
                
				Vector3 location = MenuBackground.NewLocation;
                
				LPlayer.Physics.TargetPosition = location;
				LPlayer.Model.Position = location;
				yield return null;
			}
		}
		
	    public void MakeCurrent(PlayerData Data){
			if(Constants.CHARACTER_CHOOSED){
				LPlayer.Model.Dispose();
			}
		    LPlayer.UI.ChrChooser.StopModels();//So as to fix loose ends

            GameSettings.DarkEffect = false;
			LPlayer.ClassType = Data.ClassType;
			if(LPlayer.ClassType == Class.Warrior)
				LPlayer.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType] = new []{ ItemType.Sword };
			
			else if(LPlayer.ClassType == Class.Archer)
				LPlayer.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType] = new []{ ItemType.Bow };
			
			else if(LPlayer.ClassType == Class.Rogue)
				LPlayer.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType] = new []{ ItemType.DoubleBlades };	
			
			LPlayer.Spawner.Enabled = true;
			LPlayer.HandLamp.Enabled = false;
	        LPlayer.Speed = LocalPlayer.DefaultSpeed;
			LPlayer.Physics.BaseHeight = 0;
			LPlayer.Name = Data.Name;
			LPlayer.XP = Data.XP;
			LPlayer.Mana = Data.Mana;
			LPlayer.Health = Data.Health;
			LPlayer.Level = Data.Level;
			LPlayer.AddonHealth = Data.AddonHealth;
			LPlayer.BlockPosition = Data.BlockPosition;
			LPlayer.Rotation = Data.Rotation;
			LPlayer.RandomFactor = Data.RandomFactor;
			LPlayer.Model.Dispose();
	        LPlayer.Physics.VelocityCap = float.MaxValue;

		    var colors = new[] {Data.Color0, Data.Color1};
		    LPlayer.Model = new HumanModel(LPlayer)
		    {
		        Color0 = colors[0],
		        Color1 = colors[1]
		    };

			if(! (LPlayer.Health > 0) )
				LPlayer.Model.Enabled = false;
			LPlayer.Model.Enabled = true;
			LPlayer.SkillSystem = new SkillTree(LPlayer).Load(Data);
			LPlayer.Skills = LPlayer.Skills.Load( Data.SkillIDs );
			LPlayer.Chat.Clear();
			LPlayer.View.CameraHeight = Camera.DefaultCameraHeight;
			
			Objective.Unserialize(Data.QuestData);
			if(Data.WorldSeed != 0)
			    World.Recreate(Data.WorldSeed);
			Enviroment.SkyManager.DayTime = Data.Daytime;
			Enviroment.SkyManager.LoadTime = true;
			LPlayer.Inventory.SetItems(Data.Items);

			if(LPlayer.Health == 0){
				//LPlayer.BlockPosition = new Vector3(GameSettings.SpawnPoint.X, 128, GameSettings.SpawnPoint.Y);
				LPlayer.Respawn();
				_savedHealth = LPlayer.Health;
			}else{
				_savedHealth = Data.Health;
			}
			
			_savedMana = Data.Mana;
			CoroutineManager.StartCoroutine(SpawnCoroutine);
		
			
		}

		public void NewRun(LocalPlayer Player){
			NewRun(DataManager.DataFromPlayer(Player));
		}

		public void NewRun(PlayerData Data){
			
			LocalPlayer Player = LocalPlayer.Instance;
			Player.IsRiding = false;
			if(Player.IsRolling)
				Player.FinishRoll();
		    Player.Pet.MountEntity?.Update();//Finish removing the mount

		    Data.WorldSeed = World.RandomSeed;
			Data.BlockPosition = GameSettings.SpawnPoint.ToVector3();
			Data.BlockPosition = new Vector3(Data.BlockPosition.X, 128, Data.BlockPosition.Z);
			LocalPlayer.Instance.IsGliding = false;
			SceneManager.Game.MakeCurrent(Data);
			Enviroment.SkyManager.SetTime(12000);

		    var colors = new [] {Player.Model.Color1, Player.Model.Color0};
		    Player.Model = new HumanModel(Player)
		    {
		        Color0 = colors[0],
		        Color1 = colors[1]
		    };
			
			if(Player.Inventory.Items[Inventory.WeaponHolder] != null){
				//Force to discard cache
				Player.Inventory.Items[Inventory.WeaponHolder].Type = Player.Inventory.Items[Inventory.WeaponHolder].Type;
				Player.Model.SetWeapon(Player.Inventory.Items[Inventory.WeaponHolder].Weapon);
				
			}
			Player.UI.HideMenu();
			Player.UI.Hide = false;
			Player.Enabled = true;				
			Player.Model.Enabled = true;
			_isNewRun = true;
		}
		
		private IEnumerator SpawnCoroutine(){
			if(_isNewRun){
				LPlayer.DmgComponent.Immune = true;
				LPlayer.Health = LPlayer.MaxHealth;
				LPlayer.Mana = LPlayer.MaxMana;
			}else{
				LPlayer.Health = _savedHealth;
				LPlayer.Mana = _savedMana;
			}

		    SceneManager.Game.LPlayer.UI.HideMenu();
            LPlayer.UI.GamePanel.Disable();
			LPlayer.Chat.Show = false;
			var time = 0f;
			IsLoading = true;
			var text = "LOADING";

		    _loadingScreen.TextureElement.Opacity = 1;
		    _playerText.UiText.Opacity = 1;
		    _playerText.Enable();
		    _loadingScreen.Enable();
		    UpdateManager.CursorShown = true;

		    var chunkOffset = World.ToChunkSpace(LocalPlayer.Instance.BlockPosition);
		    World.StructureGenerator.CheckStructures(chunkOffset);
            while (true){
				time += Time.FrameTimeSeconds;
				if(time >= .5f){
					text += ".";
					time = 0;
				}
				
				if(text.Contains("....")) 
					text = "LOADING";
				
				_playerText.Text = text;
				LPlayer.Physics.ResetFall();
				Chunk underChunk = World.GetChunkAt(LPlayer.BlockPosition);
				if(underChunk != null && underChunk.IsGenerated && underChunk.Landscape.StructuresPlaced && underChunk.BuildedWithStructures
				  && underChunk.Mesh != null && !underChunk.Mesh.Occluded){
					if(LPlayer.IsUnderwater){
						LPlayer.Position += Vector3.One.Xz.ToVector3() * (float) Time.FrameTimeSeconds * 60f * 5f;
						yield return null;
						continue;
					}
					
					LPlayer.Model.LeftWeapon.Mesh.TargetPosition = Vector3.Zero;
					LPlayer.Model.LeftWeapon.Mesh.TargetRotation = Vector3.Zero;
					LPlayer.Model.LeftWeapon.Mesh.LocalRotation = Vector3.Zero;
					LPlayer.Model.LeftWeapon.Mesh.LocalPosition = Vector3.Zero;
					LPlayer.Model.Fog = true;
					LPlayer.CanInteract = true;
					IsLoading = false;
					_loadingScreen.TextureElement.Opacity = 0;
				    _playerText.UiText.Opacity = 0;
					if(_isNewRun)
						LPlayer.QuestLog.Show = true;

				    LocalPlayer.Instance.PlaySpawningAnimation = true;
                    LocalPlayer.Instance.MessageDispatcher.ShowTitleMessage(World.QuestManager.GenerateName(), 1.5f);
					LPlayer.DmgComponent.Immune = false;
					_isNewRun = false;
					break;
				}
				yield return null;
			}
			LPlayer.UI.GamePanel.Enable();
			LPlayer.Chat.Show = true;
			LPlayer.Chat.LoseFocus();
		}
	}
}
