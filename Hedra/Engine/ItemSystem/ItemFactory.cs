using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Loader;
using Newtonsoft.Json;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemFactory
    {
        private static readonly Dictionary<string, ItemTemplate> ItemTemplates;
        private static readonly object Lock = new object();
        public static ItemTemplater Templater { get; }

        static ItemFactory()
        {
            ItemTemplates = new Dictionary<string, ItemTemplate>();
            Templater = new ItemTemplater(ItemTemplates, Lock);
        }

        public static void LoadModules(string AppPath)
        {
            lock (Lock)
            {
                ItemTemplates.Clear();
                var modules = Directory.GetFiles($"{AppPath}/Modules/Items/", "*", SearchOption.AllDirectories);
                var mods = ModificationsLoader.Get("/Items/");
                var itemTemplates = Load<ItemTemplate>(modules.Concat(mods).ToArray());

                foreach (var template in itemTemplates)
                {
                    ItemModelLoader.Load(template.Model);
                    ItemTemplates.Add(template.Name.ToLowerInvariant(), template);
                }
            }
        }

        private static T[] Load<T>(string[] Modules)
        {
            var list = new List<T>();
            foreach (var module in Modules)
            {
                var ext = Path.GetExtension(module);
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
