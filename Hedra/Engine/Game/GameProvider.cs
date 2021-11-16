using System;
using System.Collections;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
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
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Game;
using Hedra.Items;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.Game
{
    public class GameProvider : IGameProvider
    {
        public event EventHandler AfterSave;
        public event EventHandler BeforeSave;
        public bool Exists => Program.GameWindow.Exists;
        public bool IsExiting => Program.GameWindow.IsExiting;
        public KeyboardManager Keyboard { get; private set; }
        public bool IsLoading => _loadingScreen?.IsLoading ?? false;
        private IPlayer[] _players;
        private LoadingScreen _loadingScreen;
        private bool _isNewRun;
        private bool _spawningEffect;

        public void Load()
        {
            Keyboard = new KeyboardManager();

            Log.WriteLine("Loading the world...");
            World.Load();

            Log.WriteLine("Loading the player...");
            _players = new IPlayer[]
            {
                new LocalPlayer
                {
                    Enabled = false,
                    Model =
                    {
                        Enabled = false
                    }
                }
            };

            Log.WriteLine("Creating the GUI...");
            Player.UI.ShowMenu();
            Player.UI.CharacterSelector.Disable();
            Player.UI.Menu.Enable();
            _loadingScreen = new LoadingScreen(Player);
            RoutineManager.StartRoutine(LoadingScreenCoroutine);
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
            SoundtrackManager.PlayRepeating(SoundtrackManager.MainTheme);
            RoutineManager.StartRoutine(MenuCoroutine);
            Player.Reset();
            Player.UI.ShowMenu();
        }

        private IEnumerator MenuCoroutine()
        {
            while (GameManager.InStartMenu)
            {
                var location = MenuBackground.NewLocation;
                Player.Position = location;
                Player.Model.Position = location;
                Player.Model.Alpha = 0f;
                yield return null;
            }
        }

        public bool NearAnyPlayer(Vector3 Position, float Radius)
        {
            return _players.Any(P => (P.Position - Position).Xz().LengthSquared() < Radius * Radius);
        }

        public void LoadCharacter(PlayerInformation Information)
        {
            if (Information.RealmData.Length == 0 || GameSettings.NewWorld)
                NewRun(Information);
            else
                MakeCurrent(Information);
        }

        public void MakeCurrent(PlayerInformation Information)
        {
            Player.Reset();
            Player.Loader.Reset();
            Player.UI.CharacterSelector.StopModels(); //So as to fix loose ends
            Player.Class = Information.Class;
            Player.Customization = Information.Customization;
            Player.Level = Information.Level;
            Player.Name = Information.Name;
            Player.Speed = Player.BaseSpeed;
            Player.XP = Information.Xp;
            Player.Rotation = Information.Rotation;
            Player.Model.Dispose();
            Player.Model = new HumanoidModel(Player);
            Player.RandomFactor = Information.RandomFactor;
            Player.AbilityTree.UnSerialize(Information.SkillsData);
            Player.Toolbar.UnSerialize(Information.ToolbarData);
            Player.Realms.UnSerialize(Information.RealmData);
            Player.View.CameraHeight = Camera.DefaultCameraHeight;

            if (Player.IsDead)
                Player.Respawn();
            if (Player.IsDead)
                Player.Respawn();
            Player.Inventory.SetItems(Information.Items);
            Player.Crafting.SetRecipes(Information.Recipes);
            Player.Questing.SetSerializedQuests(Information.Quests);
            AddDefaultRecipes(Information);
            SetRestrictions(Information);
            GameSettings.DarkEffect = false;
            RoutineManager.StartRoutine(SpawnCoroutine);
            Log.WriteLine($"Making '{Information.Name}' current with seed {World.Seed}.");

            /* Skills need to be updated after everything is set */
            Player.Toolbar.UpdateSkills();
            /* We set health and mana last in order to not affect stuff like bonus health */
            Player.Mana = Information.Mana;
            Player.Health = Information.Health;
        }

        private void AddDefaultRecipes(PlayerInformation Information)
        {
            var defaultRecipes = new[]
            {
                ItemType.HealthPotionRecipe,
                ItemType.IntenseHealthPotionRecipe,
                ItemType.RefinedHealthPotionRecipe,
                ItemType.PumpkinPieRecipe,
                ItemType.CookedMeatRecipe,
                ItemType.CornSoupRecipe,
                ItemType.BaitRecipe,
                ItemType.FishingRodRecipe,
                ItemType.CookedSalmonRecipe,
                ItemType.CookedTroutRecipe,
                ItemType.CookedFishRecipe
            }.Select(T => T.ToString()).ToArray();
            for (var i = 0; i < defaultRecipes.Length; i++)
                if (!Player.Crafting.HasRecipe(defaultRecipes[i]))
                    Player.Crafting.LearnRecipe(defaultRecipes[i]);
        }

        private void SetRestrictions(PlayerInformation Information)
        {
            var modRestrictions = RestrictionsFactory.Instance.Build(Information.Class.GetType());
            for (var i = 0; i < modRestrictions.Length; i++)
                Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, modRestrictions[i]);

            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder,
                Information.Class.StartingItems.First(P => P.Key == PlayerInventory.WeaponHolder).Value.EquipmentType);
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.HoldableObject);
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
            GameManager.MakeCurrent(Information);
            Player.Realms.Reset();
            Player.Realms.Create(World.RandomSeed);
            Player.Realms.Create(World.RandomSeed, WorldType.GhostTown);
            Player.Realms.GoTo(RealmHandler.Overworld);
            Player.Model = new HumanoidModel(Player);
            if (Player.Inventory.MainWeapon != null)
            {
                Player.Inventory.MainWeapon.FlushCache();
                Player.SetWeapon(Player.Inventory.MainWeapon.Weapon);
            }

            SpawnCampfireDesign.AlignPlayer(Player);
            Player.Health = Player.MaxHealth;
            Player.Mana = Player.MaxMana;
            Player.Questing.Empty();
            Player.UI.HideMenu();
            Player.UI.Hide = false;
            Player.Enabled = true;
            _isNewRun = true;
        }

        private IEnumerator SpawnCoroutine()
        {
            SoundtrackManager.PlayAmbient();
            Player.SearchComponent<DamageComponent>().Immune = true;
            var chunkOffset = World.ToChunkSpace(Player.Position);
            World.StructureHandler.CheckStructures(chunkOffset);
            while (_loadingScreen.IsLoading) yield return null;
            Player.UI.HideMenu();
            Player.UI.GamePanel.Enable();

            Player.SearchComponent<DamageComponent>().Immune = false;
            GameManager.SpawningEffect = true;
            Player.Model.ApplyFog = true;
            Player.CanInteract = true;
            GameManager.Player.PlaySpawningAnimation = true;
            if (!GameManager.Player.MessageDispatcher.HasTitleMessages)
                GameManager.Player.MessageDispatcher.ShowTitleMessage(World.WorldBuilding.GenerateName(), 1.5f);
        }

        private IEnumerator LoadingScreenCoroutine()
        {
            var timer = new Timer(1f);
            var lastSeed = World.Seed;
            while (Program.GameWindow.Exists)
            {
                yield return null;
                if (timer.Tick() || lastSeed != World.Seed)
                {
                    lastSeed = World.Seed;
                    if (!_loadingScreen.ShouldShow) continue;
                    var wasMenuEnabled = Player.UI.Menu.Enabled || InStartMenu;
                    var wasGameUiEnabled = Player.UI.GamePanel.Enabled;
                    Player.UI.HideMenu();
                    Player.UI.GamePanel.Disable();
                    _loadingScreen.Show();
                    while (_loadingScreen.IsLoading)
                    {
                        Player.Physics.ResetFall();
                        Player.Physics.ResetVelocity();
                        Player.Physics.UsePhysics = false;
                        yield return null;
                    }

                    if (wasMenuEnabled)
                        Player.UI.ShowMenu();
                    else if (wasGameUiEnabled)
                        Player.UI.GamePanel.Enable();
                    Player.Position += Vector3.UnitY * 2;
                    Player.Physics.UsePhysics = true;
                }
            }
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

        public bool InMenu => Player != null && Player.UI.InMenu && !Player.UI.Hide && World.Seed != World.MenuSeed;

        public bool SpawningEffect
        {
            get => _spawningEffect;
            set
            {
                if (!value || SpawningEffect || GameSettings.Paused) return;
                _spawningEffect = true;
                GameSettings.BloomModifier = 8.0f;
                TaskScheduler.While(() => Math.Abs(GameSettings.BloomModifier - 1.0f) > .005f,
                    delegate
                    {
                        GameSettings.BloomModifier =
                            Mathf.Lerp(GameSettings.BloomModifier, 1.0f, Time.IndependentDeltaTime);
                    });
                TaskScheduler.When(() => Math.Abs(GameSettings.BloomModifier - 1.0f) < .005f, delegate
                {
                    GameSettings.BloomModifier = 1.0f;
                    _spawningEffect = false;
                });
            }
        }

        public IPlayer Player
        {
            get => _players[0];
            set => _players[0] = value;
        }

        public bool PlayerExists => _players != null && _players.Length > 0;
    }
}