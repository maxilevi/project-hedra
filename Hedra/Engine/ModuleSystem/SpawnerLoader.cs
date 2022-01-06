using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            ResolveSettings(settings, Type);
            return settings;
        }
        
        private static SpawnTemplate[] ResolveTemplate(SpawnTemplate[] Templates, string FileName)
        {
            if (Templates == null) return null;
            var total = 0f;
            for (var i = 0; i < Templates.Length; ++i)
            {
                Templates[i].Chance = (int)ParseDistribution(Templates[i].Distribution);
                total += Templates[i].Chance;
            }

            for (var i = 0; i < Templates.Length; ++i)
            {
                Templates[i].Chance = Templates[i].Chance / total * 100;
                Log.WriteLine($"{Templates[i].Type} resolved to chance {Templates[i].Chance}");
            }
            return Templates;
        }

        private static Distribution ParseDistribution(string DistributionStr)
        {
            return DistributionStr switch
            {
                "VeryLow" => Distribution.VeryLow,
                "Low" => Distribution.Low,
                "LowMedium" => Distribution.LowMedium,
                "Medium" => Distribution.Medium,
                "MediumHigh" => Distribution.MediumHigh,
                "High" => Distribution.High,
                "VeryHigh" => Distribution.VeryHigh,
                _ => throw new ArgumentOutOfRangeException($"Invalid distribution type '{DistributionStr}'")
            };
        }
        
        private static void ResolveSettings(SpawnerSettings Settings, string FileName)
        {

            Settings.Forest = ResolveTemplate(Settings.Forest, FileName);
            Settings.Plains = ResolveTemplate(Settings.Plains, FileName);
            Settings.Shore = ResolveTemplate(Settings.Shore, FileName);
            Settings.Mountain = ResolveTemplate(Settings.Mountain, FileName);
            //Settings.MiniBosses = ResolveTemplate(Settings.MiniBosses, FileName);
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

        private enum Distribution
        {
            VeryLow = 3,
            Low = 7,
            LowMedium = 14,
            Medium = 25,
            MediumHigh = 38,
            High = 50,
            VeryHigh = 75
        }
    }
}