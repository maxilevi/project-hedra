using System;
using System.IO;
using Newtonsoft.Json;

namespace Hedra.Engine.ModuleSystem
{
    public static class SpawnerLoader
    {

        public static SpawnerSettings Load(string AppPath, string Type)
        {
            string data = File.ReadAllText(AppPath + $"/Modules/{Type}Spawner.json");
            bool result;
            SpawnerSettings settings = FromJSON( data, out result);
            if(!result) throw new ArgumentException($"Could not load {Type}Spawner.json"); 

            return settings;
        }

        public static SpawnerSettings FromJSON(string Data, out bool Success)
        {
            try
            {
                var settings = JsonConvert.DeserializeObject<SpawnerSettings>(Data);
                Success = true;
                return settings;
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
