#if DEBUG
//#define SHOW_COLLISION
#endif

using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Hedra.API;
using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Networking;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Scripting;
using Hedra.Engine.Steamworks;
using Hedra.Engine.Windowing;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Mission;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Hedra.Engine.Loader
{
    public delegate void OnFrameChanged();

    public class Hedra : HedraWindow, IHedra
    {
        private DebugInfoProvider _debugProvider;
        private bool _forcingResize;
        private float _lastValue = float.MinValue;
        private int _passedFrames;
        private double _passedMillis;
        private SplashScreen _splashScreen;

        public Hedra(int Width, int Height, IMonitor Monitor, int Major, int Minor, ContextProfile Profile,
            ContextFlags Flags) :
            base(Width, Height, Monitor, Profile, Flags, new APIVersion(Major, Minor))
        {
        }

        public static int MainThreadId { get; private set; }
        public int BuildNumber => 17;
        public string GameVersion => /*"\u03B1 */"1.0";

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

        public bool FinishedLoadingSplashScreen => _splashScreen?.FinishedLoading ?? true;
        public event OnFrameChanged FrameChanged;

        protected override void Load()
        {
        }

        public static bool LoadBoilerplate()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            Time.RegisterThread();
            OSManager.Load(Assembly.GetExecutingAssembly().Location);
            GameLoader.CreateCrashesFolderIfNecessary();
            Renderer.LoadProvider();
            var glVersion = Renderer.GetString(StringName.Version);
            var shadingOpenGlVersion = GetShadingVersion(glVersion);
            var maxUniforms = Renderer.GetInteger(GetPName.MaxVertexUniformVectors);
            if (maxUniforms / 4 < GeneralSettings.MaxJoints)
                OSManager.Show($"Max uniforms is '{maxUniforms}' but max joints is '{GeneralSettings.MaxJoints}'",
                    "Warning");
            else
                Log.WriteLine($"Max uniforms is '{maxUniforms}' but max joints is '{GeneralSettings.MaxJoints}'");

            if (shadingOpenGlVersion < 3.3f)
            {
                OSManager.Show($"Minimum OpenGL version is 3.3, yours is {shadingOpenGlVersion}",
                    "OpenGL Version not supported");
                return false;
            }

            Log.WriteLine(glVersion);

            AssetManager.Load();
            CompatibilityManager.Load();
            GameLoader.LoadSoundEngine();
            HedraContent.Register();
            ModificationsLoader.Reload();
            NameGenerator.Load();
            CacheManager.Load();
            Translations.Load();
            BackgroundUpdater.Load();
            BulletPhysics.Load();
            Log.WriteLine("Translations loaded successfully.");

            GameLoader.CreateCharacterFolders();
            GameLoader.AllocateMemory();
            Log.WriteLine("Assets loading was successful.");

            GameSettings.LoadNormalSettings(GameSettings.SettingsPath);
            Log.WriteLine("Setting loaded successfully.");

            Renderer.Load();
            Log.WriteLine("Supported GLSL version is : " + Renderer.GetString(StringName.ShadingLanguageVersion));
            OSManager.WriteSpecs();

            GameManager.Load();
            Log.WriteLine("Scene loading was Successful.");

            Steam.Instance.Initialize();
            Log.WriteLine("Hooking steam into necessary events...");

            LoadInterpreter();
            Program.GameWindow.WindowState = WindowState.Maximized;
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
            _splashScreen.Update();
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
            if (newNumber < 0.0005f && _lastValue < 0.0005f)
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
            AnalyticsManager.PlayTime += (float)Delta;
            FrameChanged?.Invoke();
        }

        protected override void RenderFrame(double Delta)
        {
            Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit |
                           ClearBufferMask.StencilBufferBit);
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

        protected override void Resize(Vector2D<int> NewSize)
        {
            GameSettings.SurfaceWidth = NewSize.X;
            GameSettings.SurfaceHeight = NewSize.Y;
            DrawManager.UIRenderer.Adjust();
        }

        protected override void FocusChanged(bool IsFocused)
        {
            if (_splashScreen == null || !_splashScreen.FinishedLoading) return;
            if (!IsFocused)
                if (!GameManager.InStartMenu && !GameManager.IsLoading && !GameSettings.Paused &&
                    GameManager.Player != null && !GameManager.Player.InterfaceOpened)
                    GameManager.Player.UI.ShowMenu();
        }

        protected override void Unload()
        {
            AssetManager.Dispose();
            GameSettings.Save($"{AssetManager.AppData}/settings.cfg");
            if (!GameManager.InStartMenu) AutosaveManager.Save();
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
            return shadingOpenGlVersion >= 310 ? shadingOpenGlVersion / 100f : shadingOpenGlVersion;
        }
    }
}