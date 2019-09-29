using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.Steamworks;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Loader;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using OpenTK;
using OpenTK.Graphics;

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
            /*var gen = new NormalBiomeGenerationDesign();
            var noise = new FastNoiseSIMD(1);
            var s = 1024;
            var map = CreateMap<float>(s);
            gen.BuildRiverMap(noise, map, s, 1, Vector2.Zero);
            var bmp = new Bitmap(s, s);
            var data = bmp.LockBits(new Rectangle(0,0,bmp.Width,bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            unsafe
            {
                var dataPtr = (byte*)data.Scan0;
                var stride = data.Stride;
                if(dataPtr == null) throw new ArgumentNullException("dataPrt cannot be null");
                for (var y = 0; y < bmp.Height; y++)
                {
                    for (var x = 0; x < bmp.Width; x++)
                    {
                        var shade = (byte) Math.Min(255, map[x][y] * 255);
                        dataPtr[x * 4 + y * stride] = shade;
                        dataPtr[x * 4 + y * stride + 1] = shade;
                        dataPtr[x * 4 + y * stride + 2] = shade;
                        dataPtr[x * 4 + y * stride + 3] = 255;
                    }
                }
            }
            bmp.UnlockBits(data);
            bmp.Save("./rivers.png");
            return;*/
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
        
        
        private static T[][] CreateMap<T>(int Width)
        {
            var arr = new T[Width][];
            for (var x = 0; x < Width; ++x)
            {
                arr[x] = new T[Width];
            }
            return arr;
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
        
        private static void RunNormalAndDummyMode(bool DummyMode)
        {
            if(DummyMode) EnableDummyMode();
            LoadLibraries();
            
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
#if !DEBUG
            if (OSManager.RunningPlatform == Platform.Windows)
            {
                Log.WriteLine("Loading Icon...");
                GameWindow.Icon = AssetManager.LoadIcon("Assets/Icon.ico");
            }
#endif
            GameSettings.SurfaceWidth = GameWindow.Width;
            GameSettings.SurfaceHeight = GameWindow.Height;

            GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;    

            GameSettings.LoadWindowSettings(GameSettings.SettingsPath);
            Log.WriteLine("Window settings loading was Successful");
            if (!IsDummy || IsDummy && IsServer)
            {
                GameWindow.Run();
            }
            else
            {
                GameWindow.RunOnce();
                Log.WriteLine("Project Hedra loaded successfully. Exiting...");
                Environment.Exit(0);
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

