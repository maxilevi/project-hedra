﻿using System;
using System.Collections;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
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
        public KeyboardManager Keyboard { get; private set; }
		public IPlayer Player { get; set; }
	    public bool IsLoading { get; private set; }
		
        private Texture _loadingScreen;
	    private GUIText _playerText;
	    private bool _isNewRun;
	    private bool _spawningEffect;

        public void Load()
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
			_loadingScreen = new Texture(Color.FromArgb(255, 30, 30, 30), Color.FromArgb(255, 60, 60, 60),
                Vector2.Zero, Vector2.One, GradientType.Diagonal);
		    _playerText = new GUIText(string.Empty, new Vector2(0, 0), Color.White, 
			    FontCache.Get(AssetManager.NormalFamily, 15, FontStyle.Bold));
			_loadingScreen.TextureElement.Opacity = 0;
			_playerText.UIText.Opacity = 0;
			CoroutineManager.StartCoroutine(LoadingScreenCoroutine);
			LoadMenu();
		}

	    public void LoadMenu()
        {
	        World.Recreate(World.MenuSeed);
	        SkyManager.SetTime(12000);
	        SoundtrackManager.PlayTrack(SoundtrackManager.MainThemeIndex, true);
            CoroutineManager.StartCoroutine(MenuCoroutine);
            Player.Reset();
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
	        Player.Loader.Reset();
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
			SkyManager.SetTime(12000);

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
			Player.MessageDispatcher.ShowMessageWhile("[F4] HELP", () => !LocalPlayer.Instance.UI.ShowHelp);
		}

	    private IEnumerator LoadingScreenCoroutine()
	    {
		    var time = 0f;
		    var text = "LOADING";
		    while (Exists)
		    {
			    yield return null;
			    if (IsLoading)
			    {
				    time += Time.IndependantDeltaTime;
				    if (time >= .5f)
				    {
					    text += ".";
					    time = 0;
					    if (text.Contains("...."))
						    text = "LOADING";
				    }
				    _playerText.Text = text;
				    _loadingScreen.TextureElement.Opacity = 1;
				    _playerText.UIText.Opacity = 1;
                    _playerText.Enable();
                    _loadingScreen.Enable();
			    }
			    else
			    {
				    _loadingScreen.TextureElement.Opacity = 0;
				    _playerText.UIText.Opacity = 0;
			    }
		    }
	    }
	    
	    private IEnumerator SpawnCoroutine()
	    {
		    SoundtrackManager.PlayTrack(SoundtrackManager.LoopableSongsStart);
		    GameManager.Player.UI.HideMenu();
		    Player.UI.GamePanel.Disable();
		    Player.Chat.Show = false;
		    IsLoading = true;
		    Player.SearchComponent<DamageComponent>().Immune = true;

		    var chunkOffset = World.ToChunkSpace(LocalPlayer.Instance.BlockPosition);
		    World.StructureGenerator.CheckStructures(chunkOffset);
		    while (!IsLoaded(chunkOffset))
		    {
			    Player.Physics.TargetPosition = new Vector3(
				    Player.Physics.TargetPosition.X,
				    Physics.HeightAtPosition(Player.Physics.TargetPosition),
				    Player.Physics.TargetPosition.Z
			    );
			    yield return null;
		    }

		    Player.SearchComponent<DamageComponent>().Immune = false;
		    Player.UI.GamePanel.Enable();
		    Player.Chat.Show = true;
		    Player.Chat.LoseFocus();
		    IsLoading = false;
		    GameManager.SpawningEffect = true;
		    Player.Model.ApplyFog = true;
		    Player.CanInteract = true;
		    Player.QuestLog.Show = true;
		    GameManager.Player.PlaySpawningAnimation = true;
		    GameManager.Player.MessageDispatcher.ShowTitleMessage(World.WorldBuilding.GenerateName(), 1.5f);
	    }

	    private static bool IsLoaded(Vector2 Offset)
	    {
		    var minRange = -1;
		    var maxRange = 2;
		    #if DEBUG
		    minRange = 0;
		    maxRange = 1;
		    #endif
		    for (var x = minRange; x < maxRange; x++)
		    {
			    for (var z = minRange; z < maxRange; z++)
			    {
				    var chunk = World.GetChunkByOffset((int) Offset.X + x * Chunk.Width, (int) Offset.Y + z * Chunk.Width);
				    if (!chunk?.BuildedWithStructures ?? true)
					    return false;
			    } 
		    }

		    return true;
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
				    GameSettings.BloomModifier = Mathf.Lerp(GameSettings.BloomModifier, 1.0f, Time.IndependantDeltaTime);
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