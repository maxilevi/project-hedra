using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.Steamworks;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Loader;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Networking;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using System.Numerics;
using System.Runtime;
using Silk.NET.GLFW;
using Silk.NET.Windowing.Common;

namespace Hedra.Engine
{
    public static class Program
    {
        public static bool IsDebug { get; private set; }
        public static bool IsRelease => !IsDebug;
        public static bool IsServer { get; private set; }
        public static bool IsDummy { get; private set; }
        public static IHedra GameWindow { get; set; }

        [HandleProcessCorruptedStateExceptions]
        private static void Main(string[] Args)
        {
            //GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            void ProcessException(object S, UnhandledExceptionEventArgs E)
            {
                if (E.IsTerminating)
                {
                    Log.WriteLine($"UNEXPECTED EXCEPTION :{Environment.NewLine}{Environment.NewLine}----STACK TRACE----{Environment.NewLine}{Environment.NewLine}{E.ExceptionObject.ToString()}");
                }
            };
            AppDomain.CurrentDomain.UnhandledException += ProcessException;
            
            var dummyMode = Args.Length == 1 && Args[0] == "--dummy-mode";
            var serverMode = Args.Length == 1 && Args[0] == "--server-mode";
            var joinArgs = Args.Length == 2 && Args[0] == "--join";

            if (joinArgs)
            {
                Executer.ExecuteOnMainThread(() =>
                {
                    GameManager.MakeCurrent(DataManager.PlayerFiles[0]);
                    Network.Instance.Connect(ulong.Parse(Args[1]));       
                });
            }
            
            if (serverMode)
            {
                RunDedicatedServer();
            }
            else
            {

                RunNormalAndDummyMode(dummyMode);
            }

            Environment.Exit(0);
        }

        private static void RunDedicatedServer()
        {
            LoadLibraries();
           
            IsServer = IsDummy = true;
            GameWindow = new HedraServer();
            GameWindow.Run();
            
            DisposeLibraries();
        }

        private static void LoadLibraries()
        {
#if DEBUG
            IsDebug = true;
#endif
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            Steam.Instance.Load();
        }

        private static void DisposeLibraries()
        {
            Steam.Instance.Dispose();
        }
        
        private static unsafe void RunNormalAndDummyMode(bool DummyMode)
        {
            if(DummyMode) EnableDummyMode();
            LoadLibraries();

            var screens = Screen.AllScreens;
            var maxSize = Vector2.Zero;
            for (var i = 0; i < screens.Length; ++i)
            {
                var screenBounds = new Vector2(screens[i].Bounds.Width, screens[i].Bounds.Height);
                if (screenBounds.LengthSquared() > maxSize.LengthSquared())
                {
                    maxSize = screenBounds;
                }
                Log.WriteLine($"Found screen size '{screenBounds}'");
            }
            var bounds = maxSize;
            GameSettings.DeviceWidth = (int)bounds.X;
            GameSettings.DeviceHeight = (int)bounds.Y;

            Log.WriteLine("Creating the window on the Primary Device at " + GameSettings.DeviceWidth + "x" +
                            GameSettings.DeviceHeight);

            GameSettings.Width = GameSettings.DeviceWidth;
            GameSettings.Height = GameSettings.DeviceHeight;
            GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;
            var profile = ContextProfile.Core;
            var flags = ContextFlags.Default;
#if DEBUG
            profile = ContextProfile.Compatability;
            flags = ContextFlags.Debug;
#endif
            GameWindow = new Loader.Hedra(GameSettings.Width, GameSettings.Height, 3, 3, profile, flags);
            GameWindow.Setup();

            if (OSManager.RunningPlatform == Platform.Windows)
            {
                Log.WriteLine("Loading Icon...");
                var pixels = AssetManager.LoadIcon("Assets/Icon.ico", out var width, out var height);
                fixed (byte* ptr = pixels)
                {
                    GameWindow.SetIcon(new Silk.NET.GLFW.Image
                    {
                        Width = width,
                        Height = height,
                        Pixels = ptr
                    });
                }
            }
            
            GameSettings.SurfaceWidth = GameWindow.Width;
            GameSettings.SurfaceHeight = GameWindow.Height;

            GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;    

            GameSettings.LoadWindowSettings(GameSettings.SettingsPath);
            Log.WriteLine("Window settings loading was Successful");
            if (!IsDummy || IsDummy && IsServer)
            {
                GameWindow.Run();
            }

            DisposeLibraries();
        }
        
        private static void EnableDummyMode()
        {
            IsDummy = true;
            Renderer.Provider = new DummyGLProvider();
            Log.WriteLine("Dummy Mode: ENABLED");
        }
    }
}

