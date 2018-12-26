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
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.Engine.WorldBuilding;
using Hedra.Sound;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Loader
{
    class Hedra : HedraWindow, IHedra
    {
        public DebugInfoProvider DebugProvider { get; private set; }
        public SplashScreen SplashScreen { get; private set; }
        public static int MainThreadId { get; private set; }
        public string GameVersion { get; private set; }
        private float _lastValue = float.MinValue;
        private bool _forcingResize;
        private int _passedFrames;
        private double _passedMillis;
        
        public Hedra(int Width, int Height, GraphicsMode Mode, string Title, DisplayDevice Device, int Minor, int Major) 
            : base(Width, Height, Mode, Title, GameWindowFlags.Default, Device, Major, Minor, GraphicsContextFlags.Default){}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            GameVersion = "\u03B1 0.45";
            Title = $"{Title} {GameVersion}";

            OSManager.Load(Assembly.GetExecutingAssembly().Location);
            AssetManager.Load();
            CompatibilityManager.Load();
            GameLoader.LoadArchitectureSpecificFiles(GameLoader.AppPath);
            GameLoader.LoadSoundEngine();
            HedraContent.Register();
            ModificationsLoader.Reload();
            NameGenerator.Load();
            CacheManager.Load();
            Translations.Load();
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
 
            var glVersion = Renderer.GetString(StringName.Version);
            var shadingOpenGlVersion = GetShadingVersion(glVersion);

            if( shadingOpenGlVersion < 3.1f)
            {
                
                OSManager.Show($"Minimum OpenGL version is 3.1, yours is {shadingOpenGlVersion}", "OpenGL Version not supported");
                Exit();
            }
            DebugProvider = new DebugInfoProvider();
            SplashScreen = new SplashScreen();
            Log.WriteLine(glVersion);
        }

        protected override void OnUpdateFrame(double Delta)
        {
            base.OnUpdateFrame(Delta);

            this.SplashScreen.Update();
            var frameTime = Delta;
            while (frameTime > 0f)
            {
                var delta = Math.Min(frameTime, Physics.Timestep);
                Time.Set(delta, false);
                CoroutineManager.Update();
                UpdateManager.Update();
                World.Update();
                SoundPlayer.Update(LocalPlayer.Instance.Position);
                SoundtrackManager.Update();
                AutosaveManager.Update();
                Executer.Update();
                DistributedExecuter.Update();
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
            DebugProvider.Update();
            AnalyticsManager.PlayTime += (float) Delta;
        }

        protected override void OnRenderFrame(double Delta)
        {    
            base.OnRenderFrame(Delta);
            Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);        
            if (!SplashScreen.FinishedLoading)
            {
                SplashScreen.Draw();
            }
            else
            {
                DrawManager.Draw();
                DebugProvider.Draw();
            }
            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GameSettings.SurfaceWidth = Width;
            GameSettings.SurfaceHeight = Height;
            DrawManager.UIRenderer.Adjust();
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            if(!this.Focused && SplashScreen.FinishedLoading)
            {
                if(!GameManager.InStartMenu && !GameManager.IsLoading && !GameSettings.Paused &&
                    GameManager.Player != null && !GameManager.Player.InterfaceOpened)
                {
                    GameManager.Player.UI.ShowMenu();
                }
            }
        }
        
        protected override void OnUnload(EventArgs e)
        {
            AssetManager.Dispose();
            GameSettings.Save(AssetManager.AppData + "settings.cfg");
            if(!GameManager.InStartMenu) AutosaveManager.Save();
            Graphics2D.Dispose();
            DrawManager.Dispose();
            InventoryItemRenderer.Framebuffer.Dispose();
            base.OnUnload(e);
        }

        private static float GetShadingVersion(string GLVersion)
        {
            GLVersion = GLVersion.Replace(",", ".");
            var shadingReg = new Regex(@"([0-9]+\.[0-9]+|[0-9]+)");
            var shadingStringVersion = shadingReg.Match(GLVersion);
            var shadingOpenGlVersion = float.Parse(shadingStringVersion.Value, CultureInfo.InvariantCulture);
            return (shadingOpenGlVersion >= 310) ? shadingOpenGlVersion / 100f : shadingOpenGlVersion;
        }
    }
}

