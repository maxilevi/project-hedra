using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.API;
using Hedra.Engine.IO;
using Hedra.Engine.ModuleSystem.Templates;
using Newtonsoft.Json;

namespace Hedra.Engine.ModuleSystem
{
    public static class MobLoader
    {
        public static IEnemyFactory[] LoadModules(string AppPath)
        {
            var factories = new Dictionary<string, IEnemyFactory>();
            var mods = ModificationsLoader.Get($"/Mobs/");
            var modules = Directory.GetFiles($"{AppPath}/Modules/Mobs").Concat(mods).ToArray();
            foreach (var module in modules)
            {
                var ext = Path.GetExtension(module);
                if (ext != ".json") continue;

                var factory = FromJSON(File.ReadAllText(module), out var result);
                if(!result) continue;
                factory.Load();

                factories[factory.Name] = factory;
            }
            return factories.Values.ToArray();
        }

        private static CustomFactory FromJSON(string Data, out bool Success)
        {
            try
            {
                var factory = JsonConvert.DeserializeObject<CustomFactory>(Data);
                Success = true;
                return factory;
            }
            catch (Exception e)
            {
                Success = false;
                Log.WriteLine(e.ToString());
            }
            return null;
        }
    }
}
