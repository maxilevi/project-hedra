using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.Templates;
using Newtonsoft.Json;

namespace Hedra.Engine.ModuleSystem
{
    public static class HumanoidLoader
    {
        private static readonly Dictionary<string, HumanoidTemplate> HumanoidTemplates;
        public static HumanoidTemplater HumanoidTemplater;

        static HumanoidLoader()
        {
            HumanoidTemplates = new Dictionary<string, HumanoidTemplate>();
            HumanoidTemplater = new HumanoidTemplater(HumanoidTemplates);
        }

        public static void LoadModules(string AppPath)
        {
            HumanoidTemplates.Clear();

            HumanoidTemplate[] humanTemplates = Load<HumanoidTemplate>(AppPath + "/Modules/NPCs/");

            for (var i = 0; i < humanTemplates.Length; i++)
            {
                HumanoidTemplates.Add(humanTemplates[i].Name.ToLowerInvariant(), humanTemplates[i]);
            }
        }

        private static T[] Load<T>(string CompletePath)
        {
            var list = new List<T>();
            string[] modules = Directory.GetFiles(CompletePath);
            foreach (string module in modules)
            {
                string ext = Path.GetExtension(module);
                if (ext != ".json") continue;

                bool result;
                var obj = FromJSON<T>(File.ReadAllText(module), out result);

                if (!result) continue;

                list.Add(obj);
            }
            return list.ToArray();
        }

        private static T FromJSON<T>(string Data, out bool Success)
        {
            try
            {
                var factory = JsonConvert.DeserializeObject<T>(Data);
                Success = true;
                return factory;
            }
            catch (Exception e)
            {
                Success = false;
                Log.WriteLine(e.ToString());
            }
            return default(T);
        }
    }
}
