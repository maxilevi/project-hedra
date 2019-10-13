#if DEBUG
    //#define SHOW_COLLISION
#endif

using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Threading;
using Hedra.API;
using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Networking;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Scripting;
using Hedra.Engine.Sound;
using Hedra.Engine.Steamworks;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Mission;
using Hedra.Rendering;
using Hedra.Sound;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;
using Hedra.WorldObjects;
using OpenToolkit.Mathematics;
using Silk.NET.Windowing.Common;

namespace Hedra.Engine.Loader
{
    public delegate void OnFrameChanged();
    
    public class Hedra : HedraWindow, IHedra
    {
        public string GameVersion => /*"\u03B1 */"1.0";
        public event OnFrameChanged FrameChanged;
        private DebugInfoProvider _debugProvider;
        private SplashScreen _splashScreen;
        public static int MainThreadId { get; private set; }
        private float _lastValue = float.MinValue;
        private bool _forcingResize;
        private int _passedFrames;
        private double _passedMillis;

        public Hedra(int Width, int Height, int Major, int Minor, ContextProfile Profile, ContextFlags Flags) : 
            base(Width, Height, Profile, Flags, new APIVersion(Major, Minor))
        {
        }

        protected override void Load()
        {
        }

        public void Setup()
        {
            Title = $"Project Hedra {GameVersion}";
            if (!LoadBoilerplate())
            {
                Close();
            }
            else
            {
                _debugProvider = new DebugInfoProvider();
                _splashScreen = new SplashScreen();
            }
        }

        public static bool LoadBoilerplate()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Time.RegisterThread();
            OSManager.Load(Assembly.GetExecutingAssembly().Location);
            
            var glVersion = Renderer.GetString(StringName.Version);
            var shadingOpenGlVersion = GetShadingVersion(glVersion);

            if( shadingOpenGlVersion < 3.3f)
            {
                
                OSManager.Show($"Minimum OpenGL version is 3.3, yours is {shadingOpenGlVersion}", "OpenGL Version not supported");
                return false;
            }
            Log.WriteLine(glVersion);
            
            AssetManager.Load();
            CompatibilityManager.Load();
            //GameLoader.LoadSoundEngine();
            HedraContent.Register();
            ModificationsLoader.Reload();
            NameGenerator.Load();
            CacheManager.Load();
            Translations.Load();
            BackgroundUpdater.Load();
            BulletPhysics.Load();
            Log.WriteLine("Translations loaded successfully.");
            
            GameLoader.CreateCharacterFolders(GameLoader.AppData, GameLoader.AppPath);
            GameLoader.AllocateMemory();
            Log.WriteLine("Assets loading was successful.");
            
            GameSettings.LoadNormalSettings(GameSettings.SettingsPath);
            Log.WriteLine($"Setting loaded successfully.");

            Renderer.Load();
            Log.WriteLine("Supported GLSL version is : "+Renderer.GetString(StringName.ShadingLanguageVersion));
            
            GameManager.Load();
            Log.WriteLine("Scene loading was Successful.");
            
            LoadInterpreter();
            return true;
        }

        private static void LoadInterpreter()
        {
            //if(GameSettings.DebugMode)
            //    TaskScheduler.Parallel(Interpreter.Load);
            //else
                Interpreter.Load();
            MissionPool.Load();
        }

        protected override void UpdateFrame(double Delta)
        {
            this._splashScreen.Update();
            var frameTime = Delta;
            while (frameTime > 0f)
            {
                var isOnMenu = GameManager.InStartMenu;
                var delta = Math.Min(frameTime, Physics.Timestep);
                Time.Set(delta, false);
                BulletPhysics.Update(isOnMenu ? Time.IndependentDeltaTime : Time.DeltaTime);
                RoutineManager.Update();
                UpdateManager.Update();
                BackgroundUpdater.Dispatch();
                World.Update();
                SoundPlayer.Update(LocalPlayer.Instance.Position);
                SoundtrackManager.Update();
                AutosaveManager.Update();
                Executer.Update();
                DistributedExecuter.Update();
                Network.Instance.Update();
                LocalPlayer.Instance.Loader.Dispatch();
                frameTime -= delta;
            }
            Time.Set(Delta);
            Time.IncrementFrame(Delta);
            
            // Utils.RNG is not thread safe so it might break itself
            // Here is the autofix because thread-safety is for loosers
            var newNumber = Utils.Rng.NextFloat();
            if(newNumber < 0.0005f && _lastValue < 0.0005f)
            {
                Utils.Rng = new Random();
                _lastValue = float.MinValue;
            }
            else if (_lastValue > 0.0005f)
            {
                _lastValue = newNumber;
            }
            _debugProvider.Update();
            Steam.Update();
            AnalyticsManager.PlayTime += (float) Delta;
            FrameChanged?.Invoke();
        }

        protected override void RenderFrame(double Delta)
        {    
            Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);        
            if (!_splashScreen.FinishedLoading)
            {
                _splashScreen.Draw();
            }
            else
            {
                DrawManager.Draw();
                _debugProvider.Draw();
            }
        }

        protected override void Resize(Size NewSize)
        {
            GameSettings.SurfaceWidth = NewSize.Width;
            GameSettings.SurfaceHeight = NewSize.Height;
            DrawManager.UIRenderer.Adjust();
        }

        protected override void FocusChanged(bool IsFocused)
        {
            if(!IsFocused && _splashScreen.FinishedLoading)
            {
                if(!GameManager.InStartMenu && !GameManager.IsLoading && !GameSettings.Paused &&
                    GameManager.Player != null && !GameManager.Player.InterfaceOpened)
                {
                    GameManager.Player.UI.ShowMenu();
                }
            }
        }
        
        protected override void Unload()
        {
            AssetManager.Dispose();
            GameSettings.Save(AssetManager.AppData + "settings.cfg");
            if(!GameManager.InStartMenu) AutosaveManager.Save();
            Graphics2D.Dispose();
            DrawManager.Dispose();
            InventoryItemRenderer.Dispose();
        }

        private static float GetShadingVersion(string GLVersion)
        {
            GLVersion = GLVersion.Replace(",", ".");
            var shadingReg = new Regex(@"([0-9]+\.[0-9]+|[0-9]+)");
            var shadingStringVersion = shadingReg.Match(GLVersion);
            var shadingOpenGlVersion = float.Parse(shadingStringVersion.Value, CultureInfo.InvariantCulture);
            return (shadingOpenGlVersion >= 310) ? shadingOpenGlVersion / 100f : shadingOpenGlVersion;
        }
        
        public bool FinishedLoadingSplashScreen => _splashScreen?.FinishedLoading ?? true;
    }
}

