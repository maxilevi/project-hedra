/*
 * Author: Zaphyk
 * Date: 08/02/2016
 * Time: 02:19 a.m.
 *
 */

using System.Collections;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using OpenTK;

namespace Hedra.Engine
{
	/// <summary>
	/// The Scene containing the main game
	/// </summary>
	public static class GameManager
	{
		public static LocalPlayer Player { get; set; }
	    public static bool IsLoading { get; private set; }
        private static Texture LoadingScreen;
	    private static GUIText PlayerText;
	    private static bool _isNewRun;

        public static void Load()
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

			LoadingScreen = new Texture(Color.FromArgb(255,29,29,29), Color.FromArgb(255,59,59,59),
                Vector2.Zero, Vector2.One, GradientType.TopBot);
		    PlayerText = new GUIText("", new Vector2(0, 0), Color.White,
                FontCache.Get(UserInterface.Fonts.Families[0], 14, FontStyle.Bold));

		    LoadingScreen.Disable();

			GameManager.LoadMenu();
		}
		
		public static bool InStartMenu => World.Seed == World.MenuSeed;
	    public static bool InMenu => Player != null && Player.UI.Menu.Enabled && !Player.UI.Hide && World.Seed != World.MenuSeed;

	    public static void LoadMenu(){
	        World.Recreate(World.MenuSeed);
            LocalPlayer.Instance.UI.ShowMenu();
	        Enviroment.SkyManager.SetTime(12000);
          	LocalPlayer.Instance.Model.Enabled = false;
          	LocalPlayer.Instance.View.Pitch = 0f;
          	LocalPlayer.Instance.HandLamp.Enabled = false;
          	LocalPlayer.Instance.View.Yaw = 0f;
          	LocalPlayer.Instance.View.TargetDistance = 10f;
	        LocalPlayer.Instance.IsGliding = false;
	        LocalPlayer.Instance.Glider.Enabled = false;
	        LocalPlayer.Instance.Knocked = false;
	        SoundtrackManager.PlayTrack(SoundtrackManager.MainThemeIndex, true);
            CoroutineManager.StartCoroutine(GameManager.MenuCoroutine);
        }
		
		private static IEnumerator MenuCoroutine(){
			while(GameManager.InStartMenu){
                
				Vector3 location = MenuBackground.NewLocation;
                
				Player.Physics.TargetPosition = location;
				Player.Model.Position = location;
				yield return null;
			}
		}
		
	    public static void MakeCurrent(PlayerInformation Information){
			if(!GameManager.InStartMenu){
				Player.Model.Dispose();
			}
		    Player.UI.ChrChooser.StopModels();//So as to fix loose ends
            Player.Class = Information.Class;		
			Player.Spawner.Enabled = true;
			Player.HandLamp.Enabled = false;
	        Player.Speed = Player.BaseSpeed;
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
	        GameManager.SetRestrictions(Information);
            GameSettings.DarkEffect = false;

            if (Player.Health == 0) Player.Respawn();
			CoroutineManager.StartCoroutine(GameManager.SpawnCoroutine);	
		}

	    private static void SetRestrictions(PlayerInformation Information)
	    {
	        Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, Information.Class.StartingItem.EquipmentType);
            Player.Inventory.AddRestriction(PlayerInventory.BootsHolder, EquipmentType.Boots);
	        Player.Inventory.AddRestriction(PlayerInventory.PantsHolder, EquipmentType.Pants);
	        Player.Inventory.AddRestriction(PlayerInventory.ChestplateHolder, EquipmentType.Chestplate);
	        Player.Inventory.AddRestriction(PlayerInventory.HelmetHolder, EquipmentType.Helmet);
            Player.Inventory.AddRestriction(PlayerInventory.GliderHolder, EquipmentType.Vehicle);
	        Player.Inventory.AddRestriction(PlayerInventory.PetHolder, EquipmentType.Pet);
	        Player.Inventory.AddRestriction(PlayerInventory.RingHolder, EquipmentType.Ring);
        }

		public static void NewRun(LocalPlayer Player){
			GameManager.NewRun(DataManager.DataFromPlayer(Player));
		}

		public static void NewRun(PlayerInformation Information){
			Player.IsRiding = false;
			if(Player.IsRolling)
				Player.FinishRoll();
		    Player.Pet.Pet?.Update();//Finish removing the mount

		    Information.WorldSeed = World.RandomSeed;
			Information.BlockPosition = GameSettings.SpawnPoint.ToVector3();
			Information.BlockPosition = new Vector3(Information.BlockPosition.X, 128, Information.BlockPosition.Z);
			LocalPlayer.Instance.IsGliding = false;
			GameManager.MakeCurrent(Information);
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
		
		private static IEnumerator SpawnCoroutine(){
		    SoundtrackManager.PlayTrack(SoundtrackManager.LoopableSongsStart);
		    if (_isNewRun)
		    {
		        Player.Physics.TargetPosition = new Vector3(Player.Physics.TargetPosition.X, 0, Player.Physics.TargetPosition.Z);
		    }

		    GameManager.Player.UI.HideMenu();
            Player.UI.GamePanel.Disable();
			Player.Chat.Show = false;
			var time = 0f;
			IsLoading = true;
			var text = "LOADING";

		    LoadingScreen.TextureElement.Opacity = 1;
		    PlayerText.UIText.Opacity = 1;
		    PlayerText.Enable();
		    LoadingScreen.Enable();
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

                PlayerText.Text = text;
				Chunk underChunk = World.GetChunkAt(Player.BlockPosition);
				if(underChunk != null && underChunk.IsGenerated && underChunk.Landscape.StructuresPlaced && underChunk.BuildedWithStructures
				  && underChunk.Mesh != null && !underChunk.Mesh.Occluded){
                    if (Player.IsUnderwater){
						Player.BlockPosition += Vector3.One.Xz.ToVector3() * (float) Time.FrameTimeSeconds * 60f * 5f;
						yield return null;
						continue;
					}
					
                    Player.Physics.TargetPosition = new Vector3(
                        Player.Physics.TargetPosition.X,
                        Physics.HeightAtPosition(Player.Physics.TargetPosition),
                        Player.Physics.TargetPosition.Z
                        );
					Player.Model.LeftWeapon.MainMesh.TargetPosition = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.LocalRotation = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.LocalPosition = Vector3.Zero;
					Player.Model.Fog = true;
					Player.CanInteract = true;
					IsLoading = false;
					LoadingScreen.TextureElement.Opacity = 0;
				    PlayerText.UIText.Opacity = 0;
					if(_isNewRun)
						Player.QuestLog.Show = true;

				    LocalPlayer.Instance.PlaySpawningAnimation = true;
                    LocalPlayer.Instance.MessageDispatcher.ShowTitleMessage(World.QuestManager.GenerateName(), 1.5f);
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
