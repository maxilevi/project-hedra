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

namespace Hedra.Engine.Scenes
{
	/// <summary>
	/// The Scene containing the main game
	/// </summary>
	public class GameScene : IScene
	{
	    public int Id { get; set; } = 1;
		public LocalPlayer Player { get; set; }
		public PlayerInformation CurrentInformation { get; set; }
	    public bool IsLoading { get; private set; }
        private Texture _loadingScreen;
	    private GUIText _playerText;
	    private bool _isNewRun;

		public void LoadScene()
		{
		    World.Load();
			Log.WriteLine("World Created Successfully!");
			Player = new LocalPlayer();
			Log.WriteLine("Player Created Successfully!");

			Player.Model.Enabled = false;
			Player.UI.ShowMenu();
			Player.UI.ChrChooser.Disable(); 
			Player.UI.Menu.Enable();
			
			Log.WriteLine("Setted up GUI");

			Player.Enabled = false;

			_loadingScreen = new Texture(Color.FromArgb(255,29,29,29), Color.FromArgb(255,59,59,59),
                Vector2.Zero, Vector2.One, GradientType.TopBot);
		    _playerText = new GUIText("", new Vector2(0, 0), Color.White,
                FontCache.Get(UserInterface.Fonts.Families[0], 14, FontStyle.Bold));

		    _loadingScreen.Disable();

			CoroutineManager.StartCoroutine(this.MenuCoroutine);
		}
		
		public bool InMenuWorld => World.Seed == World.MenuSeed;

	    public bool InMenu => Player.UI.Menu.Enabled && !Player.UI.Hide && World.Seed != World.MenuSeed;

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
	        LocalPlayer.Instance.Knocked = false;
          	CoroutineManager.StartCoroutine(MenuCoroutine);
		}
		
		public IEnumerator MenuCoroutine(){
			while(!Constants.CHARACTER_CHOOSED){
                
				Vector3 location = MenuBackground.NewLocation;
                
				Player.Physics.TargetPosition = location;
				Player.Model.Position = location;
				yield return null;
			}
		}
		
	    public void MakeCurrent(PlayerInformation Information){
			if(Constants.CHARACTER_CHOOSED){
				Player.Model.Dispose();
			}
		    Player.UI.ChrChooser.StopModels();//So as to fix loose ends
            Player.ClassType = Information.ClassType;		
			Player.Spawner.Enabled = true;
			Player.HandLamp.Enabled = false;
	        Player.Speed = LocalPlayer.DefaultSpeed;
			Player.Physics.BaseHeight = 0;
			Player.Name = Information.Name;
			Player.XP = Information.Xp;
			Player.Mana = Information.Mana;
			Player.Health = Information.Health;
			Player.Level = Information.Level;
			Player.AddonHealth = Information.AddonHealth;
			Player.BlockPosition = Information.BlockPosition;
			Player.Rotation = Information.Rotation;
			Player.Model.Dispose();
	        Player.Physics.VelocityCap = float.MaxValue;
		    Player.Model = new HumanModel(Player);
	        Player.RandomFactor = Information.RandomFactor;
			if(! (Player.Health > 0) )
				Player.Model.Enabled = false;
			Player.Model.Enabled = true;
			Player.AbilityTree.FromInformation(Information);
			Player.Toolbar.FromInformation(Information);
			Player.Chat.Clear();
			Player.View.CameraHeight = Camera.DefaultCameraHeight;
			if(Information.WorldSeed != 0)
			    World.Recreate(Information.WorldSeed);
			Enviroment.SkyManager.DayTime = Information.Daytime;
			Enviroment.SkyManager.LoadTime = true;
	        Player.Inventory.ClearInventory();
			Player.Inventory.SetItems(Information.Items);
	        this.SetRestrictions(Information);
            GameSettings.DarkEffect = false;

            if (Player.Health == 0) Player.Respawn();
			CoroutineManager.StartCoroutine(SpawnCoroutine);	
		}

	    private void SetRestrictions(PlayerInformation Information)
	    {
	        switch (Information.ClassType)
	        {
	            case Class.Warrior:
	                Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Sword);
	                break;
	            case Class.Archer:
	                Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Bow);
	                break;
	            case Class.Rogue:
	                Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.DoubleBlades);
	                break;
	        }
            Player.Inventory.AddRestriction(PlayerInventory.BootsHolder, EquipmentType.Boots);
	        Player.Inventory.AddRestriction(PlayerInventory.PantsHolder, EquipmentType.Pants);
	        Player.Inventory.AddRestriction(PlayerInventory.ChestplateHolder, EquipmentType.Chestplate);
	        Player.Inventory.AddRestriction(PlayerInventory.HelmetHolder, EquipmentType.Helmet);
            Player.Inventory.AddRestriction(PlayerInventory.GliderHolder, EquipmentType.Vehicle);
	        Player.Inventory.AddRestriction(PlayerInventory.PetHolder, EquipmentType.Pet);
	        Player.Inventory.AddRestriction(PlayerInventory.RingHolder, EquipmentType.Ring);
        }

		public void NewRun(LocalPlayer Player){
			this.NewRun(DataManager.DataFromPlayer(Player));
		}

		public void NewRun(PlayerInformation Information){
			
			Player.IsRiding = false;
			if(Player.IsRolling)
				Player.FinishRoll();
		    Player.Pet.Pet?.Update();//Finish removing the mount

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
			if(_isNewRun) Player.DmgComponent.Immune = true;

		    SceneManager.Game.Player.UI.HideMenu();
            Player.UI.GamePanel.Disable();
			Player.Chat.Show = false;
			var time = 0f;
			IsLoading = true;
			var text = "LOADING";

		    _loadingScreen.TextureElement.Opacity = 1;
		    _playerText.UIText.Opacity = 1;
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
				Player.Physics.ResetFall();
				Chunk underChunk = World.GetChunkAt(Player.BlockPosition);
				if(underChunk != null && underChunk.IsGenerated && underChunk.Landscape.StructuresPlaced && underChunk.BuildedWithStructures
				  && underChunk.Mesh != null && !underChunk.Mesh.Occluded){
					if(Player.IsUnderwater){
						Player.BlockPosition += Vector3.One.Xz.ToVector3() * (float) Time.FrameTimeSeconds * 60f * 5f;
						yield return null;
						continue;
					}
					
					Player.Model.LeftWeapon.MainMesh.TargetPosition = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.LocalRotation = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.LocalPosition = Vector3.Zero;
					Player.Model.Fog = true;
					Player.CanInteract = true;
					IsLoading = false;
					_loadingScreen.TextureElement.Opacity = 0;
				    _playerText.UIText.Opacity = 0;
					if(_isNewRun)
						Player.QuestLog.Show = true;

				    LocalPlayer.Instance.PlaySpawningAnimation = true;
                    LocalPlayer.Instance.MessageDispatcher.ShowTitleMessage(World.QuestManager.GenerateName(), 1.5f);
					Player.DmgComponent.Immune = false;
					_isNewRun = false;
					break;
				}
				yield return null;
			}
			Player.UI.GamePanel.Enable();
			Player.Chat.Show = true;
			Player.Chat.LoseFocus();
		}
	}
}
