/*
 * Author: Zaphyk
 * Date: 08/02/2016
 * Time: 02:19 a.m.
 *
 */

using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using System.Drawing;
using System.Collections;
using Hedra.Engine.ItemSystem;
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
		public PlayerInformation CurrentInformation;
		private Texture _loadingScreen;
	    private GUIText _playerText;
	    private float _rotationY;
	    private bool _isNewRun;
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
		
	    public void MakeCurrent(PlayerInformation Information){
			if(Constants.CHARACTER_CHOOSED){
				LPlayer.Model.Dispose();
			}
		    LPlayer.UI.ChrChooser.StopModels();//So as to fix loose ends
            LPlayer.ClassType = Information.ClassType;		
			LPlayer.Spawner.Enabled = true;
			LPlayer.HandLamp.Enabled = false;
	        LPlayer.Speed = LocalPlayer.DefaultSpeed;
			LPlayer.Physics.BaseHeight = 0;
			LPlayer.Name = Information.Name;
			LPlayer.XP = Information.Xp;
			LPlayer.Mana = Information.Mana;
			LPlayer.Health = Information.Health;
			LPlayer.Level = Information.Level;
			LPlayer.AddonHealth = Information.AddonHealth;
			LPlayer.BlockPosition = Information.BlockPosition;
			LPlayer.Rotation = Information.Rotation;
			LPlayer.Model.Dispose();
	        LPlayer.Physics.VelocityCap = float.MaxValue;
		    LPlayer.Model = new HumanModel(LPlayer);
	        LPlayer.RandomFactor = Information.RandomFactor;
			if(! (LPlayer.Health > 0) )
				LPlayer.Model.Enabled = false;
			LPlayer.Model.Enabled = true;
			LPlayer.SkillSystem = new SkillTree(LPlayer).Load(Information);
			LPlayer.Skills = LPlayer.Skills.Load( Information.SkillIDs );
			LPlayer.Chat.Clear();
			LPlayer.View.CameraHeight = Camera.DefaultCameraHeight;
			if(Information.WorldSeed != 0)
			    World.Recreate(Information.WorldSeed);
			Enviroment.SkyManager.DayTime = Information.Daytime;
			Enviroment.SkyManager.LoadTime = true;
	        LPlayer.Inventory.ClearInventory();
			LPlayer.Inventory.SetItems(Information.Items);
	        if (Information.ClassType == Class.Warrior)
	            LPlayer.Inventory.AddWeaponRestriction(WeaponType.Sword);

	        else if (Information.ClassType == Class.Archer)
	            LPlayer.Inventory.AddWeaponRestriction(WeaponType.Bow);

	        else if (Information.ClassType == Class.Rogue)
	            LPlayer.Inventory.AddWeaponRestriction(WeaponType.DoubleBlades);
            GameSettings.DarkEffect = false;

            if (LPlayer.Health == 0) LPlayer.Respawn();
			CoroutineManager.StartCoroutine(SpawnCoroutine);	
		}

		public void NewRun(LocalPlayer Player){
			NewRun(DataManager.DataFromPlayer(Player));
		}

		public void NewRun(PlayerInformation Information){
			
			LocalPlayer Player = LocalPlayer.Instance;
			Player.IsRiding = false;
			if(Player.IsRolling)
				Player.FinishRoll();
		    Player.Pet.MountEntity?.Update();//Finish removing the mount

		    Information.WorldSeed = World.RandomSeed;
			Information.BlockPosition = GameSettings.SpawnPoint.ToVector3();
			Information.BlockPosition = new Vector3(Information.BlockPosition.X, 128, Information.BlockPosition.Z);
			LocalPlayer.Instance.IsGliding = false;
			SceneManager.Game.MakeCurrent(Information);
			Enviroment.SkyManager.SetTime(12000);

		    Player.Model = new HumanModel(Player);
			
			if(Player.Inventory.MainWeapon != null){
				//Force to discard cache
			    Player.Inventory.MainWeapon.FlushCache();
				Player.Model.SetWeapon(Player.Inventory.MainWeapon.Weapon);
				
			}
			Player.UI.HideMenu();
			Player.UI.Hide = false;
			Player.Enabled = true;				
			Player.Model.Enabled = true;
			_isNewRun = true;
		}
		
		private IEnumerator SpawnCoroutine(){
			if(_isNewRun) LPlayer.DmgComponent.Immune = true;

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
						LPlayer.BlockPosition += Vector3.One.Xz.ToVector3() * (float) Time.FrameTimeSeconds * 60f * 5f;
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
