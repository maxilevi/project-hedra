using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Hedra.Engine.ModuleSystem
{
    public static class MobLoader
    {

        public static IEnemyFactory[] LoadModules(string AppPath)
        {
            var factories = new List<IEnemyFactory>();
            string[] modules = Directory.GetFiles(AppPath + "/Modules/Mobs");
            foreach (string module in modules)
            {
                string ext = Path.GetExtension(module);
                if (ext != ".json") continue;

                IEnemyFactory factory = FromJSON( File.ReadAllText(module), out bool result);

                if(!result) continue;

                factories.Add(factory);
            }
            return factories.ToArray();
        }

        private static IEnemyFactory FromJSON(string Data, out bool Success)
        {
            try
            {
                IEnemyFactory factory = JsonConvert.DeserializeObject<CustomFactory>(Data);
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
