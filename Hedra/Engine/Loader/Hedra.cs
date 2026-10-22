#if DEBUG
//#define SHOW_COLLISION
#endif

using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Hedra.API;
using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
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
        private bool _wasLoading;
        private bool _hasUpdatedGameplay;
        

        public Hedra(int Width, int Height, IMonitor Monitor, int Major, int Minor, ContextProfile Profile,
            ContextFlags Flags) :
            base(Width, Height, Monitor, Profile, Flags, new APIVersion(Major, Minor))
        {
        }

        public static int RenderingThreadId { get; private set; }
        public static int MainThreadId { get; private set; }
        public int BuildNumber => 20;
        public string GameVersion => /*"\u03B1 */"";

        public void Setup()
        {
            Title = $"Project Hedra {GameVersion}";
            if (!LoadBoilerplate())
            {
                Close();
            }
        }

        public bool FinishedLoadingSplashScreen => _splashScreen?.FinishedLoading ?? true;
        public event OnFrameChanged FrameChanged;

        protected override void Load()
        {
        }

        public bool LoadBoilerplate()
        {
            RenderingThreadId = Thread.CurrentThread.ManagedThreadId;
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
            using (StartupMetrics.Measure("assets")) AssetManager.Load();
            using (StartupMetrics.Measure("translations")) Translations.Load();
            _wasLoading = true;
            _debugProvider = new DebugInfoProvider();
            _splashScreen = new SplashScreen();
            Program.GameWindow.WindowState = WindowState.Maximized;

            using (StartupMetrics.Measure("compatibility")) CompatibilityManager.Load();
            using (StartupMetrics.Measure("renderer_allocation")) GameLoader.AllocateMemory();
            using (StartupMetrics.Measure("renderer")) Renderer.Load();
            Log.WriteLine("Supported GLSL version is : " + Renderer.GetString(StringName.ShadingLanguageVersion));
            OSManager.WriteSpecs();
            DrawSplashScreenAndSwap();
            Window.IsContextControlDisabled = true;
            Window.ClearContext();

            void DrawSplashScreenAndSwap()
            {
                _splashScreen.Draw();
                Window.SwapBuffers();
            }

            Task.Run(() =>
            {
                try
                {
                    MakeGLContextCurrent();
                    Time.RegisterThread();
                    
                    using (StartupMetrics.Measure("sound")) GameLoader.LoadSoundEngine();
                    using (StartupMetrics.Measure("content")) HedraContent.Register();
                    using (StartupMetrics.Measure("mods")) ModificationsLoader.Reload();
                    using (StartupMetrics.Measure("names")) NameGenerator.Load();
                    using (StartupMetrics.Measure("landforms")) LandformLoader.Load();
                    using (StartupMetrics.Measure("model_caches")) CacheManager.Load();

                    DrawSplashScreenAndSwap();

                    BackgroundUpdater.Load();
                    BulletPhysics.Load();
                    Log.WriteLine("Translations loaded successfully.");

                    GameLoader.CreateCharacterFolders();
                    Log.WriteLine("Assets loading was successful.");
                    GameSettings.LoadNormalSettings(GameSettings.SettingsPath);
                    Log.WriteLine("Setting loaded successfully.");

                    DrawSplashScreenAndSwap();

                    using (StartupMetrics.Measure("world")) GameManager.LoadWorld();
                    Log.WriteLine("Scene loading was Successful.");

                    DrawSplashScreenAndSwap();

                    using (StartupMetrics.Measure("player_ui")) GameManager.LoadPlayer();
                    Steam.Instance.Initialize();
                    Log.WriteLine("UI loading was Successful.");

                    DrawSplashScreenAndSwap();

                    using (StartupMetrics.Measure("scripts_missions")) LoadInterpreter();
                    Window.ClearContext();
                    Window.IsContextControlDisabled = false;
                    _splashScreen.Disable();
                    Window.WindowBorder = WindowBorder.Resizable;
                    StartupMetrics.Ready();
                }
                catch (Exception e)
                {
                    Log.WriteLine(e);
                    Log.WriteLine("Crashed while loading");
                    Window.Close();
                }
            });
            return true;
        }

        private static void LoadInterpreter()
        {
            Interpreter.Load();
            MissionPool.Load();
        }

        private void MakeGLContextCurrent()
        {
            RenderingThreadId = Thread.CurrentThread.ManagedThreadId;
            Window.MakeCurrent();
        }

        private readonly bool _profileFrames = Environment.GetEnvironmentVariable("HEDRA_PROFILE") == "1";
        private readonly System.Diagnostics.Stopwatch _profileClock = System.Diagnostics.Stopwatch.StartNew();
        private long _profileRenderTicks, _profileUpdateTicks;
        private int _profileRenderCount, _profileUpdateCount;

        protected override void UpdateFrame(double Delta)
        {
            if (!_splashScreen.FinishedLoading) return;
            if (RenderingThreadId != MainThreadId)
                MakeGLContextCurrent();

            GenerationBenchmark.Update();
            var profileStart = System.Diagnostics.Stopwatch.GetTimestamp();
            // Asset loading and a suspended window must not become minutes of
            // fixed-step simulation before the first playable frame.
            var frameTime = _hasUpdatedGameplay ? Math.Min(Delta, 0.1) : Physics.Timestep;
            _hasUpdatedGameplay = true;
            while (frameTime > 0f)
            {
                var isOnMenu = GameManager.InStartMenu;
                var delta = Math.Min(frameTime, Physics.Timestep);
                Time.Set(delta, false);
                using (GenerationMetrics.Measure("frame_physics")) BulletPhysics.Update(isOnMenu ? Time.IndependentDeltaTime : Time.DeltaTime);
                using (GenerationMetrics.Measure("frame_routines")) RoutineManager.Update();
                using (GenerationMetrics.Measure("frame_updates")) UpdateManager.Update();
                using (GenerationMetrics.Measure("frame_entities")) BackgroundUpdater.UpdateEntities();
                using (GenerationMetrics.Measure("frame_commands")) BackgroundUpdater.UpdateCommands();
                //BackgroundUpdater.Dispatch();
                using (GenerationMetrics.Measure("frame_world")) World.Update();
                SoundPlayer.Update(LocalPlayer.Instance.Position);
                SoundtrackManager.Update();
                if (!GenerationBenchmark.Enabled) AutosaveManager.Update();
                using (GenerationMetrics.Measure("frame_executer")) Executer.Update();
                using (GenerationMetrics.Measure("frame_uploads")) DistributedExecuter.Update();
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
            if (_profileFrames)
            {
                _profileUpdateTicks += System.Diagnostics.Stopwatch.GetTimestamp() - profileStart;
                _profileUpdateCount++;
            }
        }

        protected override void RenderFrame(double Delta)
        {
            if (!_splashScreen.FinishedLoading) return;
            if (RenderingThreadId != MainThreadId)
                MakeGLContextCurrent();

            var profileStart = System.Diagnostics.Stopwatch.GetTimestamp();
            Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);
            DrawManager.Draw();
            _debugProvider.Draw();
            if (_profileFrames)
            {
                _profileRenderTicks += System.Diagnostics.Stopwatch.GetTimestamp() - profileStart;
                _profileRenderCount++;
                if (_profileClock.Elapsed.TotalSeconds >= 5)
                {
                    var msPerTick = 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                    Log.WriteLine($"PERF fps={_profileRenderCount / _profileClock.Elapsed.TotalSeconds:F1} render_ms={_profileRenderTicks * msPerTick / _profileRenderCount:F2} update_ms={_profileUpdateTicks * msPerTick / Math.Max(1, _profileUpdateCount):F2} menu={GameManager.InStartMenu}");
                    _profileClock.Restart();
                    _profileRenderTicks = _profileUpdateTicks = 0;
                    _profileRenderCount = _profileUpdateCount = 0;
                }
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
            if (!_splashScreen.FinishedLoading) return;
            if (!GenerationBenchmark.Enabled)
            {
                GameSettings.Save($"{AssetManager.AppData}/settings.cfg");
                if (!GameManager.InStartMenu) AutosaveManager.Save();
            }
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
