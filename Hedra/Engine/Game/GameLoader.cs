using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Sound;
using Hedra.Sound;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;
using Silk.NET.OpenGL;

namespace Hedra.Engine.Game
{
    public static class GameLoader
    {
        private static bool _loadedArchitectureFiles;
        
        public static string AppData =>
            $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Project Hedra/".Replace("\\", "/");
        
        public static string AppPath =>
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/".Replace("\\", "/");

        public static string CrashesFolder => $"{AppPath}/Crashes/";
        
        public static void AllocateMemory()
        {
            Log.WriteLine("Initializing world renderer...");
            WorldRenderer.Initialize();
            WorldRenderer.Allocate();
            
            Log.WriteLine($"Detected video card type is '{OSManager.GraphicsCard}'");
            /*Log.WriteLine("Querying available VRAM ...");

            /* If the card is integrated then clamp to 20 *
            GeneralSettings.MaxLoadingRadius = OSManager.GraphicsCard == GraphicsCardType.Intel
                ? 20
                : GeneralSettings.MaxLoadingRadius;
            Log.WriteLine($"Setting max world radius to '{GeneralSettings.MaxLoadingRadius}'");*/
            Log.WriteLine("Allocating world memory...");
            
            
            int staticMem = WorldRenderer.StaticBuffer.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + staticMem + " MB of VRAM for static rendering.");
            
            int instanceMem = WorldRenderer.InstanceBuffer.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + instanceMem + " MB of VRAM for instance rendering.");
            
            int waterMem = WorldRenderer.WaterBuffer.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + waterMem + " MB of VRAM for water rendering.");

            DrawManager.Load();
        }

        public static void EnableGLDebug()
        {
            Renderer.Enable(EnableCap.DebugOutput);
            Renderer.DebugMessageCallback(DebugCallback, IntPtr.Zero);
        }
        
        private static void DebugCallback(GLEnum Source, GLEnum Type, int Id, GLEnum Severity, int Length, IntPtr Message, IntPtr Param)
        {
            if(Type != GLEnum.DebugTypeError) return;
            Log.WriteLine(Source);
            Log.WriteLine(Marshal.PtrToStringAnsi(Message));
            Log.WriteLine(Severity);
            Log.WriteLine(Marshal.PtrToStringAnsi(Param));
        }

        public static void LoadSoundEngine()
        {
            try
            {
                Log.WriteLine("Attemping to load sound engine...");
                SoundPlayer.Provider = new SoundProvider();
                SoundPlayer.Load();
                SoundtrackManager.Load();
                Log.WriteLine("Sound engine loaded succesfully");
            }
            catch (Exception e1)
            {
                string Desc = "Sound engine couldn't load. No openal32.dll?" + Environment.NewLine + e1.Message.ToString();
                Log.WriteResult(false, "Sound Engine failed to load.");
                Log.WriteLine(e1.ToString());
                string[] Files = Directory.GetFiles(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                Desc += Environment.NewLine + " -- Files -- " + Environment.NewLine;
                for (int i = 0; i < Files.Length; i++)
                    Desc += Files[i] + Environment.NewLine;

            }
        }

        public static void LoadArchitectureSpecificFilesIfNecessary(string Path)
        {
            if(_loadedArchitectureFiles) return;
            
            string dllPath = null;
            if (IntPtr.Size == 8) dllPath = Path + "x64/";
            if (IntPtr.Size == 4) dllPath = Path + "x86/";
            Log.WriteLine($"Appending '{dllPath}' to the PATH for library finding.");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllPath);
            
            _loadedArchitectureFiles = true;
        }

        public static void CreateCharacterFolders()
        {
            var appDataCharacters = $"{AppData}Characters/";
            var appPathCharacters = $"{AppPath}Characters/";
            Directory.CreateDirectory(appPathCharacters);
            
            if (Directory.Exists(appDataCharacters))
            {
                var files = Directory.GetFiles(appDataCharacters);
                for (var i = 0; i < files.Length; i++)
                {
                    if (File.Exists(appPathCharacters + Path.GetFileName(files[i]))) continue;

                    File.Move(files[i], appPathCharacters + Path.GetFileName(files[i]));
                }
                var directoryFiles = Directory.GetFiles(appDataCharacters);
                for (var i = 0; i < directoryFiles.Length; i++)
                {
                    File.Delete(directoryFiles[i]);
                }

                Directory.Delete(appDataCharacters);
            }
        }

        public static void CreateCrashesFolderIfNecessary()
        {
            Directory.CreateDirectory(CrashesFolder);
        }
    }
}
