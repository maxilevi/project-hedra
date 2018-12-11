using System;
using System.Collections;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Input;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.Game
{
    public class GameProvider : IGameProvider
    {
        public event EventHandler AfterSave;
        public event EventHandler BeforeSave;
        public bool Exists => Program.GameWindow.Exists;
        public bool IsExiting => Program.GameWindow.IsExiting;    
        public KeyboardManager Keyboard { get; private set; }
        public IPlayer Player { get; set; }
        public bool IsLoading => _loadingScreen?.IsLoading ?? false;

        private LoadingScreen _loadingScreen;
        private bool _isNewRun;
        private bool _spawningEffect;

        public void Load()
        {
            Keyboard = new KeyboardManager();
            
            Log.WriteLine("Loading the world...");
            World.Load();
            
            Log.WriteLine("Loading the player...");
            Player = new LocalPlayer
            {
                Enabled = false,
                Model =
                {
                    Enabled = false
                }
            };
            
            Log.WriteLine("Creating the GUI...");
            Player.UI.ShowMenu();
            Player.UI.ChrChooser.Disable(); 
            Player.UI.Menu.Enable();
            _loadingScreen = new LoadingScreen(Player);
            LoadMenu();
            
            Log.WriteLine("Loading the character classes...");
            var classes = ClassDesign.AvailableClasses;
            for (var i = 0; i < classes.Length; i++)
            {
                var classObject = ClassDesign.FromType(classes[i]); 
                Log.WriteLine($"Loading character class '{classObject.Name}'");
            }
        }

        public void LoadMenu()
        {
            World.Recreate(World.MenuSeed);
            SkyManager.SetTime(12000);
            SoundtrackManager.PlayTrack(SoundtrackManager.MainThemeIndex, true);
            CoroutineManager.StartCoroutine(MenuCoroutine);
            Player.Reset();
            _loadingScreen.Show();
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
            Player.AbilityTree.FromInformation(Information);
            Player.Toolbar.FromInformation(Information);
            Player.View.CameraHeight = Camera.DefaultCameraHeight;
            if(Player.IsDead)
                Player.Respawn();
            if(Information.WorldSeed != 0)
                World.Recreate(Information.WorldSeed);
            if(Player.IsDead)
                Player.Respawn();
            SkyManager.DayTime = Information.Daytime;
            SkyManager.LoadTime = true;
            Player.Inventory.SetItems(Information.Items);
            SetRestrictions(Information);
            GameSettings.DarkEffect = false;
            CoroutineManager.StartCoroutine(SpawnCoroutine);
            Log.WriteLine($"Making '{Information.Name}' current with seed {World.Seed}.");
        }

        private void SetRestrictions(PlayerInformation Information)
        {
            var modRestrictions = RestrictionsFactory.Instance.Build(Information.Class.GetType());
            for (var i = 0; i < modRestrictions.Length; i++)
            {
                Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, modRestrictions[i]);
            }
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, Information.Class.StartingItem.EquipmentType);
            Player.Inventory.AddRestriction(PlayerInventory.BootsHolder, EquipmentType.Boots);
            Player.Inventory.AddRestriction(PlayerInventory.PantsHolder, EquipmentType.Pants);
            Player.Inventory.AddRestriction(PlayerInventory.ChestplateHolder, EquipmentType.Chestplate);
            Player.Inventory.AddRestriction(PlayerInventory.HelmetHolder, EquipmentType.Helmet);
            Player.Inventory.AddRestriction(PlayerInventory.VehicleHolder, EquipmentType.Vehicle);
            Player.Inventory.AddRestriction(PlayerInventory.PetHolder, EquipmentType.Pet);
            Player.Inventory.AddRestriction(PlayerInventory.RingHolder, EquipmentType.Ring);
        }

        public void NewRun(PlayerInformation Information)
        {
            Information.WorldSeed = World.RandomSeed;
            GameManager.MakeCurrent(Information);
            GameManager.Player.Position = World.SpawnPoint;
            SkyManager.SetTime(12000);

            Player.Model = new HumanoidModel(Player);         
            if(Player.Inventory.MainWeapon != null)
            {
                Player.Inventory.MainWeapon.FlushCache();
                Player.SetWeapon(Player.Inventory.MainWeapon.Weapon);               
            }
            Player.UI.HideMenu();
            Player.UI.Hide = false;
            Player.Enabled = true;                
            _isNewRun = true;
        }
        
        private IEnumerator SpawnCoroutine()
        {
            _loadingScreen.Show();
            SoundtrackManager.PlayAmbient();
            GameManager.Player.UI.HideMenu();
            Player.UI.GamePanel.Disable();
            Player.Chat.Show = false;
            Player.SearchComponent<DamageComponent>().Immune = true;

            var chunkOffset = World.ToChunkSpace(Player.BlockPosition);
            StructureHandler.CheckStructures(chunkOffset);
            while (_loadingScreen.IsLoading)
            {
                Player.Physics.ResetFall();
                Player.Physics.UsePhysics = false;
                yield return null;
            }
            Player.Physics.UsePhysics = true;
            Player.SearchComponent<DamageComponent>().Immune = false;
            Player.UI.GamePanel.Enable();
            Player.Chat.Show = true;
            Player.Chat.LoseFocus();
            GameManager.SpawningEffect = true;
            Player.Model.ApplyFog = true;
            Player.CanInteract = true;
            Player.QuestLog.Show = true;
            GameManager.Player.PlaySpawningAnimation = true;
            if (!GameManager.Player.MessageDispatcher.HasTitleMessages)
                GameManager.Player.MessageDispatcher.ShowTitleMessage(World.WorldBuilding.GenerateName(), 1.5f);           
        }

        public void Unload()
        {
            BeforeSave?.Invoke(this, new EventArgs());
        }

        public void Reload()
        {
            AfterSave?.Invoke(this, new EventArgs());
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
                TaskScheduler.While( () => Math.Abs(GameSettings.BloomModifier - 1.0f) > .005f, delegate
                {
                    GameSettings.BloomModifier = Mathf.Lerp(GameSettings.BloomModifier, 1.0f, Time.IndependantDeltaTime);
                });
                TaskScheduler.When( () => Math.Abs(GameSettings.BloomModifier - 1.0f) < .005f, delegate
                {
                    GameSettings.BloomModifier = 1.0f;
                    _spawningEffect = false;
                });
            }
        }
    }
}