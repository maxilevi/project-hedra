using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Native
{
    /// <summary>    
    /// Description of OSManager.
    /// </summary>
    public static class OSManager
    {
        public static GraphicsCardType GraphicsCard { get; set; }       
        public static int RamCount { get; private set; }
        public static string Specs => GraphicsCard+"|"+RamCount;
        private static readonly IConsoleManager _consoleManager;
        private static readonly IMessageManager _messageManager;
        private static readonly IScreenManager _screenManager;

        static OSManager()
        {
            _consoleManager = RunningPlatform == Platform.Windows
                ? new WindowsConsoleManager() 
                : (IConsoleManager) new DummyConsoleManager();
            _messageManager = RunningPlatform == Platform.Windows
                ? new WindowsMessageManager()
                : RunningPlatform == Platform.Linux 
                    ? new LinuxMessageManager()
                    : (IMessageManager) new DummyMessageManager();
            _screenManager = RunningPlatform == Platform.Windows
                ? new WindowsScreenManager()
                : (IScreenManager) new DummyScreenManager();
        }
        
        public static void Load(string ExecName)
        {
            if (IntPtr.Size == 4) Log.WriteLine($"Running {Program.GameWindow.GameVersion} as x86");
            if (IntPtr.Size == 8) Log.WriteLine($"Running {Program.GameWindow.GameVersion} as x64");

            if (RunningPlatform == Platform.Windows)
            {
                if (IntPtr.Size == 4 && NvidiaGPUFix32.SOP_SetProfile("Hedra", Path.GetFileName(ExecName)) == NvidiaGPUFix32.RESULT_CHANGE)
                {
                    MessageBox.Show(
                        "Your game is now configured to use your high Performance NVIDIA Graphics card. This requires a restart so please start the game again.");
                    Program.GameWindow.Close();
                    return;
                }
                if (IntPtr.Size == 8 && NvidiaGPUFix64.SOP_SetProfile("Hedra", Path.GetFileName(ExecName)) == NvidiaGPUFix64.RESULT_CHANGE)
                {
                    MessageBox.Show(
                        "Your game is now configured to use your high Performance NVIDIA Graphics card. This requires a restart so please start the game again.");
                    Program.GameWindow.Close();
                    return;
                }
            }
        }

        public static void WriteSpecs()
        {
            var graphicsCard = Renderer.GetString(StringName.Vendor) + Environment.NewLine
                                                                     + Renderer.GetString(StringName.Renderer) + Environment.NewLine 
                                                                     + Renderer.GetString(StringName.Version);
            GraphicsCard = DetectCard(graphicsCard);
            
            Log.WriteLine("OS = " + Environment.OSVersion + Environment.NewLine +
                          "CPU = " + Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") + Environment.NewLine +
                          "Graphics Card = " + OSManager.GraphicsCard + Environment.NewLine
            );
        }
/*
        public static int GetAvailableGraphicsRam(int Default)
        {
            if (GraphicsCard == GraphicsCardType.Unknown || GraphicsCard == GraphicsCardType.Intel)
                return Default;
            var mem = CompatibilityManager.QueryAvailableVideoMemory();
            if (mem == 0) return Default;
            return mem;
        }*/

        private static GraphicsCardType DetectCard(string Card)
        {
            var amdKeywords = new[] {"amd", "ati", "radeon"};
            var nvidiaKeywords = new[] {"nvidia", "gtx", "geforce"};
            var intelKeywords = new[] {"intel"};

            bool Matches(string[] Keywords, string Str)
            {
                return Keywords.Any(S => Regex.IsMatch(Str.ToLowerInvariant(), $@"\b{S}\b"));
            }

            if (Matches(intelKeywords, Card))
                return GraphicsCardType.Intel;
            if (Matches(nvidiaKeywords, Card))
                return GraphicsCardType.Nvidia;
            if (Matches(amdKeywords, Card))
                return GraphicsCardType.Amd;
            return GraphicsCardType.Unknown;
        }

        public static void Show(string Message, string Title)
        {
            _messageManager.Show(Message, Title);
        }

        public static bool ShowConsole
        {
            get => _consoleManager.Show;
            set => _consoleManager.Show = value;
        }

        public static bool CanHideConsole => !(_consoleManager is DummyConsoleManager);

        public static Vector2[] GetResolutions()
        {
            return _screenManager.GetResolutions();
        }

        public static Platform RunningPlatform
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                        // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                        if (Directory.Exists("/Applications")
                            & Directory.Exists("/System")
                            & Directory.Exists("/Users")
                            & Directory.Exists("/Volumes"))
                            return Platform.Mac;
                        else
                            return Platform.Linux;
        
                    case PlatformID.MacOSX:
                        return Platform.Mac;
        
                    default:
                        return Platform.Windows;
                }
            }
        }
    }
}
