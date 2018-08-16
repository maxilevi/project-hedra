using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using OpenTK;

namespace Hedra.Engine.Game
{
    public class GameProvider : IGameProvider
    {
        public bool Exists => Program.GameWindow.Exists;
        public KeyboardManager Keyboard { get; }
		public IPlayer Player { get; set; }
	    public bool IsLoading { get; private set; }
		
        private readonly Texture _loadingScreen;
	    private readonly GUIText _playerText;
	    private bool _isNewRun;
	    private bool _spawningEffect;

        public GameProvider()
		{
		    Keyboard = new KeyboardManager();
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
			_loadingScreen = new Texture(Color.FromArgb(255, 40, 40, 40), Color.FromArgb(255, 70, 70, 70),
                Vector2.Zero, Vector2.One, GradientType.TopBot);
		    _playerText = new GUIText(string.Empty, new Vector2(0, 0), Color.White,
                FontCache.Get(UserInterface.Fonts.Families[0], 15, FontStyle.Bold));
		    _loadingScreen.Disable();
			LoadMenu();
		}

	    public void LoadMenu()
        {
	        World.Recreate(World.MenuSeed);
	        SkyManager.SetTime(12000);
            Player.Reset();
	        SoundtrackManager.PlayTrack(SoundtrackManager.MainThemeIndex, true);
            CoroutineManager.StartCoroutine(MenuCoroutine);
        }
		
		private IEnumerator MenuCoroutine()
        {
			while(GameManager.InStartMenu)
            {               
				var location = MenuBackground.NewLocation;            
				Player.Physics.TargetPosition = location;
				Player.Model.Position = location;
			    Player.Model.Alpha = 0f;
				yield return null;
			}
		}
		
	    public void MakeCurrent(PlayerInformation Information)
        {
			Player.Reset();
		    Player.UI.ChrChooser.StopModels();//So as to fix loose ends
            Player.Class = Information.Class;
	        Player.Level = Information.Level;
	        Player.Speed = Player.BaseSpeed;
			Player.Physics.BaseHeight = 0;
			Player.Name = Information.Name;
			Player.XP = Information.Xp;
			Player.Mana = Information.Mana;
			Player.Health = Information.Health;
			Player.BlockPosition = Information.BlockPosition;
			Player.Rotation = Information.Rotation;
			Player.Model.Dispose();
	        Player.Physics.VelocityCap = float.MaxValue;
		    Player.Model = new HumanoidModel(Player);
	        Player.RandomFactor = Information.RandomFactor;
			if(! (Player.Health > 0) )
				Player.Model.Enabled = false;
			Player.AbilityTree.FromInformation(Information);
			Player.Toolbar.FromInformation(Information);
			Player.View.CameraHeight = Camera.DefaultCameraHeight;
			if(Information.WorldSeed != 0)
			    World.Recreate(Information.WorldSeed);
			SkyManager.DayTime = Information.Daytime;
			SkyManager.LoadTime = true;
			Player.Inventory.SetItems(Information.Items);
	        SetRestrictions(Information);
            GameSettings.DarkEffect = false;
			CoroutineManager.StartCoroutine(SpawnCoroutine);	
		}

	    private void SetRestrictions(PlayerInformation Information)
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

		public void NewRun(PlayerInformation Information)
		{
			Player.IsRiding = false;
		    Player.Pet.Pet?.Update();//Finish removing the mount

		    Information.WorldSeed = World.RandomSeed;
			Information.BlockPosition = GameSettings.SpawnPoint.ToVector3();
			Information.BlockPosition = new Vector3(Information.BlockPosition.X, 128, Information.BlockPosition.Z);
			LocalPlayer.Instance.IsGliding = false;
			GameManager.MakeCurrent(Information);
			EnvironmentSystem.SkyManager.SetTime(12000);

		    Player.Model = new HumanoidModel(Player);
			
			if(Player.Inventory.MainWeapon != null){
				//Force to discard cache
			    Player.Inventory.MainWeapon.FlushCache();
				Player.Model.SetWeapon(Player.Inventory.MainWeapon.Weapon);
				
			}
			Player.UI.HideMenu();
			Player.UI.Hide = false;
			Player.Enabled = true;				
			_isNewRun = true;
		}

