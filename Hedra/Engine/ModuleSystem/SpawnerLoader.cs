using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.IO;
using Hedra.Engine.ModuleSystem.Templates;
using Newtonsoft.Json;

namespace Hedra.Engine.ModuleSystem
{
    public static class SpawnerLoader
    {
        public static SpawnerSettings Load(string AppPath, string Type)
        {
            var data = File.ReadAllText(AppPath + $"/Modules/Spawners/{Type}.json");
            bool result;
            var settings = FromJSON(data, out result);
            if (!result) throw new ArgumentException($"Could not load {Type}Spawner.json");
            AssertSettings(settings, Type);
            return settings;
        }

        private static void AssertSettings(SpawnerSettings Settings, string Name)
        {
            void Assert(ISpawnTemplate[] Templates, string TypeName)
            {
                if (Templates == null) return;
                var sum = 0f;
                var set = new HashSet<string>();
                for (var i = 0; i < Templates.Length; ++i)
                {
                    sum += Templates[i].Chance;
                    if (!set.Contains(Templates[i].Type))
                        set.Add(Templates[i].Type);
                    else
                        Log.WriteWarning(
                            $"'/Modules/Spawners/{Name}.json' has duplicate entry for '{Templates[i].Type}'");
                }

                if (Math.Abs(sum - 100f) > 0.005f)
                    Log.WriteWarning(
                        $"Entries in '/Modules/Spawners/{Name}.json' for type '{TypeName}' sum up to '{sum}' but should be 100");
            }

            Assert(Settings.Forest, "Forest");
            Assert(Settings.Plains, "Plains");
            Assert(Settings.Shore, "Shore");
            Assert(Settings.Mountain, "Mountain");
            Assert(Settings.MiniBosses, "MiniBosses");
        }

        private static SpawnerSettings FromJSON(string Data, out bool Success)
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