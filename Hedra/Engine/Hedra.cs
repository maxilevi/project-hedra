#if DEBUG
    //#define SHOW_COLLISION
#endif

using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Loader;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.Inventory;
using Forms = System.Windows.Forms;

namespace Hedra
{
    class Hedra : GameWindow, IEventProvider
    {
	    public DebugInfoProvider DebugProvider { get; private set; }
	    public SplashScreen SplashScreen { get; private set; }
	    public static int MainThreadId { get; private set; }
	    public string GameVersion { get; private set; }
        private float _lastValue = float.MinValue;
	    private bool _forcingResize;
		
	    public Hedra(int Width, int Height) : base(Width, Height){}
	    
		public Hedra(int Width, int Height, GraphicsMode Mode, string Title, DisplayDevice Device, int Minor, int Major) 
			: base( Width, Height, Mode, Title, GameWindowFlags.Default, Device/*, Major, Minor, GraphicsContextFlags.ForwardCompatible*/){}

		protected override void OnLoad(EventArgs e)
        {
			base.OnLoad(e);
	        MainThreadId = Thread.CurrentThread.ManagedThreadId;
	        GameVersion = "α 0.38";
	        Title = $"{Title} {GameVersion}";
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
		    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + "Project Hedra/";

            OSManager.Load(Assembly.GetExecutingAssembly().Location);
		    AssetManager.Load();
            CompatibilityManager.Load(appPath);
            GameLoader.LoadArchitectureSpecificFiles(appPath);
            GameLoader.CreateCharacterFolders(appData, appPath);
	        GameLoader.AllocateMemory();
			NameGenerator.Load();
			CacheManager.Load();
			Physics.Threading.Load();
			Log.WriteLine("Assets loading was Successful.");
	        
		    GameLoader.LoadSoundEngine();
            GameSettings.VSync = true;
			GameSettings.Load($"{appData}settings.cfg");
			Log.WriteLine("Settings loading was Successful");
	        
            GameManager.Load();
			Log.WriteLine("Scene loading was Successful.");
	        
	        Renderer.Load();
			Log.WriteLine("Supported GLSL version is : "+Renderer.GetString(StringName.ShadingLanguageVersion));
	        
	        var glVersion = Renderer.GetString(StringName.Version);
            var shadingOpenGlVersion = GetShadingVersion(glVersion);

			if( shadingOpenGlVersion < 3.1f)
			{
				Forms.MessageBox.Show("Minimum OpenGL version is 3.1, yours is "+shadingOpenGlVersion, "OpenGL Version not supported",
				                      Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
				Exit();
			}
			DebugProvider = new DebugInfoProvider();
	        SplashScreen = new SplashScreen();
	        Log.WriteLine(glVersion);
        }


	    protected override void OnUpdateFrame(FrameEventArgs e)
        {
			base.OnUpdateFrame(e);
	        this.SplashScreen.Update();
            var frameTime = e.Time;
            while (frameTime > 0f)
            {
                var delta = Math.Min(frameTime, Physics.Timestep);
                Time.Set(delta);
                CoroutineManager.Update();
                UpdateManager.Update();
                Physics.Update();
                World.Update();
                SoundManager.Update(LocalPlayer.Instance.Position);
                SoundtrackManager.Update();
                AutosaveManager.Update();
                Executer.Update();
                DistributedExecuter.Update();
                frameTime -= delta;
            }
            Time.Set(e.Time);
            Time.IncrementFrame(e.Time);
	        
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
	        AnalyticsManager.PlayTime += (float) e.Time;
	        Utils.CalculateFrameRate();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{    
			base.OnRenderFrame(e);
			
			Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);		
			if (!SplashScreen.FinishedLoading)
			{
				SplashScreen.Draw();
			}
			else
			{
				DrawManager.Draw();
			}
			this.DebugProvider.Draw();
            this.SwapBuffers();
		}

        protected override void OnResize(EventArgs e)
	    {
            base.OnResize(e);
	        if (_forcingResize) return;

	        _forcingResize = true;
	        Width = GameSettings.Width;
	        Height = GameSettings.Height;
	        _forcingResize = false;
        }

		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);
			if(!this.Focused && SplashScreen.FinishedLoading)
			{
				if(!GameManager.InStartMenu && !GameManager.IsLoading && !GameSettings.Paused &&
                    GameManager.Player != null && !GameManager.Player.Inventory.Show &&
                    !GameManager.Player.AbilityTree.Show && !GameManager.Player.Trade.Show)
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

