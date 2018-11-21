using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hedra.API;
using Hedra.Engine.Game;

namespace Hedra.Engine.ModdingSystem
{
    public static class ModificationsLoader
    {
        private static readonly List<Mod> LoadedMods;
        public static string Path { get; }

        static ModificationsLoader()
        {
            Path = $"{GameLoader.AppPath}/Mods/";
            LoadedMods = new List<Mod>();
        }
        
        public static string[] Get(string Pattern)
        {
            if (!Directory.Exists(Path)) return new string[0];
            var modules = Directory.GetFiles(Path, "*", SearchOption.AllDirectories).Where(M => M.Replace("\\", "/").Contains(Pattern)).ToArray();
            var list = new List<string>();
            for (var i = 0; i < modules.Length; i++)
            {               
                list.Add(modules[i].Replace("\\", "/"));
            }
            return list.ToArray();
        }

        private static void Load()
        {
            var files = Get(".dll");
            for (var i = 0; i < files.Length; i++)
            {
                var dll = Assembly.LoadFile(files[i]);
                foreach (var type in dll.GetExportedTypes())
                {
                    if (!type.IsSubclassOf(typeof(Mod)) || type.IsAbstract) continue;
                    var mod = (Mod) Activator.CreateInstance(type, new object[0]);
                    mod.RegisterContent();
                    Log.WriteLine($"Loaded mod '{mod.Name}'");
                    LoadedMods.Add(mod);
                }
            }
        }

        private static void Unload()
        {
            for (var i = LoadedMods.Count - 1; i > -1; --i)
            {
                LoadedMods[i].UnregisterContent();
                LoadedMods.RemoveAt(i);
            }
        }
        
        public static void Reload()
        {
            Unload();
            Load();
        }
    }
}