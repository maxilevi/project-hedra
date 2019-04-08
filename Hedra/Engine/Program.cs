using System;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Steamworks;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Loader;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Networking;
using Hedra.Engine.Rendering;
using OpenTK;
using OpenTK.Graphics;

namespace Hedra.Engine
{
    public static class Program
    {
        public static bool IsDebug { get; private set; }
        public static bool IsRelease => !IsDebug;
        public static bool IsDummy { get; private set; }
        public static IHedra GameWindow { get; set; }

        private static void Main(string[] Args)
        {
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
            
        }
        
        private static void RunNormalAndDummyMode(bool DummyMode)
        {
            if(DummyMode) EnableDummyMode();
            #if DEBUG
            IsDebug = true;
            #endif

            GameLoader.LoadArchitectureSpecificFiles(GameLoader.AppPath);
            Steam.Instance.Load();
            
            var device = DisplayDevice.Default;
            Log.WriteLine(device.Bounds.ToString());
            GameSettings.DeviceWidth = device.Width;
            GameSettings.DeviceHeight = device.Height;

            Log.WriteLine("Creating the window on the Primary Device at " + GameSettings.DeviceWidth + "x" +
                            GameSettings.DeviceHeight);

            GameSettings.Width = GameSettings.DeviceWidth;
            GameSettings.Height = GameSettings.DeviceHeight;
            GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;

            GameWindow = new Loader.Hedra(GameSettings.Width, GameSettings.Height, GraphicsMode.Default, "Project Hedra", device, 3, 3)
            {
                WindowState = WindowState.Maximized
            };
            if (OSManager.RunningPlatform == Platform.Windows)
            {
                Log.WriteLine("Loading Icon...");
                GameWindow.Icon = AssetManager.LoadIcon("Assets/Icon.ico");
            }

            GameSettings.SurfaceWidth = GameWindow.Width;
            GameSettings.SurfaceHeight = GameWindow.Height;

            GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;    

            GameSettings.LoadWindowSettings(GameSettings.SettingsPath);
            Log.WriteLine("Window settings loading was Successful");
            if (!IsDummy)
            {
#if DEBUG
                GameWindow.Run();
#else
                try
                {
                    GameWindow.Run();
                }
                catch (Exception e)
                {
                    Log.WriteLine(e);
                    throw;
                }
#endif
            }
            else
            {
                GameWindow.RunOnce();
                Log.WriteLine("Project Hedra loaded successfully. Exiting...");
                Environment.Exit(0);
            }
            
            Steam.Instance.Dispose();
        }
        
        private static void EnableDummyMode()
        {
            IsDummy = true;
            Renderer.Provider = new DummyGLProvider();
            Log.WriteLine("Dummy Mode: ENABLED");
        }
    }
}

