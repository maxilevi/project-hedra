using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.Sound;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Game
{
    public static class GameLoader
    {

        public static string AppData =>
            $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Project Hedra/".Replace("\\", "/");
        
        public static string AppPath =>
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/".Replace("\\", "/");
        
        public static void AllocateMemory()
        {
            /*Log.WriteLine("Querying available vram ...");
            int availableVBOMemory = GraphicsWrapper.QueryAvailableMemory();
            if(availableVBOMemory == 0) Log.WriteLine("Failed to detect available VRAM");
            Log.WriteLine("Available GPU memory is of "+availableVBOMemory+" MB");*/
            Log.WriteLine("Allocating world VRAM");
            //GraphicsOptions.MaxLoadingRadius = Math.Max( 0 * (int) (GraphicsOptions.MaxLoadingRadius / 768f), GraphicsOptions.MinLoadingRadius);
            WorldRenderer.AllocateMemory();

            int staticMem = WorldRenderer.StaticBuffer.Indices.TotalMemory / 1024 / 1024;
            staticMem += WorldRenderer.StaticBuffer.Normals.TotalMemory / 1024 / 1024;
            staticMem += WorldRenderer.StaticBuffer.Colors.TotalMemory / 1024 / 1024;
            staticMem += WorldRenderer.StaticBuffer.Vertices.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + staticMem + " MB of VRAM for static rendering.");
            int waterMem = WorldRenderer.WaterBuffer.Indices.TotalMemory / 1024 / 1024;
            waterMem += WorldRenderer.WaterBuffer.Normals.TotalMemory / 1024 / 1024;
            waterMem += WorldRenderer.WaterBuffer.Colors.TotalMemory / 1024 / 1024;
            waterMem += WorldRenderer.WaterBuffer.Vertices.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + waterMem + " MB of VRAM for water rendering.");

            DrawManager.Load();
        }

        public static void EnableGLDebug()
        {
            Renderer.Enable(EnableCap.DebugOutput);
            Renderer.DebugMessageCallback(
                delegate(DebugSource Source, DebugType Type, int Id, DebugSeverity Severity, int Length, IntPtr Message,
                    IntPtr Param)
                {
                    if(Type != DebugType.DebugTypeError) return;
                    Log.WriteLine(Source);
                    Log.WriteLine(Marshal.PtrToStringAnsi(Message));
                    Log.WriteLine(Severity);
                    Log.WriteLine(Marshal.PtrToStringAnsi(Param));
                }, IntPtr.Zero);
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

        public static void LoadArchitectureSpecificFiles(string AppPath)
        {
            string DLLPath = null;
            if (IntPtr.Size == 8) DLLPath = AppPath + "x64/";
            if (IntPtr.Size == 4) DLLPath = AppPath + "x86/";

            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + DLLPath);
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
