using System;
using System.IO;
using System.Windows.Forms;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Native
{
    /// <summary>
    /// Description of OSManager.
    /// </summary>
    public static class OSManager
    {
        public static string CPUArchitecture { get; private set; }    
        public static string GraphicsCard { get; private set; }        
        public static int RamCount { get; private set; }        
        public static string Specs => CPUArchitecture+"|"+GraphicsCard+"|"+RamCount;
        private static readonly IConsoleManager _consoleManager;
        private static readonly IMessageManager _messageManager;

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
        }
        
        public static void Load(string ExecName)
        {
            if (IntPtr.Size == 4) Log.WriteLine("Running "+Program.GameWindow.GameVersion+" as x86");
            if (IntPtr.Size == 8) Log.WriteLine("Running "+Program.GameWindow.GameVersion+" as x64");

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

            RamCount = 8;
            GraphicsCard = Renderer.GetString(StringName.Vendor) + Environment.NewLine
             + Renderer.GetString(StringName.Renderer) + Environment.NewLine 
             + Renderer.GetString(StringName.Version);
            CPUArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            
            Log.WriteLine("OS = " + Environment.OSVersion + Environment.NewLine +
                          "CPU = " + OSManager.CPUArchitecture + Environment.NewLine +
                          "Graphics Card = " + OSManager.GraphicsCard + Environment.NewLine);
            
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
