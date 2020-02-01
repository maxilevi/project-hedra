using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
using Hedra.Engine.Scripting;
using Silk.NET.GLFW;
using Silk.NET.Windowing.Common;
using Monitor = Silk.NET.Windowing.Monitor;

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
                var baseText =  $":{Environment.NewLine}{Environment.NewLine}----STACK TRACE----{Environment.NewLine}{Environment.NewLine}{E.ExceptionObject.ToString()}{Environment.NewLine}{Environment.NewLine}----SCRIPT TRACE---{Environment.NewLine}{Environment.NewLine}{Interpreter.FormatException((Exception)E.ExceptionObject)}";
                if (E.IsTerminating)
                {
                    Log.WriteLine($"UNEXPECTED FATAL EXCEPTION {baseText}");
                    Log.FlushAndClose();
                    File.Copy($"{GameLoader.AppPath}/log.txt", $"{GameLoader.CrashesFolder}/CRASH_{DateTime.UtcNow:dd-MM-yyyy_hh-mm-ss}.txt");
                }
                else
                {
                    Log.WriteLine($"UNEXPECTED EXCEPTION {baseText}");
                    Log.Flush();
                    File.Copy($"{GameLoader.AppPath}/log.txt", $"{GameLoader.CrashesFolder}/CRASH_{DateTime.UtcNow:dd-MM-yyyy_hh-mm-ss}.txt");
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

        private static unsafe void InitializeResolutions()
        {
            var glfw = GlfwProvider.GLFW.Value;
            var videoModes = glfw.GetVideoModes(glfw.GetPrimaryMonitor(), out var count);
            var resolutions = new List<Vector2>();
            for (var i = 0; i < count; ++i)
            {
                resolutions.Add(new Vector2(videoModes[i].Width, videoModes[i].Height));
            }

            resolutions = resolutions.Distinct().ToList();
            if (GameSettings.ResolutionIndex == -1)
                GameSettings.ResolutionIndex = resolutions.Count - 1;
            GameSettings.AvailableResolutions = resolutions.ToArray();
        }
        
        private static unsafe void RunNormalAndDummyMode(bool DummyMode)
        {
            if(DummyMode) EnableDummyMode();
            LoadLibraries();
            InitializeResolutions();
            GameSettings.LoadSetupSettings(GameSettings.SettingsPath);
            
            var maxMonitor = Monitor.GetMainMonitor();
            var monitorSize = (maxMonitor.VideoMode.Resolution ?? maxMonitor.Bounds.Size);
            var maxSize = new Vector2(monitorSize.Width, monitorSize.Height);
            var currentSize = GameSettings.AvailableResolutions[GameSettings.ResolutionIndex];
            
            GameSettings.DeviceWidth = (int)maxSize.X;
            GameSettings.DeviceHeight = (int)maxSize.Y;

            Log.WriteLine("Creating the window on the Primary Device at " + GameSettings.DeviceWidth + "x" +
                            GameSettings.DeviceHeight);

            GameSettings.Width = (int)currentSize.X;
            GameSettings.Height = (int)currentSize.Y;
            GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;
            var profile = ContextProfile.Core;
            var flags = ContextFlags.Default;
#if DEBUG
            profile = ContextProfile.Compatability;
            flags = ContextFlags.Debug;
#endif
            GameWindow = new Loader.Hedra((int)currentSize.X, (int)currentSize.Y, maxMonitor, 3, 3, profile, flags);
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