	    private IEnumerator SpawnCoroutine()
	    {
		    SoundtrackManager.PlayTrack(SoundtrackManager.LoopableSongsStart);
		    if (_isNewRun)
		    {
			    Player.Physics.TargetPosition =
				    new Vector3(Player.Physics.TargetPosition.X, 0, Player.Physics.TargetPosition.Z);
		    }

		    GameManager.Player.UI.HideMenu();
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
		    while (true)
		    {
			    time += Time.IndependantDeltaTime;
			    if (time >= .5f)
			    {
				    text += ".";
				    time = 0;
			    }

			    if (text.Contains("...."))
				    text = "LOADING";

			    _playerText.Text = text;
			    var underChunk = World.GetChunkAt(Player.BlockPosition);
			    if (underChunk != null && underChunk.IsGenerated && underChunk.Landscape.StructuresPlaced &&
			        underChunk.BuildedWithStructures
			        && underChunk.Mesh != null)
			    {
				    if (Player.IsUnderwater)
				    {
					    Player.BlockPosition +=
						    Vector3.One.Xz.ToVector3() * (float) Time.IndependantDeltaTime * 60f * 5f;
					    yield return null;
					    continue;
				    }

				    Player.Physics.TargetPosition = new Vector3(
					    Player.Physics.TargetPosition.X,
					    Physics.HeightAtPosition(Player.Physics.TargetPosition),
					    Player.Physics.TargetPosition.Z
				    );
				    Player.LeftWeapon.MainMesh.TargetPosition = Vector3.Zero;
				    Player.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
				    Player.LeftWeapon.MainMesh.LocalRotation = Vector3.Zero;
				    Player.LeftWeapon.MainMesh.LocalPosition = Vector3.Zero;
				    Player.Model.ApplyFog = true;
				    Player.CanInteract = true;
				    GameManager.SpawningEffect = true;
				    IsLoading = false;
				    _loadingScreen.TextureElement.Opacity = 0;
				    _playerText.UIText.Opacity = 0;
				    if (_isNewRun)
					    Player.QuestLog.Show = true;

				    LocalPlayer.Instance.PlaySpawningAnimation = true;
				    LocalPlayer.Instance.MessageDispatcher.ShowTitleMessage(World.WorldBuilding.GenerateName(), 1.5f);
				    _isNewRun = false;
				    break;
			    }

			    yield return null;
		    }

		    Player.UI.GamePanel.Enable();
		    Player.Chat.Show = true;
		    Player.Chat.LoseFocus();
	    }

	    public bool InStartMenu => World.Seed == World.MenuSeed;

	    public bool InMenu => Player != null && Player.UI.Menu.Enabled && !Player.UI.Hide && World.Seed != World.MenuSeed;	    
	    
	    public bool SpawningEffect
	    {
		    get => _spawningEffect;
		    set
		    {
			    if (!value || SpawningEffect || GameSettings.Paused) return;
			    _spawningEffect = true;
			    GameSettings.BloomModifier = 8.0f;
			    TaskManager.While( () => Math.Abs(GameSettings.BloomModifier - 1.0f) > .005f, delegate
			    {
				    GameSettings.BloomModifier = Mathf.Lerp(GameSettings.BloomModifier, 1.0f, (float) Time.IndependantDeltaTime);
			    });
			    TaskManager.When( () => Math.Abs(GameSettings.BloomModifier - 1.0f) < .005f, delegate
			    {
				    GameSettings.BloomModifier = 1.0f;
				    _spawningEffect = false;
			    });
		    }
	    }
    }
}