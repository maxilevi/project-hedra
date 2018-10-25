using System.IO;
using Hedra.Engine.Game;

namespace Hedra.Engine.Loader
{
    public class ModificationsLoader
    {
        public static void Setup()
        {
            var path = $"{GameLoader.AppPath}/Mods";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void Load()
        {
            
        }
    }
}