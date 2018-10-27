using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Newtonsoft.Json;

namespace Hedra.Engine.ModuleSystem
{
    public static class HumanoidLoader
    {
        private static readonly Dictionary<string, HumanoidModelTemplate> ModelTemplates;
        private static readonly Dictionary<string, HumanoidTemplate> HumanoidTemplates;
        private static readonly Dictionary<string, HumanoidComponentsTemplate> ComponentsTemplates;
        public static HumanoidModelTemplater ModelTemplater;
        public static HumanoidTemplater HumanoidTemplater;
        public static HumanoidComponentsTemplater ComponentsTemplater;

        static HumanoidLoader()
        {
            ModelTemplates = new Dictionary<string, HumanoidModelTemplate>();
            ModelTemplater = new HumanoidModelTemplater(ModelTemplates);
            HumanoidTemplates = new Dictionary<string, HumanoidTemplate>();
            HumanoidTemplater = new HumanoidTemplater(HumanoidTemplates);
            ComponentsTemplates = new Dictionary<string, HumanoidComponentsTemplate>();
            ComponentsTemplater = new HumanoidComponentsTemplater(ComponentsTemplates);
        }

        public static void LoadModules(string AppPath)
        {
            ModelTemplates.Clear();

            HumanoidModelTemplate[] modelTemplates = Load<HumanoidModelTemplate>(AppPath + "/Modules/NPCs/Models");

            for (var i = 0; i < modelTemplates.Length; i++)
            {
                ModelTemplates.Add(modelTemplates[i].Name.ToLowerInvariant(), modelTemplates[i]);
            }

            HumanoidTemplates.Clear();

            HumanoidTemplate[] humanTemplates = Load<HumanoidTemplate>(AppPath + "/Modules/NPCs/");

            for (var i = 0; i < humanTemplates.Length; i++)
            {
                HumanoidTemplates.Add(humanTemplates[i].Name.ToLowerInvariant(), humanTemplates[i]);
            }

            ComponentsTemplates.Clear();

            HumanoidComponentsTemplate[] componentsTemplates = Load<HumanoidComponentsTemplate>(AppPath + "/Modules/NPCs/Components");

            for (var i = 0; i < componentsTemplates.Length; i++)
            {
                ComponentsTemplates.Add(humanTemplates[i].Name.ToLowerInvariant(), componentsTemplates[i]);
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
