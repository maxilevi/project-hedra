using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Game;

namespace Hedra.Engine.Loader
{
    public static class ModificationsLoader
    {
        public static string Path { get; }

        static ModificationsLoader()
        {
            Path = $"{GameLoader.AppPath}/Mods/";
        }
        
        public static void Setup()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        public static string[] Get(string Pattern)
        {
            var modules = Directory.GetFiles(Path, "*", SearchOption.AllDirectories).Where(M => M.Replace("\\", "/").Contains(Pattern)).ToArray();
            var list = new List<string>();
            for (var i = 0; i < modules.Length; i++)
            {               
                list.Add(modules[i].Replace("\\", "/"));
            }
            return list.ToArray();
        }
    }
}