using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Generation;
using Newtonsoft.Json;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageLoader
    {
        private static readonly Dictionary<string, VillageDesign> Designs;
        public static VillageDesigner Designer;

        static VillageLoader()
        {
            Designs = new Dictionary<string, VillageDesign>();
            Designer = new VillageDesigner(Designs);
            World.ModulesReload += LoadModules;
        }
        
        public static void LoadModules(string AppPath)
        {
            Designs.Clear();
            var templates = Load<VillageTemplate>(AppPath + "/Modules/Villages/");
            foreach (var template in templates)
            {
                Designs.Add(template.Name, VillageDesign.FromTemplate(template));
            }
        }
        
        private static T[] Load<T>(string CompletePath)
        {
            var list = new List<T>();
            string[] modules = Directory.GetFiles(CompletePath, "*", SearchOption.AllDirectories);
            foreach (string module in modules)
            {
                string ext = Path.GetExtension(module);
                if (ext != ".json") continue;

                var obj = FromJson<T>(File.ReadAllText(module), out var result);

                if (!result) continue;

                list.Add(obj);
            }
            return list.ToArray();
        }
        
        private static T FromJson<T>(string Data, out bool Success)
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