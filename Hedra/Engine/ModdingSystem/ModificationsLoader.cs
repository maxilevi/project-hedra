using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hedra.Engine.Game;

namespace Hedra.Engine.ModdingSystem
{
    public static class ModificationsLoader
    {
        public static string Path { get; }

        static ModificationsLoader()
        {
            Path = $"{GameLoader.AppPath}/Mods/";
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
        
        public static Type[] GetTypes(Predicate<Type> Filter)
        {
            var files = Get(".dll");
            var toReturn = new List<Type>();
            for (var i = 0; i < files.Length; i++)
            {
                var dll = Assembly.LoadFile(files[i]);
                foreach (var type in dll.GetExportedTypes())
                {
                    if(!Filter(type)) continue;
                    toReturn.Add(type);
                }
            }
            return toReturn.ToArray();
        }
    }
}