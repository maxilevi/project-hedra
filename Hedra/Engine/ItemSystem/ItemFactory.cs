using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemFactory
    {
        private static readonly Dictionary<string, ItemTemplate> ItemTemplates;
        public static ItemTemplater Templater;

        static ItemFactory()
        {
            ItemTemplates = new Dictionary<string, ItemTemplate>();
            Templater = new ItemTemplater(ItemTemplates);
        }

        public static void LoadModules(string AppPath)
        {
            ItemTemplates.Clear();

            ItemTemplate[] itemTemplates = Load<ItemTemplate>(AppPath + "/Modules/Items/");

            foreach (ItemTemplate template in itemTemplates)
            {
                ItemTemplates.Add(template.Name.ToLowerInvariant(), template);
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

                var obj = FromJSON<T>(File.ReadAllText(module), out bool result);

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
