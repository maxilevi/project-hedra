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

        public static void CreateCharacterFolders(string AppData, string AppPath)
        {
            Directory.CreateDirectory(AppData + "Characters/");

            //Move files to appdata
            if (Directory.Exists(AppPath + "Characters/"))
            {
                string[] Files = Directory.GetFiles(AppPath + "Characters/");
                for (int i = 0; i < Files.Length; i++)
                {
                    if (File.Exists(AppData + "Characters/" + Path.GetFileName(Files[i]))) continue;

                    File.Move(Files[i], AppData + "Characters/" + Path.GetFileName(Files[i]));
                }
                string[] DirectoryFiles = Directory.GetFiles(AppPath + "Characters/");
                for (int i = 0; i < DirectoryFiles.Length; i++)
                    File.Delete(DirectoryFiles[i]);

                Directory.Delete(AppPath + "Characters/");
            }
        }
    }
}
