using System;
using System.Collections.Generic;
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
using Hedra.Engine.Windowing;
using Hedra.Sound;
using SilkGL = Silk.NET.OpenGL;

namespace Hedra.Engine.Game
{
    public static class GameLoader
    {
        private static bool _loadedArchitectureFiles;
        private static List<IntPtr> _nativeLibs;

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


            var staticMem = WorldRenderer.StaticBuffer.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + staticMem + " MB of VRAM for static rendering.");

            var instanceMem = WorldRenderer.InstanceBuffer.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + instanceMem + " MB of VRAM for instance rendering.");

            var waterMem = WorldRenderer.WaterBuffer.TotalMemory / 1024 / 1024;
            Log.WriteLine("Allocated " + waterMem + " MB of VRAM for water rendering.");

            DrawManager.Load();
        }

        public static void EnableGLDebug()
        {
            Renderer.Enable(EnableCap.DebugOutput);
            Renderer.DebugMessageCallback(DebugCallback, IntPtr.Zero);
        }

        private static void DebugCallback(SilkGL.GLEnum Source, SilkGL.GLEnum Type, int Id, SilkGL.GLEnum Severity,
            int Length, IntPtr Message, IntPtr Param)
        {
            if (Type != SilkGL.GLEnum.DebugTypeError) return;
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
                var Desc = "Sound engine couldn't load. No openal32.dll?" + Environment.NewLine + e1.Message;
                Log.WriteResult(false, "Sound Engine failed to load.");
                Log.WriteLine(e1.ToString());
                var Files = Directory.GetFiles(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                Desc += Environment.NewLine + " -- Files -- " + Environment.NewLine;
                for (var i = 0; i < Files.Length; i++)
                    Desc += Files[i] + Environment.NewLine;
            }
        }

        public static void LoadArchitectureSpecificFilesIfNecessary(string Path)
        {
            if (_loadedArchitectureFiles) return;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var is64 = Environment.Is64BitProcess;
            var dllPath = $"{Path}{(is64 ? "x64" : "x86")}";
            var ext = (isWindows ? "dll" : "so");
            var prefix = (isWindows ? "" : "lib");

            _nativeLibs = new List<IntPtr>
            {
                TryLoad($"{dllPath}/{prefix}hedracore.{ext}"),
                TryLoad($"{dllPath}/{prefix}steam_api{(is64 && isWindows ? "64" : string.Empty)}.{ext}"),
                TryLoad($"{dllPath}/libbulletc.{ext}"),
                TryLoad($"{dllPath}/openal32.{ext}"),
                TryLoad($"{dllPath}/{prefix}glfw3.{ext}"),
            };
            _loadedArchitectureFiles = true;
        }

        private static IntPtr TryLoad(string Lib)
        {
            if (!NativeLibrary.TryLoad(Lib, out var outLib))
                Log.WriteLine($"Failed to load library '{Lib}'");
            else 
                Log.WriteLine($"Successfully loaded {Lib}");
            return outLib;
        }

        public static void UnloadNativeLibs()
        {
            for (var i = 0; i < _nativeLibs.Count; ++i)
            {
                NativeLibrary.Free(_nativeLibs[i]);
            }
            _nativeLibs.Clear();
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
                for (var i = 0; i < directoryFiles.Length; i++) File.Delete(directoryFiles[i]);

                Directory.Delete(appDataCharacters);
            }
        }

        public static void CreateCrashesFolderIfNecessary()
        {
            Directory.CreateDirectory(CrashesFolder);
        }
    }
}